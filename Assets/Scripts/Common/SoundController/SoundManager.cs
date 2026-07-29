using System.Collections.Generic;
using DataAccount;
using JinGroup.Base.LoadData;
using UnityEngine;

namespace Base.Core.Sound
{
    public class SoundManager : SingletonMonoDontDestroy<SoundManager>
    {
        public SoundManager(string className) : base(className)
        {
        }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource soundSource;

        private Dictionary<SoundType, AudioClip> _allAudios;

        #region API

        public AudioClip GetAudioClip(SoundType soundType)
        {
            if (_allAudios == null)
            {
                _allAudios = new Dictionary<SoundType, AudioClip>();
            }

            if (_allAudios.ContainsKey(soundType))
            {
                return _allAudios[soundType];
            }

            var audioClip = LoadResourceController.Instance.LoadAudioClip(soundType);
            if (audioClip != null)
            {
                _allAudios[soundType] = audioClip;
            }

            return audioClip;
        }

        public void PlayBackgroundMusic(SoundType soundType)
        {
            var audioClip = GetAudioClip(soundType);
            musicSource.clip = audioClip;
            musicSource.Play();
        }

        public void PauseMusic()
        {
            musicSource.Pause();
        }

        public void ResumeMusic()
        {
            musicSource.UnPause();
        }

        public void PlaySound(SoundType soundType)
        {
            var audioClip = GetAudioClip(soundType);
            soundSource.PlayOneShot(audioClip, soundSource.volume);
        }

        public void PlayOneShot(SoundType soundType, float volume = 0.8f)
        {
            var audioClip = GetAudioClip(soundType);
            soundSource.PlayOneShot(audioClip, volume);
        }

        #endregion

        private void Awake()
        {
            InitData();
            this.RegisterListener(EventID.OnSoundChange, (sender, param) => OnSoundChange());
            this.RegisterListener(EventID.OnMusicChange, (sender, param) => OnMusicChange());
        }

        private void InitData()
        {
            OnSoundChange();
            OnMusicChange();
        }

        private void OnSoundChange()
        {
            soundSource.mute = DataAccountPlayer.PlayerSettings.SoundOff;
        }

        private void OnMusicChange()
        {
            musicSource.mute = DataAccountPlayer.PlayerSettings.MusicOff;
        }
    }
}