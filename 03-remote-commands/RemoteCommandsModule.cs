using ABB.Ability.IotEdge.CST.Modules.CSharp.Common;
using ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Messages;
using MQTTnet;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.C2DMessages
{
    public class RemoteCommandsModule : ModuleBase
    {
        //Default frequency of events
        private const double DEFAULT_EVENT_FREQUENCY = 0.15;
        //Default frequency of alarms
        private const double DEFAULT_ALARM_FREQUENCY = 0.05;

        private readonly string[] _telemetryVariables = { "temperature" };

        private Guid _deviceId;
        private string _deviceSerialNumber;

        public override async Task StartModuleAsync(string[] args)
        {
            Console.WriteLine("Starting C# Remote Commands Module...");

            var terminate = new ManualResetEventSlim();

            await SubscribeToTopics();
            await CreateDevice();

            //wait indeterminately for C2D messages until the container/application is stopped 
            terminate.Wait();
        }

        private async Task SubscribeToTopics()
        {
            var topics = new[] { $"{ this.Configuration.ModelInTopic}/#", $"{this.Configuration.MethodsInTopic}/#" };
            var topicFilters = topics.Select(e => new TopicFilter(e, MqttQualityOfServiceLevel.AtLeastOnce));

            await _mqttClient.SubscribeAsync(topicFilters).ContinueWith((e) => Console.WriteLine($"Subscribed to topics: {string.Join(", ", topics)}"));
        }

        private async Task CreateDevice()
        {
            var topic = $"{this.Configuration.MessagesOutTopic}/type=deviceCreated";

            _deviceSerialNumber = GenerateSerialNumber(1);
            var msg = JsonConvert.SerializeObject(new CreateDeviceMessage(DEVICE_TYPE, _deviceSerialNumber, this.Configuration.ObjectId, "devices"));

            await _mqttClient.PublishAsync(topic, msg).ContinueWith((e) => Console.WriteLine($"Published message to topic '{topic}': {msg}"));
        }

        protected override void OnApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            if (e.ApplicationMessage == null) return;
            var messageBody = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            Console.WriteLine($"Message received on topic '{e.ApplicationMessage.Topic}': {messageBody}");

            if ((e.ApplicationMessage.Topic ?? string.Empty).StartsWith(this.Configuration.ModelInTopic, StringComparison.InvariantCultureIgnoreCase))
            {
                var jsonObj = JObject.Parse(messageBody);
                if (jsonObj["type"] != null && jsonObj["type"].Value<string>() == DEVICE_TYPE)
                {
                    var serialNumber = jsonObj["properties"]["serialNumber"]["value"].Value<string>();
                    if (serialNumber.Equals(_deviceSerialNumber))
                    {
                        //this is our created device
                        _deviceId = Guid.Parse(jsonObj["objectId"].Value<string>());
                        Console.WriteLine("Device created confirmation received.");
                    }
                }
            }
            else if((e.ApplicationMessage.Topic ?? string.Empty).StartsWith(this.Configuration.MethodsInTopic, StringComparison.InvariantCultureIgnoreCase))
            {
                HandleCloudToDeviceMethod(e.ApplicationMessage.Topic, messageBody);
            }
        }

        public async override void StartSendingTelemetryAsync(dynamic parameter)
        {
            int interval = int.TryParse(parameter.interval.ToString(), out interval) ? interval : DEFAULT_TELEMETRY_INTERVAL_SECONDS;
            var rand = new Random();
            var timeSeriesTopic = $"{this.Configuration.MessagesOutTopic}/type=timeSeries";
            var eventsTopic = $"{this.Configuration.MessagesOutTopic}/type=event";
            var alarmsTopic = $"{this.Configuration.MessagesOutTopic}/type=alarm";
            Console.WriteLine("Timeseries publishing started.");

            while (true)
            {
                foreach (var v in _telemetryVariables)
                {
                    var msg = JsonConvert.SerializeObject(new TelemetryMessage<double>(_deviceId, v, () => { return Math.Round(rand.NextDouble() * 50d, 2); }));
                    await _mqttClient.PublishAsync(timeSeriesTopic, msg).ContinueWith((e) => Console.WriteLine($"Published message to topic '{timeSeriesTopic}': {msg}"));
                }

                if (rand.NextDouble() > DEFAULT_EVENT_FREQUENCY)
                {
                    var eventMsg = JsonConvert.SerializeObject(new EventMessage<double>(_deviceId, "Sample Event", () => { return Math.Round(rand.NextDouble() * 50d, 2); }));
                    await _mqttClient.PublishAsync(eventsTopic, eventMsg).ContinueWith((e) => Console.WriteLine($"Published message to topic '{eventsTopic}': {eventMsg}"));

                }
                if (rand.NextDouble() > DEFAULT_ALARM_FREQUENCY)
                {
                    var alarmMsg = JsonConvert.SerializeObject(new TelemetryMessage<double>(_deviceId, "Sample Alarm", () => { return Math.Round(rand.NextDouble() * 50d, 2); }));
                    await _mqttClient.PublishAsync(alarmsTopic, alarmMsg).ContinueWith((e) => Console.WriteLine($"Published message to topic '{alarmsTopic}': {alarmMsg}"));
                }

                try
                {
                    //wait to send another telemetry message, unless the stop method has been invoked
                    await Task.Delay(interval * 1000, _cancelTelemetry.Token);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Timeseries publishing stopped.");
                    break;
                }
            }
        }
    }
}