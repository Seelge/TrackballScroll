using System;
using System.Drawing;
using System.Windows.Forms;

/*
 * Systray application framework for TrackballScroll.
 * Contains the Main method and handling of the systray menu.
 *
 * @author: Martin Seelge
 *
 * Credits:
 * Inspired by
 * https://alanbondo.wordpress.com/2008/06/22/creating-a-system-tray-app-with-c/
 */
namespace TrackballScroll
{
    public class MainForm : Form
    {
        [STAThread]
        public static void Main()
        {
            Application.Run(new MainForm());
        }

        private NotifyIcon trayIcon;
        private MenuItem itemEnabled;
        private MenuItem itemPreferAxis;
        private MouseHookTrackballScroll mouseHook;

        public MainForm()
        {
            var scalingFactor = GetScalingFactor();
            mouseHook = new MouseHookTrackballScroll(scalingFactor);

            itemEnabled = new MenuItem(Properties.Resources.TextButtonHookEnabled, OnToggleHook);
            itemEnabled.Checked = true;

            itemPreferAxis = new MenuItem(Properties.Resources.TextButtonPreferAxisEnabled, OnToggleAxis);
            itemPreferAxis.Checked = true;
            mouseHook.preferAxisMovement = true;

            ContextMenu trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add(itemEnabled);
            trayMenu.MenuItems.Add(itemPreferAxis);
            trayMenu.MenuItems.Add(Properties.Resources.TextButtonAbout, OnAbout);
            trayMenu.MenuItems.Add(Properties.Resources.TextButtonExit, OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = Properties.Resources.TextTitle;
            trayIcon.Icon = Properties.Resources.icon;
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;       // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
            base.OnLoad(e);
        }

        private void OnToggleHook(object sender, EventArgs e)
        {
            if(itemEnabled.Checked)
            {
                mouseHook.Unhook();
                itemEnabled.Checked = false;
                itemEnabled.Text = Properties.Resources.TextButtonHookDisabled;
            }
            else
            {
                mouseHook.Hook();
                itemEnabled.Checked = true;
                itemEnabled.Text = Properties.Resources.TextButtonHookEnabled;
            }
        }

        private void OnToggleAxis(object sender, EventArgs e)
        {
            if (itemPreferAxis.Checked)
            {
                mouseHook.preferAxisMovement = false;
                itemPreferAxis.Checked = false;
                itemPreferAxis.Text = Properties.Resources.TextButtonPreferAxisDisabled;
            }
            else
            {
                mouseHook.preferAxisMovement = true;
                itemPreferAxis.Checked = true;
                itemPreferAxis.Text = Properties.Resources.TextButtonPreferAxisEnabled;
            }
        }

        private void OnAbout(object sender, EventArgs e)
        {
            MessageBox.Show(Properties.Resources.TextMessageBoxAbout, Properties.Resources.TextTitle);
        }

        private void OnExit(object sender, EventArgs e)
        {
            mouseHook.Unhook();
            Application.Exit();
        }
        private float GetScalingFactor()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int logicalScreenHeight = NativeMethods.GetDeviceCaps(desktop, (int)WinAPI.DeviceCap.VERTRES);
            int physicalScreenHeight = NativeMethods.GetDeviceCaps(desktop, (int)WinAPI.DeviceCap.DESKTOPVERTRES);

            float ScreenScalingFactor = (float)physicalScreenHeight / (float)logicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}
