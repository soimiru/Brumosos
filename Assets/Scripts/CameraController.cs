using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("FREE")]

    public float timeSubstitute = 0.005f; //Usaríamos Time.deltaTime, pero va demasiado rapido.
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public float scrollSpeed = 20f;

    public Vector2 panLimit;
    public float minY = 5f;
    public float maxY = 80f;

    [Header ("TARGETED")]
    public bool targeted = false;
    public Transform target = null;

    public float smoothSpeed = 0.125f; //Más grande, más rapido
    public Vector3 offset;

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit) && hit.transform.gameObject.tag == "Entity" || Physics.Raycast(ray, out hit) && hit.transform.gameObject.tag == "Inquisidor" || Physics.Raycast(ray, out hit) && hit.transform.gameObject.tag == "Skaa" )
            {
                target = hit.transform;
                targeted = true;
            }
        }

        
        Vector3 pos = transform.position;

        //Input.mousePosition.y >= Screen.height - panBorderThickness = PARA MOVERNOS CON EL RATON EN EL BORDE DE LA PANTALLA
        if (Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBorderThickness)
        {
            if (!targeted) pos.z += panSpeed * 0.005f;
            targeted = false;
        }
        if (Input.GetKey("s") || Input.mousePosition.y <= panBorderThickness)
        {
            if (!targeted) pos.z -= panSpeed * 0.005f;
            targeted = false;
        }
        if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            if (!targeted) pos.x += panSpeed * 0.005f;
            targeted = false;
        }
        if (Input.GetKey("a") || Input.mousePosition.x <= panBorderThickness)
        {
            if (!targeted) pos.x -= panSpeed * 0.005f;
            targeted = false;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * timeSubstitute * 100f;

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

        transform.position = pos;
        
        
    }

    void LateUpdate()
    {
        if (targeted) {
            //FixedUpdate para mejores resultados, pero queda mejor con LateUpdate
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            //transform.LookAt(target);   //ESTO ES RARO
        }
    }
}
