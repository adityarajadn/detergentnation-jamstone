public interface IDamageable
{
    void TakeDamage(int amount);
    bool IsDead { get; }
    int CurrentHealth { get; }
    int MaxHealth { get; }
}
