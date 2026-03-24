using System.Collections.Generic;
using Liberator.Attributes;
using Liberator.Combat;
using Liberator.Combat.Controllers;
using Liberator.Combat.Skills;
using Liberator.Combat.UI;
using Liberator.Inventory;
using Liberator.TurnManager;
using Liberator.Utils;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Editor tool to auto-generate all prefabs and build the CombatScene.
/// Access via: Liberator menu in the top menu bar.
/// </summary>
public class CombatSceneBuilder : Editor
{
    // ============================================================
    // MENU ITEMS
    // ============================================================

    [MenuItem("Liberator/1. Generate All Prefabs")]
    public static void GenerateAllPrefabs()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/Enemies");
        EnsureFolder("Assets/Prefabs/Player");
        EnsureFolder("Assets/Prefabs/UI");
        EnsureFolder("Assets/Prefabs/VFX");

        // Load shared assets
        var progression = AssetDatabase.LoadAssetAtPath<Progression>("Assets/ScriptableObjects/Progression.asset");
        if (progression == null)
        {
            Debug.LogError("[CombatSceneBuilder] Progression.asset not found! Run 'Liberator → Generate All Game Data' first.");
            return;
        }

        // Generate VFX placeholder prefabs (needed by HealthManager, ExperienceModule)
        GenerateVFXPrefabs();

        // Generate skill card prefab (needed by SkillHolderCreate)
        GenerateSkillCardPrefab();

        // Generate enemy prefabs
        GenerateEnemyPrefab("Grunt", CharacterClass.Grunt, PreferLine.First, 2,
            new[] { "BasicAttack" }, progression);
        GenerateEnemyPrefab("Knight", CharacterClass.Knight, PreferLine.First, 4,
            new[] { "PowerStrike" }, progression);
        GenerateEnemyPrefab("Archer", CharacterClass.Archer, PreferLine.Second, 3,
            new[] { "ArrowShot", "DoubleShot" }, progression);
        GenerateEnemyPrefab("Hunter", CharacterClass.Hunter, PreferLine.Second, 3,
            new[] { "ArrowShot", "Volley" }, progression);
        GenerateEnemyPrefab("Healer", CharacterClass.Healer, PreferLine.Third, 3,
            new[] { "Heal", "BasicAttack" }, progression);
        GenerateEnemyPrefab("Assassin", CharacterClass.Assassin, PreferLine.First, 4,
            new[] { "PoisonStrike" }, progression);
        GenerateEnemyPrefab("SwordMaster", CharacterClass.SwordMaster, PreferLine.First, 5,
            new[] { "Cleave", "PowerStrike" }, progression);
        GenerateEnemyPrefab("Necromancer", CharacterClass.Necromancer, PreferLine.Third, 4,
            new[] { "ColumnStrike", "PoisonStrike" }, progression);
        GenerateEnemyPrefab("Boss", CharacterClass.Boss, PreferLine.Boss, 8,
            new[] { "PowerStrike", "Cleave", "StunBash" }, progression);

        // Generate player prefab
        GeneratePlayerPrefab(progression);

