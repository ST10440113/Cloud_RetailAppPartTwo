using Cloud_MVCRetailAppPartTwo.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            builder.Services.AddSingleton<TableService>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var connectionString = configuration.GetConnectionString("connection");
                return new TableService(connectionString, httpClientFactory, configuration);
            });

            //register blobStorage with configuration
            builder.Services.AddSingleton<BlobService>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var connectionString = configuration.GetConnectionString("connection");
                var containerName = "product-images";
                return new BlobService(httpClientFactory, configuration, connectionString, containerName);
            });

            //register queueStorage with configuration
            builder.Services.AddSingleton<QueueService>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>(); 
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var connectionString = configuration.GetConnectionString("connection");
                var queueName = "orders";
                return new QueueService(httpClientFactory, configuration , connectionString, queueName);
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
