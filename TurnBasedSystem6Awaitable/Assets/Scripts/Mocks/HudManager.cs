using System.Collections.Generic;
using UnityEngine;

public class HudManager : MonoBehaviour
{
    [Header("Dependencies")]
    public SkillPanel skillPanel;
    [Header("Runtime Lists")]
    public List<Transform> bonusButtons = new List<Transform>();
    [Header("Runtime State")]
    [ReadOnly] public bool bonusIsAdding;
    [ReadOnly] public bool turnComplete;
    [ReadOnly] public bool buttonsDisabled;
    [ReadOnly] public bool skillExecuting;
    [System.NonSerialized] public Queue<GameObject> bonusQueue = new Queue<GameObject>();

    public void HidePartyHp() { }
    public void ShowPartyHp() { }
    public void HideEnemyHp() { }
    public void ShowEnemyHp() { }
    public void FadePeaceUi(float duration) { }
    public void UnfadePeaceUi(float duration = 1) { }
    public void SetActivePeaceUi(bool active) { }
    public void SetActiveEndTurnButton(bool value) { }
    public void SetInteractableEndTurnButton(bool value) { }
    public void FadeMainMenuButton(float duration) { }
    public void UnfadeMainMenuButton(float duration) { }
    public void StateChange(int heroIndex) { }
    public void SetInactiveAllHeroSkillButtons() { }
    public void SetInteractableSkillButtons(int HeroIndex, bool value) { }
    public void DecreaseSkillCooldowns(int heroIndex) { }
    public void SetActiveSkillButtons(int heroIndex, bool value) { }
    public void MoveUpDownSkillPanel(bool up, bool inPeace = false) { }
    public void ResetAllCooldowns() { }
    public void CancelCurrentSkill(bool nullify, bool delay) { }
    public void SetSkillButtonsFadeDuration(float target, float delay = 0) { }
    public void ActivateHelpers() { }
    public void HideHelpers(float delay) { }
    public void SetInteractablePassives(bool value) { }

}