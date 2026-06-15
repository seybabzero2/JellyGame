using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public void PlayBTN() {
        SceneManager.LoadScene(1);
    }
    public void ExitBTN() {
        Application.Quit();
    }
}
