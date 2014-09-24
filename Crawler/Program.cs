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
        private static ISimilarityComparer<URL> similarity;
        private static Frontier<URL> frontier;

        private static Index index = new Index(new PorterStemmer());

        static void Main(string[] args)
        {
            exclusions = new Exclusions();
            similarity = new HashJaccardSimilarity<URL>(4);
            frontier = new Frontier<URL>();

            //frontier.Add(new URL(Console.ReadLine()));
            //frontier.Add(new URL("http://sablepp.deaddog.dk/"));
            //frontier.Add(new URL("http://en.wikipedia.org/wiki/World_War_II"));
            frontier.Add(new URL("http://en.wikipedia.org/wiki/Teenage_Mutant_Ninja_Turtles"));

            DateTime start = DateTime.Now;
            while (!frontier.Empty && index.SiteCount < 30)
            {
                var link = frontier.Next();
                Console.WriteLine("Loading {0}", link.Address);

                string html;
                try { html = link.GetHTML(true); }
                catch { continue; }

                similarity.LoadShingles(link, html);

                bool known = false;
                foreach (var l in index.GetURLs())
                    if (similarity.CalculateSimilarity(l, link) >= 0.9)
                    {
                        known = true;
                        break;
                    }
                if (known) continue;

                index.AddUrl(link, html);

                var links = GetLinks(link.Address, html).ToArray();
                WriteColorLine("Found {0} links", ConsoleColor.Blue, links.Length);

                foreach (var l in links)
                    if (filter(l) && !frontier.Contains(l) && exclusions.CanAccess(l))
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

            return true;
        }

        private static IEnumerable<URL> GetLinks(string origin, string html)
        {
            var matches = Regex.Matches(html, "<a[^>]+");

            for (int i = 0; i < matches.Count; i++)
            {
                string url = matches[i].Value;

                url = getUrl(url);

                if (url != null)
                    url = normalize(origin, url);

                if (url != null)
                    yield return new URL(url);
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
        private static string normalize(string origin, string url)
        {
            string domain = Regex.Match(origin, "https?://[^/]*").Value;

            if (url.StartsWith("//"))
                url = domain.Substring(0, domain.IndexOf(':')) + ":" + url;

            if (!url.StartsWith("http") && !url.StartsWith("/"))
            {
                string prefix = origin.Substring(8).Contains('/') ? origin.Substring(0, origin.LastIndexOf('/')) : origin;
                url = prefix + "/" + url;
            }

            if (url.StartsWith("http"))
            {
                domain = Regex.Match(url, "https?://[^/]*").Value;
                url = url.Substring(domain.Length);
            }

            domain = domain.ToLower();

            Regex.Replace(url, "%[a-zA-Z0-9][a-zA-Z0-9]", m => replaceEncoding(m.Value.ToUpper()));

            return domain + url;
        }
        private static string replaceEncoding(string symbol)
        {
            int i = int.Parse(symbol.Substring(1), System.Globalization.NumberStyles.HexNumber);

            if (
                (i >= 0x41 && i <= 0x5A) ||
                (i >= 0x61 && i <= 0x7A) ||
                (i >= 0x30 && i <= 0x39) ||
                i == 0x2D ||
                i == 0x2E ||
                i == 0x5F ||
                i == 0x7E
               )
                return ((char)i).ToString();
            else
                return symbol;
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
