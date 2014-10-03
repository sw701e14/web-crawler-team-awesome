using System;

namespace WebCrawler
{
    public static class TrimmingStemmer
    {
        public static TermStemmer GetStemmer(TermStemmer stemmer)
        {
            return s =>
                {
                    s = s.ToLower();
                    s = trimSymbols(s);
                    s = stemmer(s);
                    return s = trimSymbols(s);
                };
        }

        private static string trimSymbols(string input)
        {
            for (int i = 0; i < input.Length; i++)
                if (char.IsLetterOrDigit(input, i))
                    for (int j = input.Length - 1; j >= i; j--)
                        if (char.IsLetterOrDigit(input, j))
                            return input.Substring(i, j - i + 1);

            return string.Empty;
        }
    }
}
