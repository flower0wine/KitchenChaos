using System;

// 交互上下文类 - 提供交互所需的所有信息
public class InteractionContext
{
    public PlayerInteractor Player { get; private set; }
    public PickableItem HeldItem { get; private set; }
    public Interactable Target { get; private set; }
    
    public InteractionContext(PlayerInteractor player, PickableItem heldItem, Interactable target)
    {
        Player = player;
        HeldItem = heldItem;
        Target = target;
    }
    
    public bool HasItem => HeldItem != null;
}