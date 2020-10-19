using UnityEngine;

public class Mine : MonoBehaviour {
    [SerializeField]
    private GameObject gfx = default;
    [SerializeField]
    private float fullScaleDuration = 5f;

    private float timeOfExplosion;

    public void SetDuration(float duration) {
        timeOfExplosion = Time.timeSinceLevelLoad + duration;
    }

    protected void FixedUpdate() {
        float timeLeft = timeOfExplosion - Time.timeSinceLevelLoad;
        if (timeLeft <= 0) {
            Destroy(gameObject);
            return;
        }
        gfx.transform.localScale = Vector3.one * Mathf.Clamp01(timeLeft / fullScaleDuration);
    }
}