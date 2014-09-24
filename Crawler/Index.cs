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
        private StemmerInterface stemmer;

        private Dictionary<string, LinkedList<int>> stems;
        private List<URL> sites; //Evt tilføje id til typen URL så vi giver et id når den bliver tilføjet til frontier'en

        public Index(StemmerInterface stemmer)
        {
            this.stemmer = stemmer;

            stems = new Dictionary<string, LinkedList<int>>();
            sites = new List<URL>();
        }

        public void AddUrl(URL url, string document)
        {
            int index = sites.Count;
            sites.Add(url);

            foreach (var term in stemmer.GetAllStems(document))
            {
                if (stems.ContainsKey(term))
                    addToSortedList(stems[term].First, index);
                else
                {
                    LinkedList<int> l = new LinkedList<int>();
                    l.AddFirst(index);
                    stems.Add(term, l);
                }
            }
        }

        public int SiteCount
        {
            get { return sites.Count; }
        }

        private void addToSortedList(LinkedListNode<int> node, int i)
        {
            if (node.Value <= i)
            {
                if (node.Next == null)
                    node.List.AddLast(i);
                else
                    addToSortedList(node.Next, i);
            }
            else
                node.List.AddBefore(node, i);
        }

        public int GetId(URL url)
        {
            return sites.IndexOf(url);
        }
        public IEnumerable<URL> GetURLs()
        {
            foreach (var url in sites)
                yield return url;
        }
    }
}