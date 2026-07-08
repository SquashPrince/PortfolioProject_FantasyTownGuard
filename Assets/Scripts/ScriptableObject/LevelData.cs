using UnityEngine;

[CreateAssetMenu(
    fileName = "NewLevelData",
    menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int lv;
    public string lvName;

    public int maxVisitor;
}
