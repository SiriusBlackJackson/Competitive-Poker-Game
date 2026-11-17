using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

// Setup / uninstaller for Zheng Shang You in 8 languages.
// The game payload is embedded as the manifest resource "PAYLOAD" at build time,
// so this single exe installs the whole game without any external files.

static class I18n
{
    public static readonly string[] Codes = { "zh-CN", "zh-TW", "en", "fr", "ru", "ja", "es", "ar" };
    public static readonly string[] Names = { "简体中文", "繁體中文", "English", "Français", "Русский", "日本語", "Español", "العربية" };

    public const int WinSetup = 0, WinUninstall = 1, HeadInstall = 2, HeadRemove = 3, Subtitle = 4,
        LblDir = 5, ChkRun = 6, ReadyInstall = 7, ReadyUninstall = 8, NoDir = 9,
        BtnInstall = 10, BtnUninstall = 11, BtnClose = 12, MsgInstalled = 13, MsgUninstalled = 14,
        ErrInstall = 15, ConfirmRemove = 16, LblLang = 17, ErrUninstall = 18, RunningErr = 19;

    public static int Idx = 2;

    static readonly string[][] T = new string[][] {
        new string[] { // zh-CN
            "争上游 安装程序", "争上游 卸载程序", "安装 争上游", "卸载 争上游",
            "三人扑克游戏，共 5 局，支持 8 种语言。需要 Windows 自带的 .NET Framework 4.x。",
            "安装位置：", "安装完成后启动游戏",
            "点击「安装」复制游戏并创建快捷方式。", "将移除程序、快捷方式与游戏设置。", "请选择安装位置。",
            "安装", "卸载", "关闭",
            "已安装到：\r\n{0}\r\n\r\n已创建桌面与开始菜单快捷方式。",
            "已卸载。安装目录将在几秒后删除。",
            "安装失败：\r\n", "确定要从这台电脑上移除 争上游？", "语言：", "卸载失败：\r\n",
            "已安装的副本仍在运行，请关闭 {0} 后重试。"
        },
        new string[] { // zh-TW
            "爭上游 安裝程式", "爭上游 卸載程式", "安裝 爭上游", "卸載 爭上游",
            "三人撲克遊戲，共 5 局，支援 8 種語言。需要 Windows 內建的 .NET Framework 4.x。",
            "安裝位置：", "安裝完成後啟動遊戲",
            "點擊「安裝」複製遊戲並建立捷徑。", "將移除程式、捷徑與遊戲設定。", "請選擇安裝位置。",
            "安裝", "卸載", "關閉",
            "已安裝到：\r\n{0}\r\n\r\n已建立桌面與開始功能表捷徑。",
            "已卸載。安裝目錄將在幾秒後刪除。",
            "安裝失敗：\r\n", "確定要從這台電腦移除 爭上游？", "語言：", "卸載失敗：\r\n",
            "已安裝的副本仍在執行，請關閉 {0} 後重試。"
        },
        new string[] { // en
            "Zheng Shang You Setup", "Zheng Shang You Uninstaller", "Install Zheng Shang You", "Remove Zheng Shang You",
            "Card game for 3 players, 5 rounds, 8 languages. Requires Windows with .NET Framework 4.x (built in).",
            "Install folder:", "Start the game after installing",
            "Click Install to copy the game and create shortcuts.", "This removes the program, shortcuts and settings.", "Please choose an install folder.",
            "Install", "Uninstall", "Close",
            "Installed to:\r\n{0}\r\n\r\nDesktop and Start Menu shortcuts were created.",
            "Uninstalled. The installation folder is removed in a few seconds.",
            "Installation failed:\r\n", "Remove Zheng Shang You from this computer?", "Language:", "Uninstall failed:\r\n",
            "The installed copy is still running. Close {0} and try again."
        },
        new string[] { // fr
            "Installation de Zheng Shang You", "Désinstallation de Zheng Shang You", "Installer Zheng Shang You", "Supprimer Zheng Shang You",
            "Jeu de cartes à 3 joueurs, 5 manches, 8 langues. Nécessite .NET Framework 4.x (inclus dans Windows).",
            "Dossier d'installation :", "Lancer le jeu après l'installation",
            "Cliquez sur Installer pour copier le jeu et créer les raccourcis.", "Supprime le programme, les raccourcis et les réglages.", "Veuillez choisir un dossier d'installation.",
            "Installer", "Désinstaller", "Fermer",
            "Installé dans :\r\n{0}\r\n\r\nRaccourcis créés sur le Bureau et dans le menu Démarrer.",
            "Désinstallé. Le dossier sera supprimé dans quelques secondes.",
            "Échec de l'installation :\r\n", "Supprimer Zheng Shang You de cet ordinateur ?", "Langue :", "Échec de la désinstallation :\r\n",
            "La copie installée est en cours d'exécution. Fermez {0} et réessayez."
        },
        new string[] { // ru
            "Установка Zheng Shang You", "Удаление Zheng Shang You", "Установить Zheng Shang You", "Удалить Zheng Shang You",
            "Карточная игра для 3 игроков, 5 раундов, 8 языков. Требуется .NET Framework 4.x (входит в Windows).",
            "Папка установки:", "Запустить игру после установки",
            "Нажмите «Установить», чтобы скопировать игру и создать ярлыки.", "Программа, ярлыки и настройки будут удалены.", "Выберите папку установки.",
            "Установить", "Удалить", "Закрыть",
            "Установлено в:\r\n{0}\r\n\r\nСозданы ярлыки на рабочем столе и в меню «Пуск».",
            "Удалено. Папка будет удалена через несколько секунд.",
            "Ошибка установки:\r\n", "Удалить Zheng Shang You с этого компьютера?", "Язык:", "Ошибка удаления:\r\n",
            "Установленная копия ещё запущена. Закройте {0} и повторите попытку."
        },
        new string[] { // ja
            "Zheng Shang You セットアップ", "Zheng Shang You アンインストーラー", "Zheng Shang You をインストール", "Zheng Shang You を削除",
            "3人用カードゲーム、全5局、8言語対応。Windows 標準の .NET Framework 4.x が必要です。",
            "インストール先:", "インストール後にゲームを起動する",
            "「インストール」を押すと、ゲームをコピーしてショートカットを作成します。", "プログラム、ショートカット、設定を削除します。", "インストール先を選択してください。",
            "インストール", "削除", "閉じる",
            "インストール先:\r\n{0}\r\n\r\nデスクトップとスタートメニューにショートカットを作成しました。",
            "削除しました。インストール先は数秒後に削除されます。",
            "インストールに失敗しました:\r\n", "このコンピューターから Zheng Shang You を削除しますか？", "言語:", "削除に失敗しました:\r\n",
            "インストール済みのコピーが実行中です。{0} を終了して再試行してください。"
        },
        new string[] { // es
            "Instalador de Zheng Shang You", "Desinstalador de Zheng Shang You", "Instalar Zheng Shang You", "Quitar Zheng Shang You",
            "Juego de cartas para 3 jugadores, 5 rondas, 8 idiomas. Requiere .NET Framework 4.x (incluido en Windows).",
            "Carpeta de instalación:", "Iniciar el juego después de instalar",
            "Pulsa Instalar para copiar el juego y crear los accesos directos.", "Se eliminarán el programa, los accesos directos y la configuración.", "Elige una carpeta de instalación.",
            "Instalar", "Desinstalar", "Cerrar",
            "Instalado en:\r\n{0}\r\n\r\nSe crearon accesos directos en el Escritorio y en el menú Inicio.",
            "Desinstalado. La carpeta se eliminará en unos segundos.",
            "Error de instalación:\r\n", "¿Quitar Zheng Shang You de este equipo?", "Idioma:", "Error de desinstalación:\r\n",
            "La copia instalada sigue en ejecución. Cierra {0} e inténtalo de nuevo."
        },
        new string[] { // ar
            "مثبّت Zheng Shang You", "مزيل Zheng Shang You", "تثبيت Zheng Shang You", "إزالة Zheng Shang You",
            "لعبة ورق لثلاثة لاعبين، خمس جولات، ثماني لغات. تتطلب \u200E.NET Framework 4.x\u200E الموجود في Windows.",
            "مجلد التثبيت:", "تشغيل اللعبة بعد التثبيت",
            "اضغط «تثبيت» لنسخ اللعبة وإنشاء الاختصارات.", "ستتم إزالة البرنامج والاختصارات والإعدادات.", "اختر مجلد التثبيت.",
            "تثبيت", "إزالة", "إغلاق",
            "تم التثبيت في:\r\n{0}\r\n\r\nتم إنشاء اختصارات على سطح المكتب وفي قائمة ابدأ.",
            "تمت الإزالة. سيُحذف المجلد بعد ثوانٍ.",
            "فشل التثبيت:\r\n", "هل تريد إزالة Zheng Shang You من هذا الكمبيوتر؟", "اللغة:", "فشل الإزالة:\r\n",
            "النسخة المثبتة قيد التشغيل. أغلق {0} وأعد المحاولة."
        }
    };

