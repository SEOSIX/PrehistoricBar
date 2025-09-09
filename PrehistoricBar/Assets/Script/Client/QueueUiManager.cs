using System;
using UnityEngine;
using System.Collections.Generic;
using Script.Client;
using TMPro;
using UnityEngine.UI;

public class QueueUiManager : MonoBehaviour
{
    [SerializeField] private EventQueueManager QueueManager;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Transform imageContainer;
    [SerializeField] private GameObject imagePrefab;

    private EventData currentClient;
    private void Update()
    {
        NewClient();
    }

    public void ShowNextClient()
    {
        currentClient = QueueManager.GetNextClient();

        foreach (Transform child in imageContainer)
        {
            Destroy(child.gameObject);
        }

        if (currentClient != null)
        {
            Debug.Log("journée terminée");
            return;
        }
        messageText.text = currentClient.message;

        foreach (var sprite in currentClient.images)
        {
            GameObject go = Instantiate(imagePrefab, imageContainer);
            go.GetComponent<Image>().sprite = sprite;
        }
    }

    void NewClient()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowNextClient();
        }
    }
}
