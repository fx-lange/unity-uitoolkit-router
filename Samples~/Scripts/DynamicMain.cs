using UnityEngine;
using UnityEngine.UIElements;

namespace UITK.Router.Sample
{
    [DefaultExecutionOrder(500)]
    public class DynamicMain : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private DynamicNavigation _navigation;

        private Router _router;
        
        private void OnEnable()
        {
            var view = _uiDocument.rootVisualElement;
            _navigation.Init(view);
            
            _router = _navigation.Router;
            _router.BeforeEach += (to, from ) =>
            {
                if (to.Name == DynamicNavigation.Admin)
                {
                    return DynamicNavigation.Login;
                }

                return to;
            };

            view.Q<Button>("about").clickable.clicked += async () => await _router.Push(DynamicNavigation.About);
            view.Q<Button>("settings").clickable.clicked += async () => await _router.Push(DynamicNavigation.Settings);
            view.Q<Button>("admin").clickable.clicked += async () => await _router.Push(DynamicNavigation.Admin);
            view.Q<Button>("login").clickable.clicked += async () => await _router.Push(DynamicNavigation.Login);
            view.Q<Button>("user").clickable.clicked += async () => await _router.Push(DynamicNavigation.User);
       
            var backButton = view.Q<Button>("back");
            backButton.clickable.clicked += async () => await _router.Back();
            backButton.SetEnabled(false);
            _router.AfterEach += (_, _) => { backButton.SetEnabled(_router.HasHistory); };
            
            var upButton = view.Q<Button>("up");
            upButton.clickable.clicked += async () => await _router.Up();
            upButton.SetEnabled(false);
            _router.AfterEach += (_, _) => { upButton.SetEnabled(_router.Depth > 0); };
        }
    }
}