﻿using System;
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
        public const int IDM_STATE_LOAD = 135; // DeSmuMEのコマンドID（仮定）
        public const int IDM_STATE_SAVE = 136; // DeSmuMEのコマンドID（仮定）
        public const int IDM_STATE_LOAD_F10 = 149; // DeSmuMEのコマンドID（仮定）
        public const int IDM_STATE_SAVE_F10 = 139; // DeSmuMEのコマンドID（仮定）
        public const int IDM_PAUSE = 113; // DeSmuMEのコマンドID（仮定）
        public const int IDC_NEW_LUA_SCRIPT = 318; // DeSmuMEのコマンドID（仮定）
        public const int IDC_EDIT_LUAPATH = 310; // DeSmuMEのコマンドID（仮定）
        public const int IDC_CLOSE_LUA_SCRIPTS = 368; // DeSmuMEのコマンドID（仮定）
        public const int IDC_BUTTON_LUASTOP = 312; // DeSmuMEのコマンドID（仮定）
        public const int IDC_BUTTON_LUARUN = 311; // DeSmuMEのコマンドID（仮定）

        public const int WM_SETTEXT = 0x000C; // DeSmuMEのコマンドID（仮定）
        public const int WM_CLOSE = 0x0010; // ウィンドウを閉じるメッセージ
        public const int SW_RESTORE = 9;

        public const int IDC_USE_STDOUT = 1052;
        public const int BM_SETCHECK = 0x00F1;
        public const int BM_CLICK = 0x00F5;
        public const int BST_CHECKED = 1;
        public const int BST_NO_CHECKED = 0;
    }

    internal static class DeSmuMEButton
    {
        public const int IDD_INPUTCONFIG = 50038;
        public const uint WM_USER = 0x0400;
        public const uint CUSTOM_MESSAGE = WM_USER + 43;

        public const int Debug = 50019;
        public const int Up = 50020;
        public const int Left = 50021;
        public const int Down = 50022;
        public const int Right = 50023;
        public const int B = 50024;
        public const int A = 50025;
        public const int Y = 50026;
        public const int X = 50027;
        public const int Start = 50028;
        public const int Select = 50029;
        public const int L = 50030;
        public const int R = 50031;
        public const int UpLeft = 50032;
        public const int UpRight = 50033;
        public const int DownRight = 50034;
        public const int DownLeft = 50035;
    }

}