    public static string G(int key) { return T[Idx][key]; }
    public static string Code { get { return Codes[Idx]; } }
    public static bool Rtl { get { return Codes[Idx] == "ar"; } }

    public static void Detect()
    {
        string code = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
        string full = System.Globalization.CultureInfo.CurrentUICulture.Name;
        if (code == "zh") Idx = full.IndexOf("TW", StringComparison.OrdinalIgnoreCase) >= 0
            || full.IndexOf("HK", StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 0;
        else if (code == "fr") Idx = 3;
        else if (code == "ru") Idx = 4;
        else if (code == "ja") Idx = 5;
        else if (code == "es") Idx = 6;
        else if (code == "ar") Idx = 7;
        else Idx = 2;
    }
}

static class Setup
{
    const string AppName = "Zheng Shang You";
    const string AppId = "ZhengShangYou";
    const string Version = "1.0.0";
    const string Publisher = "ZhengShangYou Project";

    public static string DefaultInstallDir()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Path.Combine("Programs", AppId));
    }

    static string ExePath(string dir) { return Path.Combine(dir, AppId + ".exe"); }
    static string UninstallPath(string dir) { return Path.Combine(dir, "Uninstall.exe"); }
    static string DesktopLink()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), AppName + ".lnk");
    }
    static string StartMenuLink()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), AppName + ".lnk");
    }
    static string RegKey()
    {
        return @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + AppId;
    }

    static void CreateShortcut(string link, string target, string workDir)
    {
        object shell = null;
        try
        {
            Type t = Type.GetTypeFromProgID("WScript.Shell");
            if (t == null) return;
            shell = Activator.CreateInstance(t);
            object sc = t.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { link });
            Type st = sc.GetType();
            st.InvokeMember("TargetPath", BindingFlags.SetProperty, null, sc, new object[] { target });
            st.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, sc, new object[] { workDir });
            st.InvokeMember("IconLocation", BindingFlags.SetProperty, null, sc, new object[] { target + ",0" });
            st.InvokeMember("Description", BindingFlags.SetProperty, null, sc, new object[] { AppName });
            st.InvokeMember("Save", BindingFlags.InvokeMethod, null, sc, null);
        }
        catch { }
        finally
        {
            if (shell != null && System.Runtime.InteropServices.Marshal.IsComObject(shell))
                System.Runtime.InteropServices.Marshal.ReleaseComObject(shell);
        }
    }

    public static bool Install(string dir, string lang, out string message)
    {
        try
        {
            if (File.Exists(ExePath(dir)))
            {
                try { File.Delete(ExePath(dir)); }
                catch { return Fail(out message, I18n.G(I18n.RunningErr).Replace("{0}", AppName)); }
            }
            Directory.CreateDirectory(dir);

            Stream payload = Assembly.GetExecutingAssembly().GetManifestResourceStream("PAYLOAD");
            if (payload == null) return Fail(out message, "Installer payload is missing.");
            using (payload)
            using (FileStream fs = new FileStream(ExePath(dir), FileMode.Create, FileAccess.Write))
            {
                byte[] buf = new byte[8192];
                int n;
                while ((n = payload.Read(buf, 0, buf.Length)) > 0) fs.Write(buf, 0, n);
            }

            string self = Application.ExecutablePath;
            try { File.Copy(self, UninstallPath(dir), true); }
            catch { }

            CreateShortcut(DesktopLink(), ExePath(dir), dir);
            CreateShortcut(StartMenuLink(), ExePath(dir), dir);

            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(RegKey()))
            {
                k.SetValue("DisplayName", AppName);
                k.SetValue("DisplayVersion", Version);
                k.SetValue("Publisher", Publisher);
                k.SetValue("InstallLocation", dir);
                k.SetValue("DisplayIcon", ExePath(dir) + ",0");
                k.SetValue("UninstallString", "\"" + UninstallPath(dir) + "\" --uninstall");
                k.SetValue("NoModify", 1, RegistryValueKind.DWord);
                k.SetValue("NoRepair", 1, RegistryValueKind.DWord);
            }

            message = string.Format(I18n.G(I18n.MsgInstalled), dir);
            return true;
        }
        catch (Exception ex) { return Fail(out message, ex.Message); }
    }

    static bool Fail(out string message, string text)
    {
        message = I18n.G(I18n.ErrInstall) + text;
        return false;
    }

    public static bool Uninstall(string dir, out string message)
    {
        try
        {
            try { if (File.Exists(DesktopLink())) File.Delete(DesktopLink()); } catch { }
            try { if (File.Exists(StartMenuLink())) File.Delete(StartMenuLink()); } catch { }
            try { Registry.CurrentUser.DeleteSubKeyTree(RegKey(), false); } catch { }

            // The running exe cannot delete its own folder: hand the last step to a hidden cmd that waits briefly.
            string args = "/c ping -n 3 127.0.0.1 >nul & rmdir /s /q \"" + dir + "\"";
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", args);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process.Start(psi);

            message = I18n.G(I18n.MsgUninstalled);
            return true;
        }
        catch (Exception ex) { message = I18n.G(I18n.ErrUninstall) + ex.Message; return false; }
    }
}

