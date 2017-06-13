using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace worker
{
    class Program
    {
        static string StorageAccountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT").Trim();
        static string StorageAccountKey = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCESS_KEY").Trim();

        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            Console.WriteLine($"StorageAccountName: {StorageAccountName}");
            if (String.IsNullOrEmpty(StorageAccountName)) {
                throw new ArgumentNullException(nameof(StorageAccountName));
            }

            Console.WriteLine($"StorageAccountKey: {StorageAccountKey}");
            if (String.IsNullOrEmpty(StorageAccountKey)) {
                throw new ArgumentNullException(nameof(StorageAccountKey));
            }

            var storageAccount = CloudStorageAccount.Parse($"DefaultEndpointsProtocol=https;AccountName={StorageAccountName};AccountKey={StorageAccountKey};EndpointSuffix=core.windows.net");

            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("votes");
            queue.CreateIfNotExistsAsync().Wait(cts.Token);

            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("votes");
            table.CreateIfNotExistsAsync().Wait(cts.Token);

            Console.WriteLine($"Starting worker...");

            try {
                while (!cts.IsCancellationRequested) {
                    CheckQueueAsync(queue, table).Wait(cts.Token);
                }
            } catch (OperationCanceledException) {
                // swallow
            } finally {
                cts.Dispose();
            }
        }

        private static async Task CheckQueueAsync(CloudQueue queue, CloudTable table) 
        {
            CloudQueueMessage retrievedMessage = await queue.GetMessageAsync();

            if (retrievedMessage == null) {
                return;
            }

            dynamic record = JsonConvert.DeserializeObject(retrievedMessage.AsString);
            Console.WriteLine($"Processing {record.voter_id} with {record.vote}");

            await table.CreateIfNotExistsAsync();

            // vote 
            var voteEntity = new VoteEntity((string)record.voter_id, (string)record.vote);
            await table.ExecuteAsync(TableOperation.InsertOrReplace(voteEntity));

            // vote count 
            var batchOperation = new TableBatchOperation();
            foreach (var voteCount in await GetVoteCount(table))
            {
                batchOperation.InsertOrReplace(new VoteCountEntity(voteCount.Key, voteCount.Value));
            }
            await table.ExecuteBatchAsync(batchOperation);
            await queue.DeleteMessageAsync(retrievedMessage);
        }

        private static async Task<IDictionary<string, int>> GetVoteCount(CloudTable table)
        {
            string filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, VoteEntity.PK);
            TableQuery<VoteEntity> tableQuery = new TableQuery<VoteEntity>().Where(filter);
            TableContinuationToken continuationToken = null;

            var voteCounts = new Dictionary<string, int>()
            {
                { "a", 0 },
                { "b", 0 },
            };

            do
            {
                var rows = await table.ExecuteQuerySegmentedAsync(tableQuery, continuationToken);
                foreach (var row in rows)
                {
                    voteCounts[row.Vote]++;
                }

                continuationToken = rows.ContinuationToken;
            } while (continuationToken != null);

            return voteCounts;
        }
    }
}
