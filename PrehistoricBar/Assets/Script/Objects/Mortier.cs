using Script.Bar;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

namespace Script.Objects
{
    public class Mortier : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI currentIngredientText;
        private RecetteStep currentStep;
        private ClientClass currentCocktail;
        
        private int grindPressCount = 0;
        private int requiredPresses = 0;

        public void SetStep(RecetteStep step, ClientClass cocktail)
        {
            if (step == null || cocktail == null) return;
            currentStep = step;
            currentCocktail = cocktail;
            grindPressCount = 0;
            requiredPresses = Mathf.Max(1, Mathf.RoundToInt(step.amount));
            if (currentIngredientText != null)
                currentIngredientText.text = $"Broyer: {step.ingredientIndex}";
        }

        void OnGrind(InputValue value)
        {
            if (!value.isPressed) return;
            if (currentStep == null || currentCocktail == null) return;

            grindPressCount++;
            if (currentIngredientText != null)
                currentIngredientText.text = $"Broyer: {currentStep.ingredientIndex} ({grindPressCount}/{requiredPresses})";

            if (grindPressCount >= requiredPresses)
            {
                QueueUiManager.instance.ValidateStep(currentCocktail, currentStep); 
                currentStep = null;
                currentCocktail = null;
                grindPressCount = 0;
                requiredPresses = 0;
                if (currentIngredientText != null) currentIngredientText.text = "";
            }
        }
    }
}