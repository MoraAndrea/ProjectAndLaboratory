using System;
using Microsoft.SPOT;

namespace Progetto_Guido
{
   public class TemperatureMisure : Misure
    {
        public TemperatureMisure(float temp, string stat)
        {
            value = temp;
            sensor = "temperature";
            status = stat;
            sensor_id = 1;
        }
    }
}
