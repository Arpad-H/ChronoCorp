using UnityEngine;

public class TutorialOrchestrator : MonoBehaviour
{
    public enum TutorialStep { 
        Onboarding, 
        TimeRipples,
        Generator, 
        Scoring,
        Stability,
        Upgrades,
        Conduits
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
    }


    void OnDisable()
    {
        if (GameFrontendManager.Instance == null) return;

        GameFrontendManager.Instance.GeneratorPlaced -= OnGeneratorPlaced;
        GameFrontendManager.Instance.TimeRipplePlaced -= OnTimeRipplePlaced;
        GameFrontendManager.Instance.ScoreUpdated -= OnScoreUpdated;
    }
    

    void Start()
    {
        int numEnumValues = System.Enum.GetValues(typeof(TutorialStep)).Length;
         stepsCompleted = new  bool[numEnumValues];
        presenter.ShowStep(step);
    }
    
    private void OnGeneratorPlaced()
    {
        if(stepsCompleted[(int)TutorialStep.Generator]) return;
        stepsCompleted[(int)TutorialStep.Generator] = true;
        presenter.ShowStep(TutorialStep.Generator);
    }


    private void OnTimeRipplePlaced()
    {
        if(stepsCompleted[(int)TutorialStep.TimeRipples]) return;
        stepsCompleted[(int)TutorialStep.TimeRipples] = true;
        presenter.ShowStep(TutorialStep.TimeRipples);
    }


    private void OnScoreUpdated()
    {
        if(stepsCompleted[(int)TutorialStep.Scoring]) return;
        stepsCompleted[(int)TutorialStep.Scoring] = true;
        presenter.ShowStep(TutorialStep.Scoring);
    }
}