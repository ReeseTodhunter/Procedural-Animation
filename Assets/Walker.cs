using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour
{
    [SerializeField]
    List<IK_LegArmature> legs = null; //A list of all the walkers legs

    public legPair[] legPairs;

    public float walkSpeed = 3;
    public bool turn = false;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        MoveWalker();
        UpdateLegs();
    }

    private void Init()
    {
        legPairs = new legPair[legs.Count / 2];
        for (int i = 0, p = 0; i < legPairs.Length - 1; i++, p += 2)
        {
            legPairs[i].legOne = legs[p];
            if (legs[p + 1] != null)
            {
                legPairs[i].legTwo = legs[p + 1];
            }
            else
            {
                legPairs[i].legTwo = legs[p];
            }
            
        }
    }

    private void MoveWalker()
    {
        transform.position += transform.right * Time.deltaTime * walkSpeed;
        if (turn)
        {
            transform.eulerAngles += Vector3.up * Time.deltaTime * (walkSpeed / 2);
        }
    }

    private void UpdateLegs()
    {
        for(int i = 0; i < legs.Count; i++)
        {
            //Check if the i is even
            if (i % 2 == 0)
            {
                if (legs[i + 1] != null && legs[i + 1].IsGrounded())
                {
                    legs[i].CheckMovement();
                }
            }
            else
            {
                if (legs[i - 1] != null && legs[i - 1].IsGrounded())
                {
                    legs[i].CheckMovement();
                }
            }
        }
    }

    public struct legPair
    {
        public IK_LegArmature legOne;
        public IK_LegArmature legTwo;
    }
}
