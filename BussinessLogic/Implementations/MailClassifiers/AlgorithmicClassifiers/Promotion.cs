using System.Threading.Tasks;
using BussinessLogic.Interfaces;
using ViewModels;

namespace BussinessLogic.Implementations.MailClassifiers.AlgorithmicClassifiers
{
    public class AlgorithmicPromotionClassifier : PromotionsClassifier
    {
        public AlgorithmicPromotionClassifier() : base(null) { }
        public override Task Classify(MailHeader Mail)
        {
            Mail.Category = MailCategory.Promotion;
            return Task.CompletedTask;
        }
    }
}
