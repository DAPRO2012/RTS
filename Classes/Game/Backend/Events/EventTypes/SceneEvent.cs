public abstract class SceneEvent : Event
{
    public Scene GetScene() => Scene.Get();
    public void SetScene(Identifier id) => Scene.Set(id);
}

public class OnSceneChangeEvent(Scene to) : SceneEvent
{
    public bool AllowChange = true;
    private readonly Scene _changeTo = to;
    public Scene GetChangingToScene() => _changeTo;
}