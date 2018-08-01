using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;

namespace EntityViewerDaS3
{
    public partial class EnMainLoad : Form
    {
        public EnMainLoad()
        {
            InitializeComponent();
        }

        public static bool Is64Bit => IntPtr.Size == 8;

        public IntPtr ProcessHandle { get; private set; }

        public Process Process { get; private set; }

        public IntPtr BaseAddress { get; private set; }

        public IntPtr EntityBase { get; private set; }

        public IntPtr ThrowC { get; private set; }

        public IntPtr ThrowBase { get; private set; }

        public IntPtr AttachProc(string procName)
        {
            var ZeroRt = new IntPtr(0);
            var processes = System.Diagnostics.Process.GetProcessesByName(procName);
            if (processes.Length > 0)
            {
                var Process = processes[0];
                BaseAddress = Process.MainModule.BaseAddress;
                ProcessHandle = kernel32.OpenProcess(0x2 | 0x8 | 0x10 | 0x20 | 0x400, false, Process.Id);
                return ProcessHandle;
            }
            else
            {
                MessageBox.Show("Cant find process. Is it running?", "Process");
                return ZeroRt;
            }
        }

        private void EnMainLoad_Load(object sender, EventArgs e)
        {
            var delay = 10;
            mainTimer.Interval = delay;
            mainTimer.Start();

            ProcessHandle = AttachProc("DarkSoulsIII");
        }

