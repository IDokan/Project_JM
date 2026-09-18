# Localization

Player-facing wording belongs in Unity Localization string tables. Keep gameplay
symbol-first; explanatory wording appears only through contextual help.

## Edit existing wording

Open **Window > Asset Management > Localization Tables** and select **Combat Help**.
Edit the English (`en`) column. The collection is stored under
`Project_JM/Assets/Localization/Combat Help/`.

The current entries use these prefixes, each with `.title` and `.description`:

- `combat.party_health`
- `combat.enemy_health`
- `combat.enemy_attack`
- `combat.combo`
- `reward.power_up_<color>`
- `reward.sharp_attack_<color>`
- `reward.berserked`
- `reward.blessings`
- `reward.focus`
- `reward.fortify`

TooltipTrigger holds LocalizedString references using the collection GUID and entry
ID. Editing wording or renaming a key therefore does not require rewiring prefabs.
Do not delete and recreate an entry merely to rename it.

## Add help or a language

1. Add the title and description entries to the appropriate table. Use stable,
   meaning-based keys. Add a separate collection for a new area when needed.
2. Assign the entries to Localized Title and Localized Description on the nested
   TooltipTrigger in the deepest source prefab that owns that help topic.
3. To translate, add a Locale in Localization Settings and its language table to
   the collection. English is the project locale. No translated language ships yet.
4. Check glyph coverage, longer wording, wrapping, and placement in the game before
   shipping that locale. Fonts and right-to-left support need language-specific QA.

Use a complete Smart String with named parameters when a sentence contains changing
values; do not concatenate translated sentence fragments. Numeric HUD values and
generated resolution options remain data, not translation entries.

## Runtime and build behavior

TooltipTrigger subscribes while enabled, waits for both strings from the selected
locale before showing, and refreshes a visible tooltip when localization changes.
Dismissed tooltips stay dismissed. Each trigger uses a serialized reference to the
TooltipPresenter in its owning composition prefab. The presenter rebuilds its
adaptive layout before positioning, and a content refresh does not restart the
show animation.

RewardDefinition assets own their localized tooltip references. RewardOfferUI
passes the rolled definition's content to the matching reward-button trigger, so
reusing a button slot does not require reward-specific UI conditionals.

Combat Help is preloaded. Localization Settings and the generated Addressables
configuration under `Assets/AddressableAssetsData` are required project assets;
include them with the locale and table assets in version control. Verify the
Addressables content build when making a player build.

The reusable tooltip and dropdown prefabs have empty runtime text placeholders.
Their presenters populate them; do not add translation bindings to numeric displays
or dropdown template labels that are overwritten at runtime.

## Verification performed

- Verified the four source-prefab references and their inherited combat-scene values.
- Resolved English content in an isolated Play Mode scene.
- Switched a visible tooltip to a temporary pseudo locale with expanded text.
- Confirmed title and description do not overflow after the layout migration.
- Checked dismissal during locale changes and disable/re-enable behavior.
- Rendered the tooltip for visual inspection.

Temporary verification files are kept under ignored TestArtifacts directories.
No full player build or real translated-language QA was performed.
