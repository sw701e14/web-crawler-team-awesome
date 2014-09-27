using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeadDog;
using System.Text.RegularExpressions;
using System.Net;
using Crawler.Filtering;

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

            Filter filter = new DomainFilter("en.wikipedia.org") & new ExtentionFilter(false, "jpg", "jpeg", "gif", "png", "rar", "zip", "exe", "pdf");
            filter &= new LambdaFilter(url => !frontier.Contains(url) && exclusions.CanAccess(url));

            //frontier.Add(new URL(Console.ReadLine()));
            //frontier.Add(new URL("http://sablepp.deaddog.dk/"));
            //frontier.Add(new URL("http://en.wikipedia.org/wiki/World_War_II"));

            //frontier = new Frontier();
            //frontier.Add(new URL("http://en.wikipedia.org/wiki/Teenage_Mutant_Ninja_Turtles"));
            //Frontier.Save(frontier, "frontier.txt");

            DateTime start = DateTime.Now;
            while (!frontier.Empty && index.SiteCount < 10)
            {
                var doc = frontier.Next();
                Console.WriteLine("Loading {0}", doc.URL);

                similarity.LoadShingles(doc, doc.HTML);

                bool known = false;
                foreach (var l in index.GetDocuments())
                {
                    double simi = similarity.CalculateSimilarity(l, doc);
                    if (simi >= 0.9)
                    {
                        WriteColorLine("{0:0.0}% similar to {1}", ConsoleColor.Green, simi * 100, l.URL.Address);
                        known = true;
                        break;
                    }
                }

                if (!known)
                {
                    index.AddUrl(doc);

                    var links = GetLinks(doc.URL, doc.HTML).ToArray();
                    WriteColorLine("Found {0} links", ConsoleColor.Blue, links.Length);

                    foreach (var l in links)
                        if (filter.Allow(l))
                            frontier.Add(l);
                }
                Console.WriteLine();
            }
            DateTime end = DateTime.Now;
            Console.WriteLine("Done in {0}", (end - start).TotalSeconds);

            Ranker r = new Ranker(index, new PorterStemmer());
            Console.WriteLine("Search: ");
            string searchQuery = Console.ReadLine();
            foreach (var doc in r.GetTopHits(searchQuery))
            {
                Console.WriteLine(doc.Key.URL + " " + doc.Value);
            }
            Console.ReadKey();
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
