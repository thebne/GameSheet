using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;

namespace OneHamsa.Dexterity.Builtins
{
    [Preserve]
    public class UIPressField : BaseField
    {
        DexterityUIPressFieldProvider provider = null;
        public class DexterityUIPressFieldProvider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
        {
            internal UIPressField field;
            
            public void OnPointerDown(PointerEventData eventData) => field.SetValue(1);
            public void OnPointerUp(PointerEventData eventData) => field.SetValue(0);
        }

        protected override void Initialize(FieldNode context)
        {
            base.Initialize(context);

            provider = context.gameObject.AddComponent<DexterityUIPressFieldProvider>();
        }
        public override void Finalize(FieldNode context)
        {
            base.Finalize(context);

            UnityEngine.Object.Destroy(provider);
        }
    }
}
