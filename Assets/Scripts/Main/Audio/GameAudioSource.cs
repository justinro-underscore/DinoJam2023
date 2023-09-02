using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameAudioSource : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void SetAudioClip(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.Play();
    }

    public void SetVolume(float volume)
    {
        _audioSource.volume = volume;
    }

    public void SetLooping(bool looping)
    {
        _audioSource.loop = looping;
    }

    public void Stop()
    {
        _audioSource.Stop();
    }

    public bool IsPlaying()
    {
        return _audioSource.isPlaying;
    }

    public float GetVolume()
    {
        return _audioSource.volume;
    }

    public string GetClipName()
    {
        return _audioSource.clip ? _audioSource.clip.name : "";
    }
}