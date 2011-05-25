#region usings

using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using MongoDB.Driver;
using Newtonsoft.Json;
using NUnit.Framework;
using Raven.Abstractions.Data;
using Raven.Client.Document;
using Raven.Client.Indexes;

#endregion

namespace Scratch
{
    [TestFixture]
    public class MongoTests
    {
        private const int TransactionBatchSize = 500;
        //private const int USER_COUNT = 10000;
        private const int USER_COUNT = 25000;
        private const bool REMOVE_USERS = false;
        private const int SqlBatchSize = 50;

        [Test]
        public void SqlConnect()
        {
            Time(string.Format("inserting {0} users", USER_COUNT),
                 () =>
                     {
                         var handled = 0;
                         while(handled < USER_COUNT)
                         {
                             //Console.WriteLine("Starting new transaction");
                             using(var tx = new TransactionScope())
                             {
                                 using(var connection = new SqlConnection("Data Source=localhost;Initial Catalog=MyEventStorage;User ID=MyEventStorage;Password=MyEventStorage"))
                                 {
                                     connection.Open();

                                     for (var handledInTransaction = 0; handledInTransaction < TransactionBatchSize && handled < USER_COUNT; handledInTransaction += SqlBatchSize)
                                     {
                                         //Console.WriteLine("Starting new sql batch");
                                         var command = connection.CreateCommand();
                                         command.CommandType = CommandType.Text;
                                         for(var handledInBatch = 0; handledInBatch < SqlBatchSize && handled < USER_COUNT; handledInBatch++, handled++)
                                         {
                                             var user = new User
                                                            {
                                                                Id = Guid.NewGuid(),
                                                                Name = "Yo! " + Guid.NewGuid()
                                                            };

                                             command.CommandText +=
                                                 string.Format(
                                                     "INSERT Events(Id, Discriminator, StringValue) VALUES(@Id{0}, @Discriminator{0}, @StringValue{0})",
                                                     handledInBatch);
                                             command.Parameters.Add(new SqlParameter("Id" + handledInBatch, user.Id));
                                             command.Parameters.Add(new SqlParameter("Discriminator" + handledInBatch, user.GetType().FullName));
                                             command.Parameters.Add(new SqlParameter("StringValue" + handledInBatch, JsonConvert.SerializeObject(user)));
                                         }
                                         command.ExecuteNonQuery();
                                     }

                                     tx.Complete();
                                 }
                             }
                         }
                     });

            if(!REMOVE_USERS)
                return;

            using(var tx = new TransactionScope())
            {
                using(var connection = new SqlConnection("Data Source=localhost;Initial Catalog=MyEventStorage;User ID=MyEventStorage;Password=MyEventStorage"))
                {
                    connection.Open();
                    Time("removing all users",
                         () =>
                             {
                                 var command = connection.CreateCommand();
                                 command.CommandType = CommandType.Text;
                                 command.CommandText = "DELETE Events";
                                 command.ExecuteNonQuery();
                             });
                }
                tx.Complete();
            }
        }


        [Test]
        public void ShouldConnect()
        {
            var server = MongoServer.Create("mongodb://localhost/?safe=true");
            var db = server.GetDatabase("MyTestDB");

            var users = db.GetCollection<User>("users");

            Time(string.Format("inserting {0} users", USER_COUNT),
                 () =>
                 {
                     var toInsert = Enumerable.Range(0, USER_COUNT)
                         .Select(_ =>
                                 new User
                                 {
                                     Id = Guid.NewGuid(),
                                     Name = "Yo! " + Guid.NewGuid()
                                 });

                     users.InsertBatch(toInsert);
                     //foreach(var user in toInsert)
                     //{
                     //    users.Save(user);
                     //}
                 });

            if (!REMOVE_USERS)
                return;
            //Console.WriteLine(users.FindOneById(BsonValue.Create(user.Id)).Name);
            Time("removing all users", () => users.RemoveAll());
        }

        [Test]
        public void RavenConnect()
        {
            var _documentStore = new DocumentStore
            {
                Url = "http://localhost:8080"
                //,RunInMemory = true
            };


            _documentStore.Initialize();
            IndexCreation.CreateIndexes(typeof(Indexes.Users_ById).Assembly, _documentStore);
            using (var tx = new TransactionScope())
            {
                using (var conn = _documentStore.OpenSession())
                {
                    Time(string.Format("inserting {0} users", USER_COUNT),
                         () =>
                         {
                             for (var i = 0; i < USER_COUNT; i++)
                             {
                                 var user = new User
                                 {
                                     Id = Guid.NewGuid(),
                                     Name = "Yo! " + Guid.NewGuid()
                                 };
                                 conn.Store(user);
                             }
                             conn.SaveChanges();
                         });
                }
                tx.Complete();
            }

            if (!REMOVE_USERS)
                return;
            using (var session = _documentStore.OpenSession())
            {
                Time("removing all users",
                     () =>
                     {
                         session.Query<User, Indexes.Users_ById>().Customize(c => c.WaitForNonStaleResults()).ToList();
                         _documentStore.DatabaseCommands.DeleteByIndex("Users/ById", new IndexQuery(), allowStale: false);
                     });
            }
        }



        private static void Time(string taskDescription, Action task)
        {
            Console.WriteLine("Starting: {0}", taskDescription);
            var watch = new Stopwatch();
            watch.Start();
            task();
            Console.WriteLine("Done in: {0} milliseconds", watch.ElapsedMilliseconds);
        }
    }

    public class Indexes
    {
        public class Users_ById : AbstractIndexCreationTask<User>
        {
            public Users_ById()
            {
                Map = users => from user in users
                               select new { user.Id };
            }
        }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}