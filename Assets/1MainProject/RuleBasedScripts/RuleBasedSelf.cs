using UnityEngine;
using System.Collections;

public class RuleBasedSelf : MonoBehaviour
{
    [Header("Referensi")]
    public Transform opponent;
    public Transform ball;

    [Header("Pengaturan Gerakan")]
    public float moveSpeed = 12f;

    [Header("Pengaturan Bola")]
    public Transform holdPoint;
    public float throwForce = 50f;

    private Rigidbody ballRb;
    private Rigidbody agentRb;
    private GameObject availableBall = null;
    private GameObject heldBall = null;
    private BallController ballController;
    private SphereCollider ballCollider;

    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private DodgeballManagerTest manager;
    private int ballStatus;

    private bool dodgeMode = false;
    private Vector3 dodgeTarget;
    private float time = 0f;

    void Start()
    {
        agentRb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();
        ballCollider = ball.GetComponent<SphereCollider>();
        ballController = ball.GetComponent<BallController>();
        manager = GetComponentInParent<DodgeballManagerTest>();

        startingPosition = transform.localPosition;
        startingRotation = transform.localRotation;
    }

    public void Reset()
    {
        ReleaseBall();
        transform.localPosition = startingPosition + new Vector3(Random.Range(-2f,2f), 0f, Random.Range(-1.5f,1.5f));
        transform.localRotation = startingRotation;
        agentRb.linearVelocity = Vector3.zero;
        manager.ResetBall();
        dodgeMode = false;
    }


    void Update()
    {
        if (ballController.lastHolder == null) ballStatus = 0;
        else if (heldBall != null) ballStatus = 1;
        else ballStatus = 2;

        if (ballStatus == 0 && ball.position.x >= 0)
        {
            MoveTowardsBall();
            
            if (availableBall != null)
                PickUpBall();
        }        

        if (ballStatus == 1)
        {
            time += Time.deltaTime;
            MoveTowardsOpponent();

            if ((time >= 1.5f) || Mathf.Abs(transform.position.z - opponent.position.z) <= 0.35)
                ThrowBall();
        }

        if (dodgeMode == false && (ballStatus == 2 || (ballStatus == 0 && ball.localPosition.x < 0)))
        {
            dodgeMode = true;
            StartCoroutine(DodgeMovement());
        }

        if (dodgeMode == true)
            transform.position = Vector3.MoveTowards(transform.position, dodgeTarget, moveSpeed * Time.deltaTime);      
    }

    private IEnumerator DodgeMovement()
    {
        while (ballStatus == 2 || (ballStatus == 0 && ball.localPosition.x < 0))
        {
            dodgeTarget = new Vector3(Random.Range(1.5f, 11.3f), transform.position.y, Random.Range(4f, 12.2f));

            // if(ballStatus == 2)
            // {
            //     dodgeTarget.x = 11.5f;
            //     if (ball.position.z > 8.5) dodgeTarget.z = 4.15f;
            //     else dodgeTarget.z = 12.45f;
            // }
                
            yield return new WaitForSeconds(1.1f);          
        }

        dodgeMode = false;
        yield return null;
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
        ballCollider.enabled = false;
        heldBall.transform.position = holdPoint.position;
        heldBall.transform.parent = holdPoint;
        ballController.lastHolder = this.transform;

        time = 0f;
    }

    private void ThrowBall()
    {
        ReleaseBall();
        ballRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
    }

    private void ReleaseBall()
    {
        if (heldBall == null) return; 
        heldBall.transform.parent = manager.transform;
        ballRb.isKinematic = false;
        ballCollider.enabled = true;
        heldBall = null; 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dodgeball"))
        {
            if (ballStatus == 2)
            {
                //DIE
                manager.drlWins++;
                manager.printScore();
                Reset();
            }
        }
    }

    void MoveTowardsBall()
    {
        Vector3 target = new Vector3(ball.position.x, transform.position.y, ball.position.z);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    void MoveTowardsOpponent()
    {
        Vector3 target = new Vector3(opponent.position.x, transform.position.y, opponent.position.z);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }
}