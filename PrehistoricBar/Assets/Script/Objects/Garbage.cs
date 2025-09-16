using UnityEngine;
using UnityEngine.InputSystem;

public class Garbage : MonoBehaviour
{
    void OnGarbage(InputValue value)
    {
        if (!value.isPressed) return;

        if (QueueUiManager.instance != null && value.isPressed)
        {
            Debug.Log("poubelle");
            QueueUiManager.instance.RestartCurrentCocktail();
        }
    }   
}
