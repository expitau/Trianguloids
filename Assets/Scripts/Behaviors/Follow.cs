using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform target;
    public float amount = 0.5f;

    void Update()
    {
        transform.position = new Vector3(
            Mathf.Lerp(transform.position.x, target.position.x, amount), 
            Mathf.Lerp(transform.position.y, target.position.y, amount), 
            0f);
    }
}
