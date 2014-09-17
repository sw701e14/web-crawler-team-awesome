using DeadDog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawler
{
    public class Exclusions
    {
        private Dictionary<string, record[]> records;
        private string agent;

        public Exclusions()
            : this(string.Empty)
        {
        }
        public Exclusions(string agent)
        {
            this.records = new Dictionary<string, record[]>();
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
                records.Add(domain, LoadDomain(domain).Where(r => matchesAgent(r.UserAgent)).ToArray());

            foreach (var agent in records[domain])
            {
                foreach (var allow in agent.Allowed)
                    if (matchesDisallow(path, allow))
                        return true;

                foreach (var disallow in agent.Disallowed)
                    if (matchesDisallow(path, disallow))
                        return false;
            }

            return true;
        }

        private static bool matchesDisallow(string path, string disallow)
        {
            if (disallow.Length > path.Length)
                return false;

            return path.StartsWith(disallow);
        }

        private bool matchesAgent(string agent)
        {
            Regex r = new Regex(agent.Replace("*", ".*"));
            return r.IsMatch(this.agent);
        }

        private static IEnumerable<record> LoadDomain(string domain)
        {
            URL robotURL = new URL(domain + "/robots.txt");
            string robotTxt;
            try { robotTxt = robotURL.GetHTML(); }
            catch { yield break; }

            record rec = null;
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

                if (name == "user-agent")
                {
                    if (rec != null)
                    {
                        var temp = rec;
                        rec = new record(value.ToLower());

                        if (temp.Disallowed.Count == 0 && temp.Allowed.Count == 0)
                        {
                            temp.Disallowed = rec.Disallowed;
                            temp.Allowed = rec.Allowed;
                        }

                        yield return temp;
                    }
                    else
                        rec = new record(value);
                }
                else if (name == "disallow")
                    rec.Disallowed.Add(value);
                else if (name == "allowed")
                    rec.Allowed.Add(value);
            }

            if (rec != null)
                yield return rec;
        }

        private class record
        {
            public readonly string UserAgent;
            public List<string> Disallowed;
            public List<string> Allowed;

            public record(string userAgent)
            {
                this.UserAgent = userAgent;
                this.Disallowed = new List<string>();
                this.Allowed = new List<string>();
            }
        }
    }
}
