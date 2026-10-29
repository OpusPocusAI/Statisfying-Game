# Sea Glass Studio art direction

This document defines what Sea Glass Studio should look like and how to judge whether an area is finished. It replaces the art bar in [the development prompt](DEVELOPMENT_PROMPT.md), which only asked for "cohesive stylized placeholders". The palette and mood from that prompt still apply; this document makes them concrete.

Read this before any visual work. Every art pass should name the area it targets, the reference images it is matching, and the quality-bar items it expects to meet.

## 1. The look in one paragraph

A warm, late-afternoon coast painted in soft gradients. Simple, deliberately shaped low-poly forms, lit so that every surface has a light side, a warm bounce and a cool purple shadow. Turquoise water against cream and peach sand. Distance dissolves into peach haze. Everything the player touches (tools, glass, trays, the truck) is a designed object with a readable silhouette, not a primitive. Sea glass is the jewel of every frame: soft, rounded, frosted when dirty, luminous when clean.

**The feeling:** a quiet holiday afternoon spent tidying a beach and making something beautiful from what you found.

### What we are not

- Not realistic, gritty or photographic.
- Not heavy black outlines or cel-shaded comic style.
- Not "default Unity low-poly": flat single-colour primitives under neutral light.
- Not cluttered. Every prop earns its place in the composition.

## 2. Reference games

Collect screenshots from these games for the areas listed. Take each one for a specific lesson, not as a style to copy wholesale.

| Game | Developer | Take this | Ignore this |
| --- | --- | --- | --- |
| Firewatch (2016) | Campo Santo | Painterly atmosphere: colour gradients through fog, distance fading to a warm tint, strong golden-hour grading, graphic skies. The primary lighting reference. | Forest setting, realistic proportions. |
| Lonely Mountains: Downhill (2019) | Megagon Industries | Proof that flat-shaded low-poly can look premium: soft ambient occlusion, rich lighting, careful silhouettes, dense but readable ground detail. | Mountain terrain, speed. |
| Tchia (2023) | Awaceb | Tropical coast: turquoise shallows, shoreline foam, wet sand, lush stylized vegetation, warm Pacific light. The primary shoreline reference. | Open world scale, character systems. |
| A Short Hike (2019) | adamgryu | Compact world composition: landmarks you can see from everywhere, paths that lead the eye, small areas that feel big. | Pixelated render filter. |
| Tiny Glade (2024) | Pounce Light | Soft, warm lighting that makes simple shapes lovely; satisfying little details appearing as you build. | Procedural building tools. |
| PowerWash Simulator (2022) | FuturLab | Cleaning satisfaction: strong dirty-to-clean contrast, visible spray and runoff, a clear "done" moment per object. The primary cleaning reference. | Realistic grime, industrial settings. |
| Unpacking (2021) | Witch Beam | Tactile placement: every object detailed and recognisable, placement sounds and snaps, a room that fills up with meaning. The primary sorting/placement reference. | Pixel art. |
| Dorfromantik (2022) | Toukana Interactive | Cozy soft shading, gentle colour, small placed pieces that add up to a picture. Useful for the mosaic progress feel. | Top-down camera. |

Also collect **real-world photos**:

- Sea glass, both wet and dry, in hand and on sand. Note the rounded edges, frosted surface, and how light passes through.
- Stained-glass windows with visible lead came (the dark metal strips), backlit by sun.
- Golden-hour beaches with turquoise water, wet sand reflections and foam lines.
- Small seaside workshops: wooden benches, enamel basins, shelves of jars, warm window light.

## 3. Target images to collect

Store reference images in `ArtReference/` at the project root, one folder per area (for example `ArtReference/03-sea-glass/`). Screenshots of other games are for internal reference only: keep this folder out of Git and out of builds.

Collect at least the following. For each image, add a one-line note in a `notes.md` in that folder saying what it shows and what to take from it.

| Folder | Collect | What to look for |
| --- | --- | --- |
| `01-lighting` | 4 Firewatch golden-hour vistas; 2 Lonely Mountains: Downhill wide shots; 2 Tiny Glade evening scenes | Shadow colour, fog colour by distance, sky gradient, how bright the light side is versus the shadow side |
| `02-shoreline` | 4 Tchia coast shots (shallows, foam, wet sand); 3 real golden-hour beach photos | Colour bands from deep water to shallows to wet sand to dry sand; foam shape; how the beach edge curves |
| `03-sea-glass` | 6+ real sea-glass photos (dry and wet, each colour); 2 macro shots of frosted glass | Rounded pebble shapes, frosting, thickness, edge glow, colour saturation when wet |
| `04-cleaning` | 3 PowerWash Simulator before/mid/after shots; real photos of washing glass in a basin | Contrast between dirty and clean, spray, foam, runoff |
| `05-sorting` | 3 Unpacking shelf/drawer shots; real photos of sorted sea glass in trays or jars | Trays that look full and inviting, objects that read at a glance |
| `06-mosaic` | 4 real stained-glass windows with lead came, backlit; 1 coloured light pattern on a floor | Lead lines, glass texture and colour variation within a piece, light spill |
| `07-studio` | 3 Tiny Glade or Firewatch interiors; 3 real seaside workshop photos | Warm window light, clutter that tells a story, materials (wood grain, enamel, linen) |
| `08-props-tools` | 3 Lonely Mountains: Downhill / A Short Hike prop close-ups; real toy shovels, buckets, sieves, brushes | Silhouettes, bevels, proportions, how few polygons still read as designed |
| `09-truck-road` | 2 stylized vehicles from cozy games; real photos of small beach utility vehicles | Rounded bodywork, wear, colour blocking |
| `10-ui` | 3 HUD/menu screenshots from cozy games (A Short Hike, Unpacking, Dorfromantik) | Icon style, panel shapes, text size and contrast |
| `11-composition` | 4 A Short Hike or Firewatch shots where a landmark leads the eye | Foreground/midground/background layers, focal points |

Optional but very useful: one or two paint-overs of current Sea Glass Studio captures (from the review set in section 7) showing the target look. A paint-over over the real game view is the most direct target an art pass can have.

## 4. Global visual rules

### Palette

Based on the development prompt's palette. Hex values are starting targets; adjust in context, but keep the relationships.

| Role | Target | Notes |
| --- | --- | --- |
| Sky top | `#F2B48A` peach | Gradient to pale yellow `#FBE3B0` at the horizon |
| Sun and highlights | `#FFE9B8` warm cream | Never pure white |
| Deep water | `#0E6E73` deep teal | |
| Shallow water | `#3FD2C7` turquoise | Brightest saturated colour outside the glass |
| Dry sand | `#EED3A2` warm cream | Variation ±5% value, never one flat colour |
| Wet sand | `#C9A27A` muted orange-brown | Slight sheen |
| Wood | `#B4552E` burnt orange | |
| Shadows | `#4A3558` dark purple | Shadows are tinted purple, never neutral grey |
| Vegetation | `#2F5D57` deep blue-green | |
| Distance haze | `#E9B9A0` peach-pink | Colour that far objects fade toward |
| Glass | aqua, emerald, cobalt, amber, clear white, purple, rare red | The most saturated colours in the game; nothing else should compete |

### Lighting and post-processing

