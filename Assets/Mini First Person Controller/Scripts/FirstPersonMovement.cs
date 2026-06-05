using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    Rigidbody rb;

    /// <summary> Overrides de velocidad. Se usa el último agregado. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Determinar si está corriendo
        IsRunning = canRun && Input.GetKey(runningKey);

        // Velocidad objetivo
        float targetSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
            targetSpeed = speedOverrides[speedOverrides.Count - 1]();

        // Input WASD
        Vector2 input = new Vector2(
            Input.GetAxis("Horizontal") * targetSpeed,
            Input.GetAxis("Vertical")   * targetSpeed
        );

        if (input.magnitude > 1f)
            input.Normalize();

        input *= targetSpeed;

        // Aplicar movimiento manteniendo la velocidad Y del Rigidbody (gravedad)
        rb.linearVelocity = transform.rotation * new Vector3(input.x, rb.linearVelocity.y, input.y);
    }
}