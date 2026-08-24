using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMovent : NetworkBehaviour
{
    public float speed = 5f;

    // Public getter para ma-access ng PlayerShoot
    public Vector3 LastMoveDirection { get; private set; } = Vector3.forward;

    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        if (IsOwner)
        {
            Renderer playerRenderer = GetComponent<Renderer>();
            if (playerRenderer != null)
            {
                playerRenderer.material.color = Color.red;
            }
        }
    }

    private void Update()
    {
        if (!IsOwner || !IsSpawned)
            return;
        if (Keyboard.current == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed)
            horizontal = -1f;
        if (Keyboard.current.dKey.isPressed)
            horizontal = 1f;
        if (Keyboard.current.sKey.isPressed)
            vertical = -1f;
        if (Keyboard.current.wKey.isPressed)
            vertical = 1f;

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // I-update lang kapag may input, para hindi mawala yung facing direction pag tumigil sa paggalaw
        if (direction.sqrMagnitude > 0.001f)
        {
            LastMoveDirection = direction;
        }

        transform.position += direction * speed * Time.deltaTime;
    }
}