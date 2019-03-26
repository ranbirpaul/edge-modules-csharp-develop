using ABB.Ability.IotEdge.CST.Modules.CSharp.Common;
using ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Messages;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.SendTelemetry
{
    public class SendTelemetryModule : ModuleBase
    {
        private readonly string[] _telemetryVariables = { "temperature", "humidity", "pressure" };
        private Guid _deviceId;
        private string _deviceSerialNumber;

        public override async Task StartModuleAsync(string[] args)
        {
            Console.WriteLine("Starting C# Send Telemetry Module...");

            var terminate = new ManualResetEventSlim();

            var topic = $"{ this.Configuration.ModelInTopic}/#";
            //await _mqttClient.SubscribeAsync(topic).ContinueWith(
            //    (e) => Console.WriteLine($"Subscribed to topic '{topic}'"));

            await CreateDevice();
            // Test

            //wait indeterminately until the container/application is stopped 
            terminate.Wait();
        }

        protected override void OnApplicationMessageReceived(object sender, 
            MqttApplicationMessageReceivedEventArgs e)
        {
            if (e.ApplicationMessage == null) return;
            var messageBody = Encoding.UTF8.GetString(
                e.ApplicationMessage.Payload);
            Console.WriteLine($"Message received on topic " +
                "'{e.ApplicationMessage.Topic}': {messageBody}");

            if ((e.ApplicationMessage.Topic ?? string.Empty)
                .StartsWith(this.Configuration.ModelInTopic, 
                    StringComparison.InvariantCultureIgnoreCase))
            {
                var j = JObject.Parse(messageBody);
                if (j["type"] != null && 
                    j["type"].Value<string>() == DEVICE_TYPE)
                {
                    var serialNumber = j["properties"]["serialNumber"]["value"]
                        .Value<string>();
                    if (serialNumber.Equals(_deviceSerialNumber))
                    {
                        //this is our created device
                        _deviceId = Guid.Parse(j["objectId"].Value<string>());
                        Console.WriteLine("Device created confirmation " +
                            "received. Starting telemetry...");
                        StartSendingTelemetryAsync(null);
                    }
                }
            }
        }

        private async Task CreateDevice()
        {
            var topic = this.Configuration.MessagesOutTopic +
                "type=deviceCreated";

            _deviceSerialNumber = GenerateSerialNumber(1);
            var msg = JsonConvert.SerializeObject(new CreateDeviceMessage(
                DEVICE_TYPE,
                _deviceSerialNumber,
                this.Configuration.ObjectId,
                "devices"));

            await _mqttClient.PublishAsync(topic, msg)
                .ContinueWith((e) => Console.WriteLine(
                $"Published message to topic '{topic}': {msg}"));
        }

        public async override void StartSendingTelemetryAsync(dynamic param)
        {
            Console.WriteLine("Starting timeseries publishing...");

            var topic = $"{Configuration.MessagesOutTopic}/type=timeSeries";
            var rand = new Random();

            while (true)
            {
                foreach (var variableName in _telemetryVariables)
                {
                    var msg = JsonConvert.SerializeObject(new TelemetryMessage<double>(
                        _deviceId,
                        variableName,
                        () => Math.Round(rand.NextDouble() * 50d, 2)));
                    await _mqttClient.PublishAsync(topic, msg)
                        .ContinueWith((e) => Console.WriteLine(
                            $"Published message to topic '{topic}': {msg}"));
                }

                //send telemetry every 10 seconds
                await Task.Delay(10000);
            }
        }
    }
}