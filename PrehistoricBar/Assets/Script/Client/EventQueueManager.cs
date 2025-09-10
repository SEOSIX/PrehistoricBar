using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EventQueueManager : MonoBehaviour
{
    [Header("Configuration")] 
    [SerializeField] private string[] possiblesNames = { "Ouga", "booga", "toonah" };
    [SerializeField] private List<GameObject> possiblesCocktails;
    [SerializeField] private int minClient = 5;
    [SerializeField] private int maxClient = 10;
    
    [SerializeField] private int minCocktails = 1;
    [SerializeField] private int maxCocktails = 2;
    
    private Queue<ClientData> eventClient = new Queue<ClientData>();

    void Start()
    {
        GenerateRandomClient();
    }


    void GenerateRandomClient()
    {
        int nbClients = Random.Range(minClient, maxClient + 1);

        for (int i = 0; i < nbClients; i++)
        {
            ClientData client = new ClientData();
            
            client.name = possiblesNames[Random.Range(0, possiblesNames.Length)];
            
            int nbParts =  Random.Range(minCocktails, maxCocktails + 1);
            client.imagesPrefab = new List<GameObject>();

            for (int j = 0; j < nbParts; j++)
            {
                GameObject prefab = possiblesCocktails[Random.Range(0, possiblesCocktails.Count)];
                client.imagesPrefab.Add(prefab);
            }

            eventClient.Enqueue(client);
        }
        
        Debug.Log($"File générée : {nbClients} clients.");
    }

    public ClientData GetNextClient()
    {
        return eventClient.Count > 0 ? eventClient.Dequeue() : null;
    }

    public bool HasClient()
    {
        return eventClient.Count > 0;
    }
}
