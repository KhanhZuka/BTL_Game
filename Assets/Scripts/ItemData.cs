using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    public ItemType itemType;
    public enum ItemType
    {
        Damage,
        Speed,
        Freeze,
        HighJump,
        Shield
    }
}