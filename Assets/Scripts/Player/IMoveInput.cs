using UnityEngine;

// 이동 입력의 약속. 이동 코드는 키보드가 아니라 이 약속에만 의존한다. (전략 패턴, DIP)
public interface IMoveInput
{
    // 가고 싶은 방향. (0, 0)이면 입력 없음. 길이는 1이 아니어도 된다.
    Vector2 GetDirection();
}
