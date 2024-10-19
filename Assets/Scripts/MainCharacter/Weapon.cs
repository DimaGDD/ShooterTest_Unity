using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Weapon : MonoBehaviour
{
    public bool isActiveWeapon;

    [Header("Shooting")]
    // Shooting
    public bool isShooting, readyToShoot;
    private bool allowReset = true;
    public float shootingDelay = 2f;

    [Header("Burst")]
    // Burst
    public int bulletsPerBurst = 3;
    public int burstBulletsLeft;

    [Header("Spread")]
    // Spread
    public float spreadIntensity;
    public float hipSpreadIntensity;
    public float adsSpreadIntensity;

    [Header("Bullet")]
    // Bullet
    public GameObject bulletPrefab;
    public float bulletVelocity = 30;
    public float bulletPrefabLifeTime = 3f;

    public float holeFromBulletLifeTime = 10f;

    public GameObject muzzleEffect;
    internal Animator animator;

    [Header("ADS Speed")]
    public float ADSSpeed = 1f;
    private bool isADS;

    private Coroutine levitationCoroutine;



    [Header("Reloading")]
    // Reloading
    public float reloadTime;
    public int magazineSize, bulletsLeft;
    public bool isReloading;

    [Header("Position In Right Hand Idle")]
    public Vector3 spawnPositionInRightHand;
    public Vector3 spawnRotationInRightHand;

    [Header("Position In Left Hand Idle")]
    public Vector3 spawnPositionInLeftHand;
    public Vector3 spawnRotationInLeftHand;

    [Header("Camera Offset With Weapon")]
    public Vector3 cameraOffsetPosition;
    public Vector3 cameraOffsetRotation;

    [Header("Position In Right Hand ADS")]
    public Vector3 spawnPositionInRightHandADS;
    public Vector3 spawnRotationInRightHandADS;

    [Header("Position In Left Hand ADS")]
    public Vector3 spawnPositionInLeftHandADS;
    public Vector3 spawnRotationInLeftHandADS;

    [Header("Fingers")]
    public GameObject[] targetsForTargetsForRightFingers;
    public GameObject[] targetsForTargetsForLeftFingers;

    public enum WeaponModel
    {
        Pistol1911,
        M4,
        PT9M
    }

    public WeaponModel thisWeaponModel;

    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }

    public ShootingMode currentShootingMode;

    private void Awake()
    {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        animator = GetComponent<Animator>();

        bulletsLeft = magazineSize;

        spreadIntensity = hipSpreadIntensity;
    }

    private void Update()
    {
        if (isActiveWeapon)
        {
            if (Input.GetMouseButtonDown(1))
            {
                EnterADS();
            }

            if (Input.GetMouseButtonUp(1))
            {
                ExitADS();
            }

            GetComponent<Outline>().enabled = false;

            if (currentShootingMode == ShootingMode.Auto)
            {
                // Holding Down Left Mouse Button
                isShooting = Input.GetKey(KeyCode.Mouse0);
            }
            else if (currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst)
            {
                // Clicking Left Mouse Button Once
                isShooting = Input.GetKeyDown(KeyCode.Mouse0);
            }

            if (readyToShoot && isShooting && bulletsLeft > 0 && isReloading == false)
            {
                burstBulletsLeft = bulletsPerBurst;
                FireWeapon();
            }

            if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && isReloading == false && WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > 0)
            {
                Reload();
            }

            // if you want to automatically reload when magazine is empty
            if (readyToShoot && isReloading == false && bulletsLeft <= 0)
            {
                //Reload();
            }

            if (bulletsLeft == 0 && isShooting)
            {
                SoundManager.Instance.emptyMagazineSound1911.Play();
            }
        }
    }

    private void CreateBulletImpactEffect(RaycastHit hit)
    {
        // Создаем эффект попадания
        GameObject hole = Instantiate(
            GlobalReferences.Instance.bulletImpactEffectPrefab,
            hit.point,
            Quaternion.LookRotation(hit.normal)
        );

        // Удаляем эффект через 2 секунды
        Destroy(hole, holeFromBulletLifeTime);
    }

    private void EnterADS()
    {
        HUDManager.Instance.middleDot.SetActive(false);

        StartCoroutine(EnterADSCoroutine());
    }

    private IEnumerator EnterADSCoroutine()
    {
        // Ждем завершения EnterADS
        yield return StartCoroutine(ConstraintToWeapon.Instance.EnterADS(spawnPositionInRightHandADS, spawnRotationInRightHandADS, spawnPositionInLeftHandADS, spawnRotationInLeftHandADS, ADSSpeed, isADS, gameObject));

        // Устанавливаем spreadIntensity после завершения анимации прицеливания
        isADS = true;
        spreadIntensity = adsSpreadIntensity;
    }

    private void ExitADS()
    {
        isADS = false;
        HUDManager.Instance.middleDot.SetActive(true);
        spreadIntensity = hipSpreadIntensity;
        ConstraintToWeapon.Instance.ExitADS(spawnPositionInRightHand, spawnRotationInRightHand, spawnPositionInLeftHand, spawnRotationInLeftHand, ADSSpeed, isADS, gameObject);
    }

    private void FireWeapon()
    {
        bulletsLeft--;

        muzzleEffect.GetComponent<ParticleSystem>().Play();

        SoundManager.Instance.PlayShootingSound(thisWeaponModel);

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        RaycastHit hit;

        if (Physics.Raycast(WeaponManager.Instance.bulletSpawn.position, shootingDirection, out hit, 100f)) // Замените 100f на вашу дистанцию стрельбы
        {
            // Создаем эффект попадания
            CreateBulletImpactEffect(hit);

            // Вы можете также добавить логику, чтобы повредить цель, если это необходимо
            if (hit.collider.CompareTag("Target"))
            {
                print("hit " + hit.collider.name + " !");
            }

            if (hit.collider.CompareTag("Wall"))
            {
                print("hit a wall");
            }
        }

        // Checking if we are done shooting
        if (allowReset)
        {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }

        // Burst Mode
        if (currentShootingMode == ShootingMode.Burst && burstBulletsLeft > 1) // we already shoot once before this check
        {
            burstBulletsLeft--;
            Invoke("FireWeapon", shootingDelay);
        }
    }

    private void Reload()
    {
        SoundManager.Instance.PlayReloadSound(thisWeaponModel);
        animator.SetTrigger("RELOAD");

        isReloading = true;
        Invoke("ReloadCompleted", reloadTime);
    }

    private void ReloadCompleted()
    {
        int requiredBullets = magazineSize - bulletsLeft;
        int availableBullets = WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel);

        if (requiredBullets <= availableBullets)
        {
            bulletsLeft += requiredBullets;
            WeaponManager.Instance.DecreaseTotalAmmo(requiredBullets, thisWeaponModel);
        }
        else
        {
            bulletsLeft += availableBullets;
            WeaponManager.Instance.DecreaseTotalAmmo(availableBullets, thisWeaponModel);
        }

        isReloading = false;
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    public Vector3 CalculateDirectionAndSpread()
    {
        // Shooting from the middle of the screen to check where are we pointing at 
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            // Hitting Something
            targetPoint = hit.point;
        }
        else
        {
            // Shooting at the air
            targetPoint = ray.GetPoint(100);
        }

        Vector3 direction = targetPoint - WeaponManager.Instance.bulletSpawn.position;
        float distanceToTarget = direction.magnitude;
        direction.Normalize();

        float z = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        // Returning the shooting direction and spread
        return direction + new Vector3(0, y, z);
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }

    public void StartLevitation()
    {
        // Запускаем корутину для эффекта левитации
        if (levitationCoroutine == null)
        {
            levitationCoroutine = StartCoroutine(LevitateWeapon());
        }
    }

    private IEnumerator LevitateWeapon()
    {
        Vector3 startPosition = transform.position;
        float levitationHeight = 0.1f; // Высота левитации
        float levitationSpeed = 2f; // Скорость левитации

        while (true)
        {
            // Плавное изменение высоты с использованием синусоиды
            float newY = startPosition.y + Mathf.Sin(Time.time * levitationSpeed) * levitationHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);

            yield return null; // Ждём следующий кадр
        }
    }

    public void StopLevitation()
    {
        if (levitationCoroutine != null)
        {
            StopCoroutine(levitationCoroutine);
            levitationCoroutine = null;
        }
    }
}
