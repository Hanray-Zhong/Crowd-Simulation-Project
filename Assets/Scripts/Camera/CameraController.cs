using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("缩放视野")]
    public float ZoomRate;
    [Header("移动视野")]
    public float MoveRate;


    private Camera mainCamera;

    private void Start()
    {
        mainCamera = gameObject.GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        ZoomFieldOfView();
        MoveView();
    }

    private void ZoomFieldOfView()
    {
        float dv = Input.GetAxis("Mouse ScrollWheel");
        if (dv > 0)
        {
            // 鼠标滚轮向上滚动
            mainCamera.orthographicSize -= ZoomRate * Time.deltaTime;
        }
        else if (dv < 0)
        {
            // 鼠标滚轮向下滚动
            mainCamera.orthographicSize += ZoomRate * Time.deltaTime;
        }
    }

    private void MoveView()
    {
        if (Input.GetMouseButton(2))
        {
            float mouseMove_x = Input.GetAxis("Mouse X");
            float mouseMove_y = Input.GetAxis("Mouse Y");
            // Debug.Log(mouseMove_x + " " + mouseMove_y);
            Vector3 moveDir = new Vector3(-mouseMove_x, -mouseMove_y, 0);
            transform.position += moveDir * MoveRate * Time.deltaTime;
        }
    }
}
