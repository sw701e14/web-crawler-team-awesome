using DeadDog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Filtering
{
    public class DomainFilter : Filter
    {
        private string domain;

        public DomainFilter(string domain)
        {
            this.domain = domain.ToLower();
        }
        public DomainFilter(URL domain_page)
            : this(domain_page.Domain)
        {
        }

        public override bool Allow(URL url)
        {
            return url.Domain.Equals(domain);
        }
    }
}
