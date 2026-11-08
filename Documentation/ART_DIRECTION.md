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

**Fixed target (owner's decision, October 29):** clean glass is **glossy**. It has a sharp white glint, small flashing facets, and clear, saturated colour. A satin, softly frosted finish was rendered side by side with it and rejected. The piece at the wash bowl stays at its current size (about 0.3 m across; owner's choice, November 8, down from 0.42 m), so the brush strokes stay readable. Reviews judge clean glass against this target. "Too glossy" or "too large" is not a gap; how well the glossy glass reads as glass is.

**Fixed form (October 31, chosen by Claude at the owner's request):** over reviews 9–39, blind reviews pulled the shape in opposite directions. It went pancake, then biscuit, lens, almond, dome and jelly, and "thicker" and "flatter" were asked for in consecutive reviews. The owner asked for one reference to be picked and held. Each piece is a **worn shard of bottle wall**:
- **Outline:** blunt, broad and irregular, with one or two straight broken edges and rounded corners. Length is 1 to 1.4 times the width. No pointed tips.
- **Thickness:** a wedge. About 30% of the width at the thick edge, tapering to about 12% at the thin edge. Both faces are gently domed, like the curve of a bottle wall, and never pillow-shaped.
- **The wash-bowl piece (owner's decision, November 8):** a larger hero version of the same shard, with a crisper worn edge, small chips along the rim and fine pitting, and thinner than the pieces in the trays and palm: about 19% of the width at its thick edge, tapering to about 7% at the thin edge, so at that size it reads as a plate of bottle wall rather than a block.
- **Surface:** the glossy target above, with faint micro-pitting, so the glint breaks into a few small flashes rather than one smear.
- **Colour:** the saturated hue of the glass, deeper where thick and lighter and clearer at the thin edge. What shows through takes the glass hue. The rim catches a thin line of light in the glass colour, not a glow.
- **Trays:** low heaps up to about two layers, with only a faint colour pool on the felt.

Reviews judge form and finish against this reference. Asking for a different thickness or a satin finish is not a gap. How well a piece meets this description is.

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

### October 29 — round 20 and review 19 (three independent reviewers, averaged)

Round 20:
- **Mosaics:** the cells are larger, and each pane gets its own hue shift and streak direction. The came is thinner, with a faint warm glint.
- **Wash water:** the window streak is softer.
- **Studio floor:** the under-floor slab is now a dark wood tone, and the plank texture uses anisotropic filtering. The distant seam dashes are not fixed; they come from the gaps between floor planks breaking up at a distance.

From this round, each review uses three independent reviewers. Each one has its own reference-browser tab, and their scores are averaged. Single reviews had swung by about a point on unchanged areas.

| Area | A | B | C | Average |
| --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** |
| 5.2 Water | 4 | 4 | 4 | **4.0** |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 (partly judgeable) |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** |
| 5.9 Mosaic | 4 | 3 | 3 | 3.3 |
| 5.10 Studio | 4 | 3 | 3 | 3.3 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 |
| 5.12 Tools | 4 | 4 | 3 | 3.7 |
| 5.13 Effects | – | 3 | 3 | 3.0 (stills only) |
| 5.14 Signs | – | – | – | – |

The three reviewers agree on 10 of the 13 scored areas, so this is a much firmer baseline than any single review.

**Named by all three:**
- Glass still reads as candy:
  - every piece has the same lentil profile;
  - the colour is flat, with no thickness change from core to rim;
  - the cloudy piece reads as "a biscuit" or "a soap bar".
- The wash water looks opaque, with a single straight streak.
- The grime comes off in hard-edged blocks rather than brush-shaped strokes.
- Grass tufts are near-black and all the same.
- Large flat areas of foreground sand.
- The truck is built from boxes.
- Stray single-dot clouds.

**Named by two of the three:**
- The mosaic sub-came still reads as a uniform crackle.
- The floor colour spill is a blurred smear.
- Some studio clutter is copy-pasted.
- The built studio frames look greyer than the editor captures (that is the intended neglected state).

### October 29 — round 21 and review 20 (three reviewers, averaged)

Round 21:
- **Glass shape.** Six worn slab shapes with flat faces, rounded edges and some broken edges replace the puffy pebbles.
- **Clean glass colour.** No dark core; the edges are deeper and more saturated.
- **Brushing.** A soft round brush stamp with a thin rim of lifted grime along each stroke.
- **Wash water.** Clearer in the middle.
- **Grass.** Three colour variants. Two-sided foliage now lets the low sun through, so backlit grass glows instead of going near-black.

| Area | A | B | C | Average | Review 19 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 3 | 4 | 3.7 | 3.3 |
| 5.10 Studio | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 4 | 3 | 3.3 | 3.7 |
| 5.13 Effects | – | 3 | – | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

Mosaics went up a little and studio and tools went down a little; the rest is unchanged. The changes did land: the grass now reads with "a good base-to-tip gradient", and the cloudy piece reads as "properly frosted" for one reviewer. They did not move the glass or cleaning scores.

**What all three now say about the glass and cleaning:**
- Clean glass is "thick, flat-topped, opaque, pastel". The slab shapes made it read as soap or "a guitar pick" rather than candy, but still not as glass.
- All three ask for the felt to show through the piece and for a bright thin rim.
- The grime streaks on the cloudy piece read as "brown bands like burn marks or shadows". That pattern comes from the round 14 streak noise and should be replaced.
- The wash water still reads as "a milky grey disc with one hard streak".

**Other issues raised again:**
- the uniform Voronoi crackle in the mosaics;
- the blurry floor light under the mosaics;
- empty studio floor and paper-thin rugs;
- faceted, rock-like bushes;
- the boxy truck;
- the visible edge of the sea plane in b4;
- stray tiny clouds.

Across reviews 19 and 20, ten of the thirteen areas sit at the same average. Further tuning of the same materials is unlikely to move them; glass and cleaning need a different approach.

### October 29 — round 22 and review 21 (three reviewers, averaged)

Round 22 was the first content round after the material rounds stalled:
- Mosaic panes are about twice as large, and the floor light is crisper, with lead-line gaps.
- A layered, leafy bush (`round22.py`) replaces the faceted shrub.
- Five low clusters of crates, baskets of raw glass, buckets and jars fill the empty studio floor.
- Straw and sack packing sit in the truck bins.

| Area | A | B | C | Average | Review 20 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.4 Props/vegetation | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.10 Studio | 3 | 4 | 3 | 3.3 | 3.0 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 4 | 3 | 4 | 3.7 | 3.3 |
| 5.13 Effects | 4 | 4 | 3 | 3.7 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

Six averages rose and none fell. Mosaic now meets the bar for all three reviewers, so four areas are at 4.0.

**Glass and cleaning, from all three reviewers.**
- The held and washed pieces are now "flat-topped wedges", which the 5.6 "never" list rules out. The slabs need domed tops and tapered edges.
- The dark brown bands on the cloudy piece read as a shadow or stripe artifact. They should become a warm, mostly opaque sandy film that brushing cuts through to saturated glass.
- No light passes through the glass, and it casts no coloured light onto the felt.

**Other concrete items.**
- Single jars stand in the middle of walkways; they come from the round 22 clusters' jar placements and should be grouped against legs and walls.
- The shelf pottery repeats.
- The neglected studio is only a desaturated version of the restored one.
- Dig piles are pointed cones.
- The truck's text plates read like UI.
- The world edge of the sea shows in b4.
- Stray dot clouds.

### October 29 — round 23 and review 22 (three reviewers, averaged)

Round 23 changes:
- **Glass:** domed glass with tapered edges.
- **Grime:** a sandy grime film instead of dark bands.
- **Sand:** dig mounds a third wider and lower.
- **Studio:** no loose jars in the floor clusters.
- **Sea:** the open-water plane now runs past the horizon, so its edge no longer shows.
- **Sky:** fewer stray dot clouds.

| Area | A | B | C | Average | Review 21 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 3 | 4 | 3.3 | 3.3 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 4 | 3 | 3.3 | 3.7 |
| 5.13 Effects | 4 | 4 | – | 4.0 (stills only) | 3.7 |
| 5.14 Signs | – | – | – | – | – |

**Caveat: the reviewers could barely see the reference games.** The browser pane was hidden, so screenshots timed out. Each reviewer saw only one or two Steam sheets and judged the rest from memory. This review is not directly comparable with review 21.

**What landed:**
- The sea edge is no longer reported.
- The sandy film on the cloudy piece reads as a film for one reviewer ("good").

**Still open, named by all three:**
- **Glass** still reads as candy: nothing shows through the clean pieces and they cast no coloured light.
- **Cleaning:**
  - the grime now clears in "round blobs with contour-line rings" (the soft brush stamp's outline);
  - the wash water is a milky disc with a straight highlight band.
- **Sand and props:**
  - flat inland dunes;
  - evenly spaced grass and roadside planting;
  - box-like shore rocks and island strata.
- **Truck:** still box-built, with a flat cargo fill.
- **Small fixes:**
  - the edge-on zone sign in b2;
  - a seam in one long cloud;
  - the shovel's large shadow in 02.

**Where things stand:** four areas have held at 4.0 for two reviews, and the other nine sit between 3.0 and 3.3. The remaining gap is modelling scale: rocks and islands, the truck body, real glass refraction and denser planting. That is larger work than these tuning rounds.

### October 29 — round 24 and review 23 (three reviewers, averaged)

Round 24 changes (menu pass 36):
- **Shore rocks:** weathered boulder groups replace the stacked slabs, including the low cove shelves.
- **Islands:** one fluted cliff under a grassy cap with tree groves, instead of strata layers.
- **Dunes:** the inland ridge is rebuilt with lit backs and steep, shaded slip faces.
- **Planting:**
  - beach grass and flowers gather into 34 clumps;
  - ridge grass grows in clumps behind the crests, with shrubs in the hollows;
  - roadside trees stand in groups.
- **Signs:** the section signs face the beach.

The reference screenshots loaded properly this time (36–43 viewed per reviewer).

| Area | A | B | C | Average | Review 22 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 3 | 4 | 3.7 | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 3 | 4 | 3.3 | 3.3 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 4 | 4 | 3.7 | 3.3 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 4.0 |
| 5.14 Signs | – | – | – | – | – |

**What landed:**
- No reviewer named the shore rocks, flat dunes, edge-on sign or slab strata any more.
- One reviewer called the boulders "close to Lonely Mountains".

**Still open:**
- **Islands (all three):** the new islands read as "layer cakes": vertical walls under a cap that overhangs like a lid. The fixes asked for are tapered walls with ledges, a slumped base, and grass blending down into the rock. Two reviewers also saw bright vertical streaks on the cliff walls.
- **Sand:** large plain sand foregrounds (b1, s6), footprints that look like flat discs, and a repeated pebble triplet.
- **Glass (all three):** reads as candy, with no see-through, no bright thin rim and no coloured light.
- **Cleaning:**
  - milky basin water with a straight highlight stripe;
  - grime clearing in contour rings.
- **Truck:** box panels, with only one item per cargo bin.
- **Studio:** an empty floor in front of the mosaics, a repeated sack prop, and no railing on the open side.
- **Small fixes:**
  - vertical seams in the long clouds (b2, b7);
  - dash-like streaks in the shore foam;
  - grey-on-grey mosaic nameplates;
  - parasol canopies cast no shadow.

**Where things stand:** water drops to 3.7 and effects to 3.0. With references visible this time, these scores are probably more reliable than review 22's rather than a sign of real regression. Four areas hold at 4.0.

### October 29 — round 25 and review 24 (three reviewers, averaged)

Round 25 changes (menu pass 37):
- **Islands:** sculpted as one heightfield, with a rock apron and talus at the waterline, tapering cliffs with ledges, grass on the ledges (by slope) and a hill with tree groves.
- **Clean glass:** absorbs light through its body, with a deep core, pale thin edges, gentler refraction and four-point star glints.
- **Tray glass:** each clean piece in the trays casts a pool of its colour onto the felt.
- **Bowl water:** a 2x-multiply teal filter replaces the pale disc and straight stripe.
- **Brush strokes:** no longer leave a dark outline.

| Area | A | B | C | Average | Review 23 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.4 Props/vegetation | 3 | 4 | 3 | 3.3 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 4 | 3 | 3.3 | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 3 | 4 | 3.7 | 4.0 |
| 5.10 Studio | 4 | 3 | 4 | 3.7 | 3.3 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 4 | 3 | 3.3 | 3.7 |
| 5.13 Effects | 3 | 4 | 4 | 3.7 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What landed:**
- Tray glass now reads as glass at 2 m ("saturated, rounded and read as glass"), and the coloured pools on the felt are noticed.
- The "layer cake" lids on the islands are gone.
- No contour rings are reported for the grime.

**Still open, named by all three:**
- **Held clean piece (06):** still reads as an opaque gummy block. At hand size you can't see through it, it has a dark top band, and the star glints look stuck on.
- **Bowl water:** still "milky dishwater" in the built frames; the teal filter is too pale over the bright enamel.
- **Islands:** the ledges now repeat as evenly spaced green stripes on every tier. Reviewers want varied spacing, gullies, and grass on only some ledges.
- **Sand:** big plain foregrounds, coin-like footprints.
- **Truck:** boxy body; straight road edges with no sand drift.
- **Clouds:** a vertical seam (b2, b7).

**New notes:**
- Shadow wedges on the shallow water around the sea stacks (b4, b8).
- The floor light under the mosaics is pale.
- The tray glow sometimes reads as "neon puddles".

**Where things stand:** water is back at 4.0, and studio and effects rose to 3.7; mosaic dipped to 3.7. Four areas are at 4.0. Sea glass stays at 3.0, because the held hero piece still reads as candy even though the tray pieces now pass.

### October 30 — round 26 and review 25 (three reviewers, averaged)

Round 26 changes:
- **Held glass:** clean glass absorbs through a softened hue, and the rim and grazing-reflection terms are limited to the true silhouette. An inner crescent of light was added. The tray rim and felt now read through a held piece.
- **Glints:** fewer, softer star glints.
- **Bowl water:** a deeper teal filter.
- **Islands:** ledges with uneven spacing and sheer stretches, gullies, and grass in patches.
- **Clouds:** cloud shading saturates with distance from the outline. This did not remove the seam.

| Area | A | B | C | Average | Review 24 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.4 Props/vegetation | 4 | 3 | 4 | 3.7 | 3.3 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 4 | 3 | 4 | 3.7 | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.10 Studio | 3 | 3 | 4 | 3.3 | 3.7 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 4 | 4 | 4 | **4.0** | 3.3 |
| 5.13 Effects | – | 4 | 4 | **4.0** (stills only) | 3.7 |
| 5.14 Signs | – | – | – | – | – |

**What landed:**
- Six areas are at 4.0 for the first time: lighting, water, sorting, mosaic, tools and effects.
- Tray glass reads as glass for all three reviewers.
- The wash bowl now draws "pale and flat" rather than "milky dishwater".
- Nobody reports even island stripes any more.

**Still open:**
- **Sea glass (all three):**
  - the cloudy piece (04) reads as a "sand biscuit" or "toast": an opaque beige slab of even thickness with a hard cut edge and a dithered mask edge;
  - the held clean piece (06) is a flat-topped slab with a grey opaque patch on top.
- **Islands (all three):** the per-face grass mask reads as "blocky pixel camo". It needs smooth blended edges.
- **Sand:** flat foregrounds (b1, s6), coin footprints, one repeated pebble triplet.
- **Truck/road:** box panels, a floating seat back, straight road edges with no drift.
- **Studio:** empty floor in front of the mosaics, smeared plank grain, flat rugs, a repeated sack prop.
- **Rubbish:** unreadable at 5 m.
- **Bugs:**
  - shadow wedges on the shallow water around the sea stacks (b8);
  - the cloud seam (b2, b7);
  - mirrored sign text seen from behind.

### October 30 — round 27 and review 26 (three reviewers, averaged)

Round 27 changes (menu pass 38):
- **Glass shape:** the glass slabs are lenses: thick middle, rolled rims, no broken straight edge.
- **Frosted glass:** shows its own colour through the frost, with sand in patches.
- **Island grass:** painted per pixel by a new Painterly top colour, on a dedicated island rock material.
- **Sea:** softer shadows on the water.

| Area | A | B | C | Average | Review 25 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 4 | 3 | 3.3 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 4 | 3.3 | 3.7 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 4 | 3.3 | 3.7 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.13 Effects | 4 | 3 | – | 3.5 (stills only) | 4.0 |
| 5.14 Signs | – | – | – | – | – |

**What landed:**
- Two of three reviewers say cloudy glass meets the bar: "frosted, with the colour showing through".
- No reviewer mentions blocky island grass or water wedges any more.

**Still open, named by all three:**
- **Held clean piece (06):** reads as candy because one large highlight blob clips to white and covers about a quarter of the face, and the star glints look like stickers.
- **Tray glass:** almost every piece is now the same almond lens. Reviewers want five or more outlines (triangle, D-shape, chunky nugget, sliver) and one red piece.
- **Bowl water:** still "milky, flat"; there are no drips or splashes.
- **Studio:** empty floor in front of the mosaics, a repeated sack prop, flat rugs, a white box bookcase, and a dusty state that is only a colour grade.
- **Truck/road:** box panels, cylinder posts, straight road edges.
- **Sand:** coin footprints, a repeated pebble triplet.
- **Bugs:**
  - off-centre tray labels;
  - a dotted shadow pattern on the studio floor (s8);
  - the cloud crease;
  - grass tufts standing in the sea (b1, b5).

### October 30 — round 28 and review 27 (three reviewers, averaged)

Round 28 changes (menu pass 39):
- **Clean glass:** a small sharp glint replaces the blown-out highlight blob, and star flashes are fewer and dimmer.
- **Glass shapes:** six distinct slab shapes (oval, triangle, D-chip, sliver, nugget, shard).
- **Planting:** 20 tufts and flowers left hanging above swapped rocks were set down.

Frame time on a quiet machine: 11.1 ms. The 14–16 ms readings in round 27 came from other programs running at the same time.

| Area | A | B | C | Average | Review 26 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 3 | 3 | 3.3 | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.13 Effects | – | 3 | 3 | 3.0 (stills only) | 3.5 |
| 5.14 Signs | – | – | – | – | – |

**What landed:**
- The tray shapes now vary ("varied shapes, close to the owner's target").
- Grass in the sea and the off-centre labels are no longer reported, except one reviewer who still finds the labels off.

**Still open:**
- **Held clean piece (all three):** the outline is "lumpy or wobbly", from the outline noise on a strongly domed lens. It is still "jelly or candy", and nothing visibly passes through it.
- **Sharp shapes:** the sharper triangle and shard pieces read as "jagged star shards", which breaks the rounded-pebble rule.
- **Cloudy piece:** reads as "mint soap", and the grime boundary is a straight vertical wipe line.
- **Rare red:** never seen in any frame.
- **Cleaning:** weak contrast between dirty and clean; no splashes or drips; the tap is a bare hose arc.
- **Unchanged areas:**
  - truck boxes and the road edge;
  - bush clumps, the cooler and slab signs;
  - rubbish unreadable at 5 m;
  - empty studio floor and repeated sacks;
  - pale mosaic floor light;
  - unreadable mosaic plaques.

**Where things stand:** five areas hold at 4.0. Sea glass has now sat at 3.0 for fourteen reviews (since review 13) through many material and shape changes. At this point, more glass tuning gives reviewers a new complaint each round rather than a higher score.

### October 30 — round 29 and review 28 (three reviewers, averaged)

Round 29 changes (menu pass 40):
- **Truck:** crowned bonnet, rolled sills, tapered canopy posts, painted dashboard, and three pieces shown per loaded find.
- **Props:** rounded two-tone cooler, routed wooden signs, bush base-to-crown gradient, rubbish at 1.5× size.
- **Studio:** varied baskets, gallery benches with jars, cream mosaic plaques with larger text.

| Area | A | B | C | Average | Review 27 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 4 | 3 | 3.3 | 3.3 |
| 5.4 Props/vegetation | 4 | 3 | 4 | 3.7 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 3 | 3 | 3.3 | 3.3 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 4.0 |
| 5.13 Effects | 3 | 3 | – | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What landed:**
- Props rose to 3.7: the cooler, bench and signs now read as designed objects.
- All three reviewers note the visibly filling truck bins and the rounded hood and fenders.
- The plaques are legible for one reviewer; another still finds them small.

**Still open, named by all three:**
- **Truck:** the cab side, door and cargo bins are still plain boxes, and the road edge is straight with a dark lip.
- **Studio:**
  - one potted plant and one jar crate repeat about 15 times, and the round 29 swaps added more of them;
  - the floor strip in front of the mosaic wall still reads as empty.
- **Sea glass:**
  - the held piece has a hard seam or ragged edge on the right;
  - the tray pieces read as thin almond or leaf lenses;
  - the cloudy piece reads as "soap" and sits on top of the water;
  - no red piece appears.
- **Cleaning:** the water is a milky disc the glass floats on, and there are no splashes or drips.
- **Tools:** fell to 3.0 without any change. Reviewers flag the flat spade blade, a sleeve that fills a third of the frame, and the tool's shadow in 02.
- **Sand:** flat foreground, coin-like footprints, purple pebble pairs.

**Where things stand:** five areas hold at 4.0 and the rest sit at 3.0–3.7. Over the last six reviews the averages have moved by tenths, while the fixes each round answer the previous round's complaints. The remaining gap is breadth: almost every prop, surface and effect needs another pass of hand-modelling to reach the references.

### October 30 — round 30 and review 29 (three reviewers, averaged)

Round 30 changes (menu pass 41):
- **Truck:** bulging doors and lipped, flared cargo tubs.
- **Road:** 52 sand drifts over the road edges.
- **Studio:** four plant models in rotation, and low linen baskets alternating with the wicker ones.

| Area | A | B | C | Average | Review 28 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.4 Props/vegetation | 4 | 3 | 4 | 3.7 | 3.7 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 3 | 3 | 3.3 | 3.3 |
| 5.11 Truck/road | 4 | 4 | 3 | 3.7 | 3.0 |
| 5.12 Tools | 4 | 3 | 4 | 3.7 | 3.0 |
| 5.13 Effects | 4 | 3 | – | 3.5 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What landed:**
- The truck rose to 3.7. Two reviewers scored it 4, noting "the road has sand drifts, cracks and a varied edge" and cargo bins that fill.
- Nobody reports the repeated plant any more.

**Still open:**
- **Truck:** the front canopy posts pass through the bonnet, and the door still reads as a slab for one reviewer.
- **Sea glass (all three):**
  - the held piece's broad highlight smear and dented right edge;
  - thin almond and arrow-shaped tray pieces;
  - the cloudy piece reads as chalk or soap.
- **Cleaning:** a straight grime boundary in 04, no splashes, and the tap is a bent tube.
- **Studio:** bare floor in s8 and 07b, flat rugs, and hanging planters floating past the roof edge (s4).
- **Sand:** repeated pebble decal, coin footprints, near-black boulders.
- **Rubbish:** still too small to read.

### October 30 — round 31 and review 30 (three reviewers, averaged)

Round 31 changes (menu pass 42):
- **Truck:** the front canopy pillars moved behind the bonnet.
- **Studio:** the edge planter moved inside the roof line.
- **Sea glass:** thicker, rounder glass pebbles with a smoother outline.
- **Cleaning:** the brushed grime now clears with a ragged edge.

Reviewer C could not view the references because the browser pane was hidden. Reviewers A and B viewed 32 and 40 screenshots.

| Area | A | B | C | Average | Review 29 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.4 Props/vegetation | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 4 | 3 | 3.3 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 3 | 4 | 3.3 | 3.3 |
| 5.11 Truck/road | 4 | 3 | 4 | 3.7 | 3.7 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.7 |
| 5.13 Effects | 4 | – | 3 | 3.5 (stills only) | 3.5 |
| 5.14 Signs | – | – | – | – | – |

**What landed:**
- Props reached 4.0, joining lighting, water, sorting and mosaic, so five areas now meet the bar.
- No reviewer reports the pillars through the bonnet or the floating planter any more.

**Still open:**
- **Sea glass (all three):** the rounder pieces now read as "gumdrops, jelly beans, water droplets". Reviewers now ask for flatter, irregular worn shards, which is the direction rounds 21–28 came from. The held piece also has a brown internal smear and a clipped right edge, and cobalt and emerald cast neon halos on the felt.
- **Studio:** the empty floor in front of the mosaic wall (07b, 08) is still named by all three. Also flat rugs, repeated shelf jars, and a dusty state that is only a colour grade.
- **Cleaning:** opaque milky water and no splashes.
- **Tools:** the sleeve is a long blue cylinder across the cleaning frames; the spade is a flat paddle.
- **Sand:** coin footprints, a repeated pebble cluster, a flat foreground.

**Where things stand:** the glass shape has now been called too thin, too flat, too sharp and too round in successive reviews. That points to reviewer taste around a fixed target, not a missing feature.

### October 30 — round 32 and review 31 (three reviewers, averaged)

Round 32 changes (menu pass 43):
- **Mosaic wall:** a trestle table with a panel in progress and a leaning stepladder in the gaps between the mosaics.
- **Rugs:** thicker woven rugs with a rolled edge and fringe.
- **Wash bowl:** pebbles on the bowl floor.
- **Splashes:** larger, more frequent brush splashes.

All three reviewers viewed the references (43–48 screenshots each).

| Area | A | B | C | Average | Review 30 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 4 | 3.3 | 4.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 4 | 3 | 3.3 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 4 | 3.3 | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.11 Truck/road | 3 | 4 | 4 | 3.7 | 3.7 |
| 5.12 Tools | 3 | 3 | 4 | 3.3 | 3.0 |
| 5.13 Effects | 3 | 4 | 4 | 3.7 (stills only) | 3.5 |
| 5.14 Signs | – | – | – | – | – |

**What changed:** one reviewer scored the glass 4 for the first time, and effects rose. The new trestle and ladder were not mentioned. The empty floor is now seen in s1, s8 and 09 instead, and reviewers also ask for a finer plank grain and a physical neglected state. Props fell back to 3.3 with no change to the props.

**Consistent across the last eight reviews:** lighting, water, sorting and mosaic hold at 4.0; the truck holds at 3.7. The other areas move between 3.0 and 3.7 from review to review. The remaining complaints are now mostly the same list every time:
- the cloud seam;
- coin footprints and repeated pebbles;
- a colourless floor light under the mosaics;
- opaque basin water;
- the sleeve tube;
- a boxy truck cab.

**Recorded bugs still open:**
- A thin vertical seam in the long clouds (b2, b7, s4).
- The basin pebbles read as floating on the surface.
- The mosaic floor light at the ending is grey-white in the centre.

### October 30 — round 33/34 and review 32 (three reviewers, averaged)

Rounds 33 and 34 (menu pass 44):
- **Round 33 bug fixes:** cloud seam, seated basin pebbles, coloured mosaic floor light.
- **Footprints:** soft foot-shaped dents.
- **Pebbles:** five shape variants, about a third removed.
- **Sleeve:** tapered cloth sleeve.
- **Truck mirrors:** ivory.

All three reviewers viewed the references (36–43 screenshots each). This review judged the frames from before the fused hand and arm (round 35, below).

| Area | A | B | C | Average | Review 31 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 4 | 3 | 3.7 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 4 | 3.3 | 3.3 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 4 | 4 | 3 | 3.7 | 3.3 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 4 | 4 | 3.7 | 3.0 |
| 5.11 Truck/road | 4 | 3 | 3 | 3.3 | 3.7 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.13 Effects | 4 | 4 | 4 | **4.0** (stills only) | 3.7 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- Sand rose to 3.7. No reviewer mentions coin footprints any more, and none mentions the cloud seam.
- Sea glass, studio and effects rose.
- Five areas are at 4.0.

**Still open:**
- **Cleaning (all three):** the basin pebbles now read as "dark paw prints". The water is a flat mint disc, the tap is a bent pipe, and the grime edge is blobby.
- **Tools:** a flat shovel blade.
- **Truck:** a flat door, the sphere canopy fringe, a lumpy bonnet.
- **Vegetation:** ball-pile bushes, repeated grass tufts, V-shaped sprigs that read as birds.
- **Mosaic glass:** a stretched marble texture.
- **Sky:** tiny stray cloud dots.

**Owner feedback the same day:** "the arm and hand look terrible ... made out of different shapes put together".

**Round 35 (glove.py):** the hand, wrist, cuff and sleeve are fused into one voxel-remeshed, smoothed surface, coloured by region.

### October 30 — round 35/36 and review 33 (three reviewers, averaged)

Rounds 35 and 36 (glove.py, round36.py, menu pass 45):
- **Round 35:** the hand and arm are one fused surface.
- **Round 36:** the toy spade is one fused surface with a dished blade. The tap riser is twice as tall, so the handle stands above the rim. The working glass sits in the water, and the basin pebbles are gone.

All three reviewers viewed the references (32–47 screenshots each).

| Area | A | B | C | Average | Review 32 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 4 | 3.3 | 3.7 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 4 | 3 | 3 | 3.3 | 3.7 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 3 | 4 | 3.7 | 3.7 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.12 Tools | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.13 Effects | – | – | 3 | 3.0 (one reviewer; stills only) | 4.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- Nobody mentions paw prints, glass floating on the water or the hand being made of primitives any more. The glove and brush now "read well".
- Tools rose to 3.3. One reviewer still calls the spade blade a flat "dustpan" or "shield" from the 02 angle.
- Sand, sea glass and vegetation each lost one reviewer's point. This is reviewer spread: those assets did not change.

**Still open (agreed by two or three reviewers):**
- **Sea glass:** the shapes are plump, uniform almonds that read as "gummy sweets" or jelly beans, so they need flatter, asymmetric shapes with a chipped edge. The held piece in 06 shows a dark inner band or seam (all three).
- **Basin water:** still a flat, milky mint disc with no depth tint or reflection, and the dirty-to-clean contrast is weak (all three). One reviewer says the tap's spout does not reach over the bowl.
- **Truck:** box door and bed, and the sphere canopy fringe (all three).
- **Vegetation:** ball-pile bushes, and dark rock caps glued onto the islands (all three).
- **Rubbish:** cans read as capsules at 5 m, so scale them up 1.3–1.5× (all three).
- **Sand:** a hard terrace step down to the wet band, flat foregrounds, faceted dunes in b4.
- **Mosaic floor light:** hard-edged flat quads that read as paint (all three). The sleeve is a plain tube.

**Bugs reported:**
- A large hard shadow wedge across the beach in b4.
- Stray cloud dots and smeared cloud tails.
- The arm and tool shadow on the sand in 02.
- The mosaic name plate is hidden by a plant (s1, s10).
- There is no contact shadow under the basin.

### October 30 — round 37 and review 34 (three reviewers, averaged)

Round 37 (round21.py, menu pass 46):
- **Glass shape:** flat sea glass, about a quarter as thick as it is wide, with lopsided outlines, a worn straight edge and dented faces.
- **Glass shader:** softer refraction, so the held piece no longer shows a seam, and darker silt.
- **Basin water:** a deep teal middle, sun sparkles, brighter caustics and wobbly rings.

All three reviewers viewed the references (43–46 screenshots each).

| Area | A | B | C | Average | Review 33 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 3 | 3.7 | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 4 | 3.3 | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 3 | 4 | 3.3 | 3.7 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- The seam in the held piece is no longer mentioned.
- The flatter glass did not move the score. The tray pieces still read as "almond / leaf lenses" or "gummy sweets", and the held piece as "a jelly blob with one big highlight".
- Two reviewers now see a ragged lower edge on the held piece in 06. This is new; it is probably the dent noise at the thin rim, or the refraction blur.
- The basin water is still called "milky mint", although one reviewer now asks for a "darker centre" that already exists. The changes show only weakly in the built frames.
- Water, studio and tools each lost one reviewer's point with no change to those areas.

**Still open (agreed by two or three reviewers):**
- **Glass, still the top gap:**
  - It needs several small edge glints instead of one big highlight, and transmission strong enough to see the felt through it.
  - The coloured glow halo on the felt reads as "glow-sticks"; reduce it.
  - The cloudy piece reads as soap or stone.
- **Basin:**
  - The water still reads milky.
  - The foam reads as blue "clover" or "flower" sprites; it should be white soap.
  - The tap's spout does not reach over the bowl.
- **Truck:** disc fringe, box cab and bumper, floating mirrors.
- **Vegetation:** ball-pile bushes, dark boulder caps on the islands.
- **Shore:** a blocky, stamped foam band with a hard wet-sand edge (b6, b8); opaque shallows.
- **First-person view:** a big blue tube of an arm in the cleaning frames, and an oversized spade shadow in 02.

**Bugs reported:** dark hexagon patches on the island cliff (b8); the jar at the s11 table edge clips; mosaic plaques hidden; smeared cloud tails.

### October 30 — round 38 and review 35 (three reviewers, averaged)

Round 38 (menu pass 47):
- **Clean glass:** clearer, with a smaller main glint plus two small rim glints. No shadow term on the backlit glow.
- **Cloudy glass:** thinner frost.
- **Glass meshes:** no fine noise at the thin rim, so the ragged held-piece edge is fixed.
- **Foam:** dense white soap, drawn after the basin water so it is no longer tinted blue.
- **Tap:** a longer swan neck, aimed at the basin centre.
- **Tray glow:** the light pools under the tray glass are dimmed.

The reviewers viewed 32–43 reference screenshots each.

| Area | A | B | C | Average | Review 34 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.3 Sand/terrain | 4 | 3 | 4 | 3.7 | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 3.3 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 4 | 4 | 3.7 | 3.0 |
| 5.13 Effects | – | – | 3 | 3.0 (one reviewer; stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- Five areas now meet the bar: lighting, water, sorting, mosaic and studio. That is the most in this run of reviews.
- Studio (4.0, unanimous), tools (3.7) and sand (3.7) rose without changes to those areas.
- Nobody mentions the ragged edge, the blue foam, the glow halos or the tap any more. The foam now reads as "white blossom / popcorn" sprites.

**Still open (all three):**
- **Sea glass:**
  - The held piece is still "a blob with one big white highlight". The rim glints do not show from the 06 camera.
  - The tray pieces still read as "almond / lens jelly beans".
  - The cloudy piece reads as opaque soap or plaster.
  - All three ask for a darker saturated core with a bright rim, several small glints, and frost as a translucent veil.
- **Cleaning:** weak dirty-to-clean contrast. Grime should be darker and browner, against saturated clean glass. A straight grime seam shows in 04, and the piece shows no waterline.
- **Truck:** disc fringe, box panels, a floating board behind the seat.
- **Bushes:** sphere clumps. The dark boulder caps on the islands remain.
- **Mosaic floor light:** opaque hard-edged decals.
- **Rubbish:** tiny capsules.

**Bugs reported:**
- Hexagon blotches on the b8 island.
- The b4 shadow wedge.
- A straight seam in the sand beside the road (b7).
- Smeared cloud tails and dot clouds.

### October 30 — round 39 and review 36 (three reviewers, averaged)

Round 39 (FrostedGlass.shader, StudioStation.cs) changed the glass shader:
- **Clean glass:** a deeper core and a lit rim, a smaller main glint, and four small glints.
- **Cloudy glass:** frost as a grainy veil.
- **Grime:** browner, with a broader film and no straight seam.
- **Waterline:** a waterline on the working piece in the basin.

The reviewers viewed 41–43 reference screenshots each.

| Area | A | B | C | Average | Review 35 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 3 | 4 | 3.7 | 4.0 |
| 5.3 Sand/terrain | 4 | 3 | 3 | 3.3 | 3.7 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 3 | 3 | 3.0 | 4.0 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 4 | 4 | 3.7 | 3.7 |
| 5.13 Effects | – | 3 | 3 | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- Nothing measurable for glass or cleaning, which have been at 3.0 for four reviews running.
- Studio fell from 4.0 to 3.0 with no change to it. All three reviewers now name cloned props, empty floor and flat rugs, which shows how wide the spread between reviewers is.
- Shader-only glass work has stopped moving the score. Every reviewer still says the tray pieces are "nearly all one almond / lens silhouette", even though the six meshes have different outlines.
- Orientation is not the cause. A check after the review found all 72 tray spares lying flat (thin axis within about 15° of vertical). The almond read comes from the outlines: most taper to soft points at both ends. The low, raking camera also foreshortens them into lenses.

**Still open (all three):**
- **Sea glass:**
  - Tray shapes read as one almond. They need blunter, broader outlines (squarish, triangular, D-shaped, with broken straight edges instead of points), and the pieces should pile and overlap.
  - The held piece still reads as a gummy sweet or candy.
  - The cloudy piece reads as a flat slab or felt.
- **Cleaning:**
  - Grime needs to be an opaque brown crust that each stroke cuts through, as in PowerWash.
  - Add splashes and drips.
  - The piece still reads as "lying on the water".
- **Truck:** box cab and bed, disc fringe, floating arch strips.
- **Vegetation:** sphere bushes; boxy studio plants.
- **Studio:** cloned props, empty floor, flat rugs. The neglected state is only a colour grade.
- **Shore:** a stamped, repeating foam ribbon with a hard edge (b6, b8). The wet sand is a band of constant width.
- **Rubbish:** tiny at 5 m.
- **Arm:** a plain tube in 04/05.

**Bugs reported:**
- Dark hexagon patches on the b8 island.
- Shadows on the water draw pale.
- The EQUIPMENT sign overlaps the frame of mosaic 03 (s4, s9).
- Jars are clipped by the camera's near plane (s5).
- The 09 windows blow out to flat white.

### October 30 — round 40 and review 37 (three reviewers, averaged)

Round 40 (round21.py, menu pass 48):
- **Glass shapes:** blunt shapes with broken edges (squarish chip, rounded triangle, D-shape, pentagon, hex nugget, triangle shard).
- **Trays:** the pieces are gathered into two low heaps per tray.
- **Grime:** a near-opaque brown crust that the brush cuts clean tracks through.

The reviewers viewed 41–46 reference screenshots each.

| Area | A | B | C | Average | Review 36 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.3 Sand/terrain | 4 | 3 | 3 | 3.3 | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 4 | 3.3 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 4 | 3 | 4 | 3.7 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 3 | 4 | 3.7 | 3.0 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 4 | 3 | 4 | 3.7 | 3.7 |
| 5.13 Effects | 3 | 3 | 4 | 3.3 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- Cleaning rose to 3.7 for the first time since review 25. Two reviewers give it 4: the stroke clears a bright track through the brown crust, and there are ripple rings.
- Sea glass is still 3.0, for the fifth review running.
  - The new shapes are no longer called almonds. They are now "puffy domes / blister-pack bubbles / pillows".
  - The low heaps still read as "a single layer".
  - The held piece reads as opaque mint jelly.
  - The cloudy piece reads as a flat slab or sandstone, and its crack lines read as "jigsaw".
- Studio (3.7) and water (4.0) moved back up with no change to them.

**Still open:**
- **Glass, all three reviewers:**
  - Uneven thickness within a piece: a thick wedge that tapers to a thin edge, not an even dome.
  - Visible see-through in the held piece: the felt bent and tinted behind it. The current refraction is too blurred and too absorbed to read.
  - Heaps two to three layers deep, with real overlap.
  - A visible rare red piece.
  - The cloudy piece should be rounder, without crack lines.
- **Foam, three reviewers:** it reads as opaque popcorn discs. They want translucent bubbles with a rim highlight, droplets and a foam film on the water.
- **Truck:** block body, disc fringe.
- **Vegetation:** ball bushes, repeated Y sprigs.
- **Rubbish:** specks at 5 m; the buried hint is a flat disc ("lily pad").
- **Mosaic floor light:** reads as an opaque decal.
- **Sand:** a hard wet/dry seam.

**Bugs reported:**
- A teal gap between the two halves of the sorting table (06, s10).
- A straight seam line on the b7 shoulder.
- The s7 hanging pot looks self-lit.
- The shovel's grip cap reads as a detached disc (02).

### October 30 — round 41 and review 38 (three reviewers, averaged)

Round 41 changes:
- **Glass shape:** wedge-shaped pieces, thick along one side and tapering to a thin edge.
- **Glass shader:** a clearer, near-sharp see-through on the held piece, a lighter core, and no crack lines in the crust.
- **Trays:** heaps up to about three layers deep.
- **Foam:** see-through bubbles with a bright rim.

The reviewers viewed 40–44 reference screenshots each.

| Area | A | B | C | Average | Review 37 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 4 | 3 | 3.7 | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 4 | 4 | 3 | 3.7 | 3.7 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 4 | 4 | 3.7 | 3.7 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 4 | 3 | 3 | 3.3 | 3.7 |
| 5.13 Effects | 4 | – | – | 4.0 (one reviewer; stills only) | 3.3 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- The foam now reads as "stylised ring bubbles", and nobody calls it popcorn any more.
- Cleaning holds at 3.7. One reviewer now calls the cloudy piece "good: frosted, with colour under the grime".
- Sea glass has been at 3.0 for six reviews.
  - The tray pieces now read as "clam-shell domes / lentils / gel drops".
  - All three agree on one concrete cause: the held clean piece is a "muddy grey-teal" with low saturation. The clearer see-through shows the brown bench through teal glass, and that mix is grey.
  - Two of the three still ask for thicker pieces (2–3×).
- **New bug from the round 41 heaps:** in s10 and 06 the Amber and Violet heaps rise in front of the label boards and hide part of "Amber".

**Still open:**
- **Glass:**
  - Saturate the held piece. Tint what shows through toward the piece's own hue, and colour the rim with it.
  - Chunkier pieces.
  - Lower the heaps, below the labels.
  - Pearl reads as milky plastic.
- **Cleaning:** a hard turquoise contour band at the grime edge (one reviewer); drips and runoff; the basin has no contact shadow.
- **Truck:** box body, disc fringe.
- **Vegetation:** sphere bushes, black island caps, box cacti, plank-leaf pot plants.
- **Rubbish:** tiny; the buried hints read as "lily pads / plates".
- **Sea stacks:** read as "fluted stumps".
- **Mosaic floor light:** reads as opaque decals.

**Bugs reported:**
- A straight seam line in the b7 sand.
- Moiré on the roof underside (s4, s6, s9).
- A blue sliver at the held piece's left tip (06).
- A hanging cord into a plant that sits on a shelf (s7).

### October 31 — round 42 and review 39 (three reviewers, averaged)

Round 42 made three changes:
- **Colour:** what shows through the glass takes the glass's own hue, and the rims are lit in that colour.
- **Thickness:** the pieces are about 1.4× thicker.
- **Heaps:** lower, at about two layers, and kept back from the labels.

The reviewers viewed 37–43 reference screenshots each.

| Area | A | B | C | Average | Review 38 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 4 | 3 | 3.7 | 3.7 |
| 5.4 Props/vegetation | 3 | 4 | 3 | 3.3 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 4 | 3 | 4 | 3.7 | 3.7 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 4 | 4 | 3.7 | 3.7 |
| 5.11 Truck/road | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.12 Tools | 4 | 4 | 4 | **4.0** | 3.3 |
| 5.13 Effects | – | 3 | – | 3.0 (one reviewer; stills only) | 4.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- Five areas meet the bar: lighting, water, sorting, mosaic and tools (4.0, unanimous). Studio, sand and cleaning are at 3.7.
- Nobody calls the held glass grey or muddy any more ("glossy and saturated, matches the owner's target"). The covered "Amber" label is no longer named.
- Sea glass has been at 3.0 for seven reviews.
  - The words change, but the verdict "jelly / gummy" stays.
  - The advice now reverses itself. In review 38 two reviewers asked for chunkier pieces. In review 39, after round 42 made them 1.4× thicker, two ask to flatten them again to about 1:3, and to cut the rim glow by half.
  - Across reviews 9–39 the glass has gone back and forth between pancake, biscuit, lens, almond, dome and jelly. Each change answered the previous review.
  - This suggests the blind-review score for 5.6 is not converging. The owner may need to choose a reference piece directly. (Recommendation; not acted on.)

**Still open:**
- **Glass:** rim glow too strong; no frosted micro-pitting under the gloss; a torn or ragged fin on the held piece's back-right edge (06, two reviewers).
- **Vegetation:** sphere-clump bushes, flat flowers, box cacti.
- **Rubbish:** specks at 5 m; flat buried hints.
- **Truck:** box cab and bed, disc scallops, floating seat block.
- **Mosaic floor light:** reads as opaque paint.
- **Shore:** a repeating tread pattern in the foam.
- **Cleaning:** no splashes.

**Bugs reported:**
- A torn edge on the 06 held piece.
- Stray dot clouds (b1, t1, 03).
- Hexagon pits on the b8 island.
- Faceted brightness patches on the b4 dunes.
- The 09 basin floats with no contact shadow.
- Pieces still touch the bottom of the "Emerald" label (06, s10).

### October 31 — round 43 and review 40 (three reviewers, averaged)

Round 43 set a fixed sea-glass form in 5.6, chosen at the owner's request: a worn shard of bottle wall. The glass was rebuilt to it:
- **Shape:** flat faces and a blunt, rounded rim. Edge-on, the old profile ran to a knife edge, which explains the "almond / lens" reads. The pieces measure 29–39% at the thick edge.
- **Shader:** half the rim glow, and micro-pitted glints.
- **Trays:** fainter colour pools on the felt.

The reviewers viewed 36–39 reference screenshots each.

| Area | A | B | C | Average | Review 39 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 3 | 3 | 3 | 3.0 | 4.0 |
| 5.3 Sand/terrain | 4 | 3 | 3 | 3.3 | 3.7 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 4 | 4 | 3 | 3.7 | 3.7 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 4 | 4 | 3.7 | 3.7 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.12 Tools | 4 | 4 | 3 | 3.7 | 4.0 |
| 5.13 Effects | – | – | – | – (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- The fixed reference worked as intended for the review process. All three reviewers judged 5.6 against it, and nobody asked for a different thickness or finish.
  - The tray heaps now read as "glossy coloured glass" ("emerald and amber best", "saturated and translucent").
  - The criticism now converges on one gap. In the hero views, the held clean piece shows no wedge: it reads as "an even-thickness, near-opaque teal slab", with no clearer thin edge. The mesh is a wedge, but seen face-on the shader shows no difference with thickness.
  - The cloudy piece hides its colour and reads as a "sandy biscuit / mud cake". All three want the hue showing 30–40% through the frost.
- Water fell from 4.0 to 3.0 with no change to it. All three now name the tiled foam ribbon and the ruled edge between wet and dry sand.

**Next for the glass (against the fixed form):**
- Drive transmission and colour from the wedge's real thickness, so the thin edge goes pale and clear and the thick edge stays deep.
- Lift the hue through the frost on cloudy glass, and keep brown grime only in patches.
- Fix a notch or ledge on the held piece's right face (06, one reviewer).

**Still open:**
- **Shore:** foam band and wet-sand edge (all three).
- **Truck:** an assembly of separate parts.
- **Vegetation:** sphere bushes, black boulder caps on the islands.
- **Studio:** primitive studio plants, flat rugs.
- **Rubbish:** specks at 5 m.
- **Mosaic:** floor light reads as decals.

**Bugs reported:**
- Stray dot clouds.
- The "WASH & REVEAL" sign shows mirrored text through its back (s2).
- Roof moiré.
- The b4 shadow wedge.
- The hanging pot in s7 looks self-lit.

### October 31 — round 44 and review 41 (three reviewers, averaged)

Round 44 made these changes:
- **Clean glass:** clarity and colour follow the wedge's real thickness.
- **Cloudy glass:** the frosted hue shows between grime patches.
- **Shore:** a foam reach that varies along the shore, crisp lace, and a wavy inland edge on the wet sand.

The reviewers viewed 40–43 reference screenshots each.

| Area | A | B | C | Average | Review 40 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 3.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 4 | 3 | 3 | 3.3 | 3.7 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.13 Effects | – | 3 | 3 | 3.0 (stills only) | – |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- Six areas now meet the bar: lighting, water, sorting, mosaic, studio and tools. That is the most so far.
- Water returned to 4.0, unanimously. The foam is "lacy" now, and nobody calls the wet-sand edge ruled.
- Sea glass has been at 3.0 for nine reviews.
  - The tray heaps read as glass ("glossy and translucent with lighter thin edges").
  - All three still see the **held piece (06) as an opaque, even-thickness bar of soap**. The thickness-driven colour from round 44 does not read as transmission from the 06 camera.
  - The cloudy piece's grime now reads as "brown rust paint". Two reviewers see pointed tips on its outline.
  - Two reviewers see a stray blue line on the held piece's left rim.
  - Cobalt throws too strong a purple pool.
  - Pearl reads as white plastic.
- Cleaning fell to 3.3. The causes named are the rust-looking grime, the clip-art bubble rings and the milky water.

**Next for the glass:**
- The held piece needs visible see-through: the felt or wood bent and clearly visible through the thin side. Colour alone is not enough.
- Fix the blue rim line.
- Grime should be grey-sand, not rust.
- Round off the remaining tips on the cloudy piece's shape (the D-shape's chord corners).

**Still open (all three):**
- **Truck:** reads as boxes stuck together.
- **Vegetation:** ball bushes; dark caps on the island tops.
- **Sand:** large flat foreground areas; dark, flat-coloured shore boulders.
- **Rubbish:** reads as pills at 5 m.
- **Clouds:** smeared tails and dot clouds.
- **Mosaic:** floor light reads as decals.

**Bugs reported:**
- A straight crease in the b7 sand.
- Long straight white specular bars on the water (b3, b8).
- The s7 hanging plant both hangs and stands on the shelf.
- Blotchy shading on the truck hood.

### October 31 — round 45 and review 42 (three reviewers, averaged)

Round 45 made four changes:
- **Held piece:** held against the tray rim and felt, so what lies behind shows bent through it.
- **Refraction:** a stronger bend.
- **Grime:** sandy grey.
- **Glass outline:** rounder corners where the broken edge meets the outline.

The reviewers viewed 43–46 reference screenshots each.

| Area | A | B | C | Average | Review 41 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 4 | 3 | 3.3 | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 3 | 3 | 3.3 | 4.0 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 4 | 3 | 3.3 | 4.0 |
| 5.13 Effects | 3 | 4 | – | 3.5 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- Sea glass has been at 3.0 for ten reviews.
  - The tray heaps keep reading as glass.
  - The held piece is now read as "jelly / soap" with "a murky dark smear / dark streak inside". The bent tray rim seen through the piece is read as a flaw, not as transparency.
  - Nobody mentions rust-coloured grime any more. The cloudy piece now reads as a "felt / sponge pad".
- Studio and tools fell back, each by one reviewer's point, with no change to either. Water and lighting hold at 4.0.

**Assessment:** over reviews 33–42, ten consecutive rounds of glass work have held 5.6 at 3.0.
- The work covered shape, thickness, colour, see-through, staging and a fixed reference.
- The tray glass now reads as glass. The single held piece at about a third of the screen is where every reviewer stops.
- Further shader passes are unlikely to move this area.
- The broad areas still at 3.0 (sand, vegetation, rubbish, truck), which every reviewer flags the same way, are now the better use of rounds.

**Still open (all three):**
- **Sand:** flat foregrounds, a faceted wet/dry edge.
- **Vegetation:** ball bushes, boulder caps on the islands, repeated rows of palms.
- **Rubbish:** pills at 5 m; buried glass shows as a flat "lid".
- **Truck:** boxes and a disc fringe.
- **Studio:** empty floor, flat rugs, a swirly floor grain.
- **Cleaning:** milky water, weak dirty-to-clean value contrast.

**Bugs reported:**
- Dark half-buried boulders on the b8 island.
- Roof-trim moiré.
- A board floats behind the truck cab.
- The s7 pot looks unlit.

### November 3 — round 46 and review 43 (three reviewers, averaged)

Round 46 (round46.py, menu pass 50):
- **Truck:** the bodywork as one fused surface (lower body, cab sides with doors, crowned bonnet, flared wheel arches, rounded bumpers), the 16 separate pieces hidden; one continuous scalloped valance instead of discs.
- **Bushes:** one fused leafy canopy with leaves along the outline, lighter base colours and a sunlit top painted per pixel.
- **Test:** the mosaic-socket test starts the player on the floor instead of 21 cm above it (a timing fault, not a scene fault).

Reviewers A and B viewed 40–41 reference screenshots. Reviewer C's browser pane would not render and scored from memory of the reference games, so C's comparisons are weaker.

| Area | A | B | C | Average | Review 42 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 4 | 3.3 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.7 Cleaning | 3 | 4 | 3 | 3.3 | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 3.3 |
| 5.11 Truck/road | 4 | 4 | 4 | **4.0** | 3.0 |
| 5.12 Tools | 4 | 4 | 3 | 3.7 | 3.3 |
| 5.13 Effects | – | – | 3 | 3.0 (one reviewer; stills only) | 3.5 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Truck 4.0, unanimous.** It had been 3 from every reviewer in every review since review 19. "Fused rounded body, fenders, scalloped valance, grille, mirrors, stake sides." Remaining notes: flat disc hubcaps, no badge or wear, an empty cab interior.
- **Sea glass 3.3**, with its first 4 (reviewer A: "broad blunt shards with straight broken edges, deeper colour where thick"). B and C still call the tray pieces "puffy ovals / gumdrops" and the held piece "a dark slab / rubber eraser".
- Six areas meet the bar: lighting, water, sorting, mosaic, studio and truck.
- The fused bushes are no longer called "piles of balls". The vegetation score stays at 3 for other reasons: the near-black canopies on the island tops, identical palms, single-tone grass tufts, and the repeated inland bush.

**Still open:**
- **Sand (two reviewers):** too saturated an apricot-yellow against the cream target, and one parallel ripple pattern at one scale across the whole beach.
- **Rocks:** shore boulders render near-black with no lit top.
- **Vegetation:** black island canopies, repeated palms and tufts.
- **Water:** the wash-basin water is still milky, with no surface highlight.
- **Held glass:** a double outline along one edge, a smeared highlight, and a long outline (about 1.8:1).
- **Mosaic:** the floor light is a single hard yellow wedge.
- **Studio:** no window-light shapes on the floor; the open road side reads as a pavilion.

**Bugs reported:**
- Brush bristles clip through the glass (05).
- A double rim on the held piece (06).
- A square-cornered shadow wedge on the beach (b4).
- A flat teal disc on the sand (b2).
- The studio's open side meets the road with no threshold (s4, s9).

### November 4 — round 47 and review 44 (three reviewers, averaged)

Round 47 (PainterlyForward.hlsl, menu pass 51):
- **Sand:** a cool bias in the sand albedo so the lit result lands at cream under the golden sun; the wet shoulder a darker damp cream.
- **Ripples:** two scales that turn and fade from patch to patch, plus a broad tonal drift.
- **Boulders:** the cliff-style base darkening nearly off, a raised base value and a sun-bleached top cap.

All three reviewers viewed the references (40–44 screenshots each).

| Area | A | B | C | Average | Review 43 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 4 | 4 | 3.7 | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 4 | 3.3 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 4 | 3.3 | 3.3 |
| 5.7 Cleaning | 4 | 4 | 4 | **4.0** | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.11 Truck/road | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.12 Tools | 3 | 4 | 4 | 3.7 | 3.7 |
| 5.13 Effects | 3 | – | 4 | 3.5 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Seven areas meet the bar:** lighting, water, cleaning, sorting, mosaic, studio and truck. The most so far.
- **Cleaning 4.0, unanimous** for the first time (the basin, tap, brush, foam and the brush-following reveal all pass; the water is still "milky").
- **Sand 3.7.** Two reviewers now call the beach good (b4 "the best beach frame"). The remaining notes: the inland dunes behind the road read "near-white, like snowdrifts" (one reviewer; the cool bias overshoots where the sun is weaker), and the b1 foreground ripples still read as a fan of parallel stripes.
- **Boulders:** no reviewer calls the shore boulders black any more. The "near-black lumps" every reviewer names are now the trees on the island tops.
- Sea glass holds at 3.3, with a second reviewer giving it a 4 ("form matches the fixed target"). The held piece's lower half reads as a dark reflection band.

**Next:**
- Island-top trees: a lit-top gradient and the haze fade (all three).
- Inland dunes: warm the dune sand slightly so it does not read as snow.
- Held piece: cap the dark reflection on the lower half.
- Basin water: milky, no reflection (all three).
- Foam band: constant width with a hard inner edge (two).
- Studio: no window-light shapes on the floor; the neglected state is only a colour grade.
- Rubbish: capsules at 5 m (two).

**Bugs reported:**
- A sun disc at about 50° while shadows are long and low (02, b5, b8).
- Warm bloom patches on the studio ceiling (s1, s3).
- "Seaweed" squiggles read as scribbles (b4, b6).
- The stray dot cloud (b1, t1, 03).

### November 4 — round 48 and review 45 (three reviewers, averaged)

Round 48 (round25.py, menu pass 52):
- **Island trees:** the canopies lifted from near-black to a mid blue-green with a sunlit yellow-green top painted per pixel, and the crowns re-exported as larger overlapping domes.
- **Dunes:** the beach and dune sand kept cream with only a faint warm cap on upward faces (a stronger cap sent the beach back to apricot); the dry dune sand a touch warmer so it no longer reads as snow.
- **Ripples:** the direction swing from patch to patch widened.

Reviewers viewed 35–43 reference screenshots each; one reviewer's browser pane kept being hidden by the other two and saw the references at store-highlight size. The nine standalone frames (02–09) were judged alongside the 22 review views.

| Area | A | B | C | Average | Review 44 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.6 Sea glass | 4 | 3 | 4 | 3.7 | 3.3 |
| 5.7 Cleaning | 3 | 4 | 4 | 3.7 | 4.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.11 Truck/road | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.12 Tools | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.5 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Eight areas meet the bar, the most so far:** lighting, water, sand, sorting, mosaic, studio, truck and tools.
- **Sand 4.0 from all three** for the first time in an averaged review ("sculpted stacks, dune shapes, ripples, footprints pass"; b4 and b9 named as good frames). Nobody calls the dunes snow or the beach apricot any more. What is left is the b1 and b7 foregrounds: one cream value over the lower half of the frame under a repeated diagonal ripple stripe, with no damp patches or tide lines.
- **Sea glass 3.7**, its best since review 32, with two 4s: "broad blunt shard, saturated aqua, light passes through; reads as glass" (06) and the tray shards "glossy, varied, with faint felt colour pools". The cloudy piece (04) passes for all three (frosted grime with the aqua showing beneath, a straight broken edge). All three name the same remaining gap: the held piece's glint is one long smear, not the few small flashes the fixed form asks for. The dissenting reviewer sees the clean piece as "a glossy gel lozenge" with no visible wedge or broken edge, and the tray pieces as repeated flat ovals in a single layer.
- **Tools 4.0 unanimous** (the bevelled spade, cuffed glove and bristled brush). The forearm is still "a plain navy tube" (two).
- **Cleaning 3.7** (one 3): the basin, tap and brush pass and the dirty-to-clean contrast is called PowerWash-like, but the water is "milky soap film" with no depth or bowl floor (all three), and the suds in 05 are ring-outline sprites (all three).
- **Vegetation 3.0 unanimous.** The round 48 canopies are no longer "near-black", but every reviewer now calls them stacked discs, green lumps or lump clusters sitting on the rock; the palms are flat frond fans, the bushes flat green puddles, and one grass tuft repeats everywhere. Lifting the colour exposed the form.
- **Clouds** are named by all three for the first time: flat cut-outs with one uniform purple underside, against Tchia's lit, lobed cumulus. Lighting still passes on the gradient, sun disc and purple shadows.
- Rubbish back to 3.0 and effects to 3.0 with no change to them (reviewer spread).

**Next:**
- Island canopies, palms, bushes and grass: layered foliage with a lit top and shadowed volume, base-to-tip gradient on grass, more than one tuft (all three).
- Clouds: lit tops and shaded undersides instead of flat shapes (all three).
- Held glass: break the single smeared glint into a few small flashes with micro-pitting (all three); show the wedge taper and a broken edge on the held piece (one).
- Basin water: milky and opaque, no bowl floor (all three; open since review 19). Suds: ring outlines, no volume (all three).
- Foreground sand in b1/b7: damp patches, tide lines and shell scatter (all three).
- Rubbish: small, flat-coloured and unworn, pebble-like at 5 m (all three).
- Shallows: sand should show through the water (two). Window-light shapes on the studio floor (two).

**Bugs reported:**
- The round bush in b5 casts a spiky palm-frond shadow that does not match its silhouette (two).
- The truck's far-side mirror floats beside the cab, its stalk too thin to read (t2; two).
- A flat brown rectangle beside the studio sits on the sand with no edge or contact (b4; two).
- The aqua tray's glow spills past the tray edge onto the table, and a stray teal square sits on the sorting-table apron (s10, 07).
- A green ring (the sieve) clips through the basin rim; the basin has no contact shadow (s11, 04).
- A dark crease runs across the held clean piece (06, 07); its rim is a crisp white outline in 04.
- The EQUIPMENT sign floats unattached beside the pillar (s1, s4).
- A black faceted lump at the right edge of 02; pale flat discs on the sand in b2; a grey lump behind the bench in b9; dark squiggles on the sand in b6.
- A repeated horizontal line tile along the wet-sand strip (b3) and radial stripe banding in the foreground shallows (b8).
- The b1 near boulder shows a hard horizontal light band like a seam; the sea meets the sand at far left of b1 with no foam or wet band.
- The mosaic floor light in 08 is one arrow-shaped patch pointing at the camera.

### November 5 — round 49 and review 46 (three reviewers, averaged)

Round 49 (round25.py, menu pass 53):
- **Island groves:** each grove one voxel-fused canopy mass of broad overlapping crowns with smooth shading, rooted half-way into the hill, instead of 4-7 equal domes (review 45: "stacked green discs", "lumps sitting on the rock"). Six attempts: a crinkled crust and tall crowns both went dark at 100-170 m (noisy normals and side-facing surfaces fall under the purple shadow tint), so the crowns are broad and smooth, measured against the round 48 domes for the same olive value.
- **Island trees material:** lighter base, the sunlit cap from a normal.y of .1, more rim light.

Reviewers viewed 32-40 reference screenshots each. The shared browser pane kept hiding behind the other reviewers' tabs again, and one reviewer could not open a tab of its own (tab cap) and viewed the store screenshots full-size in the spare tab instead.

| Area | A | B | C | Average | Review 45 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 4 | 3 | 3.7 | 4.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 4 | 4 | 3 | 3.7 | 3.7 |
| 5.7 Cleaning | 4 | 4 | 3 | 3.7 | 3.7 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.11 Truck/road | 4 | 3 | 4 | 3.7 | 4.0 |
| 5.12 Tools | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Vegetation did not move (3.0, unanimous).** The fused groves are no longer called discs, but all three now call them "fused knobs", "green lobes pasted on purple mounds" and "lump clusters with a hard light/dark split", and two say what they want instead: layered canopies with distinct pine or palm silhouettes, as on Tchia's islands. A single blob mass per grove is the wrong model at this distance; the groves need to read as individual trees in tiers.
- Six areas meet the bar (lighting, water, sorting, mosaic, studio, tools). Sand and the truck each lost one reviewer's point with no change to them: the sand reviewer sees b1/b2 as "one cream tone with a single repeated ripple pattern", the truck reviewer sees t2 as "assembled parts: thin posts, rails, flat windscreen".
- Sea glass holds at 3.7 with two 4s: 04 passes for all three; 06 reads as glass for two ("glossy wedge with straight broken edge and glint"), as "glossy candy" for one. All three again name the dark crease across the held piece and its single soft glint.
- Cleaning holds at 3.7: the basin, tap and brush pass; the water is "milky opaque" (two) with a caustic web that reads as a cracked texture (two); suds are ring-outline sprites (all three).
- Clouds named by all three again as soft flat blobs with no lit volume.

**Next:**
- Island trees: replace the blob groves with tiers of individual tree silhouettes (umbrella pines or palms at island scale, like the beach's coastal pines) so each grove reads as layered foliage (all three).
- Grass and palms on the beach: spike tufts and five-frond fans with no base-to-tip variation; the roadside bush repeats (all three).
- Clouds: lit tops and shaded volume (all three).
- Basin water: milky, with a cracked caustic web; suds without body (all three).
- Held glass: the dark crease and the single soft glint (all three).
- Foreground sand in b1/b2: one tone with a uniform ripple; shallows opaque (two).
- Rubbish: tiny, flat, unworn (all three). Window-light shapes on the studio floor (all three).

**Bugs reported:**
- The far-side truck mirror floats beside the cab (t2; two).
- The b5 bush casts a jagged frond shadow that does not match its silhouette.
- Dark squiggle strands on the sand (b6; two); a green ring and a white ring lie flat on the sand as discs (b2; two).
- A dark crease across the held clean piece (06; all three).
- The completion floor light in 08 is a near-white arrow-shaped fan (all three).
- The s5 camera clips into the foreground jars (capture fault; two reviews running).
- A hanging plant at s5's top right floats outside the roof line; the brush handle is cut by the basin rim (s11).
- Pale floating facets on the left island slope (b8); a hard horizontal colour split on the far-left island canopy (b1); a stray dark shape at the left island's waterline (b5).
- The emerald tray's colour pool is far stronger than the other trays (s10, 07).

### November 5 — round 50 and review 47 (three reviewers, averaged)

Round 50 (round25.py, menu pass 54):
- **Island pines:** the islands are scaled about 24 x 40 m across and 36-54 m tall in the scene, so the round 49 crowns were only 2-3 m wide. Each island now carries 5-11 individual umbrella pines built in metres (a leaning tapered trunk, a broad flat-topped crown of 3-5 faceted pads in tiers, like the beach's coastal pines), tallest in the grove centres, a lone pine or two on the ledges.
- **Island trees material:** the sunlit cap starts at a normal.y of .25 so only upward facets take it.

All three reviewers viewed the references (39-44 screenshots each).

| Area | A | B | C | Average | Review 46 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 3 | 3 | 3.3 | 4.0 |
| 5.3 Sand/terrain | 4 | 4 | 3 | 3.7 | 3.7 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 4 | 4 | 3 | 3.7 | 3.7 |
| 5.7 Cleaning | 4 | 3 | 3 | 3.3 | 3.7 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 3 | 3.7 | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.11 Truck/road | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.12 Tools | 3 | 4 | 3 | 3.3 | 4.0 |
| 5.13 Effects | 3 | 3 | 2 | 2.7 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Vegetation still 3.0, unanimous.** The pines are now read as trees, but all three call them "stacked flat discs on stilt trunks", "parasol discs on bare poles" and "plates on sticks", with the crowns detached from the trunks and no light-to-shadow gradient. One calls them "parts stuck together", which is the owner's primitive-assembly rule. The flat faceted pads that work on the beach pines at 10 m read as plates at 100-170 m; the crown needs to be one fused, domed mass sitting on the trunk, with a lit top and a shaded underside.
- Only four areas meet the bar this time (lighting, sorting, studio, truck), the fewest since review 41. Water, cleaning, mosaic, tools and effects all lost points with no change to them. This panel was the harshest yet; the round itself only touched the islands.
- **Water 3.3:** two reviewers now lead with opacity, "no see-through turquoise shallows over sand", as the single biggest gap to Tchia. This has been raised before, but never as the top item by two reviewers.
- Sea glass holds at 3.7. The tray pieces are called "the best glass in the game" (C); the held piece still has one large glint smear and a dark diagonal streak (all three).
- Effects 2.7: hollow ring suds, a faint sparkle, a flat floor-light decal.

**Next:**
- Island pines: fuse each crown into one domed mass on its trunk with a lit top and shaded underside, so it reads as a canopy rather than stacked plates (all three).
- Shallows: sand and rock visible through the water near the shore, a depth gradient at the waterline (two, as the top gap).
- Suds with body, drips and splash; a visible clean sparkle (all three).
- Held glass: the single smear and the dark streak (all three).
- Roadside bushes and grass: one repeated sprite (all three).
- Clouds: flat cut-outs (all three). Window-light shapes on the studio floor (all three).

**Bugs reported:**
- Stray dark diamond or hexagon facets on the left island slope (b5, b8; all three).
- A lone pale dot in the sky (b1, b6, t1, 03; two).
- Brown squiggles on the dry sand (b6; two).
- Two jars clip the camera in s5 (all three; capture position).
- The EQUIPMENT sign floats (s1, s3, s4); the truck's wing mirrors are flat slabs (t2).
- A green outline around the glass piece in 05 like a selection highlight; a dark diagonal streak across 06.
- The basin interior is pink against a navy rim (s11, 04); a jar hovers off the table edge (s11).
- Flat dark disc decals under rubbish that ignore the dune shape (b2); a brown rectangle on the sand beside the studio (b4).
- The studio floor planks meet the tarmac with no threshold (s4, s9); a hard seam where the sea meets sand at far left of b1.

### November 5 — round 51 and review 48 (three reviewers, averaged)

Round 51 (Round51Pass, TidalWater.shader, menu pass 55):
- **Seabed:** the water shader has sampled the scene behind it since October, but past the wet-sand shoulder (2 m out, 0.45 m deep) there was no geometry, so the depth buffer held open sky and the sea rendered opaque. One sloping sand mesh now follows the authored coastline out to 100 m offshore (about 1.2 m deep at 10 m out, 3 m at 30 m) with low sandbars.
- **Shader:** the bed is absorbed red-first with a cool tint and stays see-through to about 4.6 m depth (was 2.6 m).

All three reviewers viewed the references (38-40 screenshots each). Two opened the full-size store images directly instead of stepping through the viewer.

| Area | A | B | C | Average | Review 47 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.3 Sand/terrain | 3 | 3 | 4 | 3.3 | 3.7 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.7 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 3 | 4 | 3 | 3.3 | 3.7 |
| 5.10 Studio | 3 | 3 | 4 | 3.3 | 4.0 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 4.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 2.7 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **The round backfired on water (3.0, unanimous).** All three reviewers name the same new fault: the sea-stack shadows now fall on the seabed, and the sunlit sand between them shows through as bright, hard-edged yellow-green wedges, so the shadows read inverted (lighter than the lit water) and one wedge runs to the horizon in b4. Reviewer A calls them "opaque pale-green wedges instead of Tchia's see-through sandbed". The see-through effect works, but the hard sun-and-shadow pattern on the bed overwhelms it. Fix: the seabed should not receive hard sun shadows (or only a soft, low-contrast version), and the bed should be less yellow under the low sun.
- **Only two areas meet the bar** (lighting, sorting). Studio, truck, tools, sea glass, mosaic and cleaning all lost points with no change to them since review 46 or 47: the truck fell from a unanimous 4.0 to a unanimous 3.0 on the same t2 frame ("panels look crumpled and lumpy"), the studio from a unanimous 4.0 to 3.3. This is the harshest panel yet, harder than review 47; read with care against reviews 43-46, which saw the same assets.
- Sea glass 3.0: the tray shards still read as glass; the held clean piece is "a jelly lozenge with one smeared glint" and shows a hard crease (two).
- The island pines: one reviewer now says they "read well"; two still call them stacked flat plates.

**Next:**
- Shallows: stop the seabed receiving hard sun shadows and cool the lit bed, so the see-through sand reads as an even turquoise ramp, not wedges (all three, as a bug).
- Clouds: smooth blobs; Tchia's are built-up cumulus (all three).
- Held glass: the jelly read, the smeared glint, the crease (all three).
- Vegetation: lumpy repeated bushes, sparse grass, plate-tier pines (all three).
- Flat cream sand foregrounds (b1, b2, s6; all three).
- Cleaning: grime off in patches, weak "done" moment, no splash or runoff (all three).
- Mosaic: more and smaller pieces; floor light that reads as light (all three).

**Bugs reported:**
- Inverted, hard-edged shadow wedges on the shallows (b4, b6, b8; all three) — caused by round 51.
- A dark navy strip under the shore foam line (b8); a hard stepped lip between wet and dry sand (b3, b6).
- Dark hexagonal patches on the left island's slope (b8; all three, fourth review running).
- A straight seam across the foreground sand parallel to the road (b7).
- A crease through the held clean piece (06; two); a bright offset outline under the piece (04).
- The flat-topped mesa rock shows a vertical seam between two boxy blocks (b5, b9, 02).
- A stray white dot in the sky (b1, t1, 03); smudged white patches on the truck bonnet and a floating dark board behind the seat (t2).
- The EQUIPMENT sign cuts through the bookshelf (09); the jar hangs off the wash-table edge (s11, 04); a hanging pot at the roof edge against the sky (s5).
- A dark, apparently unfilled area near the top centre of the completed Ocean Wave panel (08).

### November 5 — round 52 and review 49 (three reviewers, averaged)

Round 52 (Round52Pass, PainterlyLit/PainterlyForward, TidalWater.shader, menu pass 56):
- **No hard sun shadows on the seabed:** the painterly shader gained a per-material "Receives sun shadow" setting (default 1, so nothing else changes); the seabed sets it to 0. The water itself darkens only to 86% where a stack blocks the sun, so stack shadows read as slightly darker water instead of inverted bright wedges.
- **Cooler bed:** the seabed sand is cooler, so it shows as turquoise rather than yellow-green under the low sun; it now sinks away over 30 m beyond both beach ends, removing the pale tongue in b4.
- Fixed Round51Pass re-copying the beach material on every run, which had reset the new setting.

All three reviewers viewed the references (37-39 screenshots each), opening the full-size store images directly because the viewer opened trailer videos.

| Area | A | B | C | Average | Review 48 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 3 | 4 | 3.7 | 4.0 |
| 5.2 Water | 4 | 3 | 3.5 | 3.5 | 3.0 |
| 5.3 Sand/terrain | 3 | 3 | 3.5 | 3.2 | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3.5 | 3.2 | 3.0 |
| 5.7 Cleaning | 4 | 3 | 3.5 | 3.5 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 3.3 |
| 5.10 Studio | 4 | 3 | 3.5 | 3.5 | 3.3 |
| 5.11 Truck/road | 3 | 3 | 3.5 | 3.2 | 3.0 |
| 5.12 Tools | 3 | 3 | 3.5 | 3.2 | 3.0 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **The wedges are gone.** No reviewer names the inverted shadow wedges; water rose from 3.0 to 3.5, with one 4 ("clear deep-to-turquoise bands, see-through shallows ... curved coves", b3, b4, b6). Reviewer B says the see-through shallows "come close to Tchia". Two still see a straight or curved band crossing the shallows in b4.
- **Mosaic back to a unanimous 4.0**, sorting holds 4.0. Lighting dipped to 3.7 on one reviewer, who now docks it for the flat clouds.
- Cleaning, studio, truck and tools each recovered some of what review 48 took with no change to them, which supports reading review 48 as a harsh panel.
- Sea glass 3.2: the tray shards read as glass; the held clean piece is still "a jelly pillow" with a grey internal fold (06, all three), and the cloudy piece reads as a flat sheet (04, two).

**Next:**
- Shore foam: a dark navy or teal ribbon runs under the foam line in b8 (and b6); foam should be broken lace over bright shallows (all three, two as a bug).
- Clouds: smeared soft blobs; Tchia-style shaped, lit cumulus (all three).
- Island pines read as flat stacked plates; shore bushes as lumpy blobs (all three).
- Held glass: jelly pillow with an internal fold; wants the wedge taper, a clearer thin edge, a rim line (all three).
- Flat cream sand fields (b1, b2, b7, s6; all three); hard stepped wet/dry edge (b3, b6; one).
- Cleaning: grime off in patches rather than strokes, no splash, drips or "done" moment (all three).
- Rubbish unreadable at 5 m (b1; all three); the spade is a flat paddle and the sleeve a plain tube (all three).
- Studio: flat rugs, weak window light, small neglected-to-restored change (two).

**Bugs reported:**
- A dark ribbon under the shore foam (b8; all three).
- Dark hexagonal patches on the left island's slope (b8; all three, fifth review running).
- A hard-edged band or shadow across the shallows from the sea stacks (b4; all three).
- A grey fold or crease inside the held clean piece (06; all three); the held piece clips the tray rims (06; one); the brush bristles sink into the piece (05; one).
- Thin dark vertical lines under the wall shelves, and an orange pot that glows as if emissive (s7; two).
- Blotchy shading on the truck bonnet and fenders (t2; two); the mirrors appear to float (t2; one).
- Cloud edges smear into tails (b1; one); the left boardwalk edge seems to hover over the dune (s6; one).

### November 6 — round 53 and review 50 (three reviewers, averaged)

Round 53 (Round53Pass, Round51Pass offsets, TidalWater.shader, menu pass 57):
- **No shadow ribbon under the foam:** the first 2 m of water lay over the wet-sand shoulder, which takes hard sun shadows, so wherever a dune or rock shaded the beach the swash showed navy under the foam (b8), and pale where lit (b6). The seabed now reaches in to 0.1 m from the waterline, just above the shoulder's underwater strip and always below the surface, so everything seen through the water is evenly lit.
- **Lace against turquoise:** with the bed evenly lit, the white lace vanished into bare cream sand; the bed is now absorbed as if 18 cm deeper, so even the swash is faintly turquoise.

All three reviewers viewed the references (37-41 screenshots each), mostly by opening the full-size store images directly.

| Area | A | B | C | Average | Review 49 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 3.5 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.2 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.2 |
| 5.7 Cleaning | 3 | 4 | 3 | 3.3 | 3.5 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 4 | 3 | 3.3 | 3.5 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.2 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.2 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Water back to a unanimous 4.0** (first time since review 45). No reviewer names the dark ribbon; all three now describe "lacy broken foam" over turquoise bands (b6, b8). Lighting also returns to a unanimous 4.0.
- Four areas meet the bar: lighting, water, sorting, mosaic. The panel was very consistent (12 of 13 areas identical across at least two reviewers).
- The remaining water note moves inland: the wet-sand band above the waterline is "a flat taupe strip with no sheen" and a hard edge to the dry sand (b3, b6; all three).

**Next:**
- Held glass: the clean piece in 06 reads as a creased jelly lump; the cloudy piece in 04 as a thin sheet or sponge (all three).
- Vegetation: pancake-tier island pines, blob bushes, sparse grass, camouflage moss decals on the islands (all three).
- Wet sand: sheen and value variation, softer edge to the dry sand (b3, b6; all three).
- Flat cream sand fields (b1, b2, b7; all three).
- Clouds: smooth blobs, not shaped cumulus (all three).
- Cleaning: no splash, drips or runoff; weak dirty-to-clean moment (all three).
- Tools: spade a flat blade on a stick, arm a plain tube (all three); rubbish unreadable at 5 m (all three); truck bodywork lumpy and smeared (t2; all three).

**Bugs reported:**
- Dark hexagonal patches on the left island (b8; all three, sixth review running).
- The held piece cuts into the tray rims (06; all three).
- Smeared, blotchy shading on the truck bonnet and front wing (t2; all three).
- A pale, hard-edged apron or halo where the islands meet the water (b5, b3; two).
- Stray white dot-clouds (b1, t1, 03); a hanging pot floating against the sky (s5); a dark lampshade shape cutting the top of s7; a plant rope running into a shelved pot (s7).
- The jar hangs off the wash-table edge (s11, 04); the cloudy piece's rim line separates from its body (04); the spade casts an oversized shadow (02).
- The open right-hand wall of s3 ends abruptly with a floating bush; the SORT BY COLOUR sign cuts into the table slats (s3); level box-top on the mesa rock (b5, b9, 02); brown squiggles on the wet sand (b6).

### November 6 — round 54 and review 51 (three reviewers, averaged)

Round 54 (Round54Pass, round21.py, FrostedGlass.shader, menu pass 58):
- **Curved bottle wall:** both faces of every glass piece bulged outwards, so each read as a lens or pillow. glass_slab_0..5 are now curved plates: one cylindrical bend (about 10% of the width) shared by both faces, with the wedge taper and blunt rim kept. FrostedGlass measures thickness from the same bent mid-surface (`_Bend`, on the six sea-glass colours only).
- **Orientation:** the basin piece lies convex side up, so its ends stay under the water (concave side up, they broke the surface as a white blotch). The piece held at the trays shows its concave inner face, because seen convex side on the bend refracted the dark tray rim into the old crease.
- A little more sand stain on the cloudy piece, since the gentler curve shows less edge silt.

All three reviewers viewed the references (38-58 screenshots each), opening the full-size store images directly.

| Area | A | B | C | Average | Review 50 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 3.3 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **The three reviewers agree on every area**, the first fully unanimous panel. Five areas meet the bar: lighting, water, sorting, mosaic and studio (studio up from 3.3 with no change to it).
- **Sea glass holds at 3.0, but the reading moved.** Nobody calls the held piece a pillow, jelly lump or crease any more, and the tray shards are "convincing", with "good outlines, rim light and colour". All three now read the held clean piece's concave inner face as **a dish or saucer with a raised lip**, not a domed wedge, and two find it pale with little depth tint. The cloudy piece in 04 reads as stone, sandstone or sponge (all three). One reviewer sees the tray pieces as thin flat chips.
- Lesson: the held piece should show its convex outer face, as sea glass is usually seen; the crease that face showed (the dark tray rim refracted through the bend) needs a calmer background or gentler refraction instead of turning the piece over.

**Next:**
- Held glass: convex face out, deeper saturated colour, sharper glints; cloudy piece frosted glass, not stone (all three).
- Wet sand: a flat taupe band with no sheen and a hard stepped edge to the dry sand (b3, b6; all three).
- Clouds: smooth cut-out blobs (all three). Flat cream sand fields (b1, b2; all three).
- Vegetation: disc-stack island trees, blob bushes (all three).
- Cleaning: blotchy grime removal, no "done" sparkle, milky water (all three).
- Rubbish unreadable at 5 m; spade a flat plate, brush a block, arm a long tube; truck bonnet lumpy and smeared (all three).

**Bugs reported:**
- Dark hexagonal patches on the left island (b8; all three, seventh review running).
- A pale, hard-edged halo or skirt in the water around the islands' bases (b3, b4, b5; two).
- Smeared, crumpled bonnet shading (t2; all three).
- Stray cloud dots (b1, t1, 02; two); thin vertical lines under the shelves and a hanging plant's string running into a shelved pot (s7; two).
- Mosaic floor light as hard-edged decals over the rugs (08; two); the EQUIPMENT sign through the bookshelf (09); the jar hovering off the wash table and the scrub ring clipping the basin rim (s11).
- A bottle clipping the driftwood, a rock intersecting the bench end (b9); a dark line along the road edge (t1) and across the roadside sand (b7); the arm out of proportion (04-06).

### November 6 — round 55 and review 52 (three reviewers, averaged)

Round 55 (Round55Pass, StudioStation, FrostedGlass.shader, menu pass 59):
- **Convex side out:** the piece held at the sorting trays shows its convex outer face again, held up facing the camera and tilted back 35 degrees, over the light felt.
- **Gentler refraction for the held piece only** (_Refraction .025 in its property block), so the tray divider behind it no longer bends into a dark band.
- **Stronger fine pitting** on clean glass, so the glint breaks into a few small flashes.

All three reviewers viewed the references (38-42 screenshots each), opening the full-size store images directly.

| Area | A | B | C | Average | Review 51 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 3 | 3.7 | 4.0 |
| 5.11 Truck/road | 3 | 4 | 3 | 3.3 | 3.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Sea glass holds at 3.0.** Nobody reads a dish or saucer any more, and the tray shards remain "good bottle-wall pieces with rim light" (all three). The held clean piece in 06 now reads as **a thick, opaque lump**: "jelly or plastic" with stripe banding, "an ice cube", "thick opaque jade, muddy in colour". With gentler refraction and the felt behind it, it no longer looks see-through. The cloudy piece in 04 still reads as sandstone or sponge (all three).
- Studio and truck moved by a reviewer's point each way with no change to them; four areas meet the bar unanimously (lighting, water, sorting, mosaic).
- Sea glass has now held at 3.0 across five glass rounds (51-55). Each change fixed the named fault and the panel named a new one. The tray pieces pass; the held clean piece and the cloudy piece are what hold the area.

**Next:**
- Held clean piece: clearly see-through and saturated (thin edge clear, background visibly through it), not an opaque lump (all three); cloudy piece frosted glass with the hue through the frost, not stone (all three).
- Clouds: blurred inflatable blobs (all three). Flat single-tone sand fields; wet sand with no sheen and a hard step (all three).
- Vegetation: flat-disc island pines, blob bushes, sparse planting (all three).
- Cleaning: grime comes off in blotches, no splash, runoff or "done" burst (all three).
- Spade a flat plate on a stick (all three); rubbish unreadable at range (all three); truck bonnet shading (two).

**Bugs reported:**
- Hexagonal patches on the left island (b8; all three, eighth review running).
- A light-turquoise skirt or wedge with a hard edge at the island bases (b5, b3, s7; all three).
- The jar or scoop at the left edge of the wash table tilted half off the counter, and the ring handle clipping the basin rim (s11, 04, 05; all three).
- Stray cloud dots (b1, t1, b2; two); the foam band in b8 repeats as identical stamps (one).
- The held piece clips the tray rim and Amber label (06; one); floating side mirrors and a dark board behind the seat (t2; one); smeared bonnet shading (t2; two).
- Near-shore rocks cut flat at the waterline (b3, b6; one); a hard step between wet and dry sand (b3, b6; one); the buried glass a flat disc or slab on its mound (b2, 02; two); a hanging pot against open sky (s5; one); the spade shaft nearly hidden by the glove (02, 03; one).

### November 6 — round 56 and review 53 (three reviewers, averaged)

Round 56 (Round56Pass, StudioStation, FrostedGlass.shader, menu pass 60):
- **See-through held glass:** FrostedGlass gains `_Held`, set only on the piece held up at the sorting trays. It is a coloured lens there: the scene behind stays sharp and keeps its own light and shade, filtered by the glass hue (deeper where the wedge is thick) and bent more towards the curved rim, with a darker refraction band and a thin bright edge.
- **Pose:** smaller and nearer (22 cm across at 0.7 m, was 36 cm at 0.85 m), raised against the lit room rather than the plain felt, tilted back 18 degrees (was 35).
- **Glints:** no fine pitting on the held piece and tighter glint thresholds, so the flat face shows two small smooth glints instead of white splatters.

All three reviewers viewed the references (39-41 screenshots each), opening the full-size store images directly.

| Area | A | B | C | Average | Review 52 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 3 | 4 | 3.7 | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 3 | 4 | 3.7 | 3.7 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 3 | 3 | 2 | 2.7 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Sea glass holds at 3.0 for a sixth glass round.** One reviewer now calls the held clean piece "glossy and see-through", so transmission has started to land, but all three read its *form* as "a bulbous, jelly-like lump" or "a lumpy blob, not a bottle-wall shard" (06): held face-on at its convex side and smaller, it reads as a bulging pebble. The cloudy piece (04) still reads as sandstone or sponge (all three). The tray pieces pass (two of three: "good worn-shard outlines").
- Lighting (one reviewer, clouds), truck (one reviewer) and effects (one reviewer, "stills only: nothing beyond a few bubble rings") each moved a point with no change to them. Sand rose to 3.3 on one reviewer. Three areas meet the bar unanimously (water, sorting, mosaic).

**Next:**
- Held clean piece: keep the see-through lens but read as a flat bottle-wall shard, not a bulging lump (all three); a hand holding it (one). Cloudy piece frosted glass with the hue through the frost, not stone (all three).
- Clouds: smeared, puffy blobs, not shaped cumulus (all three).
- Cleaning: grime off in patches, not strokes; no splash, runoff or "done" sparkle (all three); brush a block (all three).
- Vegetation: island trees as stacked plates, blob bushes (all three). Flat sand fields (two). Rubbish unreadable at range (all three). Truck bonnet crumpled shading (two).

**Bugs reported:**
- Hexagonal patches on the left island's rock face (b8; all three, ninth review running).
- The held piece floats over the Amber tray with no hand and overlaps the Amber label (06; two).
- Stray cloud dots (b1, t1, 03; all three).
- The jar tipped at the wash-table edge (s11, 04, 05; two); the hanging plant a stack of spheres (s7; two).
- Truck: crumpled bonnet and fender shading, floating mirrors, a dark board behind the cab (t2; two).
- The glove floats over the brush, the brush head sinks into the glass (05; two); flat coloured rectangles on the floor read as stray planes, not light spill (08; one); swirling floor grain (s5, s11; one); a bottle through the driftwood log (b9; one); the buried glass a flat decal disc (b2, 02; one).

### November 6 — round 57 and review 54 (three reviewers, averaged)

Round 57 (Round57Pass, StudioStation, FrostedGlass.shader, menu pass 61):
- **Held glass shaded as a plate, not a dome:** in held mode the mesh normal separates the near-flat faces from the narrow edge wall. Through a face the scene shifts little and evenly, like a pane; the edge wall glows a deep saturated hue with a thin bright lip, and the face carries one straight soft sheen. The view-based dark rim band that rounded it is gone.
- **Pose:** tilted back 30 degrees (was 18) so the edge wall shows, and raised against the room, clear of the Amber label.

All three reviewers viewed the references (39-40 screenshots each), opening the full-size store images directly.

| Area | A | B | C | Average | Review 53 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 3 | 4 | 3.7 | 3.7 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 2 | 2 | 2 | 2.0 (stills only) | 2.7 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Sea glass holds at 3.0 for a seventh glass round, and the held piece overshot.** Nobody reads a lump or jelly any more; all three now read the held clean piece in 06 as **too thin**: "a thin curved sheet, like cellophane", "a thin curved sheet or acetate", "a thin curved acrylic sheet". The narrow edge band does not carry the wedge's thickness at this size, and the visible bottle-wall curve reads as a bent film. All three also call the tray pieces thin chips or flakes rather than the 30%-to-12% wedge. The cloudy piece (04) still reads as sandy stone (all three).
- **Effects fell to 2.0 (all three, stills only):** a few outlined bubbles in 05, no splash, runoff or "clean" sparkle in 06, no particles at the 08 reveal. Nothing changed there; the panel is now unanimous.
- Lighting returned to 4.0. Four areas meet the bar unanimously (lighting, water, sorting, mosaic).

**Next:**
- Held clean piece: between the two last rounds. Keep the see-through faces and the flat plate, but show real thickness: a visibly thick, bright-and-deep edge on the wedge's thick side and less visible curl (all three). Cloudy piece frosted glass with the hue through the frost, not stone (all three). Tray pieces showing their wedge thickness (all three).
- Effects and cleaning feedback: splash, drips, stroke-following removal, a "clean" sparkle in 06, particles at the reveal (all three).
- Clouds as shaped cumulus (all three); island trees as layered foliage, not disc stacks (all three); flat sand foregrounds (all three); rubbish unreadable at range (all three).
- First-person sleeve a long plain tube, block brush, flat spade (all three); truck bonnet lumpy and wheels plain (all three).

**Bugs reported:**
- Hexagonal patches on the left island's rock face (b8; all three, tenth review running).
- A hard-edged turquoise skirt around the island bases (b3, b5, 02; two).
- Truck: crumpled bonnet and fender shading (t2; two), floating mirrors and a board behind the seat (t2; two).
- Stray cloud dots (b1, b6, t1, 03; one); the jar tipped at the wash-table edge (s11, 04, 05; one); the hanging plant stacked spheres or glowing pot (s7; two).
- The sleeve a long tube from the screen edge, the wrist clipping the brush handle (04-06; two); the held glass floating with no hand (06; one).
- Stretched, swirling floor grain (s1, s5, 07b; one); the EQUIPMENT sign clipping the bookshelf or hanging unsupported (09, s4; one); a translucent blue slab by the cooler reads as a stray shape (02; one).

### November 6 — round 58 and review 55 (three reviewers, averaged)

Round 58 (Round58Pass, round21.py, FrostedGlass.shader, menu pass 62):
- **Thicker sea glass:** glass_slab_0..5 measure about 30% of the width at the thick edge and 11% at the thin edge (they measured about 20% and 8%), the 5.6 wedge, with a gentler bottle-wall curl (BEND and _Bend -.06, was -.1). Round37Pass re-seated all 176 tray spares and beach finds.
- **Held piece:** the rounded rim counts as edge wall, so the thick edge shows as a deep-coloured band; the hue runs from nearly clear at the thin edge to deep at the thick one.

All three reviewers viewed the references (37-41 screenshots each), opening the full-size store images directly.

| Area | A | B | C | Average | Review 54 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 2.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **The three reviewers agree on every score.** Five areas now meet the bar unanimously (lighting, water, sorting, mosaic, studio); the other eight sit at 3.0.
- **Sea glass holds at 3.0 for an eighth glass round.** The thicker tray shards pass with all three ("glossy, saturated, shard-like", "worn glass with good hue and glints"); nobody calls them thin chips any more. The held clean piece in 06 is no longer a thin sheet, but now reads as "a puffy, pillow-like blister", "a hollow, curled film" or "a jelly/plastic blob"; two say the room refracted inside it (the lamp and table) reads as a jumbled view or a hollow shell. The cloudy piece in 04 still reads as sandy stone (all three).
- Effects rose back to 3.0 and studio to 4.0 with no change to them.
- Across rounds 55-58 the held piece has read as a dish, a lump, a sheet and now a pillow or shell: each fix moved the fault rather than removing it. The single large piece floating with no hand and a magnified refracted room behind it is itself hard to read; the tray pieces, small and at rest, pass.

**Next:**
- Held clean piece: reconsider the 06 framing rather than the shader again: smaller, resting in a gloved palm or on the felt, with a calmer background through it (all three named the piece; two the refracted room). Cloudy piece frosted glass with the hue through the frost, not stone (all three).
- Cleaning: grime off along brush strokes, not blotches; more suds, splash and runoff (all three).
- Island trees as layered foliage, not flat disc canopies (all three); clouds as shaped cumulus (all three); flat cream sand fields (all three); rubbish unreadable at range (all three).
- Spade a flat shield on a stick (all three); truck bonnet crumpled shading (all three).

**Bugs reported:**
- Hexagonal patches on the left island's face (b8; all three, eleventh review running).
- The held piece hovering over the trays with a jumbled or hollow view of the lamp and table inside it (06; two).
- The jar half off the wash-table edge (04, 05, s11; all three).
- Truck: crumpled bonnet and fender shading, detached side mirrors (t2; two).
- Stray cloud dots (b1, b6, t1, 03; two); a hard-edged turquoise skirt or water seam around the islands (b3, b5; two).
- A stepped wet/dry sand edge (b3, b6; one); a thin diagonal seam across the sand and road edge (b7, t1; one); hanging planters outside the roof edge, a bush floating at roof height, an over-bright orange pot (s1, s3, s5, s7; two).

### November 6 — round 59 and review 56 (three reviewers, averaged)

Round 59 (Round59Pass, palm_glove.py, StudioStation, menu pass 63):
- **Held glass in an open palm:** at the sorting trays the clean piece rests convex side up, about 6.5 cm across, in an open gloved palm held low in front of the player (StudioStation.HoldOnPalm), with the canvas glove behind it instead of the magnified room.
- **palm_glove:** the gardening glove from glove.py opened palm up, fused into one surface the same way (canvas glove, coral knit cuff, blue sleeve).

All three reviewers viewed the references (38-39 screenshots each), opening the full-size store images directly. The brief's frame list now says 06 shows the clean piece in the palm at the trays (it said "in the basin"), and lists the palm under tools.

| Area | A | B | C | Average | Review 55 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 3 | 3 | 3.3 | 4.0 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 2 | 3 | 3 | 2.7 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Sea glass holds at 3.0, but the reading of the held piece has narrowed to one fault.** Nobody reads a pillow, shell, sheet, lump or jelly any more. All three now name only its outline: "a shield with a pointed tip", which breaks 5.6's "no pointed tips"; one adds that its edge looks like a uniform extrusion. The tray pieces pass with all three ("glossy, saturated, broad shards ... meets the fixed target"). The cloudy basin piece still reads as sandy rock or sponge (all three), and one reads the half-clean piece as cyan jelly (05).
- **The open palm reads as "a flat paddle", "a stiff, flat mitten"** (all three), with little finger form; one sees a red sliver at the cut-off wrist (06).
- Studio fell to 3.3 (two reviewers: empty floor, no window-light shapes on it) and effects to 2.7 (one), with no change to either. Four areas meet the bar unanimously (lighting, water, sorting, mosaic).

**Next:**
- Held piece: round the pointed tip of the shield-shaped outline (glass_slab with the tip; all three). Palm: curl the fingers and cup the palm so it reads as a hand, not a paddle (all three).
- Cloudy piece frosted glass with the hue through the frost, not stone (all three).
- Cleaning: foam that follows the brush, splash and runoff, a clear "done" moment (all three); milky flat basin water (two).
- Clouds as shaped cumulus (all three); island trees as layered foliage (all three); flat sand fields and a hard wet/dry step with no sheen (all three); rubbish unreadable at range (all three); spade a flat plate (two); truck bonnet blotchy (all three).
- Studio: empty floor, no window-light pools (two).

**Bugs reported:**
- Dark hexagonal patches on the left island (b8; all three, twelfth review running).
- Stray cloud dots (b1, t1, 03; two); a stepped wet/dry sand seam (b3, b6; one).
- The truck bonnet blotchy or smeared, possibly bad normals (t2; all three).
- The jar tipped off the wash-board edge (s11, 04, 05; one); the orange pot above the window self-lit (s7; one); a hanging-plant cord into a standing pot (s7; one); the EQUIPMENT sign into the bookshelf (s4; one).
- A red sliver at the cut-off wrist (06; one); a buried glass piece a flat green plate on the sand (b2; one); dark potted plants on the deck (s6; one); the basin in 09 reads empty (one); a box-topped outcrop (b5; one); a dark rock in a different shading style (02; one).

### November 7 — round 60 and review 57 (three reviewers, averaged)

Round 60 (Round60Pass, round21.py, palm_glove.py, menu pass 64):
- **Rounded glass corners:** each glass_slab outline radius is averaged over about +/-24 degrees around each angle, so every convex corner rounds over (the held piece's shield tip included). Round37Pass re-seated the pieces.
- **Cupped palm:** fingers curl up from the knuckles and fan a little, the thumb curls in, the little-finger edge and thumb mound are raised; the held piece sits a few millimetres higher.

All three reviewers viewed the references (41-49 screenshots each), opening the full-size store images directly.

| Area | A | B | C | Average | Review 56 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 3 | 4 | 3.7 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 4 | 3.3 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 3.3 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 2.7 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **The pointed tip is gone, but the rounding went too far.** Nobody names a tip any more, and one reviewer calls the held piece "the best: a glossy rounded triangle" (06). But the averaged outline also softened the straight broken edges: "most read as rounded jelly lozenges, with no straight broken edge or wedge" (A), "tray pieces are uniform lozenges" (B). The basin piece (04, 05), large and domed under the frost, now draws the glass criticism: "a big domed pebble, like a stone or bread roll", "a large domed pillow ... more like jelly" (all three). Sea glass holds at 3.0.
- **The palm still reads as stiff:** "a flat paddle with stiff, primitive-like fingers", "a stiff, rubbery spread hand", "a stiff, flat cut-out hand" (all three).
- Studio returned to 4.0, sand rose to 3.7 and cleaning to 3.3 (one reviewer each), effects to 3.0, all with no change to them. Five areas meet the bar unanimously (lighting, water, sorting, mosaic, studio).

**Next:**
- Glass outline: round only the tips. A narrower averaging window (about +/-10 degrees) or one applied only where the outline turns sharply would keep the straight broken edges (all three named their absence).
- Basin piece: the large cloudy piece reads as a domed stone, pillow or bread roll (all three): flatter in section under the frost, a visible straight broken edge, and the hue through the frost.
- Palm: fingers that bend at the joints and a relaxed curl, not a spread cut-out (all three).
- Clouds as shaped cumulus (all three); island trees as layered foliage (all three); rubbish and buried glass too small or flat decals (all three); spade a flat plate (all three); truck bonnet smeared, boxy rear (all three).

**Bugs reported:**
- Dark hexagonal patches on the left island (b8, faint in b3 and b5; all three, thirteenth review running).
- A hard-edged shallow-water shelf or halo around the islands (b3, b5; two).
- The jar at the wash-table edge overhanging or clipping (s11, 04, 05; two); a bottle clipping the driftwood log (b9; one).
- Stray cloud spheres (b1, t1, 03; one); a dark board behind the truck seat (t2; one); a smeared bonnet texture (t2; one).
- A straight seam through the foreground sand by the road and flat hard-edged sand drifts (b7, t2; one); a hard dark line along the left road edge (t1, 03; one).
- A red artefact at the bottom of the wrist cuff or the sleeve cut off abruptly (06; two); a small dark teal square on the tray table's front rail (s10, 06; one); a hanging pot that seems to float (s5; one); the reveal's floor light a hard-edged shape unlike the window (08; one).

### November 7 — round 61 and review 58 (three reviewers, averaged)

Round 61 (Round61Pass, round21.py, FrostedGlass.shader, StudioStation, menu pass 65):
- **Straight broken edges:** the outline is averaged over about +/-10 degrees (was 24), so only sharp tips round over; every glass_slab now has one straight broken edge (two had none).
- **Cloudy piece shaded flat:** the frosted state is shaded by face and edge wall instead of view angle (no view-based dark rim). The dried sand lies in distinct patches with a smoother milky frost in the glass colour between them, and the basin piece is a fifth smaller.

All three reviewers viewed the references (38-41 screenshots each), opening the full-size store images directly.

| Area | A | B | C | Average | Review 57 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 4 | 3 | 3.3 | 3.7 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 4 | 3 | 3.3 | 4.0 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 3 | 2 | 2 | 2.3 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **The cloudy piece no longer reads as a dome or bread roll, but now as camouflage.** The distinct sand patches over the tinted frost read as "opaque grey stone with teal camouflage patches", "a grey camouflage blotch over turquoise", "grey paint patches, not frosting" (all three). One sees hard pixel noise at the patch edges (04).
- **The held piece** reads as glossy glass to one, but two see "a near-triangle pick shape" with point-like corners (06): the narrower smoothing brought the corners back on that piece.
- **Tray pieces** are "close to 4" for one; the others see "flat lozenges with little wedge thickness" or "gummy acrylic with uniform thickness" (s10, 07).
- Sea glass holds at 3.0 for an eleventh glass round (rounds 51-61). Effects fell to 2.3 and studio to 3.3, sand to 3.3 and cleaning to 3.0, with no change to any of them. Four areas meet the bar unanimously (lighting, water, sorting, mosaic).

**Next:**
- Cloudy piece: an even translucent frost with the glass hue glowing through it and only light sand at the edges, not opaque grey patches on bright teal (all three).
- Held piece: round the remaining near-triangle corners on that one outline (two).
- Effects and cleaning feedback: splash, drips, a sparkle or burst at the clean moment, stroke-following grime removal (all three).
- Clouds as shaped cumulus (all three); island trees and bushes as layered foliage (all three); wet sand with sheen and a soft edge (all three); rubbish unreadable at range (all three); truck slab panels and crumpled bonnet (all three); stiff palm, block brush, plate spade, tube sleeve (all three).
- After eleven glass rounds at 3.0, the glass score has stopped responding to single fixes; the other 3.0 areas (vegetation, clouds, effects, tools, truck) each have one clear, unanimous fault.

**Bugs reported:**
- Dark hexagonal patches on the left island (b8; all three, fourteenth review running).
- Water-plane seams or a hard step at the island bases (b3, b5; one); a hard wet/dry sand edge (b3, b6; two).
- Stray cloud dots (b1, b6, t1; two); a long straight seam across the sand at the road shoulder and a line along the road edge (b7, t1; one).
- The jar at the wash-table edge cut by the table (s11, 04, 05; one); a teal handle through the basin rim (s11; one); the sleeve passing through the basin rim (04, 05; one).
- Truck: a dark board behind the seat (t2; one), melted bonnet and floating mirrors (t2; one).
- A bottle through the driftwood and a bag into the bench rail (b9; two); the half-buried glass a flat decal (b2; one); the hanging pot outside the roof line (s5; one); the hanging-plant cord into a standing pot and a stack-of-spheres plant (s7; two).

### November 7 — round 62 and review 59 (three reviewers, averaged)

Round 62 (PaintedSky.shader only, no editor pass):
- **Built cumulus instead of a noise field.** The clouds were a thresholded noise field, which drew blobs, smeared tails and lone dots. Each cloud is now a designed cumulus in one of two rings of cells around the horizon: a flat, slightly rounded base, a skirt of uneven lobes, a leaning build-up over the middle and small lobes along the top contour, fused into one outline.
- **Lobes shaded as volumes:** each lobe is lit as a rounded form against the sun, with normals blended smoothly across joins and soft creases between lobes: cream tops, a warm rose middle and a lilac underside, gold rims and light through the thin tops when backlit. The far ring sits lower, smaller and slightly hazed.

All three reviewers viewed the references (40-57 screenshots each), as full-size store images or strip images in their own tab.

| Area | A | B | C | Average | Review 58 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 4 | 3 | 3.7 | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 4 | 4 | 3.7 | 3.3 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 2.3 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **The clouds are off the list of main faults.** For the first time since review 39, nobody calls them blobs, smears or cut-outs, and nobody reports stray cloud dots. All three now read "shaped cumulus" (lighting 4.0 from all three). What is left is finish: "slightly clay-like with the same lilac underside everywhere" (A), "slightly puffy clay" (C), and "all similar sizes, matching marshmallows" (B, across b1, b2 and b7).
- Sand rose to 3.7, studio to 3.7 and effects back to 3.0, with no change to any of them. Four areas still meet the bar unanimously (lighting, water, sorting, mosaic).
- Sea glass holds at 3.0: the cloudy piece is still "camouflage-painted stone" (04) and the palm piece "a gummy guitar pick" (06; all three). The tray pieces are "close to 4" (A).

**Next:**
- Clouds (finish only, one each): more variety of size and spacing, from small fair-weather puffs to one or two large banks; less uniform lilac on the undersides, with warmer bounce light toward the sun; a little less smooth clay.
- Cloudy piece: an even translucent frost with the hue under it, not camouflage patches (all three). Palm piece: round the near-triangle outline (all three).
- Island trees and bushes as layered foliage (all three); wet sand with sheen and a soft edge (two); cleaning feedback with strokes, runoff and a clean-moment sparkle (all three); truck bonnet and flat panels (all three); the stiff open palm and the plain sleeve (all three); rubbish too small at range (all three).

**Bugs reported:**
- Dark hexagonal patches on the left island (b8; all three, fifteenth review running).
- A hard wet/dry sand seam (b3, b6; one); a turquoise wedge at the island and headland bases (b3, b5; one); a long straight line across the sand beside the road (b7; two); repeated foam stamps (b8; one).
- The jar at the wash-table edge (s11, 04, 05; two); the open palm with no sleeve or unshaded (06; two).
- Truck: blotchy smudges on the bonnet and arches (t2; two).
- Driftwood with a flat end cap (b9; one); the capsule-like striped rock (b1; one); the hanging-plant rope into a standing pot (s7; one); the 08 floor light as hard rectangles (08; one); dark front pots (s6; one).

### November 7 — round 63 and review 60 (three reviewers, averaged)

Round 63 (Round63Pass, round25.py, Island trees material, menu pass 66):
- **Layered island canopies:** the faceted umbrella pines are replaced by groves of broadleaf crowns, built in metres. Each crown has a broad lower tier and a rounded upper tier of lobes, with undergrowth at the foot. Each grove is fused into one crinkled, smooth-shaded canopy over the hilltop, sitting low on short trunks, plus small clumps on the upper ledges.
- **Island trees material:** a fresher green body and a softer, less mustard sunlit top that starts lower and blends at a lower amount.
- **No talus boulders:** the small boulders at the island waterlines are gone; at range they were the dark "hexagon patches" on the b8 cliff, and smoothed they read as bubbles.

All three reviewers viewed the references (about 52-54 screenshots each), as screenshot grids in their own tab.

| Area | A | B | C | Average | Review 59 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.7 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 3 | 4 | 3.7 | 3.7 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **The island trees are no longer plates.** Nobody calls them flat plates, umbrella layers or disc stacks any more: "the layered island canopies read better" (A), "an improvement" (B), "layered" (C). What is left: "uniform olive blob clusters with weak top light" (B) and "repeated clump stamps" (C). Vegetation holds at 3.0 because all three now name the beach bushes as lumpy single blobs (b5, b7, 02), with sparse, repeated grass and palms, and one names the boxy flat-topped rock (b5, b9).
- **The b8 hexagon patches are gone.** For the first time in fifteen reviews, nobody reports them.
- Sand fell to 3.0 with no change to it (large flat cream foregrounds, all three). Four areas still meet the bar unanimously (lighting, water, sorting, mosaic).

**Next:**
- Beach bushes as layered, lit foliage instead of single lumpy blobs; more varied canopy clumps and a stronger lit top on the island trees (all three).
- Cloudy piece frosted glass with the hue under it, not camouflage stone (all three); palm piece without pointed tips (one).
- Large flat sand foregrounds with low-frequency tone and height; wet sand with sheen and a soft edge (all three).
- Cleaning strokes, splash and a clean-moment sparkle (all three); truck slab panels and bonnet (all three); stiff flat palm and tube sleeve (all three); clouds of one size at one height (two).

**Bugs reported:**
- A pale turquoise shelf with a hard edge around the island bases (b5, 02; two); a hard wet/dry sand ledge (b3, b6; one); brown squiggle decals on the wet sand (b6; one); flat green caps on the sea stacks (b4; one).
- Truck: the dark board behind the seat (t2; two), detached-looking mirrors (t2; one) and a blotchy, lumpy bonnet (t2; two); a hard dark kerb line along the road (t1, b7; one).
- A dark notch in the tray table's front edge (06, s10; one); the jar at the wash-table edge (s11; one); the brush bristles hovering above the glass (04; one); hard pixel-noise grime edges (04; one).
- Glass on the sand as flat decals (b2, 02; one); stacked-sphere hanging plant (s7; one); the 08 floor light as hard shapes (08; one); a flat diamond banner (s3; one); the deck ending abruptly (s3, s4; one).

### November 7 — round 64 and review 61 (three reviewers, averaged)

Round 64 (Round64Pass, round64.py, menu pass 67):
- **Layered beach bushes in three variants:** the one decimated, faceted bush (with leaf cards at random angles that read as dark specks) is replaced by a broad mound, a taller rounded bush and a two-lobed spreading one. Each is a fused, tiered inner mass shingled with broad cupped leaves laid along the surface and tilted out, so the outline breaks into leaf tips.
- **Volume normals:** every vertex carries the bush's rounded volume normal, so each bush shades as one soft lit form, sunlit on top and deep green underneath.
- **Variety:** the three variants are dealt out across the 292 bushes in the scene by position. Frame time 11.7 ms average (p95 14.2 ms).

All three reviewers viewed the references (39-49 screenshots each), as full-size store images or strip images in their own tab.

| Area | A | B | C | Average | Review 60 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 4 | 3.3 | 3.0 |
| 5.4 Props/vegetation | 3 | 4 | 4 | 3.7 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 4 | 4 | 3.7 | 3.7 |
| 5.11 Truck/road | 4 | 4 | 4 | **4.0** | 3.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Vegetation rose to 3.7, its best in many reviews, with two 4s.** Nobody calls the bushes blobs or balls in b5 or 02 any more: "the bushes in b5 and 02 are good" (C), "layered island canopies, palms and bushes read well" (B), "the b5 bush is good" (A). The holdout (A) still sees "yellow blobs" for the small dune shrubs at range in b7 and b2, and sparse spike-tuft grass; one other names sparse dune planting with stray "v" sprigs (b2, b7).
- **Truck and road reached 4 unanimously** with no change to them: "rounded hood, scalloped canopy, loaded cargo" (B), "charming, cargo that visibly fills" (A). Five areas now meet the bar unanimously (lighting, water, sorting, mosaic, truck).
- Sand rose to 3.3 with no change. Sea glass holds at 3.0: the cloudy piece is still grey concrete or camouflage patches (04) and the palm piece a guitar pick (06; all three).

**Next:**
- Cloudy piece frosted glass with the hue under it (all three); palm piece without the near-triangle outline (all three).
- Cleaning strokes, splash, drips and a clean-moment sparkle (all three); the tube sleeve and stiff, mannequin-like open palm (all three).
- Wet sand: a soft blend instead of the hard stepped ledge (two); flat foreground sand fields (two); rubbish too small at range and the glass hint a flat disc (all three).
- Dune planting denser, with the small shrubs at range reading as foliage (one); clouds of one size and a clay look (all three, finish only).

**Bugs reported:**
- A hard-edged lighter shallow-water plate or seam around the island bases (b3, b5, b8, 02; two); constant-width foam rings around the sea stacks (b4, b8; two).
- A long straight seam across the b7 foreground sand (two); a hard dark line along the road edge (t1, 03; one); a hard wet/dry ledge (b3, b6; one).
- Truck: detached-looking mirrors and the board behind the seat (t2; one); a smeared bonnet texture (t2; one).
- The jar at the wash-table edge (s11, 04, 05; two); hard pixel-noise grime edges (04; one).
- The glass hint and a white ring lying flat like decals (b2; two); stray "v" sprigs on the dunes (b2, b7; one); a glowing hanging-plant pot (s7; one); mosaic labels hidden by props (s2; one).

### November 7 — round 65 and review 62 (three reviewers, averaged)

Round 65 (StudioStation, EquipmentVisuals, Feedback, FrostedGlass.shader, PlayerVerification; no editor pass):
- **Brush strokes:** the brush works in parallel side-to-side passes from one end of the piece to the other (it traced a Lissajous figure whose crossing sweeps left a patchwork), timed by a per-piece brushing clock so the brush and the cleared path agree. The cleared path carries bristle streaks along the stroke, and the shader's stroke edge is less noisy.
- **Splash:** water flicks off the brush as falling, stretched droplets thrown in the stroke direction.
- **The colour-revealed moment:** a ring of glints in the glass colour, a bright star, a crown of droplets and a short inner glow as the piece lifts. The sparkles now draw after the basin water, which had been covering them.
- **New frame 05b-colour-revealed:** the verification run captures that moment; no still showed it before. The reviewer brief lists ten build frames from this review on.

All three reviewers viewed the references (38-41 screenshots each), as full-size store images in their own tab. Reviewer C scored in half points; the averages use those scores as given.

| Area | A | B | C | Average | Review 61 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 3 | 3.5 | 3.5 | 3.3 |
| 5.4 Props/vegetation | 4 | 4 | 3.5 | 3.8 | 3.7 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3.5 | 3.2 | 3.0 |
| 5.7 Cleaning | 4 | 3 | 3.5 | 3.5 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 3 | 3.5 | 3.5 | 3.7 |
| 5.11 Truck/road | 4 | 3 | 3.5 | 3.5 | 4.0 |
| 5.12 Tools | 3 | 3 | 3.5 | 3.2 | 3.0 |
| 5.13 Effects | 3 | 3 | 3.5 | 3.2 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Cleaning rose to 3.5, its first move since review 57, with one 4.** All three now see "a clear done moment" in 05b ("the sparkle lift-out", "star sparkles"). The grime itself still reads as "noisy blotches" or "dithered noise", not strokes along the brush (05; all three), and the foam and splash in 05 are small (all three).
- **Effects rose to 3.2** on the 05b sparkles. But the falling droplets under the lifted piece read as "thin vertical blue streaks", "stray line particles" (05b; two): stretched along a vertical fall seen from above, they read as lines, not drops.
- **The 05b view of the clean piece** adds a new reading: lifted and lit from inside, it is "a domed, jelly-like pillow" or "uniform opaque cyan, reads as jelly or soap" (05b; all three). Sea glass rose to 3.2 on one 3.5.
- Truck fell back to 3.5 and studio to 3.5 with no change to either; sand and vegetation edged up. Four areas meet the bar unanimously (lighting, water, sorting, mosaic).

**Next:**
- Cleaning: the crust edge still reads as noise at the stroke front; the grime should break away in clean, brush-shaped swaths with a visible wet trail, and fuller suds at the brush (all three).
- 05b: droplets as round drops and a splash crown, not vertical streaks (two); the lifted piece keeps its glass read (no inner glow that flattens it into opaque cyan; all three).
- Cloudy piece frost, not camouflage (two); palm piece shape (two); the tube forearm and mitten palm (all three); wet sand sheen and soft edge (all three); rubbish too small at range (all three).

**Bugs reported:**
- Vertical blue streaks under the lifted piece (05b; two).
- A thin dark seam across the b7 foreground sand (two); a hard line along the left road edge (t1; one); a stepped wet/dry boundary (b3, b6; one).
- A green-handled object clipping the basin rim (s11; two); the brush bristles hovering beside the glass (04; one).
- A bottle intersecting the driftwood (b9; two); buried glass as flat decals (b2, 02; one); a grey hook-shaped object floating beside the spade (02; one).
- Crumpled bonnet shading (t2; one); dark blob plants in the front pots (s6; one); the s3 wall ending against the sky (s3; one); jars cut by the near plane (s5; one); a floating canopy read behind the bookshelf (s4; one); the studio roof edge a tiled slab from above (b4; one).

### November 7 — round 66 and review 63 (three reviewers, averaged)

Round 66 (FrostedGlass.shader, Feedback, StudioStation; no editor pass):
- **Smooth crust:** the frost and sand grain were sampled up to ~360 cycles across the piece, finer than a pixel at the basin, so they aliased into speckle over the crust and along every stroke front. The grain, pits and salt glints are now coarser and soft-edged.
- **Fuller suds:** two larger, longer-lived clumps per emission. **Round droplets** instead of stretched streaks. **A faint reveal glow**, so the glints carry the moment.
- **Reliable reveal glints:** they have their own world-space system, and the effect emitters always simulate and stay playing. On the shared pool (which the recycling sparkles could take over), or stopped with stale bounds, the glints and drops sometimes never drew.

All three reviewers viewed the references (38-39 screenshots each), as full-size store images in their own tab. The brief now asks for whole-number scores.

| Area | A | B | C | Average | Review 62 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 4 | 4 | **4.0** | 3.5 |
| 5.4 Props/vegetation | 3 | 4 | 4 | 3.7 | 3.8 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.2 |
| 5.7 Cleaning | 4 | 4 | 4 | **4.0** | 3.5 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 4 | 4 | 3.7 | 3.5 |
| 5.11 Truck/road | 3 | 4 | 4 | 3.7 | 3.5 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.2 |
| 5.13 Effects | 4 | 4 | 3 | 3.7 (stills only) | 3.2 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Cleaning reaches 4 from all three for the first time.** "The enamel basin, shaped tap, caustics, suds and sparkle moment are all there" (A); "suds where the brush works, a sparkle at the clean moment" (B); "a clear finished moment" (C). Nobody reports pixel noise, dithering or stray streaks any more. What is left is PowerWash's stronger dirty-to-clean contrast along the strokes (all three).
- **Effects rose to 3.7 with two 4s** on the suds clusters, the 05b sparkles and the floor light; one finds the sparkles generic four-point stars.
- **Sand reached 4 from all three** with no change to it. Six areas now meet the bar unanimously (lighting, water, sand, cleaning, sorting, mosaic); three hold at 3.0 (rubbish, sea glass, tools).
- Sea glass holds at 3.0: the tray pieces and the palm piece pass (all three), but the cloudy piece is opaque grey camouflage patches (04) and the lifted piece in 05b a jelly pillow (all three).

**Next:**
- Sea glass: the cloudy piece frosted and translucent with the hue through it (all three); the lifted piece in 05b as a wedge with broken glints, not a jelly pillow (all three).
- Tools: the long tube sleeve (all three), the stiff mannequin palm (two), the flat shield spade (two).
- Rubbish too small and too clean to read at 5 m (all three).
- Wet sand without sheen and a hard step (two); flat rugs and repeated vases in the studio (two); a blotchy bonnet and blocky truck body (two); one puffy cloud type (all three).

**Bugs reported:**
- A hard-edged pale shelf or seam around the island bases and between the water bands (b3, b4, b5; all three).
- Driftwood twigs on the wet sand as flat squiggle glyphs (b6; two); repeating foam glyphs (b6, b8; one); a hard wet/dry step (b3, b6; one); the ocean curving up at the b4 horizon (one).
- The sleeve crossing the basin rim (04, 05; two); a handle clipping the rim (s11; one); the jar overhanging the counter (s11, 04; one).
- Truck: blotchy bonnet shading (t2; two), floating mirrors and the board behind the seat (t2; one).
- A flat white ring or disc decal (b1, b2; two); the bottle and driftwood intersecting (b9; one).

### November 7 — round 67 and review 64 (three reviewers, averaged)

Round 67 (Round67Pass, glove.py, palm_glove.py, sleeve_shape.py, menu pass 68):
- **Shaped sleeve:** both first-person arms share one work-shirt sleeve with a forearm swell, uneven compression folds above the cuff, drape creases, a pushed-up gathered band and an underside seam, longer so its end leaves the frame. The brushing arm rises at 13 degrees (was 8) to clear the basin rim.
- **Relaxed palm:** fingers built joint by joint (tapering, with knuckles, bending at three joints), close together and curled up around the glass; the thumb curls in beside them; the coral cuff sits nearer the hand.

All three reviewers viewed the references (38-58 screenshots each), as full-size store images in their own tab or the store viewer.

| Area | A | B | C | Average | Review 63 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 3 | 4 | 3.7 | 4.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.7 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.11 Truck/road | 3 | 4 | 3 | 3.3 | 3.7 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 4 | 3 | 3 | 3.3 (stills only) | 3.7 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **The round missed: tools hold at 3.0 and the arm reads the same or worse.** All three still read the sleeve as a plain tube: "a long straight tube" (A), "a huge plain tube stretching across half the screen" (B), "a thick plain tube" (C). The folds and forearm swell do not read at basin distance under the flat denim shading, and the longer, higher arm now crosses more of the frame: "the arm length reads wrong" (A), "an implausible length" (B). The curled palm reads as "a flat mitten" (A), "flat, stiff, featureless" (B), "a flat, stiff, skin-like mitt" (C): from the camera above, the finger curl foreshortens away and the single tan colour hides the joints.
- **Lesson:** the arm's problem is how much of the frame it fills and its flat single-tone shading, not its silhouette. A shorter visible forearm (the brush held nearer the camera, or the sleeve ending at the elbow out of frame sooner) and a cloth material with weave, seams and shading would do more than more geometry.
- Studio reached 4 from all three with no change, so six areas meet the bar unanimously (lighting, water, cleaning, sorting, mosaic, studio). Vegetation fell back to 3.0, sand to 3.7, truck to 3.3 and effects to 3.3, all with no change.

**Next:**
- Tools: shorten how much sleeve is visible in 04 and 05 and give the sleeve and glove a fabric material (weave, seams, darker folds); show the palm's finger curl from the camera's side and break the single tan with seams or a darker palm patch (all three); the flat shield spade (one).
- Sea glass: the cloudy piece frosted, not opaque grey blotches (all three); the 05b and 06 pieces translucent shards, not jelly or gems (all three; one sees a pointed tip in 06).
- Vegetation: blob bushes, sparse dune planting, island canopies as blob clusters (all three). Rubbish too small at 5 m (all three).

**Bugs reported:**
- The sleeve running over the bowl rim at an implausible length (04, 05; two).
- Foam at the sea-stack bases as hard straight bands (b4, b5, b8; all three); a hard-edged shallow-water ring at the island bases (b3, b5; one); a stepped wet/dry edge (b3, b6; two); a straight seam across the b7 verge (one); a dark strip along the road edge (t1; one).
- The jar at the wash-table edge, tilted, overhanging or cut by the near plane (s11, 04, 05; all three).
- A bottle in the driftwood (b9; two); buried glass as flat discs (b2, 02; one); lumpy bonnet shading (t2; two); the EQUIPMENT sign clipping the bookshelf (s4; one); an unlit white slab (s3; one); hard floor-light rectangles (08; one).

### November 7 — round 68 and review 65 (three reviewers, averaged)

Round 68 (Round68Pass, Cloth.shader, glove.py, palm_glove.py, EquipmentVisuals, StudioStation, menu pass 69):
- **Cloth materials:** a stepped light-to-shade ramp and a woven surface in the arm's own space (canvas, denim twill, knit rib, rubber) on every glove and the held palm.
- **Rubber-dipped gloves:** a deep navy coating on the palm and fingers of the brushing glove and over the whole visible open hand.
- **Arm aim:** the brush turns with the forearm and the arm pitches toward a shoulder below and right of the camera, so the sleeve comes up from the bottom of the frame, foreshortened. The held palm is turned so the finger curl shows from the side.

All three reviewers viewed the references (37-40 screenshots each), as full-size store images in their own tab or the store's showcase.

| Area | A | B | C | Average | Review 64 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.7 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 3 | 3 | 3 | 3.0 | 4.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.11 Truck/road | 4 | 3 | 3 | 3.3 | 3.3 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.3 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **The round regressed cleaning from 4.0 to 3.0 (all three), and tools hold at 3.0.** Coming up from the bottom of the frame, the foreshortened forearm is now closer to the camera and larger: "a huge sleeve tube dominates the frame" (A), "a very long tube that fills a third of the frame" (B), "the ribbed sleeve fills a third of the frame" (C), "at an implausible scale and angle" (A). It also hides the brush, which now reads as "a flat block with its bristles hidden" (B) or "a block" (A), so the scrub reads less. The twill still aliases into moiré stripes on the near sleeve (C).
- **The open palm (06) improved for some:** "acceptable" (B), "designed" (C); one still sees sausage fingers (A). Nobody calls it a flat mitten or mannequin any more.
- **Lesson:** turning the forearm toward the viewer makes it bigger, not smaller. At the basin the arm should stay to the side, as it was before round 68, but thinner and shorter on screen: a narrower sleeve, the brush held so the bristles face the camera, and the arm leaving the frame edge sooner.
- Five areas meet the bar unanimously (lighting, water, sorting, mosaic, studio). Sand fell to 3.0 and effects to 3.0 with no change.

**Next:**
- Revert the forearm aim at the basin to the side approach, with a slimmer, shorter visible sleeve and the bristles in view; lose the moiré (all three).
- Sea glass: the cloudy piece frosted, not opaque paint patches (all three); the basin piece a wedge, not a jelly blob (all three).
- Grime clearing in brush-shaped strokes (all three), with drips (one). Sand foregrounds and the hard wet/dry edge (all three). Vegetation sparse (all three); slab and extruded rocks (one). Rubbish too small (all three). Spade a faceted plate (one).

**Bugs reported:**
- A hard-edged shallow-water skirt or ridge around the left island (b3, b5; all three).
- The sleeve at an implausible scale and angle (04, 05; two); moiré on the sleeve (04, 05; one).
- Blotchy, crumpled bonnet shading (t2; two); a dark strip along the road edge (t1; one); a straight seam across the b7 sand (one).
- The jar tilted at the wash-table edge (s11, 04; two); a plant clipping the Ocean Wave label (s2; one); a glowing hanging pot (s7; one).
- Buried glass as flat discs (b2; two); floating V marks over the dunes (b2, b7; one); flat dark decals on the sand (b1, b4; one); hard-edged floor-light quads (08; one).

### November 7 — round 69 and review 66 (three reviewers, averaged)

Round 69 (Round69Pass, glove.py, EquipmentVisuals, menu pass 70):
- **Arm from the side again:** round 68's aim toward the viewer is removed; the brushing arm comes in from the lower right as before.
- **Bristles in view:** the brush tips 18 degrees about its handle so the bristles face the camera; the glove mesh angles the forearm -10 degrees so its net rise stays about 8 degrees.
- **Slimmer sleeve:** about a quarter narrower. **Coarser twill** on the denim, so it no longer shimmers into moire.

All three reviewers viewed the references (36-43 screenshots each), as full-size store images or screenshot grids in their own tab.

| Area | A | B | C | Average | Review 65 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.7 Cleaning | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Cleaning recovered part of its loss (3.3, one 4)**; nobody calls the brush a hidden block any more, and one says "the bristled brush reads well" (A). Grime as blotches, not strokes, holds it down (all three).
- **Tools hold at 3.0 for a fourth round.** The slimmer sleeve still reads as "a long hose-like tube across the basin" (A), "a very long striped tube crossing half the frame, so the arm reads as a hose" (B), "a huge hose-like tube that dominates the frame" (C). The coarse twill now reads as stripes. Glove fingers are "blobby" or "sausage" (A, C); the spade is a good toy (A) or a flat shield plate (C).
- **Lesson after four arm rounds (66-69):** the sleeve's shape, width, material and angle have each changed and the reading has not moved. What the panel sees is the length of arm between the brush and the frame edge, which is set by the basin camera: it looks down at a piece in the middle of a wide bowl, so any arm reaching it crosses half the frame. Either the basin view frames the piece nearer the edge of the frame on the arm's side (or nearer the camera), so only the hand and a short stretch of cuff and sleeve show, or the arm work should rest while other areas move.
- Sea glass edged up to 3.3 (one 4: the tray and palm pieces "convincing glossy worn shards"); sand to 3.3; truck fell to 3.0. Five areas meet the bar unanimously (lighting, water, sorting, mosaic, studio).

**Next:**
- Basin framing: bring the piece and brush nearer the lower-right of the 04 and 05 view (or the camera nearer the piece), so the visible forearm is short; a plain (unstriped) sleeve (all three).
- Sea glass: the basin piece a wedge, not a pillow or jelly (all three); the cloudy piece frosted, not mud camouflage (one).
- Grime in strokes with drips (all three); vegetation sparse (all three); rubbish too small (all three); truck boxy with a blotchy bonnet (all three); flat sand and faceted or slab rocks (two).

**Bugs reported:**
- A hard-edged light shallow-water skirt or step around the island bases (b3, b5, b8; two); the wet/dry edge as a raised step (b3, b6; one); a repeated foam stencil (b8; one).
- The sleeve stretching unnaturally from the frame edge to the brush, over the rim (04, 05; two).
- The jar tilted at the wash-table edge (s11, 04; two); crumpled bonnet shading (t2; two).
- Floating flower sprites (t1, b2, b4; one); a flat mat beside the studio (b4; one); a dark tray-bench square (06, s10; one); slab rocks (b5, b9; one); the EQUIPMENT sign floating (s4; one); the pendant lamp cut by the frame (s7; two); buried glass as a decal (b2; one).

### November 7 — round 70 and review 67 (three reviewers, averaged)

Round 70 (Round70Pass, StudioStation, menu pass 71):
- **Basin framing:** the basin camera stands closer (0.55 m back, was 0.82), 0.26 m to the left and a little lower, and aims 0.32 m past the piece, so the glass sits low and right of centre and only the hand, cuff and a short stretch of sleeve show in 04, 05 and 05b.
- **Plain sleeve:** the denim weave is all but gone, so the sleeve no longer reads as stripes.

All three reviewers viewed the references (41-53 screenshots each), as full-size store images or screenshot grids in their own tab.

| Area | A | B | C | Average | Review 66 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 4 | 4 | **4.0** | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 4 | 3 | 3.3 | 3.3 |
| 5.7 Cleaning | 4 | 3 | 3 | 3.3 | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.11 Truck/road | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.12 Tools | 3 | 4 | 3 | 3.3 | 3.0 |
| 5.13 Effects | 4 | 3 | 3 | 3.3 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **The hose reading is gone.** After four rounds in which all three called the sleeve a hose or tube across the basin, nobody does; one still sees "a flat grey tube" in 05 (A). Tools rose to 3.3 for the first time since round 62 (one 4: "brush with real bristles", B). What holds tools down now is the spade, a "flat paddle on a stick" or plate (A, C), and the glove: a noisy knit patchwork (A) that does not match the smooth coated palm in 06 (C).
- **Sand reaches the bar unanimously (4.0)**, so six areas now meet it: lighting, water, sand, sorting, mosaic, studio. Truck (3.3) and effects (3.3) each gained a 4.
- **The closer framing shows the basin piece's faults more plainly:** the cloudy piece reads as opaque grey camouflage patches over flat turquoise, with no frosted translucency (all three), and the piece itself as a pillowy jelly blob rather than a wedge shard (A, C). Grime still comes off in patches, not strokes, with no drips or runoff (all three), so cleaning holds at 3.3.

**Next:**
- The basin piece: a frosted, translucent cloudy state with the colour showing through, and a wedge-shard form (all three).
- Grime that clears along the brush strokes, with drips or runoff (all three).
- The spade (two) and one consistent glove between the basin and the palm (one).
- Vegetation sparse (all three); rubbish too small to read (all three); truck hood shading lumpy (all three); wet sand flat, meeting dry sand in a hard step (all three).

**Bugs reported:**
- Lumpy, smudged shading on the truck hood and fenders (t2; all three).
- A hard-edged light shallow-water skirt at the island bases (b3, b5, 02; two); the wet/dry sand boundary as a mesh step (b3, b6; two).
- A straight seam across the foreground sand (b7; two); a dark blue strip along the road edge (t1, b7, 03; two).
- The glint on the revealed piece a large smeared white blotch (05b; two).
- The brush hovering above the piece in the rest pose (04; one); the jar tilted off the wash-table edge (s11; one); driftwood joins (b9; one); the glove changing between 04/05 and 06 (one); the "01 / Ocean Wave" label partly hidden (s1, s2; one).

### November 8 — round 71 and review 68 (three reviewers, averaged)

Round 71 (FrostedGlass, StudioStation; no menu pass):
- **Plate shading at the basin:** the basin piece uses the palm's shading (two flat faces and a narrow edge wall), so the wedge's thick edge shows and the revealed piece carries small glints.
- **Frost:** at the basin the cloudy piece is a milky, diffusing skin: what lies behind shows through as a soft blur in the glass hue, deeper where the wedge is thick, with light scattered inside.
- **Sand:** a warm, pale, part see-through dusting, heavier toward the edges, instead of hard-edged opaque grey islands.

All three reviewers viewed the references (35-41 screenshots each), as full-size store images in their own tab.

| Area | A | B | C | Average | Review 67 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 3 | 3 | 3.3 | 4.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.7 Cleaning | 4 | 4 | 4 | **4.0** | 3.3 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.12 Tools | 4 | 3 | 3 | 3.3 | 3.3 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.3 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Cleaning reaches the bar unanimously (4.0)** for the first time since round 63. Nobody calls the cloudy piece grey camouflage any more; the basin, tap, brush, caustic water and the 05b clean moment all read. What remains is grime coming off in patches rather than along the strokes (all three), no splashes or drips (one), and a milder dirty-to-clean contrast than PowerWash (one).
- **Sea glass fell to 3.0 (all three 3).** The revealed basin piece in 05b now has the shard outline and wedge (B), but reads as "a mint jelly or soap bar", "mint jelly or gummy", "a near-opaque mint gummy" (A, B, C): one even tint, with no clearer thin edge and little light passing through. The plate shading took away the dome but not the candy colour: against the pale basin, the filtered water is an even mint. Tray pieces still pass (all three).
- Sand fell back to 3.3 without a change (large plain foregrounds, all three); truck and effects each lost their one 4. Six areas meet the bar: lighting, water, cleaning, sorting, mosaic, studio.

**Next:**
- The revealed basin piece: clear at the thin edge and deep at the thick one, with what lies beneath visibly bent through it, so it reads as glass and not a gummy (all three).
- The glove's knit patchwork reads as a noisy or pixelated texture fault (all three) and differs from the plain palm glove in 06 (one).
- Suds as clip-art bubble rings (all three); grime in strokes (all three).
- Plain sand foregrounds (all three), vegetation (all three), rubbish too small (all three), truck bodywork blocky or smeared (all three), wet sand without sheen in a hard step (all three).

**Bugs reported:**
- The wet/dry sand boundary as a stepped mesh seam (b3, b6; two); the shallow-water skirt at the left island (b5, 02; one).
- The glove's yellow and navy pattern as an aliased texture fault (04, 05; two); the glove material changing between 04/05 and 06 (one).
- Glass on the sand as a flat disc or a hollow ring (b2; two).
- A dark triangle inside the palm piece (06; one); a bottle embedded in the driftwood (b9; one) and its blocky branch ends (b9; one); the jar half off the wash-table edge (s11; one); a stray board and posts behind the cab (t2; one); orange discs floating over the roadside grass (t1, 03; one); glass above a crate rim (s4; one); jars clipping the near plane (s5; one); a purple disc at the b5 edge (one); dark lines under the s7 shelves (one).

### November 8 — round 72 and review 69 (three reviewers, averaged)

Round 72 (FrostedGlass; no menu pass):
- **Clear to deep:** at the basin the wedge grades from nearly clear glass at its thin edge to a deep, saturated hue at its thick one (it was tinted between about half and four-fifths of the hue edge to edge).
- **Water tint:** what shows through takes the basin water's tint; the opaque pass holds the enamel floor without the water, which read cream.
- **Bend:** the floor below shifts further through the thicker glass. The caustic lines are drawn by the water itself, so they do not show through the piece.

All three reviewers viewed the references (38-41 screenshots each), as full-size store images or in Steam's viewer in their own tab.

| Area | A | B | C | Average | Review 68 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 4 | 3 | 3.7 | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.7 Cleaning | 4 | 4 | 3 | 3.7 | 4.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 3 | 3.7 | 4.0 |
| 5.11 Truck/road | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.12 Tools | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Sea glass holds at 3.0 (all three 3); the grading did not register.** The revealed piece still reads as "a thick jelly or gummy shield" (A), "a mint jelly or soap bar: uniform thickness, no visible wedge" (B), "pillowy and evenly thick, like a gummy sweet" (C). One sees a pointed lower tip (A). The cloudy piece in 04 again reads as a soap bar, patchy paint or camouflage blotches (all three). The tray pieces pass (all three).
- **Lesson after two shader rounds on the basin piece (71-72):** shading has now gone from dome to plate and from even mint to clear-to-deep, and the reading has not moved. The same mesh passes in the trays and the palm, a few centimetres across; at the basin it is 0.42 m across and fills a fifth of the frame, so its soft rounded rim and smooth faces read as a moulded sweet. The basin piece needs either a much smaller on-screen size (the real scale of a shard) or a hero form with a crisper broken edge and surface wear that holds up at that size.
- Reviewer C scored harder across the board: cleaning (3.7) and studio (3.7) each lost one 4, and only four areas now meet the bar unanimously (lighting, water, sorting, mosaic). Sand (3.7) and truck (3.3) each gained a 4; tools fell back to 3.0 (spade "a flat plate on a stick", all three; the glove knit aliasing, two).

**Next:**
- The basin piece's size or hero form (all three).
- The spade (all three) and the glove's knit aliasing (two).
- Grime in strokes and a stronger dirty-to-clean contrast (all three); no splashes or drips (two).
- Vegetation (all three), rubbish too small (all three), plain sand foregrounds (all three), wet sand in a hard step (all three), repeated cloud puffs (all three).

**Bugs reported:**
- The wet/dry sand boundary as a stepped ledge or seam (b3, b6; all three).
- A bottle clipping into the driftwood (b9; two); crumpled or mottled bonnet shading (t2; two).
- The lighter skirt at the left island's base (b5, b3; one); radial light streaks in the shallows (b8; one); the bag through the bench (b9; one); flat rings and discs on the sand (b2, 02; one); dark "v" marks over the dunes (b2, b7; one).
- A pale ring between the basin water and the enamel (04, 05, s11; one); the sieve through the basin rim (s11, 04; one); the bristles clipping the glass (05; one); hard-edged floor light (08; one); the dark road-edge line (t1; one).

### November 8 — round 73 and review 70 (three reviewers, averaged)

Round 73 (Round73Pass, round73.py, GameCatalog, StudioStation, menu pass 72):
- **Hero basin shards:** glass_basin_0..5, one per slab, with the same outline, wedge and bend, a crisper edge over a narrow worn bevel, conchoidal chips along the rim with flake scars beside them, and fine pitting on both faces. GlassDefinition.basinMesh holds each piece's hero shard; the trays and palm keep the slabs.
- **Smaller:** the basin piece is 0.3 m across (was 0.42).

All three reviewers viewed the references (36-40 screenshots each), as full-size store images or in Steam's viewer in their own tab.

| Area | A | B | C | Average | Review 69 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 4 | 3.3 | 3.7 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 4 | 3.3 | 3.0 |
| 5.7 Cleaning | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.11 Truck/road | 4 | 3 | 3 | 3.3 | 3.3 |
| 5.12 Tools | 4 | 3 | 4 | 3.7 | 3.0 |
| 5.13 Effects | 4 | 4 | 3 | 3.7 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Sea glass edged up to 3.3 (one 4)**, but the basin piece still reads as "a thick mint gummy or soap bar" (A), "a thick uniform gummy block" (B), and has "thick vertical sides, so it reads closer to a jelly or soap block than a thin bottle wall" (C). The crisper edge made the edge wall read as a straight, tall side: the reading has moved from "pillowy" to "thick". Nobody mentions the chips or pitting. The palm piece is a wedge shard (C), but has a pointed bottom tip (A) and a dark inner triangle (B). Section 5.6 fixes the thickness (about 30% of the width at the thick edge); at the basin's size that thickness is what reads as a block.
- **Six areas meet the bar unanimously again** (lighting, water, cleaning, sorting, mosaic, studio). Tools (3.7) and effects (3.7) each gained two 4s; sand fell back to 3.3 without a change.

**Next:**
- The basin piece reads thick at the basin's size (all three): a decision on 5.6's thickness for the hero piece, or a still smaller basin piece.
- The palm piece's pointed tip (one) and dark inner triangle (one).
- Plain sand foregrounds (all three); vegetation (all three); rubbish too small and buried glass as a flat disc (all three); opaque mint shallows (three); truck bonnet (all three); grime in blotches (all three); one repeated cloud shape (three).

**Bugs reported:**
- A lighter shallow-water shelf or band with a hard edge at the left island and headland (b3, b5, b8; all three).
- A dark line along the road edge and across the b7 sand (t1, b7, 03; two); bonnet shading blotches (t2; two); the jar tilted off the wash-table edge (s11; two).
- The wet/dry sand step (b3, b6; one as a bug, all three as a gap); a dark triangle inside the palm piece (06; one) and its pointed tip (06; one); the EQUIPMENT sign through the bookcase top (s4; one); the buried glass floating as a flat plate (02; one); a hanging pot glowing orange (s7; one); a dark-teal rectangle in the sorting bench apron (06, s10; one).

### November 8 — round 74 and review 71 (three reviewers, averaged)

Round 74 (round73.py, FrostedGlass, Round73Pass re-run; owner's decision):
- **Thinner hero basin shard:** about 19% of the width at the thick edge and 7% at the thin edge (was 5.6's 30% and 12%), the edge wall sloping over a broader bevel instead of standing straight up. The trays and palm keep the 5.6 wedge.
- **5.6 updated** with the wash-bowl piece's thinner hero form and its 0.3 m size (owner's choices, November 8); the reviewers' excerpt carries it.

All three reviewers viewed the references (40-43 screenshots each), as full-size store images or in Steam's viewer in their own tab.

| Area | A | B | C | Average | Review 70 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 4 | 4 | 3.7 | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 4 | 3 | 4 | 3.7 | 3.3 |
| 5.7 Cleaning | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 3 | 4 | 3 | 3.3 | 4.0 |
| 5.11 Truck/road | 4 | 3 | 3 | 3.3 | 3.3 |
| 5.12 Tools | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.13 Effects | 4 | 3 | 4 | 3.7 (stills only) | 3.7 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Sea glass rose to 3.7 (two 4s), its best since round 61.** Nobody reads the basin piece as thick any more, and the cloudy piece in 04 now passes: it "shows its colour under the frost well" (C), "the 04 cloudy frost is fine" (B). The palm piece reads as a wedge shard (A). What remains is the revealed piece in 05b, still "a jelly sweet: soft outline, blobby highlight" (A), "a mint gummy sweet: blobby highlight dimples and a soft rim" (B), "a gummy or jelly slab ... blobby highlights" (C). The pitting breaks the glint into soft blobs rather than the small sharp flashes 5.6 asks for, and the broader bevel softened the rim again.
- **Tools reach the bar unanimously (4.0)** for the first time: glove, brush and open palm all read; the spade blade is still a flat paddle (all three). Six areas meet the bar: lighting, water, cleaning, sorting, mosaic, tools.
- Studio fell to 3.3 without a change (empty floor and flat rugs in s8 and 07b, all three); sand rose to 3.7, also unchanged.

**Next:**
- The revealed piece: small, sharp glints instead of blobby highlights, and a crisper rim at the thin edge (all three).
- Studio floor and rugs (all three); vegetation (all three); rubbish too small (all three); truck bodywork (all three); flat sand foregrounds (all three).
- Grime in strokes and a stronger dirty-to-clean contrast (all three); suds as identical rings (two).

**Bugs reported:**
- A straight seam across the b7 foreground sand at the verge (two); the shallow-water skirt at the left island (b5; two).
- The truck front: a stray board behind the seat (one), dents and creases (one), a stretched texture (one) (t2).
- The jar off the wash-table edge (s11; one); the brush-holder ring through the basin rim (s11, 04, 05; one); the EQUIPMENT sign through the bookshelf (s4; one); near-black deck plants (s6; one); a plant both hanging and standing (s7; one); the bag through the bench (b9; one); the palm piece hovering (06; one); dark "V" marks on the dunes (b2, b7; one); a white ring on the sand (b2; one); a notch in the wet/dry edge (b6; one); dense sparkle noise on the water (b4; one); the "01 / Ocean Wave" plaque overlapped (s10, 06; one).

### November 8 — round 75 and review 72 (three reviewers, averaged)

Round 75 (round73.py, FrostedGlass, Round73Pass re-run):
- **No face pits:** the pits added in round 73 caught the key light as soft rings ("blobby highlight dimples"); they are off, and the bevel is tighter again (power .22, was .3).
- **Sharp flashes, at the basin only:** a fine scatter of small four-point flashes over the lit face, a smaller and harder key glint with no soft sheen around it, and a thin bright line where the face turns over the rim.

All three reviewers viewed the references (38-54 screenshots each), as full-size store images or grids in their own tab.

| Area | A | B | C | Average | Review 71 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.7 |
| 5.4 Props/vegetation | 4 | 3 | 3 | 3.3 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.7 Cleaning | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 3.3 |
| 5.11 Truck/road | 4 | 3 | 4 | 3.7 | 3.3 |
| 5.12 Tools | 4 | 3 | 3 | 3.3 | 4.0 |
| 5.13 Effects | 3 | 3 | 4 | 3.3 (stills only) | 3.7 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Sea glass reaches the bar unanimously (4.0)** for the first time since the fixed form was set: the trays, the palm piece and the frosted 04 piece all read as glass, and the 05b piece is "glossy, with a crisp rim and a glint that breaks into small flashes" (C). A residue remains: the 05b piece reads "slightly like jelly or soap" (B), "slightly jelly-like at its centre" (C), "a soft pillowy edge, no visible chipped rim" (A); the rim chips barely show (two). The run on this piece: framing (70), frost (71), plate shading (71), clear-to-deep (72), hero mesh and size (73), thinner (74), sharp flashes (75).
- **Seven areas meet the bar:** lighting, water, sea glass, cleaning, sorting, mosaic, studio (back to 4.0 without a change). Tools fell to 3.3, also without a change: the spade is "a flat paddle on a stick" or "a flat faceted plate" (two). Sand fell to 3.0: large single-colour foregrounds in b1, b2 and b7 (all three), against the 5.3 rule on one flat colour over a third of the screen.

**Next:**
- Sand foregrounds: damp patches, tide lines and tone variation in b1, b2, b7 and 02 (all three).
- The spade blade (two); rubbish too small and unworn (all three); island canopies and dune bushes (all three); shallows not see-through over sand (two).
- Effects beyond the wash: nothing visible in 06 or 08 (all three); suds as outlined circles (two).

**Bugs reported:**
- A straight seam across the b7 foreground sand along the verge (all three).
- A smeared white texture on the truck bonnet and arches (t2; two).
- The hard water shelf at the near island (b3, b5; one); the bag through the bench (b9; one); driftwood and bottle merged (b9; one); the jar half off the board (s11; one); near-black deck plants (s6; one); a doubled water ring in the basin (s11, 04; one); the basin underside flat purple (s11; one); a bright halo under the buried glass (b2, 02; one); a hard foam ring and a flat rock cut-out (b6, b8; one); the vase over the window frame (07; one); the floor light as hard-edged quads (08; one, and flat light in two more scores); the studio sign unreadable at distance (b1; one).

### November 8 — round 76 and review 73 (three reviewers, averaged)

Round 76 (Round76Pass, PainterlyLit _SandDetail, menu pass 73):
- **Sand detail** on the beach sand, dune sand, tidal sand and the road's sandy shoulders: mid-scale tone (lighter, warmer wind-swept sweeps and darker, cooler packed sand, 2-5 m), soft-edged damp hollows with a darker drying rim, and pale and dark shell grit near the player.
- Lit sand renders close to white, so a mild darkening barely showed; the damp patches are strong to read at all. Wavy wrack lines were tried and dropped (without the distance to the shore they wandered like tyre tracks).

All three reviewers viewed the references (38-43 screenshots each), as full-size store images in their own tab.

| Area | A | B | C | Average | Review 72 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 4 | 4 | 3.7 | 3.0 |
| 5.4 Props/vegetation | 4 | 3 | 4 | 3.7 | 3.3 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 4 | 4 | 3.7 | 4.0 |
| 5.7 Cleaning | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.11 Truck/road | 3 | 3 | 3 | 3.0 | 3.7 |
| 5.12 Tools | 4 | 3 | 4 | 3.7 | 3.3 |
| 5.13 Effects | 3 | 3 | 4 | 3.3 (stills only) | 3.3 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Sand rose from 3.0 to 3.7 (two 4s)**, but no reviewer names the new damp patches or tone sweeps, and all three still see large plain cream foregrounds in b1 and b2 (and b7 and s6 for two). The change is too quiet at review size; one reviewer reads the near-field ripples in b1 as "odd radial streaks" (A), and one sees "faceted, blotchy shading" on the b4 dunes (C), which may be the damp patches on the dune sand.
- Sea glass slipped to 3.7 (A: "one smeared glint and evenly thick edges"; the chips and flashes "barely visible"); two still see the 05b piece as slightly jelly. The palm piece's dark inner triangle is now reported by two reviewers (06).
- Six areas meet the bar: lighting, water, cleaning, sorting, mosaic, studio. Truck fell to 3.0 (bonnet and fenders lumpy, all three).

**Next:**
- The truck bonnet and fender shading (all three), the bug most often reported across reviews 66-73.
- The palm piece's dark inner triangle (two) and the b7 verge seam (all three).
- Sand: features large enough to read at review size, damp patches nearer the water rather than on dunes (all three).
- Rubbish too small (all three); vegetation (all three); the spade blade (all three); grime in strokes and suds as clip-art rings (all three).

**Bugs reported:**
- A straight seam across the b7 verge sand (all three).
- The truck bonnet and fender shading crumpled or faceted (t2; two as a bug, all three in scores); a dark stripe along the road edge (t1; two).
- A pale vertical band on the near-shore boulder (b1, t1; two); a dark triangle inside the palm piece (06; two).
- The lighter water wedge at the island bases (b3, b5; one); radial streaks across the b1 sand (one); blotchy dune shading (b4; one); the sieve through the basin rim (s11, 04; one); the jar sunk into the board (s11; one); the bag through the bench (b9; one); a lampshade clipping the top of s7 (one); the "01 / Ocean Wave" label hidden (s1; one); the brush head over the piece (05; one).

### November 8 — round 77 and review 74 (three reviewers, averaged)

Round 77 (Round77Pass, round77.py, PainterlyLit _AOScale, menu pass 74):
- **The bonnet blotches were screen-space occlusion noise** on the large smooth panels, pooled into the painterly shadow colour (a debug capture with occlusion off showed clean panels; smoothing the mesh and a shadow-lookup offset changed nothing). Painterly Lit gains _AOScale; the truck's four paint materials use .15, every other material keeps full occlusion.
- **truck_shell** (round77.py): a finer, smoother fuse, a lighter decimation (30%, was 17%) and normals carried over from the full-resolution surface.

All three reviewers viewed the references (38-41 screenshots each), as full-size store images in their own tab.

| Area | A | B | C | Average | Review 73 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 4 | 4 | 3 | 3.7 | 3.7 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.7 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 4 | 3 | 4 | 3.7 | 3.7 |
| 5.7 Cleaning | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 3 | 3.7 | 4.0 |
| 5.11 Truck/road | 4 | 4 | 4 | **4.0** | 3.0 |
| 5.12 Tools | 3 | 3 | 4 | 3.3 | 3.7 |
| 5.13 Effects | 4 | 3 | 4 | 3.7 (stills only) | 3.3 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **The truck reaches the bar unanimously (4.0)**, up from 3.0. Nobody reports the bonnet any more, for the first time since review 66: "rounded bonnet and fenders" (B, C), "chunky rounded bonnet, wheel arches" (A). What remains is the boxy rear bed and the flat door panel (two).
- Six areas meet the bar: lighting, water, cleaning, sorting, truck, mosaic. Studio slipped to 3.7 (one 3) and props to 3.0, both unchanged.
- **The b1 radial streaks are now reported by all three** (one in review 73): the near-field wind ripples fan across the b1 foreground. They predate round 76, but its stronger mid-scale tone may make them more visible.

**Next:**
- The b1 radial streaks (all three).
- Vegetation (all three); rubbish too small (all three); the spade blade (all three).
- The hero shard still reads slightly as jelly (all three); the palm piece small, dark or with an olive patch (two).
- Basin water milky and flat (three); grime in blotches (three); empty mosaic sockets dark (three); studio rugs flat (three).

**Bugs reported:**
- Radial streaks across the b1 foreground sand (all three).
- The b7 verge seam (one); faceting on the b4 dunes (one); dark squiggles on the wet sand (b6; one); a flat box-cut rock top (b5, b9; one); the bottle in the driftwood (b9; one); umbrella and bench shadows disagreeing (b9; one).
- The deck meeting a flat pale ground plane (s3, s4; one); the EQUIPMENT sign floating (s4; one); the plant both hanging and standing (s7; one); the jar off the table edge (s11; one); the hero piece sitting on the water (05b; one); an olive patch in the palm piece (06; one); a dark gap in the "Ocean Wave" panel (08; one); a sign board behind the seat (t2; one).

### November 8 — round 78 and review 75 (three reviewers, averaged)

Round 78 (Round78Pass, PainterlyForward.hlsl, menu pass 75):
- **The b1 radial streaks were the near-field wind ripples**, whose crests ran along the line of sight in b1 and converged in perspective (debug captures ruled out shadow acne, occlusion and the lighting normals; with the ripples off the streaks vanished). Ripples now fade where their crests run along the view, break into shorter crests and fade before they get too fine to render cleanly.
- **The streaky Painted sand texture is mostly faded on beach sand** (85% of _SandDetail).

All three reviewers viewed the references (37-41 screenshots each), as full-size store images in their own tab.

| Area | A | B | C | Average | Review 74 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.7 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 4 | 3.3 | 3.7 |
| 5.7 Cleaning | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.11 Truck/road | 3 | 4 | 3 | 3.3 | 4.0 |
| 5.12 Tools | 4 | 3 | 4 | 3.7 | 3.3 |
| 5.13 Effects | 3 | 4 | 3 | 3.3 (stills only) | 3.7 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Nobody reports the b1 radial streaks** (all three in review 74).
- **Sand fell to 3.0 unanimously.** All three call the b1/b2 foreground a large, near-empty plane, and two now report a quilted, cellular patch pattern across the b4 sand (one in review 74). Comparing r78 and r79 b4: the pattern was there before, but the streaky texture faded in round 78 was hiding it, so the fade is the likely cause of the drop.
- Six areas meet the bar: lighting, water, cleaning, sorting, mosaic, studio (back to 4.0, unanimous). The truck slipped to 3.3 (two 3s: boxy rear and wheels, flat hubs, floating mirrors); the round changed nothing on the truck, so this is reviewer spread. Sea glass, tools and effects moved within the usual ±0.3.

**Next:**
- The empty b1/b2 foreground (all three) and the b4 cellular sand pattern (two).
- Vegetation, canopies as blob clusters (all three); rubbish too small (all three); the spade blade flat (all three).
- The hero shard still reads as soap or jelly (two); the palm piece's dark patch (all three).
- Truck wheels, hubs and mirrors (three); grime in blotches (two); empty mosaic sockets dark (one); the floor light as a flat decal (two).

**Bugs reported:**
- A cellular or faceted patch pattern across the b4 sand (two).
- A dark opaque patch in the palm piece (06; all three).
- Floating side mirrors and the board behind the seat (t2; two); the bottle in the driftwood (b9; two); the flat box-topped offshore rock (b5, b9, 02; one); a pale band on a rock (b1, b9; one).
- The b7 sand seam (one); the hard wet/dry sand step (b6; one); a pale skirt around the left island (b5; one); a light patch in the water (b3; one); the flat green disc in b2 (two).
- Brush bristles hovering above the piece (04; one); the floor light lying over the rugs (08; one); a canopy above the bookshelf (s4; one); the jar off the bench edge (s11; one).

### November 8 — round 79 and review 76 (three reviewers, averaged)

Round 79 (Round79Pass, menu pass 76):
- **The b4 cell pattern was the beach's shape, not its colour** (a debug capture with the round 76 mid-scale tone off still showed it). The open beach's height came from 48 separate, overlapping "Beach swell" mounds (TerrainShapePass); wherever two cut through each other the surface creased, and from b4's height the creases drew a net of cells. Round 78's texture fade had uncovered them.
- **One fused beach surface** ("Beach surface", 0.4 m heightfield): the same swells, sampled from their transforms with the same cosine profile and joined by a smooth blend (a 4-norm of their heights), so where two meet the surface rounds over instead of creasing. Only cells above the flat sand shelf are kept, so it meets the shelf tangentially as the swells did. The swell objects are switched off, not deleted.

All three reviewers viewed the references (33-40 screenshots each), as full-size store images in their own tab.

| Area | A | B | C | Average | Review 75 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 4 | 3 | 3.3 | 3.0 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 4 | 3 | 4 | 3.7 | 3.3 |
| 5.7 Cleaning | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 4 | 3 | 3.7 | 4.0 |
| 5.11 Truck/road | 4 | 4 | 4 | **4.0** | 3.3 |
| 5.12 Tools | 4 | 4 | 4 | **4.0** | 3.7 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.3 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **Nobody reports the b4 cell pattern** (two in review 75) or the b1 streaks. Sand is back to 3.3 (one 4); what holds it below the bar is the large, low-detail cream foreground in b1, b2 and s6 (all three).
- **Seven areas meet the bar**, the most so far: lighting, water, cleaning, sorting, mosaic, truck (back to 4.0, unanimous) and tools (4.0 unanimous for the first time, though all three still call the spade blade flat). Studio slipped to 3.7 (one 3) without a change; sea glass and effects moved within the usual ±0.3.

**Next:**
- The empty sand foregrounds (all three).
- Vegetation: blob-cluster canopies, thin palms, cloned tufts (all three); rubbish too small to read at 5 m (all three).
- Effects: no splashes, drips or runoff while cleaning (all three); the mosaic floor light reads as flat painted shapes (all three).
- Studio: open floor and flat rugs (all three). Sea glass: the hero piece as jelly (two), the palm piece (all three: dark facet, pointed tip, a gem or guitar pick). The shallows opaque (two).

**Bugs reported:**
- Hard edges in the water at the island bases or around rock bases (b3, b5, b8; three).
- The board behind the truck seat (t2; two); floating side mirrors (t2; one); the "01 / Ocean Wave" label half hidden (s1, s2; two); the floor light spilling over the rugs (08; two).
- The bottle in the driftwood (b9; one); the jar off the counter edge (s11; one); the EQUIPMENT sign through the bookshelf (s4, 09; one); the tote bag through the bench (b9; one); the hanging-plant rope (s7; one); the s7 lamp cut off by the frame (one).
- A straight line across the b7 sand shoulder (one); S-shaped ribbons on the b6 sand (one); the green glass in b2 as a flat glowing disc (one); the build frames graded flatter than the review captures (one).

### November 8 — round 80 and review 77 (three reviewers, averaged)

Round 80 (Round80Pass, PainterlyForward.hlsl, menu pass 77):
- **Near-field sand detail** (Painterly Lit, beach sand only, within about 15 m): soft trodden (darker, cooler) and wind-scoured (lighter, warmer) patches, 0.5-1.5 m, short and rounded so they cannot line up into streaks; and a fine 5 cm grain stipple, faded before it is finer than a pixel. The lit sand sits near white, so the darker side carries most of it.
- **Beach grit** (Round80Pass): about 300 small groups of shells and pebbles (about 1,100 pieces) across the open beach, density following a broad noise, a clear metre kept around every collectible. Twigs and kelp strands were tried and dropped: they read as dark sticks and black squiggles.

All three reviewers viewed the references (39-41 screenshots each), as full-size store images in their own tab.

| Area | A | B | C | Average | Review 76 average |
| --- | --- | --- | --- | --- | --- |
| 5.1 Lighting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.2 Water | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.3 Sand/terrain | 3 | 3 | 3 | 3.0 | 3.3 |
| 5.4 Props/vegetation | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.5 Rubbish | 3 | 3 | 3 | 3.0 | 3.0 |
| 5.6 Sea glass | 3 | 3 | 3 | 3.0 | 3.7 |
| 5.7 Cleaning | 4 | 4 | 3 | 3.7 | 4.0 |
| 5.8 Sorting | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.9 Mosaic | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.10 Studio | 4 | 3 | 4 | 3.7 | 3.7 |
| 5.11 Truck/road | 4 | 4 | 4 | **4.0** | 4.0 |
| 5.12 Tools | 3 | 4 | 3 | 3.3 | 4.0 |
| 5.13 Effects | 3 | 3 | 3 | 3.0 (stills only) | 3.0 |
| 5.14 Signs | – | – | – | – | – |

**What changed:**
- **The round did not move sand** (3.0 unanimous, from 3.3). Two still call the b1, b2 and s6 foregrounds large plain areas "with only stipple and pebbles"; the third calls the near sand "a repeating speckle", so the grain reads as pattern, not as sand. The grit groups are not mentioned. Surface detail at this strength does not answer "empty"; the foreground needs a change of value or form that reads at a glance.
- **All three now name the wet/dry sand boundary** as a hard, angular polygon edge (b3, b4, b6; one in review 76), the most-reported sand fault this time.
- Five areas meet the bar: lighting, water, sorting, mosaic, truck. Cleaning (one 3), tools (two 3s) and sea glass (three 3s) fell without any change to them, the usual spread; sea glass's hero piece as jelly or soap is reported by all three again.

**Next:**
- Sand: the hard wet/dry edge (all three); the empty foregrounds (all three, counting the speckle).
- The hero shard as jelly, soap or ice (all three); the palm piece as a gem (two).
- Vegetation sparse and repeated, sea stacks as extrusions (all three); rubbish too small (all three).
- Water-plane edges in the shallows (b3, b4, b5; two); shallows opaque (two).
- Effects generic, no spray or runoff (all three); the mosaic floor light as a decal (all three); studio floor and rugs (all three).

**Bugs reported:**
- The wet/dry sand boundary as straight polygon segments (b3, b4, b6; all three).
- Hard-edged lighter patches or seams in the water (b3, b4, b5; two); dark spokes in the shallows around the b8 stacks (one).
- The hero piece as a plate on a plate (05b, 06; one); a basin handle through the rim (s11, 04; one); glove texture aliasing (04, 05; one).
- A straight crease across the b7 sand (one); floating mirrors (t2; one); the b2 glass and plastic as flat decals (one); a pale stripe on boulders (b1, t1; one); a floating bush (s3; one); the s7 lamp in frame (one); a dark gap in the Ocean Wave panel (08; one); a pale plane between deck and road (s9; one).
