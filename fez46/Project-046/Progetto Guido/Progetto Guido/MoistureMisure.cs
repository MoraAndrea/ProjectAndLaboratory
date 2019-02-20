using System;
using Microsoft.SPOT;

namespace Progetto_Guido
{
    public class MoistureMisure: Misure
    {
        public MoistureMisure(float m, string stat)
        {
            value = m;
            sensor = "moisture";
            status = stat;
            sensor_id = 3;
        }
    }
}
