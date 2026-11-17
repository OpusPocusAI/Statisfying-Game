# Exploration: collecting glass from broken bottles

Status: **exploration only, not planned for implementation.** Written November 16 at the owner's request, after the
November 16 playtest. Nothing in the game has been changed for this.

## The idea

Today the shore holds ready-made sea glass: you dig a mound up (or find it lying on the sand), pick it up, and later
wash it at the basin and set it in a mosaic. The idea: the shore holds **broken bottles**, a small hazard that someone
has to take care of. Clearing one up safely is how you get glass.

This matches how sea glass forms in real life. It *is* broken bottles, tumbled smooth by the sea over decades. The
game would show that whole story in one loop: a broken bottle on the beach, gathered with care, tumbled in the
studio until it is frosted, washed, then set in a window. It also suits the cosy "a little care" tone, because
you are tidying a danger away as well as treasure hunting.

## How the current system works (what an idea has to fit)

- **One find is one record.** Each `ShoreItem` has an `id` (`glass-…` or `rubbish-…`), and `Progress.items` holds one
  `ItemRecord` per id. The record moves through
  `ItemStage { Buried, Shore, Bucket, Truck, Studio, Clean, Sorted, Placed, Recycled }`.
- **Every glass piece already has its place in a mosaic.** `GameCatalog.glass` has 30 `GlassDefinition`s, and each
  one has a fixed `panel`, `socket` and outline `mesh`. The mosaic only works because piece *n* fits hole *n*.
- **Capacity counts records.** The bucket holds 7 (14 with the upgrade) and the truck 22 (40).
- **Saves are strict.** `TryLoad` rejects any version other than 1, and any id that isn't in the scene.
- **The glass shader already has the material states needed:** `_Grime` (the salt crust) and `_Smoothness`. Clean,
  sharp, freshly broken glass is mostly smoothness turned up and grime at zero.
- **Interactions follow one pattern:** `Interactable.Use(delta)` for hold-to-work (digging) and `Interact()` for E.
  A new find type can follow the same shape.

## Three ways it could work

### A. A bottle is a find that yields several pieces (smallest change)

A broken bottle lies on the sand where a cluster of glass finds is today. Gathering it (holding LMB with gloves or
tongs, the same shape as digging) puts **one bottle record** in the bucket. At the studio, a new **tumbler** turns the
bottle's shards into its pre-assigned glass pieces, which then go through the existing wash, sort and mosaic flow
unchanged.

- **Data:** a new `BottleDefinition { id, color, string[] yields }`. `yields` lists the existing glass ids that the
  bottle becomes, for example three emerald pieces for an emerald bottle. Each glass id appears in exactly one bottle.
- **Stages:** the yielded glass records start in a new state (`Inside`, added at the *end* of `ItemStage` so old
  numbers keep their meaning), and the tumbler moves them to `Studio`. The bottle record itself uses
  `Buried`/`Shore`/`Bucket`/`Truck` like any find, then a new `Tumbled` stage at the end.
- **Mosaic:** untouched. Pieces still map 1:1 to sockets.
- **Capacity:** a bottle counts as one thing in the bucket. That is generous; it could count as its piece count
  instead.

### B. Bottles and loose sea glass together

The same as A, but only some of the glass comes from bottles. The rest is still found already tumbled, with the
"ancient" bottles worn down by the sea. That keeps the satisfying moment of digging up a frosted piece, and makes
bottles a special find with their own little job, perhaps appearing more in the later coves. It is the same code as
A with fewer bottles.

### C. Shards are picked up one by one (most hands-on, most work)

The bottle breaks into 4–8 visible shards scattered around a base and a neck. The player gathers each one, so a
broken bottle is a little area-clearing task. It feels the most "taken care of", but every shard is a pickup.
Either the shard count has to match the glass ids (making the mosaic much larger) or most shards become a new kind
of material that doesn't go to the mosaic. That needs real design decisions about what extra shards are *for*: a
cap reward, a "cullet" currency for a glass-melting upgrade, and so on.

**Recommendation if this is ever built: A (as B).** It reuses almost everything, keeps the mosaic and its tests
unchanged, and puts the new work into two clear pieces: a bottle find and a tumbler station.

## The pieces in more detail

### The bottle on the shore

- **Model:** a fused bottle base and neck lying in the sand with a few shards around it, in the bottle's colour,
  built in Blender as one fused piece like the truck and props (no primitive assemblies). One "broken" mesh per
  shape is enough; nothing needs to break at runtime. If a breaking moment is ever wanted, Blender's Cell Fracture
  can pre-split a bottle into pieces offline, and the game simply swaps the meshes.