- One warm directional sun, low angle (about 20–30°), casting long shadows.
- Shadow colour tinted toward purple through ambient light and colour grading; no neutral grey shadows anywhere.
- Ambient occlusion visible where objects meet the ground and in corners.
- Distance fog with a colour gradient (warm near the sun, cooler away from it). Far hills should lose detail and shift toward the haze colour.
- Colour grading: warm highlights, slightly cool shadows, gentle contrast. The current hazy, low-contrast look is not the target.
- Bloom only on the sun, glass and light sources. No glow on sand or walls.
- No smudged or blurry blobs in the sky. Clouds are shaped painterly forms or absent.

### Shapes and models

- Low-poly is fine; primitive is not. Every object has a designed silhouette: bevels on edges, tapered and curved forms, proportions slightly exaggerated (chunkier handles, rounder bodies).
- No object should be recognisable as a Unity cube, cylinder or flat quad.
- Objects the player looks at up close (tools, glass, stations, trays, the truck) get the most detail and polygon budget.
- Repeated props (dig piles, rocks, grass) need at least 3–4 variants and random rotation/scale so repetition is not visible in one screen.

### Materials

- Mostly matte, with gradients baked into vertex colour or textures: lighter on top, darker and warmer toward the base.
- Subtle hand-painted texture is allowed (soft noise, brush marks). Photographic texture is not.
- Every large surface (sand, floor, walls, road) needs low-frequency variation so it does not read as a flat colour.

## 5. Per-area quality bar

Each area has a current state, a V1 bar (all items must be true to call the area done), the main references, and what is never acceptable. Current state is based on the September 30 and October 24 captures in `Captures/Polish` and `Builds/Windows/Verification`.

Scoring: rate each area 1–5 against its bar after each pass. 1 = placeholder, 3 = coherent but plain, 5 = matches the references. V1 ships when every area is at least 4.

### 5.1 Lighting, sky and atmosphere (Oct 24: 2 → 3 → 4/5 after side-by-side round 3)

**Now:** washed-out peach haze over everything, low contrast, grey shadows. The sun and clouds are blurry smudges.

**V1 bar:**
- [ ] Sky grades from a cool blue-lilac zenith through a peach band to a pale yellow horizon (corrected after the side-by-side with Tchia: an all-peach sky flattened every view).
- [ ] Shadows are visibly purple-tinted in every review view.
- [ ] Light side versus shadow side of objects is clearly different (not just slightly darker).
- [ ] Far hills fade to the haze colour; near objects are crisp and saturated.
- [ ] Sun is a clean disc with soft bloom; clouds are shaped or removed.
- [ ] Ambient occlusion is visible where props meet sand and floor.

**References:** `01-lighting`. **Never:** neutral grey shadows, full-screen haze that flattens near objects.

### 5.2 Water and shoreline (Oct 24: 3 → 4/5, confirmed by side-by-side round 2)

**Now:** turquoise water with brush-mark ripples, broken shore crests and rock surf. The October 24 blend removed the hard seam. The coast is still a long straight strip.

**V1 bar:**
- [ ] Clear colour bands: deep teal → turquoise shallows → foam line → wet sand → dry sand.
- [ ] Foam line follows the shore with gentle animated movement.
- [ ] Coastline has visible curves, small coves and points; no straight edge longer than about 20 m on screen.
- [ ] Wet sand has a slight sheen that catches the sun.

**References:** `02-shoreline`. **Never:** visible seams between water and sand meshes, uniform water colour from shore to horizon.

### 5.3 Sand and terrain (Oct 24: 1 → 3 → 4/5 after side-by-side round 3)

**Now:** flat, single-colour sand covers most of the frame. Dig piles are repeated cone shapes. The inland rock wall is grey boxes.

**V1 bar:**
- [ ] Sand has low-frequency colour and height variation (dunes, ripples, darker damp patches) visible in every beach view.
- [ ] Small scattered details: shells, pebbles, footprints or tide lines, used sparingly.
- [ ] Dig piles have at least 4 shapes, read as soft mounds, and sit naturally in the sand.
- [ ] Cliffs and rock walls are sculpted shapes with top-lit gradients, not boxes.

**References:** `02-shoreline`, `08-props-tools`. **Never:** a single flat colour filling more than a third of the screen.

### 5.4 Beach props and vegetation (Oct 24: 2 → 3 → 4/5 after side-by-side round 3)

**Now:** parasols and benches have had a polish pass, but the bench is three logs and grass is sparse spikes. Trees are low-poly blobs with flat colour.

**V1 bar:**
- [ ] Every prop has bevels and a designed silhouette.
- [ ] Vegetation grows in clusters with colour variation from base to tip.
- [ ] Trees have a layered canopy with light/shadow gradient.
- [ ] Each cove has one clear landmark visible from its entrance.

**References:** `08-props-tools`, `11-composition`. **Never:** single primitives standing in for props.

### 5.5 Rubbish and collectibles (Oct 24: 2 → 4/5)

**Now:** bottles and cans read at close range but are small, flat-coloured and simple.

**V1 bar:**
- [ ] Each rubbish type is recognisable at 5 m in a first-person view.
- [ ] Rubbish shows light wear (faded labels, dents) without looking gritty.
- [ ] Buried items show a readable hint (a glint, a corner) before digging.
- [ ] Removing rubbish makes the beach visibly cleaner (the "polluted to clean" transformation from the design).

**References:** `08-props-tools`. **Never:** rubbish that looks like generic coloured capsules.

### 5.6 Sea glass pieces (Oct 24: 1 → 3 → 4/5 after side-by-side round 2) — highest priority

**Now:** flat polygon tiles with uniform colour. Cloudy glass reads as white plastic; clean glass reads as cyan acrylic.

**V1 bar:**
- [ ] Pieces are rounded, pebble-like shapes with soft edges and varied thickness, in about five general shapes.
- [ ] Cloudy glass: frosted, slightly translucent, with the colour faintly visible underneath the grime.
- [ ] Clean glass: saturated colour, light passes through, bright edge highlights, subtle inner glow.
- [ ] Glass reads as glass from 2 m away without a label.
- [ ] Rare red is visibly special.

**Fixed target (owner's decision, October 29):** clean glass is **glossy**. It has a sharp white glint, small flashing facets, and clear, saturated colour. A satin, softly frosted finish was rendered side by side with it and rejected. The piece at the wash bowl stays at its current size (about a third of the bowl's width), so the brush strokes stay readable. Reviews judge clean glass against this target. "Too glossy" or "too large" is not a gap; how well the glossy glass reads as glass is.

**References:** `03-sea-glass`. **Never:** flat slabs, uniform opaque colour, glass that looks like plastic.

### 5.7 Cleaning station (Oct 24: 1 → 2 → 4/5 after side-by-side round 2)

**Now:** a flat teal basin with ripple rings. The brush is a box and the tap is two boxes. Bubbles are plain white spheres. The grime fades in patches.

**V1 bar:**
- [ ] Basin, brush and tap are modelled objects (enamel basin with rim, bristled brush, shaped tap).
- [ ] Water looks like water: surface highlights, slight transparency, depth tint.
- [ ] Brushing produces foam, small splashes and drips that follow the brush.
- [ ] Grime comes off in strokes that follow the brush.
- [ ] Each piece has a clear "clean" moment (sparkle, colour bloom, sound).

**References:** `04-cleaning`. **Never:** white spheres as bubbles, primitive tools.

### 5.8 Sorting station (Oct 24: 1 → 4/5)

