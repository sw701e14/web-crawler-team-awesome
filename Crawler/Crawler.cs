using DeadDog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class Crawler
    {
        public static void StartAndWait(Frontier frontier, Index index, Filtering.Filter filter, int count)
        {
            Spider[] spiders = new Spider[count];
            Thread[] threads = new Thread[count];

            for (int i = 0; i < count; i++)
            {
                Spider sp = spiders[i] = new Spider(frontier, index, filter, ind =>
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Merging Index");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    lock (index) { index.MergeIn(ind); }
                });
                threads[i] = new Thread(() => sp.Run());
                threads[i].Start();
            }

            for (int i = 0; i < count; i++)
                threads[i].Join();
        }

        private class Spider
        {
            private Frontier frontier;
            private Index index;
            private Filtering.Filter filter;
            private Action<Index> callback;

            public Spider(Frontier frontier, Index index, Filtering.Filter filter, Action<Index> callback)
            {
                this.frontier = frontier;
                this.index = Index.CreateEmptyCopy(index);
                this.filter = filter;
                this.callback = callback;
            }

            public void Run()
            {
                int count = 0;
                Document doc = null;
                while (doc == null) doc = frontier.Next();
                while (doc != null)
                {
                    if (index.TryAddUrl(doc))
                    {
                        var links = GetLinks(doc.URL, doc.HTML).ToArray();

                        int c = 0;
                        foreach (var l in links)
                            if (filter.Allow(l))
                            {
                                frontier.Add(l);
                                c++;
                            }
                    }
                    Console.WriteLine("{0}", doc.URL);

                    count++;
                    if (count == 10)
                        break;
                    doc = frontier.Next();
                }

                callback(this.index);
            }

            private static IEnumerable<URL> GetLinks(URL origin, string html)
            {
                var matches = Regex.Matches(html, "<a[^>]+");

                for (int i = 0; i < matches.Count; i++)
                {
                    string url = matches[i].Value;

                    url = getUrl(url);

                    if (url != null)
                    {
                        try { yield return origin.GetURLFromLink(url); }
                        finally { /* This is only here so that try is allowed */ }
                    }
                }
            }
            private static string getUrl(string anchor)
            {
                Match m;
                m = Regex.Match(anchor, "href=\"(?<url>[^\"]+)");
                if (!m.Success)
                    return null;

                string href = m.Groups["url"].Value;
                m = Regex.Match(href, "(?<pretag>[^#]*)#[^#]*");
                if (m.Success)
                    href = m.Groups["pretag"].Value;

                if (href.Length == 0)
                    return null;

                return href;
            }

        }
    }
}
