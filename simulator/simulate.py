import json
import math
import os
import time

import paho.mqtt.client as mqtt

HOST = os.getenv("MQTT_HOST", "localhost")
PORT = int(os.getenv("MQTT_PORT", "1883"))
VEHICLE = os.getenv("VEHICLE_ID", "demo")
HZ = 10  # messages per second

client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2)

# Last Will: if this process dies, the broker publishes "offline" for us.
client.will_set(f"vehicle/{VEHICLE}/status", "offline", qos=1, retain=True)

# Docker starts containers in parallel, so the broker may not be ready yet.
while True:
    try:
        client.connect(HOST, PORT)
        break
    except OSError:
        print("Broker not ready, retrying...")
        time.sleep(1)

client.loop_start()
client.publish(f"vehicle/{VEHICLE}/status", "online", qos=1, retain=True)

start = time.time()
while True:
    t = time.time() - start
    speed = 45 + 25 * math.sin(t / 8)                 # smooth 20-70 mph
    rpm = 1000 + speed * 35 + 150 * math.sin(t * 2)   # loosely tied to speed
    payload = {
        "speed": round(speed, 1),
        "rpm": int(rpm),
        "ts": int(time.time() * 1000),                # ms since epoch
    }
    client.publish(f"vehicle/{VEHICLE}/telemetry", json.dumps(payload), qos=0)
    time.sleep(1 / HZ)
