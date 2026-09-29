using UnityEngine;

// ChunkManager의 측정값을 화면 왼쪽 위에 보여 준다. 읽기만 하고 아무것도 소유하지 않는다. (SRP)
public class MapStatsOverlay : MonoBehaviour
{
    public ChunkManager chunks;

    string text = "";
    int shownGenerated = -1;
    Vector2Int shownChunk;

    void OnGUI()
    {
        if (chunks == null || chunks.Stats == null) return;
        ChunkStats s = chunks.Stats;

        // 값이 바뀔 때만 글자를 새로 만든다 (매 프레임 문자열 생성은 GC 쓰레기가 되어 측정을 흐린다)
        if (s.Generated != shownGenerated || chunks.CenterChunk != shownChunk)
        {
            shownGenerated = s.Generated;
            shownChunk = chunks.CenterChunk;
            text = $"활성 청크: {s.ActiveChunks}\n" +
                   $"생성한 청크: {s.Generated}\n" +
                   $"청크 생성 ms: 마지막 {s.LastMs:F3}, 평균 {s.AverageMs:F3}, 최대 {s.MaxMs:F3}\n" +
                   $"플레이어 청크: ({shownChunk.x}, {shownChunk.y})";
        }

        GUI.Box(new Rect(8, 8, 360, 80), GUIContent.none); // 글자가 잘 보이게 어두운 배경
        GUI.Label(new Rect(16, 12, 350, 76), text);
    }
}
