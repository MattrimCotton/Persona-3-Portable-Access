"""Convertit les textures TMX (32 bits ou 8/4 bits avec palette) d'un fichier .spr / .tmx de P3P en
PNG, pour regarder les textes dessinés en image (menus, écrans spéciaux). Affiche aussi les noms
des sprites du fichier .spr.

Usage : python tools/re/spr2png.py <fichier.spr|.tmx> <dossier sortie>
Format TMX (PS2) : en-tête 0x40 octets ("TMX0" à +8), largeur/hauteur +0x12/+0x14, format
+0x16 (0x00 = 32 bits, 0x13 = 8 bits indexé), palette puis pixels ; alpha PS2 de 0 à 0x80.
"""
import os
import struct
import sys

from PIL import Image


def unswizzle_clut(pal):
    """Les palettes 256 couleurs PS2 ont les blocs 8-15 et 16-23 échangés dans chaque groupe de 32."""
    out = list(pal)
    for i in range(0, 256, 32):
        out[i + 8:i + 16], out[i + 16:i + 24] = pal[i + 16:i + 24], pal[i + 8:i + 16]
    return out


def alpha(a):
    return min(255, a * 2)


def decode_tmx(data, offset=0):
    """Decode one TMX to RGBA, preserving transparency; reject truncated/unknown data."""
    if offset < 0 or offset + 0x40 > len(data) or data[offset + 8:offset + 12] != b"TMX0":
        raise ValueError("Invalid TMX header")
    w, h = struct.unpack_from("<HH", data, offset + 0x12)
    fmt = data[offset + 0x16]
    if not w or not h or w * h > 64 * 1024 * 1024:
        raise ValueError(f"Invalid TMX dimensions {w}x{h}")
    pix = offset + 0x40
    if fmt == 0x00:
        end = pix + w * h * 4
        if end > len(data):
            raise ValueError("Truncated RGBA texture")
        img = Image.frombytes("RGBA", (w, h), data[pix:end])
        img.putalpha(img.getchannel("A").point([alpha(a) for a in range(256)]))
    elif fmt in (0x13, 0x1B, 0x14, 0x24, 0x2C):
        count = 256 if fmt in (0x13, 0x1B) else 16
        palette_count, palette_format = data[offset + 0x10:offset + 0x12]
        if palette_count != 1 or palette_format != 0:
            raise ValueError(f"Unsupported palette count/format {palette_count}/0x{palette_format:02X}")
        palette_bytes = count * 4
        pixel_bytes = w * h if count == 256 else (w * h + 1) // 2
        end = pix + palette_bytes + pixel_bytes
        if end > len(data):
            raise ValueError("Truncated indexed texture")
        pal = [tuple(data[pix + c * 4:pix + c * 4 + 3]) + (alpha(data[pix + c * 4 + 3]),)
               for c in range(count)]
        if count == 256:
            pal = unswizzle_clut(pal)
        indices = data[pix + palette_bytes:end]
        if count == 16:
            # Amicitia PS2PixelFormatHelper.ReadPSMT4: low nibble first, no CLUT shuffle.
            indices = bytes(n for byte in indices for n in (byte & 15, byte >> 4))[:w * h]
        img = Image.frombytes("P", (w, h), indices)
        img.putpalette(bytes(channel for color in pal for channel in color), rawmode="RGBA")
        img = img.convert("RGBA")
    else:
        raise ValueError(f"Unsupported TMX format 0x{fmt:02X} ({w}x{h})")
    return img


def convert(path, outdir):
    d = open(path, "rb").read()
    base = os.path.splitext(os.path.basename(path))[0]
    if d[8:12] == b"SPR0":
        _, sc, _, so = struct.unpack_from("<HHII", d, 0x14)
        for i in range(sc):
            p = struct.unpack_from("<I", d, so + i * 8 + 4)[0]
            name = d[p + 4:p + 0x20].split(b"\0")[0].decode("latin-1")
            print(f"sprite {i}: {name}")
    i, n = 0, 0
    os.makedirs(outdir, exist_ok=True)
    while (i := d.find(b"TMX0", i)) >= 0:
        o = i - 8
        try:
            img = decode_tmx(d, o)
        except ValueError as exc:
            print(f"texture {n}: {exc}")
            i += 4
            n += 1
            continue
        w, h = img.size
        bg = Image.new("RGBA", (w, h), (40, 40, 40, 255))
        bg.alpha_composite(img)
        out = os.path.join(outdir, f"{base}_{n}.png")
        bg.convert("RGB").save(out)
        print(f"texture {n}: {w}x{h} -> {out}")
        i += 4
        n += 1


if __name__ == "__main__":
    convert(sys.argv[1], sys.argv[2])
