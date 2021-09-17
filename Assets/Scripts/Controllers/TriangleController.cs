using UnityEngine;

public class TriangleController : MonoBehaviour
{
    public GameController gameController;
    public GameObject bulletPrefab;
    public GameObject floaterPrefab;

    public Vector2Int pos;
    public int type = 0;
    private bool[] turrets = new bool[3] { false, false, false };
    private HubController Hub;


    private Color32[] colors = new Color32[] {
            new Color32(80, 80, 80, 255),
            new Color32(248, 248, 248, 255),
            new Color32(226, 153, 0, 255),
            new Color32(1, 119, 251, 255),
            new Color32(36, 123, 48, 255),
            new Color32(255, 1, 0, 255),
        };

    private void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        Hub = gameController.Hub;
    }

    void LateUpdate()
    {
        bool[] OpenSides = Hub.GetOpen(pos);
        turrets = new bool[3] { OpenSides[0] && OpenSides[1], OpenSides[1] && OpenSides[2], OpenSides[2] && OpenSides[0] };
        transform.GetComponent<SpriteRenderer>().color = colors[turrets[0] || turrets[1] || turrets[2] ? type + 1 : 1];
    }


    public void Shoot()
    {
        for (int i = 0; i < turrets.Length; i++)
        {
            if (turrets[i])
            {
                gameController.PlayClip(2);
                Transform turretTransform = transform.GetChild(0).GetChild(i);
                GameObject bullet = Instantiate(
                    bulletPrefab,
                    turretTransform.position + turretTransform.up * 0.2f,
                    turretTransform.rotation, gameController.Bullet_Holder.transform);
                Hub.transform.GetComponent<Rigidbody2D>().AddForceAtPosition(
                    turretTransform.rotation * Vector2.down * 50f,
                    turretTransform.position);
                bullet.GetComponent<BulletController>().InitialVelocity = Hub.rigidBody.velocity;
            }
        }
    }

    public void MakeFloater()
    {
        GameObject f = Instantiate(floaterPrefab, transform.position, transform.rotation, gameController.Object_Holder.transform);
        f.GetComponent<FloaterController>().type = type;
        f.GetComponent<Rigidbody2D>().velocity = (transform.position - Hub.transform.GetChild(0).position).normalized * (5f + Random.value);
        f.GetComponent<Rigidbody2D>().angularVelocity = Random.value * 5f;
        Destroy(gameObject);
    }
}
