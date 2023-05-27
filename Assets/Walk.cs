using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : MonoBehaviour
{
    public float walkSpeed = 3;
    public bool turn = false;

    void Update()
    {
        transform.position += transform.right * Time.deltaTime * walkSpeed;
        if (turn)
        {
            transform.eulerAngles += Vector3.up * Time.deltaTime * (walkSpeed / 2);
        }
    }
}
