using SoulsFormats;
using SoulsFormats.Binder;

namespace Chomp.Util {
    // Token: 0x02000008 RID: 8
    public class ParamWrapper : IComparable<ParamWrapper> {
        public PARAM.Row this[int id] => this.Param[id];

        // Token: 0x1700004D RID: 77
        // (get) Token: 0x06000104 RID: 260 RVA: 0x0000EF38 File Offset: 0x0000D138
        public bool Error;

        // Token: 0x1700004E RID: 78
        // (get) Token: 0x06000105 RID: 261 RVA: 0x0000EF40 File Offset: 0x0000D140
        public string Name {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set;
        }

        // Token: 0x1700004F RID: 79
        // (get) Token: 0x06000106 RID: 262 RVA: 0x0000EF48 File Offset: 0x0000D148
        public string Description;

        // Token: 0x0400008C RID: 140
        public PARAM Param;

        // Token: 0x0400008D RID: 141
        public PARAMDEF AppliedParamDef;

        // Token: 0x0400008D RID: 141
        public BinderFile BinderFile;

        // Token: 0x17000050 RID: 80
        // (get) Token: 0x06000107 RID: 263 RVA: 0x0000EF50 File Offset: 0x0000D150
        public List<PARAM.Row> Rows => this.Param.Rows;

        // Token: 0x06000108 RID: 264 RVA: 0x0000EF5D File Offset: 0x0000D15D
        public ParamWrapper(string name, PARAM param, BinderFile binderFile) {
            this.Name = name;
            this.Param = param;
            this.AppliedParamDef = param.AppliedParamdef;
            this.BinderFile = binderFile;
        }

        // Token: 0x06000109 RID: 265 RVA: 0x0000EF7A File Offset: 0x0000D17A
        public int CompareTo(ParamWrapper other) => String.Compare(this.Name, other.Name, StringComparison.Ordinal);
    }
}
