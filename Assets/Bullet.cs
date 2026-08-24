using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifeTime;
    private float spawnTime;

    [Header("Bounce Settings")]
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private float bounceOffset = 0.05f;

    private int bounceCount = 0;

    // =========================================================
    // INITIALIZE
    // =========================================================

    public void Initialize(Vector3 dir, float bulletSpeed, float life)
    {
        direction = dir.normalized;
        speed = bulletSpeed;
        lifeTime = life;

        spawnTime = Time.time;
        bounceCount = 0;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Server lang ang responsible sa bullet movement/collision
        if (!IsServer)
            return;

        // Lifetime
        if (Time.time >= spawnTime + lifeTime)
        {
            DespawnBullet();
            return;
        }

        // Movement ngayong frame
        Vector3 movement = direction * speed * Time.deltaTime;

        // -----------------------------------------------------
        // DEBUG RAY
        // -----------------------------------------------------

        Debug.DrawRay(
            transform.position,
            direction * 3f,
            Color.red
        );

        // -----------------------------------------------------
        // RAYCAST
        // -----------------------------------------------------

        if (Physics.Raycast(
            transform.position,
            direction,
            out RaycastHit hit,
            movement.magnitude,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        ))
        {
            Debug.Log(
                "BULLET HIT: " +
                hit.collider.name +
                " | TAG: " +
                hit.collider.tag
            );

            // =================================================
            // WALL
            // =================================================

            if (hit.collider.CompareTag("Wall"))
            {
                Debug.Log("WALL DETECTED!");

                Bounce(hit);
                return;
            }

            // =================================================
            // PLAYER
            // =================================================

            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log(
                    "PLAYER HIT: " +
                    hit.collider.name
                );

                // Damage logic dito later

                DespawnBullet();
                return;
            }

            // =================================================
            // OTHER OBJECT
            // =================================================

            Debug.Log(
                "Bullet hit another object: " +
                hit.collider.name
            );

            DespawnBullet();
            return;
        }

        // -----------------------------------------------------
        // WALANG TINAMAAN
        // -----------------------------------------------------

        transform.position += movement;
    }

    // =========================================================
    // BOUNCE
    // =========================================================

    private void Bounce(RaycastHit hit)
    {
        // Check maximum bounce
        if (bounceCount >= maxBounces)
        {
            Debug.Log("Maximum bounces reached.");

            DespawnBullet();
            return;
        }

        // -----------------------------------------------------
        // MOVE TO COLLISION POINT
        // -----------------------------------------------------

        transform.position =
            hit.point +
            hit.normal * bounceOffset;

        // -----------------------------------------------------
        // REFLECT BULLET
        // -----------------------------------------------------

        direction = Vector3.Reflect(
            direction,
            hit.normal
        ).normalized;

        // -----------------------------------------------------
        // ROTATE BULLET
        // -----------------------------------------------------

        if (direction != Vector3.zero)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }

        // Add bounce count
        bounceCount++;

        Debug.Log(
            "BULLET BOUNCED! " +
            "Bounce: " +
            bounceCount +
            "/" +
            maxBounces
        );

        Debug.Log(
            "New Direction: " +
            direction
        );
    }

    // =========================================================
    // DESPAWN
    // =========================================================

    private void DespawnBullet()
    {
        NetworkObject networkObject =
            GetComponent<NetworkObject>();

        if (networkObject != null &&
            networkObject.IsSpawned)
        {
            networkObject.Despawn(true);
        }
    }
}