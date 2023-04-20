using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ConstantManager : MonoBehaviour
{
    // 全局变量
    public int coin;
    // public string playerName;
    public List<Card> allCards = new List<Card>(); // 全部卡牌
    public List<Card> allUnlockCards = new List<Card>(); // 全部已解锁卡牌
    public List<Card> playerCardDeck = new List<Card>(); // 当前卡组
    public List<NPC> allCommonNPC; // 全部普通NPC
    public List<NPC> allSpecialNPC; // 全部特殊NPC
    public NPC MC; // 主角
    public List<Buff> allBuff;
    public List<Buff> allDebuff;

    [System.Serializable]
    public class NPCDataList
    {
        public List<NPC> npcList;
    }
    public NPCDataList npcDataList;

    [System.Serializable]
    public class CardDataList
    {
        public List<Card> cardList;
    }
    public CardDataList cardDataList;

    [System.Serializable]
    public class BuffData
    {
        public List<Buff> buffList;
    }
    public BuffData buffData;


    // Singleton模式
    private static ConstantManager _instance;
    public static ConstantManager Instance { get { return _instance; } }

    private void Awake()
    {
        // Singleton实例设置
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        // 这里你可以加载你的全局变量，例如从文件中
        LoadData();
    }

    public void SaveData()
    {
        // 在此处保存全局变量的值，例如将它们写入文件中
    }

    public void LoadData()
    {
        Debug.Log("ConstantManager: LoadData()");
        coin = PlayerPrefs.GetInt("coin", 0);
        // 加载NPC数据
        string NPCjsonPath = Path.Combine(Application.streamingAssetsPath, "Data/Characters.json");
        string NPCjsonString = File.ReadAllText(NPCjsonPath);
        npcDataList = JsonUtility.FromJson<NPCDataList>(NPCjsonString);
        allCommonNPC = npcDataList.npcList.FindAll(npc => npc.type == "common");
        allSpecialNPC = npcDataList.npcList.FindAll(npc => npc.type == "special");
        MC = npcDataList.npcList.Find(npc => npc.id == 1);

        // 加载卡组数据
        string cardJsonPath = Path.Combine(Application.streamingAssetsPath, "Data/Cards.json");
        string cardJsonString = File.ReadAllText(cardJsonPath);
        cardDataList = JsonUtility.FromJson<CardDataList>(cardJsonString);
        allCards = cardDataList.cardList;
        List<int> cardDeckIdArr = SaveManager.LoadCardDeck();
        foreach (int cardId in cardDeckIdArr)
        {
            Card cardData = cardDataList.cardList.Find(card => card.id == cardId);
            if (cardData != null)
            {
                playerCardDeck.Add(cardData);
            }
        }

        // 加载buff数据
        string buffJsonPath = Path.Combine(Application.streamingAssetsPath, "Data/Buff.json");
        string buffJsonString = File.ReadAllText(buffJsonPath);
        buffData = JsonUtility.FromJson<BuffData>(buffJsonString);
        allBuff = buffData.buffList;
        Debug.Log(allBuff);
        allDebuff = allBuff.FindAll(buff => buff.type == "debuff");
        Debug.Log(allDebuff);
    }


    // Function to return a random NPC with type "common"
    public NPC GetRandomCommonNPC()
    {
        if (allCommonNPC.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, allCommonNPC.Count);
            return allCommonNPC[randomIndex];
        }
        return null;
    }

    public NPC GetNPCDataByID(int ID)
    {
        if (npcDataList != null)
        {
            return npcDataList.npcList.Find(nd => nd.id == ID);
        }
        return null;
    }
}