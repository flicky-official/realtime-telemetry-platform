import subprocess
import time
import sys

def main():
    print("🚀 Starting ADAS Prototype System...")

    # List of scripts to run concurrently
    scripts = ["sensor_publisher.py", "sensor_subscriber.py", "hud_display.py"]
    processes = []

    try:
        # Launch each script as an independent subprocess
        for script in scripts:
            print(f"-> Launching {script}...")
            p = subprocess.Popen([sys.executable, script])
            processes.append(p)
            time.sleep(0.5) # Small stagger so output doesn't collide

        print("\n✅ All systems running! Press Ctrl+C to shut everything down cleanly.\n")

        # Keep the master script alive while subprocesses run
        while True:
            time.sleep(1)

    except KeyboardInterrupt:
        print("\n🛑 Shutting down system gracefully...")
        for p in processes:
            p.terminate()
            p.wait()
        print("All processes stopped.")

if __name__ == '__main__':
    main()
