#region usings

using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Transactions;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using NUnit.Framework;
using Raven.Abstractions.Data;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Extensions;

#endregion

namespace Scratch
{
    [TestFixture]
    public class MongoTests
    {
        private const int TransactionBatchSize = 500;
        //private const int USER_COUNT = 10000;
        private const int USER_COUNT = 100000;
        private const bool REMOVE_USERS = true;
        private const int SqlBatchSize = 20;

        private const int RavenBatchSize = 100;

        [Test]
        public void SqlInsertUsers()
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
                                 using(
                                     var connection =
                                         new SqlConnection("Data Source=localhost;Initial Catalog=MyEventStorage;User ID=MyEventStorage;Password=MyEventStorage")
                                     )
                                 {
                                     connection.Open();

                                     for(var handledInTransaction = 0;
                                         handledInTransaction < TransactionBatchSize && handled < USER_COUNT;
                                         handledInTransaction += SqlBatchSize)
                                     {
                                         //Console.WriteLine("Starting new sql batch");
                                         var command = connection.CreateCommand();
                                         command.CommandType = CommandType.Text;
                                         for(var handledInBatch = 0; handledInBatch < SqlBatchSize && handled < USER_COUNT; handledInBatch++, handled++)
                                         {
                                             var user = CreateUser();

                                             command.CommandText +=
                                                 string.Format(
                                                     "INSERT Events(Id, Discriminator, StringValue) VALUES(@Id{0}, @Discriminator{0}, @StringValue{0})",
                                                     //"INSERT Events(Id, Discriminator, BinaryValue) VALUES(@Id{0}, @Discriminator{0}, @BinaryValue{0})",
                                                     //"INSERT Events(Id, Discriminator, BinaryValue, StringValue) VALUES(@Id{0}, @Discriminator{0}, @BinaryValue{0}, @StringValue{0})",
                                                     handledInBatch);
                                             command.Parameters.Add(new SqlParameter("Id" + handledInBatch, user.Id));
                                             command.Parameters.Add(new SqlParameter("Discriminator" + handledInBatch, user.GetType().FullName));
                                             command.Parameters.Add(new SqlParameter("StringValue" + handledInBatch, JsonConvert.SerializeObject(user)));

                                             using(var ms = new MemoryStream())
                                             {
                                                 var serializer = new JsonSerializer();
                                                 var writer = new BsonWriter(ms);

                                                 serializer.Serialize(writer, user);
                                                 command.Parameters.Add(new SqlParameter("BinaryValue" + handledInBatch, ms.ToArray()));
                                             }
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
        public void MongoInsertUsers()
        {
            var server = MongoServer.Create("mongodb://localhost/?safe=true");
            var db = server.GetDatabase("MyTestDB");

            var users = db.GetCollection<User>("users");

            Time(string.Format("inserting {0} users", USER_COUNT),
                 () =>
                     {
                         for(int inserted = 0; inserted < USER_COUNT; )
                         {
                             var toInsert = Enumerable.Range(0, TransactionBatchSize)
                                 .Select(_ => CreateUser())
                                 .Take(USER_COUNT - inserted)
                                 .ToArray();
                             //users.InsertBatch(toInsert);
                             foreach (var user in toInsert)
                             {
                                 users.Insert(user);
                             }
                             inserted += toInsert.Count();
                         }
                         
                         //foreach(var user in toInsert)
                         //{
                         //    users.Save(user);
                         //}
                     });

            if(!REMOVE_USERS)
                return;
            //Console.WriteLine(users.FindOneById(BsonValue.Create(user.Id)).Name);
            Time("removing all users", () => users.RemoveAll());
        }

        [Test]
        public void ZRavenInsertUsers()
        {
            var _documentStore = new DocumentStore
                                     {
                                         Url = "http://localhost:8080"
                                         //,RunInMemory = true
                                     };

            _documentStore.Initialize();
            //IndexCreation.CreateIndexes(typeof(Indexes.Users_ById).Assembly, _documentStore);

            var handled = 0;
            Time(string.Format("inserting {0} users", USER_COUNT),
                 () =>
                     {
                         while(handled < USER_COUNT)
                         {
                             using(var tx = new TransactionScope())
                             {
                                 using(var conn = _documentStore.OpenSession())
                                 {
                                     for(var handledInTransaction = 0;
                                         handledInTransaction < TransactionBatchSize && handled < USER_COUNT;
                                         handledInTransaction++, handled++)
                                     {
                                         conn.Store(CreateUser());
                                     }
                                     conn.SaveChanges();
                                 }
                                 tx.Complete();
                             }
                         }
                     });

            if(!REMOVE_USERS)
                return;
            //using(var session = _documentStore.OpenSession())
            //{
            //    Time("removing all users",
            //         () =>
            //             {
            //                 session.Query<User, Indexes.Users_ById>().Customize(c => c.WaitForNonStaleResults()).ToList();
            //                 _documentStore.DatabaseCommands.DeleteByIndex("Users/ById", new IndexQuery(), allowStale: false);
            //             });
            //}
        }


        private static void Time(string taskDescription, Action task)
        {
            Console.WriteLine("Starting: {0}", taskDescription);
            var watch = new Stopwatch();
            watch.Start();
            task();
            Console.WriteLine("Done in: {0} milliseconds", watch.ElapsedMilliseconds);
        }

        private static User CreateUser()
        {
            return new User
                       {
                           Id = Guid.NewGuid(),
                           Name = "Yo! " + Guid.NewGuid(),
                           Text1 = "some anoying really long text hi there i'm long and annoying dont you figure?????",
                           Text2 = "some anoying really long text hi there i'm long and annoying dont you figure?????"
                       };
        }
    }

    //public class Indexes
    //{
    //    public class Users_ById : AbstractIndexCreationTask<User>
    //    {
    //        public Users_ById()
    //        {
    //            Map = users => from user in users
    //                           select new { Id = user.Id };
    //        }
    //    }
    //}


    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
    }
}