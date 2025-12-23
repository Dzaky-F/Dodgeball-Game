//23 ini yang best, dipake
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class DodgeballAgentGPT : Agent
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
    private bool pickupInputFlag = false;
    private bool throwInputFlag = false;
    private BallControllerGPT ballController;
    private SphereCollider ballCollider;

    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private DodgeballManager manager;
    private int ballStatus;

    private bool amIBlue;

    private float timeAlive;
    private Vector3 lastPosition;
    private float lastDistanceToBall;
    private float lastDistanceToOpponent;

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            pickupInputFlag = true;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            throwInputFlag = true;
        }
    }

    public override void Initialize()
    {
        agentRb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();
        ballCollider = ball.GetComponent<SphereCollider>();
        ballController = ball.GetComponent<BallControllerGPT>();
        manager = GetComponentInParent<DodgeballManager>();

        startingPosition = transform.localPosition;
        startingRotation = transform.localRotation;

        if (startingPosition.x < 0) amIBlue = true;
        else amIBlue = false;
    }

    public override void OnEpisodeBegin()
    {
        ReleaseBall();
        transform.localPosition = startingPosition + new Vector3(Random.Range(-2f,2f), 0f, Random.Range(-1.5f,1.5f));
        transform.localRotation = startingRotation;
        agentRb.linearVelocity = Vector3.zero;
        manager.ResetBall();

        lastPosition = transform.position;
        lastDistanceToBall = Vector3.Distance(transform.position, ball.position);
        lastDistanceToOpponent = Vector3.Distance(transform.position, opponent.position);
        timeAlive = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(opponent.localPosition.x);
        sensor.AddObservation(opponent.localPosition.z);
        sensor.AddObservation(ball.localPosition.x);
        sensor.AddObservation(ball.localPosition.z);
        sensor.AddObservation(ballRb.linearVelocity.x);
        sensor.AddObservation(ballRb.linearVelocity.z); 
        sensor.AddObservation(heldBall != null); 

        if (ballController.lastHolder == null) ballStatus = 0;
        else if (ballController.lastHolder == this) ballStatus = 1;
        else ballStatus = 2;
        sensor.AddObservation(ballStatus);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveX = actions.DiscreteActions[0];
        int moveZ = actions.DiscreteActions[1];
        Vector3 addForce = new Vector3(0, 0, 0);
        switch (moveX) 
        {
            case 0: addForce.x = 0f; break;
            case 1: addForce.x = -1f; break;
            case 2: addForce.x = 1f; break;
        }
        switch (moveZ) 
        {
            case 0: addForce.z = 0f; break;
            case 1: addForce.z = -1f; break;
            case 2: addForce.z = 1f; break;
        }
        agentRb.linearVelocity = addForce.normalized * moveSpeed + new Vector3(0, agentRb.linearVelocity.y, 0);

        int pickupAction = actions.DiscreteActions[2];
        int throwAction = actions.DiscreteActions[3];

        if (pickupAction == 1 && availableBall != null && heldBall == null && ballStatus == 0)
            PickUpBall();

        if (throwAction == 1 && heldBall != null)
            ThrowBall();

        timeAlive += Time.fixedDeltaTime;
        float distanceToBall = Vector3.Distance(transform.position, ball.position);    

        if (IsMoving()) AddReward(+0.001f);
        else AddReward(-0.001f);

        if (timeAlive % 3f < Time.fixedDeltaTime)
            AddReward(+0.01f);

        // (2) Reward mendekati bola
        if (amIBlue) {
            if (ball.localPosition.x <= 0 && distanceToBall < lastDistanceToBall && heldBall == null)
                AddReward(0.005f);
        } else {
            if (ball.localPosition.x >= 0 && distanceToBall < lastDistanceToBall && heldBall == null)
                AddReward(0.005f);
        }

        // (3) Penalti menjauh dari bola tanpa alasan
        if (amIBlue) {
            if (ball.localPosition.x <= 0 && distanceToBall > lastDistanceToBall && heldBall == null)
                AddReward(-0.005f);
        } else {
            if (ball.localPosition.x >= 0 && distanceToBall > lastDistanceToBall && heldBall == null)
                AddReward(-0.005f);
        }

        // Reward saat memegang bola dan mendekati musuh
        float distanceToOpponent = Vector3.Distance(transform.position, opponent.position);
        if (heldBall != null) {
            if (distanceToOpponent <= lastDistanceToOpponent) 
                AddReward(0.01f);
            else 
                AddReward(-0.01f);
        }

        // Reward jika berusaha menghindar saat musuh memegang bola
        if (ballStatus == 2) {
            if (distanceToOpponent >= lastDistanceToOpponent)
                AddReward(0.01f);
            else 
                AddReward(-0.01f);
        }
            


        lastPosition = transform.position;
        lastDistanceToBall = distanceToBall;
        lastDistanceToOpponent = distanceToOpponent;
    }

    private bool IsMoving()
    {
        return Vector3.Distance(transform.position, lastPosition) > 0.01f;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;
        discreteActions[1] = 0;

        if (Input.GetKey(KeyCode.A)) 
        {
            discreteActions[0] = 1;
        }
        else if (Input.GetKey(KeyCode.D)) 
        {
            discreteActions[0] = 2;
        }  
        if (Input.GetKey(KeyCode.S)) 
        {
            discreteActions[1] = 1;
        }
        else if (Input.GetKey(KeyCode.W)) 
        {
            discreteActions[1] = 2;
        }

        if (pickupInputFlag)
        {
            discreteActions[2] = 1;
            pickupInputFlag = false;
        }
        else
        {
            discreteActions[2] = 0;
        }

        if (throwInputFlag)
        {
            discreteActions[3] = 1;
            throwInputFlag = false;
        }
        else
        {
            discreteActions[3] = 0;
        }
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
        ballController.lastHolder = this;

        AddReward(0.1f);
    }

    private void ThrowBall()
    {
        ReleaseBall();
        ballRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        AddReward(-0.02f);
        if (Mathf.Abs(transform.position.z - opponent.position.z) <= 0.5)
            AddReward(0.15f);
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
            if (ballController.lastHolder != null && ballController.lastHolder != this)
            {
                AddReward(-1f);
                ballController.lastHolder.AddReward(1f);
                ballController.lastHolder.EndEpisode();
                EndEpisode();
            }
        }
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f);
        }
    }

    public void AddOpponentReward(float reward)
    {
        opponent.GetComponent<DodgeballAgentGPT>().AddReward(reward);
    }
}