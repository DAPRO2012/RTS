public abstract class SceneEvent(ScreenManager manager) : Event
{
    public Scene GetScene() => Scene.Get();
    public void SetScene(Identifier id) => Scene.Set(id, Manager);

    public ScreenManager Manager { get; private set; } = manager;

}

public class SceneChangeEvent(Scene to, ScreenManager manager) : SceneEvent(manager)
{
    private readonly Scene _changeTo = to;
    public Scene GetChangingToScene() => _changeTo;
}