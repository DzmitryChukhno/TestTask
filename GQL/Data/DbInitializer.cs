using GQL.Data;
using GQL.Models;

namespace GQL
{
    public static class DbInitializer
    {
        public static void Seed(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!context.Users.Any())
            {
                context.Users.AddRange(new User
                {
                    Username = "admin",
                    PasswordHash = "admin123",
                    Email = "admin@admin.com",
                    Role = "Admin"
                }, new User
                {
                    Username = "user1",
                    PasswordHash = "user123",
                    Email = "user1@user.com",
                    Role = "User"
                });
                context.SaveChanges();
            }
        }
    }
}