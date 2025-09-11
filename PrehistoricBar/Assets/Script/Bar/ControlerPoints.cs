using System;
using TMPro;
using UnityEngine;

namespace Script.Bar
{
    public class ControlerPoints : MonoBehaviour
    {
        public static ControlerPoints instance {get; private set;}
        [SerializeField] private TextMeshProUGUI pointsText;
        private int points = 0;
        private bool rewardGiven = false;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            CheckForWin(20);
        }

        private void CheckForWin(int pointsToAdd)
        {
            if (QueueUiManager.instance.HasFinnished() && !rewardGiven)
            {
                points += pointsToAdd;
                pointsText.text = points.ToString();
                rewardGiven = true;
            }
        }
        public void ResetReward()
        {
            rewardGiven = false;
        }
    }
}