using UnityEngine;
using System.Collections.Generic;
using Script.Bar;

[System.Serializable]
public class ServiceData
{
    public string name;
    public Queue<ClientClass> clients = new Queue<ClientClass>(2);
}

