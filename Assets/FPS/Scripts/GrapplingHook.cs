using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public bool IsHookMoving { get; private set; }
    public ParticleSystem speedLine;
    [SerializeField]
    private Transform debugHit;
    [SerializeField]
    private float maxHookDistance=40f;
    [SerializeField]
    private float maxHookMoveSpeed = 40f;
    [SerializeField]
    private float minHookMoveSpeed = 10f;
    [SerializeField]
    private Transform hook;

    private const float NORMAL_FOV = 60f;
    private const float HOOKMOVE_FOV = 100f;

    private CharacterController characterController;
    private PlayerCharacterController controller;
    private CameraFov cameraFov;
    private State state;
    private Vector3 hookPosition;
    private float reachHookPositionDistance = 2f;
    private float hookSize;
    private float hookShotSpeed = 80f;

    enum State
    {
        Normal,
        HookShot,
        HookMoving,
    }

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        controller=GetComponent<PlayerCharacterController>();
        cameraFov = Camera.main.GetComponent<CameraFov>();
        hook.gameObject.SetActive(false);
        IsHookMoving = false;
        state = State.Normal;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.Normal:
                IsHookMoving = false;
                HandleHookStart();
                break;
            case State.HookShot:
                IsHookMoving = false;
                HandleHookShot();
                break;
            case State.HookMoving:
                IsHookMoving = true;
                HandleHookMovement();
                break;
            default:
                break;
        }
        
    }

    void HandleHookStart()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * maxHookDistance, Color.blue);
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, maxHookDistance))
            {                
                hookPosition = hit.point;
                state = State.HookShot;
            }
        }
    }

    void HandleHookShot()
    {
        hook.gameObject.SetActive(true);
        hook.LookAt(hookPosition);
        hookSize += hookShotSpeed * Time.deltaTime;
        hook.localScale = new Vector3(1, 1, hookSize);

        if (hookSize>=Vector3.Distance(transform.position,hookPosition))
        {
            state = State.HookMoving;
        }
    }

    void HandleHookMovement()
    {
        cameraFov.SetCameraFov(HOOKMOVE_FOV);
        speedLine.Play();
        Vector3 hookDir = (hookPosition - transform.position).normalized;
        float hookMoveSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookPosition), minHookMoveSpeed, maxHookMoveSpeed);
        float hookMoveSpeedMutiplier = 5f;
        characterController.Move(hookDir * hookMoveSpeed* hookMoveSpeedMutiplier * Time.deltaTime);

        if (Vector3.Distance(transform.position,hookPosition)<reachHookPositionDistance)
        {
            StopHookMove();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            controller.hookMoveMomentum = hookDir * hookMoveSpeed * 1f;
            StopHookMove();
        }
    }

    void ResetGravity()
    {
        controller.characterVelocity = new Vector3(controller.characterVelocity.x, 0f, controller.characterVelocity.z);
    }

    void StopHookMove()
    {
        state = State.Normal;
        hookSize = 0f;
        hook.gameObject.SetActive(false);
        cameraFov.SetCameraFov(NORMAL_FOV);
        speedLine.Stop();
        ResetGravity();
    }

}
