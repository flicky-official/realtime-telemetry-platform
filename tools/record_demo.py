"""Record docs/demo.gif: live HUD -> signal lost -> recovery.

Usage (from repo root): python tools/record_demo.py
Requires: playwright (+ chromium), ffmpeg, docker compose.
"""
import pathlib
import shutil
import subprocess
import tempfile

from playwright.sync_api import sync_playwright

ROOT = pathlib.Path(__file__).resolve().parent.parent
URL = "http://localhost:8080"
W, H = 960, 600
FPS, OUT_WIDTH = 15, 720          # lower these if the GIF is over ~8 MB


def compose(*args):
    subprocess.run(["docker", "compose", *args], cwd=ROOT, check=True)


compose("up", "-d")
tmp = pathlib.Path(tempfile.mkdtemp())

with sync_playwright() as p:
    browser = p.chromium.launch()
    ctx = browser.new_context(
        viewport={"width": W, "height": H},
        record_video_dir=str(tmp),
        record_video_size={"width": W, "height": H},
    )
    page = ctx.new_page()
    page.goto(URL)
    page.wait_for_timeout(5500)      # live
    compose("stop", "simulator")
    page.wait_for_timeout(3500)      # signal lost
    compose("start", "simulator")
    page.wait_for_timeout(3000)      # recovery
    video = page.video.path()
    ctx.close()
    browser.close()

out = ROOT / "docs" / "demo.gif"
out.parent.mkdir(exist_ok=True)
vf = (
    f"fps={FPS},scale={OUT_WIDTH}:-1:flags=lanczos,split[a][b];"
    "[a]palettegen=max_colors=64[p];[b][p]paletteuse=dither=bayer:bayer_scale=4"
)
subprocess.run(
    ["ffmpeg", "-y", "-ss", "1.0", "-i", video, "-vf", vf, "-loop", "0", str(out)],
    check=True,
)
shutil.rmtree(tmp, ignore_errors=True)
print(f"wrote {out} ({out.stat().st_size / 1e6:.1f} MB)")
