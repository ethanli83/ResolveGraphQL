using System;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using ResolveGraphQL.DataModel;
using ResolveGraphQL.Schema;
using Unity;

namespace ResolveDataModel
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var db = new StarWarsContext())
            {
                if (db.Database.EnsureCreated())
                {
                    CreateDb(db);
                    Console.WriteLine();
                }

                Console.WriteLine("All humans in database:");

                var data = db.Humans.
                    Include(h => h.Friends).
                        ThenInclude(f => f.Droid).
                    ToList();

                foreach (var human in data)
                {
                    Console.WriteLine(" - Human: {0}", human.Name);
                    foreach (var droid in human.Friends.Select(f => f.Droid))
                        Console.WriteLine(" --- Droid {0}", droid.Name);
                }

                var container = new UnityContainer();

                container.RegisterInstance(db);
                
                container.RegisterType<StarWarsQuery>();
                container.RegisterType<DroidType>();
                container.RegisterType<HumanType>();

                var schema = new StarWarsSchema((t) => container.Resolve(t) as GraphType);

                var query = @"
            query HeroNameQuery {
                humans {
                    name
                    friends {
                        id
                        name
                        ...on Droid {
                            primaryFunction
                        }
                    }
                }
            }";
            
                var result = Execute(schema, null, query);

                Console.WriteLine(result.Result);
            }
        }

        public static async Task<string> Execute(
            Schema schema,
            object rootObject,
            string query,
            string operationName = null,
            Inputs inputs = null)
        {
            var executer = new DocumentExecuter();
            var writer = new DocumentWriter();

            var result = await executer.ExecuteAsync(schema, rootObject, query, operationName, inputs);
            return writer.Write(result);
        }

        private static void CreateDb(StarWarsContext db)
        {
            db.Humans.Add(new Human
            {
                HumanId = 1,
                Name = "Luke",
                HomePlanet = "Tatooine"
            });

            db.Humans.Add(new Human
            {
                HumanId = 2,
                Name = "Vader",
                HomePlanet = "Tatooine"
            });

            db.Droids.Add(new Droid
            {
                DroidId = 1,
                Name = "R2-D2",
                PrimaryFunction = "Astromech"
            });

            db.Droids.Add(new Droid
            {
                DroidId = 2,
                Name = "C-3PO",
                PrimaryFunction = "Protocol"
            });

            db.HumanFriends.Add(new HumanFreind
            {
                HumanId = 1,
                DroidId = 1
            });

            db.HumanFriends.Add(new HumanFreind
            {
                HumanId = 1,
                DroidId = 2
            });

            db.HumanFriends.Add(new HumanFreind
            {
                HumanId = 2,
                DroidId = 1
            });

            var count = db.SaveChanges();
            Console.WriteLine("{0} records saved to database", count);
        }
    }
}
