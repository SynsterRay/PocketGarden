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

1. A bare leafless tree (dead/dry brown tree, branches only)
2. A wooden log (short round log with visible rings)
3. A wooden plank (rectangular flat board)
4. A wooden crate (simple box with plank texture)
5. A piece of furniture (cute small chair)
6. A garden gazebo (small wooden gazebo with roof)
7. A cozy cottage house (tiny wooden house with chimney)

Style: cute, simple, flat icon for a mobile merge game. No text. White background.
```

> Wood Lv1 is the bare "Dead Tree" (twig_three art, replaces the old twig). Transition
> sheets (twig_three_to_log … gazebo_to_cottage_house) are sliced by `Tools/slice_wood.py`.

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
2. A small hand saw with wooden handle (wood generator - produces twigs)
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
