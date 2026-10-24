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

### 5.1 Lighting, sky and atmosphere (current: 2/5)

**Now:** washed-out peach haze over everything, low contrast, grey shadows. The sun and clouds are blurry smudges.

**V1 bar:**
- [ ] Sky has a visible gradient from peach at the top to pale yellow at the horizon.
- [ ] Shadows are visibly purple-tinted in every review view.
- [ ] Light side versus shadow side of objects is clearly different (not just slightly darker).
- [ ] Far hills fade to the haze colour; near objects are crisp and saturated.
- [ ] Sun is a clean disc with soft bloom; clouds are shaped or removed.
- [ ] Ambient occlusion is visible where props meet sand and floor.

**References:** `01-lighting`. **Never:** neutral grey shadows, full-screen haze that flattens near objects.

### 5.2 Water and shoreline (current: 3/5)

**Now:** turquoise water with brush-mark ripples, broken shore crests and rock surf. The October 24 blend removed the hard seam. The coast is still a long straight strip.

**V1 bar:**
- [ ] Clear colour bands: deep teal → turquoise shallows → foam line → wet sand → dry sand.
- [ ] Foam line follows the shore with gentle animated movement.
- [ ] Coastline has visible curves, small coves and points; no straight edge longer than about 20 m on screen.
- [ ] Wet sand has a slight sheen that catches the sun.

**References:** `02-shoreline`. **Never:** visible seams between water and sand meshes, uniform water colour from shore to horizon.

### 5.3 Sand and terrain (current: 1/5)

**Now:** flat, single-colour sand covers most of the frame. Dig piles are repeated cone shapes. The inland rock wall is grey boxes.

**V1 bar:**
- [ ] Sand has low-frequency colour and height variation (dunes, ripples, darker damp patches) visible in every beach view.
- [ ] Small scattered details: shells, pebbles, footprints or tide lines, used sparingly.
- [ ] Dig piles have at least 4 shapes, read as soft mounds, and sit naturally in the sand.
- [ ] Cliffs and rock walls are sculpted shapes with top-lit gradients, not boxes.

**References:** `02-shoreline`, `08-props-tools`. **Never:** a single flat colour filling more than a third of the screen.

### 5.4 Beach props and vegetation (current: 2/5)

**Now:** parasols and benches have had a polish pass, but the bench is three logs and grass is sparse spikes. Trees are low-poly blobs with flat colour.

**V1 bar:**
- [ ] Every prop has bevels and a designed silhouette.
- [ ] Vegetation grows in clusters with colour variation from base to tip.
- [ ] Trees have a layered canopy with light/shadow gradient.
- [ ] Each cove has one clear landmark visible from its entrance.

**References:** `08-props-tools`, `11-composition`. **Never:** single primitives standing in for props.

### 5.5 Rubbish and collectibles (current: 2/5)

**Now:** bottles and cans read at close range but are small, flat-coloured and simple.

**V1 bar:**
- [ ] Each rubbish type is recognisable at 5 m in a first-person view.
- [ ] Rubbish shows light wear (faded labels, dents) without looking gritty.
- [ ] Buried items show a readable hint (a glint, a corner) before digging.
- [ ] Removing rubbish makes the beach visibly cleaner (the "polluted to clean" transformation from the design).

**References:** `08-props-tools`. **Never:** rubbish that looks like generic coloured capsules.

### 5.6 Sea glass pieces (current: 1/5) — highest priority

**Now:** flat polygon tiles with uniform colour. Cloudy glass reads as white plastic; clean glass reads as cyan acrylic.

**V1 bar:**
- [ ] Pieces are rounded, pebble-like shapes with soft edges and varied thickness, in about five general shapes.
- [ ] Cloudy glass: frosted, slightly translucent, with the colour faintly visible underneath the grime.
- [ ] Clean glass: saturated colour, light passes through, bright edge highlights, subtle inner glow.
- [ ] Glass reads as glass from 2 m away without a label.
- [ ] Rare red is visibly special.

**References:** `03-sea-glass`. **Never:** flat slabs, uniform opaque colour, glass that looks like plastic.

### 5.7 Cleaning station (current: 1/5)

