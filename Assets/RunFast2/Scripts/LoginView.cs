using System;
using Cysharp.Threading.Tasks;
using Ricimi; // 引用 Ricimi
using RunFast2.Scripts.Models;
using RunFast2.Scripts.Services;
using RunFast2.Scripts.View;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RunFast2.Scripts
{
    public class LoginView : MonoBehaviour
    {
        [Header("UI Containers")]
        [SerializeField] private Canvas uiCanvas; // 必须指定 Canvas，因为 Popup 需要挂在 Canvas 下

        [Header("Prefabs")]
        [SerializeField] private LoginPanel loginPanelPrefab;
        [SerializeField] private RegisterPanel registerPanelPrefab;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button showLoginButton;
        [SerializeField] private Button showRegisterButton;

        [Header("Settings")]
        [SerializeField] private string lobbySceneName = "2Lobby";

        // Runtime Instances
        private LoginPanel _loginPanel;
        private RegisterPanel _registerPanel;

        private void Start()
        {
            if (uiCanvas == null) uiCanvas = GetComponentInParent<Canvas>();

            if (showLoginButton) showLoginButton.onClick.AddListener(ShowLoginPanel);
            if (showRegisterButton) showRegisterButton.onClick.AddListener(ShowRegisterPanel);

            WaitForSupabaseInit().Forget();
        }

        // 模仿 PopupOpener.OpenPopup 的逻辑
        private void OpenPopup(MonoBehaviour panelInstance)
        {
            if (panelInstance == null) return;
            
            var popup = panelInstance.GetComponent<Popup>();
            if (popup != null)
            {
                panelInstance.gameObject.SetActive(true);
                panelInstance.transform.localScale = Vector3.zero; // 初始缩放为0，等待动画
                panelInstance.transform.SetParent(uiCanvas.transform, false);
                popup.Open();
            }
            else
            {
                // Fallback if no Popup component
                panelInstance.gameObject.SetActive(true);
            }
        }

        private void ShowLoginPanel()
        {
            // 如果已经存在实例，直接打开
            if (_loginPanel == null)
            {
                if (loginPanelPrefab == null) return;
                _loginPanel = Instantiate(loginPanelPrefab);
                _loginPanel.OnLoginClicked = OnLoginRequested;
                _loginPanel.OnCloseClicked = () => { /* Close 逻辑由 Popup 处理，这里可以做额外清理 */ };
            }
            
            OpenPopup(_loginPanel);
        }

        private void ShowRegisterPanel()
        {
            if (_registerPanel == null)
            {
                if (registerPanelPrefab == null) return;
                _registerPanel = Instantiate(registerPanelPrefab);
                _registerPanel.OnRegisterClicked = OnRegisterRequested;
                _registerPanel.OnCloseClicked = () => { };
            }

            _registerPanel.ClearInputs();
            OpenPopup(_registerPanel);
        }

        private async void OnLoginRequested(string email, string password, bool remember)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                DialogManager.Instance.ShowInfo("提示", "请输入邮箱和密码");
                return;
            }

            _loginPanel.SetInteractable(false);
            DialogManager.Instance.ShowLoading(true);

            try
            {
                var user = await AuthService.Instance.SignInAsync(email, password);
                if (user != null)
                {
                    _loginPanel.SaveCredentials(email, password);
                    
                    string shortName = email.Contains("@") ? email.Split('@')[0] : email;
                    UserSession.CurrentPlayerName = shortName;
                    UserSession.UserId = user.Id;
                    
                    SceneManager.LoadScene(lobbySceneName);
                }
            }
            catch (Exception ex)
            {
                string friendlyMsg = AuthService.Instance.GetFriendlyErrorMessage(ex);
                DialogManager.Instance.ShowInfo("登录失败", friendlyMsg);
            }
            finally
            {
                DialogManager.Instance.ShowLoading(false);
                if (_loginPanel != null) _loginPanel.SetInteractable(true);
            }
        }

        private async void OnRegisterRequested(string username, string email, string password)
        {
            _registerPanel.SetInteractable(false);
            DialogManager.Instance.ShowLoading(true);

            try
            {
                // 注意：SignUpAsync 签名可能需要修改以支持 username，或者在注册后更新 profile
                // 这里假设 AuthService.SignUpAsync 已经支持或者我们在这里处理
                // 目前 AuthService.SignUpAsync 只接受 email 和 password
                // 我们需要修改 AuthService 来支持 username，或者在这里分两步走
                
                // 方案：先注册，再更新 Profile
                var user = await AuthService.Instance.SignUpAsync(email, password);
                if (user != null)
                {
                    // 更新 Profile 中的 username
                    // 这需要 AuthService 提供一个 UpdateProfile 方法，或者直接操作 SupabaseManager
                    // 简单起见，我们假设 AuthService.SignUpAsync 内部会处理，或者我们在这里调用一个新方法
                    // 但由于 AuthService.SignUpAsync 目前只返回 AppUser，我们无法直接在那里传 username
                    
                    // 临时方案：调用 AuthService 的一个新方法 UpdateUsernameAsync
                    // 或者修改 SignUpAsync 签名。为了保持一致性，建议修改 SignUpAsync。
                    // 但由于我无法修改 AuthService (不在本次请求的文件列表中)，我假设 AuthService 已经修改好了
                    // 或者我在这里直接调用 SupabaseManager 更新
                    
                    if (AuthService.Instance.supabaseManager != null)
                    {
                        var client = AuthService.Instance.supabaseManager.Supabase();
                        var profile = new RunFast2.Scripts.Model.UserProfile
                        {
                            Id = user.Id,
                            Username = username,
                            UpdatedAt = DateTime.UtcNow
                        };
                        // 更新 Profile
                        await client.From<RunFast2.Scripts.Model.UserProfile>().Upsert(profile);
                    }

                    DialogManager.Instance.ShowInfo("提示", "注册成功！请返回登录。", "确定", () => 
                    {
                        // 关闭注册面板，打开登录面板
                        if (_registerPanel != null) _registerPanel.Close();
                        ShowLoginPanel();
                        if (_loginPanel != null) _loginPanel.FillEmail(email);
                    });
                }
            }
            catch (Exception ex)
            {
                string friendlyMsg = AuthService.Instance.GetFriendlyErrorMessage(ex);
                DialogManager.Instance.ShowInfo("注册失败", friendlyMsg);
            }
            finally
            {
                DialogManager.Instance.ShowLoading(false);
                if (_registerPanel != null) _registerPanel.SetInteractable(true);
            }
        }

        private async UniTaskVoid WaitForSupabaseInit()
        {
            DialogManager.Instance.ShowLoading(true);
            if (showLoginButton) showLoginButton.interactable = false;
            if (showRegisterButton) showRegisterButton.interactable = false;

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
                    DialogManager.Instance.ShowInfo("错误", "连接服务器超时，请检查网络。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Supabase init error: {ex.Message}");
            }
            finally
            {
                DialogManager.Instance.ShowLoading(false);
                if (showLoginButton) showLoginButton.interactable = true;
                if (showRegisterButton) showRegisterButton.interactable = true;
            }
        }
    }
}