# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Project_JM is a 2D tactical puzzle-RPG built in **Unity 6 (v6000.2.9f1)** using C# and URP. Players match gems on an 8x8 board to trigger character attacks against enemies in turn-based combat.

## Design Goals

**Text-less game** — one of the primary design goals is that the game contains no written language of any kind. No English, Japanese, Chinese, Korean, or any other natural language text should appear in the in-game UI, HUD, or gameplay elements. Numbers are acceptable (HP values, damage numbers, timers, etc.), but words and sentences are not. When implementing UI or gameplay features, always represent information through icons, symbols, color, animation, and numbers instead of text labels or descriptions.

## Tech Stack

- **Engine:** Unity 6000.2.9f1
- **Rendering:** Universal Render Pipeline (URP) v17.2.0
- **Input:** Unity Input System v1.14.2
- **2D:** com.unity.2d.animation 12.0.3, com.unity.2d.aseprite 2.0.2, com.unity.2d.psdimporter 11.0.2
- **Timeline:** com.unity.timeline 1.8.9
- **Tweening:** DOTween (Assets/Plugins/Demigiant/DOTween/)
- **IDE:** Rider / Visual Studio (both supported via package)

## How to Run

- Open the project in Unity Hub with Unity version **6000.2.9f1**
- Main scene: `Assets/Scenes/SampleScene`
- Press Play in the Editor to run; no build scripts or CLI commands are needed

## Folder Structure

```
Assets/
├── Scripts/
│   ├── Action/
│   │   ├── AssetScripts/       # ScriptableObject base classes (AttackLogic, EventChannels, data)
│   │   ├── AttackLogic/        # Concrete attack implementations per character class
│   │   ├── BoardDisableLogic/  # Tile-disable implementations
│   │   ├── Characters/         # CharacterCombatant, AttackExecutor, EnemyAttackBehaviour, etc.
│   │   └── Enums/              # Match-related enums
│   ├── Puzzle/
│   │   ├── Gems/               # Gem, GemMover, GemResolver, GemShake, GemSelectionHighlight
│   │   └── Enums/              # GemColor enum + utilities
│   ├── Systems/                # Core managers (BoardManager, CombatManager, EnemySpawner, etc.)
│   │   └── Transitions/        # Transition controller scripts
│   ├── UI/
│   │   ├── BarBinders/         # HP/status bar UI bindings
│   │   ├── DamageUI/           # Floating damage number display
│   │   └── Menu/               # Pause/options menu scripts
│   ├── PlayerController/       # Mouse & gamepad input
│   ├── Camera/                 # CameraMover, CameraShake
│   ├── Backgrounds/            # Parallax scrolling
│   ├── VFXs/                   # Per-character VFX scripts (Bowman/, Cleric/, Knight/, Mage/, Enemies/)
│   ├── Tags/                   # Lightweight tag marker components (AllyTag, EnemyTag)
│   ├── GlobalRNG.cs
│   └── GlobalTimeManager.cs
├── AttackAssets/               # Serialized ScriptableObject asset instances
│   ├── AttackLogics/           # Per-character attack assets
│   ├── BoardDisableLogics/
│   ├── Data/                   # AttackBook, EnemyBook
│   └── EventChannels/          # All event channel assets
├── Prefabs/
│   ├── Actions/                # Party character prefabs
│   ├── GemBoard/               # Board + gem prefabs
│   ├── System/                 # Manager prefabs
│   ├── UI/
│   └── VFXs/
├── Resources/
│   ├── CharacterStatus/
│   └── Designs/
└── Scenes/
    └── SampleScene/
```

## File Naming

- **C# scripts:** PascalCase universally
- **Suffixes:**
  - `Manager` — system orchestrators (BoardManager, CombatManager)
  - `Logic` — behavior/attack ScriptableObject base or implementation (AttackLogic, BoardDisableLogic)
  - `Channel` — event channel ScriptableObjects (MatchEventChannel)
  - `Data` — data-container ScriptableObjects (CharacterStatusData)
  - `Book` — collection assets (AttackBook, EnemyBook)
  - `Binder` — UI-to-data bindings (BarStatusBinder)
  - `Executor` — runtime execution classes (AttackExecutor)
  - `Resolver` — finalization/resolution classes (GemResolver)
- **ScriptableObject assets:** PascalCase, prefixed with owner when character-specific (e.g. `BowmanCritChanceBuffAttack.asset`)
- **Prefabs:** PascalCase matching their primary script or character name

## Code Style

