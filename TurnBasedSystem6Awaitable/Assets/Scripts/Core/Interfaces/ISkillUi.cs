public interface ISkillUi
{
    void SetInactiveAllHeroSkillButtons();
    void SetInterctableSkillButtons(int heroIndex, bool value);
    void DecreaseSkillCooldowns(int heroindex);
    void SetActiveSkillButtons(int heroIndex, bool value);
    void MoveUpSkillPanel(bool inPeace = false);
    void MoveDownSkillPanel(bool inPeace = false);
    void ResetAllCooldowns();
    void CancelCurrentSkill(bool nullify, bool silent);
    bool SkillExecuting { get; }
    void SetSkillButtonsFadeDuration(float val, float dur);
    bool IsSkillPanelShowed { get; }
}
