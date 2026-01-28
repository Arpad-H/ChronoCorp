// UIManager.cs

using System;
using System.Collections;
using System.Collections.Generic;
using Frontend.UIComponents;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Util;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject deleteButtonPrefab;
    private GameObject deleteNodeButton;
    public GameOverScript gameOverScreen;
    public ConduitVisualizer conduitVisualizer;
    public GameObject pauseIcon;
    public TextMeshProUGUI pauseTimer;
    private UpgradeChoiceMenu upgradeChoiceMenuComponent;
    public GameObject cardChoiceMenu;
    private UpgradeCardData[] allUpgrades;
    public ScoreDisplay scoreDisplay;
private Coroutine blinkCoroutine;
private Coroutine countdownCoroutine;
float remainingTime;
    void Awake()
    {
        Instance = this;
        upgradeChoiceMenuComponent = cardChoiceMenu.GetComponent<UpgradeChoiceMenu>();
        LoadAllCards();
    }

    private void Start()
    {
       InputManager.Instance.OnRightClick += CancelDeleteButton;
    }

    private void CancelDeleteButton()
    {
        if (deleteNodeButton != null)
        {
            Destroy(deleteNodeButton);
        }
    }

    public void ShowGameOver(string reason)
    {
        gameOverScreen.gameObject.SetActive(true);
        gameOverScreen.SetScore(scoreDisplay.GetCurrentScore());
    }

    public DeleteButton SpawnDeleteButton(Vector3 position)
    {
        if (deleteNodeButton != null)
        {
            Destroy(deleteNodeButton);
        }
        deleteNodeButton = Instantiate(deleteButtonPrefab, position, Quaternion.identity);
        return deleteNodeButton.GetComponent<DeleteButton>();
    }
    public void ShowUpgradeChoiceMenu()
    {
        StartCoroutine(QueueUpgradeMenu());
    }
    
    private IEnumerator QueueUpgradeMenu()
    {
        while (!SafeToShowUpgradeUI())
        {
            yield return null; 
        }
        ExecuteShowMenu();
    }
    
    private void ExecuteShowMenu()
    {
        List<UpgradeCardData> availableUpgrades = GetRandomUpgrades(BalanceProvider.Balance.cardsShownPerUpgradeChoice);
        cardChoiceMenu.SetActive(true);
        upgradeChoiceMenuComponent.ShowChoices(availableUpgrades);
    }
    private List<UpgradeCardData> GetRandomUpgrades(int i)
    {
        List<UpgradeCardData> selectedUpgrades = new List<UpgradeCardData>();
        List<int> usedIndices = new List<int>();

        System.Random rand = new System.Random();

        while (selectedUpgrades.Count < i && selectedUpgrades.Count < allUpgrades.Length)
        {
            int index = rand.Next(allUpgrades.Length);
            if (!usedIndices.Contains(index))
            {
                usedIndices.Add(index);
                selectedUpgrades.Add(allUpgrades[index]);
            }
        }
        return selectedUpgrades;
    }

    private void LoadAllCards()
    {
       allUpgrades = Resources.LoadAll<UpgradeCardData>("UpgradeCardData");
    }
    
    private bool SafeToShowUpgradeUI()
    {
        return !conduitVisualizer.IsDraggingConduit();
    }
    public void SetScore(int score)
    {
        scoreDisplay.SetCurrentScore(score);
    }
    public void SetTargetScore(int score)
    {
        scoreDisplay.SetTargetScore(score);
    }
    public void AddScore(int score)
    {
       scoreDisplay.AddScore(score);
    }
    public void GamePaused(bool paused)
    {
        if (paused)
        {
            remainingTime = BalanceProvider.Balance.pauseDurationSeconds;


            pauseTimer.gameObject.SetActive(true);
            pauseIcon.SetActive(true);


            blinkCoroutine = StartCoroutine(BlinkPauseIcon());
            countdownCoroutine = StartCoroutine(PauseCountdown());
        }
        else
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);


            if (countdownCoroutine != null)
                StopCoroutine(countdownCoroutine);


            pauseIcon.SetActive(false);
            pauseTimer.gameObject.SetActive(false);
        }
    }
    IEnumerator PauseCountdown()
    {
        while (remainingTime > 0f)
        {
            remainingTime -= Time.unscaledDeltaTime;
            remainingTime = Mathf.Max(remainingTime, 0f);


            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);


            pauseTimer.text = $"{minutes:00}:{seconds:00}";


            yield return null; // update every frame (smooth)
        }


// optional: auto-unpause or trigger event
        pauseTimer.text = "00:00";
    }

    IEnumerator BlinkPauseIcon()
    {
        while (true)
        {
            pauseIcon.SetActive(!pauseIcon.activeSelf);
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

}