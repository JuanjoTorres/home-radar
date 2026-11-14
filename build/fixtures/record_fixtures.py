"""Records raw HTML fixtures for source adapters.

Runs in GitHub Actions (the development sandbox cannot reach the portals). Honours robots.txt with the
same User-Agent the production fetcher uses, waits between requests, and never retries on 403/429.
"""
from __future__ import annotations

import json
import re
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
import urllib.robotparser
from datetime import datetime, timezone
from pathlib import Path

import yaml

USER_AGENT = "HomeRadarBot/0.1 (+https://github.com/JuanjoTorres/home-radar; personal, non-commercial)"
DELAY_SECONDS = 4.0


def robots_allows(url: str, cache: dict[str, urllib.robotparser.RobotFileParser]) -> bool:
    parts = urllib.parse.urlsplit(url)
    origin = f"{parts.scheme}://{parts.netloc}"
    parser = cache.get(origin)
    if parser is None:
        parser = urllib.robotparser.RobotFileParser()
        parser.set_url(f"{origin}/robots.txt")
        try:
            request = urllib.request.Request(f"{origin}/robots.txt", headers={"User-Agent": USER_AGENT})
            with urllib.request.urlopen(request, timeout=30) as response:
                parser.parse(response.read().decode("utf-8", errors="replace").splitlines())
        except urllib.error.HTTPError as error:
            parser.parse([] if error.code in (404, 410) else ["User-agent: *", "Disallow: /"])
        except Exception:
            parser.parse(["User-agent: *", "Disallow: /"])
        cache[origin] = parser
    return parser.can_fetch(USER_AGENT, url)


def fetch(url: str) -> tuple[int, dict[str, str], bytes, str]:
    request = urllib.request.Request(url, headers={"User-Agent": USER_AGENT, "Accept": "text/html,application/xhtml+xml", "Accept-Language": "es-ES,es;q=0.9"})
    try:
        with urllib.request.urlopen(request, timeout=60) as response:
            return response.status, dict(response.headers.items()), response.read(), response.geturl()
    except urllib.error.HTTPError as error:
        return error.code, dict(error.headers.items()), error.read() if error.fp else b"", url


def save(target_dir: Path, name: str, url: str, status: int, headers: dict[str, str], body: bytes, final_url: str) -> None:
    target_dir.mkdir(parents=True, exist_ok=True)
    (target_dir / f"{name}.html").write_bytes(body)
    meta = {
        "url": url,
        "finalUrl": final_url,
        "status": status,
        "recordedAt": datetime.now(timezone.utc).isoformat(),
        "contentType": headers.get("Content-Type"),
        "etag": headers.get("ETag"),
        "lastModified": headers.get("Last-Modified"),
        "cacheControl": headers.get("Cache-Control"),
        "server": headers.get("Server"),
        "bytes": len(body),
    }
    (target_dir / f"{name}.meta.json").write_text(json.dumps(meta, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def first_link(body: bytes, pattern: str, base_url: str) -> str | None:
    match = re.search(pattern, body.decode("utf-8", errors="replace"))
    return urllib.parse.urljoin(base_url, match.group(0).replace("&amp;", "&")) if match else None


def main(manifest_path: str, root: str, only: str | None) -> int:
    manifest = yaml.safe_load(Path(manifest_path).read_text(encoding="utf-8"))
    robots_cache: dict[str, urllib.robotparser.RobotFileParser] = {}
    failures = 0
    for source in manifest["sources"]:
        if only and source["key"] != only:
            continue
        target_dir = Path(root) / source["key"]
        for page in source["pages"]:
            url = page["url"]
            if not robots_allows(url, robots_cache):
                print(f"[{source['key']}] robots.txt disallows {url}; skipped", flush=True)
                save(target_dir, page["name"], url, 0, {}, b"", url)
                continue
            status, headers, body, final_url = fetch(url)
            print(f"[{source['key']}] {page['name']}: {status} {len(body)} bytes", flush=True)
            save(target_dir, page["name"], url, status, headers, body, final_url)
            if status >= 400:
                failures += 1
            time.sleep(DELAY_SECONDS)
            follow = page.get("followFirst")
            if follow and status == 200:
                detail_url = first_link(body, follow["pattern"], final_url)
                if detail_url and robots_allows(detail_url, robots_cache):
                    d_status, d_headers, d_body, d_final = fetch(detail_url)
                    print(f"[{source['key']}] {follow['name']}: {d_status} {len(d_body)} bytes ({detail_url})", flush=True)
                    save(target_dir, follow["name"], detail_url, d_status, d_headers, d_body, d_final)
                    time.sleep(DELAY_SECONDS)
                else:
                    print(f"[{source['key']}] no detail link matched {follow['pattern']} or robots disallowed", flush=True)
    return 1 if failures else 0


if __name__ == "__main__":
    only_source = sys.argv[3] if len(sys.argv) > 3 and sys.argv[3] else None
    sys.exit(main(sys.argv[1], sys.argv[2], only_source))
