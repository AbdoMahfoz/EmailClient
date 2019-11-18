using ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IMAPLayer
{
    public interface IMailServer : IDisposable
    {
        public int MailCount { get; }
        Task<bool> Login(string email, string password);
        Task<IEnumerable<MailBox>> GetMailTree();
        Task<bool> SelectMailBox(string MailBox);
        Task<IEnumerable<MailHeader>> GetMails(int Skip, int Take);
        Task<(Dictionary<string, string>, Dictionary<string, byte[]>)> GetMail(int Id);
    }
}
