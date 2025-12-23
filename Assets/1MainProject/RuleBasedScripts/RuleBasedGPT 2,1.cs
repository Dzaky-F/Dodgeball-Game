using UnityEngine;

public class RuleBasedGPT2_1 : MonoBehaviour
{
    [Header("Referensi")]
    public Transform opponent;
    public Transform ball;

    [Header("Pengaturan Gerakan")]
    public float moveSpeed = 10f;
    public float avoidSpeed = 8f;

    [Header("Pengaturan Bola")]
    public Transform holdPoint;
    public float throwForce = 50f;

    private Rigidbody rb;
    private Rigidbody ballRb;
    private SphereCollider ballCollider;
    private BallControllerGPT ballController;
    private GameObject heldBall = null;
    private GameObject availableBall = null;
    private bool amIBlue;
    private DodgeballManager manager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();
        ballCollider = ball.GetComponent<SphereCollider>();
        ballController = ball.GetComponent<BallControllerGPT>();
        manager = GetComponentInParent<DodgeballManager>();

        if (transform.localPosition.x < 0)
            amIBlue = true;
        else
            amIBlue = false;
    }

    void Update()
    {
        RuleBasedBehavior();
    }

    void RuleBasedBehavior()
    {
        Vector3 targetDirection = Vector3.zero;
        float distanceToBall = Vector3.Distance(transform.position, ball.position);
        float distanceToOpponent = Vector3.Distance(transform.position, opponent.position);

        bool ballFree = (ballController.lastHolder == null);
        bool opponentHasBall = (ballController.lastHolder != null && ballController.lastHolder.transform == opponent);
        bool iHaveBall = (heldBall != null);

        // --- LOGIKA DASAR ---
        if (iHaveBall)
        {
            // kalau sudah pegang bola, dekati lawan
            targetDirection = (opponent.position - transform.position).normalized;

            // lempar kalau udah cukup dekat
            if (distanceToOpponent < 6f)
            {
                ThrowBall();
            }
        }
        else if (ballFree)
        {
            // kalau bola bebas di sisi sendiri, kejar bola
            if ((amIBlue && ball.localPosition.x < 0) || (!amIBlue && ball.localPosition.x > 0))
            {
                targetDirection = (ball.position - transform.position).normalized;
            }
            else
            {
                // kalau bola di sisi lawan, tetap siaga dekat tengah
                if (amIBlue)
                    targetDirection = (new Vector3(-2f, transform.position.y, transform.position.z) - transform.position).normalized;
                else
                    targetDirection = (new Vector3(2f, transform.position.y, transform.position.z) - transform.position).normalized;
            }
        }
        else if (opponentHasBall)
        {
            // kalau lawan pegang bola → coba hindar ke samping
            float sideDir = (Random.value > 0.5f) ? 1f : -1f;
            targetDirection = new Vector3(sideDir, 0f, 1f).normalized;
        }

        // --- Gerakan ---
        Vector3 moveVector = new Vector3(targetDirection.x, 0f, targetDirection.z) * moveSpeed;
        rb.linearVelocity = new Vector3(moveVector.x, rb.linearVelocity.y, moveVector.z);

        // --- Ambil bola kalau dekat ---
        if (!iHaveBall && availableBall != null && distanceToBall < 2f && ballFree)
        {
            PickUpBall();
        }
    }

    private void PickUpBall()
    {
        heldBall = availableBall;
        ballRb.isKinematic = true;
        ballCollider.enabled = false;
        heldBall.transform.position = holdPoint.position;
        heldBall.transform.parent = holdPoint;
        ballController.lastHolder = null; // reset supaya logika jelas
        ballController.lastHolder = null; // pastikan null dulu
        ballController.lastHolder = GetComponent<DodgeballAgentGPT>(); // optional, biar sama sistem
    }

    private void ThrowBall()
    {
        if (heldBall == null) return;
        heldBall.transform.parent = manager.transform;
        ballRb.isKinematic = false;
        ballCollider.enabled = true;
        ballRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        heldBall = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dodgeball"))
        {
            availableBall = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Dodgeball") && other.gameObject == availableBall)
        {
            availableBall = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dodgeball"))
        {
            if (ballController.lastHolder != null && ballController.lastHolder != this)
            {
                // kena bola dari lawan
                Debug.Log($"{name} kena bola, kalah!");
                manager.ResetBall();
            }
        }
    }
}
