using DeadDog;
using System;

namespace Crawler.Filtering
{
    public abstract class Filter
    {
        public abstract bool Allow(URL url);

        public static Filter operator &(Filter a, Filter b)
        {
            return new AndFilter(a, b);
        }

        public static Filter operator |(Filter a, Filter b)
        {
            return new OrFilter(a, b);
        }
    }
}
