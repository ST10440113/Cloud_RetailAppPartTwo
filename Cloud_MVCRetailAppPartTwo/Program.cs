using Cloud_MVCRetailAppPartTwo.Services;
using Microsoft.Extensions.Configuration;

namespace Cloud_MVCRetailAppPartTwo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient();

            //register tableStorage with configuration
            builder.Services.AddSingleton(new TableService(configuration.GetConnectionString("connection")));

            //register blobStorage with configuration
            builder.Services.AddSingleton(new BlobService(configuration.GetConnectionString("connection")));

            //register queueStorage with configuration
            builder.Services.AddSingleton<QueueService>(sp =>
            {
                var connectionString = configuration.GetConnectionString("connection");
                return new QueueService(connectionString, "orders");
            }

            );

            //register fileStorage with configuration
            builder.Services.AddHttpClient();

            builder.Services.AddSingleton<FileService>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var connectionString = configuration.GetConnectionString("connection");
                var fileShareName = "files";

                return new FileService(connectionString, fileShareName, httpClientFactory, configuration);
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
