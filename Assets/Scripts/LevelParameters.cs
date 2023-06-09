using UnityEngine;

[CreateAssetMenu(fileName = "LevelParams", menuName = "ScriptableObjects/Level Parameters", order = 1)]
public class LevelParameters : ScriptableObject
{
    public int actorsCount;
    public int cardsCount;
    public int playableCardsCount;
    public float scriptSpeed;
    public float missingProbability;
    public float hintTimerDuration = 3.0f; // in seconds
    public int scriptLength = 20;
    public int difficulty;
}
