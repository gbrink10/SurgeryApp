using BNG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Add this at the top with other using statements

public class LoginManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainLoginPanel;    // Reference to the main login/register panel
    public GameObject confirmationPanel; // Reference to the confirmation code panel

    [Header("UI Elements")]
    public TMP_InputField emailInput; //my email input field - amit@wingsgames.com
    public TMP_InputField passwordInput;// my password input field - Passgb1145$
    public TMP_InputField confirmationCodeInput;
    public UnityEngine.UI.Button loginButton;
    public UnityEngine.UI.Button registerButton;
    public UnityEngine.UI.Button confirmButton;
    public UnityEngine.UI.Button resendButton; // Button to resend confirmation code
    public Text feedbackText;

    [Header("VR Keyboard")]
    //public VRKeyboard vrKeyboard; // Add reference to VR Keyboard

    private const string EMAIL_PREFS_KEY = "SavedEmail";
    
    #if UNITY_EDITOR
    private const string DEV_EMAIL = "amit@wingsgames.com";
    private const string DEV_PASSWORD = "Passgb1145$";
    #endif

    void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
        confirmButton.onClick.AddListener(OnConfirmClicked);
        resendButton.onClick.AddListener(OnResendClicked); // Add listener for resend button
        // Initialize panels
       
        // Hide confirmation input and button by default
        confirmationCodeInput.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        feedbackText.text = "";

        #if UNITY_EDITOR
        // Auto-fill credentials in editor
        emailInput.text = DEV_EMAIL;
        passwordInput.text = DEV_PASSWORD;
        #else
        // Load saved email if it exists
        if (PlayerPrefs.HasKey(EMAIL_PREFS_KEY))
        {
            emailInput.text = PlayerPrefs.GetString(EMAIL_PREFS_KEY);
        }
        #endif

        // Set up VR Keyboard connections
        // if (vrKeyboard != null)
        // {
        //     emailInput.onEndEdit.AddListener((text) => {
        //         vrKeyboard.gameObject.SetActive(true);
        //         vrKeyboard.AttachToInputField(emailInput);
        //     });

        //     passwordInput.onEndEdit.AddListener((text) => {
        //         vrKeyboard.gameObject.SetActive(true);
        //         vrKeyboard.AttachToInputField(passwordInput);
        //     });

        //     confirmationCodeInput.onEndEdit.AddListener((text) => {
        //         vrKeyboard.gameObject.SetActive(true);
        //         vrKeyboard.AttachToInputField(confirmationCodeInput);
        //     });
        // }
        mainLoginPanel.SetActive(true);
        confirmationPanel.SetActive(false);
        
    }
    void Awake() {
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        #if UNITY_EDITOR
        // Check for Enter key press in editor
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Only trigger login if we're on the main login panel
            if (mainLoginPanel.activeSelf)
            {
                OnLoginClicked();
            }
        }
        #endif
    }

    void OnResendClicked()
    {
        string email = emailInput.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            SetFeedback("⚠️ Please enter your email to resend the code.", Color.yellow);
            return;
        }

        SetFeedback("📨 Sending new confirmation code...", Color.white);

        StartCoroutine(CognitoAuth.ResendConfirmationCode(
            email,
            success =>
            {
                Debug.Log("Code resent:\n" + success);
                SetFeedback("✅ New confirmation code sent to your email.", Color.green);
            },
            error =>
            {
                Debug.LogError("Failed to resend code:\n" + error);
                SetFeedback("❌ Could not resend code. Try registering again.", Color.red);
            }));
    }

    void OnLoginClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetFeedback("⚠️ Please enter both email and password.", Color.yellow);
            return;
        }

        // Save email when attempting to login
        PlayerPrefs.SetString(EMAIL_PREFS_KEY, email);
        PlayerPrefs.Save();

        SetFeedback("🔐 Logging in...", Color.white);

        StartCoroutine(CognitoAuth.LoginUser(
            email,
            password,
            success =>
            {
                SetFeedback("✅ Login successful!", Color.green);
                Debug.Log($"Login success:\n{success}");

                // Store credentials across scenes
                AWSCredentialHolder.Credentials = CognitoAuth.CurrentCredentials;
                Debug.Log("[LoginManager] Stored AWS credentials: " + AWSCredentialHolder.Credentials?.GetType().Name);

                SceneManager.LoadScene(1);
            },
            error =>
            {
                Debug.LogError($"Login failed:\n{error}");
                
                // Check if user needs to confirm their account
                if (error.Contains("UserNotConfirmedException") || 
                    error.Contains("USER_PASSWORD_AUTH flow not enabled"))
                {
                    ShowConfirmationPanel();
                    SetFeedback("⚠️ Please confirm your account first.", Color.yellow);
                }
                else
                {
                    SetFeedback(ParseErrorMessage(error), Color.red);
                }
            }));
    }

    void OnRegisterClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetFeedback("⚠️ Please enter both email and password.", Color.yellow);
            return;
        }

        // Save email when attempting to register
        PlayerPrefs.SetString(EMAIL_PREFS_KEY, email);
        PlayerPrefs.Save();

        SetFeedback("📧 Registering...", Color.white);

        StartCoroutine(CognitoAuth.RegisterUser(
            email,
            password,
            success =>
            {
                SetFeedback("✅ Registered! Check your email for the confirmation code.", Color.green);
                Debug.Log($"Register success:\n{success}");

                // Switch to confirmation panel ONLY after successful registration
                mainLoginPanel.SetActive(false);
                confirmationPanel.SetActive(true);
                confirmationCodeInput.gameObject.SetActive(true);
                confirmButton.gameObject.SetActive(true);
            },
            error =>
            {
                // If registration fails, stay on main panel and show error
                Debug.LogError($"Register failed:\n{error}");
                SetFeedback(ParseErrorMessage(error), Color.red);
            }));
    }

    void OnConfirmClicked()
    {
        string email = emailInput.text.Trim(); // Get from user input field
        string code = confirmationCodeInput.text.Trim().Replace(" ", "").Replace("-", ""); // Cleaned code input

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
        {
            SetFeedback("⚠️ Please enter your email and confirmation code.", Color.yellow);
            return;
        }

        Debug.Log($"[CONFIRM] Email: {email} | Code: {code}");

        SetFeedback("🔒 Confirming account...", Color.white);

        StartCoroutine(CognitoAuth.ConfirmUser(
            email,
            code,
            success =>
            {
                Debug.Log("Confirmation successful:\n" + success);
                SetFeedback("🎉 Account confirmed! You can now log in.", Color.green);

                // Show login panel again
                mainLoginPanel.SetActive(true);
                confirmationPanel.SetActive(false);
                confirmationCodeInput.gameObject.SetActive(false);
                confirmButton.gameObject.SetActive(false);
            },
            error =>
            {
                Debug.LogError("Confirmation failed:\n" + error);
                SetFeedback(ParseErrorMessage(error), Color.red);
            }));
    }

    private void ShowConfirmationPanel()
    {
        mainLoginPanel.SetActive(false);
        confirmationPanel.SetActive(true);
        confirmationCodeInput.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);
        confirmationCodeInput.text = ""; // Clear any previous confirmation code
    }

    private void SetFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
    }

    private string ParseErrorMessage(string json)
    {
        if (json.Contains("UsernameExistsException"))
            return "⚠️ This email is already registered.";
        if (json.Contains("InvalidPasswordException"))
            return "⚠️ Password too weak (min 6 characters, mixed case).";
        if (json.Contains("UserNotFoundException"))
            return "⚠️ No user with that email.";
        if (json.Contains("NotAuthorizedException"))
            return "⚠️ Incorrect email or password.";
        if (json.Contains("CodeMismatchException"))
            return "⚠️ Incorrect confirmation code.";
        if (json.Contains("UserNotConfirmedException"))
            return "⚠️ Please confirm your email first.";

        return "❌ Unexpected error occurred.";
    }
}
// This script manages the login, registration, and confirmation process for a user using AWS Cognito.
// It provides feedback to the user based on the success or failure of each operation.