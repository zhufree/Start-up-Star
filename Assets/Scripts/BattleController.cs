using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System.Linq;

public class BattleController : MonoBehaviour
{

    GameObject NPCObject; //当前来买东西的NPC Object
    public GameObject cardContainer; // canvas下的bag game object UI
    public GameObject slotLayout; // 放物品的网格game Object UI
    public TMP_Text tCustomName;
    public TMP_Text tItemName;
    public TMP_Text tRound;
    public TMP_Text tStatus;
    public TMP_Text tPatienceLevel;
    public TMP_Text tCurrentPrice;
    public TMP_Text tCustomHeart;
    public TMP_Text tNPCResist;
    public TMP_Text tCurrentCard;
    public TMP_Text tPlayerResist;
    public TMP_Text tNPCBuff;
    public TMP_Text tPlayerBuff;
    public TMP_Text tPlayerPressure;
    public CardSlot cardPrefab; // 与Prefab对应的数据结构 DATA
    public List<CardSlot> cardSlotList = new List<CardSlot>();

    NPC currentNPC; //当前来买东西的NPC对象，随机产生，和GameObject绑定
    Item itemToBuy = new Item("testItem", "test", 30, "null"); // 当前要卖的货物，后面改成选择
    float itemPrice; // 售价，随barging过程变化
    int priceIncrease = 0; // %涨价百分比，随NPC兴趣等级变化
    int patienceLevel = 12; // NPC耐心值，也就是剩余回合数
    int NPCResist = 0;

    int heartCount = 0; // NPC爱心值，累积到一定程度增加兴趣等级
    int currentHeartGoal = 2;
    int interestLevel = 0; // NPC兴趣等级，影响涨价百分比
    int playerResist = 0;

    int pressure = 0; // player压力值百分比

    List<Card> remainCards; // 卡组中剩余可用的牌，每一轮售卖重置
    List<Card> cardsInHand = new List<Card>(); // 手上的卡

    int currentRound = 0;
    int usedCardCount = 0; // 当前回合已打出的手牌数量

    // buffId, roundCount
    Dictionary<int, int> playerBuffDict = new Dictionary<int, int>(); // 玩家的buff和debuff，每一轮售卖重置
    Dictionary<int, int> NPCBuffDict = new Dictionary<int, int>(); // NPC的buff和debuff，每一轮售卖重置
    List<int> allDebuffIdList = new List<int>();
    // Singleton模式
    private static BattleController _instance;
    public static BattleController Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    private void Start()
    {
        StartCoroutine(Init());
    }
    private void Update()
    {
        refreshStatus();
    }

    private IEnumerator Init() {
        tStatus.text = "初始化数据...";
        allDebuffIdList = ConstantManager.Instance.allDebuff.Select(buff => buff.id).ToList();
        yield return new WaitForSeconds(1f);
        RandomNPC(); // 随机NPC
        remainCards = new List<Card>(ConstantManager.Instance.playerCardDeck); // 初始化卡组
        itemPrice = itemToBuy.price; // 初始化价格
        yield return new WaitForSeconds(1f);
        tStatus.text = "初始抽卡...";
        GetInitCards();
        tStatus.text = "玩家回合，请出卡";
    }

    private void refreshStatus() {
        tCurrentPrice.text = $"涨价百分比：{priceIncrease}%，物品当前价格为{itemPrice}";
        tRound.text = $"第{currentRound}回合";
        tPatienceLevel.text = $"顾客耐心值/剩余回合数为{patienceLevel}";
        tCustomHeart.text = $"心心数：{heartCount}";
        tPlayerResist.text = $"玩家防御值：{playerResist}";
        tNPCResist.text = $"顾客防御值：{playerResist}";
        tPlayerPressure.text = $"玩家压力值：{pressure}";
        tNPCBuff.text = "NPCBuff：";
        tPlayerBuff.text = "玩家Buff：";
        foreach (var id in NPCBuffDict.Keys.ToArray())
        {
            tNPCBuff.text += ConstantManager.Instance.getBuffByID(id).name + $"({NPCBuffDict[id]-1}),";
        }
        foreach (var id in playerBuffDict.Keys.ToArray())
        {
            tPlayerBuff.text += ConstantManager.Instance.getBuffByID(id).name + $"({playerBuffDict[id]-1}),";
        }
    }

