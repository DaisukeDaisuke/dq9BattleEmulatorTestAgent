using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);


        Dictionary<string, string> keyLuaMap = new Dictionary<string, string>
        {
            //["debug"] = "keyInput_debug.lua",
            ["R"] = "keyInput_R.lua",
            ["L"] = "keyInput_L.lua",
            ["X"] = "keyInput_X.lua",
            ["Y"] = "keyInput_Y.lua",
            ["A"] = "keyInput_A.lua",
            ["B"] = "keyInput_B.lua",
            ["start"] = "keyInput_start.lua",
            ["select"] = "keyInput_select.lua",
            ["up"] = "keyInput_up.lua",
            ["down"] = "keyInput_down.lua",
            ["left"] = "keyInput_left.lua",
            ["right"] = "keyInput_right.lua",
            //["lid"] = "keyInput_lid.lua",
        };

        Dictionary<string, IntPtr> LuaPtr = new Dictionary<string, IntPtr>();

        public Process Process { get; private set; }
        public IntPtr WindowHandle { get; private set; } = IntPtr.Zero;

        private readonly string _exePath;
        private readonly string _romPath;
        private readonly int _saveSlot;
        private readonly Action<string>? _outputCallback;
        private readonly Action<string>? _errorCallback;
        private bool Paused = true;
        private string _workingDirectory;
        private JoypadScheduler JoypadInputer;

        public DesmumeInstance(
            string exePath,
            string romPath,
            int saveSlot,
            Action<string>? outputCallback = null,
            Action<string>? errorCallback = null,
            string workingDirectory = ""

            )
        {
            _exePath = exePath;
            _romPath = romPath;
            _saveSlot = saveSlot;
            _outputCallback = outputCallback;
            _errorCallback = errorCallback;
            _workingDirectory = workingDirectory;
            JoypadInputer = new JoypadScheduler(this);
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
            JoypadInputer.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            Process = proc;
            foreach (var hWnd in await WindowFinder.WaitForWindowsByRegexAsync(proc.Id, @"0\.9\.14", 1))
            {
                WindowHandle = hWnd;
                break; // 最初に見つかったウィンドウを使用
            }
            if (WindowHandle == IntPtr.Zero)
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

            SendMessage(WindowHandle, DeSmuMECommands.WM_COMMAND, (IntPtr)DeSmuMECommands.IDM_STATE_LOAD_F10 + id, (IntPtr)id);
        }

        public void SaveState(int id = 0)
        {
            if (WindowHandle == IntPtr.Zero)
                throw new InvalidOperationException("DeSmuMEのウィンドウが見つかりません。先にDeSmuMEを起動してください。");

            SendMessage(WindowHandle, DeSmuMECommands.WM_COMMAND, (IntPtr)DeSmuMECommands.IDM_STATE_SAVE_F10 + id, (IntPtr)id);
        }
        public void closeAllLuaConsole()
        {
            if (WindowHandle == IntPtr.Zero)
                throw new InvalidOperationException("DeSmuMEのウィンドウが見つかりません。先にDeSmuMEを起動してください。");

            SendMessage(WindowHandle, DeSmuMECommands.WM_COMMAND, (IntPtr)DeSmuMECommands.IDC_CLOSE_LUA_SCRIPTS, (IntPtr)0);
        }

        public void closeLuaConsole(IntPtr handle)
        {
            if (WindowHandle == IntPtr.Zero)
                throw new InvalidOperationException("DeSmuMEのウィンドウが見つかりません。先にDeSmuMEを起動してください。");

            if (handle == IntPtr.Zero)
                throw new ArgumentException("無効なウィンドウハンドルです。");

            SendMessage(handle, DeSmuMECommands.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
        public void stopLuaScript(IntPtr handle)
        {
            ClickButton(handle, DeSmuMECommands.IDC_BUTTON_LUASTOP);
        }


        public static async Task ActiveWindow(IntPtr parentWindow)
        {
            // ウィンドウをアクティブにする
            ShowWindow(parentWindow, DeSmuMECommands.SW_RESTORE);
            SetForegroundWindow(parentWindow);
          
            await Task.Delay(50); // 少し待つことでアクティブ化を確実にする
            await Task.CompletedTask;
        }

        public async Task runLuaScript(IntPtr handle)
        {
            await ActiveWindow(handle);
            ClickButton(handle, DeSmuMECommands.IDC_BUTTON_LUARUN);
            await Task.CompletedTask;
        }

        public async Task<IntPtr> OpenLuaConsoleAndRunScript(string luaPath)
        {
            if (WindowHandle == IntPtr.Zero)
                throw new InvalidOperationException("DeSmuMEのウィンドウが見つかりません。先にDeSmuMEを起動してください。");

            SendMessage(WindowHandle, DeSmuMECommands.WM_COMMAND, (IntPtr)DeSmuMECommands.IDC_NEW_LUA_SCRIPT, (IntPtr)0);

            foreach (IntPtr WindowHandleLua in await WindowFinder.WaitForWindowsByRegexAsync(Process.Id, "Lua S", 1))
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

            if (Paused == enable)
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
                    JoypadInputer.Stop();
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

        public static void ClickButton(IntPtr parentWindow, int Button)
        {
            IntPtr checkboxHandle = GetDlgItem(parentWindow, Button);
            if (checkboxHandle == IntPtr.Zero)
            {
               
                return;
            }

            SendMessage(checkboxHandle, DeSmuMECommands.BM_CLICK, IntPtr.Zero, IntPtr.Zero);
        }

        public async Task initJoyPad(string key)
        {
            if (string.IsNullOrEmpty(key) || !keyLuaMap.ContainsKey(key))
            {
                throw new ArgumentException($"無効なキー: {key}");
            }
            var path = Path.Combine(_workingDirectory, "resource", "keys", keyLuaMap[key]);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Lua スクリプトが見つかりません: {path}");
            }
            IntPtr luaWindowHandle = await OpenLuaConsoleAndRunScript(path);
            LuaPtr.Add(key, luaWindowHandle);

            await Task.CompletedTask;
        }

        internal async Task ProcessKeyAsync(string key, CancellationToken token)
        {
            if (!LuaPtr.ContainsKey(key))
            {
                await initJoyPad(key);
            }
            else
            {
                var handle = LuaPtr[key];
                if (handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException($"Lua コンソールが開かれていません: {key}");
                }
                await runLuaScript(handle);
            }
            await Task.CompletedTask;
        }

        public void pushJoypadInput(string key)
        {
            if (string.IsNullOrEmpty(key) || !keyLuaMap.ContainsKey(key))
            {
                throw new ArgumentException($"無効なキー: {key}");
            }
            JoypadInputer.PushInput(key);
        }
        public void pushJoypadSleep(int ms)
        {
            JoypadInputer.PushSleep(ms);
        }

    }
}