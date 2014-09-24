using DeadDog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Filtering
{
    public class ExtentionFilter : Filter
    {
        private bool allow;
        private string[] extensions;

        public ExtentionFilter(bool allow, params string[] extensions)
        {
            this.allow = allow;
            this.extensions = extensions;
        }

        public override bool Allow(URL url)
        {
            int lastPeriod = url.Address.LastIndexOf('.');
            if (lastPeriod == 0)
                return !allow;

            string ext = url.Address.Substring(lastPeriod + 1).ToLower();

            for (int i = 0; i < extensions.Length; i++)
                if (extensions[i] == ext)
                    return allow;

            return !allow;
        }
    }
}
