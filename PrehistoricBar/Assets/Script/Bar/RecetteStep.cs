using Script.Objects;

namespace Script.Bar
{
    [System.Serializable]
    public class RecetteStep
    {
        public string description;
        public IngredientIndex ingredientIndex;
        public float amount = 1f;
        public bool isDone;
    }
}