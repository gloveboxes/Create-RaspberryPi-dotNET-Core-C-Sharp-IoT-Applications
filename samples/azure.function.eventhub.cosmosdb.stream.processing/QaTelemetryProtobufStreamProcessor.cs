/**************************************************************************************************** 

Author: Dave Glover
Date: Aug 2020

References:

Working with Azure Cosmos DB in your Azure Functions    https://towardsdatascience.com/working-with-azure-cosmos-db-in-your-azure-functions-cc4f0f98a44d


Generate protobuf class:

Note, the TelemetryProtobuf.proto file match the send device protobuf definition in the samples/dotnet.core.iot.csharp.dps.iothub.protobuf project. 

https://docs.microsoft.com/en-us/azure/iot-accelerators/iot-accelerators-device-simulation-protobuf#generate-the-protobuf-class

protoc -I . --csharp_out=. TelemetryProtobuf.proto

****************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using DotNet.Core.IotHub.Models.Protobuf;

namespace Glovebox.Function
{
    public static class QaTelemetryProtobufStreamProcessor
    {
        [FunctionName("QaTelemetryProtobufStreamProcessor")]
        public static async Task Run([EventHubTrigger(
            Constants.EVENT_HUB_QA_TELEMETRY_PROTOBUF,
            ConsumerGroup = Constants.EVENT_HUB_CONSUMER_GROUP,
            Connection = "PACKAGING_QA_TELEMETRY_PROTOBUF_EVENTHUB"
            )] EventData[] events,
        [CosmosDB(
            databaseName: Constants.COSMOS_DB_DATABASE_NAME,
            collectionName: Constants.COSMOS_DB_CONTAINER_NAME,
            ConnectionStringSetting =  "FACTORY_COSMOS_DB"
            )] IAsyncCollector<object> items,
        ILogger log)
        {
            var exceptions = new List<Exception>();
            TelemetryProtobuf deserialized = null;

            foreach (EventData eventData in events)
            {
                try
                {

                    deserialized = TelemetryProtobuf.Parser.ParseFrom(eventData.Body.Array);

                    deserialized.DeviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();

                    await items.AddAsync(deserialized);


                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
