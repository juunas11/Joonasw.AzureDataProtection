# Sample app for ASP.NET Core Data Protection integration with Azure services

This app uses the new Azure integration packages for data protection with Azure Key Vault and Azure Storage:

- https://www.nuget.org/packages/Azure.AspNetCore.DataProtection.Keys
- https://www.nuget.org/packages/Azure.AspNetCore.DataProtection.Blobs

It also uses the Azure.Identity package to connect to both Azure Key Vault and Blob Storage with the developer user account / Managed Identity in Azure.
