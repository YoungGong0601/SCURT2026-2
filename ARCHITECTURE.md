# ARCHITECTURE

> 최종 갱신 2026-09-29
> **시스템 구조, 구성 요소와 구현 상태, 설계 결정**의 단일 출처다.
> 프로젝트가 무엇인지는 [docs/00-project-brief.md](docs/00-project-brief.md), 설계 근거 자료는 [docs/research/01-infinite-map-techniques.md](docs/research/01-infinite-map-techniques.md).

## 1. 한눈에 보기

```
 KeyboardMoveInput ──(IMoveInput: 방향)──▶ PlayerMovement ──(FacingChanged 이벤트)──▶ PlayerFacingSprite
   (WASD 읽기)                              (이동, 바라보는 방향)                        (방향별 스프라이트)
                                                  │ 위치 (target)
                                                  ▼
  MapSettings ──(모든 맵 설정값)──▶ ChunkManager ──(x, y)──▶ ITileGenerator ◀── PerlinTileGenerator
  TilePalette ──(TileType → 타일)─▶ (청크 만들기/지우기) ◀─(TileType)──       (seed + Perlin 노이즈)
                                      │        │ 측정
                                      │        ▼
                                      │    ChunkStats ──▶ MapStatsOverlay (화면 표시)
                                      │ SetTilesBlock
                                      ▼
                                Grid / Tilemap (1개)

 Main Camera: Player 오브젝트의 자식 (따라다님, 코드 없음)
```

- **입력**은 `IMoveInput` 인터페이스 뒤에 숨는다. 키보드 대신 자동 입력(실험용)으로 바꿔 끼울 수 있다.
- **PlayerMovement**가 위치와 바라보는 방향의 유일한 주인이다. 방향이 바뀌면 이벤트로 알린다.
- **ChunkManager**는 플레이어 주변에 있어야 할 청크를 계산해, 없는 청크는 **생성기**에게 타일 종류를 물어 채우고 멀어진 청크는 지운다.
- **생성기**(`ITileGenerator`)는 결과가 `(seed, x, y)`로만 정해지는 순수 함수다. 같은 좌표는 언제나 같은 타일이 된다. 그래서 저장하지 않아도 무한하고 일관된 맵이 된다. 다른 알고리즘은 새 구현체를 **추가**해서 비교 실험한다.

## 2. 구성 요소

| 구성 요소 | 책임 | 원칙, 패턴 | 상태 |
|---|---|---|---|
| `IMoveInput` | 이동 방향을 알려 주는 약속(인터페이스) | Strategy, 의존 역전 | ✅ 완료 |
| `KeyboardMoveInput` | WASD 읽기. 키를 아는 유일한 곳 | Strategy 구현 | ✅ 완료 |
| `PlayerMovement` | 이동. 속도와 바라보는 방향의 단일 출처. `SetInput`으로 입력 전략 교체 | 단일 책임, Observer(발행) | ✅ 완료 |
| `PlayerFacingSprite` | 방향 → 스프라이트. 방향별 스프라이트의 단일 출처 | 단일 책임, Observer(구독) | ✅ 완료 |
| 플레이어 아트 | 16×16 "흰 네모" ([설계](docs/02-player-design.md), [원본과 변환](Art/README.md)) | | ✅ 완료 |
| Main Camera | Player의 자식으로 따라다님 | | ✅ 완료 |
| `TileType` | 타일 종류: Water, Sand, Grass, Forest | | ✅ 완료 |
| `ITileGenerator` | 좌표 → 타일 종류의 약속 | Strategy, 개방 폐쇄 | ✅ 완료 |
| `PerlinTileGenerator` | seed 오프셋 + Perlin 노이즈 + 임계값. 결과는 `(seed, x, y)`로만 정해짐 | Strategy 구현, 순수 함수 | ✅ 완료 |
| `MapSettings` (에셋) | 시드, 청크 크기, 반경, 프레임당 청크 수, 노이즈 배율, 임계값. **모든 맵 설정값의 단일 출처** | 진실 단일성 | ✅ 완료 |
| `TilePalette` (에셋) | TileType → 타일 에셋. 어떤 그림을 쓰는지의 단일 출처 | 진실 단일성 | ✅ 완료 |
| `ChunkCoords` | 타일 좌표 ↔ 청크 좌표 계산의 유일한 곳 | 진실 단일성 | ✅ 완료 |
| `ChunkManager` | 청크 로드/언로드, 가까운 청크부터, 프레임당 N청크 | 단일 책임 | ✅ 완료 |
| `ChunkStats` | 측정값(활성 청크 수, 생성 수, 청크당 ms) | 단일 책임 | ✅ 완료 |
| `MapStatsOverlay` | 측정값 화면 표시 (읽기만 함) | 단일 책임 | ✅ 완료 |
| 타일 아트 | 16×16 타일 4종, 이음새 없이 반복 ([원본과 변환](Art/README.md)) | | ✅ 완료 |
| Grid / Tilemap | 맵 표시. Tilemap 1개에 청크 단위로 채움 | | ✅ 완료 |
| `AutoMoveInput` | 실험용 자동 입력 (정한 방향으로 계속 걷기). 씬에는 붙이지 않고 실행 중에 `PlayerMovement.SetInput`으로 교체 | Strategy 교체 | ✅ 완료 |

