using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{

    public List<GameObject> playerSpawnPositions = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        if (playerSpawnPositions.Count < 1)
        {
            Debug.LogError("No spawn positions set in Room");
        }

        foreach (GameObject spawnPosition in playerSpawnPositions)
        {
            //"Hide placement graphic
            spawnPosition.SetActive(false);
        }
    }



    // Update is called once per frame
    void Update()
    {

    }
}
