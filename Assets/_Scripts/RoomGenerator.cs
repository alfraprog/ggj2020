using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{

    public GameObject startRoom;

    public GameObject[] potentialRooms;

    public GameObject endRoom;

    public GameObject bossRoom;

    public int numberOfRooms;

    public float roomWidthInTiles = 30f;

    // Start is called before the first frame update
    void Start()
    {
        if (potentialRooms.Length < 1)
            Debug.LogError("No potential rooms set up in generator");

        CreateRoom(Instantiate(startRoom, transform.position, transform.rotation));

        float offsetX = roomWidthInTiles;

        for (int i = 0; i < numberOfRooms; i++)
        {
            int randomPotentialRoom = Random.Range(0, potentialRooms.Length);
            CreateRoom(Instantiate(potentialRooms[randomPotentialRoom], new Vector3(offsetX, 0f, 0f), transform.rotation));
            offsetX += roomWidthInTiles;
        }
        
       if (bossRoom != null)
       {
            CreateRoom(Instantiate(bossRoom, new Vector3(offsetX, 0f, 0f), transform.rotation));
            offsetX += roomWidthInTiles;
       }

       CreateRoom(Instantiate(endRoom, new Vector3(offsetX, 0f, 0f), transform.rotation));
    }

    void CreateRoom(GameObject room)
    {
        GameController.instance.rooms.Add(room.GetComponent<RoomController>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
