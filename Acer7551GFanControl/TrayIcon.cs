using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Acer7551GFanControl
{

    class TrayIcon : ApplicationContext
    {
    
        private NotifyIcon notifyIcon;
        private IContainer components;
        private ContextMenu contextMenu;
        private Regulator regulator;
        Config config;
        
        public TrayIcon(Regulator regulator)
        {
            components = new System.ComponentModel.Container();
            notifyIcon = new NotifyIcon(this.components);

            this.regulator = regulator;
            config = new Config("config.xml");
            regulator.profile = config.defaultProfile;

            //context menu
            contextMenu = new ContextMenu();
            foreach (IProfile profile in config.profiles)
            {
                MenuItem menuItem = new MenuItem(profile.name);
                menuItem.Checked = config.defaultProfile == profile;
                menuItem.Click += delegate(Object sender, EventArgs e)
                    {
                        regulator.profile = profile;
                        foreach (MenuItem other in contextMenu.MenuItems) other.Checked = false;
                        menuItem.Checked = true;
                    };
                contextMenu.MenuItems.Add(menuItem);
            }
            contextMenu.MenuItems.Add("-");
            MenuItem bios = new MenuItem("BIOS");
            bios.Checked = config.defaultProfile == null;
            bios.Click += delegate(Object sender, EventArgs e)
                {
                    regulator.profile = null;
                    foreach (MenuItem other in contextMenu.MenuItems) other.Checked = false;
                    bios.Checked = true;
                };
            contextMenu.MenuItems.Add(bios);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add("Exit", OnExitClicked);
            notifyIcon.ContextMenu = contextMenu;

            RenderIcon(-1,0);
            notifyIcon.Visible = true;
            regulator.UpdateEvent += UpdateView;
        }

        public void UpdateView(Regulator regulator, float fanSpeed, float temperature, String status)
        {
            RenderIcon((int)fanSpeed, (int)temperature);
            notifyIcon.Text = status;
        }


        
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle); 

        private void RenderIcon(int temp, int fan)
        {
            using (Brush brush = new SolidBrush(Color.FromArgb(255, 255, 255)))
            using (Bitmap bitmap = Properties.Resources.icon16)
            using (Bitmap font = Properties.Resources.font16)
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                if (temp >= 0 && temp <= 99)
                {
                    graphics.DrawImage(font, new Rectangle(0, 0, 8, 11), new Rectangle(((temp / 10) % 10) * 8, 0, 8, 11), GraphicsUnit.Pixel);
                    graphics.DrawImage(font, new Rectangle(8, 0, 8, 11), new Rectangle((temp % 10) * 8, 0, 8, 11), GraphicsUnit.Pixel);
                }
                else
                {
                    graphics.DrawImage(font, new Rectangle(0, 0, 8, 11), new Rectangle(80, 0, 8, 11), GraphicsUnit.Pixel);
                    graphics.DrawImage(font, new Rectangle(8, 0, 8, 11), new Rectangle(80, 0, 8, 11), GraphicsUnit.Pixel);
                }
                graphics.FillRectangle(brush, 1, 12, fan * 14 / 100, 3);
                Icon prevIcon = notifyIcon.Icon;
                notifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
                if(prevIcon != null) DestroyIcon(prevIcon.Handle);
            }
        }

        private void OnExitClicked(Object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

    }
}
