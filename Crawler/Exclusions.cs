using DeadDog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawler
{
    public class Exclusions
    {
        private Dictionary<string, record> records;
        private string agent;

        public Exclusions()
            : this(string.Empty)
        {
        }
        public Exclusions(string agent)
        {
            this.records = new Dictionary<string, record>();
            this.agent = agent;

            if (this.agent == null)
                this.agent = string.Empty;
            else
                this.agent = this.agent.ToLower();
        }

        public bool CanAccess(URL url)
        {
            string domain = Regex.Match(url.Address, "https?://[^/]*").Value;
            string path = url.Address.Substring(domain.Length);

            if (!records.ContainsKey(domain))
                records.Add(domain, LoadDomain(url, this.agent));

            var agent = records[domain];

            foreach (var allow in agent.Allowed)
                if (matchesDisallow(path, allow))
                    return true;

            foreach (var disallow in agent.Disallowed)
                if (matchesDisallow(path, disallow))
                    return false;

            return true;
        }

        private static bool matchesDisallow(string path, string disallow)
        {
            if (disallow.Length > path.Length)
                return false;

            return path.StartsWith(disallow);
        }

        private static bool matchesAgent(string agent, string myagent)
        {
            Regex r = new Regex(agent.Replace("*", ".*"));
            return r.IsMatch(myagent);
        }

        #region Caching

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

        private static string getFilePath(URL url)
        {
            Directory.CreateDirectory("robots_cache");
            return Path.Combine("robots_cache", getHashString(url.Domain) + ".txt");
        }

        private static string getRobotsText(URL url)
        {
            string file = getFilePath(url);
            if (File.Exists(file))
                return File.ReadAllText(file);

            URL robotURL = url.GetURLFromLink("/robots.txt");
            string robotTxt = null;

            try { robotTxt = robotURL.GetHTML(); }
            catch { robotTxt = ""; }

            robotTxt = robotTxt.Trim();

            File.WriteAllText(file, robotTxt);

            return robotTxt.Length == 0 ? null : robotTxt;
        }

        #endregion

        private static record LoadDomain(URL url, string userAgent)
        {
            record rec = new record();

            string robotTxt = getRobotsText(url);
            if (robotTxt == "" || robotTxt == null)
                return rec;

            bool lastisagent = false;
            bool use = false;

            foreach (var l in robotTxt.Split('\r', '\n'))
            {
                string line = l;

                if (line.Contains('#'))
                    line = line.Substring(0, line.IndexOf('#'));

                if (line.Length == 0)
                    continue;

                Match field = Regex.Match(line, "^(?<field>[^:]+): *(?<value>[^ ]+) *$");
                if (!field.Success)
                    continue;

                string name = field.Groups["field"].Value.ToLower();
                string value = field.Groups["value"].Value;

                if (name == "user-agent" || name == "useragent")
                {
                    bool useNew = matchesAgent(value, userAgent);
                    use = lastisagent ? use || useNew : useNew;
                    lastisagent = true;
                }
                else
                {
                    lastisagent = false;
                    if (use)
                    {
                        if (name == "disallow")
                            rec.Disallowed.Add(value);
                        else if (name == "allowed")
                            rec.Allowed.Add(value);
                    }
                }
            }

            return rec;
        }

        private class record
        {
            public List<string> Disallowed;
            public List<string> Allowed;

            public record()
            {
                this.Disallowed = new List<string>();
                this.Allowed = new List<string>();
            }
        }
    }
}
