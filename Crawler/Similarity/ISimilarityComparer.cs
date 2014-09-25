using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    public interface ISimilarityComparer<TKey>
    {
        double CalculateSimilarity(TKey keya, TKey keyb);
        void LoadShingles(TKey key, string content);
    }
}
