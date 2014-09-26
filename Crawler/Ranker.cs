using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class Ranker
    {
        private Index index;
        private Dictionary<string, Dictionary<Document, decimal>> termFrequencies;

        public Ranker(Index index)
        {
            this.index = index;
            this.termFrequencies = new Dictionary<string, Dictionary<Document, decimal>>();
            var tf = calculateTermFrequencyWeighting();
        }


        private void termFrequency()
        {
            var s = index.Stems;
        }

        private void documentFrequency()
        {

        }

        //Returns a matrix with terms as rows and documents as columns and filled in is the tf*
        private Dictionary<string, Dictionary<Document, decimal>> calculateTermFrequencyWeighting()
        {
            foreach (var term in index.Stems)
            {
                Dictionary<Document, decimal> docs = new Dictionary<Document, decimal>();
                foreach (var doc in term.Value)
                {
                    docs.Add(doc.Document, Convert.ToDecimal(1 + Math.Log10(doc.Count)));
                }
                termFrequencies.Add(term.Key, docs);

            }
            return termFrequencies;
        }

        private void inverseDocumentFrequencyWeighting() { }

        private void termFrequencyInverseDocumentWeigthing() { }

        private void normalizeVector() { }

        private void calculateScore() { }

    }
}
