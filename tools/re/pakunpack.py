"""Extrait une archive PAK d'Atlus (.bin / .pak / .pac de P3P) : suite d'entrées
[nom 252 octets][taille int32][données, alignées sur 64 octets].

Usage : python tools/re/pakunpack.py <archive> <dossier sortie> [-r]
  -r : extrait aussi récursivement les archives trouvées à l'intérieur.
"""
import os
import struct
import sys

NAME_LEN = 252
ALIGN = 64


def parse(data):
    """Liste (nom, octets) ; None si ce n'est pas une archive PAK valide."""
    entries, off = [], 0
    while off + NAME_LEN + 4 <= len(data):
        raw = data[off:off + NAME_LEN]
        if raw[0] == 0:
            break
        name = raw.split(b"\0", 1)[0]
        try:
            name = name.decode("ascii")
        except UnicodeDecodeError:
            return None
        if not name.isprintable() or any(c in name for c in '<>:"|?*\\'):
            return None  # données qui ressemblent à une archive mais n'en sont pas
        size = struct.unpack_from("<I", data, off + NAME_LEN)[0]
        start = off + NAME_LEN + 4
        if start + size > len(data):
            return None
        entries.append((name, data[start:start + size]))
        off = start + ((size + ALIGN - 1) // ALIGN) * ALIGN
    return entries or None


def unpack(path, out, recursive):
    entries = parse(open(path, "rb").read())
    if entries is None:
        return 0
    os.makedirs(out, exist_ok=True)
    n = 0
    for name, blob in entries:
        dst = os.path.join(out, name.replace("/", os.sep))
        os.makedirs(os.path.dirname(dst) or out, exist_ok=True)
        with open(dst, "wb") as f:
            f.write(blob)
        n += 1
        if recursive and os.path.splitext(name)[1].lower() in (".bin", ".pak", ".pac", ".arc"):
            n += unpack(dst, dst + "_", True)
    return n


if __name__ == "__main__":
    a = sys.argv[1:]
    rec = "-r" in a
    a = [x for x in a if x != "-r"]
    print(f"{unpack(a[0], a[1], rec)} fichiers extraits dans {a[1]}")
