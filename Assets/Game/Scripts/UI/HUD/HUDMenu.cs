using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class HUDMenu : Menu
{
    [Header("Damage Alert")]
    [Range(1, 100)][SerializeField] int maxAlert = 50;
    [SerializeField] float damageAlertTime = 1.0f;
    public VisualEffect highlightEffect;

    // Player
    VisualElement healthDamageVE;
    ProgressBar healthProgressBar;
    ProgressBar specialAttackBar;

    // Pickups
    [Header("Item Pickup")]
    [SerializeField] private VisualTreeAsset itemPickupListItem;
    [SerializeField] private float itemPopupLifetime = 5.0f;
    [SerializeField] private Color weaponColor = Color.red;
    [SerializeField] private Color spellColor = Color.magenta;
    [SerializeField] private Color materialColor = Color.blue;

    private VisualElement itemPickupContainer;

    private struct ListItem
    {
        public VisualElement element;
        public Label quantityLabel;
        public float lifetime;
    }
    private Dictionary<ItemSO, ListItem> itemListMap = new Dictionary<ItemSO, ListItem>();

    // Weapons
    VisualElement weaponIcon;

    // Spells
    private const string spellDisabledStyle = "spell-glyph-disabled";
    private const string spellActiveStyle = "spell-glyph-active";

    // Quests
    VisualElement questNameVE;
    Label questNameLabel;
    Label questDescriptionLabel;

    // Navigation
    [Header("Navigation")]
    [SerializeField] float compassRotationOffset = 180.0f;
    [Tooltip("This value must be same as the base rotation of main camera")]
    [SerializeField] float cameraBaseRotation = -45.0f;
    Vector3 navTarget = Vector3.zero;  // in world space
    bool navigate = false;
    VisualElement compassContainer;
    VisualElement compassNeedle;

    // Attack combo
    [Header("Attack Combo")]
    VisualElement attackComboVE;
    Label comboCountLabel;
    VisualElement splAttackPopup;
    VisualElement splAttackNotification;

    bool isComboHUDUp = false;
    bool redAlertUp = false;

    // Cached as they're getting called in OnDestroy
    GameManager gameManager;
    QuestManager questManager;

    void Start()
    {
        gameManager = GameManager.Instance;
        questManager = QuestManager.Instance;

        // Player
        healthDamageVE = root.Q<VisualElement>("Red_Alert");
        VisualElement background = root.Q<VisualElement>("Background");
        VisualElement leftBg = background.Q<VisualElement>("Left");
        VisualElement leftStatusArea = leftBg.Q<VisualElement>("StatusArea");
        VisualElement statusBars = leftStatusArea.Q<VisualElement>("Bars");

        // Health
        VisualElement healthVE = statusBars.Q<VisualElement>("Health");
        healthProgressBar = healthVE.Q<ProgressBar>("HealthBar");
        gameManager.OnPlayerHealthChange += UpdateHealthBar;
        UpdateHealthBar(gameManager.PlayerHealth);

        // Special Attack
        VisualElement splAttackVE = statusBars.Q<VisualElement>("SpecialAttack");
        specialAttackBar = splAttackVE.Q<ProgressBar>("SplAttackBar");
        gameManager.OnPlayerSpecialAttackChange += UpdateSpecialAttackBar;
        splAttackPopup = root.Q<VisualElement>("SplAttackPopup");
        splAttackPopup.style.opacity = 0.0f;
        splAttackNotification = statusBars.Q<VisualElement>("SplAttackNotification");
        splAttackNotification.style.opacity = 0.0f;

        // Pickups
        InventoryManager.Instance.OnItemAdded += OnItemPickup;
        itemPickupContainer = root.Q<VisualElement>("ItemPickupListContainer");

        // Weapons
        weaponIcon = root.Q<VisualElement>("WeaponIcon");

        // Quests
        VisualElement questContainer = root.Q<VisualElement>("QuestContainer");
        questNameVE = questContainer.Q<VisualElement>("Header");
        questNameLabel = questNameVE.Q<Label>("Quest-Name");
        questDescriptionLabel = questContainer.Q<Label>("Content");
        questManager.OnActiveQuestChange += UpdateActiveQuest;
        
        questNameVE.style.display = DisplayStyle.None;
        questDescriptionLabel.style.display = DisplayStyle.None;

        // Navigation
        VisualElement navigation = leftBg.Q<VisualElement>("NavigationContainer");
        compassContainer = navigation.Q<VisualElement>("CompassContainer");
        compassNeedle = navigation.Q<VisualElement>("CompassNeedle");
        navigate = false;

        // Combo
        attackComboVE = root.Q<VisualElement>("ComboContainer");
        comboCountLabel = attackComboVE.Q<Label>("ComboLabel");
        isComboHUDUp = false;
        attackComboVE.style.opacity = 0;

        // TODO: Hardcode effect onto main camera
        CameraManager.Instance.MainCameraChanged += (Camera c) =>
        {
            highlightEffect = c.GetComponent<VisualEffect>();
        };
    }

    private void FixedUpdate()
    {
        UpdateCompass();
    }

    public void HighlightCompass(bool showTrail = true)
    {
        HighlightHUDElement(compassContainer, showTrail);
    }

    private void HighlightHUDElement(VisualElement ve, bool showTrail = true)
    {
        if (highlightEffect == null) return;

        // Get the center of element and UI size
        Vector2 pos = ve.worldBound.center;
        Vector2 parentSize = root.contentRect.size;

        // Calculate the position as a value between 0 and 1
        Vector3 target = (pos / parentSize);
        target.y = 1 - target.y;
        target.z = 1;

        // Play effect
        highlightEffect.SetBool("showTrail", showTrail);
        highlightEffect.SetVector3("endPos", target);
        highlightEffect.Play();
    }

    // ------------------------------ Health ------------------------------

    void UpdateHealthBar(int health)
    {
        // Damage red alert UI
        if (!redAlertUp)
        {
            StartCoroutine(InitiateRedAlert());
        }

        if (healthProgressBar != null)
        {
            healthProgressBar.value = (float)health / GameManager.Instance.PlayerMaxHealth;
        }
    }

    IEnumerator InitiateRedAlert()
    {
        redAlertUp = true;
        // Set red alert to maxAlert. Reduce it to 0 over time
        int opacity = maxAlert;
        healthDamageVE.style.opacity = opacity;
        float timeDelta = damageAlertTime / maxAlert;

        while (opacity > 0)
        {
            --opacity;
            healthDamageVE.style.opacity = opacity;

            yield return new WaitForSecondsRealtime(timeDelta);
        }
        redAlertUp = false;
    }

    // ------------------------------ Pickup ------------------------------

    private ListItem CreatePickupListItem(ItemSO itemSO)
    {
        VisualElement listElement = itemPickupListItem.CloneTree();
        VisualElement pickupListItemElement = listElement.Q<VisualElement>(name: "PickupListItem");
        VisualElement itemImage = listElement.Q<VisualElement>(name: "Image");  ;
        Label itemType = listElement.Q<Label>(name: "ItemType");
        Label itemName = listElement.Q<Label>(name: "ItemName");
        Label itemQuantity = listElement.Q<Label>(name: "ItemQuantity");
        pickupListItemElement.AddToClassList("list-item-inactive");
        itemImage.style.backgroundImage = itemSO.icon.texture;
        itemType.text = itemSO.itemType.ToString();
        itemName.text = itemSO.itemName.ToString();
        itemQuantity.text = 1.ToString(); // Initial quantity is one

        Color tintColor = Color.black;
        switch (itemSO.itemType)
        {
            case ItemType.Weapon:
                tintColor = weaponColor;
                break;
            case ItemType.Spell:
                tintColor = spellColor;
                break;
            case ItemType.Material:
                tintColor = materialColor;
                break;
        }
        pickupListItemElement.style.unityBackgroundImageTintColor = tintColor;

        ListItem lstItem = new ListItem
        {
            element = listElement,
            quantityLabel = itemQuantity,
            lifetime = itemPopupLifetime
        };

        return lstItem;
    }

    private void OnItemPickup(ItemSO itemSO)
    {
        if (itemListMap.ContainsKey(itemSO))
        {
            ListItem lstItem = itemListMap[itemSO];
            int.TryParse(lstItem.quantityLabel.text, out int value);
            lstItem.quantityLabel.text = (value+1).ToString();
            lstItem.lifetime = itemPopupLifetime; // Reset Lifetime
            itemListMap[itemSO] = lstItem;
            return;
        }
        else
        {
            itemListMap[itemSO] = CreatePickupListItem(itemSO);
            VisualElement element = itemListMap[itemSO].element;
            itemPickupContainer.Add(element);
            StartCoroutine(TrackListItemLifetime(itemSO));
        }
    }

    private IEnumerator TrackListItemLifetime(ItemSO itemSO)
    {
        ListItem lstItem = itemListMap[itemSO];

        yield return new WaitForEndOfFrame();
        lstItem.element.Q<VisualElement>(name: "PickupListItem").RemoveFromClassList("list-item-inactive");

        while (lstItem.lifetime > 0)
        {
            lstItem = itemListMap[itemSO];
            lstItem.lifetime = lstItem.lifetime - Time.unscaledDeltaTime;
            itemListMap[itemSO] = lstItem;
            yield return null;
        }

        lstItem.element.Q<VisualElement>(name: "PickupListItem").AddToClassList("list-item-inactive");
        yield return new WaitForSecondsRealtime(0.3f);

        itemPickupContainer.Remove(itemListMap[itemSO].element);
        itemListMap.Remove(itemSO);
    }

    // ------------------------------ Special Attack ------------------------------

    void UpdateSpecialAttackBar(float value)
    {
        if (specialAttackBar != null)
        {
            specialAttackBar.value = value;
        }

        if (value == gameManager.PlayerMaxSpecialAttack)
        {
            StartCoroutine(SpecialAttackReadyPopup(2));
            StartCoroutine(SpecialAttackReadyNotification(true));
        }
        else
        {
            StartCoroutine(SpecialAttackReadyNotification(false));
        }
    }

    IEnumerator SpecialAttackReadyNotification(bool ready = true)
    {
        // Don't run remove aniamtion if already removed
        if (!ready && splAttackNotification.style.opacity == 0) yield break;

        StartCoroutine(GameManager.RunEasedLerp(
            ready ? 0 : 1,
            ready ? 1 : 0,
            0.3f,
            Easing.EaseInCubic,
            val =>
            {
                splAttackNotification.style.opacity = val;
            },
            true
        ));

        yield return GameManager.RunEasedLerp(
            1,
            1.5f,
            0.5f,
            Easing.EaseOutCubic,
            val =>
            {
                splAttackNotification.style.scale = new Scale(Vector2.one * val);
            },
            true
        );

        yield return GameManager.RunEasedLerp(
            1.5f,
            1,
            0.5f,
            Easing.EaseOutCubic,
            val =>
            {
                splAttackNotification.style.scale = new Scale(Vector2.one * val);
            },
            true
        );
    }

    IEnumerator SpecialAttackReadyPopup(float duration = 1.0f)
    {
        yield return GameManager.RunEasedLerp(
            0,
            1,
            duration,
            Easing.EaseInQuart,
            val =>
            {
                splAttackPopup.style.opacity = Mathf.Clamp01(val*500f);
                splAttackPopup.style.translate = new Translate(0, (1 - val) * 50, 0);
            }
        );

        yield return GameManager.RunEasedLerp(
            1,
            0,
            1.0f,
            Easing.EaseOut,
            val =>
            {
                splAttackPopup.style.opacity = val;
                splAttackPopup.style.translate = new Translate(0, -(1-val) * 50, 0);
            }
        );
    }

    // ------------------------------ Weapons ------------------------------

    public void UpdateWeaponIcon(Sprite icon)
    {
        if (weaponIcon != null && icon != null)
        {
            weaponIcon.style.backgroundImage = icon.texture;
        }
    }

    // ------------------------------ Spells -------------------------------

    public void UpdateSpellCooldown(int spellNumber, int value)
    {
        VisualElement skillElement = root.Q<VisualElement>($"Spell{spellNumber}");
        Label skillTimer = skillElement.Q<Label>("Timer");

        if (value > 0) {
            skillElement.AddToClassList(spellDisabledStyle);
        } else
        {
            skillElement.RemoveFromClassList(spellDisabledStyle);
        }

        skillTimer.text = value.ToString();
    }

    public void SetSpellActive(int spellNumber)
    {
        // Set active style to selected spell and remove from rest
        var skillElements = root.Query<VisualElement>().Where(
            ve => ve.name != null && ve.name.StartsWith("Spell")
            ).ToList();
        foreach (var el in skillElements)
        {
            if (el.name.Contains(spellNumber.ToString()))
            {
                el.AddToClassList(spellActiveStyle);
            } else
            {
                el.RemoveFromClassList(spellActiveStyle);
            }
        }
    }

    public void SetSpellIcon(int spellNumber, Sprite icon)
    {
        VisualElement skillElement = root.Q<VisualElement>($"Spell{spellNumber}");
        skillElement.style.backgroundImage = icon?.texture;
    }

    // ------------------------------ Quests -------------------------------

    void UpdateActiveQuest(string questName, string questDescription)
    {
        if (questName == "")
        {
            questNameVE.style.display = DisplayStyle.None;
            questDescriptionLabel.style.display = DisplayStyle.None;
            return;
        }
        questNameVE.style.display = DisplayStyle.Flex;
        questDescriptionLabel.style.display = DisplayStyle.Flex;
        questNameLabel.text = questName;
        questDescriptionLabel.text = questDescription;
    }

    // ------------------------------ Navigation -------------------------------

    void UpdateCompass()
    {
        Transform cameraT = CameraManager.ActiveCamera.transform;
        // Compass rotation
        compassContainer.transform.rotation = Quaternion.Euler(0, 0, cameraBaseRotation - cameraT.eulerAngles.y);

        if (GameManager.Instance.PlayerRef == null || !navigate)
        {
            return;
        }
        Vector3 playerPosition = GameManager.Instance.PlayerRef.transform.position;
        Vector3 direction = (navTarget - playerPosition);
        direction.y = 0;
        direction.Normalize();
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
        // Needle rotation
        compassNeedle.transform.rotation = Quaternion.Euler(0, 0, cameraBaseRotation - angle + compassRotationOffset);
    }

    public void StartNavigation(Vector3 target)
    {
        navigate = true;
        navTarget = target;
    }

    public void StopNavigation()
    {
        navigate = false;
        compassNeedle.transform.rotation = Quaternion.Euler(0, 0, 0);  // North
        navTarget = Vector3.zero;
    }

    // ------------------------------ Attack Combo -------------------------------

    public void ShowCombo()
    {
        isComboHUDUp = true;
        attackComboVE.style.opacity = 1;
    }

    public void SetComboOpacity(float opacity)
    {
        isComboHUDUp = (opacity > 0);
        attackComboVE.style.opacity = opacity;
    }

    public void HideCombo()
    {
        isComboHUDUp = false;
        attackComboVE.style.opacity = 0;
    }

    public void UpdateComboCount(int comboCounter)
    {
        if (!isComboHUDUp)
            ShowCombo();

        comboCountLabel.text = comboCounter.ToString();
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnPlayerHealthChange -= UpdateHealthBar;
        }
        if (questManager != null)
        {
            questManager.OnActiveQuestChange -= UpdateActiveQuest;
        }
    }
}