    private void RandomNPC() {
        List<NPC> allCommonNPC = ConstantManager.Instance.allCommonNPC;
        currentNPC = allCommonNPC[Random.Range(0, allCommonNPC.Count)];
        tStatus.text = "随机生成顾客...";
        tCustomName.text = $"当前顾客是：{currentNPC.name}";
        tItemName.text = $"当前出售的物品是：{itemToBuy.name}，初始价格是{itemToBuy.price}";
    }

    private void GetInitCards() {
        currentRound += 1;
        // Get three or four random cards from the player's card deck and add them to the cardsInHand list
        for (int i = 0; i < 3 || i < 4; i++) {
            if (remainCards.Count == 0) {
                break;
            }
            int randomIndex = Random.Range(0, remainCards.Count);
            cardsInHand.Add(remainCards[randomIndex]);
            remainCards.RemoveAt(randomIndex);
        }
        refreshCard();
    }

    private void refreshCard() {
        cardSlotList.Clear();
        foreach (Card card in cardsInHand) {
            CardSlot newCard = Instantiate(cardPrefab, slotLayout.transform.position, 
                Quaternion.identity);
            newCard.SetItem(card);
            newCard.gameObject.transform.SetParent(slotLayout.transform);
            cardSlotList.Add(newCard);
        }
    }

    private void GetOneMoreCard() {
        Debug.Log("GetOneMoreCard()");
        tStatus.text = "抽卡...";
        if (remainCards.Count == 0) {
            return;
        }
        int randomIndex = Random.Range(0, remainCards.Count);
        Card newCard = remainCards[randomIndex];
        cardsInHand.Add(newCard);
        CardSlot newCardSlot = Instantiate(cardPrefab, slotLayout.transform.position, 
                Quaternion.identity);
        newCardSlot.SetItem(newCard);
        newCardSlot.gameObject.transform.SetParent(slotLayout.transform);
        cardSlotList.Add(newCardSlot);
        remainCards.RemoveAt(randomIndex);
    }

    private void GetSpecificCard(int cardId) {

    }

    public void ResetCardSelectStatus() {
        foreach (CardSlot slot in cardSlotList) {
            slot.bg.enabled = false;
        }
    }

    bool finishFlag = false;
    public void UseCard(CardSlot cardSlot) {
        Card currentCard = cardSlot.cardItem;
        tCurrentCard.text = $"打出卡牌[{currentCard.name}]";
        cardSlotList.Remove(cardSlot);
        Destroy(cardSlot.gameObject);
        Debug.Log("计算用户兴趣");
        patienceLevel -= currentCard.round;
        if (patienceLevel < 0) {
            // round end
            FinishDeal();
        }
        heartCount += currentCard.heart;
        playerResist += currentCard.resist;
        CaculateHeart();
        Debug.Log("计算卡片效果");
        CardEffect(currentCard); // 卡片效果有可能结束交易
        if (!finishFlag) {
            Debug.Log("计算BUFF效果");
            int[] playerBuffkeysArray = playerBuffDict.Keys.ToArray();
            int[] NPCBuffkeysArray = NPCBuffDict.Keys.ToArray();
            Debug.Log(playerBuffDict);
            foreach (int key in playerBuffkeysArray)
            {
                BuffEffect(0, key);
            }
            foreach (int key in NPCBuffkeysArray)
            {
                BuffEffect(1, key);
            }
            usedCardCount += 1;
        } else {
            FinishDeal();
        }
    }

    public void FinishDeal() {
        if (finishFlag) {
            tStatus.text = $"交易结束，获得{itemPrice}";
        } else {
            tStatus.text = $"顾客耐心值为{patienceLevel}，交易失败";
        }
        cardsInHand.Clear();
        refreshCard();
    }

    public void FinishRound() {
        tStatus.text = "回合结束";
        currentRound += 1;
        usedCardCount = 0;
        // buff持续回合数-1
        // target 0 player, 1 NPC
        int[] playerBuffkeysArray = playerBuffDict.Keys.ToArray();
        int[] NPCBuffkeysArray = NPCBuffDict.Keys.ToArray();
        foreach (int key in playerBuffkeysArray) {
            if (playerBuffDict[key] > 1) {
                playerBuffDict[key] -= 1;
            } else {
                playerBuffDict.Remove(key);
            }
        }
        foreach (int key in NPCBuffkeysArray) {
            if (NPCBuffDict[key] > 1) {
                NPCBuffDict[key] -= 1;
            } else {
                NPCBuffDict.Remove(key);
            }
        }
        
        GetOneMoreCard();
    }
    
