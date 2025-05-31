using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dq9BattleEmulatorTestAgent
{
    internal static class Program
    {
        private static Form1 form;
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) // Windows 7 (6.1)以降かをチェック
            {
                Environment.Exit(1);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(form = new Form1());
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            form?.OnExit();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                // 例外オブジェクトを取得
                Exception exception = e.ExceptionObject as Exception;

                // エラーメッセージウィンドウを表示
                string message = $"未処理の例外が発生しました。\n\n詳細:\n{exception?.Message ?? "不明なエラー"}";
                string stackTrace = exception?.StackTrace ?? "スタックトレースはありません。";

                // ログファイルにエラーを書き込む
                File.WriteAllText("error_log.txt", $"{DateTime.Now}: {message}\n\n{stackTrace}");

                // エラーダイアログを表示
                MessageBox.Show(
                    $"{message}\n\nスタックトレース:\n{stackTrace}",
                    "アプリケーション エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                // エラーハンドラ内で別の例外が発生した場合の処理
                File.WriteAllText("critical_error_log.txt", $"例外ハンドラ内エラー: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                form?.OnExit();
                // アプリケーションを安全に終了
                Environment.Exit(1);
            }
        }
    }
}
