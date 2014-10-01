using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebCrawler
{
    public static class StemmerExtension
    {
        public static IEnumerable<Tuple<string, int>> GetAllStems(this TermStemmer stemmer, string document)
        {
            return GetAllStems(stemmer, document, x => x.Split(' '));
        }

        public static IEnumerable<Tuple<string, int>> GetAllStems(this TermStemmer stemmer, string document, Func<string, IEnumerable<string>> splitter)
        {
            string last = null;
            int count = 0;
            foreach (var s in getStemsInOrder(stemmer, splitter(getContent(document))))
            {
                if (last != s)
                {
                    if (last != null)
                        yield return Tuple.Create(last, count);

                    last = s;
                    count = 1;
                }
                else
                    count++;
            }

            if (last != null)
                yield return Tuple.Create(last, count);
        }

        private static IEnumerable<string> getStemsInOrder(TermStemmer stemmer, IEnumerable<string> collection)
        {
            return from e in collection
                   let term = stemmer(e).Trim('\0', ' ', '\t', '\r', '\n')
                   where term.Length > 0
                   orderby term
                   select term;
        }

        private static string getContent(string document)
        {
            HtmlNode doc;
            {
                HtmlDocument htmldoc = new HtmlDocument();
                htmldoc.LoadHtml(document);
                doc = htmldoc.DocumentNode;
            }

            return Regex.Replace(Regex.Replace(getContent(doc), @"[\n\r][ \t\n\r]*[\n\r]", "").Replace('\t', ' '), "  +", " ").Trim();
        }
        private static string getContent(HtmlNode node)
        {
            string res = "";

            if (!allowElement(node))
                return res;

            switch (node.NodeType)
            {
                case HtmlNodeType.Comment: break;

                case HtmlNodeType.Document:
                case HtmlNodeType.Element:
                    foreach (var c in node.ChildNodes)
                        res += getContent(c);
                    break;

                case HtmlNodeType.Text:
                    res += node.InnerHtml;
                    break;
            }

            return res;
        }
        private static bool allowElement(HtmlNode node)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment: return false;
                case HtmlNodeType.Document: return true;
                case HtmlNodeType.Element:
                    {
                        switch (node.Name)
                        {
                            case "head":
                            case "script":
                                return false;
                            default:
                                return true;
                        }
                    }

                case HtmlNodeType.Text: return true;

                default:
                    return false;
            }
        }
    }
}
