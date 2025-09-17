using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Utils
{
    public class SaveGameData : MonoBehaviour
    {

        public static void Save()
        {
            var binaryFormatter = new BinaryFormatter();
            var fileStream = File.Create(Application.persistentDataPath + "/playerData.dat");
            
            // manipulate data here
            
            // binaryFormatter.Serialize(fileStream, );
            fileStream.Close();
        }

        public static void Load()
        {
            if (!File.Exists(Application.persistentDataPath + "/playerData.dat")) return;

            var binaryFormatter = new BinaryFormatter();
            var fileStream = File.Open(Application.persistentDataPath + "/playerData.dat", FileMode.Open);
            
            // deserialize data here
            
            fileStream.Close();
            
            // use data here
        }
    
    }
}
