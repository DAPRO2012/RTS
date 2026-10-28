using System;

public abstract class EntityType
{
    protected readonly EntityCategory _category;
    protected readonly bool serialize;

    protected EntityType(EntityCategory category, bool serialize)
    {
        _category = category;
        this.serialize = serialize;
    }
}

public class EntityType<T> : EntityType where T : Entity
{
    protected readonly Func<ServerLevel, T> _factory;

    public T Create(ServerLevel level)
    {
        T entity = _factory(level);
        level.AddFreshEntity(entity);
    }

    public EntityType(Func<ServerLevel, T> factory, EntityCategory category)
        : base(category, true)
    {
        _factory = factory;
    }

    public EntityType(Func<ServerLevel, T> factory, EntityCategory category, bool serialize)
        : base(category, serialize)
    {
        _factory = factory;
    }

    public class Builder
    {
        private readonly Func<ServerLevel, T> _factory;
        private readonly EntityCategory _category;

        private bool serialize = true;

        private Builder(Func<ServerLevel, T> factory, EntityCategory category)
        {
            _factory = factory;
            _category = category;
        }

        public static Builder Of(Func<ServerLevel, T> factory, EntityCategory category)
        {
            return new Builder(factory, category);
        }

        public Builder NoSave()
        {
            serialize = false;
            return this;
        }

        public EntityType<T> Build() =>
            new EntityType<T>(
                _factory,
                _category,
                serialize
            );
    }
}