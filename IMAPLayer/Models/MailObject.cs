
namespace IMAPLayer.Models
{
    public class MailHeaderObject
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public bool Seen { get; set; }
    }
    public class MailObject : MailHeaderObject
    {
        public string Body { get; set; }
    }
}
