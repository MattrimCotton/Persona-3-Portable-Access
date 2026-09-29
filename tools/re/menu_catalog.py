"""Build an offline P3P menu image catalogue from common + French game archives.

All copyrighted outputs stay under extracted/. No game/mod deployment or live memory access.
Usage: python tools/re/menu_catalog.py --game D:/SteamLibrary/steamapps/common/P3P
"""
from __future__ import annotations

import argparse
from collections import Counter
from datetime import datetime, timezone
from functools import lru_cache
import hashlib
import html
import io
import json
from pathlib import Path
import struct
import subprocess
import sys

from PIL import Image, ImageDraw, ImageFont

from pakunpack import parse
from spr2png import decode_tmx

ROOT = Path(__file__).resolve().parents[2]
CATALOG_VERSION = 2
UI_ROOTS = {"battle", "camp", "card", "commu", "dict", "facility", "font", "help",
            "limit", "lmap", "memcard", "oped", "scheduler", "smap", "title", "win"}
LABELS = {
    "camp": "Menu pause, compétences, objets, équipement, Persona, statut et système",
    "title": "Écran titre, personnage, difficulté, licences et crédits de localisation",
    "dict": "Saisie du nom",
    "memcard": "Sauvegarde et chargement",
    "facility": "Boutiques, Velvet Room, fusion et requêtes",
    "commu": "Liens sociaux",
    "battle": "Menus de combat et résultats",
    "card": "Cartes et Shuffle Time",
    "calendar": "Date, heure et calendrier",
    "maps": "Cartes et navigation",
    "shared": "Interface commune, fenêtres, boutons et configuration",
    "other": "Autres éléments et crédits",
}


