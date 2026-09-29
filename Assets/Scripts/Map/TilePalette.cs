using UnityEngine;
using UnityEngine.Tilemaps;

// 타일 종류 -> 그림(TileBase). "어떤 종류에 어떤 그림"의 단일 출처. (SSOT)
[CreateAssetMenu(fileName = "TilePalette", menuName = "CurtInfMap/Tile Palette")]
public class TilePalette : ScriptableObject
{
    public TileBase water, sand, grass, forest;

    public TileBase Get(TileType t)
    {
        switch (t)
        {
            case TileType.Water: return water;
            case TileType.Sand: return sand;
            case TileType.Grass: return grass;
            default: return forest;
        }
    }
}
