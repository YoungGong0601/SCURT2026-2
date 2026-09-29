# INDEX.md 다시 만들기: 모든 폴더의 ..INFO.md 첫 줄(폴더 설명)을 모아 저장소 루트의 INDEX.md를 새로 쓴다.
# 사용법: python sub-tools/update_index.py  (어느 폴더에서 실행해도 같은 결과)
# ..INFO.md 규칙: 1줄 = 폴더 설명 한 줄, 2줄 = 빈 줄, 3줄부터 = 자유 메모. 경고가 있으면 종료 코드 1.
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent  # 저장소 루트 (이 파일 = 루트/sub-tools/update_index.py)
SKIP = {".git", ".vs", ".idea", ".vscode", "Library", "Temp", "Logs", "UserSettings",
        "obj", "Build", "Builds", "MemoryCaptures", "__pycache__"}  # 색인하지 않을 폴더 이름

lines = []     # INDEX.md에 들어갈 목록 줄
warnings = []  # 끝에 화면으로 보여줄 경고


def visit(folder, depth):
    """folder 한 줄을 목록에 넣고, 하위 폴더를 이름순으로 차례로 방문한다."""
    rel = folder.relative_to(ROOT).as_posix()  # 예: "Assets/Art", 루트는 "."
    link = "./" if rel == "." else rel + "/"
    name = "./" if rel == "." else folder.name + "/"
    info = folder / "..INFO.md"
    if not info.exists():
        desc = "⚠ ..INFO.md 없음"
        warnings.append(f"{link} : ..INFO.md 없음")
    else:
        text = info.read_text(encoding="utf-8-sig").splitlines()  # utf-8-sig: BOM이 있어도 읽음
        desc = text[0].strip() if text else ""
        if not desc:
            warnings.append(f"{link} : 1줄(폴더 설명)이 비어 있음")
        if len(text) > 1 and text[1].strip():
            warnings.append(f"{link} : 2줄이 빈 줄이 아님")
    target = link.replace(" ", "%20")  # 링크 주소에는 공백을 쓸 수 없어 %20으로 바꿈
    lines.append(f"{'  ' * depth}- [{name}]({target}): {desc}")
    children = [p for p in folder.iterdir() if p.is_dir() and p.name not in SKIP]
    for child in sorted(children, key=lambda p: p.name.lower()):  # 대소문자 무시 정렬
        visit(child, depth + 1)


visit(ROOT, 0)
header = ["# INDEX", "",
          "> 자동 생성 문서입니다. 직접 고치지 마세요. 각 폴더 `..INFO.md`의 첫 줄을 모아 만듭니다.",
          "> 다시 만들기: `python sub-tools/update_index.py`", ""]
# 시간 정보 없이 늘 같은 내용을 LF 줄바꿈으로 씀 → 폴더/설명이 바뀔 때만 git 변경이 생김
(ROOT / "INDEX.md").write_text("\n".join(header + lines) + "\n", encoding="utf-8", newline="\n")

print(f"INDEX.md 갱신: 폴더 {len(lines)}개")
for w in warnings:
    print("경고:", w)
sys.exit(1 if warnings else 0)
