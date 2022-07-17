using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.MainMenuSoundReplacer", "Main Menu Sound Replacer", "1.0.0")]
    public class MainMenuSoundReplacer : BaseUnityPlugin
    {
        private ConfigEntry<float> _mainMenuSoundVolume;
        private ConfigEntry<bool> _shuffleEnabled;
        private ConfigEntry<bool> _forceSingleClip;
        private ConfigEntry<string> _forcedClipName;

        private static readonly string s_sideloaderPath = Path.Combine(Paths.BepInExRootPath, "Sideloader");
        private static readonly string s_soundPath = Path.Combine(s_sideloaderPath, "MainMenuSoundReplacer");

        private List<AudioClip> _loadedAudioClips = new List<AudioClip>();
        private AudioSource _ambientAudio = null;

        private bool _mainMenuConfigured = false;
        private float _audioClipsLoading = 0f;
        private int _clipIndex = 0;

        public MainMenuSoundReplacer()
        {
            _mainMenuSoundVolume = Config.Bind("Main Menu Sound Replacer", "Main Menu Sound Volume", 0.3f, "Sets the volume for the audio source.");
            _shuffleEnabled = Config.Bind("Main Menu Sound Replacer", "Shuffle audio clips", true, "If true, will shuffle the audio clips whenever you enter the main menu.");
            _forceSingleClip = Config.Bind("Main Menu Sound Replacer", "Force Single Clip", false, "If true, will force the system to only play the audio clip specified below.");
            _forcedClipName = Config.Bind("Main Menu Sound Replacer", "Forced Clip Name", "", "Forced clip name with file ending (aka .wav)");
        }

        public void Awake()
        {
            if (!Directory.Exists(s_sideloaderPath))
            {
                Logger.LogError("Sideloader folder not found. Can't replace MainMenuSounds!");
                Destroy(this);
                return;
            }
            if (!Directory.Exists(s_soundPath))
            {
                Logger.LogError("MainMenuSoundReplacer folder not found. Can't replace MainMenuSounds!");
                Destroy(this);
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(s_soundPath);
            FileInfo[] filesInDir = directoryInfo.GetFiles();
            if (filesInDir.Length > 0)
            {
                Logger.LogInfo($"Found {filesInDir.Length} audio clips!");
                foreach (FileInfo foundFile in filesInDir)
                {
                    Logger.LogInfo($"Loading audio clip {foundFile.Name}.");

                    StartCoroutine(LoadAudio(foundFile));
                }
                Logger.LogInfo($"Finished loading audio clips, enjoy the tunes!");
            }
        }

        public void Update()
        {
            if (_audioClipsLoading == 1f)
            {
                Scene scene = SceneManager.GetSceneByName("MainMenu3");

                if (!_mainMenuConfigured && scene.name == "MainMenu3")
                {
                    _ambientAudio = GameObject.Find("[AmbientAudio]").GetComponent<AudioSource>();
                    AudioSource AmbientWind = GameObject.Find("[AmbientWind]").GetComponent<AudioSource>();
                    AmbientWind.Stop();
                    _ambientAudio.Stop();

                    _ambientAudio.pitch = 1f;
                    _ambientAudio.loop = false;
                    _ambientAudio.volume = _mainMenuSoundVolume.Value;

                    ShuffleClips();
                    _clipIndex = 0;
                    _mainMenuConfigured = true;
                }
                else if (_mainMenuConfigured && scene.name != "MainMenu3")
                {
                    _ambientAudio = null;
                    _mainMenuConfigured = false;
                }

                if (_mainMenuConfigured && _ambientAudio != null && !_ambientAudio.isPlaying) ChangeClip();
            }
        }
        
        private IEnumerator LoadAudio(FileInfo foundFile)
        {
            string path = "file:///" + Path.Combine(s_soundPath, foundFile.FullName);
            WWW www = new WWW(path);
            while (www.progress < 1f)
            {
                _audioClipsLoading = www.progress;
                yield return null;
            }
            _audioClipsLoading = www.progress;
            AudioClip clip = www.GetAudioClip();
            clip.name = foundFile.Name;
            _loadedAudioClips.Add(clip);
            Logger.LogInfo($"{clip.name} loaded.");

        }

        private void ChangeClip()
        {
            if (!_forceSingleClip.Value)
            {
                AudioClip clip = _loadedAudioClips[_clipIndex];
                clip.LoadAudioData();

                _ambientAudio.clip = clip;

                _ambientAudio.Play();
                _clipIndex++;
                if (_clipIndex >= _loadedAudioClips.Count) _clipIndex = 0;
            }
            else
            {
                foreach (AudioClip audioClip in _loadedAudioClips)
                {
                    if (audioClip.name == _forcedClipName.Value)
                    {
                        audioClip.LoadAudioData();
                        _ambientAudio.clip = audioClip;
                        _ambientAudio.Play();
                    }
                }
            }
        }

        private void ShuffleClips()
        {
            if (!_shuffleEnabled.Value) return;
            for (int i = 0; i < _loadedAudioClips.Count; i++)
            {
                AudioClip temp = _loadedAudioClips[i];
                int randomIndex = UnityEngine.Random.Range(i, _loadedAudioClips.Count);
                _loadedAudioClips[i] = _loadedAudioClips[randomIndex];
                _loadedAudioClips[randomIndex] = temp;
            }
        }
        private void Unhook()
        {
            _shuffleEnabled.SettingChanged -= SettingsChanged;
            _mainMenuSoundVolume.SettingChanged -= SettingsChanged;
        }
        private void Hook()
        {
            _shuffleEnabled.SettingChanged += SettingsChanged;
            _mainMenuSoundVolume.SettingChanged += SettingsChanged;
        }

        private void SettingsChanged(object sender, EventArgs e)
        {
            if (_ambientAudio != null)
            {
                _ambientAudio.volume = _mainMenuSoundVolume.Value;
            }
        }
    }
}
