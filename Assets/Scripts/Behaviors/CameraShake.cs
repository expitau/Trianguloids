using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = new Vector3(Random.Range(-magnitude, magnitude), Random.Range(-magnitude, magnitude), transform.localPosition.z);
            yield return null;
        }
        transform.localPosition = originalPos;
    }
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }
}
