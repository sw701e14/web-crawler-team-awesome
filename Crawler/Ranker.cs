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
        private TermStemmer stemmer;

        Dictionary<Document, double> lengths = new Dictionary<Document, double>();
        Dictionary<string, double[]> tf_idf = new Dictionary<string, double[]>();

        public Ranker(Index index, TermStemmer stemmer)
        {
            int docCount = index.SiteCount;

            foreach (var t in index.GetStems())
                tf_idf.Add(t.Key, getTF_IDF(t.Value, docCount));

            Document[] keys = lengths.Keys.ToArray();
            foreach (var d in keys)
                lengths[d] = Math.Sqrt(lengths[d]);

            this.index = index;
            this.stemmer = stemmer;
            //this.TF_WT = getTF_WT();
            //this.IDF_WT = getIDF_WT();
            //this.TF_IDF_WT = getTF_IDF_WT();
            //this.NORM_WT = getNORM_WT();
        }
        private double[] getTF_IDF(WebCrawler.Index.DocumentReference[] documents, int docCount)
        {
            double N = docCount;

            double[] values = new double[documents.Length];

            for (int i = 0; i < documents.Length; i++)
            {
                Document d = documents[i].Document;
                int c = documents[i].Count;
                if (!lengths.ContainsKey(d))
                    lengths.Add(d, c * c);
                else
                    lengths[d] += c * c;

                values[i] = (1 + Math.Log10(c)) * Math.Log10(N / documents.Length);
            }
            return values;
        }

        private IEnumerable<string> getQueryTerms(string searchQuery)
        {
            return stemmer.GetAllStems(searchQuery).Select(t => t.Item1).Distinct();
        }
        
        public IEnumerable<Tuple<Document, double>> GetHits(string searchQuery)
        {
            var terms = getQueryTerms(searchQuery).ToArray();
            var docs = new Dictionary<Document, double>();
            
            foreach (var t in terms)
            {
                if (!tf_idf.ContainsKey(t))
                    continue;

                var tf_idfs = tf_idf[t];
                var docArr = index.GetDocuments(t);

                for (int i = 0; i < docArr.Length; i++)
                {
                    var d = docArr[i].Document;
                    if (!docs.ContainsKey(d))
                        docs.Add(d, tf_idfs[i] / lengths[d]);
                    else
                        docs[d] += tf_idfs[i] / lengths[d];
                }
            }

            foreach (var v in docs)
                yield return Tuple.Create(v.Key, v.Value);
        }
    }
}
