# INDEX

> 자동 생성 문서입니다. 직접 고치지 마세요. 각 폴더 `..INFO.md`의 첫 줄을 모아 만듭니다.
> 다시 만들기: `python sub-tools/update_index.py`

- [./](./): CurtInfMap: 고려대학교 S-CURT 연구, 무한 맵을 가진 2D 탑뷰 게임 초안 (저장소 루트 = Unity 프로젝트 루트)
  - [.claude/](.claude/): Claude 작업 환경 공간: 프로젝트와 구분되는 환경 설정, 규칙을 둔다
    - [environment/](.claude/environment/): 환경 규칙(Environment Setting Rules): 작업 환경 구성 지시문 모음, 추후 스킬로 분리 예정
  - [Art/](Art/): 원본 아트(SVG): Unity 밖에 두는 원본 그림, 변환 방법은 Art/README.md
    - [Player/](Art/Player/): 플레이어 원본 SVG와 미리보기 PNG
  - [Assets/](Assets/): Unity 에셋 루트: 씬, 스크립트, 스프라이트, URP 설정
    - [Art/](Assets/Art/): 게임용 스프라이트 (Art/의 SVG를 변환한 PNG)
      - [Player/](Assets/Art/Player/): 플레이어 스프라이트 PNG (Art/Player의 SVG에서 변환한 게임용 파일)
    - [Scenes/](Assets/Scenes/): Unity 씬 파일
    - [Scripts/](Assets/Scripts/): C# 스크립트 (게임 동작 코드, 기능별 하위 폴더)
      - [Player/](Assets/Scripts/Player/): 플레이어 구성 요소 (입력 전략, 이동, 방향 스프라이트)
    - [Settings/](Assets/Settings/): URP 2D 렌더 설정 (Universal 2D 템플릿 기본값)
      - [Scenes/](Assets/Settings/Scenes/): URP 2D 템플릿이 제공한 새 씬용 템플릿 (URP2DSceneTemplate)
  - [docs/](docs/): 문서 공간: 개요, 결정 기록, 설계, 규칙, 조사 (목차: docs/README.md)
    - [research/](docs/research/): 자료 조사 결과: 검색 서브에이전트가 작성, 출처(URL) 포함
  - [Packages/](Packages/): Unity 패키지 목록 (manifest.json, 손으로 고치지 않음)
  - [ProjectSettings/](ProjectSettings/): Unity 프로젝트 설정 (에디터 버전, 입력 방식 등의 단일 출처)
  - [sub-tools/](sub-tools/): 자동화 도구 모음: 인덱스 재구성(update_index.py), SVG→PNG 변환(svg2png.py)
