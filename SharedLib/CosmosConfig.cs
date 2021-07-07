namespace AzCosmosDB.Samples.SharedLib
{
    using System;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;
    using System.Threading.Tasks;
    public static class CosmosConfig
    {
        public static CosmosClient GetConnection(CosmosClientOptions cosmosClientOptions = null)
        {
            try
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                   .AddJsonFile("appSettings.json")
                   .Build();
                var EndPointUrl = configuration["EndPointUrl"];
                var auth = configuration["AuthorizationKey"];

                return new CosmosClient(configuration["EndPointUrl"], configuration["AuthorizationKey"], cosmosClientOptions);
            }
            catch (CosmosException cre)
            {
                Console.WriteLine(cre.ToString());
                return null;
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
                return null;
            }
        }
        public static async Task<Container> CreateContainerAsync(
            CosmosClient client,
            string databaseName,
            string containerName,
            string partitionKey,
            int throughput)
        {
            Database database = await client.CreateDatabaseIfNotExistsAsync(databaseName);


            // Indexing Policy to exclude all attributes to maximize RU/s usage
            Container container = await database.DefineContainer(containerName, "/" + partitionKey)
                    .WithIndexingPolicy()
                        .WithIndexingMode(IndexingMode.Consistent)
                        .WithIncludedPaths()
                            .Attach()
                        .WithExcludedPaths()
                            .Path("/*")
                            .Attach()
                    .Attach()
                .CreateIfNotExistsAsync(throughput);

            return container;
        }
    }
}
