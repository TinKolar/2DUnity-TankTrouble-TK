using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource Music;
    public AudioSource SFX;

    [Header("Audio Clips")]
    public AudioClip ShootBullet;
    public AudioClip ReflectBullet;
    public AudioClip TankExploded;
    public AudioClip BulletExploded;
    public AudioClip PlayerWin;
    public AudioClip AIWin;
    public AudioClip BackgroundMusic;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Music.clip = BackgroundMusic;
        Music.Play();
    }



    public void PlaySFX(AudioClip audioClip)
    {
        SFX.PlayOneShot(audioClip);

    }

}
