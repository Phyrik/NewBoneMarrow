using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadAndSaveManagerBehaviour : MonoBehaviour
{
    public static string SaveDirectory { get; private set; }
    public static string SaveFilePath { get; private set; }
    public static LoadAndSaveManagerBehaviour Instance { get; private set; }
    public static bool NeedsLoaded { get; set; }
    public static SaveData CurrentSaveData { get; private set; }

    // Start is called before the first frame update
    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        SaveDirectory = Path.Join(Application.persistentDataPath, "bonemarrowsaves");
        SaveFilePath = Path.Join(SaveDirectory, "save.json");

        CurrentSaveData = GetCurrentSave();

        NeedsLoaded = true;
        SceneManager.LoadSceneAsync(CurrentSaveData.SceneName, LoadSceneMode.Single);
    }

    public static void Save(SaveData saveData)
    {
        Directory.CreateDirectory(SaveDirectory);
        File.WriteAllText(SaveFilePath, saveData.Serialise());
    }

    private SaveData GetCurrentSave()
    {
        string serialisedSaveData = File.ReadAllText(SaveFilePath);
        return SaveData.Deserialise(serialisedSaveData);
    }
}