using UnityEngine;
using KBCore.Refs;
using Cinemachine;
using System;
using System.Collections;

public class CameraManager: MonoBehaviour
{
    [SerializeField, Anywhere] InputReader input;
    [SerializeField, Anywhere] CinemachineFreeLook freeLookVCam;

    [Header("Settings")]
    [SerializeField, Range(0.5f, 3f)] float speedMultiplier = 1f;

    bool isRMBPressed;
    
    bool cameraMovementLock;

    private void OnEnable()
    {
        input.Look += OnLook;
        input.EnableMouseControlCamera += OnEnableMouseControlCamera;
        input.DisableMouseControlCamera += OnDisableMouseControlCamera;
    }
    private void OnDisable()
    {
        input.Look -= OnLook;
        input.EnableMouseControlCamera -= OnEnableMouseControlCamera;
        input.DisableMouseControlCamera -= OnDisableMouseControlCamera;
    }
    private void OnLook(Vector2 cameraMovement, bool isDeviceMouse)
    {
        if (cameraMovementLock) return;

        if (isDeviceMouse && !isRMBPressed) return;

        float deviceMultiplier = isDeviceMouse ? Time.fixedDeltaTime : Time.deltaTime;

        freeLookVCam.m_XAxis.m_InputAxisValue = cameraMovement.x * speedMultiplier * deviceMultiplier;
        freeLookVCam.m_YAxis.m_InputAxisValue = cameraMovement.y * speedMultiplier * deviceMultiplier;
    }
    private void OnEnableMouseControlCamera()
    {
        isRMBPressed = true;

        //lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(DisableMouseForFrame());

    }
    private void OnDisableMouseControlCamera()
    {
        isRMBPressed = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        freeLookVCam.m_XAxis.m_InputAxisValue = 0f;
        freeLookVCam.m_YAxis.m_InputAxisValue = 0f;
    }

    private IEnumerator DisableMouseForFrame()
    {
        cameraMovementLock = true;
        yield return new WaitForEndOfFrame();
        cameraMovementLock = false;
    }

}
