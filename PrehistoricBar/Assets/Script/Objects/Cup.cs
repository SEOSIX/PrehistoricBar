using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Script.Objects
{
    public class Cup : MonoBehaviour
    {
        [Header("Score Mult")]
        [SerializeField] private float scoreMult1;
        [SerializeField] private float scoreMult2;
        [SerializeField] private float scoreMult3;
        [SerializeField] private float scoreMult4;

        [Header("UI")]
        [SerializeField] private Slider cupSlider; 
        [SerializeField] private Image fillImage;

        [Header("Colors")]
        [SerializeField] private Color laitColor;
        [SerializeField] private Color baveColor;
        [SerializeField] private Color alcoolColor;
        [SerializeField] private Color jusColor;

        [HideInInspector] public float targetDosage;
        private bool isLocked = false;

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
        public void Fill(IngredientIndex ingredientType, float amount)
        {
            if (isLocked) return;

            content[ingredientType] += amount;

            UpdateUI();
        }
        public void EmptyCup()
        {
            foreach (var key in content.Keys)
                content[key] = 0f;

            isLocked = false;
            UpdateUI();
        }
        
        public void SetTargetDosage(float amount)
        {
            targetDosage = amount + TotalAmount;
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
    }
}
