using DeadDog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class ThreadedFrontier
    {
        private Exclusions exclusions;
        private Frontier frontier;
        private Queue<URL> tempQueue;

        private System.Threading.Thread loaderThread;
        private bool killed = false;
        private bool killReady = false;

        public ThreadedFrontier(Exclusions exclusions)
        {
            this.exclusions = exclusions;
            this.frontier = new Frontier();
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

            URL item;
            lock (tempQueue) item = tempQueue.Dequeue();

            if (!exclusions.CanAccess(item))
                return;

            lock (frontier)
                if (frontier.Contains(item))
                    return;

            frontier.Add(item);
        }

        public void Add(URL item)
        {
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
                        doc = frontier.Next();
                        killReady = false;
                    }
                    else if (tempQueue.Count == 0)
                    {
                        if (!killReady)
                        {
                            killReady = true;
                            System.Threading.Thread.Sleep(500);
                        }
                        else
                            return null;
                    }
                    else
                    {
                        killReady = false;
                        System.Threading.Thread.Sleep(100);
                    }
                }

            return doc;
        }
    }
}
