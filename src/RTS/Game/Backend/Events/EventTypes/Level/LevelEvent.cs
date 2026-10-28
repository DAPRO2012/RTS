public class LevelEvent<TLevel>(TLevel level) : Event where TLevel : ILevel
{
    public readonly TLevel Level = level;
    public TLevel GetLevel() => Level;

    public class Load(ServerLevel level) : LevelEvent<ServerLevel>(level);
    public class Unload(ServerLevel level) : LevelEvent<ServerLevel>(level);
}