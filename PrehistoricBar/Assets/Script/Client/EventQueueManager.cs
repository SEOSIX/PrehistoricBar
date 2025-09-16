using System;
using UnityEngine;
using System.Collections.Generic;
using Script.Bar;
using Script.Objects;
using Random = UnityEngine.Random;

[System.Serializable]
public class EventQueueManager : MonoBehaviour
{
    public static EventQueueManager instance;
    
    [Header("Configuration")] 
    [SerializeField] private string[] possiblesNames = { "Ouga", "booga", "toonah" };
    [SerializeField] private List<GameObject> possiblesCocktails;
    [SerializeField] private int minClient = 5;
    [SerializeField] private int maxClient = 10;
    
    [SerializeField] private int minCocktails = 1;
    [SerializeField] private int maxCocktails = 2;
    
    private Queue<ServiceData> eventService = new Queue<ServiceData>();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        GenerateRandomClient();
    }

    void GenerateRandomClient()
    {
        int nbClients = Random.Range(minClient, maxClient + 1);

        for (int i = 0; i < nbClients; i++)
        {
            ServiceData service = new ServiceData();
            service.name = possiblesNames[Random.Range(0, possiblesNames.Length)];

            int nbCocktails = Random.Range(minCocktails, maxCocktails + 1);
            List<GameObject> availablePrefabs = new List<GameObject>(possiblesCocktails);

            for (int j = 0; j < nbCocktails; j++)
            {
                if (availablePrefabs.Count == 0) break; 

                int randomIndex = Random.Range(0, availablePrefabs.Count);
                GameObject prefab = availablePrefabs[randomIndex];

                availablePrefabs.RemoveAt(randomIndex);

                ClientClass client = new ClientClass();
                client.cocktailsImage.Enqueue(prefab);

                var data = prefab.GetComponent<Cocktails>();
                if (data != null)
                {
                    client.index = j;
                }
                else
                {
                    client.index = j;
                }

                service.clients.Add(client);
            }

            eventService.Enqueue(service);
        }

        Debug.Log($"File générée : {nbClients} clients.");
    }
    
    public ServiceData GetNextService()
    {
        return eventService.Count > 0 ? eventService.Dequeue() : null;
    }

    public bool HasClient()
    {
        return eventService.Count > 0;
    }

    public List<ServiceData> PeakNextClient(int count)
    {
        List<ServiceData> previewClients = new List<ServiceData>();
        int i = 0;
        foreach (var clients in eventService)
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

    public static ServiceData GetcurrentClient()
    {
        return instance.eventService.Peek();
    }

    public static Cocktails GetCurrentCocktail()
    {
        return null;
    }
}