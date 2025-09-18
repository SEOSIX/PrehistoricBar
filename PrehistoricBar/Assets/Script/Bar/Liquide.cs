using UnityEngine;

namespace Script.Bar
{
    public class Liquide : MonoBehaviour
    {
        [SerializeField] private GameObject[] liquidesToActivate;


        public void AcitaveObject(int index, bool activate)
        {
            liquidesToActivate[index].SetActive(activate);
        }
    }
}