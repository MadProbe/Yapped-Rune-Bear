namespace Chomp.Util {
    // Token: 0x02000007 RID: 7
    internal class GameMode {
        // Token: 0x1700004A RID: 74
        // (get) Token: 0x060000FF RID: 255 RVA: 0x0000EE3C File Offset: 0x0000D03C
        public GameType Game;

        // Token: 0x1700004B RID: 75
        // (get) Token: 0x06000100 RID: 256 RVA: 0x0000EE44 File Offset: 0x0000D044
        public string Name;

        // Token: 0x1700004C RID: 76
        // (get) Token: 0x06000101 RID: 257 RVA: 0x0000EE4C File Offset: 0x0000D04C
        public string Directory;

        // Token: 0x06000102 RID: 258 RVA: 0x0000EE54 File Offset: 0x0000D054
        public GameMode(GameType game, string name, string directory) {
            this.Game = game;
            this.Name = name;
            this.Directory = directory;
        }

        public override string ToString() => this.Name;

        // Token: 0x04000088 RID: 136
        public static readonly GameMode[] Modes = new GameMode[] {
            new GameMode(GameType.DemonsSouls, "Demon's Souls", "DES"),
            new GameMode(GameType.DarkSouls1, "Dark Souls 1", "DS1"),
            new GameMode(GameType.DarkSouls2, "Dark Souls 2", "DS2"),
            new GameMode(GameType.DarkSouls2Scholar, "Dark Souls 2: Scholar of the First Sin", "DS2S"),
            new GameMode(GameType.DarkSouls3, "Dark Souls 3", "DS3"),
            new GameMode(GameType.Bloodborne, "Bloodborne", "BB"),
            new GameMode(GameType.DarkSoulsRemastered, "Dark Souls Remastered", "DS1R"),
            new GameMode(GameType.Sekiro, "Sekiro", "SDT"),
            new GameMode(GameType.EldenRing, "Elden Ring", "ER")
        };

        // Token: 0x02000022 RID: 34
        public enum GameType {
            // Token: 0x04000166 RID: 358
            DemonsSouls,
            // Token: 0x04000167 RID: 359
            DarkSouls1,
            // Token: 0x04000168 RID: 360
            DarkSouls2,
            // Token: 0x04000169 RID: 361
            DarkSouls2Scholar,
            // Token: 0x0400016A RID: 362
            DarkSouls3,
            // Token: 0x0400016B RID: 363
            Bloodborne,
            // Token: 0x0400016C RID: 364
            DarkSoulsRemastered,
            // Token: 0x0400016D RID: 365
            Sekiro,
            // Token: 0x0400016E RID: 366
            EldenRing
        }
    }
}
