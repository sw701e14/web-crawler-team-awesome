using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            foreach (var s in document.Split(' '))
                yield return stemmer.StemTerm(s);
        }

        public static IEnumerable<string> GetAllStems(this StemmerInterface stemmer, string document, Func<string, IEnumerable<string>> splitter)
        {
            foreach (var s in splitter(document))
                yield return stemmer.StemTerm(s);
        }
    }
}