class SetupForm : Form
{
    TextBox txtDir;
    Label lblStatus, lblDir, lblLang;
    Label title, sub;
    Button btnInstall, btnUninstall, btnClose;
    CheckBox chkRun;
    ComboBox cboLang;
    bool uninstallMode;

    public SetupForm(bool uninstall)
    {
        uninstallMode = uninstall;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(580, 316);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.FromArgb(10, 58, 40);
        ForeColor = Color.FromArgb(232, 244, 236);
        Font = new Font("Segoe UI", 9f, FontStyle.Regular);

        title = new Label();
        title.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
        title.ForeColor = Color.FromArgb(255, 214, 122);
        title.SetBounds(24, 16, 532, 30);
        Controls.Add(title);

        lblLang = new Label();
        lblLang.SetBounds(24, 54, 80, 22);
        Controls.Add(lblLang);

        cboLang = new ComboBox();
        cboLang.DropDownStyle = ComboBoxStyle.DropDownList;
        cboLang.Items.AddRange(I18n.Names);
        cboLang.SelectedIndex = I18n.Idx;
        cboLang.SetBounds(108, 52, 170, 24);
        cboLang.SelectedIndexChanged += delegate { I18n.Idx = cboLang.SelectedIndex; ApplyLang(); };
        Controls.Add(cboLang);

        sub = new Label();
        sub.ForeColor = Color.FromArgb(200, 226, 210);
        sub.SetBounds(24, 84, 532, 34);
        Controls.Add(sub);

        lblDir = new Label();
        lblDir.SetBounds(24, 126, 120, 20);
        Controls.Add(lblDir);

        txtDir = new TextBox();
        txtDir.Text = Setup.DefaultInstallDir();
        txtDir.SetBounds(24, 148, 532, 24);
        if (uninstallMode) txtDir.ReadOnly = true;
        Controls.Add(txtDir);

        chkRun = new CheckBox();
        chkRun.Checked = true;
        chkRun.ForeColor = Color.FromArgb(214, 236, 222);
        chkRun.SetBounds(24, 180, 380, 24);
        if (uninstallMode) chkRun.Visible = false;
        Controls.Add(chkRun);

        lblStatus = new Label();
        lblStatus.ForeColor = Color.FromArgb(190, 222, 204);
        lblStatus.SetBounds(24, 210, 532, 44);
        Controls.Add(lblStatus);

        btnInstall = MakeButton(24, 262, 150);
        btnInstall.Click += OnInstall;
        Controls.Add(btnInstall);

        btnClose = MakeButton(190, 262, 110);
        btnClose.Click += delegate { Close(); };
        Controls.Add(btnClose);

        if (!uninstallMode)
        {
            btnUninstall = MakeButton(314, 262, 110);
            btnUninstall.Click += delegate { RunUninstall(); };
            Controls.Add(btnUninstall);
        }
        AcceptButton = btnInstall;

        ApplyLang();
    }

