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
                    Console.WriteLine("Loading human: " + id);
                    var humans = db.Humans.Where(h => h.HumanId == id).ToList();
                    var collection = new NodeCollection<Human>(humans);

                    return collection.Single();
                }
            );

            Field<ListGraphType<HumanType>>(
                "humans",
                resolve: context => {
                    Console.WriteLine("Loading all humans");
                    var humans = db.Humans.ToList();
                    var collection = new NodeCollection<Human>(humans);

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
                    Console.WriteLine("Loading droid: " + id);
                    var driods = db.Droids.Where(d => d.DroidId == id).ToList();
                    var collection = new NodeCollection<Droid>(driods);

                    return collection.Single();
                }
            );

            Field<ListGraphType<CharacterInterface>>(
                "characters",
                resolve: context => {
                    Console.WriteLine("Loading all humans  for characters");
                    var humans = new NodeCollection<Human>(db.Humans.ToList());

                    Console.WriteLine("Loading all droids for characters");
                    var droids = new NodeCollection<Droid>(db.Droids.ToList());

                    return humans.Cast<object>().Union(droids);
                }
            );
        }
    }
}
