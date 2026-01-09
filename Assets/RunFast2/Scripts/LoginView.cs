using System;
using Cysharp.Threading.Tasks;
using RunFast2.Scripts.Models;
using RunFast2.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// 引用 AuthService

namespace RunFast2.Scripts
{
    public class LoginView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button signInButton;
        [SerializeField] private Button signUpButton;
        [SerializeField] private TMP_InputField emailInput = null!;
        [SerializeField] private TMP_InputField passwordInput = null!;
        
        // 不再需要直接引用 SupabaseManager，因为 AuthService 已经处理了
        
        private void Start()
        {
            signInButton.onClick.AddListener(OnSignInClicked);
            signUpButton.onClick.AddListener(OnSignUpClicked);
        }

        private async void OnSignInClicked() => await HandleAuthAction(AuthType.SignIn);
        private async void OnSignUpClicked() => await HandleAuthAction(AuthType.SignUp);

        private enum AuthType { SignIn, SignUp }

        private async UniTask HandleAuthAction(AuthType type)
        {
            // 1. UI 输入校验
            if (string.IsNullOrWhiteSpace(emailInput.text) || string.IsNullOrWhiteSpace(passwordInput.text))
            {
                ShowMessage("请输入邮箱和密码");
                return;
            }

            // 2. 锁定 UI
            Button currentButton = type == AuthType.SignIn ? signInButton : signUpButton;
            currentButton.interactable = false;

            try
            {
                // 获取销毁 Token，防止异步期间 UI 被销毁报错
                var token = this.GetCancellationTokenOnDestroy();

                AppUser user = null;

                // 3. 调用 AuthService 执行逻辑
                if (type == AuthType.SignIn)
                {
                    user = await AuthService.Instance.SignInAsync(emailInput.text, passwordInput.text);
                    if (user != null)
                    {
                        ShowMessage($"登录成功: {user.Email}");
                        Debug.Log("跳转场景中...");
                        // TODO: SceneManager.LoadScene("GameScene");
                    }
                }
                else
                {
                    user = await AuthService.Instance.SignUpAsync(emailInput.text, passwordInput.text);
                    // 注册成功逻辑
                    ShowMessage($"注册请求已发送。用户: {user?.Email}");
                }
            }
            catch (Exception ex)
            {
                // 4. 错误处理委托给 AuthService 获取友好文本
                string friendlyMsg = AuthService.Instance.GetFriendlyErrorMessage(ex);
                Debug.LogError($"Auth Failed: {ex}");
                ShowMessage(friendlyMsg);
            }
            finally
            {
                // 5. 恢复 UI
                if (this != null && currentButton != null)
                {
                    currentButton.interactable = true;
                }
            }
        }

        // UI 弹窗封装
        private void ShowMessage(string msg)
        {
            // 假设你有一个单例的 DialogManager
            DialogManager.Instance.ShowInfo("提示", msg);
        }
    }
}