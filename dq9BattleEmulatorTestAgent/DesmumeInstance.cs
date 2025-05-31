using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace dq9BattleEmulatorTestAgent
{
    internal class DesmumeInstance
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetDlgItem(IntPtr hWnd, int nIDDlgItem);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendDlgItemMessage(IntPtr hDlg, int nIDDlgItem, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPTStr)] string lParam);


        public Process Process { get; private set; }
        public IntPtr WindowHandle { get; private set; } = IntPtr.Zero;

        private readonly string _exePath;
        private readonly string _romPath;
        private readonly int _saveSlot;
        private readonly Action<string>? _outputCallback;
        private readonly Action<string>? _errorCallback;
        private bool Paused = true;

        public DesmumeInstance(
            string exePath,
            string romPath,
            int saveSlot,
            Action<string>? outputCallback = null,
            Action<string>? errorCallback = null)
        {
            _exePath = exePath;
            _romPath = romPath;
            _saveSlot = saveSlot;
            _outputCallback = outputCallback;
            _errorCallback = errorCallback;
        }

        public async Task StartAsync()
        {
            if (!File.Exists(_exePath))
                throw new FileNotFoundException("DeSmuME 実行ファイルが見つかりません", _exePath);

            if (!File.Exists(_romPath))
                throw new FileNotFoundException("ROMファイルが見つかりません", _romPath);

            string args = $"--preload-rom \"{_romPath}\" --load-slot {_saveSlot} --disable-sound --frameskip 9";

            var psi = new ProcessStartInfo
            {
                FileName = _exePath,
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(_exePath) ?? ".",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
            proc.OutputDataReceived += (s, e) => { if (e.Data != null) _outputCallback?.Invoke(e.Data); };
            proc.ErrorDataReceived += (s, e) => { if (e.Data != null) _errorCallback?.Invoke(e.Data); };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            Process = proc;
            foreach (var hWnd in await WindowFinder.WaitForWindowsByRegexAsync(proc.Id, @"0\.9\.14", 1))
            {
                WindowHandle = hWnd;
                break; // 最初に見つかったウィンドウを使用
            }
            if(WindowHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("DeSmuMEのウィンドウが見つかりません。DeSmuMEが正しく起動しているか確認してください。");
            }
        }

        public void LaunchLuaWindow(int id = 0)
        {
            if (WindowHandle == IntPtr.Zero)
                throw new InvalidOperationException("DeSmuMEのウィンドウが見つかりません。先にDeSmuMEを起動してください。");

            SendMessage(WindowHandle, DeSmuMECommands.WM_COMMAND, (IntPtr)(DeSmuMECommands.IDD_LUARECENT_RESERVE_START + id), IntPtr.Zero);
        }

        public void LoadState(int id = 0)
        {
            if (WindowHandle == IntPtr.Zero)
                throw new InvalidOperationException("DeSmuMEのウィンドウが見つかりません。先にDeSmuMEを起動してください。");

            SendMessage(WindowHandle, DeSmuMECommands.WM_COMMAND, (IntPtr)DeSmuMECommands.IDM_STATE_LOAD_F10 + id, (IntPtr) id);
        }

        public void SaveState(int id = 0)
        {
            if (WindowHandle == IntPtr.Zero)
                throw new InvalidOperationException("DeSmuMEのウィンドウが見つかりません。先にDeSmuMEを起動してください。");

            SendMessage(WindowHandle, DeSmuMECommands.WM_COMMAND, (IntPtr)DeSmuMECommands.IDM_STATE_SAVE_F10 + id, (IntPtr)id);
        }
        public async Task<IntPtr> OpenLuaConsoleAndRunScript(string luaPath)
        {
            if (WindowHandle == IntPtr.Zero)
                throw new InvalidOperationException("DeSmuMEのウィンドウが見つかりません。先にDeSmuMEを起動してください。");

            SendMessage(WindowHandle, DeSmuMECommands.WM_COMMAND, (IntPtr)DeSmuMECommands.IDC_NEW_LUA_SCRIPT, (IntPtr)0);

            foreach(IntPtr WindowHandleLua in await WindowFinder.WaitForWindowsByRegexAsync(Process.Id, "Lua S", 1))
            {
                // 子コントロール（Edit）を取得
                IntPtr editBox = GetDlgItem(WindowHandleLua, DeSmuMECommands.IDC_EDIT_LUAPATH);
                if (editBox == IntPtr.Zero)
                    continue;

                // 絶対パスを設定
                SendMessage(editBox, DeSmuMECommands.WM_SETTEXT, IntPtr.Zero, luaPath);

                return WindowHandleLua; // 成功したウィンドウのハンドルを返す
            }
            return IntPtr.Zero; // Luaコンソールのウィンドウハンドルを取得するための処理は省略
        }
        public void TogglePause(bool enable)
        {
            if (WindowHandle == IntPtr.Zero)
                throw new InvalidOperationException("DeSmuMEのウィンドウが見つかりません。先にDeSmuMEを起動してください。");

            if(Paused == enable)
                return; // 既に状態が一致している場合は何もしない

            Paused = !Paused;

            SendMessage(WindowHandle, DeSmuMECommands.WM_COMMAND, (IntPtr)DeSmuMECommands.IDM_PAUSE, IntPtr.Zero);
        }

        public bool isPaused()
        {
            return Paused;
        }

        public void Terminate()
        {
            try
            {
                if (Process != null && !Process.HasExited)
                {
                    Process.Kill(entireProcessTree: true);
                    Process.Dispose();
                }
            }
            catch (Exception ex)
            {
                _errorCallback?.Invoke("終了失敗: " + ex.Message);
            }
        }

        public async Task ToggleConsoleOutput()
        {
            foreach (var hWnd in await WindowFinder.WaitForWindowsByRegexAsync(Process.Id, @".lua", 1))
            {
                ToggleCheckBox(hWnd, DeSmuMECommands.BST_CHECKED);
            }
            await Task.CompletedTask;
        }


        public static void ToggleCheckBox(IntPtr parentWindow, int Checked = DeSmuMECommands.BST_CHECKED)
        {
            IntPtr checkboxHandle = GetDlgItem(parentWindow, DeSmuMECommands.IDC_USE_STDOUT);
            if (checkboxHandle == IntPtr.Zero)
            {
                Debug.WriteLine("stdout チェックボックスが見つかりませんでした。");
                return;
            }

            SendMessage(checkboxHandle, DeSmuMECommands.BM_SETCHECK, (IntPtr)Checked, IntPtr.Zero);
            Debug.WriteLine("stdout チェックボックスを ON にしました。");
        }


    }
}