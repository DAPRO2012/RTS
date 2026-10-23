using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class EventBus : IEventBus
{
    private readonly Dictionary<Type, Delegate> _handlers = new();

    //Send Event to be run
    public void Publish<T>(T @event) where T : Event
    {
        if (_handlers.TryGetValue(typeof(T), out var handler) && !@event.IsEventEnding())
                ((Action<T>)handler)(@event);
    }

    //Subscribe Event to be run when publish with this type is called
    public void Subscribe<T>(Action<T> handler) where T : Event
    {
        Type eventType = typeof(T);
        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = handler;
        else
            _handlers[eventType] = Delegate.Combine(_handlers[eventType], handler);
    }

    public void Register(object subscriber)
    {
        MethodInfo[] methods = [..
            subscriber.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => m.IsDefined(typeof(SubscribeEvent), inherit: false))
        ];

        foreach (MethodInfo method in methods)
        {
            var @params = method.GetParameters();
            if (@params.Length != 1)
                throw new ArgumentException("Too many arguments for SubscribeEvent Methods");

            Type paramType = @params[0].ParameterType;
            Type handlerType = typeof(Action<>).MakeGenericType(paramType);
            Delegate handler = method.CreateDelegate(handlerType, subscriber);

            MethodInfo subscribeMethod = GetType().GetMethod(nameof(Subscribe))!;
            subscribeMethod.MakeGenericMethod(paramType).Invoke(this, [handler]);
        }
    }

    public void Register(Type subscriber)
    {
        object obj = Activator.CreateInstance(subscriber);
        Register(obj);
    }

}