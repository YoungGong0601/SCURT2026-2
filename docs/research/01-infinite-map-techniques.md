# 01. 무한 맵 생성 기법 조사 (Phase 1)

> 작성: 조사 서브에이전트 / 2026-09-29 / 대상: Unity 6000.3 (6.3 LTS), URP 2D, Tilemap
> 원칙: **가장 단순한 방법 우선**, 코드는 이해 가능한 범위로.

---

## 요약

1. 무한 맵은 "무한히 만들어 두는 것"이 아니라, **월드를 고정 크기 청크(chunk)로 나누고 플레이어 주변 청크만 그때그때 만들고 버리는 것**이다.
2. 같은 좌표에서는 항상 같은 지형이 나오도록 **시드(seed) + 좌표만으로 결정되는 함수**(노이즈, 좌표 해시)로 생성한다. 그러면 저장 없이도 다시 돌아왔을 때 같은 맵이 나온다.
3. Unity 2D에서 가장 단순한 조합은 **Tilemap 1개 + `Mathf.PerlinNoise` + `SetTilesBlock`으로 청크 단위 채우기/지우기**이다.
4. 주요 함정은 음수 좌표의 청크 계산(내림 처리), PerlinNoise의 정수 좌표, 시드 부재, 한 프레임에 여러 청크를 만들 때의 끊김(hitch), 원점에서 매우 멀어질 때의 float 정밀도이다.
5. 연구 보고용으로는 **청크 1개 생성 시간, 프레임 시간(평균/최대), 활성 청크 수, 메모리 추이, 결정론 검증**을 측정하면 된다.

---

## 핵심 개념

### 1. 청크 (Chunk): 월드를 바둑판처럼 나누기

- 월드를 예를 들어 **16×16 타일짜리 정사각형 칸(청크)** 으로 나눈다. 각 청크는 정수 좌표 `(cx, cy)`로 부른다.
- 플레이어가 있는 청크를 중심으로 반경 `r` 안의 청크만 존재하게 한다. 동시에 존재하는 청크 수는 **(2r + 1)²** 개로 항상 일정하다 (r=2 → 25개). 그래서 맵이 "무한"이어도 메모리는 일정하게 유지된다.
- 타일 좌표 → 청크 좌표 변환은 **나눗셈 후 내림(floor)** 이어야 한다. C#의 정수 나눗셈은 0 쪽으로 버림이라 음수에서 틀린다 (아래 "함정" 참고).

```csharp
// 타일 좌표 → 청크 좌표 (음수에서도 올바름)
Vector2Int ChunkOf(Vector3Int tile, int size) =>
    new Vector2Int(Mathf.FloorToInt(tile.x / (float)size),
                   Mathf.FloorToInt(tile.y / (float)size));
```

### 2. 시드와 결정론 (Seed / Determinism)

- **결정론**: 같은 입력(시드, 좌표) → 항상 같은 출력. 마인크래프트도 하나의 시드 숫자로 월드 전체를 다시 만들어 낸다 (Zucconi).
- 결정론을 지키는 가장 쉬운 규칙: **"타일 (x, y)의 종류를 정하는 함수는 오직 `seed, x, y`만 입력으로 받는다."** 생성 순서, 이전 상태, `Random.Range` 같은 전역 난수에 의존하면 안 된다. 그래야 청크를 지웠다가 다시 만들어도 같은 모양이 나온다.
- 무작위 요소(나무, 바위 위치 등)가 필요하면 **좌표 해시**를 쓴다: `(seed, x, y)` 정수를 섞어서 "무작위처럼 보이지만 항상 같은" 숫자를 얻는 방식이다. 서브시스템(지형, 나무, 바위 …)마다 시드를 조금씩 다르게 섞으면 서로 독립적인 난수가 된다 (seed splitting).

```csharp
// 좌표 해시: 같은 (seed,x,y)면 항상 같은 0~1 값
static float Hash01(int seed, int x, int y) {
    unchecked {
        uint h = (uint)seed;
        h ^= (uint)x * 374761393u; h ^= (uint)y * 668265263u;
        h = (h ^ (h >> 13)) * 1274126177u;
        return (h ^ (h >> 16)) / (float)uint.MaxValue;
    }
}
// 사용 예: if (Hash01(seed + 1, x, y) < 0.02f) → 이 칸에 나무
```

