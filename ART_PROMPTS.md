# Prompty do generowania sprite'ów — Merge Garden

Styl: **cute flat 2D game icon, white background, no shadow, no outline, simple shapes, vibrant colors, centered, 512x512px**

---

## Prompt 1: Garden Chain (7 sprite'ów)

```
Generate 7 separate game icons in cute flat 2D style, white background, no shadow, no outline, vibrant green tones, each centered, 512x512px. Arrange them in a grid:

1. A tiny seed (small brown oval with a green dot)
2. A small sprout (two green leaves coming from soil)
3. A bush (round green bush, compact)
4. A colorful flower (pink/purple petals, green stem)
5. A young tree (thin trunk, round green crown)
6. A big tree (thick trunk, large leafy crown)
7. A magical glowing tree (golden leaves, sparkles, fairy-tale style)

Style: cute, simple, flat icon for a mobile merge game. No text. White background.
```

---

## Prompt 2: Wood Chain (7 sprite'ów)

```
Generate 7 separate game icons in cute flat 2D style, white background, no shadow, no outline, warm brown tones, each centered, 512x512px. Arrange them in a grid:

1. A Dead Tree (bare leafless dry brown tree, branches only, no leaves)
2. A wooden log (short round log with visible rings)
3. A wooden plank (rectangular flat board)
4. A wooden crate (simple box with plank texture)
5. A piece of furniture (cute small chair)
6. A garden gazebo (small wooden gazebo with roof)
7. A cozy cottage house (tiny wooden house with chimney)

Style: cute, simple, flat icon for a mobile merge game. No text. White background.
```

> Wood Lv1 is the **Dead Tree** (twig_three art). The item name in-game is "Dead Tree" —
> NOT "Twig". Transition sheets (twig_three_to_log … gazebo_to_cottage_house) are sliced
> by `Tools/slice_wood.py`.

---

## Prompt 3: Stone Chain (7 sprite'ów)

```
Generate 7 separate game icons in cute flat 2D style, white background, no shadow, no outline, gray/blue-gray tones, each centered, 512x512px. Arrange them in a grid:

1. A small pebble (round gray stone)
2. A medium stone (irregular gray rock)
3. A brick (rectangular red-brown brick)
4. A stone wall section (stacked stones/bricks)
5. A decorative pillar (classical column)
6. A garden fountain (round fountain with water)
7. A fairy-tale castle (small cute castle with towers)

Style: cute, simple, flat icon for a mobile merge game. No text. White background.
```

---

## Prompt 4: Generatory (3 sprite'y)

```
Generate 3 separate game icons in cute flat 2D style, white background, no shadow, no outline, vibrant colors, each centered, 512x512px:

1. A green garden bush with sparkles (seed generator - produces seeds)
2. A small hand saw with wooden handle (wood generator - produces dead trees)
3. A stone quarry / pickaxe on rock (stone generator - produces pebbles)

Style: cute, simple, flat icon for a mobile merge game. No text. White background.
```

---

## Prompt 5: Dekoracje ogrodu (10 sprite'ów)

```
Generate 10 separate garden decoration icons in cute flat 2D style, white background, no shadow, no outline, colorful, each centered, 512x512px. Arrange in a grid:

1. A garden bench (wooden, simple)
2. A street lamp (classic garden lamp, warm light)
3. A flower bed (rectangular with colorful flowers)
4. A bird bath (stone bowl on pedestal)
5. A garden gnome (cute, colorful hat)
6. A butterfly decoration (colorful wings)
7. A small pond (blue water, lily pads)
8. A rose arch (wooden arch with roses)
9. A garden statue (small angel or animal)
10. A picket fence section (white wooden fence)

Style: cute, simple, flat icon for a mobile merge game. No text. White background.
```

---

## Prompt 6: UI ikony (10 sprite'ów)

```
Generate 10 separate UI icons in cute flat 2D style, white background, no shadow, no outline, each centered, 256x256px. Arrange in a grid:

1. Lightning bolt (yellow - energy icon)
2. Golden coin (round, shiny)
3. Purple gem/diamond
4. Green play button (triangle in circle)
5. Gear/settings icon (gray)
6. Shopping cart (for shop)
7. Clipboard with checkmark (quest icon)
8. Gift box (colorful, with bow)
9. Plus sign in circle (add/buy)
10. Back arrow (left-pointing)

Style: cute, flat, simple mobile game UI icon. No text. White background.
```

---

## Prompt 7: Tła (3 obrazy)

```
Generate a serene garden background for a mobile game in cute flat 2D style, 1080x1920px portrait orientation. Green grass, blue sky, soft clouds, a few distant trees, warm sunlight. No UI elements, no text. Cheerful, cozy atmosphere. Light and airy colors.
```

