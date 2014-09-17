using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public abstract class JaccardSimilarity<TKey, TValue> : ISimilarityComparer<TKey>
    {
        private readonly int shinglesize;
        private Dictionary<TKey, TValue[]> shingleValues;

        public JaccardSimilarity(int shinglesize)
        {
            this.shinglesize = shinglesize;
            this.shingleValues = new Dictionary<TKey, TValue[]>();
        }

        public double CalculateSimilarity(TKey keya, TKey keyb)
        {
            if (!shingleValues.ContainsKey(keya))
                throw new ArgumentException("Unknown key.", "keya");

            if (!shingleValues.ContainsKey(keyb))
                throw new ArgumentException("Unknown key.", "keyb");

            var collectiona = shingleValues[keya];
            var collectionb = shingleValues[keyb];

            var inter = (double)collectiona.Intersect(collectionb).Count();
            var union = (double)collectiona.Union(collectionb).Count();

            return inter / union;
        }

        public void LoadShingles(TKey key, string content)
        {
            if (shingleValues.ContainsKey(key))
                shingleValues.Remove(key);

            shingleValues.Add(key, GetValues(GetShingles(content, shinglesize)).ToArray());
        }

        protected abstract IEnumerable<TValue> GetValues(IEnumerable<string> shingles);

        private static IEnumerable<string> GetShingles(string input, int shinglesize)
        {
            string[] words = input.Split(' ');
            for (int i = 0; i < words.Length - shinglesize + 1; i++)
                yield return string.Join(" ", words.Skip(i).Take(shinglesize));
        }
    }
}
