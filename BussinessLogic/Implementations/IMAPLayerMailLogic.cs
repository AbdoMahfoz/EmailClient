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
        private MailBox CurrentMailBox;
        public void Dispose()
        {
            if(MailServer != null) MailServer.Dispose();
        }
        public async Task<Mail> GetMail(MailHeader Mail)
        {
            if(CurrentMailBox.FullName != Mail.MailBox.FullName)
            {
                await SelectMailBox(Mail.MailBox);
            }
            var res = await MailServer.GetMail(Mail.Id);
            return new Mail
            {
                Body = res.Item1,
                Attachments = res.Item2,
                From = Mail.From,
                Seen = Mail.Seen,
                Id = Mail.Id,
                MailBox = CurrentMailBox
            };
        }
        public async Task<IEnumerable<MailBox>> GetMailBoxTree()
        {
            return await MailServer.GetMailTree();
        }
        public async Task<IEnumerable<MailHeader>> GetMails(MailBox MailBox, int Skip, int Take)
        {
            await MailServer.SelectMailBox(MailBox.FullName);
            CurrentMailBox = MailBox;
            List<MailHeader> res = new List<MailHeader>();
            foreach(MailHeader mail in (await MailServer.GetMails(Skip, Take)).Reverse())
            {
                mail.MailBox = MailBox;
                res.Add(mail);
            }
            return res;
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
        public async Task<MailBox> GetAllMailBox()
        {
            static MailBox GetBox(MailBox b)
            {
                if (b.IsAll) return b;
                foreach(var v in b.Next)
                {
                    var vv = GetBox(v);
                    if (vv != null) return vv;
                }
                return null;
            }
            foreach(var b in await GetMailBoxTree())
            {
                var v = GetBox(b);
                if (v != null) return v;
            }
            return null;
        }
        public async Task<MailBox> GetSpamMailBox()
        {
            static MailBox GetBox(MailBox b)
            {
                if (b.IsSpam) return b;
                foreach (var v in b.Next)
                {
                    var vv = GetBox(v);
                    if (vv != null) return vv;
                }
                return null;
            }
            foreach (var b in await GetMailBoxTree())
            {
                var v = GetBox(b);
                if (v != null) return v;
            }
            return null;
        }
        public async Task SelectMailBox(MailBox MailBox)
        {
            await MailServer.SelectMailBox(MailBox.FullName);
            CurrentMailBox = MailBox;
        }
    }
}
