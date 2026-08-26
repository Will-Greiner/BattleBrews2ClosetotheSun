public interface IItemReceiver
{
    bool CanReceiveItem(GrabbableItem item);
    void ReceiveItem(GrabbableItem item);
    string GetReceivePrompt(GrabbableItem item);
}

public interface IItemHoverFeedback
{
    void SetItemHover(bool hovering, GrabbableItem item);
}
