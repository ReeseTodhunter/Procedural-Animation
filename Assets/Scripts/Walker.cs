using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour
{
    [SerializeField]
    List<IK_LegArmature> legs = null; //A list of all the walkers legs

    public List<legPair> legPairs;

    public float setWalkerHeight = 7.0f; //Defined usual height of walker
    public float minWalkerHeight = 4.0f; //The minimum height the walker is allowed to go to

    public float minDistanceFromRoof = 1.0f; //How far from an overhead obstacle the walker should attempt to be
    public float heightLeeway = 0.1f; //How far off the desired height the walker can be

    public float walkSpeed = 5.0f; //Speed walker will move 
    public float turnSpeed = 10.0f; //Speed walker will turn

    private Vector3 currentPosition; //Current position of walker
    private Vector3 currentVelocity; //Current velocity of walker

    private float currentDistanceFromFloor; //Current height above the floor
    private float currentDistanceFromRoof; //Current distance from overhead obstacle

    private float currentHeightLeeway; //How far away from the desired height the walker can currently be
    private float walkerHeight; //Current height the walker is aiming to be at

    private Vector3 directionToCastRay; //Storage for raycast direction

    private int i = 0;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        MoveWalker();
        UpdateWalkerBody();
        UpdateLegs();
        CheckQuit();
    }

    private void Init()
    {
        //Setup the starting current position
        currentPosition = transform.position;

        //Setup the starting height leeway
        currentHeightLeeway = heightLeeway;

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
            if (i + 1 < legs.Count)
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

        //Allow player to control walker with WASD
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            transform.position += transform.right * Input.GetAxisRaw("Horizontal") * walkSpeed * Time.deltaTime;
            transform.position += transform.forward * Input.GetAxisRaw("Vertical") * walkSpeed * Time.deltaTime;
        }

        //Rotate the walker with the arrow keys
        if (Input.GetAxisRaw("Turn") != 0)
        {
            transform.eulerAngles += Vector3.up * Input.GetAxisRaw("Turn") * turnSpeed * Time.deltaTime;
        }
    }

    private void UpdateWalkerBody()
    {
        RaycastHit hit;
        //currentHeightLeeway = heightLeeway;
        directionToCastRay = new Vector3(currentVelocity.x, (Mathf.Sin(Mathf.Acos(currentVelocity.magnitude / (walkerHeight * (currentVelocity.magnitude + 1)))) * (walkerHeight * (currentVelocity.magnitude + 1))), currentVelocity.z).normalized;

        //Lower the walker down when in motion
        if (currentVelocity.magnitude > 0 && walkerHeight == setWalkerHeight)
        {
            walkerHeight -= heightLeeway * 4;
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
                //If the walker is above their minimum height
                if (currentDistanceFromFloor > minWalkerHeight)
                {
                    //Increase the current amount of height leeway
                    currentHeightLeeway += heightLeeway;
                    //And lower the walker
                    transform.position -= Vector3.up * walkSpeed * Time.deltaTime;
                }
            }
            //Otherwise if the distance to the roof is greater than the minimum distance or the walker is lower than their minimum height and the current height leeway is bigger than the base leeway
            else if ((currentDistanceFromRoof > minDistanceFromRoof || (currentDistanceFromFloor < minWalkerHeight)) && currentHeightLeeway > heightLeeway)
            {
                //decrease the current height leeway
                currentHeightLeeway -= heightLeeway;
            }
        }
        //If the ray hits nothing reset the current distance from roof
        else
        {
            currentDistanceFromRoof = minDistanceFromRoof + 1;
            //Also reset the current height leeway
            currentHeightLeeway = heightLeeway;
        }

        //Fire a ray down and forwards to check for distance from the floor
        if (Physics.Raycast(transform.position, new Vector3(directionToCastRay.x, -directionToCastRay.y, directionToCastRay.z), out hit, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
        {
            //Store the current distance to the floor
            currentDistanceFromFloor = Vector3.Distance(transform.position, hit.point);
            //If the distance to the floor is less than the walker height - the height leeway
            if (currentDistanceFromFloor < walkerHeight - currentHeightLeeway)
            {
                //Raise the walker's position up
                transform.position += Vector3.up * walkSpeed * Time.deltaTime;
            }
            //Otherwise if the distance to the floor is greater than the walkerHeight + currentLeeway
            else if (currentDistanceFromFloor > walkerHeight + currentHeightLeeway)
            {
                transform.position -= Vector3.up * walkSpeed * Time.deltaTime;
            }
        }
    }

    private void UpdateLegs()
    {
        if (i >= 100)
        {
            i -= 100;
        }
        //For all of the pairs of legs
        foreach (legPair pair in legPairs)
        {
            //Check that both legs are grounded before lifting a leg
            if (pair.legOne.IsGrounded() && pair.legTwo.IsGrounded())
            {
                //Update both legs raycasts
                pair.legOne.CheckMovement(currentVelocity);
                pair.legTwo.CheckMovement(currentVelocity);

                //If leg two is grounded and it's leg one's turn to move or if both legs in the pair are the same leg
                if ((pair.legTwo.IsGrounded() && (i % 2 == 0)) || pair.legTwo == pair.legOne)
                {
                    //Update leg one's target positioning
                    pair.legOne.UpdateTarget();
                    i++;
                }
                if ((pair.legOne.IsGrounded() && (i % 2 != 0)) || pair.legOne == pair.legTwo)
                {
                    //Otherwise if leg one is grounded and it's leg two's turn to move or if both legs in the pair are the same leg update leg two's positioning
                    pair.legTwo.UpdateTarget();
                    i++;
                }
            }
        }
    }

    private void CheckQuit()
    {
        //If player presses Escape quit game
        if (Input.GetButton("Quit"))
        {
            Application.Quit();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        //Draw ray cast to roof
        Gizmos.DrawLine(transform.position, transform.position + directionToCastRay * (walkerHeight * (currentVelocity.magnitude + 1)));
        //Draw ray cast to floor
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(directionToCastRay.x, -directionToCastRay.y, directionToCastRay.z) * (walkerHeight * (currentVelocity.magnitude + 1)));
    }

    //Struct for storing legArmatures
    public struct legPair
    {
        public IK_LegArmature legOne;
        public IK_LegArmature legTwo;
    }
}
