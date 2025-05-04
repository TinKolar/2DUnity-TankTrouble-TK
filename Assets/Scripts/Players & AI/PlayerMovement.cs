using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public enum PlayerNumber { Player1, Player2 }

    [Header("Movement Settings")]
    public PlayerNumber playerNumber;
    public float moveSpeed = 5f;
    public float rotationSpeed = 200f;
    public float reverseSpeedMultiplier = 0.6f;

    [Header("Components")]
    private Rigidbody2D tankRb;

    private KeyCode forwardKey = KeyCode.W;
    private KeyCode backwardKey = KeyCode.S;
    private KeyCode leftKey = KeyCode.A;
    private KeyCode rightKey = KeyCode.D;
    [HideInInspector]
    public KeyCode shootKey = KeyCode.Q;


    private void Awake()
    {
        tankRb = GetComponent<Rigidbody2D>();
        SetupControls();
        SpawnManager.RegisterPlayer(gameObject);
        GameFinishManager.RegisterTank(gameObject);
    }


    private void SetupControls()
    {
        if (playerNumber == PlayerNumber.Player2)
        {
            forwardKey = KeyCode.UpArrow;
            backwardKey = KeyCode.DownArrow;
            leftKey = KeyCode.LeftArrow;
            rightKey = KeyCode.RightArrow;
            shootKey = KeyCode.Space;
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float moveInput = 0f;
        float rotationInput = 0f;

        // Forward/Backward
        if (Input.GetKey(forwardKey)) moveInput = 1f;
        else if (Input.GetKey(backwardKey)) moveInput = -1f;

        // Rotation
        if (Input.GetKey(leftKey)) rotationInput = 1f;
        else if (Input.GetKey(rightKey)) rotationInput = -1f;

        // Apply movement
        float currentSpeed = moveInput > 0 ? moveSpeed : moveSpeed * reverseSpeedMultiplier;
        tankRb.velocity = transform.up * moveInput * currentSpeed;

        // Apply rotation
        transform.Rotate(Vector3.forward * rotationInput * rotationSpeed * Time.deltaTime);
    }

    private void OnDestroy()
    {
        GameFinishManager.UnregisterTank(gameObject);

    }
}