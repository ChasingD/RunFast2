using System;
using Ricimi; // 引用 Ricimi 命名空间
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.View
{
    // 确保物体上有 Popup 组件
    [RequireComponent(typeof(Popup))]
    public class LoginPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Toggle rememberMeToggle;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button closeButton;

        // 事件委托
        public Action<string, string, bool> OnLoginClicked;
        public Action OnCloseClicked;

        private Popup _popup;
        private const string PREF_EMAIL = "SavedEmail";
        private const string PREF_PASSWORD = "SavedPassword";
        private const string PREF_REMEMBER = "RememberMe";

        private void Awake()
        {
            _popup = GetComponent<Popup>();
        }

        private void Start()
        {
            if (loginButton) loginButton.onClick.AddListener(HandleLoginClick);
            if (closeButton) closeButton.onClick.AddListener(HandleCloseClick);

            LoadSavedCredentials();
        }

        private void HandleLoginClick()
        {
            string email = emailInput.text;
            string password = passwordInput.text;
            bool remember = rememberMeToggle != null && rememberMeToggle.isOn;

            OnLoginClicked?.Invoke(email, password, remember);
        }

        private void HandleCloseClick()
        {
            // 调用 Popup 的 Close 方法播放关闭动画
            if (_popup != null) _popup.Close();
            OnCloseClicked?.Invoke();
        }

        public void Close()
        {
            if (_popup != null) _popup.Close();
        }

        public void SetInteractable(bool interactable)
        {
            if (loginButton) loginButton.interactable = interactable;
            if (emailInput) emailInput.interactable = interactable;
            if (passwordInput) passwordInput.interactable = interactable;
        }

        public void SaveCredentials(string email, string password)
        {
            if (rememberMeToggle != null && rememberMeToggle.isOn)
            {
                PlayerPrefs.SetInt(PREF_REMEMBER, 1);
                PlayerPrefs.SetString(PREF_EMAIL, email);
                PlayerPrefs.SetString(PREF_PASSWORD, password);
            }
            else
            {
                PlayerPrefs.SetInt(PREF_REMEMBER, 0);
                PlayerPrefs.DeleteKey(PREF_EMAIL);
                PlayerPrefs.DeleteKey(PREF_PASSWORD);
            }
            PlayerPrefs.Save();
        }

        private void LoadSavedCredentials()
        {
            if (rememberMeToggle == null) return;

            bool remember = PlayerPrefs.GetInt(PREF_REMEMBER, 0) == 1;
            rememberMeToggle.isOn = remember;

            if (remember)
            {
                if (emailInput) emailInput.text = PlayerPrefs.GetString(PREF_EMAIL, "");
                if (passwordInput) passwordInput.text = PlayerPrefs.GetString(PREF_PASSWORD, "");
            }
        }
        
        public void FillEmail(string email)
        {
            if (emailInput) emailInput.text = email;
            if (passwordInput) passwordInput.text = "";
        }
    }
}