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

This sample code trys to improve the perform of resolving graphql by only calling database once per child property. For example, assuming in the StarWar database we have 3 human and each of them has 2 friends. When we query the data by:
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
We only want to call the database once for resolving 'friends', rather than calling database 3 times.



