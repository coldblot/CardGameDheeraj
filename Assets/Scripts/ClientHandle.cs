using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
   public static void Welcome(Packet packets)
    {
        string msg = packets.ReadString();
        int myid = packets.ReadInt();
        Debug.Log($"Message from the server:{msg}");
        Client.instance.id = myid;
        ClientSend.WelcomeReceived();
        Client.instance.udp.UDPConnect(((IPEndPoint)(Client.instance.tcp.socket.Client.LocalEndPoint)).Port);
    }

    public static void SpawnPlayer(Packet packets)
    {
        int id = packets.ReadInt();
        string username = packets.ReadString();
        Vector3 position = packets.ReadVector();
        Quaternion rotation = packets.ReadQuaternion();
      
        GameManager.instance.SpawnPlayer(id,username,position,rotation);
    }

    public static void PlayerPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector();

        if (GameManager.players.ContainsKey(id))
            GameManager.players[id].transform.position = position;
    }

    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();

       if( GameManager.players.ContainsKey(id))
        GameManager.players[id].transform.rotation = rotation;
    }
}
