using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public GameObject playerPrefab;

    public List<RoomController> rooms = new List<RoomController>();

    private List<PlayerController> players = new List<PlayerController>();

    private int currentActive = 0;

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


        // Create and position players
        for (int i = 0; i < selectPlayerCount; i++)
        {
            GameObject player = Instantiate(playerPrefab);
            players.Add(player.GetComponent<PlayerController>());
            Debug.Log("rooms[currentActive].playerSpawnPositions[i].transform.position" + rooms[currentActive].playerSpawnPositions[i].transform.position);
            player.transform.position = rooms[currentActive].playerSpawnPositions[i].transform.position;
        }
       
    }
}
