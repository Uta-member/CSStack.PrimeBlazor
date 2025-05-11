using Microsoft.AspNetCore.Components;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CSStack.PrimeBlazor
{
    /// <summary>
    /// 通知表示制御コンポーネント
    /// </summary>
    public partial class PrimeNotificationComponent : IDisposable
    {
        private void HandleNotificationStateChanged(object? sender, EventArgs args)
        {
            InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// ソート済みの通知リスト
        /// </summary>
        private ReadOnlyCollection<PrimeNotificationContext> _notificationContexts => NotificationService?.NotificationContexts.OrderBy(
                x => x.Index)
                .ToList()
                .AsReadOnly() ??
            new ReadOnlyCollection<PrimeNotificationContext>([]);

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            NotificationService.OnNotificationStateChanged += HandleNotificationStateChanged;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            NotificationService.OnNotificationStateChanged -= HandleNotificationStateChanged;
        }

        /// <summary>
        /// 通知サービス
        /// </summary>
        [Parameter]
        [EditorRequired]
        public required PrimeNotificationService NotificationService { get; set; }
    }
}
