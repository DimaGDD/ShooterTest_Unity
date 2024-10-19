using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; set; }

    public AudioSource shootingChannel;

    public AudioClip M4Shot;
    public AudioClip P1911Shot;

    public AudioSource emptyMagazineSound1911;

    public AudioSource reloadingSoundM4;
    public AudioSource reloadingSound1911;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void PlayShootingSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.Pistol1911:
                shootingChannel.PlayOneShot(P1911Shot);
                break;
            case WeaponModel.M4:
                shootingChannel.PlayOneShot(M4Shot);
                break;
        }
    }

    public void PlayReloadSound(WeaponModel weapon)
    {
        switch (weapon)
        {
            case WeaponModel.Pistol1911:
                reloadingSound1911.Play();
                break;
            case WeaponModel.M4:
                reloadingSoundM4.Play();
                break;
        }
    }
}
