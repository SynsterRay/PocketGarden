"""Bake content-box normalized Pixels-Per-Unit into Resources/Items sprite .meta files.

Mirrors PocketGarden.Editor.ItemSpriteImporter exactly (which was edited to
content-normalize but never re-run in Unity, so the on-disk .meta files still
carry the old CANVAS-based PPU that made the Garden chain render oversized).

  PPU = normDim / TARGET, where
    - top-level static sprites: normDim = the file's own alpha-trimmed content max dim
    - animation frame folders : normDim = the folder's LARGEST content max dim
                                (so frames keep relative growth + match the static)

Run:  python Tools/normalize_ppu.py
"""
import os, re, glob
from PIL import Image

ITEMS = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "Items")
TARGET = 0.72
ALPHA = 10


def content_max_dim(png):
    im = Image.open(png).convert("RGBA")
    bb = im.getchannel("A").point(lambda a: 255 if a > ALPHA else 0).getbbox()
    if not bb:
        return max(im.size)
    return max(bb[2] - bb[0], bb[3] - bb[1])


def main():
    pngs = glob.glob(os.path.join(ITEMS, "**", "*.png"), recursive=True)
    dims = {p: content_max_dim(p) for p in pngs}

    # group by parent folder; top-level files are each their own group
    folder_max = {}
    for p, d in dims.items():
        folder_max[os.path.dirname(p)] = max(folder_max.get(os.path.dirname(p), 0), d)

    top = os.path.normpath(ITEMS)
    patched = 0
    for p in sorted(pngs):
        folder = os.path.dirname(p)
        is_top = os.path.normpath(folder) == top
        norm = dims[p] if is_top else folder_max[folder]
        ppu = max(1.0, norm / TARGET)

        meta = p + ".meta"
        if not os.path.exists(meta):
            print("  (no meta)", os.path.relpath(p, ITEMS)); continue
        txt = open(meta, encoding="utf-8").read()
        new = re.sub(r"spritePixelsToUnits: [0-9.]+",
                     "spritePixelsToUnits: %.3f" % ppu, txt)
        if new != txt:
            open(meta, "w", encoding="utf-8").write(new)
            patched += 1
        rel = os.path.relpath(p, ITEMS)
        print("%-40s content=%-4d ppu=%.1f  visible=%.2f" %
              (rel, dims[p], ppu, dims[p] / ppu))

    print("\nPatched %d meta files (target visible size = %.2f world units)." % (patched, TARGET))


if __name__ == "__main__":
    main()
