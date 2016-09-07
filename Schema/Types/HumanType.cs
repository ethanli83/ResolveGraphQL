using System.Linq;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using ResolveGraphQL.DataModel;

namespace ResolveGraphQL.Schema
{
    public class HumanType : ObjectGraphType
    {
        public HumanType(StarWarsContext db)
        {
            Name = "Human";

            Field<NonNullGraphType<StringGraphType>>(
                "id", "The id of the human.", 
                resolve: context => context.GetGraphNode<Human>().HumanId);
                
            Field<StringGraphType>(
                "name", "The name of the human.",
                resolve: context => context.GetGraphNode<Human>().Name);

            Field<StringGraphType>(
                "homePlanet", "The home planet of the human.",
                resolve: context => context.GetGraphNode<Human>().HomePlanet);

            Field<ListGraphType<CharacterInterface>>(
                "friends",
                resolve: context =>
                {
                    var human = context.GetGraphNode<Human>();
                    var collection = context.GetNodeCollection<Human>();
                    var childCollection = collection.GetOrAddRelation(
                        "friends",
                        () => 
                        {
                            var humanIds = collection.Select(n => n.Node.HumanId).ToArray();
                            return new NodeCollection<Droid>(
                                () => db.Droids.
                                    Where(d => d.Friends.Any(f => humanIds.Contains(f.HumanId))).
                                    Include(d => d.Friends).
                                    ToList());
                        });

                    return childCollection.Where(d => d.Node.Friends.Any(f => f.HumanId == human.HumanId));
                }
            );

            Interface<CharacterInterface>();

            IsTypeOf = value => value is GraphNode<Human>;
        }
    }
}
