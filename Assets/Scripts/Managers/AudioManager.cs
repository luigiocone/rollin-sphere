using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource MusicAudioSource;

    public bool IsMusicOn => 
	    MusicAudioSource && MusicAudioSource.clip && MusicAudioSource.isPlaying;

    void Update()
    {
        if (AudioListener.volume != GameSettings.GlobalVolume)
            AudioListener.volume = GameSettings.GlobalVolume;

        if (MusicAudioSource && MusicAudioSource.volume != GameSettings.MusicVolume)
            MusicAudioSource.volume = GameSettings.MusicVolume;
    }
}
