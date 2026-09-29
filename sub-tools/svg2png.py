# Art/<폴더>/*.svg → Assets/Art/<폴더>/*.png (16×16) + Art/<폴더>/preview.png (8배 확대, 나란히)
# Tiles 폴더는 Art/Tiles/preview-mosaic.png도 만든다 (타일을 이어 붙인 작은 예시 지도 + 가운데 플레이어).
# SVG는 정수 좌표의 <rect>만 쓰므로, rect를 그대로 픽셀로 칠하면 정확히 변환된다.
# 실행: python sub-tools/svg2png.py  (어느 폴더에서 실행해도 같은 결과)
import math
import xml.etree.ElementTree as ET
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent.parent  # 저장소 루트 (이 파일 = 루트/sub-tools/svg2png.py)
ART = ROOT / "Art"                  # 원본 SVG 폴더들
OUT = ROOT / "Assets" / "Art"       # 게임용 PNG 폴더들
# 미리보기에 놓는 순서 (파일 이름에서 .svg를 뺀 것). 여기 없는 파일은 뒤에 이름순으로 붙는다.
ORDER = {
    "Player": ["player_down", "player_up", "player_left", "player_right"],
    "Tiles": ["water", "sand", "grass", "forest"],  # 낮은 곳부터 높은 곳 순 (맵 생성의 타일 순서와 같음)
}


def render(svg_path):
    """SVG 하나를 같은 크기의 RGBA 이미지로 칠한다."""
    svg = ET.parse(svg_path).getroot()
    img = Image.new("RGBA", (int(svg.get("width")), int(svg.get("height"))), (0, 0, 0, 0))  # 투명 배경
    draw = ImageDraw.Draw(img)
    for r in svg.iter("{http://www.w3.org/2000/svg}rect"):  # 파일에 적힌 순서대로 덧칠
        x, y, w, h = (int(r.get(k)) for k in ("x", "y", "width", "height"))
        draw.rectangle([x, y, x + w - 1, y + h - 1], fill=r.get("fill"))  # 끝 좌표 포함이라 -1
    return img


def save_preview(sprites, path):
    """16×16 그림들을 8배 확대(nearest)해 회색 배경 위에 나란히 놓는다."""
    S = 8
    preview = Image.new("RGBA", (len(sprites) * 20 * S, 20 * S), (128, 128, 128, 255))  # 한 칸 = 20px(16 + 여백 2×2)
    for i, img in enumerate(sprites):
        big = img.resize((16 * S, 16 * S), Image.NEAREST)
        preview.alpha_composite(big, ((i * 20 + 2) * S, 2 * S))
    preview.save(path)


def save_mosaic(tiles, player, path):
    """타일 이음매와 대비를 보기 위한 예시 지도: 24×14칸, 가운데 플레이어, 4배 확대."""
    W, H, S = 24, 14, 4
    # 부드러운 높이 값: 사인파 몇 개를 더해 덩어리(blob) 모양을 만든다. 실제 맵 생성과는 무관한 예시.
    def height(x, y):
        return (math.sin(x * 0.42 + 1.0) + math.sin(y * 0.55 + 0.3)
                + 0.8 * math.sin((x + y) * 0.28) + 0.6 * math.sin((x - 1.6 * y) * 0.22 + 2.0))
    ranked = sorted(height(x, y) for y in range(H) for x in range(W))
    cuts = [ranked[len(ranked) * k // len(tiles)] for k in range(1, len(tiles))]  # 타일 종류마다 넓이가 비슷하도록 나눔
    world = Image.new("RGBA", (W * 16, H * 16))
    for y in range(H):
        for x in range(W):
            level = sum(height(x, y) >= c for c in cuts)  # 낮으면 물, 높으면 숲
            world.alpha_composite(tiles[level], (x * 16, y * 16))
    world.alpha_composite(player, (W // 2 * 16, H // 2 * 16))
    world.resize((W * 16 * S, H * 16 * S), Image.NEAREST).save(path)


for src in sorted(p for p in ART.iterdir() if p.is_dir()):
    order = ORDER.get(src.name, [])
    svgs = sorted(src.glob("*.svg"), key=lambda p: (order.index(p.stem) if p.stem in order else len(order), p.stem))
    if not svgs:
        continue
    dst = OUT / src.name
    dst.mkdir(parents=True, exist_ok=True)
    sprites = []
    for svg_path in svgs:
        img = render(svg_path)
        img.save(dst / f"{svg_path.stem}.png")
        sprites.append(img)
    save_preview(sprites, src / "preview.png")
    if src.name == "Tiles":
        save_mosaic(sprites, render(ART / "Player" / "player_down.svg"), src / "preview-mosaic.png")
    print("done:", dst.relative_to(ROOT).as_posix())