**Now:** dark empty tray slabs with one small coloured rectangle each. Labels are low-contrast teal on beige.

**V1 bar:**
- [ ] Trays look inviting: lighter lining, visible depth, sorted pieces piled with natural variation.
- [ ] Each tray's colour is readable from across the room (lining tint or a glass sample).
- [ ] Labels are high-contrast and legible at normal viewing distance.
- [ ] Trays visibly fill up over the game.

**References:** `05-sorting`. **Never:** dark empty slabs, unreadable labels.

### 5.9 Mosaic panels and reveal (Oct 24: 3 → 4/5 after side-by-side round 2)

**Now:** the best visual moment in the game; coloured light on the floor works. Panels are about 10 large flat pieces each and look like vector clip art. Empty sockets are dark grey shapes.

**V1 bar:**
- [ ] Pieces separated by visible lead came: dark, slightly raised, with a soft highlight.
- [ ] Each piece has subtle internal texture and colour variation.
- [ ] Backlight glow increases as pieces are placed; completion is a clear, beautiful reveal.
- [ ] Empty sockets show a faint outline of the intended image.
- [ ] Coloured light falls across the studio floor at completion.

**References:** `06-mosaic`. **Never:** flat vector shapes with no came or texture.

### 5.10 Studio interior (Oct 24: 2 → 3 → 4/5 after side-by-side round 4)

**Now:** coherent palette (teal walls, cream, burnt-orange beams), curtains, pendants and pottery. Flat shading, large empty floor areas, tables are slabs on blocks.

**V1 bar:**
- [ ] Warm window light falls across the floor in visible shapes.
- [ ] Every station is a modelled piece of furniture with legs, edges and material detail.
- [ ] Storytelling clutter around stations: jars of glass, tools on hooks, sketches. No empty corners in the main views.
- [ ] Clear neglected-to-restored change: dull and dusty at the start, warm and bright at the end.

**References:** `07-studio`. **Never:** slab tables, large empty floor planes.

### 5.11 Truck and road (Oct 24: 3 → 4/5)

**Now:** the truck is charming and recognisable, with colour blocking, but boxy. The road is a flat grey plane with dashes.

**V1 bar:**
- [ ] Truck bodywork has rounded panels, soft bevels and small details (badge, light wear).
- [ ] Cargo visibly fills with recognisable items.
- [ ] Road has edge variation, sand drifting onto the tarmac, and colour variation.
- [ ] Roadside has varied planting and props, not a repeated row.

**References:** `09-truck-road`. **Never:** flat uniform grey road.

### 5.12 First-person tools (Oct 24: 2 → 3 → 4/5 after side-by-side round 2)

**Now:** the shovel is a hexagon on a stick. The bucket, sack and sieve have been reshaped and read better.

**V1 bar:**
- [ ] Each tool is a designed toy-like object with bevels, curves and a material read (plastic, metal, canvas, wood).
- [ ] Upgrades are visibly better (shape, material, size), not just recoloured.
- [ ] Tools are lit consistently with the scene and do not cast oversized shadows.

**References:** `08-props-tools`. **Never:** flat-shaded polygons on a stick.

### 5.13 Effects and feedback (Oct 24: 1 → 4/5)

**Now:** plain white sphere bubbles; little visible effect on digging, loading or recycling.

**V1 bar:**
- [ ] Digging throws small sand puffs.
- [ ] Collecting, loading, recycling, sorting and placing each have a small particle or motion response.
- [ ] Effects are soft and stylized (shaped particles, colour-matched), never default Unity particle squares.

**References:** `04-cleaning`, `05-sorting`. **Never:** default particle sprites, plain spheres.

### 5.14 UI and HUD (Oct 24: 3 → 4/5 after side-by-side round 2)

**Now:** cream panels with dark teal text match the brief. There are no icons, and in-world labels are low contrast and sometimes unreadable.

**V1 bar:**
- [ ] Simple illustrated icons for equipment, caps, XP and capacity.
- [ ] HUD takes less screen space during interactions (cleaning, sorting, mosaic).
- [ ] All in-world signs are legible at their normal viewing distance.

**References:** `10-ui`. **Never:** text-only HUD, unreadable signage.

## 6. Priority order

Work in this order. Each step improves the most frames for the least effort, and later steps depend on earlier ones being right.

1. **Lighting, sky and post-processing (5.1).** Changes every screen; must be right before judging any model or material.
2. **Sea glass pieces (5.6) and cleaning station (5.7).** The core satisfaction loop and the game's identity.
3. **Sand, terrain and shoreline composition (5.2, 5.3).** The beach fills most of the play time.
4. **Sorting station (5.8) and mosaic panels (5.9).** Completes the glass journey.
5. **Studio interior (5.10) and first-person tools (5.12).**
6. **Effects (5.13), props (5.4, 5.5), truck and road (5.11), UI (5.14).**

## 7. Review method

Every art pass must include matched before/after captures from the fixed review set, compared side by side with the relevant reference images.

**Fixed review set:**

- The eight standalone verification views (`--verify-seaglass`, written to `Builds/Windows/Verification`): shoreline, loaded truck, cloudy glass, half-clean glass, clean glass, sorted tray, ending, restored studio.
- The three shoreline views from October 24 (`First-person camera`, positioned captures):
  - Coast: position (-27, 2.2, 0), looking at (-35, -0.6, 16)
  - Waterline: position (-30, 1.6, 60), looking at (-34.5, -0.7, 66)
  - Overview: position (-22, 9, 90), looking at (-34, -0.6, 110)

**For each pass, record in [IMPLEMENTATION.md](IMPLEMENTATION.md):**

- Area(s) targeted and the reference folders used.
- Bar items now met, and items still open.
- New score per area.
- Captures from the fixed review set.

A pass is not done because it was applied; it is done when its bar items can be checked off in the captures.

## 8. Open decisions

These need a decision from the project owner before the larger passes:

- **Asset source.** Current art is generated by editor scripts. That approach has reached its limit for hero objects (glass, tools, stations). Options: hand-model in Blender, buy a stylized asset pack that matches this direction and adapt it, or a mix (bought environment, hand-made hero objects).
- **Paint-overs.** Whether to commission or make paint-overs of the review views as exact targets.
- **Shader approach.** Whether to build a custom stylized lighting shader (gradient ramps, purple shadow tint) for all materials, or keep URP Lit materials and rely on lighting and post-processing.

## 9. Progress log

### October 24 — passes 1 to 6

Six repeatable passes (menu `Sea Glass/Art Direction/1`–`5`; pass 6 is runtime code) were applied and compared against official Steam screenshots of the reference games (notes in `ArtReference/notes.md`). New models are built by Blender scripts in `ArtSource/blender` and imported by `ModelLibraryImporter`. Matched before/after captures of ten fixed views are in `Captures/ArtPass/baseline` and `Captures/ArtPass/after` (positions in `IMPLEMENTATION.md`).

Six areas now meet 4/5. V1 is **not** yet met: eight areas are still at 2–3.

