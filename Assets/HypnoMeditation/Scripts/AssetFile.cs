using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class AssetFile : ScriptableObject
{
    public string named = "22";
    public List<ItemDB.Item> Items = new List<ItemDB.Item>();
    public List<int> items_easy = new List<int>();
    public List<int> items_hard = new List<int>();
    public List<int> items_unused = new List<int>();
}
