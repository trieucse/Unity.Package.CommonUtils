using UnityEngine;

namespace Trackman
{
    public abstract class MonoBehaviourInjectable : MonoBehaviour, IMonoBehaviourInjectable
    {
        #region Base Methods
        protected virtual void OnEnable() => this.Inject();
        protected virtual void OnDisable() => this.Eject();
        #endregion
    }
}