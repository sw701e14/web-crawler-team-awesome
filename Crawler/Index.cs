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

        private Dictionary<string, LinkedList<DocumentReference>> stems;
        private List<Document> sites;

        public Index(StemmerInterface stemmer)
        {
            this.stemmer = stemmer;

            stems = new Dictionary<string, LinkedList<DocumentReference>>();
            sites = new List<Document>();
        }

        public Dictionary<string, LinkedList<DocumentReference>> Stems { get; }

        public void AddUrl(Document document)
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

        public IEnumerable<Document> GetDocuments()
        {
            foreach (var doc in sites)
                yield return doc;
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