"""
Slice Stone-chain art for Pocket Garden.

stone_asset.png: 3 rows → row1(3): pebble, stone, brick; row2(3): wall, pillar, fountain; row3(1): castle
Transitions (2x4=8 frames, except pebble_to_stone 1x7):
  pebble_to_stone, stone_to_brick, brick_to_wall, wall_to_pillar (stone_4→5),
  wall_to_fountain (actually pillar→fountain, stone_5→6), fountain_to_castle
castle_animation: 2x4=8 frames (idle loop at final level)
"""

import os, sys
import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(__file__))
from slice_wood import row_bands, cells, clean, base_x

SRC = r"C:\Users\mbaja\source\pocket_garden_assets\stone_asset"
DST = r"C:\Users\mbaja\source\repos\PocketGarden\Assets\Resources\Items"

ALPHA_T = 16
MIN_BAND = 40

STAGE_SHEET = "stone_asset"
STAGE_ROWS = [["pebble", "stone", "brick"],
              ["wall", "pillar", "fountain"],
              ["castle"]]

TRANSITIONS = {
    "pebble_to_stone":     [7],
    "stone_to_brick":      [4, 4],
    "brick_to_wall":       [4, 4],
    "wall_to_pillar":      [4, 4],
    "pillar_to_fountain":  [4, 4],   # file is wall_to_fountain.png (misnamed)
    "fountain_to_castle":  [4, 4],
}

# Map transition name → actual filename (without .png)
FILE_MAP = {
    "pillar_to_fountain": "wall_to_fountain",
}

IDLE_ANIM = ("castle_animation", [4, 4])  # 2x4 idle loop for castle


def tight(img, box):
    sub = np.array(img.crop(box))
    mask = clean(sub[:, :, 3] > ALPHA_T)
    if not mask.any():
        return None
    sub[~mask, 3] = 0
    ys, xs = np.where(mask)
    return Image.fromarray(sub).crop((int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1))


def cells_local(img, ncols_per_row, expected_rows):
    W = img.size[0]
    bands = row_bands(img, expected_rows)
    out = []
    for (y0, y1), n in zip(bands, ncols_per_row):
        for k in range(n):
            x0 = int(round(k * W / n))
            x1 = int(round((k + 1) * W / n))
            c = tight(img, (x0, y0, x1, y1))
            if c is not None:
                out.append(c)
    return out


def collect():
    recs = []
    sheet = Image.open(os.path.join(SRC, STAGE_SHEET + ".png")).convert("RGBA")
    flat = [n for row in STAGE_ROWS for n in row]
    crops = cells_local(sheet, [len(r) for r in STAGE_ROWS], len(STAGE_ROWS))
    assert len(crops) == len(flat), f"stage: {len(crops)} vs {len(flat)}"
    for name, c in zip(flat, crops):
        recs.append(["stage", name, 0, c])

    for t, grid in TRANSITIONS.items():
        fname = FILE_MAP.get(t, t)
        sh = Image.open(os.path.join(SRC, fname + ".png")).convert("RGBA")
        crops = cells_local(sh, grid, len(grid))
        for i, c in enumerate(crops):
            recs.append(["frame", t, i, c])
        print(f"  {t}: {len(crops)} frames")

    # Castle idle animation
    aname, agrid = IDLE_ANIM
    sh = Image.open(os.path.join(SRC, aname + ".png")).convert("RGBA")
    crops = cells_local(sh, agrid, len(agrid))
    for i, c in enumerate(crops):
        recs.append(["frame", aname, i, c])
    print(f"  {aname}: {len(crops)} frames (idle)")

    return recs


def main():
    os.makedirs(DST, exist_ok=True)
    recs = collect()
    bases = [base_x(r[3]) for r in recs]

    side = 0
    for (_, _, _, crop), bx in zip(recs, bases):
        w, h = crop.size
        side = max(side, int(2 * max(bx, w - bx)) + 1, h)
    side = int(side * 1.06) + 2
    margin = max(2, int(side * 0.03))

    def place(crop, bx):
        canvas = Image.new("RGBA", (side, side), (0, 0, 0, 0))
        w, h = crop.size
        x = max(0, int(round(side / 2 - bx)))
        y = max(0, side - margin - h)
        canvas.alpha_composite(crop, (x, y))
        return canvas

    n = 0
    for (kind, name, idx, crop), bx in zip(recs, bases):
        out = place(crop, bx)
        if kind == "stage":
            out.save(os.path.join(DST, name + ".png"))
        else:
            folder = os.path.join(DST, name)
            os.makedirs(folder, exist_ok=True)
            out.save(os.path.join(folder, f"frame_{idx}.png"))
        n += 1

    print(f"canvas {side}x{side}, margin {margin}px; wrote {n} PNGs -> {DST}")


if __name__ == "__main__":
    main()
