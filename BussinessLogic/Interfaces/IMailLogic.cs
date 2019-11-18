using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViewModels;

namespace BussinessLogic.Interfaces
{
    public enum LoginResult { Ok, WrongCredintials, UnknownProvider, IncorrectProviderData }
    public interface IMailLogic : IDisposable
    {
        bool IsAuthenticated { get; }
        Task<LoginResult> Login(string Email, string Password, string IMAPServer = null, int? IMAPPort = null, bool? SSL = null);
        Task<IEnumerable<MailBox>> GetMailBoxTree();
        Task SelectMailBox(MailBox MailBox);
        Task<MailBox> GetAllMailBox();
        Task<MailBox> GetSpamMailBox();
        Task<IEnumerable<MailHeader>> GetMails(MailBox MailBox, int Skip = 0, int Take = 50);
        Task<Mail> GetMail(MailHeader Mail);
    }
}
