using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Trackman
{
    [DebuggerStepThrough]
    public static class EventExtensions
    {
        #region Methods
        public static IEnumerable<T> FindEvents<T>(this MonoBehaviour _) where T : IEvent => ServiceLocator.FindInterfaces<T>().OrderBy(x => x.Order);
        public static void ExecuteEvent<T>(this MonoBehaviour value, Action<T> execute) where T : IEvent => FindEventsActive<T>(value).ForEach(execute);
        public static void ExecuteEventValid<T>(this MonoBehaviour value, Action<T> execute) where T : IEvent => FindEventsValid<T>(value).ForEach(execute);

        public static async Task ExecuteEventAsync<T>(this MonoBehaviour value, Func<T, Task> execute) where T : IEvent
        {
            foreach (T item in FindEventsActive<T>(value)) await execute(item);
        }
        public static async Task<List<U>> ExecuteEventFuncAsync<T, U>(this MonoBehaviour value, Func<T, Task<U>> execute) where T : IEvent
        {
            List<U> results = new();
            foreach (T item in FindEventsActive<T>(value))
                results.Add(await execute(item));

            return results;
        }
        public static async Task<U> ExecuteEventFuncAsync<T, U>(this MonoBehaviour value, Func<T, Task<U>> execute, Func<U, bool> predicate) where T : IEvent
        {
            U result = default;
            foreach (T item in FindEventsActive<T>(value))
            {
                U obj = await execute(item.As<T>());
                if (predicate(obj)) result = obj;
            }

            return result;
        }
        #endregion

        #region Support Methods
        static IEnumerable<T> FindEventsActive<T>(MonoBehaviour value) where T : IEvent => FindEvents<T>(value).Where(x => x is MonoBehaviour { isActiveAndEnabled: true});
        static IEnumerable<T> FindEventsValid<T>(MonoBehaviour value) where T : IEvent => FindEvents<T>(value).Where(x => x.CanExecute && x is MonoBehaviour { isActiveAndEnabled: true});
        #endregion
    }
}