### 3. 노이즈 (Noise): 자연스러운 지형 만들기

- 순수 난수로 타일을 고르면 TV 잡음처럼 된다. **Perlin 노이즈**는 가까운 좌표끼리 비슷한 값을 줘서 "언덕/호수" 같은 덩어리 모양이 생긴다.
- Unity에는 `Mathf.PerlinNoise(x, y)`가 내장되어 있다. 같은 좌표 → 같은 값이고, 평면이 사실상 무한하다 (공식 문서). **단, 시드 파라미터가 없다** → 시드로 만든 오프셋을 좌표에 더해서 시드 효과를 낸다.
- 값이 대략 0~1이지만 **살짝 0 미만 / 1 초과가 나올 수 있다** (공식 문서) → `Mathf.Clamp01` 권장.
- **scale(주파수)**: 좌표에 곱하는 작은 수(예 0.05). 작을수록 지형 덩어리가 커진다.
- **옥타브 / fBm**: 주파수를 2배씩 올리고 세기(진폭)를 절반씩 줄인 노이즈를 여러 장 겹치면 "큰 지형 + 작은 디테일"이 생긴다. 1단계에선 **옥타브 1개로 시작**하고, 실험 항목으로 1 vs 3 옥타브를 비교하면 좋다 (Red Blob Games).
- **바이옴**: 노이즈 값을 임계값으로 잘라 타일 종류를 정한다 (예: <0.35 물, <0.45 모래, 나머지 풀). 더 나아가 두 번째 노이즈(습도)를 조합하면 사막/숲 등으로 나눌 수 있다 (Red Blob Games).

```csharp
// 시드 → 오프셋 (한 번만 계산)
var rng = new System.Random(seed);
float ox = rng.Next(0, 10000), oy = rng.Next(0, 10000);

float h = Mathf.Clamp01(Mathf.PerlinNoise((x + ox) * 0.05f, (y + oy) * 0.05f));
TileBase t = h < 0.35f ? water : h < 0.45f ? sand : grass;
```

### 4. 스트리밍 (Streaming): 만들고 버리기

매 프레임(또는 플레이어가 다른 청크로 넘어갔을 때만):

1. 플레이어 청크 `(pcx, pcy)` 계산.
2. 반경 `r` 안의 청크 중 **아직 없는 것 → 생성 대기열에 추가**.
3. 반경 밖(`r + 1` 이상)으로 벗어난 청크 → **삭제**.
4. 대기열에서 **프레임당 1~N개만** 실제 생성 (한 번에 다 만들면 끊김 발생).

- 현재 존재하는 청크는 `HashSet<Vector2Int>`(또는 `Dictionary`)로 관리하면 충분하다.
- 삭제 반경을 생성 반경보다 1 크게(히스테리시스) 두면, 청크 경계에서 왔다 갔다 할 때 생성/삭제가 반복되는 것을 막을 수 있다.

---

## Unity 2D 구현 선택지 비교

| 방식 | 단순성 | 성능 | 추천도 (1단계) | 메모 |
|---|---|---|---|---|
| **A. Tilemap 1개 + 청크 단위 `SetTilesBlock`** (채우기/`null`로 지우기) | ★★★ 매우 쉬움 | 좋음. 렌더러 1개가 전체 타일을 배칭 | **★★★ 최우선** | 청크는 "좌표 목록"일 뿐, GameObject 불필요 |
| B. 청크마다 Tilemap GameObject 1개 (생성/`Destroy` 또는 풀링) | ★★ 보통 | 좋음. 청크 삭제가 오브젝트 삭제로 끝나 간단. 다만 Instantiate/Destroy 비용 | ★★ 2단계 비교 실험용 | 풀링하면 GC, 생성 비용 감소 |
| C. 타일 하나하나 `SetTile` 호출 | ★★★ 가장 직관적 | 나쁨. 128×128에 약 80ms 보고 사례 | ★ 비권장 | 배치 API(`SetTilesBlock`/`SetTiles`)가 더 빠름 |
| D. 청크마다 SpriteRenderer 여러 개 / 타일마다 GameObject | ★★ | 나쁨. Unity 사례에서 Sprite 방식 대비 Tilemap이 프레임 시간, RAM을 크게 줄임 | ✕ | 1단계에서 쓸 이유 없음 |
| E. 청크마다 Mesh 직접 생성 (텍스처 UV) | ★ 어려움 | 매우 좋음 | ✕ (향후 연구) | 코드 복잡 → 현재 원칙에 맞지 않음 |
| F. Job System / Burst / 멀티스레드 생성 | ★ 어려움 | 최고 | ✕ (향후 연구) | 측정 결과 병목이 확인된 뒤에만 고려 |

