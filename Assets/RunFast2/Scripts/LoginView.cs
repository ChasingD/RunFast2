using System;
using Cysharp.Threading.Tasks;
using RunFast2.Scripts.Models;
using RunFast2.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RunFast2.Scripts
{
    public class LoginView : MonoBehaviour
    {
        [Header("Login Panel References")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private TMP_InputField loginEmailInput;
        [SerializeField] private TMP_InputField loginPasswordInput;
        [SerializeField] private Toggle rememberMeToggle;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button goToRegisterButton;

        [Header("Register Panel References")]
        [SerializeField] private GameObject registerPanel;
        [SerializeField] private TMP_InputField regEmailInput;
        [SerializeField] private TMP_InputField regPasswordInput;
        [SerializeField] private TMP_InputField regConfirmPasswordInput;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button backToLoginButton;

        [Header("Settings")]
        [SerializeField] private string lobbySceneName = "2Lobby";
        
        private const string PREF_EMAIL = "SavedEmail";
        private const string PREF_PASSWORD = "SavedPassword";
        private const string PREF_REMEMBER = "RememberMe";

        private void Start()
        {
            // 绑定事件
            loginButton.onClick.AddListener(OnLoginClicked);
            goToRegisterButton.onClick.AddListener(ShowRegisterPanel);
            
            registerButton.onClick.AddListener(OnRegisterClicked);
            backToLoginButton.onClick.AddListener(ShowLoginPanel);

            // 初始化状态
            ShowLoginPanel();
            LoadSavedCredentials();
            
            // 启动时等待 Supabase 初始化
            WaitForSupabaseInit().Forget();
        }

        private void ShowLoginPanel()
        {
            loginPanel.SetActive(true);
            registerPanel.SetActive(false);
        }

        private void ShowRegisterPanel()
        {
            loginPanel.SetActive(false);
            registerPanel.SetActive(true);
            // 清空注册输入
            regEmailInput.text = "";
            regPasswordInput.text = "";
            regConfirmPasswordInput.text = "";
        }

        private void LoadSavedCredentials()
        {
            bool remember = PlayerPrefs.GetInt(PREF_REMEMBER, 0) == 1;
            rememberMeToggle.isOn = remember;

            if (remember)
            {
                loginEmailInput.text = PlayerPrefs.GetString(PREF_EMAIL, "");
                // 注意：实际项目中密码应该加密存储，这里为了演示直接存明文
                loginPasswordInput.text = PlayerPrefs.GetString(PREF_PASSWORD, "");
            }
        }

        private void SaveCredentials(string email, string password)
        {
            if (rememberMeToggle.isOn)
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

        private async UniTaskVoid WaitForSupabaseInit()
        {
            DialogManager.Instance.ShowLoading(true);
            loginButton.interactable = false;
            registerButton.interactable = false;

            try
            {
                float timeout = 10f;
                float timer = 0f;
                
                while (timer < timeout)
                {
                    if (AuthService.Instance != null && 
                        AuthService.Instance.supabaseManager != null)
                    {
                        var client = AuthService.Instance.supabaseManager.Supabase();
                        if (client != null) break;
                    }
                    
                    await UniTask.Delay(100);
                    timer += 0.1f;
                }

                if (timer >= timeout)
                {
                    Debug.LogError("Supabase initialization timed out.");
                    ShowMessage("连接服务器超时，请检查网络。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Supabase init error: {ex.Message}");
            }
            finally
            {
                DialogManager.Instance.ShowLoading(false);
                loginButton.interactable = true;
                registerButton.interactable = true;
            }
        }

        private async void OnLoginClicked()
        {
            string email = loginEmailInput.text;
            string password = loginPasswordInput.text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("请输入邮箱和密码");
                return;
            }

            loginButton.interactable = false;
            DialogManager.Instance.ShowLoading(true);

            try
            {
                var user = await AuthService.Instance.SignInAsync(email, password);
                if (user != null)
                {
                    SaveCredentials(email, password);
                    
                    string shortName = email.Contains("@") ? email.Split('@')[0] : email;
                    UserSession.CurrentPlayerName = shortName;
                    UserSession.UserId = user.Id;
                    
                    SceneManager.LoadScene(lobbySceneName);
                }
            }
            catch (Exception ex)
            {
                string friendlyMsg = AuthService.Instance.GetFriendlyErrorMessage(ex);
                ShowMessage(friendlyMsg);
            }
            finally
            {
                DialogManager.Instance.ShowLoading(false);
                if (this != null) loginButton.interactable = true;
            }
        }

        private async void OnRegisterClicked()
        {
            string email = regEmailInput.text;
            string password = regPasswordInput.text;
            string confirm = regConfirmPasswordInput.text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("请输入邮箱和密码");
                return;
            }

            if (password != confirm)
            {
                ShowMessage("两次输入的密码不一致");
                return;
            }

            registerButton.interactable = false;
            DialogManager.Instance.ShowLoading(true);

            try
            {
                var user = await AuthService.Instance.SignUpAsync(email, password);
                if (user != null)
                {
                    ShowMessage("注册成功！请返回登录。", () => 
                    {
                        ShowLoginPanel();
                        // 自动填充注册的账号
                        loginEmailInput.text = email;
                        loginPasswordInput.text = "";
                    });
                }
            }
            catch (Exception ex)
            {
                string friendlyMsg = AuthService.Instance.GetFriendlyErrorMessage(ex);
                ShowMessage(friendlyMsg);
            }
            finally
            {
                DialogManager.Instance.ShowLoading(false);
                if (this != null) registerButton.interactable = true;
            }
        }

        private void ShowMessage(string msg, Action onClose = null)
        {
            DialogManager.Instance.ShowInfo("提示", msg, "确定", onClose);
        }
    }
}