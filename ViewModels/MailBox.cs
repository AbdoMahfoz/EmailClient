
using System.Collections.Generic;

namespace ViewModels
{
    public class MailBox
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public bool IsSelectable { get; set; }
        public bool IsSpam { get; set; }
        public bool IsAll { get; set; }
        public List<MailBox> Next { get; set; } = new List<MailBox>();
        public MailBox(string Name, string FullName, bool IsSelectable, bool IsSpam, bool IsAll) 
        {
            this.Name = Name;
            this.FullName = FullName;
            this.IsSelectable = IsSelectable;
            this.IsSpam = IsSpam;
            this.IsAll = IsAll;
        }
    }
}