| Area | Bar items met | Still open |
| --- | --- | --- |
| 5.1 Lighting (4) | Graded sky, purple-tinted shadows, clear light/shadow split, crisp sun, distance haze, visible AO | Clouds are better shaped but still soft-edged in places |
| 5.2 Water (3) | Colour bands, animated foam line on sand and rocks, wet-sand sheen, no seam | Coastline is still a long straight strip apart from rocky points |
| 5.3 Sand (3) | Painted ripples and patches, soft dig mounds, shells/pebbles/kelp, no flat single colour | Little height variation on the open beach; inland rock hills still very faceted |
| 5.4 Props (2) | Faceted rocks | Trees, grass, benches and parasols not reworked; no cove focal landmarks added |
| 5.5 Rubbish (2) | — | Not reworked |
| 5.6 Sea glass (4) | Rounded pebbles, frosted/stained dirty state, refracting clean glass with rim colour and backlight glow, inner streaks | Rare red is not visibly special |
| 5.7 Cleaning (4) | Modelled basin, tap, brush and sprayer; clear water; foam and droplets; brush-streak reveal; colour sparkle when clean | Grime clears by a global value, not under the actual brush position |
| 5.8 Sorting (4) | Lined trays readable across the room, legible labels, pieces visibly fill trays | — |
| 5.9 Mosaic (4) | Raised lead came, glass streaks and seeds, empty panels show the picture outline, floor light spill | Panels are still about 10 large pieces each |
| 5.10 Studio (3) | Modelled workbenches, rug, bench, easel, stools, crates, jars; window light shapes | No neglected → restored change beyond the lights; walls and ceiling still plain |
| 5.11 Truck/road (3) | Sand drifts break the road edges | Truck bodywork unchanged |
| 5.12 Tools (3) | Designed toy spade and round sieve | Bucket and sack unchanged; no hand model |
| 5.13 Effects (3) | Foam, droplets, sand puffs, clean sparkle | Loading, recycling and placing still lack their own effects |
| 5.14 UI (4) | Drawn icons, HUD steps aside at stations, legible signs | — |

Highest-value next work: coastline shaping and cove landmarks (5.2, 5.4), rubbish models (5.5), the neglected-to-restored studio change (5.10), and effects for loading, recycling and placing (5.13).

### October 24 — passes 7 to 11

Passes `Sea Glass/Art Direction/7`–`11` (pass 10 is runtime code). Twelve of fourteen areas now meet 4/5. V1 is still **not** met: sand and terrain (5.3) and first-person tools (5.12) remain at 3.

| Area | Change | Still open |
| --- | --- | --- |
| 5.2 Water (3 → 4) | `CoastShape` authors five headlands and four bays; the water shader, wet-sand shoulder and tide-line details share it; boulder spines end each headland | — |
| 5.4 Props (2 → 4) | Windswept umbrella pines with layered faceted canopies replace box-and-blob trees; 98 two-tone grass tufts; headlands act as cove focal points | Benches and parasols are the earlier polish models |
| 5.5 Rubbish (2 → 4) | Labelled can, ribbed blue bottle with cap, crushed can, crimped cap and branching driftwood, readable at standing distance | No light wear on labels; no pre-dig glint |
| 5.10 Studio (3 → 4) | Neglected → restored: wilted planters green up, flowers open and festoon lights come on one third per completed panel | Walls and ceiling still plain |
| 5.11 Truck (3 → 4) | Rounded chassis, cargo tubs and cushions; cargo shows real glass pebbles and rubbish | — |
| 5.13 Effects (3 → 4) | Reward bursts are coloured four-point sparkles that rise and fade | — |
| 5.3 Sand (3) | — | Open beach has little height variation; inland hills are still large faceted masses |
| 5.12 Tools (3) | — | Bucket and sack unchanged; contents are a neutral grey pile; no hand model |

### October 24 — passes 12 and 13

**Correction (later on October 24):** this claim did not hold. These scores were self-assessed from our own captures, without opening each reference screenshot next to the matching game view. The side-by-side rounds below re-scored ten of fourteen areas at 2–3.

| Area | Change | Still open |
| --- | --- | --- |
| 5.3 Sand (3 → 4) | The blocky inland rock row is one continuous sculpted dune ridge with wind-cut crests, grass on the high ground and a few outcrops; 48 broad, walkable beach swells keep 1.2 m clear of every collectible; the lighting shader now grades light within the lit side so gentle slopes read as form | Swells are simple smooth domes; no footprints |
| 5.12 Tools (3 → 4) | Modelled enamel bucket (band, rolled rim, bail handle, wooden grip) and soft canvas sack (rope drawstring, straps, patch); contents show sand, coloured glass, a can, a cap and a bottle | No hand model; contents are a representative heap, not the exact finds |

### October 24 — side-by-side comparison, rounds 1 to 3 (passes 1b and 14 to 17)

Method: each official Steam screenshot of the reference games was opened full width in a browser (viewed, not downloaded) and compared against a game capture taken from a fixed position with the matching subject. Notes were written after each pair. The full notes and the per-round tables are in `ArtReference/comparison.md` (local, git-ignored). Game captures: `Captures/Compare/current` (round 1), `round2`, `round3`.

| Area | Round 1 | Round 2 | Round 3 | Main change / what still differs |
| --- | --- | --- | --- | --- |
| 5.1 Lighting | 3 | 3 | **4** | Cool blue-lilac zenith, lit cumulus, deeper violet shadows (pass 1b); zenith lowered so eye-level views show blue (round 3). No light shafts. |
| 5.2 Water | 3 | **4** | 4 | See-through shallows over sand, sun-glint path, saturated deep colour. |
| 5.3 Sand/terrain | 3 | 3 | **4** | Layered headland rocks (14); layered sea stacks, cliff-and-hill far islands, broad sand colour patches, pebbles/shells/tufts and a tide line (17). From low angles the beach is still a broad warm plane. |
| 5.4 Props/vegetation | 3 | 3 | **4** | Flower clumps (14); dark shrub groups and a picnic scene under each parasol (17). |
| 5.5 Rubbish | 4 | 4 | 4 | — |
| 5.6 Sea glass | 3 | **4** | 4 | Working piece at a believable 42 cm. |
| 5.7 Cleaning | 2 | **4** | 4 | Salt clears under the brush with a crisp stroke edge; foam at the contact point. |
| 5.8 Sorting | 4 | 4 | 4 | — |
| 5.9 Mosaic | 3 | **4** | 4 | Stained-glass fragments with near-black lead inside each piece (FrostedGlass shader). |
| 5.10 Studio | 3 | 3 | **3** | Wall textures, nets, paintings, chalkboard (15); plants, baskets, crate shelf, hanging planters, rugs, bench-end jars and tools (17). Still a long bare walkway down the hall next to Unpacking's density. |
| 5.11 Truck | 4 | 4 | 4 | — |
| 5.12 Tools | 3 | **4** | 4 | Gloved hands grip each tool (16). Bucket forearm angle is acceptable, not natural. |
| 5.13 Effects | 4 | 4 | 4 | Not compared in motion. |
| 5.14 UI | 3 | **4** | 4 | Paper-card panels with tape and a rounded system face (Candara where installed). |

After round 3, V1 was **not** met: the studio interior (5.10) was still at 3.

### October 24 — studio round 4 (pass 18)

Pass `Sea Glass/Art Direction/18` (`StudioLayoutPass`, models `ArtSource/blender/studio_island.py`) answers the Unpacking side-by-side: a work island across the central walkway, a second table on the sorting rug, a 4 m open shelving divider filled with books, jars of glass, bottles, pottery and baskets, and desk sets on the island, table and equipment bench. Furniture has colliders. Every walking gap is at least 2.2 m for the 1 m player capsule, checked with capsule overlaps along the routes. The mosaic band stays clear.

