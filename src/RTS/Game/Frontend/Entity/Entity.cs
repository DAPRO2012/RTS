using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public abstract class Entity
{
    private readonly Level _level;
    private Vector2 position;
    private Vector2 velocity = Vector2.Zero;
    private Vector2? lastKnownPosition;
    private Vector2 lastKnownSpeed;
    private float rot;
    private int id;
    private readonly ISet<string> _tags = new HashSet<string>();
    public ISet<string> Tags => _tags;

    public Entity(Level level)
    {
        _level = level;
        id = level.ConsumeNextEntityId();
    }

    public int GetId() {
        if (id == 0) {
            throw new Exception("Tried to access entity ID before ID assignment");
        } else {
            return id;
        }
    }
    public void SetId(int id) {
        this.id = id;
    }

    public bool AddTag(string tag) =>
        _tags.Add(tag);
    public bool RemoveTag(string tag) =>
        _tags.Remove(tag);

    public void Tick()
    {
        ComputeSpeed();
    }

    protected void ComputeSpeed()
    {   
        if (!lastKnownPosition.HasValue)
            lastKnownPosition = position;
        lastKnownSpeed = position - lastKnownPosition.Value;
        lastKnownPosition = position;
    }

    public void SetVelocity(Vector2 velocity) =>
        this.velocity = velocity;
    public void SetVelocity(float xd, float yd)
    {
        velocity.X = xd;
        velocity.Y = yd;
    }
    public void AddVelocity(Vector2 momentum) =>
        velocity += momentum;
    public void AddVelocity(float xd, float yd)
    {
        velocity.X += xd;
        velocity.Y += yd;
    }
    public Vector2 GetVelocity() =>
        velocity;
}

public abstract class Entity<TSelf> : Entity where TSelf : Entity<TSelf>
{
    private readonly EntityType<TSelf> _entityType;

    protected Entity(EntityType<TSelf> entityType, Level level) : base(level)
    {
        _entityType = entityType;
    }
}