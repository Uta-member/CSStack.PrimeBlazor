# 概要
DialogはHTML/CSSの機能のみでは実装が難しいため、C#やJavaScriptで制御を行う必要があります。PrimeBlazorではダイアログの見た目の部分にはほとんど干渉せず、ダイアログの表示・非表示部分のみを提供します。

# PrimeDialogService
基本的にはPrimeDialogServiceでダイアログの表示に必要な情報を管理します。PrimeDialogServiceは抽象クラスとなっており、継承して使用することを前提としています。これはPrimeDialogServiceが最低限の機能しか備えていないためです。
ダイアログサービスで扱う要素は、大きく「背景」と「ダイアログ」に分けられます。
背景は基本的にただのdivタグに対して外部から様々なパラメータを渡すことを想定して設計されています。
ダイアログの本体は`PrimeDialogContext`クラスで情報を管理し、ダイアログサービスの`DialogContexts`というプロパティにインスタンスを保存します。
`DialogContexts`は`ObservableCollection`型で外部にも公開されています。ただしsetを行ってインスタンスを挿げ替えることはできません。

## ダイアログの表示
ダイアログの表示には`ShowDialog`メソッドを使用します。`PrimeDialogService`には`DialogContext`のインスタンスを受け取って`DialogContexts`に追加をする機能しか備わっていません。
外部からの利用を便利にしたい場合、以下のように継承先のクラスで実装を行うと良いでしょう。

```csharp
public sealed class ExampleDialogService : PrimeDialogService 
{
    public ExampleDialogService(Dictionary<string, object> backgroundParameters) : base(backgroundParameters)
    {
    }

    public void ShowDialog<TDialogType>(Dictionary<string, object>? parameters = null, string? identifier = null)
        where TDialogType : ComponentBase
    {
        int index = DialogContexts.MaxBy(x => x.Index)?.Index + 1 ?? 0;
        ShowDialog(
            new PrimeDialogContext()
            {
                ComponentType = typeof(TDialogType),
                Identifier = identifier ?? Guid.NewGuid().ToString(),
                Index = index,
                Parameters = parameters ?? new Dictionary<string, object>()
            });
    }
}
```

## ダイアログを閉じる
ダイアログを閉じる際も、`PrimeDialogService`では`PrimeDialogContext`型の引数を渡す必要があります。これを継承先でもう少し便利に実装することも可能です。
例として以下のような実装が考えられます。
```csharp
public void CloseDialog()
{
    // 一番最新のダイアログだけを閉じる
    var latestDialog = DialogContexts.MaxBy(x => x.Index);
    if(latestDialog == null)
    {
        return;
    }
    CloseDialog(latestDialog);
}
```
Indexを適切に管理していれば一番上に表示されているものから順番に閉じていくことが可能です。IndexやIdentifierをあえてライブラリで制御していないのは柔軟性を上げるためです。
ただ、MaxByで取得したり最新のIndexを取得するのはハードルが高い可能性もあるので、今後簡単に取得できるメソッドの実装なども考えています。

## 背景の制御
`DialogContexts`に1つ以上の要素がある場合は、背景のdivのCSSクラスに`show`というクラスが追加されます。また、中身が1つもなくなったら`hidden`というクラスが追加されます。
これはデフォルト値でそうなっているだけで、もしほかのUIライブラリと併用する際に都合が悪い場合は`PrimeDialogService`のコンストラクタでCSSクラス名をそれぞれカスタマイズすることが可能です。
背景を制御してダイアログの表示・非表示を切り替える場合は以下のように実装すると良いでしょう。

まずコンストラクタで背景のCSSクラスを指定します。ここで必要に応じてshow、hiddenのときのCSSクラスも指定可能です。
```csharp:ExampleDialogService.cs
public ExampleDialogService()
        : base(
        new Dictionary<string, object>()
        {
        {
            "class",
            "dialog-background"
        },
        })
    {
    }
}
```

CSSには以下のように記述することで、最低限の制御が可能です。
```css
.dialog-background {
    position: fixed;
    background-color: rgba(0, 0, 0, 0.3);
    width: 100vw;
    height: 100vh;
    z-index: 1000;
    overflow: hidden;
    
    &.show {
        display: block;
    }

    &.hidden {
        display: none;
    }
}
```

このように書くことで、ダイアログが1つもない時は何も表示されず、1つでもあれば画面の一番上にダイアログっが表示されて下のコンポーネントを触ることは出来なくなります。
z-indexやbackgroundに関しては適宜変更してください。また、displayではなくvisibilityやopacityなどで制御する方法もありますので、システムの要件に合わせて変更してください。

# PrimeDialogComponent
`PrimeDialogService`をパラメータとして受け取って実際に表示を行うrazorコンポーネントです。
`PrimeDialogService`をDIコンテナではなくパラメータで受け取っているのは、`PrimeDialogService`が継承される前提のため、DIコンテナに追加を忘れるようなケースを避けて複雑度を下げるためです。
基本的にこのコンポーネントをラップしたりする必要はありません。`PrimeDialogService`内の`DialogContexts`の変更通知を受けてコンポーネントの再描画などの処理を行ってくれます。
注意点として、Interactiveなコンポーネント内でしか使用できないので、App.razor内などに配置する際は必ずrendermodeでInteractiveなモードを指定してください。
このコンポーネントはアプリ内の全ページで同じインスタンスを使いまわす想定なので、できるだけApp.razorやRoutes.razor内に配置してください。
以下の例ではアプリケーション側で`DialogService`をそれぞれInjectしていますが、実際はコンポーネントのラッパーとなるrazorコンポーネントを作成して、そこで必要なサービスのインスタンスをDIコンテナから受け取るのがおすすめです。

### Routesなどrendermodeが確定しているであろうコンポーネントに配置する場合
```razor:Routes.razor
@* 追加 *@
<PrimeDialogComponent DialogService="DialogService" />
<Router AppAssembly="typeof(Program).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>

@code 
{
    [Inject]
    public required ExampleDialogService DialogService { get; set; }
}
```

### Appなどrendermodeが確定していないであろうコンポーネントに配置する場合
```razor:App.razor
@* ～中略～ *@
<body>
    @* 追加 *@
    @* rendermodeにInteractiveAuto、InteractiveServer、InteractiveWebAssemblyのいずれかを指定する *@
    <PrimeDialogComponent DialogService="DialogService"  @rendermode="InteractiveWebAssembly"/>
    <Routes @rendermode="InteractiveWebAssembly" />
    <script src="_framework/blazor.web.js"></script>
</body>

@code 
{
    [Inject]
    public required ExampleDialogService DialogService { get; set; }
}
```