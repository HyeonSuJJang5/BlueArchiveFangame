using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip steakClip;
    public AudioClip friedRiceClip;
    public AudioClip ramenClip;
    public AudioClip serveFoodClip;
    public AudioClip customerEatClip;
    public AudioClip foodActivateClip;

    private Queue<AudioClip> soundQueue = new Queue<AudioClip>();
    private AudioSource audioSource;
    private bool isPlaying = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!audioSource.isPlaying && soundQueue.Count > 0)
        {
            PlayNextSound();
        }
    }

    public void PlaySound(AudioClip clip)
    {
        soundQueue.Enqueue(clip);
        if (!isPlaying)
        {
            PlayNextSound();
        }
    }

    private void PlayNextSound()
    {
        if (soundQueue.Count > 0)
        {
            AudioClip clip = soundQueue.Dequeue();
            audioSource.clip = clip;
            audioSource.Play();
            isPlaying = true;
            Invoke("OnSoundFinished", clip.length);
        }
    }

    private void OnSoundFinished()
    {
        isPlaying = false;
    }

    public void StopSound()
    {
        audioSource.Stop();
        soundQueue.Clear();
        isPlaying = false;
    }
}
