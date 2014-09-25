using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class SketchJaccardSimilarity<Tkey> : JaccardSimilarity<Tkey, int>
    {
        private HashMethod[] hashes;
        private Encoding encoding;

        public SketchJaccardSimilarity(int shinglesize)
            : this(shinglesize, Encoding.Unicode)
        {
        }

        public SketchJaccardSimilarity(int shinglesize, Encoding encoding)
            : base(shinglesize)
        {
            this.hashes = HashMethods.MixCount(84).ToArray();
            this.encoding = encoding;
        }
        protected override IEnumerable<int> GetValues(IEnumerable<string> shingles)
        {
            int[] mins = new int[hashes.Length];
            for (int i = 0; i < mins.Length; i++)
                mins[i] = int.MaxValue;

            foreach (var s in shingles)
            {
                byte[] data = encoding.GetBytes(s);
                for (int i = 0; i < mins.Length; i++)
                {
                    int hash = hashes[i](data).Sum(x => (int)x);
                    if (mins[i] > hash) mins[i] = hash;
                }
            }

            return mins;
        }
    }
}
