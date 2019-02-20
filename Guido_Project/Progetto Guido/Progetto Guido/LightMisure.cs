using System;
using Microsoft.SPOT;

namespace Progetto_Guido
{
    public class LightMisure : Misure
    {
        public LightMisure(float m, string stat)
        {
            value = m;
            sensor = "lightsense";
            status = stat;
            sensor_id = 4;
        }
    }
}
