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
        private readonly ConcurrentDictionary<object, object> _relations = new ConcurrentDictionary<object, object>();

        private readonly GraphNode<T>[] _nodes;

        private Dictionary<object, GraphNode<T>[]> _index;

        public NodeCollection(IEnumerable<T> nodes)
        {
            _nodes = nodes.Select(n => new GraphNode<T>(n, this)).ToArray();
        }

        public GraphNode<T> GetSingleByKey(object key)
        {
            return _index != null && _index.ContainsKey(key) 
                ? _index[key].SingleOrDefault()
                : default(GraphNode<T>);
        }

        public GraphNode<T>[] GetManyByKey(object key)
        {
            return _index != null && _index.ContainsKey(key) 
                ? _index[key].ToArray()
                : null;
        }

        internal NodeCollection<TC> GetOrAddRelation<TC>(
            Func<NodeCollection<TC>, Dictionary<object, GraphNode<TC>[]>> indexFunc, 
            Func<NodeCollection<TC>> childCollectionLoader)
        {
            return (NodeCollection<TC>)_relations.GetOrAdd(
                indexFunc, 
                rn => 
                {
                    var collection = childCollectionLoader();
                    collection.ApplyIndex(indexFunc);
                    return collection;
                });
        }

        private void ApplyIndex(Func<NodeCollection<T>, Dictionary<object, GraphNode<T>[]>> indexFunc)
        {
            _index = indexFunc(this);
        }

        public IEnumerator<GraphNode<T>> GetEnumerator()
        {
            return ((IEnumerable<GraphNode<T>>)_nodes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _nodes.GetEnumerator();
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

        public static T GetGraphNode<T>(this ResolveFieldContext<GraphNode<T>> context)
        {
            return context.Source.Node;
        }

        public static NodeCollection<T> GetNodeCollection<T>(this ResolveFieldContext context)
        {
            return ((GraphNode<T>)context.Source).Collection;
        }

        public static NodeCollection<T> GetNodeCollection<T>(this ResolveFieldContext<GraphNode<T>> context)
        {
            return context.Source.Collection;
        }
    }
}