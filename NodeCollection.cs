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

        public NodeCollection(IEnumerable<T> nodes)
        {
            _nodes = nodes.Select(n => new GraphNode<T>(n, this)).ToArray();
        }

        internal IndexedNodeCollection<TC, TI> GetOrAddRelation<TC, TI>(
            NodeCollectionIndexer<TC, TI> indexer, 
            Func<NodeCollection<TC>> childCollectionLoader)
        {
            return (IndexedNodeCollection<TC, TI>)_relations.GetOrAdd(
                indexer, 
                rn => 
                {
                    var collection = childCollectionLoader();
                    return indexer.Apply(collection);
                });
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
    public class IndexedNodeCollection<T, TIndex>
    {
        private Dictionary<TIndex, GraphNode<T>[]> _index;

        public IndexedNodeCollection(Dictionary<TIndex, GraphNode<T>[]> index)
        {
            _index = index;
        }

        public GraphNode<T> GetSingleByKey(TIndex key)
        {
            return _index != null && _index.ContainsKey(key) 
                ? _index[key].SingleOrDefault()
                : default(GraphNode<T>);
        }

        public GraphNode<T>[] GetManyByKey(TIndex key)
        {
            return _index != null && _index.ContainsKey(key) 
                ? _index[key].ToArray()
                : null;
        }

    }

    public class NodeCollectionIndexer<TN, TI>
    {
        private readonly Func<NodeCollection<TN>, Dictionary<TI, GraphNode<TN>[]>> _indexFunc;
        public NodeCollectionIndexer(Func<NodeCollection<TN>, Dictionary<TI, GraphNode<TN>[]>> indexFunc)
        {
            _indexFunc = indexFunc;
        }

        public IndexedNodeCollection<TN, TI> Apply(NodeCollection<TN> collection)
        {
            return new IndexedNodeCollection<TN, TI>(_indexFunc(collection));
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