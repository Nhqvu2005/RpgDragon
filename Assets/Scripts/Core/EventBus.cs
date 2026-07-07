using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGDragon.Core
{
    // ───── Event Data Structs ──────────────────────────────────────────────────

    /// <summary>
    /// Raised when the player takes damage.
    /// </summary>
    public struct PlayerDamagedEvent
    {
        public int Damage;
        public int CurrentHP;
    }

    /// <summary>
    /// Raised when the player dies.
    /// </summary>
    public struct PlayerDiedEvent { }

    /// <summary>
    /// Raised when an enemy is defeated.
    /// </summary>
    public struct EnemyDefeatedEvent
    {
        public string EnemyType;
        public Vector2 Position;
    }

    /// <summary>
    /// Raised when a new quest is started.
    /// </summary>
    public struct QuestStartedEvent
    {
        public string QuestId;
    }

    /// <summary>
    /// Raised when progress is made toward a quest objective.
    /// </summary>
    public struct QuestProgressEvent
    {
        public string QuestId;
        public string ObjectiveId;
        public int Current;
        public int Target;
    }

    /// <summary>
    /// Raised when a quest is completed.
    /// </summary>
    public struct QuestCompletedEvent
    {
        public string QuestId;
    }

    /// <summary>
    /// Raised when dialogue with an NPC begins.
    /// </summary>
    public struct DialogueStartedEvent
    {
        public string NpcId;
    }

    /// <summary>
    /// Raised when dialogue ends.
    /// </summary>
    public struct DialogueEndedEvent
    {
        public string QuestId;
    }

    /// <summary>
    /// Raised when a boss enemy is defeated.
    /// </summary>
    public struct BossDefeatedEvent { }

    /// <summary>
    /// Raised when a scene transition is triggered.
    /// </summary>
    public struct SceneTransitionEvent
    {
        public string SceneName;
    }

    /// <summary>
    /// Raised when the game-over sequence should begin.
    /// </summary>
    public struct GameOverEvent { }

    // ───── Event Bus ───────────────────────────────────────────────────────────

    /// <summary>
    /// A generic, type-safe event bus for decoupled communication between systems.
    /// Uses a Dictionary{Type, Delegate} to store handlers. Events are value-type
    /// structs to minimize allocations.
    ///
    /// <para>
    /// Usage:
    /// <code>
    /// EventBus.Register&lt;PlayerDamagedEvent&gt;(OnPlayerDamaged);
    /// EventBus.Raise(new PlayerDamagedEvent { Damage = 10, CurrentHP = 50 });
    /// EventBus.Unregister&lt;PlayerDamagedEvent&gt;(OnPlayerDamaged);
    /// </code>
    /// </para>
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _events = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Registers a handler to be invoked when <typeparamref name="T"/> is raised.
        /// </summary>
        /// <typeparam name="T">The event data struct type.</typeparam>
        /// <param name="handler">The callback action.</param>
        public static void Register<T>(Action<T> handler) where T : struct
        {
            Type type = typeof(T);
            if (_events.TryGetValue(type, out Delegate existing))
            {
                _events[type] = Delegate.Combine(existing, handler);
            }
            else
            {
                _events[type] = handler;
            }
        }

        /// <summary>
        /// Unregisters a previously registered handler.
        /// </summary>
        /// <typeparam name="T">The event data struct type.</typeparam>
        /// <param name="handler">The callback action to remove.</param>
        public static void Unregister<T>(Action<T> handler) where T : struct
        {
            Type type = typeof(T);
            if (_events.TryGetValue(type, out Delegate existing))
            {
                Delegate newDelegate = Delegate.Remove(existing, handler);
                if (newDelegate == null)
                {
                    _events.Remove(type);
                }
                else
                {
                    _events[type] = newDelegate;
                }
            }
        }

        /// <summary>
        /// Raises an event, invoking all registered handlers for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The event data struct type.</typeparam>
        /// <param name="eventData">The event payload.</param>
        public static void Raise<T>(T eventData) where T : struct
        {
            Type type = typeof(T);
            if (_events.TryGetValue(type, out Delegate handler))
            {
                (handler as Action<T>)?.Invoke(eventData);
            }
        }

        /// <summary>
        /// Removes all registered event handlers. Call this on scene changes or
        /// when resetting the game state to avoid stale listeners.
        /// </summary>
        public static void Clear()
        {
            _events.Clear();
        }
    }
}
