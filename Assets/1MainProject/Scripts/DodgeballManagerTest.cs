using UnityEngine;
using Unity.MLAgents;
using System.Collections.Generic;

public class DodgeballManagerTest : MonoBehaviour
{
    public Transform ball;

    private Rigidbody ballRb;
    private Vector3 ballStartingPosition;

    public int rulebBasedWins = 0;
    public int drlWins = 0;

    private double winrateDRL;

    void Awake()
    {
        ballRb = ball.GetComponent<Rigidbody>();
        ballStartingPosition = ball.localPosition;
    }
    
    public void ResetBall()
    {
        ball.localPosition = ballStartingPosition + new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-2f, 2f));
        
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        ball.GetComponent<BallController>().lastHolder = null;
        ball.transform.parent = this.transform; // Jadikan anak dari manager agar terlepas
    }

    public void printScore()
    {
        winrateDRL = drlWins / (drlWins + rulebBasedWins);
        Debug.Log("DRL: " + drlWins + "  Rule Based : " + rulebBasedWins + ", winrate DRL : " + winrateDRL + "%");
    }
}