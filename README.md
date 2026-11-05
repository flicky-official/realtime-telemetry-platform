<div align="center">

# realtime-telemetry-platform

**Telemetry over MQTT: a one-command vehicle demo, and a laptop-monitoring backend in progress.**

[![CI](https://github.com/flicky-official/realtime-telemetry-platform/actions/workflows/ci.yml/badge.svg)](https://github.com/flicky-official/realtime-telemetry-platform/actions/workflows/ci.yml)
![License](https://img.shields.io/badge/license-MIT-00ff9c?style=flat-square&labelColor=0a0a0a)
![MQTT](https://img.shields.io/badge/MQTT-Mosquitto-00ff9c?style=flat-square&labelColor=0a0a0a)
![Docker](https://img.shields.io/badge/run%20with-docker%20compose-00ff9c?style=flat-square&labelColor=0a0a0a)

<img src="docs/demo.gif" width="720" alt="HUD running live, losing signal when the producer stops, then recovering">

</div>

## What's in this repo

| Path | What it is | Status |
|---|---|---|
| `/` (root) | Vehicle telemetry demo: simulator → Mosquitto → browser HUD | Working. Runs with `docker compose up` |
| [`projects/laptop-monitor/`](projects/laptop-monitor) | ASP.NET Core (.NET 10) API, PostgreSQL and an MQTT 5 subscriber for laptop hardware telemetry | Foundation working. Persistence and agent not built yet |
| [`prototypes/driving-hud/`](prototypes/driving-hud) | Tkinter driving-alert HUD | Prototype. **Simulation only, not for real driving** |

## Quickstart: vehicle demo

```bash
git clone https://github.com/flicky-official/realtime-telemetry-platform
cd realtime-telemetry-platform
docker compose up
```

Open **http://localhost:8080**. No Docker? Try the [live demo](https://flicky-official.github.io/realtime-telemetry-platform/hud/?demo), an in-browser simulation of the same HUD.

> The demo broker allows anonymous connections and publishes ports 1883, 9001 and 8080.
> The laptop-monitor stack also uses 1883, so run only one of the two at a time.

### Try the failure modes

The point of a telemetry display is knowing when to stop trusting it.

```bash
docker compose stop simulator    # producer dies  -> HUD turns red: NO DATA within 2 s
docker compose start simulator   # producer back  -> HUD recovers on its own
docker compose restart broker    # broker bounces -> browser reconnects, no refresh needed
```

## Architecture: vehicle demo

<p align="center"><img src="docs/architecture.svg" width="860" alt="Simulator publishes MQTT to Mosquitto, which serves the browser HUD over WebSockets"></p>

| Component | Role |
|---|---|
| `simulator/` | Python producer publishing speed and RPM at 10 Hz, with a Last Will |
| `broker/` | Eclipse Mosquitto: MQTT on 1883, MQTT-over-WebSocket on 9001 for browsers |
| `hud/` | Static single-file dashboard (no build step) subscribing straight from the browser |

### Message contract

```text
vehicle/demo/telemetry   QoS 0   {"speed": 62.4, "rpm": 2800, "ts": 1718000000123}
vehicle/demo/status      QoS 1   "online" | "offline"   (retained, set via Last Will)
```

Full table and reasoning: [`docs/protocol.md`](docs/protocol.md)

## Laptop monitor (in progress)

A backend for collecting real hardware telemetry from a laptop. See [`projects/laptop-monitor/`](projects/laptop-monitor) for setup.

| Works today | Not built yet |
|---|---|
| PostgreSQL 17 and Mosquitto 2 via Compose, bound to `127.0.0.1` | MQTT → PostgreSQL ingestion (validated, duplicate-safe) |
| EF Core migration: `devices` and `telemetry_samples` tables | The monitoring agent that publishes real CPU/GPU/fan data |
| `GET /health/db` and `GET /devices/{id}/latest` (404 for unknown devices) | Bounded history API for graphs |
| MQTT 5 subscriber on `telemetry/+/samples` (QoS 1), **logging only** | Flutter dashboard |
| CI: .NET Release build and secret scan | Production auth and durable delivery handling |

Unlike the vehicle demo, this stack uses a Mosquitto password file. It is local-only and not hardened for production.

## Design decisions

| Decision | Why | Tradeoff |
|---|---|---|
| QoS 0 for vehicle telemetry | A dropped reading is replaced 100 ms later; retrying old data is pointless | Occasional gaps are possible |
| QoS 1 + retained for status | A late-joining client must learn online/offline immediately | One retained message per vehicle |
| Last Will & Testament | The broker announces `offline` if the producer crashes | Detection time depends on keepalive |
| Producer-side timestamps | The HUD can compute true message age and detect staleness | Assumes roughly synced clocks |
| Browser connects directly via WebSocket | No backend to build or deploy | Needs auth and TLS before real use |
| QoS 1 subscription in the laptop monitor | Samples should survive brief disconnects | Duplicates are possible, so ingestion must be idempotent |
| Simulator first | Anyone can run the demo without hardware | Not real sensor behaviour |

## Roadmap

| Now | Next | Later |
|---|---|---|
| Vehicle demo with stale-data failsafe | MQTT → PostgreSQL ingestion | Flutter dashboard |
| Laptop-monitor API foundation | Laptop monitoring agent (CPU/GPU temps, fans) | MQTT 5 message expiry for the vehicle demo |
| CI: smoke test, .NET build, secret scan | Bounded history API | TLS + authentication |
| | | Latency benchmark (p50/p95) |

## Repo layout

```text
.
├── broker/                Mosquitto config (vehicle demo)
├── simulator/             Python telemetry producer (vehicle demo)
├── hud/                   Browser HUD, static, no build step
├── docs/                  Protocol, architecture diagram, demo assets
├── tools/                 record_demo.py (regenerates docs/demo.gif)
├── projects/
│   └── laptop-monitor/    .NET 10 API + PostgreSQL + MQTT 5 subscriber
├── prototypes/
│   └── driving-hud/       Tkinter simulation (not for real driving)
├── .github/workflows/     CI
└── docker-compose.yml     Vehicle demo stack
```

## Known limitations

- The vehicle-demo broker allows anonymous connections. **Demo only.** Production needs TLS and auth.
- Vehicle demo: single vehicle, no persistence.
- Laptop monitor: received MQTT messages are logged, not stored. There is no agent yet.
- The driving HUD prototype randomly simulates alerts. It is not ADAS and has no stale-data failsafe.
- No latency numbers yet. They'll be added when measured, not estimated.
