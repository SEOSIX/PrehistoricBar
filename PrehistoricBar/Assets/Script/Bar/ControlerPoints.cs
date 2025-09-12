using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Script.Bar
{
    public class ControlerPoints : MonoBehaviour
    {
        public static ControlerPoints instance { get; private set; }

        [SerializeField] private TextMeshProUGUI pointsText;
        public int points = 0;

        [SerializeField] private int life;
        private bool rewardGiven = false;
        private Coroutine pointsCoroutine;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            CheckForWin(20);
            CheckForLoose();
        }

        private void CheckForLoose()
        {
            if (QueueUiManager.instance.timerSlider.value <= 0)
            {
                LoseLife();
            }
        }

        private void CheckForWin(int pointsToAdd)
        {
            if (QueueUiManager.instance.HasFinnished() && !rewardGiven)
            {
                StartPointChange(pointsToAdd);
                rewardGiven = true;
            }
        }

        private void StartPointChange(int amount)
        {
            if (pointsCoroutine != null) StopCoroutine(pointsCoroutine);
            pointsCoroutine = StartCoroutine(ChangePoints(amount));
        }

        private IEnumerator ChangePoints(int amount)
        {
            int step = amount > 0 ? 1 : -1;
            int target = points + amount;

            while (points != target)
            {
                points += step;
                pointsText.text = points.ToString("D9");
                yield return new WaitForSeconds(0.01f);
            }
        }

        public void ResetReward()
        {
            rewardGiven = false;
        }

        private void LoseLife()
        {
            if (life > 0)
            {
                life--;
                QueueUiManager.instance.ShowNextClient();
            }
            else
            {
                Debug.Log("Game Over");
            }
        }
    }
}