**Fields & properties**
- `[SerializeField]` fields are `private` or `protected`, camelCase, no underscore: `[SerializeField] protected int rows = 8;`
- All other `private`/`protected` fields use underscore prefix: `private int _currentIndex;`
- Public read access is exposed via expression-body properties: `public int Rows => rows;`
- Avoid `public` fields; only use them when the value must be accessible from outside the script

**Naming**
- Classes, methods, properties: PascalCase
- `[SerializeField]` fields: camelCase (no underscore)
- Other private/protected fields: `_camelCase`
- Local variables: camelCase
- Constants: PascalCase (matching C# convention)
- All `public` methods must be called from outside the declaring script at least once

**File header** — every `.cs` file begins with:
```csharp
// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) [DD/MM/YYYY] Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FileName.cs
// Summary: One-line description (can be longer than one line if needed)
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.
```

**Event subscription pattern** — always subscribe in `OnEnable`, unsubscribe in `OnDisable`:
```csharp
protected void OnEnable()  => channel.OnRaised += Handler;
protected void OnDisable() => channel.OnRaised -= Handler;
```

**Data passing** — use structs for inter-system parameter bundles (`AttackContext`, `MatchEvent`, `BoardDisableContext`); prefer structs over classes for immutable data.

**Coroutines** — only store a `Coroutine` reference in a field if there is an explicit reason (e.g. needing to stop it early). When stored, null-check before stopping:
```csharp
if (_routine != null) { StopCoroutine(_routine); }
_routine = StartCoroutine(MyRoutine());
```

**Namespaces** — only enum/utility files define namespaces (`GemEnums`, `MatchEnums`); MonoBehaviour/ScriptableObject classes do not use namespaces.

## ScriptableObject Rules

- All `ScriptableObject` subclasses must include a `[CreateAssetMenu]` attribute.
- Menu path follows the pattern: `JM/<Category>/<SubCategory>/<Name>` (e.g. `JM/Combat/AttackLogic/Ally/Crit Chance Buff Attack`, `JM/Events/Match Event Channel`).
- Attack-related asset instances live under `Assets/AttackAssets/`, organized by type then by character class. ScriptableObject assets unrelated to attacks must still live somewhere under `Assets/`.
- New attack logic classes extend `AttackLogic` (abstract ScriptableObject) and implement `Execute(AttackContext)` as a coroutine and `GetTargetMotionOffset()`.

## Architecture

### Event-Driven Communication
The game uses **ScriptableObject event channels** for decoupled communication between systems. Key channels in `Assets/AttackAssets/EventChannels/`:
- `MatchEventChannel` — fired when 3+ gems match (carries color + tier)
- `GemPowerArrivedEventChannel` — gem power reaches a character
- `EnemyAttackEventChannel` — enemy performs an action
- `EnemySpawnedEventChannel` — enemy has registered itself to all systems (fired after instantiation, not at it)
- `CharacterDeathEventChannel` — character or enemy dies
- `TransitionEventChannel` — drives scene transitions (CombatIntro, Middle, Defeated, etc.)
- `BoardDisableEventChannel` — marks board tiles as unavailable

### Core Systems (`Assets/Scripts/Systems/`)
| Class | Role |
|---|---|
| `BoardManager` | 8x8 gem grid: swap, fall, match detection, hints |
| `CombatManager` | Orchestrates combat flow, listens to combat-relevant event channels |
| `EnemySpawner` | Handles enemy instantiation and system registration (independently — registration may happen after instantiation); sets enemy HP from progress data right after instantiation |
| `GameProgressManager` | Tracks enemy defeats, game state, difficulty scaling; controls party character HP scaling over progress |
| `GlobalTimeManager` | Global time scale, pause, transition gating |
| `PauseManager` | Pause menu logic |
| `TransitionManager` | Scene transition sequencing |
| `ComboManager` | Tracks consecutive matches |
| `DamageMultiplierManager` | Manages damage amounts for both allies and enemies — tracks active damage buffs and scales base damage values over game progress; consulted in `CharacterCombatant.TakeDamage()` |

### Gem Board → Combat Data Flow
1. Player drags gem → `PlayerController` (mouse/gamepad input via Input System)
2. `BoardManager` detects match → raises `MatchEventChannel(color, tier)`
3. `CombatManager` receives event → queries `PartyRoster` for the matching character
4. `AttackExecutor` runs the character's `AttackLogic` ScriptableObject
5. `CharacterCombatant.TakeDamage()` applies modifiers: color advantage (×1.2/×0.8), crits, `DamageMultiplierManager` value and buff
6. `DamageUIManager` spawns floating numbers; death fires `CharacterDeathEventChannel`

### Character Classes (`Assets/Scripts/Action/AttackLogic/`)
Four color-coded party members, each with a ScriptableObject-based `AttackLogic`:
- **Knight (Red)** — Shield, Stun
- **Mage (Blue)** — Delay, TimeStop
- **Cleric (Green)** — Heal, Damage buff
- **Bowman (Yellow)** — Crit chance buff, Crit damage buff

Match tiers (3/4/5+ gems) scale attack power.

### Key Patterns
- **Singletons** — only `GlobalRNG` and `GlobalTimeManager` are built on the singleton pattern; all other managers exist as a single unique instance in the scene by convention
- **`GlobalRNG`** — custom deterministic RNG singleton; use only for board gem-related logic (e.g. spawning new gems, determining which gem slot gets disabled)
- **`GlobalTimeAnimatorBinder`** — synchronizes Animators to `GlobalTimeManager`'s time scale; attach to any animated game object that must pause with the game

## Branching

- Main branch: `main`
- Feature branches: descriptive kebab-case (e.g. `Gem-board-mechs`)
- Commit messages follow Conventional Commits — see `## Commit Convention`

## Commit Convention

Follow **Conventional Commits 1.0.0** strictly.

### Commit Subject
- Format: `#<issue number> - <type>(<scope>): <summary>`
- Types allowed: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`, `ci`, `build`, `revert`
- Max 72 characters
- Lowercase only
- Imperative mood ("add" not "added", "fix" not "fixed")
- No period at the end
- Scope is optional but encouraged (e.g. `feat(auth):`, `fix(api):`)

### Commit Body
- Separate from subject with one blank line
- Explain *what* and *why*, never *how*
- Wrap at 72 characters per line
- Required for any non-trivial change
- List each changed file and what changed: `FileName.cs - Description` (e.g. `DamageUIManager.cs - Add header comment`)
- Separate each file entry with one blank line

### Commit Footer
- Reference issues: `Closes #123`, `Fixes #456`
- Note breaking changes: `BREAKING CHANGE: <description>`

### Example Bad ❌
- `"fixed stuff"`
- `"update"`
- `"WIP"`
- `"auth changes"`

### Never
- Never use vague messages: "fix", "update", "changes", "WIP", "misc"
- Never skip the body for non-trivial commits
- Never commit multiple unrelated changes in one commit
- Never exceed 72 characters on the subject line

### Atomic Commits
Each commit must represent exactly ONE logical change.
If you find yourself writing "and" in the subject, split it into two commits.

## Never

- Never read or write large verbose files such as `.prefab` or `.scene` — ask the user first if you think you need to perform actions on these files
- Never add underscore prefixes to `[SerializeField]` fields — they must be plain camelCase
- Never use `public` fields unless the value genuinely needs to be accessed from outside the script
- Never add a `public` method that isn't called from outside the declaring script
- Never use `UnityEngine.Random` for board gem logic — use `GlobalRNG` instead
- Never subscribe to an event channel without a matching unsubscription in `OnDisable`
- Never bypass `GlobalTimeManager` for time-sensitive code (use `GlobalTimeManager.DeltaTime` / `GlobalTimeManager.Time`)

## Test Rules (Unity Test Framework)

**Location:** `Assets/Tests/EditMode/` with a single `.asmdef` referencing `Assembly-CSharp`.

**File naming:** `<ClassUnderTest>Tests.cs` (e.g. `GlobalRNGTests.cs`, `BoardManagerTests.cs`)

**Test method naming:** `MethodName_Condition_ExpectedResult` (e.g. `NextInt_SameSeed_ReturnsSameSequence`)

**What qualifies for EditMode tests:** no `MonoBehaviour` lifecycle, no `Instantiate`, no `GetComponent`, no coroutines, no `[SerializeField]` wiring required to run. Current candidates: `GlobalRNG`, damage calculation math in `CharacterCombatant.TakeDamage()`, `FindMatchGroups()` in `BoardManager`, and `DamageMultiplierManager` scaling formulas.

**Proactive testing:** When modifying a class that already has a test file, check coverage and add tests for any changed logic. When writing new pure-logic code that meets the EditMode criteria above, add tests without being asked.

## Communication

- If unsure about Unity-specific behavior, say so.
- Provide enough information and explanation for non-code changes, such as modifications to `SampleScene.unity` or `*.meta` files.
