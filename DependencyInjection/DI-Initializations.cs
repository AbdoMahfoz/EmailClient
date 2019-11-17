using Microsoft.EntityFrameworkCore;
using Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyInjection
{
    public static partial class DI
    {
        public static void Initialize()
        {
            using(var db = new ApplicationDbContext())
            {
                db.Database.Migrate();
            }
        }
    }
}
