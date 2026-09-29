# 작업 보고용 캡처: 실행 중인 헤드리스 Unity 에디터에서 씬의 Camera.main 화면을 PNG로 저장한다.
# 실행 (저장소 루트에서): python sub-tools/capture.py <작업-이름> [--force]
#   결과: captures/<오늘 날짜>_<작업-이름>.png  (이미 있으면 --force 없이는 덮어쓰지 않는다)
# 에디터가 떠 있어야 한다. 띄우는 방법: .claude/environment/Environment-Setting-Rules.md 의 ENV-7
import argparse, base64, datetime, json, subprocess, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent  # 저장소 루트 (이 파일 = 루트/sub-tools/capture.py)

# 에디터 안에서 실행할 C# 코드. PNG는 글자(base64)로 돌려받고, 파일 저장은 아래 Python이 한다.
# 편집 모드(Play 없이)에서 cam.Render()를 직접 부르면 URP 2D도 이 카메라 한 대를 그대로 그린다.
# eval은 코드를 메서드 안에 넣어 실행하므로 using을 못 쓴다. 그래서 UnityEngine. 을 붙여 쓴다.
CSHARP = """
var cam = UnityEngine.Camera.main;                      // 씬에서 MainCamera 태그가 붙은 카메라
if (cam == null) throw new System.Exception("씬에 Camera.main(MainCamera 태그)이 없습니다");
var rt = new UnityEngine.RenderTexture(960, 540, 24);   // 캡처 해상도
var tex = new UnityEngine.Texture2D(rt.width, rt.height, UnityEngine.TextureFormat.RGB24, false);
cam.targetTexture = rt;                                 // 화면 대신 rt에 그리게 한다
cam.Render();
UnityEngine.RenderTexture.active = rt;                  // ReadPixels는 active RT에서 읽는다
tex.ReadPixels(new UnityEngine.Rect(0, 0, rt.width, rt.height), 0, 0);
cam.targetTexture = null;                               // 카메라를 원래대로 되돌린다
UnityEngine.RenderTexture.active = null;
var png = UnityEngine.ImageConversion.EncodeToPNG(tex);
UnityEngine.Object.DestroyImmediate(rt);                // 임시 텍스처 정리 (메모리 누수 방지)
UnityEngine.Object.DestroyImmediate(tex);
return System.Convert.ToBase64String(png);
"""

p = argparse.ArgumentParser(description="Camera.main 화면을 captures/에 PNG로 저장")
p.add_argument("name", help="작업 이름 (예: player-solid-refactor)")
p.add_argument("--force", action="store_true", help="같은 이름의 파일이 있어도 덮어쓴다")
args = p.parse_args()

out = ROOT / "captures" / f"{datetime.date.today().isoformat()}_{args.name}.png"
rel = out.relative_to(ROOT).as_posix()
if out.exists() and not args.force:
    sys.exit(f"이미 있습니다: {rel}  (덮어쓰려면 --force)")

# --caller/--skill 은 명령 이름(eval) 앞에, --project-path 에는 실행 시점에 계산한 절대 경로 (ENV-7)
cmd = ["unity", "command", "--caller", "plugin", "--skill", "unity-cli", "--format", "json",
       "eval", CSHARP, "--project-path", str(ROOT)]
try:
    res = json.loads(subprocess.run(cmd, capture_output=True, encoding="utf-8").stdout)
except (OSError, ValueError):  # unity CLI가 없거나 출력이 JSON이 아님
    res = {"success": False, "errors": [{"message": "unity CLI를 실행하지 못했습니다"}]}
if not res.get("success"):
    print("Unity 에디터에 캡처를 요청하지 못했습니다:", *[e.get("message") for e in res.get("errors", [])], sep="\n  ")
    print("에디터가 떠 있는지 확인하세요 (unity status). 띄우는 방법: .claude/environment/Environment-Setting-Rules.md 의 ENV-7")
    sys.exit(1)
out.write_bytes(base64.b64decode(res["data"]["result"]["result"]))
print(rel)