| Area | Round 3 | Round 4 | Verdict |
| --- | --- | --- | --- |
| 5.10 Studio | 3 | **4** | The walkway now ends at the island and the hall reads as zones; shelf density is close to Unpacking's bookshelves. Still short of Unpacking: a large pale ceiling, open floor between zones and the open road side. Weakest 4 with 5.3. |

All fourteen areas now score at least 4/5 in side-by-side comparison with the reference screenshots, which meets this document's V1 art threshold as judged from fixed captures. This is not a human play-test sign-off, and effects were not compared in motion.

### October 25 — independent blind reviews, rounds 5 to 9 (passes 19 to 22)

**Correction to the round 4 entry above.** "All fourteen areas at 4/5" was again my own judgement of my own work. From round 5 on, every score comes from a fresh independent reviewer (a separate agent) that sees only this document's sections 1–5, the current captures and the reference games' Steam screenshots, never earlier scores. Its findings drove each round's fixes. Captures: `Captures/Review/r5-before` … `r9`; notes in `ArtReference/comparison.md`.

| Area | Review 1 | Review 2 | Review 3 | Review 4 | Review 5 | Review 6 |
| --- | --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 3 | 3 | 4 | 4 | 4 | 4 |
| 5.2 Water | 3 | 3 | 4 | 4 | 3 | 4 |
| 5.3 Sand/terrain | 2 | 2 | 3 | 3 | 3 | 3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3 | 4 | 3 |
| 5.5 Rubbish | – | – | – | 3 | 3 | ~3 |
| 5.6 Sea glass | 2 | 2 | 2 | 3 | 3 | 3 |
| 5.7 Cleaning | 2 | 2 | 2 | 3 | 3 | 3 |
| 5.8 Sorting | 2 | 3 | 3 | 4 | 4 | 4 |
| 5.9 Mosaic | 3 | 3 | 4 | 4 | 4 | 4 |
| 5.10 Studio | 3 | 3 | 3 | 3 | 4 | 3 (close to 4) |
| 5.11 Truck/road | 2 | 2 | 2–3 | 3 | 3 | 3 |
| 5.12 Tools | 2 | 3 | 3 | 3 | 4 | 4 |
| 5.13 Effects | – | – | – | – | – | – |
| 5.14 UI / signs | – | – | – | – | – | 4 (signs) |

(– = not judgeable from stills.) Reviews 1–3 were before or between rounds 5–6, review 4 after round 6, review 5 after round 7, review 6 after round 8; round 9's changes (clouds, zone signs, canopy grading, glass thickness, per-pixel water fog) have not been reviewed yet.

**V1 is not met.** No area is at 2 any more and six reach 4, but sand/terrain, props, sea glass, cleaning, studio and truck/road remain at 3. Reviewers also disagree near the line (one called the glass too puffy, the next too thin), so single scores near 3/4 are not precise. The largest remaining gaps need new modelling rather than tuning: a rounded truck body, a second tree species with layered canopies, a dusty-to-restored studio state, see-through basin water with the glass sitting in it, and denser near-field sand detail.

### October 25 — round 10 and review 7 (pass 23)

Round 10 did the bigger modelling work: truck canopy and tube rails, palms as a second tree species, see-through basin water, a dusty-to-restored studio, and near-field sand detail (ripples, footprints, shell drifts).

| Area | Review 6 | Review 7 |
| --- | --- | --- |
| 5.1 Lighting | 4 | 4 |
| 5.2 Water | 4 | 4 |
| 5.3 Sand/terrain | 3 | **4** |
| 5.4 Props/vegetation | 3 | 3 |
| 5.5 Rubbish | ~3 | – |
| 5.6 Sea glass | 3 | 3 |
| 5.7 Cleaning | 3 | 3 |
| 5.8 Sorting | 4 | 3 |
| 5.9 Mosaic | 4 | 4 |
| 5.10 Studio | 3 | **4** |
| 5.11 Truck/road | 3 | 3 |
| 5.12 Tools | 4 | 3 |
| 5.14 UI / signs | 4 | – |

Review 7 judged a build in which the new studio dust also greyed the first-person sleeve and the basin; that made the sleeve read as "a grey drainpipe" and the water as milky, and likely cost 5.12 and 5.7 points. Dust no longer applies within 3.5 m of the camera (fixed after the review, not re-reviewed). Sorting dropped from 4 to 3 on the thin tray pebbles; reviewers keep disagreeing on glass thickness.

**V1 is not met.** Five areas at 4 (lighting, water, sand, mosaic, studio), six at 3 (props, glass, cleaning, sorting, truck, tools). Remaining work named by review 7: chunkier tray and cloudy pieces with a visible rim and contact shadow; clearer basin water with a visible enamel wall; a rounded truck bonnet and fenders plus visibly heaped cargo; grass in drifts with 3–4 shapes; shaped cooler, log and towel; a larger brush head.

### October 25 — round 11 and review 8 (pass 24)

Round 11: 12 chunky pebbles per tray, a rim on cloudy glass, a more teal basin, a larger brush with lighter sleeve folds, rounded truck fenders, a forked driftwood branch, and grass drifts.

| Area | Review 7 | Review 8 |
| --- | --- | --- |
| 5.1 Lighting | 4 | 4 |
| 5.2 Water | 4 | 4 |
| 5.3 Sand/terrain | 4 | 3 |
| 5.4 Props/vegetation | 3 | 3 |
| 5.5 Rubbish | – | 3 (little evidence) |
| 5.6 Sea glass | 3 | 3 |
| 5.7 Cleaning | 3 | 3 |
| 5.8 Sorting | 3 | **4** |
| 5.9 Mosaic | 4 | 4 |
| 5.10 Studio | 4 | 3 |
| 5.11 Truck/road | 3 | 3 |
| 5.12 Tools | 3 | **4** |
| 5.14 Signs | – | 3 |

Five areas at 4 again. Several areas now move between 3 and 4 from one reviewer to the next with little change in between (sand, studio, sorting, tools), so the scores have plateaued within reviewer noise; the remaining gaps need larger new work, not tuning.

**Bug found by review 8 and fixed:** the placement helper used for all scatter since round 3 (shrubs, pebbles, shells, footprints, grass drifts) took the first collider hit, which could be an invisible boundary wall or a dune's box collider taller than the visible mound, so some dressing floated 1–2 m above the sand (it read as "grass growing out of the sea" along the horizon). It now only accepts visible, non-sea surfaces within the object's visible height; all dressing was re-seated (0 items left floating). Earlier reviews saw builds with this bug.

Remaining work named by review 8: one shared rounded-pebble mesh family for every colour (pieces currently follow their mosaic socket outlines), clearly transparent basin water with a rounded enamel bowl and higher-contrast grime, storytelling clusters instead of repeated filler in the studio and a stronger neglected state, clustered grass on dune crests with open sand between, a domed truck bonnet with badge and wear, faceted stratified sea stacks, larger mosaic nameplates.

### October 26 — round 12 and review 9 (pass 25)

Round 12: one shared family of five tumbled pebble meshes for held, sorted, found and carried glass (mosaic pieces keep their socket-fitting shapes); an oval enamel bowl with a blue rolled rim and see-through water; dark, hard-edged grime patches with crisp brush-stroke edges.

