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
            var client = CosmosConfig.GetConnection();
            var container = await CosmosConfig.CreateContainerAsync(client, "aztestdb", "item", "pk", 1000);

            var player = GeneratorModels.GenerateGamePlayer();

            TransactionalBatchResponse response = await container.CreateTransactionalBatch(new PartitionKey(player.Id))
                .CreateItem<GameItem>(new GameItem(player.Id,"test_01",1))
                .CreateItem<GameItem>(new GameItem(player.Id, "test_02", 10))
                .CreateItem<GameItem>(new GameItem(player.Id, "test_03", 10))
                .UpsertItem<GameItem>(new GameItem(player.Id, "test_04", 1))
                .ExecuteAsync();


            Console.ReadLine();
        }
    }
}
