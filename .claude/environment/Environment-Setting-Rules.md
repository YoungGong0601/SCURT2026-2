# Environment Setting Rules (환경 규칙)

> 2026-09-29, 상태: 확정 (연구자 지시를 정리한 것)
> **작업 환경을 어떻게 구성하고 운영하는가**의 단일 출처다.
> 프로젝트 자체의 규칙(코드, Unity 작업)은 [docs/03-rules.md](../../docs/03-rules.md)에 따로 둔다.
> 이 파일은 추후 **스킬로 분리**할 수 있도록, 프로젝트와 구분되는 공간(`.claude/environment/`)에 둔다.
> 새로운 환경 구성 지시가 나오면 **이 파일에 추가**한다.

---

## 작업 끝 체크리스트 (매 작업 후)
1. 새 폴더를 만들었다면 그 폴더에 `..INFO.md`를 만든다 (ENV-4).
2. `python sub-tools/update_index.py` 실행 후 **경고 0** 확인 (ENV-5).
3. 결정사항이 있었다면 `docs/01-decision-log.md`에 기록한다 (ENV-3).
4. 구조나 구현 상태가 바뀌었다면 `ARCHITECTURE.md`를 갱신한다 (ENV-3).
5. 캡처를 찍어 `captures/`에 저장하고, 작업 보고에 포함한다 (ENV-10).
6. 한 작업 단위로 커밋한다 (ENV-8).

---

## ENV-1. 상대 경로
- 프로젝트는 **모든 것을 저장소 루트 기준 상대 경로로** 파악한다.
- 문서와 스크립트에 절대 경로를 쓰지 않는다.
- 도구는 **자기 파일 위치에서 저장소 루트를 찾는다** (Python: `Path(__file__).resolve().parent.parent`). 어디서 실행해도 결과가 같아야 한다.
- 명령은 **저장소 루트에서 실행**하는 것을 기본으로 한다. 절대 경로가 꼭 필요한 도구에는 실행 시점에 계산해서 넘긴다 (PowerShell: `(Get-Location).Path`).

## ENV-2. 역할 분담 (에이전트)
- **메인 에이전트:** 연구자와 대화하고, 대화 중 정해진 것을 문서로 저장한다.
- **생성 작업**(프로젝트 생성, 코드, 씬, 에셋 작성, 도구 제작, 헤드리스 실행)은 **서브에이전트**에 맡긴다.
- **자료 검색과 문서화**는 **서브에이전트**에 맡긴다. 검색 시간은 **10분** 제한. 결과는 `docs/research/`에 출처와 함께 저장한다.
- 서브에이전트에게는 [프로젝트 규칙](../../docs/03-rules.md)과 이 환경 규칙을 따르라고 전달한다.

## ENV-3. 문서 체계와 진실 단일성
**한 가지 사실은 한 곳에만 적는다.** 다른 곳에서는 링크로 참조한다.

| 알고 싶은 것 | 단일 출처 |
|---|---|
| 프로젝트 정체성, 목표, 범위 | `docs/00-project-brief.md` |
| 코드, Unity 작업 규칙 | `docs/03-rules.md` |
| 작업 환경 규칙 | 이 파일 |
| 시스템 구조, 구성 요소, 구현 상태, 설계 결정 | `ARCHITECTURE.md` |
| 폴더 구조와 폴더 설명 | 각 폴더 `..INFO.md` (모아서 `INDEX.md` 자동 생성) |
| 문서 목록 | `docs/README.md` |
| 지금까지의 결정 역사, 미결 사항 | `docs/01-decision-log.md` (역사 기록이며 현재 상태의 출처는 아님) |
| 플레이어 디자인 컨셉 | `docs/02-player-design.md` (정확한 픽셀은 `Art/Player/*.svg`) |
| 아트 원본 규칙과 변환 방법 | `Art/README.md` |
| 도구 목록 / 도구 사용법 | `sub-tools/..INFO.md` 첫 줄 / 각 도구 파일 맨 위 주석 |
| Unity 에디터 버전 | `ProjectSettings/ProjectVersion.txt` |
| 패키지 | `Packages/manifest.json` |
| git 제외 목록 | `.gitignore` |
| 무한 맵 기법 조사 근거 | `docs/research/` |
| 작업 보고 캡처 | `captures/` |

- `CLAUDE.md`는 입구일 뿐 내용을 적지 않는다. 위 문서들을 `@경로`로 가져와 세션마다 자동으로 읽히게 한다.
- 문서는 한국어로 쓴다. 기술 용어와 코드 식별자는 원문 유지.
- **특수기호 가운뎃점(U+00B7)과 엠 대시(U+2014)는 쓰지 않는다.** 대신 쉼표(`,`)나 콜론(`:`)을 쓴다.
- `docs/` 최상위 문서는 `NN-이름.md`로 번호를 붙이고, 새 문서는 `docs/README.md` 목록에 추가한다.
- 문서 맨 위에 날짜와 상태(초안/확정)를 적는다.
- 조사 자료는 출처(URL)를 반드시 적고, 확인하지 못한 내용은 "미확인"으로 표시한다.

