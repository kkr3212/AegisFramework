using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Aegis.Client
{
    public enum ConnectionStatus
    {
        Closed = 0,
        Connecting = 1,
        Connected = 2,
        Closing = 3
    }

    public enum MessageType
    {
        Connect = 0,
        Close,
        Disconnect,
        Send,
        Receive
    }





    public delegate void Event_Connect(Boolean connected);
    public delegate void Event_Disconnect();
    public delegate void Event_Send(Int32 transBytes);
    public delegate void Event_Receive(StreamBuffer buffer);
    public delegate bool ValidPacketHandler(StreamBuffer buffer, out Int32 packetSize);
}
