"""Cherche une ou plusieurs signatures (style Reloaded SigScan, '??' = joker) dans un exe PE.

Usage : python tools/re/sigscan.py [--exe CHEMIN] "48 89 5C 24 ?? ..." ["..."]
Affiche pour chaque signature le nombre de résultats et leurs adresses virtuelles
(base 0x140000000 pour P3P.exe, pas d'ASLR). Une signature valide doit avoir UN seul résultat.
"""
import re
import struct
import sys

DEFAULT_EXE = r"D:\SteamLibrary\steamapps\common\P3P\P3P.exe"


def load_pe(path):
    data = open(path, "rb").read()
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    nsec = struct.unpack_from("<H", data, pe + 6)[0]
    opt_size = struct.unpack_from("<H", data, pe + 20)[0]
    opt = pe + 24
    image_base = struct.unpack_from("<Q", data, opt + 24)[0]
    sections = []
    for i in range(nsec):
        s = opt + opt_size + i * 40
        name = data[s:s + 8].rstrip(b"\0").decode(errors="replace")
        vsize, va, rsize, raw = struct.unpack_from("<IIII", data, s + 8)
        chars = struct.unpack_from("<I", data, s + 36)[0]
        sections.append((name, va, vsize, raw, rsize, chars))
    return data, image_base, sections


def to_regex(sig):
    parts = []
    for tok in sig.split():
        parts.append(b"." if tok in ("??", "?") else re.escape(bytes([int(tok, 16)])))
    return re.compile(b"".join(parts), re.DOTALL)


def main(argv):
    exe = DEFAULT_EXE
    if len(argv) >= 2 and argv[0] == "--exe":
        exe, argv = argv[1], argv[2:]
    data, base, sections = load_pe(exe)
    code = [s for s in sections if s[5] & 0x20000000]  # sections exécutables
    for sig in argv:
        rx = to_regex(sig)
        hits = []
        for name, va, vsize, raw, rsize, _ in code:
            for m in rx.finditer(data, raw, raw + rsize):
                hits.append((name, base + va + (m.start() - raw)))
        print(f"{len(hits)} résultat(s) : {sig}")
        for name, addr in hits[:10]:
            print(f"   {name} 0x{addr:X}")


if __name__ == "__main__":
    main(sys.argv[1:])
