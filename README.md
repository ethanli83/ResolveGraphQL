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
    // This index create function will generate a dictionay for search droid friends of human
    // It will perform better than search for droids in for loop.
    private static Func<NodeCollection<Droid>, Dictionary<object, GraphNode<Droid>[]>> _friendsIndexer = 
        nc => nc.
            Select(n => n.Node).SelectMany(h => h.Friends).
            GroupBy(h => (object)h.HumanId).
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

                // GetOrAddRelation will first check if there is a stored result for the index function
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
```
