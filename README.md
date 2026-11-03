<div align="center">

# realtime-telemetry-platform

**Hardware telemetry → MQTT → live HUD. One command, no hardware required.**

![License](https://img.shields.io/badge/license-MIT-00ff9c?style=flat-square&labelColor=0a0a0a)
![MQTT](https://img.shields.io/badge/MQTT-Mosquitto-00ff9c?style=flat-square&labelColor=0a0a0a)
![Docker](https://img.shields.io/badge/run%20with-docker%20compose-00ff9c?style=flat-square&labelColor=0a0a0a)

<img src="docs/demo.gif" width="720" alt="Live HUD">

</div>

## Run it

```bash
docker compose up
```

Open **http://localhost:8080**. Then run `docker compose stop simulator` to watch the HUD flag stale data.
Or try the [live demo](https://flicky-official.github.io/realtime-telemetry-platform/hud/?demo) (in-browser simulation).

## What this demonstrates

- A pub/sub telemetry pipeline: producer → broker → browser
- Reliability patterns: Last Will & Testament, stale-data detection, startup retry
- A documented message contract: [`docs/protocol.md`](docs/protocol.md)

## Architecture

```mermaid
flowchart LR
  S[Simulator<br/>10 Hz] -->|MQTT| B{{Mosquitto}}
  B -->|WebSocket :9001| H[Browser HUD]
```

## Design notes

- **QoS 0 for telemetry.** A dropped reading is replaced 100 ms later, so retransmitting old data is pointless.
- **QoS 1 + retained for status.** A late-joining client must learn online/offline immediately.
- **Simulator first.** Anyone can run the system without owning the hardware.

## Roadmap

- [x] Simulator, broker, browser HUD
- [ ] C++ telemetry daemon (replaces the simulator on real hardware)
- [ ] Next.js and Blazor clients on the same topic contract
- [ ] MQTT 5, PostgreSQL, Flutter client
- [ ] Latency benchmark
- [ ] TLS + authentication
- [ ] CI

## Known limitations

- The broker allows anonymous connections. **Demo only.** Production needs TLS and auth.
- Single vehicle, no persistence.
