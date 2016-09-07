using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;

namespace ResolveGraphQL
{
    public class NodeCollection<T> : IEnumerable<GraphNode<T>>
    {
        private ConcurrentDictionary<string, object> _relations = new ConcurrentDictionary<string, object>();

        private GraphNode<T>[] _nodes;

        private Func<IEnumerable<T>> _nodeLoader;

        public NodeCollection(Func<IEnumerable<T>> nodeLoader)
        {
            _nodeLoader = nodeLoader;
        }

        private object _loaderLock = new object();
        protected IEnumerable<GraphNode<T>> Nodes 
        {
            get 
            {
                if (_nodes == null)
                {
                    lock(_loaderLock)
                    {
                        if (_nodes == null)
                        {
                            var result = _nodeLoader();
                            _nodes = result.Select(n => new GraphNode<T>(n, this)).ToArray();
                        } 
                    }
                }

                return _nodes;
            }
        }

        internal NodeCollection<TC> GetOrAddRelation<TC>(string relationName, Func<NodeCollection<TC>> childCollectionLoader)
        {
            return (NodeCollection<TC>)_relations.GetOrAdd(relationName, rn => childCollectionLoader());
        }

        public IEnumerator<GraphNode<T>> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }
    }

    public class GraphNode<T>
    {
        public GraphNode(T node, NodeCollection<T> collection)
        {
            Node = node;
            Collection = collection;
        }

        public T Node { get; protected set; }

        public NodeCollection<T> Collection { get; protected set; }
    }

    public static class GraphNodeExtensions
    {
        public static T GetGraphNode<T>(this ResolveFieldContext context)
        {
            return ((GraphNode<T>)context.Source).Node;
        }

        public static NodeCollection<T> GetNodeCollection<T>(this ResolveFieldContext context)
        {
            return ((GraphNode<T>)context.Source).Collection;
        }
    }
}