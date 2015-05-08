using System.Collections.Generic;
using System.Linq;

namespace N2.Azure.Replication
{
    public class ConcurrentList<T>
    {
        protected readonly List<T> InternalList = new List<T>();
        protected readonly object Sync = new object();

        /// <summary>
        /// not thread safe
        /// </summary>
        public List<T> List
        {
            get { return InternalList; }
        }

        public void Add(T item)
        {
            lock (Sync)
            {
                InternalList.Add(item);
            }
        }

        public void Remove(T item)
        {
            lock (Sync)
            {
                InternalList.Remove(item);
            }
        }

        public bool Contains(T item)
        {
            return InternalList.Contains(item);
        }

        public int Count()
        {
            lock (Sync)
            {
                return InternalList.Count();
            }
        }

        public void Insert(int i, T item)
        {
            lock (Sync)
            {
                InternalList.Insert(i, item);
            }
        }
    }

    public class ConcurrentContentList : ConcurrentList<ContentItem>
    {
        public ConcurrentContentList(IEnumerable<ContentItem> init)
        {
            foreach (var i in init)
            {
                InternalList.Add(i);
            }
        }

        public ContentItem Get(int id)
        {
            lock (Sync)
            {
                return InternalList.SingleOrDefault(p => p.ID == id);
            }
        }
    }
}