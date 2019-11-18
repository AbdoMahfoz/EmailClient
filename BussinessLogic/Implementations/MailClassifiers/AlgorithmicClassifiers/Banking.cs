using System.Collections.Generic;
using System.Threading.Tasks;
using BussinessLogic.Interfaces;
using ViewModels;

namespace BussinessLogic.Implementations.MailClassifiers.AlgorithmicClassifiers
{
    public class AlgorithmicBankingClassifier : BankingClassifier
    {
        public AlgorithmicBankingClassifier(UpdatesClassifier Next) : base(Next) { }
        static readonly private SortedSet<string> BankingDomains = new SortedSet<string>
        {
            "eg-bank.com",
            "blombankegypt.com",
            "bankaudi.com",
            "theubeg.com",
            "saib.com.eg",
            "scbank.com.eg",
            "unb-egypy.com",
            "aaib.com",
            "adib.eg",
            "faisalbank.com.eg",
            "alexbank.com",
            "arabbank.com.eg",
            "bbvausa.com",
            "centralbankofindia.co.in",
            "db.com",
            "unionbank.com",
            "kotak.com",
            "nbe.com.eg",
            "usbank.com",
            "cibeg.com",
            "axisbank.com",
            "standardbank.co.za",
            "td.com",
            "lloydsbank.com",
            "ebebank.com",
            "huntington.com",
            "banquemisr.com"
        };
        public override Task Classify(MailHeader Mail)
        {
            string From;
            if(Mail.From.Contains('<'))
            {
                From = Mail.From.Split('<')[1].Replace(">", "");
            }
            else
            {
                From = Mail.From;
            }
            if(BankingDomains.Contains(From.Split('@')[1].Trim()))
            {
                Mail.Category = MailCategory.Banking;
                return Task.CompletedTask;
            }
            return Next(Mail);
        }
    }
}
