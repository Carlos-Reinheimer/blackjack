using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Scriptable_Objects;
using UnityEngine;
using UnityEngine.Events;

namespace Utils {

    [Serializable]
    public class CoreData {
        public List<JokerCard> unlockedJokers;
        public int globalScore;
        
        public int prLevelIndex;
        
        public int prScore;
        public int prScoreLevelIndex;
        
        public CoreData(){ }

        public CoreData(CoreData coreData) {
            unlockedJokers = new List<JokerCard>(coreData.unlockedJokers);
            globalScore = coreData.globalScore;

            prLevelIndex = coreData.prLevelIndex;

            prScore = coreData.prScore;
            prScoreLevelIndex = coreData.prScoreLevelIndex;
        }
    }
    
    public static class SaveGameData {

        public static CoreData coreData;

        public static void Save(string filename, UnityAction callback = null) {
            var binaryFormatter = new BinaryFormatter();
            var fileStream = File.Create(Application.persistentDataPath + $"/{filename}.dat");
            
            // TODO: If I want to manipulate other files later, I need to change this.
            var newCoreData = new CoreData(coreData);
            newCoreData.globalScore += RunStats.CurrentScore;
            if (RunStats.CurrentRound > newCoreData.prLevelIndex) newCoreData.prLevelIndex = RunStats.CurrentRound;
            if (RunStats.CurrentScore > newCoreData.prScore) {
                newCoreData.prScore = RunStats.CurrentScore;
                newCoreData.prScoreLevelIndex = RunStats.CurrentRound;
            }
            
            binaryFormatter.Serialize(fileStream, newCoreData);
            fileStream.Close();
            
            callback?.Invoke();
        }

        public static void Load(string filename, UnityAction callback = null) {
            if (!File.Exists(Application.persistentDataPath + $"/{filename}.dat")) return;

            var binaryFormatter = new BinaryFormatter();
            var fileStream = File.Open(Application.persistentDataPath + $"/{filename}.dat", FileMode.Open);
            
            var loadedCoreData = (CoreData)binaryFormatter.Deserialize(fileStream);
            fileStream.Close();
            
            coreData = new CoreData(loadedCoreData);
            callback?.Invoke();
        }
    }
}
