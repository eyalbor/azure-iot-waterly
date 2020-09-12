# Using the Python Device SDK for IoT Hub:
# https://github.com/Azure/azure-iot-sdk-python
# The sample connects to a device-specific MQTT endpoint on your IoT Hub.

from WaterlyDeviceSimulation import iothub_client_telemetry_sample_run
import threading

COSMOS_CONNECTION_STRING = "AccountEndpoint=https://waterly-iot.documents.azure.com:443/;AccountKey=cC49BNfE7uQTuEVdSNeJAUZuzTjpzl5j0MLSsb8aHGL6jGh3JmubV2TAbgxW05vYtmMA8LqTitsbRPjUZY8YsA==;"
IOTHUB_CONNECTION_STRING = "HostName=WaterlyIoTHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=Nmpsy46w8JaU7/Ssd7vB18lMxzomjHdcdBgtJsPY1iU="

DEVICE_CONNECTION_STRINGS = \
    ["HostName=WaterlyIoTHub.azure-devices.net;DeviceId=WaterlyIotDevice1;SharedAccessKey=eljF4px1Z/xPDqmhjU5VlKCf7aotRgkmlkArJbJi83A=",
     "HostName=WaterlyIoTHub.azure-devices.net;DeviceId=WaterlyIotDevice2;SharedAccessKey=KFcPJAZLf6p+zdCv2qKDwMK7ZNIlBZ75oLXPjZ9MmUs=",
     "HostName=WaterlyIoTHub.azure-devices.net;DeviceId=WaterlyIotDevice3;SharedAccessKey=5+rSDG3SsrD+UUPnL9PHq8RfXZYek4toC5raCOvLQUk=",
     "HostName=WaterlyIoTHub.azure-devices.net;DeviceId=WaterlyIotDevice4;SharedAccessKey=9axZVLfcHreylumdZWGUtMzxsMruRn//V9pxBZ/RjEg=",
    "HostName=WaterlyIoTHub.azure-devices.net;DeviceId=WaterlyIotDevice5;SharedAccessKey=Kdb7mwp5StAyUipmRC2IVJ6U9wdMuIJGPlVIj83d94g=",
    "HostName=WaterlyIoTHub.azure-devices.net;DeviceId=WaterlyIotDevice6;SharedAccessKey=oEz+sOfBEzLDE/iMghhx/maIlAdj4bocmWR4LYNUnYc=",
    "HostName=WaterlyIoTHub.azure-devices.net;DeviceId=WaterlyIotDevice7;SharedAccessKey=3U9QH0g+mpzppe1h/6qxQH45YgL8vj3BL6rcnowQb1Y=",
    "HostName=WaterlyIoTHub.azure-devices.net;DeviceId=WaterlyIotDevice8;SharedAccessKey=Qf1hL0bkK7LxdTTfzVoRfm2Y3lsGBJulj9DR2V90sqU=",
    "HostName=WaterlyIoTHub.azure-devices.net;DeviceId=WaterlyIotDevice9;SharedAccessKey=wD/ajYx2cDT0gAqG6DuguXs0yOn77vdCwNp90n/THgU=",
    "HostName=WaterlyIoTHub.azure-devices.net;DeviceId=WaterlyIotDevice10;SharedAccessKey=IOvsTdX4rJNDzFc1QRrR3TxpWsRY7xhdCh4+OD6OsVc="]

DEVICES = {}
for i in range(len(DEVICE_CONNECTION_STRINGS)):
    connection_string = DEVICE_CONNECTION_STRINGS[i]
    key = "WaterlyIotDevice"+str(i+1)
    DEVICES[key] = connection_string


class DeviceThread(threading.Thread):

    def __init__(self, thread_id, device_connection_string, device_id):
        threading.Thread.__init__(self)
        self.threadID = thread_id
        self.device_connection_string = device_connection_string
        self.device_id = device_id

    def run(self):
        print("Starting {}\n".format(self.device_id))
        iothub_client_telemetry_sample_run(self.device_connection_string, self.device_id)


if __name__ == '__main__':
    print("\nIoT Hub - Simulated device")
    print("Press Ctrl-C to exit\n")

    threads = []
    thread_index = 0
    for device, connection_string in DEVICES.items():
        # if thread_index > 4:
        t = DeviceThread(thread_index, connection_string, device)
        t.start()
        threads.append(t)
        thread_index += 1

    # Wait for all threads to complete
    for t in threads:
        t.join()
    print("Exiting Main Thread")

