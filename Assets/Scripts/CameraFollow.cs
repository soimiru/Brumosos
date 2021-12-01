using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float smoothSpeed = 0.125f; //Más grande, más rapido
    public Vector3 offset;

    void LateUpdate()
    {
        //FixedUpdate para mejores resultados, pero queda mejor con LateUpdate
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        //transform.LookAt(target);   //ESTO ES RARO
    }
}
