using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target;
    public float speed;
    public float scaleSpeed;
    public Camera cam;

    // Update is called once per frame
    void Update()
    {
        Vector3 positionLerp = Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime);
        positionLerp.z = transform.position.z -20;

        transform.position = positionLerp;

        Vector3 rotationLerp = Vector3.Lerp(transform.eulerAngles, target.eulerAngles, speed * Time.deltaTime);
        transform.eulerAngles = rotationLerp;


        //cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 5 * target.localScale.x, scaleSpeed * Time.deltaTime);
    }
}