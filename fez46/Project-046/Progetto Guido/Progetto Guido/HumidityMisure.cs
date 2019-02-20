using System;
using Microsoft.SPOT;

namespace Progetto_Guido
{
    public class HumidityMisure: Misure
    {       
        public HumidityMisure(float h, string stat)
        {
            value = h;
            sensor = "humidity";
            status = stat;
            sensor_id = 2;
        }
    }
}
