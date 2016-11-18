using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using ResolveGraphQL.DataModel;

namespace ResolveGraphQL.Schema
{
    public class HumanType : ObjectGraphType<GraphNode<Human>>
    {
        // a indexer which will give node collection a index 
        private static Func<NodeCollection<Droid>, Dictionary<int, GraphNode<Droid>[]>> _friendsIndexer = 
            nc => 
                nc.Select(n => n.Node).SelectMany(h => h.Friends).
                GroupBy(h => h.HumanId).
                ToDictionary(g => g.Key, g => g.Select(f => new GraphNode<Droid>(f.Droid, nc)).ToArray());

        public HumanType(StarWarsContext db)
        {
            Name = "Human";

            Field(x => x.Node.HumanId).
                Name("id").
                Description("The id of the human.");

            Field(x => x.Node.Name).
                Name("name").
                Description("The name of the human.");

            Field(x => x.Node.HomePlanet).
                Name("homePlanet").
                Description("The home planet of the human.");

            Field<ListGraphType<CharacterInterface>>(
                "friends",
                resolve: context =>
                {
                    // first to get the node which is a type of Human
                    var human = context.GetGraphNode<Human>();
                    // get all its siblings of the node
                    var collection = context.GetNodeCollection<Human>();

                    // GetOrAddRelation will first check if there is a stored result for the indexer
                    // if the result exist, it will immidately return the stored result. 
                    // otherwise, it will create a new NodeCollection with the given loader function
                    var childCollection = collection.GetOrAddRelation(
                        _friendsIndexer,
                        () => 
                        {
                            var humanIds = collection.Select(n => n.Node.HumanId).ToArray();

                            Console.WriteLine("Loading all friends for humans");
                            var droids = db.Droids.
                                Where(d => d.Friends.Any(f => humanIds.Contains(f.HumanId))).
                                Include(d => d.Friends).
                                ToList();

                            return new NodeCollection<Droid>(droids);
                        });

                    return childCollection.GetManyByKey(human.HumanId);
                }
            );

            Interface<CharacterInterface>();

            IsTypeOf = value => value is GraphNode<Human>;
        }
    }
}
