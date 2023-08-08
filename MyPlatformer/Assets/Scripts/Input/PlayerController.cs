using UnityEngine;
using KBCore.Refs;
using Cinemachine;
using System;
using System.Collections.Generic;

public class PlayerController: ValidatedMonoBehaviour 
{
    [Header("References")]
    
    [SerializeField, Self] Rigidbody rb;
    [SerializeField, Self] Animator animator;
    [SerializeField, Self] GroundChecker groundChecker;
    [SerializeField, Anywhere] CinemachineFreeLook freeLookVCam;
    [SerializeField, Anywhere] InputReader input;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float rotationSpeed = 15f;
    [SerializeField] float smoothTime = 0.2f;

    [Header("Jump Settings")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float jumpDuration = 0.5f;
    [SerializeField] float jumpCooldown = 0f;
    [SerializeField] float jumpMaxHeight = 2f;
    [SerializeField] float gravityMultiplier = 3f;


    const float ZeroF = 0f;
    Transform mainCam;
    float currentSpeed;
    float Velocity;
    float jumpVelocity;

    Vector3 movement;

    List<Timer> timers;
    CountdownTimer jumpTimer;
    CountdownTimer jumpCooldownTimer;

    static readonly int Speed = Animator.StringToHash("Speed");

    private void Awake()
    {
        mainCam = Camera.main.transform;
        freeLookVCam.Follow = transform;
        freeLookVCam.LookAt = transform;
        freeLookVCam.OnTargetObjectWarped(transform, transform.position - freeLookVCam.transform.position - Vector3.forward);

        rb.freezeRotation = true;
        // rb.useGravity = false;

        //timer setup
        jumpTimer = new CountdownTimer(jumpDuration);
        jumpCooldownTimer = new CountdownTimer(jumpCooldown);   
        timers = new List<Timer>(2) { jumpTimer, jumpCooldownTimer};

        jumpTimer.OnTimerStop += () => jumpCooldownTimer.Start();
    }

    void Start() => input.EnablePlayerActions();

    private void OnEnable()
    {
        input.Jump += OnJump;
    }

    private void OnDisable()
    {
        input.Jump -= OnJump;
    }

    void OnJump(bool performed)
    {
        if(performed && !jumpTimer.IsRunning && !jumpCooldownTimer.IsRunning && groundChecker.isGrounded) 
        {
            jumpTimer.Start();
        }
        else if (!performed && jumpTimer.IsRunning)
        {
            jumpTimer.Stop();
        }
    }

    private void Update()
    {
        movement = new Vector3(input.Direction.x, 0f, input.Direction.y);
        
        UpdateAnimator();

        HandleTimers();
    }
    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    private void HandleJump()
    {
        if(!jumpTimer.IsRunning && groundChecker.isGrounded)
        {
            jumpVelocity = ZeroF;
            jumpTimer.Stop();
            return;
        
        }

        //if jumping or falling

        if (jumpTimer.IsRunning)
        {
            float launchPoint = 0.9f;
            if(jumpTimer.Progress > launchPoint ) 
            {
                //velocity required to reach the jump height using physics equations v = sqrt(2gh)
                jumpVelocity = Mathf.Sqrt(2 * jumpMaxHeight * Mathf.Abs(Physics.gravity.y));
            }
            else
            {
                //gradually apply less velocity

                jumpVelocity += (1 - jumpTimer.Progress) * jumpForce * Time.fixedDeltaTime;
            }
           
        }
        else
        {
            //gravity takes over
            jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
        }

        // Apply velocity
        rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
    }

    private void HandleTimers()
    {
        foreach (var timer in timers)
        {
            timer.Tick(Time.deltaTime);
        }
    }

    void UpdateAnimator()
    {
        animator.SetFloat(Speed, currentSpeed);
    }

    void HandleMovement()
    {
        var adjustedDirection = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movement;

        if (adjustedDirection.magnitude > ZeroF)
        {
            HandleRotation(adjustedDirection);

            HandleHorizontalMovement(adjustedDirection);

            SmoothSpeed(adjustedDirection.magnitude);

        }
        else
        {
            SmoothSpeed(ZeroF);

            //reset horizontal velocity for a snappy stop
            rb.velocity = new Vector3(ZeroF, rb.velocity.y, ZeroF);

        }


    }
    void HandleHorizontalMovement(Vector3 adjustedDirection)
    {
        Vector3 velocity = adjustedDirection * moveSpeed * Time.fixedDeltaTime;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
       
    }
    void SmoothSpeed(float value)
    {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref Velocity, smoothTime);
    }

    void HandleRotation(Vector3 adjustedDirection)
    {
        var targetRotation = Quaternion.LookRotation(adjustedDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        transform.LookAt(transform.position + adjustedDirection);
    }
}
