namespace AzCosmosDB.Samples.SharedLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;
    class Program
    {
        static async Task Main(string[] args)
        {
            var userorders = GeneratorModels.GenerateUsers(1000);

            Console.WriteLine($"Generate random data complete");

            var client = CosmosConfig.GetConnection(new CosmosClientOptions() { AllowBulkExecution = true });
            var container = await CosmosConfig.CreateContainerAsync(client, "aztestdb", "userorder", "id", 1000);

            await CreateItemsConcurrentlyAsync<User>(container, userorders);
            await UpdateItemsConcurrentlyAsync<User>(container, userorders);

            Console.ReadLine();
        }

        public static async Task CreateItemsConcurrentlyAsync<T>(
                Container container,
                IReadOnlyList<T> documentsToWorkWith) where T : ICosmosDocument
        {
            // <BulkImport>
            BulkOperations<T> bulkOperations = new BulkOperations<T>(documentsToWorkWith.Count);
            foreach (T document in documentsToWorkWith)
            {
                bulkOperations.Tasks.Add(CaptureOperationResponse(container.CreateItemAsync(document), document));
            }
            // </BulkImport>

            // <WhenAll>
            BulkOperationResponse<T> bulkOperationResponse = await bulkOperations.ExecuteAsync();
            // </WhenAll>
            Console.WriteLine($"Bulk create operation finished in {bulkOperationResponse.TotalTimeTaken}");
            Console.WriteLine($"Consumed {bulkOperationResponse.TotalRequestUnitsConsumed} RUs in total");
            Console.WriteLine($"Consumed {bulkOperationResponse.TotalRequestUnitsConsumed / bulkOperationResponse.TotalTimeTaken.TotalSeconds } RUs per Second");
            Console.WriteLine($"Created {bulkOperationResponse.SuccessfulDocuments} documents");
            Console.WriteLine($"Failed {bulkOperationResponse.Failures.Count} documents");
            if (bulkOperationResponse.Failures.Count > 0)
            {
                Console.WriteLine($"First failed sample document {bulkOperationResponse.Failures[0].Item1.Id} - {bulkOperationResponse.Failures[0].Item2}");
            }
        }
        public static async Task UpdateItemsConcurrentlyAsync<T>(
            Container container,
            IReadOnlyList<T> documentsToWorkWith) where T : ICosmosDocument
        {
            // <BulkUpdate>
            BulkOperations<T> bulkOperations = new BulkOperations<T>(documentsToWorkWith.Count);
            foreach (T document in documentsToWorkWith)
            {
                bulkOperations.Tasks.Add(CaptureOperationResponse(container.ReplaceItemAsync(document, document.Id), document));
            }
            // </BulkUpdate>

            BulkOperationResponse<T> bulkOperationResponse = await bulkOperations.ExecuteAsync();
            Console.WriteLine($"Bulk update operation finished in {bulkOperationResponse.TotalTimeTaken}");
            Console.WriteLine($"Consumed {bulkOperationResponse.TotalRequestUnitsConsumed} RUs in total");
            Console.WriteLine($"Consumed {bulkOperationResponse.TotalRequestUnitsConsumed / bulkOperationResponse.TotalTimeTaken.TotalSeconds } RUs per Second");
            Console.WriteLine($"Updated {bulkOperationResponse.SuccessfulDocuments} documents");
            Console.WriteLine($"Failed {bulkOperationResponse.Failures.Count} documents");
            if (bulkOperationResponse.Failures.Count > 0)
            {
                Console.WriteLine($"First failed sample document {bulkOperationResponse.Failures[0].Item1.Id} - {bulkOperationResponse.Failures[0].Item2}");
            }
        }

        private static async Task<OperationResponse<T>> CaptureOperationResponse<T>(Task<ItemResponse<T>> task, T item)
        {
            try
            {
                ItemResponse<T> response = await task;
                return new OperationResponse<T>()
                {
                    Item = item,
                    IsSuccessful = true,
                    RequestUnitsConsumed = task.Result.RequestCharge
                };
            }
            catch (Exception ex)
            {
                if (ex is CosmosException cosmosException)
                {
                    return new OperationResponse<T>()
                    {
                        Item = item,
                        RequestUnitsConsumed = cosmosException.RequestCharge,
                        IsSuccessful = false,
                        CosmosException = cosmosException
                    };
                }

                return new OperationResponse<T>()
                {
                    Item = item,
                    IsSuccessful = false,
                    CosmosException = ex
                };
            }
        }
    }
    public class BulkOperations<T>
    {
        public readonly List<Task<OperationResponse<T>>> Tasks;

        private readonly Stopwatch stopwatch = Stopwatch.StartNew();

        public BulkOperations(int operationCount)
        {
            this.Tasks = new List<Task<OperationResponse<T>>>(operationCount);
        }

        public async Task<BulkOperationResponse<T>> ExecuteAsync()
        {
            await Task.WhenAll(this.Tasks);
            this.stopwatch.Stop();
            return new BulkOperationResponse<T>()
            {
                TotalTimeTaken = this.stopwatch.Elapsed,
                TotalRequestUnitsConsumed = this.Tasks.Sum(task => task.Result.RequestUnitsConsumed),
                SuccessfulDocuments = this.Tasks.Count(task => task.Result.IsSuccessful),
                Failures = this.Tasks.Where(task => !task.Result.IsSuccessful).Select(task => (task.Result.Item, task.Result.CosmosException)).ToList()
            };
        }
    }

    public class BulkOperationResponse<T>
    {
        public TimeSpan TotalTimeTaken { get; set; }
        public int SuccessfulDocuments { get; set; } = 0;
        public double TotalRequestUnitsConsumed { get; set; } = 0;

        public IReadOnlyList<(T, Exception)> Failures { get; set; }
    }
    // </ResponseType>

    // <OperationResult>
    public class OperationResponse<T>
    {
        public T Item { get; set; }
        public double RequestUnitsConsumed { get; set; } = 0;
        public bool IsSuccessful { get; set; }
        public Exception CosmosException { get; set; }
    }
}
