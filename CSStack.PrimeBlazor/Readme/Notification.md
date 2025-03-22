# 概要
NotificationはHTML/CSSの機能のみでは実装が難しいため、C#やJavaScriptで制御を行う必要があります。PrimeBlazorでは通知の見た目の部分にはほとんど干渉せず、通知の表示・非表示部分のみを提供します。

# PrimeNotificationService
基本的にはPrimeNotificationServiceで通知の表示に必要な情報を管理します。PrimeNotificationServiceは抽象クラスとなっており、継承して使用することを前提としています。これはPrimeNotificationServiceが最低限の機能しか備えていないためです。
通知サービスで扱う要素は、大きく「背景」と「通知」に分けられます。
背景は基本的にただのdivタグに対して外部から様々なパラメータを渡すことを想定して設計されています。
通知の本体は`PrimeNotificationContext`クラスで情報を管理し、通知サービスの`NotificationContexts`というプロパティにインスタンスを保存します。
`NotificationContexts`は`ObservableCollection`型で外部にも公開されています。ただしsetを行ってインスタンスを挿げ替えることはできません。
なお、PrimeNotificationServiceクラスはタイマーを使用して定期的に各通知のDurationとIsAutoCloseを確認し、表示期間が過ぎているものをリストから削除します。タイマーをサービス側で動かすかどうかはコンストラクタの引数（useTimer）で切り替えをおこなうことができます。
useTimerをfalseにして外部のタイマーから制御する場合は`CloseTimeoutNotifications`というメソッドを呼び出すことで表示期間を過ぎた通知を削除することができます。
useTimerをtrueにした場合、デフォルトでは100msごとにチェックを行いますが、このチェックスパンをコンストラクタの引数（autoCloseCheckSpan）で制御することができます。

## 通知の表示
通知の表示には`Notify`メソッドを使用します。`PrimeNotificationService`には`NotificationContext`のインスタンスを受け取って`NotificationContexts`に追加をする機能しか備わっていません。
外部からの利用を便利にしたい場合、以下のように継承先のクラスで実装を行うと良いでしょう。

```csharp
public sealed class ExampleNotificationService : PrimeNotificationService
{
    public ExampleNotificationService()
        : base(
        new Dictionary<string, object>())
    {
    }

    public void Notify(string summary, string detail)
    {
        var latestContext = NotificationContexts.MaxBy(x => x.Index);
        var index = latestContext?.Index + 1 ?? 0;
        Notify(
            new PrimeNotificationContext()
            {
                ComponentType = typeof(BootstrapToast),
                IsAutoClose = true,
                Identifier = Guid.NewGuid().ToString(),
                Index = index,
                Parameters =
                    new Dictionary<string, object>()
                        {
                    { "Summary", summary },
                    { "Detail", detail }
                        },
                StartViewTimeStamp = DateTime.Now,
                Duration = 4000,
            });
    }
}
```

IndexやIdentifierをあえてライブラリで制御していないのは柔軟性を上げるためです。
ただ、MaxByで取得したり最新のIndexを取得するのはハードルが高い可能性もあるので、今後簡単に取得できるメソッドの実装なども考えています。

## 背景の制御
`NotificationContexts`に1つ以上の要素がある場合は、背景のdivのCSSクラスに`show`というクラスが追加されます。また、中身が1つもなくなったら`hidden`というクラスが追加されます。
これはデフォルト値でそうなっているだけで、もしほかのUIライブラリと併用する際に都合が悪い場合は`PrimeNotificationService`のコンストラクタでCSSクラス名をそれぞれカスタマイズすることが可能です。
背景を制御してダイアログの表示・非表示を切り替える場合は以下のように実装すると良いでしょう。

まずコンストラクタで背景のCSSクラスを指定します。ここで必要に応じてshow、hiddenのときのCSSクラスも指定可能です。

```csharp
public sealed class NotificationService : PrimeNotificationService
{
    public NotificationService()
        : base(
        new Dictionary<string, object>()
        {
        { "class", "notification-background" }
        })
    {
    }
```

CSSには以下のように記述することで、最低限の制御が可能です。
DialogServiceと併用する場合、z-indexはDialogより上に設定することをお勧めします。
```css
.notification-background {
    position: fixed;
    z-index: 1001;
    overflow: hidden;
    right: 10px;
    top: 10px;

    &.show {
        display: block;
    }

    &.hidden {
        display: none;
    }
}
```

# PrimeNotificationComponent
`PrimeNotificationService`をパラメータとして受け取って実際に表示を行うrazorコンポーネントです。
`PrimeNotificationService`をDIコンテナではなくパラメータで受け取っているのは、`PrimeNotificationService`が継承される前提のため、DIコンテナに追加を忘れるようなケースを避けて複雑度を下げるためです。
基本的にこのコンポーネントをラップしたりする必要はありません。`PrimeNotificationService`内の`NotificationContexts`の変更通知を受けてコンポーネントの再描画などの処理を行ってくれます。
注意点として、Interactiveなコンポーネント内でしか使用できないので、App.razor内などに配置する際は必ずrendermodeでInteractiveなモードを指定してください。
このコンポーネントはアプリ内の全ページで同じインスタンスを使いまわす想定なので、できるだけApp.razorやRoutes.razor内に配置してください。
以下の例ではアプリケーション側で`NotificationService`をそれぞれInjectしていますが、実際はコンポーネントのラッパーとなるrazorコンポーネントを作成して、そこで必要なサービスのインスタンスをDIコンテナから受け取るのがおすすめです。

### Routesなどrendermodeが確定しているであろうコンポーネントに配置する場合
```razor:Routes.razor
@* 追加 *@
<PrimeNotificationComponent NotificationService="NotificationService" />
<Router AppAssembly="typeof(Program).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>

@code 
{
    [Inject]
    public required ExampleNotificationService NotificationService { get; set; }
}
```

### Appなどrendermodeが確定していないであろうコンポーネントに配置する場合
```razor:App.razor
@* ～中略～ *@
<body>
    @* 追加 *@
    @* rendermodeにInteractiveAuto、InteractiveServer、InteractiveWebAssemblyのいずれかを指定する *@
    <PrimeNotificationComponent NotificationService="NotificationService"  @rendermode="InteractiveWebAssembly"/>
    <Routes @rendermode="InteractiveWebAssembly" />
    <script src="_framework/blazor.web.js"></script>
</body>

@code 
{
    [Inject]
    public required ExampleNotificationService NotificationService { get; set; }
}
```