"""Build crisp PWA icons from the cuidamed wordmark (c + brand arc)."""
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

SRC = Path(r"C:\Users\cuidamed\.cursor\projects\c-Users-cuidamed-CuidaNet-AppCuidammed-Cuidamed-master\assets\c__Users_cuidamed_AppData_Roaming_Cursor_User_workspaceStorage_564cc814839f87b82f83fba4e010fea4_images_image-05ae50c7-a22b-4c2b-b59b-91c4d261ba9d.png")
OUT = Path(r"c:\Users\cuidamed\CuidaNet\AppCuidammed\Cuidamed-master\wwwroot")

BG_WHITE = (255, 255, 255, 255)
BG_NAVY = (3, 23, 61, 255)


def rgba(im: Image.Image) -> np.ndarray:
    return np.array(im.convert("RGBA"))


def is_content(arr: np.ndarray) -> np.ndarray:
    a = arr[:, :, 3]
    rgb = arr[:, :, :3].astype(int)
    near_white = rgb.min(axis=2) > 245
    return (a > 15) & (~near_white)


def trim_rgba(arr: np.ndarray) -> np.ndarray:
    m = arr[:, :, 3] > 15
    ys, xs = np.where(m)
    return arr[ys.min() : ys.max() + 1, xs.min() : xs.max() + 1]


def sample_brand_colors(arr: np.ndarray):
    m = is_content(arr)
    pixels = arr[m][:, :3].astype(int)
    # Split by greenness: g - b and relative to teal
    # Teal/petrol for "cuida", mint for "med"+arc
    score_mint = pixels[:, 1] - pixels[:, 2]  # greener
    # darkest teal cluster ≈ lower luminance + not minty
    lum = pixels.mean(axis=1)
    teal = pixels[(score_mint < 25) & (lum < 120)]
    mint = pixels[(score_mint >= 25) | ((pixels[:, 1] > 140) & (lum > 130))]
    if len(teal) == 0:
        teal_rgb = (0, 112, 140)
    else:
        teal_rgb = tuple(int(x) for x in np.median(teal, axis=0))
    if len(mint) == 0:
        mint_rgb = (160, 190, 175)
    else:
        mint_rgb = tuple(int(x) for x in np.median(mint, axis=0))
    return teal_rgb, mint_rgb


