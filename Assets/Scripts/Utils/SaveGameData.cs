using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;

namespace Utils {

    [Serializable]
    public class CoreData {
        public List<string> unlockedJokers;
        public int globalScore;
        
        public int prLevelIndex;
        
        public int prScore;
        public int prScoreLevelIndex;
        
        public CoreData(){ }

        public CoreData(CoreData coreData) {
            if (coreData?.unlockedJokers != null) unlockedJokers = new List<string>(coreData?.unlockedJokers);
            if (coreData == null) return;
            globalScore = coreData.globalScore;

            prLevelIndex = coreData.prLevelIndex;

            prScore = coreData.prScore;
            prScoreLevelIndex = coreData.prScoreLevelIndex;
        }
    }
    
    public static class SaveGameData {

        public const string MAIN_SAVE_FILENAME = "saveDataFile";
        
        public static CoreData coreData;

        /// <summary>
        /// This saves the current CoreData only. Useful when I manipulate the core data directly (e.g: Joker Shop)
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="completeCallback"></param>
        public static void Save(string filename, UnityAction completeCallback = null) {
            var binaryFormatter = new BinaryFormatter();
            var fileStream = File.Create(Application.persistentDataPath + $"/{filename}.dat");
            
            binaryFormatter.Serialize(fileStream, coreData);
            fileStream.Close();
            
            completeCallback?.Invoke();
        }

        /// <summary>
        /// This saves the game data based on the RunStats static class. Useful to save data after a run.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="completeCallback"></param>
        public static void SaveRunStats(string filename, UnityAction completeCallback = null) {
            var binaryFormatter = new BinaryFormatter();
            Debug.Log("Application.persistentDataPath + $\"/{filename}.dat\": " + Application.persistentDataPath + $"/{filename}.dat");
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
            
            completeCallback?.Invoke();
        }

        public static void Load(string filename, UnityAction completeCallback = null) {
            if (!File.Exists(Application.persistentDataPath + $"/{filename}.dat")) return;
            Debug.Log("Application.persistentDataPath + $\"/{filename}.dat\": " + Application.persistentDataPath + $"/{filename}.dat");

            var binaryFormatter = new BinaryFormatter();
            var fileStream = File.Open(Application.persistentDataPath + $"/{filename}.dat", FileMode.Open);
            
            var loadedCoreData = (CoreData)binaryFormatter.Deserialize(fileStream);
            fileStream.Close();
            
            coreData = new CoreData(loadedCoreData);
            completeCallback?.Invoke();
        }
    }
}
