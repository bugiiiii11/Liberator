using System.Collections.Generic;
using Liberator.Attributes;
using Liberator.Combat;
using Liberator.Combat.Skills;
using Liberator.Combat.UI;
using UnityEditor;
using UnityEngine;
using Attribute = Liberator.Attributes.Attribute;

/// <summary>
/// Editor tool to auto-generate all ScriptableObject game data.
/// Access via: Liberator → Generate All Game Data
/// </summary>
public class GameDataGenerator : Editor
{
    [MenuItem("Liberator/Generate All Game Data")]
    public static void GenerateAll()
    {
        GenerateProgression();
        GenerateSkills();
        GenerateDifficultyLists();
        GenerateEnemyInformation();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[GameDataGenerator] All game data generated successfully!");
    }

    [MenuItem("Liberator/Generate Progression Only")]
    public static void GenerateProgression()
    {
        var progression = ScriptableObject.CreateInstance<Progression>();

        // Use SerializedObject to set the private _charaters field
        var so = new SerializedObject(progression);
        var charsProp = so.FindProperty("_charaters");

        charsProp.ClearArray();

        // Player
        AddCharacter(charsProp, CharacterClass.Player, new Dictionary<Attribute, float[]>
        {
            { Attribute.Health, new float[] { 100, 120, 150 } },
            { Attribute.Damage, new float[] { 15, 18, 22 } },
            { Attribute.Defense, new float[] { 5, 7, 9 } },
            { Attribute.AttackSpeed, new float[] { 1, 1, 1 } },
            { Attribute.AttackRange, new float[] { 1, 1, 1 } },
            { Attribute.CriticalChance, new float[] { 10, 12, 15 } },
            { Attribute.CriticalDamage, new float[] { 150, 160, 175 } },
            { Attribute.DodgeChance, new float[] { 5, 7, 10 } },
            { Attribute.Initiative, new float[] { 10, 12, 14 } },
            { Attribute.Mana, new float[] { 3, 4, 5 } },
            { Attribute.ExperienceToLevelUp, new float[] { 100, 300 } },
        });

        // Grunt (basic melee enemy)
        AddCharacter(charsProp, CharacterClass.Grunt, new Dictionary<Attribute, float[]>
        {
            { Attribute.Health, new float[] { 40, 55, 70 } },
            { Attribute.Damage, new float[] { 8, 11, 14 } },
            { Attribute.Defense, new float[] { 2, 3, 5 } },
            { Attribute.AttackSpeed, new float[] { 1, 1, 1 } },
            { Attribute.AttackRange, new float[] { 1, 1, 1 } },
            { Attribute.CriticalChance, new float[] { 5, 7, 10 } },
            { Attribute.CriticalDamage, new float[] { 120, 130, 140 } },
            { Attribute.DodgeChance, new float[] { 3, 4, 5 } },
            { Attribute.Initiative, new float[] { 5, 6, 7 } },
            { Attribute.Mana, new float[] { 2, 3, 3 } },
            { Attribute.ExperienceToLevelUp, new float[] { 50, 150 } },
        });

        // Archer (ranged, high crit)
        AddCharacter(charsProp, CharacterClass.Archer, new Dictionary<Attribute, float[]>
        {
            { Attribute.Health, new float[] { 30, 40, 50 } },
            { Attribute.Damage, new float[] { 12, 15, 19 } },
            { Attribute.Defense, new float[] { 1, 2, 3 } },
            { Attribute.AttackSpeed, new float[] { 1, 1, 1 } },
            { Attribute.AttackRange, new float[] { 3, 3, 3 } },
            { Attribute.CriticalChance, new float[] { 15, 18, 22 } },
            { Attribute.CriticalDamage, new float[] { 140, 155, 170 } },
            { Attribute.DodgeChance, new float[] { 8, 10, 12 } },
            { Attribute.Initiative, new float[] { 8, 9, 11 } },
            { Attribute.Mana, new float[] { 2, 3, 3 } },
            { Attribute.ExperienceToLevelUp, new float[] { 50, 150 } },
        });

        // Healer (support, low damage, high mana)
        AddCharacter(charsProp, CharacterClass.Healer, new Dictionary<Attribute, float[]>
        {
            { Attribute.Health, new float[] { 35, 45, 60 } },
            { Attribute.Damage, new float[] { 6, 8, 10 } },
            { Attribute.Defense, new float[] { 3, 4, 6 } },
            { Attribute.AttackSpeed, new float[] { 1, 1, 1 } },
            { Attribute.AttackRange, new float[] { 2, 2, 2 } },
            { Attribute.CriticalChance, new float[] { 5, 7, 8 } },
            { Attribute.CriticalDamage, new float[] { 110, 120, 130 } },
            { Attribute.DodgeChance, new float[] { 5, 6, 8 } },
            { Attribute.Initiative, new float[] { 6, 7, 8 } },
            { Attribute.Mana, new float[] { 4, 5, 6 } },
            { Attribute.ExperienceToLevelUp, new float[] { 50, 150 } },
        });

        // Knight (tank, high defense/health)
        AddCharacter(charsProp, CharacterClass.Knight, new Dictionary<Attribute, float[]>
        {
            { Attribute.Health, new float[] { 60, 80, 100 } },
            { Attribute.Damage, new float[] { 10, 13, 16 } },
            { Attribute.Defense, new float[] { 8, 11, 14 } },
            { Attribute.AttackSpeed, new float[] { 1, 1, 1 } },
            { Attribute.AttackRange, new float[] { 1, 1, 1 } },
            { Attribute.CriticalChance, new float[] { 5, 6, 8 } },
            { Attribute.CriticalDamage, new float[] { 130, 140, 150 } },
            { Attribute.DodgeChance, new float[] { 2, 3, 4 } },
            { Attribute.Initiative, new float[] { 4, 5, 6 } },
            { Attribute.Mana, new float[] { 5, 6, 7 } },
            { Attribute.ExperienceToLevelUp, new float[] { 50, 150 } },
        });

        // Hunter (fast, high dodge)
        AddCharacter(charsProp, CharacterClass.Hunter, new Dictionary<Attribute, float[]>
        {
            { Attribute.Health, new float[] { 35, 45, 55 } },
            { Attribute.Damage, new float[] { 14, 17, 21 } },
            { Attribute.Defense, new float[] { 2, 3, 4 } },
            { Attribute.AttackSpeed, new float[] { 1, 1, 1 } },
            { Attribute.AttackRange, new float[] { 2, 2, 2 } },
            { Attribute.CriticalChance, new float[] { 20, 23, 27 } },
            { Attribute.CriticalDamage, new float[] { 160, 175, 190 } },
            { Attribute.DodgeChance, new float[] { 10, 13, 16 } },
            { Attribute.Initiative, new float[] { 9, 11, 13 } },
            { Attribute.Mana, new float[] { 2, 3, 3 } },
            { Attribute.ExperienceToLevelUp, new float[] { 50, 150 } },
        });

        // Assassin (glass cannon, very high crit/dodge)
        AddCharacter(charsProp, CharacterClass.Assassin, new Dictionary<Attribute, float[]>
        {
            { Attribute.Health, new float[] { 25, 33, 42 } },
            { Attribute.Damage, new float[] { 18, 23, 28 } },
            { Attribute.Defense, new float[] { 1, 2, 3 } },
            { Attribute.AttackSpeed, new float[] { 1, 1, 1 } },
            { Attribute.AttackRange, new float[] { 1, 1, 1 } },
            { Attribute.CriticalChance, new float[] { 25, 30, 35 } },
            { Attribute.CriticalDamage, new float[] { 180, 200, 220 } },
            { Attribute.DodgeChance, new float[] { 15, 18, 22 } },
            { Attribute.Initiative, new float[] { 12, 14, 16 } },
            { Attribute.Mana, new float[] { 3, 4, 4 } },
            { Attribute.ExperienceToLevelUp, new float[] { 50, 150 } },
        });

        // SwordMaster (balanced elite)
        AddCharacter(charsProp, CharacterClass.SwordMaster, new Dictionary<Attribute, float[]>
        {
            { Attribute.Health, new float[] { 50, 65, 80 } },
            { Attribute.Damage, new float[] { 16, 20, 25 } },
            { Attribute.Defense, new float[] { 4, 6, 8 } },
            { Attribute.AttackSpeed, new float[] { 1, 1, 1 } },
            { Attribute.AttackRange, new float[] { 1, 1, 1 } },
            { Attribute.CriticalChance, new float[] { 15, 18, 22 } },
            { Attribute.CriticalDamage, new float[] { 170, 185, 200 } },
            { Attribute.DodgeChance, new float[] { 5, 7, 9 } },
            { Attribute.Initiative, new float[] { 7, 8, 10 } },
            { Attribute.Mana, new float[] { 3, 4, 5 } },
            { Attribute.ExperienceToLevelUp, new float[] { 50, 150 } },
        });

        // Boss (mini-boss, very tanky + high damage)
        AddCharacter(charsProp, CharacterClass.Boss, new Dictionary<Attribute, float[]>
        {
            { Attribute.Health, new float[] { 150, 200, 260 } },
            { Attribute.Damage, new float[] { 20, 26, 33 } },
            { Attribute.Defense, new float[] { 10, 13, 17 } },
            { Attribute.AttackSpeed, new float[] { 1, 1, 1 } },
            { Attribute.AttackRange, new float[] { 1, 1, 1 } },
            { Attribute.CriticalChance, new float[] { 10, 12, 15 } },
            { Attribute.CriticalDamage, new float[] { 200, 220, 250 } },
            { Attribute.DodgeChance, new float[] { 3, 4, 5 } },
            { Attribute.Initiative, new float[] { 6, 7, 8 } },
            { Attribute.Mana, new float[] { 6, 7, 8 } },
            { Attribute.ExperienceToLevelUp, new float[] { 50, 150 } },
        });

        // Necromancer (summoner, medium stats)
        AddCharacter(charsProp, CharacterClass.Necromancer, new Dictionary<Attribute, float[]>
        {
            { Attribute.Health, new float[] { 45, 58, 72 } },
            { Attribute.Damage, new float[] { 10, 13, 16 } },
            { Attribute.Defense, new float[] { 3, 4, 6 } },
            { Attribute.AttackSpeed, new float[] { 1, 1, 1 } },
            { Attribute.AttackRange, new float[] { 2, 2, 2 } },
            { Attribute.CriticalChance, new float[] { 10, 12, 15 } },
            { Attribute.CriticalDamage, new float[] { 130, 145, 160 } },
            { Attribute.DodgeChance, new float[] { 5, 6, 8 } },
            { Attribute.Initiative, new float[] { 7, 8, 9 } },
            { Attribute.Mana, new float[] { 5, 6, 7 } },
            { Attribute.ExperienceToLevelUp, new float[] { 50, 150 } },
        });

        so.ApplyModifiedProperties();

        // Save or overwrite existing
        string path = "Assets/ScriptableObjects/Progression.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Progression>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(progression, existing);
            EditorUtility.SetDirty(existing);
        }
        else
        {
            AssetDatabase.CreateAsset(progression, path);
        }

