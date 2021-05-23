using UnityEngine;
using FistVR;

public class SwitchBladeProxy : MonoBehaviour
{
    public Transform Blade;
    public Vector2 BladeRotRange = new Vector2(-90f, 90f);
    public float BladeOpeningTime;
    public float BladeClosingTime;
    public FVRMeleeWeapon FVRMelee;
    public AudioSource audio_source;
    public AudioClip open_clip;
    public AudioClip close_clip;

    private void Awake()
    {
                
        FistVR.SwitchBladeWeapon real = FistVR.SwitchBladeWeapon.CopyFromMeleeWeapon(FVRMelee, this.gameObject);
        real.Blade = Blade;
        real.BladeRotRange = BladeRotRange;
        real.BladeOpeningTime = BladeOpeningTime;
        real.BladeClosingTime = BladeClosingTime;
        real.audio_source = audio_source;
        real.open_clip = open_clip;
        real.close_clip = close_clip;
        this.gameObject.SetActive(true);
    }
}