using UnityEngine;

public class RuleBasedGemini1 : MonoBehaviour
{
    [Header("Referensi")]
    public Transform opponent;
    public Transform ball;
    public DodgeballManager manager; // Kita tetap perlu manajer untuk reset

    [Header("Pengaturan Gerakan")]
    public float moveSpeed = 12f;

    [Header("Pengaturan Bola")]
    public Transform holdPoint;
    public float throwForce = 50f;

    // Variabel Internal
    private Rigidbody ballRb;
    private Rigidbody agentRb;
    private BallControllerGPT ballController;
    private GameObject availableBall = null;
    private GameObject heldBall = null;
    private Vector3 startingPosition;
    private Quaternion startingRotation;

    // --- FUNGSI BAWAAN UNITY ---

    void Start()
    {
        // Salin dari Initialize()
        agentRb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();
        ballController = ball.GetComponent<BallControllerGPT>();

        // (Kita tidak perlu manager untuk grup, tapi mungkin untuk reset)
        // manager = GetComponentInParent<DodgeballManager>(); 

        startingPosition = transform.localPosition;
        startingRotation = transform.localRotation;
    }

    // "OTAK" DARI AI RULE-BASED ADA DI SINI
    void Update()
    {
        // Cek status kita saat ini
        if (heldBall == null)
        {
            // STATUS A: KITA TIDAK PEGANG BOLA
            State_SearchAndDodge();
        }
        else
        {
            // STATUS B: KITA PEGANG BOLA
            State_AimAndThrow();
        }
    }

    void FixedUpdate()
    {
        // Kita akan menggerakkan agen menggunakan velocity di dalam fungsi State
    }

    // --- FUNGSI STATUS (LOGIKA ATURAN) ---

    void State_SearchAndDodge()
    {
        // ATURAN #1: MENGHINDAR (PRIORITAS TERTINGGI)
        // Cek apakah bola "hidup" dan dipegang oleh lawan
        bool isBallDangerous = ballController.lastHolder != null && ballController.lastHolder.transform == opponent;
        
        if (isBallDangerous)
        {
            // Logika menghindar sederhana: bergerak ke samping (strafe)
            Vector3 strafeDirection = transform.right; // Selalu bergerak ke kanan
            agentRb.linearVelocity = new Vector3(strafeDirection.x * moveSpeed, agentRb.linearVelocity.y, strafeDirection.z * moveSpeed);
            return; // Jangan lakukan hal lain
        }

        // ATURAN #2: AMBIL BOLA
        // Jika bola ada di jangkauan (trigger) dan tidak berbahaya
        if (availableBall != null)
        {
            // Logika mengejar bola sederhana: bergerak lurus ke arah bola
            Vector3 directionToBall = (ball.position - transform.position).normalized;
            agentRb.linearVelocity = new Vector3(directionToBall.x * moveSpeed, agentRb.linearVelocity.y, directionToBall.z * moveSpeed);

            // Jika sudah cukup dekat, ambil
            if (Vector3.Distance(transform.position, ball.position) < 2f)
            {
                PickUpBall();
            }
            return;
        }
        
        // ATURAN #3: JAGA JARAK (JIKA BOLA DI TANGAN LAWAN)
        if (ballController.lastHolder == null && availableBall == null) // Berarti bola dipegang lawan
        {
            // Logika menjaga jarak: mundur jika lawan terlalu dekat
            if (Vector3.Distance(transform.position, opponent.position) < 8f)
            {
                Vector3 directionAway = (transform.position - opponent.position).normalized;
                agentRb.linearVelocity = new Vector3(directionAway.x * moveSpeed, agentRb.linearVelocity.y, directionAway.z * moveSpeed);
            }
            else
            {
                // Diam
                agentRb.linearVelocity = new Vector3(0, agentRb.linearVelocity.y, 0);
            }
            return;
        }

        // ATURAN #4: DIAM
        agentRb.linearVelocity = new Vector3(0, agentRb.linearVelocity.y, 0);
    }

    void State_AimAndThrow()
    {
        // ATURAN #1: SELALU LIHAT KE LAWAN
        Vector3 lookDirection = (opponent.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(lookDirection); // Langsung putar badan

        // ATURAN #2: JIKA SUDAH MENGARAH, LANGSUNG LEMPAR
        // Ini adalah AI "bodoh" yang akan terus melempar.
        // AI yang lebih pintar akan menunggu "waktu yang tepat"
        ThrowBall();
    }


    // --- FUNGSI HELPER (Salin dari DodgeballAgent) ---
    // (PENTING: Modifikasi OnCollisionEnter agar tidak memanggil manager)

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dodgeball"))
        {
            if (ballController.lastHolder != null && ballController.lastHolder != this)
            {
                // SAYA TERKENA!
                // Dalam skenario perbandingan, Manajer Game akan mendeteksi ini
                // dan mereset pertandingan.
                Debug.Log("Rule-Based Agent TERKENA!");
                
                // Di sini Anda akan memanggil manajer game Anda untuk mereset
                // manager.RuleBasedAgentDied();
            }
        }
        // Tidak perlu AddReward untuk tembok, biarkan fisika yang menghentikannya
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

    private void PickUpBall()
    {
        heldBall = availableBall;
        ballRb.isKinematic = true;
        ballCollider.enabled = false; // Jangan lupa matikan collider
        heldBall.transform.position = holdPoint.position;
        heldBall.transform.parent = holdPoint;
        ballController.lastHolder = null;
    }

    private void ReleaseBall()
    {
        if (heldBall == null) return;
        heldBall.transform.parent = null; 
        ballRb.isKinematic = false;
        ballCollider.enabled = true; // Nyalakan lagi collider
        heldBall = null;
    }
    
    private void ThrowBall()
    {
        ReleaseBall(); 
        ballRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
    }

    // Tambahkan variabel ballCollider di atas
    private SphereCollider ballCollider;
    // Dan tambahkan ini di Start()
    // ballCollider = ball.GetComponent<SphereCollider>();
}