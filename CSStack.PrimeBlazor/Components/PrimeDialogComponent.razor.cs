using Microsoft.AspNetCore.Components;
using System.Collections.ObjectModel;

namespace CSStack.PrimeBlazor
{
    /// <summary>
    /// ダイアログ表示制御コンポーネント
    /// </summary>
    public partial class PrimeDialogComponent : IDisposable
    {
        private void HandleDialogStateChanged(object? sender, EventArgs args)
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
            DialogService.OnDialogStateChanged += HandleDialogStateChanged;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DialogService.OnDialogStateChanged -= HandleDialogStateChanged;
        }

        /// <summary>
        /// ダイアログサービス
        /// </summary>
        [Parameter]
        [EditorRequired]
        public required PrimeDialogService DialogService { get; set; }
    }
}
