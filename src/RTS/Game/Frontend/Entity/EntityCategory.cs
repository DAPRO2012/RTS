public record EntityCategory(bool IsFriendly, bool IsPersistent, int DespawnDistance = 128)
{
    public static readonly EntityCategory MISC = new EntityCategory(true, true);
};