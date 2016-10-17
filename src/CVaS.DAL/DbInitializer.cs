using System.Linq;
using CVaS.DAL.Model;

namespace CVaS.DAL
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.Algorithms.Any())
            {
                var algorithm = new Algorithm()
                {
                    CodeName = "hello-world",
                    FilePath = "Project1",
                    Title = "Mega Giga Super Algo"
                };
                context.Algorithms.Add(algorithm);
            }
            context.SaveChanges();
        }
    }
}
