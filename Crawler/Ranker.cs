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
        private Dictionary<string, Dictionary<Document, double>> termFrequenciesWeigthed;
        private Dictionary<string, double> inverseDocumentFrequencies;
        private Dictionary<string, double> tfidf;

        public Ranker(Index index)
        {
            this.index = index;
            this.termFrequenciesWeigthed = new Dictionary<string, Dictionary<Document, double>>();
            this.inverseDocumentFrequencies = new Dictionary<string, double>();
            this.tfidf = new Dictionary<string, double>();
            var tf = calculateTermFrequencyWeighting();
            var idf = inverseDocumentFrequencyWeighting();
        }


        private void termFrequency()
        {
            var s = index.Stems;
        }

        private void documentFrequency()
        {

        }

        //Returns a matrix with terms as rows and documents as columns and filled in is the tf*
        private Dictionary<string, Dictionary<Document, double>> calculateTermFrequencyWeighting()
        {
            foreach (var term in index.Stems)
            {
                Dictionary<Document, double> docs = new Dictionary<Document, double>();
                foreach (var doc in term.Value)
                {
                    docs.Add(doc.Document, 1 + Math.Log10(doc.Count));
                }
                termFrequenciesWeigthed.Add(term.Key, docs);

            }
            return termFrequenciesWeigthed;
        }

        private Dictionary<string, double> inverseDocumentFrequencyWeighting() 
        {
            foreach (var term in index.Stems)
            {
                inverseDocumentFrequencies.Add(term.Key, Math.Log10(index.SiteCount/term.Value.Count));
            }
            return inverseDocumentFrequencies;
        }

        private void termFrequencyInverseDocumentWeigthing() 
        {

        }

        private void normalizeVector() { }

        private void calculateScore() { }

    }
}
