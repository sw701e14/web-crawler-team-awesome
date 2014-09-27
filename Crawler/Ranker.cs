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
        private StemmerInterface stemmer;
        private Dictionary<string, Dictionary<Document, double>> termFrequenciesWeigthed;
        private Dictionary<string, double> inverseDocumentFrequencies;
        private Dictionary<string, Dictionary<Document, double>> tfidf;
        private Dictionary<string, Dictionary<Document, double>> normWt;

        public Ranker(Index index, StemmerInterface stemmer)
        {
            this.index = index;
            this.stemmer = stemmer;
            this.termFrequenciesWeigthed = new Dictionary<string, Dictionary<Document, double>>();
            this.inverseDocumentFrequencies = new Dictionary<string, double>();
            this.tfidf = new Dictionary<string, Dictionary<Document, double>>();
            this.normWt = new Dictionary<string, Dictionary<Document, double>>();
            var tf = calculateTermFrequencyWeighting();
            var idf = inverseDocumentFrequencyWeighting();
            var s = termFrequencyInverseDocumentWeigthing();
            var x = normalizeVectorWeigthed();
            var test = getScores(x, getNormWTSearchQuery(getWTSearchQuery(getTfForSearchQuery("popularity in the late 1980s through the early 1990s, it gained considerable worldwide success and fame"))));
        }


        public Dictionary<Document, double> GetTopHits(string searchQuery)
        {
            throw new NotImplementedException();
        }


        private double calculateTF_WT(int count)
        {
            return 1 + Math.Log10(count);
        }

        private double calculateIDF_WT(int siteCount, int termCount)
        {
            return Math.Log10(siteCount / termCount);
        }

        private double calculateTF_IDF_WT(double TF_WT, double IDF_WT)
        {
            return TF_WT * IDF_WT;
        }

        private double calculateNormVector(double TF_IDF_WT, double vectorLength)
        {
            return TF_IDF_WT / Math.Sqrt(vectorLength);
        }

        private double calculateVectorLength(double TF_IDF_WT)
        {
            return Math.Pow(TF_IDF_WT, 2);
        }

        private double calculateVectorLength(Dictionary<string, double> wt)
        {
            double length = 0;
            foreach (var term in wt)
            {
                length += calculateVectorLength(term.Value);
            }
            return length;
        }



        //Documents in index:

        private Dictionary<string, Dictionary<Document, double>> calculateTermFrequencyWeighting()
        {
            foreach (var term in index.Stems)
            {
                Dictionary<Document, double> docs = new Dictionary<Document, double>();
                foreach (var doc in term.Value)
                {
                    docs.Add(doc.Document, calculateTF_WT(doc.Count));
                }
                termFrequenciesWeigthed.Add(term.Key, docs);

            }
            return termFrequenciesWeigthed;
        }

        private Dictionary<string, double> inverseDocumentFrequencyWeighting()
        {
            foreach (var term in index.Stems)
            {
                inverseDocumentFrequencies.Add(term.Key, calculateIDF_WT(index.SiteCount, term.Value.Count));
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
                    docs.Add(doc.Key, calculateTF_IDF_WT(doc.Value, inverseDocumentFrequencies[term.Key]));
                }
                tfidf.Add(term.Key, docs);

            }
            return tfidf;
        }

        private Dictionary<string, Dictionary<Document, double>> normalizeVectorWeigthed()
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
                            length += Math.Pow(t.Value[doc.Key], 2);
                        }
                    }
                    docs.Add(doc.Key, doc.Value / Math.Sqrt(length));
                }
                normWt.Add(term.Key, docs);
            }
            return tfidf;
        }



        //Search Query:
        
        private Dictionary<string, int> getTfForSearchQuery(string searchQuery)
        {
            Dictionary<string, int> stems = new Dictionary<string, int>();
            foreach (var term in stemmer.GetAllStems(searchQuery))
            {
                stems.Add(term.Item1, term.Item2);
            }
            return stems;
        }

        private Dictionary<string, double> getWTSearchQuery(Dictionary<string, int> tf)
        {
            Dictionary<string, double> wt = new Dictionary<string, double>();
            foreach (var term in tf)
            {
                wt.Add(term.Key, calculateTF_IDF_WT(calculateTF_WT(term.Value), calculateIDF_WT(index.SiteCount, term.Value)));
            }
            return wt;
        }

        private Dictionary<string, double> getNormWTSearchQuery(Dictionary<string, double> wt)
        {
            Dictionary<string, double> normWT = new Dictionary<string, double>();
            double vectorLength = calculateVectorLength(wt);
            foreach (var term in wt)
            {
                normWT.Add(term.Key, calculateNormVector(term.Value, vectorLength));
            }
            return normWT;
        }



        //Score:

        private Dictionary<Document, double> getScores(Dictionary<string, Dictionary<Document, double>> documentsNormWT, Dictionary<string, double> searchQueryNormWT)
        {
            Dictionary<Document, double> scores = new Dictionary<Document, double>();
            foreach (var term in searchQueryNormWT)
            {
                if (documentsNormWT.Keys.Contains(term.Key))
                {
                    foreach (var doc in documentsNormWT[term.Key])
                    {
                        if (scores.Keys.Contains(doc.Key))
                        {
                            scores[doc.Key] += term.Value * doc.Value;
                        }
                        else
                        {
                            scores.Add(doc.Key, term.Value * doc.Value);
                        }                        
                    }
                }
            }
            return scores;
        }        
    }
}
