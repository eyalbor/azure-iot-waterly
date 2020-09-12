# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
# Using the Python Device SDK for IoT Hub:
# https://github.com/Azure/azure-iot-sdk-python
# The sample connects to a device-specific MQTT endpoint on your IoT Hub.

import random
import time
import json
from azure.iot.device import IoTHubDeviceClient, Message
import numpy as np
import threading

# Define the JSON message to send to IoT Hub.

MSG_TXT = '{{"device_id": "{device_id}",' \
          '"timestamp": {timestamp},' \
          '"water_read": {water_read},' \
          '"ph": {ph},' \
          '"pressure": {pressure},' \
          '"salinity": {salinity}}}'


def iothub_client_init(device_connection_string):
    # Create an IoT Hub client
    client = IoTHubDeviceClient.create_from_connection_string(device_connection_string)
    return client


def init_last_water_read_and_last_read_timestamp(device_last_update_file_name):
    try:
        with open(device_last_update_file_name) as json_file:
            json_msg = json.load(json_file)
    except IOError:
        json_msg = {}
    json_msg = dict(json_msg)
    last_water_read = json_msg.get('water_read', 0)
    last_read_timestamp = json_msg.get('timestamp', time.time())
    return last_water_read, last_read_timestamp


def update_device_memory(json_msg, device_last_update_file_name):
    with open(device_last_update_file_name, 'w') as outfile:
        json.dump(json_msg, outfile)


def message_listener(client, device_id):
    while True:
        message = client.receive_message()
        print("\nMessage received from Azure to device {}: ".format(device_id))
        print(message + "\n")


def iothub_client_telemetry_sample_run(device_connection_string, device_id):
    device_last_update_file_name = '{device_id}_last_update.json'.format(device_id=device_id)
    try:
        client = iothub_client_init(device_connection_string)
        print("IoT Hub device sending periodic messages from " + device_id + ", press Ctrl-C to exit\n")

        # open listener to device
        message_listener_thread = threading.Thread(target=message_listener, args=(client, device_id))
        message_listener_thread.daemon = True
        message_listener_thread.start()

        last_water_read, last_read_timestamp = \
            init_last_water_read_and_last_read_timestamp(device_last_update_file_name)

        while True:
            # Build the message with simulated telemetry values.
            current_read_timestamp = time.time()
            current_water_read = last_water_read + (current_read_timestamp-last_read_timestamp) * 10 * random.random()
            ph = abs(np.random.normal(7, 0.4, 1)[0])
            water_pressure = abs(np.random.normal(4, 0.5, 1)[0])
            salinity = abs(np.random.normal(200, 40, 1)[0])
            msg_txt_formatted = MSG_TXT.format(device_id=device_id,
                                               timestamp=int(current_read_timestamp),
                                               water_read=int(current_water_read),
                                               ph=ph,
                                               pressure=water_pressure,
                                               salinity=salinity)
            message = Message(msg_txt_formatted)

            # Send the message.
            print("\n{} sending message: {}\n".format(device_id, message))
            update_device_memory(json.loads(msg_txt_formatted), device_last_update_file_name)
            client.send_message(message)
            print("{}: Message successfully sent\n".format(device_id))

            last_read_timestamp = current_read_timestamp
            last_water_read = current_water_read

            time.sleep(60)

    except KeyboardInterrupt:
        print("IoTHubClient sample stopped")



