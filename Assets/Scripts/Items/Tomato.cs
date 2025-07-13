using UnityEngine;

public class Tomato : PickableItem
{
    // 当西红柿被拿起时
    protected override void OnPickedUp()
    {
        base.OnPickedUp();
        Debug.Log("西红柿被拿起！");
    }
    
    // 当西红柿被放置时
    protected override void OnPlaced()
    {
        base.OnPlaced();
        Debug.Log("西红柿被放在台面上！");
    }
    
    // 当西红柿被丢弃时
    protected override void OnDropped()
    {
        base.OnDropped();
        Debug.Log("西红柿被丢弃了！");
    }
} 