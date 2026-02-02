using System.Collections.Generic;
using UnityEngine;

public class TutorialOrchestrator : MonoBehaviour
{
    public ScoreDisplay scoreDisplay;
    public enum TutorialStep { 
        Onboarding, 
        TimeRipples,
        SupplyTimeRippleWithEnergy,
        Scoring,
        Stability,
        End1,
        End2,
        End3
    }
    public TutorialScreen presenter;
    private bool[] stepsCompleted;
    private TutorialStep step = TutorialStep.Onboarding;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
// Example — replace with your real event sources
        GameFrontendManager.Instance.GeneratorPlaced += OnGeneratorPlaced;
        GameFrontendManager.Instance.TimeRipplePlaced += OnTimeRipplePlaced;
        GameFrontendManager.Instance.ScoreUpdated += OnScoreUpdated;
        GameFrontendManager.Instance.GameStarted += OnGameStarted;
        scoreDisplay.ScoreAlmostReached += OnScoreUpdated;
        GameFrontendManager.Instance.ExpansionTutorial += OnExpansionTutorial;
    }


    void OnDisable()
    {
        if (GameFrontendManager.Instance == null) return;
        
        GameFrontendManager.Instance.GeneratorPlaced -= OnGeneratorPlaced;
        GameFrontendManager.Instance.TimeRipplePlaced -= OnTimeRipplePlaced;
        GameFrontendManager.Instance.ScoreUpdated -= OnScoreUpdated;
        scoreDisplay.ScoreAlmostReached -= OnScoreUpdated;
        GameFrontendManager.Instance.ExpansionTutorial -= OnExpansionTutorial;
        GameFrontendManager.Instance.GameStarted -= OnGameStarted;

    }

    void OnGameStarted()
    {
        presenter.ShowStep(step);
    }
    void Start()
    {
        int numEnumValues = System.Enum.GetValues(typeof(TutorialStep)).Length;
         stepsCompleted = new  bool[numEnumValues];
        
    }
    
    private void OnGeneratorPlaced()
    {
        if(stepsCompleted[(int)TutorialStep.Stability]) return;
        stepsCompleted[(int)TutorialStep.Stability] = true;
        presenter.ShowStep(TutorialStep.Stability);
    }


    private void OnTimeRipplePlaced()
    {
        if(stepsCompleted[(int)TutorialStep.TimeRipples]) return;
        stepsCompleted[(int)TutorialStep.TimeRipples] = true;
        stepsCompleted[(int)TutorialStep.SupplyTimeRippleWithEnergy] = true;
        presenter.ShowSteps(new List<TutorialStep>{TutorialStep.TimeRipples, TutorialStep.SupplyTimeRippleWithEnergy});
    }


    private void OnScoreUpdated()
    {
        if(stepsCompleted[(int)TutorialStep.Scoring]) return;
        stepsCompleted[(int)TutorialStep.Scoring] = true;
        presenter.ShowSteps(new List<TutorialStep>{TutorialStep.Scoring,TutorialStep.End3});
    }
    private void OnExpansionTutorial()
    {
        if(stepsCompleted[(int)TutorialStep.End1]) return;
        stepsCompleted[(int)TutorialStep.End1] = true;
        stepsCompleted[(int)TutorialStep.End2] = true;
        stepsCompleted[(int)TutorialStep.End3] = true;
        presenter.ShowSteps(new List<TutorialStep>{ TutorialStep.End2});
    }
}