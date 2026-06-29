"""
Slice the Wood-chain art for Pocket Garden, mirroring the Garden-chain method.

Sheets are 1254x1254 RGBA in pocket_garden_assets/wooden_assets/. Frames sit in a
regular grid but some touch (so gap-segmentation is unreliable); we therefore use
EXPLICIT per-sheet grids and even-divide each detected row band into N columns,
then tight-crop each cell to its alpha bbox.

Output into Assets/Resources/Items/:
  - <stage>.png                 (twig_three, log, plank, crate, furniture, gazebo, cottage_house)
  - <x>_to_<y>/frame_0..N.png   (animation frames, read top row first, left->right)

Every output shares ONE square canvas and is base-center anchored & bottom-aligned
(centroid of the bottom strip -> canvas center X, content bottom -> common baseline),
exactly like the Garden chain. PPU is normalized later by the Unity importer
(PocketGarden -> Import Item Sprites), so identical canvas => identical world size.
"""

import os
import numpy as np
from PIL import Image

SRC = r"C:\Users\mbaja\source\pocket_garden_assets\wooden_assets"
DST = r"C:\Users\mbaja\source\repos\PocketGarden\Assets\Resources\Items"

ALPHA_T = 16
MIN_BAND = 40       # ignore row bands thinner than this
ROW_MERGE = 130     # join content runs within one grid row (large vertical gap = new row)

# Stage sheet: rows of stage names (reading order).
STAGE_SHEET = "wooden_assets"
STAGE_ROWS = [["twig_three", "log", "plank"],
              ["crate", "furniture", "gazebo", "cottage_house"]]

# Transition sheets: columns-per-row (read top row first, left->right).
TRANSITIONS = {
    "twig_three_to_log":      [4, 4],
    "log_to_plank":           [7],
    "plank_to_crate":         [4, 4],
    "crate_to_furniture":     [4, 4],
    "furniture_to_gazebo":    [4, 4],
    "gazebo_to_cottage_house":[4, 4],
}


def runs(mask):
    out, s = [], None
    for i, v in enumerate(mask):
        if v and s is None:
            s = i
        elif not v and s is not None:
            out.append([s, i]); s = None
    if s is not None:
        out.append([s, len(mask)])
    return out


def row_bands(img, expected):
    a = np.array(img)[:, :, 3] > ALPHA_T
    segs = [r for r in runs(a.any(axis=1)) if r[1] - r[0] >= MIN_BAND]
    # Keep the `expected` tallest bands (drops stray sawdust specks), then order top->bottom.
    segs.sort(key=lambda r: r[1] - r[0], reverse=True)
    if len(segs) < expected:
        raise AssertionError(f"expected {expected} row bands, got {len(segs)}: {segs}")
    bands = sorted(segs[:expected], key=lambda r: r[0])
    return bands


def clean(mask):
    """Drop tiny detached fragments (even-division bleed / sawdust) but keep
    substantial pieces (e.g. exploded-plank frames)."""
    try:
        from scipy import ndimage
        lbl, n = ndimage.label(mask)
        if n <= 1:
            return mask
        areas = np.bincount(lbl.ravel())
        areas[0] = 0
        thr = max(80, areas.max() * 0.05)
        keep = np.where(areas >= thr)[0]
        return np.isin(lbl, keep)
    except Exception:
        import collections
        H, W = mask.shape
        lbl = np.zeros((H, W), np.int32)
        comps = []
        cur = 0
        for sy in range(H):
            for sx in range(W):
                if mask[sy, sx] and lbl[sy, sx] == 0:
                    cur += 1; area = 0
                    q = collections.deque([(sy, sx)]); lbl[sy, sx] = cur
                    while q:
                        y, x = q.popleft(); area += 1
                        for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                            ny, nx = y + dy, x + dx
                            if 0 <= ny < H and 0 <= nx < W and mask[ny, nx] and lbl[ny, nx] == 0:
                                lbl[ny, nx] = cur; q.append((ny, nx))
                    comps.append((cur, area))
        if not comps:
            return mask
        mx = max(a for _, a in comps)
        thr = max(80, mx * 0.05)
        keep = [c for c, a in comps if a >= thr]
        return np.isin(lbl, keep)


def tight(img, box):
    sub = np.array(img.crop(box))
    mask = clean(sub[:, :, 3] > ALPHA_T)
    if not mask.any():
        return None
    sub[~mask, 3] = 0
    ys, xs = np.where(mask)
    return Image.fromarray(sub).crop((int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1))


def base_x(crop):
    a = np.array(crop)[:, :, 3] > ALPHA_T
    h = a.shape[0]
    strip = a[max(0, h - max(4, int(h * 0.12))):, :]
    cols = strip.sum(axis=0)
    if cols.sum() == 0:
        return crop.size[0] / 2.0
    xs = np.arange(cols.shape[0])
    return float((xs * cols).sum() / cols.sum())


def cells(img, ncols_per_row, expected_rows):
    """Even-divide each detected row band into N columns; tight-crop each cell."""
    W = img.size[0]
    bands = row_bands(img, expected_rows)
    out = []  # list of crops in reading order
    for (y0, y1), n in zip(bands, ncols_per_row):
        for k in range(n):
            x0 = int(round(k * W / n))
            x1 = int(round((k + 1) * W / n))
            c = tight(img, (x0, y0, x1, y1))
            if c is not None:
                out.append(c)
    return out


def collect():
    recs = []  # [kind, name, idx, crop]
    sheet = Image.open(os.path.join(SRC, STAGE_SHEET + ".png")).convert("RGBA")
    flat = [n for row in STAGE_ROWS for n in row]
    crops = cells(sheet, [len(r) for r in STAGE_ROWS], len(STAGE_ROWS))
    assert len(crops) == len(flat), f"stage: {len(crops)} crops vs {len(flat)} names"
    for name, c in zip(flat, crops):
        recs.append(["stage", name, 0, c])

    for t, grid in TRANSITIONS.items():
        sh = Image.open(os.path.join(SRC, t + ".png")).convert("RGBA")
        crops = cells(sh, grid, len(grid))
        for i, c in enumerate(crops):
            recs.append(["frame", t, i, c])
        print(f"  {t}: {len(crops)} frames")
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

    print(f"canvas {side}x{side}, baseline margin {margin}px; wrote {n} PNGs -> {DST}")


if __name__ == "__main__":
    main()
