using TMPro;
using UnityEngine;

public class FakeTerminalAutoTyper : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI terminalText;
    [SerializeField] private TextMeshProUGUI cursorText; // your >_ blinking thing (optional)

    [Header("Script")]
    [TextArea(10, 30)]
    [SerializeField] private string fullScript =
        @"> ssh demo@factory-node
password: ********
> sudo ./run_quality_check --mode=deep
> EXECUTE";

    [Header("Typing feel")]
    [SerializeField] private int charsPerPress = 3;
    [SerializeField] private bool playKeyClick = false;
    [SerializeField] private AudioSource keyClickSource;

    private int index = 0;
    private bool finished = false;

    public bool IsFinished => finished;

    public void ResetTyping()
    {
        index = 0;
        finished = false;
        terminalText.text = "";
    }

    // Call this from your “same button” press
    public void AdvanceTyping()
    {
        if (finished) return;

        int remaining = fullScript.Length - index;
        int count = Mathf.Min(charsPerPress, remaining);

        // Reveal next chars
        terminalText.text += fullScript.Substring(index, count);
        index += count;

        if (playKeyClick && keyClickSource) keyClickSource.Play();

        if (index >= fullScript.Length)
            finished = true;
    }
}