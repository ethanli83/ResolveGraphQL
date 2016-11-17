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
        private static NodeCollectionIndexer<Human> _friendsIndexer = 
            new NodeCollectionIndexer<Human>((d, index) => {
                var hIds = d.Node.Friends.Select(f => f.DroidId).Distinct();
                foreach(var hId in hIds)
                {
                    if (!index.ContainsKey(hId))
                        index[hId] = new List<GraphNode<Human>>();

                    var dList = index[hId];
                    dList.Add(d);
                }
            });
            
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
                        (indexer) => 
                        {
                            var droidIds = collection.Select(n => n.Node.DroidId).ToArray();
                            
                            Console.WriteLine("Loading all friends for droids");
                            var humans = db.Humans.
                                Where(d => d.Friends.Any(f => droidIds.Contains(f.DroidId))).
                                Include(d => d.Friends).
                                ToList();

                            return new NodeCollection<Human>(humans, indexer);
                        });

                    return childCollection.Where(d => d.Node.Friends.Any(f => f.DroidId == droid.DroidId));
                }
            );

            Interface<CharacterInterface>();

            IsTypeOf = value => value is GraphNode<Droid>;
        }
    }
}
