using System.Collections.Generic;

namespace ViewModels
{
    public class MailHeader
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public string StrippedFrom 
        {   
            get
            {
                return From.Split('<')[0].Trim();
            }
        }
        public string StrippedSubject 
        {
            get
            {
                return (Subject.Length > 60) ? Subject[0..(60 - 3)] + "..." : Subject;
            }
        }
        public bool Seen { get; set; }
        public MailBox MailBox { get; set; }
        public MailCategory Category { get; set; }
    }
    public class Mail : MailHeader
    {
        public Dictionary<string, string> Body { get; set; }
        public Dictionary<string, byte[]> Attachments { get; set; }
    }
}
