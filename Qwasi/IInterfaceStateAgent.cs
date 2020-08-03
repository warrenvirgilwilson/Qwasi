using System;
using System.Collections.Generic;
using System.Text;

namespace Qwasi
{
    public struct VariableIdentifier<TInterface>
    {
        public static VariableIdentifier<TInterface> Create(string variableName)
        {
            return new VariableIdentifier<TInterface>(variableName);
        }

        public string VariableName { get; private set; }

        private VariableIdentifier(string variableName)
        {
            this.VariableName = variableName;
        }
    }

    public struct EventIdentifier<TInterface>
    {
        public static EventIdentifier<TInterface> Create(string eventName)
        {
            return new EventIdentifier<TInterface>(eventName);
        }

        public string EventName { get; private set; }

        private EventIdentifier(string eventName)
        {
            this.EventName = eventName;
        }
    }

    public interface IInterfaceStateAgent<TInterface>
    {
        protected abstract InterfaceStateCatalog<TInterface> InterfaceStateCatalog { get; set; }

        protected virtual void Constructor() { }

        private void __invokeConstructorOnce()
        {
            this.InterfaceStateCatalog ??= new InterfaceStateCatalog<TInterface>();
            if (!this.InterfaceStateCatalog.ConstructorFired)
            {
                this.InterfaceStateCatalog.MarkConstructorAsFired();
                this.Constructor();
            }
        }

        protected sealed void SetInstance(VariableIdentifier<TInterface> variableId, object objectInstance)
        {
            this.InterfaceStateCatalog ??= new InterfaceStateCatalog<TInterface>();
            __invokeConstructorOnce();
            this.InterfaceStateCatalog.SetInstance(variableId, objectInstance);
        }

        protected sealed void DeleteInstance(VariableIdentifier<TInterface> variableId)
        {
            if (this.InterfaceStateCatalog == null)
                throw new Exception("Cannot delete interface object instance: state catalog has not been initialized.");

            __invokeConstructorOnce();
            this.InterfaceStateCatalog.DeleteInstance(variableId);
        }

        protected sealed object GetInstance(VariableIdentifier<TInterface> variableId) => this.GetInstance(variableId, null);
        protected sealed object GetInstance(VariableIdentifier<TInterface> variableId, object initialValue)
        {
            this.InterfaceStateCatalog ??= new InterfaceStateCatalog<TInterface>();
            __invokeConstructorOnce();
            return this.InterfaceStateCatalog.GetInstance(variableId, initialValue);
        }

        protected sealed void AddEventHandler<TEventArgs>(EventIdentifier<TInterface> eventId, EventHandler<TEventArgs> eventHandler)
        {
            this.InterfaceStateCatalog ??= new InterfaceStateCatalog<TInterface>();
            __invokeConstructorOnce();
            this.InterfaceStateCatalog.AddEventHandler<TEventArgs>(eventId, eventHandler);
        }

        protected sealed void RemoveEventHandler<TEventArgs>(EventIdentifier<TInterface> eventId, EventHandler<TEventArgs> eventHandler)
        {
            if (this.InterfaceStateCatalog == null)
                throw new Exception("Cannot remove event handler: the event catalog has not been instantiated.");

            __invokeConstructorOnce();
            this.InterfaceStateCatalog.RemoveEventHandler<TEventArgs>(eventId, eventHandler);
        }

        protected sealed void RaiseEvent<TEventArgs>(EventIdentifier<TInterface> eventId, TEventArgs args)
        {
            __invokeConstructorOnce();
            this.InterfaceStateCatalog?.GetEventHandler<TEventArgs>(eventId)?.Invoke(this, args);
        }
    }

    public class InterfaceStateCatalog<TInterface>
    {
        protected Dictionary<VariableIdentifier<TInterface>, object> InstanceDictionary { get; private set; } = null;
        protected Dictionary<EventIdentifier<TInterface>, Delegate> EventDictionary { get; private set; } = null;
        public bool ConstructorFired { get; private set; } = false;

        public void MarkConstructorAsFired() => this.ConstructorFired = true;

        public void SetInstance(VariableIdentifier<TInterface> variableId, object objectInstance)
        {
            this.InstanceDictionary ??= new Dictionary<VariableIdentifier<TInterface>, object>();

            if (!this.InstanceDictionary.ContainsKey(variableId))
                this.InstanceDictionary.Add(variableId, objectInstance);
            else
                this.InstanceDictionary[variableId] = objectInstance;
        }

        public object GetInstance(VariableIdentifier<TInterface> variableId) => this.GetInstance(variableId, null);
        public object GetInstance(VariableIdentifier<TInterface> variableId, object initialValue)
        {
            this.InstanceDictionary ??= new Dictionary<VariableIdentifier<TInterface>, object>();

            if (!this.InstanceDictionary.ContainsKey(variableId))
                this.InstanceDictionary.Add(variableId, initialValue);

            return this.InstanceDictionary[variableId];
        }

        public void DeleteInstance(VariableIdentifier<TInterface> variableId)
        {
            if (this.InstanceDictionary == null)
                throw new Exception("Cannot delete interface object instance: instance dictionary has not been initialized.");

            if (!this.InstanceDictionary.ContainsKey(variableId))
                throw new Exception("Cannot delete interface object instance: it is not contained within the instance dictionary.");

            this.InstanceDictionary.Remove(variableId);
        }

        public void AddEventHandler<TEventArgs>(EventIdentifier<TInterface> eventId, EventHandler<TEventArgs> eventHandler)
        {
            this.EventDictionary ??= new Dictionary<EventIdentifier<TInterface>, Delegate>();

            if (!this.EventDictionary.ContainsKey(eventId))
                this.EventDictionary.Add(eventId, eventHandler);
            else
            {
                EventHandler<TEventArgs> storedDelegate = this.EventDictionary[eventId] as EventHandler<TEventArgs>;
                if (storedDelegate == null)
                    throw new Exception("Stored event handler delegate is not the same type as that being added.");

                this.EventDictionary[eventId] = storedDelegate + eventHandler;
            }
        }

        public void RemoveEventHandler<TEventArgs>(EventIdentifier<TInterface> eventId, EventHandler<TEventArgs> eventHandler)
        {
            if (!this.EventDictionary.ContainsKey(eventId))
                throw new Exception("Event dictionary does not have an entry for the event name provided.");

            EventHandler<TEventArgs> storedDelegate = this.EventDictionary[eventId] as EventHandler<TEventArgs>;
            if (storedDelegate == null)
                throw new Exception("Stored event handler delegate is not the same type as that being removed.");

            this.EventDictionary[eventId] = storedDelegate - eventHandler;
        }

        public EventHandler<TEventArgs> GetEventHandler<TEventArgs>(EventIdentifier<TInterface> eventId)
        {
            if (this.EventDictionary == null || !this.EventDictionary.ContainsKey(eventId))
                return null;

            Delegate storedDelegate = this.EventDictionary[eventId];
            if (storedDelegate == null)
                return null;

            if (!(storedDelegate is EventHandler<TEventArgs>))
                throw new Exception("Stored event handler delegate is not of the type expected.");

            return (EventHandler<TEventArgs>)storedDelegate;
        }
    }
}
