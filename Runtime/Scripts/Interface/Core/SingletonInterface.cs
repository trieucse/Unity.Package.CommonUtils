using System.Diagnostics;
using UnityEngine;

namespace Trackman
{
    [DebuggerStepThrough]
    public abstract class SingletonInterface<ClassType, Interface> : MonoBehaviour, ISingletonInterface where ClassType : MonoBehaviour, Interface where Interface : ISingletonInterface
    {
        #region Fields
        static ClassType instance;
        string className;
        #endregion

        #region Properties
        protected static ClassType Instance => instance = !instance ? FindObjectOfType<ClassType>(true) : instance;
        public static bool Enabled => Instance;
        public static Interface I => Instance.As<Interface>();
        public string ClassName => className.NotNullOrEmpty() ? className : GetType().Name;

        public virtual string Status => string.Empty;
        public virtual bool Testable => false;
        #endregion

        #region Methods
        protected virtual void Awake()
        {
            instance = (ClassType)(object)this;
            className = GetType().Name;
        }
        protected virtual void OnEnable()
        {
            instance = (ClassType)(object)this;
            className = GetType().Name;
        }
        void ISingletonInterface.InitializeForTests()
        {
            Awake();
            OnEnable();
        }
        #endregion
    }
}