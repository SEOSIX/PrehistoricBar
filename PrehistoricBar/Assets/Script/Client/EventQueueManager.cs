using UnityEngine;
using System.Collections.Generic;
using Script.Bar;
using Script.Objects;

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

            int nbCocktails = Random.Range(minCocktails, maxCocktails + 1);
            List<GameObject> availablePrefabs = new List<GameObject>(possiblesCocktails);

            for (int j = 0; j < nbCocktails; j++)
            {
                if (availablePrefabs.Count == 0) break; 

                int randomIndex = Random.Range(0, availablePrefabs.Count);
                GameObject prefab = availablePrefabs[randomIndex];

                availablePrefabs.RemoveAt(randomIndex);

                ClientClass cocktail = new ClientClass();
                cocktail.cocktailsImage.Add(prefab);

                var data = prefab.GetComponent<Cocktails>();
                if (data != null)
                {
                    cocktail.index = j;
                }
                else
                {
                    cocktail.index = j;
                }

                client.cocktails.Add(cocktail);
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

    public List<ClientData> PeakNextClient(int count)
    {
        List<ClientData> previewClients = new List<ClientData>();
        int i = 0;
        foreach (var clients in eventClient)
        {
            if (i >= count)
            {
                break;
            }
            previewClients.Add(clients);
            i++;
        }
        return previewClients;
    }
}