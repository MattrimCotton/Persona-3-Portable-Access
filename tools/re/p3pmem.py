"""Lire et écrire la mémoire du jeu P3P en cours d'exécution (pour vérifier une structure, ou
dépanner une partie du joueur, avec son accord).

Usage dans un script Python :
    import sys; sys.path.insert(0, "tools/re")
    from p3pmem import rd, wr
    print(rd(0x1409F37A8, 0x18).hex(" "))   # octets
    wr(0x1409F3788, (4).to_bytes(4, "little"))
Le jeu doit tourner (sinon : sortie avec « P3P not running »). Pas d'ASLR : les adresses de
docs/SIGNATURES.md sont valables telles quelles pour la version du 28/09/2026.
"""
import ctypes
import subprocess
import sys

_k = ctypes.windll.kernel32


def _pid():
    out = subprocess.run(["tasklist", "/FO", "CSV", "/NH", "/FI", "IMAGENAME eq P3P.exe"],
                         capture_output=True).stdout.decode("mbcs", "replace")
    for line in out.splitlines():
        if line.startswith('"P3P.exe"'):
            return int(line.split('","')[1])
    sys.exit("P3P not running")


# PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION
_h = _k.OpenProcess(0x0010 | 0x0020 | 0x0008 | 0x0400, False, _pid())


def rd(addr, size):
    """Octets lus à addr, ou None si la page est illisible."""
    buf = ctypes.create_string_buffer(size)
    got = ctypes.c_size_t()
    ok = _k.ReadProcessMemory(_h, ctypes.c_void_p(addr), buf, size, ctypes.byref(got))
    return buf.raw if ok else None


def wr(addr, data):
    """Écrit data (bytes) à addr ; True si tout est écrit."""
    done = ctypes.c_size_t()
    return bool(_k.WriteProcessMemory(_h, ctypes.c_void_p(addr), data, len(data), ctypes.byref(done)))
