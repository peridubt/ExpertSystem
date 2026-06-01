// Готовый компонент интеграции. Вешается на агента, связывается с поставщиком снимка
// и потребителем решения и крутит цикл "ощущение → вывод → действие" по интервалу.
// Движок общий для всех мозгов (ExpertSystemRuntime), сеть компилируется один раз.
using ExpertSystem.RuleEngine.Core.Application;
using ExpertSystem.RuleEngine.Core.Rules;
using ExpertSystem.RuleEngine.Core.Runtime;
using UnityEngine;

namespace ExpertSystem.Integration
{
    /// <summary>Компонент-"мозг": связывает игру с экспертной системой.</summary>
    [AddComponentMenu("Expert System/Expert System Brain")]
    public class ExpertSystemBrain : MonoBehaviour
    {
        /// <summary>Режим работы мозга.</summary>
        public enum EvaluationMode
        {
            DecisionSupport, // только рекомендация, факты не меняются
            StateEvolution,  // плюс запись мутаций фактов обратно в игру
        }

        [Header("Вывод")]
        [Tooltip("DecisionSupport: только рекомендация.\n" +
                 "StateEvolution: ещё и запись изменённых фактов обратно через OnStateEvolved.")]
        [SerializeField] private EvaluationMode _mode = EvaluationMode.DecisionSupport;

        [Tooltip("Секунды между автоматическими вычислениями. 0 = только вручную (вызов Evaluate()).")]
        [SerializeField, Min(0f)] private float _evaluateInterval = 0.5f;

        [Tooltip("Выполнить один вывод в Start().")]
        [SerializeField] private bool _evaluateOnStart = true;

        [Header("Привязки (необязательно — иначе ищутся на этом GameObject)")]
        [Tooltip("Компонент, реализующий ICombatSnapshotProvider (сторона ощущения).")]
        [SerializeField] private MonoBehaviour _snapshotProvider;

        [Tooltip("Компонент, реализующий IDecisionConsumer (сторона действия).")]
        [SerializeField] private MonoBehaviour _decisionConsumer;

        private ICombatSnapshotProvider _provider;
        private IDecisionConsumer _consumer;
        private RuleEvaluationService _service;
        private float _timer;

        /// <summary>Последнее принятое решение (для отладки/интерфейса).</summary>
        public DecisionResult LastDecision { get; private set; }

        /// <summary>Текущий режим работы.</summary>
        public EvaluationMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        /// <summary>Разрешает привязки и строит сервис на общем движке. Отключает компонент,
        /// если поставщик снимка не найден.</summary>
        private void Awake()
        {
            _provider = Resolve<ICombatSnapshotProvider>(_snapshotProvider);
            _consumer = Resolve<IDecisionConsumer>(_decisionConsumer);

            if (_provider == null)
            {
                Debug.LogError(
                    $"[ExpertSystemBrain] На '{name}' не найден ICombatSnapshotProvider. " +
                    "Назначьте его в инспекторе или добавьте компонент. Отключаюсь.", this);
                enabled = false;
                return;
            }

            // Если потребитель реализует INotificationService — он получит оповещения правил,
            // иначе оповещения отбрасываются.
            var notification = _consumer as INotificationService ?? new NullNotificationService();
            _service = new RuleEvaluationService(ExpertSystemRuntime.Engine, notification);
        }

        /// <summary>Запускает первый вывод, если включено.</summary>
        private void Start()
        {
            if (_evaluateOnStart) Evaluate();
        }

        /// <summary>Тикает вывод по интервалу; при интервале 0 — ручной режим.</summary>
        private void Update()
        {
            if (_evaluateInterval <= 0f) return; // ручной режим
            _timer += Time.deltaTime;
            if (_timer < _evaluateInterval) return;
            _timer = 0f;
            Evaluate();
        }

        /// <summary>
        /// Один полный цикл: ощущение → вывод → действие. Можно вызывать вручную (по событию)
        /// независимо от интервала. Ничего не делает, если нет снимка.
        /// </summary>
        public void Evaluate()
        {
            if (_service == null || _provider == null) return;

            var snapshot = _provider.CaptureSnapshot();
            if (snapshot == null) return;

            var outcome = _service.EvaluateDetailed(snapshot);
            LastDecision = outcome.Decision;

            _consumer?.OnDecision(outcome.Decision);

            // В режиме эволюции отдаём изменённое состояние, чтобы игра записала его у себя.
            if (_mode == EvaluationMode.StateEvolution)
            {
                _consumer?.OnStateEvolved(outcome.UpdatedSnapshot);
            }
        }

        /// <summary>Возвращает привязку: явную ссылку, если она нужного типа, иначе ищет на
        /// этом GameObject. Принимает явную ссылку, возвращает реализацию интерфейса или null.</summary>
        private T Resolve<T>(MonoBehaviour explicitRef) where T : class
        {
            if (explicitRef is T typed) return typed;
            return GetComponent<T>();
        }
    }
}
