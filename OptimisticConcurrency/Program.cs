namespace AzCosmosDB.Samples.SharedLib
{
    using System;
    using System.Net;
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
            var client = CosmosConfig.GetConnection(new CosmosClientOptions() { AllowBulkExecution = true });
            var container = await CosmosConfig.CreateContainerAsync(client, "aztestdb", "userorder", "id", 1000);

            //sessions 01
            //더미 아이템 생성
            var dummy = GeneratorModels.GenerateUserSingle();
            var response1 = await container.CreateItemAsync(dummy);

            Console.WriteLine($"sessions 01 response _etag : {response1.ETag.ToString()}");

            //sessions 02
            //다른 세션에서 이메일을 변경한다. (동일 id값의 데이터를 replace)
            dummy.Email = "dummy@cloocus.com";
            var response2 = await container.ReplaceItemAsync(dummy, dummy.Id);
            Console.WriteLine($"sessions 02 response _etag : {response2.ETag.ToString()}");

            //sessions 01
            //현재 데이터를 업데이트. sessions 02와 충돌 sessions 01에서는 00의 email을 가지고 있어서 replace 시 이메일이 기존 이메일로 덮어 써지게 된다
            //원자성을 보장하기 위해 ItemRequestOptions에 response의 etag값을 비교해 다를 경우 exception 처리를 한다

            try
            {
                await container.ReplaceItemAsync(item: dummy, id: dummy.Id, requestOptions: new ItemRequestOptions { IfMatchEtag = response1.ETag });
            }
            catch (CosmosException cre)
            {
                //   now notice the failure when attempting the update 
                //   this is because the ETag on the server no longer matches the ETag of doc (b/c it was changed in step 2)
                if (cre.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    Console.WriteLine("As expected, we have a pre-condition failure exception\n");
                }
            }

            Console.ReadLine();
        }
    }
}
