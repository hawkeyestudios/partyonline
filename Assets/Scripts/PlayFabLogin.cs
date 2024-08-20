using System;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

namespace KnoxGameStudios
{
    public class PlayfabLogin : MonoBehaviour
    {

        [SerializeField] private InputField usernameInput;
        [SerializeField] private InputField emailInput;
        [SerializeField] private InputField passwordInput;
        [SerializeField] private Text feedbackText; // Geri bildirim metni için Text elemaný
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

        private Text coinstext;
        private Text gemstext;

        #region Unity Methods
        void Start()
        {
            LoadingImage.SetActive(true);
            LoadingText.SetActive(false);
            if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
            {
                PlayFabSettings.TitleId = "DC6A9"; // Your PlayFab TitleId
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
            return !string.IsNullOrEmpty(password) && password.Length >= 6; // Minimum password length check
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
            passwordInput.ForceLabelUpdate(); // Ensure the input field updates its display
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
            GetCharacterPurchaseStatus();
            GetUserAccountInfo();
            GetVirtualCurrencies();
        }
        private void GetCharacterPurchaseStatus()
        {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnGetUserDataSuccess, OnFailure);
        }
        public void GetVirtualCurrencies()
        {
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnGetUserInventorySuccess, OnFailure);
        }
        public void OnGetUserInventorySuccess(GetUserInventoryResult result)
        {
            int coins = result.VirtualCurrency["CN"];
            coinstext.text = coins.ToString();

            int gems = result.VirtualCurrency["GM"];
            gemstext.text = gems.ToString();
        }
        private void OnGetUserDataSuccess(GetUserDataResult result)
        {
            foreach (var entry in result.Data)
            {
                string key = entry.Key;
                string value = entry.Value.Value;

                if (key.EndsWith("_Purchased"))
                {
                    bool purchased = value == "1";
                    PlayerPrefs.SetInt(key, purchased ? 1 : 0);
                }
                else if (key.EndsWith("_Equipped"))
                {
                    bool equipped = value == "1";
                    PlayerPrefs.SetInt(key, equipped ? 1 : 0);
                }
                else if (key == "LastEquippedCharacter")
                {
                    PlayerPrefs.SetString("LastEquippedCharacter", value);
                }
                else if (key == "CurrentCoins")
                {
                    int coins = int.Parse(value);
                    CoinManager.Instance.SetCurrentCoins(coins); // Mevcut coinleri ayarla
                }
                else if (key == "CurrentGems")
                {
                    int gems = int.Parse(value);
                    GemManager.Instance.SetCurrentGems(gems); // Mevcut gemleri ayarla
                }
            }
            PlayerPrefs.Save();
        }


        private void GetUserAccountInfo()
        {
            var request = new GetAccountInfoRequest();
            PlayFabClientAPI.GetAccountInfo(request, OnGetAccountInfoSuccess, OnFailure);
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

                // Kullanýcýyý baþka bir sahneye yönlendirin
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

            // Oyuncunun ilk kez kayýt olduðunu belirle
            PlayerPrefs.SetInt("FirstTimeRegistration", 1);
            PlayerPrefs.Save();

            UpdateDisplayName(username); // Update display name after successful registration
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
