using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurtainListener : MonoBehaviour
{
    // Event handler for curtain state changes
    public delegate void CurtainStateChangeHandler(string state);
    public event CurtainStateChangeHandler OnCurtainStateChanged;

    void CurtainStateChanged(string state) {
        OnCurtainStateChanged(state);
    }
}
