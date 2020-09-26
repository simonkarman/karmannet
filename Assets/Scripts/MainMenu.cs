using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public void ToServerScene() {
        SceneManager.LoadScene("Server");
    }
    public void ToClientScene() {
        SceneManager.LoadScene("Client");
    }
}
