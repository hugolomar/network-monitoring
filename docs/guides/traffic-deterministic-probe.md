# Traffic Simulator and Deterministic Probe

**Audience:** Developers validating capture, Kafka publication, and downstream inventory locally  
**Prerequisites:** Docker, reference stack running, `tools/traffic` composer dependencies installed  
**Related:** `tools/traffic/README.md`, `specs/001-session-detection/quickstart.md`, `docs/notes/003-probe-separated-stack.md`

## Goal

Run repeatable end-to-end validation using scenario-built PCAPs and the probe in `DeterministicTest`
mode — especially on WSL, where live `tcpreplay` often fails to reproduce expected device identities.

## When to use / when not to use

| Approach | Use when |
|----------|----------|
| **Deterministic PCAP** (`build-pcap` + `DeterministicTest`) | WSL, CI, or any environment where live capture distorts MAC/IP identities |
| **Live replay** (`tcpreplay` + probe `Live` mode) | Physical Linux hosts where replayed packets preserve scenario identities on the capture interface |

On WSL, live replay against `eth0` typically surfaces virtual bridge MACs, not the MACs encoded in
scenario PCAPs. Device intersection with expected scenario identities can be zero.

## Two ways to simulate traffic

| Path | How it works | Best for |
|------|--------------|----------|
| **Live replay** (`tcpreplay`) | Composer or ad-hoc replay sends packets to a host interface (`eth0`, etc.). Probe runs in `Live` mode and captures from that interface. | Physical Linux hosts or environments where replayed MAC/IP identities match the scenario. |
| **Deterministic PCAP** (`build-pcap` + probe) | Composer builds one merged PCAP from a scenario YAML. Probe runs in `DeterministicTest` mode and reads the file with `tshark -r`. | WSL, CI, and any run where interface capture would distort identities or make results non-reproducible. |

## Steps

1. Start the reference stack (Kafka, backend, integration-console, Postgres, etc.):

   ```bash
   bash ./infrastructure/stack/bootstrap/reference-stack-init.sh
   ```

2. Build and validate scenario PCAPs:

   ```bash
   python3 -m pip install -r tools/traffic/composer/requirements.txt
   ./tools/traffic/run.sh build-pcap --file tools/traffic/scenarios/five-minute-mac-coverage.yaml
   ./tools/traffic/run.sh validate-pcaps --file tools/traffic/scenarios/five-minute-mac-coverage.yaml
   ```

   Output: `tools/traffic/pcaps/generated/<scenario-name>.pcap`

3. Run the probe against that PCAP (Docker example):

   ```bash
   docker compose -f docker-compose.probe.yml run --rm --build \
     -v "$(pwd)/tools/traffic/pcaps/generated:/pcaps:ro" \
     -e Probe__InputMode=DeterministicTest \
     -e Probe__DeterministicTestPcapPath=/pcaps/five-minute-mac-coverage.pcap \
     -e Probe__DeterministicPlaybackSpeed=0 \
     -e Probe__KafkaBootstrapServers=127.0.0.1:9092 \
     probe
   ```

4. Verify downstream effects, for example:

   - Devices in inventory: `curl -sS http://localhost:5090/devices`
   - Kafka offsets: topics `devices.detected`, `sessions.detected`

The probe stack is intentionally separate from the reference stack; see `docs/notes/003-probe-separated-stack.md`.

## Playback speed (`Probe__DeterministicPlaybackSpeed`)

Applies **only** in `DeterministicTest` mode. Controls how long the probe waits between observations
based on PCAP packet timestamps — not how fast `tshark` reads the file.

| Value | Wall-clock behavior | Kafka / observability profile | Typical use |
|-------|---------------------|-------------------------------|-------------|
| **`0` (default)** | Ingest as fast as possible (seconds, not minutes). | Events arrive in **bursts**: high produce rate; `occurredAtUtc` still reflects PCAP timestamps but delivery to consumers is back-to-back. | Smoke tests, filling inventory quickly, functional regression, CI slices where duration does not matter. |
| **`1`** | Real-time pacing: a 300 s scenario takes ~5 minutes. | **More realistic observability**: consumers, Connect sinks, and dashboards see traffic spread over the scenario timeline. | Manual E2E demos, lag and deduplication window validation, behaviour closer to production capture rate. |
| **`10`, `100`, …** | Accelerated pacing: scenario duration divided by the multiplier (~30 s for 300 s at `10`). | Middle ground between burst load and full real-time. | Long scenarios (`one-hour-realistic`) where `1` is too slow and `0` is too bursty. |

