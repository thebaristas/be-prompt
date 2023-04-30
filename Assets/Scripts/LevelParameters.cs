using UnityEngine;

[CreateAssetMenu(fileName = "LevelParams", menuName = "ScriptableObjects/Level Parameters", order = 1)]
public class LevelParameters : ScriptableObject
{
    public int actorsCount;
    public int cardsCount;
    public float scriptSpeed;
}
