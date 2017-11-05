using System;
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
#if DEBUG
            NativeMethods.AllocConsole();
            Console.WriteLine("Running TrackballScroll in debug mode"); 
#endif

            Application.Run(new MainForm());
        }

        private NotifyIcon trayIcon;
        private MenuItem itemEnabled;
        private MenuItem itemPreferAxis;
        private MouseHookTrackballScroll mouseHook;

        public MainForm()
        {
            mouseHook = new MouseHookTrackballScroll();

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

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                mouseHook.Dispose();
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}
