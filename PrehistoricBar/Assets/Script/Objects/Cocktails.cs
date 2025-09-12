using System;
using System.Collections.Generic;

namespace Script.Objects
{
    using UnityEngine;
    
    public enum IngredientIndex
    {
        cocktail0,
        cocktail1,
        cocktail2,
    }


    public class Cocktails : MonoBehaviour
    {
        public List<IngredientIndex> cocktailIndices = new List<IngredientIndex>();     
        public string cocktailName;
    }
}