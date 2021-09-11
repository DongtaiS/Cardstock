using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public enum MusicEnum { Combat_ArmadaOut, Combat_ThroughTheAlps, Explore_DividedWeEcho, Explore_WindStoleMyLove, }
    public enum CombatSFXEnum { FleshPierce, SwingHigh, SwingLow, SwingNormal, DaggerThrow, AirWoosh, MetalWoosh, FleshHit, MetalHit, 
                                MetalHit2, Unsheath, FleshHit2, FleshSmash, BigWoosh, FastMetalWoosh1, FastMetalWoosh2, FastMetalWoosh3, 
                                FastMetalWoosh4 }
    public enum UnitSFXEnum { SlimeBounce1, SlimeBounce2, FootstepLight1, FootstepLight2, FootstepLight3, FootstepLight4 }
    public List<UnitSFXEnum> FootstepsLight = new List<UnitSFXEnum> { UnitSFXEnum.FootstepLight1, UnitSFXEnum.FootstepLight2, UnitSFXEnum.FootstepLight3, UnitSFXEnum.FootstepLight4 };
    public enum UISFXEnum { PageSlide, PageFlip, PageFlip2, ShortSlide, Woosh}
    [SerializeField] private AudioMixer Mixer;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private GenericListToDictionary<MusicEnum, AudioClip> MusicList = new GenericListToDictionary<MusicEnum, AudioClip>();
    [SerializeField] private GenericListToDictionary<CombatSFXEnum, AudioClip> CombatSFXList = new GenericListToDictionary<CombatSFXEnum, AudioClip>();
    [SerializeField] private GenericListToDictionary<UnitSFXEnum, AudioClip> UnitSFXList = new GenericListToDictionary<UnitSFXEnum, AudioClip>();
    [SerializeField] private GenericListToDictionary<UISFXEnum, AudioClip> UISFXList = new GenericListToDictionary<UISFXEnum, AudioClip>();
    private Dictionary<MusicEnum, AudioClip> music;
    public Dictionary<CombatSFXEnum, AudioClip> combatSoundEffects;
    private Dictionary<UnitSFXEnum, AudioClip> unitSoundEffects;
    private Dictionary<UISFXEnum, AudioClip> uiSoundEffects;
    void Awake()
    {
        music = MusicList.ToDictionary();
        combatSoundEffects = CombatSFXList.ToDictionary();
        unitSoundEffects = UnitSFXList.ToDictionary();
        uiSoundEffects = UISFXList.ToDictionary();
    }
    private void Start()
    {
        SetMasterVolume(0.5f);
    }
    public void SetMusicVolume(float vol)
    {
        if (vol == 0)
        {
            Mixer.SetFloat("MusicVolume", -80);
        }
        else
        {
            Mixer.SetFloat("MusicVolume", Mathf.Log10(vol) * 20);
        }
    }
    public void SetSFXVolume(float vol)
    {
        if (vol == 0)
        {
            Mixer.SetFloat("SFXVolume", -80);
        }
        else
        {
            Mixer.SetFloat("SFXVolume", Mathf.Log10(vol) * 20);
        }
    }
    public void SetMasterVolume(float vol)
    {
        if (vol == 0)
        {
            Mixer.SetFloat("MasterVolume", -80);
        }
        else
        {
            Mixer.SetFloat("MasterVolume", Mathf.Log10(vol) * 20);
        }
    }
    public IEnumerator FadeMusic(MusicEnum newMusic, float duration)
    {
        float oldVol = musicSource.volume;
        yield return Globals.InterpFloat(musicSource.volume, 0, duration * 0.75f, Globals.AnimationCurves.IncEaseIn, vol => musicSource.volume = vol);
        PlayMusic(newMusic);
        yield return Globals.InterpFloat(0, oldVol, duration * 0.25f, Globals.AnimationCurves.IncEaseIn, vol => musicSource.volume = vol);
    }
    public void PlayMusic(MusicEnum newMusic)
    {
        musicSource.clip = music[newMusic];
        musicSource.Play();
    }
    public AudioClip GetClip(CombatSFXEnum combatsfx)
    {
        return combatSoundEffects[combatsfx];
    }
    public AudioClip GetClip(UnitSFXEnum unitsfx)
    {
        return unitSoundEffects[unitsfx];
    }
    public AudioClip GetClip(UISFXEnum uisfx)
    {
        return uiSoundEffects[uisfx];
    }
    public void PlaySoundEffectGlobal(AudioClip clip)
    {
        sfxSource.clip = clip;
        sfxSource.Play();
    }
    public void PlayOneShotSoundEffectGlobal(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip, sfxSource.volume);
    }
    public void PlaySoundEffect(AudioSource src, AudioClip clip)
    {
        src.PlayOneShot(clip, sfxSource.volume);
    }
}