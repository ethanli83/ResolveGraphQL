using System;
using System.Linq;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using ResolveGraphQL.DataModel;

namespace ResolveGraphQL.Schema
{
    public class DroidType : ObjectGraphType
    {
        public DroidType(StarWarsContext db)
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
                resolve: context => 
                {
                    var droid = context.GetGraphNode<Droid>();
                    var collection = context.GetNodeCollection<Droid>();
                    var childCollection = collection.GetOrAddRelation(
                        "friends",
                        () => 
                        {
                            var droidIds = collection.Select(n => n.Node.DroidId).ToArray();
                            return new NodeCollection<Human>(
                                () => 
                                {
                                    Console.WriteLine("Loading all friends for droids");
                                    return db.Humans.
                                    Where(d => d.Friends.Any(f => droidIds.Contains(f.DroidId))).
                                    Include(d => d.Friends).
                                    ToList();
                                });
                        });

                    return childCollection.Where(d => d.Node.Friends.Any(f => f.DroidId == droid.DroidId));
                }
            );

            Interface<CharacterInterface>();

            IsTypeOf = value => value is GraphNode<Droid>;
        }
    }
}
