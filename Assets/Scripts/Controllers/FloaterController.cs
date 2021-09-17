using UnityEngine;

public class FloaterController : MonoBehaviour
{

    public int type;
    private Color32[] colors = new Color32[] {
            new Color32(80, 80, 80, 255),
            new Color32(248, 248, 248, 255),
            new Color32(226, 153, 0, 255),
            new Color32(1, 119, 251, 255),
            new Color32(36, 123, 48, 255),
            new Color32(255, 1, 0, 255),
        };
    // Start is called before the first frame update
    void Start()
    {
        if (type == -1) type = Random.Range(1, 5);
        transform.GetComponent<SpriteRenderer>().color = colors[type + 1];
    }
}
