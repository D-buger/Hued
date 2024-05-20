using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [System.Serializable]
    public class MonsterData
    {
        public Vector3 initialPosition;
    }

    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "monsterData.json");
    }

    public static void SaveMonsters(Monster[] monsters)
    {
        MonsterData[] monsterDataArray = new MonsterData[monsters.Length];

        for (int i = 0; i < monsters.Length; i++)
        {
            Monster monster = monsters[i];
            MonsterData monsterData = new MonsterData();
            monsterData.initialPosition = monster.transform.position;

            monsterDataArray[i] = monsterData;
        }

        string jsonData = JsonUtility.ToJson(new Wrapper<MonsterData> { Items = monsterDataArray }, true);
        File.WriteAllText(GetSavePath(), jsonData);
    }

    public static void LoadMonsters(Monster[] monsters)
    {
        string path = GetSavePath();
        if (!File.Exists(path))
            return;

        string jsonData = File.ReadAllText(path);
        Wrapper<MonsterData> wrapper = JsonUtility.FromJson<Wrapper<MonsterData>>(jsonData);
        MonsterData[] monsterDataArray = wrapper.Items;

        for (int i = 0; i < monsters.Length && i < monsterDataArray.Length; i++)
        {
            Monster monster = monsters[i];
            MonsterData monsterData = monsterDataArray[i];
            monster.transform.position = monsterData.initialPosition;
        }
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}