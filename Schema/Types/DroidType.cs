using GraphQL.Types;
using ResolveGraphQL.DataModel;

namespace ResolveGraphQL.Schema
{
    public class DroidType : ObjectGraphType
    {
        public DroidType(StarWarsContext dtx)
        {
            Name = "Droid";
            Description = "A mechanical creature in the Star Wars universe.";

            Field<NonNullGraphType<StringGraphType>>("id", "The id of the droid.",
                resolve: context => ((Droid)context.Source).DroidId);

            Field<StringGraphType>("name", "The name of the droid.");
            Field<ListGraphType<CharacterInterface>>(
                "friends",
                resolve: context => null
            );
            
            Field<StringGraphType>("primaryFunction", "The primary function of the droid.");

            Interface<CharacterInterface>();

            IsTypeOf = value => value is Droid;
        }
    }
}
