using Script.Objects;
using UnityEngine.UI;

namespace Script.Bar
{
    [System.Serializable]
    public class RecetteStep
    {
        public Image description;
        public IngredientIndex ingredientIndex;
        public float amount = 1f;
        public bool isDone;
    }
}