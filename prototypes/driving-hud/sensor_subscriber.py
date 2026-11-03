import json
import time
import paho.mqtt.client as mqtt_client

broker = 'localhost'
port = 1883
topic = "vehicle/sensors"
client_id = 'python-sensor-subscriber'

# This function runs automatically whenever a new message arrives from your publisher
def on_message(client, userdata, msg):
    try:
        # Decode the incoming raw payload into a Python dictionary
        payload = msg.payload.decode('utf-8')
        data = json.loads(payload)

        status = data.get("status")
        seq = data.get("sequence")

        # Trigger mock actions based on the status
        print(f"\n[Incoming Packet #{seq}] Status: {status}")

        if status == "Lane Drift Left":
            print("🚨 [MOCK HUD WARNING]: FLASHING YELLOW LEFT BAR ON SCREEN")
            print("📳 [MOCK HAPTIC SEAT]: >>> LEFT SIDE VIBRATING (Vw-Vw-Vw) <<<")

        elif status == "Obstacle Detected Ahead":
            print("🛑 [MOCK HUD WARNING]: FLASHING RED BORDER - BRAKE NOW!")
            print("📳 [MOCK HAPTIC SEAT]: >>> BOTH SIDES HEAVY PULSE VIBRATION <<<")

        elif status == "Clear":
            print("✅ [SYSTEM STATUS]: Path clear. Monitoring...")

    except Exception as e:
        print(f"Error parsing message: {e}")

def run():
    # Setup the MQTT client (using v1.x syntax compatible with your Arch package)
    client = mqtt_client.Client(client_id)
    client.on_message = on_message

    print(f"Connecting to MQTT broker at {broker}:{port}...")
    client.connect(broker, port)

    # Subscribe to the vehicle sensor topic
    client.subscribe(topic)
    print(f"Subscribed to topic `{topic}`. Waiting for sensor data...\n")

    # Use loop_start() with a loop so KeyboardInterrupt exits cleanly
    client.loop_start()
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        client.loop_stop()
        client.disconnect()
        print("\nSubscriber stopped gracefully.")

if __name__ == '__main__':
    run()
