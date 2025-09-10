using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class QueueUiManager : MonoBehaviour
{
    [SerializeField] private EventQueueManager queueManager;
    [SerializeField] private Transform spawnContainer;
    //[SerializeField] private TextMeshProUGUI nameText;


    private ClientData currentClient;

    public void ShowNextClient()
    {
        currentClient = queueManager.GetNextClient();
        
        foreach (Transform child in spawnContainer)
            Destroy(child.gameObject);

        if (currentClient == null)
        {
            Debug.Log("Plus de clients !");
            return;
        }
        
        foreach (var prefab in currentClient.imagesPrefab)
        {
            if (prefab != null)
                Instantiate(prefab, spawnContainer);
        }
        //name du client (si il y a un client récurent
        //nameText.text = currentClient.name;
    }

    void OnNextClient(InputValue value)
    {
        if (value.isPressed)
        {
            ShowNextClient();
        }
    }
}
