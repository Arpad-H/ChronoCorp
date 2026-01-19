using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Util;

public class ScoreDisplay : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI targetText;
    int currentTarget = 0;
    const int DIGITS = 5;

    private void Start()
    {
        SetCurrentScore(0);
        SetTargetScore(BalanceProvider.Balance.startTargetScore);
    }

    public void SetTargetScore(int target)
    {
        AnimateTarget(target);
    }

    public void SetCurrentScore(int score)
    {
        scoreText.text = score.ToString($"D{DIGITS}");
    }
    public void AnimateTarget(int newTarget)
    {
        StopAllCoroutines();
        StartCoroutine(CountUpStepped(currentTarget, newTarget));
        currentTarget = newTarget;
    }
    IEnumerator CountUpStepped(int from, int to)
    {
        int value = from;

        while (value < to)
        {
            value += Mathf.Max(1, (to - value) / 15);
            targetText.text = "/" + value.ToString($"D{DIGITS}");
            yield return new WaitForSeconds(0.03f);
        }

        targetText.text = "/" + to.ToString($"D{DIGITS}");
    }

}
