using DirectoryExplorer.Domain.Interfaces;
using DirectoryExplorer.Domain.Services;
using DirectoryExplorer.Models;

namespace TestProject {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddScoped<IDirectoryExplorerService, DirectoryExplorerService>();

            builder.Services.Configure<DirectoryExplorerOptions>(
                builder.Configuration.GetSection("DirectoryExplorer"));


            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }
    }
}