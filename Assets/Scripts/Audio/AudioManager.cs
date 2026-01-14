using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AdaptiveAudioTrack adaptiveAudioTrack;
    public AudioSource winAudioSource;
    public AudioSource lossAudioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }
    void Start()
    {
        PlayBackgroundMusic();
    }
    
    public void PlayWinAudio()
    {
        winAudioSource.Play();
    }
    public void PlayLossAudio()
    {
        lossAudioSource.Play();
    }
    public void PlayBackgroundMusic()
    {
        adaptiveAudioTrack.Play();
    }
    public void StopBackgroundMusic() 
    {
        adaptiveAudioTrack.Stop();
    }
    public void ToggleAdaptiveLayer(int intensity, bool enable)
    {
        if (enable)
            adaptiveAudioTrack.ToggleState(intensity, AdaptiveAudioTrack.LayerState.UNMUTED);
        else adaptiveAudioTrack.ToggleState(intensity, AdaptiveAudioTrack.LayerState.MUTED);
    }
}
