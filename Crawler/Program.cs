using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeadDog;
using System.Text.RegularExpressions;
using System.Net;

namespace Crawler
{
    class Program
    {
        private static Exclusions exclusions;
        private static ISimilarityComparer<Document> similarity;
        private static Frontier frontier;

        private static Index index = new Index(new PorterStemmer());

        static void Main(string[] args)
        {
            Console.WindowWidth += 50;

            exclusions = new Exclusions();
            similarity = new HashJaccardSimilarity<Document>(4);
            frontier = Frontier.Load("frontier.txt");
            //frontier.Add(new URL(Console.ReadLine()));
            //frontier.Add(new URL("http://sablepp.deaddog.dk/"));
            //frontier.Add(new URL("http://en.wikipedia.org/wiki/World_War_II"));

            //frontier = new Frontier();
            //frontier.Add(new URL("http://en.wikipedia.org/wiki/Teenage_Mutant_Ninja_Turtles"));
            //Frontier.Save(frontier, "frontier.txt");

            DateTime start = DateTime.Now;
            while (!frontier.Empty && index.SiteCount < 30)
            {
                var doc = frontier.Next();
                Console.WriteLine("Loading {0}", doc.URL);

                similarity.LoadShingles(doc, doc.HTML);

                bool known = false;
                foreach (var l in index.GetDocuments())
                    if (similarity.CalculateSimilarity(l, doc) >= 0.9)
                    {
                        WriteColorLine("{0} is similar to {1}.", ConsoleColor.Green, doc, l);
                        known = true;
                        break;
                    }
                if (known) continue;

                index.AddUrl(doc);

                var links = GetLinks(doc.URL, doc.HTML).ToArray();
                WriteColorLine("Found {0} links", ConsoleColor.Blue, links.Length);

                foreach (var l in links)
                    if (filter(l))
                        frontier.Add(l);
            }
            DateTime end = DateTime.Now;
            Console.WriteLine("Done in {0}", (end - start).TotalSeconds);
            Console.ReadLine();
        }

        private static bool filter(URL url)
        {
            //if (!url.Address.StartsWith("http://en.wikipedia.org"))
            //    return false;

            int lastPeriod = url.Address.LastIndexOf('.');
            if (lastPeriod > 0)
            {
                string ext = url.Address.Substring(lastPeriod).ToLower();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".png")
                    return false;
            }

            if (frontier.Contains(url))
                return false;
            if (!exclusions.CanAccess(url))
                return false;

            return true;
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

        private static void WriteColorLine(string text, ConsoleColor color, params object[] args)
        {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, args);
            Console.ForegroundColor = temp;
        }
    }
}
