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
        private IStemmer stemmer;

        private Dictionary<string, LinkedList<DocumentReference>> stems;
        private List<Document> sites;

        private ISimilarityComparer<Document> similarity;

        public Index(IStemmer stemmer, ISimilarityComparer<Document> similarity)
        {
            this.stemmer = stemmer;
            this.similarity = similarity;

            stems = new Dictionary<string, LinkedList<DocumentReference>>();
            sites = new List<Document>();
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
            return new Index(new PorterStemmer(), copyFrom.similarity);
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
                        addToSortedList(stems[term.Item1].First, reference);
                    else
                    {
                        LinkedList<DocumentReference> l = new LinkedList<DocumentReference>();
                        l.AddFirst(reference);
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

        private void addToSortedList(LinkedListNode<DocumentReference> node, DocumentReference reference)
        {
            if (node.Value.Document.Id <= reference.Document.Id)
            {
                if (node.Next == null)
                    node.List.AddLast(reference);
                else
                    addToSortedList(node.Next, reference);
            }
            else
                node.List.AddBefore(node, reference);
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