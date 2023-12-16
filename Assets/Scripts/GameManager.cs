using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>(); //tracks player with their id

    public GameObject localPlayer, playerPrefab;  //Local player is current window player and playerprefab is for new player added

    public static GameObject localPlayerSpawn;

    public delegate void PlayerInformation(Dictionary<int, PlayerManager> playerInfo);
    public static PlayerInformation playerInfo;

    //Creating a singleton  of this object and if other exists delete the script
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exist!");
            Destroy(this);
        }
    }
    public void SpawnPlayer(int id,string username,Vector3 position, Quaternion rotation)
    {
        GameObject player;
        if(id==Client.instance.id)
        {
            player = Instantiate(localPlayer, position, rotation);
            localPlayerSpawn = player;
        }
        else
        {
            player = Instantiate(playerPrefab, position, rotation);
        }

        player.GetComponent<PlayerManager>().id = id;
        player.GetComponent<PlayerManager>().username = username;
        if(!players.ContainsKey(id))
        players.Add(id, player.GetComponent<PlayerManager>());

        //Invoking players dictionary
        playerInfo(players);
    }
}
