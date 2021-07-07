namespace AzCosmosDB.Samples.SharedLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    class Program
    {
        static async Task Main(string[] args)
        {
            var client = CosmosConfig.GetConnection(new CosmosClientOptions()
            {
                Serializer = new CosmosCustomSerializer(new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    ReferenceLoopHandling = ReferenceLoopHandling.Error,
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    Converters = new JsonConverter[]
                        {
                            new StringEnumConverter()
                        }
                })
            });
            var container = await CosmosConfig.CreateContainerAsync(client, "aztestdb", "userorder", "id", 10000);

            var document = GeneratorModels.GenerateUsers(1).FirstOrDefault();
            await container.CreateItemAsync(document);

            Console.ReadLine();
        }
    }
}
