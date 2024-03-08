[assembly: IgnoresAccessChecksTo("System.Runtime.CoreLib")]

static class Program {
    [STAThread]
    static void Main(string[] args) {
        try {
            if (args.Length != 0) {
                var mods = new List<Mod>();
                string baseRegulationBin = null;
                string outputRegulationBin = null;
                bool backup = false;
                for (int i = 0; i < args.Length; i++) {
                    string argument = args[i].ToLowerInvariant();
                    if (argument == "--backup") {
                        backup = true;
                    }
                    if (i < args.Length - 1) {
                        string value = args[i + 1];
                        switch (argument.ToLowerInvariant()) {
                            case "-b":
                            case "--base":
                                baseRegulationBin = value;
                                break;
                            case "-i":
                            case "--input":
                                mods.Add(new Mod {
                                    path = value,
                                    type = Path.GetExtension(value) switch {
                                        ".bin" or ".parambnd" => ModType.ParamBND,
                                        ".csv" => ModType.CSV,
                                        _ when Directory.Exists(value) => ModType.Folder,
                                        _ => throw new Exception($"Invalid input file \"{value}\"")
                                    }
                                });
                                break;
                            case "-o":
                            case "--output":
                                outputRegulationBin = value;
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (baseRegulationBin is null) {
                    throw new Exception("Please specify base ParamBND file (.bin / .parambnd)");
                }
                if (outputRegulationBin is null) {
                    throw new Exception("Please specify output ParamBND file (.bin / .parambnd)");
                }
                HandleModMerging(baseRegulationBin, mods.ToArray(), outputRegulationBin, backup);
                return;
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
            Chomp.Main.settings.Save();
        } catch (Exception ex) {
            Console.WriteLine(ex);
        }
    }
    static void HandleModMerging(string @base, Mod[] mods, string outputFile, bool backup) {

    }
}

readonly record struct Mod(ModType type, string path) { }

enum ModType : byte {
    ParamBND,
    CSV,
    Folder
}
