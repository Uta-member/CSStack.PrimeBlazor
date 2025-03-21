using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CSStack.PrimeBlazor
{
    /// <summary>
    /// ダイアログ表示制御コンポーネント
    /// </summary>
    public partial class PrimeDialogComponent : IDisposable
    {
        private void Update(object? sender, NotifyCollectionChangedEventArgs args)
        {
            InvokeAsync(StateHasChanged);
        }

        private ReadOnlyCollection<PrimeDialogContext> _dialogContexts => DialogService.DialogContexts
            .OrderBy(x => x.Index)
            .ToList()
            .AsReadOnly();

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            DialogService.DialogContexts.CollectionChanged += Update;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DialogService.DialogContexts.CollectionChanged -= Update;
        }

        /// <summary>
        /// ダイアログサービス
        /// </summary>
        [Parameter]
        [EditorRequired]
        public required PrimeDialogService DialogService { get; set; }
    }
}
