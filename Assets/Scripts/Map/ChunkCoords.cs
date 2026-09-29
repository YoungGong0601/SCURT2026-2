using UnityEngine;

// 타일 좌표와 청크 좌표 계산의 유일한 곳. (SSOT)
// 청크 좌표: 월드를 chunkSize x chunkSize 칸 묶음으로 나눈 번호. 크기 16이면 타일 (0~15, 0~15)가 청크 (0, 0).
public static class ChunkCoords
{
    // 타일이 속한 청크. 정수 나눗셈은 음수에서 틀리므로(-1 / 16 == 0) FloorToInt를 쓴다(-1 / 16f -> -1).
    public static Vector2Int ChunkOf(Vector3Int tile, int chunkSize)
    {
        return new Vector2Int(
            Mathf.FloorToInt(tile.x / (float)chunkSize),
            Mathf.FloorToInt(tile.y / (float)chunkSize));
    }

    // 청크의 왼쪽 아래 타일 좌표.
    public static Vector3Int ChunkOrigin(Vector2Int chunk, int chunkSize)
    {
        return new Vector3Int(chunk.x * chunkSize, chunk.y * chunkSize, 0);
    }

    // 청크가 차지하는 타일 영역 (SetTilesBlock에 넘기는 범위).
    public static BoundsInt ChunkBounds(Vector2Int chunk, int chunkSize)
    {
        return new BoundsInt(ChunkOrigin(chunk, chunkSize), new Vector3Int(chunkSize, chunkSize, 1));
    }

    // 청크 안 i번째 칸의 타일 좌표. SetTilesBlock 배열 순서: x가 먼저 늘고, 한 줄이 차면 y가 는다.
    public static Vector3Int TileAt(Vector2Int chunk, int i, int chunkSize)
    {
        return ChunkOrigin(chunk, chunkSize) + new Vector3Int(i % chunkSize, i / chunkSize, 0);
    }

    // 두 청크 사이 거리 (가로, 세로 중 큰 쪽). 반경 r 안 = 한 변 2r+1인 정사각형.
    public static int Distance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }
}
