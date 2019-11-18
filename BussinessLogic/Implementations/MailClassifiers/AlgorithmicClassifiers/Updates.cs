using System.Threading.Tasks;
using BussinessLogic.Interfaces;
using ViewModels;

namespace BussinessLogic.Implementations.MailClassifiers.AlgorithmicClassifiers
{
    public class AlgorithmicUpdatesClassifier : UpdatesClassifier
    {
        public AlgorithmicUpdatesClassifier(PromotionsClassifier Next) : base(Next) { }
        public override async Task Classify(MailHeader Mail)
        {
            if(Mail.Subject.ToLower().Contains("update"))
            {
                Mail.Category = MailCategory.Updates;
                return;
            }
            await Next(Mail);
        }
    }
}
