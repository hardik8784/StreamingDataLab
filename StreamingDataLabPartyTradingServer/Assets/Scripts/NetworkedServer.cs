using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;

public class NetworkedServer : MonoBehaviour
{
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;

    
    public LinkedList<SharingRoom> SharingRooms;

    // Start is called before the first frame update
    void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelID = config.AddChannel(QosType.Reliable);
        unreliableChannelID = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, maxConnections);
        hostID = NetworkTransport.AddHost(topology, socketPort, null);

        SharingRooms = new LinkedList<SharingRoom>();
    }

    // Update is called once per frame
    void Update()
    {

        int recHostID;
        int recConnectionID;
        int recChannelID;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error = 0;

        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Connection, " + recConnectionID);
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                ProcessRecievedMsg(msg, recConnectionID);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnection, " + recConnectionID);
                PlayerDisconnected(recConnectionID);
                break;
        }

    }

    public void SendMessageToClient(string msg, int id)
    {
        byte error = 0;
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, id, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }

    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);

        string[] csv = msg.Split(',');

        int Signifier = int.Parse(csv[0]);

        if(Signifier == ClientToServerSignifiers.JoinSharingRoom)
        {
            string RoomToJoinName = csv[1];

            SharingRoom FoundSharingRoom = null;

            foreach(SharingRoom sr in SharingRooms)
            {
                if(sr.Name == RoomToJoinName)
                {
                   // hasBeenFound = true;

                    FoundSharingRoom = sr;
                   // sr.ConnectionIDs.AddLast(id);
                    Debug.Log("Added to Sharing Room");
                    break;
                }
            }

            if(FoundSharingRoom == null)
            {
               FoundSharingRoom = new SharingRoom();
                //FoundSharingRoom.ConnectionIDs = new LinkedList<int>(); 
               FoundSharingRoom.Name = RoomToJoinName;

               // FoundSharingRoom.ConnectionIDs.AddLast(id);
                SharingRooms.AddLast(FoundSharingRoom);
                Debug.Log("Created Sharing Room");
            }

            if(!(FoundSharingRoom.ConnectionIDs.Contains(id)))
            {
                FoundSharingRoom.ConnectionIDs.AddLast(id);
            }
            else
            {
                Debug.Log("Preventing Duplicate in Sharing Room");
            }

           
        }
    }
    public void PlayerDisconnected(int id)
    {
        SharingRoom FoundSR = FindSharingRoomWithConnectionID(id);

        //foreach(SharingRoom sr in SharingRooms)
        //{
        //    foreach(int pID in sr.ConnectionIDs)
        //    {
        //        if(pID == id)
        //        {
        //            FoundSR = sr;
        //        }
        //    }
        //}

        if(FoundSR != null)
        {
            FoundSR.ConnectionIDs.Remove(id);
            Debug.Log("Removing Player from Room");

            if (FoundSR.ConnectionIDs.Count == 0)
            {
                SharingRooms.Remove(FoundSR);
                Debug.Log("Removing Room from List");
            }
        }
    }


    public SharingRoom FindSharingRoomWithConnectionID(int id)
    {
        foreach (SharingRoom sr in SharingRooms)
        {
            foreach (int pID in sr.ConnectionIDs)
            {
                if (pID == id)
                {
                    return sr;
                }
            }
        }

        return null;
    }
}


public class SharingRoom
{
    public string Name;

    public LinkedList<int> ConnectionIDs;

    public SharingRoom()
    {
        ConnectionIDs = new LinkedList<int>();
    }
}

static public class ClientToServerSignifiers
{
    public const int JoinSharingRoom = 1;
}

static public class ServerToClientSignifiers
{

}