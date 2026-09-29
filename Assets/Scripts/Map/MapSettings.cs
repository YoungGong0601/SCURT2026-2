using UnityEngine;

// 맵 설정값의 단일 출처. 값을 바꾸려면 이 에셋만 바꾼다. (SSOT)
// seed, chunkSize, 노이즈 값은 플레이 시작 때 읽어 고정된다. loadRadius, chunksPerFrame은 플레이 중 바꿔도 된다.
[CreateAssetMenu(fileName = "MapSettings", menuName = "CurtInfMap/Map Settings")]
public class MapSettings : ScriptableObject
{
    [Header("결정론")]
    public int seed = 12345; // 같은 시드 = 같은 맵

    [Header("청크 스트리밍")]
    [Min(1)] public int chunkSize = 16;     // 청크 한 변의 타일 수
    [Min(0)] public int loadRadius = 2;     // 플레이어 청크에서 이 거리까지 만든다: (2r+1)^2 청크
    [Min(1)] public int chunksPerFrame = 1; // 한 프레임에 만드는 최대 청크 수 (끊김 방지)

    [Header("노이즈 (값이 작은 쪽부터 물, 모래, 풀, 숲)")]
    public float noiseScale = 0.05f;             // 작을수록 지형 덩어리가 크다
    [Range(0, 1)] public float waterMax = 0.35f; // 노이즈 값이 이보다 작으면 물
    [Range(0, 1)] public float sandMax = 0.45f;  // 이보다 작으면 모래
    [Range(0, 1)] public float grassMax = 0.7f;  // 이보다 작으면 풀, 나머지는 숲

    // 지우는 반경. 로드 반경보다 1 크게 둬서 경계에서 만들고 지우기를 반복하지 않게 한다 (계산은 여기 한 곳)
    public int UnloadRadius => loadRadius + 1;
}