```
Generate a garden path background for a mobile game, cute flat 2D style, 1080x1920px portrait. Stone path winding through green grass, flower bushes on sides, wooden fence in distance, blue sky. Warm cozy feel. No text.
```

```
Generate a pond garden background for a mobile game, cute flat 2D style, 1080x1920px portrait. Small blue pond with lily pads, surrounded by green grass and flowers, willow tree, butterflies. Peaceful cozy atmosphere. No text.
```

---

## Wskazówki

- Po wygenerowaniu: wytnij każdy sprite osobno (jeśli w gridzie)
- Skaluj do 512x512 (items) lub 256x256 (UI)
- Zapisuj jako PNG z przezroczystym tłem (usuń białe tło w Photoshop/remove.bg)
- Import do Unity: Texture Type = Sprite, Alpha Is Transparency = true
- Po dodaniu nowych sprite'ów: uruchom **PocketGarden → Import Item Sprites** (normalizuje PPU + pivot)

---

## Prompt 8: Muzyka (Suno / Udio / inny generator)

```
Cozy garden background music for a casual mobile puzzle game. Gentle acoustic guitar,
soft xylophone, light piano, nature sounds (birds chirping). Happy, relaxing, looping.
Tempo: 90 BPM. Duration: 2 minutes. No vocals.
```

```
Light cheerful menu music for a mobile game. Ukulele, bells, soft percussion.
Warm, inviting, short loop (45 seconds). No vocals.
```

---

## Prompt 9: Efekty dźwiękowe (brakujące)

| Sound | Description | Generator prompt |
|-------|-------------|-----------------|
| Button_Click | UI tap | "Short soft click sound, mobile game UI, 0.1s, clean" |
| Item_Deliver | Quest delivery success | "Cheerful ding + whoosh, reward collected, 0.5s" |
| Board_Full | Warning | "Gentle low thud, warning tone, not alarming, 0.3s" |
| Chain_Unlock | New chain available | "Ascending chime fanfare, achievement unlocked, 1s" |

---

## Prompt 10: Parallax Background Layers (przyszłe)

```
Generate 3 separate transparent PNG layers for a garden parallax background,
1080x1920px portrait, cute flat 2D style:

Layer 1 (far): Blue sky with soft white clouds, distant mountains silhouette
Layer 2 (mid): Green rolling hills with scattered trees, soft
Layer 3 (near): Grass blades, small flowers, slightly blurred for depth

No text, no UI, transparent between elements. Mobile portrait format.
```

---

## Prompt 11: App Icon (512×512)

```
A cute flat 2D mobile game app icon, 512x512px. A magical glowing tree (golden-green leaves, sparkles) growing from a small garden patch with a tiny seed, sprout, and flower around the base. Vibrant green and gold tones, soft gradient background (sky blue to warm cream). Clean rounded square shape with no text. Style: cozy, inviting, immediately readable at small sizes. No border, no shadow.
```

> Matches the Magic Pairs project icon style: centered main element, vibrant colors,
> immediately recognizable at 48px thumbnail. Export as 512×512 PNG (Google Play requirement).

---

## Prompt 12: Feature Graphic — Google Play (1024×500)

```
A wide banner for a cozy garden merge puzzle game, 1024x500px. Left side: a cute magical tree with golden sparkles. Center: a merge grid showing colorful items (seed, flower, log, brick) being combined. Right side: a small fairy-tale castle and fountain. Background: rolling green hills, blue sky with soft clouds, warm sunlight. Style: cute flat 2D, vibrant colors, cheerful. Text space left in the upper-center area for the game title. No text rendered.
```

> Google Play feature graphic: 1024×500. Title text will be composited in Photoshop/Figma.
> Keep the center-top ~30% relatively clean for overlay text readability.

---

## Prompt 13: Store Screenshots (1080×1920 portrait) — 4 mockups

### Screenshot 1: Core Merge Gameplay
```
A mobile game screenshot mockup, 1080x1920px portrait. Shows a 5x7 grid of cute merge items (seeds, sprouts, flowers, logs, stones) on a soft green background. A finger is dragging one item onto another with a sparkle trail. Top bar shows energy/coins/gems. Bottom shows a quest card and delivery zone. Style: cute flat 2D, vibrant, cozy garden theme. Clean UI, no real phone frame needed.
```

### Screenshot 2: Quest Progression
```
A mobile game screenshot mockup, 1080x1920px portrait. Shows a quest completion celebration: golden coins floating up, sparkle particles, a quest card reading "Complete!" with a checkmark. The merge grid is visible behind with higher-level items (trees, furniture, pillars). Cheerful atmosphere with confetti. Style: cute flat 2D mobile game.
```