추가 메모:
- `RuleTile`(자동 연결 타일)이나 `TilemapCollider2D + CompositeCollider2D`는 청크 생성 시 **큰 끊김의 원인**으로 보고되었다 (Unity 포럼). 1단계는 **일반 Tile + 콜라이더 없음**으로 시작한다.
- 사용하는 스프라이트는 Sprite Atlas 하나에 모으면 드로우콜이 줄어든다 (Unity 공식 가이드).

---

## 가장 단순한 1단계 구현 제안 (스크립트 3개)

씬 구성: `Grid` → 자식 `Tilemap` 1개 (TilemapRenderer 기본값), `Player`(스프라이트 + 스크립트), `Main Camera`를 Player의 자식으로 두면 카메라 추적 스크립트도 필요 없다.

| 스크립트 | 책임 | 핵심 내용 |
|---|---|---|
| **`PlayerMove.cs`** | WASD 이동만 | `transform.position += dir * speed * Time.deltaTime`. **이 프로젝트는 `activeInputHandler: 1`(새 Input System 전용)** 으로 설정되어 있어, 옛날 `Input.GetAxis`는 에러가 난다 → `UnityEngine.InputSystem.Keyboard.current.wKey.isPressed` 식으로 읽는다. |
| **`WorldGen.cs`** (static 함수 모음) | "좌표 → 타일 종류" 순수 함수 | `GetTile(int seed, int x, int y)` : 시드 오프셋 + `Mathf.PerlinNoise` + 임계값. **상태를 갖지 않는다** (결정론 보장). |
| **`ChunkManager.cs`** | 청크 로드/언로드 + 간단 측정 | 플레이어 청크 계산(`FloorToInt`) → 필요한 청크 집합 비교 → 대기열에서 프레임당 1개씩 `SetTilesBlock`으로 채움, 멀어진 청크는 `null` 배열로 지움. `HashSet<Vector2Int> loaded`. `Stopwatch`로 청크당 생성 시간 기록, 활성 청크 수를 화면(OnGUI)이나 로그로 표시. |

추천 초기값: 청크 16×16, 로드 반경 r=2 (25청크 = 6,400타일), 언로드 반경 r+1, 프레임당 1청크, scale 0.05, 옥타브 1.
재사용 팁: `TileBase[]` 버퍼(256칸)와 "빈 청크용 `null` 배열"은 **한 번만 만들어 재사용**한다 (GC 방지).

---

## 주의할 함정

