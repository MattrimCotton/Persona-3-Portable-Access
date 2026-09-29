"""Check the mod's translation files (lang/*.json) against the English reference.

Usage:  python tools/lang_check.py [--complete french] [--complete <id> ...]

Errors (exit code 1): invalid JSON, non-string values, keys unknown to english.json,
placeholders ({0}, {1}...) that differ from English, keys of english.json without context
in context.json, a language listed with --complete that misses keys.
Warnings: missing keys (the mod then speaks the English text), untranslated keys.
"""
import argparse
import json
import re
import sys
from pathlib import Path

LANG_DIR = Path(__file__).resolve().parent.parent / "lang"
REFERENCE = "english"
PLACEHOLDER = re.compile(r"\{\d+[^}]*\}")


def load(path):
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    if not isinstance(data, dict):
        raise ValueError("the file must contain one JSON object")
    return {k: v for k, v in data.items() if not k.startswith("_")}


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--complete", action="append", default=[],
                        help="language id that must have every key (repeatable)")
    args = parser.parse_args()

    errors, warnings = [], []
    ref = load(LANG_DIR / f"{REFERENCE}.json")
    for key, value in ref.items():
        if not isinstance(value, str):
            errors.append(f"{REFERENCE}: '{key}' is not a string")

    context = load(LANG_DIR / "context.json")
    for key in ref:
        if key not in context:
            errors.append(f"context.json: no context for '{key}'")
    for key in context:
        if key not in ref:
            errors.append(f"context.json: '{key}' is not in {REFERENCE}.json")

    files = sorted(p for p in LANG_DIR.glob("*.json") if p.stem not in (REFERENCE, "context"))
    for path in files:
        lang = path.stem
        try:
            data = load(path)
        except (ValueError, json.JSONDecodeError) as e:
            errors.append(f"{lang}: invalid JSON: {e}")
            continue
        missing = [k for k in ref if k not in data]
        for key, value in data.items():
            if key not in ref:
                errors.append(f"{lang}: unknown key '{key}' (not in {REFERENCE}.json)")
            elif not isinstance(value, str):
                errors.append(f"{lang}: '{key}' is not a string")
            elif sorted(PLACEHOLDER.findall(value)) != sorted(PLACEHOLDER.findall(ref[key])):
                errors.append(f"{lang}: '{key}' placeholders {PLACEHOLDER.findall(value)} "
                              f"differ from English {PLACEHOLDER.findall(ref[key])}")
            elif value == ref[key] and len(value) > 8:
                warnings.append(f"{lang}: '{key}' is identical to English (untranslated?)")
        if missing:
            msg = f"{lang}: {len(missing)} missing key(s), English is used: {', '.join(missing)}"
            (errors if lang in args.complete else warnings).append(msg)
        done = len(ref) - len(missing)
        print(f"{lang}: {done}/{len(ref)} translated")

    for lang in args.complete:
        if not (LANG_DIR / f"{lang}.json").exists() and lang != REFERENCE:
            errors.append(f"{lang}: file lang/{lang}.json not found")

    for w in warnings:
        print("warning:", w)
    for e in errors:
        print("ERROR:", e)
    print("OK" if not errors else f"{len(errors)} error(s)")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
