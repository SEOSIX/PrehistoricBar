using UnityEngine;

namespace Script.Bar
{
    public class Liquide : MonoBehaviour
    {
        public static Liquide singleton {get; private set;}
        [SerializeField] private GameObject[] liquidesToActivate;


        void Awake()
        {
            singleton = this;
        }
        public void AcitaveObject(int index, bool activate)
        {
            liquidesToActivate[index].SetActive(activate);
        }
    }
}