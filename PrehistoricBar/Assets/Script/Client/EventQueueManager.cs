using UnityEngine;
using System.Collections.Generic;
using Script.Bar;

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

            for (int j = 0; j < nbCocktails; j++)
            {
                CocktailClass cocktail = new CocktailClass();
                cocktail.name = "Cocktail" + j;

                GameObject prefab = possiblesCocktails[Random.Range(0, possiblesCocktails.Count)];
                cocktail.cocktailsImage.Add(prefab);

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
}