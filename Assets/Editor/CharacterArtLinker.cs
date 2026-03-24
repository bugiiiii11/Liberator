using Liberator.Attributes;
using Liberator.Combat.Controllers;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Replaces placeholder sprites on combat prefabs with Meda Wars character art (Rogue prefabs).
/// Access via: Liberator → 4. Link Character Art
///
/// Mapping:
///   Player    → Rogue_01 (tan hood)
///   Grunt     → Rogue_02 (dark, red eyes)
///   Archer    → Rogue_03 (dark, scarred)
///   Knight    → Rogue_04 (eyepatch)
///   Healer    → Rogue_05 (?)
///   Hunter    → Rogue_06 (white mask, red eyes)
///   Assassin  → Rogue_04 (tinted dark purple)
///   SwordMaster → Rogue_03 (tinted crimson)
///   Boss      → Rogue_06 (scaled up, tinted red)
///   Necromancer → Rogue_02 (tinted purple)
/// </summary>
public class CharacterArtLinker : Editor
{
    private const string ROGUE_PATH = "Assets/_temporaryAssets/Mighty Heroes (Rogue) 2D Fantasy Characters Pack/Prefabs/";
    private const string ENEMY_PREFAB_PATH = "Assets/Prefabs/Enemies/";
    private const string PLAYER_PREFAB_PATH = "Assets/Prefabs/Player/";

    // Rogue variant mapping — matches original Meda Wars prefabs exactly
    // Verified from meda-wars-master/Assets/Prefabs/TurnBased/GameObjects/ GUIDs
    private static readonly (string enemyName, string rogueVariant, Color tint, float scale)[] EnemyMapping = new[]
    {
        ("Grunt",       "Rogue_03", Color.white,                          0.4f),  // No Grunt in original, use Rogue_03
        ("Archer",      "Rogue_03", new Color(0.8f, 1f, 0.8f),           0.4f),  // No Archer in original, green-tint Rogue_03
        ("Knight",      "Rogue_05", Color.white,                          0.45f), // Original: Rogue_05
        ("Healer",      "Rogue_05", new Color(0.9f, 1f, 0.8f),           0.4f),  // Original: Rogue_05 (shared with Knight)
        ("Hunter",      "Rogue_02", Color.white,                          0.4f),  // Original: Rogue_02
        ("Assassin",    "Rogue_06", Color.white,                          0.4f),  // Original: Rogue_06
        ("SwordMaster", "Rogue_04", Color.white,                          0.45f), // Original: Rogue_04
        ("Boss",        "Rogue_06", Color.white,                          0.6f),  // Original: Rogue_06 (larger)
        ("Necromancer", "Rogue_04", new Color(0.8f, 0.6f, 1f),           0.4f),  // Original: Rogue_04 (purple tint)
    };

    [MenuItem("Liberator/4. Link Character Art")]
    public static void LinkCharacterArt()
    {
        int linked = 0;

        // Link player — Original Meda Wars used Rogue_01
        if (LinkPrefabArt($"{PLAYER_PREFAB_PATH}Player.prefab", "Rogue_01", Color.white, 0.45f))
            linked++;

        // Link enemies
        foreach (var (enemyName, rogueVariant, tint, scale) in EnemyMapping)
        {
            if (LinkPrefabArt($"{ENEMY_PREFAB_PATH}{enemyName}.prefab", rogueVariant, tint, scale))
                linked++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CharacterArtLinker] Linked art to {linked} prefabs. Re-run 'Liberator → 2. Build CombatScene' to see changes.");
    }

    private static bool LinkPrefabArt(string combatPrefabPath, string rogueVariant, Color tint, float scale)
    {
        // Load our combat prefab
        var combatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(combatPrefabPath);
        if (combatPrefab == null)
        {
            Debug.LogWarning($"[CharacterArtLinker] Combat prefab not found: {combatPrefabPath}");
            return false;
        }

        // Load Rogue prefab
        var roguePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ROGUE_PATH}{rogueVariant}.prefab");
        if (roguePrefab == null)
        {
            Debug.LogWarning($"[CharacterArtLinker] Rogue prefab not found: {rogueVariant}");
            return false;
        }

        // Open prefab for editing
        string assetPath = AssetDatabase.GetAssetPath(combatPrefab);
        var prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

        // Remove old placeholder SpriteRenderer on root (if exists)
        var oldSR = prefabRoot.GetComponent<SpriteRenderer>();
        if (oldSR != null)
            DestroyImmediate(oldSR);

        // Remove old visual child if we previously linked
        var oldVisual = prefabRoot.transform.Find("Visual");
        if (oldVisual != null)
            DestroyImmediate(oldVisual.gameObject);

        // Instantiate Rogue as child named "Visual"
        var visual = (GameObject)PrefabUtility.InstantiatePrefab(roguePrefab, prefabRoot.transform);
        visual.name = "Visual";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(scale, scale, scale);

        // Apply tint to all SpriteRenderers in the visual hierarchy
        if (tint != Color.white)
        {
            foreach (var sr in visual.GetComponentsInChildren<SpriteRenderer>())
            {
                sr.color = tint;
            }
        }

        // Ensure the root still has a Renderer for _myMaterial access
        // EntityController fetches material via GetComponentInChildren<Renderer>()
        // The Rogue's child SpriteRenderers satisfy this requirement

        // Flip enemies to face left (toward player)
        bool isEnemy = prefabRoot.CompareTag("Enemy");
        if (isEnemy)
        {
            visual.transform.localScale = new Vector3(-scale, scale, scale);
        }

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log($"[CharacterArtLinker] {System.IO.Path.GetFileNameWithoutExtension(assetPath)} → {rogueVariant} (scale={scale}, tint={tint})");
        return true;
    }
}
