using BussinessLogic.Interfaces;
using System.Threading.Tasks;
using ViewModels;

namespace BussinessLogic.Implementations.MailClassifiers
{
    public abstract class Classifier : IMailClassifier
    {
        private readonly IMailClassifier NextClassifier;
        protected Classifier(IMailClassifier NextClassifier)
        {
            this.NextClassifier = NextClassifier;
        }
        public abstract Task Classify(MailHeader Mail);
        protected Task Next(MailHeader Mail)
        {
            return NextClassifier.Classify(Mail);
        }
    }
    public abstract class BankingClassifier : Classifier
    {
        protected BankingClassifier(IMailClassifier Next) : base(Next) { }
    }
    public abstract class UpdatesClassifier : Classifier
    {
        protected UpdatesClassifier(IMailClassifier Next) : base(Next) { }
    }
    public abstract class SpamClassifier : Classifier
    {
        protected SpamClassifier(IMailClassifier Next) : base(Next) { }
    }
    public abstract class PromotionsClassifier : Classifier
    {
        protected PromotionsClassifier(IMailClassifier Next) : base(Next) { }
    }
}
