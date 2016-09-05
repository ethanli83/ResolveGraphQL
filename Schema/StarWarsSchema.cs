using System;
using GraphQL.Types;

namespace ResolveGraphQL.Schema
{
    public class StarWarsSchema : GraphQL.Types.Schema
    {
        public StarWarsSchema(Func<Type, GraphType> resolveType)
            : base(resolveType)
        {
            Query = (ObjectGraphType)resolveType(typeof (StarWarsQuery));
        }
    }
}
