using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleMovement : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        if (Input.GetKey("p")) {
            pos.x += 0.05f;
        }
        if (Input.GetKey("o")) {
            pos.x -= 0.05f;
        }

        transform.position = pos;
    }
}
