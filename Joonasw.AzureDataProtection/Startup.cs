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
                    new Uri(keyIdentifier),
                    GetTokenCredential(options))
                .PersistKeysToAzureBlobStorage(GetBlobClient(options));
            // Key Vault requires the user/app to have following permissions on Keys:
            // -Read
            // -Wrap key
            // -Unwrap key

            // Blob Storage requires the user/app to have
            // Storage Blob Data Contributor role
            // at Storage account or container level
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
            var credentialOptions = new DefaultAzureCredentialOptions();
            if (options.SharedTokenCacheTenantId != null)
            {
                credentialOptions.SharedTokenCacheTenantId = options.SharedTokenCacheTenantId;
            }

            return new DefaultAzureCredential(credentialOptions);
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
