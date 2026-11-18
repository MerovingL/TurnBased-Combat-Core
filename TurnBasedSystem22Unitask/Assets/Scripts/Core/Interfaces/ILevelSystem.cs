public interface ILevelSystem
{
    bool WasPassivePanelOpened { get; }
    bool WasPassivePanelEdited { get; }
    void SetInteractablePassives(bool val);
}
