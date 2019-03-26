'use strict';

const fs = require('fs'), util = require('util');
const pem = require('pem');
const Registry = require('azure-iothub').Registry;
const chalk = require('chalk');
const argv = require('yargs')
    .usage('Usage: $0 --connectionString <IOTHUB CONNECTION STRING> --deviceId <DEVICE ID> --daysValid <number of days valid>')
    .demand(['connectionString', 'deviceId'])
    .default('certificateDirectory', '../files/ability/certs')
    .default('daysValid', 1)
    .describe('connectionString', 'Azure IoT Hub service connection string that shall have device creation permissions')
    .describe('deviceId', 'Unique identifier for the device that shall be created')
    .describe('daysValid', 'Number of days the certificate will remain valid')
    .describe('certificateDirectory', 'Directory name to put the certificates in')
    .argv;

const certFile = argv.certificateDirectory + '/edgedevice-cert.pem';
const keyFile = argv.certificateDirectory + '/edgedevice-key.pem';
let thumbprint = null;

function createCertsAndDevice(done) {
    let certOptions = {
        selfSigned: true,
        commonName: argv.deviceId,
        days: argv.daysValid
    };

    pem.createCertificate(certOptions, function (err, result) {
        if (err) {
            done(err);
        } else {
            console.log(util.inspect(result));
            fs.writeFileSync(certFile, result.certificate);
            fs.writeFileSync(keyFile, result.clientKey);
            pem.getFingerprint(result.certificate, function (err, result) {
                thumbprint = result.fingerprint.replace(/:/g, '');

                let deviceDescription = {
                    deviceId: argv.deviceId,
                    status: 'enabled',
                    authentication: {
                        x509Thumbprint: {
                            primaryThumbprint: thumbprint
                        }
                    }
                };

                registry.create(deviceDescription, function (err, deviceInfo) {
                    if(!!err) {
                        console.log(chalk.red('Could not create device: ' + err.message + '\n' + err.responseBody));
                        process.exit(1);
                    } else {
                        console.log(chalk.green('Device successfully created:'));
                        console.log(JSON.stringify(deviceInfo, null, 2));
                    }
                });
            });
        }
    });
}

let registry = Registry.fromConnectionString(argv.connectionString);
registry.get(argv.deviceId, function(err) {
    if (!err) {
        console.log(chalk.red('Device already exists: ' + argv.deviceId));
        process.exit(1);
    } else {
        createCertsAndDevice(function(err) {
            if (err) {
                console.log(chalk.red('Could not create certificates or device: ' + err.message));
                process.exit(1);
            } else {
                console.log(chalk.green('Device \'' + argv.deviceId + '\' created successfully.'));
                console.log(chalk.white('\tcertificate file: ' + certFile));
                console.log(chalk.white('\tkey file: ' + keyFile));
                process.exit(0);
            }
        });
    }
});
