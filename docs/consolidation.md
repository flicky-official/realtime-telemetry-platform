# Repository consolidation

## Destination

realtime-telemetry-platform is the intended parent repository.

Private source must be reviewed for secrets, ownership, and third-party
license requirements before publication.

## Current status

| Source | Status | Outstanding work |
|---|---|---|
| SQL-MQTT-FLUTTER | Snapshot imported into projects/laptop-monitor | Public review, CI, and merge |
| mqtt-driving-hud | Snapshot imported into prototypes/driving-hud | Syntax checked; runtime compatibility and integration not verified |
| mqtt-hud-blazor | History backed up; source not imported | Review source; exclude tracked bin/ and obj/ |
| mqtt-hud-nextjs | History backed up; source not imported | Review dashboard and publisher; verify remote repository name |
| mqtt-hud-web | History backed up; source not imported | Review Next.js files and remaining Flutter files |
| mqtt_hud | History backed up; source not imported | Review remaining Flutter files and publisher |

## Verified laptop-monitor functionality

- Imported API builds.
- Database health endpoint successfully queries the existing database.
- Latest-reading endpoint returns the registered laptop.
- Latest-reading database query uses LIMIT 1.
- MQTT subscriber receives a test message.
- MQTT subscriber currently logs messages; persistence is not implemented.

## Flutter findings

The main.dart and pubspec.yaml files in mqtt-hud-web and mqtt_hud
were compared and are identical.

Other files have not yet been fully compared. Neither repository
should be treated as fully redundant.

Flutter is not currently available on the development shell PATH.

## Python publisher findings

The mqtt_hud publisher includes CPU, RAM, and network metrics, but also
contains synthetic fallback temperatures and a hardcoded GPU load.

The mqtt-hud-web publisher has more specific CPU sensor matching,
but reports missing readings as zero and polls aggressively.

Both need authenticated MQTT configuration, explicit protocol selection,
network-loop handling, and a new telemetry contract before reuse.

## Operational safety

- Automatic Git syncing is disabled pending replacement.
- Original repositories and verified history bundles are retained.
- Git bundles do not include uncommitted files, ignored credentials,
  or database contents.
- The original SQL-MQTT-FLUTTER Compose environment remains active.
- No original MQTT repository has been archived as part of this migration.
