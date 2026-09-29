# Art/Player/player_*.svg → Assets/Art/Player/player_*.png (16×16) + Art/Player/preview.png
# SVG는 정수 좌표의 <rect>만 쓰므로, rect를 그대로 픽셀로 칠하면 정확히 변환된다.
# 실행: python sub-tools/svg2png.py  (어느 폴더에서 실행해도 같은 결과)
import xml.etree.ElementTree as ET
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent.parent  # 저장소 루트 (이 파일 = 루트/sub-tools/svg2png.py)
SRC = ROOT / "Art" / "Player"
DST = ROOT / "Assets" / "Art" / "Player"
DST.mkdir(parents=True, exist_ok=True)

sprites = []
for name in ["down", "up", "left", "right"]:
    svg = ET.parse(SRC / f"player_{name}.svg").getroot()
    img = Image.new("RGBA", (int(svg.get("width")), int(svg.get("height"))), (0, 0, 0, 0))  # 투명 배경
    draw = ImageDraw.Draw(img)
    for r in svg.iter("{http://www.w3.org/2000/svg}rect"):  # 파일에 적힌 순서대로 덧칠
        x, y, w, h = (int(r.get(k)) for k in ("x", "y", "width", "height"))
        draw.rectangle([x, y, x + w - 1, y + h - 1], fill=r.get("fill"))  # 끝 좌표 포함이라 -1
    img.save(DST / f"player_{name}.png")
    sprites.append(img)

# 미리보기: 아래/위/왼쪽/오른쪽 순서로 8배 확대(nearest)해 회색 배경 위에 나란히
S = 8
preview = Image.new("RGBA", (4 * 20 * S, 20 * S), (128, 128, 128, 255))  # 한 칸 = 20px(16 + 여백 2×2)
for i, img in enumerate(sprites):
    big = img.resize((16 * S, 16 * S), Image.NEAREST)
    preview.alpha_composite(big, ((i * 20 + 2) * S, 2 * S))
preview.save(SRC / "preview.png")
print("done:", DST.relative_to(ROOT).as_posix())
