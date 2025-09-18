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
    public GameObject Over;
    
    private ClientAnimManager clientAnimManager;

    [Header("Timer")]
    public Slider timerSlider;
    public float clientTime = 10f;
    private float currentTime;
    private Coroutine timerCoroutine;
    private Coroutine blinkCoroutine;
    
    [Header("Transition entre services")]
    [SerializeField] private GameObject transitionPanel;
    [SerializeField] private float transitionDuration = 2f;
    
    
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
    [SerializeField] private Transform mortierPos;
    [SerializeField] private Transform PoubellePos;
    
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
    [SerializeField] private GameObject recetteImagePrefab;
    
    [Header("Preview des prochains clients")]
    [SerializeField] private Transform nextClientSlot1;
    [SerializeField] private Transform nextClientSlot2;

    private GameObject nextClient1UI;
    private GameObject nextClient2UI;
    

    private Dictionary<ClientClass, List<Image>> recetteTexts = new();
    
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
    
    private void UpdateNextClientsPreview()
    {
        if (nextClient1UI != null) Destroy(nextClient1UI);
        if (nextClient2UI != null) Destroy(nextClient2UI);
        var nextServices = EventQueueManager.instance.PeakNextClient(2);

        if (nextServices.Count > 0)
        {
            var service1 = nextServices[0];
            if (service1.clients.Count > 0)
            {
                var firstCocktail = service1.clients.Peek().cocktailsImage.Peek();
                nextClient1UI = Instantiate(firstCocktail, nextClientSlot1);
                nextClient1UI.transform.localScale = Vector3.one * 0.5f;
            }
        }
        if (nextServices.Count > 1)
        {
            var service2 = nextServices[1];
            if (service2.clients.Count > 0)
            {
                var firstCocktail = service2.clients.Peek().cocktailsImage.Peek();
                nextClient2UI = Instantiate(firstCocktail, nextClientSlot2);
                nextClient2UI.transform.localScale = Vector3.one * 0.5f;
            }
        }
    }
    
    public void ShowNextClient()
    {
        if (clientAnimManager != null) clientAnimManager.LeaveBar();
        
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
            timerSlider.value = clientTime;
            timerSlider.fillRect.GetComponent<Image>().color = Color.white;
            return;
        }
        
        GameObject newClient = Instantiate(currentClient.prefab, new Vector3(-12f, -5f, 0f), transform.rotation);
        clientAnimManager = newClient.GetComponent<ClientAnimManager>();
        
        foreach (var cocktail in currentClient.clients)
        {
            remainingCocktails.Add(cocktail);
            cocktailIngredientsRemaining[cocktail] = new HashSet<IngredientIndex>();
            recetteTexts[cocktail] = new List<Image>();
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
                                isDone = false,
                                amount = step.amount
                            };
                            cocktailRecettes[cocktail].Add(newStep);

                            var textObj = Instantiate(recetteImagePrefab, recetteContainer);
                            var textMesh = textObj.GetComponent<Image>();
                            textMesh.sprite = newStep.description.sprite;
                            recetteTexts[cocktail].Add(textMesh);
                        }
                    }
                }
            }
            NextStep(cocktail);
        }
        StartTimer(clientTime);
        UpdateNextClientsPreview();
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
        {
            recetteTexts[cocktail][idx].color = Color.gray;
        }

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
        if (!value.isPressed) return;
        if (!IsCupAtPosition(laitPos)) return;  
        if (!laitLocked)
        {
            laitLocked = true;
            SounfManager.Singleton.PlaySound(5, true);
            Liquide.singleton.AcitaveObject(2, true);
            Cup.instance.SetTargetDosage(IngredientIndex.Laitdemammouth);
            
            StartCoroutine(FillRoutine(IngredientIndex.Laitdemammouth, tireuseLaitSpeed, 
                InputSystem.actions["Colors"]));
        }
    }

    void OnColorsR(InputValue value)
    {
        Liquide.singleton.AcitaveObject(2, false);
        SounfManager.Singleton.StopSound(5);
        ValidateIngredient(IngredientIndex.Laitdemammouth);
    }

    void OnColors1(InputValue value)
    {
        if (!value.isPressed) return;
        if (!IsCupAtPosition(alcoolPos)) return;  
        if (!alcoolLocked)
        {
            alcoolLocked = true;
            SounfManager.Singleton.PlaySound(5, true);
            Liquide.singleton.AcitaveObject(0, true);
            Cup.instance.SetTargetDosage(IngredientIndex.Alcooldefougere);
            
            StartCoroutine(FillRoutine(IngredientIndex.Alcooldefougere, tireuseAlcoolSpeed, 
                InputSystem.actions["Colors1"]));
        }
    }

    void OnColors1R(InputValue value)
    {
        Liquide.singleton.AcitaveObject(0, false);
        SounfManager.Singleton.StopSound(5);
        ValidateIngredient(IngredientIndex.Alcooldefougere);
    }

    void OnColors2(InputValue value)
    {
        if (!value.isPressed) return;
        if (!IsCupAtPosition(bavePos)) return;  
        
        if (!baveLocked)
        {
            baveLocked = true;
            SounfManager.Singleton.PlaySound(5, true);
            Liquide.singleton.AcitaveObject(1, true);
            Cup.instance.SetTargetDosage(IngredientIndex.Bavedeboeuf);
            
            StartCoroutine(FillRoutine(IngredientIndex.Bavedeboeuf, tireuseBaveSpeed, 
                InputSystem.actions["Colors2"]));
        }
    }

    void OnColors2R(InputValue value)
    {
        Liquide.singleton.AcitaveObject(1, false);
        SounfManager.Singleton.StopSound(5);
        ValidateIngredient(IngredientIndex.Bavedeboeuf);
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
        float amountBefore = cup.content[ingredient];

        while (action.inProgress)
        {
            if (cup == null) yield break;
            if (cup.TotalAmount <= 0) yield return null;
            if (cup != null && cup.GetType()
                    .GetField("isLocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(cup) is bool locked && locked)
            {
                yield break;
            }
            cup.Fill(ingredient, speed * Time.deltaTime);
            yield return null;
        }
        float amountAfter = cup.content[ingredient];
        if (amountAfter > amountBefore)
        {
            ValidateIngredient(ingredient);
        }
        else
        {
            Debug.LogWarning($"Aucun liquide versé pour {ingredient}, donc pas de validation !");
        }
    }
    
    private bool IsCupAtPosition(Transform target)
    {
        RectTransform cupRect = cup.GetComponent<RectTransform>();
        RectTransform targetRect = target.GetComponent<RectTransform>();
        Vector2 targetPos = targetRect.anchoredPosition + offset;
        return Vector2.Distance(cupRect.anchoredPosition, targetPos) < 0.1f;
    }


    void OnNextClient(InputValue value)
    {
        if (!value.isPressed) return;

        if (currentClient != null)
        {
            foreach (var cocktail in remainingCocktails)
            {
                if (HasIncorrectIngredients(cocktail))
                {
                    Debug.LogWarning("Vous avez mis des ingrédients incorrects pour ce cocktail !");
                    if (clientAnimManager != null)
                    {
                        clientAnimManager.ServeCocktail(false);
                    }
                    ShowNextClient();
                    return;
                }
                else
                {
                    ShowNextClient();
                }
            }
        }
        if (queueManager.HasMoreWaves())
        {
            ShowNextClient();
        }
        else
        {
            ResetGame();
        }
    }
    
    private void ResetGame()
    {
        ControlerPoints.instance.ResetReward();
        // ControlerPoints.instance.ResetLife();
        // ControlerPoints.instance.ResetPoints();

        Debug.Log("Game Reset");
        ShowNextClient();
    }
    
    private bool HasIncorrectIngredients(ClientClass cocktail)
    {
        if (!cocktailIngredientsRemaining.ContainsKey(cocktail)) return false;

        var allIngredients = new HashSet<IngredientIndex>();
        if (cocktailRecettes.ContainsKey(cocktail))
        {
            foreach (var step in cocktailRecettes[cocktail])
            {
                allIngredients.Add(step.ingredientIndex);
            }
        }
        
        foreach (var ingredient in Enum.GetValues(typeof(IngredientIndex)))
        {
            var ing = (IngredientIndex)ingredient;
            if (!allIngredients.Contains(ing) && !cocktailIngredientsRemaining[cocktail].Contains(ing))
            {
                return true; 
            }
        }

        return false;
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
    
    
    private bool laitPressed;
    private bool alcoolPressed;
    private bool bavePressed;

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
            cupRect.anchoredPosition = Vector2.MoveTowards(
                cupRect.anchoredPosition, targetPos, moveSpeed * Time.deltaTime);
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
            recetteTexts[cocktail] = new List<Image>();
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
                    var textObj = Instantiate(recetteImagePrefab, recetteContainer);
                    var textMesh = textObj.GetComponent<Image>();
                    textMesh.sprite = newStep.description.sprite;
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
    }
    
    private IEnumerator ShowTransitionThenNextClient()
    {
        transitionPanel.SetActive(true);
        yield return new WaitForSeconds(transitionDuration);
        transitionPanel.SetActive(false);
        ShowNextClient();
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
