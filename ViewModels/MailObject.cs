
namespace ViewModels
{
    public class MailHeader
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public bool Seen { get; set; }
    }
    public class Mail : MailHeader
    {
        public string Body { get; set; }
    }
}
