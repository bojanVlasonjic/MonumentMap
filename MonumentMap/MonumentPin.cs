using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonumentMap
{
    [Serializable]
    public class MonumentPin
    {
        //Location
        //Content - monument id
        public double latitude;
        public double longitude;

        public MonumentPin() { }

        public MonumentPin(Location location)
        {
            latitude = location.Latitude;
            longitude = location.Longitude;
        }

    }
}
