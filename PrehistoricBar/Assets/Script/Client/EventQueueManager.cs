using Script.Client;
using UnityEngine;
using System.Collections.Generic;

public class EventQueueManager : MonoBehaviour
{
    [SerializeField] private List<ClientData> allClient = new List<ClientData>();
    
    private Client<ClientData> eventClient = new Client<ClientData>();


    void Start()
    {
        foreach (var e in allClient)
        {
            eventClient.Enqueue(e);
        }
    }

    public EventData GetNextClient()
    {
        return eventClient.Count > 0 ? eventClient.Dequeue() : null;
    }

    public bool HasClient()
    {
        return eventClient.Count > 0;
    }
}
