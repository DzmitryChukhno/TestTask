using System;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using GQL.GraphQL;
using GQL.Data;
using GQL.Services;

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


            app.MapGraphQL();
            app.Run();
        }
    }
}
