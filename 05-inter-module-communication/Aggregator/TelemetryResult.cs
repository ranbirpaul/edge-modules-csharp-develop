using System;

namespace ABB.Ability.IotEdge.CST.Modules.CSharp.InterModuleCommunication.Aggregator
{
    public class TelemetryResult
    {
        public TelemetryResult(string deviceId, string variable, double value, DateTimeOffset timestamp)
        {
            DeviceId = deviceId;
            Variable = variable;
            Value = value;
            Timestamp = timestamp;
        }

        public string DeviceId { get; private set; }

        public string Variable { get; private set; }

        public double Value { get; private set; }

        public DateTimeOffset Timestamp { get; private set; }

    }
}