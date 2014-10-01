using DeadDog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class Frontier
    {
        private Exclusions exclusions;
        private InnerFrontier frontier;
        private Queue<URL> tempQueue;

        private System.Threading.Thread loaderThread;
        private bool killed = false;
        private bool inProgress = false;
        private bool empty = true;
        private object emptyObject = new object();

        public Frontier(Exclusions exclusions)
        {
            this.exclusions = exclusions;
            this.frontier = new InnerFrontier();
            this.tempQueue = new Queue<URL>();

            this.loaderThread = new System.Threading.Thread(() => { while (!killed)loadTemp(); });
            this.loaderThread.Start();
        }

        public void Kill()
        {
            killed = true;
        }

        private void loadTemp()
        {
            while (tempQueue.Count == 0) { System.Threading.Thread.Sleep(100); if (killed)return; }

            inProgress = true;

            URL item;
            lock (tempQueue) item = tempQueue.Dequeue();

            if (!exclusions.CanAccess(item))
                return;

            lock (frontier)
                if (frontier.Contains(item))
                    return;

            lock (frontier)
                frontier.Add(item);

            inProgress = false;
        }

        public void Add(URL item)
        {
            lock (emptyObject)
                empty = false;

            lock (tempQueue)
                tempQueue.Enqueue(item);
        }

        public Document Next()
        {
            Document doc = null;
            while (doc == null)
                lock (frontier)
                {
                    if (!frontier.Empty)
                    {
                        lock (emptyObject)
                        {
                            doc = frontier.Next();
                            empty = frontier.Empty && tempQueue.Count == 0;
                        }
                    }
                    else if (empty && !inProgress)
                        return null;
                }

            return doc;
        }
    }
}