        // Wire enemy prefabs into difficulty lists
        WireDifficultyLists();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CombatSceneBuilder] All prefabs generated! Player + 9 enemies + VFX + SkillCard.");
    }

    [MenuItem("Liberator/2. Build CombatScene")]
    public static void BuildCombatScene()
    {
        // Open or create the CombatScene
        string scenePath = "Assets/Scenes/CombatScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Clear existing objects (except camera and lighting)
        var rootObjects = scene.GetRootGameObjects();
        foreach (var obj in rootObjects)
        {
            if (obj.name == "Main Camera" || obj.name == "Directional Light" ||
                obj.name == "Global Volume")
                continue;
            DestroyImmediate(obj);
        }

        // ---- COMBAT INITIALIZER (sets difficulty + enemy count for Spawner) ----
        var initObj = new GameObject("CombatInitializer");
        initObj.AddComponent<CombatInitializer>();

        // Load prefabs
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Player.prefab");
        if (playerPrefab == null)
        {
            Debug.LogError("[CombatSceneBuilder] Player prefab not found! Run 'Liberator → 1. Generate All Prefabs' first.");
            return;
        }

        // ---- PLAYER ----
        var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        player.name = "Player";
        player.transform.position = new Vector3(-4, 0, 0);

        // ---- SPAWNER (with 6 position holders) ----
        var spawnerObj = new GameObject("Spawner");
        spawnerObj.tag = "Spawner";
        var spawnerComp = spawnerObj.AddComponent<Spawner>();

        // Create 6 spawn positions in a 2x3 grid
        var spawnerPositions = new GameObject[6];
        Vector3[] positions = new Vector3[]
        {
            new Vector3(2, 1.5f, 0),   // Pos0: Front-Top (First line)
            new Vector3(2, -1.5f, 0),  // Pos1: Front-Bottom (First line)
            new Vector3(4, 1.5f, 0),   // Pos2: Mid-Top (Second line)
            new Vector3(4, -1.5f, 0),  // Pos3: Mid-Bottom (Second line)
            new Vector3(6, 1.5f, 0),   // Pos4: Back-Top (Third line)
            new Vector3(6, -1.5f, 0),  // Pos5: Back-Bottom (Third line)
        };

        for (int i = 0; i < 6; i++)
        {
            var pos = new GameObject($"Pos{i}");
            pos.transform.parent = spawnerObj.transform;
            pos.transform.position = positions[i];
            pos.tag = "Placement";
            pos.AddComponent<EnemyHolder>();
            // Add a BoxCollider for raycast detection (SkillHandler uses Physics.Raycast)
            var col = pos.AddComponent<BoxCollider>();
            col.size = new Vector3(1.5f, 2f, 1f);
            spawnerPositions[i] = pos;
        }

        // Wire Spawner serialized fields via SerializedObject
        var spawnerSO = new SerializedObject(spawnerComp);
        var spawnersArrayProp = spawnerSO.FindProperty("_spawners");
        spawnersArrayProp.arraySize = 6;
        for (int i = 0; i < 6; i++)
            spawnersArrayProp.GetArrayElementAtIndex(i).objectReferenceValue = spawnerPositions[i];

        // Wire difficulty lists
        var diffProp = spawnerSO.FindProperty("_difficultyLists");
        var easy = AssetDatabase.LoadAssetAtPath<DifficultyList>("Assets/ScriptableObjects/Difficulty/Easy.asset");
        var normal = AssetDatabase.LoadAssetAtPath<DifficultyList>("Assets/ScriptableObjects/Difficulty/Normal.asset");
        var hard = AssetDatabase.LoadAssetAtPath<DifficultyList>("Assets/ScriptableObjects/Difficulty/Hard.asset");
        diffProp.arraySize = 3;
        diffProp.GetArrayElementAtIndex(0).objectReferenceValue = easy;
        diffProp.GetArrayElementAtIndex(1).objectReferenceValue = normal;
        diffProp.GetArrayElementAtIndex(2).objectReferenceValue = hard;
        spawnerSO.ApplyModifiedProperties();

        // ---- TURN MANAGER (Tag: Finish) ----
        var turnManagerObj = new GameObject("TurnManager");
        turnManagerObj.tag = "Finish";
        turnManagerObj.AddComponent<TurnModule>();
        var tmComp = turnManagerObj.AddComponent<Liberator.TurnManager.TurnManager>();
        // Add a Button component (TurnManager uses it for end-turn interactability)
        var tmImg = turnManagerObj.AddComponent<Image>();
        tmImg.color = new Color(0.15f, 0.4f, 0.6f, 1f);
        var tmButton = turnManagerObj.AddComponent<Button>();
        // Wire player controller reference
        var tmSO = new SerializedObject(tmComp);
        tmSO.FindProperty("_playerController").objectReferenceValue = player.GetComponent<PlayerController>();
        tmSO.ApplyModifiedProperties();
        // Wire End Turn button onClick -> TurnModule.EndPlayerTurn
        var turnModule = turnManagerObj.GetComponent<TurnModule>();
        UnityEventTools.AddVoidPersistentListener(tmButton.onClick,
            new UnityEngine.Events.UnityAction(turnModule.EndPlayerTurn));

        // ---- DEBUFF MODULE (Tag: Debuff) ----
        var debuffObj = new GameObject("DebuffModule");
        debuffObj.tag = "Debuff";
        debuffObj.AddComponent<DebuffModule>();

        // ---- CANVAS (Tag: Canvas) ----
        var canvasObj = new GameObject("Canvas");
        canvasObj.tag = "Canvas";
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Move TurnManager under Canvas (it needs Button + Image for UI)
        turnManagerObj.transform.SetParent(canvasObj.transform, false);
        var tmRect = turnManagerObj.GetComponent<RectTransform>();
        tmRect.anchorMin = new Vector2(1, 0);
        tmRect.anchorMax = new Vector2(1, 0);
        tmRect.pivot = new Vector2(1, 0);
        tmRect.anchoredPosition = new Vector2(-20, 20);
        tmRect.sizeDelta = new Vector2(150, 50);
        // Add text label
        var tmTextObj = new GameObject("TurnText");
        tmTextObj.transform.SetParent(turnManagerObj.transform, false);
        var tmText = tmTextObj.AddComponent<TextMeshProUGUI>();
        tmText.text = "End Turn";
        tmText.fontSize = 24;
        tmText.alignment = TextAlignmentOptions.Center;
        tmText.color = Color.white;
        var tmTextRect = tmTextObj.GetComponent<RectTransform>();
        tmTextRect.anchorMin = Vector2.zero;
        tmTextRect.anchorMax = Vector2.one;
        tmTextRect.sizeDelta = Vector2.zero;

        // Turn counter text
        var turnCounterObj = new GameObject("TurnCounter");
        turnCounterObj.transform.SetParent(canvasObj.transform, false);
        var turnCounterText = turnCounterObj.AddComponent<TextMeshProUGUI>();
        turnCounterText.text = "Turn: 1";
        turnCounterText.fontSize = 28;
        turnCounterText.alignment = TextAlignmentOptions.TopLeft;
        turnCounterText.color = Color.white;
        var tcRect = turnCounterObj.GetComponent<RectTransform>();
        tcRect.anchorMin = new Vector2(0, 1);
        tcRect.anchorMax = new Vector2(0, 1);
        tcRect.pivot = new Vector2(0, 1);
        tcRect.anchoredPosition = new Vector2(20, -20);
        tcRect.sizeDelta = new Vector2(200, 40);
        // Wire turn text
        tmSO = new SerializedObject(tmComp);
        tmSO.FindProperty("_turnText").objectReferenceValue = turnCounterText;
        tmSO.ApplyModifiedProperties();

        // ---- CARD HOLDERS (skill cards UI) ----
        var cardHolderMelee = CreateUIPanel(canvasObj.transform, "CardHolderMelee",
            new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0, 0),
            new Vector2(20, 10), new Vector2(500, 120));

        var cardHolderRanged = CreateUIPanel(canvasObj.transform, "CardHolderRanged",
            new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0, 0),
            new Vector2(20, 10), new Vector2(500, 120));
        cardHolderRanged.transform.position = new Vector3(50, -400, 0); // off-screen initially

        // Add horizontal layout groups for nice card arrangement
        var meleeLayout = cardHolderMelee.AddComponent<HorizontalLayoutGroup>();
        meleeLayout.spacing = 10;
        meleeLayout.childAlignment = TextAnchor.MiddleLeft;
        meleeLayout.childForceExpandWidth = false;
        meleeLayout.childForceExpandHeight = false;

        var rangedLayout = cardHolderRanged.AddComponent<HorizontalLayoutGroup>();
        rangedLayout.spacing = 10;
        rangedLayout.childAlignment = TextAnchor.MiddleLeft;
        rangedLayout.childForceExpandWidth = false;
        rangedLayout.childForceExpandHeight = false;

        // ---- SKILL HOLDER CREATE ----
        var skillCreateObj = new GameObject("SkillHolderCreate");
        skillCreateObj.transform.SetParent(canvasObj.transform, false);
        var skillCreate = skillCreateObj.AddComponent<SkillHolderCreate>();
        var skillCreateSO = new SerializedObject(skillCreate);
        skillCreateSO.FindProperty("_cardHolderMelee").objectReferenceValue = cardHolderMelee;
        skillCreateSO.FindProperty("_cardHolderRanged").objectReferenceValue = cardHolderRanged;
        var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/SkillCard.prefab");
        skillCreateSO.FindProperty("_cardPrefab").objectReferenceValue = cardPrefab;
        skillCreateSO.ApplyModifiedProperties();

        // ---- INFO TEXT ----
        var infoTextObj = new GameObject("InfoText");
        infoTextObj.transform.SetParent(canvasObj.transform, false);
        var infoTextComp = infoTextObj.AddComponent<InfoText>();
        var infoTMP = infoTextObj.AddComponent<TextMeshProUGUI>();
        infoTMP.fontSize = 24;
        infoTMP.alignment = TextAlignmentOptions.Center;
        infoTMP.color = Color.yellow;
        var infoRect = infoTextObj.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.5f, 1);
        infoRect.anchorMax = new Vector2(0.5f, 1);
        infoRect.pivot = new Vector2(0.5f, 1);
        infoRect.anchoredPosition = new Vector2(0, -10);
        infoRect.sizeDelta = new Vector2(400, 40);
        // Wire InfoText._infoText
        var infoTextSO = new SerializedObject(infoTextComp);
        infoTextSO.FindProperty("_infoText").objectReferenceValue = infoTMP;
        infoTextSO.ApplyModifiedProperties();

        // Wire player's _infoText reference
        var playerControllerSO = new SerializedObject(player.GetComponent<PlayerController>());
        playerControllerSO.FindProperty("_infoText").objectReferenceValue = infoTextComp;
        playerControllerSO.ApplyModifiedProperties();

        // ---- BATTLE LOG (Tag: BattleLog) ----
        var battleLogObj = new GameObject("BattleLog");
        battleLogObj.tag = "BattleLog";
        battleLogObj.transform.SetParent(canvasObj.transform, false);
        var battleLogComp = battleLogObj.AddComponent<BattleLog>();

        // Dark screen overlay for victory/defeat
        var darkScreen = new GameObject("DarkScreen");
        darkScreen.transform.SetParent(canvasObj.transform, false);
        var darkImg = darkScreen.AddComponent<Image>();
        darkImg.color = new Color(0, 0, 0, 0.8f);
        var darkRect = darkScreen.GetComponent<RectTransform>();
        darkRect.anchorMin = Vector2.zero;
        darkRect.anchorMax = Vector2.one;
        darkRect.sizeDelta = Vector2.zero;
        // Victory/defeat text
        var resultTextObj = new GameObject("ResultText");
        resultTextObj.transform.SetParent(darkScreen.transform, false);
        var resultText = resultTextObj.AddComponent<TextMeshProUGUI>();
        resultText.fontSize = 72;
        resultText.alignment = TextAlignmentOptions.Center;
        resultText.color = Color.white;
        resultText.text = "VICTORY";
        var resultRect = resultTextObj.GetComponent<RectTransform>();
        resultRect.anchorMin = Vector2.zero;
        resultRect.anchorMax = Vector2.one;
        resultRect.sizeDelta = Vector2.zero;

        // Wire BattleLog
        var battleLogSO = new SerializedObject(battleLogComp);
        battleLogSO.FindProperty("_battleLog").objectReferenceValue = battleLogObj;
        battleLogSO.FindProperty("_darkScreen").objectReferenceValue = darkScreen;
        battleLogSO.FindProperty("_playerController").objectReferenceValue = player.GetComponent<PlayerController>();
        battleLogSO.ApplyModifiedProperties();

        // ---- PLAYER INFO / DESCRIPTION (Tag: PlayerInfo) ----
        var playerInfoObj = new GameObject("PlayerInfo");
        playerInfoObj.tag = "PlayerInfo";
        playerInfoObj.transform.SetParent(canvasObj.transform, false);
        var descComp = playerInfoObj.AddComponent<Description>();

        // Description panel (child)
        var descPanel = CreateUIPanel(canvasObj.transform, "DescriptionPanel",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-20, 0), new Vector2(300, 400));
        var descPanelImg = descPanel.GetComponent<Image>();
        if (descPanelImg == null) descPanelImg = descPanel.AddComponent<Image>();
        descPanelImg.color = new Color(0, 0, 0, 0.85f);

        // Description text
        var descTextObj = new GameObject("DescText");
        descTextObj.transform.SetParent(descPanel.transform, false);
        var descTMP = descTextObj.AddComponent<TextMeshProUGUI>();
        descTMP.fontSize = 18;
        descTMP.alignment = TextAlignmentOptions.TopLeft;
        descTMP.color = Color.white;
        var descTextRect = descTextObj.GetComponent<RectTransform>();
        descTextRect.anchorMin = new Vector2(0.05f, 0.05f);
        descTextRect.anchorMax = new Vector2(0.95f, 0.95f);
        descTextRect.sizeDelta = Vector2.zero;

        // Wire Description
        var descSO = new SerializedObject(descComp);
        descSO.FindProperty("_playerDescription").objectReferenceValue = descPanel;
        descSO.ApplyModifiedProperties();

        // ---- WEAPON SWITCH BUTTON ----
        var switchBtnObj = new GameObject("WeaponSwitchButton");
        switchBtnObj.transform.SetParent(canvasObj.transform, false);
        var switchImg = switchBtnObj.AddComponent<Image>();
        switchImg.color = new Color(0.2f, 0.6f, 0.9f, 1f);
        var switchBtn = switchBtnObj.AddComponent<Button>();
        var switchRect = switchBtnObj.GetComponent<RectTransform>();
        switchRect.anchorMin = new Vector2(0.5f, 0);
        switchRect.anchorMax = new Vector2(0.5f, 0);
        switchRect.pivot = new Vector2(0.5f, 0);
        switchRect.anchoredPosition = new Vector2(0, 10);
        switchRect.sizeDelta = new Vector2(150, 40);
        // Switch button text
        var switchTextObj = new GameObject("Text");
        switchTextObj.transform.SetParent(switchBtnObj.transform, false);
        var switchTMP = switchTextObj.AddComponent<TextMeshProUGUI>();
        switchTMP.text = "Switch Weapon";
        switchTMP.fontSize = 18;
        switchTMP.alignment = TextAlignmentOptions.Center;
        switchTMP.color = Color.white;
        var switchTextRect = switchTextObj.GetComponent<RectTransform>();
        switchTextRect.anchorMin = Vector2.zero;
        switchTextRect.anchorMax = Vector2.one;
        switchTextRect.sizeDelta = Vector2.zero;
        // Wire Switch Weapon button onClick -> PlayerCombatManager.SwitchSkills + SkillHolderCreate.Switch
        var playerCombatMgr = player.GetComponent<PlayerCombatManager>();
        UnityEventTools.AddVoidPersistentListener(switchBtn.onClick,
            new UnityEngine.Events.UnityAction(playerCombatMgr.SwitchSkills));
        UnityEventTools.AddVoidPersistentListener(switchBtn.onClick,
            new UnityEngine.Events.UnityAction(skillCreate.Switch));

        // ---- EVENT SYSTEM (required for UI interaction) ----
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // ---- CAMERA SETUP ----
        var cam = Object.FindObjectOfType<Camera>();
        if (cam != null)
        {
            cam.transform.position = new Vector3(1, 0, -10);
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
        }

        // ---- BACKGROUND ----
        var bgObj = new GameObject("Background");
        var bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.color = new Color(0.08f, 0.12f, 0.18f);
        bgRenderer.sortingOrder = -10;
        bgObj.transform.position = new Vector3(1, 0, 1);

        // Mark scene dirty and save
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[CombatSceneBuilder] CombatScene fully built! All GameObjects, tags, and references wired.");
        Debug.Log("[CombatSceneBuilder] Press Play to test the combat system!");
    }

    [MenuItem("Liberator/3. Generate Everything (Prefabs + Scene)")]
    public static void GenerateEverything()
    {
        GenerateAllPrefabs();
        BuildCombatScene();
    }

    // ============================================================
    // ENEMY PREFAB GENERATOR
    // ============================================================

    private static void GenerateEnemyPrefab(string enemyName, CharacterClass charClass,
        PreferLine preferLine, int enemyValue, string[] skillNames, Progression progression)
    {
        string prefabPath = $"Assets/Prefabs/Enemies/{enemyName}.prefab";

        // Create root GameObject
        var enemyObj = new GameObject(enemyName);
        enemyObj.tag = "Enemy";

        // SpriteRenderer (placeholder — white square)
        var sr = enemyObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreatePlaceholderSprite(charClass);
        sr.color = GetEnemyColor(charClass);
        sr.sortingOrder = 1;

        // BoxCollider2D for click detection
        var col = enemyObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1, 1.5f);

        // Core components
        var baseAttr = enemyObj.AddComponent<BaseAttributes>();
        var healthMgr = enemyObj.AddComponent<HealthManager>();
        var manaMgr = enemyObj.AddComponent<ManaManager>();
        var deathMgr = enemyObj.AddComponent<DeathManager>();
        var combatMgr = enemyObj.AddComponent<EnemyCombatManager>();
        // EnemyController is auto-added by [RequireComponent] on EnemyCombatManager
        var enemyCtrl = enemyObj.GetComponent<EnemyController>();
        if (enemyCtrl == null)
            enemyCtrl = enemyObj.AddComponent<EnemyController>();

        // Child: "Skills" empty (needed by SkillHolderCreate for enemy skill storage)
        var skillsChild = new GameObject("Skills");
        skillsChild.transform.SetParent(enemyObj.transform, false);

        // Child: Highlight (visual indicator for active turn)
        var highlight = CreateHighlightChild(enemyObj.transform);

        // Child: ActionImage (UI image showing enemy intent)
        var actionHolder = new GameObject("ActionImage");
        actionHolder.transform.SetParent(enemyObj.transform, false);
        actionHolder.transform.localPosition = new Vector3(0, 1.2f, 0);
        var actionCanvas = actionHolder.AddComponent<Canvas>();
        actionCanvas.renderMode = RenderMode.WorldSpace;
        actionCanvas.sortingOrder = 5;
        var actionImg = actionHolder.AddComponent<Image>();
        actionImg.color = Color.white;
        actionHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);
        actionHolder.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // Child: Health bar (Slider)
        var healthSlider = CreateHealthBarChild(enemyObj.transform);

        // Child: Mana text
        var manaTextObj = CreateManaTextChild(enemyObj.transform);

        // Load VFX prefabs
        var bleedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/BleedEffect.prefab");
        var healPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/HealEffect.prefab");
        var hurtTextPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/HurtText.prefab");
        var healTextPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/HealText.prefab");
        var poisonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/PoisonEffect.prefab");
        var enemyInfo = AssetDatabase.LoadAssetAtPath<Information>($"Assets/ScriptableObjects/Enemies/{enemyName}.asset");

        // Wire BaseAttributes
        var baseAttrSO = new SerializedObject(baseAttr);
        baseAttrSO.FindProperty("_class").enumValueIndex = (int)charClass;
        baseAttrSO.FindProperty("_fixedLevel").intValue = 1;
        baseAttrSO.FindProperty("_progression").objectReferenceValue = progression;
        baseAttrSO.FindProperty("_activeModifiers").boolValue = false;
        baseAttrSO.ApplyModifiedProperties();

        // Wire HealthManager
        var healthSO = new SerializedObject(healthMgr);
        healthSO.FindProperty("_slider").objectReferenceValue = healthSlider.GetComponent<Slider>();
        healthSO.FindProperty("_bleed").objectReferenceValue = bleedPrefab;
        healthSO.FindProperty("_heal").objectReferenceValue = healPrefab;
        healthSO.FindProperty("_hurtTextGameObject").objectReferenceValue = hurtTextPrefab;
        healthSO.FindProperty("_healTextGameObject").objectReferenceValue = healTextPrefab;
        healthSO.FindProperty("_bleedPosition").vector3Value = new Vector3(0, -0.5f, 0);
        healthSO.ApplyModifiedProperties();

        // Wire ManaManager
        var manaSO = new SerializedObject(manaMgr);
        manaSO.FindProperty("_textMana").objectReferenceValue = manaTextObj.GetComponentInChildren<TextMeshProUGUI>();
        manaSO.ApplyModifiedProperties();

        // Wire EnemyController
        var ctrlSO = new SerializedObject(enemyCtrl);
        ctrlSO.FindProperty("_entityAttributes").objectReferenceValue = baseAttr;
        ctrlSO.FindProperty("_entityCombatModule").objectReferenceValue = combatMgr;
        ctrlSO.FindProperty("_entityManaManager").objectReferenceValue = manaMgr;
        ctrlSO.FindProperty("_entityHealthManager").objectReferenceValue = healthMgr;
        ctrlSO.FindProperty("_entityDeathManager").objectReferenceValue = deathMgr;
        ctrlSO.FindProperty("_entityType").enumValueIndex = (int)EntityType.Enemy;
        ctrlSO.FindProperty("_highlight").objectReferenceValue = highlight;
        ctrlSO.FindProperty("_preferLine").enumValueIndex = (int)preferLine;
        ctrlSO.FindProperty("_poison").objectReferenceValue = poisonPrefab;
        ctrlSO.FindProperty("_info").objectReferenceValue = enemyInfo;
        ctrlSO.FindProperty("_actionImageHolder").objectReferenceValue = actionHolder;
        ctrlSO.FindProperty("_enemyValue").intValue = enemyValue;
        ctrlSO.ApplyModifiedProperties();

        // Wire CombatManager basic skills
        var combatSO = new SerializedObject(combatMgr);
        var skillsProp = combatSO.FindProperty("_basicSkills");
        skillsProp.arraySize = skillNames.Length;
        for (int i = 0; i < skillNames.Length; i++)
        {
            var skill = AssetDatabase.LoadAssetAtPath<Skill>($"Assets/ScriptableObjects/Skills/{skillNames[i]}.asset");
            skillsProp.GetArrayElementAtIndex(i).objectReferenceValue = skill;
        }
        combatSO.ApplyModifiedProperties();

        // Save as prefab
        var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
            PrefabUtility.SaveAsPrefabAsset(enemyObj, prefabPath);
        else
            PrefabUtility.SaveAsPrefabAsset(enemyObj, prefabPath);

        DestroyImmediate(enemyObj);
        Debug.Log($"[CombatSceneBuilder] Enemy prefab created: {enemyName} ({charClass}, {preferLine})");
    }

    // ============================================================
    // PLAYER PREFAB GENERATOR
    // ============================================================

    private static void GeneratePlayerPrefab(Progression progression)
    {
        string prefabPath = "Assets/Prefabs/Player/Player.prefab";

        var playerObj = new GameObject("Player");
        playerObj.tag = "Player";

        // Also mark as CombatPlayer (used by FindGameObjectWithTag("CombatPlayer"))
        // Note: Unity only supports one tag per object, so we use "Player" tag
        // The code also searches for "CombatPlayer" — we need to set tag to "CombatPlayer"
        // Actually, looking at the code: Player tag is used for initiative list, CombatPlayer for references
        // We'll use "CombatPlayer" and handle initiative via code awareness
        // Actually SkillHandler.Awake uses "CombatPlayer", TurnManager.DoCharacterList uses "Player"
        // We need BOTH tags but Unity only supports one tag per GameObject
        // Solution: Use "Player" tag (required for turn system), add a child with "CombatPlayer" tag
        // Wait - looking at PlayerController.Awake: it does FindGameObjectWithTag("Player") for BaseAttributes
        // And SkillHandler uses FindGameObjectWithTag("CombatPlayer")
        // Let's check... EnemyController uses FindGameObjectWithTag("CombatPlayer")
        // We'll set the main tag to "Player" and create a child marker tagged "CombatPlayer"
        // Actually, simpler: just set tag to "CombatPlayer" since that's what enemies/skills look for
        // and the turn system finds "Player" tagged objects...
        // Best approach: set to "Player" and add the CombatPlayer tag to the same object
        // Unity can't do dual tags. Let's just tag as "CombatPlayer" since more things reference it
        // and modify the flow... No, we shouldn't modify code.
        // Looking again: TurnManager.DoCharacterList() uses FindGameObjectsWithTag("Player")
        // EnemyController._target uses FindGameObjectWithTag("CombatPlayer")
        // Solution: Keep player tagged "Player" and also create a thin CombatPlayer reference object

        // Actually the cleanest solution is tag "CombatPlayer" on the player,
        // since TurnManager can work with a direct reference (_playerController is serialized).
        // But DoCharacterList() concatenates enemies + players by tag...
        // Let's just go with "Player" tag and note that CombatPlayer tag lookup needs a small fix.
        // We'll add a comment noting this.

        // For now: tag as "Player" (critical for turn system).
        // The CombatPlayer lookups in EnemyController/SkillHandler will find it if we add
        // a child tagged CombatPlayer that has a PlayerController reference.
        // Actually simplest: just add the "CombatPlayer" tag to the same object. Can we?
        // No, Unity only allows 1 tag. So let's pick the one used more: "CombatPlayer"
        // Actually "Player" is used in: TurnManager.DoCharacterList (finds by tag),
        // PlayerController.Awake (finds by tag for BaseAttributes — finds itself),
        // WhosTurn (checks tag == "Player")
        // That's 3 critical uses. "CombatPlayer" is used in EnemyController.Awake and SkillHandler.Awake.
        // The EnemyController.Awake does its own GetComponent calls, so it's fine — just needs the reference.
        // SkillHandler.Awake: needs to find player.

        // Decision: Tag as "Player". For "CombatPlayer" references, we add a child empty tagged CombatPlayer
        // with a script that returns the parent's PlayerController.
        // OR: just set to "Player" and we'll fix the 2 CombatPlayer lookups to use "Player" instead.
        // That's the simplest fix. Let's do that after prefab generation.

        playerObj.tag = "Player";

        // SpriteRenderer
        var sr = playerObj.AddComponent<SpriteRenderer>();
        sr.sprite = CreatePlaceholderSprite(CharacterClass.Player);
        sr.color = new Color(0.2f, 0.8f, 1f); // Cyan (Swarm style)
        sr.sortingOrder = 1;

        // Collider
        var col = playerObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1, 1.5f);

        // Core components
        var baseAttr = playerObj.AddComponent<BaseAttributes>();
        var healthMgr = playerObj.AddComponent<HealthManager>();
        var manaMgr = playerObj.AddComponent<ManaManager>();
        var deathMgr = playerObj.AddComponent<DeathManager>();
        var expModule = playerObj.AddComponent<ExperienceModule>();
        var equipment = playerObj.AddComponent<Equipment>();
        var enemyHolder = playerObj.AddComponent<EnemyHolder>();
        var combatMgr = playerObj.AddComponent<PlayerCombatManager>();
        // PlayerController is auto-added by [RequireComponent] on PlayerCombatManager
        var playerCtrl = playerObj.GetComponent<PlayerController>();
        if (playerCtrl == null)
            playerCtrl = playerObj.AddComponent<PlayerController>();

        // Child: Highlight
        var highlight = CreateHighlightChild(playerObj.transform);

        // Child: Health bar
        var healthSlider = CreateHealthBarChild(playerObj.transform);

        // Child: Mana text
        var manaTextObj = CreateManaTextChild(playerObj.transform);

        // Load VFX
        var bleedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/BleedEffect.prefab");
        var healPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/HealEffect.prefab");
        var hurtTextPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/HurtText.prefab");
        var healTextPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/HealText.prefab");
        var poisonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/PoisonEffect.prefab");
        var levelUpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/LevelUpEffect.prefab");

        // Wire BaseAttributes
        var baseAttrSO = new SerializedObject(baseAttr);
        baseAttrSO.FindProperty("_class").enumValueIndex = (int)CharacterClass.Player;
        baseAttrSO.FindProperty("_fixedLevel").intValue = 1;
        baseAttrSO.FindProperty("_progression").objectReferenceValue = progression;
        baseAttrSO.FindProperty("_activeModifiers").boolValue = true; // Player uses equipment modifiers
        baseAttrSO.ApplyModifiedProperties();

        // Wire HealthManager
        var healthSO = new SerializedObject(healthMgr);
        healthSO.FindProperty("_slider").objectReferenceValue = healthSlider.GetComponent<Slider>();
        healthSO.FindProperty("_bleed").objectReferenceValue = bleedPrefab;
        healthSO.FindProperty("_heal").objectReferenceValue = healPrefab;
        healthSO.FindProperty("_hurtTextGameObject").objectReferenceValue = hurtTextPrefab;
        healthSO.FindProperty("_healTextGameObject").objectReferenceValue = healTextPrefab;
        healthSO.FindProperty("_bleedPosition").vector3Value = new Vector3(0, -0.5f, 0);
        healthSO.ApplyModifiedProperties();

        // Wire ManaManager
        var manaSO = new SerializedObject(manaMgr);
        manaSO.FindProperty("_textMana").objectReferenceValue = manaTextObj.GetComponentInChildren<TextMeshProUGUI>();
        manaSO.ApplyModifiedProperties();

        // Wire ExperienceModule
        var expSO = new SerializedObject(expModule);
        expSO.FindProperty("_levelUpEffect").objectReferenceValue = levelUpPrefab;
        expSO.FindProperty("_attributes").objectReferenceValue = baseAttr;
        expSO.ApplyModifiedProperties();

        // Wire EntityController fields
        var ctrlSO = new SerializedObject(playerCtrl);
        ctrlSO.FindProperty("_entityAttributes").objectReferenceValue = baseAttr;
        ctrlSO.FindProperty("_entityCombatModule").objectReferenceValue = combatMgr;
        ctrlSO.FindProperty("_entityManaManager").objectReferenceValue = manaMgr;
        ctrlSO.FindProperty("_entityHealthManager").objectReferenceValue = healthMgr;
        ctrlSO.FindProperty("_entityDeathManager").objectReferenceValue = deathMgr;
        ctrlSO.FindProperty("_entityType").enumValueIndex = (int)EntityType.Player;
        ctrlSO.FindProperty("_highlight").objectReferenceValue = highlight;
        ctrlSO.FindProperty("_poison").objectReferenceValue = poisonPrefab;
        ctrlSO.ApplyModifiedProperties();

        // Wire CombatManager basic skills
        var combatSO = new SerializedObject(combatMgr);
        var skillsProp = combatSO.FindProperty("_basicSkills");
        skillsProp.arraySize = 1;
        var basicAttack = AssetDatabase.LoadAssetAtPath<Skill>("Assets/ScriptableObjects/Skills/BasicAttack.asset");
        skillsProp.GetArrayElementAtIndex(0).objectReferenceValue = basicAttack;
        combatSO.ApplyModifiedProperties();

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(playerObj, prefabPath);
        DestroyImmediate(playerObj);

        Debug.Log("[CombatSceneBuilder] Player prefab created with all components wired.");
    }

    // ============================================================
    // VFX PLACEHOLDER PREFABS
    // ============================================================

    private static void GenerateVFXPrefabs()
    {
        // BleedEffect — red particle burst
        CreateVFXPrefab("BleedEffect", new Color(0.8f, 0, 0), true);

        // HealEffect — green particle burst
        CreateVFXPrefab("HealEffect", new Color(0, 0.8f, 0.3f), true);

        // PoisonEffect — purple particle
        CreateVFXPrefab("PoisonEffect", new Color(0.5f, 0, 0.8f), true);

        // LevelUpEffect — gold particle
        CreateVFXPrefab("LevelUpEffect", new Color(1f, 0.85f, 0), true);

        // HurtText — floating damage number (TextMeshPro)
        CreateFloatingTextPrefab("HurtText", Color.red);

        // HealText — floating heal number
        CreateFloatingTextPrefab("HealText", Color.green);

        Debug.Log("[CombatSceneBuilder] 6 VFX prefabs created in Assets/Prefabs/VFX/");
    }

    private static void CreateVFXPrefab(string name, Color color, bool isParticle)
    {
        string path = $"Assets/Prefabs/VFX/{name}.prefab";

        var obj = new GameObject(name);
        var ps = obj.AddComponent<ParticleSystem>();

        // Configure basic burst particle
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.15f;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 10)
        });

        // Auto-destroy
        var autoDestroy = obj.AddComponent<AutoDestroy>();

        PrefabUtility.SaveAsPrefabAsset(obj, path);
        DestroyImmediate(obj);
    }

    private static void CreateFloatingTextPrefab(string name, Color color)
    {
        string path = $"Assets/Prefabs/VFX/{name}.prefab";

        var obj = new GameObject(name);

        // TextMeshPro for world-space text
        var tmp = obj.AddComponent<TextMeshPro>();
        tmp.fontSize = 8;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.text = "0";
        tmp.sortingOrder = 100;

        // Float upward and destroy
        var floater = obj.AddComponent<FloatingText>();
        var autoDestroy = obj.AddComponent<AutoDestroy>();

        PrefabUtility.SaveAsPrefabAsset(obj, path);
        DestroyImmediate(obj);
    }

    // ============================================================
    // SKILL CARD PREFAB
    // ============================================================

    private static void GenerateSkillCardPrefab()
    {
        string path = "Assets/Prefabs/UI/SkillCard.prefab";

        var cardObj = new GameObject("SkillCard");

        // RectTransform (required for UI)
        var rt = cardObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100);

        // Visual background
        var img = cardObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.25f, 0.35f, 0.9f);

        // SkillHolder + SkillHandler + ColorButton
        cardObj.AddComponent<SkillHolder>();
        cardObj.AddComponent<SkillHandler>();
        var colorBtn = cardObj.AddComponent<ColorButton>();

        // Cooldown slider (inside card)
        var sliderObj = new GameObject("CooldownSlider");
        sliderObj.transform.SetParent(cardObj.transform, false);
        var sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0);
        sliderRect.anchorMax = new Vector2(1, 0.1f);
        sliderRect.sizeDelta = Vector2.zero;
        var slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 5;
        slider.direction = Slider.Direction.LeftToRight;
        slider.interactable = false;
        // Slider fill
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.8f, 0.3f, 0.1f);
        slider.fillRect = fillRect;

        // Wire ColorButton cooldown slider
        var cbSO = new SerializedObject(colorBtn);
        cbSO.FindProperty("_cldSlider").objectReferenceValue = slider;
        cbSO.FindProperty("_activeColor").colorValue = new Color(0.3f, 0.7f, 1f);
        cbSO.FindProperty("_cooldownColor").colorValue = new Color(0.5f, 0.2f, 0.1f);
        cbSO.FindProperty("_lowManaColor").colorValue = new Color(0.3f, 0.3f, 0.3f);
        cbSO.ApplyModifiedProperties();

        // Skill name text
        var nameObj = new GameObject("SkillName");
        nameObj.transform.SetParent(cardObj.transform, false);
        var nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
        nameTMP.fontSize = 14;
        nameTMP.alignment = TextAlignmentOptions.Center;
        nameTMP.color = Color.white;
        nameTMP.text = "Skill";
        var nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.4f);
        nameRect.anchorMax = new Vector2(1, 0.8f);
        nameRect.sizeDelta = Vector2.zero;

        // Mana cost text
        var manaObj = new GameObject("ManaCost");
        manaObj.transform.SetParent(cardObj.transform, false);
        var manaTMP = manaObj.AddComponent<TextMeshProUGUI>();
        manaTMP.fontSize = 16;
        manaTMP.alignment = TextAlignmentOptions.Center;
        manaTMP.color = new Color(0.3f, 0.7f, 1f);
        manaTMP.text = "1";
        var manaRect = manaObj.GetComponent<RectTransform>();
        manaRect.anchorMin = new Vector2(0, 0.8f);
        manaRect.anchorMax = new Vector2(1, 1);
        manaRect.sizeDelta = Vector2.zero;

        // SkillAppearance to auto-populate card text
        var appearance = cardObj.AddComponent<SkillAppearance>();
        var appSO = new SerializedObject(appearance);
        appSO.FindProperty("_skillManager").objectReferenceValue = cardObj.GetComponent<SkillHandler>();
        appSO.FindProperty("_manaCostText").objectReferenceValue = manaTMP;
        appSO.FindProperty("_nameOfSkillText").objectReferenceValue = nameTMP;
        appSO.ApplyModifiedProperties();

        // Layout element for card sizing in horizontal layout
        var layout = cardObj.AddComponent<LayoutElement>();
        layout.preferredWidth = 100;
        layout.preferredHeight = 100;

        PrefabUtility.SaveAsPrefabAsset(cardObj, path);
        DestroyImmediate(cardObj);

        Debug.Log("[CombatSceneBuilder] SkillCard prefab created.");
    }

    // ============================================================
    // DIFFICULTY LIST WIRING
    // ============================================================

    private static void WireDifficultyLists()
    {
        // Load all enemy prefabs as EnemyController references
        var grunt = LoadEnemyPrefab("Grunt");
        var archer = LoadEnemyPrefab("Archer");
        var healer = LoadEnemyPrefab("Healer");
        var knight = LoadEnemyPrefab("Knight");
        var hunter = LoadEnemyPrefab("Hunter");
        var assassin = LoadEnemyPrefab("Assassin");
        var swordMaster = LoadEnemyPrefab("SwordMaster");
        var boss = LoadEnemyPrefab("Boss");
        var necromancer = LoadEnemyPrefab("Necromancer");

        // Easy: Grunt, Archer, Healer
        WireDifficultyList("Easy", new[] { grunt, archer, healer });

        // Normal: Grunt, Archer, Healer, Knight, Hunter
        WireDifficultyList("Normal", new[] { grunt, archer, healer, knight, hunter });

        // Hard: Knight, Hunter, Assassin, SwordMaster, Necromancer, Boss
        WireDifficultyList("Hard", new[] { knight, hunter, assassin, swordMaster, necromancer, boss });

        Debug.Log("[CombatSceneBuilder] Difficulty lists wired with enemy prefabs.");
    }

    private static EnemyController LoadEnemyPrefab(string name)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Enemies/{name}.prefab");
        return prefab != null ? prefab.GetComponent<EnemyController>() : null;
    }

    private static void WireDifficultyList(string name, EnemyController[] enemies)
    {
        var list = AssetDatabase.LoadAssetAtPath<DifficultyList>($"Assets/ScriptableObjects/Difficulty/{name}.asset");
        if (list == null) return;

        var so = new SerializedObject(list);
        var enemyListProp = so.FindProperty("_enemyList");
        if (enemyListProp == null)
        {
            // Try alternate field name
            enemyListProp = so.FindProperty("enemyList");
        }
        if (enemyListProp != null)
        {
            enemyListProp.arraySize = enemies.Length;
            for (int i = 0; i < enemies.Length; i++)
                enemyListProp.GetArrayElementAtIndex(i).objectReferenceValue = enemies[i];
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(list);
        }
        else
        {
            Debug.LogWarning($"[CombatSceneBuilder] Could not find enemy list property in DifficultyList '{name}'. Check field name.");
        }
    }

    // ============================================================
    // HELPER: Create child GameObjects
    // ============================================================

    private static GameObject CreateHighlightChild(Transform parent)
    {
        var highlight = new GameObject("Highlight");
        highlight.transform.SetParent(parent, false);
        var hlSr = highlight.AddComponent<SpriteRenderer>();
        hlSr.color = new Color(1f, 1f, 0f, 0.3f); // yellow glow
        hlSr.sortingOrder = 0;
        highlight.transform.localScale = new Vector3(1.3f, 1.8f, 1f);
        highlight.SetActive(false); // hidden by default
        return highlight;
    }

    private static GameObject CreateHealthBarChild(Transform parent)
    {
        // World-space canvas for health bar
        var hbCanvas = new GameObject("HealthBarCanvas");
        hbCanvas.transform.SetParent(parent, false);
        hbCanvas.transform.localPosition = new Vector3(0, -1f, 0);
        var canvasComp = hbCanvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.WorldSpace;
        canvasComp.sortingOrder = 5;
        var hbRect = hbCanvas.GetComponent<RectTransform>();
        hbRect.sizeDelta = new Vector2(120f, 15f);
        hbCanvas.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // Slider
        var sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(hbCanvas.transform, false);
        var sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.sizeDelta = Vector2.zero;
        var slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;
        slider.interactable = false;
        slider.direction = Slider.Direction.LeftToRight;

        // Background
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        var bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        var bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Fill area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.1f, 0.8f, 0.2f);

        slider.fillRect = fillRect;

        return sliderObj;
    }

    private static GameObject CreateManaTextChild(Transform parent)
    {
        var manaCanvas = new GameObject("ManaCanvas");
        manaCanvas.transform.SetParent(parent, false);
        manaCanvas.transform.localPosition = new Vector3(0, -1.3f, 0);
        var canvasComp = manaCanvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.WorldSpace;
        canvasComp.sortingOrder = 5;
        var mRect = manaCanvas.GetComponent<RectTransform>();
        mRect.sizeDelta = new Vector2(200f, 30f);
        manaCanvas.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        var textObj = new GameObject("ManaText");
        textObj.transform.SetParent(manaCanvas.transform, false);
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.3f, 0.6f, 1f);
        tmp.text = "AP: 3/3";
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return manaCanvas;
    }

    private static GameObject CreateUIPanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        return panel;
    }

    // ============================================================
    // HELPER: Placeholder sprites + colors
    // ============================================================

    private static Sprite CreatePlaceholderSprite(CharacterClass charClass)
    {
        // Create a simple colored texture as placeholder
        var tex = new Texture2D(32, 48, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color fill = GetEnemyColor(charClass);

        for (int x = 0; x < 32; x++)
            for (int y = 0; y < 48; y++)
                tex.SetPixel(x, y, fill);

        // Simple border
        for (int x = 0; x < 32; x++)
        {
            tex.SetPixel(x, 0, Color.black);
            tex.SetPixel(x, 47, Color.black);
        }
        for (int y = 0; y < 48; y++)
        {
            tex.SetPixel(0, y, Color.black);
            tex.SetPixel(31, y, Color.black);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 32, 48), new Vector2(0.5f, 0.5f), 32);
    }

    private static Color GetEnemyColor(CharacterClass charClass)
    {
        switch (charClass)
        {
            case CharacterClass.Player: return new Color(0.2f, 0.8f, 1f);     // Cyan
            case CharacterClass.Grunt: return new Color(0.5f, 0.4f, 0.3f);    // Brown
            case CharacterClass.Archer: return new Color(0.3f, 0.6f, 0.3f);   // Green
            case CharacterClass.Healer: return new Color(0.9f, 0.9f, 0.5f);   // Yellow
            case CharacterClass.Knight: return new Color(0.5f, 0.5f, 0.6f);   // Steel
            case CharacterClass.Hunter: return new Color(0.4f, 0.5f, 0.3f);   // Olive
            case CharacterClass.Assassin: return new Color(0.3f, 0.1f, 0.3f); // Dark purple
            case CharacterClass.SwordMaster: return new Color(0.7f, 0.3f, 0.2f); // Crimson
            case CharacterClass.Boss: return new Color(0.8f, 0.1f, 0.1f);     // Red
            case CharacterClass.Necromancer: return new Color(0.4f, 0.2f, 0.5f); // Purple
            default: return Color.white;
        }
    }

    // ============================================================
    // HELPER: Ensure folder exists
    // ============================================================

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string folder = System.IO.Path.GetFileName(path);

        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, folder);
    }
}
