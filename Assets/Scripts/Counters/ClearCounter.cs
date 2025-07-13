using UnityEngine;

public class ClearCounter : ItemContainer
{
    public override bool CanHighlight(PlayerInteractor player)
    {
        if (player.GetHeldItem() != null || GetHeldItem() != null)
        {
            return true;
        }
        return false;
    }
} 