using System;
using System.Collections.Generic;

namespace Stereo.Modules;

public class ModuleCollection
{
    public List<Type> ModuleTypes { get; } = [];

    public ModuleCollectionPointer Pointer { get; }

    public ModuleCollection()
    {
        Pointer = new ModuleCollectionPointer(this, () => ModuleTypes.Count);
    }

    public class ModuleCollectionPointer(ModuleCollection collection, Func<int> getInsertionIndex)
    {
        public List<Type> ModuleTypes => collection.ModuleTypes;

        public ModuleCollectionPointer Register<TModule>()
            where TModule : BaseModule
        {
            var moduleType = typeof(TModule);
            if (!collection.ModuleTypes.Contains(moduleType))
            {
                collection.ModuleTypes.Insert(getInsertionIndex.Invoke(), moduleType);
            }

            return this;
        }

        public ModuleCollectionPointer Deregister<TModule>()
            where TModule : BaseModule
        {
            collection.ModuleTypes.Remove(typeof(TModule));
            return this;
        }

        public ModuleCollectionPointer AtStart(Action<ModuleCollectionPointer> configureModules)
        {
            configureModules.Invoke(new ModuleCollectionPointer(collection, () => 0));
            return this;
        }

        public ModuleCollectionPointer AtEnd(Action<ModuleCollectionPointer> configureModules)
        {
            configureModules.Invoke(new ModuleCollectionPointer(collection, () => collection.ModuleTypes.Count));
            return this;
        }

        public ModuleCollectionPointer At(int index, Action<ModuleCollectionPointer> configureModules)
        {
            configureModules.Invoke(
                new ModuleCollectionPointer(collection, () => Math.Clamp(index, 0, collection.ModuleTypes.Count - 1)));

            return this;
        }

        public ModuleCollectionPointer Step(int steps, Action<ModuleCollectionPointer> configureModules)
        {
            configureModules.Invoke(
                new ModuleCollectionPointer(
                    collection,
                    () => Math.Clamp(getInsertionIndex.Invoke() + steps, 0, collection.ModuleTypes.Count - 1)));

            return this;
        }

        public ModuleCollectionPointer StepBack(int steps, Action<ModuleCollectionPointer> configureModules)
        {
            configureModules.Invoke(
                new ModuleCollectionPointer(
                    collection,
                    () => Math.Clamp(getInsertionIndex.Invoke() - steps, 0, collection.ModuleTypes.Count - 1)));

            return this;
        }

        public ModuleCollectionPointer Before<TModule>(Action<ModuleCollectionPointer> configureModules)
            where TModule : BaseModule
        {
            var moduleType = typeof(TModule);

            configureModules.Invoke(
                new ModuleCollectionPointer(
                    collection,
                    () => collection.ModuleTypes.FindIndex(type => moduleType.IsAssignableFrom(type))));

            return this;
        }

        public ModuleCollectionPointer After<TModule>(Action<ModuleCollectionPointer> configureModules)
            where TModule : BaseModule
        {
            var moduleType = typeof(TModule);

            configureModules.Invoke(
                new ModuleCollectionPointer(
                    collection,
                    () => collection.ModuleTypes.FindIndex(type => moduleType.IsAssignableFrom(type)) + 1));

            return this;
        }
    }
}
