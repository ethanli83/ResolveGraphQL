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
                        () => db.Humans.Where(h => h.HumanId == id).ToList());

                    return collection.Single();
                }
            );

            Field<ListGraphType<HumanType>>(
                "humans",
                resolve: context => {
                    var collection = new NodeCollection<Human>(
                        () => db.Humans.ToList());

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
                        () => db.Droids.Where(d => d.DroidId == id).ToList());

                    return collection.Single();
                }
            );
        }
    }
}
