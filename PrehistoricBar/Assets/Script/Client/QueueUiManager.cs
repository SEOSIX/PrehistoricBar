using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Script.Bar;
using Script.Objects;

public class QueueUiManager : MonoBehaviour
{
    [SerializeField] private EventQueueManager queueManager;
    [SerializeField] private Transform spawnContainer;
    [SerializeField] private TextMeshProUGUI nameText;

    private ClientData currentClient;
    private List<GameObject> spawnedCocktails = new List<GameObject>();
    private List<CocktailClass> remainingCocktails = new List<CocktailClass>();

    // Pour chaque cocktail en cours, on garde les ingrédients restants à valider
    private Dictionary<CocktailClass, HashSet<IngredientIndex>> cocktailIngredientsRemaining = new();

    public void ShowNextClient()
    {
        currentClient = queueManager.GetNextClient();

        foreach (Transform child in spawnContainer)
            Destroy(child.gameObject);

        spawnedCocktails.Clear();
        remainingCocktails.Clear();
        cocktailIngredientsRemaining.Clear();

        if (currentClient == null)
        {
            Debug.Log("Plus de clients !");
            nameText.text = "Fin de la file";
            return;
        }

        foreach (var cocktail in currentClient.cocktails)
        {
            remainingCocktails.Add(cocktail);
            cocktailIngredientsRemaining[cocktail] = new HashSet<IngredientIndex>();

            // Copie des ingrédients depuis le prefab
            foreach (var prefab in cocktail.cocktailsImage)
            {
                if (prefab != null)
                {
                    var instance = Instantiate(prefab, spawnContainer);
                    spawnedCocktails.Add(instance);

                    var data = prefab.GetComponent<Cocktails>();
                    if (data != null)
                        foreach (var ingredient in data.cocktailIndices)
                            cocktailIngredientsRemaining[cocktail].Add(ingredient);
                }
            }
        }

        nameText.text = currentClient.name;
        Debug.Log($"Client {currentClient.name} avec {currentClient.cocktails.Count} cocktails");
    }

    private void ValidateIngredient(IngredientIndex ingredient)
    {
        if (currentClient == null) return;

        foreach (var cocktail in remainingCocktails.ToArray())
        {
            if (cocktailIngredientsRemaining[cocktail].Contains(ingredient))
            {
                cocktailIngredientsRemaining[cocktail].Remove(ingredient);
                Debug.Log($"Ingrédient {ingredient} correct pour {cocktail.name}");

                if (cocktailIngredientsRemaining[cocktail].Count == 0)
                {
                    // Tous les ingrédients sont validés
                    ValidateCocktail(cocktail);
                }

                return; // On ne valide l'ingrédient qu'une seule fois
            }
        }

        Debug.Log($"Ingrédient {ingredient} incorrect ou déjà utilisé !");
    }

    private void ValidateCocktail(CocktailClass cocktail)
    {
        if (cocktail != null)
            ShowDoneText(cocktail);

        remainingCocktails.Remove(cocktail);
        cocktailIngredientsRemaining.Remove(cocktail);

        if (remainingCocktails.Count == 0)
            Debug.Log("Tous les cocktails du client sont servis !");
    }

    private void ShowDoneText(CocktailClass cocktail)
    {
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
    void OnColors(InputValue value)   { TryValidateIngredient(IngredientIndex.cocktail0, value); }
    void OnColors1(InputValue value)  { TryValidateIngredient(IngredientIndex.cocktail1, value); }
    void OnColors2(InputValue value)  { TryValidateIngredient(IngredientIndex.cocktail2, value); }
    void OnColors3(InputValue value)  { TryValidateIngredient(IngredientIndex.cocktail3, value); }
    void OnColors4(InputValue value)  { TryValidateIngredient(IngredientIndex.cocktail4, value); }

    private void TryValidateIngredient(IngredientIndex ingredient, InputValue value)
    {
        if (!value.isPressed) return;
        ValidateIngredient(ingredient);
    }

    void OnNextClient(InputValue value)
    {
        if (value.isPressed) ShowNextClient();
    }
}
