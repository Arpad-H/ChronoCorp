using TMPro;
using UnityEngine;

public class FakeTerminalAutoTyper : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI terminalText;
    [SerializeField] private TextMeshProUGUI cursorText; // optional

    [Header("Function typing feel")]
    [Tooltip("Characters appended per button press while typing the Lua function.")]
    [SerializeField] private int charsPerPress = 2;

    [Tooltip("Chance [0..1] to inject a tiny typo + backspace while typing.")]
    [Range(0f, 1f)]
    [SerializeField] private float typoChance = 0.05f;

    [SerializeField] private Vector2Int typoLengthRange = new Vector2Int(1, 2);

    [Header("Optional SFX")]
    [SerializeField] private bool playKeyClick = false;
    [SerializeField] private AudioSource keyClickSource;

    [Header("Boot + Compile (instant)")]
    [TextArea(6, 20)]
    [SerializeField] private string bootText =
@"> boot subsystem: energy_router
> loading embedded lua runtime...
> sandbox mode enabled
>";

    [TextArea(6, 20)]
    [SerializeField] private string compileText =
@"> compile ok
> binding graph context...
> EXECUTE";

    [Header("Lua function (typed)")]
    [TextArea(10, 30)]
    [SerializeField] private string luaFunction =
@"-- user defined energy logic
function distribute(node)
    if node.index % 2 == 0 then
        forward(node, node.energy * 0.75)
    else
        buffer(node, node.energy)
    end
end";

    // If you want to hook your rickroll without polling IsFinished:
    public System.Action OnExecuteConfirmed;

    private enum Phase { Idle, TypingFunction, CompileShownWaiting, Done }
    private Phase phase = Phase.Idle;

    private int index = 0;
    private bool finished = false;

    public bool IsFinished => finished;

    public void ResetTyping()
    {
        index = 0;
        finished = false;
        phase = Phase.Idle;
        if (terminalText) terminalText.text = "";
    }

    /// <summary>
    /// Same button:
    /// 1) Boot appears instantly
    /// 2) Types function char-by-char
    /// 3) Shows compile instantly, but waits for one more press
    /// 4) Next press marks finished / triggers OnExecuteConfirmed
    /// </summary>
    public void AdvanceTyping()
    {
        if (finished) return;

        switch (phase)
        {
            case Phase.Idle:
                terminalText.text = bootText + "\n\n";
                phase = Phase.TypingFunction;
                index = 0;
                return;

            case Phase.TypingFunction:
                TypeFunctionChunk();

                // When we reach the end, show compile instantly BUT don't finish yet.
                if (index >= luaFunction.Length)
                {
                    terminalText.text += "\n\n" + compileText;
                    phase = Phase.CompileShownWaiting;
                }
                return;

            case Phase.CompileShownWaiting:
                // This extra press is your "execute confirmation" moment.
                finished = true;
                phase = Phase.Done;
                OnExecuteConfirmed?.Invoke();
                return;
        }
    }

    private void TypeFunctionChunk()
    {
        int remaining = luaFunction.Length - index;
        if (remaining <= 0) return;

        int count = Mathf.Min(charsPerPress, remaining);

        // Rare typo injection (kept subtle so it doesn't look staged)
        if (Random.value < typoChance)
        {
            int typoLen = Random.Range(typoLengthRange.x, typoLengthRange.y + 1);
            typoLen = Mathf.Clamp(typoLen, 1, 3);

            string wrong = GetPlausibleWrongChunk(typoLen);
            Append(wrong);
            Backspace(wrong.Length);
        }

        terminalText.text += luaFunction.Substring(index, count);
        index += count;

        if (playKeyClick && keyClickSource) keyClickSource.Play();
    }

    private void Append(string s)
    {
        terminalText.text += s;
        if (playKeyClick && keyClickSource) keyClickSource.Play();
    }

    private void Backspace(int count)
    {
        string t = terminalText.text;
        count = Mathf.Clamp(count, 0, t.Length);
        if (count <= 0) return;

        terminalText.text = t.Substring(0, t.Length - count);
        if (playKeyClick && keyClickSource) keyClickSource.Play();
    }

    private string GetPlausibleWrongChunk(int len)
    {
        string[] chunks = { "fn", "fun", "fuc", "fro", "forw", "buf", "thn", "ned" };
        string pick = chunks[Random.Range(0, chunks.Length)];
        if (pick.Length > len) pick = pick.Substring(0, len);
        if (pick.Length < len) pick = pick.PadRight(len, 'x');
        return pick;
    }
}
