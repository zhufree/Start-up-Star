using System.Collections;
using UnityEngine;

// 执行自动任务，挂载
public class AutoController: MonoBehaviour
{
    [SerializeField] private float saveInterval = 10f; // 每隔60秒自动保存

    private void Start()
    {
        StartCoroutine(AutoSave());
    }

    private IEnumerator AutoSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(saveInterval);
            SaveGame();
        }
    }

    public void SaveGame()
    {
        // 实现存档逻辑
        SaveManager.SaveCardDeck(ConstantManager.Instance.playerCardDeck);
        // Debug.Log("Game saved!");
    }
}