# ResolveGraphQL
An example on how to solving N+1 problem for GraphQL using graphql-dotnet

# Setup .Net core
This exmaple uses .net core. You can get it and learn more about it from the link below:
https://www.microsoft.com/net/core#windows

# Setup Entity Framework
I use entity framework + sqlite to create a simple database to run queries. 

You need to run commands below if you are running the code for the first time.  
### 1. Create migration file
dotnet ef migrations add InitialSetup
### 2. Update database with generated schema
dotnet ef database update

If you want to recreate the database, run following commands to drop the db 
first, then you can run previous commands to create it again.
### 1. Drop the database first
dotnet ef database drop
### 2. Remove the migration file
dotnet ef migrations remove