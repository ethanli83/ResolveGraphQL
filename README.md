# ResolveGraphQL
An example on how to solving N+1 problem for GraphQL using graphql-dotnet

# Setup Entity Framework

## To create a new database
### 1. Create migration file
dotnet ef migrations add InitialSetup
### 2. Update database with generated schema
dotnet ef database update

## To drop a database
### 1. Drop the database first
dotnet ef database drop
### 2. Remove the migration file
dotnet ef migrations remove