- **Look:** fresh glass catches light, unlike frosted sea glass. That makes bottles readable at a distance (sharp
  highlights, clear colour) and a natural "something's wrong here" signal on a calm beach. It uses the same glass
  shader with high `_Smoothness`, no `_Grime` and a little sand at the base.
- **Taking care of it:** "Hold LMB · gather the broken bottle carefully". A slower hold than digging, with a
  glove/tongs hand pose in place of the scoop, and a soft clink loop where digging has its sand loop. Optionally a
  slightly tense "careful" rhythm: release, and progress holds, it never resets. **No injury or fail state**, to
  stay cosy. Gloves could be an early upgrade that speeds it up, like the scoop does for digging.
- **Hint system:** `F` (highlight remaining glass) would highlight bottles too.

### The tumbler (new studio station)

- A small rotating drum on the workbench: `StationKind.Tumble` in `StudioStation`, or its own small `Interactable`.
  Load bottles with E; it turns.
- **Time:** the most interesting choice. Tumbling could finish instantly with an animation, take a minute or two of
  real time, or finish "while you were out" (done when you come back from the next beach trip). The last is the
  most cosy and gives the trip a reason, but it means a wait before the very first mosaic piece. That needs care
  in the tutorial (for example the first bottle tumbles instantly).
- **Output:** pieces drop out of the drum as the existing frosted pebbles (`HeldMesh`) with a full salt crust, and
  the wash basin takes it from there. The drum's sound and a little sand-and-water spill sell the transformation.
- **Visual change, sharp to frosted:** the sharp shard and the frosted pebble already exist as separate meshes for
  each piece (the basin mesh is crisper, `pebbleMesh` is tumbled). The tumbler can show the change by swapping from
  a crisper mesh to the pebble while `_Smoothness` falls and a frost term rises. No new shader is needed.

### Truck, road and spills

- Bottles ride in the glass compartment. `RoadHazards` can spill cargo onto the road; a spilled bottle would become
  a small "go back and clear it" task on the road, which fits the theme. Or bottles could be exempt (padded crate).
- Optional upgrade: "padded crate", where bottles never spill.

## What would change (rough size)

| Area | Change | Size |
|---|---|---|
| `GameCatalog.cs` | `BottleDefinition[] bottles`; new stages added at the end of `ItemStage` | small |
| `Progress` / `GameSession` save | `version` 2, a migration from 1 (existing glass records keep their stage), load check knows bottle ids | small, but must be careful |
| Shore find | `ShoreItem` gains a bottle kind, or a `BottleFind : Interactable` with the same Buried/Shore/Bucket handling | medium |
| Studio | tumbler station, drum model, animation, sound | medium |
| Art | 6 broken-bottle models (one per colour family, varied shapes), gloves/tongs hand pose, drum | medium–large (Tchia bar) |
| Scene pass | an editor pass replaces chosen glass clusters with bottles and writes `yields` | small |
| Tutorial and HUD | new steps ("gather the bottle", "tumble it"), prompts, counts that show bottles | small–medium |
| Tests and verification | the PlayMode tests and `--verify-seaglass` walk the glass flow from the shore; they need a bottle-and-tumbler path | medium |

Roughly two to three focused rounds for A/B including art, most of it the bottle models and the tumbler station.

## Risks and things to decide

1. **Replace or add?** All glass from bottles (A), or bottles alongside sea glass (B)? B keeps the current loop's
   best moment.
2. **Waiting.** Real-time tumbling is the most atmospheric and the most likely to frustrate. It needs a decision
   and probably a test with the owner.
3. **Clarity**, which the November 14 playtest ranked above art. The loop gets one more step. It has to be obvious
   that a bottle is *glass you haven't made yet*, not rubbish. A good test: does a new player put bottles in the
   recycling press? (It could even accept them, at a worse reward, as a gentle lesson.)
4. **Safety tone.** Broken glass on a beach is a real hazard, and the game should treat clearing it as good care,
   not scare the player. No blood, no damage.
5. **Saves.** The current strict loader would reject old saves after any id or stage change unless a migration is
   written. This is easy to get wrong and should be covered by a test that loads a version-1 save.

## A cheap way to try it first

Before building anything permanent: in one cove, swap three existing glass finds for one placeholder bottle with
`yields` = those three ids, and let the existing wash basin also act as the tumbler (an instant convert on E). That
is a few hours' work, tests whether gathering bottles *feels* better than digging up glass, and can be thrown away.
