using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LaunchGame : MonoBehaviour
{
    public string gameSceneName;
    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
