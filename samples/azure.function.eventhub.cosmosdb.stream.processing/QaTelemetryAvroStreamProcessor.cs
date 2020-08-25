/**************************************************************************************************** 

Author: Dave Glover
Date: Aug 2020

References:

Working with Azure Cosmos DB in your Azure Functions    https://towardsdatascience.com/working-with-azure-cosmos-db-in-your-azure-functions-cc4f0f98a44d

Note, the TelemetryAvro.cs file must match the send device TelemetryAvro.cs file in the samples/dotnet.core.iot.csharp.dps.iothub.avro project. 

****************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using DotNet.Core.IotHub.Avro;
using SolTechnology.Avro;


namespace Glovebox.Function
{
    public static class QaTelemetryAvroStreamProcessor
    {
        static string avroTelemetrySchema = AvroConvert.GenerateSchema(typeof(TelemetryAvro));

        [FunctionName("QaTelemetryAvroStreamProcessor")]
        public static async Task Run(
            [EventHubTrigger(
                Constants.EVENT_HUB_QA_TELEMETRY_AVRO,
                ConsumerGroup = Constants.EVENT_HUB_CONSUMER_GROUP,
                Connection = "PACKAGING_QA_TELEMETRY_AVRO_EVENTHUB"
                )] EventData[] events,
            [CosmosDB(
                databaseName: Constants.COSMOS_DB_DATABASE_NAME,
                collectionName: Constants.COSMOS_DB_CONTAINER_NAME,
                ConnectionStringSetting =  "FACTORY_COSMOS_DB"
                )] IAsyncCollector<object> items,
            ILogger log)
        {
            var exceptions = new List<Exception>();

            TelemetryAvro deserialized = null;

            foreach (EventData eventData in events)
            {
                try
                {
                    deserialized = AvroConvert.DeserializeHeadless<TelemetryAvro>(eventData.Body.Array, avroTelemetrySchema);

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
