using System;
using Microsoft.EntityFrameworkCore;
using GQL.GraphQL;
using GQL.Data;
using GQL.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GQL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                    };
                });
            builder.Services.AddAuthorization();

            builder.Services
                .AddDbContext<AppDbContext>()
                .AddGraphQLServer()
                .AddAuthorization()
                .AddQueryType<GQL.GraphQL.Query>()
                .AddMutationType<Mutation>()
                .AddProjections()
                .AddFiltering()
                .AddSorting();

            builder.Services.AddScoped<AuthService>();
            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();
            DbInitializer.Seed(app);
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGraphQL();
            app.Run();
        }
    }
}
