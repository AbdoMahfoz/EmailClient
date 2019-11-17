using BussinessLogic.Implementations;
using BussinessLogic.Interfaces;

namespace DependencyInjection
{
    public static partial class DI
    {
        public static void Initialize()
        {
            /*
            using(var db = new ApplicationDbContext())
            {
                db.Database.Migrate();
            }
            */
            AddSingleton<IMailLogic, IMAPLayerMailLogic>();
        }
    }
}
