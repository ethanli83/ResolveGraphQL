using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using ResolveGraphQL.DataModel;

namespace ResolveGraphQL.Schema
{
    public class DroidType : ObjectGraphType<GraphNode<Droid>>
    {
        private static Func<NodeCollection<Human>, Dictionary<object, GraphNode<Human>[]>> _friendsIndexer = 
            nc => nc.
                Select(n => n.Node).SelectMany(h => h.Friends).
                GroupBy(h => (object)h.DroidId).
                ToDictionary(g => g.Key, g => g.Select(f => new GraphNode<Human>(f.Human, nc)).ToArray());
            
        public DroidType(StarWarsContext db)
        {
            Name = "Droid";
            Description = "A mechanical creature in the Star Wars universe.";

            Field(x => x.Node.DroidId).
                Name("id").
                Description("The id of the droid.");

            Field(x => x.Node.Name).
                Name("name").
                Description("The name of the droid.");

            Field(x => x.Node.PrimaryFunction).
                Name("primaryFunction").
                Description("The primary function of the droid.");

            Field<ListGraphType<CharacterInterface>>(
                "friends",
                resolve: context => 
                {
                    var droid = context.GetGraphNode<Droid>();
                    var collection = context.GetNodeCollection<Droid>();
                    var childCollection = collection.GetOrAddRelation(
                        _friendsIndexer,
                        () => 
                        {
                            var droidIds = collection.Select(n => n.Node.DroidId).ToArray();
                            
                            Console.WriteLine("Loading all friends for droids");
                            var humans = db.Humans.
                                Where(d => d.Friends.Any(f => droidIds.Contains(f.DroidId))).
                                Include(d => d.Friends).
                                ToList();

                            return new NodeCollection<Human>(humans);
                        });

                    return childCollection.GetManyByKey(droid.DroidId);
                }
            );

            Interface<CharacterInterface>();

            IsTypeOf = value => value is GraphNode<Droid>;
        }
    }
}
