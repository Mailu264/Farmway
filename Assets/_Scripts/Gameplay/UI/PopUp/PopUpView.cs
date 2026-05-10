using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Farmway.Gameplay.UI.PopUp
{
    public abstract class PopUpView : MonoBehaviour
    {
        [field: SerializeField] public GameObject Container { get; private set; }

        [SerializeField] private bool _isAutomaticInit;
        
        [CanBeNull] public Button OpenButton;
        [CanBeNull] public Button CloseButton;

        public bool IsOpen { get; private set; }

        private void OnEnable()
        {
            if (!_isAutomaticInit || OpenButton != null && CloseButton == null)
                return;
            
            OpenButton?.onClick.AddListener(Show); 
            CloseButton?.onClick.AddListener(Hide); 
        }

        public void Show()
        {
            IsOpen = true;
            OnShow();
        }

        public void Hide()
        {
            IsOpen = false;
            OnHide();
        }
        
        public virtual void OnShow()
        {
            Container.SetActive(true);
        }
        
        public virtual void OnHide()
        {
            Container.SetActive(false);
        }
    }
}