using System;
using System.Collections.Generic;

namespace Script.Objects
{
    using UnityEngine;
    
    public enum IngredientIndex
    {
        Laitdemammouth,
        Alcooldefougere,
        Bavedeboeuf
    }


    public class Cocktails : MonoBehaviour
    {
        public List<IngredientIndex> cocktailIndices = new List<IngredientIndex>();     
        public string cocktailName;
    }
}