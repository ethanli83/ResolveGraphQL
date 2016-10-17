# ResolveGraphQL
An example on how to solving N+1 problem for GraphQL using graphql-dotnet

# Setup .Net core
This exmaple uses .net core. You can get it and learn more about it from the link below:
https://www.microsoft.com/net/core#windows

# Setup Entity Framework
I use entity framework + sqlite to create a simple database to run queries. 

You need to run commands below if you are running the code for the first time.  
### 1. Create migration file
```dotnet ef migrations add InitialSetup```
### 2. Update database with generated schema
```dotnet ef database update```

If you want to recreate the database, run following commands to drop the db 
first, then you can run previous commands to create it again.
### 1. Drop the database first
```dotnet ef database drop```
### 2. Remove the migration file
```dotnet ef migrations remove```


#The concept of the solution.

In the original resolving process, siblings are resolved individually. Which means, even through all the children of given siblings are stored in the same table, we will still call database per each sibling. This is inefficient for most database such as mssql.

This sample code trys to improve the performance of resolving graphql by only calling database once per child property. For example, assuming in the StarWar database we have 3 human and each of them has 2 friends. When we query the database by:
```
query HumansQuery {
    humans {
        name
        friends {
            name
        }
    }
}
```
We only want to call the database once for resolving 'friends', rather than calling the database 3 times.

To achive this goal, we need two piece of information when we resolve child property of a sibling. Firstly, we need to have access to all siblings so that we can composite a query to database containing all the parent ids. Secondly, we need a way to tell if a query to database for the child property has be called to make sure we only call database once.

In the same code, we have two core classes namely: GraphNode<T> and NodeCollection<T>. 

GraphNode<T> has a property called 'Collection' which contains all the siblings of the Node. 

NodeCollection<T> has a loader function and a private field '_nodes'. this class will only call the loader function once to fill the '_nodes' field, and will not call the loader again if the '_nodes' has be initialized.

NodeCollection<T> also has a private dictionary called '_relations'. This dictionary stores all resolved child properties. When resolving a child, the program firstly checks the dictionary to see if the child has been resolved or not. If it has, the program will grab the stored result rather than calling database again.

With helps from this two classes, we achive the goal of only calling database once per child property.

Now, let's have a look at a real example. 

Here is how the resolve function of 'friends' implemented for HumanType:   

```csharp
public class HumanType : ObjectGraphType
{
    public HumanType(StarWarsContext db)
    {
        Name = "Human";

        Field<ListGraphType<CharacterInterface>>(
            "friends",
            resolve: context =>
            {
                // first to get the graph node which is a type of GraphNode<Human>    
                var graphNode = (GraphNode<Human>)context.Source;
                
                // get the actal node 
                var human = graphNode.Node;

                // get all its siblings of the node
                var collection = graphNode.Collection;
                
                // GetOrAddRelation will first check if there is a stored result for key 'friends'
                // if the result exist, it will immidately return the stored result. 
                // otherwise, it will create a new NodeCollection with the given loader function 
                var childCollection = collection.GetOrAddRelation(
                    "friends",
                    () => 
                    {
                        // get ids from all human
                        var humanIds = collection.Select(n => n.Node.HumanId).ToArray();

                        // create a node collection for 'friends' with a loader function
                        return new NodeCollection<Droid>(
                            () => 
                            {
                                Console.WriteLine("Loading all friends for humans");

                                // call entity frameworks to get all friends for given all human
                                return db.Droids.
                                    Where(d => d.Friends.Any(f => humanIds.Contains(f.HumanId))).
                                    Include(d => d.Friends).
                                    ToList();
                            });
                    });

                // look for the friends for the actual node
                // notice that the sample code is looping through the whole collection to search
                // for friends for each human. this is inefficient especially if the collection is huge.
                // to solve this issue, we need to index the query result so that we can search quicker. 
                // for example, make a dictionary using HumanId and search on the dictionary instead.
                return childCollection.Where(d => d.Node.Friends.Any(f => f.HumanId == human.HumanId));
            }
        );
    }
}
```