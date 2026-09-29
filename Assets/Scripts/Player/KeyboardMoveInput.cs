using UnityEngine;
using UnityEngine.InputSystem;

// W/A/S/D 키를 방향으로 바꾼다. 키를 아는 곳은 여기 하나뿐. (전략 패턴: 구체 전략 하나)
public class KeyboardMoveInput : MonoBehaviour, IMoveInput
{
    public Vector2 GetDirection()
    {
        Keyboard k = Keyboard.current;
        if (k == null) return Vector2.zero; // 키보드가 연결되지 않은 경우

        Vector2 dir = Vector2.zero;
        if (k.wKey.isPressed) dir.y += 1;
        if (k.sKey.isPressed) dir.y -= 1;
        if (k.aKey.isPressed) dir.x -= 1;
        if (k.dKey.isPressed) dir.x += 1;
        return dir;
    }
}