Formula: delay between consecutive observations = `(timestamp delta in PCAP) / playbackSpeed`.

Important distinction:

- **PCAP timestamps** (`ObservedAtUtc`, `occurredAtUtc` in emitted events) always come from the capture file, regardless of playback speed.
- **Playback speed** only affects *when* the probe publishes relative to wall clock, not the event payload timestamps.

### Choosing a speed

- **Need data in Kafka/Postgres fast?** → `DeterministicPlaybackSpeed=0`
- **Need to watch the system behave over time (UI refresh, lag, dedup windows)?** → `1` or `10`
- **Long scenario, limited patience?** → `10` or `20`, not `0`, if you still want some temporal separation between consumer batches

With default session/device deduplication windows (10 minutes), a burst at speed `0` may suppress fewer duplicate `SessionDetected` emissions than the same PCAP at speed `1`, because identical session fingerprints arrive within a shorter wall-clock span. Both runs still carry the same PCAP-derived timestamps inside the events.

## Quick reference commands

```bash
# Build all scenario PCAPs
./tools/traffic/run.sh build-all-pcaps
./tools/traffic/run.sh validate-pcaps

# Fast deterministic run (burst to Kafka)
docker compose -f docker-compose.probe.yml run --rm --build \
  -v "$(pwd)/tools/traffic/pcaps/generated:/pcaps:ro" \
  -e Probe__InputMode=DeterministicTest \
  -e Probe__DeterministicTestPcapPath=/pcaps/five-minute-mac-coverage.pcap \
  -e Probe__DeterministicPlaybackSpeed=0 \
  -e Probe__KafkaBootstrapServers=127.0.0.1:9092 \
  probe

# Real-time pacing (~5 min for five-minute-mac-coverage)
# Same as above with Probe__DeterministicPlaybackSpeed=1
# Remember to docker stop the container when logs stop advancing.
```

## Troubleshooting and operational notes

### Rebuild the probe image after code changes

Always pass `--build` on `docker compose run` when probe behaviour or options changed. An old image may still run `Live` capture on `eth0` even if env vars request `DeterministicTest`.

### The probe does not exit automatically after the PCAP ends

`DeterministicTest` finishes reading the file, but the .NET host keeps running. With `docker compose run --rm`, the container stays up until you stop it (`Ctrl+C` or `docker stop`). `--rm` only removes the container **after** the process exits.

### WSL Kafka bootstrap

On WSL, `Probe__KafkaBootstrapServers=localhost:9092,9093,9094` can fail on IPv6 (`[::1]`) for brokers 2 and 3. Prefer `127.0.0.1:9092`.

### `--rm` vs `up`

| Command | Role |
|---------|------|
| `docker compose -f docker-compose.probe.yml up --build` | Long-running probe in **`Live`** mode |
| `docker compose -f docker-compose.probe.yml run --rm --build …` | One-shot **`DeterministicTest`** job against a mounted PCAP |

### What gets validated end-to-end today

- **Devices:** probe → `devices.detected` → integration-console → Postgres ✅
- **Sessions:** probe → `sessions.detected` → Elasticsearch (Kafka Connect sink) ✅
- **Communication graph (Neo4j):** not wired in reference bootstrap yet; empty graph is expected even when devices and sessions flow. See `specs/007-device-communication-graph/`.

### Timestamps in generated PCAPs

Scenario-built PCAPs use synthetic `frame.time_epoch` values aligned to the YAML timeline (often starting near epoch 0). Event `occurredAtUtc` / `lastSeenUtc` reflect those observation times, not the wall clock when you run the probe.

### Malformed lines in some PCAPs

Non-IP frames (ARP-only, malformed tshark field rows) are skipped with warnings. Valid IP traffic still flows; warnings at the end of a scenario are normal for mixed captures.

## See also

- Probe configuration and Kafka validation: `specs/001-session-detection/quickstart.md`
- Traffic lab overview: `tools/traffic/README.md`
- Composer PCAP build: `tools/traffic/docs/composer-guide.md`
