using System;
using System.Data.SqlClient;
using System.Transactions;
using FluentAssertions;
using NUnit.Framework;

namespace Scratch.Tests
{
    [TestFixture]
    public class EscalationDoesNotWorkOnPrepareEvenWithEnlistmentDuringPrepareRequired
    {
        private string DB1 = @"Data Source=.\SQLEXPRESS;AttachDbFileName=|DataDirectory|DB1.mdf;Integrated Security=True;User Instance=False";
        private string DB2 = @"Data Source=.\SQLEXPRESS;AttachDbFileName=|DataDirectory|DB2.mdf;Integrated Security=True;User Instance=False";
        private string DB3 = @"Data Source=.\SQLEXPRESS;AttachDbFileName=|DataDirectory|DB3.mdf;Integrated Security=True;User Instance=False";

        [Test]
        public void Escalates_when_transaction_is_not_distributed_1()
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMilliseconds(3000)))
            {
                new CachingParticipant(DB1).DoCachedWork();

                new CachingParticipant(DB2).DoCachedWork();

                transaction.Complete();
            }
        }

        [Test]
        public void Doesnt_hang_when_transaction_is_already_distributed_1()
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMilliseconds(3000)))
            {
                new PersistentParticipant(DB1).DoWork();

                new PersistentParticipant(DB2).DoWork();

                new CachingParticipant(DB1).DoCachedWork();

                new CachingParticipant(DB2).DoCachedWork();

                transaction.Complete();
            }
        }

        [Test]
        public void Doesnt_hang_when_transaction_is_already_distributed_2_1()
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMilliseconds(3000)))
            {
                new PersistentParticipant(DB3).DoWork();

                new PersistentParticipant(DB1).DoWork();

                new PersistentParticipant(DB2).DoWork();

                new CachingParticipant(DB1).DoCachedWork();

                new CachingParticipant(DB2).DoCachedWork();

                transaction.Complete();
            }
        }

        private string _experisDevSql = "Data Source=experisdevsql;Initial Catalog=ApplicationLogs ;User ID=ApplicationLogs ;Password=ApplicationLogs";
        private string _experisTestSql = "Data Source=experistestsql;Initial Catalog=ApplicationLogs ;User ID=ApplicationLogs ;Password=ApplicationLogs";
        private string _localHost = "Data Source=localhost;Initial Catalog=master;integrated security=true";


        [Test]
        public void Escalates_when_transaction_is_not_distributed()
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMilliseconds(3000)))
            {
                new CachingParticipant(_experisDevSql).DoCachedWork();

                new CachingParticipant(_experisTestSql).DoCachedWork();

                transaction.Complete();
            }
        }

        [Test]
        public void Doesnt_hang_when_transaction_is_already_distributed()
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMilliseconds(3000)))
            {
                new PersistentParticipant(_experisDevSql).DoWork();

                new PersistentParticipant(_experisTestSql).DoWork();

                new CachingParticipant(_experisDevSql).DoCachedWork();

                new CachingParticipant(_experisTestSql).DoCachedWork();

                transaction.Complete();
            }
        }

        [Test]
        public void Doesnt_hang_when_transaction_is_already_distributed_2()
        {
            using (var transaction = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMilliseconds(3000)))
            {
                new PersistentParticipant(_localHost).DoWork();

                new PersistentParticipant(_experisDevSql).DoWork();

                new PersistentParticipant(_experisTestSql).DoWork();

                new CachingParticipant(_experisDevSql).DoCachedWork();

                new CachingParticipant(_experisTestSql).DoCachedWork();

                transaction.Complete();
            }
        }



    }


    public class PersistentParticipant
    {
        private readonly string _connectionString;
        public PersistentParticipant(string connectionString) { _connectionString = connectionString; }

        public void DoWork()
        {
           using (var connection = OpenSession())
           {
                using (var command = connection.CreateCommand())
               {
                    command.CommandText = "select GetDate()";
                    using (var reader = command.ExecuteReader())
                   {
                        while (reader.Read()){}
                   }
               }
           }
        }

        private SqlConnection OpenSession()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }


    public class CachingParticipant : IEnlistmentNotification
    {
        private readonly string _connectionString;
        public CachingParticipant(string connectionString) { _connectionString = connectionString; }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            try
            {
                Console.WriteLine($"PREPARE PREPARE PREPARE PREPARE PREPARE PREPARE PREPARE");
                using (var resurrectedTransaction = new TransactionScope(AmbientTransaction))
                {
                    new PersistentParticipant($"{_connectionString};")//Add a little; to get a better exception
                        .DoWork();
                    resurrectedTransaction.Complete();
                }

                AmbientTransaction.Dispose();

                preparingEnlistment.Prepared();
            }
            catch(Exception exception)
            {
                Console.WriteLine($@"PREPARE FAILED PREPARE FAILED PREPARE FAILED PREPARE FAILED PREPARE FAILED 

{exception}

");
                preparingEnlistment.ForceRollback(exception);
            }            
        }

        public void Commit(Enlistment enlistment)
        {
            Console.WriteLine("COMMIT COMMIT COMMIT COMMIT COMMIT COMMIT COMMIT COMMIT");
            enlistment.Done();            
        }

        public void Rollback(Enlistment enlistment)
        {
            Console.WriteLine("ROLLBACK ROLLBACK ROLLBACK ROLLBACK ROLLBACK ROLLBACK");
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            Console.WriteLine("INDOUBT INDOUBT INDOUBT INDOUBT INDOUBT INDOUBT INDOUBT");
            enlistment.Done();
        }


        public void DoCachedWork()
        {            
            Transaction.Current.EnlistVolatile(this, EnlistmentOptions.EnlistDuringPrepareRequired);
            AmbientTransaction = Transaction.Current.Clone();
        }

        private Transaction AmbientTransaction { get; set; }
    }

}