using UnityEngine;
using UnityEngine.UI;

public class ScreenLogLine : MonoBehaviour {
    [SerializeField]
    private Text text = null;
    [SerializeField]
    private float openSizeScale = 2f;

    private RectTransform rectTransform;
    private bool open = true;
    private Vector2 initialSizeDelta;

    protected void Start() {
        rectTransform = GetComponent<RectTransform>();
        initialSizeDelta = rectTransform.sizeDelta;
        if (open) {
            ToggleOpen();
        }
    }

    public void ToggleOpen() {
        open = !open;
        text.horizontalOverflow = open ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
        text.alignment = open ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft;
        Vector2 sizeDelta = new Vector2(initialSizeDelta.x, initialSizeDelta.y * (open ? openSizeScale : 1f));
        rectTransform.sizeDelta = sizeDelta;
    }

    public void Set(string log, LogType type) {
        text.color = LogTypeToColor(type);
        text.text = string.Format("<i>{0}</i> {1}> {2}", Time.realtimeSinceStartup.ToString("00.000"), type, log);
    }

    private static Color LogTypeToColor(LogType type) {
        switch (type) {
        case LogType.Log:
            return Color.black;
        case LogType.Error:
            return Color.red;
        case LogType.Warning:
            return Color.magenta;
        case LogType.Assert:
        case LogType.Exception:
        default:
            return Color.gray;
        }
    }
}
