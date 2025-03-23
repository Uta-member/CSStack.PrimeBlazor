using System.Collections.ObjectModel;

namespace CSStack.PrimeBlazor
{
    /// <summary>
    /// 通知サービス
    /// </summary>
    public abstract class PrimeNotificationService : IDisposable
    {
        /// <summary>
        /// 自動で閉じる通知のチェックスパン（ms）
        /// </summary>
        private int _autoCloseCheckSpan = 100;
        /// <summary>
        /// 外から受け取ったCSS
        /// </summary>
        private readonly string _backgroundClassName = string.Empty;

        /// <summary>
        /// タイマー
        /// </summary>
        private Timer? _timer;

        /// <summary>
        /// 排他制御用オブジェクト
        /// </summary>
        protected readonly object Lock = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="backgroundParameters">背景コンポーネントに渡すパラメータ</param>
        /// <param name="showClassName">表示時のクラス名</param>
        /// <param name="hiddenClassName">非表示時のクラス名</param>
        /// <param name="autoCloseCheckSpan">自動で閉じる通知のチェックスパン（ms）</param>
        /// <param name="useTimer">自動で通知を閉じるタイマーをサービス側で動作させるかどうか</param>
        protected PrimeNotificationService(
            Dictionary<string, object> backgroundParameters,
            string? showClassName = null,
            string? hiddenClassName = null,
            int? autoCloseCheckSpan = null,
            bool useTimer = true)
        {
            if(autoCloseCheckSpan != null)
            {
                _autoCloseCheckSpan = (int)autoCloseCheckSpan;
            }

            if(useTimer)
            {
                _timer = new(
                    _ => CloseTimeoutNotifications(),
                    null,
                    _autoCloseCheckSpan,
                    _autoCloseCheckSpan);
            }

            if(!string.IsNullOrWhiteSpace(showClassName))
            {
                ShowClassName = showClassName;
            }
            if(!string.IsNullOrWhiteSpace(hiddenClassName))
            {
                HiddenClassName = hiddenClassName;
            }

            BackgroundParameters = backgroundParameters;

            object? backgroundClassName;
            if(backgroundParameters.TryGetValue("class", out backgroundClassName))
            {
                _backgroundClassName = (string)backgroundClassName;
            }

            BackgroundParameters["class"] = _backgroundClassName;

            if(NotificationContexts.Any())
            {
                BackgroundParameters["class"] = $"{ShowClassName} {_backgroundClassName}";
            }
            else
            {
                BackgroundParameters["class"] = $"{HiddenClassName} {_backgroundClassName}";
            }
        }

        /// <summary>
        /// 表示時間を過ぎた通知を削除する
        /// </summary>
        protected void CloseTimeoutNotifications()
        {
            lock(Lock)
            {
                var targets = NotificationContexts.Where(
                    x => x.IsAutoClose && DateTime.Now > x.StartViewTimeStamp.AddMilliseconds(x.Duration))
                    .ToList();
                foreach(var target in targets)
                {
                    NotificationContexts.Remove(target);
                }
            }
        }

        /// <summary>
        /// 通知を閉じる
        /// </summary>
        /// <param name="context">閉じたい通知のコンテキスト</param>
        public void CloseNotification(PrimeNotificationContext context)
        {
            lock(Lock)
            {
                NotificationContexts.Remove(context);
                if(!NotificationContexts.Any())
                {
                    BackgroundParameters["class"] = $"{HiddenClassName} {_backgroundClassName}";
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _timer?.Dispose();
        }

        /// <summary>
        /// 通知を表示する
        /// </summary>
        /// <param name="context">開きたい通知のコンテキスト</param>
        public void Notify(PrimeNotificationContext context)
        {
            lock(Lock)
            {
                NotificationContexts.Add(context);
                if(NotificationContexts.Any())
                {
                    BackgroundParameters["class"] = $"{ShowClassName} {_backgroundClassName}";
                }
            }
        }

        /// <summary>
        /// 背景コンポーネントに渡すパラメータ
        /// </summary>
        public Dictionary<string, object> BackgroundParameters { get; set; }

        /// <summary>
        /// 非表示時のクラス名
        /// </summary>
        public string HiddenClassName { get; set; } = "hidden";

        /// <summary>
        /// ダイアログコンテキストリスト
        /// </summary>
        public ObservableCollection<PrimeNotificationContext> NotificationContexts
        {
            get;
        } = new();

        /// <summary>
        /// 表示時のクラス名
        /// </summary>
        public string ShowClassName { get; set; } = "show";
    }

    /// <summary>
    /// 通知情報
    /// </summary>
    public record PrimeNotificationContext
    {
        /// <summary>
        /// ダイアログコンポーネントの型
        /// </summary>
        public required Type ComponentType { get; set; }

        /// <summary>
        /// 表示時間（ms）
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public required string Identifier { get; set; }

        /// <summary>
        /// 表示順
        /// </summary>
        public required int Index { get; set; }

        /// <summary>
        /// 表示時間を指定して自動で閉じるかどうか
        /// </summary>
        public bool IsAutoClose { get; set; }

        /// <summary>
        /// コンポーネントに渡すパラメータ
        /// </summary>
        public required Dictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// 表示開始日時
        /// </summary>
        public required DateTime StartViewTimeStamp { get; set; }
    }
}
