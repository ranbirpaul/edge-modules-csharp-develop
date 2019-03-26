using ABB.Ability.IotEdge.CST.Modules.CSharp.Common;
using ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Messages;
using MQTTnet;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.ListenForConfigChanges
{
    public class ConfigurableModule : ModuleBase
    {
        private int _devicesCreated = 0;
        private readonly object _configureDevicesLock = new object();
        private Dictionary<Guid, string> _deviceMap = new Dictionary<Guid, string>();
        private int _telemetryInterval = DEFAULT_TELEMETRY_INTERVAL_SECONDS;

        private readonly string[] _telemetryVariables = { "temperature" };

        public override async Task StartModuleAsync(string[] args)
        {
            Console.WriteLine("Starting C# Configurable Module...");

            var terminate = new ManualResetEventSlim();

            await SubscribeToTopics();

            //request the configuration model for this module
            //once retrieved, determine how many devices to create and what the sending interval should be
            await RequestModel();

            //wait indeterminately for C2D messages until the container/application is stopped 
            terminate.Wait();
        }

        private async Task SubscribeToTopics()
        {
            var topics = new[] { $"{ this.Configuration.ModelInTopic}/#", $"{this.Configuration.MethodsInTopic}/#" };
            var topicFilters = topics.Select(e => new TopicFilter(e, MqttQualityOfServiceLevel.AtLeastOnce));
            await _mqttClient.SubscribeAsync(topicFilters);
        }
        private async Task RequestModel()
        {
            //get the configuration model for this module so we know how many devices to create and what the publishing interval should be
            var topic = $"{this.Configuration.MessagesOutTopic}/type=model&objectId={this.Configuration.ObjectId}&model=abb.ability.configuration";
            await _mqttClient.PublishAsync(topic).ContinueWith(e => { Console.WriteLine($"Published to topic '{topic}' with empty payload"); });
        }

        protected override void OnApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            if (e.ApplicationMessage == null) return;
            var messageBody = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            //methods
            if ((e.ApplicationMessage.Topic ?? string.Empty).StartsWith(this.Configuration.MethodsInTopic, StringComparison.InvariantCultureIgnoreCase))
            {
                HandleCloudToDeviceMethod(e.ApplicationMessage.Topic, messageBody);
            }
            else if ((e.ApplicationMessage.Topic ?? string.Empty).StartsWith(this.Configuration.ModelInTopic, StringComparison.InvariantCultureIgnoreCase))
            {
                var jsonObj = JObject.Parse(messageBody);

                if ((e.ApplicationMessage.Topic ?? string.Empty).EndsWith("abb.ability.configuration", StringComparison.InvariantCultureIgnoreCase) &&
                    jsonObj["objectId"] != null && jsonObj["objectId"].Value<string>() == this.Configuration.ObjectId)
                {
                    //if the configuration model for the module is received
                    var requestedDevices = jsonObj["properties"]["numberOfDevices"]["value"].Value<int>();
                    _telemetryInterval = jsonObj["properties"]["telemetryInterval"]["value"].Value<int>();

                    if (requestedDevices != _devicesCreated)
                    {
                        //if the desired number of devices is different than what we have
                        Console.WriteLine($"Module config retrieved: {messageBody}");
                        lock (_configureDevicesLock)
                        {
                            ConfigureDevices(requestedDevices).Wait();
                        }
                    }
                }
                else if (jsonObj["type"] != null && jsonObj["type"].Value<string>() == DEVICE_TYPE)
                {
                    var deviceId = Guid.Parse(jsonObj["objectId"].Value<string>());

                    if (_devicesCreated > _deviceMap.Count && !_deviceMap.ContainsKey(deviceId))
                    {
                        //if we are waiting on a model from a created device and this id isn't in the map, add it
                        Console.WriteLine($"Device model received: {messageBody}");

                        var serialNumber = jsonObj["properties"]["serialNumber"]["value"].Value<string>();
                        _deviceMap.Add(deviceId, serialNumber);
                        Console.WriteLine($"Device {serialNumber} added to list of known devices");
                    }
                }
            }
        }

        private async Task ConfigureDevices(int requestedDevices)
        {
            Console.WriteLine($"Request to configure devices received. Requested: {requestedDevices}. Currently: {_devicesCreated}");

            var createDeviceTopic = $"{this.Configuration.MessagesOutTopic}/type=deviceCreated";
            while (_devicesCreated < requestedDevices)
            {
                var msg = JsonConvert.SerializeObject(new CreateDeviceMessage(DEVICE_TYPE, GenerateSerialNumber(_devicesCreated + 1), this.Configuration.ObjectId, "devices"));
                Console.WriteLine($"Publishing message to topic '{createDeviceTopic}': {msg}");
                await _mqttClient.PublishAsync(createDeviceTopic, msg).ContinueWith((e) => _devicesCreated++);
            }

            var deleteDeviceTopic = $"{this.Configuration.MessagesOutTopic}/type=deviceDeleted";
            while (_devicesCreated > requestedDevices)
            {
                var device = _deviceMap.FirstOrDefault(e => e.Value.Equals(GenerateSerialNumber(_devicesCreated), StringComparison.InvariantCultureIgnoreCase));
                if (device.Key != Guid.Empty)
                {
                    Console.WriteLine($"Publishing message to topic '{deleteDeviceTopic}&objectId={device.Key}' with empty payload");
                    await _mqttClient.PublishAsync($"{deleteDeviceTopic}&objectId={device.Key}").ContinueWith(e => _deviceMap.Remove(device.Key));
                }
                _devicesCreated--;
            }
        }

        public async override void StartSendingTelemetryAsync(dynamic parameter)
        {
            var rand = new Random();
            var topic = $"{this.Configuration.MessagesOutTopic}/type=timeSeries";
            Console.WriteLine($"Timeseries publishing started for {_deviceMap.Count} devices.");

            while (true)
            {
                foreach (var deviceId in _deviceMap.Keys)
                {
                    var msgs = _telemetryVariables.Select(variable => new MqttApplicationMessage()
                    {
                        QualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce,
                        Topic = topic,
                        Payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new TelemetryMessage<double>(deviceId, variable, () => { return Math.Round(rand.NextDouble() * 50d, 2); })))
                    }).ToArray();

                    await _mqttClient.PublishAsync(msgs).ContinueWith(e => Console.WriteLine($"Published {msgs.Length} messages to topic '{topic}'"));
                }

                try
                {
                    //wait to send another telemetry message, unless the stop method has been invoked
                    await Task.Delay(_telemetryInterval * 1000, _cancelTelemetry.Token);
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