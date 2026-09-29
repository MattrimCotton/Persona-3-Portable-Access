"""Prépare une copie de P3P.exe analysable par Ghidra : la section chiffrée .arch (400 Mo) est
déclarée vide et non exécutable, les autres sections gardent leurs octets et leurs adresses.

Usage : python tools/re/ghidra/prepare.py [--exe CHEMIN] [--out DOSSIER]
Par défaut : copie dans C:/Users/asdes.ASUS/Tools/ghidra_work/P3P_noarch.exe.
Ensuite (une fois, environ 25 min) : voir docs/GHIDRA.md.
"""
import os
import shutil
import struct
import sys

sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))
from sigscan import DEFAULT_EXE  # noqa: E402

DEFAULT_OUT = "C:/Users/asdes.ASUS/Tools/ghidra_work"


def main(argv):
    exe, out = DEFAULT_EXE, DEFAULT_OUT
    while argv:
        if argv[0] == "--exe":
            exe, argv = argv[1], argv[2:]
        elif argv[0] == "--out":
            out, argv = argv[1], argv[2:]
        else:
            print(__doc__)
            return 1
    os.makedirs(out, exist_ok=True)
    dst = os.path.join(out, "P3P_noarch.exe")
    shutil.copyfile(exe, dst)
    with open(dst, "r+b") as f:
        head = f.read(0x1000)
        pe = struct.unpack_from("<I", head, 0x3C)[0]
        count = struct.unpack_from("<H", head, pe + 6)[0]
        opt = struct.unpack_from("<H", head, pe + 20)[0]
        table = pe + 24 + opt
        for i in range(count):
            o = table + 40 * i
            if head[o:o + 8].rstrip(b"\0") != b".arch":
                continue
            f.seek(o + 16)
            f.write(struct.pack("<I", 0))  # SizeOfRawData = 0 : section non chargée
            ch = struct.unpack_from("<I", head, o + 36)[0]
            f.seek(o + 36)
            f.write(struct.pack("<I", ch & ~0x20000000 & ~0x20))  # ni code ni exécutable
            print(f".arch neutralisée (caractéristiques 0x{ch:X}) dans {dst}")
            return 0
    print("section .arch introuvable")
    return 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
