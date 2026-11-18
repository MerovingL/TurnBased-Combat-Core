public interface ITurnUi 
{
    void SetActiveEndTurnButton(bool value);
    void SetInteractableEndTurnButton(bool value);
    void FadeMainMenuButton(float duration);
    void UnfadeMainMenuButton(float duration);
    bool TurnComplete { get; }
    bool ButtonsDisabled { get; set; }
    void ChangeState(int heroIndex);
}
