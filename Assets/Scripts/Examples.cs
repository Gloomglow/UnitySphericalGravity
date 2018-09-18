using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Examples : MonoBehaviour 
{
    GameObject objectA = new GameObject();
    GameObject objectB = new GameObject();
   

    public void ExampleFunction()
    {
        //Get outer point on a sphere// 
        Vector3 randomShereDirection = Random.onUnitSphere;

        //Radius of a Sphere//
        float sphereRadius = gameObject.transform.localScale.y;

        //Combine Normalized Vector & Radius to get point on sphere surface//
        Vector3 pointOnSphere = randomShereDirection * sphereRadius; 

        //Downward Direction Local//
        Vector3 localRotation = -gameObject.transform.up; 

        //Rotation Inwards to Globe// 
        Vector3 rotationDirection = (objectA.transform.position - objectB.transform.position).normalized;

        //Do Rotation//
        gameObject.transform.rotation = Quaternion.FromToRotation(gameObject.transform.up, rotationDirection) * gameObject.transform.rotation;

        //Smooth Rotation//
        gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, Quaternion.FromToRotation(gameObject.transform.up, rotationDirection) * gameObject.transform.rotation, 1f * Time.deltaTime);


    }

}
