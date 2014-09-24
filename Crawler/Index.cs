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
        private List<URL> sites; //Evt tilføje id til typen URL så vi giver et id når den bliver tilføjet il frontier'en

        public Index()
        {
            stems = new Dictionary<string, LinkedList<int>>();
            sites = new List<URL>();
        }

        private void addStems(StemmerInterface stemmer, URL document)
        {
            foreach (var term in stemmer.GetAllStems(document.GetHTML()))
            {
                if (stems.ContainsKey(term))
                {
                    stems[term].AddLast(1);
                }
                else
                {
                    LinkedList<int> l = new LinkedList<int>();
                    l.AddFirst(1);
                    stems.Add(term, l);
                }
            }            
        }

        public void AddUrl(URL url)
        {
            sites.Add(url);
        }

        public int GetId(URL url)
        {
            return sites.IndexOf(url);
        }
        
    }
}