    Button MakeButton(int x, int y, int w)
    {
        Button b = new Button();
        b.SetBounds(x, y, w, 34);
        b.FlatStyle = FlatStyle.Flat;
        b.BackColor = Color.FromArgb(18, 96, 64);
        b.ForeColor = Color.FromArgb(255, 226, 160);
        b.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        b.FlatAppearance.BorderColor = Color.FromArgb(120, 200, 150);
        return b;
    }

    void ApplyLang()
    {
        RightToLeft = I18n.Rtl ? RightToLeft.Yes : RightToLeft.No;
        RightToLeftLayout = I18n.Rtl;

        Text = uninstallMode ? I18n.G(I18n.WinUninstall) : I18n.G(I18n.WinSetup);
        title.Text = uninstallMode ? I18n.G(I18n.HeadRemove) : I18n.G(I18n.HeadInstall);
        sub.Text = I18n.G(I18n.Subtitle);
        lblDir.Text = I18n.G(I18n.LblDir);
        lblLang.Text = I18n.G(I18n.LblLang);
        chkRun.Text = I18n.G(I18n.ChkRun);
        lblStatus.Text = uninstallMode ? I18n.G(I18n.ReadyUninstall) : I18n.G(I18n.ReadyInstall);
        btnInstall.Text = uninstallMode ? I18n.G(I18n.BtnUninstall) : I18n.G(I18n.BtnInstall);
        btnClose.Text = I18n.G(I18n.BtnClose);
        if (btnUninstall != null) btnUninstall.Text = I18n.G(I18n.BtnUninstall);
    }

