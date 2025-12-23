using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AgentMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    
    [Header("Dodgeball Settings")]
    public Transform holdPoint;          // Titik di mana bola akan dipegang
    public float throwForce = 20f;       // Kekuatan lemparan

    private Rigidbody rb;
    private GameObject availableBall = null; // Bola yang ada di jangkauan
    private GameObject heldBall = null;      // Bola yang sedang dipegang

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && availableBall != null && heldBall == null)
        {
            PickUpBall();
        }

        if (Input.GetMouseButtonDown(0) && heldBall != null)
        {
            ThrowBall();
        }
    }

    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal") * -1;
        float verticalInput = Input.GetAxisRaw("Vertical") * -1;
        Vector3 movementDirection = new Vector3(horizontalInput, 0.0f, verticalInput).normalized;
        rb.linearVelocity = new Vector3(movementDirection.x * moveSpeed, rb.linearVelocity.y, movementDirection.z * moveSpeed);  
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("dodgeBall"))
        {
            availableBall = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("dodgeBall") && other.gameObject == availableBall)
        {
            availableBall = null; 
        }
    }

    void PickUpBall()
    {
        heldBall = availableBall;
        Rigidbody ballRb = heldBall.GetComponent<Rigidbody>();
        ballRb.isKinematic = true;
        heldBall.transform.position = holdPoint.position; 
        heldBall.transform.parent = holdPoint;            
    }

    void ThrowBall()
    {
        Rigidbody ballRb = heldBall.GetComponent<Rigidbody>();
        heldBall.transform.parent = null;
        ballRb.isKinematic = false;
        ballRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        heldBall = null;
    }
}