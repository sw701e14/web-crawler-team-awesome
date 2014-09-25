using DeadDog;

namespace WebCrawler.Filtering
{
    public class AndFilter : Filter
    {
        private Filter[] filters;

        public AndFilter(params Filter[] filters)
        {
            this.filters = filters;
        }

        public override bool Allow(URL url)
        {
            foreach (var f in filters)
                if (!f.Allow(url))
                    return false;

            return true;
        }
    }
}
