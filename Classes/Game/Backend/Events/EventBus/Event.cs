public class Event : IEndable
{
    private bool _endEvent = false;
    public void EndEvent() => _endEvent = true;
    public bool IsEventEnding() => _endEvent;
}