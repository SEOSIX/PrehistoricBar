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
                larve.value += fillStep;
                larve.value = Mathf.Clamp(larve.value, larve.minValue, larve.maxValue);
            }
        }

        void OnTransfer(InputValue _value)
        {
            if (larve.value >= larve.maxValue)
                TransferToCup();
            else
            {
                return;
            }
        }

        private void TransferToCup()
        {
            if (cup == null) return;
            larve.value = larve.minValue;
            cup.Fill(IngredientIndex.JusLarve, transferAmount);
        }
    }
}