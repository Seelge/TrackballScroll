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
            System.Diagnostics.Debug.WriteLine("Running TrackballScroll in debug mode"); // Writes to VS output view
#endif

            Application.Run(new MainForm());
        }

        private NotifyIcon trayIcon;
        private MenuItem itemEnabled;
        private MenuItem itemUseX1;
        private MenuItem itemUseX2;
        private MenuItem itemPreferAxis;
        private MouseHookTrackballScroll mouseHook;

        public MainForm()
        {
            Properties.Settings.Default.Upgrade();

            mouseHook = new MouseHookTrackballScroll();

            itemEnabled = new MenuItem(Properties.Resources.TextButtonHookEnabled, OnToggleHook)
            {
                Checked = true
            };

            var useX1 = Properties.Settings.Default.useX1;
            itemUseX1 = new MenuItem(Properties.Resources.TextButtonHookUseX1, OnToggleUseX1);
            itemUseX1.Checked = useX1;
            mouseHook.useX1   = useX1;

            var useX2 = Properties.Settings.Default.useX2;
            itemUseX2 = new MenuItem(Properties.Resources.TextButtonHookUseX2, OnToggleUseX2);
            itemUseX2.Checked = useX2;
            mouseHook.useX2   = useX2;

            var preferAxis = Properties.Settings.Default.preferAxis;
            itemPreferAxis = new MenuItem(Properties.Resources.TextButtonPreferAxisEnabled, OnToggleAxis);
            itemPreferAxis.Checked = preferAxis;
            mouseHook.preferAxisMovement = preferAxis;

            ContextMenu trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add(itemEnabled);
            trayMenu.MenuItems.Add(itemUseX1);
            trayMenu.MenuItems.Add(itemUseX2);
            trayMenu.MenuItems.Add(itemPreferAxis);
            trayMenu.MenuItems.Add(Properties.Resources.TextButtonAbout, OnAbout);
            trayMenu.MenuItems.Add(Properties.Resources.TextButtonExit, OnExit);

            trayIcon = new NotifyIcon
            {
                Text = Properties.Resources.TextTitle,
                Icon = Properties.Resources.icon,
                ContextMenu = trayMenu,
                Visible = true
            };
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
                itemUseX1.Enabled = false;
                itemUseX2.Enabled = false;
                itemPreferAxis.Enabled = false;
            }
            else
            {
                mouseHook.Hook();
                itemEnabled.Checked = true;
                itemEnabled.Text = Properties.Resources.TextButtonHookEnabled;
                itemUseX1.Enabled = true;
                itemUseX2.Enabled = true;
                itemPreferAxis.Enabled = true;
            }
        }

        private void OnToggleUseX1(object sender, EventArgs e)
        {
            var useX1 = !itemUseX1.Checked;
            itemUseX1.Checked = useX1;
            mouseHook.useX1 = useX1;
            Properties.Settings.Default.useX1 = useX1;
            Properties.Settings.Default.Save();
            if (!itemUseX1.Checked && !itemUseX2.Checked)
            {
                OnToggleUseX2(null, null);
            }
        }

        private void OnToggleUseX2(object sender, EventArgs e)
        {
            itemUseX2.Checked = !itemUseX2.Checked;
            mouseHook.useX2 = itemUseX2.Checked;
            Properties.Settings.Default.useX2 = itemUseX2.Checked;
            Properties.Settings.Default.Save();
            if (!itemUseX1.Checked && !itemUseX2.Checked)
            {
                OnToggleUseX1(null, null);
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
            Properties.Settings.Default.preferAxis = itemPreferAxis.Checked;
            Properties.Settings.Default.Save();
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
