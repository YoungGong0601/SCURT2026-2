using UnityEngine;

// Perlin 노이즈로 타일 종류를 고른다. (Strategy 패턴: 구체 전략 하나, 순수 함수)
// Perlin 노이즈: 가까운 좌표끼리 비슷한 0~1 값을 주는 부드러운 무늬. 그래서 물, 모래, 풀, 숲이 덩어리로 모인다.
// 결과는 (seed, x, y)로만 정해진다. 만든 뒤에는 상태가 바뀌지 않는다 (지웠다 다시 만들어도 같은 맵).
public class PerlinTileGenerator : ITileGenerator
{
    readonly int offsetX, offsetY; // 시드로 정한 노이즈 위치. Mathf.PerlinNoise에는 시드가 없어서 이렇게 넣는다
    readonly float scale, waterMax, sandMax, grassMax;

    // 설정값은 만들 때 한 번 읽어 고정한다 (플레이 중 인스펙터를 바꿔도 이미 만든 맵과 섞이지 않게)
    public PerlinTileGenerator(MapSettings s)
    {
        var rng = new System.Random(s.seed); // 같은 시드 = 같은 오프셋
        offsetX = rng.Next(0, 10000);         // 0~10000: 너무 크면 float 정밀도가 떨어진다
        offsetY = rng.Next(0, 10000);
        scale = s.noiseScale;
        waterMax = s.waterMax;
        sandMax = s.sandMax;
        grassMax = s.grassMax;
    }

    public TileType GetTile(int x, int y)
    {
        // scale을 곱해 소수 좌표로 만든다 (정수 격자점에서는 값이 늘 같다). 범위를 살짝 벗어날 수 있어 Clamp01.
        float n = Mathf.Clamp01(Mathf.PerlinNoise((x + offsetX) * scale, (y + offsetY) * scale));
        if (n < waterMax) return TileType.Water;
        if (n < sandMax) return TileType.Sand;
        if (n < grassMax) return TileType.Grass;
        return TileType.Forest;
    }
}
