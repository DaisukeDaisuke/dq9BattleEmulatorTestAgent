using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dq9BattleEmulatorTestAgent
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class WindowFinder
    {
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary>
        /// 特定のプロセスに属するウィンドウのうち、タイトルが正規表現に一致するものを探します
        /// </summary>
        public static IEnumerable<IntPtr> FindWindowsByRegex(
            int targetProcessId,
            string titlePattern,
            Func<IntPtr, string, bool>? onWindowFound = null)
        {
            Regex regex = new(titlePattern, RegexOptions.IgnoreCase);
            List<IntPtr> matchingWindows = new();

            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint pid);
                if (pid != targetProcessId)
                    return true;

                StringBuilder sb = new(512);
                GetWindowText(hWnd, sb, sb.Capacity);
                string title = sb.ToString();

                //Debug.WriteLine("found windows: " + title);

                if (regex.IsMatch(title))
                {
                    // コールバックが指定されている場合、false を返したら列挙停止
                    if (onWindowFound != null && !onWindowFound(hWnd, title))
                        return false;

                    matchingWindows.Add(hWnd); // 一致するウィンドウをリストに追加
                }

                return true;
            }, IntPtr.Zero);

            return matchingWindows; // リストを返す
        }
        public static async Task<IEnumerable<IntPtr>> WaitForWindowsByRegexAsync(
            int targetProcessId,
            string titlePattern,
            int requiredCount,
            int maxTries = 10,
            int delayMs = 250)
        {
            var result = new HashSet<IntPtr>(); // 重複防止

            for (int i = 0; i < maxTries; i++)
            {
                var matches = FindWindowsByRegex(targetProcessId, titlePattern)
                    .Where(hWnd => !result.Contains(hWnd));

                foreach (var hWnd in matches)
                {
                    result.Add(hWnd);
                }

                if (result.Count >= requiredCount)
                    return result;

                await Task.Delay(delayMs);
            }

            return result;
        }
    }
}
