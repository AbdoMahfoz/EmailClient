using System.Collections.Generic;
using System.Threading.Tasks;
using ViewModels;
using BussinessLogic.Interfaces;
using IMAPLayer;
using System.Linq;

namespace BussinessLogic.Implementations
{
    public class IMAPLayerMailLogic : IMailLogic
    {
        public bool IsAuthenticated { get; private set; } = false;
        private IMailServer MailServer;
        public void Dispose()
        {
            if(MailServer != null) MailServer.Dispose();
        }
        public async Task<Dictionary<string, string>> GetMail(MailHeader Mail)
        {
            return await MailServer.GetMail(Mail.Id);
        }
        public async Task<IEnumerable<MailBox>> GetMailBoxTree()
        {
            return await MailServer.GetMailTree();
        }
        public async Task<IEnumerable<MailHeader>> GetMails(MailBox MailBox, int Skip, int Take)
        {
            await MailServer.SelectMailBox(MailBox.FullName);
            return (await MailServer.GetMails(Skip, Take)).Reverse();
        }
        public async Task<LoginResult> Login(string Email, string Password, string IMAPServer = null, int? IMAPPort = null, bool? SSL = null)
        {
            if(IMAPServer == null || IMAPPort == null || SSL == null)
            {
                await Task.Run(() =>
                {
                    MailServer = MailServerFactory.CreateFor(Email);
                });
                if(MailServer == null)
                {
                    return LoginResult.UnknownProvider;
                }
            }
            else
            {
                await Task.Run(() =>
                {
                    try
                    {
                        MailServer = MailServerFactory.CreateCustom(IMAPServer, (int)IMAPPort, (bool)SSL);
                    }
                    catch 
                    {
                        if (MailServer != null) MailServer.Dispose();
                        MailServer = null;
                    }
                });
                if(MailServer == null)
                {
                    return LoginResult.IncorrectProviderData;
                }
            }
            if(await MailServer.Login(Email, Password))
            {
                IsAuthenticated = true;
                return LoginResult.Ok;
            }
            return LoginResult.WrongCredintials;
        }
    }
}
