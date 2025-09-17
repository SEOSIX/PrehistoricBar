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
    public float clientTime = 10f;
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
    [SerializeField] private Transform JusLarvePos;
    
    [SerializeField] private float moveSpeed = 5f;
    
    private float baseTireuseLaitSpeed;
    private float baseTireuseBaveSpeed;
    private float baseTireuseAlcoolSpeed;
    
    private ServiceData currentClient;
    private List<GameObject> spawnedCocktails = new List<GameObject>();
    private List<ClientClass> remainingCocktails = new List<ClientClass>();

    private Dictionary<ClientClass, HashSet<IngredientIndex>> cocktailIngredientsRemaining = new();
    private Dictionary<ClientClass, List<RecetteStep>> cocktailRecettes = new();
    private Dictionary<ClientClass, int> cocktailStepIndices = new();

    [Header("UI Recette")]
    [SerializeField] private Transform recetteContainer;
    [SerializeField] private GameObject recetteTextPrefab;

    private Dictionary<ClientClass, List<TextMeshProUGUI>> recetteTexts = new();
    
    [HideInInspector] public bool laitLocked = false;
    [HideInInspector] public bool alcoolLocked = false;
    [HideInInspector] private bool baveLocked = false;

    void Awake()
    {
        instance = this;
        baseTireuseLaitSpeed = tireuseLaitSpeed;
        baseTireuseBaveSpeed = tireuseBaveSpeed;
        baseTireuseAlcoolSpeed = tireuseAlcoolSpeed;
    }
    
    public void ShowNextClient()
    {
        //ControlerPoints.instance.CheckForWin(20);
        ControlerPoints.GetScore(currentTime);
        Over.SetActive(false);
        currentClient = queueManager.GetNextService();

        foreach (Transform child in spawnContainer) Destroy(child.gameObject);
        foreach (Transform child in recetteContainer) Destroy(child.gameObject);

        spawnedCocktails.Clear();
        remainingCocktails.Clear();
        cocktailIngredientsRemaining.Clear();
        cocktailRecettes.Clear();
        recetteTexts.Clear();
        cocktailStepIndices.Clear();
        
        Cup.instance.ResetCup();
        Cup.instance.SlideToResetPoint();
        
        laitLocked = false;
        alcoolLocked = false;
        baveLocked = false;

        if (currentClient == null)
        {
            StopAllCoroutines();
            timerCoroutine = null;
            blinkCoroutine = null;
            Over.SetActive(true);
            timerSlider.value = 0;
            timerSlider.fillRect.GetComponent<Image>().color = Color.white;
            return;
        }

        foreach (var cocktail in currentClient.clients)
        {
            remainingCocktails.Add(cocktail);
            cocktailIngredientsRemaining[cocktail] = new HashSet<IngredientIndex>();
            recetteTexts[cocktail] = new List<TextMeshProUGUI>();
            cocktailStepIndices[cocktail] = 0;

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

                        cocktailRecettes[cocktail] = new List<RecetteStep>();
                        foreach (var step in data.recette)
                        {
                            var newStep = new RecetteStep
                            {
                                ingredientIndex = step.ingredientIndex,
                                description = step.description,
                                isDone = false
                            };
                            cocktailRecettes[cocktail].Add(newStep);

                            var textObj = Instantiate(recetteTextPrefab, recetteContainer);
                            var textMesh = textObj.GetComponent<TextMeshProUGUI>();
                            textMesh.text = newStep.description;
                            recetteTexts[cocktail].Add(textMesh);
                        }
                    }
                }
            }

            NextStep(cocktail);
        }
        StartTimer(clientTime);
    }

    private void StartTimer(float duration)
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
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

    public void ValidateIngredient(IngredientIndex ingredient)
    {
        if (currentClient == null) return;

        foreach (var cocktail in remainingCocktails.ToArray())
        {
            if (!cocktailRecettes.ContainsKey(cocktail)) continue;
            var steps = cocktailRecettes[cocktail];
            int currentStep = cocktailStepIndices[cocktail];
            if (currentStep < steps.Count && steps[currentStep].ingredientIndex == ingredient)
            {
                ValidateStep(cocktail, steps[currentStep]);
                return;
            }
        }
        Debug.Log($"Ingrédient {ingredient} incorrect ou déjà utilisé !");
    }

    public void ValidateStep(ClientClass cocktail, RecetteStep step)
    {
        if (!cocktailRecettes.ContainsKey(cocktail)) return;
        var steps = cocktailRecettes[cocktail];
        int idx = steps.IndexOf(step);
        if (idx == -1) return;
        if (!recetteTexts.ContainsKey(cocktail)) return;
        steps[idx].isDone = true;
        if (recetteTexts[cocktail].Count > idx)
            recetteTexts[cocktail][idx].text = $"<color=green>{steps[idx].description}</color>";
        cocktailStepIndices[cocktail]++;
        if (cocktailStepIndices[cocktail] >= steps.Count)
            ValidateCocktail(cocktail);
        else
            NextStep(cocktail);
    }

    private void ValidateCocktail(ClientClass cocktail)
    {
        if (cocktail != null) ShowDoneText(cocktail);
        remainingCocktails.Remove(cocktail);
        cocktailIngredientsRemaining.Remove(cocktail);
        cocktailRecettes.Remove(cocktail);
        recetteTexts.Remove(cocktail);
        cocktailStepIndices.Remove(cocktail);
        if (remainingCocktails.Count == 0) Debug.Log("Tous les cocktails du client sont servis !");
        ControlerPoints.instance.ResetReward();
    }

    private void ShowDoneText(ClientClass cocktail)
    {
        if (cocktail.cocktailsImage.Count == 0) return;
        var targetImage = spawnedCocktails.Find(x => x.name.Contains(cocktail.cocktailsImage.Peek().name));
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
        if (laitLocked) return;  
        laitPressed = value.isPressed;
        UpdateSpeeds();
        TryValidateIngredient(IngredientIndex.Laitdemammouth, value);
        if (value.isPressed) laitLocked = true; 
    }

    void OnColors1(InputValue value)
    {
        if (alcoolLocked) return;
        alcoolPressed = value.isPressed;
        UpdateSpeeds();
        TryValidateIngredient(IngredientIndex.Alcooldefougere, value);
        if (value.isPressed) alcoolLocked = true;
    }

    void OnColors2(InputValue value)
    {
        if (baveLocked) return;
        bavePressed = value.isPressed;
        UpdateSpeeds();
        TryValidateIngredient(IngredientIndex.Bavedeboeuf, value);
        if (value.isPressed) baveLocked = true;
    }

    public void TryValidateIngredient(IngredientIndex ingredient, InputValue value)
    {
        if (!value.isPressed) return;
        Cup.instance.SetTargetDosage(ingredient);
        Transform targetPos = ingredient switch
        {
            IngredientIndex.Laitdemammouth => laitPos,
            IngredientIndex.Bavedeboeuf => bavePos,
            IngredientIndex.Alcooldefougere => alcoolPos,
            IngredientIndex.JusLarve => JusLarvePos,
            _ => null
        };
        if (targetPos != null) StartMove(ingredient, targetPos);
    }

    IEnumerator FillRoutine(IngredientIndex ingredient, float speed, InputAction action)
    {
        do
        {
            if (cup == null) yield break;
            if (cup.TotalAmount <= 0) yield return null;
            if (cup != null && cup.GetType().GetField("isLocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cup) is bool locked && locked)
            {
                yield break;
            }
            cup.Fill(ingredient, speed * Time.deltaTime);
            yield return null;
        }
        while (action.inProgress);
        ValidateIngredient(ingredient);
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
        else ResetSpeeds(); 
    }

    [SerializeField] private Vector2 offset = new Vector2(0, -350f);
    private Coroutine currentMoveCoroutine;

    private void StartMove(IngredientIndex ingredient, Transform target)
    {
        if (currentMoveCoroutine != null) StopCoroutine(currentMoveCoroutine);
        currentMoveCoroutine = StartCoroutine(MoveAndFill(ingredient, target));
    }

    private IEnumerator MoveCupTo(Transform target)
    {
        RectTransform cupRect = cup.GetComponent<RectTransform>();
        RectTransform targetRect = target.GetComponent<RectTransform>();
        Vector2 targetPos = targetRect.anchoredPosition + offset;
        while (Vector2.Distance(cupRect.anchoredPosition, targetPos) > 0.1f)
        {
            cupRect.anchoredPosition = Vector2.MoveTowards(cupRect.anchoredPosition, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        cupRect.anchoredPosition = targetPos;
        currentMoveCoroutine = null;
    }

    private IEnumerator MoveAndFill(IngredientIndex ingredient, Transform target)
    {
        yield return StartCoroutine(MoveCupTo(target));
        float speed = ingredient switch
        {
            IngredientIndex.Laitdemammouth => tireuseLaitSpeed,
            IngredientIndex.Bavedeboeuf => tireuseBaveSpeed,
            IngredientIndex.Alcooldefougere => tireuseAlcoolSpeed,
            _ => 0f
        };
        InputAction action = ingredient switch
        {
            IngredientIndex.Laitdemammouth => InputSystem.actions["Colors"],
            IngredientIndex.Bavedeboeuf => InputSystem.actions["Colors2"],
            IngredientIndex.Alcooldefougere => InputSystem.actions["Colors1"],
            _ => null
        };
        if (action != null) yield return StartCoroutine(FillRoutine(ingredient, speed, action));
    }

    public void SendCup()
    {
        Cup.instance.SlideToSendPoint(() => { Cup.instance.ResetCup(); });
    }

    public void RestartCurrentCocktail()
    {
        if (currentClient == null) return;
        foreach (var go in spawnedCocktails.ToArray())
        {
            if (go == null) continue;
            var done = go.transform.Find("DoneText");
            if (done != null) Destroy(done.gameObject);
        }
        foreach (Transform child in recetteContainer) Destroy(child.gameObject);

        remainingCocktails.Clear();
        cocktailIngredientsRemaining.Clear();
        cocktailRecettes.Clear();
        recetteTexts.Clear();
        cocktailStepIndices.Clear();

        foreach (var cocktail in currentClient.clients)
        {
            remainingCocktails.Add(cocktail);
            var set = new HashSet<IngredientIndex>();
            cocktailIngredientsRemaining[cocktail] = set;
            foreach (var prefab in cocktail.cocktailsImage)
            {
                if (prefab == null) continue;
                var data = prefab.GetComponent<Script.Objects.Cocktails>();
                if (data != null)
                {
                    foreach (var ing in data.cocktailIndices) set.Add(ing);
                }
            }
            cocktailRecettes[cocktail] = new List<RecetteStep>();
            recetteTexts[cocktail] = new List<TextMeshProUGUI>();
            cocktailStepIndices[cocktail] = 0;
            Script.Objects.Cocktails sourceData = null;
            foreach (var prefab in cocktail.cocktailsImage)
            {
                if (prefab == null) continue;
                sourceData = prefab.GetComponent<Script.Objects.Cocktails>();
                if (sourceData != null) break;
            }
            if (sourceData != null)
            {
                foreach (var step in sourceData.recette)
                {
                    var newStep = new RecetteStep
                    {
                        ingredientIndex = step.ingredientIndex,
                        description = step.description,
                        isDone = false
                    };
                    cocktailRecettes[cocktail].Add(newStep);
                    var textObj = Instantiate(recetteTextPrefab, recetteContainer);
                    var textMesh = textObj.GetComponent<TextMeshProUGUI>();
                    textMesh.text = newStep.description;
                    recetteTexts[cocktail].Add(textMesh);
                }
            }

            NextStep(cocktail);
        }
        Cup.instance.ResetCup();
        Cup.instance.SlideToResetPoint();
        laitLocked = false;
        alcoolLocked = false;
        baveLocked = false;
        
        ControlerPoints.ResetScore();
    }

    public void NextStep(ClientClass cocktail)
    {
        if (!cocktailRecettes.ContainsKey(cocktail)) return;
        var steps = cocktailRecettes[cocktail];
        int idx = cocktailStepIndices[cocktail];
        if (idx >= steps.Count) return;
        var step = steps[idx];
        if (IsMortierIngredient(step.ingredientIndex))
        {
            var mortier = FindObjectOfType<Mortier>();
            if (mortier != null) mortier.SetStep(step, cocktail);
        }
    }

    private bool IsMortierIngredient(IngredientIndex index)
    {
        return index == IngredientIndex.Bababe
            || index == IngredientIndex.Froz
            || index == IngredientIndex.Glacon
            || index == IngredientIndex.Kitron
            || index == IngredientIndex.Mouche
            || index == IngredientIndex.Cacao
            || index == IngredientIndex.Qassos;
    }
}
