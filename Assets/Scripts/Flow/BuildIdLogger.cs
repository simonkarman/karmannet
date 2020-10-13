using UnityEngine;

public class BuildIdLogger : MonoBehaviour {
    protected void Start() {
        string buildId = Application.isEditor ? "<IN EDITOR>" : Application.buildGUID;
        Debug.Log(string.Format("Build id: {0}", buildId));
    }
}
