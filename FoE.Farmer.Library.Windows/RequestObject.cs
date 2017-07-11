using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoE.Farmer.Library.Windows.Events;

namespace FoE.Farmer.Library.Windows
{
    public class RequestObject
    {
        public static event DataRecivedHandler DataRecived;
        public delegate void DataRecivedHandler(RequestObject m, Events.DataRecivedEventArgs e);

        [DebuggerHidden]
        public void setData(string data, string id)
        {
            DataRecived?.Invoke(this, new DataRecivedEventArgs { Data = data, Id = id});
        }

        public void setError(string data)
        {
            DataRecived?.Invoke(this, new DataRecivedEventArgs { Data = data, IsError = RecivedDataType.Error});
        }
        
    }
}
