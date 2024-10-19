using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float runSpeed = 20f;
    [SerializeField] private float gravity = -9.18f * 2;
    [SerializeField] private float jumpHeight = 3f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance;
    [SerializeField] private LayerMask groundMask;

     [SerializeField] private Animator _animator;

    Vector3 velocity;

    private bool _isGrounded;
    private bool _isMoving;
    public bool isRunning;

    private Vector3 _lastPosition = new Vector3(0f, 0f, 0f);

    private CharacterController _controller;

    private float smoothTime = 0.1f; // Время для сглаживания
    private float currentVelocityX = 0f;
    private float currentVelocityZ = 0f;

    private float RunZ;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
         _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Ground Check
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Resseting the default velocity
        if (_isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Getting the inputs
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        print(x);
        print(z);

        // Creating the moving vector
        isRunning = z > 0 && Input.GetKey(KeyCode.LeftShift);
        RunZ = isRunning ? 2f : z;

        // Плавный переход между бегом и ходьбой
        //currentRunMultiplier = Mathf.Lerp(1f, 2f, Time.deltaTime * smoothTime);

        float currentSpeed = isRunning ? runSpeed : speed;
        Vector3 move = transform.right * x + transform.forward * z;

        // Moving the player
        _controller.Move(move * currentSpeed * Time.deltaTime);

        // Check if the player is jump
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            // Jumping
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Falling down
        velocity.y += gravity * Time.deltaTime;

        // Executing the jump
        _controller.Move(velocity * Time.deltaTime);

        // Smooth transition for animation
        float smoothX = Mathf.SmoothDamp(_animator.GetFloat("Horizontal"), x, ref currentVelocityX, smoothTime);
        float smoothZ = Mathf.SmoothDamp(_animator.GetFloat("Vertical"), RunZ, ref currentVelocityZ, smoothTime);

        // Set the smoothed values to animator
        _animator.SetFloat("Vertical", smoothZ);
        _animator.SetFloat("Horizontal", smoothX);

        // Update _isMoving based on smoothed values
        _isMoving = (smoothX != 0 || smoothZ != 0);
    }
}
