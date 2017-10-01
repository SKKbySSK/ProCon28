using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProCon28
{
    /// <summary>
    /// キーボードのグローバルフックに関するクラス
    /// </summary>
    public static class KeyboardHook
    {
        /// <summary>
        /// P/Invoke
        /// </summary>
        private static class NativeMethods
        {
            /// <summary>
            /// フックプロシージャのデリゲート
            /// </summary>
            /// <param name="nCode">フックプロシージャに渡すフックコード</param>
            /// <param name="msg">フックプロシージャに渡す値</param>
            /// <param name="msllhookstruct">フックプロシージャに渡す値</param>
            /// <returns>フックチェーン内の次のフックプロシージャの戻り値</returns>
            public delegate System.IntPtr KeyboardHookCallback(int nCode, uint msg, ref KBDLLHOOKSTRUCT kbdllhookstruct);

            /// <summary>
            /// アプリケーション定義のフックプロシージャをフックチェーン内にインストールします。
            /// フックプロシージャをインストールすると、特定のイベントタイプを監視できます。
            /// 監視の対象になるイベントは、特定のスレッド、または呼び出し側スレッドと同じデスクトップ内のすべてのスレッドに関連付けられているものです。
            /// </summary>
            /// <param name="idHook">フックタイプ</param>
            /// <param name="lpfn">フックプロシージャ</param>
            /// <param name="hMod">アプリケーションインスタンスのハンドル</param>
            /// <param name="dwThreadId">スレッドの識別子</param>
            /// <returns>関数が成功すると、フックプロシージャのハンドルが返ります。関数が失敗すると、NULL が返ります。</returns>
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern System.IntPtr SetWindowsHookEx(int idHook, KeyboardHookCallback lpfn, System.IntPtr hMod, uint dwThreadId);

            /// <summary>
            /// 現在のフックチェーン内の次のフックプロシージャに、フック情報を渡します。
            /// フックプロシージャは、フック情報を処理する前でも、フック情報を処理した後でも、この関数を呼び出せます。
            /// </summary>
            /// <param name="hhk">現在のフックのハンドル</param>
            /// <param name="nCode">フックプロシージャに渡すフックコード</param>
            /// <param name="msg">フックプロシージャに渡す値</param>
            /// <param name="msllhookstruct">フックプロシージャに渡す値</param>
            /// <returns>フックチェーン内の次のフックプロシージャの戻り値</returns>
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern System.IntPtr CallNextHookEx(System.IntPtr hhk, int nCode, uint msg, ref KBDLLHOOKSTRUCT kbdllhookstruct);

            /// <summary>
            /// SetWindowsHookEx 関数を使ってフックチェーン内にインストールされたフックプロシージャを削除します。
            /// </summary>
            /// <param name="hhk">削除対象のフックプロシージャのハンドル</param>
            /// <returns>関数が成功すると、0 以外の値が返ります。関数が失敗すると、0 が返ります。</returns>
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(System.IntPtr hhk);
        }

        /// <summary>
        /// キーボードの状態の構造体
        /// </summary>
        public struct StateKeyboard
        {
            public Stroke Stroke;
            public Keys Key;
            public uint ScanCode;
            public uint Flags;
            public uint Time;
            public System.IntPtr ExtraInfo;
        }

        /// <summary>
        /// 挙動の列挙型
        /// </summary>
        public enum Stroke
        {
            KEY_DOWN,
            KEY_UP,
            SYSKEY_DOWN,
            SYSKEY_UP,
            UNKNOWN
        }

        /// <summary>
        /// キーボードのグローバルフックを実行しているかどうかを取得、設定します。
        /// </summary>
        public static bool IsHooking
        {
            get;
            private set;
        }

        /// <summary>
        /// キーボードのグローバルフックを中断しているかどうかを取得、設定します。
        /// </summary>
        public static bool IsPaused
        {
            get;
            private set;
        }

        /// <summary>
        /// キーボードの状態を取得、設定します。
        /// </summary>
        public static StateKeyboard State;

        /// <summary>
        /// フックプロシージャ内でのイベント用のデリゲート
        /// </summary>
        /// <param name="msg">キーボードに関するウィンドウメッセージ</param>
        /// <param name="msllhookstruct">低レベルのキーボードの入力イベントの構造体</param>
        public delegate void HookHandler(ref StateKeyboard state);

        /// <summary>
        /// 低レベルのキーボードの入力イベントの構造体
        /// </summary>
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public System.IntPtr dwExtraInfo;
        }

        /// <summary>
        /// フックプロシージャのハンドル
        /// </summary>
        private static System.IntPtr Handle;

        /// <summary>
        /// 入力をキャンセルするかどうかを取得、設定します。
        /// </summary>
        private static bool IsCancel;

        /// <summary>
        /// フックプロシージャ内でのイベント
        /// </summary>
        public static event HookHandler HookEvent;

        /// <summary>
        /// フックチェーンにインストールするフックプロシージャのイベント
        /// </summary>
        private static event NativeMethods.KeyboardHookCallback hookCallback;

        /// <summary>
        /// フックプロシージャをフックチェーン内にインストールし、キーボードのグローバルフックを開始します。
        /// </summary>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        public static void Start()
        {
            if (IsHooking)
            {
                return;
            }

            IsHooking = true;
            IsPaused = false;

            hookCallback = HookProcedure;
            System.IntPtr h = System.Runtime.InteropServices.Marshal.GetHINSTANCE(typeof(KeyboardHook).Assembly.GetModules()[0]);

            // WH_KEYBOARD_LL = 13;
            Handle = NativeMethods.SetWindowsHookEx(13, hookCallback, h, 0);

            if (Handle == System.IntPtr.Zero)
            {
                IsHooking = false;
                IsPaused = true;

                throw new System.ComponentModel.Win32Exception();
            }
        }

        /// <summary>
        /// キーボードのグローバルフックを停止し、フックプロシージャをフックチェーン内から削除します。
        /// </summary>
        public static void Stop()
        {
            if (!IsHooking)
            {
                return;
            }

            if (Handle != System.IntPtr.Zero)
            {
                IsHooking = false;
                IsPaused = true;

                NativeMethods.UnhookWindowsHookEx(Handle);
                Handle = System.IntPtr.Zero;
                hookCallback -= HookProcedure;
            }
        }

        /// <summary>
        /// 次のフックプロシージャにフック情報を渡すのをキャンセルします。
        /// </summary>
        public static void Cancel()
        {
            IsCancel = true;
        }

        /// <summary>
        /// キーボードのグローバルフックを中断します。
        /// </summary>
        public static void Pause()
        {
            IsPaused = true;
        }

        /// <summary>
        /// フックチェーンにインストールするフックプロシージャ
        /// </summary>
        /// <param name="nCode">フックプロシージャに渡すフックコード</param>
        /// <param name="msg">フックプロシージャに渡す値</param>
        /// <param name="msllhookstruct">フックプロシージャに渡す値</param>
        /// <returns>フックチェーン内の次のフックプロシージャの戻り値</returns>
        private static System.IntPtr HookProcedure(int nCode, uint msg, ref KBDLLHOOKSTRUCT s)
        {
            if (nCode >= 0 && HookEvent != null && !IsPaused)
            {
                State.Stroke = GetStroke(msg);
                State.Key = (System.Windows.Forms.Keys)s.vkCode;
                State.ScanCode = s.scanCode;
                State.Flags = s.flags;
                State.Time = s.time;
                State.ExtraInfo = s.dwExtraInfo;

                HookEvent(ref State);

                if (IsCancel)
                {
                    IsCancel = false;

                    return (System.IntPtr)1;
                }
            }

            return NativeMethods.CallNextHookEx(Handle, nCode, msg, ref s);
        }

        /// <summary>
        /// キーボードキーの挙動を取得します。
        /// </summary>
        /// <param name="msg">キーボードに関するウィンドウメッセージ</param>
        /// <returns>キーボードボタンの挙動</returns>
        private static Stroke GetStroke(uint msg)
        {
            switch (msg)
            {
                case 0x100:
                    // WM_KEYDOWN
                    return Stroke.KEY_DOWN;
                case 0x101:
                    // WM_KEYUP
                    return Stroke.KEY_UP;
                case 0x104:
                    // WM_SYSKEYDOWN
                    return Stroke.SYSKEY_DOWN;
                case 0x105:
                    // WM_SYSKEYUP
                    return Stroke.SYSKEY_UP;
                default:
                    return Stroke.UNKNOWN;
            }
        }
    }
}
