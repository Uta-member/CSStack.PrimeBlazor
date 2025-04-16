using System.Collections.Immutable;
using System.Text;

namespace CSStack.PrimeBlazor
{
    /// <summary>
    /// CSSクラス管理クラス
    /// </summary>
    public sealed class CssClass
    {
        /// <summary>
        /// CSSクラスのリスト
        /// </summary>
        private ImmutableList<string> _classes = ImmutableList<string>.Empty;

        /// <summary>
        /// 空のCSSクラスを作成します。
        /// </summary>
        public CssClass()
        {
            _classes = ImmutableList<string>.Empty;
        }

        /// <summary>
        /// 既に存在するCSSクラスのリストをもとにインスタンスを作成します。
        /// </summary>
        /// <param name="classes"></param>
        public CssClass(IEnumerable<string> classes)
        {
            _classes = TrimCssClasses(classes);
        }

        /// <summary>
        /// CSSクラスをもとにインスタンスを作成します。
        /// </summary>
        /// <param name="cssClass"></param>
        public CssClass(CssClass cssClass)
        {
            _classes = TrimCssClasses(cssClass.Classes);
        }

        /// <summary>
        /// CSSクラスのリストを取得します。
        /// </summary>
        public IReadOnlyCollection<string> Classes => _classes.AsReadOnly();

        /// <summary>
        /// CSSクラスを追加します。インスタンスに破壊的な変更を加えません。
        /// </summary>
        /// <param name="cssClass"></param>
        /// <returns></returns>
        public CssClass AddCssClass(string cssClass)
        {
            if (string.IsNullOrWhiteSpace(cssClass))
            {
                return new CssClass(_classes);
            }

            var trimCssClass = TrimCssClasses(_classes.Add(cssClass));

            return new CssClass(trimCssClass);
        }

        /// <summary>
        /// CSSクラスを削除します。インスタンスに破壊的な変更を加えません。
        /// </summary>
        /// <param name="cssClass"></param>
        /// <returns></returns>
        public CssClass RemoveCssClass(string cssClass)
        {
            if (string.IsNullOrWhiteSpace(cssClass))
            {
                return new CssClass(_classes);
            }
            var trimCssClass = TrimCssClasses(_classes.Remove(cssClass));
            return new CssClass(trimCssClass);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var cssClass in _classes)
            {
                sb.Append($"{cssClass} ");
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// 文字をトリムし、重複を削除します。
        /// </summary>
        /// <param name="classes"></param>
        /// <returns></returns>
        private static ImmutableList<string> TrimCssClasses(IEnumerable<string> classes)
        {
            return classes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToImmutableList();
        }
    }
}
