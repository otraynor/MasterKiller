using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    enum RotationAxes
    {
        MouseXandY = 0,
        MouseX = 1,
        MouseY = 2
    }

    [Header("Degrees of Freedom")]
    [SerializeField] RotationAxes _axes = RotationAxes.MouseXandY;

    [Header("Sensitivity")]
    [SerializeField] float _sensitivityHor = 9.0f;
    [SerializeField] float _sensitivityVert = 9.0f;

    [Header("Constraints")]
    [SerializeField] float _minVert = -45.0f;
    [SerializeField] float _maxVert = 45.0f;

    private float _verticalRot = 0;

    private void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.freezeRotation = true;
    }

    private void Update()
    {
        switch (_axes)
        {
            case RotationAxes.MouseX:
                transform.Rotate(
                    0, Input.GetAxis("Mouse X") * _sensitivityHor, 0);

                break;

            case RotationAxes.MouseY:
                _verticalRot -= Input.GetAxis("Mouse Y") * _sensitivityVert;
                _verticalRot = Mathf.Clamp(_verticalRot, _minVert, _maxVert);

                float horizontalRot = transform.localEulerAngles.y;
                
                transform.localEulerAngles = new Vector3(
                    _verticalRot, horizontalRot, 0);

                break;

            case RotationAxes.MouseXandY:
                _verticalRot -= Input.GetAxis("Mouse Y") * _sensitivityVert;
                _verticalRot = Mathf.Clamp(_verticalRot, _minVert, _maxVert);

                float deltaX = Input.GetAxis("Mouse X") * _sensitivityHor;
                horizontalRot = transform.localEulerAngles.y + deltaX;
                
                transform.localEulerAngles = new Vector3(
                    _verticalRot, horizontalRot, 0);

                break;

            default:
                break;
        }
    }
}
