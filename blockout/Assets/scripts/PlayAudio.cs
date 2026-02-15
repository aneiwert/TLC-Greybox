using System.Collections; 
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.InputSystem; 

//Alex Neiwert

public enum SoundType
{
Footsteps,
Click,

Menu,

Null

}

[RequireComponent(typeof(AudioSource))]
public class PlayAudio : MonoBehaviour
{

  [SerializeField] private AudioClip[] soundList; 
  private static PlayAudio instance; 
  private AudioSource audioSource; 
  private void Awake()
  {
    instance = this; 
  }

  private void Start()
  {
    audioSource = GetComponent<AudioSource>();
  }

  public static void PlaySound(SoundType sound, float volume = 1)
  {
    instance.audioSource.PlayOneShot(instance.soundList[(int)sound], volume); 
  }
   public static void CancelSound(SoundType sound, float volume = 1)
  {
    instance.audioSource.PlayOneShot(instance.soundList[(int)sound], volume); 
  }
}
    



