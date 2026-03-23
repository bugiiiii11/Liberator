using System.Collections;
using UnityEngine;

namespace Liberator.Utils
{
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] private GameObject _canvas;
        [SerializeField] private CanvasGroup _cavnasGroup;
        [SerializeField] private float _fadeOutDuration = 0.5f;
        [SerializeField] private float _fadeInDuration = 0.5f;

        private Coroutine _fadeRoutine;

        public void FadeImmediate(float alpha)
        {
            _cavnasGroup.alpha = alpha;
        }

        public Coroutine FadeOut(bool disableCanvas = false)
        {
            _canvas.SetActive(true);
            TryStopRoutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeRoutine(1, _fadeOutDuration, disableCanvas));
            return _fadeRoutine;
        }

        public Coroutine FadeIn(bool disableCanvas = false)
        {
            _canvas.SetActive(true);
            TryStopRoutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeRoutine(0, _fadeInDuration, disableCanvas));
            return _fadeRoutine;
        }

        private void TryStopRoutine(Coroutine routine)
        {
            if (routine != null)
                StopCoroutine(routine);
        }

        private IEnumerator FadeRoutine(float target, float time, bool disableCanvas = false)
        {
            while (!Mathf.Approximately(_cavnasGroup.alpha, target))
            {
                float delta = Time.deltaTime / time;
                _cavnasGroup.alpha = Mathf.MoveTowards(_cavnasGroup.alpha, target, delta);
                yield return new WaitForEndOfFrame();
            }

            if (disableCanvas)
                _canvas.SetActive(false);
        }
    }
}
