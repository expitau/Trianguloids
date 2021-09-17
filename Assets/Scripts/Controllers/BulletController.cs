using UnityEngine;

public class BulletController : MonoBehaviour
{
    private GameController gameController;
    public Vector3 InitialVelocity;
    void Start()
    {
        transform.GetComponent<Rigidbody2D>().velocity = 
            (transform.up * 6f + InitialVelocity).sqrMagnitude > (transform.up * 6f).sqrMagnitude 
            ? transform.up * 6f + InitialVelocity 
            : transform.up * 6f;
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>().Shake(0.3f, 0.2f);
            gameController.PlayClip(4);
            if (!gameController.Hub.isDead) gameController.AddScore(100);
            gameController.CreateParticles(transform.position);
        }
        Destroy(gameObject);
    }
}
