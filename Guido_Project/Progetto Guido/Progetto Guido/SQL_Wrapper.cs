using System;
using System.Threading;
using Microsoft.SPOT;
using GHI.SQLite;
using Gadgeteer.Modules.GHIElectronics;
using System.IO;

namespace Progetto_Guido
{
    public class SQL_Wrapper
    {
        SDCard sd;
        Database db;

        public SQL_Wrapper(SDCard sd)
        {
        this.sd = sd;
        }

        /// <summary>
        /// Create Database on SDcard
        /// </summary>
        public void setup_database()
        {
            Thread.Sleep(1000);
            try
            {
                db = new Database(sd.StorageDevice.RootDirectory + "\\Database.dat");
                db.ExecuteNonQuery("CREATE Table IF NOT EXISTS Misure(sensor TEXT, sensor_id INTEGER, iso_timestamp TEXT, value REAL, status TEXT, packet INTEGER)");
            }
            catch (Exception error)
            {
                Debug.Print("Error on DB"+error.Message);
               // File.Copy(Path.Combine(sd.StorageDevice.RootDirectory, "DatabaseBackup.dat"), Path.Combine(sd.StorageDevice.RootDirectory, "Database.dat"), true);
            }
        }

        /// <summary>
        /// Insert Misure into Database
        /// </summary>
        /// <param name="m">Misura</param>
        /// <param name="numberP">Number of packet</param>
        public void insert(Misure m,int numberP)
        {
            db.ExecuteNonQuery("INSERT INTO Misure(sensor,sensor_id, iso_timestamp, value, status, packet) VALUES('" + m.sensor + "','" + m.sensor_id + "','" + m.iso_timestamp + "','" + m.value + "','" + m.status + "','" + numberP + "')");
        }

        /// <summary>
        /// Dispose Database
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }

        /// <summary>
        /// Extract Data from Database(last two minutes)
        /// </summary>
        /// <param name="numberP">Package number to be extract</param>
        /// <returns></returns>
        public ResultSet get_last2mindata(int numberP)
        {
            DateTime intero = DateTime.Now;
            intero = intero.AddMinutes(-2);
            db.ExecuteNonQuery("UPDATE Misure SET packet =" + numberP + " WHERE packet=0");
            ResultSet res = db.ExecuteQuery("SELECT * FROM Misure WHERE packet="+numberP);
            return res;
        }

        /// <summary>
        /// Delete Database misure of Paket 
        /// </summary>
        /// <param name="numPacket">Package number to be deleted</param>
        public void DeleteMisuresRecived(int numPacket){
            try
            {
                db.ExecuteNonQuery("DELETE FROM Misure WHERE packet=" + numPacket);
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
            }
            
        }
    }
}
