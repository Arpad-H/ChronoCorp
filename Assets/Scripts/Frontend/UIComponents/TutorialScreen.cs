using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScreen : MonoBehaviour
{[System.Serializable]
    public class TutorialStep
    {
        [TextArea(2, 4)] public string text;
        public RectTransform pointerTarget;
        public Vector3 startingRotation; 
        public int tapCount = 2;
    }

    [Header("Tutorial Data")]
    public TutorialStep[] steps;
    private int[] currentSequence;   
    private int currentSequencePos = 0;   
    [Header("UI References")]
    public TextMeshProUGUI tutorialText;
    public GameObject crtTextHolder;
    public RectTransform moverRoot;    // Pivot (0.5, 1)
    public RectTransform tapperRoot;   // Pivot (0.5, 0)
    public UITapEffect tapperScript;

    [Header("Settings")]
    public float typingSpeed = 0.04f;
    public float pointerMoveSpeed = 8f;

    private int currentStepIndex = 0;
    private bool isTyping = false;
    private bool isMoving = false; // Track movement state
    private Coroutine typingCoroutine;
    private Coroutine movementCoroutine;
    public bool end = false;
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) HandleClick();
    }

    private void StartStep()
    {
        tutorialText.text = "";
        
        // Clean up previous routines
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        
        movementCoroutine = StartCoroutine(AnimateSequence(steps[currentStepIndex]));
        typingCoroutine = StartCoroutine(TypeText(steps[currentStepIndex].text));
    }

    private IEnumerator AnimateSequence(TutorialStep step)
    {
        if (step.pointerTarget == null) yield break;

        isMoving = true;
        moverRoot.gameObject.SetActive(true);
        tapperRoot.gameObject.SetActive(false);

        Quaternion targetRot = Quaternion.Euler(step.startingRotation);

        while (Vector3.Distance(moverRoot.position, step.pointerTarget.position) > 0.1f || 
               Quaternion.Angle(moverRoot.localRotation, targetRot) > 0.1f)
        {
            moverRoot.position = Vector3.Lerp(moverRoot.position, step.pointerTarget.position, Time.deltaTime * pointerMoveSpeed);
            moverRoot.localRotation = Quaternion.Slerp(moverRoot.localRotation, targetRot, Time.deltaTime * pointerMoveSpeed);
            yield return null;
        }

        CompleteMovement(step);
    }

    // Helper method to handle the swap safely
    private void CompleteMovement(TutorialStep step)
    {
        isMoving = false;
        moverRoot.position = step.pointerTarget.position;
        moverRoot.localRotation = Quaternion.Euler(step.startingRotation);

        tapperRoot.localRotation = moverRoot.localRotation;
        float stickLength = tapperRoot.rect.height * tapperRoot.lossyScale.y;
        tapperRoot.position = moverRoot.position - (tapperRoot.up * stickLength);

        moverRoot.gameObject.SetActive(false);
        tapperRoot.gameObject.SetActive(true);
        tapperScript.UpdateRestRotation();
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        foreach (char c in text)
        {
            tutorialText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;

        // Start tap if not already handled by a skip
        if (tapperRoot.gameObject.activeInHierarchy)
        {
            tapperScript.TriggerMultiTap(steps[currentStepIndex].tapCount);
        }
    }

    private void HandleClick()
    {
        if (isTyping || isMoving)
        {
            // 1. Stop the movement and force the swap to Tapper
            if (movementCoroutine != null) StopCoroutine(movementCoroutine);
            CompleteMovement(steps[currentStepIndex]);

            // 2. Stop the typing and show full text
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            tutorialText.text = steps[currentStepIndex].text;
            
            isTyping = false;
            isMoving = false;

            // 3. Now it is safe to tap because CompleteMovement activated the Tapper
            tapperScript.TriggerMultiTap(steps[currentStepIndex].tapCount);
        }
        else
        {
            AdvanceSequenceOrClose();
        }
    }

    private void EndTutorial()
    {
        GameFrontendManager.Instance.EndTutorial();
        Destroy(this.gameObject);
        Destroy(crtTextHolder);
    }

    public void ShowStep(TutorialOrchestrator.TutorialStep step)
    {
        ShowSteps(new List<TutorialOrchestrator.TutorialStep> { step });
    }
    public void ShowSteps(List<TutorialOrchestrator.TutorialStep> group)
    {
        GameFrontendManager.Instance.SetGameState(GameFrontendManager.GameState.PAUSED);
        this.gameObject.SetActive(true);
        crtTextHolder.SetActive(true);

        // Convert orchestrator enum steps to indices into steps[]
        currentSequence = new int[group.Count];
        for (int i = 0; i < group.Count; i++)
            currentSequence[i] = (int)group[i];

        currentSequencePos = 0;
        StartCurrentInSequence();
    }
    private void StartCurrentInSequence()
    {
        if (currentSequence == null || currentSequence.Length == 0)
            return;

        currentStepIndex = currentSequence[currentSequencePos];
        StartStep();
    }
    private void AdvanceSequenceOrClose()
    {
        currentSequencePos++;

        if (currentSequencePos < currentSequence.Length)
        {
            StartCurrentInSequence();
            return;
        }

        // Sequence finished
        this.gameObject.SetActive(false);
        crtTextHolder.SetActive(false);
        GameFrontendManager.Instance.SetGameState(GameFrontendManager.GameState.PLAYING);
   //     if (currentSequencePos == currentSequence.Length-1) EndTutorial();
    }
}