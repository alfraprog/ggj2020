using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{

    public List<GameObject> playerSpawnPositions = new List<GameObject>();
    private int playerExitCount = 0;
    public GameObject roomActivation;

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


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            // disable the player when exiting the room
            other.gameObject.GetComponent<PlayerController>().DisableMovement();

            playerExitCount++;
            Debug.Log("Detected the player "+ other.gameObject.GetComponent<PlayerController>().playerIndex);

            if (playerExitCount >= GameController.instance.playerCount)
            {
                Debug.Log("Leaving this town...");
                GameController.instance.TransitionRoom(this);              
            }
        }
    }

    public void DisableActivationArea()
    {
        roomActivation.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
