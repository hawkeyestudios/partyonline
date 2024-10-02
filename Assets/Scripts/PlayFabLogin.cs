using System;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
//using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

namespace KnoxGameStudios
{
    public class PlayfabLogin : MonoBehaviour
    {
        [SerializeField] public static string playFabId;

        [SerializeField] private InputField usernameInput;
        [SerializeField] private InputField emailInput;
        [SerializeField] private InputField passwordInput;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Button showPasswordButton;
        public GameObject LoadingText;
        public GameObject LoadingImage;
        public Animator animator;
        public Animator loginCameraAnim;

        private string username;
        private string email;
        private string password;
        private bool isPasswordVisible = false;

        private const string EMAIL_PREF_KEY = "USER_EMAIL";
        private const string PASSWORD_PREF_KEY = "USER_PASSWORD";

        #region Unity Methods
        void Start()
        {
            LoadingImage.SetActive(true);
            LoadingText.SetActive(false);
            if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
            {
                PlayFabSettings.TitleId = "DC6A9"; //PlayFab TitleId
            }
        }
        public void TapToStart()
        {
            Debug.Log("Basýldý");
            if (PlayerPrefs.HasKey(EMAIL_PREF_KEY) && PlayerPrefs.HasKey(PASSWORD_PREF_KEY))
            {
                LoadingImage.SetActive(false);
                LoadingText.SetActive(true);
                email = PlayerPrefs.GetString(EMAIL_PREF_KEY);
                password = PlayerPrefs.GetString(PASSWORD_PREF_KEY);
                LoginWithEmail();
                loginCameraAnim.SetTrigger("LoginCamera");
            }
            else
            {
                LoadingImage.SetActive(false);
                animator.SetTrigger("TapToStart");
            }
        }
        #endregion

        #region Private Methods
        private bool IsValidUsername()
        {
            return !string.IsNullOrEmpty(username) && username.Length >= 3 && username.Length <= 15;
        }

        private bool IsValidEmail()
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPassword()
        {
            return !string.IsNullOrEmpty(password) && password.Length >= 6; // Minimum password length
        }

        private void LoginWithEmail()
        {
            Debug.Log($"Login to PlayFab with Email: {email}");
            ShowFeedback("Logging in...");
            var request = new LoginWithEmailAddressRequest { Email = email, Password = password };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginEmailSuccess, OnFailure);
        }

        private void SignUpWithEmail()
        {
            Debug.Log($"Signing up to PlayFab with Email: {email}");
            ShowFeedback("Signing up...");
            var request = new RegisterPlayFabUserRequest { Email = email, Password = password, Username = username };
            PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnFailure);
        }

        private void UpdateDisplayName(string displayName)
        {
            Debug.Log($"Updating PlayFab account's Display name to: {displayName}");
            ShowFeedback("Updating display name...");
            var request = new UpdateUserTitleDisplayNameRequest { DisplayName = displayName };
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameSuccess, OnFailure);
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
            }
        }

        private void SaveCredentials()
        {
            PlayerPrefs.SetString(EMAIL_PREF_KEY, email);
            PlayerPrefs.SetString(PASSWORD_PREF_KEY, password);
            PlayerPrefs.Save();
        }

        private void TogglePasswordVisibility()
        {
            isPasswordVisible = !isPasswordVisible;
            passwordInput.contentType = isPasswordVisible ? InputField.ContentType.Standard : InputField.ContentType.Password;
            passwordInput.ForceLabelUpdate(); 
        }
        #endregion

        #region Public Methods
        public void SetUsername()
        {
            username = usernameInput.text;
            PlayerPrefs.SetString("DISPLAYNAME", username);
        }

        public void SetEmail()
        {
            email = emailInput.text;
        }

        public void SetPassword()
        {
            password = passwordInput.text;
        }

        public void Login()
        {
            SetEmail();
            SetPassword();
            if (!IsValidEmail() || !IsValidPassword())
            {
                ShowFeedback("Invalid email or password.");
                return;
            }

            LoginWithEmail();
        }

        public void SignUp()
        {
            SetUsername();
            SetEmail();
            SetPassword();
            if (!IsValidUsername() || !IsValidEmail() || !IsValidPassword())
            {
                ShowFeedback("Invalid username, email, or password.");
                return;
            }

            SignUpWithEmail();
        }
        #endregion

        #region PlayFab Callbacks
        private void OnLoginEmailSuccess(LoginResult result)
        {
            Debug.Log($"You have logged into PlayFab with email {email}");
            ShowFeedback("Login successful!");
            SaveCredentials();
            GetUserAccountInfo();

            var request = new LoginWithEmailAddressRequest
            {
                Email = email,
                Password = password,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetUserAccountInfo = true // Oyuncu bilgilerini almak için bu gerekli
                }
            };

            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnFailure);
        }
        private void GetUserAccountInfo()
        {
            var request = new GetAccountInfoRequest();
            PlayFabClientAPI.GetAccountInfo(request, OnGetAccountInfoSuccess, OnFailure);
        }
        private void OnLoginSuccess(LoginResult result)
        {
            playFabId = result.PlayFabId;  // PlayFab ID'yi burada alıyoruz
            Debug.Log("Login successful! PlayFab ID: " + playFabId);
        }
        private void OnGetAccountInfoSuccess(GetAccountInfoResult result)
        {
            if (result.AccountInfo != null)
            {
                string displayName = result.AccountInfo.TitleInfo.DisplayName;
                PhotonNetwork.NickName = displayName;
                PlayerPrefs.SetString("DISPLAYNAME", displayName);
                PlayerPrefs.Save();

                Debug.Log($"DisplayName retrieved from PlayFab: {displayName}");

                SceneController.LoadScene("MainMenu");
            }
            else
            {
                Debug.LogWarning("Failed to retrieve DisplayName from PlayFab.");
            }
        }

        private void OnRegisterSuccess(RegisterPlayFabUserResult result)
        {
            Debug.Log($"You have successfully registered with PlayFab using email {email}");

            PlayerPrefs.SetInt("FirstTimeRegistration", 1);
            PlayerPrefs.Save();

            UpdateDisplayName(username);
        }


        private void OnDisplayNameSuccess(UpdateUserTitleDisplayNameResult result)
        {
            Debug.Log($"You have updated the display name of the PlayFab account!");
            ShowFeedback("Registration successful!");
            SaveCredentials();
            SceneController.LoadScene("MainMenu");
        }

        private void OnFailure(PlayFabError error)
        {
            Debug.Log($"There was an issue with your request: {error.GenerateErrorReport()}");
            ShowFeedback($"Error: {error.ErrorMessage}");
        }
        #endregion
    }
}
