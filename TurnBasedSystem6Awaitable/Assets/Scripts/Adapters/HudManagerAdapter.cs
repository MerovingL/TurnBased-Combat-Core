using System.Collections.Generic;
using UnityEngine;

public class HudManagerAdapter : MonoBehaviour,
    IPeaceUi,
    ITurnUi,
    IHpUi,
    ISkillUi,
    IBonusUi,
    IHelpersUi
{
    [SerializeField] private HudManager _hud;

    // IHpUi
    public void HidePartyHp() => _hud.HidePartyHp();
    public void ShowPartyHp() => _hud.ShowPartyHp();
    public void HideEnemyHp() => _hud.HideEnemyHp();
    public void ShowEnemyHp() => _hud.ShowEnemyHp();

    // IPeaceUi
    public void FadePeaceUi(float duration) => _hud.FadePeaceUi(duration);
    public void UnfadePeaceUi(float duration = 1) => _hud.UnfadePeaceUi(duration);
    public void SetActivePeaceUi(bool active) => _hud.SetActivePeaceUi(active);

    // IBonusUi
    public int BonusQueueCount => _hud.bonusQueue.Count;
    public bool BonusIsAdding => _hud.bonusIsAdding;
    public void ClearBonusButtons() => _hud.bonusButtons.Clear();
    public IReadOnlyList<Transform> BonusButtons { get => _hud.bonusButtons; }

    // ITurnUi
    public void SetActiveEndTurnButton(bool value) => _hud.SetActiveEndTurnButton(value);
    public void SetInteractableEndTurnButton(bool value) => _hud.SetInteractableEndTurnButton(value);
    public void FadeMainMenuButton(float duration) => _hud.FadeMainMenuButton(duration);
    public void UnfadeMainMenuButton(float duration) => _hud.UnfadeMainMenuButton(duration);
    public bool TurnComplete => _hud.turnComplete;
    public bool ButtonsDisabled { get => _hud.buttonsDisabled; set => _hud.buttonsDisabled = value; }
    public void ChangeState(int heroIndex) => _hud.StateChange(heroIndex);

    // ISkillUi
    public void SetInactiveAllHeroSkillButtons() => _hud.SetInactiveAllHeroSkillButtons();
    public void SetInterctableSkillButtons(int heroIndex, bool value) => _hud.SetInteractableSkillButtons(heroIndex, value);
    public void DecreaseSkillCooldowns(int heroindex) => _hud.DecreaseSkillCooldowns(heroindex);
    public void SetActiveSkillButtons(int heroIndex, bool value) => _hud.SetActiveSkillButtons(heroIndex, value);
    public void MoveUpSkillPanel(bool inPeace = false) => _hud.MoveUpDownSkillPanel(true, inPeace);
    public void MoveDownSkillPanel(bool inPeace = false) => _hud.MoveUpDownSkillPanel(false, inPeace);
    public bool IsSkillPanelShowed {get =>_hud.skillPanel.show;}
    public void ResetAllCooldowns() => _hud.ResetAllCooldowns();
    public void CancelCurrentSkill(bool nullify, bool silent) => _hud.CancelCurrentSkill(nullify, silent);
    public bool SkillExecuting => _hud.skillExecuting;
    public void SetSkillButtonsFadeDuration(float val, float del) => _hud.SetSkillButtonsFadeDuration(val, del);

    // IHelpersUi
    public void HideHelpers(float del) => _hud.HideHelpers(del);
    public void ActivateHelpers() => _hud.ActivateHelpers();
}
