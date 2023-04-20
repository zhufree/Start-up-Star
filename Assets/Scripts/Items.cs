using System;

[System.Serializable]
public class Item {
    public string name;
    public string desc;
    public int price;
    public String icon;

    public Item(string name, string desc, int price, String icon) {
        this.name = name;
        this.desc = desc;
        this.price = price;
        this.icon = icon;
    }
}
