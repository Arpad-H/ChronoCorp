using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CRTController : MonoBehaviour
{
    [Header("Material (asset reference)")]
    [SerializeField] private Material crtMaterialAsset;

    public Image image;

    [Header("CRT States")]
    [SerializeField] private CRTState onState = new CRTState { collapse = 0f, flash = 0f };

    [SerializeField] private CRTState offState = new CRTState { collapse = 1f, flash = 1.5f };

    private Material runtimeMat;
    private Coroutine currentTransition;

    void Awake()
    {
        // Make a runtime instance so we never touch the asset.
        runtimeMat = new Material(crtMaterialAsset);
        image.material = runtimeMat;
    }

    void OnDestroy()
    {
        // Prevent leaks when you destroy the object.
        if (runtimeMat != null)
            Destroy(runtimeMat);
    }

    public void ShutOff(float duration) => StartTransition(onState, offState, duration);
    public void TurnOn(float duration) => StartTransition(offState, onState, duration);

    void StartTransition(CRTState from, CRTState to, float duration)
    {
        if (currentTransition != null) StopCoroutine(currentTransition);
        currentTransition = StartCoroutine(LerpState(from, to, duration));
    }

    IEnumerator LerpState(CRTState from, CRTState to, float duration)
    {
        float t = 0f;
        duration = Mathf.Max(0.0001f, duration);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            float collapse = Mathf.Lerp(from.collapse, to.collapse, t);
            float flash = Mathf.Lerp(from.flash, to.flash, t);

            runtimeMat.SetFloat("_Collapse", collapse);
            runtimeMat.SetFloat("_Flash", flash);

            yield return null;
        }

        ApplyState(to);
        currentTransition = null;

        // If you're done forever:
        Destroy(gameObject);
    }

    void ApplyState(CRTState state)
    {
        runtimeMat.SetFloat("_Collapse", state.collapse);
        runtimeMat.SetFloat("_Flash", state.flash);
    }

    [System.Serializable]
    public struct CRTState
    {
        public float collapse;
        public float flash;
    }

    // Optional: expose the runtime instance for whatever uses it
    public Material RuntimeMaterial => runtimeMat;
}
