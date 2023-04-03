using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization;

namespace Encore.Serializables
{
    [AddComponentMenu("Encore/Saves/Serialization Manager")]
    public class SerializationManager : MonoBehaviour
    {
        public static readonly string folderName = "/saves";

        public static bool canSave = true;

        public static bool Save(string saveName, object saveData)
        {
            if (!canSave) return false;

            BinaryFormatter formatter = GetBinaryFormatter();

            if (!Directory.Exists(Application.persistentDataPath + folderName))
            {
                Directory.CreateDirectory(Application.persistentDataPath + folderName);
            }

            string path = Application.persistentDataPath + folderName + "/" + saveName + ".dat";

            FileStream file = File.Create(path);

            formatter.Serialize(file, saveData);

            file.Close();

            Debug.Log("Saved to: " + saveName + ".dat");

            // Also keep the changes to save.dat; this file and save.dat will have the same data
            if (saveName != GameManager.DEFAULT_SAVE_FILE_NAME)
            {
                string _path = Application.persistentDataPath + folderName + "/" + GameManager.DEFAULT_SAVE_FILE_NAME + ".dat";
                BinaryFormatter _formatter = GetBinaryFormatter();
                FileStream _file = File.Create(_path);
                _formatter.Serialize(_file, saveData);
                _file.Close();
            }

            return true;
        }

        public static object Load(string saveName)
        {
            string path = Application.persistentDataPath + folderName + "/" + saveName + ".dat";

            if (!File.Exists(path))
            {
                return null;
            }

            BinaryFormatter formatter = GetBinaryFormatter();

            FileStream file = File.Open(path, FileMode.Open);

            try
            {
                object save = formatter.Deserialize(file);
                file.Close();
                return save;
            }
            catch
            {
                Debug.LogErrorFormat("Failed to load file at {0}", path);
                file.Close();
                return null;
            }
        }

        public static BinaryFormatter GetBinaryFormatter()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            SurrogateSelector selector = new SurrogateSelector();

            Vector3SerializationSurrogate vector3Surrogate = new Vector3SerializationSurrogate();
            QuaternionSerializationSurrogate quaternionSurrogate = new QuaternionSerializationSurrogate();

            selector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3Surrogate);
            selector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quaternionSurrogate);

            formatter.SurrogateSelector = selector;

            return formatter;
        }

        #region [For Debugging]

        public static object LoadCache()
        {
            string path = Application.persistentDataPath + folderName + "/" + GameManager.DEFAULT_SAVE_FILE_NAME + ".dat";

            if (!File.Exists(path))
            {
                return null;
            }

            BinaryFormatter formatter = GetBinaryFormatter();

            FileStream file = File.Open(path, FileMode.Open);

            try
            {
                object save = formatter.Deserialize(file);
                file.Close();
                return save;
            }
            catch
            {
                Debug.LogErrorFormat("Failed to load file at {0}", path);
                file.Close();
                return null;
            }
        }

        public static bool SetCache(object saveData, string otherSaveFile)
        {
            if (!canSave) return false;

            BinaryFormatter formatter = GetBinaryFormatter();

            if (!Directory.Exists(Application.persistentDataPath + folderName))
            {
                Directory.CreateDirectory(Application.persistentDataPath + folderName);
            }

            string path = Application.persistentDataPath + folderName + "/" + GameManager.DEFAULT_SAVE_FILE_NAME + ".dat";

            FileStream file = File.Create(path);

            formatter.Serialize(file, saveData);

            file.Close();

            return true;
        }

        #endregion
    }
}