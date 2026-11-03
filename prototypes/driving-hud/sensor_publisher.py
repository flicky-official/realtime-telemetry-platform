import time
import json
import random
import paho.mqtt.client as mqtt_client

broker = 'localhost'
port = 1883
topic = "vehicle/sensors"
client_id = f'python-sensor-publisher-{random.randint(0, 1000)}'

def run():
    client = mqtt_client.Client(client_id)
    client.connect(broker, port)
    client.loop_start()

    msg_count = 0
    statuses = ["Clear", "Lane Drift Left", "Obstacle Detected Ahead", "Clear"]

    try:
        while True:
            time.sleep(3)
            status = random.choice(statuses)

            data = {
                "sequence": msg_count,
                "status": status,
                "timestamp": time.time()
            }
            payload = json.dumps(data)

            result = client.publish(topic, payload)
            if result[0] == 0:
                print(f"Sent `{payload}` to topic `{topic}`")
            else:
                print(f"Failed to send message to topic {topic}")

            msg_count += 1
    except KeyboardInterrupt:
        client.loop_stop()
        print("\nPublisher stopped gracefully.")

if __name__ == '__main__':
    run()
