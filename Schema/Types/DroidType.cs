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

            Field<NonNullGraphType<StringGraphType>>(
                "id", "The id of the droid.",
                resolve: context => context.GetGraphNode<Droid>().DroidId);

            Field<StringGraphType>(
                "name", "The name of the droid.",
                resolve: context => context.GetGraphNode<Droid>().Name);

            Field<StringGraphType>(
                "primaryFunction", "The primary function of the droid.",
                resolve: context => context.GetGraphNode<Droid>().PrimaryFunction);

            Field<ListGraphType<CharacterInterface>>(
                "friends",
                resolve: context => null
            );

            Interface<CharacterInterface>();

            IsTypeOf = value => value is GraphNode<Droid>;
        }
    }
}
