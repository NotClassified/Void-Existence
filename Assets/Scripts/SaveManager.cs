using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveManager
{
    public static void SaveData()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/progressData.ve";
        FileStream stream = new FileStream(path, FileMode.Create);

        SavedData data = new SavedData();

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SavedData LoadData()
    {
        string path = Application.persistentDataPath + "/progressData.ve";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SavedData data = formatter.Deserialize(stream) as SavedData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogWarning("File not found in " + path);
            return null;
        }
    }
}
