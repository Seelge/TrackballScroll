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

        private const string trackballScrollText = "TrackballScroll";
        private const string enabledText  = "Enabled (click to pause)";
        private const string disabledText = "Paused (click to resume)";
        private const string enabledPreferAxisText = "Prefer vertical/horizontal movement";
        private const string disabledPreferAxisText = "Prefer vertical/horizontal movement (disabled)";
        private const string aboutText = "TrackballScroll v2.0.0\r\n"
                                       + "Copyright (c) 2016 Martin Seelge\r\n"
                                       + "License: The MIT License\r\n"
                                       + "Project URL: https://github.com/Seelge/TrackballScroll/\r\n"
                                       + "\r\n"
                                       + "Scroll with your trackball / mouse movement when holding down button 3 or 4.\r\n"
                                       + "See README.md for additional information.";

        private NotifyIcon trayIcon;
        private MenuItem itemEnabled;
        private MenuItem itemPreferAxis;
        private MouseHookTrackballScroll mouseHook = new MouseHookTrackballScroll();

        public MainForm()
        {
            itemEnabled = new MenuItem(enabledText, OnToggleHook);
            itemEnabled.Checked = true;

            itemPreferAxis = new MenuItem(enabledPreferAxisText, OnToggleAxis);
            itemPreferAxis.Checked = true;
            mouseHook.preferAxisMovement = true;

            ContextMenu trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add(itemEnabled);
            trayMenu.MenuItems.Add(itemPreferAxis);
            trayMenu.MenuItems.Add("About " + trackballScrollText, OnAbout);
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = trackballScrollText;
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
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
                itemEnabled.Text = disabledText;
            }
            else
            {
                mouseHook.Hook();
                itemEnabled.Checked = true;
                itemEnabled.Text = enabledText;
            }
        }

        private void OnToggleAxis(object sender, EventArgs e)
        {
            if (itemPreferAxis.Checked)
            {
                mouseHook.preferAxisMovement = false;
                itemPreferAxis.Checked = false;
                itemPreferAxis.Text = disabledPreferAxisText;
            }
            else
            {
                mouseHook.preferAxisMovement = true;
                itemPreferAxis.Checked = true;
                itemPreferAxis.Text = enabledPreferAxisText;
            }
        }

        private void OnAbout(object sender, EventArgs e)
        {
            MessageBox.Show(aboutText, trackballScrollText);
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
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}
