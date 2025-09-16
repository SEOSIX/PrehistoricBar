using UnityEngine;
using System.Collections.Generic;
using Script.Bar;

[System.Serializable]
public class ServiceData
{
    public string name;
    public List<ClientClass> cocktails = new List<ClientClass>(2);
}

