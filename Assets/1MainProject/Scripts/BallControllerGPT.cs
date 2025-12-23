using UnityEngine;

public class BallControllerGPT : MonoBehaviour
{
    public DodgeballAgentGPT lastHolder = null;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BackWall"))
        {
            if (lastHolder != null) 
            {
                lastHolder.AddReward(-0.05f); //missed shot
                lastHolder.AddOpponentReward(0.1f);
            }
            lastHolder = null;
        }
    }
}