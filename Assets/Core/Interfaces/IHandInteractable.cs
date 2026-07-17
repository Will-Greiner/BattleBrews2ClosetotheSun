public interface IHandInteractable
{
    bool CanInteract(GrabController grabController);
    void Interact(GrabController grabController);
    string GetInteractionPrompt(GrabController grabController);
}