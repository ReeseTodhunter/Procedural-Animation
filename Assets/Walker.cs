using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour
{
    [SerializeField]
    List<IK_LegArmature> legs = null; //A list of all the walkers legs

    public List<legPair> legPairs;

    public float setWalkerHeight = 7.0f;

    public float minDistanceFromRoof = 1.0f;
    public float minHeightLeeway = 0.1f;
    public float maxHeightLeeway = 1.0f;

    public float walkSpeed = 5.0f;
    public float rotateSpeed = 3.0f;

    private Vector3 currentPosition;
    private Vector3 currentVelocity;

    private float currentDistanceFromFloor;
    private float currentDistanceFromRoof;

    private float currentHeightLeeway;
    private float walkerHeight;

    private Vector3 directionToCastRay;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        MoveWalker();
        UpdateWalkerBody();
        UpdateLegs();
    }

    private void Init()
    {
        //Setup the starting current position
        currentPosition = transform.position;

        //Setup the starting height leeway
        currentHeightLeeway = minHeightLeeway;

        //Setup the starting walker height
        walkerHeight = setWalkerHeight;

        //Setup the walker distances
        currentDistanceFromFloor = walkerHeight;
        currentDistanceFromRoof = minDistanceFromRoof + 1;

        //Create the list of legPairs
        legPairs = new List<legPair>();

        //Run a loop through all of the leg armatures in the list of legs
        for (int i = 0; i < legs.Count; i += 2)
        {
            //if the leg has a pairing leg available
            if (i + 1 <= legs.Count)
            {
                //Check that the legs aren't null values
                if (legs[i] != null && legs[i + 1] != null)
                {
                    //Create a new legPair with both leg armatures and add it to the list of leg pairs
                    legPairs.Add(new legPair { legOne = legs[i], legTwo = legs[i + 1] });
                }
            }
            //if there is an uneven amount of legs add the remaining leg to the list of leg pairs
            else
            {
                //Check for null value
                if (legs[i] != null)
                {
                    legPairs.Add(new legPair { legOne = legs[i], legTwo = legs[i] });
                }
            }
        }
    }

    private void MoveWalker()
    {
        //Work out the current velocity of the walker based on it's previous position
        currentVelocity = (new Vector3(transform.position.x, currentPosition.y, transform.position.z) - currentPosition) / Time.deltaTime;
        //Update the walker's current position
        currentPosition = transform.position;

        //Allow player to control walker with arrow keys / WASD
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            transform.position += transform.right * Input.GetAxisRaw("Horizontal") * walkSpeed * Time.deltaTime;
            transform.position += transform.forward * Input.GetAxisRaw("Vertical") * walkSpeed * Time.deltaTime;
        }
    }

    private void UpdateWalkerBody()
    {
        RaycastHit hit;
        currentHeightLeeway = minHeightLeeway;
        directionToCastRay = new Vector3(currentVelocity.x, (Mathf.Sin(Mathf.Acos(currentVelocity.magnitude / (walkerHeight * (currentVelocity.magnitude + 1)))) * (walkerHeight * (currentVelocity.magnitude + 1))), currentVelocity.z).normalized;

        //Lower the walker down when in motion
        if (currentVelocity.magnitude > 0 && walkerHeight == setWalkerHeight)
        {
            walkerHeight -= minHeightLeeway * 10;
        }
        //Raise back up to full height when stopped
        else if (currentVelocity.magnitude <= 0)
        {
            walkerHeight = setWalkerHeight;
        }

        //Fire a ray up and forwards to check for a roof
        if (Physics.Raycast(transform.position, directionToCastRay, out hit, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
        {
            //update the currentDistance from the roof
            currentDistanceFromRoof = Vector3.Distance(transform.position, hit.point);
            //If the ray hits and the walker is closer to the collision point than the minimum distance from the roof
            if (currentDistanceFromRoof < minDistanceFromRoof)
            {

                currentHeightLeeway = maxHeightLeeway;
                transform.position -= Vector3.up * walkSpeed * Time.deltaTime;
            }
        }
        //If the ray hits nothing reset the current distance from roof
        else
        {
            currentDistanceFromRoof = minDistanceFromRoof + 1;
        }
        //Fire a ray down and forwards to check for distance from the floor
        if (Physics.Raycast(transform.position, new Vector3(directionToCastRay.x, -directionToCastRay.y, directionToCastRay.z), out hit, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
        {
            currentDistanceFromFloor = Vector3.Distance(transform.position, hit.point);
            if (currentDistanceFromFloor < walkerHeight - currentHeightLeeway)
            {
                transform.position += Vector3.up * walkSpeed * Time.deltaTime;
            }
            else if (currentDistanceFromFloor > walkerHeight + currentHeightLeeway)
            {
                transform.position -= Vector3.up * walkSpeed * Time.deltaTime;
            }
        }
    }

    private void UpdateLegs()
    {
        int i = 0;
        //For all of the pairs of legs
        foreach (legPair pair in legPairs)
        {
            //Check that both legs are grounded before lifting a leg
            if (pair.legOne.IsGrounded() && pair.legTwo.IsGrounded())
            {
                //Update both legs raycasts
                pair.legOne.CheckMovement(currentVelocity);
                pair.legTwo.CheckMovement(currentVelocity);

                //If leg two is grounded and it's leg one's turn to move
                if (pair.legTwo.IsGrounded() && (i % 2 == 0))
                {
                    //Update leg one's target positioning
                    pair.legOne.UpdateTarget();
                    i++;
                }
                if (pair.legOne.IsGrounded() && (i % 2 != 0))
                {
                    //Otherwise if leg one is grounded and it's leg two's turn to move
                    pair.legTwo.UpdateTarget();
                    i++;
                }
                Debug.Log(i);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + directionToCastRay * (walkerHeight * (currentVelocity.magnitude + 1)));
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(directionToCastRay.x, -directionToCastRay.y, directionToCastRay.z) * (walkerHeight * (currentVelocity.magnitude + 1)));
    }
    public struct legPair
    {
        public IK_LegArmature legOne;
        public IK_LegArmature legTwo;
    }
}
