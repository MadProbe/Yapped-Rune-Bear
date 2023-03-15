using Chomp.Properties;
using Chomp.Util;
using SoulsFormats;

namespace Chomp.Tools {
    // Token: 0x0200000B RID: 11
    internal static class AffinityGeneration {
        // Token: 0x06000114 RID: 276 RVA: 0x0000F40C File Offset: 0x0000D60C
        public static void GenerateAffinityRows(DataGridViewRow paramRow, ParamWrapper wrapper, DataGridView dgvRows, GameMode gameMode) {
            if (paramRow.Index != 45) {
                Utility.ShowError("You can't generate Affinity rows outside of the EquipWeaponParam.");
                return;
            }
            if (dgvRows.SelectedCells.Count == 0) {
                Utility.ShowError("You can't generate Affinity rows without a row selected!");
                return;
            }
            const string configDir = "Tools\\AffinityGeneration\\\\";
            if (!Directory.Exists(configDir)) {
                Utility.ShowError("Affinity Generation configuration directory not found.");
                return;
            }
            int index = dgvRows.SelectedCells[0].RowIndex;
            PARAM.Row currentRow = wrapper.Rows[index];
            Console.WriteLine(currentRow.Name);
            foreach (FileInfo file in new DirectoryInfo(configDir).GetFiles("*.txt")) {
                string rawName = file.Name;
                string[] splitted = rawName.Split(separator: '-');
                string offset = splitted[0].Trim();
                string name = splitted[1].Trim().Replace(".txt", "");
                var instructions = new List<string[]>();
                using (var reader = new StreamReader(file.FullName)) {
                    while (!reader.EndOfStream) {
                        string[] values = reader.ReadLine().Split(separator: ';');
                        instructions.Add(values);
                    }
                }
                int num = currentRow.ID + Convert.ToInt32(offset);
                string new_name = currentRow.Name + " [" + name + "]";
                var newRow = new PARAM.Row(num, new_name, wrapper.AppliedParamDef);
                for (int i = 0; i < currentRow.Cells.Count; i++) {
                    newRow.Cells[i].Value = currentRow.Cells[i].Value;
                }
                foreach (string[] instruction in instructions) {
                    ModifyAffinityField(currentRow, newRow, instruction);
                }
                wrapper.Rows.Add(newRow);
                wrapper.Rows.Sort((r1, r2) => r1.ID.CompareTo(r2.ID));
            }
            if (!Settings.Default.ShowConfirmationMessages) {
                _ = MessageBox.Show("Affinity rows generated!", "Affinity Generator");
            }
        }

