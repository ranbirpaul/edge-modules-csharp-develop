using ABB.Ability.IotEdge.CST.Modules.CSharp.Common.Config;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.Common
{
    public abstract class ModuleBase
    {
        //Time, in milliseconds, to wait before attempting to reconnect if connection is dropped
        protected const double DEFAULT_AUTO_RECONNECT_DELAY_MS = 100;
        //Time, in milliseconds, of the MQTT client "heartbeat" to maintain the connection
        protected const double DEFAULT_KEEP_ALIVE_PERIOD_MS = 10000;
        //Default interval, in seconds, between sending of telemetry messages once started
        protected const int DEFAULT_TELEMETRY_INTERVAL_SECONDS = 5;
        //type of the device being used
        protected const string DEVICE_TYPE = "abb.ability.device.cst.environment.sensor@1";
        protected Regex _methodInvocationRegex;
        protected IManagedMqttClient _mqttClient;
        protected Random _rand = new Random();
        protected CancellationTokenSource _cancelTelemetry = null;

        public ModuleBase()
        {
            //used to parse the method name from the request
            _methodInvocationRegex = new Regex(
                $@"^{Regex.Escape(this.Configuration.MethodsInTopic)}/(?<{"methodName"}>.+)/(?<{"requestId"}>.+)$",
                RegexOptions.Compiled);

            StartMessagingClient();
        }

        protected string GenerateSerialNumber(int counter)
        {
            return counter.ToString().PadLeft(4, '0');
        }

        protected void HandleCloudToDeviceMethod(string topic, string messageBody)
        {
            //parse the method name out of the topic for the message
            var match = _methodInvocationRegex.Match(topic);
            var methodName = match.Groups["methodName"].Value;
            var requestId = match.Groups["requestId"].Value;

            //look for method to call, and if it exists, execute it via reflection with parameters passed in from message payload
            var methodInfo = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
            if (methodInfo == null)
            {
                Console.WriteLine($"The requested method '{methodName}' was not found in the module.");
                return;
            }
            var attr = methodInfo.GetCustomAttribute<InvokableMethod>();
            if (attr == null)
            {
                Console.WriteLine($"The requested method '{methodName}' is not invokable via remote commanding.");
                return;
            }

            var parameterInfo = methodInfo.GetParameters().FirstOrDefault();
            var parameters = parameterInfo != null
                ? new[] { JsonConvert.DeserializeObject(messageBody, parameterInfo.ParameterType) }
                : Array.Empty<object>();
            try
            {
                var confirmTopic = $"{this.Configuration.MethodsOutTopic}/{HttpStatusCode.OK}/{requestId}";
                _mqttClient.PublishAsync(confirmTopic, "{}");
                Console.WriteLine($"Requested method '{methodName}' found and will be executed. Confirmation published to: {confirmTopic}");

                methodInfo.Invoke(this, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while calling method '{methodName}': {ex.Message}");
            }
        }


        public abstract Task StartModuleAsync(string[] args);

        public abstract void StartSendingTelemetryAsync(dynamic parameter);

        protected abstract void OnApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e);

        private IConfiguration _configuration;
        protected IConfiguration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    //this environment variable is defined in the microsoft/dotnet:2.1-runtime-deps-alpine3.7 base image
                    var envVariable = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
                    var isDocker = (envVariable ?? string.Empty).Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase);
                    Console.WriteLine($"Is module running as a Docker container: {isDocker}");

                    _configuration = isDocker ? new EdgeConfiguration() : (IConfiguration)new ManualConfiguration();
                }
                return _configuration;
            }
        }

        private async void StartMessagingClient()
        {
            _mqttClient = new MqttFactory().CreateManagedMqttClient();

            //Build options for the MQTT client based off of config values
            //If running as a Docker container on Ability Edge, the Edge runtime will supply these values to the container
            var options = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(
                    new MQTTnet.Client.MqttClientOptionsBuilder()
                        .WithClientId(this.Configuration.ClientId)
                        .WithTcpServer(
                            this.Configuration.ServerUri.Host,
                            this.Configuration.ServerUri.Port)
                        .WithCredentials(
                            this.Configuration.Username,
                            this.Configuration.Password)
                        .WithKeepAlivePeriod(
                            TimeSpan.FromMilliseconds(
                            DEFAULT_KEEP_ALIVE_PERIOD_MS))
                        .WithCleanSession(this.Configuration.CleanSession)
                        .Build())
                .WithAutoReconnectDelay(
                    TimeSpan.FromMilliseconds(DEFAULT_AUTO_RECONNECT_DELAY_MS))
                .Build();

            _mqttClient.ApplicationMessageReceived += OnApplicationMessageReceived;

            await _mqttClient.StartAsync(options);
            Console.WriteLine($"MQTT client created and started.");
        }

        [InvokableMethod]
        protected void Start(dynamic parameter)
        {
            if (_cancelTelemetry == null)
            {
                _cancelTelemetry = new CancellationTokenSource();
                StartSendingTelemetryAsync(parameter);
            }
            else
            {
                Console.WriteLine("Start command received, but telemetry sending is already in progress.");
            }
        }

        [InvokableMethod]
        protected void Stop()
        {
            Console.WriteLine("Request to stop timeseries publishing received.");
            //stop sending telemetry
            if (_cancelTelemetry != null)
            {
                _cancelTelemetry.Cancel();
                _cancelTelemetry = null;
            }
        }
    }
}
