"""Bake content-box normalized Pixels-Per-Unit AND content-centered pivot into
Resources/Items sprite .meta files.

Mirrors PocketGarden.Editor.ItemSpriteImporter logic:
  PPU = normDim / TARGET
  pivot = content_center / canvas_size  (so Unity places the content center at transform.position)

For animation folders: all frames share ONE pivot (the folder's average content center)
so frames don't jitter side-to-side during playback.

Run:  python Tools/normalize_ppu.py
"""
import os, re, glob
from PIL import Image

ITEMS = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "Items")
TARGET = 0.72
ALPHA = 10


def content_box(png):
    """Returns (content_max_dim, pivot_x, pivot_y) for a PNG.
    pivot is in Unity convention: 0,0 = bottom-left, 1,1 = top-right."""
    im = Image.open(png).convert("RGBA")
    w, h = im.size
    bb = im.getchannel("A").point(lambda a: 255 if a > ALPHA else 0).getbbox()
    if not bb:
        return max(w, h), 0.5, 0.5
    dim = max(bb[2] - bb[0], bb[3] - bb[1])
    # Content center in pixel coords (origin top-left, y-down in PIL)
    cx = (bb[0] + bb[2]) / 2.0
    cy = (bb[1] + bb[3]) / 2.0
    # Unity pivot: x = cx/w, y = 1 - cy/h (Unity y is bottom-up)
    px = cx / w
    py = 1.0 - cy / h
    return dim, px, py


def main():
    pngs = glob.glob(os.path.join(ITEMS, "**", "*.png"), recursive=True)
    info = {}  # path -> (dim, px, py)
    for p in pngs:
        info[p] = content_box(p)

    # Group by folder for animation frames
    folder_max = {}   # folder -> max dim
    folder_pivot = {} # folder -> (sum_px, sum_py, count)
    top = os.path.normpath(ITEMS)

    for p, (d, px, py) in info.items():
        folder = os.path.dirname(p)
        folder_max[folder] = max(folder_max.get(folder, 0), d)
        if folder not in folder_pivot:
            folder_pivot[folder] = [0.0, 0.0, 0]
        folder_pivot[folder][0] += px
        folder_pivot[folder][1] += py
        folder_pivot[folder][2] += 1

    patched = 0
    for p in sorted(pngs):
        folder = os.path.dirname(p)
        is_top = os.path.normpath(folder) == top
        d, px, py = info[p]

        # PPU: top-level uses own dim, folders use folder max
        norm = d if is_top else folder_max[folder]
        ppu = max(1.0, norm / TARGET)

        # Pivot: top-level uses own center, folders use folder average
        if is_top:
            piv_x, piv_y = px, py
        else:
            s = folder_pivot[folder]
            piv_x = s[0] / s[2]
            piv_y = s[1] / s[2]

        meta = p + ".meta"
        if not os.path.exists(meta):
            continue
        txt = open(meta, encoding="utf-8").read()
        new = re.sub(r"spritePixelsToUnits: [0-9.]+",
                     "spritePixelsToUnits: %.3f" % ppu, txt)
        new = re.sub(r"spritePivot: \{x: [0-9.]+, y: [0-9.]+\}",
                     "spritePivot: {x: %.4f, y: %.4f}" % (piv_x, piv_y), new)
        # Unity ignores spritePivot unless alignment is Custom (9). The relevant field is the
        # top-level "alignment:" immediately preceding "spritePivot:".
        new = re.sub(r"alignment: \d+(\s*\n\s*spritePivot:)",
                     r"alignment: 9\1", new)
        if new != txt:
            open(meta, "w", encoding="utf-8").write(new)
            patched += 1
        rel = os.path.relpath(p, ITEMS)
        print("%-40s ppu=%6.1f  pivot=(%.3f, %.3f)" % (rel, ppu, piv_x, piv_y))

    print("\nPatched %d meta files." % patched)


if __name__ == "__main__":
    main()
