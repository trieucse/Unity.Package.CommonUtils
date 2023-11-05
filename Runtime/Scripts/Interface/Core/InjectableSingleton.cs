using System.Diagnostics;
using UnityEngine;

namespace Trackman
{
    [DebuggerStepThrough]
    public abstract class InjectableSingleton<TClass, TInterface> : InjectSingleton<TClass, TInterface>, IMonoBehaviourInjectable where TClass : MonoBehaviour, TInterface where TInterface : ISingletonInjectable
    {
         #region Methods
        protected override void OnEnable()
        {
            base.OnEnable();

            this.Inject();
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            this.Eject();
        }
        #endregion
    }
}