using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScriptItem {
    public float delay; // in seconds
    public string cardId;
    public string actorId;
    public bool isMissing;

    public ScriptItem(string cardId, string actorId, bool isMissing = false, float delay = 1.0f) {
        this.delay = delay;
        this.cardId = cardId;
        this.actorId = actorId;
        this.isMissing = isMissing;
    }
}

[System.Serializable]
public class Script
{
    public List<ScriptItem> items;
}
