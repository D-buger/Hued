using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [System.Serializable]
    public class MonsterData
    {
        public Vector3 initialPosition;
        public MonsterStat data;
    }

    public static void SaveMonsters(Monster[] monsters)
    {
        MonsterData[] monsterDataArray = new MonsterData[monsters.Length];

        for (int i = 0; i < monsters.Length; i++)
        {
            Monster monster = monsters[i];
            MonsterData monsterData = new MonsterData();
            monsterData.initialPosition = monster.transform.position;
            monsterData.data = monster.baseStat;

            monsterDataArray[i] = monsterData;
        }

        string jsonData = JsonUtility.ToJson(monsterDataArray);
        PlayerPrefs.SetString("MonsterData", jsonData);
        PlayerPrefs.Save();
    }

    public static void LoadMonsters(Monster[] monsters)
    {
        string jsonData = PlayerPrefs.GetString("MonsterData");
        if (string.IsNullOrEmpty(jsonData))
            return;

        MonsterData[] monsterDataArray = JsonUtility.FromJson<MonsterData[]>(jsonData);

        for (int i = 0; i < monsters.Length && i < monsterDataArray.Length; i++)
        {
            Monster monster = monsters[i];
            MonsterData monsterData = monsterDataArray[i];
            monster.transform.position = monsterData.initialPosition;
            monster.baseStat = monsterData.data;
        }
    }
}