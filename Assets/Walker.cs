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

    }

    #endregion
}