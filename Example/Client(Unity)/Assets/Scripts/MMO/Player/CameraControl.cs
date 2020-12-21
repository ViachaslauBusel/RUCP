using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{


    [SerializeField] Transform target;
    [SerializeField] float speed_rotation = 100.0f;
    [SerializeField] float distance = 10.0f;



    private void Awake()
    {
        transform.SetParent(target);
        transform.localPosition = new Vector3(0, 0, -distance);
        transform.rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
    }

    void LateUpdate()
    {
        Vector3 rotation = target.parent.rotation.eulerAngles;
        rotation.y += (Input.GetAxis("Mouse X") * speed_rotation) * Time.deltaTime;
        target.parent.rotation = Quaternion.Euler(rotation);

        rotation = target.rotation.eulerAngles;
        rotation.x = Mathf.Clamp(rotation.x - (Input.GetAxis("Mouse Y") * speed_rotation) * Time.deltaTime, 0.0f, 65.0f);

        target.rotation = Quaternion.Euler(rotation);
    }
}
