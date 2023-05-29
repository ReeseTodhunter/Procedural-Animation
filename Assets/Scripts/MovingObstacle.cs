using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    [SerializeField]
    private Transform startPosition;

    [SerializeField]
    private Transform endPosition;

    private float moveTime;
    private float moveTimer;
    private Vector3 targetPosition;

    // Start is called before the first frame update
    void Start()
    {
        if (startPosition != null && endPosition != null)
        {
            transform.position = startPosition.position;
            moveTime = 1.0f;
            targetPosition = endPosition.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (startPosition != null && endPosition != null)
        {
            moveTimer += Time.deltaTime;
            float percentComplete = moveTimer / moveTime;
            if (targetPosition == endPosition.position)
            {
                transform.position = Vector3.Lerp(startPosition.position, targetPosition, percentComplete);
            }
            else
            {
                transform.position = Vector3.Lerp(endPosition.position, targetPosition, percentComplete);
            }

            if (percentComplete >= 1)
            {
                moveTimer = 0;
                percentComplete = 0;
                if (targetPosition == endPosition.position)
                {
                    targetPosition = startPosition.position;
                }
                else
                {
                    targetPosition = endPosition.position;
                }
            }
        }
    }
}
