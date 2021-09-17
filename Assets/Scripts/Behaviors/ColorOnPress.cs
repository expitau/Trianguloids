using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorOnPress : MonoBehaviour
{
    public Color Original;
    public Color OnClick;
    public string KeyCode;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton(KeyCode))
        {
            transform.GetComponent<Image>().color = OnClick;
        }
        else
        {
            transform.GetComponent<Image>().color = Original;
        }
    }
}
