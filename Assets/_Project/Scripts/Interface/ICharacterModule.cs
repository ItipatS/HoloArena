public interface ICharacterModule
{
    void Initialize(FighterController controller);
    void Tick(float deltaTime);
    void FixedTick(float fixedDeltaTime);
}
