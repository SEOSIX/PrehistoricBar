using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Script.Bar
{
    public class ControlerPoints : MonoBehaviour
    {
        public static ControlerPoints instance { get; private set; }
        
        private static float scoreMultNv;
        private static float scoreMultPrepTime;
        private static float scoreMultDosage;
        private static float scoreMultCombo;
        private static float scoreMultService;

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
            CheckForLoose();
        }

        private void CheckForLoose()
        {
            if (QueueUiManager.instance.timerSlider.value <= 0)
            {
                LoseLife();
            }
            else
            {
                return;
            }
        }

        public void CheckForWin(int pointsToAdd)
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
                QueueUiManager.instance.Over.SetActive(true);
            }
        }

        public static void ResetScore()
        {
            scoreMultNv = 1;
            scoreMultPrepTime = 0;
            scoreMultDosage = 0;
        }

        public static void AddtoDosageMult(float scoremult)
        {
            scoreMultDosage += scoremult;
        }

        public static void GetScore(float time)
        {
            scoreMultNv = EventQueueManager.GetCurrentCocktail().recette.Count;
            Debug.Log($"Score | scoreMultNv : {scoreMultNv}");
            scoreMultPrepTime = 0; //je connais pas le temps
            Debug.Log($"Score | scoreMultPrepTime : {scoreMultPrepTime}");
            Debug.Log($"Score | scoreMultDosage : {scoreMultDosage}");
            bool conditioncombo = false;
            if (conditioncombo) scoreMultCombo += 1;
            else scoreMultCombo = 1;
            Debug.Log($"Score | scoreMultCombo : {scoreMultCombo}");
            float scoretotal = 0;
            scoretotal += scoreMultNv * (1 + scoreMultPrepTime + scoreMultDosage) * 1 /* Service */ * scoreMultCombo;
            Debug.Log($"Score | scoretotal : {scoretotal}");
            
            instance.CheckForWin(Mathf.RoundToInt(scoretotal));
            
            instance.StartPointChange((int)scoretotal);
        }

public void ResetPointsAndLife()
        {
            points = 0;
            life = 2;
            rewardGiven = false;
            pointsText.text = points.ToString("D9");
        }
    }
}
