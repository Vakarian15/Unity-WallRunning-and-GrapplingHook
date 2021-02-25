using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFov : MonoBehaviour
{
    private Camera playerCamera;
    private float targetFov;
    private float fov;
    // Start is called before the first frame update
    void Start()
    {
        playerCamera = GetComponent<Camera>();
        targetFov = playerCamera.fieldOfView;
        fov = targetFov;
    }

    // Update is called once per frame
    void Update()
    {
        float fovSpeed = 4f;
        fov = Mathf.Lerp(fov, targetFov, fovSpeed * Time.deltaTime);
        playerCamera.fieldOfView = fov;
    }

    public void SetCameraFov(float tFov)
    {
        targetFov = tFov;
    }
}
