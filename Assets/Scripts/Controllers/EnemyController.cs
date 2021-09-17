using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public static float shootRate;
    public float timeToShoot;
    public GameObject bulletPrefab;
    public GameController gameController;
    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        timeToShoot = 0f;
        if (player == null)
        {
            player = gameController.Hub.transform;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 diff = player.position - transform.position;
        Vector3 Target = player.position - 3f * (diff.normalized);
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            float dist;
            if ((dist = (enemy.transform.position - transform.position).sqrMagnitude) < 5f)
            {
                Target += (-1 / Mathf.Max(dist, 0.1f)) * (enemy.transform.position - transform.position);
            }
        }

        if ((timeToShoot += Time.deltaTime) > shootRate && diff.sqrMagnitude < 25f)
        {
            Instantiate(bulletPrefab, transform.position + transform.up * 0.5f, transform.rotation,
                gameController.Bullet_Holder.transform);
            timeToShoot = 0f;
        }
        transform.position = Vector2.Lerp(transform.position, Target, 0.02f);
        transform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, diff));
    }
}
