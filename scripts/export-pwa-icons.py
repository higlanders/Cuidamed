"""Export PWA icon sizes from the generated master monogram."""
from pathlib import Path
from PIL import Image, ImageFilter

MASTER = Path(r"C:\Users\cuidamed\.cursor\projects\c-Users-cuidamed-CuidaNet-AppCuidammed-Cuidamed-master\assets\icon-master-1024.png")
LOGO_SRC = Path(r"C:\Users\cuidamed\.cursor\projects\c-Users-cuidamed-CuidaNet-AppCuidammed-Cuidamed-master\assets\c__Users_cuidamed_AppData_Roaming_Cursor_User_workspaceStorage_564cc814839f87b82f83fba4e010fea4_images_image-05ae50c7-a22b-4c2b-b59b-91c4d261ba9d.png")
OUT = Path(r"c:\Users\cuidamed\CuidaNet\AppCuidammed\Cuidamed-master\wwwroot")
WHITE = (255, 255, 255, 255)


def with_padding(im: Image.Image, size: int, content_ratio: float) -> Image.Image:
    src = im.convert("RGBA")
    side = min(src.size)
    src = src.crop((0, 0, side, side))
    content = int(size * content_ratio)
    mark = src.resize((content, content), Image.Resampling.LANCZOS)
    if size <= 192:
        mark = mark.filter(ImageFilter.UnsharpMask(radius=1.0, percent=110, threshold=2))
    canvas = Image.new("RGBA", (size, size), WHITE)
    offset = (size - content) // 2
    canvas.alpha_composite(mark, (offset, offset))
    return canvas


def main():
    master = Image.open(MASTER).convert("RGBA")
    side = min(master.size)
    master = master.crop((0, 0, side, side))
    master.save(OUT / "icon-source-monogram.png", optimize=True)

    # any: mark fills most of the tile
    with_padding(master, 512, 0.92).save(OUT / "icon-512.png", optimize=True)
    with_padding(master, 192, 0.92).save(OUT / "icon-192.png", optimize=True)
    # maskable: extra safe zone (~20% margin) for Android adaptive clip
    with_padding(master, 512, 0.68).save(OUT / "icon-512-maskable.png", optimize=True)
    with_padding(master, 192, 0.68).save(OUT / "icon-192-maskable.png", optimize=True)
    with_padding(master, 64, 0.90).save(OUT / "favicon.png", optimize=True)

    if LOGO_SRC.exists():
        Image.open(LOGO_SRC).convert("RGBA").save(OUT / "LogoCuidamed.png")

    for name in ("_debug-letter.png", "_debug-arc.png"):
        p = OUT / name
        if p.exists():
            p.unlink()

    print("ok")


if __name__ == "__main__":
    main()
