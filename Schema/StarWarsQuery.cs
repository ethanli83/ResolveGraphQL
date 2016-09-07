using System;
using System.Linq;
using GraphQL.Types;
using ResolveGraphQL.DataModel;

namespace ResolveGraphQL.Schema
{
    public class StarWarsQuery : ObjectGraphType
    {
        public StarWarsQuery(StarWarsContext db)
        {
            Name = "Query";

            Field<CharacterInterface>("hero", resolve: context => null );

            Field<HumanType>(
                "human",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "id", Description = "id of the human" }
                ),
                resolve: context => 
                {
                    var id = context.GetArgument<int>("id");
                    var collection = new NodeCollection<Human>(
                        () => 
                        {
                            Console.WriteLine("Loading human: " + id);
                            return db.Humans.Where(h => h.HumanId == id).ToList();
                        });

                    return collection.Single();
                }
            );

            Field<ListGraphType<HumanType>>(
                "humans",
                resolve: context => {
                    var collection = new NodeCollection<Human>(
                        () => 
                        {
                            Console.WriteLine("Loading all humans");
                            return db.Humans.ToList();
                        });

                    return collection;
                }
            );

            Field<DroidType>(
                "droid",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "id", Description = "id of the droid" }
                ),
                resolve: context => 
                {
                    var id = context.GetArgument<int>("id");
                    var collection = new NodeCollection<Droid>(
                        () => 
                        {
                            Console.WriteLine("Loading droid: " + id);
                            return db.Droids.Where(d => d.DroidId == id).ToList();
                        });

                    return collection.Single();
                }
            );

            Field<ListGraphType<CharacterInterface>>(
                "characters",
                resolve: context => {
                    var humans = new NodeCollection<Human>(
                        () => 
                        {
                            Console.WriteLine("Loading all humans for characters");
                            return db.Humans.ToList();
                        });

                    var droids = new NodeCollection<Droid>(
                        () =>
                        {
                            Console.WriteLine("Loading all droids for characters");
                            return db.Droids.ToList(); 
                        });

                    return humans.Cast<object>().Union(droids);
                }
            );
        }
    }
}
