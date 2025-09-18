using Script.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace Script.Bar
{
    [System.Serializable]
    public class RecetteStep
    {
        public GameObject description;
        public IngredientIndex ingredientIndex;
        public float amount = 1f;
        public bool isDone;
    }
}