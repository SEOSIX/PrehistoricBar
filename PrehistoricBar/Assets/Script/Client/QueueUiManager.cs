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
    
    public static QueueUiManager instance {get; private set;}
    [SerializeField] private EventQueueManager queueManager;
    [SerializeField] private Transform spawnContainer;
    

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
    
    [Header("Positions des ingrédients")]
    [SerializeField] private Transform laitPos;
    [SerializeField] private Transform bavePos;
    [SerializeField] private Transform alcoolPos;
    
    [SerializeField] private float moveSpeed = 5f;
    
    private float baseTireuseLaitSpeed;
    private float baseTireuseBaveSpeed;
    private float baseTireuseAlcoolSpeed;
    

    private ClientData currentClient;
    private List<GameObject> spawnedCocktails = new List<GameObject>();
    private List<CocktailClass> remainingCocktails = new List<CocktailClass>();

    private Dictionary<CocktailClass, HashSet<IngredientIndex>> cocktailIngredientsRemaining = new();

    void Awake()
    {
        instance = this;
    }
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

    void OnColors(InputValue value)
    {
        TryValidateIngredient(IngredientIndex.Laitdemammouth, value);
    }
    void OnColors1(InputValue value)  { TryValidateIngredient(IngredientIndex.Alcooldefougere, value); }
    void OnColors2(InputValue value)  { TryValidateIngredient(IngredientIndex.Bavedeboeuf, value); }

    private void TryValidateIngredient(IngredientIndex ingredient, InputValue value)
    {
        if (!value.isPressed) return;

        Transform targetPos = ingredient switch
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
            Debug.Log(action.name);
            cup.Fill(ingredient, speed * Time.deltaTime);
            yield return null;
        } while (action.inProgress);
        
        ValidateIngredient(ingredient);
        //pour reset mais ca sera a mettre quand on appel le prochain client
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
