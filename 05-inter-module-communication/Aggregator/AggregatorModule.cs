using ABB.Ability.IotEdge.CST.Modules.CSharp.Common;
using ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Messages;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.InterModuleCommunication.Aggregator
{
    public class AggregatorModule : ModuleBase
    {
        private List<TelemetryResult> _receivedTelemetry = new List<TelemetryResult>();

        public override async Task StartModuleAsync(string[] args)
        {
            Console.WriteLine("Starting C# Aggregator Module...");

            var terminate = new ManualResetEventSlim();

            await SubscribeToTopics();
            StartSendingTelemetryAsync(null);

            //wait indeterminately for messages to come in until the container is stopped 
            terminate.Wait();
        }

        private async Task SubscribeToTopics()
        {
            var topic = $"{ this.Configuration.LocalInTopic}/#";
            await _mqttClient.SubscribeAsync(topic).ContinueWith((e) => Console.WriteLine($"Subscribed to topic '{topic}'"));
        }

        protected override void OnApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            if (e.ApplicationMessage == null) return;
            var messageBody = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            if ((e.ApplicationMessage.Topic ?? string.Empty).StartsWith(this.Configuration.LocalInTopic, StringComparison.InvariantCultureIgnoreCase))
            {
                var jsonObj = JObject.Parse(messageBody);

                if (jsonObj["type"].Value<string>().Equals("timeSeries", StringComparison.InvariantCultureIgnoreCase))
                {
                    _receivedTelemetry.Add(new TelemetryResult(jsonObj["objectId"].Value<string>(),
                        jsonObj["variable"].Value<string>(),
                        jsonObj["value"].Value<double>(),
                        new DateTimeOffset(jsonObj["timestamp"].Value<DateTime>())));
                }
            }
        }

        public async override void StartSendingTelemetryAsync(dynamic parameter)
        {
            while (true)
            {
                //only send aggregation once a minute
                await Task.Delay(60000);

                var relevantData = _receivedTelemetry.Where(e => e.Timestamp > DateTimeOffset.Now.AddMinutes(-1));
                if (relevantData.Any())
                {
                    var variables = relevantData.Select(e => e.Variable).Distinct();
                    foreach (var v in variables)
                    {
                        var avg = Math.Round(relevantData.Where(e => e.Variable.Equals(v)).Select(e => e.Value).Average(), 2);
                        var msg = JsonConvert.SerializeObject(new TelemetryMessage<double>(Guid.Parse(this.Configuration.ObjectId), $"average{v}", () => avg));

                        await _mqttClient.PublishAsync(this.Configuration.MessagesOutTopic, msg)
                            .ContinueWith(e => { Console.WriteLine($"Published to topic '{this.Configuration.MessagesOutTopic}': {msg}"); });
                    }
                    
                    //sanity check to make sure all messages are is being received
                    Console.WriteLine($"Telemetry messages received in past minute: {relevantData.Count()}");
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: No telemetry received in the past minute");
                }
            }
        }
    }
}