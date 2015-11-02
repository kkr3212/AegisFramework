using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Aegis.Client.Network
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





    public delegate void EventHandler_Connected(AegisClient ac, bool connected);
    public delegate void EventHandler_Disconnected(AegisClient ac);
    public delegate void EventHandler_Sent(AegisClient ac, int transBytes);
    public delegate void EventHandler_Received(AegisClient ac, StreamBuffer buffer);
    public delegate bool EventHandler_IsValidPacket(AegisClient ac, StreamBuffer buffer, out int packetSize);
}
