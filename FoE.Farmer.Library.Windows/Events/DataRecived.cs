using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoE.Farmer.Library.Windows.Events
{
    public class DataRecivedEventArgs : EventArgs
    {
        public string Data { get; set; }
        public string Id { get; set; }

    }
}