1. **음수 좌표의 청크 계산**: C#에서 `-5 / 16 == 0` (0 쪽으로 버림). 그러면 청크 -1과 0이 겹친다. → `Mathf.FloorToInt(x / (float)size)` 사용.
2. **`Mathf.PerlinNoise`에 정수 좌표만 넣기**: 정수 격자점에서는 값이 항상 같게 나온다 (포럼에서 "항상 0.4652731이 나온다"는 사례). → 반드시 `scale`(예 0.05)을 곱해 소수 좌표로 만든다.
3. **`Mathf.PerlinNoise`에는 시드가 없다**: 시드로 만든 오프셋을 더해야 한다. 오프셋을 너무 크게(예: 수백만) 잡으면 float 정밀도가 떨어져 지형이 계단/뭉개짐 → **0~10,000 정도**의 오프셋 권장.
4. **음수 좌표 대칭, 주기 반복 의심**: 고전 Perlin 구현은 256 격자 단위로 반복된다 (Perlin math FAQ). Unity 구현의 음수 영역 대칭이나 반복 주기는 **공식 문서에 명시되어 있지 않아 확인하지 못함** → 오프셋으로 입력을 양수로 유지하는 것을 기본으로 하고, 실제 동작은 아래 실험 5로 직접 검증한다.
5. **반환값 범위**: 0~1을 약간 벗어날 수 있음 → `Clamp01` (공식 문서).
6. **한 프레임에 여러 청크 생성 → 끊김(hitch)**: 청크 경계를 넘는 순간 한 줄(2r+1개)이 한꺼번에 필요해진다. 대기열 + 프레임당 1개 처리로 분산 (포럼의 코루틴 분산 조언과 같은 원리). 로그(`Debug.Log`)도 비용이 크므로 측정 시 최소화.
7. **RuleTile / Composite Collider**: 생성 비용이 커서 끊김의 주 원인으로 보고됨. 1단계에선 사용하지 않는다.
8. **GC 스파이크**: 청크마다 `new TileBase[256]`, LINQ, 문자열 조합 등을 하면 가비지가 쌓여 주기적 끊김. 버퍼 재사용.
9. **원점에서 멀어질 때 float 정밀도**: Unity 위치는 32비트 float라 멀어질수록 떨림(jitter)이 생긴다. 참고 자료 기준 대략 1,000 단위 이하는 안전, 10,000 이상에서 뚜렷한 떨림. 해결책은 **floating origin**(일정 거리마다 모든 오브젝트를 원점 쪽으로 되돌리기). 걷기만 하는 1단계에서는 도달하기 어려우므로 **구현하지 않고 실험으로 한계만 측정**하는 것을 권장. (타일 좌표 자체는 정수 `Vector3Int`라 정밀도 문제 없음.)
10. **SetTilesBlock 배열 순서**: 배열 길이는 영역 크기와 같아야 하며, 공식 문서 예제상 x가 먼저 증가하는 순서로 보이나 명시적 설명은 없다 → 첫 구현 때 비대칭 패턴으로 한 번 확인.

---

## 측정, 실험 아이디어 (연구용)

측정 도구 (단순한 순):
- `System.Diagnostics.Stopwatch`: 청크 1개 생성 시간 (ms).
- `Unity.Profiling.ProfilerRecorder`: `"Main Thread"`(프레임 시간), `"GC Reserved Memory"`, `"System Used Memory"`를 에디터/빌드 모두에서 코드로 읽을 수 있다 (공식 문서).
- Unity Profiler 창: 스파이크가 난 프레임을 눌러 원인 함수 확인.
- 재현성을 위해 **플레이어를 스크립트로 일정 속도, 일정 경로로 자동 이동**시키는 모드를 두면 실험 간 비교가 공정해진다. 정확한 수치는 에디터보다 Development Build에서 재는 것이 좋다.

실험 목록:

| # | 실험 | 변수 | 측정값 | 기대/목적 |
|---|---|---|---|---|
| 1 | 청크 크기 | 8 / 16 / 32 / 64 | 청크당 생성 ms, 최대 프레임 시간 | 청크가 클수록 1회 비용↑, 호출 횟수↓: 적정점 찾기 |
| 2 | 로드 반경 | r = 1~5 | 활성 청크 수(이론 (2r+1)²과 비교), 메모리 | 반경과 비용의 관계 그래프 |
| 3 | 생성 분산 | 한 번에 전부 / 프레임당 1개 / N개 | 최대, 평균 프레임 시간, 빈 칸 노출 여부 | 끊김 vs 로딩 지연 트레이드오프 |
| 4 | 결정론 검증 | 같은 시드로 청크 생성→삭제→재생성, 다른 시드 | 청크 타일 배열의 해시값 일치 여부 | "같은 좌표 → 같은 내용" 증명 |
| 5 | 먼 거리 테스트 | 1e3 / 1e4 / 1e5 / 1e6 위치로 순간이동 | 화면 떨림, 노이즈 반복, 대칭 무늬 | float 정밀도 한계, PerlinNoise 주기 확인 |
| 6 | 장시간 주행 | 10분 자동 이동 | 메모리 추이 | 언로드가 제대로 되는지(누수 없음) 확인 |
| 7 | 방식 비교 (2단계) | Tilemap 1개 vs 청크별 Tilemap | 생성, 삭제 ms, 프레임 시간 | 구현 선택 근거 |
| 8 | 노이즈 품질 | 옥타브 1 vs 3, scale 변화 | 타일 종류 비율, 스크린샷 | 지형 다양성의 정성, 정량 비교 |