def extract_letter_c(arr: np.ndarray) -> np.ndarray:
    mask = is_content(arr)
    x0, y0, x1, y1 = [int(v) for v in (
        np.where(mask)[1].min(),
        np.where(mask)[0].min(),
        np.where(mask)[1].max(),
        np.where(mask)[0].max(),
    )]
    crop = arr[y0 : y1 + 1, x0 : x1 + 1]
    m = mask[y0 : y1 + 1, x0 : x1 + 1]
    col = m.sum(axis=0).astype(float)

    # Smooth and find first strong valley after first peak (= gap after c).
    k = max(5, len(col) // 60)
    smooth = np.convolve(col, np.ones(k) / k, mode="same")
    # Only look at left 22% — letter c is the first glyph.
    limit = max(8, int(len(smooth) * 0.22))
    peak = int(np.argmax(smooth[:limit]))
    # Find minimum after peak within left band
    band = smooth[peak:limit]
    valley = peak + int(np.argmin(band))
    # Walk left from valley until density rises again above 35% of peak — cut before u.
    peak_val = smooth[peak]
    cut = valley
    for i in range(valley, peak, -1):
        if smooth[i] > peak_val * 0.35:
            cut = i
            break
    # Prefer the deepest point in a small window around detected gap
    win0 = max(peak + 1, int(len(smooth) * 0.08))
    win1 = limit
    if win1 > win0:
        cut = win0 + int(np.argmin(smooth[win0:win1]))

    # Hard cap: c is roughly ~14-17% of full wordmark width for "cuidamed"
    cut = min(cut, int(len(col) * 0.17))
    cut = max(cut, int(len(col) * 0.11))

    letter = crop[:, : cut + 1].copy()
    # Keep only teal-ish pixels (drop accidental mint if any)
    rgb = letter[:, :, :3].astype(int)
    a = letter[:, :, 3]
    near_white = rgb.min(axis=2) > 245
    letter[near_white, 3] = 0
    return trim_rgba(letter)


def extract_arc(arr: np.ndarray) -> np.ndarray:
    """Arc sits above the ē in 'med' — top band, right half, mint-colored."""
    mask = is_content(arr)
    ys, xs = np.where(mask)
    x0, y0, x1, y1 = int(xs.min()), int(ys.min()), int(xs.max()), int(ys.max())
    w = x1 - x0 + 1
    h = y1 - y0 + 1
    # Top ~28% and right ~45% of the wordmark
    top = arr[y0 : y0 + max(2, int(h * 0.32)), x0 + int(w * 0.55) : x1 + 1].copy()
    rgb = top[:, :, :3].astype(int)
    a = top[:, :, 3]
    near_white = rgb.min(axis=2) > 245
    # Keep greener / lighter mint strokes
    minty = (rgb[:, :, 1] - rgb[:, :, 2] >= 8) | (rgb[:, :, 1] > 140)
    keep = (a > 15) & (~near_white) & minty
    top[~keep, 3] = 0
    if top[:, :, 3].max() == 0:
        # Fallback: any content in that region
        top = arr[y0 : y0 + max(2, int(h * 0.32)), x0 + int(w * 0.55) : x1 + 1].copy()
        rgb = top[:, :, :3].astype(int)
        near_white = rgb.min(axis=2) > 245
        top[near_white | (top[:, :, 3] <= 15), 3] = 0
    return trim_rgba(top)


def compose_monogram(letter: np.ndarray, arc: np.ndarray, scale: int = 8) -> Image.Image:
    """Upscale pieces with nearest-then-LANCZOS via large canvas for sharper final icons."""
    letter_im = Image.fromarray(letter, "RGBA")
    arc_im = Image.fromarray(arc, "RGBA")

    # Target composition canvas before final resize
    base_h = 320
    # Scale letter to height
    lw, lh = letter_im.size
    letter_h = int(base_h * 0.72)
    letter_w = max(1, int(lw * (letter_h / lh)))
    letter_im = letter_im.resize((letter_w, letter_h), Image.Resampling.LANCZOS)

    aw, ah = arc_im.size
    arc_w = int(letter_w * 1.05)
    arc_h = max(2, int(ah * (arc_w / aw)))
    arc_im = arc_im.resize((arc_w, arc_h), Image.Resampling.LANCZOS)

    pad_x = int(letter_w * 0.25)
    pad_top = int(base_h * 0.08)
    gap = int(base_h * 0.04)
    canvas_w = max(letter_w, arc_w) + pad_x * 2
    canvas_h = pad_top + arc_h + gap + letter_h + int(base_h * 0.08)
    canvas = Image.new("RGBA", (canvas_w, canvas_h), (0, 0, 0, 0))

    ax = (canvas_w - arc_w) // 2
    ay = pad_top
    lx = (canvas_w - letter_w) // 2
    ly = pad_top + arc_h + gap
    canvas.alpha_composite(arc_im, (ax, ay))
    canvas.alpha_composite(letter_im, (lx, ly))
    return canvas


def place_on_bg(mono: Image.Image, size: int, bg: tuple, fill: float) -> Image.Image:
    canvas = Image.new("RGBA", (size, size), bg)
    max_side = int(size * fill)
    mw, mh = mono.size
    ratio = min(max_side / mw, max_side / mh)
    nw, nh = max(1, int(mw * ratio)), max(1, int(mh * ratio))
    resized = mono.resize((nw, nh), Image.Resampling.LANCZOS)
    if size <= 192:
        resized = resized.filter(ImageFilter.UnsharpMask(radius=1.2, percent=140, threshold=2))
    x = (size - nw) // 2
    y = (size - nh) // 2
    canvas.alpha_composite(resized, (x, y))
    return canvas


def main():
    arr = rgba(Image.open(SRC))
    teal, mint = sample_brand_colors(arr)
    print("colors teal", teal, "mint", mint)

    letter = extract_letter_c(arr)
    arc = extract_arc(arr)
    print("letter", letter.shape, "arc", arc.shape)

    Image.fromarray(letter).save(OUT / "_debug-letter.png")
    Image.fromarray(arc).save(OUT / "_debug-arc.png")

    mono = compose_monogram(letter, arc)
    mono.save(OUT / "icon-source-monogram.png")
    print("monogram", mono.size)

    # any: white background
    for size, name in [(192, "icon-192.png"), (512, "icon-512.png")]:
        place_on_bg(mono, size, BG_WHITE, 0.78).save(OUT / name, optimize=True)
        print("saved", name)

    # maskable: navy + more padding (safe zone)
    for size, name in [(192, "icon-192-maskable.png"), (512, "icon-512-maskable.png")]:
        # On navy, teal c is ok; mint arc ok. Extra padding.
        place_on_bg(mono, size, BG_NAVY, 0.62).save(OUT / name, optimize=True)
        print("saved", name)

    place_on_bg(mono, 64, BG_WHITE, 0.82).save(OUT / "favicon.png", optimize=True)

    # Keep full logo asset
    Image.open(SRC).convert("RGBA").save(OUT / "LogoCuidamed.png")
    print("done")


if __name__ == "__main__":
    main()
