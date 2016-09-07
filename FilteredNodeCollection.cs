using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ResolveGraphQL
{
    public class FilteredNodeCollection<T> : IEnumerable<GraphNode<T>> where T : new()
    {
        private Func<T, bool> _filter;

        private NodeCollection<T> _collection;

        public FilteredNodeCollection(NodeCollection<T> collection, Func<T, bool> filter = null)
        {
            _collection = collection;
            _filter = filter;
        }

        protected IEnumerable<GraphNode<T>> Collection 
        {
            get 
            {
                return _collection.Where(n => _filter == null || _filter(n.Node));
            }
        }

        public IEnumerator<GraphNode<T>> GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Collection.GetEnumerator();
        }
    }

    public class FilteredGraphNode<T> : GraphNode<T> where T : new()
    {
        public FilteredGraphNode(NodeCollection<T> collection, Func<T, bool> filter = null)
        {
            Collection = collection;
            Node = collection.FirstOrDefault(n => filter == null || filter(n.Node)).Node;
        }
    }
}