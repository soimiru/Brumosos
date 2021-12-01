using UnityEngine;

public class CameraController : MonoBehaviour
{

    public float timeSubstitute = 0.005f; //Usaríamos Time.deltaTime, pero va demasiado rapido.
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public float scrollSpeed = 20f;

    public Vector2 panLimit;
    public float minY = 5f;
    public float maxY = 80f;

    // Update is called once per frame
    void Update()
    {

        Vector3 pos = transform.position;

        //Input.mousePosition.y >= Screen.height - panBorderThickness = PARA MOVERNOS CON EL RATON EN EL BORDE DE LA PANTALLA
        if (Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBorderThickness) {
            pos.z += panSpeed * 0.005f;
        }
        if (Input.GetKey("s") || Input.mousePosition.y <= panBorderThickness)
        {
            pos.z -= panSpeed * 0.005f;
        }
        if (Input.GetKey("d") || Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            pos.x += panSpeed * 0.005f;
        }
        if (Input.GetKey("a") || Input.mousePosition.x <= panBorderThickness)
        {
            pos.x -= panSpeed * 0.005f;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * timeSubstitute * 100f ;

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

        transform.position = pos;
    }
}