### Screenshot 3: All Three Chains
```
A mobile game screenshot mockup, 1080x1920px portrait. Showcase of all merge chains: Garden (seed→magic tree), Wood (dead tree→house), Stone (pebble→castle). Items arranged in 3 horizontal rows showing the progression from level 1 to level 7. Soft green/cream background with chain labels. Style: cute flat 2D, informative, colorful.
```

### Screenshot 4: Shop & Rewards
```
A mobile game screenshot mockup, 1080x1920px portrait. Shows the in-game shop with scrollable cards: gem packs, energy refills, and a special "Gardener's Bundle" highlighted with a "GREAT DEAL" badge. Gold coins and blue gems scattered decoratively. Daily bonus popup partially visible. Style: cute flat 2D, warm inviting colors, cozy garden theme.
```

> All screenshots 1080×1920 (Google Play minimum). Optionally add phone frame in post.
> Screenshots should show real gameplay value propositions — not just static art.

---

## Prompt 14: Promotional Tile (180×120)

```
A tiny promotional tile for a cozy garden game, 180x120px. A miniature magical tree with golden sparkles on a green gradient background. Extremely simple, readable at small size. No text, no border. Style: cute flat 2D, bright colors, single focal element.
```

> Used for small placements (cross-promo, ad networks, store tiles). Must be instantly
> recognizable at thumbnail size. Keep to 1-2 elements max.

---

## Prompt 15: Milestone Celebration Images (1080×1080 square)

These images display when the player reaches key quest milestones. They celebrate the
player's achievements and show a visual representation of what they've built so far.

### Milestone 1: After Quest 10 — Garden Mastered, Wood Unlocked
```
A celebratory square image, 1080x1080px. A lush garden scene: a magical glowing tree in the center surrounded by flowers, bushes, and sprouts. A wooden gate is opening on the right side, revealing a forest path (symbolizing Wood chain unlock). Golden sparkles, butterflies, warm sunlight through leaves. Banner at top: space for "Chapter Complete!" text. Style: cute flat 2D, joyful, achievement celebration. No text rendered.
```

### Milestone 2: After Quest 20 — Wood Chain Progressing
```
A celebratory square image, 1080x1080px. A cozy workshop scene: wooden furniture, crates stacked neatly, a half-built gazebo in the background. A craftsman's workbench with tools. The garden is visible through a window with flowers blooming. Sawdust particles floating in warm light. Festive ribbon decoration. Style: cute flat 2D, warm browns and greens, sense of accomplishment.
```

### Milestone 3: After Quest 30 — Stone Unlocked, Master Builder
```
A celebratory square image, 1080x1080px. A grand reveal: the player's garden now has a completed gazebo and house (Wood chain). In the foreground, a stone quarry is being unveiled — pebbles, stones, and the first bricks are appearing with magical blue sparkles. A wooden bridge crosses a small stream. Sunrise colors, epic feeling. Style: cute flat 2D, transition from wood to stone, celebratory.
```

### Milestone 4: After Quest 40 — Grand Constructions
```
A celebratory square image, 1080x1080px. A growing village: stone walls surround a central garden with flowers and trees. A temple with pillars is being constructed in the background. Wooden houses and gazebos dot the landscape. Workers (cute small characters) celebrating. Fireworks in the evening sky. Style: cute flat 2D, epic scale, village-building achievement.
```

### Milestone 5: After Quest 50 — Near Completion
```
A celebratory square image, 1080x1080px. A majestic scene: a beautiful fountain at the center of a stone plaza, surrounded by pillared walkways. The garden has grown into a paradise with magical trees and flower arches. A castle is partially visible being built in the far background. Golden hour lighting, doves flying. Style: cute flat 2D, regal, approaching the finale.
```

### Milestone 6: After Quest 60 — Ultimate Garden Paradise
```
A celebratory square image, 1080x1080px. The final paradise: a fairy-tale castle with towers dominates the background. In front: a grand fountain, magical glowing trees with golden leaves, flower gardens in full bloom, stone pillars lining a path, a cozy cottage with smoke from the chimney. The entire scene is bathed in warm golden light with sparkles and butterflies everywhere. A rainbow arches over the castle. "The End" feeling — but inviting to keep playing. Style: cute flat 2D, magical, ultimate achievement, paradise garden.
```

> All milestone images are 1080×1080 square. They will be displayed in a full-screen overlay
> with semi-transparent background when the player completes the milestone quest.
> Text (quest milestone name, "Continue" button) will be composited in Unity UI overlay.
