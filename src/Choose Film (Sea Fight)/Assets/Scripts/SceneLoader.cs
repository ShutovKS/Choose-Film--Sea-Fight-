using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private const string SCENE_HORIZONTAL = "Game scene - Horizontal";
    private const string SCENE_VERTICAL = "Game scene - Vertical";

    private void Start()
    {
        LoadScene();
    }

    private static void LoadScene()
    {
        var sceneName = Screen.width > Screen.height ? SCENE_HORIZONTAL : SCENE_VERTICAL;

        SceneManager.LoadScene(sceneName);
    }
}