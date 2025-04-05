using MatchMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MatchMate
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            try
            {
                Task.Run(() => RoleInitializer.InitializeAsync()).Wait();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during role/user initialization: {ex.Message}");
                throw;
            }
            InitializeGames();
        }

        private void InitializeGames()
        {
            using (var context = new ApplicationDbContext())
            {
                if (!context.Games.Any())
                {
                    context.Games.Add(new Game
                    {
                        GameName = "Tic-Tac-Toe",
                        Description = "Classic game of X's and O's on a 3x3 grid.",
                        CreatedDate = DateTime.Now
                    });

                    context.Games.Add(new Game
                    {
                        GameName = "Rock-Paper-Scissors",
                        Description = "Classic hand game where rock beats scissors, scissors beats paper, and paper beats rock.",
                        CreatedDate = DateTime.Now
                    });

                    context.SaveChanges();
                }
            }
        }
    }
}