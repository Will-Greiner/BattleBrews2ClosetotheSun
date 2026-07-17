public interface IItemReceiver
{
    bool CanReceiveItem(GrabbableItem item);
    void ReceiveItem(GrabbableItem item);
    string GetReceivePrompt(GrabbableItem item);
}