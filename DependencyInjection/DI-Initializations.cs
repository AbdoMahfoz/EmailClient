using BussinessLogic.Implementations;
using BussinessLogic.Implementations.MailClassifiers;
using BussinessLogic.Implementations.MailClassifiers.AlgorithmicClassifiers;
using BussinessLogic.Interfaces;

namespace DependencyInjection
{
    public static partial class DI
    {
        public static void Initialize()
        {
            AddSingleton<IMailLogic, IMAPLayerMailLogic>();
            AddTransient<IMailClassifier, SpamClassifier>();
            AddTransient<BankingClassifier, AlgorithmicBankingClassifier>();
            AddTransient<SpamClassifier, AlgorithmicSpamClassifier>();
            AddTransient<UpdatesClassifier, AlgorithmicUpdatesClassifier>();
            AddTransient<PromotionsClassifier, AlgorithmicPromotionClassifier>();
        }
    }
}
