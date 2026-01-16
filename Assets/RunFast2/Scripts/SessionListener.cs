using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using UnityEngine;
using RunFast2.Scripts.Services; // 引用 AuthService

namespace RunFast2.Scripts
{
    public class SessionListener : MonoBehaviour
    {
        public SupabaseManager SupabaseManager = null!;

        public void UnityAuthListener(IGotrueClient<User, Session> sender, Constants.AuthState newState)
        {
            if (sender.CurrentUser?.Email == null)
                print("No user logged in");
            else
            {
                print($"Logged in as {sender.CurrentUser.Email}");
                
                // 当检测到用户登录或会话恢复时，同步到 AuthService
                if (AuthService.Instance != null && sender.CurrentSession != null)
                {
                    AuthService.Instance.SetCurrentUser(sender.CurrentUser, sender.CurrentSession.AccessToken);
                }
            }

            switch (newState)
            {
                case Constants.AuthState.SignedIn:
                    Debug.Log("Signed In");
                    break;
                case Constants.AuthState.SignedOut:
                    Debug.Log("Signed Out");
                    if (AuthService.Instance != null)
                    {
                        AuthService.Instance.ClearCurrentUser();
                    }
                    break;
                case Constants.AuthState.UserUpdated:
                    Debug.Log("User Updated");
                    break;
                case Constants.AuthState.PasswordRecovery:
                    Debug.Log("Password Recovery");
                    break;
                case Constants.AuthState.TokenRefreshed:
                    Debug.Log("Token Refreshed");
                    break;
                case Constants.AuthState.Shutdown:
                    Debug.Log("Shutdown");
                    break;
                default:
                    Debug.Log("Unknown Auth State Update");
                    break;
            }
        }
    }
}