using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 플레이어 주변 청크를 만들고, 멀어진 청크를 지운다. 스트리밍만 책임진다. (SRP)
// 타일 종류는 ITileGenerator에게 묻기만 하고, 어떤 알고리즘인지는 모른다. (Strategy 패턴, DIP)
public class ChunkManager : MonoBehaviour
{
    public MapSettings settings;
    public TilePalette palette;
    public Tilemap tilemap;
    public Transform target; // 따라갈 대상 (플레이어)

    public ChunkStats Stats { get; private set; }
    public Vector2Int CenterChunk { get; private set; } // 플레이어가 있는 청크

    ITileGenerator generator;
    int size; // 청크 한 변. 시작할 때 읽어 고정 (버퍼 크기와 맞아야 해서)
    readonly HashSet<Vector2Int> loaded = new HashSet<Vector2Int>(); // 타일맵에 있는 청크
    readonly Queue<Vector2Int> queue = new Queue<Vector2Int>();      // 만들 차례를 기다리는 청크 (가까운 순)
    readonly List<Vector2Int> toUnload = new List<Vector2Int>();     // 지울 청크 임시 목록 (재사용)
    readonly System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
    TileBase[] buffer; // 청크 하나 분량 그림. 한 번만 만들어 재사용 (GC 방지)
    TileBase[] empty;  // 전부 null. 청크 지우기용, 재사용
    bool started;

    void Awake()
    {
        if (settings == null || palette == null || tilemap == null || target == null)
        {
            Debug.LogError("ChunkManager: settings, palette, tilemap, target을 모두 연결해야 한다", this);
            enabled = false;
            return;
        }

        // 구체 생성기를 고르는 곳은 여기 한 곳
        generator = new PerlinTileGenerator(settings);
        size = settings.chunkSize;
        buffer = new TileBase[size * size];
        empty = new TileBase[size * size];
        Stats = new ChunkStats(loaded);
    }

    void Update()
    {
        Vector2Int c = ChunkCoords.ChunkOf(tilemap.WorldToCell(target.position), size);
        if (!started || c != CenterChunk)
        {
            started = true;
            CenterChunk = c;
            OnCenterChanged();
        }

        // 한 프레임에 최대 chunksPerFrame개만 만든다 (한꺼번에 만들면 끊김)
        for (int i = 0; i < settings.chunksPerFrame && queue.Count > 0; i++)
            Generate(queue.Dequeue());
    }

    // 플레이어 청크가 바뀌면: 멀어진 청크를 지우고, 대기열을 새로 짠다.
    void OnCenterChanged()
    {
        // 1) 지우는 반경 밖의 청크를 null로 덮어 지운다
        toUnload.Clear();
        foreach (Vector2Int c in loaded)
            if (ChunkCoords.Distance(c, CenterChunk) > settings.UnloadRadius) toUnload.Add(c);
        foreach (Vector2Int c in toUnload)
        {
            tilemap.SetTilesBlock(ChunkCoords.ChunkBounds(c, size), empty);
            loaded.Remove(c);
        }

        // 2) 대기열을 처음부터 다시 짠다: 거리 0, 1, 2 ... 테두리 순서(가까운 청크 먼저), 이미 있는 청크는 뺀다.
        //    다시 짜므로 중복이 없고, 이제 멀어진 대기 청크도 자연히 빠진다.
        queue.Clear();
        int r = settings.loadRadius;
        for (int d = 0; d <= r; d++)
            for (int x = -d; x <= d; x++)
                for (int y = -d; y <= d; y++)
                {
                    Vector2Int c = CenterChunk + new Vector2Int(x, y);
                    if (ChunkCoords.Distance(c, CenterChunk) == d && !loaded.Contains(c)) queue.Enqueue(c);
                }
    }

    // 청크 하나를 생성기로 채워 타일맵에 한 번에 넣고, 걸린 시간을 기록한다.
    void Generate(Vector2Int chunk)
    {
        watch.Restart();
        for (int i = 0; i < buffer.Length; i++)
        {
            Vector3Int t = ChunkCoords.TileAt(chunk, i, size);
            buffer[i] = palette.Get(generator.GetTile(t.x, t.y));
        }
        tilemap.SetTilesBlock(ChunkCoords.ChunkBounds(chunk, size), buffer);
        loaded.Add(chunk);
        watch.Stop();
        Stats.Record(watch.Elapsed.TotalMilliseconds);
    }

    // 그 청크에 대한 생성기 결과의 해시. 같은 시드면 언제나 같아야 한다 (결정론 검증용).
    public int ChunkHash(Vector2Int chunk)
    {
        int h = 17;
        for (int i = 0; i < size * size; i++)
        {
            Vector3Int t = ChunkCoords.TileAt(chunk, i, size);
            h = unchecked(h * 31 + (int)generator.GetTile(t.x, t.y));
        }
        return h;
    }
}
