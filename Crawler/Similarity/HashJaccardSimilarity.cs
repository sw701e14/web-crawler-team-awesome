using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class HashJaccardSimilarity<TKey> : JaccardSimilarity<TKey, int>
    {
        private Func<string, int> hash;

        public HashJaccardSimilarity(int shinglesize)
            : base(shinglesize)
        {
            this.hash = x => x.GetHashCode();
        }

        protected override IEnumerable<int> GetValues(IEnumerable<string> shingles)
        {
            foreach (var s in shingles)
                yield return hash(s);
        }
    }
}
