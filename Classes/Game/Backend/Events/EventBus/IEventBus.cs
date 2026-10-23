using System;
using System.Collections.Generic;
using System.Linq;

public interface IEventBus
{
    void Publish<T>(T @event) where T : Event;
    void Subscribe<T>(Action<T> handler) where T : Event;
}