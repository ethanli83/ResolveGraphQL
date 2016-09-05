using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public interface INodeCollection : IEnumerable
    {
        int[] GetIds();
    }

    public class NodeCollection<T> : INodeCollection, IList<T> where T : Node
    {
        private Func<T, int> _idSelector;

        private IList<T> _nodes;

        private Dictionary<string, INodeCollection> _childrenDict = new Dictionary<string, INodeCollection>();

        public NodeCollection(Func<T, int> idSelector, IEnumerable<T> collection = null)
        {
            _idSelector = idSelector;
            _nodes = collection != null ? new List<T>(collection) : new List<T>();
        }

        public NodeCollection<TC> GetChildren<TC>(
            string relation, Func<IEnumerable<T>, IEnumerable<TC>> getFunc, Func<TC, int> idSelector)
            where TC : Node
        {
            INodeCollection children;
            if (!_childrenDict.TryGetValue(relation, out children))
            {
                children = new NodeCollection<TC>(idSelector, getFunc(_nodes));
                _childrenDict.Add(relation, children);
            }

            return children as NodeCollection<TC>;
        }

        public int[] GetIds()
        {
            return _nodes.Select(_idSelector).ToArray();
        }

        public T this[int index]
        {
            get
            {
                return _nodes[index];
            }

            set
            {
                _nodes[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return _nodes.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _nodes.IsReadOnly;
            }
        }

        public void Add(T item)
        {
            item.Collection = this;
            _nodes.Add(item);
        }

        public void Clear()
        {
            _nodes.Clear();
        }

        public bool Contains(T item)
        {
            return _nodes.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _nodes.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _nodes.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            item.Collection = this;
            _nodes.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return _nodes.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _nodes.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }
    }

    public class Node
    {
        public INodeCollection Collection;
    }

    public class NodeCollectionQuery<T> : IEnumerable<T> where T : Node
    {
        private Func<NodeCollection<T>> _action;

        private Func<T, bool> _filter;

        private NodeCollection<T> _collection;

        public NodeCollectionQuery(Func<NodeCollection<T>> action, Func<T, bool> filter = null)
        {
            _action = action;
            _filter = filter;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_collection == null)
                _collection = _action();

            return _collection.Where(i => _filter == null || _filter(i)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_collection == null)
                _collection = _action();

            return _collection.Where(i => _filter == null || _filter(i)).GetEnumerator();
        }
    }
}