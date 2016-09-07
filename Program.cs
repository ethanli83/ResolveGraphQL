using System;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
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
                // insert some testing data into database
                if (db.Humans.Count() == 0)
                    InsertData(db);

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
                characters {
                    name
                    friends {
                        name
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

        private static void InsertData(StarWarsContext db)
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
