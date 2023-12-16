using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    //This is the method to send tcp data which ensures all data transfers
    private static void SendTcpData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.tcp.SendData(packet);
    }
    //This is the method to send udp data which will ensures all data transfers but it is faster than tcp
    private static void SendUdpData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.udp.SendData(packet);
    }
    //this method is use to send the message with client id and username to server after received the message from server
    public static void WelcomeReceived()
    {
        using (Packet packet = new Packet((int)(ClientPackets.welcomeReceived)))
        {
            packet.Write(Client.instance.id);  //packet will write client id 
            packet.Write(DataCommunicateServer.nameOfUser); //packet will write client username which he wrote in input field of unity
            SendTcpData(packet); //then data transfers through tcp protocol
        }
    }

    //This method continuosly send this input data to server as when or when not player is pressing the input
    public static void PlayerMovement(bool[] inputs)
    {
        //We are generating a packet class with a packet of playerMovement as a id
        using (Packet packet =new Packet((int)ClientPackets.playerMovement))
        {
            packet.Write(inputs.Length);  //packet writes the length of array of inputs  
            foreach (var input in inputs)
            {
                packet.Write(input);  //packet will write seperate inputs from inputs array  
            }
            packet.Write(GameManager.players[Client.instance.id].transform.rotation);

            SendUdpData(packet);
        }
    }
}
