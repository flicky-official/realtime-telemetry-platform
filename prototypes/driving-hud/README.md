# Driving HUD prototype

Legacy Python/Tkinter demonstration of simulated driving alerts over MQTT.

## Safety and scope

Simulation only. Not suitable for real driving, collision avoidance,
lane-departure detection, or other safety-critical use.

Alerts are randomly generated. No real vehicle sensors or haptic
actuators are connected.

## Components

- sensor_publisher.py: publishes randomly selected alert statuses.
- sensor_subscriber.py: prints simulated HUD and haptic actions.
- hud_display.py: displays alerts in a Tkinter window.
- run_system.py: launches the three scripts as subprocesses.

## Message contract

Topic: vehicle/sensors

Payload fields:
- sequence: publisher sequence number.
- status: simulated alert text.
- timestamp: Unix timestamp in seconds.

## Compatibility and limitations

- Expects an anonymous MQTT broker at localhost:1883.
- Does not authenticate to the laptop-monitor broker.
- Uses older Paho client constructor conventions; requires migration
  for Paho 2.x.
- Does not explicitly select MQTT 5.
- Has no stale-data detection or reliable reconnect/resubscribe handling.
- The launcher assumes the scripts are in the current working directory.
- Runtime dependencies include paho-mqtt and Python Tkinter.
- Dependencies have not been pinned or runtime-tested during import.

Do not run alongside the active monitoring stack without first reviewing
broker configuration and client compatibility.

## Verification

Imported as a source snapshot. Python syntax is checked during migration;
GUI behavior and MQTT integration are not yet verified.

See IMPORT.md for source provenance.
