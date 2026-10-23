using System;

/*
[Flags]
public enum Dist
{
    SERVER,
    CLIENT
}*/

[AttributeUsage(AttributeTargets.Class)]
public class EventBusSubscriber : Attribute
{
    /*
    public EventBusSubscriber(Dist Dist = Dist.SERVER | Dist.CLIENT)
    {
    }
    */
}

[AttributeUsage(AttributeTargets.Method)]
public class SubscribeEvent : Attribute
{
}
