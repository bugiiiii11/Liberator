using System.Runtime.InteropServices;
using UnityEngine;

namespace Liberator.Backend
{
    /// <summary>
    /// Bridge between React frontend and Unity WebGL.
    /// React calls SendMessage("WebGLBridge", "MethodName", "jsonData")
    /// Unity calls JavaScript functions via DllImport.
    /// </summary>
    public class WebGLBridge : MonoBehaviour
    {
        private static WebGLBridge _instance;
        public static WebGLBridge Instance => _instance;

        // JavaScript functions we can call from Unity
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SendCombatResult(string jsonResult);

        [DllImport("__Internal")]
        private static extern void SendGameReady();

        [DllImport("__Internal")]
        private static extern void SendGameError(string errorMessage);

        [DllImport("__Internal")]
        private static extern void RequestGameClose();
#else
        // Editor stubs for testing
        private static void SendCombatResult(string jsonResult) => Debug.Log($"[WebGLBridge] CombatResult: {jsonResult}");
        private static void SendGameReady() => Debug.Log("[WebGLBridge] GameReady sent");
        private static void SendGameError(string errorMessage) => Debug.Log($"[WebGLBridge] Error: {errorMessage}");
        private static void RequestGameClose() => Debug.Log("[WebGLBridge] Close requested");
#endif

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

        private void Start()
        {
            // Notify React that the game is loaded and ready
            NotifyGameReady();
        }

        // ============ CALLED BY REACT (via SendMessage) ============

        /// <summary>
        /// React sends player context when launching combat.
        /// Called via: unityInstance.SendMessage("WebGLBridge", "InitializeGame", jsonString)
        /// </summary>
        public void InitializeGame(string jsonData)
        {
            var config = JsonUtility.FromJson<GameInitConfig>(jsonData);
            Debug.Log($"[WebGLBridge] InitializeGame: wallet={config.wallet_address}, sector={config.sector_id}");

            // Initialize the API service with player credentials
            SwarmAPIService.Instance.Initialize(config.wallet_address, config.auth_token, config.api_base_url);

            // Store sector context for combat
            GameContext.SectorId = config.sector_id;
            GameContext.Difficulty = config.difficulty;
            GameContext.WalletAddress = config.wallet_address;

            // Load player data and start combat
            SwarmAPIService.Instance.SpendFightEnergy((result) =>
            {
                if (result.success)
                {
                    Debug.Log("[WebGLBridge] Fight energy spent, loading combat...");
                    // TODO: Load combat scene with sector-appropriate enemies
                }
                else
                {
                    NotifyError("Not enough fight energy");
                }
            });
        }

        /// <summary>
        /// React can request the game to pause/resume
        /// </summary>
        public void SetPaused(string paused)
        {
            Time.timeScale = paused == "true" ? 0f : 1f;
        }

        // ============ CALLED BY UNITY (sends to React) ============

        /// <summary>
        /// Tell React the game is loaded and ready to receive commands
        /// </summary>
        public void NotifyGameReady()
        {
            SendGameReady();
        }

        /// <summary>
        /// Send combat results back to React for display
        /// </summary>
        public void NotifyCombatComplete(bool victory, int score, int enemiesDefeated, float xpEarned, float energyEarned)
        {
            var result = new CombatResultMessage
            {
                victory = victory,
                score = score,
                enemies_defeated = enemiesDefeated,
                xp_earned = xpEarned,
                energy_earned = energyEarned
            };
            SendCombatResult(JsonUtility.ToJson(result));
        }

        /// <summary>
        /// Notify React of an error
        /// </summary>
        public void NotifyError(string message)
        {
            SendGameError(message);
        }

        /// <summary>
        /// Request React to close/hide the game iframe
        /// </summary>
        public void NotifyCloseGame()
        {
            RequestGameClose();
        }
    }

    // ============ DATA MODELS ============

    [System.Serializable]
    public class GameInitConfig
    {
        public string wallet_address;
        public string auth_token;
        public string api_base_url;
        public int sector_id;
        public int difficulty;
    }

    [System.Serializable]
    public class CombatResultMessage
    {
        public bool victory;
        public int score;
        public int enemies_defeated;
        public float xp_earned;
        public float energy_earned;
    }

    /// <summary>
    /// Static context for the current game session
    /// </summary>
    public static class GameContext
    {
        public static string WalletAddress { get; set; }
        public static int SectorId { get; set; }
        public static int Difficulty { get; set; }
    }
}