| Area | Review 8 | Review 9 |
| --- | --- | --- |
| 5.1 Lighting | 4 | 4 |
| 5.2 Water | 4 | 4 |
| 5.3 Sand/terrain | 3 | 4 |
| 5.4 Props/vegetation | 3 | 3 |
| 5.5 Rubbish | 3 | 3 (limited evidence) |
| 5.6 Sea glass | 3 | 3 |
| 5.7 Cleaning | 3 | 3 |
| 5.8 Sorting | 4 | 4 |
| 5.9 Mosaic | 4 | 4 |
| 5.10 Studio | 3 | 3 |
| 5.11 Truck/road | 3 | 3 |
| 5.12 Tools | 4 | 4 |
| 5.14 Signs | 3 | 3 |

The shape problem is solved (the reviewer calls the glass "pebble-like" now), but the targeted areas stayed at 3 on material and presentation. Clean glass reads as an evenly coloured "jelly lozenge" without inner glow, and cloudy glass as "brown speckle on teal", a dirty stone rather than pale frost. The bowl reads as "a dinner plate": its white enamel floor shows through the clear water, so the water has no visible depth, and the piece is small inside it. Next for these two: a pale frost layer instead of brown speckle when cloudy; thickness-graded colour, inner glow and a bright rim when clean; a darker teal depth tint and a visible waterline in the bowl; and a smaller bowl or larger piece (about 40% of the bowl).

### October 27 — round 13 and review 10 (follow-up to pass 25)

