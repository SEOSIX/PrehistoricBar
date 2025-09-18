using Script.Bar;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Script.Objects
{
    public class Larve : MonoBehaviour
    {
        [SerializeField] private Slider larve;
        [SerializeField] private Cup cup;
        [SerializeField] private float transferAmount = 10f;
        [SerializeField] private float fillStep = 1f;

        private void Start()
        {
            larve.value = larve.minValue;
        }

        void OnGoon(InputValue _value)
        {
            if (_value.isPressed)
            {
                SounfManager.Singleton.PlaySound(9);
                larve.value += fillStep;
                larve.value = Mathf.Clamp(larve.value, larve.minValue, larve.maxValue);
            }
        }

        void OnTransfer(InputValue _value)
        {
            if (!_value.isPressed) return;
    
            if (larve.value >= larve.maxValue)
            {
                larve.value = larve.minValue;
                SounfManager.Singleton.PlaySound(10);
                cup.Fill(IngredientIndex.JusLarve, transferAmount);
                QueueUiManager.instance.ValidateIngredient(IngredientIndex.JusLarve);
            }
        }
    }
}