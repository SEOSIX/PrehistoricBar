using Script.Objects;

namespace Script.Bar
{
    [System.Serializable]
    public class RecetteStep
    {
        public string description;
        public IngredientIndex ingredientIndex;
        public bool isDone;
    }
}