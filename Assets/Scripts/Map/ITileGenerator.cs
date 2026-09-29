// 타일 좌표 -> 타일 종류의 약속. 새 생성 알고리즘은 이것을 구현해 "추가"한다. ChunkManager는 고치지 않는다. (Strategy 패턴, OCP, DIP)
public interface ITileGenerator
{
    // 같은 좌표는 언제나 같은 종류를 돌려줘야 한다 (결정론).
    TileType GetTile(int x, int y);
}
