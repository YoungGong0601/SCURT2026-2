using UnityEngine;

// 실험용 자동 입력 (헤드리스에서 키보드 없이 걷기). 정한 방향을 계속 돌려준다. (Strategy 패턴: 구체 전략 하나)
// 씬에는 붙이지 않는다. 실험할 때 실행 중에 붙이고 PlayerMovement.SetInput으로 바꿔 끼운다.
public class AutoMoveInput : MonoBehaviour, IMoveInput
{
    public Vector2 direction = Vector2.right; // 가고 싶은 방향. (0, 0)이면 멈춤

    public Vector2 GetDirection()
    {
        return direction;
    }
}
