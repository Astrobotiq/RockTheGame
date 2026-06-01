using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace New_Scripts.KinematicActionSystem.Core
{
    /// <summary>
    /// Kinematik eylem sekansını yöneten ve oynatan MonoBehaviour bileşeni.
    /// Düğüm listesindeki eylemleri asenkron olarak, tanımlı başlangıç sürelerine göre yürütür.
    /// </summary>
    [DefaultExecutionOrder(-95)]
    public class KinematicActionSystemRunner : MonoBehaviour
    {
        [SerializeReference] private List<ActionNode> actions = new List<ActionNode>();
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool loop = true;

        private IKinematicSolver _solver;
        private CancellationTokenSource _cts;
        private bool _isPlaying;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
        private Vector3 _originalScale;

        public List<ActionNode> Actions => actions;
        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            TryGetComponent(out _solver);
            if (_solver != null)
            {
                _solver.Initialize(gameObject);
            }
            
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
            _originalScale = transform.localScale;
        }

        private void Start()
        {
            if (playOnStart)
            {
                StartSequence().Forget();
            }
        }

        public async UniTaskVoid StartSequence()
        {
            StopSequence();
            _cts = new CancellationTokenSource();
            _isPlaying = true;

            try
            {
                do
                {
                    await RunTimelineAsync(_cts.Token);
                    
                    // Döngü aralarında transformu başlangıç durumuna çekelim
                    if (loop && !_cts.IsCancellationRequested)
                    {
                        ResetTransformToOriginal();
                    }
                } while (loop && !_cts.IsCancellationRequested);
            }
            catch (System.OperationCanceledException)
            {
                // İptal edildi
            }
            finally
            {
                _isPlaying = false;
            }
        }

        public void StopSequence()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            _isPlaying = false;
            if (_solver != null)
            {
                _solver.ResetSolver();
            }
        }

        private void ResetTransformToOriginal()
        {
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
            transform.localScale = _originalScale;
            if (_solver != null)
            {
                _solver.ResetSolver();
            }
        }

        private async UniTask RunTimelineAsync(CancellationToken token)
        {
            List<UniTask> tasks = new List<UniTask>();
            foreach (var action in actions)
            {
                if (action != null && action.isEnabled)
                {
                    tasks.Add(RunDelayedAction(action, token));
                }
            }
            await UniTask.WhenAll(tasks);
        }

        private async UniTask RunDelayedAction(ActionNode action, CancellationToken token)
        {
            if (action.startTime > 0f)
            {
                // Milisaniye cinsinden bekleme
                await UniTask.Delay(System.TimeSpan.FromSeconds(action.startTime), cancellationToken: token);
            }
            await action.ExecuteAsync(transform, _solver, token);
        }

        private void OnDestroy()
        {
            StopSequence();
        }
    }
}
