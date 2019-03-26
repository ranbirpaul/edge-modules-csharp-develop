using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ABB.Ability.IotEdge.CST.Modules.CSharp.Common;
using ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Messages;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.FileStorage
{
    public class UploadFileModule : ModuleBase
    {
        private bool _noDeviceConnected = true;
        private const string _correlationID = "file_upload";
        private int _filesUploaded = 0;

        public override async Task StartModuleAsync(string[] args)
        {
            Console.WriteLine("Starting C# File Upload Module...");

            var terminate = new ManualResetEventSlim();

            var modelTopic = $"{this.Configuration.ModelInTopic}/#";
            var filesInTopic = $"{this.Configuration.FilesInTopic}/#";

            await _mqttClient.SubscribeAsync(filesInTopic).ContinueWith((e) => Console.WriteLine($"Subscribed to topic '{filesInTopic}'"));
            await _mqttClient.SubscribeAsync(modelTopic).ContinueWith((e) => Console.WriteLine($"Subscribed to topic '{modelTopic}'"));

            await CreateDeviceIfNecessary();
            
            //wait indeterminately until the container/application is stopped 
            terminate.Wait();
        }

        public string FileName(string deviceId)
        {
            return deviceId + ".txt";
        }

        public string CreateSampleFile(string deviceId)
        {
            var fileName = FileName(deviceId);
            System.IO.File.WriteAllText("files/" + fileName, "Sample content for device with id: " + deviceId);
            return fileName;
        }

        public string LoadFile(string deviceId)
        {
            var fileName = FileName(deviceId);
            return System.IO.File.ReadAllText("files/" + fileName);
        }

        public void UploadFile(string deviceId)
        {
            var fileName = CreateSampleFile(deviceId);

            var filesOutTopic = $"{this.Configuration.FilesOutTopic}";
            var sendFileMessage = new SendFileMessage(deviceId, "abb.ability.device", fileName, fileName, false, _filesUploaded.ToString());
            var sendFileJsonString = JsonConvert.SerializeObject(sendFileMessage);
            _mqttClient.PublishAsync(filesOutTopic, sendFileJsonString).ContinueWith((e) => Console.WriteLine($"Published message to topic '{filesOutTopic}': {sendFileJsonString}"));
            _filesUploaded++;
        }

        protected override void OnApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            if (e.ApplicationMessage == null) return;

            var messageBody = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            Console.WriteLine($"Message received on topic '{e.ApplicationMessage.Topic}': {messageBody}");
            var jsonObj = JObject.Parse(messageBody);
            var deviceId = Guid.Parse(jsonObj["objectId"].Value<string>()).ToString();

            if ((e.ApplicationMessage.Topic ?? string.Empty).StartsWith(this.Configuration.ModelInTopic, StringComparison.InvariantCultureIgnoreCase))
            {
                if (jsonObj["type"] != null && jsonObj["type"].Value<string>() == DEVICE_TYPE)
                {
                    Console.WriteLine("There is a connected device. Beginning file upload for deviceId: " + deviceId);
                    try
                    {
                        //File for this device exists. Print in logs
                        string fileContent = LoadFile(deviceId);
                        Console.WriteLine("Found a persistent file at files/" + deviceId + " with content: \n" + fileContent + "\nCloud upload aborted.");
                    }
                    catch
                    {
                        //File does not exist. Create a file and upload to cloud
                        UploadFile(deviceId);
                    }
                    _noDeviceConnected = false;
                }
            }
            else if ((e.ApplicationMessage.Topic ?? string.Empty).StartsWith(this.Configuration.FilesInTopic, StringComparison.InvariantCultureIgnoreCase))
            {
                //This is the notification for our file upload
                var isSuccess = jsonObj["isSuccess"].Value<bool>();
                Console.WriteLine("File upload for device: " + deviceId + " " + (isSuccess ? "succeeded" : "failed"));
            }
        }

        private async Task CreateDeviceIfNecessary()
        {
            //Wait for 2 seconds to gather data about connected devices
            await Task.Delay(2000);
            if (_noDeviceConnected)
            {
                await CreateDevice();
            }
        }

        private async Task CreateDevice()
        {
            var topic = $"{this.Configuration.MessagesOutTopic}/type=deviceCreated";

            //Let's simulate a device serial number 
            var deviceSerialNumber = Guid.NewGuid().ToString().Replace("-", "");
            var msgObj = new CreateDeviceMessage(DEVICE_TYPE, deviceSerialNumber, this.Configuration.ObjectId, "devices");
            var msg = JsonConvert.SerializeObject(msgObj);

            await _mqttClient.PublishAsync(topic, msg).ContinueWith((e) => Console.WriteLine($"Published message to topic '{topic}': {msg}"));
        }

        public override void StartSendingTelemetryAsync(dynamic parameter)
        {
            throw new NotImplementedException();
        }
    }
}