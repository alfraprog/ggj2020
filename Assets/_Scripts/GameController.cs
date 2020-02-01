using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public GameObject playerPrefab;

    public List<RoomController> rooms = new List<RoomController>();

    private List<PlayerController> players = new List<PlayerController>();

    private int currentActiveRoom = 0;
    internal int playerCount;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (rooms.Count < 1)
        {
            Debug.LogError("No rooms in game controller");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreatePlayers(int selectPlayerCount)
    {
        Debug.Log("Creating " + selectPlayerCount + "players");
        UIManager.instance.HidePlayerSelection();
        playerCount = selectPlayerCount;

        // Create and position players
        for (int i = 0; i < selectPlayerCount; i++)
        {
            GameObject player = Instantiate(playerPrefab);
            players.Add(player.GetComponent<PlayerController>());
            Debug.Log("rooms[currentActive].playerSpawnPositions[i].transform.position" + rooms[currentActiveRoom].playerSpawnPositions[i].transform.position);
            PositionPlayer(i);
        }
       
    }

    public void PositionPlayer(int playerIndex)
    {
        players[playerIndex].transform.position = rooms[currentActiveRoom].playerSpawnPositions[playerIndex].transform.position;
    }

    public void TransitionRoom(RoomController roomController)
    {
        Debug.Log("Transitioning ");
        currentActiveRoom++;
        if (currentActiveRoom >= rooms.Count)
        {
            currentActiveRoom = rooms.Count - 1;
        }
        CameraController.instance.target = rooms[currentActiveRoom].transform;

        rooms[currentActiveRoom].DisableActivationArea();

        for (int i = 0; i < players.Count; i++)
        {
            players[i].gameObject.SetActive(true);
            PositionPlayer(i);
        }
        
    }
}
