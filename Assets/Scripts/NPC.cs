using System;

[System.Serializable]
public class NPC
{
    public int id;
    public string name;
    public string type;
    public string description;

    public NPC(int id, string name, string type, string description)
    {
        this.id = id;
        this.name = name;
        this.type = type;
        this.description = description;
    }
}
