using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetFollow : MonoBehaviour
{

    [SerializeField] private Transform Target;
    [SerializeField] private Vector3 offset;

    private void LateUpdate()
    {
        transform.position = Target.position + offset;
    }

    
}
