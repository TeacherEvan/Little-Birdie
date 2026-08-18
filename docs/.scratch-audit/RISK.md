# Risk Register

| ID | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| RSK-001 | Custom Convex URL points at an unreachable/invalid deployment | MEDIUM | LOW | PlacesService/WeatherService fall back to mock data when ConvexClient is null or the query throws; the Ping button surfaces reachability. No crash path. |
| RSK-002 | Geolocation prompt denied by user/OS | HIGH | LOW | LocationPromptModal + LocationPage fall back to manual city selection or show "unavailable"; location is non-critical. |
| RSK-003 | Service-worker update unsupported (no registration / unsupported browser) | LOW | LOW | `mathilda.sw.update` returns `{success:false,reason}` instead of throwing; UI shows a failure message, no swallowed exception. |
| RSK-004 | Stale `localStorage` schema from an older AppSettings (with removed fields) on an existing user's device | LOW | LOW | `LocalStore.LoadAsync` deserializes with `JsonSerializer` which ignores unknown properties and uses defaults for missing ones; removed fields are simply dropped on next save. |
| RSK-005 | Prefecture of dropped settings controls (Language/Theme/Currency/UnitSystem) leaves users wanting localization | LOW | MEDIUM | Documented decision (OBJ-08): strip dead controls rather than ship false affordances; revisit only when a real consumer lands. No behavior regression. |

## Risk scoring
LOW / MEDIUM / HIGH / CRITICAL
All HIGH/CRITICAL risks require explicit disposition. None present; MEDIUM risks (RSK-001, RSK-005)
are accepted with the mitigations above.
