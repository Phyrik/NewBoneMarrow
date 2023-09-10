using Newtonsoft.Json;

public class SaveData
{
    // scene
    public string SceneName { get; set; }

    // main character movement and location data
    public float MainCharacterPosX { get; set; }
    public float MainCharacterPosY { get; set; }
    public bool DoubleJumpUsed { get; set; }
    public float MainCharacterVelX { get; set; }
    public float MainCharacterVelY { get; set; }

    public string Serialise()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static SaveData Deserialise(string serialisedString)
    {
        return JsonConvert.DeserializeObject<SaveData>(serialisedString);
    }
}