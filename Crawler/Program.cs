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

            Crawler.StartAndWait(frontier, index, filter, 10);

            DateTime end = DateTime.Now;
            Console.WriteLine("Done in {0}", (end - start).TotalSeconds);

            Console.ReadKey(true);
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
