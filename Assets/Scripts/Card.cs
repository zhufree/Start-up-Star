using System;

[System.Serializable]
public class Card
{
    public int id;
    public string name;
    public int round;
    public int heart;
    public int resist;
    public string description;
    public string image;

    public Card(int id, string name, int round, string description, string image)
    {
        this.id = id;
        this.name = name;
        this.round = round;
        this.description = description;
        this.image =  image;
    }

    public Card(int id) {
        this.id = id;
        this.name = ConstantManager.Instance.allCards.Find(card => card.id == id).name;
        this.round = ConstantManager.Instance.allCards.Find(card => card.id == id).round;
        this.description = ConstantManager.Instance.allCards.Find(card => card.id == id).description;
        this.image = ConstantManager.Instance.allCards.Find(card => card.id == id).image;
    }
}