    private void CaculateHeart() {
        if (interestLevel == 0 && heartCount >= currentHeartGoal) {
            interestLevel += 1;
            heartCount -= 2;
            priceIncrease += 8;
            currentHeartGoal = 5;
        }
        if (interestLevel == 1 && heartCount >= currentHeartGoal) {
            interestLevel += 1;
            heartCount -= 5;
            priceIncrease += 8;
            currentHeartGoal = 7;
        }
        if (interestLevel == 2 && heartCount >= currentHeartGoal) {
            interestLevel += 1;
            heartCount -= 7;
            priceIncrease += 8;
            currentHeartGoal = 9;
        }
        if (interestLevel == 3 && heartCount >= currentHeartGoal) {
            interestLevel += 1;
            heartCount -= 9;
            priceIncrease += 8;
            currentHeartGoal = 11;
        }
        if (interestLevel == 4 && heartCount >= currentHeartGoal) {
            interestLevel += 1;
            heartCount -= 11;
            priceIncrease += 8;
            currentHeartGoal = 13;
        }
        if (interestLevel == 5 && heartCount >= currentHeartGoal) {
            interestLevel += 1;
            heartCount -= 13;
            priceIncrease += 8;
            currentHeartGoal = 99;
        }
        itemPrice = itemToBuy.price * (1+ priceIncrease/100f);
    }

    int playerAddBuffId = -1;
    int NPCAddBuffId = -1;
    int playerDeleteBuffId = -1;
    int NPCDeleteBuffId = -1;

