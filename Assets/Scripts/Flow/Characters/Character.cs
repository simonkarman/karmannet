using UnityEngine;

public class Character : MonoBehaviour {
    [SerializeField]
    private SpriteRenderer spriteRenderer = default;
    [SerializeField]
    private Rigidbody2D rigidbody2d = default;
    [SerializeField]
    private float moveSpeed = 1f;

    public void SetColor(Color color) {
        spriteRenderer.color = color;
    }

    protected void Update() {
        Vector2 input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f) * moveSpeed;
        rigidbody2d.AddForce(input, ForceMode2D.Impulse);
    }
}
