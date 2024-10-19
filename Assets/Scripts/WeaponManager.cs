using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; set; }

    public List<GameObject> weaponSlots;

    public GameObject activeWeaponSlot;

    public GameObject player;

    public Transform bulletSpawn;

    [Header("Ammo")]
    public int totalRifleAmmo = 0;
    public int totalPistolAmmo = 0;
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

    private void Start()
    {
        activeWeaponSlot = weaponSlots[0];
    }

    private void Update()
    {
        foreach (GameObject weaponSlot in weaponSlots)
        {
            if (weaponSlot == activeWeaponSlot)
            {
                weaponSlot.SetActive(true);
            }
            else
            {
                weaponSlot.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Q)) // Нажимаем "Q" для выброса оружия
        {
            DropWeapon();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchActiveSlot(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchActiveSlot(1);
        }
    }

    public void PickupWeapon(GameObject pickedupWeapon)
    {
        AddWeaponIntoActiveSlot(pickedupWeapon);
    }

    private void AddWeaponIntoActiveSlot(GameObject pickedupWeapon)
    {
        DropCurrentWeapon(pickedupWeapon);

        pickedupWeapon.GetComponent<BoxCollider>().enabled = false;
        pickedupWeapon.GetComponent<Rigidbody>().isKinematic = true;

        // Ставим слой, чтобы оружие рендерилось поверх других объектов
        pickedupWeapon.gameObject.layer = 6;
        SetLayerRecursively(pickedupWeapon.gameObject, 6);

        Weapon weapon = pickedupWeapon.GetComponent<Weapon>();

        ConstraintToWeapon.Instance.AddWeaponIntoHands(pickedupWeapon);

        pickedupWeapon.transform.SetParent(activeWeaponSlot.transform, false);

        weapon.isActiveWeapon = true;
        weapon.animator.enabled = true;

        ActivateConstraint(pickedupWeapon);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void DropCurrentWeapon(GameObject pickedupWeapon)
    {
        if (activeWeaponSlot.transform.childCount > 0)
        {
            var weaponToDrop = activeWeaponSlot.transform.GetChild(0).gameObject;

            DropWeapon(weaponToDrop);
        }
    }

    public void SwitchActiveSlot(int slotNumber)
    {
        if (activeWeaponSlot.transform.childCount > 0)
        {
            Weapon currentWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            currentWeapon.isActiveWeapon = false;
            DeactivateConstraint(currentWeapon.gameObject);
        }

        activeWeaponSlot = weaponSlots[slotNumber];

        if (activeWeaponSlot.transform.childCount > 0)
        {
            Weapon newWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            newWeapon.isActiveWeapon = true;
            ActivateConstraint(newWeapon.gameObject);
        }

    }

    internal void PickupAmmo(AmmoBox ammo)
    {
        switch (ammo.ammoType)
        {
            case AmmoBox.AmmoType.PistolAmmo:
                totalPistolAmmo += ammo.ammoAmount;
                break;
            case AmmoBox.AmmoType.RifleAmmo:
                totalRifleAmmo += ammo.ammoAmount;
                break;
        }
    }

    internal void DecreaseTotalAmmo(int bulletsToDecrease, Weapon.WeaponModel thisWeaponModel)
    {
        switch (thisWeaponModel)
        {
            case Weapon.WeaponModel.M4:
                totalRifleAmmo -= bulletsToDecrease;
                break;
            case Weapon.WeaponModel.Pistol1911:
                totalPistolAmmo -= bulletsToDecrease;
                break;
            case Weapon.WeaponModel.PT9M:
                totalPistolAmmo -= bulletsToDecrease;
                break;
        }
    }
    public int CheckAmmoLeftFor(Weapon.WeaponModel thisWeaponModel)
    {
        switch (thisWeaponModel)
        {
            case Weapon.WeaponModel.M4:
                return totalRifleAmmo;

            case Weapon.WeaponModel.Pistol1911:
                return totalPistolAmmo;

            case Weapon.WeaponModel.PT9M:
                return totalPistolAmmo;

            default:
                return 0;
        }
    }
    public void DropWeapon(GameObject weaponDroped = null)
    {
        // Проверяем, есть ли оружие в активном слоте
        if (activeWeaponSlot.transform.childCount > 0)
        {
            GameObject weaponToDrop;
            // Получаем активное оружие
            if (weaponDroped == null)
            {
                weaponToDrop = activeWeaponSlot.transform.GetChild(0).gameObject;
            }
            else
            {
                weaponToDrop = weaponDroped;
                weaponToDrop.transform.SetParent(null);
            }

            // Отключаем активность оружия
            Weapon weaponComponent = weaponToDrop.GetComponent<Weapon>();
            weaponComponent.isActiveWeapon = false;
            weaponComponent.animator.enabled = false;

            // Отключаем ParentConstraint
            weaponToDrop.GetComponent<ParentConstraint>().enabled = false;

            // Включаем коллайдер для взаимодействия с окружением
            weaponToDrop.GetComponent<BoxCollider>().enabled = true;

            weaponToDrop.layer = 10;
            SetLayerRecursively(weaponToDrop.gameObject, 10);

            Rigidbody rb = weaponToDrop.GetComponent<Rigidbody>();
            rb.isKinematic = false;



            weaponToDrop.transform.rotation = Quaternion.Euler(0f, weaponToDrop.transform.rotation.eulerAngles.y, 0f);
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // Устанавливаем начальную позицию сброса чуть выше текущего положения
            Vector3 dropPosition = weaponToDrop.transform.position; // Левитация перед сбросом
            weaponToDrop.transform.SetParent(null);
            weaponToDrop.transform.position = dropPosition;
            float forwardForce = 1f;  // Сила вперед
            float upwardForce = 1f;
            Vector3 forceDirection = (player.transform.forward * forwardForce) + (Vector3.up * upwardForce);
            rb.AddForce(forceDirection, ForceMode.Impulse);

            //weaponComponent.StartLevitation();
            DeactivateConstraint(weaponToDrop);
        }
    }


    public GameObject GetActiveWeapon()
    {
        // Проверяем, есть ли оружие в активном слоте
        if (activeWeaponSlot != null && activeWeaponSlot.transform.childCount > 0)
        {
            // Возвращаем объект активного оружия
            return activeWeaponSlot.transform.GetChild(0).gameObject;
        }

        // Если оружия нет, возвращаем null
        return null;
    }

    public void ActivateConstraint(GameObject weapon)
    {
        ConstraintToWeapon.Instance.ActivateConstraint(player, weapon);
    }

    public void DeactivateConstraint(GameObject weapon)
    {
        ConstraintToWeapon.Instance.DeactivateConstraint(player, weapon);
    }
}
