using UnityEngine;

public class RuleBasedGPT1_2 : MonoBehaviour
{
    [Header("References")]
    public Transform ball;
    public Transform enemyAgent;
    public Transform throwPoint;
    public Rigidbody ballRb;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float throwForce = 8f;
    public float detectionRange = 10f;
    public float throwRange = 8f;

    [Header("Arena Settings")]
    public bool isLeftSide = true; // set di Inspector: true = kiri, false = kanan
    public float arenaHalfBoundary = 0f; // posisi tengah arena (misal x = 0)

    private bool hasBall = false;

    void Update()
    {
        if (ball == null || enemyAgent == null) return;

        // Cek apakah sedang memegang bola
        hasBall = IsHoldingBall();

        if (!hasBall)
        {
            MoveTowardsBall();
        }
        else
        {
            MoveToThrowPosition();
            TryThrow();
        }
    }

    bool IsHoldingBall()
    {
        // Anggap agen memegang bola jika jarak ke bola sangat kecil
        return Vector3.Distance(transform.position, ball.position) < 1.0f;
    }

    void MoveTowardsBall()
    {
        // Hindari keluar dari wilayah sendiri
        if (isLeftSide && transform.position.x > arenaHalfBoundary - 0.5f) 
            transform.position = new Vector3(arenaHalfBoundary - 0.5f, transform.position.y, transform.position.z);
        else if (!isLeftSide && transform.position.x < arenaHalfBoundary + 0.5f) 
            transform.position = new Vector3(arenaHalfBoundary + 0.5f, transform.position.y, transform.position.z);

        // Gerak menuju bola kalau masih di wilayah sendiri
        Vector3 target = new Vector3(ball.position.x, transform.position.y, ball.position.z);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    void MoveToThrowPosition()
    {
        // Dekat ke garis tengah biar lemparan efektif
        float targetX = isLeftSide ? arenaHalfBoundary - 0.5f : arenaHalfBoundary + 0.5f;
        Vector3 targetPos = new Vector3(targetX, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // Hadap ke lawan
        Vector3 lookDir = enemyAgent.position - transform.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);
    }

    void TryThrow()
    {
        // Cek kalau sejajar atau cukup dekat
        float zDiff = Mathf.Abs(transform.position.z - enemyAgent.position.z);
        float distance = Vector3.Distance(transform.position, enemyAgent.position);

        if (zDiff < 1.0f && distance < throwRange)
        {
            ThrowBall();
        }
    }

    void ThrowBall()
    {
        if (ballRb == null) return;

        Vector3 throwDir = (enemyAgent.position - throwPoint.position).normalized;
        ballRb.isKinematic = false;
        ballRb.transform.position = throwPoint.position;
        ballRb.linearVelocity = throwDir * throwForce;

        Debug.Log($"{gameObject.name} melempar bola!");
    }
}