    private void CardEffect(Card card) {
        // 卡片效果，施加buff等
        switch (card.id) {
            case 1:
                // 花招：自身获得3回合DEBUFF“心虚”（每回合结束压力+5%，可以用[resist]抵消）
                playerBuffDict.Add(1, 3+1);
                break;
            case 2:
                // 谄媚"：若顾客存在“设局”BUFF则额外提升3点[heart]
                if (playerBuffDict.ContainsKey(8)) {
                    heartCount += 3;
                }
                break;
            case 3:
                // 收尾工作：强制结束交易
                finishFlag = true;
                break;
            case 4:
                // 及时行乐：根据当前回合已打出的手牌数量，每张提升7点[heart]
                heartCount += usedCardCount * 7;
                break;
            case 5:
                // 同舟共济：获得1回合“同理心”BUFF
                playerBuffDict.Add(2, 1+1);
                break;
            case 6:
                // 防御姿态：获取2点[resist]防备，开场额外获取2点[resist]
                if (currentRound == 0) {
                    playerResist += 2;
                }
                break;
            case 7:
                // 疼痛耐性：移除2个自身DEBUFF
                RemoveDebuff(2);
                break;
            case 9:
                // 打成一片：抽取1张卡牌
                GetOneMoreCard();
                break;
            case 10:
                // 浑水摸鱼：指定1张手牌丢弃，抽取2张卡牌。只有在手牌数≥2时才可使用
                if (cardsInHand.Count > 2) {
                    // TODO Drop a card
                    GetOneMoreCard();
                    GetOneMoreCard();
                }
                break;
            case 11:
                // 知难而退：每5%压力值提升3点[heart]，抽取3张卡牌。使用后清除手牌中所有情绪卡
                heartCount += (pressure / 5) * 3;
                GetOneMoreCard();
                GetOneMoreCard();
                GetOneMoreCard();
                // TODO 清除手牌中所有情绪卡
                break;
            case 12:
                // 领袖魅力：增加1点[round],若开场使用则额外获得2点[round]
                if (patienceLevel == 0) {
                    patienceLevel += 2;
                } else {
                    patienceLevel += 1;
                }
                break;
            case 13:
                // 不容让步：移除自身1个DEBUFF
                RemoveDebuff(1);
                break;
            case 14:
                // 压迫感:对顾客施加1回合“恐惧”BUFF
                NPCBuffDict.Add(3, 1+1);
                break;
            case 15:
                // 一往无前:若此卡牌为最后1张手牌则额外提升4点[heart]并强制结束交易
                if (cardsInHand.Count == 1) {
                    finishFlag = true;
                }
                break;
            case 16:
                // 设局:对顾客施加无限回合的\"设局\"BUFF
                NPCBuffDict.Add(8, int.MaxValue);
                break;
            case 17:
                // 运筹帷幄:若顾客存在\"设局\"BUFF则额外提升9点[heart]，并清除该BUFF
                if (NPCBuffDict.ContainsKey(8)) {
                    heartCount += 9;
                    NPCBuffDict.Remove(8);
                }
                break;
            case 18:
                // 王车易位:提升14点[heart]，只有在手牌≤3张时可以使用
                if (cardsInHand.Count < 3) {
                    heartCount += 14;
                }
                break;
            case 19:
                // 后翼弃兵，指定丢弃1张手牌并提升3点[heart]，若使用时手牌数≤1则额外提升9点[heart]
                if (cardsInHand.Count <= 1) {
                    heartCount += 9;
                }
                // todo drop card
                break;
            case 20:
                // 长线运营：自身获得3回合“同理心”BUFF,若开场使用则效果延长至4回合
                if (patienceLevel == 0) {
                    playerBuffDict.Add(2, 3+1);
                } else {
                    playerBuffDict.Add(2, 4+1);
                }
                break;
            case 21:
                // 以小钓大:指定丢弃1张手牌，对顾客施加3回合“好奇”BUFF
                // TODO
                NPCBuffDict.Add(4, 3+1);
                break;
            case 22:
                // 互惠互利:清除顾客所有BUFF提升9点[heart]若顾客存在“设局”BUFF则额外获得3点[heart]
                // Clear all items in NPCBuffDict
                if (NPCBuffDict.ContainsKey(8)) {
                    heartCount += 3;
                }
                NPCBuffDict.Clear();
                break;
            case 23:
                // 爱要不要：清点顾客及自身的BUFF+DEBUFF数量，每个提升3点[heart]并强制结束交易
                // Count the number of items in playerBuffDict and NPCBuffDict, then multiply the total count by 3
                heartCount += (playerBuffDict.Count + NPCBuffDict.Count) * 3;
                finishFlag = true;
                break;
            case 24:
                // 孤注一掷:获得无限回合“急迫”DEBUFF
                playerBuffDict.Add(9, int.MaxValue);
                break;
            case 25:
                // 虚张声势:提升3点[round]，并自身获得2回合DEBUFF“心虚”（每回合结束后压力值+5%，可以用[resist]抵消），
                // 顾客获得2回合“猜疑”DEBUFF（每回合结束获得2点[resist]
                patienceLevel += 3;
                playerBuffDict.Add(1, 2+1);
                NPCBuffDict.Add(7, 2+1);
                break;
            case 26:
                // 假死:提升2点[round]，自身获取2回合“装死”DEBUFF（无法添加任意BUFF）
                patienceLevel += 2;
                playerBuffDict.Add(5, 2+1);
                break;
            case 27:
                // 狂厄大怨种:自身每个DEBUFF可以提升9点[heart]并强制结束交易
                foreach (int key in playerBuffDict.Keys) {
                    if (allDebuffIdList.Contains(key)) {
                        heartCount += 9;
                    }
                }
                finishFlag = true;
                break;
            case 28:
                // 认证鉴定:自身获得1回合“自信”BUFF,并使商品溢价固定提升2%
                playerBuffDict.Add(6, 1+1);
                priceIncrease += 2;
                break;
            case 29:
                // 花言巧语:清除顾客所有[resist],对顾客施加2回合“好奇”BUFF
                NPCResist = 0;
                NPCBuffDict.Add(4, 2+1);
                break;
            case 30:
                // 营私戏码:使商品溢价固定提升5%，并使得顾客获得3回合“猜疑”DEBUFF（每回合结束获得2点[resist]）。
                // 若顾客存在“设局”BUFF则额外提升6[heart]，并清除该BUFF
                priceIncrease += 5;
                NPCBuffDict.Add(7, 3+1);
                if (NPCBuffDict.ContainsKey(8)) {
                    heartCount += 6;
                    NPCBuffDict.Remove(8);
                }
                break;
            case 31:
                // 私人藏品:若顾客存在“好奇”BUFF则额外使商品溢价固定提升5%，并移除该BUFF
                if (NPCBuffDict.ContainsKey(4)) {
                    priceIncrease += 5;
                    NPCBuffDict.Remove(4);
                }
                break;
            case 32:
                // 好事成双:随机提升1~9点[heart]。仅在自身BUFF数量≥1时可以使用
                if (playerBuffDict.Count >=1) {
                    // Generate a random integer between 1 and 9 (inclusive) and add it to heartCount
                    heartCount += UnityEngine.Random.Range(1, 10);
                }
                break;
            case 33:
                // 押小:随机提升1~9点[heart]。若提升[heart]＜顾客剩余[round]，则再获得1张*押小卡牌和1张*押大卡牌。仅限顾客剩余[round]≤9时才可使用
                if (patienceLevel <= 9) {
                    int addHeart = UnityEngine.Random.Range(1, 10);
                    heartCount += addHeart;
                    if (addHeart < patienceLevel) {
                        cardsInHand.Add(new Card(33));
                        cardsInHand.Add(new Card(34));
                    }
                }
                break;
            case 34:
                // 押大:随机提升1~9点[heart]。若提升[heart]＞顾客剩余[round]，则再获得1张*押小卡牌和1张*押大卡牌。仅限顾客剩余[round]≤9时才可使用。
                if (patienceLevel <= 9) {
                    int addHeart = UnityEngine.Random.Range(1, 10);
                    heartCount += addHeart;
                    if (addHeart > patienceLevel) {
                        cardsInHand.Add(new Card(33));
                        cardsInHand.Add(new Card(34));
                    }
                }
                break;
            case 35:
                // 赌徒心态：将顾客的[round]固定降低至1，并根据剩余手牌数量，每张提升6点[heart]
                patienceLevel = 1;
                heartCount += cardsInHand.Count * 6;
                break;
            case 36:
                // 同理心：自身获得3回合“同理心”BUFF
                playerBuffDict.Add(2, 3+1);
                break;
            case 37:
                // 平静：移除自身和顾客的“急迫”DEBUFF和“心虚”DEBUFF。使用后清除手牌中所有情绪卡
                NPCBuffDict.Remove(1);
                NPCBuffDict.Remove(9);
                playerBuffDict.Remove(1);
                playerBuffDict.Remove(9);
                break;
            case 38:
                // 亲和力:清除顾客所有[resist]和“猜疑”DEBUFF,自己获得1回合“同理心”BUFF
                // 若顾客存在“恐惧”BUFF则额外提升4点[round]，并清除该BUFF
                NPCResist = 0;
                NPCBuffDict.Remove(7);
                playerBuffDict.Add(2, 1+1);
                if (NPCBuffDict.ContainsKey(3)) {
                    patienceLevel += 4;
                    NPCBuffDict.Remove(3);
                }
                break;
            default:
                // code to handle unexpected card id
                break;
        }
        CaculateHeart();
    }

