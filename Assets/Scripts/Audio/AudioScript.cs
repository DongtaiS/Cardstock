using System.Collections.Generic;
using UnityEngine;
public class AudioScript : MonoBehaviour
{
    [SerializeField] private List<AudioClip> audioClips;
    private AudioSource source;
    private void Awake()
    {
        source = GetComponentInChildren<AudioSource>();
    }
    public void SetAudioSource(AudioSource src)
    {
        source = src;
    }
    public void PlayAudio(int index, float volume)
    {
        source.PlayOneShot(audioClips[index], source.volume);
    }
    public void PlayCombatSFX(AudioManager.CombatSFXEnum sfx)
    {
        Globals.AudioManager.PlaySoundEffect(source, Globals.AudioManager.GetClip(sfx));
    }
    public void PlayUnitSFX(AudioManager.UnitSFXEnum sfx)
    {
        Globals.AudioManager.PlaySoundEffect(source, Globals.AudioManager.GetClip(sfx));
    }
    public void PlayUISFX(AudioManager.UISFXEnum sfx)
    {
        Globals.AudioManager.PlaySoundEffect(source, Globals.AudioManager.GetClip(sfx));
    }
    public void PlayFootstepLight()
    {
        Globals.AudioManager.PlaySoundEffect(source, Globals.AudioManager.GetClip(Globals.AudioManager.FootstepsLight[Globals.random.Next(0, Globals.AudioManager.FootstepsLight.Count)]));
    }
}
