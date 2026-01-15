using System;
using Ricimi; // 引用 Ricimi
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.View
{
    [RequireComponent(typeof(Popup))]
    public class RegisterPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField usernameInput; // 新增用户名输入框
        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private TMP_InputField confirmPasswordInput;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button closeButton;

        // 事件委托
        public Action<string, string, string> OnRegisterClicked; // 增加 username 参数
        public Action OnCloseClicked;

        private Popup _popup;

        private void Awake()
        {
            _popup = GetComponent<Popup>();
        }

        private void Start()
        {
            if (registerButton) registerButton.onClick.AddListener(HandleRegisterClick);
            if (closeButton) closeButton.onClick.AddListener(HandleCloseClick);
        }

        private void HandleRegisterClick()
        {
            string username = usernameInput != null ? usernameInput.text : "";
            string email = emailInput.text;
            string password = passwordInput.text;
            string confirm = confirmPasswordInput.text;

            if (string.IsNullOrWhiteSpace(username))
            {
                DialogManager.Instance.ShowInfo("提示", "请输入用户名");
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                DialogManager.Instance.ShowInfo("提示", "请输入邮箱和密码");
                return;
            }

            if (password != confirm)
            {
                DialogManager.Instance.ShowInfo("提示", "两次输入的密码不一致");
                return;
            }

            OnRegisterClicked?.Invoke(username, email, password);
        }

        private void HandleCloseClick()
        {
            if (_popup != null) _popup.Close();
            OnCloseClicked?.Invoke();
        }

        public void Close()
        {
            if (_popup != null) _popup.Close();
        }

        public void SetInteractable(bool interactable)
        {
            if (registerButton) registerButton.interactable = interactable;
            if (usernameInput) usernameInput.interactable = interactable;
            if (emailInput) emailInput.interactable = interactable;
            if (passwordInput) passwordInput.interactable = interactable;
            if (confirmPasswordInput) confirmPasswordInput.interactable = interactable;
        }
        
        public void ClearInputs()
        {
            if (usernameInput) usernameInput.text = "";
            if (emailInput) emailInput.text = "";
            if (passwordInput) passwordInput.text = "";
            if (confirmPasswordInput) confirmPasswordInput.text = "";
        }
    }
}