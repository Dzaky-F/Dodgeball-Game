using UnityEngine;

public class BallController : MonoBehaviour
{
    public Transform lastHolder = null;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BackWall"))
            lastHolder = null;
    }
}