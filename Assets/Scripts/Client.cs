using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


//This is a class which combines with two classes called tcp and udp. These are two methods to send and receive the data in different terms
public class Client : MonoBehaviour
{
    //instance of the client where no more than this class should be created 
    public static Client instance;

    public TCP tcp;
    public UDP udp;

    public static int dataBufferSize = 4096;

    public int port = 11000;
    public string ip = "127.0.0.1";
    public int id = 1;

    private bool isConnected = false;
    
    //this is delegate function which use to track the packets 
    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int,PacketHandler> packetHandler;

    private void Awake()
    {
        if(instance==null)
        {
            instance = this;
        }
        else if(instance!=this)
        {
            Debug.Log("Instance already exist!");
            Destroy(this);
        }
     
    }
    private void OnApplicationQuit()
    {
       //when user quit the game window it should ensure that this disconnects the from the server properly
        TCPUDPDisconnect();
    }
    //function to connect to the server where tcpclient connects
    public void ConnectToServer()
    {
        if (isConnected)
            return;
    
        isConnected = true; //if connect to server should trigger true as client connected
        InitializeClientData();
        tcp.Connect();
       
    }

    //this is class for udp client which is fast, reliable but does not guarantees to send all the data
    public class UDP
    {
        public UdpClient udpSocket;  //class declaration of udpclient
        public IPEndPoint endpoint;  //endpoint which consists of ip address and port

        //constructor of udp class to initiliaze the endpoint which consist of port and ip address which needs to be connected
        public UDP()
        {
            endpoint = new IPEndPoint(IPAddress.Parse(instance.ip),instance.port);
        }
        //this function use to the connect the udp client by inserting the local port and connect to the server
        public void UDPConnect(int _port)
        {
            udpSocket = new UdpClient(_port);

            Debug.LogError(endpoint);
            udpSocket.Connect(endpoint);
            udpSocket.BeginReceive(ReceiveCallBack,null);

            using (Packet packet = new Packet())
            {
                SendData(packet);
            }
        }

        //this function used to send the data to the server using packets in packets class
        public async  void SendData(Packet packet)
        {
            try
            {
                //inserting the id of the client in the starting of the array in packet which is readablebuffer
                packet.InsertInt(instance.id);
                if (udpSocket != null && udpSocket.Client.Connected)
                {
                    udpSocket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }

            }
            catch (Exception exp)
            {
                Debug.Log($"Error sending the data to server via UDP:{exp}");
            }
        }

        //callback function to receive the data from the server and loops continuosly 
        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                byte[] receiveData = udpSocket.EndReceive(ar, ref endpoint);
                udpSocket.BeginReceive(ReceiveCallBack, null);
                if (receiveData.Length < 4)
                {
                    instance.TCPUDPDisconnect();
                    return;
                }
                HandleData(receiveData);
            }
            catch(Exception exp)
            {
                UDPDisconnect();
            }
        }


        //Handles the data in udp class where it will check the data in a packet form
        private void HandleData(byte[] receiveData)
        {
           using(Packet packet=new Packet(receiveData))
            {
                int packetLength = packet.ReadInt(); //this method the read the int numbers of 4bytes and then increment the byte position with 4

                receiveData = packet.ReadBytes(packetLength); //after receiving the integer values then it reads the data in bytes from packet class 

            }
           //by using the thread manager it optimize the cpu performance
            ThreadManager.ExecuteOnMainThread(() => 
            {
                using (Packet packet = new Packet(receiveData))
                {
                
                    int packetID=packet.ReadInt(); //after fetching the data from packet and its length and now its turn to fetch packet id from packet 
                    packetHandler[packetID](packet);  //invoking the packet handler method with the packet id
                }
            });
        }
        private void UDPDisconnect()
        {
            instance.TCPUDPDisconnect();
            endpoint = null;
        }
    }

    //this is a class which establishes a connection between a server and client and guarantees to send all the data and also receives by client or server 
    public class TCP
    {
        public TcpClient socket; //declare tcpclient clas

        private NetworkStream stream; //use to receive and send the data

        private byte[] receiveBuffer;  // data which will receive from other party like client or server but in this case it is a client

        public Packet receivedData;   //data in the packet form

        //function to initialize the tcp client class and start connecting with the server
        public void Connect()
        {
            socket = new TcpClient()
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize,
            };
            receiveBuffer = new byte[dataBufferSize];
            
            socket.BeginConnect(instance.ip, instance.port,ConnectCallBack,socket);
        }

        //this is a callback function
        private void ConnectCallBack(IAsyncResult ar)
        {
            socket.EndConnect(ar);
            if (!socket.Connected)
                return;
            stream = socket.GetStream();
            receivedData = new Packet();
            stream.BeginRead(receiveBuffer,0,dataBufferSize,ReceiveCallBack,null);
        }
        //this is a callback function which received data continuosly from the server
        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                int byteLength = stream.EndRead(ar);

                Debug.LogError("receivedata:" + byteLength);
                if (byteLength <= 0)
                {
                    instance.TCPUDPDisconnect();
                    return;
                }
                   
                byte[] receiveData = new byte[byteLength];
                Array.Copy(receiveBuffer, receiveData, byteLength);
                
                receivedData.Reset(HandleData(receiveData));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallBack, null);
               
            }
          catch(Exception exp)
            {
                TCPDisconnect();    
            }
        }
        //this is to handle the data should be receive in a proper no data should be left
        private bool HandleData(byte[] receiveData)
        {
            int packetLength = 0;
            receivedData.SetBytes(receiveData);
           
            if (receivedData.UnreadLength()>=4)
            {
                packetLength = receivedData.ReadInt();

                if (packetLength <= 0)
                    return true;
            }
            while (packetLength>0 && packetLength<=receivedData.UnreadLength())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetID = packet.ReadInt();
                        packetHandler[packetID](packet);
                      
                    }
                });
                packetLength = 0;
                if(receivedData.UnreadLength()>=4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }
            if (packetLength <= 0)
                return true;
            return false;
        }
        //this function use to send the packet to the server
        public void SendData(Packet packet)
        {
            try
            {
                if(socket!=null)
                {
                    stream.BeginWrite(packet.ToArray(),0,packet.Length(),null,null);
                }
            }
            catch(Exception exp)
            {
                Debug.LogError($"Error sending the  data to the server via TCP {exp}");
            }
        }
        private void TCPDisconnect()
        {
            instance.TCPUDPDisconnect();
            socket = null;
            stream = null;
            receiveBuffer = null;
            receivedData = null;
        }
    }
    //Properly dispose and disconnect connecting methods as udp and tcp
    public void TCPUDPDisconnect()
    {
        if(isConnected)
        {
            isConnected = false;
            Debug.Log("Client is disconnected!");
            DestroyImmediate(GameManager.players[instance.id].gameObject);
            GameManager.players.Remove(instance.id);
            tcp.socket.Close();
            udp.udpSocket.Close();
       
        


        }
    }
    //this function is just the initialize the packet handler dictionary where welcome message sends 
    private void InitializeClientData()
    {
        tcp = new TCP();
        udp = new UDP();
        packetHandler = new Dictionary<int, PacketHandler>()
            {
                {(int)ServerPackets.welcome,ClientHandle.Welcome},
                {(int)ServerPackets.spawnPlayer,ClientHandle.SpawnPlayer},
                {(int)ServerPackets.playerPostion,ClientHandle.PlayerPosition},
                {(int)ServerPackets.playerRotation,ClientHandle.PlayerRotation},
            };
        Debug.Log("Initialized Packets");
    }
}

