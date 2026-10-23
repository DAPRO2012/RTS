public class LevelEvent(Level level) : Event
{
    public readonly Level Level = level;
    public Level GetLevel() => Level;

    public class Load(Level level) : LevelEvent(level);
    public class Unload(Level level) : LevelEvent(level);
}