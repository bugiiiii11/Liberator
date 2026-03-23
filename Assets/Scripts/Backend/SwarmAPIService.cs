using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Liberator.Backend
{
    /// <summary>
    /// Handles all communication between the Liberator game and the Swarm Resistance backend.
    /// Loads NFT heroes/weapons, manages Meda Energy, submits combat results.
    /// </summary>
    public class SwarmAPIService : MonoBehaviour
    {
        private static SwarmAPIService _instance;
        public static SwarmAPIService Instance => _instance;

        [Header("API Configuration")]
        [SerializeField] private string _baseUrl = "https://swarm-backend-dev.up.railway.app";

        private string _walletAddress;
        private string _authToken;

        // Events for UI updates
        public event Action<EnergyData> OnEnergyLoaded;
        public event Action<CombatResult> OnCombatResultSubmitted;
        public event Action<string> OnError;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Initialize with wallet address from React frontend (via WebGL bridge)
        /// </summary>
        public void Initialize(string walletAddress, string authToken, string baseUrl = null)
        {
            _walletAddress = walletAddress;
            _authToken = authToken;
            if (!string.IsNullOrEmpty(baseUrl))
                _baseUrl = baseUrl;
        }

        // ============ ENERGY SYSTEM ============

        /// <summary>
        /// Get player's current Meda Energy and fight energy
        /// </summary>
        public void GetEnergy(Action<EnergyData> callback = null)
        {
            StartCoroutine(GetRequest($"/api/energy/balance/{_walletAddress}", (json) =>
            {
                var data = JsonUtility.FromJson<EnergyData>(json);
                callback?.Invoke(data);
                OnEnergyLoaded?.Invoke(data);
            }));
        }

        /// <summary>
        /// Get fight energy (pips available for dungeon runs)
        /// </summary>
        public void GetFightEnergy(Action<FightEnergyData> callback = null)
        {
            StartCoroutine(GetRequest($"/api/energy/fight-energy/{_walletAddress}", (json) =>
            {
                var data = JsonUtility.FromJson<FightEnergyData>(json);
                callback?.Invoke(data);
            }));
        }

        /// <summary>
        /// Spend 1 fight energy to start a dungeon run
        /// </summary>
        public void SpendFightEnergy(Action<SpendResult> callback = null)
        {
            var body = new SpendEnergyRequest
            {
                wallet_address = _walletAddress,
                amount = 1,
                reason = "dungeon_run"
            };

            StartCoroutine(PostRequest("/api/energy/spend", JsonUtility.ToJson(body), (json) =>
            {
                var result = JsonUtility.FromJson<SpendResult>(json);
                callback?.Invoke(result);
            }));
        }

        // ============ COMBAT RESULTS ============

        /// <summary>
        /// Submit combat result to backend (with score for XP/rewards)
        /// </summary>
        public void SubmitCombatResult(int sectorId, bool victory, int score, int enemiesDefeated, Action<CombatResult> callback = null)
        {
            var body = new CombatResultRequest
            {
                wallet_address = _walletAddress,
                sector_id = sectorId,
                victory = victory,
                score = score,
                enemies_defeated = enemiesDefeated
            };

            StartCoroutine(PostRequest("/api/liberation/contribute", JsonUtility.ToJson(body), (json) =>
            {
                var result = JsonUtility.FromJson<CombatResult>(json);
                callback?.Invoke(result);
                OnCombatResultSubmitted?.Invoke(result);
            }));
        }

        // ============ SECTOR DATA ============

        /// <summary>
        /// Get sector details (enemies, difficulty, rewards) before starting combat
        /// </summary>
        public void GetSectorDetail(int sectorId, Action<SectorData> callback = null)
        {
            StartCoroutine(GetRequest($"/api/liberation/sector/{sectorId}", (json) =>
            {
                var data = JsonUtility.FromJson<SectorData>(json);
                callback?.Invoke(data);
            }));
        }

        // ============ NFT DATA ============

        /// <summary>
        /// Get player's NFT heroes for combat
        /// </summary>
        public void GetPlayerHeroes(Action<HeroListData> callback = null)
        {
            StartCoroutine(GetRequest($"/api/nfts/heroes/{_walletAddress}", (json) =>
            {
                var data = JsonUtility.FromJson<HeroListData>(json);
                callback?.Invoke(data);
            }));
        }

        /// <summary>
        /// Get player's NFT weapons for combat
        /// </summary>
        public void GetPlayerWeapons(Action<WeaponListData> callback = null)
        {
            StartCoroutine(GetRequest($"/api/nfts/weapons/{_walletAddress}", (json) =>
            {
                var data = JsonUtility.FromJson<WeaponListData>(json);
                callback?.Invoke(data);
            }));
        }

        // ============ HTTP HELPERS ============

        private IEnumerator GetRequest(string endpoint, Action<string> onSuccess)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(_baseUrl + endpoint))
            {
                if (!string.IsNullOrEmpty(_authToken))
                    request.SetRequestHeader("Authorization", "Bearer " + _authToken);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                    onSuccess?.Invoke(request.downloadHandler.text);
                else
                {
                    Debug.LogError($"[SwarmAPI] GET {endpoint} failed: {request.error}");
                    OnError?.Invoke(request.error);
                }
            }
        }

        private IEnumerator PostRequest(string endpoint, string jsonBody, Action<string> onSuccess)
        {
            using (UnityWebRequest request = new UnityWebRequest(_baseUrl + endpoint, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(_authToken))
                    request.SetRequestHeader("Authorization", "Bearer " + _authToken);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                    onSuccess?.Invoke(request.downloadHandler.text);
                else
                {
                    Debug.LogError($"[SwarmAPI] POST {endpoint} failed: {request.error}");
                    OnError?.Invoke(request.error);
                }
            }
        }
    }

    // ============ DATA MODELS ============

    [Serializable]
    public class PlayerData
    {
        public string wallet_address;
        public string codename;
        public int level;
        public float total_xp;
        public float meda_energy;
    }

    [Serializable]
    public class EnergyData
    {
        public float meda_energy;
        public float lifetime_meda_energy;
    }

    [Serializable]
    public class FightEnergyData
    {
        public int current_pips;
        public int max_pips;
        public string next_regen_at;
    }

    [Serializable]
    public class SpendEnergyRequest
    {
        public string wallet_address;
        public int amount;
        public string reason;
    }

    [Serializable]
    public class SpendResult
    {
        public bool success;
        public float remaining_energy;
    }

    [Serializable]
    public class CombatResultRequest
    {
        public string wallet_address;
        public int sector_id;
        public bool victory;
        public int score;
        public int enemies_defeated;
    }

    [Serializable]
    public class CombatResult
    {
        public bool success;
        public float xp_earned;
        public float energy_earned;
        public float liberation_points;
    }

    [Serializable]
    public class SectorData
    {
        public int id;
        public string name;
        public int difficulty;
        public int threat_level;
        public float liberation_percent;
    }

    [Serializable]
    public class HeroListData
    {
        public HeroData[] heroes;
    }

    [Serializable]
    public class HeroData
    {
        public string token_id;
        public string name;
        public int level;
        public float health;
        public float damage;
        public float defense;
        public float crit_chance;
        public float crit_damage;
        public float dodge_chance;
        public float initiative;
    }

    [Serializable]
    public class WeaponListData
    {
        public WeaponData[] weapons;
    }

    [Serializable]
    public class WeaponData
    {
        public string token_id;
        public string name;
        public int tier;
        public float damage;
        public float crit_chance;
        public float crit_damage;
        public string weapon_type;
    }
}
