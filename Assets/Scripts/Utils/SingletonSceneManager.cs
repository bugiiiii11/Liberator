using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Liberator.Utils
{
    public class SingletonSceneManager : SingletonMonoBehaviour<SingletonSceneManager>
    {
        [SerializeField] private string _mainMenuScene;
        [SerializeField] private ScreenFader _fader;

        private Coroutine _loadCoroutine;

        protected override void Awake()
        {
            DontDestroyOnLoad();
            LoadBackToMenu(false);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += FadeIn;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= FadeIn;
        }

        private void FadeIn(Scene arg0, LoadSceneMode arg1)
        {
            _fader.FadeIn(true);
        }

        public void LoadSceneByName(string sceneName)
        {
            TryStopCoroutine(_loadCoroutine);
            _loadCoroutine = StartCoroutine(LoadScene(() => { SceneManager.LoadScene(sceneName); }));
        }

        public void LoadSceneByID(int sceneID)
        {
            TryStopCoroutine(_loadCoroutine);
            _loadCoroutine = StartCoroutine(LoadScene(() => { SceneManager.LoadScene(sceneID); }));
        }

        private void TryStopCoroutine(Coroutine routine)
        {
            if (routine != null)
                StopCoroutine(routine);
        }

        private IEnumerator LoadScene(Action loadSceneAction)
        {
            yield return _fader.FadeOut();
            loadSceneAction();
        }

        public void LoadBackToMenu(bool fade)
        {
            if (fade)
            {
                TryStopCoroutine(_loadCoroutine);
                _loadCoroutine = StartCoroutine(LoadScene(() => { SceneManager.LoadScene(_mainMenuScene); }));
            }
            else
                SceneManager.LoadScene(_mainMenuScene);
        }
    }
}
