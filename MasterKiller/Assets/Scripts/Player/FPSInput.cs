using UnityEngine;

[RequireComponent(typeof(CharacterController))]

[AddComponentMenu("Control Script/FPS Input")]
    
public class FPSInput : MonoBehaviour
{
    [SerializeField] float _speed = 6.0f;
    [SerializeField] float _gravity = -9.8f;

    private CharacterController _controller;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float deltaX = Input.GetAxis("Horizontal") * _speed;
        float deltaZ = Input.GetAxis("Vertical") * _speed;

        Vector3 movement = new(deltaX, 0, deltaZ);
        
        movement = Vector3.ClampMagnitude(movement, _speed);
        
        movement.y = _gravity;

        movement *= Time.deltaTime;
        
        movement = transform.TransformDirection(movement);

        _controller.Move(movement);
    }
}