    private void RemoveDebuff(int count) {
        List<int> debuffKeys = new List<int>(playerBuffDict.Keys);
        // Find all keys in playerBuffDict that match any of the ids in allDebuffIdList
        debuffKeys = debuffKeys.FindAll(id => allDebuffIdList.Contains(id));
        for (int i = 0; i < count; i++) {
            if (debuffKeys.Count == 0) {
                break;
            }
            int randomIndex = UnityEngine.Random.Range(0, debuffKeys.Count);
            int debuffKey = debuffKeys[randomIndex];
            playerBuffDict.Remove(debuffKey);
            debuffKeys.RemoveAt(randomIndex);
        }
    }
    private void BuffEffect(int target, int buff) {
        // BUFF效果
        switch (buff) {
            case 1:
                // 心虚, 每回合结束压力+5%，可以用[resist]抵消
                if (playerResist > 5) {
                    playerResist -= 5;
                } else {
                    pressure += playerResist - 5;
                }
                break;
            case 2:
                // 同理心, 使手牌提升[heart]增加25%
                // TODO caculate heart before 
                break;
            case 3:
                // 恐惧 使当前回合打出的任意3张手牌不消耗[round]
                // TODO before useCard
                break;
            case 4:
                // 好奇 每回合结束后提升6点[heart]
                heartCount += 6;
                break;
            case 5:
                // 装死 使得打出手牌提升[heart]的效果降低25%
                // TODO caculate heart before 
                break;
            case 6:
                // 自信 使每回合获取2点[resist]
                playerResist += 2;
                break;
            case 7:
                // 猜疑 每回合结束获得2点[resist]。敌方若开局携带则为无限回合。
                playerResist += 2;
                break;
            case 8:
                // 设局 增益特定卡牌。如伊琳娜的“运筹帷幄”，白逸的“谄媚”，菲的“互惠互利”
                // TODO nothing
                break;
            case 9:
                // 急迫 此后每回合[round]额外-1。敌方若开局携带则为无限回合。
                patienceLevel -= 1;
                break;
            default:
                break;
        }

        
    }
}