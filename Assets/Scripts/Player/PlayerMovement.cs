using System;
using UnityEngine;

// 입력 방향으로 움직이고, 바라보는 방향(Facing)을 가진다. (SRP, 관찰자 패턴: 알리는 쪽)
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f; // 초당 이동 거리 (1 unit = 16px 한 칸)

    // 마지막으로 움직인 방향 (길이 1). 멈춰도 그대로 남는다.
    public Vector2 Facing { get; private set; } = Vector2.down;

    // Facing이 바뀔 때만 알린다.
    public event Action<Vector2> FacingChanged;

    IMoveInput input; // 키보드인지 몰라도 된다 (같은 오브젝트의 IMoveInput 구현)

    void Awake()
    {
        input = GetComponent<IMoveInput>();
        if (input == null)
        {
            Debug.LogError("IMoveInput을 구현한 컴포넌트가 필요하다", this);
            enabled = false;
        }
    }

    // 입력을 실행 중에 바꿔 끼운다. 예: 헤드리스 실험에서 AutoMoveInput으로 교체 (Strategy 패턴: 전략 교체)
    public void SetInput(IMoveInput newInput)
    {
        input = newInput;
    }

    void Update()
    {
        Vector2 dir = input.GetDirection();
        if (dir == Vector2.zero) return; // 멈추면 마지막 방향 유지

        // normalized: 대각선으로 갈 때도 같은 속도
        dir = dir.normalized;
        transform.position += (Vector3)(dir * speed * Time.deltaTime);

        if (dir != Facing)
        {
            Facing = dir;
            FacingChanged?.Invoke(Facing);
        }
    }
}
