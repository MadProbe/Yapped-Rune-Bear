[assembly: IgnoresAccessChecksTo("System.Runtime.CoreLib")]

static class Program {
    [STAThread]
    static void Main() {

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Main());
        Chomp.Main.settings.Save();
    }
}
