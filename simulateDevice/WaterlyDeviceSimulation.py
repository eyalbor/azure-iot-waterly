# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
# Using the Python Device SDK for IoT Hub:
# https://github.com/Azure/azure-iot-sdk-python
# The sample connects to a device-specific MQTT endpoint on your IoT Hub.

import random
import time
import json
import uuid
from azure.iot.device import IoTHubDeviceClient, Message
import numpy as np

# Define the JSON message to send to IoT Hub.

MSG_TXT = '{{"id": "{uuid}",'\
          '"device_id": "{device_id}",' \
          '"timestamp": {timestamp},' \
          '"water_read": {water_read},' \
          '"ph": {ph},' \
          '"water_pressure": {water_pressure}}}'


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


def iothub_client_telemetry_sample_run(device_connection_string, device_id):
    device_last_update_file_name = '{device_id}_last_update.json'.format(device_id=device_id)
    try:
        client = iothub_client_init(device_connection_string)
        print("IoT Hub device sending periodic messages, press Ctrl-C to exit" )

        last_water_read, last_read_timestamp = \
            init_last_water_read_and_last_read_timestamp(device_last_update_file_name)

        while True:
            # Build the message with simulated telemetry values.
            current_read_timestamp = time.time()
            current_water_read = last_water_read + (current_read_timestamp-last_read_timestamp) * 10 * random.random()
            ph = np.random.normal(7, 0.4, 1)[0]
            water_pressure = np.random.normal(4, 0.5, 1)[0]
            msg_txt_formatted = MSG_TXT.format(uuid=uuid.uuid1(),
                                               device_id=device_id,
                                               timestamp=int(current_read_timestamp),
                                               water_read=int(current_water_read),
                                               ph=ph,
                                               water_pressure=water_pressure)
            message = Message(msg_txt_formatted)

            # Add a custom application property to the message.
            # An IoT hub can filter on these properties without access to the message body.

            # Send the message.
            print("Sending message: {}".format(message))
            update_device_memory(json.loads(msg_txt_formatted), device_last_update_file_name)
            client.send_message(message)
            print("Message successfully sent")

            last_read_timestamp = current_read_timestamp
            last_water_read = current_water_read

            time.sleep(60)

    except KeyboardInterrupt:
        print("IoTHubClient sample stopped")



