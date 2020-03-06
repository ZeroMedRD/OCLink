using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OCLink
{
    public class HotKey
    {
        /// <summary>
        /// 如果函式執行成功，返回值不為0，如果執行失敗，返回值為0
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(
            IntPtr hWnd,                 // 視窗的控制代碼, 當熱鍵按下時，會產生WM_HOTKEY資訊，該資訊該會發送該視窗控制代碼
            int id,                      // 定義熱鍵ID,屬於唯一標識熱鍵的作用
            uint fsModifiers,            // 熱鍵只有在按下Alt、 Ctrl、Shift、Windows等鍵時才會生效，即才會產生WM_HOTKEY資訊
            Keys vk                      // 虛擬鍵,即按了Alt+Ctrl+ X ，X就是代表虛擬鍵
            );

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(
            IntPtr hWnd,                   // 視窗控制代碼
            int id                         // 要取消熱鍵的ID
            );
    }
}
