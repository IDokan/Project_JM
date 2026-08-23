# Steam Cloud Save Data — Scope Document

Status: decided, not yet implemented. This defines what will and will not be
written to Steam Cloud storage, ahead of migrating `SaveDataManager` off
`PlayerPrefs` and onto an actual file.

## Decision

Only `SaveDataManager` data goes to Steam Cloud. `LeaderboardManager` data
(local top-6 cache, offline pending-score queue, leaderboard icon picks) is
**excluded**.

**Why:** Firebase Firestore is already the authoritative store for
leaderboard data — `SyncLocalLeaderboard()` fully rebuilds the local cache
from Firestore on every successful sign-in (effectively every online launch),
so the local copy is disposable by design. Backing it up to Steam Cloud would
mean shipping a second copy of state that's already designed to be thrown
away and re-derived. The one edge case where Cloud backup would help — a
score earned fully offline on one machine, read on another before the first
machine ever reconnects — was judged not worth the added file/sync
complexity.

## In scope: `SaveDataManager` (1 file — proposed `save.json`)

All values below currently live in Windows registry via `PlayerPrefs`
(`HKCU\Software\<company>\<product>`), which Steam Cloud cannot see regardless
of quota. Migrating this manager to a real JSON file on disk is the
prerequisite for any of this being cloud-syncable.

| Field(s) | Current PlayerPrefs key(s) | Type | Count | Notes |
|---|---|---|---|---|
| Master/BGM/SFX volume | `Audio_MasterVolume`, `Audio_BGMVolume`, `Audio_SFXVolume` | float | 3 | default 0.5 |
| Tutorial progress | `tutorialProgress` | int (`TutorialProgress` enum) | 1 | Easy=0 … Challenge=3 |
| Tutorial completed flags | `tutorialCompleted_{level}` | bool | 4 | one per `TutorialProgress` value |
| Enemy defeat counts | `enemyDefeatCount_{characterId}` | int | 8 | one per real `CharacterId` (excludes `Unassigned`) |
| Damage dealt | `damageDealt_{color}` | int | 4 | one per playable `GemColor` (Red/Green/Blue/Yellow) |
| Jewel match counts | `jewelMatchCount_{color}_{tier}` | int | 12 | 4 colors × 3 `MatchTier` values (Three/Four/Five) |
| Jewel match counts (aggregate) | `jewelMatchCount_None` | int | 1 | all-colors total, tier omitted |
| Max combo | `maxComboRecorded` | int | 1 | monotonic max, local only |
| Best score | `bestScoreLifetime` | int | 1 | monotonic max, local only |

**Total: 35 scalar values, ~1–1.5 KB serialized as JSON.** This will not grow
over time — every field is a fixed key or a bounded enumeration, no
open-ended lists.

## Out of scope: `LeaderboardManager` / `LeaderboardIconPrefs`

Stays on `PlayerPrefs` exactly as-is, untouched by this migration:

- `Leaderboard_LocalTop6` — Firestore-derived cache, rebuilt every online launch
- `Leaderboard_PendingEntries` — offline queue, flushed to Firestore on next successful sign-in
- `LeaderboardIconId_0/1/2` — cosmetic icon picks tied to the leaderboard feature

## Steamworks Cloud page settings

| Setting | Value | Rationale |
|---|---|---|
| Bytes quota per user | 1,048,576 (1 MB) | ~700x the actual ~1.5 KB payload; cheap headroom for future save fields without a second Steamworks-page edit |
| Files per user | 2 | 1 for `save.json` + 1 spare for an atomic write pattern (write `.tmp`, rename over the old file) so a mid-sync crash can't corrupt Cloud data |

## Follow-up (not yet started)

- Replace `SaveDataManager`'s `PlayerPrefs` calls with reads/writes to a JSON
  file at a Steam Cloud-visible path.
- Decide: Auto-Cloud (Steamworks page watches a folder pattern, no API calls)
  vs explicit `ISteamRemoteStorage` calls (already vendored via
  Steamworks.NET, `isteamremotestorage.cs`) for finer control over sync
  timing/conflicts.
