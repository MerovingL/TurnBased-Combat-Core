public interface IHeroControlService
{
    bool CanControl { get; set; }
    void SetControl(bool enabled);
    void StopHeroes();
    void UnstopHeroes();
}
