using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour
{
    #region Walker variables

    //Store all armatures
    [SerializeField]
    private List<InverseKinematicArm> armatures = null;
    //How long a step should be
    [SerializeField]
    private float strideLength = 5.0f;
    //How high a step should go
    [SerializeField]
    private float stepHeight = 5.0f;
    //How long it should take to take a step
    [SerializeField]
    private float stepTime = 1.0f;
    //Minimum tolerance for foot placement
    [SerializeField]
    private float minDistanceToTarget = 0.1f;

    private Vector3 previousTarget = new Vector3();
    private Vector3 midPoint = new Vector3();
    private Vector3 newTarget = new Vector3();
    private float elapsedTime = 0;
    private float percentageComplete;
    private float verticalPercentageComplete;

    private Vector3 horizontalPosition;
    private Vector3 verticalPosition;
    

    #endregion

    #region WalkerInitalisation

    private void Awake()
    {
        //If there are no armatures setup destroy this script
        if (armatures == null) Destroy(this);
    }

    #endregion

    #region Calculate Target Movement

    private void LateUpdate()
    {
        UpdateTarget();
    }

    private void UpdateTarget()
    {
        if (newTarget != Vector3.zero && (armatures[0].GetTarget().position != newTarget))
        {
            elapsedTime += Time.deltaTime;
            percentageComplete = elapsedTime / stepTime;
            //Vector3 currentPosition = arm.GetTarget().position;

            horizontalPosition = Vector3.Lerp(previousTarget, newTarget, percentageComplete);

            if (percentageComplete < 0.5)
            {
                verticalPercentageComplete = elapsedTime / (stepTime / 2);
                verticalPosition = Vector3.Lerp(previousTarget, midPoint, percentageComplete);
            }
            else
            {
                verticalPercentageComplete = elapsedTime / (stepTime / 2);
                verticalPosition = Vector3.Lerp(midPoint, newTarget, percentageComplete);
            }

            armatures[0].SetTarget(new Vector3(horizontalPosition.x, verticalPosition.y, horizontalPosition.z));
            //armatures[0].SetTarget(horizontalPosition);
            return;
        }
        CheckMovement();
    }

    private void CheckMovement()
    {
        foreach (InverseKinematicArm arm in armatures)
        {
            if (Physics.Raycast(arm.GetRoot().position, new Vector3(1, -3, 0).normalized, out var hit, arm.GetLength(), LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
            {
                //If the ray hit position is at the strideLength or greater move the armatures target
                if (Vector3.Distance(hit.point, arm.GetTarget().position) >= strideLength)
                {
                    previousTarget = arm.GetTarget().position;
                    midPoint = (hit.point - previousTarget)/2 + previousTarget;
                    midPoint.y += stepHeight;
                    newTarget = hit.point;
                    elapsedTime = 0.0f;
                    return;
                    //arm.SetTarget(hit.point);
                }
            }
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        foreach (InverseKinematicArm arm in armatures)
        {
            if(arm.GetRoot() != null)
            {
                Gizmos.DrawLine(arm.GetRoot().position, arm.GetRoot().position + ((new Vector3(1, -3, 0)).normalized * arm.GetLength()));
            }
        }
    }

    #endregion
}