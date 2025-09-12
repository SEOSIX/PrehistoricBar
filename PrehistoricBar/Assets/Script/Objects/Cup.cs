using System.Collections.Generic;
using UnityEngine;

namespace Script.Objects
{
    public class Cup : MonoBehaviour
    {
        public Dictionary<IngredientIndex, float> content = new Dictionary<IngredientIndex, float>()
        {
            { IngredientIndex.Laitdemammouth, 0f},
            { IngredientIndex.Alcooldefougere, 0f},
            { IngredientIndex.Bavedeboeuf, 0f},
        };

        public void Fill(IngredientIndex ingredientType, float amount)
        {
            content[ingredientType] += amount;
        }

        public void EmptyCup()
        {
            content = new Dictionary<IngredientIndex, float>()
            {
                { IngredientIndex.Laitdemammouth, 0f},
                { IngredientIndex.Alcooldefougere, 0f},
                { IngredientIndex.Bavedeboeuf, 0f},
            };
        }
    }
}