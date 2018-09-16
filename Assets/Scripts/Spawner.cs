using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour 
{
    public GameObject[] spawnPrefabs;
    public GameObject[] activeGlobes;
    public int initialSpawnObjects; 

    private void Awake()
    {
        //Spawn objects//
        for(int i = 0; i < initialSpawnObjects; i++)
        {
            SpawnPrefab();
        }
    }

    public void SpawnPrefab()
    {
        //Pick Random Planet & Object//
        int prefabIND = Random.Range(0, spawnPrefabs.Length);
        int globeIND = Random.Range(0, activeGlobes.Length); 

        //Random Outward Direction//
        Vector3 randomDir = Random.onUnitSphere;
        
        //Starting Point (Planet Center)//
        Vector3 spawnPoint = activeGlobes[globeIND].transform.position;

        //Modify Point to edge of planet// 
        spawnPoint += (randomDir * activeGlobes[globeIND].transform.localScale.y); 

        //Outward Rotation for Object//
        Vector3 spawnRotation = (spawnPoint - activeGlobes[globeIND].transform.position).normalized;

        //Spawn And Rotate into Place// 
        GameObject addSpawn = Instantiate(spawnPrefabs[prefabIND], spawnPoint, new Quaternion(0,0,0,0));
        addSpawn.transform.rotation = Quaternion.FromToRotation(addSpawn.transform.up, spawnRotation) * addSpawn.transform.rotation;
    }



}
