﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeadDog;
using System.Text.RegularExpressions;
using System.Net;
using WebCrawler.Filtering;

namespace WebCrawler
{
    class Program
    {
        private static ISimilarityComparer<Document> similarity;
        private static ThreadedFrontier frontier;

        private static Index index = new Index(new PorterStemmer());

        static void Main(string[] args)
        {
            Console.WindowWidth += 50;

            similarity = new HashJaccardSimilarity<Document>(4);
            frontier = new ThreadedFrontier(new Exclusions());
            frontier.Add(new URL("http://en.wikipedia.org/wiki/Teenage_Mutant_Ninja_Turtles"));

            Filter filter = new DomainFilter("en.wikipedia.org") & new ExtentionFilter(false, "jpg", "jpeg", "gif", "png", "rar", "zip", "exe", "pdf");

            DateTime start = DateTime.Now;
            while (index.SiteCount < 30)
            {
                var doc = frontier.Next();
                Console.WriteLine("{0}", doc.URL);
                Console.WriteLine("Loading {0} shingles...", doc.HTML.Split(' ').Length);
                similarity.LoadShingles(doc, doc.HTML);

                Console.WriteLine("Determining similarities...");
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
                    Console.WriteLine("Extracting links...");
                    var links = GetLinks(doc.URL, doc.HTML).ToArray();

                    int c = 0;
                    foreach (var l in links)
                        if (filter.Allow(l))
                        {
                            frontier.Add(l);
                            c++;
                        }
                    WriteColorLine("Found {0} links, added {1} to frontier", ConsoleColor.Cyan, links.Length, c);
                }
                Console.WriteLine();
            }
            DateTime end = DateTime.Now;
            Console.WriteLine("Done in {0}", (end - start).TotalSeconds);

            Console.ReadKey(true);
            frontier.Kill();
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