보고서 표현 팁: 평균만이 아니라 **최대 프레임 시간(또는 상위 1% 프레임)** 을 함께 보고하면 "끊김"을 수치로 보여 줄 수 있다.

---

## S-CURT 메모

공개 자료에서 확인됨 (**고려대학교 세종캠퍼스 KUS 대학혁신사업단 공지 기준**: 본 프로젝트가 서울캠퍼스 프로그램이라면 조건이 다를 수 있으니 확인 필요):
- CURT 프로그램 중 **S-CURT = Short term CURT**(단기형). 장기형은 L-CURT.
- 2026년 공지 기준 기간: S-CURT 2026.5.1 ~ 7.24 (약 3개월), L-CURT 5.1 ~ 12.7.
- 대상: 재학 중인 학부생, 개인 또는 5인 이하 팀.
- 연구비(학부 연구원): S-CURT 개인 50만 원 / 팀 50만 원 + 추가 인원당 10만 원.
- 지도교수 면담, 멘토링 6회 이상, 학기별 우수 연구보고서 선정자가 **1월 성과발표회**에 참여.
- "CURT" 약어의 정식 풀네임은 확인한 자료에 나오지 않아 **확인하지 못함**.

---

## 참고 자료

- Unity Scripting API: Mathf.PerlinNoise: https://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html
- Unity Scripting API: Tilemaps.Tilemap.SetTilesBlock: https://docs.unity3d.com/ScriptReference/Tilemaps.Tilemap.SetTilesBlock.html
- Unity Scripting API: Unity.Profiling.ProfilerRecorder: https://docs.unity3d.com/ScriptReference/Unity.Profiling.ProfilerRecorder.html
- Unity: Optimize performance of 2D games with Unity Tilemap: https://unity.com/how-to/optimize-performance-2d-games-unity-tilemap
- Unity Discussions: Lag Spikes: Procedural Chunk-based 2D Tilemap World Generation w/ Advanced Rule Tiles on the fly: https://discussions.unity.com/t/lag-spikes-procedural-chunk-based-2d-tilemap-world-generation-w-advanced-rule-tiles-on-the-fly/900130
- Unity Discussions: SetTile is slow... Plans to implement ECS Pattern to tilemap? (SetTile 128×128 약 80ms 사례): https://forum.unity.com/threads/settile-is-slow-plans-to-implement-ecs-pattern-to-tilemap.639619/
- Unity Answers: Mathf.PerlinNoise always returns 0.4652731: https://answers.unity.com/questions/940343/mathfperlinnoise-always-returns-04652731-1.html
- Red Blob Games: Making maps with noise functions: https://www.redblobgames.com/maps/terrain-from-noise/
- The Perlin noise math FAQ (256 주기): https://mzucker.github.io/html/perlin-noise-math-faq.html
- Catlike Coding: Perlin Noise (격자 좌표 해시로 결정론적 난수): https://catlikecoding.com/unity/tutorials/pseudorandom-noise/perlin-noise/
- Alan Zucconi: The World Generation of Minecraft: https://www.alanzucconi.com/2022/06/05/minecraft-world-generation/
- Abratabia: Procedural Generation for Games (청크, 시드, 좌표 해시, seed splitting): https://www.abratabia.com/procedural-generation/
- Bugnet Blog: How to Fix Floating-Point Precision Bugs in Large Worlds: https://bugnet.io/blog/how-to-fix-floating-point-precision-bugs-in-large-worlds
- Wikipedia: Floating origin: https://en.wikipedia.org/wiki/Floating_origin
- 고려대학교 세종 KUS 대학혁신사업단: CURT 프로그램 목표 및 운영체계: https://kusui.korea.ac.kr/kusui/10018/subview.do
- 고려대학교: 2026학년도 1학기 CURT 프로그램 모집 안내: https://cwms.korea.ac.kr/bbs/cwms/167/268615/artclView.do
