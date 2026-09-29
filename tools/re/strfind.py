"""Cherche une chaîne ASCII dans un exe PE et donne l'adresse du DÉBUT de chaque chaîne qui la
contient (utile pour ensuite chercher les références avec xref.py).

Usage : python tools/re/strfind.py [--exe CHEMIN] "titledraw.c" ["autre"]
"""
import sys

from sigscan import DEFAULT_EXE, load_pe


def main(argv):
    exe = DEFAULT_EXE
    if len(argv) >= 2 and argv[0] == "--exe":
        exe, argv = argv[1], argv[2:]
    data, base, sections = load_pe(exe)
    for needle in argv:
        key = needle.encode()
        for name, va, vsize, raw, rsize, _ in sections:
            if rsize > 0x4000000:
                continue  # l'énorme section .arch
            off = data.find(key, raw, raw + rsize)
            while off != -1:
                start = off
                while start > raw and data[start - 1] != 0:
                    start -= 1
                end = data.find(b"\0", off)
                text = data[start:end].decode("ascii", "replace")
                print(f"{name} 0x{base + va + start - raw:X}  {text}")
                off = data.find(key, end, raw + rsize)


if __name__ == "__main__":
    main(sys.argv[1:])
