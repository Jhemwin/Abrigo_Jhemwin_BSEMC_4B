using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerShoot : NetworkBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float fireRate = 0.25f;
    public float bulletLifeTime = 3f;

    private float lastFireTime = -999f;
    private PlayerMovent playerMovent;

    private void Awake()
    {
        playerMovent = GetComponent<PlayerMovent>();
    }

    private void Update()
    {
        if (!IsOwner || !IsSpawned) return;
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= lastFireTime + fireRate)
        {
            lastFireTime = Time.time;
            Vector3 shootDirection = playerMovent != null ? playerMovent.LastMoveDirection : transform.forward;
            ShootServerRpc(firePoint.position, shootDirection);
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        GameObject bulletInstance = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(direction));
        bulletInstance.GetComponent<NetworkObject>().Spawn(true);

        Bullet bulletScript = bulletInstance.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(direction, bulletSpeed, bulletLifeTime);
        }
    }
}