Round 13 (shader and size only): cloudy glass is a pale frost with soft sandy patches instead of dark brown speckle; clean glass has a stronger inner glow and rim with less satin haze; the bowl water uses neutral lighting so its teal holds under the warm studio light, with a deeper centre, a reflection band and a bright waterline (before this, the water's colour was being greyed out by the warm light and read as bare enamel); the piece at the cleaning bowl is about 50% larger.

| Area | Review 9 | Review 10 |
| --- | --- | --- |
| 5.1 Lighting | 4 | 3 |
| 5.2 Water | 4 | 4 |
| 5.3 Sand/terrain | 4 | 3 |
| 5.4 Props/vegetation | 3 | 3 |
| 5.5 Rubbish | 3 (limited evidence) | 3 (barely judgeable) |
| 5.6 Sea glass | 3 | **2** |
| 5.7 Cleaning | 3 | **2** |
| 5.8 Sorting | 4 | 3 |
| 5.9 Mosaic | 4 | 4 |
| 5.10 Studio | 3 | 3 |
| 5.11 Truck/road | 3 | 3 |
| 5.12 Tools | 4 | 3 |
| 5.14 Signs | 3 | – (plaques low-contrast) |

The round did not raise the targeted areas; review 10 scored glass and cleaning lower. Review 9 asked for a larger piece in the bowl; review 10 calls the larger piece an "oversized jelly blob" and asks for pieces at tray scale. Review 10 also still reads the cloudy piece as "grey dough or stone" and the water as a "pale flat disc", and finds no foam, splash or brush-path cleaning. It is the strictest reviewer so far: four areas outside glass and cleaning dropped from 4 to 3 with no changes to them. Only water and mosaic are at 4.

Taken together, reviews 8 to 10 show that tuning the glass shader and bowl does not move these areas. What all three ask for is new work, not tuning:
- Clean glass: crisp specular glints and sparkle.
- Cloudy glass: a matte white crust with the colour showing only at thin edges.
- Cleaning: grime removed along the brush path with hard edges, foam clumps and ripples where the brush touches, and a bristle-tuft brush.
- Tray pieces: varied silhouettes, thickness and tilt.

New issues review 10 raised:
- Clouds are flat cutouts with stain-like blotches.
- The open studio side reads as a stage set.
- Mosaics have a uniform Voronoi "crackle".
- Truck cargo does not visibly fill.
- Bright cross-shaped streaks appear in the shallows.
- The world edge is visible from the high coast view.

### October 27 — round 13b and review 11 (pass 26)

Round 13b: new cleaning and glass work, not tuning.
- Grime clears along the brush path with hard edges: the first pass lifts a dark sand crust, the next reveals the clear glass.
- Cloudy glass is a matte white crust with pits and solid dark sand patches; its colour shows at the thin rim.
- Clean glass has a crisp key-light glint, a soft sheen and flashing facets.
- Brushing leaves lasting clumps of suds and ripple rings in the bowl water.
- A new bristle-tuft scrub brush (`ArtSource/blender/round13.py`, menu pass 26).
- Sorted tray pieces now match the spare pebbles around them; they were about 3× larger.
- The verification run now brushes for 2.5 s, so frame 05 shows real strokes, foam and a moving brush.

| Area | Review 10 | Review 11 |
| --- | --- | --- |
| 5.1 Lighting | 3 | 3 ("3.5") |
| 5.2 Water | 4 | 4 |
| 5.3 Sand/terrain | 3 | 3 |
| 5.4 Props/vegetation | 3 | 3 |
| 5.5 Rubbish | 3 (barely judgeable) | 2.5 (little visible) |
| 5.6 Sea glass | 2 | 2 |
| 5.7 Cleaning | 2 | **3** |
| 5.8 Sorting | 3 | **4** |
| 5.9 Mosaic | 4 | 3 ("3.5") |
| 5.10 Studio | 3 | 3 |
| 5.11 Truck/road | 3 | 3 |
| 5.12 Tools | 3 | 3 |
| 5.13 Effects | – | 2.5 (stills only) |
| 5.14 Signs | – | – |

The brush-path cleaning registered: review 11 confirms that "grime does come off in strokes". Cleaning rose to 3, and sorting returned to 4 with the matched tray pieces.

Review 11's verdicts:
- **Glass** stays at 2. The reviewer reads clean pieces as glossy jelly beans and wants a satin, lower-gloss finish with a rim glow. That contradicts reviews 9 and 10, which asked for sharper glints.
- **Piece size:** the reviewer wants the held piece about 40% smaller. Review 9 asked for it larger.
- **Cloudy glass:** "a potato or stone". The reviewer wants the glass hue tinting the frost and mottled two-tone grime.
- **Brush:** it reads as a shoe or clog. The reviewer wants a long, slim head with a handle.
- **Sleeve:** the arm is a rigid tube.
- **Foam:** two foam droplets float in mid-air above the floor.

New in review 11:
- Idle mosaics read as three black slabs.
- The mosaic came is thick and flat.
- Frames 08 and 09 look identical.
- The restored studio barely changes.

Glass has now been judged 2–3 by four reviewers with opposing fixes (glossier vs satin, larger vs smaller). Its remaining gap is less a single fix than a consistent target, which this log does not yet define.

### October 29 — round 13c and review 12

The owner fixed the clean-glass target as glossy at the current bowl size (5.6), after a side-by-side render of glossy and satin finishes and of two bowl sizes.

Round 13c:
- The scrub brush is now a flat, rectangular wooden block with a tall bristle block and an arched grip; review 11 read the old rounded back as "a clog".
- Cloudy glass shows more of its hue through the frost and at the thin rim.
- Splash droplets are low and short-lived. They used to hang in mid-air because the foam emitter has no gravity.
- Review captures apply the idle tint the game gives empty mosaic sockets at runtime, so the empty wall no longer renders as black slabs that no player sees.
- Verification frame 09 shows the whole restored room instead of repeating frame 08.

| Area | Review 11 | Review 12 |
| --- | --- | --- |
| 5.1 Lighting | 3 | 3 |
| 5.2 Water | 4 | 4 |
| 5.3 Sand/terrain | 3 | 3 |
| 5.4 Props/vegetation | 3 | 3 |
| 5.5 Rubbish | 2.5 | 3 (partly judgeable) |
| 5.6 Sea glass | 2 | **3** |
| 5.7 Cleaning | 3 | 3 |
| 5.8 Sorting | 4 | 4 |
| 5.9 Mosaic | 3 | **4** |
| 5.10 Studio | 3 | 3 |
| 5.11 Truck/road | 3 | 3 |
| 5.12 Tools | 3 | **4** |
| 5.13 Effects | 2.5 | – |
| 5.14 Signs | – | – |

Four areas are at 4: water, sorting, mosaic and tools. The brush now reads as "designed, toy-like" and the glove and spade pass.

What review 12 still asks for:
- **Glass**, judged against the glossy target:
  - thickness-based colour (a darker, saturated centre and a bright thin rim);
  - one or two large crisp highlights instead of glitter-like specks;
  - more varied tray silhouettes and sizes;
  - grime as thin streaks over frost rather than a solid coat.
- **Cleaning:** a deeper bowl with a visible inner wall and a stronger depth tint, and a tap whose spout arcs over the bowl (from the player's view it reads as a coat stand).
- **Unchanged from earlier reviews:**
  - flat clouds;
  - one grass tuft type with near-black bases;
  - an empty studio floor with flat rugs;
  - a box-built truck with near-empty cargo;
  - flat rock faces.

**Capture artefact found by review 12 and fixed:** both stations' working-glass pieces were saved as visible in the scene. The game hides them on start, but every editor review capture since round 10 showed a large clean piece floating over the trays (s10) and resting in the bowl (s11). They are now saved hidden.

### October 29 — round 14 and review 13 (pass 27)

Round 14 (glass and bowl, from review 12):
- Clean glass has a view-based dark core with a thin bright rim and one large crisp highlight instead of glitter specks.
- Cloudy glass is a whiter crust with thin brown streaks.
- Four new silhouettes (shard, sliver, nugget, thin oval) join the catalog and the tray spares, with varied size and tilt.
- The bowl is 60% deeper, with its inner wall showing above a lower water line.
- Each piece floats at the waterline.
- The gooseneck tap arcs over the back of the bowl.
- A stray brass bar from an older tap is hidden.

| Area | Review 12 | Review 13 |
| --- | --- | --- |
| 5.1 Lighting | 3 | **4** |
| 5.2 Water | 4 | 3 ("3.5") |
| 5.3 Sand/terrain | 3 | 3 |
| 5.4 Props/vegetation | 3 | 3 |
| 5.5 Rubbish | 3 | 3 |
| 5.6 Sea glass | 3 | 2 ("2.5") |
| 5.7 Cleaning | 3 | 3 |
| 5.8 Sorting | 4 | 3 ("3.5") |
| 5.9 Mosaic | 4 | 3 ("3.5") |
| 5.10 Studio | 3 | 3 ("3.5") |
| 5.11 Truck/road | 3 | 3 |
| 5.12 Tools | 4 | 3 |
| 5.13 Effects | – | 3 (stills only) |
| 5.14 Signs | – | – |

**The round made glass and sorting worse in this reviewer's eyes.** Two of the new shapes caused it:
- **Bowl piece:** the piece used at the wash bowl in the verification run became the new thin flat oval. Floating at the waterline, it read as "a paper-thin disc lying flush on the water", "a pancake" and "a decal".
- **Amber shards:** the new triangular shards in the amber tray read as "Cheetos".
- **Trays:** piles still sit in "one thin strip under the label", so the trays look 80% empty.

**Clean glass** still reads as candy. The reviewer asks for visible thickness and transmission (the tray felt seen through the piece), within the glossy target.

**Untouched areas also moved.** Water, mosaic and tools each lost a point and lighting gained one, with no changes to any of them. Across reviews 9 to 13 most areas have moved between 3 and 4 from one reviewer to the next, so a single review cannot confirm a one-point change.

**New issues:**
- The equipment pegboard covers half of mosaic 3 in the restored-room view.
- The cargo bins look closed, so loaded glass sits on the lid.
- The glass jars glow like lamps.

### October 29 — round 15 and review 14 (pass 28)

Round 15 undid what backfired in round 14:
- Held, washed, found and carried glass uses only chunky shapes (the five pebbles and the nugget); the thin oval is no longer used.
- The shard is much rounder.
- Tray glass spreads across the middle of each tray with a few strays; sorted pieces now rest on the felt (they sat 12 cm up).
- The equipment pegboard and its tools are hidden. They covered half of the Coastal Sunset mosaic, and every review disliked them.

| Area | Review 13 | Review 14 |
| --- | --- | --- |
| 5.1 Lighting | 4 | 3 |
| 5.2 Water | 3 | 4 |
| 5.3 Sand/terrain | 3 | 3 |
| 5.4 Props/vegetation | 3 | 3 |
| 5.5 Rubbish | 3 | 3 (thin evidence) |
| 5.6 Sea glass | 2 | 2 |
| 5.7 Cleaning | 3 | 3 |
| 5.8 Sorting | 3 | 3 |
| 5.9 Mosaic | 3 | 4 |
| 5.10 Studio | 3 | 3 |
| 5.11 Truck/road | 3 | 3 |
| 5.12 Tools | 3 | 3 |
| 5.13 Effects | 3 | – |
| 5.14 Signs | – | – |

**Glass stays at 2.** Against the owner's glossy target, the reviewer still reads clean glass as candy:
- "one round white blob of highlight, no small flashing facets".
- The target in 5.6 names small flashing facets, and round 14 removed them on review 12's advice ("glitter specks"). They should come back, fewer and crisper.
- The reviewer also asks for darker, saturated edges with a lighter centre. That is the opposite of review 12's "darker centre, bright rim", so thickness colouring keeps flipping between reviewers.
- Cloudy glass reads as "a chocolate-chip cookie": the round spots should be streaks.

**Sorting stays at 3.** New concrete point: the aqua, cobalt and violet linings match their glass, which kills contrast. Neutral linings with a coloured label band are suggested.

**Lighting and mosaic swapped again** with no changes to either: lighting 4 → 3, mosaic 3 → 4, water 3 → 4.

**Bright rectangular bars on the shallows (b3, b8)** have now been reported by four reviews and are still unfixed.

### October 29 — round 16 and review 15 (pass 29)

Round 16:
- **Facets back:** a few crisp, diamond-shaped facet flashes return near the highlight on clean glass. The 5.6 target names them; round 14 had removed them.
- **Grime:** cloudy grime is streaks plus silt at the edges, not round spots.
- **Tray linings:** neutral linen, with a band in the glass colour along each front rim.
- **Sea:** the brush-mark pattern is rotated and warped, so the sun sheen no longer lights it up as straight-edged bars.

| Area | Review 14 | Review 15 |
| --- | --- | --- |
| 5.1 Lighting | 3 | **4** |
| 5.2 Water | 4 | 3 |
| 5.3 Sand/terrain | 3 | 3 |
| 5.4 Props/vegetation | 3 | 3 |
| 5.5 Rubbish | 3 | 3 |
| 5.6 Sea glass | 2 | **3** |
| 5.7 Cleaning | 3 | 3 |
| 5.8 Sorting | 3 | **4** |
| 5.9 Mosaic | 4 | 4 |
| 5.10 Studio | 3 | 3 |
| 5.11 Truck/road | 3 | 3 |
| 5.12 Tools | 3 | 3 |
| 5.13 Effects | – | 3 (partly judgeable) |
| 5.14 Signs | – | – |

**Gains.** Glass returned to 3: "the glossy target is met in that pieces have a glint and saturated colour", and the cloudy state "reads acceptably as frosted". Sorting reached 4 with the neutral linings and colour bands. The glint bars are no longer reported.

**Still missing.**
- **Glass:** transmission. The pieces read as opaque, with no felt seen through them. Round 14's darker core probably works against that.
- **Bowl water:** no reflection, and the floor of the bowl does not show through.

**New this review.**
- Beach shadows are too dense and too long.
- Close-range foam is a blurry smear.
- Possibly mirrored text on signs seen from behind.
- The bowl water does not show from the far frame 09 view. It is likely hidden by the deeper rim from that angle; this is not checked.

Areas at 4: lighting, sorting, mosaic. Water moved 4 → 3 with no change since round 16's glint fix.

### October 29 — round 17 and review 16 (pass 30)

Round 17:
- Clean glass shows the background through it: a lighter core and stronger refraction.
- The bowl water reflects the room and has a clearer middle.
- Sun shadow strength drops from 1 to 0.75.

| Area | Review 15 | Review 16 |
| --- | --- | --- |
| 5.1 Lighting | 4 | 3 |
| 5.2 Water | 3 | 4 |
| 5.3 Sand/terrain | 3 | 3 |
| 5.4 Props/vegetation | 3 | 3 |
| 5.5 Rubbish | 3 | 3 (limited evidence) |
| 5.6 Sea glass | 3 | 3 |
| 5.7 Cleaning | 3 | 3 |
| 5.8 Sorting | 4 | 4 |
| 5.9 Mosaic | 4 | 4 |
| 5.10 Studio | 3 | 3 |
| 5.11 Truck/road | 3 | 3 |
| 5.12 Tools | 3 | 3 |
| 5.13 Effects | 3 | – (partly judgeable) |
| 5.14 Signs | – | – |

**Regression, confirmed by me in frame 04.** The lighter bowl water now matches the pale frosted piece, so the cloudy glass "barely shows up at all" ("a near-transparent flat pancake, almost the same colour as the water"). I checked only frames 05 and 06 before the review. Next time:
- **Water:** darken it again, or tint it more strongly.
- **Cloudy piece:** give it a more opaque, warmer crust and a dark contact edge.

**Clean glass** stays at 3. Glint and facets meet the target. The lighter core now reads "milky pastel" (mint emerald, periwinkle cobalt), and this reviewer again asks for deeper, more saturated colour. Glass has gone back and forth between "too opaque" and "too pale" with each change.

**Clouds** are this reviewer's top must-fix. Several reviews have named them, and lighting has moved between 3 and 4 accordingly.

**Notes on two other findings.**
- The pieces in the "idle" tray captures are the trays' standing dressing. They are not a capture error.
- The doc's reference table lists Dorfromantik, while the review prompt names Glass Masquerade and Lil Gator Game.

### October 29 — round 18 and review 17 (pass 31)

Round 18 changes:
- The bowl water is a deeper teal again, so the cloudy piece stands out. I checked this in an editor test and in frame 04 before the review.
- The cloudy crust is denser and has a soft dark edge.
- Clean glass has a deeper, more saturated colour.
- The clouds are shaded by a clean ramp with no noise blotches, and their band sits higher, clear of the dune crests.

| Area | Review 16 | Review 17 |
| --- | --- | --- |
| 5.1 Lighting | 3 | **4** |
| 5.2 Water | 4 | 4 |
| 5.3 Sand/terrain | 3 | 3 |
| 5.4 Props/vegetation | 3 | 3 |
| 5.5 Rubbish | 3 | 3 |
| 5.6 Sea glass | 3 | 3 |
| 5.7 Cleaning | 3 | 3 |
| 5.8 Sorting | 4 | 4 |
| 5.9 Mosaic | 4 | 4 |
| 5.10 Studio | 3 | 3 |
| 5.11 Truck/road | 3 | 3 |
| 5.12 Tools | 3 | **4** |
| 5.13 Effects | – | 2 (stills: flat foam circles) |
| 5.14 Signs | – | – |

**Five areas are at 4, the most so far:** lighting, water, sorting, mosaic and tools.

**Glass and cleaning stay at 3.**
- The reviewer still reads clean glass as opaque candy with "nothing behind showing through". That conflicts with frame 06, where the tray rim and its band are visibly bent through the held piece. The effect is likely too subtle at review scale.
- The cloudy piece now reads as "a white plaster disc".
- The bowl water still has no depth gradient or reflection streak.

**New and concrete:**
- Studio furniture has no contact shadows or AO, so it looks pasted on.
- The brushing foam reads as flat circle sprites; this is why 5.13 scored 2.
- The floor texture shows blurry dark smudges, and the s8 floor has a seam.
- A black slab sits above the truck cab.

### October 29 — round 19 and review 18 (pass 32)

Round 19:
- Soft lilac contact-shadow blobs under 45 floor-standing studio pieces, and stronger SSAO.
- Brushing foam is drawn as soap-bubble clumps with rims and highlights.
- The wash water is deeper in the middle and has a crisp window reflection streak.
- Clean glass refracts more strongly.

| Area | Review 17 | Review 18 |
| --- | --- | --- |
| 5.1 Lighting | 4 | 4 |
| 5.2 Water | 4 | 3 |
| 5.3 Sand/terrain | 3 | 3 |
| 5.4 Props/vegetation | 3 | 3 |
| 5.5 Rubbish | 3 | 3 |
| 5.6 Sea glass | 3 | 3 |
| 5.7 Cleaning | 3 | 3 |
| 5.8 Sorting | 4 | 4 |
| 5.9 Mosaic | 4 | 3 |
| 5.10 Studio | 3 | 3 |
| 5.11 Truck/road | 3 | 3 |
| 5.12 Tools | 4 | 3 |
| 5.13 Effects | 2 | **3** (suds and ripples "good") |
| 5.14 Signs | – | – |

**The effects change landed.** The suds rose from 2 to 3 and are now called "good". The contact shadows were not mentioned as missing.

**Three areas fell a point with no change to them:** water, mosaic and tools. Over reviews 9–18 every area except sorting has moved between 3 and 4 with unchanged art. A single reviewer's 3 or 4 is within noise; only several consecutive reviews show a trend.

**New concrete items.**
- **Bowl water:** the new streak reads as "a crack or tape stripe".
- **Floor:** the nail-head grid aliases into a "perforated board" pattern.
- **Shelf under the sorting table:** the slatted shelf shows moiré.
- **Build vs editor:** the built studio frames are greyer than the editor captures, worth checking.
- **Clouds:** stray single-dot clouds.

**Recurring items, still open.**
- Mosaic crackle with the same cell size everywhere and no per-cell colour.
- The glint on clean glass reads as a flat sticker.
- The cloudy piece reads as a flat plate.
