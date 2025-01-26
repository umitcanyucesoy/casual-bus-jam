using System;
using DG.Tweening;
using UnityEngine;

namespace _CasualBusJam.Scripts.SFX
{
    public class SoundController : MonoBehaviour
    {
        [Header("------- GAME SOURCES ------")]
        public AudioSource audioSource;
        public AudioClip tapSound, buttonSound, hitSound, sort, full, win, fail, nocoinPOP;
        public AudioClip moving;
        
        private bool _isFullSoundPlaying = false;
        
        public static SoundController Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }
        
        public void PlayOneShot(AudioClip clip, float volume)
        {
            audioSource.PlayOneShot(clip, volume);
        }

        public void PlayOneShot(AudioClip clip)
        {
            audioSource.PlayOneShot(clip);
        }
        
        public void PlayFullSound()
        {
            if (!_isFullSoundPlaying)
            {
                audioSource.PlayOneShot(full);
                _isFullSoundPlaying = true;
                DOVirtual.DelayedCall(full.length, ()=> _isFullSoundPlaying = false);
            }
        }
        
    }
}