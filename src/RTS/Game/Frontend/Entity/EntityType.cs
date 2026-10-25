using System;

public class EntityType<T> where T : Entity
{
    protected readonly Func<T> _factory;
    protected readonly EntityCategory _category;

    protected readonly bool serialize = true;

    public EntityType(Func<T> factory, EntityCategory category)
    {
        _factory = factory;
        _category = category;
    }
    public EntityType(Func<T> factory, EntityCategory category, bool serialize)
    {
        _factory = factory;
        _category = category;

        this.serialize = serialize;
    }

    public class Builder
    {
        private readonly Func<T> _factory;
        private readonly EntityCategory _category;

        private bool serialize = true;

        private Builder(Func<T> factory, EntityCategory category)
        {
            _factory = factory;
            _category = category;
        }

        public static Builder Of(Func<T> factory, EntityCategory category)
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