def dump(path, obj):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(obj, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def run(command):
    result = subprocess.run(command, cwd=ROOT, capture_output=True, text=True,
                            encoding="utf-8", errors="replace")
    if result.returncode:
        raise RuntimeError(result.stdout + result.stderr)
    return result.stdout


def selected(path):
    p = path.lower()
    return p.split('/')[0] in UI_ROOTS or p.startswith('init') or p.endswith('.spr')


def category(source):
    p = source.lower().replace('!', '/')
    parts = set(p.split('/'))
    if 'camp' in parts or 'camp.bin' in parts or any(s in p for s in ('c_main', 'c_status', 'c_skill', 'c_item', 'c_equip')):
        return 'camp'
    for key in ('title', 'dict', 'memcard', 'facility', 'commu', 'battle', 'card'):
        if key in parts:
            return key
    if any(s in p for s in ('calendar', '/date/', 'blbrd', 'schedule', 'drktime')):
        return 'calendar'
    if parts & {'smap', 'lmap', 'field2d'}:
        return 'maps'
    if any(s in p for s in ('init', 'font', 'window', 'sysdat', '/win/')):
        return 'shared'
    return 'other'


def checked_slice(data, offset, length):
    if offset < 0 or offset + length > len(data):
        raise ValueError(f'Invalid range {offset}+{length} in {len(data)} bytes')
    return data[offset:offset + length]


def spr_records(data):
    checked_slice(data, 0, 0x20)
    tc, sc, to, so = struct.unpack_from('<HHII', data, 0x14)
    checked_slice(data, to, tc * 8)
    checked_slice(data, so, sc * 8)
    textures = [struct.unpack_from('<I', data, to + i * 8 + 4)[0] for i in range(tc)]
    sprites = []
    for i in range(sc):
        offset = struct.unpack_from('<I', data, so + i * 8 + 4)[0]
        checked_slice(data, offset, 0x64)
        raw_name = data[offset + 4:offset + 0x14].split(b'\0')[0]
        name = raw_name.decode('shift_jis', errors='replace')
        sprites.append({'index': i, 'name': name, 'offset': offset,
                        'texture': struct.unpack_from('<i', data, offset + 0x14)[0],
                        'rect_psp': list(struct.unpack_from('<4i', data, offset + 0x54))})
    return textures, sprites


def flatten(data, source, depth=0):
    if depth > 16:
        raise ValueError(f'Archive nesting too deep: {source}')
    if data[8:12] in (b'SPR0', b'TMX0') or data.startswith((b'\x89PNG', b'DDS ')):
        yield source, data
        return
    entries = parse_counted(data) or parse(data)
    if entries:
        for name, blob in entries:
            yield from flatten(blob, source + '!' + name, depth + 1)
        return
    # Keep embedded textures even if their containing format is not a PAK.
    pos = 0
    while (pos := data.find(b'TMX0', pos)) >= 0:
        offset = pos - 8
        if offset >= 0:
            size = struct.unpack_from('<I', data, offset + 4)[0]
            if size >= 0x40 and offset + size <= len(data):
                yield f'{source}!embedded_{offset:08x}.tmx', data[offset:offset + size]
        pos += 4


def parse_counted(data):
    """P4G AreaArcUnpack layout also used by P3P camp.bin/name.bin: count, name[32], size, data."""
    if len(data) < 40:
        return None
    count = struct.unpack_from('<I', data)[0]
    if not 0 < count <= (len(data) - 4) // 36:
        return None
    off, entries = 4, []
    for _ in range(count):
        if off + 36 > len(data):
            return None
        raw = data[off:off + 32].split(b'\0')[0]
        try:
            name = raw.decode('ascii')
        except UnicodeDecodeError:
            return None
        size = struct.unpack_from('<I', data, off + 32)[0]
        start = off + 36
        if not name or not name.isprintable() or start + size > len(data):
            return None
        entries.append((name, data[start:start + size]))
        off = start + size
    if any(data[off:]):
        return None
    return entries


@lru_cache(maxsize=8)
def font(size):
    for name in ('C:/Windows/Fonts/YuGothM.ttc', 'C:/Windows/Fonts/arial.ttf', 'DejaVuSans.ttf'):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            pass
    return ImageFont.load_default()


def sheet(items, target, title, per_page=30):
    """items: (PIL image, short label); save readable bounded contact sheets."""
    files = []
    for first in range(0, len(items), per_page):
        chunk = items[first:first + per_page]
        cols, cw, ch = 5, 260, 150
        canvas = Image.new('RGB', (cols * cw, 55 + ((len(chunk) + cols - 1) // cols) * ch), '#20242a')
        draw = ImageDraw.Draw(canvas)
        draw.text((12, 12), title[:120], fill='white', font=font(19))
        for j, (img, label) in enumerate(chunk):
            x, y = (j % cols) * cw, 55 + (j // cols) * ch
            thumb = img.copy()
            thumb.thumbnail((cw - 16, ch - 42))
            if thumb.mode == 'RGBA':
                canvas.paste(thumb, (x + 8, y + 5), thumb)
            else:
                canvas.paste(thumb.convert('RGB'), (x + 8, y + 5))
            draw.text((x + 8, y + ch - 33), label[:34], fill='white', font=font(13))
        out = target.parent / f'{target.name}_{first // per_page:03}.png'
        out.parent.mkdir(parents=True, exist_ok=True)
        canvas.save(out)
        files.append(out)
    return files


def make_asset(data, source, output):
    digest = hashlib.sha256(data).hexdigest()
    asset_dir = output / 'assets' / digest[:20]
    asset_dir.mkdir(parents=True, exist_ok=True)
    metadata = asset_dir / 'metadata.json'
    if metadata.exists():
        cached = json.loads(metadata.read_text(encoding='utf-8'))
        paths = [t['png'] for t in cached['textures']] + [s['png'] for s in cached['sprites'] if 'png' in s] + cached['sheets']
        if cached.get('version') == CATALOG_VERSION and not cached['issues'] and all((output / p).exists() for p in paths):
            cached['sources'] = [source]
            cached['category'] = category(source)
            return cached
    record = {'version': CATALOG_VERSION, 'id': digest[:20], 'sha256': digest, 'sources': [source],
              'category': category(source), 'bytes': len(data), 'textures': [], 'sprites': [], 'issues': []}
    kind = data[8:12]
    if kind == b'SPR0':
        offsets, record['sprites'] = spr_records(data)
    elif kind == b'TMX0':
        offsets = [0]
    else:
        offsets = [None]
    images = {}
    for index, offset in enumerate(offsets):
        try:
            if offset is None:
                with Image.open(io.BytesIO(data)) as original:
                    img = original.convert('RGBA')
            else:
                img = decode_tmx(data, offset)
            target = asset_dir / f'texture_{index:03}.png'
            img.save(target)
            images[index] = img
            record['textures'].append({'index': index, 'offset': offset, 'width': img.width,
                                       'height': img.height, 'png': target.relative_to(output).as_posix(),
                                       'format': None if offset is None else data[offset + 0x16]})
        except (ValueError, OSError, struct.error) as exc:
            record['issues'].append(f'Texture {index}: {exc}')
    crops = []
    for sprite in record['sprites']:
        image = images.get(sprite['texture'])
        x1, y1, x2, y2 = sprite['rect_psp']
        if image is None:
            sprite['status'] = 'no_texture'
            continue
        # The PC remaster uses x4 PSP atlas coordinates (documented in NAME_ENTRY.md).
        bounds = [min(x1, x2) * 4, min(y1, y2) * 4, max(x1, x2) * 4, max(y1, y2) * 4]
        sprite['rect_pc'] = bounds
        if x1 == x2 or y1 == y2:
            sprite['status'] = 'empty_rectangle'
            continue
        if bounds[0] < 0 or bounds[1] < 0 or bounds[2] > image.width or bounds[3] > image.height:
            sprite['status'] = 'bounds_unresolved'
            continue
        crop = image.crop(bounds)
        if x2 < x1:
            crop = crop.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
        if y2 < y1:
            crop = crop.transpose(Image.Transpose.FLIP_TOP_BOTTOM)
        target = asset_dir / f'sprite_{sprite["index"]:04}.png'
        crop.save(target)
        sprite['status'] = 'exported'
        sprite['png'] = target.relative_to(output).as_posix()
        crops.append((crop, f'{sprite["index"]}: {sprite["name"]}'))
    record['sheets'] = [p.relative_to(output).as_posix() for p in sheet(crops, asset_dir / 'sprites', source.split('!')[-1])]
    dump(asset_dir / 'metadata.json', record)
    return record


STYLE = '<style>body{font:18px system-ui;max-width:1100px;margin:2em auto;padding:1em}img{max-width:100%;height:auto;background:#282828}a{color:#124b9b}li{margin:.6em 0}code{overflow-wrap:anywhere}figure{margin:1em 0}summary{cursor:pointer}</style>'


def page(title, body):
    return '<!doctype html><html lang="fr"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>' + html.escape(title) + '</title>' + STYLE + '<body><h1>' + html.escape(title) + '</h1>' + body + '</body></html>'


def write_gallery(output, records, summary):
    body = '<p>Images extraites des ressources du jeu, pas des captures des écrans assemblés en jeu. Français et ressources communes. Les noms internes ne constituent pas une transcription des libellés visibles.</p>'
    body += f'<p>{summary["unique_assets"]} ressources uniques, {summary["textures"]} planches et {summary["sprite_exports"]} éléments découpés.</p><ul>'
    index = ['# Images de menus P3P : index local', '',
             'Ressources extraites, pas des captures des écrans assemblés en jeu.',
             'FR = data_FR ; data = ressources communes, pouvant contenir du japonais.',
             'Les noms internes ne sont pas une transcription des textes visibles.', '',
             f'{summary["unique_assets"]} ressources uniques ; {summary["textures"]} planches ; '
             f'{summary["sprite_exports"]} éléments découpés.', '',
             '[Galerie](index.html) · [Manifest](manifest.json) · [Couverture](coverage.json)', '']
    for cat, label in LABELS.items():
        assets = [r for r in records if r['category'] == cat]
        if not assets:
            continue
        body += f'<li><a href="{cat}.html">{html.escape(label)}</a> : {len(assets)} ressources</li>'
        index.extend([f'## {label}', ''])
        content = '<p><a href="index.html">Retour à l’index</a></p>'
        for r in assets:
            title = html.escape(r['sources'][0])
            content += f'<section id="{r["id"]}"><h2><code>{title}</code></h2>'
            content += f'<p><a href="assets/{r["id"]}/metadata.json">Métadonnées et rectangles des éléments</a></p>'
            index.append(f'- `{r["sources"][0]}` : [images]({cat}.html#{r["id"]}), '
                         f'[métadonnées](assets/{r["id"]}/metadata.json), '
                         f'{len(r["textures"])} planches, {len(r["sprites"])} éléments déclarés.')
            for source in r['sources'][1:]:
                index.append(f'  Copie identique : `{source}`.')
            if len(r['sources']) > 1:
                content += '<details><summary>Autres emplacements identiques</summary><ul>' + ''.join('<li><code>' + html.escape(s) + '</code></li>' for s in r['sources'][1:]) + '</ul></details>'
            for t in r['textures']:
                alt = f'Planche {t["index"]}, {t["width"]} par {t["height"]}, {r["sources"][0]}'
                content += f'<figure><a href="{t["png"]}"><img loading="lazy" src="{t["png"]}" alt="{html.escape(alt, quote=True)}" width="600"></a><figcaption>{html.escape(alt)}</figcaption></figure>'
            if r['sheets']:
                content += '<details><summary>Éléments découpés avec leurs numéros</summary>'
                content += ''.join(f'<a href="{s}"><img loading="lazy" src="{s}" alt="Planche des éléments numérotés"></a>' for s in r['sheets']) + '</details>'
            if r['issues']:
                content += '<p>Limites : ' + html.escape('; '.join(r['issues'])) + '</p>'
            skipped = Counter(s['status'] for s in r['sprites'] if s['status'] != 'exported')
            if skipped:
                content += '<p>Éléments sans découpe : ' + html.escape(str(dict(skipped))) + '. Voir les métadonnées ; les planches complètes sont conservées.</p>'
            content += '</section>'
        (output / f'{cat}.html').write_text(page(label, content), encoding='utf-8')
        index.append('')
    body += '</ul><p><a href="manifest.json">Inventaire complet JSON</a> · <a href="coverage.json">Couverture des archives et exclusions</a></p>'
    (output / 'index.html').write_text(page('Catalogue local des images de menus P3P', body), encoding='utf-8')
    (output / 'INDEX.md').write_text('\n'.join(index) + '\n', encoding='utf-8')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--game', type=Path, required=True)
    parser.add_argument('--output', type=Path, default=ROOT / 'extracted/menu_catalog')
    parser.add_argument('--refresh-sheets', action='store_true', help='Rebuild sprite contact sheets from exported crops.')
    parser.add_argument('--reextract', action='store_true', help='Refresh the source cache after a game update.')
    args = parser.parse_args()
    output = args.output.resolve()
    if not output.is_relative_to((ROOT / 'extracted').resolve()):
        parser.error('Output must stay under this repository’s extracted/ directory.')
    output.mkdir(parents=True, exist_ok=True)
    print('Building archive extractor (not the mod)...', flush=True)
    run(['dotnet', 'build', 'tools/re/CpkExtract', '-v', 'quiet'])
    dll = ROOT / 'tools/re/CpkExtract/bin/Debug/net9.0/CpkExtract.dll'
    if not dll.exists():
        dll = next((ROOT / 'tools/re/CpkExtract/bin/Debug').rglob('CpkExtract.dll'))
    coverage, inputs, errors = [], [], []
    for folder in ('data', 'data_FR', 'sysdat'):
        archives = sorted((args.game / folder).glob('*.cpk'))
        if not archives:
            raise FileNotFoundError(f'No archive found in {args.game / folder}')
        for archive in archives:
            name = folder + '/' + archive.name
            raw_dir = output / 'sources' / folder / archive.stem
            listing = run(['dotnet', str(dll), str(archive), str(raw_dir)])
            entries = []
            for line in listing.splitlines():
                if '\t' not in line:
                    continue
                path, size = line.rsplit('\t', 1)
                include = folder == 'sysdat' or selected(path)
                entries.append({'path': path, 'bytes': int(size), 'selected': include})
            if not entries:
                raise ValueError(f'Empty CPK inventory: {name}')
            dump(output / 'inventories' / f'{folder}_{archive.stem}.json', entries)
            wanted = [e for e in entries if e['selected']]
            coverage.append({'archive': name, 'files': len(entries), 'selected': len(wanted),
                             'selected_bytes': sum(e['bytes'] for e in wanted)})
            if wanted:
                print(f'Extracting {name}: {len(wanted)} files...', flush=True)
                # Cache is private to this catalogue. Redo extraction if a file is missing or size changed.
                if args.reextract or any(not (raw_dir / e['path']).exists() or (raw_dir / e['path']).stat().st_size != e['bytes'] for e in wanted):
                    filters = '.' if folder == 'sysdat' else ','.join(sorted(p + '/' for p in UI_ROOTS)) + ',init,.spr'
                    run(['dotnet', str(dll), str(archive), str(raw_dir), filters])
                for e in wanted:
                    inputs.append((name + '!' + e['path'], raw_dir / e['path']))
    records, by_hash, occurrence_count = [], {}, 0
    for number, (source, path) in enumerate(inputs):
        if number % 50 == 0:
            print(f'Inspecting {number}/{len(inputs)} files; {len(records)} unique image resources...', flush=True)
        try:
            for origin, data in flatten(path.read_bytes(), source):
                occurrence_count += 1
                digest = hashlib.sha256(data).hexdigest()
                if digest in by_hash:
                    by_hash[digest]['sources'].append(origin)
                    continue
                record = make_asset(data, origin, output)
                by_hash[digest] = record
                records.append(record)
        except (ValueError, OSError, struct.error) as exc:
            errors.append({'source': source, 'error': str(exc)})
    for record in records:
        if args.refresh_sheets:
            crops = []
            for sprite in record['sprites']:
                if 'png' in sprite:
                    with Image.open(output / sprite['png']) as img:
                        crops.append((img.copy(), f'{sprite["index"]}: {sprite["name"]}'))
            record['sheets'] = [p.relative_to(output).as_posix() for p in sheet(
                crops, output / 'assets' / record['id'] / 'sprites', record['sources'][0].split('!')[-1])]
        dump(output / 'assets' / record['id'] / 'metadata.json', record)
    statuses = Counter(s['status'] for r in records for s in r['sprites'])
    summary = {'created_utc': datetime.now(timezone.utc).isoformat(), 'archives': coverage,
               'input_files': len(inputs), 'image_occurrences': occurrence_count,
               'unique_assets': len(records), 'textures': sum(len(r['textures']) for r in records),
               'sprites': sum(len(r['sprites']) for r in records), 'sprite_exports': statuses['exported'],
               'sprite_statuses': dict(statuses), 'errors': errors,
               'texture_issues': sum(len(r['issues']) for r in records),
               'categories': dict(Counter(r['category'] for r in records))}
    dump(output / 'manifest.json', {'summary': summary, 'assets': records})
    dump(output / 'coverage.json', {'selection': sorted(UI_ROOTS), 'additional_selection': ['init*', '*.spr', 'all sysdat'],
         'scope': 'Common assets and French overrides. Other language archives are not included.',
         'limits': ['Resources are not assembled screenshots of live screens.',
                    'Sprite crops use documented PSP-to-PC scale x4; unresolved bounds are recorded.',
                    'Other game resources (scenes, 3D models, voices, music) are outside the menu catalogue.'],
         'summary': summary})
    write_gallery(output, records, summary)
    print(json.dumps(summary, ensure_ascii=False, indent=2), flush=True)
    print(f'Gallery: {output / "index.html"}', flush=True)
    return 1 if errors or summary['texture_issues'] else 0


if __name__ == '__main__':
    sys.exit(main())
