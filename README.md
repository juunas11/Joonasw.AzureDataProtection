# Sample app for ASP.NET Core Data Protection integration with Azure services

This app uses the new Azure integration packages for data protection with Azure Key Vault and Azure Storage:

- [Azure.AspNetCore.DataProtection.Keys](https://www.nuget.org/packages/Azure.AspNetCore.DataProtection.Keys)
- [Azure.AspNetCore.DataProtection.Blobs](https://www.nuget.org/packages/Azure.AspNetCore.DataProtection.Blobs)

It also uses the [Azure.Identity](https://www.nuget.org/packages/Azure.Identity) package to connect to both Azure Key Vault and Blob Storage with the developer user account / Managed Identity in Azure.

The purpose is to configure the data protection system in such a way
that its keys are stored outside the app process,
but also to do so in a secure manner.
By default data protection keys may be stored in-memory,
or in a local folder.
This can cause issues in certain situations like when running in Azure App Service
and using its deployment slots feature.
When swapping the slots for deploying a new version,
the data protection keys get swapped with the old version.
This then causes things which are dependent on those keys to no longer be valid,
like authentication cookies.

It's quite an unwanted situation to log out all users when deploying a new version.
But we also don't want to store the keys in plaintext in some file.
So this is where the combo of Key Vault and Blob Storage comes in.
The app generates a data protection key when it is needed.
This key is then encrypted with another key in Key Vault.
The result is then stored in Blob Storage.

So a user would need access to the Unwrap Key operation + read access to the blob container
in order to decrypt the keys.
It is definitely a good idea to enable audit logs on a production Key Vault,
and limit the number of users with access to cryptographic operations/access management on it.

## Local setup

I used Visual Studio 2019 for development,
but you can use basically any IDE with the dotnet CLI to build and run the app.
You will need the .NET Core 3.1 SDK though: [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).

The app requires an Azure Storage account and an Azure Key Vault to be created.

- Create an Azure Storage account and create a blob container there.
- Assign your user account as a Storage Blob Data Contributor on the account or the container.
- Create an Azure Key Vault
- Create a _key_ in the Key Vault
- Define an _access policy_ in the Key Vault that gives your user at least _Get, Wrap Key, and Unwrap Key_ permissions on Keys
  - If you created the Key Vault, you should have access to everything by default

Next you need add configuration to _appsettings.json:

- **KeyVaultKeyId**: The id for the _key_ you created in Key Vault (Keys -> your key name -> version id -> Key Identifier)
  - Example: `https://keyvaultname.vault.azure.net/keys/TestKey/237b3f4013e2447794e65a2cfe8c5d49`
- **StorageAccountBlobBaseUrl**: Blob endpoint URL for the Storage account (Properties tab)
  - Example: `https://storageaccountname.blob.core.windows.net`
- **StorageContainerName**: Name of the container you created in the Storage account
- **StorageBlobName**: Name of the file that will store the data protection keys
  - This will be created if it does not exist
- **SharedTokenCacheTenantId**: Id for the Azure AD tenant which is linked to the subscription containing your Key Vault and Storage account (Azure Active Directory -> Properties -> Directory ID)
  - This is used only in local development
  - It helps the local tools figure out what tenant they need tokens for, in case you are in multiple tenants

You can also setup these settings as user secrets,
as I have done for running locally.

If you get errors about no user account being found in shared cache,
you can login to the account in Visual Studio,
or download the [cross-platform AZ CLI](https://docs.microsoft.com/en-us/cli/azure/?view=azure-cli-latest) and login there.

## Azure setup

I have tested the app on Azure App Service for Windows,
though technically you can run it anywhere that supports Managed Identities.
It is also possible to configure the app to use client credentials authentication with AAD,
which will let you run the app _anywhere_.

After creating an App Service,
enable a system-assigned Managed Identity via the _Identity_ tab.
Then give this identity the access you gave your user in local setup.

Then you can add the relevant configuration in the Configuration tab:

- App:KeyVaultKeyId
- App:StorageAccountBlobBaseUrl
- App:StorageContainerName
- App:StorageBlobName

See the local setup section for what these are and where to get them.

You should then be able to publish the app as stand-alone or framework-dependent to there and run it.
