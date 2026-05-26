using UnityEngine;

public class BuffData
{
    public ItemData item;
    public Sprite icon;
    public float timeLeft;

    public BuffData(ItemData item, float duration)
    {
        this.item = item;
        this.icon = item.icon;
        this.timeLeft = duration;
    }
}