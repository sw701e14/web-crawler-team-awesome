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
                    s = trimSymbols(s);
                    return s.Length < 3 ? string.Empty : s;
                };
        }

        private static string trimSymbols(string input)
        {
            for (int i = 0; i < input.Length; i++)
                if (char.IsLetter(input, i))
                    for (int j = input.Length - 1; j >= i; j--)
                        if (char.IsLetter(input, j))
                            return input.Substring(i, j - i + 1);

            return string.Empty;
        }
    }
}
