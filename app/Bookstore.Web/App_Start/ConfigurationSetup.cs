using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using BobsBookstoreClassic.Data;
using Bookstore.Common;

namespace Bookstore.Web
{
    public static class ConfigurationSetup
    {
        public static async Task ConfigureConfigurationAsync(IConfiguration configuration)
        {
            if (BookstoreConfiguration.GetSetting("Services/Database") == "aws")
            {
                using var client = new AmazonSimpleSystemsManagementClient();
                var request = new GetParameterRequest
                {
                    Name = $"/{Constants.AppName}/Database/ConnectionStrings/BookstoreDatabaseConnection"
                };
                var response = await client.GetParameterAsync(request);
                BookstoreConfiguration.AddSetting(response.Parameter.Name.Replace($"/{Constants.AppName}/Database/", string.Empty), response.Parameter.Value);
            }

            if (BookstoreConfiguration.GetSetting("Services/Authentication") == "aws")
            {
                using var client = new AmazonSimpleSystemsManagementClient();
                var request = new GetParametersByPathRequest
                {
                    Path = $"/{Constants.AppName}/Authentication/",
                    Recursive = true
                };
                var response = await client.GetParametersByPathAsync(request);
                foreach (var parameter in response.Parameters)
                {
                    BookstoreConfiguration.AddSetting(parameter.Name.Replace($"/{Constants.AppName}/", string.Empty), parameter.Value);
                }
            }

            if (BookstoreConfiguration.GetSetting("Services/FileService") == "aws")
            {
                using var client = new AmazonSimpleSystemsManagementClient();
                var request = new GetParametersByPathRequest
                {
                    Path = $"/{Constants.AppName}/Files/",
                    Recursive = true
                };
                var response = await client.GetParametersByPathAsync(request);
                foreach (var parameter in response.Parameters)
                {
                    BookstoreConfiguration.AddSetting(parameter.Name.Replace($"/{Constants.AppName}/", string.Empty), parameter.Value);
                }
            }
        }
    }
}
