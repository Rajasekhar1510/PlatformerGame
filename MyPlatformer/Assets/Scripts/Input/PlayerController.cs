using UnityEngine;
using KBCore.Refs;
using Cinemachine;
using Unity.PlasticSCM.Editor.WebApi;
using System;

public class PlayerController: ValidatedMonoBehaviour 
{
    [Header("References")]
    [SerializeField, Self] CharacterController controller;
    [SerializeField, Self] Animator animator;
    [SerializeField, Anywhere] CinemachineFreeLook freeLookVCam;
    [SerializeField, Anywhere] InputReader input;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float rotationSpeed = 15f;
    [SerializeField] float smoothTime = 0.2f;

    const float ZeroF = 0f;
    Transform mainCam;
    float currentSpeed;
    float velocity;

    static readonly int Speed = Animator.StringToHash("Speed");

    /*[Header("Jump Settings")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float jumpDuration = 0.5f;
    [SerializeField] float jumpCooldown = 0f;
    [SerializeField] float gravityMultiplier = 3f;*/


    private void Awake()
    {
        mainCam = Camera.main.transform;
        freeLookVCam.Follow = transform;
        freeLookVCam.LookAt = transform;
        freeLookVCam.OnTargetObjectWarped(transform, transform.position - freeLookVCam.transform.position - Vector3.forward);
    }

    void Start() => input.EnablePlayerActions();


    private void Update()
    {
        HandleMovement();
        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        animator.SetFloat(Speed, currentSpeed);
    }

    void HandleMovement()
    {
        var movementDirection = new Vector3(input.Direction.x, 0f, input.Direction.y).normalized;

        var adjustedDirection = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movementDirection;

        if (adjustedDirection.magnitude > ZeroF)
        {
            HandleRotation(adjustedDirection);

            HandleCharacterController(adjustedDirection);

            SmoothSpeed(adjustedDirection.magnitude);

        }
        else
        {
            SmoothSpeed(ZeroF);

        }


    }
    void HandleCharacterController(Vector3 adjustedDirection)
    {
        var adjustedMovement = adjustedDirection * (moveSpeed * Time.deltaTime);
        controller.Move(adjustedMovement);
       
    }
    void SmoothSpeed(float value)
    {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref velocity, smoothTime);
    }

    void HandleRotation(Vector3 adjustedDirection)
    {
        var targetRotation = Quaternion.LookRotation(adjustedDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        transform.LookAt(transform.position + adjustedDirection);
    }
}