범례: ✅ 완료, 🔨 진행 중, 📋 계획

## 3. 설계 결정

| 결정 | 이유 |
|---|---|
| 1 타일 = 16px = 1 world unit (Pixels Per Unit 16, Point 필터) | 플레이어와 타일 크기 통일, 픽셀 선명 |
| 입력은 `IMoveInput`으로 추상화 | 키보드 없는 헤드리스 실험에서 자동 입력으로 교체 가능 |
| 바라보는 방향은 `PlayerMovement`만 계산, 스프라이트는 이벤트로 받음 | 진실 단일성, 단일 책임 |
| 청크 16×16 타일, 로드 반경 r=2 (25청크), 언로드 반경 r+1 | 활성 청크 수가 25~49 사이로 제한되어 맵이 무한해도 메모리는 일정. 반경 차이는 경계에서 만들고 지우기를 반복하지 않게 함 (실측 범위: [실험 01](docs/experiments/01-infinite-map-v1.md)) |
| 맵 설정값은 `MapSettings` 에셋 한 곳에만 둔다. 언로드 반경도 저장하지 않고 `로드 반경 + 1`로 계산 | 진실 단일성 |
| 타일 생성은 `ITileGenerator` 뒤에 두고, 구체 생성기를 고르는 곳은 `ChunkManager` 한 곳 | 알고리즘 비교 실험 시 기존 코드 수정 없이 추가 |
| 측정(`ChunkStats`)과 표시(`MapStatsOverlay`)를 스트리밍과 분리 | 단일 책임, 연구용 측정 |
| 타일 좌표 → 청크 좌표는 `Mathf.FloorToInt` | 정수 나눗셈은 음수 좌표에서 틀림 |
| 맵 내용 = `f(seed, x, y)` 만 | 결정론: 지웠다 다시 만들어도 같은 맵 |
| Tilemap 1개 + `SetTilesBlock` | 가장 단순하고 충분히 빠름 (조사 문서 비교표 A안) |
| 프레임당 청크 1개만 생성 | 한 번에 여러 개 만들면 끊김(hitch) |
| 1단계에서 물리, 콜라이더, RuleTile 없음 | 단순성, 알려진 끊김 원인 회피 |
| 그리기 순서는 TilemapRenderer `sortingOrder -10` 한 곳에서만 정함 (플레이어는 기본값 0) | 진실 단일성, 플레이어가 항상 맵 위에 보임 |
| 맵 설정과 팔레트 에셋은 `Assets/Data/`에 둠 | 데이터 에셋의 위치를 한 곳으로 |

## 4. 폴더 구조와 개발 환경
- 폴더 구조: [INDEX.md](INDEX.md) (자동 생성, 단일 출처)
- 개발 환경, 역할 분담, 도구: [Environment Setting Rules](.claude/environment/Environment-Setting-Rules.md)

## 5. 앞으로 (계획)
1. ~~무한 맵 1차 구현~~ 완료 ([실험 01](docs/experiments/01-infinite-map-v1.md))
2. 끊김 측정: 경계를 넘는 프레임의 언로드 비용, 첫 청크 준비 비용
3. 비교 실험 (조사 문서의 "측정, 실험 아이디어")
