using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dq9BattleEmulatorTestAgent
{
    internal class DeSmuMECommands
    {
        public const uint WM_COMMAND = 0x0111;
        public const int IDD_LUARECENT_RESERVE_START = 58100; // DeSmuMEのコマンドID（仮定）
        public const int IDC_LUACONSOLE = 309; // DeSmuMEのコマンドID（仮定）

        public const int IDC_USE_STDOUT = 1052;
        public const int BM_SETCHECK = 0x00F1;
        public const int BST_CHECKED = 1;
        public const int BST_NO_CHECKED = 0;
    }
}
