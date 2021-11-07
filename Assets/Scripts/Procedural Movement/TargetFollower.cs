using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollower : MonoBehaviour
{
    public InverseKinematics targetResolver;
    public float maxDistance;
    public float followSpeed = 100f;

    private Transform _transform;


    private float timer = 0;
    private bool isMoving = false;


    private void Start()
    {
        _transform = transform;
    }

    void Update()
    {
        if (targetResolver == null)
            return;

        var distance = Vector3.Distance(targetResolver.GetTargetPosition(), _transform.position);

        if(distance > maxDistance)
        {
            isMoving = true;
        }

        if (isMoving)
        {
            timer += Time.deltaTime * followSpeed;
            if (timer > 1)
            {
                timer = 0;
                isMoving = false;
            }

            targetResolver.SetTarget(Vector3.Lerp(targetResolver.GetTargetPosition(), _transform.position, timer));
            //Target.position = ;
        }

    }
}
