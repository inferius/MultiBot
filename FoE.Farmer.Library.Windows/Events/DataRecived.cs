using System;

namespace FoE.Farmer.Library.Windows.Events
{
    public class DataRecivedEventArgs : EventArgs
    {
        public string Data { get; set; }
        public string Id { get; set; }

    }
}
