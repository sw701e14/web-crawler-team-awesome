using DeadDog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Filtering
{
    public class LambdaFilter : Filter
    {
        private Func<URL, bool> filter;

        public LambdaFilter(Func<URL, bool> filter)
        {
            this.filter = filter;
        }

        public override bool Allow(URL url)
        {
            return filter(url);
        }
    }
}