        // Token: 0x06000115 RID: 277 RVA: 0x0000F680 File Offset: 0x0000D880
        public static void ModifyAffinityField(PARAM.Row baseRow, PARAM.Row row, string[] instruction) {
            string instruction_field = instruction[0];
            string instruction_type = instruction[1];
            string instruction_value = instruction[2];
            string base_STR_correction = "";
            string base_DEX_correction = "";
            string base_INT_correction = "";
            string base_FTH_correction = "";
            string base_PHYSICAL_damage = "";
            string base_MAGIC_damage = "";
            string base_FIRE_damage = "";
            string base_LIGHTNING_damage = "";
            string base_HOLY_damage = "";
            foreach (PARAM.Cell cell in baseRow.Cells) {
                PARAMDEF.DefType type = cell.Def.DisplayType;
                string name = cell.Def.InternalName.ToString();
                string value = cell.Value.ToString();
                if (type != PARAMDEF.DefType.dummy8 && name != null) {
                    if (name == "attackBaseFire") {
                        base_FIRE_damage = value;
                    } else if (name == "correctAgility") {
                        base_DEX_correction = value;
                    } else if (name == "disableGemAttr") {
                        cell.Value = 0;
                    } else if (name == "correctStrength") {
                        base_STR_correction = value;
                    } else if (name == "correctMagic") {
                        base_INT_correction = value;
                    } else if (name == "attackBasePhysics") {
                        base_PHYSICAL_damage = value;
                    } else if (name == "attackBaseDark") {
                        base_HOLY_damage = value;
                    } else if (name == "attackBaseThunder") {
                        base_LIGHTNING_damage = value;
                    } else if (name == "attackBaseMagic") {
                        base_MAGIC_damage = value;
                    } else if (name == "correctFaith") {
                        base_FTH_correction = value;
                    }
                }
            }
            float highest_value = new string[] {
                base_STR_correction,
                base_DEX_correction,
                base_INT_correction,
                base_FTH_correction
            }.Max(Convert.ToSingle);
            string base_HIGHEST_correction = Convert.ToString(highest_value);
            int index = 0;
            foreach (PARAM.Cell cell2 in row.Cells) {
                PARAMDEF.DefType type2 = cell2.Def.DisplayType;
                string name2 = cell2.Def.InternalName;
                _ = cell2.Value.ToString();
                string base_value = baseRow.Cells[index].Value.ToString();
                string cell_formula = "";
                var stf = new StringToFormula();
                if (type2 != PARAMDEF.DefType.dummy8 && instruction_field == name2 && instruction_type != null) {
                    if (!(instruction_type == "SET")) {
                        if (!(instruction_type == "CALC")) {
                            if (!(instruction_type == "STAT_CALC")) {
                                if (instruction_type == "DMG_CALC") {
                                    if (instruction_value.Contains("PHYSICAL")) {
                                        cell_formula = instruction_value.Replace("PHYSICAL", base_PHYSICAL_damage);
                                    } else if (instruction_value.Contains("MAGIC")) {
                                        cell_formula = instruction_value.Replace("MAGIC", base_MAGIC_damage);
                                    } else if (instruction_value.Contains("FIRE")) {
                                        cell_formula = instruction_value.Replace("FIRE", base_FIRE_damage);
                                    } else if (instruction_value.Contains("LIGHTNING")) {
                                        cell_formula = instruction_value.Replace("LIGHTNING", base_LIGHTNING_damage);
                                    } else if (instruction_value.Contains("HOLY")) {
                                        cell_formula = instruction_value.Replace("HOLY", base_HOLY_damage);
                                    }
                                    float dmg_result = (float)decimal.Floor(stf.Eval(cell_formula));
                                    if (type2 == PARAMDEF.DefType.s8) {
                                        cell2.Value = Convert.ToSByte(dmg_result);
                                    } else if (type2 == PARAMDEF.DefType.u8) {
                                        cell2.Value = Convert.ToByte(dmg_result);
                                    } else if (type2 == PARAMDEF.DefType.s16) {
                                        cell2.Value = Convert.ToInt16(dmg_result);
                                    } else if (type2 == PARAMDEF.DefType.u16) {
                                        cell2.Value = Convert.ToUInt16(dmg_result);
                                    } else if (type2 == PARAMDEF.DefType.s32) {
                                        cell2.Value = Convert.ToInt32(dmg_result);
                                    } else if (type2 == PARAMDEF.DefType.u32) {
                                        cell2.Value = Convert.ToUInt32(dmg_result);
                                    } else if (type2 == PARAMDEF.DefType.f32) {
                                        cell2.Value = dmg_result;
                                    }
                                }
                            } else {
                                if (instruction_value.Contains("STR")) {
                                    cell_formula = instruction_value.Replace("STR", base_STR_correction);
                                } else if (instruction_value.Contains("DEX")) {
                                    cell_formula = instruction_value.Replace("DEX", base_DEX_correction);
                                } else if (instruction_value.Contains("INT")) {
                                    cell_formula = instruction_value.Replace("INT", base_INT_correction);
                                } else if (instruction_value.Contains("FTH")) {
                                    cell_formula = instruction_value.Replace("FTH", base_FTH_correction);
                                } else if (instruction_value.Contains("HIGHEST")) {
                                    cell_formula = instruction_value.Replace("HIGHEST", base_HIGHEST_correction);
                                }
                                float stat_result = (float)decimal.Floor(stf.Eval(cell_formula));
                                if (type2 == PARAMDEF.DefType.s8) {
                                    cell2.Value = Convert.ToSByte(stat_result);
                                } else if (type2 == PARAMDEF.DefType.u8) {
                                    cell2.Value = Convert.ToByte(stat_result);
                                } else if (type2 == PARAMDEF.DefType.s16) {
                                    cell2.Value = Convert.ToInt16(stat_result);
                                } else if (type2 == PARAMDEF.DefType.u16) {
                                    cell2.Value = Convert.ToUInt16(stat_result);
                                } else if (type2 == PARAMDEF.DefType.s32) {
                                    cell2.Value = Convert.ToInt32(stat_result);
                                } else if (type2 == PARAMDEF.DefType.u32) {
                                    cell2.Value = Convert.ToUInt32(stat_result);
                                } else if (type2 == PARAMDEF.DefType.f32) {
                                    cell2.Value = stat_result;
                                }
                            }
                        } else {
                            cell_formula = instruction_value.Replace("x", base_value);
                            float result = (float)decimal.Floor(stf.Eval(cell_formula));
                            if (type2 == PARAMDEF.DefType.s8) {
                                cell2.Value = Convert.ToSByte(result);
                            } else if (type2 == PARAMDEF.DefType.u8) {
                                cell2.Value = Convert.ToByte(result);
                            } else if (type2 == PARAMDEF.DefType.s16) {
                                cell2.Value = Convert.ToInt16(result);
                            } else if (type2 == PARAMDEF.DefType.u16) {
                                cell2.Value = Convert.ToUInt16(result);
                            } else if (type2 == PARAMDEF.DefType.s32) {
                                cell2.Value = Convert.ToInt32(result);
                            } else if (type2 == PARAMDEF.DefType.u32) {
                                cell2.Value = Convert.ToUInt32(result);
                            } else if (type2 == PARAMDEF.DefType.f32) {
                                cell2.Value = result;
                            }
                        }
                    } else if (type2 == PARAMDEF.DefType.s8) {
                        cell2.Value = Convert.ToSByte(instruction_value);
                    } else if (type2 == PARAMDEF.DefType.u8) {
                        cell2.Value = Convert.ToByte(instruction_value);
                    } else if (type2 == PARAMDEF.DefType.s16) {
                        cell2.Value = Convert.ToInt16(instruction_value);
                    } else if (type2 == PARAMDEF.DefType.u16) {
                        cell2.Value = Convert.ToUInt16(instruction_value);
                    } else if (type2 == PARAMDEF.DefType.s32) {
                        cell2.Value = Convert.ToInt32(instruction_value);
                    } else if (type2 == PARAMDEF.DefType.u32) {
                        cell2.Value = Convert.ToUInt32(instruction_value);
                    } else if (type2 == PARAMDEF.DefType.f32) {
                        cell2.Value = Convert.ToSingle(instruction_value);
                    } else if (type2 == PARAMDEF.DefType.fixstr) {
                        cell2.Value = Convert.ToString(instruction_value);
                    } else if (type2 == PARAMDEF.DefType.fixstrW) {
                        cell2.Value = Convert.ToString(instruction_value);
                    }
                }
                index++;
            }
        }
    }
}
