using UnityEngine;

public class Character : MonoBehaviour {
    [SerializeField]
    private SpriteRenderer spriteRenderer = default;
    [SerializeField]
    private float moveSpeed = 10f;

    public void SetColor(Color color) {
        spriteRenderer.color = color;
    }

    protected void Update() {
        transform.position += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f) * Time.deltaTime * moveSpeed;
    }
}
