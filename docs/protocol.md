# Message contract

| Topic | Direction | QoS | Retained | Payload |
|---|---|---|---|---|
| `vehicle/{id}/telemetry` | simulator -> HUD | 0 | no | `{"speed": 62.4, "rpm": 2800, "ts": 1718000000123}` |
| `vehicle/{id}/status` | simulator -> HUD | 1 | yes | `"online"` / `"offline"` (via Last Will) |

## Why these choices
- **Telemetry uses QoS 0.** A dropped reading is replaced 100 ms later, so retransmitting old data is pointless.
- **Status uses QoS 1 + retained.** A client that connects late must learn online/offline immediately, and the value must not get lost.
- **Last Will & Testament.** If the producer crashes, the broker announces `offline` on its behalf.
- **`ts` is milliseconds since epoch**, set by the producer, so consumers can detect stale data.
