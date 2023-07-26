#if PACKAGE_JAEGER
using OpenTracing;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Trackman
{
    public interface IMonoBehaviour
    {
        #region Properties
        MonoBehaviour MonoBehavior => this.As<MonoBehaviour>();
        #endregion
    }

    public interface IClassName
    {
        #region Properties
        string ClassName { get; }
        #endregion
    }

    public interface ISingletonInterface : IClassName, IMonoBehaviour
    {
        #region Properties
        string Status => string.Empty;
        bool Testable => false;
        #endregion

        #region Methods
        void InitializeForTests() => throw new NotSupportedException();
        #endregion
    }

    public interface ITracing : ISingletonInterface
    {
        #region Properties
#if PACKAGE_JAEGER
        IScope Active { get; }
#else
        IDisposable Active { get; }
#endif
        #endregion

        #region Methods
#if PACKAGE_JAEGER
        IScope Scope(string name, bool finishSpanOnDispose = true);
        IScope Scope(string name, IScope scope, bool finishSpanOnDispose = true);
        IScope Scope(string name, ISpanContext context, bool finishSpanOnDispose = true);
        IScope Scope(string name, DateTimeOffset timestamp, bool finishSpanOnDispose = true);
        IScope Scope(string name, DateTimeOffset timestamp, IScope scope, bool finishSpanOnDispose = true);
        IScope Scope(string name, DateTimeOffset timestamp, ISpanContext context, bool finishSpanOnDispose = true);
        void Dispose(IScope scope);

        IDictionary<string, string> Inject(IScope scope, IDictionary<string, string> dictionary);
        ISpanContext Extract(IDictionary<string, string> dictionary);
#else
        IDisposable Scope(string name, bool finishSpanOnDispose = true);
        IDisposable Scope(string name, IDisposable scope, bool finishSpanOnDispose = true);
        IDisposable Scope(string name, DateTimeOffset timestamp, bool finishSpanOnDispose = true);
        IDisposable Scope(string name, DateTimeOffset timestamp, IDisposable scope, bool finishSpanOnDispose = true);
        void Dispose(IDisposable scope);

        IDictionary<string, string> Inject(IDisposable scope, IDictionary<string, string> dictionary);
        IDisposable Extract(IDictionary<string, string> dictionary);
#endif
        void Connect(string address, string username, string password);
        void Close();
        #endregion
    }

    public interface IPrefs : IDictionary<string, object>
    {
        #region Methods
        bool Contains(string path);
        void Write(string path, object value);
        T Read<T>(string path, T defaultValue = default);
        #endregion
    }
}