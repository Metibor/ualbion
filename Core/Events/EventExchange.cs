﻿using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class EventExchange
    {
        readonly object _syncRoot = new object();
        readonly IDictionary<Type, IList<IComponent>> _subscriptions = new Dictionary<Type, IList<IComponent>>();
        readonly IDictionary<IComponent, IList<Type>> _subscribers = new Dictionary<IComponent, IList<Type>>();
        readonly IDictionary<Type, RegisteredComponent> _registrations = new Dictionary<Type, RegisteredComponent>();
        readonly EventExchange _parent;
        readonly IList<EventExchange> _children = new List<EventExchange>();
        readonly SubscribedEvent _subscribedEvent = new SubscribedEvent();
        public string Name { get; }
        public bool IsActive { get; set; } = true;

        public IReadOnlyList<EventExchange> Children { get { lock (_syncRoot) return _children.ToList(); } }

        public EventExchange(string name, EventExchange parent = null)
        {
            Name = name;
            _parent = parent;
            _parent?.AddChild(this);
        }

        void AddChild(EventExchange eventExchange)
        {
            lock(_syncRoot)
                _children.Add(eventExchange);
        }

        public bool Contains(IComponent component) { lock(_syncRoot) return _subscribers.ContainsKey(component); }
        public void Raise(IEvent e, object sender) { Raise(e, sender, null); }
        void Raise(IEvent e, object sender, EventExchange sourceExchange)
        {
            if (!IsActive)
                return;

            HashSet<IComponent> subscribers = new HashSet<IComponent>();
            var interfaces = e.GetType().GetInterfaces();
            string eventText = null;
            if (CoreTrace.Log.IsEnabled())
            {
                eventText = e.ToString();
                CoreTrace.Log.StartRaise(e.GetType().Name, eventText);
            }

            lock (_syncRoot)
            {
                if (_subscriptions.TryGetValue(e.GetType(), out var tempSubscribers))
                    foreach (var subscriber in tempSubscribers)
                        subscribers.Add(subscriber);

                foreach(var @interface in interfaces)
                    if (_subscriptions.TryGetValue(@interface, out var interfaceSubscribers))
                        foreach (var subscriber in interfaceSubscribers)
                            subscribers.Add(subscriber);
            }

            foreach(var subscriber in subscribers)
                subscriber.Receive(e, sender);

            if(_parent != null && sourceExchange != _parent)
                _parent.Raise(e, sender, this);

            IList<EventExchange> children;
            lock (_children) { children = _children.ToList(); }
            foreach(var childExchange in children)
                if(childExchange != sourceExchange)
                    childExchange.Raise(e, sender, this);

            if (eventText != null)
                CoreTrace.Log.StopRaise(e.GetType().Name, eventText, subscribers.Count);
        }

        public void Subscribe<T>(IComponent subscriber) { Subscribe(typeof(T), subscriber); }
        public void Subscribe(Type eventType, IComponent subscriber)
        {
            bool newSubscriber = false;
            lock (_syncRoot)
            {
                if (_subscribers.TryGetValue(subscriber, out var subscribedTypes))
                {
                    if (subscribedTypes.Contains(eventType))
                        return;
                }
                else
                {
                    _subscribers[subscriber] = new List<Type>();
                    newSubscriber = true;
                }

                if (!_subscriptions.ContainsKey(eventType))
                    _subscriptions.Add(eventType, new List<IComponent>());

                _subscriptions[eventType].Add(subscriber);
                _subscribers[subscriber].Add(eventType);
            }
            if (newSubscriber)
                subscriber.Receive(_subscribedEvent, this);
        }

        public void Unsubscribe(IComponent subscriber)
        {
            lock (_syncRoot)
            {
                if (!_subscribers.TryGetValue(subscriber, out var subscribedTypes))
                    return;

                foreach (var type in subscribedTypes.ToList())
                    _subscriptions[type].Remove(subscriber);

                _subscribers.Remove(subscriber);
            }
        }

        public void Unsubscribe<T>(IComponent subscriber)
        {
            lock (_syncRoot)
            {
                if (!_subscribers.TryGetValue(subscriber, out var subscribedTypes))
                    return;

                subscribedTypes.Remove(typeof(T));
                _subscriptions[typeof(T)].Remove(subscriber);
            }
        }

        public void Register(RegisteredComponent rc) { _registrations.Add(rc.GetType(), rc); }

        public T Resolve<T>() where T : RegisteredComponent
        {
            if (!_registrations.ContainsKey(typeof(T)))
                return null;
            return (T) _registrations[typeof(T)];
        }

        public void PruneInactiveChildren()
        {
            lock (_syncRoot)
            {
                for (int i = 0; i < _children.Count;)
                {
                    if (!_children[i].IsActive)
                        _children.RemoveAt(i);
                    else i++;
                }
            }
        }
    }
}