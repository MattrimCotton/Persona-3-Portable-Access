"""Convertit les textures TMX (32 bits ou 8 bits avec palette) d'un fichier .spr / .tmx de P3P en
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
        w, h = struct.unpack_from("<HH", d, o + 0x12)
        fmt = d[o + 0x16]
        pix = o + 0x40
        if fmt == 0x00:
            px = bytearray(d[pix:pix + w * h * 4])
            for k in range(3, len(px), 4):
                px[k] = alpha(px[k])
            img = Image.frombytes("RGBA", (w, h), bytes(px))
        elif fmt == 0x13:
            pal = [tuple(d[pix + c * 4:pix + c * 4 + 3]) + (alpha(d[pix + c * 4 + 3]),) for c in range(256)]
            pal = unswizzle_clut(pal)
            idx = d[pix + 1024:pix + 1024 + w * h]
            img = Image.new("RGBA", (w, h))
            img.putdata([pal[x] for x in idx])
        else:
            print(f"texture {n}: format 0x{fmt:02X} non géré ({w}x{h})")
            i += 4
            n += 1
            continue
        bg = Image.new("RGBA", (w, h), (40, 40, 40, 255))
        bg.alpha_composite(img)
        out = os.path.join(outdir, f"{base}_{n}.png")
        bg.convert("RGB").save(out)
        print(f"texture {n}: {w}x{h} -> {out}")
        i += 4
        n += 1


if __name__ == "__main__":
    convert(sys.argv[1], sys.argv[2])
