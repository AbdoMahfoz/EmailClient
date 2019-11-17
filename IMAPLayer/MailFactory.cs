using IMAPLayer.MailProviders;
using System;
using System.Collections.Generic;

namespace IMAPLayer
{
    public static class MailFactory
    {
        private readonly static Dictionary<string, Type> Lookup = new Dictionary<string, Type>
        {
            { "gmail.com", typeof(GmailProvider) }
        };
        public static IMailServer CreateFor(string email)
        {
            if(Lookup.TryGetValue(email.Substring(email.IndexOf('@') + 1), out Type t))
            {
                return (IMailServer)Activator.CreateInstance(t);
            }
            return null;
        }
        public static IMailServer CreateCustom(string IMAPaddress, int port, bool UseSSL)
        {
            return new GenericProvider(IMAPaddress, port, UseSSL);
        }
    }
}
