using System.Text.Json;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Azure.SecretsExtractor
{
    class Program
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        [STAThread]
        private static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Azure Key Vault secrets extractor. Please input key vault url down below:");
            var keyVaultUrl = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(keyVaultUrl))
            {
                ExitWithMessage("Url cannot be empty.");
                return;
            }

            Console.WriteLine($"Stealing secrets from '{keyVaultUrl}'...");

            var validConfigurations = GetAzureKeyVaultConfigs(keyVaultUrl);

            var secretsDictionary = validConfigurations.ToDictionary(x => x.Key, x => x.Value);
            var json = JsonSerializer.Serialize(secretsDictionary, _jsonOptions);

            Clipboard.SetText(json);

            ExitWithMessage("All secrets copied into your clipboard.");
        }

        private static void ExitWithMessage(string message)
        {
            Console.WriteLine($"{message} Exiting in 2 seconds...");
            Thread.Sleep(2000);
        }

        private static IEnumerable<KeyValuePair<string, string>> GetAzureKeyVaultConfigs(string keyVaultUrl)
        {
            var builder = new ConfigurationBuilder();

            var azureCredentialOptions = new DefaultAzureCredentialOptions();
            var credentials = new DefaultAzureCredential(azureCredentialOptions);

            builder.AddAzureKeyVault(new Uri(keyVaultUrl), credentials);

            var configuration = builder.Build();
            var validConfigurations = configuration.AsEnumerable().Where(x => x.Value != null);

            Console.WriteLine($"Found '{validConfigurations.Count()}' valid config entries.");

            return validConfigurations;
        }
    }
}
