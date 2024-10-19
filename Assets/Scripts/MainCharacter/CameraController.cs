using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Fusion.Sockets.NetBitBuffer;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _sensitivity = 2f;

    public float _rotationX = 0f;
    public float _rotationY = 0f;

    public float topClamp = -90f;
    public float bottomClamp = 90f;

    public Transform playerBody;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;

        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, topClamp, bottomClamp);

        transform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);

        _rotationY += mouseX;
        playerBody.rotation = Quaternion.Euler(0f, _rotationY, 0f);
    }
}
