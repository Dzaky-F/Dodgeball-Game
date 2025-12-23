using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AgentMovementCopy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Cek apakah agen menyentuh tanah
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Input manual (bisa nanti diganti oleh ML-Agent)
        float moveX = Input.GetAxis("Horizontal"); // kiri-kanan (A-D)
        float moveZ = Input.GetAxis("Vertical");   // maju-mundur (W-S)

        Vector3 move = new Vector3(moveX, 0f, moveZ).normalized;

        // Gerakan translasi
        Vector3 moveDirection = transform.TransformDirection(move) * moveSpeed;
        rb.MovePosition(rb.position + moveDirection * Time.deltaTime);

        // Loncat
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
