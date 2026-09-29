# ARCHITECTURE

> 최종 갱신 2026-09-29
> **시스템 구조, 구성 요소와 구현 상태, 설계 결정**의 단일 출처다.
> 프로젝트가 무엇인지는 [docs/00-project-brief.md](docs/00-project-brief.md), 설계 근거 자료는 [docs/research/01-infinite-map-techniques.md](docs/research/01-infinite-map-techniques.md).

## 1. 한눈에 보기

```
 KeyboardMoveInput ──(IMoveInput: 방향)──▶ PlayerMovement ──(FacingChanged 이벤트)──▶ PlayerFacingSprite
   (WASD 읽기)                              (이동, 바라보는 방향)                        (방향별 스프라이트)
                                                  │ 위치
                                                  ▼
                                            ChunkManager ──(seed, x, y)──▶ WorldGen
                                          (청크 만들기/지우기) ◀──(타일 종류)──  (좌표 → 타일 종류)
                                                  │ SetTilesBlock
                                                  ▼
                                            Grid / Tilemap (1개)

 Main Camera: Player 오브젝트의 자식 (따라다님, 코드 없음)
```

- **입력**은 `IMoveInput` 인터페이스 뒤에 숨는다. 키보드 대신 자동 입력(실험용)으로 바꿔 끼울 수 있다.
- **PlayerMovement**가 위치와 바라보는 방향의 유일한 주인이다. 방향이 바뀌면 이벤트로 알린다.
- **ChunkManager**는 플레이어 주변에 있어야 할 청크를 계산해, 없는 청크는 **WorldGen**에게 타일 종류를 물어 채우고 멀어진 청크는 지운다.
- **WorldGen**은 상태가 없는 순수 함수다. 같은 좌표는 언제나 같은 타일이 된다. 그래서 저장하지 않아도 무한하고 일관된 맵이 된다.

## 2. 구성 요소

| 구성 요소 | 책임 | 원칙, 패턴 | 상태 |
|---|---|---|---|
| `IMoveInput` | 이동 방향을 알려 주는 약속(인터페이스) | Strategy, 의존 역전 | ✅ 완료 |
| `KeyboardMoveInput` | WASD 읽기. 키를 아는 유일한 곳 | Strategy 구현 | ✅ 완료 |
| `PlayerMovement` | 이동. 속도와 바라보는 방향의 단일 출처 | 단일 책임, Observer(발행) | ✅ 완료 |
| `PlayerFacingSprite` | 방향 → 스프라이트. 방향별 스프라이트의 단일 출처 | 단일 책임, Observer(구독) | ✅ 완료 |
| 플레이어 아트 | 16×16 "흰 네모" ([설계](docs/02-player-design.md), [원본과 변환](Art/README.md)) | | ✅ 완료 |
| Main Camera | Player의 자식으로 따라다님 | | ✅ 완료 |
| `WorldGen` | `(seed, x, y)` → 타일 종류 | 순수 함수 | 📋 계획 |
| `ChunkManager` | 청크 로드/언로드, 프레임당 1청크, 측정(생성 ms, 활성 청크 수) | 단일 책임 | 📋 계획 |
| Grid / Tilemap | 맵 표시. Tilemap 1개에 청크 단위로 채움 | | 📋 계획 |

범례: ✅ 완료, 🔨 진행 중, 📋 계획

## 3. 설계 결정

| 결정 | 이유 |
|---|---|
| 1 타일 = 16px = 1 world unit (Pixels Per Unit 16, Point 필터) | 플레이어와 타일 크기 통일, 픽셀 선명 |
| 입력은 `IMoveInput`으로 추상화 | 키보드 없는 헤드리스 실험에서 자동 입력으로 교체 가능 |
| 바라보는 방향은 `PlayerMovement`만 계산, 스프라이트는 이벤트로 받음 | 진실 단일성, 단일 책임 |
| 청크 16×16 타일, 로드 반경 r=2 (항상 25청크) | 맵이 무한해도 메모리는 일정 |
| 맵 설정값(청크 크기, 반경, 시드)은 한 곳에만 둔다 | 진실 단일성 (위치는 무한 맵 구현 때 결정) |
| 타일 좌표 → 청크 좌표는 `Mathf.FloorToInt` | 정수 나눗셈은 음수 좌표에서 틀림 |
| 맵 내용 = `f(seed, x, y)` 만 | 결정론: 지웠다 다시 만들어도 같은 맵 |
| Tilemap 1개 + `SetTilesBlock` | 가장 단순하고 충분히 빠름 (조사 문서 비교표 A안) |
| 프레임당 청크 1개만 생성 | 한 번에 여러 개 만들면 끊김(hitch) |
| 1단계에서 물리, 콜라이더, RuleTile 없음 | 단순성, 알려진 끊김 원인 회피 |

## 4. 폴더 구조와 개발 환경
- 폴더 구조: [INDEX.md](INDEX.md) (자동 생성, 단일 출처)
- 개발 환경, 역할 분담, 도구: [Environment Setting Rules](.claude/environment/Environment-Setting-Rules.md)

## 5. 앞으로 (계획)
1. `WorldGen` + `ChunkManager`로 무한 맵 1차 구현
2. 측정: 청크 생성 ms, 프레임 시간, 활성 청크 수, 결정론 검증
3. 비교 실험 (조사 문서의 "측정, 실험 아이디어")
