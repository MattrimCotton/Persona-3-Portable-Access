"""Trouve les instructions qui référencent une adresse en RIP-relatif (lea/mov/cmp… [rip+disp32])
et donne la fonction qui les contient.

Usage : python tools/re/xref.py [--exe CHEMIN] 0x14062E7BE [0x...]
Méthode : pour chaque position du code, lit un int32 et teste si « adresse de fin d'instruction
+ disp » tombe sur la cible, pour les longueurs d'instruction courantes (disp suivi de 0, 1 ou 4
octets d'immédiat). Puis confirme en désassemblant l'instruction trouvée.
"""
import sys

import capstone
import numpy as np

from sigscan import DEFAULT_EXE, load_pe


def func_start(data, off):
    """Remonte jusqu'au bourrage CC qui précède la fonction (heuristique)."""
    i = off
    while i > 0 and not (data[i - 1] == 0xCC and data[i] != 0xCC):
        i -= 1
    return i


def main(argv):
    exe = DEFAULT_EXE
    if len(argv) >= 2 and argv[0] == "--exe":
        exe, argv = argv[1], argv[2:]
    data, base, sections = load_pe(exe)
    md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
    code = [s for s in sections if s[5] & 0x20000000 and s[4] < 0x4000000]  # sections de code (pas l'énorme .arch)
    for t in argv:
        target = int(t, 16)
        print(f"== références à 0x{target:X}")
        for name, va, vsize, raw, rsize, _ in code:
            buf = np.frombuffer(data, dtype=np.uint8, count=rsize, offset=raw)
            # int32 non aligné à chaque position
            n = rsize - 4
            d32 = (buf[:n].astype(np.int64) | (buf[1:n + 1].astype(np.int64) << 8)
                   | (buf[2:n + 2].astype(np.int64) << 16) | (buf[3:n + 3].astype(np.int8).astype(np.int64) << 24))
            pos_va = base + va + np.arange(n, dtype=np.int64)
            for tail in (0, 1, 4):
                hits = np.nonzero(pos_va + 4 + tail + d32 == target)[0]
                for h in hits:
                    # l'instruction commence 2 à 4 octets avant le disp : on essaie
                    for back in (2, 3, 4):
                        s = h - back
                        ins = next(md.disasm(data[raw + s:raw + s + 16], base + va + s), None)
                        if ins and ins.size == back + 4 + tail and "rip" in ins.op_str:
                            fs = func_start(data, raw + s)
                            print(f"   0x{ins.address:X}  {ins.mnemonic} {ins.op_str}   (fonction ~0x{base + va + fs - raw:X})")
                            break


if __name__ == "__main__":
    main(sys.argv[1:])
