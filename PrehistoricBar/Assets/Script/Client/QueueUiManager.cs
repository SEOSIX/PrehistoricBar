using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Script.Bar;
using Script.Objects;

public class QueueUiManager : MonoBehaviour
{
    public static QueueUiManager instance { get; private set; }

    [SerializeField] private EventQueueManager queueManager;
    [SerializeField] private Transform spawnContainer;
    [SerializeField] private GameObject Over;

    [Header("Timer")]
    public Slider timerSlider;
    [SerializeField] private float clientTime = 10f;
    private float currentTime;
    private Coroutine timerCoroutine;
    private Coroutine blinkCoroutine;
    
    [Header("Tireuse")]
    public Cup cup;
    [Space(5f)]
    [SerializeField] private float tireuseLaitSpeed;
    [SerializeField] private float tireuseBaveSpeed;
    [SerializeField] private float tireuseAlcoolSpeed;
    
    private float baseTireuseLaitSpeed;
    private float baseTireuseBaveSpeed;
    private float baseTireuseAlcoolSpeed;
    

    private ClientData currentClient;
    private List<GameObject> spawnedCocktails = new List<GameObject>();
    private List<CocktailClass> remainingCocktails = new List<CocktailClass>();

    private Dictionary<CocktailClass, HashSet<IngredientIndex>> cocktailIngredientsRemaining = new();
    private Dictionary<CocktailClass, List<RecetteStep>> cocktailRecettes = new();

    [Header("UI Recette")]
    [SerializeField] private Transform recetteContainer;
    [SerializeField] private GameObject recetteTextPrefab;

    private Dictionary<CocktailClass, List<TextMeshProUGUI>> recetteTexts = new();
    
    

    void Awake()
    {
        instance = this;
        
        baseTireuseLaitSpeed = tireuseLaitSpeed;
        baseTireuseBaveSpeed = tireuseBaveSpeed;
        baseTireuseAlcoolSpeed = tireuseAlcoolSpeed;
    }

    public void ShowNextClient()
    {
        Over.SetActive(false);
        currentClient = queueManager.GetNextClient();

        foreach (Transform child in spawnContainer)
            Destroy(child.gameObject);

        foreach (Transform child in recetteContainer)
            Destroy(child.gameObject);

        spawnedCocktails.Clear();
        remainingCocktails.Clear();
        cocktailIngredientsRemaining.Clear();
        cocktailRecettes.Clear();
        recetteTexts.Clear();

        if (currentClient == null)
        {
            Debug.Log("Plus de clients !");
            StopAllCoroutines();
            timerCoroutine = null;
            blinkCoroutine = null;

            Over.SetActive(true);
            timerSlider.value = 0;
            timerSlider.fillRect.GetComponent<Image>().color = Color.white;
            return;
        }

        foreach (var cocktail in currentClient.cocktails)
        {
            remainingCocktails.Add(cocktail);
            cocktailIngredientsRemaining[cocktail] = new HashSet<IngredientIndex>();
            recetteTexts[cocktail] = new List<TextMeshProUGUI>();

            foreach (var prefab in cocktail.cocktailsImage)
            {
                if (prefab != null)
                {
                    var instance = Instantiate(prefab, spawnContainer);
                    spawnedCocktails.Add(instance);

                    var data = prefab.GetComponent<Cocktails>();
                    if (data != null)
                    {
                        foreach (var ingredient in data.cocktailIndices)
                            cocktailIngredientsRemaining[cocktail].Add(ingredient);

                        cocktailRecettes[cocktail] = new List<RecetteStep>(data.recette);

                        foreach (var step in cocktailRecettes[cocktail])
                        {
                            var textObj = Instantiate(recetteTextPrefab, recetteContainer);
                            var textMesh = textObj.GetComponent<TextMeshProUGUI>();
                            textMesh.text = step.description;
                            recetteTexts[cocktail].Add(textMesh);
                        }
                    }
                }
            }
        }

        StartTimer(clientTime);
    }

