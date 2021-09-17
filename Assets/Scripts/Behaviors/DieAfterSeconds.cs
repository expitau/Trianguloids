using System.Collections;
using UnityEngine;

public class DieAfterSeconds : MonoBehaviour
{
    public float Time = 0;
    void Start()
    {
        StartCoroutine(DestroyAfterTime(Time));
    }

    IEnumerator DestroyAfterTime(float t)
    {
        yield return new WaitForSeconds(t);
        Destroy(gameObject);

    }
}
