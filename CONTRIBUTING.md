# Contributing

All contributons to this samples repository are very welcome. To create a new sample please send a pull request to this repo. Please mind **we are using [GitFlow](https://nvie.com/posts/a-successful-git-branching-model/)** in this repository. Please keep all your work
in a feature branch and refrain from doing any commits to the `master`, `develop` or release branches.

## HOWTO
1. [Creating a fork](https://docs.gitlab.com/ee/gitlab-basics/fork-project.html) of this repository in your private namespace
2. Create a feature branch according to [GitFlow](https://danielkummer.github.io/git-flow-cheatsheet/). Prefix the name with `TASKNNNN-` where `NNNN` is a VSTS feature numer.
   ```bash
   git checkout origin/develop
   git checkout -b feature/TASKNNNN-tutorial-title
   ```
3. Do some coding! Make sure to put your new example in a directory conforming to the naming scheme 
   `NN-example-title`, where NN is a two digit number of a tutorial and `example-title` is a _lower case_
   name of the application separated with dashes. Also review the content of the `common` directory and
   reuse all the files you can (request/response models, MQTT client etc). You can also put a code in the `common` directory if you believe it can be reused by more applications. The name of your .csproj shoul be the same as a subdirectory.
4. Make sure to update a `README.md` file by adding a short description of your tutorial.
5. Add all the type definition you are using to the `typedefinitions` subfolder. Make sure to keep to the
   naming convention.
6. Add your .csproj project to the `edge-modules-csharp.sln` solution:
   ```bash
   dotnet sln edge-modules-csharp.sln add 07-advanced-telemetry-aggregation/07-advanced-telemetry-aggregation.csproj
   ```
7. When you're done, rebase your feature branch to the current `origin/develop` and send    a pull request to the `develop` branch. Make sure to gracefully resolve all the 
   conflists not loosing someone else's code. Put me (Adam Kolakowski) in the reviewers list.
   ```bash
   git fetch origin/develop
   git rebase origin/develop
   git add <your files>
   git commit -m "ADDED 07-advanced-telemetry-aggregation sample"
   git push
   ```

## File naming convention
1. Keep all names in ASCII charset.
2. Use lowercase where possible
3. Use a dash (-) as a divider
4. Some of the users use case sensitive filesystems (Linux, MacOS). Make sure 
   to put correct casing in references
5. Start sample projects subdirectories with 2 digit number (lower number = simpler 
   project).
6. Keep all common files (DTO, models, common abstraction) in `common` folder
7. Name type definition JSON files as `NN-type.definition.id-vX.json` where NN is a 2 
   digit number (same as 1st sample module that use it) and `X` is a master version number of a type definition.

## Make git commit meaningful
This is a public repository, available to both Ability enginering team and 
Ability Platform users. To make it easier to read the changelog of the repository
we are trying to make commit messages self explanatory. Let's avoid "fixed some 
bugs" messages. [Read more](https://chris.beams.io/posts/git-commit/) about 
good commit messages.

### Good commit messages
- `ADDED 07-advanced-telemetry-aggregation project`
- `FIXED Issue #12`
- `CHANGED 07-advanced-telemetry-aggregation logic for a telemetry timer`

### Bad commit message
- `Added some files`
- `Fixed some typo`
- `Fixed something in aggregator module`
- `commit ;-)`

Prefix all your commit messages with `TASKNNNN: ` where `NNNN` is a VSTS feature number.

## Ambient Environment Sensor
For the sake of simplicity and consistency of our tutorial program, we use 
an ambient environment sensor device (mostly simulated) in all simple 
tutorials. This device can report various information about itself listed 
in a table below.
In all simple _hello-world-like_ examples, whenever we take input data from a device 
that is external to the Ability Edge, we use the same, well defined device.

   | Field        | Data type | Direction | Description                     |
   |--------------|-----------|-----------|---------------------------------|
   | serialNumber | String    | GET       | device unique serial number     |
   | firmwareVersion | String | GET       | current firmware version (ex. 1.0.0) |
   | temperature  | Number    | GET       | Ambient temperature in C        |
   | humidity     | Number    | GET       | Ambient humidity in percent     |
   | pressure     | Number    | GET       | Ambient atmospheric pressure in hPa |
   | interval     | Number    | SET       | Measurement interval in seconds |

This Ambient Environment Sensor has a well known type definition, HTTP API 
(for those samples where we actually read data from the device, not simulate it).
   