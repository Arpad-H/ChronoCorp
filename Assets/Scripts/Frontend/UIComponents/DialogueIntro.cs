using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DialogueIntro : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(2, 4)] public string text;
        public Sprite image;
    }

    [Header("Dialogue Data")] public DialogueLine[] dialogueLines;

    [Header("UI References")] public TextMeshProUGUI dialogueText;
    public Image dialogueImage;
    public GameObject dialogueBox;
    public GameObject dialoguebg;
    public Animation anim;
    public Animation anim2;
    public CanvasGroup frameCanvasGroup;
    [Header("Typing Settings")] public float typingSpeed = 0.04f;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        StartLine();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        if (isTyping)
        {
            // Skip typing animation
            StopCoroutine(typingCoroutine);
            dialogueText.text = dialogueLines[currentLineIndex].text;
            isTyping = false;
        }
        else
        {
            // Go to next sentence
            currentLineIndex++;

            if (currentLineIndex < dialogueLines.Length)
            {
                StartLine();
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void StartLine()
    {
        dialogueText.text = "";
        dialogueImage.sprite = dialogueLines[currentLineIndex].image;

        typingCoroutine = StartCoroutine(TypeText(dialogueLines[currentLineIndex].text));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        dialogueBox.SetActive(false);
       
        anim.Play("anim");
        anim2.Play("anim2");
        frameCanvasGroup.alpha = 1;
        StartCoroutine(WaitForSeconds(3f));
        //load scene
     
    }
    IEnumerator WaitForSeconds(float waitTime)
    {
        yield return new WaitForSeconds(waitTime/2);
        dialoguebg.SetActive(false);
        yield return new WaitForSeconds(waitTime/2);
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
