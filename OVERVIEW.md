Project: stack - Implementation Specification

1. プロジェクト概要

stack は、Windows標準の Sticky Notes の代替となる、極限までミニマリズムを追求した付箋アプリケーションです。

哲学: 「メモ帳が忘れた精神」の具現化。メニューを排し、すべての操作をキーボード（ホットキー）で完結させる。

構造: LIFO（後入れ先出し）のスタック構造。メモを「積む（保存）」か「捨てる」か「戻す」かを選択するワークフロー。

2. 技術スタック

Runtime: .NET 10 (Preview/Latest)

Framework: WPF

Language: C# 14

Optimization: Native AOT 互換。リフレクションを避け、System.Text.Json ソース生成を使用すること。

Styling: ThemeMode="System" を適用し、OS設定（ライト/ダーク）と完全同期させる。

3. UI/UX 仕様

Window Size: デフォルト 640 × 400。

Title Bar: OS標準（DWM描画）のタイトルバーを維持（最小化・最大化・閉じる）。

Editor Area:

画面いっぱいの TextBox 1つのみで構成。

BorderThickness="0", Padding="15", FontSize="16", SpellCheck.IsEnabled="False"。

フォント: Segoe UI, 游ゴシック UI。

Focus: ウィンドウ生成時、および復元（Pop）時は即座に TextBox にフォーカスを当てる。

4. メニューバー

メニューバーは通常非表示。Alt キー押下でトグル表示する。Escape キーで閉じる。

5. コマンド・ショートカットマップ

File

New (n) / Ctrl+N: 新規ウィンドウ生成

Push (w) / Ctrl+W または 閉じる(X)ボタン または Alt+F4: 状態を保存（Stashed）してウィンドウを閉じる。

Pop (r) / Ctrl+R: スタック（LIFO）の最上部にある保存済みメモを1つ、保存した当時の座標・サイズで復元。

Discard (q) / Ctrl+Q: 保存せず、このウィンドウの情報を完全に破棄して閉じる。

Edit

All (a) / Ctrl+A: 全選択

Cut (x) / Ctrl+X: 切り取り

Copy (c) / Ctrl+C: コピー

Paste (v) / Ctrl+V: 貼り付け

Undo (z) / Ctrl+Z: 元に戻す

Redo (y) / Ctrl+Y: やり直し

View

Wrap (j) / Ctrl+J: テキスト折り返しの切り替え

Minimize (h) / Ctrl+H: ウィンドウ最小化

Maximize (m) / Ctrl+M: ウィンドウ最大化

List

保存されている（Stashedな）ウィンドウの一覧を表示する。
フォーマット: 「番号 - 内容先頭10文字」
クリックするとそのメモを開く（復元する）。

Help

Readme (r): stackの使い方が記された新しいウィンドウを開く。

6. データの永続化 (Session Management)

Storage: アプリケーションの実行ファイル（.exe）と同じディレクトリ の session.json。

Data Model:

Guid Id: 一意識別子

string Text: メモ内容

double X, Y, Width, Height: 位置とサイズ

bool IsStashed: Ctrl+W で閉じられたかどうかのフラグ

DateTime LastAccessed: LIFO順序制御用のタイムスタンプ

Behavior:

自動保存タイミング:

ウィンドウからフォーカスが離れたとき（LostFocus）。

メモをPush（Ctrl+W、Alt+F4 または Xボタン）したとき。

Ctrl + Q 以外で閉じられたメモはすべて保存対象。

アプリ起動時、IsStashed == false（前回終了時に開いていた）メモをすべて元の座標で復元する。

全ウィンドウが閉じたらアプリを終了する。

7. 実装上の注意

Native AOT: dynamic や Reflection.Emit を使用しないこと。

InputBindings: メニューの有無に関わらず、ホットキーが常に優先して動作するように Window.InputBindings を定義すること。

Multi-Window: Application.Current.Windows を適切に管理し、スタック操作が全ウィンドウで同期されるようにすること。