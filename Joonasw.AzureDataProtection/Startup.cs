using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Joonasw.AzureDataProtection
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            var options = _configuration.GetSection("App").Get<AppOptions>();

            var keyIdentifier = options.KeyVaultKeyId;
            services.AddDataProtection()
                .ProtectKeysWithAzureKeyVault(
                    keyIdentifier,
                    GetTokenCredential(options))
                .PersistKeysToAzureBlobStorage(GetBlobClient(options));
        }

        private BlobClient GetBlobClient(AppOptions options)
        {
            var client = new BlobServiceClient(
                new Uri(options.StorageAccountBlobBaseUrl),
                GetTokenCredential(options));

            var containerName = options.StorageContainerName;
            BlobContainerClient containerClient = client.GetBlobContainerClient(containerName);
            var blobName = options.StorageBlobName;
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            return blobClient;
        }

        private TokenCredential GetTokenCredential(AppOptions options)
        {
            return new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                SharedTokenCacheTenantId = options.SharedTokenCacheTenantId
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
