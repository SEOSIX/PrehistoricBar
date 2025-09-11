using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Script.Bar;
using Script.Objects;
using UnityEngine.UI;
using System.Collections;

public class QueueUiManager : MonoBehaviour
{
    [SerializeField] private EventQueueManager queueManager;
    [SerializeField] private Transform spawnContainer;
    

    [Header("Timer")]
    [SerializeField] private Slider timerSlider; 
    [SerializeField] private float clientTime = 10f;
    private float currentTime;
    private Coroutine timerCoroutine;

    private ClientData currentClient;
    private List<GameObject> spawnedCocktails = new List<GameObject>();
    private List<CocktailClass> remainingCocktails = new List<CocktailClass>();

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
            return;
        }

        foreach (var cocktail in currentClient.cocktails)
        {
            remainingCocktails.Add(cocktail);
            cocktailIngredientsRemaining[cocktail] = new HashSet<IngredientIndex>();
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

        Debug.Log($"Client {currentClient.name} avec {currentClient.cocktails.Count} cocktails");
        StartTimer(clientTime);
    }
    private void StartTimer(float duration)
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        currentTime = duration;
        timerSlider.maxValue = duration;
        timerSlider.value = duration;
        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            if (currentTime < 0) currentTime = 0;

            timerSlider.value = currentTime;
            yield return null;
        }

        Debug.Log("tu t'es pas goon assez fort");
  
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
                    ValidateCocktail(cocktail);
                }

                return;
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
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.BottomLeft;
        text.color = Color.green;
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
