using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using Script.Bar;

public class QueueUiManager : MonoBehaviour
{
    [SerializeField] private EventQueueManager queueManager;
    [SerializeField] private Transform spawnContainer;
    [SerializeField] private TextMeshProUGUI nameText;

    private ClientData currentClient;
    private CocktailClass currentCocktail;
    private int currentCocktailIndex = 0;

    // Pour retrouver les instances instanciées en scène
    private List<GameObject> spawnedCocktails = new List<GameObject>();

    public void ShowNextClient()
    {
        currentClient = queueManager.GetNextClient();
        currentCocktailIndex = 0;

        foreach (Transform child in spawnContainer)
            Destroy(child.gameObject);

        spawnedCocktails.Clear();

        if (currentClient == null)
        {
            Debug.Log("Plus de clients !");
            nameText.text = "Fin de la file";
            return;
        }

        foreach (var cocktail in currentClient.cocktails)
        {
            foreach (var prefab in cocktail.cocktailsImage)
            {
                if (prefab != null)
                {
                    var instance = Instantiate(prefab, spawnContainer);
                    spawnedCocktails.Add(instance);
                }
            }
        }

        if (currentClient.cocktails.Count > 0)
            currentCocktail = currentClient.cocktails[0];

        nameText.text = currentClient.name;
    }

    void NextCocktail()
    {
        if (currentClient == null || currentClient.cocktails.Count == 0)
            return;

        if (currentCocktailIndex < currentClient.cocktails.Count)
        {
            currentCocktail = currentClient.cocktails[currentCocktailIndex];
            Debug.Log($"Cocktail en cours : {currentCocktail.name}");
        }
        else
        {
            Debug.Log("Tous les cocktails de ce client sont servis !");
        }
    }

    private void ValidateCocktail()
    {
        if (currentCocktail != null)
            ShowDoneText(currentCocktail);

        currentCocktailIndex++;
        NextCocktail();
    }

    private void ShowDoneText(CocktailClass cocktail)
    {
        // On prend la première image de ce cocktail pour attacher le texte
        if (cocktail.cocktailsImage.Count == 0) return;

        var targetImage = spawnedCocktails.Find(x => x.name.Contains(cocktail.cocktailsImage[0].name));
        if (targetImage == null) return;

        var doneTextGO = new GameObject("DoneText");
        doneTextGO.transform.SetParent(targetImage.transform, false);

        var text = doneTextGO.AddComponent<TextMeshProUGUI>();
        text.text = "DONE";
        text.fontSize = 28;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.green;

        var rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -20);
    }

    // Inputs
    void OnColors(InputValue value)
    {
        if (!value.isPressed || currentCocktail == null) return;
        if (currentCocktail.name == "Cocktail0") ValidateCocktail();
    }

    void OnColors1(InputValue value)
    {
        if (!value.isPressed || currentCocktail == null) return;
        if (currentCocktail.name == "Cocktail1") ValidateCocktail();
    }

    void OnColors2(InputValue value)
    {
        if (!value.isPressed || currentCocktail == null) return;
        if (currentCocktail.name == "Cocktail2") ValidateCocktail();
    }

    void OnColors3(InputValue value)
    {
        if (!value.isPressed || currentCocktail == null) return;
        if (currentCocktail.name == "Cocktail3") ValidateCocktail();
    }

    void OnColors4(InputValue value)
    {
        if (!value.isPressed || currentCocktail == null) return;
        if (currentCocktail.name == "Cocktail4") ValidateCocktail();
    }

    void OnNextClient(InputValue value)
    {
        if (value.isPressed) ShowNextClient();
    }
}
