using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Camera healthCamera;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        if (slider != null)
        {
            slider.value = currentValue / maxValue;
          //  Debug.Log($"Health bar: {currentValue}/{maxValue} = {slider.value}");
        }
        else
        {
          //  Debug.LogError("Slider component not assigned");
        }
    }

    void Update()
    {
        transform.rotation = healthCamera.transform.rotation;
        transform.position = target.position + offset;
    }
}
