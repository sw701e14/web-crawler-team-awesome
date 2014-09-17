using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeadDog;

namespace Crawler
{
    public class Index
    {
        private Dictionary<string, LinkedList<int>> stems;

        public Index()
        {
            stems = new Dictionary<string, LinkedList<int>>();
            sites = new SitesCollection<URL>();
        }

        public void AddStems(StemmerInterface stemmer, string document, URL url)
        {
            foreach (var term in stemmer.GetAllStems(document))
            {
                if (stems.ContainsKey(term))
                {
                    stems[term].AddLast(sites.Count + 1);
                }
                else
                {
                    LinkedList<int> l = new LinkedList<int>();
                    l.AddFirst(sites.Count + 1);
                    stems.Add(term, l);
                }
            }
            sites.Add(url);
        }

        private SitesCollection<URL> sites;

        public SitesCollection<URL> Sites
        {
            get { return sites; }
        }

        public int Count { get { return sites.Count; } }

        public class SitesCollection<T> : IEnumerable<T>
        {
            private List<T> sites;

            public SitesCollection()
            {
                this.sites = new List<T>();
            }

            public int Count { get { return sites.Count; } }

            public void Add(T item)
            {
                sites.Add(item);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return sites.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return sites.GetEnumerator();
            }
        }

    }
}