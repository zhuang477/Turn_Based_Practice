using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBillboard : MonoBehaviour
{
    public GameObject Billboard;
    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = Billboard.transform.rotation;
    }
}
