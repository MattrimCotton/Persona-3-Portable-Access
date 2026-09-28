"""Désassemble N instructions à une adresse virtuelle d'un exe PE (lecture du fichier sur disque).

Usage : python tools/re/disasm.py [--exe CHEMIN] 0x140461880 [N=60]
Les cibles RIP-relatives (lea/mov/call) sont résolues en adresses absolues.
"""
import sys

import capstone

from sigscan import DEFAULT_EXE, load_pe


def va_to_off(sections, base, va):
    for name, sva, vsize, raw, rsize, _ in sections:
        if base + sva <= va < base + sva + max(vsize, rsize):
            return raw + (va - base - sva)
    raise ValueError(f"adresse hors sections : 0x{va:X}")


def disasm(data, base, sections, va, count):
    off = va_to_off(sections, base, va)
    md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
    md.detail = True
    out = []
    for ins in md.disasm(data[off:off + count * 16], va):
        out.append(ins)
        if len(out) >= count:
            break
    return out


def main(argv):
    exe = DEFAULT_EXE
    if len(argv) >= 2 and argv[0] == "--exe":
        exe, argv = argv[1], argv[2:]
    va = int(argv[0], 16)
    count = int(argv[1]) if len(argv) > 1 else 60
    data, base, sections = load_pe(exe)
    for ins in disasm(data, base, sections, va, count):
        raw = ins.bytes.hex(" ").upper()
        print(f"{ins.address:X}  {raw:<32} {ins.mnemonic} {ins.op_str}")


if __name__ == "__main__":
    main(sys.argv[1:])
