using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class StringJaccardSimilarity<TKey> : JaccardSimilarity<TKey, string>
    {
        public StringJaccardSimilarity(int shinglesize)
            : base(shinglesize)
        {
        }

        protected override IEnumerable<string> GetValues(IEnumerable<string> shingles)
        {
            return shingles;
        }
    }
}
