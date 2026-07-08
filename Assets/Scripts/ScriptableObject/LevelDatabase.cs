using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewLevelData",
    menuName = "Game/Level Database")]
public class LevelDatabase : ScriptableObject
{
    public List<LevelData> levels;
}