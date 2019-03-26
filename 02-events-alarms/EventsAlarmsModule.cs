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
    public class EventsAlarmsModule : ModuleBase
    {
        private const double ALARM_TEMPERATURE_TRIGGER_THRESHOLD = 100;
        private const double ALARM_TEMPERATURE_CANCEL_THRESHOLD = 80;

        private Guid _deviceId;
        private string _deviceSerialNumber;
        private bool alarmActive = false;

        public override async Task StartModuleAsync(string[] args)
        {
            Console.WriteLine("Starting C# Events & Alarms Module...");

            var terminate = new ManualResetEventSlim();

            //send moduleStarted event
            var eventsTopic = $"{Configuration.MessagesOutTopic}/type=event";
            var msg = JsonConvert.SerializeObject(new EventMessage<double>(Guid.Parse(Configuration.ObjectId), "abb.ability.eventsModule.started", () => 0));
            await _mqttClient.PublishAsync(eventsTopic, msg).ContinueWith((e) => Console.WriteLine($"Published message to topic '{eventsTopic}': {msg}"));

            var modelTopic = $"{Configuration.ModelInTopic}/#";
            await _mqttClient.SubscribeAsync(modelTopic).ContinueWith((e) => Console.WriteLine($"Subscribed to topic '{modelTopic}'"));

            await CreateDevice();

            //wait indeterminately until the container/application is stopped 
            terminate.Wait();

            //send moduleStopped event
            msg = JsonConvert.SerializeObject(new EventMessage<double>(Guid.Parse(Configuration.ObjectId), "abb.ability.ambientsensor.moduleStopped", () => 0));
            await _mqttClient.PublishAsync(eventsTopic, msg).ContinueWith((e) => Console.WriteLine($"Published message to topic '{eventsTopic}': {msg}"));
        }

        protected override void OnApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            if (e.ApplicationMessage == null) return;
            var messageBody = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            Console.WriteLine($"Message received on topic '{e.ApplicationMessage.Topic}': {messageBody}");

            if ((e.ApplicationMessage.Topic ?? string.Empty).StartsWith(Configuration.ModelInTopic, StringComparison.InvariantCultureIgnoreCase))
            {
                var jsonObj = JObject.Parse(messageBody);
                if (jsonObj["type"] != null && jsonObj["type"].Value<string>() == DEVICE_TYPE)
                {
                    var serialNumber = jsonObj["properties"]["serialNumber"]["value"].Value<string>();
                    if (serialNumber.Equals(GenerateSerialNumber(1)))
                    {
                        //this is our created device
                        _deviceId = Guid.Parse(jsonObj["objectId"].Value<string>());
                        Console.WriteLine("Device created confirmation received. Beginning events and alarms publishing...");
                        StartSendingTelemetryAsync(null);
                    }
                }
            }
        }

        private async Task CreateDevice()
        {
            var topic = $"{Configuration.MessagesOutTopic}/type=deviceCreated";

            _deviceSerialNumber = GenerateSerialNumber(1);
            var msg = JsonConvert.SerializeObject(new CreateDeviceMessage(DEVICE_TYPE, _deviceSerialNumber, Configuration.ObjectId, "devices"));

            await _mqttClient.PublishAsync(topic, msg).ContinueWith((e) => Console.WriteLine($"Published message to topic '{topic}': {msg}"));
        }

        public async override void StartSendingTelemetryAsync(dynamic parameter)
        {
            Console.WriteLine("Starting events and alarms publishing...");
            var rand = new Random();

            while (true)
            {
                //Generate random temperature to simulate the sensor reading
                var randTemp = Math.Round(rand.NextDouble() * 150, 2);

                if (randTemp > ALARM_TEMPERATURE_TRIGGER_THRESHOLD)
                {
                    SendAlarm(randTemp);
                    alarmActive = true;
                } else if (randTemp < ALARM_TEMPERATURE_CANCEL_THRESHOLD && alarmActive)
                {
                    CancelAlarm();
                    alarmActive = false;
                }
                await Task.Delay(10000);
            }
        }

        public async void SendAlarm(double Temp)
        {
            var alarmsTopic = $"{Configuration.MessagesOutTopic}/type=alarm";

            var msg = JsonConvert.SerializeObject(new AlarmMessage<double>(_deviceId, "TempToHigh", true, () => Temp));
            await _mqttClient.PublishAsync(alarmsTopic, msg).ContinueWith((e) => Console.WriteLine($"Published message to topic '{alarmsTopic}': {msg}"));
        }

        public async void CancelAlarm()
        {
            var alarmsTopic = $"{Configuration.MessagesOutTopic}/type=alarm";

            var msg = JsonConvert.SerializeObject(new AlarmMessage<double>(_deviceId, "TempToHigh", false));
            await _mqttClient.PublishAsync(alarmsTopic, msg).ContinueWith((e) => Console.WriteLine($"Published message to topic '{alarmsTopic}': {msg}"));
        }
    }
}