using BussinessLogic.Interfaces;
using ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BussinessLogic.Implementations.MailClassifiers.AlgorithmicClassifiers
{
    public class AlgorithmicSpamClassifier : SpamClassifier
    {
        private readonly IMailLogic MailLogic;
        private static SortedSet<(string, string)> SavedIds = null;
        private static readonly object IdLock = new object();
        public AlgorithmicSpamClassifier(BankingClassifier Next, IMailLogic MailLogic) : base(Next) 
        {
            this.MailLogic = MailLogic;
        }
        public override async Task Classify(MailHeader Mail)
        {
            await Task.Run(() =>
            {
                lock (IdLock)
                {
                    if (SavedIds == null)
                    {
                        SavedIds = new SortedSet<(string, string)>(MailLogic.GetMails(MailLogic.GetSpamMailBox().Result, 0, 200).Result.Select(u => (u.From, u.Subject)));
                    }
                }
            });
            if(SavedIds.Contains((Mail.From, Mail.Subject)))
            {
                Mail.Category = MailCategory.Spam;
                return;
            }
            await Next(Mail);
        }
    }
}
