using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SaveManager
{

    // Function to save data using JSON
    public static void SaveCardDeck(List<Card> dataList)
    {

        // Translate Card list to id list by mapping card => it
        List<int> idList = dataList.ConvertAll(card => card.id);

        // Convert the dictionary to a JSON string
        string jsonData = JsonHelper.ToJson(idList.ToArray());

        // Save the JSON string to a file
        string filePath = Application.dataPath + "/cardDeck.json";
        System.IO.File.WriteAllText(filePath, jsonData);
    }


    // Function to read JSON data for card IDs that the player has now
    public static List<int> LoadCardDeck()
    {
        // Read the JSON string from the file
        string filePath = Application.dataPath + "/cardDeck.json";
        if (File.Exists(filePath))
        {
            string jsonData = System.IO.File.ReadAllText(filePath);
            int[] cardArr = JsonHelper.FromJson<int>(jsonData);
            // Convert the JSON string to a list of integers
            List<int> cardIDs = new List<int>(cardArr);
            // Use the cardIDs list as needed
            return cardIDs;
        } else {
            return new List<int>{};
        }
    }


    public static void Save(){
        PlayerPrefs.SetInt("level", 10);
        PlayerPrefs.SetInt("coin", ConstantManager.Instance.coin);
        PlayerPrefs.SetFloat("experience", 1000.0f);

        PlayerPrefs.Save();
    }
}
