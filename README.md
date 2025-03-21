# CSStack.PrimeBlazor
PrimeBlazorはBlazor向けのUIコンポーネントライブラリを作る際に便利なコンポーネントをまとめた、UIコンポーネント開発のためのライブラリです。
できる限りC#とBlazoeの仕様以外のものに依存せず、カスタマイズ性の高いプレーンなコンポーネントを目指して開発を行っています。

# モチベーション
基本的にHTML/CSSだけで簡単に実装できるようなものは実装しません。
ターゲットはダイアログや通知、ツールチップなどのようにJavaScriptなどを使わないと難しいものを実装していく予定です。
できる限りC#で実装を行い、どうしてもレンダリング後でないと処理あできないようなものだけJavaScriptを使うような方向性です。