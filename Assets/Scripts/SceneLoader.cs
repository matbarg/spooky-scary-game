using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Called by the button
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
