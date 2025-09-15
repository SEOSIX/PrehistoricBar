using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Script.Objects
{
    public class Cup : MonoBehaviour
    {
        [Header("Score mult")]
        [SerializeField] private float scoreMult1;
        [SerializeField] private float scoreMult2;
        [SerializeField] private float scoreMult3;
        [SerializeField] private float scoreMult4;
        
        [Header("Sliders")]
        [SerializeField] private Slider sliderLait;
        [SerializeField] private Slider sliderBave;
        [SerializeField] private Slider sliderAlcool;
        [SerializeField] private Slider sliderJus;
        
        [HideInInspector] public float targetDosage;
        
        private bool isLocked = false;
        
        public Dictionary<IngredientIndex, float> content = new Dictionary<IngredientIndex, float>()
        {
            { IngredientIndex.Laitdemammouth, 0f},
            { IngredientIndex.Alcooldefougere, 0f},
            { IngredientIndex.Bavedeboeuf, 0f},
            { IngredientIndex.JusLarve, 0f }
        };

        public float TotalAmount
        {
            get
            {
                float fillAmount = 0f;
                foreach (var ingredient in content.Keys)
                {
                    fillAmount += content[ingredient];
                }
                
                return fillAmount;
            }
        }

        public void Fill(IngredientIndex ingredientType, float amount)
        {
            
            if (isLocked) return;
            
            switch (ingredientType)
            {
                case IngredientIndex.Laitdemammouth :
                    content[IngredientIndex.Laitdemammouth] += amount;
                    content[IngredientIndex.Alcooldefougere] += amount;
                    content[IngredientIndex.Bavedeboeuf] += amount;
                    break;
                
                case IngredientIndex.Bavedeboeuf :
                    content[IngredientIndex.Bavedeboeuf] += amount;
                    content[IngredientIndex.Alcooldefougere] += amount;
                    break;
                
                case IngredientIndex.Alcooldefougere :
                    content[IngredientIndex.Bavedeboeuf] += amount;
                    break;
            }
            
            content[ingredientType] += amount;

            SetSliders();
        }

        public void EmptyCup()
        {
            content = new Dictionary<IngredientIndex, float>()
            {
                { IngredientIndex.Laitdemammouth, 0f},
                { IngredientIndex.Alcooldefougere, 0f},
                { IngredientIndex.Bavedeboeuf, 0f},
                { IngredientIndex.JusLarve, 0f }
            };
            
            SetSliders();
        }

        public void SetTargetDosage(float amount)
        {
            targetDosage = amount + TotalAmount;
        }

        public float EvaluateScoreMult()
        {
            float targetDif = Mathf.Abs(TotalAmount - targetDosage);
            
            if (targetDif <= 5f)
                return scoreMult1;
            if (targetDif <= 10f)
                return scoreMult2;
            if (targetDif <= 15f)
                return scoreMult3;
            if (targetDif <= 20f)
                return scoreMult4;
            
            return 0f;
        }


        private void CheckFullCup()
        {
            if (sliderLait.value >= sliderLait.maxValue || sliderBave.value >= sliderBave.maxValue 
            || sliderAlcool.value >= sliderAlcool.maxValue || sliderJus.value >= sliderJus.maxValue)
            {
                BlockAll();
                isLocked = true;
            }
        }
        
        void BlockAll()
        {
            sliderLait.interactable = false;
            sliderBave.interactable = false;
            sliderAlcool.interactable = false;
            sliderJus.interactable = false;
        }
        private void SetSliders()
        {
            sliderLait.value = content[IngredientIndex.Laitdemammouth];
            sliderBave.value = content[IngredientIndex.Bavedeboeuf];
            sliderAlcool.value = content[IngredientIndex.Alcooldefougere];
            sliderJus.value = content[IngredientIndex.JusLarve];
            
            CheckFullCup();
        }
    }
}