using DeadDog;

namespace WebCrawler.Filtering
{
    public class OrFilter : Filter
    {
        private Filter[] filters;

        public OrFilter(params Filter[] filters)
        {
            this.filters = filters;
        }

        public override bool Allow(URL url)
        {
            foreach (var f in filters)
                if (!f.Allow(url))
                    return true;

            return false;
        }
    }
}
