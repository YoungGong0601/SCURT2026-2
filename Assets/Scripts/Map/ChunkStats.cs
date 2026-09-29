using System.Collections.Generic;
using UnityEngine;

// 청크 측정값. 스트리밍과 분리해 측정만 책임진다. (SRP)
public class ChunkStats
{
    readonly IReadOnlyCollection<Vector2Int> loaded; // ChunkManager의 로드된 청크 목록 (읽기만)

    public ChunkStats(IReadOnlyCollection<Vector2Int> loaded)
    {
        this.loaded = loaded;
    }

    public int ActiveChunks => loaded.Count;    // 지금 타일맵에 있는 청크 수 (복사하지 않고 원본에서 읽는다)
    public int Generated { get; private set; }  // 지금까지 만든 청크 수 (지웠다 다시 만든 것 포함)
    public double LastMs { get; private set; }  // 마지막 청크 생성 시간
    public double MaxMs { get; private set; }   // 가장 오래 걸린 청크
    public double AverageMs => Generated == 0 ? 0 : totalMs / Generated;

    double totalMs;

    // 청크 하나를 만든 시간(ms)을 기록한다.
    public void Record(double ms)
    {
        Generated++;
        LastMs = ms;
        totalMs += ms;
        if (ms > MaxMs) MaxMs = ms;
    }
}
