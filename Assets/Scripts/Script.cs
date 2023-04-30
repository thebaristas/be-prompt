using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptItem {
    public float delay; // in seconds
    public string emojiCode;
    public string characterID;
    public bool isMissing;

    public ScriptItem(string emojiCode, string characterID, bool isMissing = false, float delay = 1.0f) {
        this.delay = delay;
        this.emojiCode = emojiCode;
        this.characterID = characterID;
        this.isMissing = isMissing;
    }
}

public class Script
{
    public List<ScriptItem> items;
}
