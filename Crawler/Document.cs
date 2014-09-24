using DeadDog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class Document
    {
        private readonly int id;
        private readonly URL url;
        private string html;

        private string filePath
        {
            get { return "url_" + id + ".txt"; }
        }

        public Document(int id, URL url)
        {
            this.id = id;
            this.url = url;
            this.html = null;
        }

        public int Id
        {
            get { return id; }
        }
        
        public URL URL
        {
            get { return url; }
        }

        public string HTML
        {
            get
            {
                if (html == null && !loadHtmlFromDisc())
                {
                    html = url.GetHTML(true);
                    File.WriteAllText(filePath, html, Encoding.Unicode);
                }

                return html;
            }
        }

        private bool loadHtmlFromDisc()
        {
            if (File.Exists(filePath))
            {
                html = File.ReadAllText(filePath, Encoding.Unicode);
                return true;
            }

            return false;
        }
    }
}