        public byte ReadInt8(IntPtr address)
        {
            var readBuffer = new byte[sizeof(byte)];
            var success = kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)1, UIntPtr.Zero);
            var value = readBuffer[0];
            return value;
        }

        public short ReadInt16(IntPtr address)
        {
            var readBuffer = new byte[sizeof(short)];
            var success = kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)2, UIntPtr.Zero);
            var value = BitConverter.ToInt16(readBuffer, 0);
            return value;
        }

        public int ReadInt32(IntPtr address)
        {
            var readBuffer = new byte[sizeof(int)];
            var success = kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var value = BitConverter.ToInt32(readBuffer, 0);
            return value;
        }

        public long ReadInt64(IntPtr address)
        {
            var readBuffer = new byte[sizeof(long)];
            var success = kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var value = BitConverter.ToInt64(readBuffer, 0);
            return value;
        }

        public float ReadFloat(IntPtr address)
        {
            var readBuffer = new byte[sizeof(float)];
            var success = kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var value = BitConverter.ToSingle(readBuffer, 0);
            return value;
        }

        public double ReadDouble(IntPtr address)
        {
            var readBuffer = new byte[sizeof(double)];
            var success = kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var value = BitConverter.ToDouble(readBuffer, 0);
            return value;
        }

        public string ReadString(IntPtr address, int length, string encodingName)
        {
            var readBuffer = new byte[length];
            var success = kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
            var encodingType = System.Text.Encoding.GetEncoding(encodingName);
            string value = encodingType.GetString(readBuffer, 0, readBuffer.Length);

            return value;
        }

        //write to address
        public bool WriteInt8(IntPtr address, byte value)
        {
            return kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)1, UIntPtr.Zero);
        }

        public bool WriteInt16(IntPtr address, short value)
        {
            return kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)2, UIntPtr.Zero);
        }

        public bool WriteInt32(IntPtr address, int value)
        {
            return kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)4, UIntPtr.Zero);
        }

        public bool WriteInt64(IntPtr address, long value)
        {
            return kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)8, UIntPtr.Zero);
        }

        public bool WriteFloat(IntPtr address, float value)
        {
            return kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)4, UIntPtr.Zero);
        }

        public bool WriteDouble(IntPtr address, double value)
        {
            return kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)8, UIntPtr.Zero);
        }

        public bool WriteBytes(IntPtr addr, Byte[] val)
        {
            return kernel32.WriteProcessMemory(ProcessHandle, addr, val, new UIntPtr((uint)val.Length), UIntPtr.Zero);
        }

        private void UncheckAll(Control ctrl)
        {
            CheckBox chkBox = ctrl as CheckBox;
            if (chkBox == null)
            {
                foreach (Control child in ctrl.Controls)
                {
                    UncheckAll(child);
                }
            }
            else
            {
                chkBox.Checked = false;
            }
        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            if ((long)BaseAddress > 0)
            {

                if (ReadInt32(IntPtr.Add(BaseAddress, 0x494C768)) == (int)1)
                {
                    EMainControlTab.Enabled = true;
                }
                else
                {
                    EMainControlTab.Enabled = false;

                    UncheckAll(this);
                }
            }

            WriteInt8(BaseAddress + 0x5F7723, 0xC5);
            int MapOffset = (int)(MapUpDown.Value);
            MapOffset = 0xC8 + (MapOffset * 8);
            int EnemyOffset = (int)(EnemyUpDown.Value);
            EnemyOffset = EnemyOffset * 0x38;

            EntityBase = IntPtr.Add(BaseAddress, 0x4768E78);
            EntityBase = new IntPtr(ReadInt64(EntityBase));

            var MapEntries = EntityBase;

            EntityBase = IntPtr.Add(EntityBase, MapOffset);
            EntityBase = new IntPtr(ReadInt64(EntityBase));

            var MapDataPtr = EntityBase;

            EntityBase = IntPtr.Add(EntityBase, 0x88);
            EntityBase = new IntPtr(ReadInt64(EntityBase));
            EntityBase = IntPtr.Add(EntityBase, EnemyOffset);
            var EntityBase_ = new IntPtr(ReadInt64(EntityBase));

            var EventPtr = EntityBase_;
            EventPtr = IntPtr.Add(EventPtr, 0x30);
            EventPtr = new IntPtr(ReadInt64(EventPtr));
            long CheckValue = ReadInt64(EventPtr + 0x2A);

            bool IsHuman;

            if (CheckValue == 13511005043687472)
            {
                IsHuman = true;
            }
            else
            {
                IsHuman = false;
            }

            WriteFloat(BaseAddress + 0x3D6ACEC, (float)1.2);

            if ((long)ThrowC != (long)EntityBase_)
            {
                if ((long) ThrowBase > 0)
                {
                    WriteInt8(ThrowBase + 0xEC, 0);
                }
            }

            ThrowBase = EntityBase_;
            ThrowC = EntityBase_;

            ThrowBase = IntPtr.Add(ThrowBase, 0x1F90);
            ThrowBase = new IntPtr(ReadInt64(ThrowBase));
            ThrowBase = IntPtr.Add(ThrowBase, 0x88);
            ThrowBase = new IntPtr(ReadInt64(ThrowBase));
            ThrowBase = IntPtr.Add(ThrowBase, 0x28);
            ThrowBase = new IntPtr(ReadInt64(ThrowBase));
            WriteInt8(ThrowBase + 0xEC, 1);

            switch (EMainControlTab.SelectedIndex)
            {
                case 0:
                    {
                        int MapCountL = ReadInt32(MapEntries + 0xC0);
                        int EnemyCountL = ReadInt32(MapDataPtr + 0x80);
                        var MSBPtr = IntPtr.Add(EntityBase_, 0x1F90);
                        MSBPtr = new IntPtr(ReadInt64(MSBPtr));
                        MSBPtr = IntPtr.Add(MSBPtr, 0x18);
                        MSBPtr = new IntPtr(ReadInt64(MSBPtr));
                        var LoadedPtr = IntPtr.Add(EntityBase_, 0x30);
                        LoadedPtr = new IntPtr(ReadInt64(LoadedPtr));
                        var MapPtr = IntPtr.Add(MapDataPtr, 0x8);
                        MapPtr = new IntPtr(ReadInt64(MapPtr));
                        int MapIdVal = ReadInt32(MapPtr + 0x8);

                        if (EMainControlTab.Enabled == true)
                        {
                            MapUpDown.Maximum = MapCountL - 1;
                            EnemyUpDown.Maximum = EnemyCountL - 1;
                        }

                        mapCountLabel.Text = MapCountL.ToString();
                        enemyCountLabel.Text = EnemyCountL.ToString();
                        loadedLabel.Text = (ReadInt8(LoadedPtr + 0x84)).ToString();
                        MSBIdLabel.Text = ReadString(MSBPtr + 0x130, 20, "UNICODE");

                        switch (MapIdVal)
                        {
                            case 0x1E000000:
                                {
                                    MapNameLabel.Text = "High Wall of Lothric";
                                    break;
                                }
                            case 0x1E010000:
                                {
                                    MapNameLabel.Text = "Lothric Castle";
                                    break;
                                }
                            case 0x1F000000:
                                {
                                    MapNameLabel.Text = "Undead Settlement";
                                    break;
                                }
                            case 0x20000000:
                                {
                                    MapNameLabel.Text = "Archdragon Peak";
                                    break;
                                }
                            case 0x21000000:
                                {
                                    MapNameLabel.Text = "Road of Sacrifices";
                                    break;
                                }
                            case 0x22010000:
                                {
                                    MapNameLabel.Text = "Grand Archives";
                                    break;
                                }
                            case 0x23000000:
                                {
                                    MapNameLabel.Text = "Cathedral of the Deep";
                                    break;
                                }
                            case 0x25000000:
                                {
                                    MapNameLabel.Text = "Irithyll of the Boreal Valley";
                                    break;
                                }
                            case 0x26000000:
                                {
                                    MapNameLabel.Text = "Catacombs of Carthus";
                                    break;
                                }
                            case 0x27000000:
                                {
                                    MapNameLabel.Text = "Irithyll Dungeon";
                                    break;
                                }
                            case 0x28000000:
                                {
                                    MapNameLabel.Text = "Untended Graves";
                                    break;
                                }
                            case 0x29000000:
                                {
                                    MapNameLabel.Text = "Kiln of the First Flame";
                                    break;
                                }
                            case 0x2D000000:
                                {
                                    MapNameLabel.Text = "Painted World of Ariandel";
                                    break;
                                }
                            case 0x2E000000:
                                {
                                    MapNameLabel.Text = "Grand Rooftop (PvP)";
                                    break;
                                }
                            case 0x2F000000:
                                {
                                    MapNameLabel.Text = "Kiln (PvP)";
                                    break;
                                }
                            case 0x32000000:
                                {
                                    MapNameLabel.Text = "The Dreg Heap";
                                    break;
                                }
                            case 0x33000000:
                                {
                                    MapNameLabel.Text = "The Ringed City";
                                    break;
                                }
                            case 0x33010000:
                                {
                                    MapNameLabel.Text = "Filianore's Rest";
                                    break;
                                }
                            case 0x35000000:
                                {
                                    MapNameLabel.Text = "Dragon Ruins (PvP)";
                                    break;
                                }
                            case 0x36000000:
                                {
                                    MapNameLabel.Text = "Round Plaza (PvP)";
                                    break;
                                }
                        }

                        
                        break;
                    }
                case 1:
                    {
                        //DrawGroup
                        maskDraw1.Text = (ReadInt32(EntityBase_ + 0x1A7C)).ToString("X");
                        maskDraw2.Text = (ReadInt32(EntityBase_ + 0x1A80)).ToString("X");
                        maskDraw3.Text = (ReadInt32(EntityBase_ + 0x1A84)).ToString("X");
                        maskDraw4.Text = (ReadInt32(EntityBase_ + 0x1A88)).ToString("X");
                        //DispGroup
                        DispDraw1.Text = (ReadInt32(EntityBase_ + 0x1A8C)).ToString("X");
                        dispDraw2.Text = (ReadInt32(EntityBase_ + 0x1A90)).ToString("X");
                        dispDraw3.Text = (ReadInt32(EntityBase_ + 0x1A94)).ToString("X");
                        dispDraw4.Text = (ReadInt32(EntityBase_ + 0x1A98)).ToString("X");
                        //Other
                        playRegion_.Text = (ReadInt32(EntityBase_ + 0x1ABC)).ToString();

                        var DrawPtr1 = IntPtr.Add(EntityBase_, 0x1F90);
                        DrawPtr1 = new IntPtr(ReadInt64(DrawPtr1));
                        var DrawPtr2 = DrawPtr1;

                        DrawPtr1 = IntPtr.Add(DrawPtr1, 0x68);
                        DrawPtr1 = new IntPtr(ReadInt64(DrawPtr1));

                        DrawPtr2 = IntPtr.Add(DrawPtr2, 0xA0);
                        DrawPtr2 = new IntPtr(ReadInt64(DrawPtr2));

                        FloorT.Text = (ReadInt32(DrawPtr1 + 0x1F0)).ToString();
                        FloorAlphaT.Text = (ReadFloat(EntityBase_ + 0x1AD8)).ToString();
                        MapNameIdT.Text = (ReadInt16(DrawPtr2 + 0x10)).ToString();
                        armorSE1.Text = (ReadInt16(DrawPtr2 + 0x12)).ToString();
                        armoreSFX1.Text = (ReadInt16(DrawPtr2 + 0x14)).ToString();
                        armorSE2.Text = (ReadInt16(DrawPtr2 + 0x16)).ToString();
                        armorSFX2.Text = (ReadInt16(DrawPtr2 + 0x18)).ToString();
                        break;
                    }
                case 2:
                    {
                        //Flags
                        var EnableLogic = IntPtr.Add(EntityBase_, 0x50);
                        EnableLogic = new IntPtr(ReadInt64(EnableLogic));
                        WriteInt8(EnableLogic + 0x182, (byte)numericLogic.Value); 

                        var FlagBase1 = IntPtr.Add(EntityBase_, 0x1F90);
                        FlagBase1 = new IntPtr(ReadInt64(FlagBase1));
                        FlagBase1 = IntPtr.Add(FlagBase1, 0x18);
                        FlagBase1 = new IntPtr(ReadInt64(FlagBase1));

                        var NoDeadT = 4;
                        NoDeadT = (byte)numericNoDead.Value * NoDeadT;
                        var NoDamageT = 8;
                        NoDamageT = (byte)numericNoDamage.Value * NoDamageT;

                        NoDamageT = NoDamageT + NoDeadT;

                        WriteInt8(FlagBase1 + 0x1C0, (byte)NoDamageT);

                        var NoUpdateT = 8;
                        NoUpdateT = (byte)numericNoUpdate.Value * NoUpdateT;
                        WriteInt8(EntityBase_ + 0x1EE9, (byte)NoUpdateT);

                        var NoHitT = 32;
                        NoHitT = (byte)numericNoHit.Value * NoHitT;
                        var NoAttackT = 64;
                        NoAttackT = (byte)numericNoAttack.Value * NoAttackT;
                        var NoMoveT = 128;
                        NoMoveT = (byte)numericNoMove.Value * NoMoveT;
                        NoMoveT = NoMoveT + NoAttackT + NoHitT;
                        WriteInt8(EntityBase_ + 0x1EE8, (byte)NoMoveT);

                        var NoGravity = 64;
                        NoGravity = (byte)numericNoGrav.Value * NoGravity;

                        WriteInt8(EntityBase_ + 0x1A08, (byte)NoGravity);

                        var MapHit = 8;
                        MapHit = (byte)numericMapHit.Value * MapHit;
                        WriteInt8(EnableLogic + 0x186, (byte)MapHit);

                        //Stats
                        var HpBase = EntityBase_;
                        HpBase = IntPtr.Add(HpBase, 0x1F90);
                        HpBase = new IntPtr(ReadInt64(HpBase));
                        HpBase = IntPtr.Add(HpBase, 0x18);
                        HpBase = new IntPtr(ReadInt64(HpBase));

                        hpBox.Text = (ReadInt32(HpBase + 0xD8)).ToString();
                        hpMaxBox.Text = (ReadInt32(HpBase + 0xE0)).ToString();
                        staminaBox.Text = (ReadInt32(HpBase + 0xF0)).ToString();
                        StaminaMaxBox.Text = (ReadInt32(HpBase + 0xF8)).ToString();
                        //Attributes
                        var AttrBase = EntityBase_;
                        AttrBase = IntPtr.Add(AttrBase, 0x1FA0);
                        AttrBase = new IntPtr(ReadInt64(AttrBase));

                        if (IsHuman == true)
                        {
                            Attrs.Enabled = true;
                            VigorUp.Value = (ReadInt8(AttrBase + 0x44));
                            AttunementUp.Value = (ReadInt8(AttrBase + 0x48));
                            EnduranceUp.Value = (ReadInt8(AttrBase + 0x4C));
                            VitalityUp.Value = (ReadInt8(AttrBase + 0x6C));
                            StrengthUp.Value = (ReadInt8(AttrBase + 0x50));
                            DexUp.Value = (ReadInt8(AttrBase + 0x54));
                            IntelligenceUp.Value = (ReadInt8(AttrBase + 0x58));
                            FaithUp.Value = (ReadInt8(AttrBase + 0x5C));
                            LuckUp.Value = (ReadInt8(AttrBase + 0x60));
                            LvlUp.Value = (ReadInt32(AttrBase + 0x70));

                            IsHuman = false;
                        }
                        else
                        {
                            Attrs.Enabled = false;
                        }

                        //Resists
                        var ResistBase = EntityBase_;
                        ResistBase = IntPtr.Add(ResistBase, 0x1F90);
                        ResistBase = new IntPtr(ReadInt64(ResistBase));
                        ResistBase = IntPtr.Add(ResistBase, 0x20);
                        ResistBase = new IntPtr(ReadInt64(ResistBase));

                        poisonUp.Value = (ReadInt32(ResistBase + 0x10));
                        PoisonMaxUp.Value = (ReadInt32(ResistBase + 0x24));
                        toxicUp.Value = (ReadInt32(ResistBase + 0x14));
                        ToxicMaxUp.Value = (ReadInt32(ResistBase + 0x28));
                        BleedUp.Value = (ReadInt32(ResistBase + 0x18));
                        BleedMaxUp.Value = (ReadInt32(ResistBase + 0x2C));
                        CurseUp.Value = (ReadInt32(ResistBase + 0x1C));
                        CurseMaxUp.Value = (ReadInt32(ResistBase + 0x30));
                        FrostUp.Value = (ReadInt32(ResistBase + 0x20));
                        FrostMaxUp.Value = (ReadInt32(ResistBase + 0x34));

                        //BasicInfo
                        handleBox.Text = (ReadInt32(EntityBase_ + 0x8)).ToString("X");
                        CHRTYPEUP.Value = ReadInt32(EntityBase_ + 0x70);
                        TEAMTYPEUP.Value = ReadInt32(EntityBase_ + 0x74);

                        var EventBase_ = EntityBase_;

                        if (IsHuman == true)
                        {
                            EventBase_ = IntPtr.Add(EventBase_, 0x2090);
                            EventBase_ = new IntPtr(ReadInt64(EventBase_));
                            eventIdMain.Text = (ReadInt32(EventBase_ + 0x8)).ToString();
                        }
                        else
                        {
                            EventBase_ = IntPtr.Add(EventBase_, 0x1FE0);
                            EventBase_ = new IntPtr(ReadInt64(EventBase_));
                            eventIdMain.Text = (ReadInt32(EventBase_ + 0x8)).ToString();
                        }

                        var PosBase = EntityBase_;
                        PosBase = IntPtr.Add(PosBase, 0x1F90);
                        PosBase = new IntPtr(ReadInt64(PosBase));
                        PosBase = IntPtr.Add(PosBase, 0x68);
                        PosBase = new IntPtr(ReadInt64(PosBase));

                        posX.Text = (ReadFloat(PosBase + 0x90)).ToString();
                        posZ.Text = (ReadFloat(PosBase + 0x94)).ToString();
                        posY.Text = (ReadFloat(PosBase + 0x98)).ToString();

                        var AiBase = EntityBase_;
                        AiBase = IntPtr.Add(AiBase, 0x50);
                        AiBase = new IntPtr(ReadInt64(AiBase));
                        AiBase = IntPtr.Add(AiBase, 0x110);
                        AiBase = new IntPtr(ReadInt64(AiBase));

                        AiSet1.Text = (ReadInt32(AiBase + 0x390)).ToString();
                        AiSet2.Text = (ReadInt32(AiBase + 0x394)).ToString();
                        MovDirx.Text = (ReadFloat(AiBase + 0x10)).ToString();
                        MovDiry.Text = (ReadFloat(AiBase + 0x18)).ToString();

                        break;
                    }
                case 3:
                    {
                        var BaseAnim = EntityBase_;
                        BaseAnim = IntPtr.Add(BaseAnim, 0x1F90);
                        BaseAnim = new IntPtr(ReadInt64(BaseAnim));
                        var BaseAnim2 = IntPtr.Add(BaseAnim, 0x28);
                        BaseAnim2 = new IntPtr(ReadInt64(BaseAnim2));

                        BaseAnim = IntPtr.Add(BaseAnim, 0x80);
                        BaseAnim = new IntPtr(ReadInt64(BaseAnim));

                        animName.Text = ReadString(BaseAnim2 + 0x898, 32 * 2, "UNICODE");
                        lastAnim.Text = (ReadInt32(BaseAnim + 0xC8)).ToString();

                        var RagdollBase = EntityBase_;
                        RagdollBase = IntPtr.Add(RagdollBase, 0x50);
                        RagdollBase = new IntPtr(ReadInt64(RagdollBase));
                        RagdollBase = IntPtr.Add(RagdollBase, 0x20);
                        RagdollBase = new IntPtr(ReadInt64(RagdollBase));

                        WriteInt8(BaseAddress + 0x457D6A8, (byte)EnablePenetrateRag.Value);
                        WriteInt32(RagdollBase + 0xE4, (int)ragdollUp.Value);

                        break;
                    }
                case 4:
                    {
                        break;
                    }
                case 5:
                    {

                        //Chr Asm
                        if (IsHuman == true)
                        {
                            chrAsmMain.Enabled = true;
                            ChrAsmFlag.Value = ReadInt8(EntityBase_ + 0x2098);

                            var ChrAsmBase = EntityBase_;
                            ChrAsmBase = IntPtr.Add(ChrAsmBase, 0x1FA0);
                            ChrAsmBase = new IntPtr(ReadInt64(ChrAsmBase));

                            armStyle.Value = ReadInt32(ChrAsmBase + 0x2B8);

                            VowId.Text = (ReadInt32(ChrAsmBase + 0x328)).ToString("X");
                            LeftHand1.Value = ReadInt32(ChrAsmBase + 0x32C);
                            RightHand1.Value = ReadInt32(ChrAsmBase + 0x330);
                            LeftHand2.Value = ReadInt32(ChrAsmBase + 0x334);
                            RightHand2.Value = ReadInt32(ChrAsmBase + 0x338);
                            LeftHand3.Value = ReadInt32(ChrAsmBase + 0x33C);
                            RightHand3.Value = ReadInt32(ChrAsmBase + 0x340);
                            ArrowSlot1.Value = ReadInt32(ChrAsmBase + 0x344);
                            BoltSlot1.Value = ReadInt32(ChrAsmBase + 0x348);
                            ArrowSlot2.Value = ReadInt32(ChrAsmBase + 0x34C);
                            BoltSlot2.Value = ReadInt32(ChrAsmBase + 0x350);
                            Head.Value = ReadInt32(ChrAsmBase + 0x35C);
                            Chest.Value = ReadInt32(ChrAsmBase + 0x360);
                            Arms.Value = ReadInt32(ChrAsmBase + 0x364);
                            Legs.Value = ReadInt32(ChrAsmBase + 0x368);
                            RingSlot1.Value = ReadInt32(ChrAsmBase + 0x370);
                            RingSlot2.Value = ReadInt32(ChrAsmBase + 0x374);
                            RingSlot3.Value = ReadInt32(ChrAsmBase + 0x378);
                            RingSlot4.Value = ReadInt32(ChrAsmBase + 0x37C);
                            IsHuman = false;

                        }
                        else
                        {
                            chrAsmMain.Enabled = false;
                        }
                        break;
                    }
                case 6:
                    {
                        var ThrowNodeBase = EntityBase_;
                        ThrowNodeBase = IntPtr.Add(ThrowNodeBase, 0x1F90);
                        ThrowNodeBase = new IntPtr(ReadInt64(ThrowNodeBase));
                        ThrowNodeBase = IntPtr.Add(ThrowNodeBase, 0x88);
                        ThrowNodeBase = new IntPtr(ReadInt64(ThrowNodeBase));
                        ThrowNodeBase = IntPtr.Add(ThrowNodeBase, 0x28);
                        ThrowNodeBase = new IntPtr(ReadInt64(ThrowNodeBase));

                        ThrowParamId.Text = (ReadInt32(ThrowNodeBase + 0x10)).ToString();
                        ThrowState.Text = (ReadInt8(ThrowNodeBase + 0x18)).ToString();
                        ThrowEscHp.Text = (ReadInt8(ThrowNodeBase + 0x1A)).ToString();
                        ThrowHandle.Text = (ReadInt32(ThrowNodeBase + 0x20)).ToString("X");

                        attackX.Text = (ReadFloat(ThrowNodeBase + 0xA0)).ToString();
                        attackZ.Text = (ReadFloat(ThrowNodeBase + 0xA4)).ToString();
                        attackY.Text = (ReadFloat(ThrowNodeBase + 0xA8)).ToString();

                        defenderX.Text = (ReadFloat(ThrowNodeBase + 0xB0)).ToString();
                        defenderZ.Text = (ReadFloat(ThrowNodeBase + 0xB4)).ToString();
                        defenderY.Text = (ReadFloat(ThrowNodeBase + 0xB8)).ToString();
                        break;
                    }
            }
        }

        private void takeCtrl_Click(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x48, 0xA1, 0, 0, 0, 0, 0, 0, 0, 0, 0x48, 0x8B, 0xC8, 0xBA, 0x00, 0x00, 0x00, 0x00, 0x49, 0xBE, 0x00, 0xA7, 0x88, 0x40, 0x01, 0x00, 0x00, 0x00, 0x48, 0x83, 0xEC, 0x28, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x28, 0xC3 };
            var bytes2 = new byte[7];
            var bytjmp = 0x2;

            var EnableLogic = new IntPtr(ReadInt64(EntityBase));
            EnableLogic = IntPtr.Add(EnableLogic, 0x50);
            EnableLogic = new IntPtr(ReadInt64(EnableLogic));
            WriteInt8(EnableLogic + 0x182, 1);

            var buffer = 512;
            var address = kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            bytes2 = BitConverter.GetBytes((long)EntityBase);
            Array.Copy(bytes2, 0, bytes, bytjmp, bytes2.Length);


            if (address != IntPtr.Zero)
            {
                if (kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {
                    var threadHandle = kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
            }
        }

        private void InitPosCtrl_Click(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x48, 0xA1, 0, 0, 0, 0, 0, 0, 0, 0, 0x48, 0x8B, 0xC8, 0xBA, 0x29, 0x00, 0x00, 0x00, 0x49, 0xBE, 0x00, 0xA7, 0x88, 0x40, 0x01, 0x00, 0x00, 0x00, 0x48, 0x83, 0xEC, 0x28, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x28, 0xC3 };
            var bytes2 = new byte[7];
            var bytjmp = 0x2;

            var buffer = 512;
            var address = kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            bytes2 = BitConverter.GetBytes((long)EntityBase);
            Array.Copy(bytes2, 0, bytes, bytjmp, bytes2.Length);


            if (address != IntPtr.Zero)
            {
                if (kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {
                    var threadHandle = kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
            }
        }

        private void RstrCtrl_Click(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x48, 0xA1, 0, 0, 0, 0, 0, 0, 0, 0, 0x48, 0x8B, 0xC8, 0xBA, 0x28, 0x00, 0x00, 0x00, 0x49, 0xBE, 0x00, 0xA7, 0x88, 0x40, 0x01, 0x00, 0x00, 0x00, 0x48, 0x83, 0xEC, 0x28, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x28, 0xC3 };
            var bytes2 = new byte[7];
            var bytjmp = 0x2;

            var buffer = 512;
            var address = kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            bytes2 = BitConverter.GetBytes((long)EntityBase);
            Array.Copy(bytes2, 0, bytes, bytjmp, bytes2.Length);


            if (address != IntPtr.Zero)
            {
                if (kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {
                    var threadHandle = kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
            }
        }

        private void ghostUpDown_ValueChanged(object sender, EventArgs e)
        {
            var EntityBase_ = new IntPtr(ReadInt64(EntityBase));
            WriteFloat(EntityBase_ + 0x1A44, (float)ghostUpDown.Value);
        }

        private void reloadChar_Click(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x48, 0xA1, 0, 0, 0, 0, 0, 0, 0, 0, 0x48, 0x8B, 0xC8, 0xBA, 0x82, 0x00, 0x00, 0x00, 0x49, 0xBE, 0x00, 0xA7, 0x88, 0x40, 0x01, 0x00, 0x00, 0x00, 0x48, 0x83, 0xEC, 0x28, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x28, 0xC3 };
            var bytes2 = new byte[7];
            var bytjmp = 0x2;

            var buffer = 512;
            var address = kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            bytes2 = BitConverter.GetBytes((long)EntityBase);
            Array.Copy(bytes2, 0, bytes, bytjmp, bytes2.Length);


            if (address != IntPtr.Zero)
            {
                if (kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {
                    var threadHandle = kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
            }
        }

        private void KillEnemy_Click(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x48, 0xA1, 0, 0, 0, 0, 0, 0, 0, 0, 0x48, 0x8B, 0xC8, 0xBA, 0x06, 0x00, 0x00, 0x00, 0x49, 0xBE, 0x00, 0xA7, 0x88, 0x40, 0x01, 0x00, 0x00, 0x00, 0x48, 0x83, 0xEC, 0x28, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x28, 0xC3 };
            var bytes2 = new byte[7];
            var bytjmp = 0x2;

            var buffer = 512;
            var address = kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            bytes2 = BitConverter.GetBytes((long)EntityBase);
            Array.Copy(bytes2, 0, bytes, bytjmp, bytes2.Length);


            if (address != IntPtr.Zero)
            {
                if (kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {
                    var threadHandle = kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
            }
        }

        private void HideEnemy_Click(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x48, 0xA1, 0, 0, 0, 0, 0, 0, 0, 0, 0x48, 0x8B, 0xC8, 0xBA, 0x79, 0x00, 0x00, 0x00, 0x49, 0xBE, 0x00, 0xA7, 0x88, 0x40, 0x01, 0x00, 0x00, 0x00, 0x48, 0x83, 0xEC, 0x28, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x28, 0xC3 };
            var bytes2 = new byte[7];
            var bytjmp = 0x2;

            var buffer = 512;
            var address = kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            bytes2 = BitConverter.GetBytes((long)EntityBase);
            Array.Copy(bytes2, 0, bytes, bytjmp, bytes2.Length);


            if (address != IntPtr.Zero)
            {
                if (kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {
                    var threadHandle = kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
            }
        }
        //SHIT  
        private void CHRTYPEUP_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            WriteInt32(Type + 0x70, (int)CHRTYPEUP.Value);
        }

        private void TEAMTYPEUP_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            WriteInt32(Type + 0x74, (int)TEAMTYPEUP.Value);
        }

        private void VigorUp_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x1FA0);
            Type = new IntPtr(ReadInt64(Type));
            WriteInt8(Type + 0x44, (byte)VigorUp.Value);
        }

        private void AttunementUp_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x1FA0);
            Type = new IntPtr(ReadInt64(Type));
            WriteInt8(Type + 0x48, (byte)AttunementUp.Value);
        }

        private void EnduranceUp_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x1FA0);
            Type = new IntPtr(ReadInt64(Type));
            WriteInt8(Type + 0x4C, (byte)EnduranceUp.Value);
        }

        private void VitalityUp_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x1FA0);
            Type = new IntPtr(ReadInt64(Type));
            WriteInt8(Type + 0x6C, (byte)VitalityUp.Value);
        }

        private void StrengthUp_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x1FA0);
            Type = new IntPtr(ReadInt64(Type));
            WriteInt8(Type + 0x50, (byte)StrengthUp.Value);
        }

        private void DexUp_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x1FA0);
            Type = new IntPtr(ReadInt64(Type));
            WriteInt8(Type + 0x54, (byte)DexUp.Value);
        }

        private void IntelligenceUp_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x1FA0);
            Type = new IntPtr(ReadInt64(Type));
            WriteInt8(Type + 0x58, (byte)IntelligenceUp.Value);
        }

        private void FaithUp_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x1FA0);
            Type = new IntPtr(ReadInt64(Type));
            WriteInt8(Type + 0x5C, (byte)FaithUp.Value);
        }

        private void LuckUp_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x1FA0);
            Type = new IntPtr(ReadInt64(Type));
            WriteInt8(Type + 0x60, (byte)LuckUp.Value);
        }

        private void LvlUp_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x1FA0);
            Type = new IntPtr(ReadInt64(Type));
            WriteInt32(Type + 0x70, (int)LvlUp.Value);
        }

        private void debugPlaySpeed_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x1F90);
            Type = new IntPtr(ReadInt64(Type));
            Type = IntPtr.Add(Type, 0x28);
            Type = new IntPtr(ReadInt64(Type));
            WriteFloat(Type + 0xA58, (float)debugPlaySpeed.Value);
        }

        private void stayAnim_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x1F90);
            Type = new IntPtr(ReadInt64(Type));
            Type = IntPtr.Add(Type, 0x58);
            Type = new IntPtr(ReadInt64(Type));
            WriteInt32(Type + 0x20, (int)stayAnim.Value);
        }

        private void damageLabel_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x1F90);
            Type = new IntPtr(ReadInt64(Type));
            Type = IntPtr.Add(Type, 0x0);
            Type = new IntPtr(ReadInt64(Type));
            WriteInt32(Type + 0x1C, (int)damageLabel.Value);
        }

        private void loadEffects_Click(object sender, EventArgs e)
        {
            dataGridEffects.Columns.Clear();
            dataGridEffects.Rows.Clear();

            var SpEffectBase = new IntPtr (ReadInt64(EntityBase));
            SpEffectBase = IntPtr.Add(SpEffectBase, 0x11C8);
            SpEffectBase = new IntPtr(ReadInt64(SpEffectBase));
            SpEffectBase = IntPtr.Add(SpEffectBase, 0x8);
            SpEffectBase = new IntPtr(ReadInt64(SpEffectBase));

            dataGridEffects.ColumnCount = 1;
            dataGridEffects.Columns[0].Name = "SpEffect";

            int SpEffectID;
            string strN;
            long quitOffset;

            dataGridEffects.ColumnCount = 1;
            dataGridEffects.Columns[0].Name = "SpEffect";

            while (true)
            {
                SpEffectID = (ReadInt32(IntPtr.Add(SpEffectBase, 0x60)));

                strN = SpEffectID.ToString();
                dataGridEffects.Rows.Add(strN);

                quitOffset = ReadInt64(IntPtr.Add(SpEffectBase, 0x78));

                if (quitOffset == 0)
                    break;

                SpEffectBase = IntPtr.Add(SpEffectBase, 0x78);
                SpEffectBase = new IntPtr(ReadInt64(SpEffectBase));
            }
        }

        private void effectDebug_ValueChanged(object sender, EventArgs e)
        {
            var Type = new IntPtr(ReadInt64(EntityBase));
            Type = IntPtr.Add(Type, 0x11C8);
            Type = new IntPtr(ReadInt64(Type));
            WriteInt32(Type + 0x30, (int)effectDebug.Value);
        }

        private void addEff_Click(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x48, 0xA1, 0, 0, 0, 0, 0, 0, 0, 0, 0x48, 0x8B, 0xC8, 0xBA, 0x00, 0x00, 0x00, 0x00, 0x49, 0xBE, 0x30, 0x3C, 0x9F, 0x40, 0x01, 0x00, 0x00, 0x00, 0x48, 0x83, 0xEC, 0x28, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x28, 0xC3 };
            var bytes2 = new byte[7];
            var bytjmp = 0x2;

            var buffer = 512;
            var address = kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            var EffectBase = new IntPtr(ReadInt64(EntityBase));
            EffectBase = IntPtr.Add(EffectBase, 0x11C8);

            bytes2 = BitConverter.GetBytes((long)EffectBase);
            Array.Copy(bytes2, 0, bytes, bytjmp, bytes2.Length);


            if (address != IntPtr.Zero)
            {
                if (kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {
                    var threadHandle = kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
            }
        }

        private void EraseEff_Click(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x48, 0xA1, 0, 0, 0, 0, 0, 0, 0, 0, 0x48, 0x8B, 0xC8, 0xBA, 0x03, 0x00, 0x00, 0x00, 0x49, 0xBE, 0x30, 0x3C, 0x9F, 0x40, 0x01, 0x00, 0x00, 0x00, 0x48, 0x83, 0xEC, 0x28, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x28, 0xC3 };
            var bytes2 = new byte[7];
            var bytjmp = 0x2;

            var buffer = 512;
            var address = kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            var EffectBase = new IntPtr(ReadInt64(EntityBase));
            EffectBase = IntPtr.Add(EffectBase, 0x11C8);

            bytes2 = BitConverter.GetBytes((long)EffectBase);
            Array.Copy(bytes2, 0, bytes, bytjmp, bytes2.Length);


            if (address != IntPtr.Zero)
            {
                if (kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {
                    var threadHandle = kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
            }
        }

        private void EraseAll_Click(object sender, EventArgs e)
        {
            var bytes = new byte[] { 0x48, 0xA1, 0, 0, 0, 0, 0, 0, 0, 0, 0x48, 0x8B, 0xC8, 0xBA, 0x01, 0x00, 0x00, 0x00, 0x49, 0xBE, 0x30, 0x3C, 0x9F, 0x40, 0x01, 0x00, 0x00, 0x00, 0x48, 0x83, 0xEC, 0x28, 0x41, 0xFF, 0xD6, 0x48, 0x83, 0xC4, 0x28, 0xC3 };
            var bytes2 = new byte[7];
            var bytjmp = 0x2;

            var buffer = 512;
            var address = kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

            var EffectBase = new IntPtr(ReadInt64(EntityBase));
            EffectBase = IntPtr.Add(EffectBase, 0x11C8);

            bytes2 = BitConverter.GetBytes((long)EffectBase);
            Array.Copy(bytes2, 0, bytes, bytjmp, bytes2.Length);


            if (address != IntPtr.Zero)
            {
                if (kernel32.WriteProcessMemory(ProcessHandle, address, bytes, new UIntPtr((uint)bytes.Length), UIntPtr.Zero))
                {
                    var threadHandle = kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        kernel32.WaitForSingleObject(threadHandle, 30000);
                    }
                }
                kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
            }
        }

        private void ChrAsmFlag_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            WriteInt8(ChrAsm + 0x2098, (byte)ChrAsmFlag.Value);
            
        }

        private void armStyle_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x2B8, (int)armStyle.Value);
        }

        private void LeftHand1_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x32C, (int)LeftHand1.Value);
        }

        private void LeftHand2_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x334, (int)LeftHand2.Value);
        }

        private void LeftHand3_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x33C, (int)LeftHand3.Value);
        }

        private void ArrowSlot1_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x344, (int)ArrowSlot1.Value);
        }

        private void ArrowSlot2_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x34C, (int)ArrowSlot2.Value);
        }

        private void Head_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x35C, (int)Head.Value);
        }

        private void Arms_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x364, (int)Arms.Value);
        }

        private void RingSlot1_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x370, (int)RingSlot1.Value);
        }

        private void RingSlot3_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x378, (int)RingSlot3.Value);
        }

        private void RightHand1_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x330, (int)RightHand1.Value);
        }

        private void RightHand2_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x338, (int)RightHand2.Value);

        }

        private void RightHand3_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x340, (int)RightHand3.Value);
        }

        private void BoltSlot1_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x348, (int)BoltSlot1.Value);
        }

        private void BoltSlot2_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x350, (int)BoltSlot2.Value);
        }

        private void Chest_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x360, (int)Chest.Value);

        }

        private void Legs_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x368, (int)Legs.Value);
        }

        private void RingSlot2_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x374, (int)RingSlot2.Value);
        }

        private void RingSlot4_ValueChanged(object sender, EventArgs e)
        {
            var ChrAsm = new IntPtr(ReadInt64(EntityBase));
            ChrAsm = IntPtr.Add(ChrAsm, 0x1FA0);
            ChrAsm = new IntPtr(ReadInt64(ChrAsm));
            WriteInt32(ChrAsm + 0x37C, (int)RingSlot4.Value);
        }
    }
}
