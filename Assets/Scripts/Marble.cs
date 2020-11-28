using System;
using UnityEngine;

[RequireComponent(typeof(Movement))]
public class Marble : MonoBehaviour
{
    public Vector3 StartPoint;
    private Movement _movement;
    private OrbitCamera _camera;
    
    private void Start()
    {
        _movement = GetComponent<Movement>();
        _camera = Camera.main.GetComponent<OrbitCamera>();
        transform.position = StartPoint;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            /*_movement.Velocity = Vector3.zero;
            _movement.AngularVelocity = Vector3.zero;*/
            transform.position = StartPoint;
            _camera.pitch = 25f;
            _camera.yaw = 0;
        }
    }
}