public interface IInteractable
{
    string ActionLabel { get; }
    void OnInteract(PlayerCombat player);
}
