
namespace Trackman
{
    public class MonoBehaviourCollectionItem : MonoBehaviourInjectable, IMonoBehaviourCollectionItem
    {
        #region Base Methods
        protected override void OnEnable()
        {
            base.OnEnable();

            this.Add();
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            this.Remove();
        }
        #endregion
    }
}