# ResolveGraphQL
An example of how to solve the N+1 problem for GraphQL using graphql-dotnet

# Setup .Net core
This exmaple uses .net core. You can get it and learn more about it from
https://www.microsoft.com/net/core

# Setup Entity Framework
I use entity framework + sqlite to create a simple database to run queries.

You will need to run the commands below if you are running the code for the
first time.

### 0. Restore packages

    dotnet restore

### 1. Create migration file

    dotnet ef migrations add InitialSetup

### 2. Update database with generated schema

    dotnet ef database update

If you want to recreate the database, run the following commands to drop the db
first, then you can run the previous commands to create it again.

### 1. Drop the database first

    dotnet ef database drop

### 2. Remove the migration file

    dotnet ef migrations remove

# The concept of the solution.
In the original resolving process, siblings are resolved individually.
Which means that even though all of the children of some given siblings
are stored in the same table, we will call the database once for each sibling,
hitting the same table over and over. This is inefficient for most databases
such as mssql, or sqlite.

This sample code tries to improve the performance of resolving graphql by
only calling the database once per child property. For example, assuming,
in the StarWars database, we have 3 humans and each of them has 2 friends.

When we resolve this graphql

```graphql
query HumansQuery {
    humans {
        name
        friends {
            name
        }
    }
}
```

We only want to call the database once for resolving `friends`, rather than
calling the database 3 times.

To achieve this goal, we need two pieces of information when we resolve a
child property of a sibling. Firstly, we need to have access to all of the
siblings so that we can compose a query to the database containing all the
parent ids (that is, the ids of the siblings). Secondly, we need a way to
tell if the query for the child property has already been called to ensure
we only call the database once.

In this code, we have two core classes, namely:
`GraphNode<T>` and `NodeCollection<T>`.

`GraphNode<T>` has a property called `Collection` which contains all of the
siblings of the Node.

`NodeCollection<T>` has a loader function, and a private field `_nodes`.
This class will only call the loader function once to fill the `_nodes` field
and will not call the loader again if `_nodes` has already been initialized.

`NodeCollection<T>` also has a private dictionary called `_relations`.
This dictionary stores all of the resolved child properties. When resolving a
child, we first check the dictionary to see if the child has already been resolved.
If it has, we grab the stored result rather than calling the database again.

With help from these two classes, we achieve the goal of only calling the
database once per child property.

Now, let's have a look at a real example.

Here's how the resolve function of the `friends` field is implemented for `HumanType`:

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
                // First get the graph node which must be a GraphNode<Human>
                var graphNode = (GraphNode<Human>)context.Source;

                // The resolve function is called once for each human while resolving 'friends';
                // this node is the current human for which 'friends' is being resolved.
                var human = graphNode.Node;

                // Get all of the siblings of the node
                var collection = graphNode.Collection;

                // GetOrAddRelation will first check if there is a stored result for the key 'friends'
                // If the result exists, it will immediately return the stored result.
                // Otherwise, it will create a new NodeCollection using the given loader function
                var childCollection = collection.GetOrAddRelation(
                    "friends",
                    () =>
                    {
                        // get the ids from all the humans
                        var humanIds = collection.Select(n => n.Node.HumanId).ToArray();

                        // create a node collection for 'friends' with a loader function
                        return new NodeCollection<Droid>(
                            () =>
                            {
                                Console.WriteLine("Loading all friends for humans");

                                // call entity framework to get all the friends for all the given humans
                                return db.Droids.
                                    Where(d => d.Friends.Any(f => humanIds.Contains(f.HumanId))).
                                    Include(d => d.Friends).
                                    ToList();
                            });
                    });

                // Look for the friends of only the current human.
                // Notice that this sample code is looping through the whole collection to search
                // for the friends for each human. This is inefficient, especially if the collection is huge.
                // Normally we should index the query result so that we don't have to do a full search every
                // time. For example, make a dictionary using HumanId and search the dictionary instead.
                return childCollection.Where(d => d.Node.Friends.Any(f => f.HumanId == human.HumanId));
            }
        );
    }
}
```
