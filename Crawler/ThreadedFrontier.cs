using DeadDog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class ThreadedFrontier
    {
        private Exclusions exclusions;
        private Frontier frontier;
        private Queue<URL> tempQueue;

        private System.Threading.Thread loaderThread;
        private bool killed = false;

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

        public bool Empty
        {
            get { return false; }// lock (tempQueue) return frontier.Empty && tempQueue.Count == 0; }
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
                        doc = frontier.Next();
                    else
                        System.Threading.Thread.Sleep(100);
                }

            return doc;
        }
    }
}
