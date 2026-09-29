"""Verify catalogue links/PNG files and binary decoder edge cases, without running P3P.

Usage: python tools/re/check_menu_catalog.py [extracted/menu_catalog]
"""
import argparse
from collections import Counter
from html.parser import HTMLParser
import json
from pathlib import Path
import struct
from urllib.parse import unquote

from PIL import Image

from menu_catalog import ROOT, parse_counted
from spr2png import decode_tmx


def decoder_checks():
    # Explicit color oracle detects nibble-order, alpha and palette-order regressions.
    data = bytearray(0x40)
    data[8:12] = b'TMX0'
    data[0x10] = 1
    struct.pack_into('<HH', data, 0x12, 2, 1)
    data[0x16] = 0x14
    palette = bytearray(64)
    palette[4:8] = bytes((255, 0, 0, 128))
    palette[8:12] = bytes((0, 255, 0, 0))
    packed = bytes(data + palette + bytes((0x21,)))
    img = decode_tmx(packed)
    assert img.getpixel((0, 0)) == (255, 0, 0, 255)
    assert img.getpixel((1, 0)) == (0, 255, 0, 0)
    for truncated in (packed[:20], packed[:-1]):
        try:
            decode_tmx(truncated)
        except ValueError:
            pass
        else:
            raise AssertionError('Truncated TMX accepted')
    # A real two-level layout: no alignment between counted archive members.
    def entry(name, payload):
        return name.encode().ljust(32, b'\0') + struct.pack('<I', len(payload)) + payload
    inner = struct.pack('<I', 1) + entry('sample.tmx', packed)
    outer = struct.pack('<I', 2) + entry('nested.bin', inner) + entry('odd.txt', b'abc')
    entries = parse_counted(outer)
    assert entries and entries[1] == ('odd.txt', b'abc')
    assert parse_counted(entries[0][1]) == [('sample.tmx', packed)]
    assert parse_counted(outer[:-1]) is None
    assert parse_counted(b'\xff' * 40) is None


class Links(HTMLParser):
    def __init__(self):
        super().__init__()
        self.paths = []

    def handle_starttag(self, tag, attrs):
        for name, value in attrs:
            if name in ('src', 'href') and value:
                self.paths.append(value)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('catalogue', nargs='?', type=Path, default=ROOT / 'extracted/menu_catalog')
    args = parser.parse_args()
    output = args.catalogue.resolve()
    decoder_checks()
    manifest = json.loads((output / 'manifest.json').read_text(encoding='utf-8'))
    assets, summary = manifest['assets'], manifest['summary']
    assert len(assets) == summary['unique_assets']
    assert not summary['errors'] and not summary['texture_issues'], summary
    sprites = Counter(s['status'] for r in assets for s in r['sprites'])
    assert dict(sprites) == summary['sprite_statuses']
    assert sum(len(r['textures']) for r in assets) == summary['textures']
    assert sum(len(r['sources']) for r in assets) == summary['image_occurrences']
    pngs = set()
    for record in assets:
        assert record['textures'], record['sources']
        metadata = output / 'assets' / record['id'] / 'metadata.json'
        assert json.loads(metadata.read_text(encoding='utf-8')) == record
        for item in record['textures'] + record['sprites']:
            if 'png' not in item:
                continue
            path = (output / item['png']).resolve()
            assert path.is_relative_to(output)
            pngs.add(path)
            with Image.open(path) as image:
                image.verify()
            with Image.open(path) as image:
                if 'width' in item:
                    assert image.size == (item['width'], item['height'])
                elif 'rect_pc' in item:
                    x1, y1, x2, y2 = item['rect_pc']
                    assert image.size == (x2 - x1, y2 - y1)
        for sheet in record['sheets']:
            path = output / sheet
            with Image.open(path) as image:
                image.verify()
            pngs.add(path)
    links = 0
    for page in output.glob('*.html'):
        parsed = Links()
        parsed.feed(page.read_text(encoding='utf-8'))
        for href in parsed.paths:
            target = (page.parent / unquote(href.split('#')[0])).resolve()
            assert target.is_relative_to(output) and target.exists(), (page, href)
            links += 1
    report = {'status': 'OK', 'assets': len(assets), 'png_files_verified': len(pngs),
              'html_links_verified': links, 'decoder_edge_cases': 'OK',
              'sprite_statuses': dict(sprites), 'note': 'Offline checks only, not a game test.'}
    (output / 'verification.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(report, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()
