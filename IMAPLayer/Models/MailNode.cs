
using System.Collections.Generic;

namespace IMAPLayer.Models
{
    public class MailNode
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public bool IsSelectable { get; set; }
        public List<MailNode> Next { get; set; } = new List<MailNode>();
        public MailNode(string Name, string FullName, bool IsSelectable) 
        {
            this.Name = Name;
            this.FullName = FullName;
            this.IsSelectable = IsSelectable;
        }
    }
}
