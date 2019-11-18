using System.Threading.Tasks;
using ViewModels;

namespace BussinessLogic.Interfaces
{
    public interface IMailClassifier
    {
        Task Classify(MailHeader Mail);
    }
}