## ENV-4. 디렉터리 설명 파일 `..INFO.md`
- **모든 디렉터리**(루트 포함, 제외 목록은 ENV-5)에 `..INFO.md`를 둔다.
- 형식:
  ```
  1번째 줄: 이 폴더가 무엇인지 한 줄 설명      <- 인덱스에 쓰임
  2번째 줄: (항상 비워 둔다)
  3번째 줄부터: 원하면 자유롭게 추가 설명
  ```
- 첫 줄은 폴더의 **목적**을 적는다. 다른 문서에 출처가 있는 값(버전, 설정값 등)을 옮겨 적지 않는다.
- 점(`.`)으로 시작하는 이름이라 Unity가 무시한다. `Assets/` 안에 두어도 `.meta`가 생기지 않는다.

## ENV-5. 인덱스 `INDEX.md`
- 루트의 `INDEX.md`는 각 폴더 `..INFO.md`의 **첫 줄을 모아 자동으로** 만든다. **손으로 고치지 않는다.**
- 다시 만들기 (**매 작업 후 실행**):
  ```
  python sub-tools/update_index.py
  ```
- 경고(INFO 없음, 첫 줄 비어 있음, 둘째 줄이 비어 있지 않음)가 0이어야 한다. 경고가 있으면 종료 코드 1.
- 색인에서 빼는 폴더 목록의 출처는 `sub-tools/update_index.py`의 `SKIP`이다.

## ENV-6. 자동화 도구 `sub-tools/`
- 자동화 도구로 만든 것은 **모두 `sub-tools/` 한 폴더에** 둔다.
- 도구는 단순하게: 가능하면 표준 라이브러리만, 파일 맨 위에 한국어 사용법 주석.
- 도구를 추가하면 `sub-tools/..INFO.md` 첫 줄(도구 목록)을 갱신한다.

## ENV-7. Unity 개발 환경
- 프로젝트 생성과 에디터 관리는 **Unity CLI**(`unity`)로 한다.
- 에디터는 **헤드리스(batchmode)로 띄워 두고**, `com.unity.pipeline` 패키지를 통해 CLI 명령으로 조종한다.
- `unity command`의 `--caller plugin --skill <스킬명>`은 **명령 이름 앞에** 둔다 (뒤에 두면 명령 인자로 오인됨).
- `--project-path`에는 `.` 대신 `(Get-Location).Path`를 넘긴다 (`unity status`가 `.`를 인식하지 못함).
- 연결이 안 되면 먼저 Safe Mode(컴파일 에러)를 의심한다: `unity pipeline list`.

```powershell
# 모두 저장소 루트에서 실행
$p   = (Get-Location).Path
$ver = ((Get-Content ProjectSettings/ProjectVersion.txt)[0] -split ' ')[1]   # 버전의 단일 출처
$exe = Join-Path (unity editors path $ver) "Editor\Unity.exe"

# 시작 (-quit 없이 실행해야 계속 떠 있음)
Start-Process $exe -ArgumentList @('-batchmode','-projectPath',"`"$p`"",'-logFile',"`"$p\Logs\headless-editor.log`"") -WindowStyle Hidden

unity status --format json --project-path $p                        # state "ready" 확인
unity command --caller plugin --skill unity-cli --project-path $p   # 사용 가능한 명령 목록
unity command --caller plugin --skill unity-cli console_status --project-path $p   # 컴파일 에러 확인
unity command quit --project-path $p                                # 정상 종료
```

## ENV-8. 소스 관리
- 원격: GitHub **private** 저장소 [YoungGong0601/SCURT2026-2](https://github.com/YoungGong0601/SCURT2026-2), 브랜치 `main`.
- 생성물(`Library/`, `Temp/`, `Logs/`, `UserSettings/` 등)은 커밋하지 않는다. 목록의 출처는 `.gitignore`.
- 에셋과 스크립트는 반드시 `.meta` 파일과 **함께** 커밋한다.
- 한 커밋 = 한 가지 작업. 커밋 메시지는 한국어 한 줄로 무엇을 했는지.

## ENV-9. 백그라운드 실행과 병렬 작업
- 빌드, 테스트, 설치, 서버 실행처럼 **시간이 걸리는 명령은 반드시 백그라운드로** 실행한다 (예: 10분짜리 테스트 스크립트).
- 백그라운드 명령을 띄운 직후, **그 결과에 의존하지 않는 작업을 먼저** 진행한다.
  - 예: 다음 단계 코드 작성, 관련 파일 읽기, 테스트 케이스 작성, 문서 수정, 변경 사항 검토.
- 결과는 완료 알림을 받은 뒤에 확인한다. 기다리는 동안 짧은 간격으로 반복 확인(polling)하지 않는다.

## ENV-10. 캡처
- **모든 작업 보고는 캡처를 포함한다.** 용어는 "스크린샷"이 아니라 **"캡처"**를 쓴다.
- 캡처는 `captures/`에 `YYYY-MM-DD_작업-이름.png`로 저장하고, 보고할 때 이미지를 함께 보여 준다.
- Unity 화면 캡처: 헤드리스 에디터가 떠 있는 상태에서
  ```
  python sub-tools/capture.py <작업-이름>
  ```
- 화면에 보이지 않는 작업(문서, 도구 등)은 그 결과가 드러나는 화면(렌더링한 문서, 실행 결과 등)을 캡처한다.
