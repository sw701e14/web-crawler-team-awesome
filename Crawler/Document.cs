using DeadDog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class Document : IEquatable<Document>
    {
        private readonly int id;
        private readonly URL url;
        private string html;

        private static System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        private static string getHashString(string input)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(input);
            buffer = md5.ComputeHash(buffer);

            StringBuilder sb = new StringBuilder(buffer.Length * 2);
            foreach (byte b in buffer)
                sb.AppendFormat("{0:x2}", b);

            return sb.ToString();
        }

        private string filePath
        {
            get
            {
                Directory.CreateDirectory("url_cache");
                return Path.Combine("url_cache", "url_" + getHashString(url.Address) + ".txt");
            }
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

        public bool Equals(Document other)
        {
            return id == other.id;
        }
    }
}
