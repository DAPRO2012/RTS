using System;
using System.Collections.Generic;

public class Pipeline<T> : IPipeline<T> where T : IEndable
{
    private readonly List<Action<T>> _steps = new();
    public IReadOnlyCollection<Action<T>> Steps => _steps;

    public void Run(T e) =>
        _steps.ForEach(step =>
        {
            if (!e.IsEventEnding())
                step(e);
        });

    public void WithStep(Action<T> step) =>
        _steps.Add(step);
}