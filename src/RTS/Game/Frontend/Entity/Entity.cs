using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Xna.Framework;

public abstract class Entity
{
    private static readonly AABB INITIAL_BB = new(0, 0, 0, 0);

    private readonly ServerLevel _level;
    private Vector2 position;
    private Vector2 velocity = Vector2.Zero;
    private Vector2? lastKnownPosition;
    private Vector2 lastKnownSpeed;
    private float rot = 0f;
    private int id;
    private AABB bb = INITIAL_BB;
    private readonly ISet<string> _tags = new HashSet<string>();
    public ISet<string> Tags => _tags;
    public bool NoPhysics = false;
    public bool Collision;
    public RemovalReason? removal = null;

    public Entity(ServerLevel level)
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

    public bool AddTag(string tag) =>
        _tags.Add(tag);
    public bool RemoveTag(string tag) =>
        _tags.Remove(tag);

    public void Tick()
    {
        Move(velocity);
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

    public void SetRot(float rot) =>
        this.rot = rot;

    public void SetPos(Vector2 position) =>
        SetPos(position.X, position.Y);
    public void SetPos(float x, float y)
    {
        SetPosRaw(x, y);
    }
    public void SetPosRaw(float x, float y)
    {
        if (position.X != x || position.Y != y)
        {
            position = new Vector2(x, y);
            Collision = false;
        }
    }

    public void Move(Vector2 velocity)
    {
        if (NoPhysics)
        {
            Collision = false;
            SetPos(position + velocity);

        } else
        {
            Vector2 movement = Collide(velocity);
            float movementLength = movement.LengthSquared();
            if (movementLength > 1.0E-7 || velocity.LengthSquared() - movementLength < 1.0E-7)
            {
                Vector2 newPosition = position + movement;
                SetPos(newPosition);
            }

            Collision = !(velocity == movement);
        }
    }

    public Vector2 Collide(Vector2 velocity)
    {
        ImmutableList<AABB> colliders = CollectColliders(this, _level, [], bb);
        return velocity;
    }

    private static ImmutableList<AABB> CollectColliders(Entity source, ServerLevel level, ImmutableList<AABB> entityColliders, AABB boundingBox)
    {
        ImmutableList<AABB>.Builder colliders = ImmutableList.CreateBuilder<AABB>();
        if (!entityColliders.IsEmpty)
            colliders.AddRange(entityColliders);

        colliders.AddRange(level.GetBlockCollisions(source, boundingBox));
        return colliders.ToImmutable();
    }

    public bool IsRemoved() =>
        removal.HasValue;

    public struct RemovalReason(bool destroy, bool save)
    {
        public static readonly RemovalReason KILLED = new(true, false);
        public static readonly RemovalReason DISCARDED = new(true, false);
        public static readonly RemovalReason UNLOADED_TO_CHUNK = new(false, true);

        private readonly bool destroy = destroy;
        private readonly bool save = save;

        public bool ShouldDestroy() => destroy;
        public bool ShouldSave() => save;
    }
}

public abstract class Entity<TSelf> : Entity where TSelf : Entity<TSelf>
{
    private readonly EntityType<TSelf> _entityType;

    protected Entity(EntityType<TSelf> entityType, ServerLevel level) : base(level)
    {
        _entityType = entityType;
    }
}