        Debug.Log("[GameDataGenerator] Progression created with 10 character classes x 3 levels");
    }

    [MenuItem("Liberator/Generate Skills Only")]
    public static void GenerateSkills()
    {
        // Basic Attack (all classes get this)
        CreateSkill("BasicAttack", "Basic Attack", "A simple melee strike.",
            manaCost: 1, damagePercent: 100, cooldown: 0,
            skillType: SkillType.Melee, aoe: AreaOfEffect.Single,
            numberOfAttacks: 1, delay: 0.2f, debuffs: new List<Debuff>());

        // Power Strike
        CreateSkill("PowerStrike", "Power Strike", "A devastating blow that deals 150% damage.",
            manaCost: 3, damagePercent: 150, cooldown: 2,
            skillType: SkillType.Melee, aoe: AreaOfEffect.Single,
            numberOfAttacks: 1, delay: 0.3f, debuffs: new List<Debuff>());

        // Cleave (hit entire row)
        CreateSkill("Cleave", "Cleave", "Sweep attack hitting all enemies in a row.",
            manaCost: 4, damagePercent: 80, cooldown: 3,
            skillType: SkillType.Melee, aoe: AreaOfEffect.Line,
            numberOfAttacks: 1, delay: 0.2f, debuffs: new List<Debuff>());

        // Poison Strike
        CreateSkill("PoisonStrike", "Poison Strike", "Melee attack that poisons the target for 3 turns.",
            manaCost: 2, damagePercent: 80, cooldown: 2,
            skillType: SkillType.Melee, aoe: AreaOfEffect.Single,
            numberOfAttacks: 1, delay: 0.2f, debuffs: new List<Debuff> { Debuff.Poison });

        // Stun Bash
        CreateSkill("StunBash", "Stun Bash", "A heavy hit that stuns the target for 1 turn.",
            manaCost: 3, damagePercent: 60, cooldown: 3,
            skillType: SkillType.Melee, aoe: AreaOfEffect.Single,
            numberOfAttacks: 1, delay: 0.3f, debuffs: new List<Debuff> { Debuff.Stun });

        // Arrow Shot (ranged basic)
        CreateSkill("ArrowShot", "Arrow Shot", "A ranged attack hitting any enemy.",
            manaCost: 1, damagePercent: 100, cooldown: 0,
            skillType: SkillType.Ranged, aoe: AreaOfEffect.Single,
            numberOfAttacks: 1, delay: 0.2f, debuffs: new List<Debuff>());

        // Volley (ranged AoE)
        CreateSkill("Volley", "Volley", "Rain of arrows hitting all enemies.",
            manaCost: 5, damagePercent: 60, cooldown: 4,
            skillType: SkillType.Ranged, aoe: AreaOfEffect.All,
            numberOfAttacks: 1, delay: 0.1f, debuffs: new List<Debuff>());

        // Double Shot
        CreateSkill("DoubleShot", "Double Shot", "Fire two arrows at a single target.",
            manaCost: 2, damagePercent: 70, cooldown: 1,
            skillType: SkillType.Ranged, aoe: AreaOfEffect.Single,
            numberOfAttacks: 2, delay: 0.3f, debuffs: new List<Debuff>());

        // Heal
        CreateSkill("Heal", "Heal", "Restore health based on your damage stat.",
            manaCost: 3, damagePercent: 120, cooldown: 2,
            skillType: SkillType.Heal, aoe: AreaOfEffect.Myself,
            numberOfAttacks: 1, delay: 0.2f, debuffs: new List<Debuff>());

        // Column Strike
        CreateSkill("ColumnStrike", "Column Strike", "Strike all enemies in a column.",
            manaCost: 3, damagePercent: 90, cooldown: 2,
            skillType: SkillType.Melee, aoe: AreaOfEffect.Column,
            numberOfAttacks: 1, delay: 0.2f, debuffs: new List<Debuff>());

        Debug.Log("[GameDataGenerator] 10 skills created in Assets/ScriptableObjects/Skills/");
    }

    [MenuItem("Liberator/Generate Difficulty Lists Only")]
    public static void GenerateDifficultyLists()
    {
        // We can't create full difficulty lists without enemy prefabs,
        // but we create the assets as placeholders
        CreateDifficultyList("Easy", "Assets/ScriptableObjects/Difficulty/Easy.asset");
        CreateDifficultyList("Normal", "Assets/ScriptableObjects/Difficulty/Normal.asset");
        CreateDifficultyList("Hard", "Assets/ScriptableObjects/Difficulty/Hard.asset");

        Debug.Log("[GameDataGenerator] 3 difficulty lists created (assign enemy prefabs later)");
    }

    [MenuItem("Liberator/Generate Enemy Info Only")]
    public static void GenerateEnemyInformation()
    {
        CreateEnemyInfo("Grunt", "A basic foot soldier of the Swarm. Slow but sturdy.");
        CreateEnemyInfo("Archer", "Swarm marksman. Attacks from range with high critical chance.");
        CreateEnemyInfo("Healer", "Swarm medic. Heals allies when their health drops below 50%.");
        CreateEnemyInfo("Knight", "Swarm heavy guard. High defense, unleashes special at full mana.");
        CreateEnemyInfo("Hunter", "Swift Swarm tracker. High dodge and always attacks first.");
        CreateEnemyInfo("Assassin", "Invisible Swarm killer. Strikes from the shadows every 3 turns.");
        CreateEnemyInfo("SwordMaster", "Elite Swarm warrior. Gets stronger with each passing turn.");
        CreateEnemyInfo("Boss", "Swarm Commander. Alternates between devastating attacks and specials.");
        CreateEnemyInfo("Necromancer", "Dark Swarm caster. Summons minions when allies fall.");

        Debug.Log("[GameDataGenerator] 9 enemy info assets created in Assets/ScriptableObjects/Enemies/");
    }

    // ============ HELPERS ============

    private static void AddCharacter(SerializedProperty charsProp, CharacterClass charClass, Dictionary<Attribute, float[]> stats)
    {
        int index = charsProp.arraySize;
        charsProp.InsertArrayElementAtIndex(index);
        var charProp = charsProp.GetArrayElementAtIndex(index);

        charProp.FindPropertyRelative("_character").enumValueIndex = (int)charClass;

        var statsProp = charProp.FindPropertyRelative("_stats");
        statsProp.ClearArray();

        int statIndex = 0;
        foreach (var kvp in stats)
        {
            statsProp.InsertArrayElementAtIndex(statIndex);
            var statProp = statsProp.GetArrayElementAtIndex(statIndex);

            statProp.FindPropertyRelative("_stat").enumValueIndex = (int)kvp.Key;

            var valuesProp = statProp.FindPropertyRelative("_valuesByLevel");
            valuesProp.ClearArray();
            for (int i = 0; i < kvp.Value.Length; i++)
            {
                valuesProp.InsertArrayElementAtIndex(i);
                valuesProp.GetArrayElementAtIndex(i).floatValue = kvp.Value[i];
            }

            statIndex++;
        }
    }

    private static void CreateSkill(string fileName, string skillName, string description,
        int manaCost, int damagePercent, int cooldown,
        SkillType skillType, AreaOfEffect aoe,
        int numberOfAttacks, float delay, List<Debuff> debuffs)
    {
        string path = $"Assets/ScriptableObjects/Skills/{fileName}.asset";

        var skill = ScriptableObject.CreateInstance<Skill>();
        var so = new SerializedObject(skill);

        so.FindProperty("_name").stringValue = skillName;
        so.FindProperty("_description").stringValue = description;
        so.FindProperty("_manaCost").intValue = manaCost;
        so.FindProperty("_damagePercentage").intValue = damagePercent;
        so.FindProperty("_cooldown").intValue = cooldown;
        so.FindProperty("_skillType").enumValueIndex = (int)skillType;
        so.FindProperty("_areaOfEffect").enumValueIndex = (int)aoe;
        so.FindProperty("_numberOfAttacks").intValue = numberOfAttacks;
        so.FindProperty("_delay").floatValue = delay;

        var debuffsProp = so.FindProperty("_activeDebuffs");
        debuffsProp.ClearArray();
        for (int i = 0; i < debuffs.Count; i++)
        {
            debuffsProp.InsertArrayElementAtIndex(i);
            debuffsProp.GetArrayElementAtIndex(i).enumValueIndex = (int)debuffs[i];
        }

        so.ApplyModifiedProperties();

        var existing = AssetDatabase.LoadAssetAtPath<Skill>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(skill, existing);
            EditorUtility.SetDirty(existing);
        }
        else
        {
            AssetDatabase.CreateAsset(skill, path);
        }
    }

    private static void CreateDifficultyList(string name, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<DifficultyList>(path);
        if (existing != null) return;

        var list = ScriptableObject.CreateInstance<DifficultyList>();
        AssetDatabase.CreateAsset(list, path);
    }

    private static void CreateEnemyInfo(string enemyName, string description)
    {
        string path = $"Assets/ScriptableObjects/Enemies/{enemyName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Information>(path);
        if (existing != null) return;

        var info = ScriptableObject.CreateInstance<Information>();
        info.detailInformation = description;
        AssetDatabase.CreateAsset(info, path);
    }
}
