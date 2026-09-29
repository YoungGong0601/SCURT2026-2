# Art (원본 그림)

> **아트 원본 규칙과 변환 방법**의 단일 출처다.

Unity 밖에 두는 **원본(source) 그림** 폴더. Unity는 `Assets/Art/`의 PNG만 사용한다.
`Art/<폴더>/<이름>.svg` 하나가 `Assets/Art/<폴더>/<이름>.png` 하나(16×16)가 된다.

| 원본 (여기) | 결과 PNG | 미리보기 (여기) |
|---|---|---|
| `Player/player_down.svg` 외 3개 | `Assets/Art/Player/player_down.png` 외 3개 | `Player/preview.png` (8배 확대, 아래, 위, 왼쪽, 오른쪽 순) |
| `Tiles/water.svg`, `sand.svg`, `grass.svg`, `forest.svg` | `Assets/Art/Tiles/water.png` 외 3개 | `Tiles/preview.png` (8배 확대, 물, 모래, 풀, 숲 순), `Tiles/preview-mosaic.png` (예시 지도) |

Tile 에셋(`Assets/Art/Tiles/Water.asset`, `Sand.asset`, `Grass.asset`, `Forest.asset`)은 같은 폴더의 PNG 스프라이트를 가리킨다.
PNG를 다시 만들어도 스프라이트 연결은 그대로 유지된다.

## SVG 규칙
- 16×16 격자, `<rect>`만 사용, 좌표와 크기는 정수, `shape-rendering="crispEdges"`
- 나중에 적힌 `<rect>`가 앞의 것을 덮어 칠한다 (외곽선, 몸통, 그림자, 눈 순서)
- 타일은 추가로:
  - 첫 `<rect>`로 16×16 전체를 한 색으로 채우고 테두리를 두지 않는다 (이어 붙였을 때 격자선이 보이지 않게).
  - 무늬(물결, 모래알, 풀잎, 덤불)는 바탕보다 조금 밝거나 어두운 색으로, 가장자리에 닿지 않게 안쪽에 둔다.
  - 흰 플레이어가 항상 눈에 띄도록 중간 밝기의 차분한 색만 쓴다 (순백, 검정에 가까운 색 금지).
    멀리서도 구분되게 밝기 순서를 지킨다: 모래 > 풀 > 물 > 숲.

## PNG 다시 만들기
SVG를 고친 뒤, 저장소 루트에서:

```
python sub-tools/svg2png.py
```

- 필요: Python 3 + Pillow (`pip install pillow`)
- `Art/` 아래 SVG가 있는 **모든 폴더**를 변환하고, 폴더마다 `preview.png`를 만든다.
  새 폴더(예: `Art/Items/`)를 만들면 `Assets/Art/Items/`로 그대로 따라온다.
- 미리보기에 놓는 순서는 `sub-tools/svg2png.py`의 `ORDER`에서 정한다 (적지 않은 파일은 이름순으로 뒤에 붙는다).
- `Tiles` 폴더는 `preview-mosaic.png`도 만든다: 타일 4종을 덩어리 모양으로 이어 붙인 24×14칸 예시 지도(4배 확대) 가운데에 플레이어를 놓은 그림.
  이음매가 보이지 않는지, 플레이어가 어느 타일 위에서도 잘 보이는지 확인하는 용도이며 실제 맵 생성과는 무관하다.
- `sub-tools/svg2png.py`는 SVG의 `<rect>`를 그대로 픽셀로 칠한다 (정수 격자라 오차 없음).
  `<rect>` 외의 도형(원, 경로 등)은 무시되므로 쓰지 않는다.
- PNG를 덮어쓰면 Unity가 자동으로 다시 가져온다. 가져오기 설정은 각 PNG의 `.meta`에 저장되어 그대로 유지된다
  (설정값의 근거: [ARCHITECTURE.md 설계 결정](../ARCHITECTURE.md#3-설계-결정)).
- 새 폴더의 PNG는 처음 한 번, 실행 중인 에디터에 명령을 보내 플레이어 PNG와 같은 가져오기 설정을 준다
  (Sprite, Single, Pixels Per Unit 16, Point 필터, 압축 없음, 밉맵 없음).
