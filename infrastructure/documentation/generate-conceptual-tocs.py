#!/usr/bin/env python3
"""Inject auto-generated sidebar items for docs/guides, docs/notes, and docs/adr into the root DocFX toc."""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
DOCFX_DIR = ROOT / "infrastructure" / "documentation"
TOC_TEMPLATE = DOCFX_DIR / "toc.template.yml"
ROOT_TOC = DOCFX_DIR / "toc.yml"

SECTIONS: tuple[tuple[str, Path, str], ...] = (
    ("__GENERATED_GUIDES_ITEMS__", ROOT / "docs" / "guides", "../../docs/guides"),
    ("__GENERATED_NOTES_ITEMS__", ROOT / "docs" / "notes", "../../docs/notes"),
    ("__GENERATED_ADR_ITEMS__", ROOT / "docs" / "adr", "../../docs/adr"),
)

H1_PATTERN = re.compile(r"^#\s+(.+?)\s*$", re.MULTILINE)


def title_from_markdown(path: Path) -> str:
    text = path.read_text(encoding="utf-8")
    match = H1_PATTERN.search(text)
    if not match:
        return path.stem.replace("-", " ").title()

    return match.group(1).strip()


def yaml_quoted(value: str) -> str:
    return json.dumps(value, ensure_ascii=False)


def build_items(section_dir: Path, docs_href_prefix: str) -> str:
    lines: list[str] = []

    for path in sorted(section_dir.glob("*.md")):
        if path.name.upper() == "README.MD":
            continue
        title = title_from_markdown(path)
        href = f"{docs_href_prefix}/{path.name}"
        lines.append(f"    - name: {yaml_quoted(title)}")
        lines.append(f"      href: {href}")

    return "\n".join(lines)


def inject_marker(template_text: str, marker: str, items_yaml: str) -> str:
    if marker not in template_text:
        print(f"error: marker {marker!r} not found in {TOC_TEMPLATE.relative_to(ROOT)}", file=sys.stderr)
        return template_text

    replacement = items_yaml if items_yaml else "    []"
    return template_text.replace(marker, replacement)


def main() -> int:
    if not TOC_TEMPLATE.is_file():
        print(f"error: missing {TOC_TEMPLATE.relative_to(ROOT)}", file=sys.stderr)
        return 1

    toc_text = TOC_TEMPLATE.read_text(encoding="utf-8")

    for marker, section_dir, href_prefix in SECTIONS:
        if not section_dir.is_dir():
            print(f"error: missing directory {section_dir}", file=sys.stderr)
            return 1
        toc_text = inject_marker(toc_text, marker, build_items(section_dir, href_prefix))

    ROOT_TOC.write_text(toc_text, encoding="utf-8")
    print(f"Wrote {ROOT_TOC.relative_to(ROOT)} from {TOC_TEMPLATE.relative_to(ROOT)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
