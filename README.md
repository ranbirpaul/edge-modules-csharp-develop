# ABB Ability Edge modules samples

Sample Ability Edge modules in C# programming language. You cant find 
different examples of C# Ability Edge modules in the subdirectories of this repository.

## What's included?

* `common` - common C# code used across all the examples, such as MQTT connectivity and module configuration
* `typedefinitions` - JSON files with type definitions used by those
  examples. Please make sure to upload them to your Ability Platform instance's Type Definition Registry before launching the examples
* `01-send-telemetry` - demonstrates how to create a simulated device and send timeSeries data on a predefined time interval
* `02-events-alarms` - expands upon the previous module to send events and alarms when timeSeries data is reported beyond a given threshold
* `03-remote-commands` - based upon the first module, shows how to use remote commands to start and stop sending telemetry
* `04-configurable` - this module creates a variable number of devices based on the configuration model, and reacts to any subsequent changes in the model
* `05-inter-module-communication` - the first module is changed to send its telemetry data to another module, which then sends aggregations to the cloud
* `06-files-storage` - creates a connected device for which it creates a file with device's objectID and uploads it to the file storage in the cloud.

More samples will be available soon. Please stay tuned.

## Reporting bugs
Please report all the bugs and issues with those samples using the [Issues](https://codebits.abb.com/ABB-Ability-Client-Success-Team/tutorials/edge-modules-csharp/issues) tracker of this repository.

## More information
To get more in-depth information about Ability Platform and Ability Edge, please refer to the [ABB Ability Wiki](https://abb.sharepoint.com/sites/ABBAbility/Wiki) or ask your Client Success Team representative.


_Copyright 2019 ABB Ability Client Success Team_