using System.Linq;
using GraphQL.Types;
using ResolveGraphQL.DataModel;

namespace ResolveGraphQL.Schema
{
    public class HumanType : ObjectGraphType
    {
        public HumanType(StarWarsContext db)
        {
            Name = "Human";

            Field<NonNullGraphType<StringGraphType>>("id", "The id of the human.", 
                resolve: context => ((Human)context.Source).HumanId);
                
            Field<StringGraphType>("name", "The name of the human.");

            Field<ListGraphType<CharacterInterface>>(
                "friends",
                resolve: context =>
                {
                    var human = context.Source as Human;
                    if (human == null)
                        return null;

                    return db.Droids.Where(d => d.Friends.Any(f => f.HumanId == human.HumanId)).ToList();
                }
            );

            Field<StringGraphType>("homePlanet", "The home planet of the human.");

            Interface<CharacterInterface>();

            IsTypeOf = value => value is Human;
        }
    }
}
