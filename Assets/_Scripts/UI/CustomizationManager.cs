using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Data class for bird customization items
/// </summary>
[System.Serializable]
public class BirdSkin
{
    public string skinName;
    public string description;
    public Sprite previewImage;
    public GameObject birdPrefab; // The actual bird model with this skin
    public int cost; // Cost to unlock (0 = free/default)
    public bool isUnlocked;
}

/// <summary>
/// Manages bird customization (skins, colors, accessories)
/// </summary>
public class CustomizationManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject customizationPanel;
    [SerializeField] private Button backButton;
    
    [Header("Bird Preview")]
    [SerializeField] private Image birdPreviewImage;
    [SerializeField] private Transform birdPreview3D; // Optional 3D model preview
    [SerializeField] private TextMeshProUGUI selectedBirdName;
    [SerializeField] private TextMeshProUGUI selectedBirdDescription;
    
    [Header("Skin Selection")]
    [SerializeField] private Transform skinScrollContent;
    [SerializeField] private GameObject skinButtonPrefab;
    [SerializeField] private Button selectButton;
    [SerializeField] private TextMeshProUGUI selectButtonText;
    
    [Header("Bird Skins")]
    [SerializeField] private List<BirdSkin> availableSkins = new List<BirdSkin>();
    
    [Header("Currency Display")]
    [SerializeField] private TextMeshProUGUI coinsText;
    
    private BirdSkin currentlySelectedSkin;
    private BirdSkin equippedSkin;
    private List<GameObject> skinButtons = new List<GameObject>();
    private MainMenuManager mainMenuManager;
    
    private const string EQUIPPED_SKIN_KEY = "EquippedSkin";
    private const string COINS_KEY = "PlayerCoins";
    
    private void Awake()
    {
        mainMenuManager = FindFirstObjectByType<MainMenuManager>();
        
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
        
        if (selectButton != null)
            selectButton.onClick.AddListener(OnSelectButtonClicked);
    }
    
    private void Start()
    {
        LoadCustomizationData();
        PopulateSkinList();
        UpdateCurrencyDisplay();
        
        // Select equipped skin by default
        if (equippedSkin != null)
        {
            SelectSkin(equippedSkin);
        }
        else if (availableSkins.Count > 0)
        {
            SelectSkin(availableSkins[0]);
        }
    }
    
    #region Initialization
    
    private void LoadCustomizationData()
    {
        // Load equipped skin
        string equippedSkinName = PlayerPrefs.GetString(EQUIPPED_SKIN_KEY, "");
        
        // Load unlocked status for each skin
        foreach (var skin in availableSkins)
        {
            string unlockKey = "Skin_" + skin.skinName + "_Unlocked";
            
            // Default skin (cost 0) is always unlocked
            if (skin.cost == 0)
            {
                skin.isUnlocked = true;
            }
            else
            {
                skin.isUnlocked = PlayerPrefs.GetInt(unlockKey, 0) == 1;
            }
            
            // Set equipped skin
            if (skin.skinName == equippedSkinName)
            {
                equippedSkin = skin;
            }
        }
        
        // If no equipped skin, equip the first unlocked one
        if (equippedSkin == null)
        {
            foreach (var skin in availableSkins)
            {
                if (skin.isUnlocked)
                {
                    equippedSkin = skin;
                    PlayerPrefs.SetString(EQUIPPED_SKIN_KEY, skin.skinName);
                    break;
                }
            }
        }
    }
    
    private void PopulateSkinList()
    {
        if (skinScrollContent == null || skinButtonPrefab == null)
            return;
        
        // Clear existing buttons
        foreach (var button in skinButtons)
        {
            Destroy(button);
        }
        skinButtons.Clear();
        
        // Create button for each skin
        foreach (var skin in availableSkins)
        {
            GameObject buttonObj = Instantiate(skinButtonPrefab, skinScrollContent);
            skinButtons.Add(buttonObj);
            
            // Setup button
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => SelectSkin(skin));
            }
            
            // Setup visuals
            Image previewImage = buttonObj.transform.Find("PreviewImage")?.GetComponent<Image>();
            if (previewImage != null && skin.previewImage != null)
            {
                previewImage.sprite = skin.previewImage;
            }
            
            TextMeshProUGUI nameText = buttonObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = skin.skinName;
            }
            
            // Show lock icon if not unlocked
            GameObject lockIcon = buttonObj.transform.Find("LockIcon")?.gameObject;
            if (lockIcon != null)
            {
                lockIcon.SetActive(!skin.isUnlocked);
            }
            
            // Show cost if not unlocked
            TextMeshProUGUI costText = buttonObj.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
            if (costText != null)
            {
                if (skin.isUnlocked)
                {
                    costText.gameObject.SetActive(false);
                }
                else
                {
                    costText.gameObject.SetActive(true);
                    costText.text = skin.cost.ToString();
                }
            }
            
            // Show equipped indicator
            GameObject equippedIndicator = buttonObj.transform.Find("EquippedIndicator")?.gameObject;
            if (equippedIndicator != null)
            {
                equippedIndicator.SetActive(skin == equippedSkin);
            }
        }
    }
    
    #endregion
    
    #region Skin Selection
    
    private void SelectSkin(BirdSkin skin)
    {
        currentlySelectedSkin = skin;
        
        // Update preview
        if (birdPreviewImage != null && skin.previewImage != null)
        {
            birdPreviewImage.sprite = skin.previewImage;
        }
        
        if (selectedBirdName != null)
        {
            selectedBirdName.text = skin.skinName;
        }
        
        if (selectedBirdDescription != null)
        {
            selectedBirdDescription.text = skin.description;
        }
        
        // Update select button
        UpdateSelectButton();
        
        Debug.Log("Selected skin: " + skin.skinName);
    }
    
    private void UpdateSelectButton()
    {
        if (selectButton == null || currentlySelectedSkin == null)
            return;
        
        if (!currentlySelectedSkin.isUnlocked)
        {
            // Skin is locked - show unlock button
            selectButton.interactable = true;
            if (selectButtonText != null)
            {
                int coins = GetPlayerCoins();
                if (coins >= currentlySelectedSkin.cost)
                {
                    selectButtonText.text = "UNLOCK (" + currentlySelectedSkin.cost + " coins)";
                }
                else
                {
                    selectButtonText.text = "INSUFFICIENT COINS";
                    selectButton.interactable = false;
                }
            }
        }
        else if (currentlySelectedSkin == equippedSkin)
        {
            // Already equipped
            selectButton.interactable = false;
            if (selectButtonText != null)
            {
                selectButtonText.text = "EQUIPPED";
            }
        }
        else
        {
            // Can equip
            selectButton.interactable = true;
            if (selectButtonText != null)
            {
                selectButtonText.text = "SELECT";
            }
        }
    }
    
    #endregion
    
    #region Button Callbacks
    
    private void OnSelectButtonClicked()
    {
        if (currentlySelectedSkin == null)
            return;
        
        if (!currentlySelectedSkin.isUnlocked)
        {
            // Unlock the skin
            if (TryUnlockSkin(currentlySelectedSkin))
            {
                Debug.Log("Unlocked skin: " + currentlySelectedSkin.skinName);
                PopulateSkinList(); // Refresh list
                UpdateSelectButton();
            }
        }
        else
        {
            // Equip the skin
            EquipSkin(currentlySelectedSkin);
        }
    }
    
    private void OnBackButtonClicked()
    {
        if (mainMenuManager != null)
        {
            mainMenuManager.ReturnToMainMenu();
        }
    }
    
    #endregion
    
    #region Skin Management
    
    private bool TryUnlockSkin(BirdSkin skin)
    {
        int coins = GetPlayerCoins();
        
        if (coins >= skin.cost)
        {
            // Deduct coins
            SetPlayerCoins(coins - skin.cost);
            
            // Unlock skin
            skin.isUnlocked = true;
            string unlockKey = "Skin_" + skin.skinName + "_Unlocked";
            PlayerPrefs.SetInt(unlockKey, 1);
            PlayerPrefs.Save();
            
            UpdateCurrencyDisplay();
            return true;
        }
        
        Debug.LogWarning("Not enough coins to unlock " + skin.skinName);
        return false;
    }
    
    private void EquipSkin(BirdSkin skin)
    {
        if (!skin.isUnlocked)
            return;
        
        equippedSkin = skin;
        PlayerPrefs.SetString(EQUIPPED_SKIN_KEY, skin.skinName);
        PlayerPrefs.Save();
        
        Debug.Log("Equipped skin: " + skin.skinName);
        
        // Refresh UI
        PopulateSkinList();
        UpdateSelectButton();
    }
    
    /// <summary>
    /// Get the currently equipped skin name
    /// </summary>
    public static string GetEquippedSkinName()
    {
        return PlayerPrefs.GetString(EQUIPPED_SKIN_KEY, "");
    }
    
    #endregion
    
    #region Currency Management
    
    private int GetPlayerCoins()
    {
        return PlayerPrefs.GetInt(COINS_KEY, 0);
    }
    
    private void SetPlayerCoins(int amount)
    {
        PlayerPrefs.SetInt(COINS_KEY, amount);
        PlayerPrefs.Save();
    }
    
    private void UpdateCurrencyDisplay()
    {
        if (coinsText != null)
        {
            coinsText.text = GetPlayerCoins().ToString();
        }
    }
    
    /// <summary>
    /// Add coins to player (called from gameplay)
    /// </summary>
    public static void AddCoins(int amount)
    {
        int current = PlayerPrefs.GetInt(COINS_KEY, 0);
        PlayerPrefs.SetInt(COINS_KEY, current + amount);
        PlayerPrefs.Save();
    }
    
    #endregion
    
    #region Debug Methods
    
    /// <summary>
    /// Add coins for testing (can be called from inspector button)
    /// </summary>
    public void AddTestCoins(int amount)
    {
        AddCoins(amount);
        UpdateCurrencyDisplay();
        UpdateSelectButton();
        Debug.Log("Added " + amount + " test coins");
    }
    
    /// <summary>
    /// Unlock all skins for testing
    /// </summary>
    public void UnlockAllSkinsForTesting()
    {
        foreach (var skin in availableSkins)
        {
            skin.isUnlocked = true;
            string unlockKey = "Skin_" + skin.skinName + "_Unlocked";
            PlayerPrefs.SetInt(unlockKey, 1);
        }
        PlayerPrefs.Save();
        PopulateSkinList();
        Debug.Log("Unlocked all skins for testing");
    }
    
    #endregion
}
