using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lidgren.Network;
using EmpireAttackServer.Networking.NetworkMessages;

public class SimpleClient : MonoBehaviour
{
    #region Private Fields

    // Client Object
    private static NetClient Client;

    private NetPeerConfiguration Config;

    private bool isConnected = false;

    #endregion Private Fields

    #region Private Methods

    // Use this for initialization
    public void Connect()
    {
        // Create new instance of configs. Parameter is "application Id". It has to be same on client and server.
        Config = new NetPeerConfiguration("game");
        // Create new client, with previously created configs
        Client = new NetClient(Config);
        // Create new outgoing message
        NetOutgoingMessage outmsg = Client.CreateMessage();
        // Start client
        Client.Start();

        // Write byte ( first byte informs server about the message type ) ( This way we know, what kind of variables to read )
        outmsg.Write((byte)PacketTypes.LOGIN);

        // Write String "Name" . Not used, but just showing how to do it
        outmsg.Write("Test01");

        // Connect client, to ip previously requested from user
        Client.Connect("127.0.0.1", 14242, outmsg);

        Debug.Log("Client started");

        StartCoroutine(WaitForStartingInfo());
    }

    public void SendClientMessage()
    {
        NetOutgoingMessage outmsg = Client.CreateMessage();
        outmsg.Write((byte)PacketTypes.TEST);
        outmsg.Write("Whaddup?");

        Client.SendMessage(outmsg, NetDeliveryMethod.ReliableOrdered, 0);
    }

    // Update is called once per frame
    private void Update()
    {
        if (isConnected)
        {
            // Create new incoming message holder
            NetIncomingMessage inc;

            // While theres new messages
            //
            // THIS is exactly the same as in WaitForStartingInfo() function
            while ((inc = Client.ReadMessage()) != null)
            {
                if (inc.MessageType == NetIncomingMessageType.Data)
                {
                    if (inc.ReadByte() == (byte)PacketTypes.TEST)
                    {
                        Debug.Log(inc.ReadString());
                    }
                }
            }
        }
    }

    // Before main looping starts, we loop here and wait for approval message
    private IEnumerator WaitForStartingInfo()
    {
        // When this is set to true, we are approved and ready to go
        bool CanStart = false;

        // New incomgin message
        NetIncomingMessage inc;

        // Loop untill we are approved
        while (!CanStart)
        {
            // If new messages arrived
            while ((inc = Client.ReadMessage()) != null)
            {
                // Switch based on the message types
                switch (inc.MessageType)
                {
                    // All manually sent messages are type of "Data"
                    case NetIncomingMessageType.Data:

                        break;

                    case NetIncomingMessageType.DebugMessage:

                        break;

                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus sts = (NetConnectionStatus)inc.ReadByte();
                        Debug.Log("STATUS: " + sts);
                        if (sts == NetConnectionStatus.Connected)
                        {
                            CanStart = true;
                            Debug.Log("SUCCESSFULLY CONNECTED!");
                        }
                        break;

                    default:
                        // Should not happen and if happens, don't care
                        Debug.LogError(inc.ReadString() + " Strange message. Type: " + inc.MessageType);
                        break;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
        isConnected = true;
    }

    #endregion Private Methods
}