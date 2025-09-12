using UnityEngine;
using UnityEngine.InputSystem;
using Script.Bar;
using Script.Objects;

namespace Script.Objects
{
    public class Mortier : MonoBehaviour
    {
        private IngredientIndex? selectedIngredient = null;

        private void SelectIngredient(IngredientIndex ingredient)
        {
            selectedIngredient = ingredient;
            Debug.Log($"[Mortier] Ingrédient sélectionné: {ingredient}");
        }
        
        void OnBababe(InputValue value)  
        { if (value.isPressed) SelectIngredient(IngredientIndex.Bababe); }

        void OnFroz(InputValue value)  
        { if (value.isPressed) SelectIngredient(IngredientIndex.Froz); }

        void OnIce(InputValue value)  
        { if (value.isPressed) SelectIngredient(IngredientIndex.Glacon); }

        void OnKitronVert(InputValue value)  
        { if (value.isPressed) SelectIngredient(IngredientIndex.Kitron); }

        void OnFly(InputValue value)  
        { if (value.isPressed) SelectIngredient(IngredientIndex.Mouche); }

        void OnCacaoLaid(InputValue value)
        { if (value.isPressed) SelectIngredient(IngredientIndex.Cacao); }
        

        void OnQassos(InputValue value)  
        { if (value.isPressed) SelectIngredient(IngredientIndex.Qassos); }

        void OnGrind(InputValue value)
        {
            if (!value.isPressed) return;

            if (selectedIngredient.HasValue && value.isPressed)
            {
                QueueUiManager.instance.SendIngredient(selectedIngredient.Value);
                Debug.Log($"[Mortier] Ingrédient {selectedIngredient.Value} broyé !");
                selectedIngredient = null;
            }
            else
            {
                Debug.Log("[Mortier] Aucun ingrédient sélectionné !");
            }
        }
    }
}