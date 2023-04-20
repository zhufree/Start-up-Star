using System;

[System.Serializable]
public class Buff {
    public int id;
    public string type;
    public string desc;
    public string name;
    //fields: id, type, desc, name(String)
    //class named buff
    
    public Buff(int id, string type, string desc, string name) {
        this.id = id;
        this.type = type;
        this.desc = desc;
        this.name = name;
    }
}

