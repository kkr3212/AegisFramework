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





    public delegate void EventHandler_Connected(bool connected);
    public delegate void EventHandler_Disconnected();
    public delegate void EventHandler_Send(int transBytes);
    public delegate void EventHandler_Received(StreamBuffer buffer);
    public delegate bool EventHandler_IsValidPacket(StreamBuffer buffer, out int packetSize);
}
