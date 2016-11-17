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
        private readonly ConcurrentDictionary<NodeCollectionIndexer, object> _relations = 
            new ConcurrentDictionary<NodeCollectionIndexer, object>();

        private readonly GraphNode<T>[] _nodes;

        private readonly Dictionary<object, List<GraphNode<T>>> _index;

        public NodeCollection(IEnumerable<T> nodes, NodeCollectionIndexer<T> indexer = null)
        {
            _nodes = nodes.Select(n => new GraphNode<T>(n, this)).ToArray();
            if (indexer != null)
                _index = indexer.Index(_nodes);
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
            NodeCollectionIndexer<TC> indexer, Func<NodeCollectionIndexer<TC>, NodeCollection<TC>> childCollectionLoader)
        {
            return (NodeCollection<TC>)_relations.GetOrAdd(indexer, rn => childCollectionLoader((NodeCollectionIndexer<TC>)rn));
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

    public abstract class NodeCollectionIndexer
    {
        
    }

    public class NodeCollectionIndexer<TTo> : NodeCollectionIndexer
    {
        private readonly Action<GraphNode<TTo>, Dictionary<object, List<GraphNode<TTo>>>> _indexFunc;

        public NodeCollectionIndexer(Action<GraphNode<TTo>, Dictionary<object, List<GraphNode<TTo>>>> indexFunc)
        {
            _indexFunc = indexFunc;
        }

        public Dictionary<object, List<GraphNode<TTo>>> Index(IEnumerable<GraphNode<TTo>> nodes)
        {
            var index = new Dictionary<object, List<GraphNode<TTo>>>();
            foreach(var node in nodes)
                _indexFunc(node, index);
            
            return index;
        }
    }
}