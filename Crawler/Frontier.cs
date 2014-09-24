using DeadDog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class Frontier
    {
        private int nextID = 0;
        private List<Document> allIKnow;
        private Queue<Document> elements;

        public Frontier()
        {
            this.allIKnow = new List<Document>();
            this.elements = new Queue<Document>();
        }

        public static Frontier Load(string filename)
        {
            Frontier front = new Frontier();

            using (StreamReader reader = new StreamReader(filename))
            {
                front.nextID = int.Parse(reader.ReadLine());
                string line;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    int comma = line.IndexOf(',');
                    int id = int.Parse(line.Substring(0, comma));
                    string url = line.Substring(comma + 1);

                    Document doc = new Document(id, new DeadDog.URL(url));
                    front.allIKnow.Add(doc);
                    front.elements.Enqueue(doc);
                }
            }

            return front;
        }

        public static void Save(Frontier frontier, string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine(frontier.nextID);
                foreach (var e in frontier.allIKnow)
                    writer.WriteLine("{0},{1}", e.Id, e.URL.Address);
            }
        }

        public bool Empty
        {
            get { return elements.Count == 0; }
        }

        public void Add(URL item)
        {
            Document doc = new Document(nextID++, item);
            this.elements.Enqueue(doc);
            this.allIKnow.Add(doc);
        }
        public bool Contains(Document item)
        {
            foreach (var v in elements)
                if (v.Equals(item))
                    return true;
            return false;
        }
        public bool Contains(URL url)
        {
            foreach (var v in elements)
                if (v.URL.Equals(url))
                    return true;
            return false;
        }

        public Document Next()
        {
            return elements.Dequeue();
        }
    }
}
