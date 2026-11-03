import json
import tkinter as tk
from paho.mqtt import client as mqtt_client

broker = 'localhost'
port = 1883
topic = "vehicle/sensors"
client_id = 'python-hud-display'

# Set up the main graphical window
root = tk.Tk()
root.title("Accessible ADAS HUD")
root.geometry("600x350")
root.configure(bg="black")

# HUD Title Label
title_label = tk.Label(root, text="ADAPTIVE RETROFIT HUD", font=("Arial", 14, "bold"), fg="gray", bg="black")
title_label.pack(pady=15)

# Main Status Display Box
status_label = tk.Label(root, text="INITIALIZING SYSTEM...", font=("Arial", 20, "bold"), fg="white", bg="black", wraplength=550)
status_label.pack(expand=True)

# Haptic / Seat Alert Indicator
haptic_label = tk.Label(root, text="SEAT HAPTICS: STANDBY", font=("Arial", 12), fg="cyan", bg="black")
haptic_label.pack(pady=20)

def update_hud(status):
    if status == "Clear":
        root.configure(bg="black")
        title_label.configure(bg="black", fg="gray")
        status_label.configure(text="SYSTEM CLEAR\nMonitoring Road...", fg="green", bg="black")
        haptic_label.configure(text="SEAT HAPTICS: IDLE", fg="cyan", bg="black")

    elif status == "Lane Drift Left":
        root.configure(bg="#3b2e00") # Dark yellow tint
        title_label.configure(bg="#3b2e00", fg="yellow")
        status_label.configure(text="⚠️ WARNING: LANE DRIFT LEFT", fg="yellow", bg="#3b2e00")
        haptic_label.configure(text="📳 SEAT VIBRATION: LEFT SIDE ACTIVE", fg="yellow", bg="#3b2e00")

    elif status == "Obstacle Detected Ahead":
        root.configure(bg="#4a0000") # Dark red tint
        title_label.configure(bg="#4a0000", fg="red")
        status_label.configure(text="🛑 COLLISION ALERT: BRAKE NOW!", fg="white", bg="#4a0000")
        haptic_label.configure(text="📳 SEAT VIBRATION: DUAL PULSE ACTIVE", fg="red", bg="#4a0000")

def on_message(client, userdata, msg):
    try:
        payload = msg.payload.decode('utf-8')
        data = json.loads(payload)
        status = data.get("status")

        # Schedule the UI update safely on the main thread
        root.after(0, update_hud, status)
    except Exception as e:
        print(f"UI Error: {e}")

def run_mqtt():
    client = mqtt_client.Client(client_id)
    client.on_message = on_message
    client.connect(broker, port)
    client.subscribe(topic)
    client.loop_forever()

# Start the MQTT listener in the background using tkinter's thread handling
import threading
mqtt_thread = threading.Thread(target=run_mqtt, daemon=True)
mqtt_thread.start()

# Run the graphical window loop
root.mainloop()
