namespace Joonasw.AzureDataProtection
{
    public class AppOptions
    {
        public string KeyVaultKeyId { get; set; }
        public string StorageAccountBlobBaseUrl { get; set; }
        public string StorageContainerName { get; set; }
        public string StorageBlobName { get; set; }
        public string SharedTokenCacheTenantId { get; set; }
    }
}
