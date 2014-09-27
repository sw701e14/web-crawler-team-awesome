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
        private Dictionary<string, Dictionary<Document, double>> tfidf;
        private Dictionary<string, Dictionary<Document, double>> normWt;

        public Ranker(Index index)
        {
            this.index = index;
            this.termFrequenciesWeigthed = new Dictionary<string, Dictionary<Document, double>>();
            this.inverseDocumentFrequencies = new Dictionary<string, double>();
            this.tfidf = new Dictionary<string, Dictionary<Document, double>>();
            this.normWt = new Dictionary<string, Dictionary<Document, double>>();
            var tf = calculateTermFrequencyWeighting();
            var idf = inverseDocumentFrequencyWeighting();
            var s = termFrequencyInverseDocumentWeigthing();
            var x = normalizeVectorWeigthed();
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

        private Dictionary<string, Dictionary<Document, double>> termFrequencyInverseDocumentWeigthing() 
        {
            foreach (var term in termFrequenciesWeigthed)
            {
                Dictionary<Document, double> docs = new Dictionary<Document, double>();
                foreach (var doc in term.Value)
                {
                    docs.Add(doc.Key, doc.Value*inverseDocumentFrequencies[term.Key]);
                }
                tfidf.Add(term.Key, docs);

            }
            return tfidf;
        }

        private Dictionary<string,Dictionary<Document,double>> normalizeVectorWeigthed() 
        {
            double length = 0;
            foreach (var term in tfidf)
            {
                Dictionary<Document, double> docs = new Dictionary<Document, double>();
                foreach (var doc in term.Value)
                {
                    foreach (var t in tfidf)
                    {
                        if (t.Value.ContainsKey(doc.Key))
                        {
                             length += Math.Pow(t.Value[doc.Key],2);
                        }
                    }
                    docs.Add(doc.Key, doc.Value / Math.Sqrt(length));
                }
                normWt.Add(term.Key, docs);
            }
            return tfidf;
        }

        private void calculateScore() { }

    }
}