**Now:** a flat teal basin with ripple rings. The brush is a box and the tap is two boxes. Bubbles are plain white spheres. The grime fades in patches.

**V1 bar:**
- [ ] Basin, brush and tap are modelled objects (enamel basin with rim, bristled brush, shaped tap).
- [ ] Water looks like water: surface highlights, slight transparency, depth tint.
- [ ] Brushing produces foam, small splashes and drips that follow the brush.
- [ ] Grime comes off in strokes that follow the brush.
- [ ] Each piece has a clear "clean" moment (sparkle, colour bloom, sound).

**References:** `04-cleaning`. **Never:** white spheres as bubbles, primitive tools.

### 5.8 Sorting station (current: 1/5)

**Now:** dark empty tray slabs with one small coloured rectangle each. Labels are low-contrast teal on beige.

**V1 bar:**
- [ ] Trays look inviting: lighter lining, visible depth, sorted pieces piled with natural variation.
- [ ] Each tray's colour is readable from across the room (lining tint or a glass sample).
- [ ] Labels are high-contrast and legible at normal viewing distance.
- [ ] Trays visibly fill up over the game.

**References:** `05-sorting`. **Never:** dark empty slabs, unreadable labels.

### 5.9 Mosaic panels and reveal (current: 3/5)

**Now:** the best visual moment in the game; coloured light on the floor works. Panels are about 10 large flat pieces each and look like vector clip art. Empty sockets are dark grey shapes.

**V1 bar:**
- [ ] Pieces separated by visible lead came: dark, slightly raised, with a soft highlight.
- [ ] Each piece has subtle internal texture and colour variation.
- [ ] Backlight glow increases as pieces are placed; completion is a clear, beautiful reveal.
- [ ] Empty sockets show a faint outline of the intended image.
- [ ] Coloured light falls across the studio floor at completion.

**References:** `06-mosaic`. **Never:** flat vector shapes with no came or texture.

### 5.10 Studio interior (current: 2/5)

**Now:** coherent palette (teal walls, cream, burnt-orange beams), curtains, pendants and pottery. Flat shading, large empty floor areas, tables are slabs on blocks.

**V1 bar:**
- [ ] Warm window light falls across the floor in visible shapes.
- [ ] Every station is a modelled piece of furniture with legs, edges and material detail.
- [ ] Storytelling clutter around stations: jars of glass, tools on hooks, sketches. No empty corners in the main views.
- [ ] Clear neglected-to-restored change: dull and dusty at the start, warm and bright at the end.

**References:** `07-studio`. **Never:** slab tables, large empty floor planes.

### 5.11 Truck and road (current: 3/5)

**Now:** the truck is charming and recognisable, with colour blocking, but boxy. The road is a flat grey plane with dashes.

**V1 bar:**
- [ ] Truck bodywork has rounded panels, soft bevels and small details (badge, light wear).
- [ ] Cargo visibly fills with recognisable items.
- [ ] Road has edge variation, sand drifting onto the tarmac, and colour variation.
- [ ] Roadside has varied planting and props, not a repeated row.

**References:** `09-truck-road`. **Never:** flat uniform grey road.

### 5.12 First-person tools (current: 2/5)

**Now:** the shovel is a hexagon on a stick. The bucket, sack and sieve have been reshaped and read better.

**V1 bar:**
- [ ] Each tool is a designed toy-like object with bevels, curves and a material read (plastic, metal, canvas, wood).
- [ ] Upgrades are visibly better (shape, material, size), not just recoloured.
- [ ] Tools are lit consistently with the scene and do not cast oversized shadows.

**References:** `08-props-tools`. **Never:** flat-shaded polygons on a stick.

### 5.13 Effects and feedback (current: 1/5)

**Now:** plain white sphere bubbles; little visible effect on digging, loading or recycling.

**V1 bar:**
- [ ] Digging throws small sand puffs.
- [ ] Collecting, loading, recycling, sorting and placing each have a small particle or motion response.
- [ ] Effects are soft and stylized (shaped particles, colour-matched), never default Unity particle squares.

**References:** `04-cleaning`, `05-sorting`. **Never:** default particle sprites, plain spheres.

### 5.14 UI and HUD (current: 3/5)

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
