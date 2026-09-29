using UnityEngine;

// Facing이 바뀌면 그 방향의 스프라이트로 바꾼다. (SRP, 관찰자 패턴: 듣는 쪽)
[RequireComponent(typeof(SpriteRenderer), typeof(PlayerMovement))]
public class PlayerFacingSprite : MonoBehaviour
{
    public Sprite up, down, left, right;

    SpriteRenderer sr;
    PlayerMovement movement;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        movement = GetComponent<PlayerMovement>();
    }

    void OnEnable()
    {
        movement.FacingChanged += Apply;
        Apply(movement.Facing); // 켜질 때 지금 방향을 바로 반영
    }

    void OnDisable()
    {
        movement.FacingChanged -= Apply;
    }

    void Apply(Vector2 dir)
    {
        // 대각선일 때는 좌우 모습을 우선한다
        if (dir.x < 0) sr.sprite = left;
        else if (dir.x > 0) sr.sprite = right;
        else if (dir.y > 0) sr.sprite = up;
        else sr.sprite = down;
    }
}
