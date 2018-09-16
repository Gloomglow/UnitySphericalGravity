using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Examples : MonoBehaviour 
{
    public void ExampleFunction()
    {
        //Get outer point on a sphere// 
        Vector3 rand = Random.onUnitSphere;


        //Rotate Inwards to Globe// 
        GameObject objectA = new GameObject();
        GameObject objectB = new GameObject();

        Vector3 rotationDirection = (objectA.transform.position - objectB.transform.position).normalized; 
    }

}
