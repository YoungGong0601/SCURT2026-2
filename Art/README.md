# Art (원본 그림)

> **아트 원본 규칙과 변환 방법**의 단일 출처다.

Unity 밖에 두는 **원본(source) 그림** 폴더. Unity는 `Assets/Art/`의 PNG만 사용한다.

| 원본 (여기) | 결과 |
|---|---|
| `Player/player_down.svg` 외 3개 | `Assets/Art/Player/player_down.png` 외 3개 (16×16) |
| (같은 SVG 4개) | `Player/preview.png` (4방향 8배 확대 미리보기: 아래, 위, 왼쪽, 오른쪽 순) |

## SVG 규칙
- 16×16 격자, `<rect>`만 사용, 좌표와 크기는 정수, `shape-rendering="crispEdges"`
- 나중에 적힌 `<rect>`가 앞의 것을 덮어 칠한다 (외곽선, 몸통, 그림자, 눈 순서)

## PNG 다시 만들기
SVG를 고친 뒤, 저장소 루트에서:

```
python sub-tools/svg2png.py
```

- 필요: Python 3 + Pillow (`pip install pillow`)
- `sub-tools/svg2png.py`는 SVG의 `<rect>`를 그대로 픽셀로 칠한다 (정수 격자라 오차 없음).
  `<rect>` 외의 도형(원, 경로 등)은 무시되므로 쓰지 않는다.
- PNG를 덮어쓰면 Unity가 자동으로 다시 가져온다. 가져오기 설정은 각 PNG의 `.meta`에 저장되어 그대로 유지된다
  (설정값의 근거: [ARCHITECTURE.md 설계 결정](../ARCHITECTURE.md#3-설계-결정)).
