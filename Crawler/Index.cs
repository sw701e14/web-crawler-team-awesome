using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeadDog;

namespace WebCrawler
{
    public class Index
    {
        private TermStemmer stemmer;

        private Dictionary<string, List<DocumentReference>> stems;
        private List<Document> sites;

        private ISimilarityComparer<Document> similarity;

        public Index(TermStemmer stemmer, ISimilarityComparer<Document> similarity)
        {
            this.stemmer = stemmer;
            this.similarity = similarity;

            stems = new Dictionary<string, List<DocumentReference>>();
            sites = new List<Document>();
        }

        public IEnumerable<KeyValuePair<string, DocumentReference[]>> GetStems()
        {
            foreach (var s in stems)
                yield return new KeyValuePair<string, DocumentReference[]>(s.Key, s.Value.ToArray());
        }

        public DocumentReference[] GetDocuments(string stem)
        {
            if (!stems.ContainsKey(stem))
                return new DocumentReference[0];
            else
                return stems[stem].ToArray();
        }

        public void MergeIn(Index index)
        {
            List<Document> unique = new List<Document>(index.sites);

            for (int i = 0; i < unique.Count; i++)
                foreach (var s in sites)
                {
                    double simi = similarity.CalculateSimilarity(s, unique[i]);
                    if (simi >= 0.9)
                    {
                        unique.RemoveAt(i--);
                        break;
                    }
                }

            foreach (var doc in unique)
                sites.Add(doc);

            foreach (var term in index.stems.Keys)
            {
                if (!stems.ContainsKey(term))
                    stems.Add(term, index.stems[term]);
                else
                    stems[term].MergeInto(index.stems[term], (a, b) => a.Document.Id.CompareTo(b.Document.Id), d => !unique.Contains(d.Document));
            }
        }

        public static Index CreateEmptyCopy(Index copyFrom)
        {
            return new Index(copyFrom.stemmer, copyFrom.similarity);
        }

        public bool TryAddUrl(Document document)
        {
            similarity.LoadShingles(document, document.HTML);

            bool known = false;

            foreach (var s in sites)
            {
                double simi = similarity.CalculateSimilarity(s, document);
                if (simi >= 0.9)
                {
                    known = true;
                    break;
                }
            }

            if (!known)
            {
                sites.Add(document);

                foreach (var term in stemmer.GetAllStems(document.HTML))
                {
                    DocumentReference reference = new DocumentReference(document, term.Item2);
                    if (stems.ContainsKey(term.Item1))
                    {
                        var list = stems[term.Item1];
                        int index = list.BinarySearch(reference.Document.Id, (x, y) => x.CompareTo(y), doc => doc.Document.Id);

                        if (index > 0)
                            throw new NotImplementedException("Index is not yet capable of handling count updates for documents.");
                        else
                            list.Insert(~index, reference);
                    }
                    else
                    {
                        List<DocumentReference> l = new List<DocumentReference>();
                        l.Add(reference);
                        stems.Add(term.Item1, l);
                    }
                }
            }

            return !known;
        }

        public int SiteCount
        {
            get { return sites.Count; }
        }

        public class DocumentReference
        {
            private Document doc;
            private int count;

            public DocumentReference(Document doc, int count)
            {
                this.doc = doc;
                this.count = count;
            }

            public Document Document
            {
                get { return doc; }
            }
            public int Count
            {
                get { return count; }
            }
        }
    }
}