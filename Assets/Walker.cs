using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour
{
    #region Walker variables

    //Store all armatures
    [SerializeField]
    private List<InverseKinematicArm> armatures = null;
    //How long steps should be
    [SerializeField]
    private float strideLength = 5.0f;
    
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
        UpdateTargets();
    }

    private void UpdateTargets()
    {
        foreach (InverseKinematicArm arm in armatures)
        {
            if (Physics.Raycast(arm.GetRoot().position, new Vector3(1,-3, 0).normalized, out var hit, arm.GetLength(), LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
            {
                //If the ray hit position is at the strideLength or greater move the armatures target
                if (Vector3.Distance(hit.point, arm.GetTarget().position) >= strideLength)
                {
                    arm.SetTarget(hit.point);
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