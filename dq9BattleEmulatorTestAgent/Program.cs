using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dq9BattleEmulatorTestAgent
{
    internal static class Program
    {
        private static Form1 form;
        private static DesmumeInstance instance;

        private static readonly Dictionary<string, IntPtr> LuaWindowHandles = new();

        [STAThread]
        static async Task Main()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1)) // Windows 7以降
            {
                Environment.Exit(1);
                return;
            }

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            Application.ApplicationExit += (_, __) => OnProcessExit(null, null);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ROM・スロット設定
            string romPath = Path.Combine(Directory.GetCurrentDirectory(), "resource", "dq9_new.nds");
            int saveSlot = 7;

            instance = new DesmumeInstance(
                Path.Combine(Directory.GetCurrentDirectory(), "resource", "DeSmuME-VS2022-x64-Release.exe"),
                romPath,
                saveSlot,
                outputCallback: msg => Debug.WriteLine("[OUT] " + msg),
                errorCallback: msg => Debug.WriteLine("[ERR] " + msg),
                Directory.GetCurrentDirectory()
            );
            await instance.StartAsync();
            string luaScriptPath = "D:\\csharp\\dq9beta\\Ctable.lua";
            //string luaScriptPath = "D:\\csharp\\dq9beta\\dq9BattleEmulatorTestAgent\\dq9BattleEmulatorTestAgent\\resource\\keys\\keyInput_A.lua";
            IntPtr luaWindowHandle = await instance.OpenLuaConsoleAndRunScript(luaScriptPath);
            if (luaWindowHandle != IntPtr.Zero)
            {
                LuaWindowHandles[luaScriptPath] = luaWindowHandle;
                //instance.closeLuaConsole(luaWindowHandle); // すべてのLuaコンソールを閉じる
            }
            //await instance.initJoyPad();
            await instance.ToggleConsoleOutput();

            await Task.Delay(1000); // メインスレッドをブロックしないように定期的に待機

            instance.pushJoypadSleep(1000);
            instance.pushJoypadInput(Joypad.A);
            instance.pushJoypadInput(Joypad.Left);
            instance.pushJoypadInput(Joypad.A);
            //instance.pushJoypadInput(Joypad.Down);
            //instance.pushJoypadInput(Joypad.Down);
            //instance.pushJoypadInput(Joypad.A);


            //await Task.Delay(1000); // メインスレッドをブロックしないように定期的に待機
            //instance.LoadState(0);
            //instance.SaveState(0);

            //while (true)
            //{
            //    instance.TogglePause(!instance.isPaused());
            //    await Task.Delay(1000); // メインスレッドをブロックしないように定期的に待機
            //}


            // アプリケーション起動
            form = new Form1();
            Application.Run(form);
        }

        private static void OnProcessExit(object? sender, EventArgs? e)
        {
            instance.Terminate();
            form?.OnExit(); // フォームがあればクリーンアップ
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception? exception = e.ExceptionObject as Exception;
                string message = $"未処理の例外が発生しました。\n\n詳細:\n{exception?.Message ?? "不明なエラー"}";
                string stackTrace = exception?.StackTrace ?? "スタックトレースはありません。";

                File.WriteAllText("error_log.txt", $"{DateTime.Now}: {message}\n\n{stackTrace}");

                MessageBox.Show($"{message}\n\nスタックトレース:\n{stackTrace}", "アプリケーション エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                File.WriteAllText("critical_error_log.txt", $"例外ハンドラ内エラー: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                instance.Terminate();
                form?.OnExit();
                Environment.Exit(1);
            }
        }
    }
}
