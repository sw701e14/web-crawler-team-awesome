using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class Frontier<T> where T : IEquatable<T>
    {
        private Queue<T> elements;

        public Frontier()
        {
            this.elements = new Queue<T>();
        }

        public bool Empty
        {
            get { return elements.Count == 0; }
        }

        public void Add(T item)
        {
            this.elements.Enqueue(item);
        }
        public bool Contains(T item)
        {
            foreach (var v in elements)
                if (v.Equals(item))
                    return true;
            return false;
        }

        public T Next()
        {
            return elements.Dequeue();
        }
    }
}
