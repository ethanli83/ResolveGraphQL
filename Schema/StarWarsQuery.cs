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
                    return db.Humans.Where(h => h.HumanId == id).ToList();
                }
            );

            Field<ListGraphType<HumanType>>(
                "humans",
                resolve: context => {
                    return db.Humans.ToList();
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
                    return db.Droids.Where(d => d.DroidId == id).ToList();
                }
            );
        }
    }
}
