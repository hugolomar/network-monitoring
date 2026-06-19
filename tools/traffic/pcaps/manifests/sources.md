# PCAP Sources Manifest

This file tracks origin and integrity of captures used in `tools/traffic/pcaps/external`.

## Current captures

| File | Purpose | Suggested scenario | SHA256 |
| --- | --- | --- | --- |
| `http.cap` | Baseline TCP/HTTP replay | `one-hour-realistic` (`baseline-http`) | `25a72bdf10339f2c29916920c8b9501d294923108de8f29b19aba7cc001ab60d` |
| `dns.cap` | DNS query/response replay | custom replay | `041eeb6f98bb398f1ee8b09651b5b5a84f6a62639f95bf226f9e7b77355d9f28` |
| `dhcp.pcap` | DHCP noise / instability replay | `one-hour-realistic` (`dhcp-periodic-noise`) | `2471b5420bdac826eecf8f61a2bbb4a3eb20dbfab7c02ff2be502f349f368214` |
| `SkypeIRC.cap` | Mixed TCP traffic burst replay | `one-hour-realistic` (`early-bursts`, `late-burst-window`) | `bac79a9c3413637f871193589d848697af895b7f2700d949022224d59aa6830f` |

## Provenance notes

- Source website/repository:
- Download date:
- License/use constraints:
- Operator notes:
