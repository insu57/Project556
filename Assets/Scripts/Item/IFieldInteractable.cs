using Player;
using UnityEngine;

public interface IFieldInteractable //필드 상호작용 가능 - 현재는 상호작용이라는 구분용도만
{
    public void PlayerGetFieldInteractInfo(PlayerManager playerManager);
}

public sealed class FieldInteractionContext
{
    public Inventory LootInventory { get; init; }
    public ItemInstance FieldItem { get; init; }
    public PlayerManager PlayerManager { get; init; }
}