    void OnInstall(object s, EventArgs e)
    {
        string dir = txtDir.Text.Trim();
        if (dir.Length == 0) { lblStatus.Text = I18n.G(I18n.NoDir); return; }
        btnInstall.Enabled = false;
        string msg;
        bool ok;
        if (uninstallMode) ok = Setup.Uninstall(dir, out msg);
        else
        {
            ok = Setup.Install(dir, I18n.Code, out msg);
            if (ok && chkRun.Checked)
            {
                try { Process.Start(Path.Combine(dir, "ZhengShangYou.exe"), "-lang=" + I18n.Code); }
                catch { }
            }
        }
        lblStatus.Text = msg.Replace("\r\n", " ");
        MessageBox.Show(this, msg, "Zheng Shang You", MessageBoxButtons.OK,
            ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        btnInstall.Enabled = true;
        if (ok && uninstallMode) Close();
    }

    void RunUninstall()
    {
        if (MessageBox.Show(this, I18n.G(I18n.ConfirmRemove), "Zheng Shang You",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        string msg;
        Setup.Uninstall(txtDir.Text.Trim(), out msg);
        MessageBox.Show(this, msg, "Zheng Shang You", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }
}

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        bool uninstall = false;
        bool silent = false;
        string dir = null;
        string langArg = null;
        string shot = null;
        foreach (string a in args)
        {
            if (a == "--uninstall") uninstall = true;
            else if (a == "--silent") silent = true;
            else if (a.StartsWith("--dir=")) dir = a.Substring(6);
            else if (a.StartsWith("--lang=")) langArg = a.Substring(7);
            else if (a.StartsWith("--shot=")) shot = a.Substring(7);
        }
        if (dir == null || dir.Length == 0) dir = Setup.DefaultInstallDir();

        if (langArg != null)
        {
            int i = Array.IndexOf(I18n.Codes, langArg);
            if (i >= 0) I18n.Idx = i;
        }
        else I18n.Detect();

        if (silent)
        {
            string msg;
            bool ok = uninstall ? Setup.Uninstall(dir, out msg) : Setup.Install(dir, I18n.Code, out msg);
            Environment.Exit(ok ? 0 : 1);
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Render one setup window to a png (used to eyeball every translation without clicking through).
        if (shot != null)
        {
            SetupForm f = new SetupForm(uninstall);
            f.Show();
            Application.DoEvents();
            Bitmap bmp = new Bitmap(f.Width, f.Height);
            f.DrawToBitmap(bmp, new Rectangle(0, 0, f.Width, f.Height));
            bmp.Save(shot, System.Drawing.Imaging.ImageFormat.Png);
            bmp.Dispose();
            f.Close();
            return;
        }

        Application.Run(new SetupForm(uninstall));
    }
}
