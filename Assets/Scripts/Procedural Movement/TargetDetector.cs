using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TargetDetector : MonoBehaviour
{

    public Transform startPoint;
    public Transform directionTowards;
    public LayerMask mask;
    public Transform target;

    RaycastHit hit;
    private Vector3 direction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        direction = directionTowards.position - startPoint.position;
        Ray ray = new Ray(startPoint.position, direction);
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            target.position = hit.point;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(startPoint.position, direction);
    }
}
