using System.Collections.ObjectModel;

namespace CSStack.PrimeBlazor
{
    /// <summary>
    /// ダイアログサービス
    /// </summary>
    public abstract class PrimeDialogService
    {
        /// <summary>
        /// 外から受け取ったCSS
        /// </summary>
        private readonly string _backgroundClassName = string.Empty;

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
        public PrimeDialogService(
            Dictionary<string, object> backgroundParameters,
            string? showClassName = null,
            string? hiddenClassName = null)
        {
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

            if(DialogContexts.Any())
            {
                BackgroundParameters["class"] = $"{ShowClassName} {_backgroundClassName}";
            }
            else
            {
                BackgroundParameters["class"] = $"{HiddenClassName} {_backgroundClassName}";
            }
        }

        /// <summary>
        /// ダイアログを閉じる
        /// </summary>
        /// <param name="context"></param>
        public void CloseDialog(PrimeDialogContext context)
        {
            lock(Lock)
            {
                DialogContexts.Remove(context);
                if(!DialogContexts.Any())
                {
                    BackgroundParameters["class"] = $"{HiddenClassName} {_backgroundClassName}";
                }
            }
        }

        /// <summary>
        /// ダイアログを表示する
        /// </summary>
        /// <param name="context">開きたいダイアログのコンテキスト</param>
        public void ShowDialog(PrimeDialogContext context)
        {
            lock(Lock)
            {
                DialogContexts.Add(context);
                if(DialogContexts.Any())
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
        /// ダイアログコンテキストリスト
        /// </summary>
        public ObservableCollection<PrimeDialogContext> DialogContexts
        {
            get;
        } = new ObservableCollection<PrimeDialogContext>();

        /// <summary>
        /// 非表示時のクラス名
        /// </summary>
        public string HiddenClassName { get; set; } = "hidden";

        /// <summary>
        /// 表示時のクラス名
        /// </summary>
        public string ShowClassName { get; set; } = "show";
    }

    /// <summary>
    /// ダイアログ情報
    /// </summary>
    public record PrimeDialogContext
    {
        /// <summary>
        /// ダイアログコンポーネントの型
        /// </summary>
        public required Type ComponentType { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public required string Identifier { get; set; }

        /// <summary>
        /// 表示順
        /// </summary>
        public required int Index { get; set; }

        /// <summary>
        /// コンポーネントに渡すパラメータ
        /// </summary>
        public required Dictionary<string, object> Parameters { get; set; }
    }
}
