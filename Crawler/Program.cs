using System;
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
        private static Frontier frontier;

        private static Index index = new Index(TrimmingStemmer.GetStemmer(PorterStemmer.StemTerm), new HashJaccardSimilarity<Document>(4));

        static void Main(string[] args)
        {
            Console.WindowWidth += 50;

            frontier = new Frontier(new Exclusions());
            frontier.Add(new URL("http://en.wikipedia.org/wiki/Teenage_Mutant_Ninja_Turtles"));

            Filter filter = new DomainFilter("en.wikipedia.org") & new ExtentionFilter(false, "jpg", "jpeg", "gif", "png", "rar", "zip", "exe", "pdf");

            DateTime start = DateTime.Now;

            Crawler.StartAndWait(frontier, index, filter, 100);

            DateTime end = DateTime.Now;
            Console.WriteLine("Crawler done in {0:0.00} sec ({1:0.00} pages per sec).", (end - start).TotalSeconds, index.SiteCount / (end - start).TotalSeconds);
            Console.WriteLine("Press any key to start querying.");
            Console.ReadKey(true);
            Console.WriteLine();

            start = DateTime.Now;
            Ranker r = new Ranker(index, TrimmingStemmer.GetStemmer(PorterStemmer.StemTerm));
            end = DateTime.Now;
            Console.WriteLine("Ranker created in {0:0.00} sec.", (end - start).TotalSeconds);

            string searchQuery = "";
            while (true)
            {
                Console.WriteLine("Query for data below. Enter an empty string to quit.");
                Console.Write("Search for: ");
                searchQuery = Console.ReadLine();
                if (searchQuery == "")
                    break;

                start = DateTime.Now;
                foreach (var doc in r.GetHits(searchQuery).OrderByDescending(x => x.Item2))
                    Console.WriteLine("Rank: {1:0.000000} for: {0}", doc.Item1.URL.Address, doc.Item2);
                end = DateTime.Now;

                Console.WriteLine("Query completed in {0:0.00} sec", (end - start).TotalSeconds);
                Console.WriteLine();
            }
            frontier.Kill();
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
