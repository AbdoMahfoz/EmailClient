using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ViewModels;

namespace IMAPLayer.MailProviders
{
    internal class GenericProvider : BaseProvider, IMailServer
    {
        public int MailCount { get; private set; } = -1;
        public GenericProvider(string address, int port, bool ssl) : base(address, port, ssl) { }

        public async Task<Dictionary<string, string>> GetMail(int Id)
        {
            Dictionary<string, string> outDic = new Dictionary<string, string>();
            string[] res = await Command($"FETCH {Id} (BODY[])");
            await Task.Run(() =>
            {
                string boundary = "";
                bool boundaryHit = false;
                bool InContentArea = false;
                bool InBoundaryArea = false;
                bool isQuotedPrintable = false;
                bool multivalue = false;
                string curKey = "";
                List<byte> over = new List<byte>();
                void ExtractArgs(string s)
                {
                    string[] attributes = s.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    foreach (string attr in attributes)
                    {
                        string[] args = attr.Split(':');
                        if (args.Length == 1)
                        {
                            args = new string[] { attr.Split('=')[0].Trim(), attr.Substring(attr.IndexOf('=') + 1).Trim() };
                        }
                        else if (args[0].Trim() == "Content-Type")
                        {
                            if (args[1].Trim() == "multipart/alternative")
                            {
                                multivalue = true;
                            }
                            else
                            {
                                curKey = args[1].Trim();
                            }
                        }
                        else if (args[0].Trim() == "Content-Transfer-Encoding")
                        {
                            if (args[1].Trim().ToLower() == "quoted-printable")
                            {
                                isQuotedPrintable = true;
                            }
                        }
                        if (args[0].Trim() == "boundary")
                        {
                            boundary = args[1].Replace("\"", "").Trim();
                        }
                    }
                }
                StringBuilder Content = new StringBuilder();
                foreach (string s in res)
                {
                    if (s == res[^1]) break;
                    if (!InBoundaryArea)
                    {
                        if(string.IsNullOrWhiteSpace(s))
                        {
                            InBoundaryArea = true;
                            if(!multivalue)
                            {
                                InContentArea = true;
                            }
                            continue;
                        }
                        ExtractArgs(s);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(s))
                        {
                            if(boundaryHit)
                            {
                                boundaryHit = false;
                                InContentArea = true;
                            }
                        }
                        if (boundaryHit)
                        {
                            ExtractArgs(s);
                        }
                        if (multivalue && s.Trim().Contains($"--{boundary}"))
                        {
                            boundaryHit = true;
                            if (InContentArea)
                            {
                                outDic[curKey] = Content.ToString();
                                Content.Clear();
                            }
                        }
                    }
                    if (InContentArea && !boundaryHit)
                    {
                        if (string.IsNullOrWhiteSpace(s))
                        {
                            Content.AppendLine();
                        }
                        else
                        {
                            if (isQuotedPrintable)
                            {
                                Content.Append(UTFQHelper(s, true, over, "\r\n"));
                            }
                            else
                            {
                                Content.Append(s);
                            }
                        }
                    }
                }
                if(!multivalue)
                {
                    if(Content[Content.Length - 1] == ')') Content.Remove(Content.Length - 1, 1);
                    outDic[curKey] = Content.ToString();
                }
            });
            return outDic;
        }
        protected string UTFQHelper(string line, bool Formatted, List<byte> over, string termination = " ")
        {
            if (!Formatted)
            {
                line = line.TrimStart();
            }
            string res = "";
            while (!string.IsNullOrWhiteSpace(line))
            {
                int i;
                if (Formatted)
                {
                    i = 0;
                }
                else
                {
                    i = line.ToUpper().IndexOf("=?UTF-8?Q?");
                    if (i == -1)
                    {
                        res += line;
                        break;
                    }
                }
                string x;
                if (Formatted)
                {
                    x = line;
                }
                else
                {
                    res += line.Substring(0, i);
                    x = line.Substring(i + 10, line.Substring(i + 10).IndexOf("?="));
                    x = x.Replace("_", " ");
                }
                bool appendNewLine = false;
                if(x[^1] != '=')
                {
                    appendNewLine = true;
                }
                List<byte> buff = new List<byte>(over);
                over.Clear();
                int threshhold = 0;
                bool overCheck = true;
                while (threshhold < x.Length)
                {
                    int j = x.IndexOf('=', threshhold);
                    if(overCheck)
                    {
                        if(j != 0 && buff.Count > 0)
                        {
                            string t = Encoding.UTF8.GetString(buff.ToArray());
                            x = t + x;
                            threshhold += t.Length;
                            buff.Clear();
                            overCheck = false;
                            continue;
                        }
                        overCheck = false;
                    }
                    if (j == -1 || j == x.Length - 1) break;
                    int k = j;
                    for (; k < x.Length; k += 3)
                    {
                        if (x[k] != '=' || k == x.Length - 1)
                        {
                            break;
                        }
                        buff.Add((byte)int.Parse($"{x[k + 1]}{x[k + 2]}", System.Globalization.NumberStyles.HexNumber));
                    }
                    if (buff.Count > 0)
                    {
                        if (k >= x.Length - 1 && !appendNewLine)
                        {
                            over.AddRange(buff);
                            x = x[0..j];
                            buff.Clear();
                            break;
                        }
                        else
                        {
                            string t = Encoding.UTF8.GetString(buff.ToArray());
                            threshhold = j + t.Length;
                            x = x[0..j] + t + x[k..^0];
                        }
                    }
                    buff.Clear();
                }
                if(buff.Count > 0)
                {
                    x = Encoding.UTF8.GetString(buff.ToArray()) + x;
                }
                res += x;
                if (appendNewLine) res += termination;
                else if (over.Count == 0 && !string.IsNullOrWhiteSpace(res)) res = res.Remove(res.Length - 1, 1);
                if (Formatted) line = "";
                else line = line.Substring(line.Substring(i + 10).IndexOf("?=") + 12);
            }
            return res;
        }
        protected List<byte> UTFBHelper(string line)
        {
            List<byte> res = new List<byte>();
            foreach (string segment in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                res.AddRange(Convert.FromBase64String(segment.Replace("=?utf-8?b?", "").Replace("=?UTF-8?B?", "").Replace("?=", "").Trim()));
            }
            return res;
        }
        public async Task<IEnumerable<MailHeader>> GetMails(int Skip, int Take)
        {
            string[] output;
            if (MailCount == -1)
            {
                output = await Command($"FETCH {Skip + 1}:{Skip + Take} (FLAGS BODY[HEADER.FIELDS (Subject From)])");
            }
            else
            {
                output = await Command($"FETCH {MailCount - Skip - Take + 1}:{MailCount - Skip} (FLAGS BODY[HEADER.FIELDS (Subject From)])");
            }
            List<MailHeader> res = new List<MailHeader>();
            MailHeader o = new MailHeader();
            bool idSet = false;
            bool readingSubject = false;
            bool readingFrom = false;
            List<byte> utf8Subject = new List<byte>();
            List<byte> utf8From = new List<byte>();
            List<byte> utf8SubjectOver = new List<byte>();
            List<byte> utf8FromOver = new List<byte>();
            string utf8Helper(string line, string target, List<byte> l, List<byte> o)
            {
                if (line.ToUpper().Contains("=?UTF-8?B?"))
                {
                    l.AddRange(UTFBHelper(line));
                }
                else if (line.ToUpper().Contains("=?UTF-8?Q?"))
                {
                    target += UTFQHelper(line, false, o);
                }
                else
                {
                    target += line;
                }
                return target;
            }
            foreach (string s in output)
            {
                if (string.IsNullOrEmpty(s) || s.Trim() == ")") continue;
                if (s.Contains("Subject:"))
                {
                    readingFrom = false;
                    readingSubject = true;
                    o.Subject = utf8Helper(s.Substring(s.IndexOf(':') + 1).Trim(), o.Subject, utf8Subject, utf8SubjectOver);
                }
                else if (s.Contains("From:"))
                {
                    readingSubject = false;
                    readingFrom = true;
                    o.From = utf8Helper(s.Substring(s.IndexOf(':') + 1).Trim(), o.From, utf8From, utf8FromOver);
                }
                else if (s[0] == '*' && s.Contains("FETCH"))
                {
                    readingFrom = false;
                    readingSubject = false;
                    if (!idSet)
                    {
                        idSet = true;
                    }
                    else
                    {
                        if (utf8Subject.Count > 0)
                        {
                            o.Subject = Encoding.UTF8.GetString(utf8Subject.ToArray());
                            utf8Subject.Clear();
                        }
                        if(utf8From.Count > 0)
                        {
                            o.From = Encoding.UTF8.GetString(utf8From.ToArray());
                            utf8From.Clear();
                        }
                        o.From = o.From.Replace("\"", "");
                        res.Add(o);
                        o = new MailHeader();
                    }
                    o.Id = int.Parse(s.Split(' ')[1]);
                    o.Seen = s.Contains("\\Seen");
                }
                else if (readingSubject)
                {
                    o.Subject = utf8Helper(s, o.Subject, utf8Subject, utf8SubjectOver);
                }
                else if (readingFrom)
                {
                    o.From = utf8Helper(s, o.From, utf8Subject, utf8FromOver);
                }
            }
            return res;
        }
        public async Task<IEnumerable<MailBox>> GetMailTree()
        {
            string[] res = await Command($"LIST \"\" \"*\"");
            Dictionary<string, MailBox> nodes = new Dictionary<string, MailBox>();
            foreach(string s in res)
            {
                if (s.StartsWith("* OK")) break;
                string Element = s.Split('\"', StringSplitOptions.RemoveEmptyEntries)[^1];
                nodes[Element] = new MailBox(Element.Substring(Element.LastIndexOf('/') + 1), Element, !s.Contains("\\Noselect"));
            }
            foreach(var p in nodes)
            {
                if(p.Key.Contains('/'))
                {
                    nodes[p.Key.Substring(0, p.Key.LastIndexOf('/'))].Next.Add(p.Value);
                }
            }
            List<MailBox> resList = new List<MailBox>();
            foreach(var p in nodes)
            {
                if (!p.Key.Contains('/')) resList.Add(p.Value);
            }
            return resList;
        }
        public async Task<bool> Login(string email, string password)
        {
            string[] res = await Command($"LOGIN {email} {password}");
            if (!res[^1].Contains("* OK")) return false;
            return true;
        }
        public async Task<bool> SelectMailBox(string MailBox)
        {
            string[] res = await Command($"SELECT \"{MailBox}\"");
            if (res[^1].Contains("* OK"))
            {
                foreach (string s in res)
                {
                    if (s.Contains("EXISTS"))
                    {
                        MailCount = int.Parse(s.Split(' ')[1]);
                    }
                }
                return true;
            }
            return false;
        }
    }
}