    private void StartTimer(float duration)
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
            timerSlider.fillRect.GetComponent<Image>().color = Color.white;
        }

        currentTime = duration;
        timerSlider.maxValue = duration;
        timerSlider.value = duration;
        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        bool blinkingStarted = false;

        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            if (currentTime < 0) currentTime = 0;

            timerSlider.value = currentTime;

            if (!blinkingStarted && currentTime <= clientTime / 4f)
            {
                blinkingStarted = true;
                blinkCoroutine = StartCoroutine(BlinkTimer());
            }

            yield return null;
        }

        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        timerSlider.fillRect.GetComponent<Image>().color = Color.white;
    }

    private IEnumerator BlinkTimer()
    {
        var img = timerSlider.fillRect.GetComponent<Image>();
        Color baseColor = Color.white;
        Color blinkColor = Color.red;

        while (true)
        {
            float blinkSpeed = Mathf.Lerp(0.5f, 0.05f, 1f - (currentTime / (clientTime / 4f)));

            img.color = blinkColor;
            yield return new WaitForSeconds(blinkSpeed);

            img.color = baseColor;
            yield return new WaitForSeconds(blinkSpeed);
        }
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

                if (cocktailRecettes.ContainsKey(cocktail))
                {
                    var steps = cocktailRecettes[cocktail];
                    for (int i = 0; i < steps.Count; i++)
                    {
                        if (!steps[i].isDone && steps[i].ingredientIndex == ingredient)
                        {
                            steps[i].isDone = true;
                            recetteTexts[cocktail][i].text = $"<color=green>{steps[i].description}</color>";
                            break;
                        }
                    }
                }

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
        cocktailRecettes.Remove(cocktail);
        recetteTexts.Remove(cocktail);

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
        text.color = Color.cyan;
    }

    void OnColors(InputValue value)
    {
        laitPressed = value.isPressed;
        UpdateSpeeds();
        TryValidateIngredient(IngredientIndex.Laitdemammouth, value);
    }

    void OnColors1(InputValue value)
    {
        alcoolPressed = value.isPressed;
        UpdateSpeeds();
        TryValidateIngredient(IngredientIndex.Alcooldefougere, value);
    }

    void OnColors2(InputValue value)
    {
        bavePressed = value.isPressed;
        UpdateSpeeds();
        TryValidateIngredient(IngredientIndex.Bavedeboeuf, value);
    }


    private void TryValidateIngredient(IngredientIndex ingredient, InputValue value)
    {
        if (!value.isPressed) return;
        
        switch (ingredient)
        {
            case IngredientIndex.Laitdemammouth:
                StartCoroutine(FillRoutine(ingredient, tireuseLaitSpeed, InputSystem.actions["Colors"]));
                break;
            case IngredientIndex.Alcooldefougere:
                StartCoroutine(FillRoutine(ingredient, tireuseAlcoolSpeed, InputSystem.actions["Colors1"]));
                break;
            case IngredientIndex.Bavedeboeuf:
                StartCoroutine(FillRoutine(ingredient, tireuseBaveSpeed, InputSystem.actions["Colors2"]));
                break;
        }
    }

    IEnumerator FillRoutine(IngredientIndex ingredient, float speed, InputAction action)
    {
        do
        {
            if (cup == null) yield break;
            if (cup.TotalAmount <= 0) yield return null;
            
            if (cup != null && cup.GetType().GetField("isLocked", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cup) is bool locked && locked)
            {
                yield break;
            }
            
            Debug.Log(action.name);
            cup.Fill(ingredient, speed * Time.deltaTime);
            yield return null;
        } while (action.inProgress);
        
        ValidateIngredient(ingredient);
        ControlerPoints.instance.ResetReward();
    }

    void OnNextClient(InputValue value)
    {
        if (value.isPressed) ShowNextClient();
    }

    public bool HasFinnished()
    {
        return remainingCocktails.Count == 0;
    }

    public void SendIngredient(IngredientIndex ingredient)
    {
        ValidateIngredient(ingredient);
        ControlerPoints.instance.ResetReward();
    }
    private void SetAllSpeeds(float speed)
    {
        tireuseLaitSpeed = speed;
        tireuseBaveSpeed = speed;
        tireuseAlcoolSpeed = speed;
    }
    private void ResetSpeeds()
    {
        tireuseLaitSpeed = baseTireuseLaitSpeed;
        tireuseBaveSpeed = baseTireuseBaveSpeed;
        tireuseAlcoolSpeed = baseTireuseAlcoolSpeed;
    }
    
    private bool laitPressed;
    private bool alcoolPressed;
    private bool bavePressed;
    
    private void UpdateSpeeds()
    {
        if (laitPressed) SetAllSpeeds(baseTireuseLaitSpeed);
        else if (alcoolPressed) SetAllSpeeds(baseTireuseAlcoolSpeed);
        else if (bavePressed) SetAllSpeeds(baseTireuseBaveSpeed);
        else ResetSpeeds(); // Aucun bouton pressé
    }
}
