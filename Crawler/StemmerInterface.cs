using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawler
{
    public interface StemmerInterface
    {
        string StemTerm(string s);
    }

    public static class StemmerExtension
    {
        public static IEnumerable<string> GetAllStems(this StemmerInterface stemmer, string document)
        {
            return GetAllStems(stemmer, document, x => x.Split(' '));
        }

        public static IEnumerable<string> GetAllStems(this StemmerInterface stemmer, string document, Func<string, IEnumerable<string>> splitter)
        {
            foreach (var s in splitter(getContent(document)))
                yield return stemmer.StemTerm(s);
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
