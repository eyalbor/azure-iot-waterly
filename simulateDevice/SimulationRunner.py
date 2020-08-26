# Using the Python Device SDK for IoT Hub:
# https://github.com/Azure/azure-iot-sdk-python
# The sample connects to a device-specific MQTT endpoint on your IoT Hub.

import random
import time
import json
from azure.cosmos import CosmosClient, PartitionKey, exceptions
from azure.iot.device import IoTHubDeviceClient, Message
from azure.iot.hub import IoTHubRegistryManager
from WaterlyDeviceSimulation import iothub_client_telemetry_sample_run
import threading

COSMOS_CONNECTION_STRING = "AccountEndpoint=https://waterly-iot.documents.azure.com:443/;AccountKey=cC49BNfE7uQTuEVdSNeJAUZuzTjpzl5j0MLSsb8aHGL6jGh3JmubV2TAbgxW05vYtmMA8LqTitsbRPjUZY8YsA==;"
COSMOS_DB = "waterly_db"
COSMOS_DB_CONTAINER = "water_table"
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


# def query_db():
#     client = CosmosClient.from_connection_string(COSMOS_CONNECTION_STRING)
#     database = client.get_database_client(COSMOS_DB)
#     container = database.get_container_client(COSMOS_DB_CONTAINER)
#     device_query = 'SELECT * FROM c WHERE c.device_id="{device_id}"'.format(device_id=DEVICE_ID)
#     for item in container.query_items(
#             query=device_query,
#             enable_cross_partition_query=True):
#         print(json.dumps(item, indent=True))
#
#
# def get_iot_devices_from_iot_hub(connection_string):
#     registry = IoTHubRegistryManager(IOTHUB_CONNECTION_STRING)
#     return registry.get_devices()


class DeviceThread(threading.Thread):

    def __init__(self, thread_id, device_connection_string, device_id):
        threading.Thread.__init__(self)
        self.threadID = thread_id
        self.device_connection_string = device_connection_string
        self.device_id = device_id

    def run(self):
        print("Starting " + self.device_id)
        iothub_client_telemetry_sample_run(self.device_connection_string, self.device_id)


if __name__ == '__main__':
    print("IoT Hub - Simulated device")
    print("Press Ctrl-C to exit")

    threads = []

    # Create new threads, start, add to the list
    # thread1 = DeviceThread(1, DEVICE_CONNECTION_STRING1, DEVICE_ID1)
    # thread1.start()
    # threads.append(thread1)
    #
    # thread2 = DeviceThread(2, DEVICE_CONNECTION_STRING2, DEVICE_ID2)
    # thread2.start()
    # threads.append(thread2)
    #
    # thread3 = DeviceThread(3, DEVICE_CONNECTION_STRING3, DEVICE_ID3)
    # thread3.start()
    # threads.append(thread3)
    #
    # thread4 = DeviceThread(4, DEVICE_CONNECTION_STRING4, DEVICE_ID4)
    # thread4.start()
    # threads.append(thread4)

    # thread5 = DeviceThread(5, DEVICE_CONNECTION_STRING5, DEVICE_ID5)
    # thread5.start()
    # threads.append(thread5)

    thread_index = 0
    for device, connection_string in DEVICES.items():
        t = DeviceThread(thread_index, connection_string, device)
        t.start()
        threads.append(t)
        thread_index += 1

    # Wait for all threads to complete
    for t in threads:
        t.join()
    print("Exiting Main Thread")

