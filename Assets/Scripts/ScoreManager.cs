using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : Singleton<ScoreManager>
{
    public Text ScoreText;

    private int _currentScore = 0;
    private int _counterValue = 0;
    private int _increment = 1;

    private void Start()
    {
        UpdateScoreText(_currentScore);
    }

    public void UpdateScoreText(int scoreValue)
    {
        if (ScoreText != null)
        {
            ScoreText.text = scoreValue.ToString();
        }
    }

    public void AddScore(int value)
    {
        _currentScore += value;
        StartCoroutine(CountScoreRoutine());
    }

    IEnumerator CountScoreRoutine()
    {
        int iterations = 0;

        while(_counterValue< _currentScore && iterations< 100000)
        {
            _counterValue += _increment;
            UpdateScoreText(_counterValue);
            iterations++;
            yield return null;
        }

        _counterValue = _currentScore;
        UpdateScoreText(_currentScore);

    }
}
