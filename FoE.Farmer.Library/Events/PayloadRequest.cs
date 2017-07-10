using System;

namespace FoE.Farmer.Library.Events
{
    public class PayloadRequestEventArgs : EventArgs
    {
        public Payload Payload { get; set; }
    }
}
