using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallRun : MonoBehaviour
{
    [SerializeField]
    float maxWallDistance = 1;
    [SerializeField]
    float minHeight = 0.6f;
    [SerializeField]
    float jumpDuration = 0.25f;
    [SerializeField]
    float wallBounceForce = 3;
    [SerializeField]
    float cameraTransitionDuration = 1;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float angleThreshold=0.3f;
    [SerializeField]
    float wallSpeedMultiplier = 12f;
    [SerializeField]
    float wallGravityDown = 20f;
    [SerializeField]
    float maxAngleRoll = 30;

    PlayerCharacterController characterController;
    PlayerInputHandler inputHandler;

    Vector3[] directions;
    RaycastHit[] hits;
    Vector3 lastWallPosition;
    Vector3 lastwallNormal;

    float verticalInput;
    bool isJumping = false;
    bool isWallRunning;
    float elapsedTimeSinceJump = 0;
    float elapsedTimeSinceWallAttach = 0;
    float elapsedTimeSinceWallDetach = 0;


    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<PlayerCharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();

        directions = new Vector3[]{
            Vector3.forward,
            Vector3.right,
            Vector3.left,
            Vector3.forward+Vector3.right,
            Vector3.forward+Vector3.left
            };
    }

    // Update is called once per frame
    void LateUpdate()
    {
        isWallRunning = false;
        verticalInput = Input.GetAxisRaw(GameConstants.k_AxisNameVertical);
        if (inputHandler.GetJumpInputDown())
        {
            isJumping = true;
        }
        if (CanAttach())
        {
            hits = new RaycastHit[directions.Length];
            for (int i = 0; i < directions.Length; i++)
            {
                Vector3 dir = transform.TransformDirection(directions[i]);
                Physics.Raycast(transform.position, dir, out hits[i], maxWallDistance);
                if (hits[i].collider != null)
                {
                    Debug.DrawRay(transform.position, dir * hits[i].distance, Color.green);
                }
                else
                {
                    Debug.DrawRay(transform.position, dir * maxWallDistance, Color.red);
                }
            }

            if (CanWallRun())
            {
                var hitQuery =
                    (from h in hits
                    where (h.collider != null)
                    orderby h.distance ascending
                    select h).ToArray();

                if (hitQuery.Length>0)
                {
                    OnWall(hitQuery[0]);
                    lastWallPosition = hitQuery[0].point;
                    lastwallNormal = hitQuery[0].normal;
                }
            }
        }

        if (isWallRunning)
        {
            elapsedTimeSinceWallDetach = 0;
            elapsedTimeSinceWallAttach += Time.deltaTime;
        }
        else
        {
            elapsedTimeSinceWallAttach = 0;
            elapsedTimeSinceWallDetach += Time.deltaTime;
        }
    }

    bool CanAttach()
    {
        if (isJumping)
        {
            elapsedTimeSinceJump += Time.deltaTime;
            if (elapsedTimeSinceJump > jumpDuration)
            {
                elapsedTimeSinceJump = 0;
                isJumping = false;
            }
            return false;

        }
        return true;
    }

    bool CanWallRun()
    {
        return !characterController.isGrounded&&verticalInput>0&&VerticalCheck();
    }

    bool VerticalCheck()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minHeight);
    }

    void OnWall(RaycastHit hit)
    {
        float a = Vector3.Dot(hit.normal, Vector3.up);
        if (a>=-angleThreshold&&a<=angleThreshold)
        {
            isWallRunning = true;
            Vector3 alongWall = transform.TransformDirection(Vector3.forward);
            characterController.characterVelocity = alongWall * verticalInput * wallSpeedMultiplier;
            characterController.characterVelocity += Vector3.down * wallGravityDown * Time.deltaTime;
        }
    }

    public float GetCameraRoll()
    {
        float side = GetSide();
        float cameraAngle = characterController.playerCamera.transform.eulerAngles.z;
        float targetAngle = 0;
        if (side!=0)
        {
            targetAngle = Mathf.Sign(side) * maxAngleRoll;
        }
        return Mathf.LerpAngle(cameraAngle, targetAngle, Mathf.Max(elapsedTimeSinceWallAttach, elapsedTimeSinceWallDetach) / cameraTransitionDuration);
    }

    float GetSide()
    {
        if (isWallRunning)
        {
            Vector3 p = Vector3.Cross(transform.forward, -1*lastwallNormal);
            //    Vector3 p = Vector3.Cross(transform.forward, lastWallPosition - transform.position);
            return Vector3.Dot(p, transform.up);
        }
        return 0;
    }


    public bool IsWallRunning()
    {
        return isWallRunning;
    }

    public Vector3 GetWallJumpDirection()
    {
        if (isWallRunning)
        {
            return lastwallNormal * wallBounceForce + Vector3.up;
        }
        return Vector3.zero;
    }
}
