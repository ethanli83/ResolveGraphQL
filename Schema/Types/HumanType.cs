using System;
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
                    // first to get the node which is a type of Human
                    var human = context.GetGraphNode<Human>();
                    // get all its siblings of the node
                    var collection = context.GetNodeCollection<Human>();

                    // GetOrAddRelation will first check if there is a stored result for key 'friends'
                    // if the result exist, it will immidately return the stored result. 
                    // otherwise, it will create a new NodeCollection with the given loader function
                    var childCollection = collection.GetOrAddRelation(
                        "friends",
                        () => 
                        {
                            var humanIds = collection.Select(n => n.Node.HumanId).ToArray();
                            return new NodeCollection<Droid>(
                                () => 
                                {
                                    Console.WriteLine("Loading all friends for humans");
                                    return db.Droids.
                                    Where(d => d.Friends.Any(f => humanIds.Contains(f.HumanId))).
                                    Include(d => d.Friends).
                                    ToList();
                                });
                        });

                    // look for the friends for the actual node
                    // todo: need to index the query result so that we can search quicker.
                    return childCollection.Where(d => d.Node.Friends.Any(f => f.HumanId == human.HumanId));
                }
            );

            Interface<CharacterInterface>();

            IsTypeOf = value => value is GraphNode<Human>;
        }
    }
}
