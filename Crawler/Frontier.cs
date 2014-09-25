using DeadDog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class Frontier
    {
        private int nextID = 0;
        private SimpleSortedList allIKnow;
        private Queue<Document> elements;

        public Frontier()
        {
            this.allIKnow = new SimpleSortedList();
            this.elements = new Queue<Document>();
        }

        public bool Empty
        {
            get { return elements.Count == 0; }
        }

        public void Add(URL item)
        {
            int index = allIKnow.IndexOf(item);

            if (index != -1)
            {
                this.elements.Enqueue(allIKnow[index]);
            }
            else
            {
                Document doc = new Document(nextID++, item);
                this.elements.Enqueue(doc);
                this.allIKnow.Add(doc);
            }
        }
        public bool Contains(Document item)
        {
            foreach (var v in elements)
                if (v.Equals(item))
                    return true;
            return false;
        }
        public bool Contains(URL url)
        {
            foreach (var v in elements)
                if (v.URL.Equals(url))
                    return true;
            return false;
        }

        public Document Next()
        {
            return elements.Dequeue();
        }

        private class SimpleSortedList : IEnumerable<Document>
        {
            private List<Document> list;

            public SimpleSortedList()
            {
                this.list = new List<Document>();
            }

            private int searchBinary(URL url)
            {
                return list.BinarySearch(url, (d1, d2) => d1.Address.CompareTo(d2.Address), doc => doc.URL);
            }

            public void Add(Document doc)
            {
                int index = searchBinary(doc.URL);
                if (index < 0) index = ~index;
                list.Insert(index, doc);
            }

            public int IndexOf(URL url)
            {
                int index = searchBinary(url);

                return index < 0 ? -1 : index;
            }

            public Document this[int index]
            {
                get { return list[index]; }
            }

            IEnumerator<Document> IEnumerable<Document>.GetEnumerator()
            {
                foreach (var d in list)
                    yield return d;
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                foreach (var d in list)
                    yield return d;
            }
        }
    }
}
