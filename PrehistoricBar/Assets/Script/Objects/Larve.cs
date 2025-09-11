using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Script.Objects
{
    public class Larve : MonoBehaviour
    {
        [SerializeField] private Slider larve;

        private void Start()
        {
            larve.value = larve.minValue;
        }

        void OnGoon(InputValue _value)
        {
            if (_value.isPressed)
            {
                larve.value ++;
            }
        }
    }
}