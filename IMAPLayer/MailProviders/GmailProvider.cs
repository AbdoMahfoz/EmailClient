using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IMAPLayer.MailProviders
{
    internal class GmailProvider : GenericProvider
    {
        public GmailProvider() : base("imap.gmail.com", 993, true) { }
    }
}
