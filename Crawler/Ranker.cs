using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    //Abbrevations:
    //TF = Term frequency
    //IDF = Inverse document frequency
    //WT = Weigthed
    //NORM = Normalized vector
    public class Ranker
    {
        private Index index;
        private StemmerInterface stemmer;
        private Dictionary<string, Dictionary<Document, double>> TF_WT;
        private Dictionary<string, double> IDF_WT;
        private Dictionary<string, Dictionary<Document, double>> TF_IDF_WT;
        private Dictionary<string, Dictionary<Document, double>> NORM_WT;

        public Ranker(Index index, StemmerInterface stemmer)
        {
            this.index = index;
            this.stemmer = stemmer;
            this.TF_WT = getTF_WT();
            this.IDF_WT = getIDF_WT();
            this.TF_IDF_WT = getTF_IDF_WT();
            this.NORM_WT = getNORM_WT();
        }


        public Dictionary<Document, double> GetTopHits(string searchQuery)
        {
            return getScores(NORM_WT, getNORM_WTSearchQuery(getTF_IDF_WTSearchQuery(getTFSearchQuery(searchQuery)))).OrderByDescending(v => v.Value).ToDictionary(v => v.Key, v => v.Value);
        }


        //Documents in index:

        private Dictionary<string, Dictionary<Document, double>> getTF_WT()
        {
            Dictionary<string, Dictionary<Document, double>> tf_wt = new Dictionary<string, Dictionary<Document, double>>();
            foreach (var term in index.Stems)
            {
                Dictionary<Document, double> docs = new Dictionary<Document, double>();
                foreach (var doc in term.Value)
                {
                    docs.Add(doc.Document, calculateTF_WT(doc.Count));
                }
                tf_wt.Add(term.Key, docs);

            }
            return tf_wt;
        }

        private Dictionary<string, double> getIDF_WT()
        {
            Dictionary<string, double> idf_wt = new Dictionary<string, double>();
            foreach (var term in index.Stems)
            {
                idf_wt.Add(term.Key, calculateIDF_WT(index.SiteCount, term.Value.Count));
            }
            return idf_wt;
        }

        private Dictionary<string, Dictionary<Document, double>> getTF_IDF_WT()
        {
            Dictionary<string, Dictionary<Document, double>> tf_idf_wt = new Dictionary<string, Dictionary<Document, double>>();
            foreach (var term in TF_WT)
            {
                Dictionary<Document, double> docs = new Dictionary<Document, double>();
                foreach (var doc in term.Value)
                {
                    docs.Add(doc.Key, calculateTF_IDF_WT(doc.Value, IDF_WT[term.Key]));
                }
                tf_idf_wt.Add(term.Key, docs);

            }
            return tf_idf_wt;
        }

        private Dictionary<string, Dictionary<Document, double>> getNORM_WT()
        {
            Dictionary<string, Dictionary<Document, double>> norm_wt = new Dictionary<string, Dictionary<Document, double>>();
            double length = 0;
            foreach (var term in TF_IDF_WT)
            {
                Dictionary<Document, double> docs = new Dictionary<Document, double>();
                foreach (var doc in term.Value)
                {
                    foreach (var t in TF_IDF_WT)
                    {
                        if (t.Value.ContainsKey(doc.Key))
                        {
                            length += Math.Pow(t.Value[doc.Key], 2);
                        }
                    }
                    docs.Add(doc.Key, doc.Value / Math.Sqrt(length));
                }
                norm_wt.Add(term.Key, docs);
            }
            return norm_wt;
        }



        //Search Query:
        
        private Dictionary<string, int> getTFSearchQuery(string searchQuery)
        {
            Dictionary<string, int> stems = new Dictionary<string, int>();
            foreach (var term in stemmer.GetAllStems(searchQuery))
            {
                stems.Add(term.Item1, term.Item2);
            }
            return stems;
        }

        private Dictionary<string, double> getTF_IDF_WTSearchQuery(Dictionary<string, int> TF)
        {
            Dictionary<string, double> TF_IDF_WT = new Dictionary<string, double>();
            foreach (var term in TF)
            {
                TF_IDF_WT.Add(term.Key, calculateTF_IDF_WT(calculateTF_WT(term.Value), calculateIDF_WT(index.SiteCount, term.Value)));
            }
            return TF_IDF_WT;
        }

        private Dictionary<string, double> getNORM_WTSearchQuery(Dictionary<string, double> TF_IDF_WT)
        {
            Dictionary<string, double> NORM_WT = new Dictionary<string, double>();
            double vectorLength = calculateVectorLength(TF_IDF_WT);
            foreach (var term in TF_IDF_WT)
            {
                NORM_WT.Add(term.Key, calculateNormVector(term.Value, vectorLength));
            }
            return NORM_WT;
        }



        //Score:

        private Dictionary<Document, double> getScores(Dictionary<string, Dictionary<Document, double>> documentsNORM_WT, Dictionary<string, double> searchQueryNORM_WT)
        {
            Dictionary<Document, double> scores = new Dictionary<Document, double>();
            foreach (var term in searchQueryNORM_WT)
            {
                if (documentsNORM_WT.Keys.Contains(term.Key))
                {
                    foreach (var doc in documentsNORM_WT[term.Key])
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


        
        //Calculation methods:

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
    }
}
