using GraphQL.Types;

namespace ResolveGraphQL.Schema
{
    public class CharacterInterface : InterfaceGraphType
    {
        public CharacterInterface()
        {
            Name = "Character";
            Field<NonNullGraphType<StringGraphType>>("id", "The id of the character.");
            Field<StringGraphType>("name", "The name of the character.");
            Field<ListGraphType<CharacterInterface>>("friends");
        }
    }
}
