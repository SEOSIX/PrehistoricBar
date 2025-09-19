using System;
using System.Collections.Generic;
using System.Linq;
using Script.Bar;
using UnityEngine;
using UnityEngine.UI;

namespace Script.Objects
{
    public class Cup : MonoBehaviour
    {
        public static Cup instance { get; private set; }
        
        [Header("Score Mult")]
        [SerializeField] private float scoreMult1;
        [SerializeField] private float scoreMult2;
        [SerializeField] private float scoreMult3;
        [SerializeField] private float scoreMult4;

        [Header("UI")]
        public Slider cupSlider; 
		public Slider targetSlider;
        [SerializeField] private Image fillImage;
        [HideInInspector]
        public bool IsAchieved = false;

        [Header("Colors")]
        [SerializeField] private Color laitColor;
        [SerializeField] private Color baveColor;
        [SerializeField] private Color alcoolColor;
        [SerializeField] private Color jusColor;

        [HideInInspector] public float targetDosage;
        private bool isLocked = false;

        [Header("Sliding Points")]
        [SerializeField] private RectTransform sendPoint;
        [SerializeField] private RectTransform resetPoint;
        [SerializeField] private float moveSpeed = 800f;
        
        private Coroutine currentSlideCoroutine;

        public Dictionary<IngredientIndex, float> content = new Dictionary<IngredientIndex, float>()
        {
            { IngredientIndex.Laitdemammouth, 0f },
            { IngredientIndex.Alcooldefougere, 0f },
            { IngredientIndex.Bavedeboeuf, 0f },
            { IngredientIndex.JusLarve, 0f }
        };

        public float TotalAmount
        {
            get
            {
                float total = 0f;
                foreach (var val in content.Values)
                    total += val;
                return total;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        #region Filling

        public void Fill(IngredientIndex ingredientType, float amount)
        {
            if (isLocked) return;

            content[ingredientType] += amount;

            UpdateUI();
        }

        public void EmptyCup()
        {
            foreach (var key in content.Keys.ToList())
                content[key] = 0f;

            isLocked = false;
            UpdateUI();
        }

        public void ResetCup()
        {
            foreach (var key in content.Keys.ToList())
                content[key] = 0f;

            isLocked = false;
            cupSlider.value = 0f;
            fillImage.color = Color.clear;
            if (resetPoint != null)
                GetComponent<RectTransform>().anchoredPosition = resetPoint.anchoredPosition;
        }

        public void SetTargetDosage(IngredientIndex ingredient)
        {
            if (EventQueueManager.GetCurrentStep() != null)
            {
                if (EventQueueManager.GetCurrentStep().ingredientIndex == ingredient)
                {
                    float amount = EventQueueManager.GetCurrentStep().amount;
                    targetDosage = amount + TotalAmount;
					targetSlider.value = cupSlider.value + targetDosage;
                    Debug.Log($"Dosage recommendé{targetDosage}");
                     
                    return;
                }
                else
                {
                    Debug.LogWarning("Tireuse : Mauvais liquide sélectionné");
                }
            }
            else
            {
                Debug.LogWarning("Tireuse : Aucune étape de recette");
            }
            
            Debug.Log("Tireuse : Dosage invalide");
            targetDosage = 0f;
        }

        public float EvaluateScoreMult()
        {
            float targetDif = Mathf.Abs(TotalAmount - targetDosage);

            if (targetDif <= 5f) return scoreMult1;
            if (targetDif <= 10f) return scoreMult2;
            if (targetDif <= 15f) return scoreMult3;
            if (targetDif <= 20f) return scoreMult4;

            return 0f;
        }

        #endregion

        #region UI Update

        private void UpdateUI()
        {
            cupSlider.value = TotalAmount;

            fillImage.color = MixColors();
            if (cupSlider.value >= cupSlider.maxValue)
                isLocked = true;
        }

        private Color MixColors()
        {
            float total = TotalAmount;
            if (total <= 0f) return Color.clear;

            Color result = Color.black;

            result += laitColor   * (content[IngredientIndex.Laitdemammouth] / total);
            result += baveColor   * (content[IngredientIndex.Bavedeboeuf] / total);
            result += alcoolColor * (content[IngredientIndex.Alcooldefougere] / total);
            result += jusColor    * (content[IngredientIndex.JusLarve] / total);

            return result;
        }

        #endregion

        #region Sliding (Cup Animation)

        public void SlideToSendPoint(Action onComplete = null)
        {
            if (sendPoint == null) return;
            SlideTo(sendPoint, onComplete);
        }

        public void SlideToResetPoint(Action onComplete = null)
        {
            if (resetPoint == null) return;
            SlideTo(resetPoint, onComplete);
        }

        private void SlideTo(RectTransform target, Action onComplete = null)
        {
            if (currentSlideCoroutine != null)
                StopCoroutine(currentSlideCoroutine);

            currentSlideCoroutine = StartCoroutine(SlideRoutine(target, onComplete));
        }

        private System.Collections.IEnumerator SlideRoutine(RectTransform target, Action onComplete)
        {
            RectTransform cupRect = GetComponent<RectTransform>();
            Vector2 startPos = cupRect.anchoredPosition;
            Vector2 endPos = target.anchoredPosition;

            float distance = Vector2.Distance(startPos, endPos);
            float duration = distance / moveSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                cupRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            cupRect.anchoredPosition = endPos;
            onComplete?.Invoke();
            currentSlideCoroutine = null;
        }

        #endregion
    }
}
