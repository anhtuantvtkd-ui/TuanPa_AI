
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.8.3928.0
//  Copyright (c) Microsoft Corporation.  All rights reserved.



// Metadata version: v4.0.30319
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly extern System.Windows.Forms
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly extern Acdbmgd
{
  .ver 24:0:0:0
}
.assembly extern accoremgd
{
  .ver 24:0:0:0
}
.assembly extern System.Drawing
{
  .publickeytoken = (B0 3F 5F 7F 11 D5 0A 3A )                         // .?_....:
  .ver 4:0:0:0
}
.assembly extern Acmgd
{
  .ver 24:0:0:0
}
.assembly ToadoPart
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module ToadoPart.dll
// MVID: {8EA03387-957C-4926-B420-D07410055FC6}
.imagebase 0x10000000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x05D40000


// =============== CLASS MEMBERS DECLARATION ===================

.class public auto ansi sealed beforefieldinit Tvxd.Meps.ToadoSettings
       extends [mscorlib]System.Object
{
  .field public string Style
  .field public string Layer
  .field public float64 Height
  .field public float64 Rotation
  .field public float64 ArrowSize
  .field public float64 Distance
  .field public int32 Decimals
  .field public int32 Corner
  .field public bool Vn2000
  .field public bool TwoLines
  .field public bool Mask
  .field public bool Frame
  .field public bool Arrow
  .field public bool Center
  .field public bool AddName
  .field public bool PickEach
  .field public bool Reformat
  .field public bool UnitIsMm
  .field public bool CircleArrow
  .field public bool UnderlineBetween
  .method public hidebysig specialname static 
          string  get_IniPath() cil managed
  {
    // Code size       57 (0x39)
    .maxstack  3
    .locals init (string V_0,
             string V_1)
    IL_0000:  nop
    IL_0001:  ldc.i4.s   28
    IL_0003:  call       string [mscorlib]System.Environment::GetFolderPath(valuetype [mscorlib]System.Environment/SpecialFolder)
    IL_0008:  ldstr      "TVXD"
    IL_000d:  ldstr      "MEPS060"
    IL_0012:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string,
                                                                  string)
    IL_0017:  stloc.0
    .try
    {
      IL_0018:  nop
      IL_0019:  ldloc.0
      IL_001a:  call       class [mscorlib]System.IO.DirectoryInfo [mscorlib]System.IO.Directory::CreateDirectory(string)
      IL_001f:  pop
      IL_0020:  nop
      IL_0021:  leave.s    IL_0028

    }  // end .try
    catch [mscorlib]System.Object 
    {
      IL_0023:  pop
      IL_0024:  nop
      IL_0025:  nop
      IL_0026:  leave.s    IL_0028

    }  // end handler
    IL_0028:  nop
    IL_0029:  ldloc.0
    IL_002a:  ldstr      "toado.ini"
    IL_002f:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_0034:  stloc.1
    IL_0035:  br.s       IL_0037

    IL_0037:  ldloc.1
    IL_0038:  ret
  } // end of method ToadoSettings::get_IniPath

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       188 (0xbc)
    .maxstack  2
    IL_0000:  ldarg.0
    IL_0001:  ldstr      "Tuan_arial"
    IL_0006:  stfld      string Tvxd.Meps.ToadoSettings::Style
    IL_000b:  ldarg.0
    IL_000c:  ldstr      "TOA_DO"
    IL_0011:  stfld      string Tvxd.Meps.ToadoSettings::Layer
    IL_0016:  ldarg.0
    IL_0017:  ldc.r8     2.5
    IL_0020:  stfld      float64 Tvxd.Meps.ToadoSettings::Height
    IL_0025:  ldarg.0
    IL_0026:  ldc.r8     0.0
    IL_002f:  stfld      float64 Tvxd.Meps.ToadoSettings::Rotation
    IL_0034:  ldarg.0
    IL_0035:  ldc.r8     1.5
    IL_003e:  stfld      float64 Tvxd.Meps.ToadoSettings::ArrowSize
    IL_0043:  ldarg.0
    IL_0044:  ldc.r8     15.
    IL_004d:  stfld      float64 Tvxd.Meps.ToadoSettings::Distance
    IL_0052:  ldarg.0
    IL_0053:  ldc.i4.3
    IL_0054:  stfld      int32 Tvxd.Meps.ToadoSettings::Decimals
    IL_0059:  ldarg.0
    IL_005a:  ldc.i4.0
    IL_005b:  stfld      int32 Tvxd.Meps.ToadoSettings::Corner
    IL_0060:  ldarg.0
    IL_0061:  ldc.i4.1
    IL_0062:  stfld      bool Tvxd.Meps.ToadoSettings::Vn2000
    IL_0067:  ldarg.0
    IL_0068:  ldc.i4.1
    IL_0069:  stfld      bool Tvxd.Meps.ToadoSettings::TwoLines
    IL_006e:  ldarg.0
    IL_006f:  ldc.i4.0
    IL_0070:  stfld      bool Tvxd.Meps.ToadoSettings::Mask
    IL_0075:  ldarg.0
    IL_0076:  ldc.i4.0
    IL_0077:  stfld      bool Tvxd.Meps.ToadoSettings::Frame
    IL_007c:  ldarg.0
    IL_007d:  ldc.i4.1
    IL_007e:  stfld      bool Tvxd.Meps.ToadoSettings::Arrow
    IL_0083:  ldarg.0
    IL_0084:  ldc.i4.0
    IL_0085:  stfld      bool Tvxd.Meps.ToadoSettings::Center
    IL_008a:  ldarg.0
    IL_008b:  ldc.i4.0
    IL_008c:  stfld      bool Tvxd.Meps.ToadoSettings::AddName
    IL_0091:  ldarg.0
    IL_0092:  ldc.i4.1
    IL_0093:  stfld      bool Tvxd.Meps.ToadoSettings::PickEach
    IL_0098:  ldarg.0
    IL_0099:  ldc.i4.0
    IL_009a:  stfld      bool Tvxd.Meps.ToadoSettings::Reformat
    IL_009f:  ldarg.0
    IL_00a0:  ldc.i4.0
    IL_00a1:  stfld      bool Tvxd.Meps.ToadoSettings::UnitIsMm
    IL_00a6:  ldarg.0
    IL_00a7:  ldc.i4.1
    IL_00a8:  stfld      bool Tvxd.Meps.ToadoSettings::CircleArrow
    IL_00ad:  ldarg.0
    IL_00ae:  ldc.i4.1
    IL_00af:  stfld      bool Tvxd.Meps.ToadoSettings::UnderlineBetween
    IL_00b4:  ldarg.0
    IL_00b5:  call       instance void [mscorlib]System.Object::.ctor()
    IL_00ba:  nop
    IL_00bb:  ret
  } // end of method ToadoSettings::.ctor

  .property string IniPath()
  {
    .get string Tvxd.Meps.ToadoSettings::get_IniPath()
  } // end of property ToadoSettings::IniPath
} // end of class Tvxd.Meps.ToadoSettings

.class public auto ansi sealed beforefieldinit Tvxd.Meps.ToadoResult
       extends [mscorlib]System.Object
{
  .field public int32 created
  .field public int32 updated
  .field public int32 same
  .field public int32 moved
  .field public int32 skipped
  .field public class [mscorlib]System.Collections.Generic.List`1<string> warnings
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       54 (0x36)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.0
    IL_0002:  stfld      int32 Tvxd.Meps.ToadoResult::created
    IL_0007:  ldarg.0
    IL_0008:  ldc.i4.0
    IL_0009:  stfld      int32 Tvxd.Meps.ToadoResult::updated
    IL_000e:  ldarg.0
    IL_000f:  ldc.i4.0
    IL_0010:  stfld      int32 Tvxd.Meps.ToadoResult::same
    IL_0015:  ldarg.0
    IL_0016:  ldc.i4.0
    IL_0017:  stfld      int32 Tvxd.Meps.ToadoResult::moved
    IL_001c:  ldarg.0
    IL_001d:  ldc.i4.0
    IL_001e:  stfld      int32 Tvxd.Meps.ToadoResult::skipped
    IL_0023:  ldarg.0
    IL_0024:  newobj     instance void class [mscorlib]System.Collections.Generic.List`1<string>::.ctor()
    IL_0029:  stfld      class [mscorlib]System.Collections.Generic.List`1<string> Tvxd.Meps.ToadoResult::warnings
    IL_002e:  ldarg.0
    IL_002f:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0034:  nop
    IL_0035:  ret
  } // end of method ToadoResult::.ctor

} // end of class Tvxd.Meps.ToadoResult

.class public auto ansi sealed beforefieldinit Tvxd.Meps.Toado100Form
       extends [System.Windows.Forms]System.Windows.Forms.Form
{
  .field public class Tvxd.Meps.ToadoSettings S
  .field public bool TriggerRefreshOnly
  .field private class [System.Windows.Forms]System.Windows.Forms.TextBox txtStyle
  .field private class [System.Windows.Forms]System.Windows.Forms.TextBox txtHeight
  .field private class [System.Windows.Forms]System.Windows.Forms.TextBox txtArrowSize
  .field private class [System.Windows.Forms]System.Windows.Forms.TextBox txtDecimals
  .field private class [System.Windows.Forms]System.Windows.Forms.RadioButton rbVn2000
  .field private class [System.Windows.Forms]System.Windows.Forms.RadioButton rbCad
  .field private class [System.Windows.Forms]System.Windows.Forms.RadioButton rbUnitM
  .field private class [System.Windows.Forms]System.Windows.Forms.RadioButton rbUnitMm
  .field private class [System.Windows.Forms]System.Windows.Forms.RadioButton rbPickFree
  .field private class [System.Windows.Forms]System.Windows.Forms.RadioButton rbBatch
  .field private class [System.Windows.Forms]System.Windows.Forms.CheckBox chkMask
  .field private class [System.Windows.Forms]System.Windows.Forms.CheckBox chkCircle
  .field private class [System.Windows.Forms]System.Windows.Forms.CheckBox chkUnderline
  .field private class [System.Windows.Forms]System.Windows.Forms.CheckBox chkAddName
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Tvxd.Meps.ToadoSettings s) cil managed
  {
    // Code size       40 (0x28)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldc.i4.0
    IL_0002:  stfld      bool Tvxd.Meps.Toado100Form::TriggerRefreshOnly
    IL_0007:  ldarg.0
    IL_0008:  call       instance void [System.Windows.Forms]System.Windows.Forms.Form::.ctor()
    IL_000d:  nop
    IL_000e:  nop
    IL_000f:  ldarg.0
    IL_0010:  ldarg.1
    IL_0011:  dup
    IL_0012:  brtrue.s   IL_001a

    IL_0014:  pop
    IL_0015:  newobj     instance void Tvxd.Meps.ToadoSettings::.ctor()
    IL_001a:  stfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_001f:  ldarg.0
    IL_0020:  call       instance void Tvxd.Meps.Toado100Form::InitializeComponent()
    IL_0025:  nop
    IL_0026:  nop
    IL_0027:  ret
  } // end of method Toado100Form::.ctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class Tvxd.Meps.ToadoSettings s,
                               class [mscorlib]System.Collections.Generic.IList`1<string> styles,
                               int32 count) cil managed
  {
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  call       instance void Tvxd.Meps.Toado100Form::.ctor(class Tvxd.Meps.ToadoSettings)
    IL_0007:  nop
    IL_0008:  nop
    IL_0009:  nop
    IL_000a:  ret
  } // end of method Toado100Form::.ctor

  .method private hidebysig instance void 
          InitializeComponent() cil managed
  {
    // Code size       2362 (0x93a)
    .maxstack  5
    .locals init (int32 V_0,
             class [System.Windows.Forms]System.Windows.Forms.GroupBox V_1,
             class [System.Windows.Forms]System.Windows.Forms.Label V_2,
             class [System.Windows.Forms]System.Windows.Forms.Label V_3,
             class [System.Windows.Forms]System.Windows.Forms.Label V_4,
             class [System.Windows.Forms]System.Windows.Forms.Label V_5,
             class [System.Windows.Forms]System.Windows.Forms.GroupBox V_6,
             class [System.Windows.Forms]System.Windows.Forms.Label V_7,
             class [System.Windows.Forms]System.Windows.Forms.GroupBox V_8,
             class [System.Windows.Forms]System.Windows.Forms.Button V_9,
             class [System.Windows.Forms]System.Windows.Forms.Button V_10,
             class [System.Windows.Forms]System.Windows.Forms.Button V_11,
             class [System.Windows.Forms]System.Windows.Forms.GroupBox V_12,
             class [System.Windows.Forms]System.Windows.Forms.Label V_13,
             class [System.Windows.Forms]System.Windows.Forms.TextBox V_14,
             class [System.Windows.Forms]System.Windows.Forms.Label V_15,
             class [System.Windows.Forms]System.Windows.Forms.TextBox V_16,
             class [System.Windows.Forms]System.Windows.Forms.Label V_17,
             class [System.Windows.Forms]System.Windows.Forms.TextBox V_18,
             class [System.Windows.Forms]System.Windows.Forms.Label V_19,
             class [System.Windows.Forms]System.Windows.Forms.TextBox V_20,
             class [System.Windows.Forms]System.Windows.Forms.CheckBox V_21,
             class [System.Windows.Forms]System.Windows.Forms.CheckBox V_22,
             class [System.Windows.Forms]System.Windows.Forms.GroupBox V_23,
             class [System.Windows.Forms]System.Windows.Forms.RadioButton V_24,
             class [System.Windows.Forms]System.Windows.Forms.RadioButton V_25,
             class [System.Windows.Forms]System.Windows.Forms.Label V_26,
             class [System.Windows.Forms]System.Windows.Forms.RadioButton V_27,
             class [System.Windows.Forms]System.Windows.Forms.RadioButton V_28,
             class [System.Windows.Forms]System.Windows.Forms.GroupBox V_29,
             class [System.Windows.Forms]System.Windows.Forms.RadioButton V_30,
             class [System.Windows.Forms]System.Windows.Forms.RadioButton V_31,
             class [System.Windows.Forms]System.Windows.Forms.CheckBox V_32,
             class [System.Windows.Forms]System.Windows.Forms.CheckBox V_33,
             class [System.Windows.Forms]System.Windows.Forms.Button V_34,
             class [System.Windows.Forms]System.Windows.Forms.Button V_35,
             class [System.Windows.Forms]System.Windows.Forms.Button V_36,
             class [System.Windows.Forms]System.Windows.Forms.Control[] V_37)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldstr      bytearray (54 00 2D 00 54 00 4F 00 41 00 44 00 4F 00 20 00   // T.-.T.O.A.D.O. .
                                    14 20 20 00 4E 00 68 00 E3 00 6E 00 20 00 54 00   // .  .N.h...n. .T.
                                    CD 1E 61 00 20 00 10 01 D9 1E 20 00 56 00 4E 00   // ..a. ..... .V.N.
                                    2D 00 32 00 30 00 30 00 30 00 20 00 28 00 42 00   // -.2.0.0.0. .(.B.
                                    A3 1E 6E 00 20 00 4E 00 E2 00 6E 00 67 00 20 00   // ..n. .N...n.g. .
                                    43 00 A5 1E 70 00 29 00 )                         // C...p.).
    IL_0007:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_000c:  nop
    IL_000d:  ldarg.0
    IL_000e:  ldc.i4.3
    IL_000f:  call       instance void [System.Windows.Forms]System.Windows.Forms.Form::set_FormBorderStyle(valuetype [System.Windows.Forms]System.Windows.Forms.FormBorderStyle)
    IL_0014:  nop
    IL_0015:  ldarg.0
    IL_0016:  ldc.i4.0
    IL_0017:  call       instance void [System.Windows.Forms]System.Windows.Forms.Form::set_MaximizeBox(bool)
    IL_001c:  nop
    IL_001d:  ldarg.0
    IL_001e:  ldc.i4.0
    IL_001f:  call       instance void [System.Windows.Forms]System.Windows.Forms.Form::set_MinimizeBox(bool)
    IL_0024:  nop
    IL_0025:  ldarg.0
    IL_0026:  ldc.i4.1
    IL_0027:  call       instance void [System.Windows.Forms]System.Windows.Forms.Form::set_StartPosition(valuetype [System.Windows.Forms]System.Windows.Forms.FormStartPosition)
    IL_002c:  nop
    IL_002d:  ldarg.0
    IL_002e:  ldc.i4     0x212
    IL_0033:  ldc.i4     0x23a
    IL_0038:  newobj     instance void [System.Drawing]System.Drawing.Size::.ctor(int32,
                                                                                  int32)
    IL_003d:  call       instance void [System.Windows.Forms]System.Windows.Forms.Form::set_Size(valuetype [System.Drawing]System.Drawing.Size)
    IL_0042:  nop
    IL_0043:  ldarg.0
    IL_0044:  ldstr      "Segoe UI"
    IL_0049:  ldc.r4     9.
    IL_004e:  ldc.i4.0
    IL_004f:  newobj     instance void [System.Drawing]System.Drawing.Font::.ctor(string,
                                                                                  float32,
                                                                                  valuetype [System.Drawing]System.Drawing.FontStyle)
    IL_0054:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Font(class [System.Drawing]System.Drawing.Font)
    IL_0059:  nop
    IL_005a:  ldc.i4.s   15
    IL_005c:  stloc.0
    IL_005d:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.GroupBox::.ctor()
    IL_0062:  stloc.s    V_12
    IL_0064:  ldloc.s    V_12
    IL_0066:  ldstr      bytearray (31 00 2E 00 20 00 54 00 72 00 EC 00 6E 00 68 00   // 1... .T.r...n.h.
                                    20 00 62 00 E0 00 79 00 20 00 26 00 20 00 10 01   //  .b...y. .&. ...
                                    A7 1E 75 00 20 00 6D 00 FA 00 74 00 20 00 68 00   // ..u. .m...t. .h.
                                    EC 00 6E 00 68 00 20 00 74 00 72 00 F2 00 6E 00 ) // ..n.h. .t.r...n.
    IL_006b:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_0070:  nop
    IL_0071:  ldloc.s    V_12
    IL_0073:  ldc.i4.s   15
    IL_0075:  ldloc.0
    IL_0076:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_007b:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_0080:  nop
    IL_0081:  ldloc.s    V_12
    IL_0083:  ldc.i4     0x1e5
    IL_0088:  ldc.i4     0x91
    IL_008d:  newobj     instance void [System.Drawing]System.Drawing.Size::.ctor(int32,
                                                                                  int32)
    IL_0092:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Size(valuetype [System.Drawing]System.Drawing.Size)
    IL_0097:  nop
    IL_0098:  ldloc.s    V_12
    IL_009a:  stloc.1
    IL_009b:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.Label::.ctor()
    IL_00a0:  stloc.s    V_13
    IL_00a2:  ldloc.s    V_13
    IL_00a4:  ldstr      bytearray (43 00 68 00 69 00 C1 1E 75 00 20 00 63 00 61 00   // C.h.i...u. .c.a.
                                    6F 00 20 00 63 00 68 00 EF 1E 3A 00 )             // o. .c.h...:.
    IL_00a9:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_00ae:  nop
    IL_00af:  ldloc.s    V_13
    IL_00b1:  ldc.i4.s   15
    IL_00b3:  ldc.i4.s   25
    IL_00b5:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_00ba:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_00bf:  nop
    IL_00c0:  ldloc.s    V_13
    IL_00c2:  ldc.i4.1
    IL_00c3:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_00c8:  nop
    IL_00c9:  ldloc.s    V_13
    IL_00cb:  stloc.2
    IL_00cc:  ldarg.0
    IL_00cd:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.TextBox::.ctor()
    IL_00d2:  stloc.s    V_14
    IL_00d4:  ldloc.s    V_14
    IL_00d6:  ldarg.0
    IL_00d7:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_00dc:  ldflda     float64 Tvxd.Meps.ToadoSettings::Height
    IL_00e1:  ldstr      "0.##"
    IL_00e6:  call       class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()
    IL_00eb:  call       instance string [mscorlib]System.Double::ToString(string,
                                                                           class [mscorlib]System.IFormatProvider)
    IL_00f0:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_00f5:  nop
    IL_00f6:  ldloc.s    V_14
    IL_00f8:  ldc.i4.s   120
    IL_00fa:  ldc.i4.s   22
    IL_00fc:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_0101:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_0106:  nop
    IL_0107:  ldloc.s    V_14
    IL_0109:  ldc.i4.s   70
    IL_010b:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Width(int32)
    IL_0110:  nop
    IL_0111:  ldloc.s    V_14
    IL_0113:  stfld      class [System.Windows.Forms]System.Windows.Forms.TextBox Tvxd.Meps.Toado100Form::txtHeight
    IL_0118:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.Label::.ctor()
    IL_011d:  stloc.s    V_15
    IL_011f:  ldloc.s    V_15
    IL_0121:  ldstr      bytearray (4B 00 ED 00 63 00 68 00 20 00 74 00 68 00 B0 01   // K...c.h. .t.h...
                                    DB 1E 63 00 20 00 11 01 A7 1E 75 00 20 00 74 00   // ..c. .....u. .t.
                                    72 00 F2 00 6E 00 3A 00 )                         // r...n.:.
    IL_0126:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_012b:  nop
    IL_012c:  ldloc.s    V_15
    IL_012e:  ldc.i4     0xdc
    IL_0133:  ldc.i4.s   25
    IL_0135:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_013a:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_013f:  nop
    IL_0140:  ldloc.s    V_15
    IL_0142:  ldc.i4.1
    IL_0143:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_0148:  nop
    IL_0149:  ldloc.s    V_15
    IL_014b:  stloc.3
    IL_014c:  ldarg.0
    IL_014d:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.TextBox::.ctor()
    IL_0152:  stloc.s    V_16
    IL_0154:  ldloc.s    V_16
    IL_0156:  ldarg.0
    IL_0157:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_015c:  ldflda     float64 Tvxd.Meps.ToadoSettings::ArrowSize
    IL_0161:  ldstr      "0.##"
    IL_0166:  call       class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()
    IL_016b:  call       instance string [mscorlib]System.Double::ToString(string,
                                                                           class [mscorlib]System.IFormatProvider)
    IL_0170:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_0175:  nop
    IL_0176:  ldloc.s    V_16
    IL_0178:  ldc.i4     0x177
    IL_017d:  ldc.i4.s   22
    IL_017f:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_0184:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_0189:  nop
    IL_018a:  ldloc.s    V_16
    IL_018c:  ldc.i4.s   70
    IL_018e:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Width(int32)
    IL_0193:  nop
    IL_0194:  ldloc.s    V_16
    IL_0196:  stfld      class [System.Windows.Forms]System.Windows.Forms.TextBox Tvxd.Meps.Toado100Form::txtArrowSize
    IL_019b:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.Label::.ctor()
    IL_01a0:  stloc.s    V_17
    IL_01a2:  ldloc.s    V_17
    IL_01a4:  ldstr      bytearray (43 00 68 00 EF 1E 20 00 73 00 D1 1E 20 00 73 00   // C.h... .s... .s.
                                    61 00 75 00 20 00 70 00 68 00 A9 1E 79 00 3A 00 ) // a.u. .p.h...y.:.
    IL_01a9:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_01ae:  nop
    IL_01af:  ldloc.s    V_17
    IL_01b1:  ldc.i4.s   15
    IL_01b3:  ldc.i4.s   58
    IL_01b5:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_01ba:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_01bf:  nop
    IL_01c0:  ldloc.s    V_17
    IL_01c2:  ldc.i4.1
    IL_01c3:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_01c8:  nop
    IL_01c9:  ldloc.s    V_17
    IL_01cb:  stloc.s    V_4
    IL_01cd:  ldarg.0
    IL_01ce:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.TextBox::.ctor()
    IL_01d3:  stloc.s    V_18
    IL_01d5:  ldloc.s    V_18
    IL_01d7:  ldarg.0
    IL_01d8:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_01dd:  ldflda     int32 Tvxd.Meps.ToadoSettings::Decimals
    IL_01e2:  call       instance string [mscorlib]System.Int32::ToString()
    IL_01e7:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_01ec:  nop
    IL_01ed:  ldloc.s    V_18
    IL_01ef:  ldc.i4.s   120
    IL_01f1:  ldc.i4.s   55
    IL_01f3:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_01f8:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_01fd:  nop
    IL_01fe:  ldloc.s    V_18
    IL_0200:  ldc.i4.s   70
    IL_0202:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Width(int32)
    IL_0207:  nop
    IL_0208:  ldloc.s    V_18
    IL_020a:  stfld      class [System.Windows.Forms]System.Windows.Forms.TextBox Tvxd.Meps.Toado100Form::txtDecimals
    IL_020f:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.Label::.ctor()
    IL_0214:  stloc.s    V_19
    IL_0216:  ldloc.s    V_19
    IL_0218:  ldstr      bytearray (4B 00 69 00 C3 1E 75 00 20 00 63 00 68 00 EF 1E   // K.i...u. .c.h...
                                    20 00 28 00 53 00 74 00 79 00 6C 00 65 00 29 00   //  .(.S.t.y.l.e.).
                                    3A 00 )                                           // :.
    IL_021d:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_0222:  nop
    IL_0223:  ldloc.s    V_19
    IL_0225:  ldc.i4     0xdc
    IL_022a:  ldc.i4.s   58
    IL_022c:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_0231:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_0236:  nop
    IL_0237:  ldloc.s    V_19
    IL_0239:  ldc.i4.1
    IL_023a:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_023f:  nop
    IL_0240:  ldloc.s    V_19
    IL_0242:  stloc.s    V_5
    IL_0244:  ldarg.0
    IL_0245:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.TextBox::.ctor()
    IL_024a:  stloc.s    V_20
    IL_024c:  ldloc.s    V_20
    IL_024e:  ldarg.0
    IL_024f:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_0254:  ldfld      string Tvxd.Meps.ToadoSettings::Style
    IL_0259:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_025e:  nop
    IL_025f:  ldloc.s    V_20
    IL_0261:  ldc.i4     0x177
    IL_0266:  ldc.i4.s   55
    IL_0268:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_026d:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_0272:  nop
    IL_0273:  ldloc.s    V_20
    IL_0275:  ldc.i4.s   95
    IL_0277:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Width(int32)
    IL_027c:  nop
    IL_027d:  ldloc.s    V_20
    IL_027f:  stfld      class [System.Windows.Forms]System.Windows.Forms.TextBox Tvxd.Meps.Toado100Form::txtStyle
    IL_0284:  ldarg.0
    IL_0285:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.CheckBox::.ctor()
    IL_028a:  stloc.s    V_21
    IL_028c:  ldloc.s    V_21
    IL_028e:  ldstr      bytearray (CF 25 20 00 10 01 A7 1E 75 00 20 00 6D 00 69 01   // .% .....u. .m.i.
                                    69 00 20 00 74 00 EA 00 6E 00 20 00 6C 00 E0 00   // i. .t...n. .l...
                                    20 00 48 00 CC 00 4E 00 48 00 20 00 54 00 52 00   //  .H...N.H. .T.R.
                                    D2 00 4E 00 20 00 28 00 44 00 6F 00 74 00 29 00 ) // ..N. .(.D.o.t.).
    IL_0293:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_0298:  nop
    IL_0299:  ldloc.s    V_21
    IL_029b:  ldarg.0
    IL_029c:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_02a1:  ldfld      bool Tvxd.Meps.ToadoSettings::CircleArrow
    IL_02a6:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.CheckBox::set_Checked(bool)
    IL_02ab:  nop
    IL_02ac:  ldloc.s    V_21
    IL_02ae:  ldc.i4.s   15
    IL_02b0:  ldc.i4.s   90
    IL_02b2:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_02b7:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_02bc:  nop
    IL_02bd:  ldloc.s    V_21
    IL_02bf:  ldc.i4.1
    IL_02c0:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_02c5:  nop
    IL_02c6:  ldloc.s    V_21
    IL_02c8:  call       valuetype [System.Drawing]System.Drawing.Color [System.Drawing]System.Drawing.Color::get_DarkGreen()
    IL_02cd:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_ForeColor(valuetype [System.Drawing]System.Drawing.Color)
    IL_02d2:  nop
    IL_02d3:  ldloc.s    V_21
    IL_02d5:  ldstr      "Segoe UI"
    IL_02da:  ldc.r4     9.
    IL_02df:  ldc.i4.1
    IL_02e0:  newobj     instance void [System.Drawing]System.Drawing.Font::.ctor(string,
                                                                                  float32,
                                                                                  valuetype [System.Drawing]System.Drawing.FontStyle)
    IL_02e5:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Font(class [System.Drawing]System.Drawing.Font)
    IL_02ea:  nop
    IL_02eb:  ldloc.s    V_21
    IL_02ed:  stfld      class [System.Windows.Forms]System.Windows.Forms.CheckBox Tvxd.Meps.Toado100Form::chkCircle
    IL_02f2:  ldarg.0
    IL_02f3:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.CheckBox::.ctor()
    IL_02f8:  stloc.s    V_22
    IL_02fa:  ldloc.s    V_22
    IL_02fc:  ldstr      bytearray (01 25 20 00 10 01 B0 01 DD 1E 6E 00 67 00 20 00   // .% .......n.g. .
                                    67 00 69 00 F3 00 6E 00 67 00 20 00 58 00 45 00   // g.i...n.g. .X.E.
                                    4E 00 20 00 47 00 49 00 EE 1E 41 00 20 00 32 00   // N. .G.I...A. .2.
                                    20 00 64 00 F2 00 6E 00 67 00 20 00 58 00 2F 00   //  .d...n.g. .X./.
                                    59 00 )                                           // Y.
    IL_0301:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_0306:  nop
    IL_0307:  ldloc.s    V_22
    IL_0309:  ldarg.0
    IL_030a:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_030f:  ldfld      bool Tvxd.Meps.ToadoSettings::UnderlineBetween
    IL_0314:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.CheckBox::set_Checked(bool)
    IL_0319:  nop
    IL_031a:  ldloc.s    V_22
    IL_031c:  ldc.i4.s   15
    IL_031e:  ldc.i4.s   115
    IL_0320:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_0325:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_032a:  nop
    IL_032b:  ldloc.s    V_22
    IL_032d:  ldc.i4.1
    IL_032e:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_0333:  nop
    IL_0334:  ldloc.s    V_22
    IL_0336:  call       valuetype [System.Drawing]System.Drawing.Color [System.Drawing]System.Drawing.Color::get_DarkBlue()
    IL_033b:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_ForeColor(valuetype [System.Drawing]System.Drawing.Color)
    IL_0340:  nop
    IL_0341:  ldloc.s    V_22
    IL_0343:  ldstr      "Segoe UI"
    IL_0348:  ldc.r4     9.
    IL_034d:  ldc.i4.1
    IL_034e:  newobj     instance void [System.Drawing]System.Drawing.Font::.ctor(string,
                                                                                  float32,
                                                                                  valuetype [System.Drawing]System.Drawing.FontStyle)
    IL_0353:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Font(class [System.Drawing]System.Drawing.Font)
    IL_0358:  nop
    IL_0359:  ldloc.s    V_22
    IL_035b:  stfld      class [System.Windows.Forms]System.Windows.Forms.CheckBox Tvxd.Meps.Toado100Form::chkUnderline
    IL_0360:  ldloc.1
    IL_0361:  callvirt   instance class [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection [System.Windows.Forms]System.Windows.Forms.Control::get_Controls()
    IL_0366:  ldc.i4.s   10
    IL_0368:  newarr     [System.Windows.Forms]System.Windows.Forms.Control
    IL_036d:  stloc.s    V_37
    IL_036f:  ldloc.s    V_37
    IL_0371:  ldc.i4.0
    IL_0372:  ldloc.2
    IL_0373:  stelem.ref
    IL_0374:  ldloc.s    V_37
    IL_0376:  ldc.i4.1
    IL_0377:  ldarg.0
    IL_0378:  ldfld      class [System.Windows.Forms]System.Windows.Forms.TextBox Tvxd.Meps.Toado100Form::txtHeight
    IL_037d:  stelem.ref
    IL_037e:  ldloc.s    V_37
    IL_0380:  ldc.i4.2
    IL_0381:  ldloc.3
    IL_0382:  stelem.ref
    IL_0383:  ldloc.s    V_37
    IL_0385:  ldc.i4.3
    IL_0386:  ldarg.0
    IL_0387:  ldfld      class [System.Windows.Forms]System.Windows.Forms.TextBox Tvxd.Meps.Toado100Form::txtArrowSize
    IL_038c:  stelem.ref
    IL_038d:  ldloc.s    V_37
    IL_038f:  ldc.i4.4
    IL_0390:  ldloc.s    V_4
    IL_0392:  stelem.ref
    IL_0393:  ldloc.s    V_37
    IL_0395:  ldc.i4.5
    IL_0396:  ldarg.0
    IL_0397:  ldfld      class [System.Windows.Forms]System.Windows.Forms.TextBox Tvxd.Meps.Toado100Form::txtDecimals
    IL_039c:  stelem.ref
    IL_039d:  ldloc.s    V_37
    IL_039f:  ldc.i4.6
    IL_03a0:  ldloc.s    V_5
    IL_03a2:  stelem.ref
    IL_03a3:  ldloc.s    V_37
    IL_03a5:  ldc.i4.7
    IL_03a6:  ldarg.0
    IL_03a7:  ldfld      class [System.Windows.Forms]System.Windows.Forms.TextBox Tvxd.Meps.Toado100Form::txtStyle
    IL_03ac:  stelem.ref
    IL_03ad:  ldloc.s    V_37
    IL_03af:  ldc.i4.8
    IL_03b0:  ldarg.0
    IL_03b1:  ldfld      class [System.Windows.Forms]System.Windows.Forms.CheckBox Tvxd.Meps.Toado100Form::chkCircle
    IL_03b6:  stelem.ref
    IL_03b7:  ldloc.s    V_37
    IL_03b9:  ldc.i4.s   9
    IL_03bb:  ldarg.0
    IL_03bc:  ldfld      class [System.Windows.Forms]System.Windows.Forms.CheckBox Tvxd.Meps.Toado100Form::chkUnderline
    IL_03c1:  stelem.ref
    IL_03c2:  ldloc.s    V_37
    IL_03c4:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection::AddRange(class [System.Windows.Forms]System.Windows.Forms.Control[])
    IL_03c9:  nop
    IL_03ca:  ldarg.0
    IL_03cb:  call       instance class [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection [System.Windows.Forms]System.Windows.Forms.Control::get_Controls()
    IL_03d0:  ldloc.1
    IL_03d1:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection::Add(class [System.Windows.Forms]System.Windows.Forms.Control)
    IL_03d6:  nop
    IL_03d7:  ldloc.0
    IL_03d8:  ldc.i4     0x9b
    IL_03dd:  add
    IL_03de:  stloc.0
    IL_03df:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.GroupBox::.ctor()
    IL_03e4:  stloc.s    V_23
    IL_03e6:  ldloc.s    V_23
    IL_03e8:  ldstr      bytearray (32 00 2E 00 20 00 48 00 C7 1E 20 00 74 00 CD 1E   // 2... .H... .t...
                                    61 00 20 00 11 01 D9 1E 20 00 56 00 4E 00 2D 00   // a. ..... .V.N.-.
                                    32 00 30 00 30 00 30 00 20 00 26 00 20 00 10 01   // 2.0.0.0. .&. ...
                                    A1 01 6E 00 20 00 76 00 CB 1E )                   // ..n. .v...
    IL_03ed:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_03f2:  nop
    IL_03f3:  ldloc.s    V_23
    IL_03f5:  ldc.i4.s   15
    IL_03f7:  ldloc.0
    IL_03f8:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_03fd:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_0402:  nop
    IL_0403:  ldloc.s    V_23
    IL_0405:  ldc.i4     0x1e5
    IL_040a:  ldc.i4.s   110
    IL_040c:  newobj     instance void [System.Drawing]System.Drawing.Size::.ctor(int32,
                                                                                  int32)
    IL_0411:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Size(valuetype [System.Drawing]System.Drawing.Size)
    IL_0416:  nop
    IL_0417:  ldloc.s    V_23
    IL_0419:  stloc.s    V_6
    IL_041b:  ldarg.0
    IL_041c:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.RadioButton::.ctor()
    IL_0421:  stloc.s    V_24
    IL_0423:  ldloc.s    V_24
    IL_0425:  ldstr      bytearray (56 00 4E 00 2D 00 32 00 30 00 30 00 30 00 20 00   // V.N.-.2.0.0.0. .
                                    28 00 44 00 F2 00 6E 00 67 00 20 00 74 00 72 00   // (.D...n.g. .t.r.
                                    EA 00 6E 00 20 00 58 00 20 00 3D 00 20 00 54 00   // ..n. .X. .=. .T.
                                    72 00 E5 1E 63 00 20 00 59 00 20 00 43 00 41 00   // r...c. .Y. .C.A.
                                    44 00 2C 00 20 00 44 00 F2 00 6E 00 67 00 20 00   // D.,. .D...n.g. .
                                    64 00 B0 01 DB 1E 69 00 20 00 59 00 20 00 3D 00   // d.....i. .Y. .=.
                                    20 00 54 00 72 00 E5 1E 63 00 20 00 58 00 20 00   //  .T.r...c. .X. .
                                    43 00 41 00 44 00 29 00 )                         // C.A.D.).
    IL_042a:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_042f:  nop
    IL_0430:  ldloc.s    V_24
    IL_0432:  ldarg.0
    IL_0433:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_0438:  ldfld      bool Tvxd.Meps.ToadoSettings::Vn2000
    IL_043d:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.RadioButton::set_Checked(bool)
    IL_0442:  nop
    IL_0443:  ldloc.s    V_24
    IL_0445:  ldc.i4.s   15
    IL_0447:  ldc.i4.s   23
    IL_0449:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_044e:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_0453:  nop
    IL_0454:  ldloc.s    V_24
    IL_0456:  ldc.i4.1
    IL_0457:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_045c:  nop
    IL_045d:  ldloc.s    V_24
    IL_045f:  ldstr      "Segoe UI"
    IL_0464:  ldc.r4     9.
    IL_0469:  ldc.i4.1
    IL_046a:  newobj     instance void [System.Drawing]System.Drawing.Font::.ctor(string,
                                                                                  float32,
                                                                                  valuetype [System.Drawing]System.Drawing.FontStyle)
    IL_046f:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Font(class [System.Drawing]System.Drawing.Font)
    IL_0474:  nop
    IL_0475:  ldloc.s    V_24
    IL_0477:  call       valuetype [System.Drawing]System.Drawing.Color [System.Drawing]System.Drawing.Color::get_Firebrick()
    IL_047c:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_ForeColor(valuetype [System.Drawing]System.Drawing.Color)
    IL_0481:  nop
    IL_0482:  ldloc.s    V_24
    IL_0484:  stfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbVn2000
    IL_0489:  ldarg.0
    IL_048a:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.RadioButton::.ctor()
    IL_048f:  stloc.s    V_25
    IL_0491:  ldloc.s    V_25
    IL_0493:  ldstr      bytearray (48 00 C7 1E 20 00 43 00 41 00 44 00 20 00 74 00   // H... .C.A.D. .t.
                                    68 00 75 00 A7 1E 6E 00 20 00 28 00 58 00 20 00   // h.u...n. .(.X. .
                                    3D 00 20 00 54 00 72 00 E5 1E 63 00 20 00 6E 00   // =. .T.r...c. .n.
                                    67 00 61 00 6E 00 67 00 20 00 58 00 2C 00 20 00   // g.a.n.g. .X.,. .
                                    59 00 20 00 3D 00 20 00 54 00 72 00 E5 1E 63 00   // Y. .=. .T.r...c.
                                    20 00 11 01 E9 1E 6E 00 67 00 20 00 59 00 29 00 ) //  .....n.g. .Y.).
    IL_0498:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_049d:  nop
    IL_049e:  ldloc.s    V_25
    IL_04a0:  ldarg.0
    IL_04a1:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_04a6:  ldfld      bool Tvxd.Meps.ToadoSettings::Vn2000
    IL_04ab:  ldc.i4.0
    IL_04ac:  ceq
    IL_04ae:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.RadioButton::set_Checked(bool)
    IL_04b3:  nop
    IL_04b4:  ldloc.s    V_25
    IL_04b6:  ldc.i4.s   15
    IL_04b8:  ldc.i4.s   48
    IL_04ba:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_04bf:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_04c4:  nop
    IL_04c5:  ldloc.s    V_25
    IL_04c7:  ldc.i4.1
    IL_04c8:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_04cd:  nop
    IL_04ce:  ldloc.s    V_25
    IL_04d0:  stfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbCad
    IL_04d5:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.Label::.ctor()
    IL_04da:  stloc.s    V_26
    IL_04dc:  ldloc.s    V_26
    IL_04de:  ldstr      bytearray (10 01 A1 01 6E 00 20 00 76 00 CB 1E 20 00 43 00   // ....n. .v... .C.
                                    41 00 44 00 3A 00 )                               // A.D.:.
    IL_04e3:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_04e8:  nop
    IL_04e9:  ldloc.s    V_26
    IL_04eb:  ldc.i4.s   15
    IL_04ed:  ldc.i4.s   78
    IL_04ef:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_04f4:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_04f9:  nop
    IL_04fa:  ldloc.s    V_26
    IL_04fc:  ldc.i4.1
    IL_04fd:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_0502:  nop
    IL_0503:  ldloc.s    V_26
    IL_0505:  stloc.s    V_7
    IL_0507:  ldarg.0
    IL_0508:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.RadioButton::.ctor()
    IL_050d:  stloc.s    V_27
    IL_050f:  ldloc.s    V_27
    IL_0511:  ldstr      bytearray (4D 00 E9 00 74 00 20 00 28 00 6D 00 29 00 )       // M...t. .(.m.).
    IL_0516:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_051b:  nop
    IL_051c:  ldloc.s    V_27
    IL_051e:  ldarg.0
    IL_051f:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_0524:  ldfld      bool Tvxd.Meps.ToadoSettings::UnitIsMm
    IL_0529:  ldc.i4.0
    IL_052a:  ceq
    IL_052c:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.RadioButton::set_Checked(bool)
    IL_0531:  nop
    IL_0532:  ldloc.s    V_27
    IL_0534:  ldc.i4.s   120
    IL_0536:  ldc.i4.s   76
    IL_0538:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_053d:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_0542:  nop
    IL_0543:  ldloc.s    V_27
    IL_0545:  ldc.i4.1
    IL_0546:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_054b:  nop
    IL_054c:  ldloc.s    V_27
    IL_054e:  stfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbUnitM
    IL_0553:  ldarg.0
    IL_0554:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.RadioButton::.ctor()
    IL_0559:  stloc.s    V_28
    IL_055b:  ldloc.s    V_28
    IL_055d:  ldstr      bytearray (4D 00 69 00 6C 00 69 00 6D 00 E9 00 74 00 20 00   // M.i.l.i.m...t. .
                                    28 00 6D 00 6D 00 20 00 2D 00 20 00 74 00 F1 1E   // (.m.m. .-. .t...
                                    20 00 63 00 68 00 69 00 61 00 20 00 31 00 30 00   //  .c.h.i.a. .1.0.
                                    30 00 30 00 20 00 72 00 61 00 20 00 6D 00 E9 00   // 0.0. .r.a. .m...
                                    74 00 29 00 )                                     // t.).
    IL_0562:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_0567:  nop
    IL_0568:  ldloc.s    V_28
    IL_056a:  ldarg.0
    IL_056b:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_0570:  ldfld      bool Tvxd.Meps.ToadoSettings::UnitIsMm
    IL_0575:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.RadioButton::set_Checked(bool)
    IL_057a:  nop
    IL_057b:  ldloc.s    V_28
    IL_057d:  ldc.i4     0xdc
    IL_0582:  ldc.i4.s   76
    IL_0584:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_0589:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_058e:  nop
    IL_058f:  ldloc.s    V_28
    IL_0591:  ldc.i4.1
    IL_0592:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_0597:  nop
    IL_0598:  ldloc.s    V_28
    IL_059a:  stfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbUnitMm
    IL_059f:  ldloc.s    V_6
    IL_05a1:  callvirt   instance class [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection [System.Windows.Forms]System.Windows.Forms.Control::get_Controls()
    IL_05a6:  ldc.i4.5
    IL_05a7:  newarr     [System.Windows.Forms]System.Windows.Forms.Control
    IL_05ac:  stloc.s    V_37
    IL_05ae:  ldloc.s    V_37
    IL_05b0:  ldc.i4.0
    IL_05b1:  ldarg.0
    IL_05b2:  ldfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbVn2000
    IL_05b7:  stelem.ref
    IL_05b8:  ldloc.s    V_37
    IL_05ba:  ldc.i4.1
    IL_05bb:  ldarg.0
    IL_05bc:  ldfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbCad
    IL_05c1:  stelem.ref
    IL_05c2:  ldloc.s    V_37
    IL_05c4:  ldc.i4.2
    IL_05c5:  ldloc.s    V_7
    IL_05c7:  stelem.ref
    IL_05c8:  ldloc.s    V_37
    IL_05ca:  ldc.i4.3
    IL_05cb:  ldarg.0
    IL_05cc:  ldfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbUnitM
    IL_05d1:  stelem.ref
    IL_05d2:  ldloc.s    V_37
    IL_05d4:  ldc.i4.4
    IL_05d5:  ldarg.0
    IL_05d6:  ldfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbUnitMm
    IL_05db:  stelem.ref
    IL_05dc:  ldloc.s    V_37
    IL_05de:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection::AddRange(class [System.Windows.Forms]System.Windows.Forms.Control[])
    IL_05e3:  nop
    IL_05e4:  ldarg.0
    IL_05e5:  call       instance class [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection [System.Windows.Forms]System.Windows.Forms.Control::get_Controls()
    IL_05ea:  ldloc.s    V_6
    IL_05ec:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection::Add(class [System.Windows.Forms]System.Windows.Forms.Control)
    IL_05f1:  nop
    IL_05f2:  ldloc.0
    IL_05f3:  ldc.i4.s   120
    IL_05f5:  add
    IL_05f6:  stloc.0
    IL_05f7:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.GroupBox::.ctor()
    IL_05fc:  stloc.s    V_29
    IL_05fe:  ldloc.s    V_29
    IL_0600:  ldstr      bytearray (33 00 2E 00 20 00 48 00 B0 01 DB 1E 6E 00 67 00   // 3... .H.....n.g.
                                    20 00 11 01 B7 1E 74 00 20 00 6E 00 68 00 E3 00   //  .....t. .n.h...
                                    6E 00 20 00 26 00 20 00 54 00 F9 00 79 00 20 00   // n. .&. .T...y. .
                                    63 00 68 00 CD 1E 6E 00 )                         // c.h...n.
    IL_0605:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_060a:  nop
    IL_060b:  ldloc.s    V_29
    IL_060d:  ldc.i4.s   15
    IL_060f:  ldloc.0
    IL_0610:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_0615:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_061a:  nop
    IL_061b:  ldloc.s    V_29
    IL_061d:  ldc.i4     0x1e5
    IL_0622:  ldc.i4.s   105
    IL_0624:  newobj     instance void [System.Drawing]System.Drawing.Size::.ctor(int32,
                                                                                  int32)
    IL_0629:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Size(valuetype [System.Drawing]System.Drawing.Size)
    IL_062e:  nop
    IL_062f:  ldloc.s    V_29
    IL_0631:  stloc.s    V_8
    IL_0633:  ldarg.0
    IL_0634:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.RadioButton::.ctor()
    IL_0639:  stloc.s    V_30
    IL_063b:  ldloc.s    V_30
    IL_063d:  ldstr      bytearray (43 00 6C 00 69 00 63 00 6B 00 20 00 63 00 68 00   // C.l.i.c.k. .c.h.
                                    75 00 D9 1E 74 00 20 00 63 00 68 00 CD 1E 6E 00   // u...t. .c.h...n.
                                    20 00 68 00 B0 01 DB 1E 6E 00 67 00 20 00 74 00   //  .h.....n.g. .t.
                                    F1 1E 20 00 64 00 6F 00 20 00 33 00 36 00 30 00   // .. .d.o. .3.6.0.
                                    B0 00 20 00 28 00 4B 00 68 00 75 00 79 00 EA 00   // .. .(.K.h.u.y...
                                    6E 00 20 00 64 00 F9 00 6E 00 67 00 29 00 )       // n. .d...n.g.).
    IL_0642:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_0647:  nop
    IL_0648:  ldloc.s    V_30
    IL_064a:  ldarg.0
    IL_064b:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_0650:  ldfld      bool Tvxd.Meps.ToadoSettings::PickEach
    IL_0655:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.RadioButton::set_Checked(bool)
    IL_065a:  nop
    IL_065b:  ldloc.s    V_30
    IL_065d:  ldc.i4.s   15
    IL_065f:  ldc.i4.s   22
    IL_0661:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_0666:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_066b:  nop
    IL_066c:  ldloc.s    V_30
    IL_066e:  ldc.i4.1
    IL_066f:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_0674:  nop
    IL_0675:  ldloc.s    V_30
    IL_0677:  call       valuetype [System.Drawing]System.Drawing.Color [System.Drawing]System.Drawing.Color::get_DarkGreen()
    IL_067c:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_ForeColor(valuetype [System.Drawing]System.Drawing.Color)
    IL_0681:  nop
    IL_0682:  ldloc.s    V_30
    IL_0684:  ldstr      "Segoe UI"
    IL_0689:  ldc.r4     9.
    IL_068e:  ldc.i4.1
    IL_068f:  newobj     instance void [System.Drawing]System.Drawing.Font::.ctor(string,
                                                                                  float32,
                                                                                  valuetype [System.Drawing]System.Drawing.FontStyle)
    IL_0694:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Font(class [System.Drawing]System.Drawing.Font)
    IL_0699:  nop
    IL_069a:  ldloc.s    V_30
    IL_069c:  stfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbPickFree
    IL_06a1:  ldarg.0
    IL_06a2:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.RadioButton::.ctor()
    IL_06a7:  stloc.s    V_31
    IL_06a9:  ldloc.s    V_31
    IL_06ab:  ldstr      bytearray (54 00 F1 1E 20 00 11 01 D9 1E 6E 00 67 00 20 00   // T... .....n.g. .
                                    6C 00 C7 1E 63 00 68 00 20 00 67 00 F3 00 63 00   // l...c.h. .g...c.
                                    20 00 63 00 D1 1E 20 00 11 01 CB 1E 6E 00 68 00   //  .c... .....n.h.
                                    20 00 34 00 35 00 B0 00 )                         //  .4.5...
    IL_06b0:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_06b5:  nop
    IL_06b6:  ldloc.s    V_31
    IL_06b8:  ldarg.0
    IL_06b9:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_06be:  ldfld      bool Tvxd.Meps.ToadoSettings::PickEach
    IL_06c3:  ldc.i4.0
    IL_06c4:  ceq
    IL_06c6:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.RadioButton::set_Checked(bool)
    IL_06cb:  nop
    IL_06cc:  ldloc.s    V_31
    IL_06ce:  ldc.i4.s   15
    IL_06d0:  ldc.i4.s   47
    IL_06d2:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_06d7:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_06dc:  nop
    IL_06dd:  ldloc.s    V_31
    IL_06df:  ldc.i4.1
    IL_06e0:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_06e5:  nop
    IL_06e6:  ldloc.s    V_31
    IL_06e8:  stfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbBatch
    IL_06ed:  ldarg.0
    IL_06ee:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.CheckBox::.ctor()
    IL_06f3:  stloc.s    V_32
    IL_06f5:  ldloc.s    V_32
    IL_06f7:  ldstr      bytearray (4E 00 C1 1E 6E 00 20 00 63 00 68 00 65 00 20 00   // N...n. .c.h.e. .
                                    6E 00 E9 00 74 00 20 00 28 00 4D 00 61 00 73 00   // n...t. .(.M.a.s.
                                    6B 00 29 00 )                                     // k.).
    IL_06fc:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_0701:  nop
    IL_0702:  ldloc.s    V_32
    IL_0704:  ldarg.0
    IL_0705:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_070a:  ldfld      bool Tvxd.Meps.ToadoSettings::Mask
    IL_070f:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.CheckBox::set_Checked(bool)
    IL_0714:  nop
    IL_0715:  ldloc.s    V_32
    IL_0717:  ldc.i4.s   15
    IL_0719:  ldc.i4.s   75
    IL_071b:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_0720:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_0725:  nop
    IL_0726:  ldloc.s    V_32
    IL_0728:  ldc.i4.1
    IL_0729:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_072e:  nop
    IL_072f:  ldloc.s    V_32
    IL_0731:  stfld      class [System.Windows.Forms]System.Windows.Forms.CheckBox Tvxd.Meps.Toado100Form::chkMask
    IL_0736:  ldarg.0
    IL_0737:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.CheckBox::.ctor()
    IL_073c:  stloc.s    V_33
    IL_073e:  ldloc.s    V_33
    IL_0740:  ldstr      bytearray (47 00 68 00 69 00 20 00 6B 00 E8 00 6D 00 20 00   // G.h.i. .k...m. .
                                    54 00 EA 00 6E 00 2F 00 54 00 41 00 47 00 20 00   // T...n./.T.A.G. .
                                    42 00 6C 00 6F 00 63 00 6B 00 )                   // B.l.o.c.k.
    IL_0745:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_074a:  nop
    IL_074b:  ldloc.s    V_33
    IL_074d:  ldarg.0
    IL_074e:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_0753:  ldfld      bool Tvxd.Meps.ToadoSettings::AddName
    IL_0758:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.CheckBox::set_Checked(bool)
    IL_075d:  nop
    IL_075e:  ldloc.s    V_33
    IL_0760:  ldc.i4     0xdc
    IL_0765:  ldc.i4.s   75
    IL_0767:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_076c:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_0771:  nop
    IL_0772:  ldloc.s    V_33
    IL_0774:  ldc.i4.1
    IL_0775:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_AutoSize(bool)
    IL_077a:  nop
    IL_077b:  ldloc.s    V_33
    IL_077d:  stfld      class [System.Windows.Forms]System.Windows.Forms.CheckBox Tvxd.Meps.Toado100Form::chkAddName
    IL_0782:  ldloc.s    V_8
    IL_0784:  callvirt   instance class [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection [System.Windows.Forms]System.Windows.Forms.Control::get_Controls()
    IL_0789:  ldc.i4.4
    IL_078a:  newarr     [System.Windows.Forms]System.Windows.Forms.Control
    IL_078f:  stloc.s    V_37
    IL_0791:  ldloc.s    V_37
    IL_0793:  ldc.i4.0
    IL_0794:  ldarg.0
    IL_0795:  ldfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbPickFree
    IL_079a:  stelem.ref
    IL_079b:  ldloc.s    V_37
    IL_079d:  ldc.i4.1
    IL_079e:  ldarg.0
    IL_079f:  ldfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbBatch
    IL_07a4:  stelem.ref
    IL_07a5:  ldloc.s    V_37
    IL_07a7:  ldc.i4.2
    IL_07a8:  ldarg.0
    IL_07a9:  ldfld      class [System.Windows.Forms]System.Windows.Forms.CheckBox Tvxd.Meps.Toado100Form::chkMask
    IL_07ae:  stelem.ref
    IL_07af:  ldloc.s    V_37
    IL_07b1:  ldc.i4.3
    IL_07b2:  ldarg.0
    IL_07b3:  ldfld      class [System.Windows.Forms]System.Windows.Forms.CheckBox Tvxd.Meps.Toado100Form::chkAddName
    IL_07b8:  stelem.ref
    IL_07b9:  ldloc.s    V_37
    IL_07bb:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection::AddRange(class [System.Windows.Forms]System.Windows.Forms.Control[])
    IL_07c0:  nop
    IL_07c1:  ldarg.0
    IL_07c2:  call       instance class [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection [System.Windows.Forms]System.Windows.Forms.Control::get_Controls()
    IL_07c7:  ldloc.s    V_8
    IL_07c9:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection::Add(class [System.Windows.Forms]System.Windows.Forms.Control)
    IL_07ce:  nop
    IL_07cf:  ldloc.0
    IL_07d0:  ldc.i4.s   115
    IL_07d2:  add
    IL_07d3:  stloc.0
    IL_07d4:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.Button::.ctor()
    IL_07d9:  stloc.s    V_34
    IL_07db:  ldloc.s    V_34
    IL_07dd:  ldstr      bytearray (B6 25 20 00 10 01 B6 1E 54 00 20 00 54 00 CC 1E   // .% .....T. .T...
                                    41 00 20 00 10 01 D8 1E )                         // A. .....
    IL_07e2:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_07e7:  nop
    IL_07e8:  ldloc.s    V_34
    IL_07ea:  ldc.i4.s   25
    IL_07ec:  ldloc.0
    IL_07ed:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_07f2:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_07f7:  nop
    IL_07f8:  ldloc.s    V_34
    IL_07fa:  ldc.i4     0x87
    IL_07ff:  ldc.i4.s   40
    IL_0801:  newobj     instance void [System.Drawing]System.Drawing.Size::.ctor(int32,
                                                                                  int32)
    IL_0806:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Size(valuetype [System.Drawing]System.Drawing.Size)
    IL_080b:  nop
    IL_080c:  ldloc.s    V_34
    IL_080e:  call       valuetype [System.Drawing]System.Drawing.Color [System.Drawing]System.Drawing.Color::get_LightSteelBlue()
    IL_0813:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_BackColor(valuetype [System.Drawing]System.Drawing.Color)
    IL_0818:  nop
    IL_0819:  ldloc.s    V_34
    IL_081b:  ldstr      "Segoe UI"
    IL_0820:  ldc.r4     9.
    IL_0825:  ldc.i4.1
    IL_0826:  newobj     instance void [System.Drawing]System.Drawing.Font::.ctor(string,
                                                                                  float32,
                                                                                  valuetype [System.Drawing]System.Drawing.FontStyle)
    IL_082b:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Font(class [System.Drawing]System.Drawing.Font)
    IL_0830:  nop
    IL_0831:  ldloc.s    V_34
    IL_0833:  stloc.s    V_9
    IL_0835:  ldloc.s    V_9
    IL_0837:  ldarg.0
    IL_0838:  ldftn      instance void Tvxd.Meps.Toado100Form::'<InitializeComponent>b__19'(object,
                                                                                            class [mscorlib]System.EventArgs)
    IL_083e:  newobj     instance void [mscorlib]System.EventHandler::.ctor(object,
                                                                            native int)
    IL_0843:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::add_Click(class [mscorlib]System.EventHandler)
    IL_0848:  nop
    IL_0849:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.Button::.ctor()
    IL_084e:  stloc.s    V_35
    IL_0850:  ldloc.s    V_35
    IL_0852:  ldstr      bytearray (3D D8 04 DD 20 00 43 00 AC 1E 50 00 20 00 4E 00   // =... .C...P. .N.
                                    48 00 AC 1E 54 00 20 00 54 00 A4 1E 54 00 20 00   // H...T. .T...T. .
                                    43 00 A2 1E )                                     // C...
    IL_0857:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_085c:  nop
    IL_085d:  ldloc.s    V_35
    IL_085f:  ldc.i4     0xaf
    IL_0864:  ldloc.0
    IL_0865:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_086a:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_086f:  nop
    IL_0870:  ldloc.s    V_35
    IL_0872:  ldc.i4     0xb9
    IL_0877:  ldc.i4.s   40
    IL_0879:  newobj     instance void [System.Drawing]System.Drawing.Size::.ctor(int32,
                                                                                  int32)
    IL_087e:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Size(valuetype [System.Drawing]System.Drawing.Size)
    IL_0883:  nop
    IL_0884:  ldloc.s    V_35
    IL_0886:  call       valuetype [System.Drawing]System.Drawing.Color [System.Drawing]System.Drawing.Color::get_LemonChiffon()
    IL_088b:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_BackColor(valuetype [System.Drawing]System.Drawing.Color)
    IL_0890:  nop
    IL_0891:  ldloc.s    V_35
    IL_0893:  ldstr      "Segoe UI"
    IL_0898:  ldc.r4     9.
    IL_089d:  ldc.i4.1
    IL_089e:  newobj     instance void [System.Drawing]System.Drawing.Font::.ctor(string,
                                                                                  float32,
                                                                                  valuetype [System.Drawing]System.Drawing.FontStyle)
    IL_08a3:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Font(class [System.Drawing]System.Drawing.Font)
    IL_08a8:  nop
    IL_08a9:  ldloc.s    V_35
    IL_08ab:  stloc.s    V_10
    IL_08ad:  ldloc.s    V_10
    IL_08af:  ldarg.0
    IL_08b0:  ldftn      instance void Tvxd.Meps.Toado100Form::'<InitializeComponent>b__1a'(object,
                                                                                            class [mscorlib]System.EventArgs)
    IL_08b6:  newobj     instance void [mscorlib]System.EventHandler::.ctor(object,
                                                                            native int)
    IL_08bb:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::add_Click(class [mscorlib]System.EventHandler)
    IL_08c0:  nop
    IL_08c1:  newobj     instance void [System.Windows.Forms]System.Windows.Forms.Button::.ctor()
    IL_08c6:  stloc.s    V_36
    IL_08c8:  ldloc.s    V_36
    IL_08ca:  ldstr      bytearray (10 01 F3 00 6E 00 67 00 )                         // ....n.g.
    IL_08cf:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Text(string)
    IL_08d4:  nop
    IL_08d5:  ldloc.s    V_36
    IL_08d7:  ldc.i4     0x177
    IL_08dc:  ldloc.0
    IL_08dd:  newobj     instance void [System.Drawing]System.Drawing.Point::.ctor(int32,
                                                                                   int32)
    IL_08e2:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Location(valuetype [System.Drawing]System.Drawing.Point)
    IL_08e7:  nop
    IL_08e8:  ldloc.s    V_36
    IL_08ea:  ldc.i4.s   110
    IL_08ec:  ldc.i4.s   40
    IL_08ee:  newobj     instance void [System.Drawing]System.Drawing.Size::.ctor(int32,
                                                                                  int32)
    IL_08f3:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::set_Size(valuetype [System.Drawing]System.Drawing.Size)
    IL_08f8:  nop
    IL_08f9:  ldloc.s    V_36
    IL_08fb:  stloc.s    V_11
    IL_08fd:  ldloc.s    V_11
    IL_08ff:  ldarg.0
    IL_0900:  ldftn      instance void Tvxd.Meps.Toado100Form::'<InitializeComponent>b__1b'(object,
                                                                                            class [mscorlib]System.EventArgs)
    IL_0906:  newobj     instance void [mscorlib]System.EventHandler::.ctor(object,
                                                                            native int)
    IL_090b:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control::add_Click(class [mscorlib]System.EventHandler)
    IL_0910:  nop
    IL_0911:  ldarg.0
    IL_0912:  call       instance class [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection [System.Windows.Forms]System.Windows.Forms.Control::get_Controls()
    IL_0917:  ldc.i4.3
    IL_0918:  newarr     [System.Windows.Forms]System.Windows.Forms.Control
    IL_091d:  stloc.s    V_37
    IL_091f:  ldloc.s    V_37
    IL_0921:  ldc.i4.0
    IL_0922:  ldloc.s    V_9
    IL_0924:  stelem.ref
    IL_0925:  ldloc.s    V_37
    IL_0927:  ldc.i4.1
    IL_0928:  ldloc.s    V_10
    IL_092a:  stelem.ref
    IL_092b:  ldloc.s    V_37
    IL_092d:  ldc.i4.2
    IL_092e:  ldloc.s    V_11
    IL_0930:  stelem.ref
    IL_0931:  ldloc.s    V_37
    IL_0933:  callvirt   instance void [System.Windows.Forms]System.Windows.Forms.Control/ControlCollection::AddRange(class [System.Windows.Forms]System.Windows.Forms.Control[])
    IL_0938:  nop
    IL_0939:  ret
  } // end of method Toado100Form::InitializeComponent

  .method private hidebysig instance void 
          Collect() cil managed
  {
    // Code size       456 (0x1c8)
    .maxstack  4
    .locals init (bool V_0)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldfld      class [System.Windows.Forms]System.Windows.Forms.TextBox Tvxd.Meps.Toado100Form::txtHeight
    IL_0007:  callvirt   instance string [System.Windows.Forms]System.Windows.Forms.Control::get_Text()
    IL_000c:  ldc.i4     0xa7
    IL_0011:  call       class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()
    IL_0016:  ldarg.0
    IL_0017:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_001c:  ldflda     float64 Tvxd.Meps.ToadoSettings::Height
    IL_0021:  call       bool [mscorlib]System.Double::TryParse(string,
                                                                valuetype [mscorlib]System.Globalization.NumberStyles,
                                                                class [mscorlib]System.IFormatProvider,
                                                                float64&)
    IL_0026:  pop
    IL_0027:  ldarg.0
    IL_0028:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_002d:  ldfld      float64 Tvxd.Meps.ToadoSettings::Height
    IL_0032:  ldc.r8     0.0
    IL_003b:  cgt.un
    IL_003d:  stloc.0
    IL_003e:  ldloc.0
    IL_003f:  brtrue.s   IL_0055

    IL_0041:  ldarg.0
    IL_0042:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_0047:  ldc.r8     2.5
    IL_0050:  stfld      float64 Tvxd.Meps.ToadoSettings::Height
    IL_0055:  ldarg.0
    IL_0056:  ldfld      class [System.Windows.Forms]System.Windows.Forms.TextBox Tvxd.Meps.Toado100Form::txtArrowSize
    IL_005b:  callvirt   instance string [System.Windows.Forms]System.Windows.Forms.Control::get_Text()
    IL_0060:  ldc.i4     0xa7
    IL_0065:  call       class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()
    IL_006a:  ldarg.0
    IL_006b:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_0070:  ldflda     float64 Tvxd.Meps.ToadoSettings::ArrowSize
    IL_0075:  call       bool [mscorlib]System.Double::TryParse(string,
                                                                valuetype [mscorlib]System.Globalization.NumberStyles,
                                                                class [mscorlib]System.IFormatProvider,
                                                                float64&)
    IL_007a:  pop
    IL_007b:  ldarg.0
    IL_007c:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_0081:  ldfld      float64 Tvxd.Meps.ToadoSettings::ArrowSize
    IL_0086:  ldc.r8     0.0
    IL_008f:  cgt.un
    IL_0091:  stloc.0
    IL_0092:  ldloc.0
    IL_0093:  brtrue.s   IL_00a9

    IL_0095:  ldarg.0
    IL_0096:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_009b:  ldc.r8     1.5
    IL_00a4:  stfld      float64 Tvxd.Meps.ToadoSettings::ArrowSize
    IL_00a9:  ldarg.0
    IL_00aa:  ldfld      class [System.Windows.Forms]System.Windows.Forms.TextBox Tvxd.Meps.Toado100Form::txtDecimals
    IL_00af:  callvirt   instance string [System.Windows.Forms]System.Windows.Forms.Control::get_Text()
    IL_00b4:  ldarg.0
    IL_00b5:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_00ba:  ldflda     int32 Tvxd.Meps.ToadoSettings::Decimals
    IL_00bf:  call       bool [mscorlib]System.Int32::TryParse(string,
                                                               int32&)
    IL_00c4:  pop
    IL_00c5:  ldarg.0
    IL_00c6:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_00cb:  ldfld      int32 Tvxd.Meps.ToadoSettings::Decimals
    IL_00d0:  ldc.i4.0
    IL_00d1:  blt.s      IL_00e6

    IL_00d3:  ldarg.0
    IL_00d4:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_00d9:  ldfld      int32 Tvxd.Meps.ToadoSettings::Decimals
    IL_00de:  ldc.i4.6
    IL_00df:  cgt
    IL_00e1:  ldc.i4.0
    IL_00e2:  ceq
    IL_00e4:  br.s       IL_00e7

    IL_00e6:  ldc.i4.0
    IL_00e7:  nop
    IL_00e8:  stloc.0
    IL_00e9:  ldloc.0
    IL_00ea:  brtrue.s   IL_00f8

    IL_00ec:  ldarg.0
    IL_00ed:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_00f2:  ldc.i4.3
    IL_00f3:  stfld      int32 Tvxd.Meps.ToadoSettings::Decimals
    IL_00f8:  ldarg.0
    IL_00f9:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_00fe:  ldarg.0
    IL_00ff:  ldfld      class [System.Windows.Forms]System.Windows.Forms.TextBox Tvxd.Meps.Toado100Form::txtStyle
    IL_0104:  callvirt   instance string [System.Windows.Forms]System.Windows.Forms.Control::get_Text()
    IL_0109:  call       bool [mscorlib]System.String::IsNullOrWhiteSpace(string)
    IL_010e:  brtrue.s   IL_0122

    IL_0110:  ldarg.0
    IL_0111:  ldfld      class [System.Windows.Forms]System.Windows.Forms.TextBox Tvxd.Meps.Toado100Form::txtStyle
    IL_0116:  callvirt   instance string [System.Windows.Forms]System.Windows.Forms.Control::get_Text()
    IL_011b:  callvirt   instance string [mscorlib]System.String::Trim()
    IL_0120:  br.s       IL_0127

    IL_0122:  ldstr      "Tuan_arial"
    IL_0127:  nop
    IL_0128:  stfld      string Tvxd.Meps.ToadoSettings::Style
    IL_012d:  ldarg.0
    IL_012e:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_0133:  ldarg.0
    IL_0134:  ldfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbVn2000
    IL_0139:  callvirt   instance bool [System.Windows.Forms]System.Windows.Forms.RadioButton::get_Checked()
    IL_013e:  stfld      bool Tvxd.Meps.ToadoSettings::Vn2000
    IL_0143:  ldarg.0
    IL_0144:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_0149:  ldarg.0
    IL_014a:  ldfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbUnitMm
    IL_014f:  callvirt   instance bool [System.Windows.Forms]System.Windows.Forms.RadioButton::get_Checked()
    IL_0154:  stfld      bool Tvxd.Meps.ToadoSettings::UnitIsMm
    IL_0159:  ldarg.0
    IL_015a:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_015f:  ldarg.0
    IL_0160:  ldfld      class [System.Windows.Forms]System.Windows.Forms.CheckBox Tvxd.Meps.Toado100Form::chkCircle
    IL_0165:  callvirt   instance bool [System.Windows.Forms]System.Windows.Forms.CheckBox::get_Checked()
    IL_016a:  stfld      bool Tvxd.Meps.ToadoSettings::CircleArrow
    IL_016f:  ldarg.0
    IL_0170:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_0175:  ldarg.0
    IL_0176:  ldfld      class [System.Windows.Forms]System.Windows.Forms.CheckBox Tvxd.Meps.Toado100Form::chkUnderline
    IL_017b:  callvirt   instance bool [System.Windows.Forms]System.Windows.Forms.CheckBox::get_Checked()
    IL_0180:  stfld      bool Tvxd.Meps.ToadoSettings::UnderlineBetween
    IL_0185:  ldarg.0
    IL_0186:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_018b:  ldarg.0
    IL_018c:  ldfld      class [System.Windows.Forms]System.Windows.Forms.RadioButton Tvxd.Meps.Toado100Form::rbPickFree
    IL_0191:  callvirt   instance bool [System.Windows.Forms]System.Windows.Forms.RadioButton::get_Checked()
    IL_0196:  stfld      bool Tvxd.Meps.ToadoSettings::PickEach
    IL_019b:  ldarg.0
    IL_019c:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_01a1:  ldarg.0
    IL_01a2:  ldfld      class [System.Windows.Forms]System.Windows.Forms.CheckBox Tvxd.Meps.Toado100Form::chkMask
    IL_01a7:  callvirt   instance bool [System.Windows.Forms]System.Windows.Forms.CheckBox::get_Checked()
    IL_01ac:  stfld      bool Tvxd.Meps.ToadoSettings::Mask
    IL_01b1:  ldarg.0
    IL_01b2:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_01b7:  ldarg.0
    IL_01b8:  ldfld      class [System.Windows.Forms]System.Windows.Forms.CheckBox Tvxd.Meps.Toado100Form::chkAddName
    IL_01bd:  callvirt   instance bool [System.Windows.Forms]System.Windows.Forms.CheckBox::get_Checked()
    IL_01c2:  stfld      bool Tvxd.Meps.ToadoSettings::AddName
    IL_01c7:  ret
  } // end of method Toado100Form::Collect

  .method private hidebysig instance void 
          '<InitializeComponent>b__19'(object sender,
                                       class [mscorlib]System.EventArgs e) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       36 (0x24)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  call       instance void Tvxd.Meps.Toado100Form::Collect()
    IL_0007:  nop
    IL_0008:  ldarg.0
    IL_0009:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_000e:  call       void Tvxd.Meps.Toado100::Save(class Tvxd.Meps.ToadoSettings)
    IL_0013:  nop
    IL_0014:  ldarg.0
    IL_0015:  ldc.i4.1
    IL_0016:  call       instance void [System.Windows.Forms]System.Windows.Forms.Form::set_DialogResult(valuetype [System.Windows.Forms]System.Windows.Forms.DialogResult)
    IL_001b:  nop
    IL_001c:  ldarg.0
    IL_001d:  call       instance void [System.Windows.Forms]System.Windows.Forms.Form::Close()
    IL_0022:  nop
    IL_0023:  ret
  } // end of method Toado100Form::'<InitializeComponent>b__19'

  .method private hidebysig instance void 
          '<InitializeComponent>b__1a'(object sender,
                                       class [mscorlib]System.EventArgs e) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       43 (0x2b)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  call       instance void Tvxd.Meps.Toado100Form::Collect()
    IL_0007:  nop
    IL_0008:  ldarg.0
    IL_0009:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
    IL_000e:  call       void Tvxd.Meps.Toado100::Save(class Tvxd.Meps.ToadoSettings)
    IL_0013:  nop
    IL_0014:  ldarg.0
    IL_0015:  ldc.i4.1
    IL_0016:  stfld      bool Tvxd.Meps.Toado100Form::TriggerRefreshOnly
    IL_001b:  ldarg.0
    IL_001c:  ldc.i4.1
    IL_001d:  call       instance void [System.Windows.Forms]System.Windows.Forms.Form::set_DialogResult(valuetype [System.Windows.Forms]System.Windows.Forms.DialogResult)
    IL_0022:  nop
    IL_0023:  ldarg.0
    IL_0024:  call       instance void [System.Windows.Forms]System.Windows.Forms.Form::Close()
    IL_0029:  nop
    IL_002a:  ret
  } // end of method Toado100Form::'<InitializeComponent>b__1a'

  .method private hidebysig instance void 
          '<InitializeComponent>b__1b'(object sender,
                                       class [mscorlib]System.EventArgs e) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       17 (0x11)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldc.i4.2
    IL_0003:  call       instance void [System.Windows.Forms]System.Windows.Forms.Form::set_DialogResult(valuetype [System.Windows.Forms]System.Windows.Forms.DialogResult)
    IL_0008:  nop
    IL_0009:  ldarg.0
    IL_000a:  call       instance void [System.Windows.Forms]System.Windows.Forms.Form::Close()
    IL_000f:  nop
    IL_0010:  ret
  } // end of method Toado100Form::'<InitializeComponent>b__1b'

} // end of class Tvxd.Meps.Toado100Form

.class public abstract auto ansi sealed beforefieldinit Tvxd.Meps.Toado100
       extends [mscorlib]System.Object
{
  .field public static literal string Reg = "TVXD_TOADO"
  .field public static string IniOverride
  .field private static initonly class [mscorlib]System.Globalization.CultureInfo Inv
  .method public hidebysig specialname static 
          string  get_IniPath() cil managed
  {
    // Code size       31 (0x1f)
    .maxstack  2
    .locals init (string V_0,
             bool V_1)
    IL_0000:  nop
    IL_0001:  ldsfld     string Tvxd.Meps.Toado100::IniOverride
    IL_0006:  ldnull
    IL_0007:  ceq
    IL_0009:  stloc.1
    IL_000a:  ldloc.1
    IL_000b:  brtrue.s   IL_0015

    IL_000d:  ldsfld     string Tvxd.Meps.Toado100::IniOverride
    IL_0012:  stloc.0
    IL_0013:  br.s       IL_001d

    IL_0015:  call       string Tvxd.Meps.ToadoSettings::get_IniPath()
    IL_001a:  stloc.0
    IL_001b:  br.s       IL_001d

    IL_001d:  ldloc.0
    IL_001e:  ret
  } // end of method Toado100::get_IniPath

  .method public hidebysig static class Tvxd.Meps.ToadoSettings 
          Load() cil managed
  {
    // Code size       804 (0x324)
    .maxstack  4
    .locals init (class Tvxd.Meps.ToadoSettings V_0,
             string V_1,
             int32 V_2,
             string V_3,
             string V_4,
             class Tvxd.Meps.ToadoSettings V_5,
             bool V_6,
             string[] V_7,
             int32 V_8)
    IL_0000:  nop
    IL_0001:  newobj     instance void Tvxd.Meps.ToadoSettings::.ctor()
    IL_0006:  stloc.0
    .try
    {
      IL_0007:  nop
      IL_0008:  call       string Tvxd.Meps.Toado100::get_IniPath()
      IL_000d:  call       bool [mscorlib]System.IO.File::Exists(string)
      IL_0012:  ldc.i4.0
      IL_0013:  ceq
      IL_0015:  stloc.s    V_6
      IL_0017:  ldloc.s    V_6
      IL_0019:  brtrue     IL_0313

      IL_001e:  nop
      IL_001f:  nop
      IL_0020:  call       string Tvxd.Meps.Toado100::get_IniPath()
      IL_0025:  call       class [mscorlib]System.Text.Encoding [mscorlib]System.Text.Encoding::get_UTF8()
      IL_002a:  call       string[] [mscorlib]System.IO.File::ReadAllLines(string,
                                                                           class [mscorlib]System.Text.Encoding)
      IL_002f:  stloc.s    V_7
      IL_0031:  ldc.i4.0
      IL_0032:  stloc.s    V_8
      IL_0034:  br         IL_0301

      IL_0039:  ldloc.s    V_7
      IL_003b:  ldloc.s    V_8
      IL_003d:  ldelem.ref
      IL_003e:  stloc.1
      IL_003f:  nop
      IL_0040:  ldloc.1
      IL_0041:  ldc.i4.s   61
      IL_0043:  callvirt   instance int32 [mscorlib]System.String::IndexOf(char)
      IL_0048:  stloc.2
      IL_0049:  ldloc.2
      IL_004a:  ldc.i4.0
      IL_004b:  cgt
      IL_004d:  stloc.s    V_6
      IL_004f:  ldloc.s    V_6
      IL_0051:  brtrue.s   IL_0058

      IL_0053:  br         IL_02fb

      IL_0058:  ldloc.1
      IL_0059:  ldc.i4.0
      IL_005a:  ldloc.2
      IL_005b:  callvirt   instance string [mscorlib]System.String::Substring(int32,
                                                                              int32)
      IL_0060:  callvirt   instance string [mscorlib]System.String::Trim()
      IL_0065:  stloc.3
      IL_0066:  ldloc.1
      IL_0067:  ldloc.2
      IL_0068:  ldc.i4.1
      IL_0069:  add
      IL_006a:  callvirt   instance string [mscorlib]System.String::Substring(int32)
      IL_006f:  callvirt   instance string [mscorlib]System.String::Trim()
      IL_0074:  stloc.s    V_4
      IL_0076:  ldloc.3
      IL_0077:  ldstr      "Style"
      IL_007c:  ldc.i4.5
      IL_007d:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_0082:  ldc.i4.0
      IL_0083:  ceq
      IL_0085:  stloc.s    V_6
      IL_0087:  ldloc.s    V_6
      IL_0089:  brtrue.s   IL_0098

      IL_008b:  ldloc.0
      IL_008c:  ldloc.s    V_4
      IL_008e:  stfld      string Tvxd.Meps.ToadoSettings::Style
      IL_0093:  br         IL_02fa

      IL_0098:  ldloc.3
      IL_0099:  ldstr      "Layer"
      IL_009e:  ldc.i4.5
      IL_009f:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_00a4:  ldc.i4.0
      IL_00a5:  ceq
      IL_00a7:  stloc.s    V_6
      IL_00a9:  ldloc.s    V_6
      IL_00ab:  brtrue.s   IL_00ba

      IL_00ad:  ldloc.0
      IL_00ae:  ldloc.s    V_4
      IL_00b0:  stfld      string Tvxd.Meps.ToadoSettings::Layer
      IL_00b5:  br         IL_02fa

      IL_00ba:  ldloc.3
      IL_00bb:  ldstr      "Height"
      IL_00c0:  ldc.i4.5
      IL_00c1:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_00c6:  ldc.i4.0
      IL_00c7:  ceq
      IL_00c9:  stloc.s    V_6
      IL_00cb:  ldloc.s    V_6
      IL_00cd:  brtrue.s   IL_00ec

      IL_00cf:  ldloc.s    V_4
      IL_00d1:  ldc.i4     0xa7
      IL_00d6:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
      IL_00db:  ldloc.0
      IL_00dc:  ldflda     float64 Tvxd.Meps.ToadoSettings::Height
      IL_00e1:  call       bool [mscorlib]System.Double::TryParse(string,
                                                                  valuetype [mscorlib]System.Globalization.NumberStyles,
                                                                  class [mscorlib]System.IFormatProvider,
                                                                  float64&)
      IL_00e6:  pop
      IL_00e7:  br         IL_02fa

      IL_00ec:  ldloc.3
      IL_00ed:  ldstr      "ArrowSize"
      IL_00f2:  ldc.i4.5
      IL_00f3:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_00f8:  ldc.i4.0
      IL_00f9:  ceq
      IL_00fb:  stloc.s    V_6
      IL_00fd:  ldloc.s    V_6
      IL_00ff:  brtrue.s   IL_011e

      IL_0101:  ldloc.s    V_4
      IL_0103:  ldc.i4     0xa7
      IL_0108:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
      IL_010d:  ldloc.0
      IL_010e:  ldflda     float64 Tvxd.Meps.ToadoSettings::ArrowSize
      IL_0113:  call       bool [mscorlib]System.Double::TryParse(string,
                                                                  valuetype [mscorlib]System.Globalization.NumberStyles,
                                                                  class [mscorlib]System.IFormatProvider,
                                                                  float64&)
      IL_0118:  pop
      IL_0119:  br         IL_02fa

      IL_011e:  ldloc.3
      IL_011f:  ldstr      "Distance"
      IL_0124:  ldc.i4.5
      IL_0125:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_012a:  ldc.i4.0
      IL_012b:  ceq
      IL_012d:  stloc.s    V_6
      IL_012f:  ldloc.s    V_6
      IL_0131:  brtrue.s   IL_0150

      IL_0133:  ldloc.s    V_4
      IL_0135:  ldc.i4     0xa7
      IL_013a:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
      IL_013f:  ldloc.0
      IL_0140:  ldflda     float64 Tvxd.Meps.ToadoSettings::Distance
      IL_0145:  call       bool [mscorlib]System.Double::TryParse(string,
                                                                  valuetype [mscorlib]System.Globalization.NumberStyles,
                                                                  class [mscorlib]System.IFormatProvider,
                                                                  float64&)
      IL_014a:  pop
      IL_014b:  br         IL_02fa

      IL_0150:  ldloc.3
      IL_0151:  ldstr      "Decimals"
      IL_0156:  ldc.i4.5
      IL_0157:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_015c:  ldc.i4.0
      IL_015d:  ceq
      IL_015f:  stloc.s    V_6
      IL_0161:  ldloc.s    V_6
      IL_0163:  brtrue.s   IL_0178

      IL_0165:  ldloc.s    V_4
      IL_0167:  ldloc.0
      IL_0168:  ldflda     int32 Tvxd.Meps.ToadoSettings::Decimals
      IL_016d:  call       bool [mscorlib]System.Int32::TryParse(string,
                                                                 int32&)
      IL_0172:  pop
      IL_0173:  br         IL_02fa

      IL_0178:  ldloc.3
      IL_0179:  ldstr      "Vn2000"
      IL_017e:  ldc.i4.5
      IL_017f:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_0184:  ldc.i4.0
      IL_0185:  ceq
      IL_0187:  stloc.s    V_6
      IL_0189:  ldloc.s    V_6
      IL_018b:  brtrue.s   IL_01a0

      IL_018d:  ldloc.s    V_4
      IL_018f:  ldloc.0
      IL_0190:  ldflda     bool Tvxd.Meps.ToadoSettings::Vn2000
      IL_0195:  call       bool [mscorlib]System.Boolean::TryParse(string,
                                                                   bool&)
      IL_019a:  pop
      IL_019b:  br         IL_02fa

      IL_01a0:  ldloc.3
      IL_01a1:  ldstr      "UnitIsMm"
      IL_01a6:  ldc.i4.5
      IL_01a7:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_01ac:  ldc.i4.0
      IL_01ad:  ceq
      IL_01af:  stloc.s    V_6
      IL_01b1:  ldloc.s    V_6
      IL_01b3:  brtrue.s   IL_01c8

      IL_01b5:  ldloc.s    V_4
      IL_01b7:  ldloc.0
      IL_01b8:  ldflda     bool Tvxd.Meps.ToadoSettings::UnitIsMm
      IL_01bd:  call       bool [mscorlib]System.Boolean::TryParse(string,
                                                                   bool&)
      IL_01c2:  pop
      IL_01c3:  br         IL_02fa

      IL_01c8:  ldloc.3
      IL_01c9:  ldstr      "Mask"
      IL_01ce:  ldc.i4.5
      IL_01cf:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_01d4:  ldc.i4.0
      IL_01d5:  ceq
      IL_01d7:  stloc.s    V_6
      IL_01d9:  ldloc.s    V_6
      IL_01db:  brtrue.s   IL_01f0

      IL_01dd:  ldloc.s    V_4
      IL_01df:  ldloc.0
      IL_01e0:  ldflda     bool Tvxd.Meps.ToadoSettings::Mask
      IL_01e5:  call       bool [mscorlib]System.Boolean::TryParse(string,
                                                                   bool&)
      IL_01ea:  pop
      IL_01eb:  br         IL_02fa

      IL_01f0:  ldloc.3
      IL_01f1:  ldstr      "Frame"
      IL_01f6:  ldc.i4.5
      IL_01f7:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_01fc:  ldc.i4.0
      IL_01fd:  ceq
      IL_01ff:  stloc.s    V_6
      IL_0201:  ldloc.s    V_6
      IL_0203:  brtrue.s   IL_0218

      IL_0205:  ldloc.s    V_4
      IL_0207:  ldloc.0
      IL_0208:  ldflda     bool Tvxd.Meps.ToadoSettings::Frame
      IL_020d:  call       bool [mscorlib]System.Boolean::TryParse(string,
                                                                   bool&)
      IL_0212:  pop
      IL_0213:  br         IL_02fa

      IL_0218:  ldloc.3
      IL_0219:  ldstr      "Arrow"
      IL_021e:  ldc.i4.5
      IL_021f:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_0224:  ldc.i4.0
      IL_0225:  ceq
      IL_0227:  stloc.s    V_6
      IL_0229:  ldloc.s    V_6
      IL_022b:  brtrue.s   IL_0240

      IL_022d:  ldloc.s    V_4
      IL_022f:  ldloc.0
      IL_0230:  ldflda     bool Tvxd.Meps.ToadoSettings::Arrow
      IL_0235:  call       bool [mscorlib]System.Boolean::TryParse(string,
                                                                   bool&)
      IL_023a:  pop
      IL_023b:  br         IL_02fa

      IL_0240:  ldloc.3
      IL_0241:  ldstr      "CircleArrow"
      IL_0246:  ldc.i4.5
      IL_0247:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_024c:  ldc.i4.0
      IL_024d:  ceq
      IL_024f:  stloc.s    V_6
      IL_0251:  ldloc.s    V_6
      IL_0253:  brtrue.s   IL_0268

      IL_0255:  ldloc.s    V_4
      IL_0257:  ldloc.0
      IL_0258:  ldflda     bool Tvxd.Meps.ToadoSettings::CircleArrow
      IL_025d:  call       bool [mscorlib]System.Boolean::TryParse(string,
                                                                   bool&)
      IL_0262:  pop
      IL_0263:  br         IL_02fa

      IL_0268:  ldloc.3
      IL_0269:  ldstr      "UnderlineBetween"
      IL_026e:  ldc.i4.5
      IL_026f:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_0274:  ldc.i4.0
      IL_0275:  ceq
      IL_0277:  stloc.s    V_6
      IL_0279:  ldloc.s    V_6
      IL_027b:  brtrue.s   IL_028d

      IL_027d:  ldloc.s    V_4
      IL_027f:  ldloc.0
      IL_0280:  ldflda     bool Tvxd.Meps.ToadoSettings::UnderlineBetween
      IL_0285:  call       bool [mscorlib]System.Boolean::TryParse(string,
                                                                   bool&)
      IL_028a:  pop
      IL_028b:  br.s       IL_02fa

      IL_028d:  ldloc.3
      IL_028e:  ldstr      "PickEach"
      IL_0293:  ldc.i4.5
      IL_0294:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_0299:  ldc.i4.0
      IL_029a:  ceq
      IL_029c:  stloc.s    V_6
      IL_029e:  ldloc.s    V_6
      IL_02a0:  brtrue.s   IL_02b2

      IL_02a2:  ldloc.s    V_4
      IL_02a4:  ldloc.0
      IL_02a5:  ldflda     bool Tvxd.Meps.ToadoSettings::PickEach
      IL_02aa:  call       bool [mscorlib]System.Boolean::TryParse(string,
                                                                   bool&)
      IL_02af:  pop
      IL_02b0:  br.s       IL_02fa

      IL_02b2:  ldloc.3
      IL_02b3:  ldstr      "AddName"
      IL_02b8:  ldc.i4.5
      IL_02b9:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_02be:  ldc.i4.0
      IL_02bf:  ceq
      IL_02c1:  stloc.s    V_6
      IL_02c3:  ldloc.s    V_6
      IL_02c5:  brtrue.s   IL_02d7

      IL_02c7:  ldloc.s    V_4
      IL_02c9:  ldloc.0
      IL_02ca:  ldflda     bool Tvxd.Meps.ToadoSettings::AddName
      IL_02cf:  call       bool [mscorlib]System.Boolean::TryParse(string,
                                                                   bool&)
      IL_02d4:  pop
      IL_02d5:  br.s       IL_02fa

      IL_02d7:  ldloc.3
      IL_02d8:  ldstr      "TwoLines"
      IL_02dd:  ldc.i4.5
      IL_02de:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_02e3:  ldc.i4.0
      IL_02e4:  ceq
      IL_02e6:  stloc.s    V_6
      IL_02e8:  ldloc.s    V_6
      IL_02ea:  brtrue.s   IL_02fa

      IL_02ec:  ldloc.s    V_4
      IL_02ee:  ldloc.0
      IL_02ef:  ldflda     bool Tvxd.Meps.ToadoSettings::TwoLines
      IL_02f4:  call       bool [mscorlib]System.Boolean::TryParse(string,
                                                                   bool&)
      IL_02f9:  pop
      IL_02fa:  nop
      IL_02fb:  ldloc.s    V_8
      IL_02fd:  ldc.i4.1
      IL_02fe:  add
      IL_02ff:  stloc.s    V_8
      IL_0301:  ldloc.s    V_8
      IL_0303:  ldloc.s    V_7
      IL_0305:  ldlen
      IL_0306:  conv.i4
      IL_0307:  clt
      IL_0309:  stloc.s    V_6
      IL_030b:  ldloc.s    V_6
      IL_030d:  brtrue     IL_0039

      IL_0312:  nop
      IL_0313:  nop
      IL_0314:  leave.s    IL_031b

    }  // end .try
    catch [mscorlib]System.Object 
    {
      IL_0316:  pop
      IL_0317:  nop
      IL_0318:  nop
      IL_0319:  leave.s    IL_031b

    }  // end handler
    IL_031b:  nop
    IL_031c:  ldloc.0
    IL_031d:  stloc.s    V_5
    IL_031f:  br.s       IL_0321

    IL_0321:  ldloc.s    V_5
    IL_0323:  ret
  } // end of method Toado100::Load

  .method public hidebysig static void  Save(class Tvxd.Meps.ToadoSettings s) cil managed
  {
    // Code size       503 (0x1f7)
    .maxstack  4
    .locals init (class [mscorlib]System.IO.StreamWriter V_0,
             bool V_1)
    IL_0000:  nop
    .try
    {
      IL_0001:  nop
      IL_0002:  call       string Tvxd.Meps.Toado100::get_IniPath()
      IL_0007:  ldc.i4.0
      IL_0008:  call       class [mscorlib]System.Text.Encoding [mscorlib]System.Text.Encoding::get_UTF8()
      IL_000d:  newobj     instance void [mscorlib]System.IO.StreamWriter::.ctor(string,
                                                                                 bool,
                                                                                 class [mscorlib]System.Text.Encoding)
      IL_0012:  stloc.0
      .try
      {
        IL_0013:  nop
        IL_0014:  ldloc.0
        IL_0015:  ldstr      "Style="
        IL_001a:  ldarg.0
        IL_001b:  ldfld      string Tvxd.Meps.ToadoSettings::Style
        IL_0020:  call       string [mscorlib]System.String::Concat(string,
                                                                    string)
        IL_0025:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_002a:  nop
        IL_002b:  ldloc.0
        IL_002c:  ldstr      "Layer="
        IL_0031:  ldarg.0
        IL_0032:  ldfld      string Tvxd.Meps.ToadoSettings::Layer
        IL_0037:  call       string [mscorlib]System.String::Concat(string,
                                                                    string)
        IL_003c:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_0041:  nop
        IL_0042:  ldloc.0
        IL_0043:  ldstr      "Height="
        IL_0048:  ldarg.0
        IL_0049:  ldflda     float64 Tvxd.Meps.ToadoSettings::Height
        IL_004e:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
        IL_0053:  call       instance string [mscorlib]System.Double::ToString(class [mscorlib]System.IFormatProvider)
        IL_0058:  call       string [mscorlib]System.String::Concat(string,
                                                                    string)
        IL_005d:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_0062:  nop
        IL_0063:  ldloc.0
        IL_0064:  ldstr      "ArrowSize="
        IL_0069:  ldarg.0
        IL_006a:  ldflda     float64 Tvxd.Meps.ToadoSettings::ArrowSize
        IL_006f:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
        IL_0074:  call       instance string [mscorlib]System.Double::ToString(class [mscorlib]System.IFormatProvider)
        IL_0079:  call       string [mscorlib]System.String::Concat(string,
                                                                    string)
        IL_007e:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_0083:  nop
        IL_0084:  ldloc.0
        IL_0085:  ldstr      "Distance="
        IL_008a:  ldarg.0
        IL_008b:  ldflda     float64 Tvxd.Meps.ToadoSettings::Distance
        IL_0090:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
        IL_0095:  call       instance string [mscorlib]System.Double::ToString(class [mscorlib]System.IFormatProvider)
        IL_009a:  call       string [mscorlib]System.String::Concat(string,
                                                                    string)
        IL_009f:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_00a4:  nop
        IL_00a5:  ldloc.0
        IL_00a6:  ldstr      "Decimals="
        IL_00ab:  ldarg.0
        IL_00ac:  ldfld      int32 Tvxd.Meps.ToadoSettings::Decimals
        IL_00b1:  box        [mscorlib]System.Int32
        IL_00b6:  call       string [mscorlib]System.String::Concat(object,
                                                                    object)
        IL_00bb:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_00c0:  nop
        IL_00c1:  ldloc.0
        IL_00c2:  ldstr      "Vn2000="
        IL_00c7:  ldarg.0
        IL_00c8:  ldfld      bool Tvxd.Meps.ToadoSettings::Vn2000
        IL_00cd:  box        [mscorlib]System.Boolean
        IL_00d2:  call       string [mscorlib]System.String::Concat(object,
                                                                    object)
        IL_00d7:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_00dc:  nop
        IL_00dd:  ldloc.0
        IL_00de:  ldstr      "UnitIsMm="
        IL_00e3:  ldarg.0
        IL_00e4:  ldfld      bool Tvxd.Meps.ToadoSettings::UnitIsMm
        IL_00e9:  box        [mscorlib]System.Boolean
        IL_00ee:  call       string [mscorlib]System.String::Concat(object,
                                                                    object)
        IL_00f3:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_00f8:  nop
        IL_00f9:  ldloc.0
        IL_00fa:  ldstr      "Mask="
        IL_00ff:  ldarg.0
        IL_0100:  ldfld      bool Tvxd.Meps.ToadoSettings::Mask
        IL_0105:  box        [mscorlib]System.Boolean
        IL_010a:  call       string [mscorlib]System.String::Concat(object,
                                                                    object)
        IL_010f:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_0114:  nop
        IL_0115:  ldloc.0
        IL_0116:  ldstr      "Frame="
        IL_011b:  ldarg.0
        IL_011c:  ldfld      bool Tvxd.Meps.ToadoSettings::Frame
        IL_0121:  box        [mscorlib]System.Boolean
        IL_0126:  call       string [mscorlib]System.String::Concat(object,
                                                                    object)
        IL_012b:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_0130:  nop
        IL_0131:  ldloc.0
        IL_0132:  ldstr      "Arrow="
        IL_0137:  ldarg.0
        IL_0138:  ldfld      bool Tvxd.Meps.ToadoSettings::Arrow
        IL_013d:  box        [mscorlib]System.Boolean
        IL_0142:  call       string [mscorlib]System.String::Concat(object,
                                                                    object)
        IL_0147:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_014c:  nop
        IL_014d:  ldloc.0
        IL_014e:  ldstr      "CircleArrow="
        IL_0153:  ldarg.0
        IL_0154:  ldfld      bool Tvxd.Meps.ToadoSettings::CircleArrow
        IL_0159:  box        [mscorlib]System.Boolean
        IL_015e:  call       string [mscorlib]System.String::Concat(object,
                                                                    object)
        IL_0163:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_0168:  nop
        IL_0169:  ldloc.0
        IL_016a:  ldstr      "UnderlineBetween="
        IL_016f:  ldarg.0
        IL_0170:  ldfld      bool Tvxd.Meps.ToadoSettings::UnderlineBetween
        IL_0175:  box        [mscorlib]System.Boolean
        IL_017a:  call       string [mscorlib]System.String::Concat(object,
                                                                    object)
        IL_017f:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_0184:  nop
        IL_0185:  ldloc.0
        IL_0186:  ldstr      "PickEach="
        IL_018b:  ldarg.0
        IL_018c:  ldfld      bool Tvxd.Meps.ToadoSettings::PickEach
        IL_0191:  box        [mscorlib]System.Boolean
        IL_0196:  call       string [mscorlib]System.String::Concat(object,
                                                                    object)
        IL_019b:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_01a0:  nop
        IL_01a1:  ldloc.0
        IL_01a2:  ldstr      "AddName="
        IL_01a7:  ldarg.0
        IL_01a8:  ldfld      bool Tvxd.Meps.ToadoSettings::AddName
        IL_01ad:  box        [mscorlib]System.Boolean
        IL_01b2:  call       string [mscorlib]System.String::Concat(object,
                                                                    object)
        IL_01b7:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_01bc:  nop
        IL_01bd:  ldloc.0
        IL_01be:  ldstr      "TwoLines="
        IL_01c3:  ldarg.0
        IL_01c4:  ldfld      bool Tvxd.Meps.ToadoSettings::TwoLines
        IL_01c9:  box        [mscorlib]System.Boolean
        IL_01ce:  call       string [mscorlib]System.String::Concat(object,
                                                                    object)
        IL_01d3:  callvirt   instance void [mscorlib]System.IO.TextWriter::WriteLine(string)
        IL_01d8:  nop
        IL_01d9:  nop
        IL_01da:  leave.s    IL_01ec

      }  // end .try
      finally
      {
        IL_01dc:  ldloc.0
        IL_01dd:  ldnull
        IL_01de:  ceq
        IL_01e0:  stloc.1
        IL_01e1:  ldloc.1
        IL_01e2:  brtrue.s   IL_01eb

        IL_01e4:  ldloc.0
        IL_01e5:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_01ea:  nop
        IL_01eb:  endfinally
      }  // end handler
      IL_01ec:  nop
      IL_01ed:  nop
      IL_01ee:  leave.s    IL_01f5

    }  // end .try
    catch [mscorlib]System.Object 
    {
      IL_01f0:  pop
      IL_01f1:  nop
      IL_01f2:  nop
      IL_01f3:  leave.s    IL_01f5

    }  // end handler
    IL_01f5:  nop
    IL_01f6:  ret
  } // end of method Toado100::Save

  .method public hidebysig static string 
          Esc(string text) cil managed
  {
    // Code size       73 (0x49)
    .maxstack  3
    .locals init (string V_0,
             bool V_1)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  call       bool [mscorlib]System.String::IsNullOrEmpty(string)
    IL_0007:  ldc.i4.0
    IL_0008:  ceq
    IL_000a:  stloc.1
    IL_000b:  ldloc.1
    IL_000c:  brtrue.s   IL_0016

    IL_000e:  ldstr      ""
    IL_0013:  stloc.0
    IL_0014:  br.s       IL_0047

    IL_0016:  ldarg.0
    IL_0017:  ldstr      "\\"
    IL_001c:  ldstr      "\\\\"
    IL_0021:  callvirt   instance string [mscorlib]System.String::Replace(string,
                                                                          string)
    IL_0026:  ldstr      "{"
    IL_002b:  ldstr      "\\{"
    IL_0030:  callvirt   instance string [mscorlib]System.String::Replace(string,
                                                                          string)
    IL_0035:  ldstr      "}"
    IL_003a:  ldstr      "\\}"
    IL_003f:  callvirt   instance string [mscorlib]System.String::Replace(string,
                                                                          string)
    IL_0044:  stloc.0
    IL_0045:  br.s       IL_0047

    IL_0047:  ldloc.0
    IL_0048:  ret
  } // end of method Toado100::Esc

  .method public hidebysig static string 
          Format(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d p,
                 class Tvxd.Meps.ToadoSettings s,
                 string blockName,
                 bool 'unused') cil managed
  {
    // Code size       301 (0x12d)
    .maxstack  4
    .locals init (float64 V_0,
             float64 V_1,
             float64 V_2,
             string V_3,
             string V_4,
             string V_5,
             string V_6,
             string V_7,
             bool V_8)
    IL_0000:  nop
    IL_0001:  ldarg.1
    IL_0002:  ldfld      bool Tvxd.Meps.ToadoSettings::UnitIsMm
    IL_0007:  brtrue.s   IL_0014

    IL_0009:  ldc.r8     1.
    IL_0012:  br.s       IL_001d

    IL_0014:  ldc.r8     1.e-03
    IL_001d:  nop
    IL_001e:  stloc.0
    IL_001f:  ldarg.1
    IL_0020:  ldfld      bool Tvxd.Meps.ToadoSettings::Vn2000
    IL_0025:  brtrue.s   IL_0030

    IL_0027:  ldarga.s   p
    IL_0029:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
    IL_002e:  br.s       IL_0037

    IL_0030:  ldarga.s   p
    IL_0032:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
    IL_0037:  nop
    IL_0038:  ldloc.0
    IL_0039:  mul
    IL_003a:  stloc.1
    IL_003b:  ldarg.1
    IL_003c:  ldfld      bool Tvxd.Meps.ToadoSettings::Vn2000
    IL_0041:  brtrue.s   IL_004c

    IL_0043:  ldarga.s   p
    IL_0045:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
    IL_004a:  br.s       IL_0053

    IL_004c:  ldarga.s   p
    IL_004e:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
    IL_0053:  nop
    IL_0054:  ldloc.0
    IL_0055:  mul
    IL_0056:  stloc.2
    IL_0057:  ldarg.1
    IL_0058:  ldfld      int32 Tvxd.Meps.ToadoSettings::Decimals
    IL_005d:  ldc.i4.0
    IL_005e:  bgt.s      IL_0067

    IL_0060:  ldstr      "0"
    IL_0065:  br.s       IL_007e

    IL_0067:  ldstr      "0."
    IL_006c:  ldc.i4.s   48
    IL_006e:  ldarg.1
    IL_006f:  ldfld      int32 Tvxd.Meps.ToadoSettings::Decimals
    IL_0074:  newobj     instance void [mscorlib]System.String::.ctor(char,
                                                                      int32)
    IL_0079:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_007e:  nop
    IL_007f:  stloc.3
    IL_0080:  ldstr      "X="
    IL_0085:  ldloca.s   V_1
    IL_0087:  ldloc.3
    IL_0088:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
    IL_008d:  call       instance string [mscorlib]System.Double::ToString(string,
                                                                           class [mscorlib]System.IFormatProvider)
    IL_0092:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_0097:  stloc.s    V_4
    IL_0099:  ldstr      "Y="
    IL_009e:  ldloca.s   V_2
    IL_00a0:  ldloc.3
    IL_00a1:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
    IL_00a6:  call       instance string [mscorlib]System.Double::ToString(string,
                                                                           class [mscorlib]System.IFormatProvider)
    IL_00ab:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_00b0:  stloc.s    V_5
    IL_00b2:  ldarg.1
    IL_00b3:  ldfld      bool Tvxd.Meps.ToadoSettings::TwoLines
    IL_00b8:  ldc.i4.0
    IL_00b9:  ceq
    IL_00bb:  stloc.s    V_8
    IL_00bd:  ldloc.s    V_8
    IL_00bf:  brtrue.s   IL_00da

    IL_00c1:  nop
    IL_00c2:  ldstr      "\\pxqc;"
    IL_00c7:  ldloc.s    V_4
    IL_00c9:  ldstr      "\\P\\pxqc;"
    IL_00ce:  ldloc.s    V_5
    IL_00d0:  call       string [mscorlib]System.String::Concat(string,
                                                                string,
                                                                string,
                                                                string)
    IL_00d5:  stloc.s    V_6
    IL_00d7:  nop
    IL_00d8:  br.s       IL_00f1

    IL_00da:  nop
    IL_00db:  ldstr      "\\pxqc;"
    IL_00e0:  ldloc.s    V_4
    IL_00e2:  ldstr      ", "
    IL_00e7:  ldloc.s    V_5
    IL_00e9:  call       string [mscorlib]System.String::Concat(string,
                                                                string,
                                                                string,
                                                                string)
    IL_00ee:  stloc.s    V_6
    IL_00f0:  nop
    IL_00f1:  ldarg.1
    IL_00f2:  ldfld      bool Tvxd.Meps.ToadoSettings::AddName
    IL_00f7:  brfalse.s  IL_0101

    IL_00f9:  ldarg.2
    IL_00fa:  call       bool [mscorlib]System.String::IsNullOrEmpty(string)
    IL_00ff:  br.s       IL_0102

    IL_0101:  ldc.i4.1
    IL_0102:  nop
    IL_0103:  stloc.s    V_8
    IL_0105:  ldloc.s    V_8
    IL_0107:  brtrue.s   IL_0124

    IL_0109:  nop
    IL_010a:  ldstr      "\\pxqc;"
    IL_010f:  ldarg.2
    IL_0110:  call       string Tvxd.Meps.Toado100::Esc(string)
    IL_0115:  ldstr      "\\P"
    IL_011a:  ldloc.s    V_6
    IL_011c:  call       string [mscorlib]System.String::Concat(string,
                                                                string,
                                                                string,
                                                                string)
    IL_0121:  stloc.s    V_6
    IL_0123:  nop
    IL_0124:  ldloc.s    V_6
    IL_0126:  stloc.s    V_7
    IL_0128:  br.s       IL_012a

    IL_012a:  ldloc.s    V_7
    IL_012c:  ret
  } // end of method Toado100::Format

  .method public hidebysig static string 
          NameOf(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction tr,
                 class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference 'br') cil managed
  {
    // Code size       88 (0x58)
    .maxstack  3
    .locals init (class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord V_0,
             string V_1,
             bool V_2)
    IL_0000:  nop
    IL_0001:  ldarg.1
    IL_0002:  ldnull
    IL_0003:  call       bool [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper::op_Equality(class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper,
                                                                                               class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper)
    IL_0008:  ldc.i4.0
    IL_0009:  ceq
    IL_000b:  stloc.2
    IL_000c:  ldloc.2
    IL_000d:  brtrue.s   IL_0017

    IL_000f:  ldstr      ""
    IL_0014:  stloc.1
    IL_0015:  br.s       IL_0055

    .try
    {
      IL_0017:  nop
      IL_0018:  ldarg.1
      IL_0019:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference::get_IsDynamicBlock()
      IL_001e:  ldc.i4.0
      IL_001f:  ceq
      IL_0021:  stloc.2
      IL_0022:  ldloc.2
      IL_0023:  brtrue.s   IL_0042

      IL_0025:  nop
      IL_0026:  ldarg.0
      IL_0027:  ldarg.1
      IL_0028:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference::get_DynamicBlockTableRecord()
      IL_002d:  ldc.i4.0
      IL_002e:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
      IL_0033:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord
      IL_0038:  stloc.0
      IL_0039:  ldloc.0
      IL_003a:  callvirt   instance string [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord::get_Name()
      IL_003f:  stloc.1
      IL_0040:  leave.s    IL_0055

      IL_0042:  ldarg.1
      IL_0043:  callvirt   instance string [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference::get_Name()
      IL_0048:  stloc.1
      IL_0049:  leave.s    IL_0055

    }  // end .try
    catch [mscorlib]System.Object 
    {
      IL_004b:  pop
      IL_004c:  nop
      IL_004d:  ldstr      ""
      IL_0052:  stloc.1
      IL_0053:  leave.s    IL_0055

    }  // end handler
    IL_0055:  nop
    IL_0056:  ldloc.1
    IL_0057:  ret
  } // end of method Toado100::NameOf

  .method public hidebysig static valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d 
          Tip(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference 'br',
              bool center) cil managed
  {
    // Code size       34 (0x22)
    .maxstack  2
    .locals init (valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_0,
             bool V_1)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldnull
    IL_0003:  call       bool [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper::op_Equality(class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper,
                                                                                               class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper)
    IL_0008:  ldc.i4.0
    IL_0009:  ceq
    IL_000b:  stloc.1
    IL_000c:  ldloc.1
    IL_000d:  brtrue.s   IL_0017

    IL_000f:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Origin()
    IL_0014:  stloc.0
    IL_0015:  br.s       IL_0020

    IL_0017:  ldarg.0
    IL_0018:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference::get_Position()
    IL_001d:  stloc.0
    IL_001e:  br.s       IL_0020

    IL_0020:  ldloc.0
    IL_0021:  ret
  } // end of method Toado100::Tip

  .method public hidebysig static valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId 
          EnsureLayer(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction tr,
                      class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db,
                      string layerName) cil managed
  {
    // Code size       111 (0x6f)
    .maxstack  3
    .locals init (class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTable V_0,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTableRecord V_1,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_2,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTableRecord V_3,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_4,
             bool V_5)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_LayerTableId()
    IL_0008:  ldc.i4.0
    IL_0009:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                              valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
    IL_000e:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTable
    IL_0013:  stloc.0
    IL_0014:  ldloc.0
    IL_0015:  ldarg.2
    IL_0016:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::Has(string)
    IL_001b:  stloc.s    V_5
    IL_001d:  ldloc.s    V_5
    IL_001f:  brtrue.s   IL_0061

    IL_0021:  nop
    IL_0022:  ldloc.0
    IL_0023:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::UpgradeOpen()
    IL_0028:  nop
    IL_0029:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTableRecord::.ctor()
    IL_002e:  stloc.3
    IL_002f:  ldloc.3
    IL_0030:  ldarg.2
    IL_0031:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord::set_Name(string)
    IL_0036:  nop
    IL_0037:  ldloc.3
    IL_0038:  ldc.i4     0xc3
    IL_003d:  ldc.i4.2
    IL_003e:  call       class [Acdbmgd]Autodesk.AutoCAD.Colors.Color [Acdbmgd]Autodesk.AutoCAD.Colors.Color::FromColorIndex(valuetype [Acdbmgd]Autodesk.AutoCAD.Colors.ColorMethod,
                                                                                                                             int16)
    IL_0043:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTableRecord::set_Color(class [Acdbmgd]Autodesk.AutoCAD.Colors.Color)
    IL_0048:  nop
    IL_0049:  ldloc.3
    IL_004a:  stloc.1
    IL_004b:  ldloc.0
    IL_004c:  ldloc.1
    IL_004d:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::Add(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord)
    IL_0052:  stloc.2
    IL_0053:  ldarg.0
    IL_0054:  ldloc.1
    IL_0055:  ldc.i4.1
    IL_0056:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::AddNewlyCreatedDBObject(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject,
                                                                                                                       bool)
    IL_005b:  nop
    IL_005c:  ldloc.2
    IL_005d:  stloc.s    V_4
    IL_005f:  br.s       IL_006c

    IL_0061:  ldloc.0
    IL_0062:  ldarg.2
    IL_0063:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::get_Item(string)
    IL_0068:  stloc.s    V_4
    IL_006a:  br.s       IL_006c

    IL_006c:  ldloc.s    V_4
    IL_006e:  ret
  } // end of method Toado100::EnsureLayer

  .method public hidebysig static void  EnsureRegApp(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction tr,
                                                     class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db) cil managed
  {
    // Code size       82 (0x52)
    .maxstack  3
    .locals init (class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.RegAppTable V_0,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.RegAppTableRecord V_1,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.RegAppTableRecord V_2,
             bool V_3)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_RegAppTableId()
    IL_0008:  ldc.i4.0
    IL_0009:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                              valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
    IL_000e:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.RegAppTable
    IL_0013:  stloc.0
    IL_0014:  ldloc.0
    IL_0015:  ldstr      "TVXD_TOADO"
    IL_001a:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::Has(string)
    IL_001f:  stloc.3
    IL_0020:  ldloc.3
    IL_0021:  brtrue.s   IL_0051

    IL_0023:  nop
    IL_0024:  ldloc.0
    IL_0025:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::UpgradeOpen()
    IL_002a:  nop
    IL_002b:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.RegAppTableRecord::.ctor()
    IL_0030:  stloc.2
    IL_0031:  ldloc.2
    IL_0032:  ldstr      "TVXD_TOADO"
    IL_0037:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord::set_Name(string)
    IL_003c:  nop
    IL_003d:  ldloc.2
    IL_003e:  stloc.1
    IL_003f:  ldloc.0
    IL_0040:  ldloc.1
    IL_0041:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::Add(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord)
    IL_0046:  pop
    IL_0047:  ldarg.0
    IL_0048:  ldloc.1
    IL_0049:  ldc.i4.1
    IL_004a:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::AddNewlyCreatedDBObject(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject,
                                                                                                                       bool)
    IL_004f:  nop
    IL_0050:  nop
    IL_0051:  ret
  } // end of method Toado100::EnsureRegApp

  .method public hidebysig static valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId 
          EnsureTextStyle(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction tr,
                          class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db,
                          string styleName) cil managed
  {
    // Code size       153 (0x99)
    .maxstack  3
    .locals init (class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTable V_0,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTableRecord V_1,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_2,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTableRecord V_3,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_4,
             bool V_5)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TextStyleTableId()
    IL_0008:  ldc.i4.0
    IL_0009:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                              valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
    IL_000e:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTable
    IL_0013:  stloc.0
    IL_0014:  ldloc.0
    IL_0015:  ldarg.2
    IL_0016:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::Has(string)
    IL_001b:  ldc.i4.0
    IL_001c:  ceq
    IL_001e:  stloc.s    V_5
    IL_0020:  ldloc.s    V_5
    IL_0022:  brtrue.s   IL_002f

    IL_0024:  ldloc.0
    IL_0025:  ldarg.2
    IL_0026:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::get_Item(string)
    IL_002b:  stloc.s    V_4
    IL_002d:  br.s       IL_0095

    .try
    {
      IL_002f:  nop
      IL_0030:  ldloc.0
      IL_0031:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::UpgradeOpen()
      IL_0036:  nop
      IL_0037:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTableRecord::.ctor()
      IL_003c:  stloc.3
      IL_003d:  ldloc.3
      IL_003e:  ldarg.2
      IL_003f:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord::set_Name(string)
      IL_0044:  nop
      IL_0045:  ldloc.3
      IL_0046:  ldstr      "arial.ttf"
      IL_004b:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTableRecord::set_FileName(string)
      IL_0050:  nop
      IL_0051:  ldloc.3
      IL_0052:  ldc.r8     0.0
      IL_005b:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTableRecord::set_TextSize(float64)
      IL_0060:  nop
      IL_0061:  ldloc.3
      IL_0062:  ldc.r8     1.
      IL_006b:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTableRecord::set_XScale(float64)
      IL_0070:  nop
      IL_0071:  ldloc.3
      IL_0072:  stloc.1
      IL_0073:  ldloc.0
      IL_0074:  ldloc.1
      IL_0075:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::Add(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord)
      IL_007a:  stloc.2
      IL_007b:  ldarg.0
      IL_007c:  ldloc.1
      IL_007d:  ldc.i4.1
      IL_007e:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::AddNewlyCreatedDBObject(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject,
                                                                                                                         bool)
      IL_0083:  nop
      IL_0084:  ldloc.2
      IL_0085:  stloc.s    V_4
      IL_0087:  leave.s    IL_0095

    }  // end .try
    catch [mscorlib]System.Object 
    {
      IL_0089:  pop
      IL_008a:  nop
      IL_008b:  ldarg.1
      IL_008c:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_Textstyle()
      IL_0091:  stloc.s    V_4
      IL_0093:  leave.s    IL_0095

    }  // end handler
    IL_0095:  nop
    IL_0096:  ldloc.s    V_4
    IL_0098:  ret
  } // end of method Toado100::EnsureTextStyle

  .method public hidebysig static class [mscorlib]System.Collections.Generic.List`1<string> 
          StyleNames(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db) cil managed
  {
    // Code size       172 (0xac)
    .maxstack  3
    .locals init (class [mscorlib]System.Collections.Generic.List`1<string> V_0,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction V_1,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTable V_2,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_3,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTableRecord V_4,
             class [mscorlib]System.Collections.Generic.List`1<string> V_5,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableEnumerator V_6,
             bool V_7)
    IL_0000:  nop
    IL_0001:  newobj     instance void class [mscorlib]System.Collections.Generic.List`1<string>::.ctor()
    IL_0006:  stloc.0
    .try
    {
      IL_0007:  nop
      IL_0008:  ldarg.0
      IL_0009:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TransactionManager()
      IL_000e:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartTransaction()
      IL_0013:  stloc.1
      .try
      {
        IL_0014:  nop
        IL_0015:  ldloc.1
        IL_0016:  ldarg.0
        IL_0017:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TextStyleTableId()
        IL_001c:  ldc.i4.0
        IL_001d:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
        IL_0022:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTable
        IL_0027:  stloc.2
        IL_0028:  nop
        IL_0029:  ldloc.2
        IL_002a:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableEnumerator [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::GetEnumerator()
        IL_002f:  stloc.s    V_6
        .try
        {
          IL_0031:  br.s       IL_005a

          IL_0033:  ldloc.s    V_6
          IL_0035:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableEnumerator::get_Current()
          IL_003a:  stloc.3
          IL_003b:  nop
          IL_003c:  ldloc.1
          IL_003d:  ldloc.3
          IL_003e:  ldc.i4.0
          IL_003f:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
          IL_0044:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTableRecord
          IL_0049:  stloc.s    V_4
          IL_004b:  ldloc.0
          IL_004c:  ldloc.s    V_4
          IL_004e:  callvirt   instance string [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord::get_Name()
          IL_0053:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
          IL_0058:  nop
          IL_0059:  nop
          IL_005a:  ldloc.s    V_6
          IL_005c:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableEnumerator::MoveNext()
          IL_0061:  stloc.s    V_7
          IL_0063:  ldloc.s    V_7
          IL_0065:  brtrue.s   IL_0033

          IL_0067:  leave.s    IL_007d

        }  // end .try
        finally
        {
          IL_0069:  ldloc.s    V_6
          IL_006b:  ldnull
          IL_006c:  ceq
          IL_006e:  stloc.s    V_7
          IL_0070:  ldloc.s    V_7
          IL_0072:  brtrue.s   IL_007c

          IL_0074:  ldloc.s    V_6
          IL_0076:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
          IL_007b:  nop
          IL_007c:  endfinally
        }  // end handler
        IL_007d:  nop
        IL_007e:  ldloc.1
        IL_007f:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::Commit()
        IL_0084:  nop
        IL_0085:  nop
        IL_0086:  leave.s    IL_009a

      }  // end .try
      finally
      {
        IL_0088:  ldloc.1
        IL_0089:  ldnull
        IL_008a:  ceq
        IL_008c:  stloc.s    V_7
        IL_008e:  ldloc.s    V_7
        IL_0090:  brtrue.s   IL_0099

        IL_0092:  ldloc.1
        IL_0093:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_0098:  nop
        IL_0099:  endfinally
      }  // end handler
      IL_009a:  nop
      IL_009b:  nop
      IL_009c:  leave.s    IL_00a3

    }  // end .try
    catch [mscorlib]System.Object 
    {
      IL_009e:  pop
      IL_009f:  nop
      IL_00a0:  nop
      IL_00a1:  leave.s    IL_00a3

    }  // end handler
    IL_00a3:  nop
    IL_00a4:  ldloc.0
    IL_00a5:  stloc.s    V_5
    IL_00a7:  br.s       IL_00a9

    IL_00a9:  ldloc.s    V_5
    IL_00ab:  ret
  } // end of method Toado100::StyleNames

  .method public hidebysig static class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer 
          Tag(string handle,
              valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d p) cil managed
  {
    // Code size       180 (0xb4)
    .maxstack  4
    .locals init (class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer V_0,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[] V_1)
    IL_0000:  nop
    IL_0001:  ldc.i4.5
    IL_0002:  newarr     [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_0007:  stloc.1
    IL_0008:  ldloc.1
    IL_0009:  ldc.i4.0
    IL_000a:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_000f:  ldc.i4     0x3e9
    IL_0014:  ldstr      "TVXD_TOADO"
    IL_0019:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::.ctor(int32,
                                                                                                    object)
    IL_001e:  stobj      [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_0023:  ldloc.1
    IL_0024:  ldc.i4.1
    IL_0025:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_002a:  ldc.i4     0x3e8
    IL_002f:  ldarg.0
    IL_0030:  dup
    IL_0031:  brtrue.s   IL_0039

    IL_0033:  pop
    IL_0034:  ldstr      ""
    IL_0039:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::.ctor(int32,
                                                                                                    object)
    IL_003e:  stobj      [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_0043:  ldloc.1
    IL_0044:  ldc.i4.2
    IL_0045:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_004a:  ldc.i4     0x410
    IL_004f:  ldarga.s   p
    IL_0051:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
    IL_0056:  box        [mscorlib]System.Double
    IL_005b:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::.ctor(int32,
                                                                                                    object)
    IL_0060:  stobj      [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_0065:  ldloc.1
    IL_0066:  ldc.i4.3
    IL_0067:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_006c:  ldc.i4     0x410
    IL_0071:  ldarga.s   p
    IL_0073:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
    IL_0078:  box        [mscorlib]System.Double
    IL_007d:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::.ctor(int32,
                                                                                                    object)
    IL_0082:  stobj      [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_0087:  ldloc.1
    IL_0088:  ldc.i4.4
    IL_0089:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_008e:  ldc.i4     0x410
    IL_0093:  ldarga.s   p
    IL_0095:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Z()
    IL_009a:  box        [mscorlib]System.Double
    IL_009f:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::.ctor(int32,
                                                                                                    object)
    IL_00a4:  stobj      [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_00a9:  ldloc.1
    IL_00aa:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer::.ctor(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[])
    IL_00af:  stloc.0
    IL_00b0:  br.s       IL_00b2

    IL_00b2:  ldloc.0
    IL_00b3:  ret
  } // end of method Toado100::Tag

  .method public hidebysig static valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId 
          EnsureDotArrow(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db) cil managed
  {
    // Code size       492 (0x1ec)
    .maxstack  6
    .locals init (class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction V_0,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTable V_1,
             object V_2,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord V_3,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Polyline V_4,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_5,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_6,
             bool V_7)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TransactionManager()
    IL_0007:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartTransaction()
    IL_000c:  stloc.0
    .try
    {
      IL_000d:  nop
      IL_000e:  ldloc.0
      IL_000f:  ldarg.0
      IL_0010:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_BlockTableId()
      IL_0015:  ldc.i4.0
      IL_0016:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
      IL_001b:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTable
      IL_0020:  stloc.1
      IL_0021:  ldloc.1
      IL_0022:  ldstr      "_DOT"
      IL_0027:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::Has(string)
      IL_002c:  ldc.i4.0
      IL_002d:  ceq
      IL_002f:  stloc.s    V_7
      IL_0031:  ldloc.s    V_7
      IL_0033:  brtrue.s   IL_0048

      IL_0035:  nop
      IL_0036:  ldloc.1
      IL_0037:  ldstr      "_DOT"
      IL_003c:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::get_Item(string)
      IL_0041:  stloc.s    V_6
      IL_0043:  leave      IL_01e8

      IL_0048:  ldloc.0
      IL_0049:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::Commit()
      IL_004e:  nop
      IL_004f:  nop
      IL_0050:  leave.s    IL_0064

    }  // end .try
    finally
    {
      IL_0052:  ldloc.0
      IL_0053:  ldnull
      IL_0054:  ceq
      IL_0056:  stloc.s    V_7
      IL_0058:  ldloc.s    V_7
      IL_005a:  brtrue.s   IL_0063

      IL_005c:  ldloc.0
      IL_005d:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_0062:  nop
      IL_0063:  endfinally
    }  // end handler
    IL_0064:  nop
    .try
    {
      IL_0065:  nop
      IL_0066:  ldstr      "DIMBLK"
      IL_006b:  call       object [accoremgd]Autodesk.AutoCAD.ApplicationServices.Core.Application::GetSystemVariable(string)
      IL_0070:  stloc.2
      IL_0071:  ldstr      "DIMBLK"
      IL_0076:  ldstr      "_DOT"
      IL_007b:  call       void [accoremgd]Autodesk.AutoCAD.ApplicationServices.Core.Application::SetSystemVariable(string,
                                                                                                                    object)
      IL_0080:  nop
      IL_0081:  ldloc.2
      IL_0082:  ldnull
      IL_0083:  ceq
      IL_0085:  stloc.s    V_7
      IL_0087:  ldloc.s    V_7
      IL_0089:  brtrue.s   IL_0097

      IL_008b:  ldstr      "DIMBLK"
      IL_0090:  ldloc.2
      IL_0091:  call       void [accoremgd]Autodesk.AutoCAD.ApplicationServices.Core.Application::SetSystemVariable(string,
                                                                                                                    object)
      IL_0096:  nop
      IL_0097:  nop
      IL_0098:  leave.s    IL_009f

    }  // end .try
    catch [mscorlib]System.Object 
    {
      IL_009a:  pop
      IL_009b:  nop
      IL_009c:  nop
      IL_009d:  leave.s    IL_009f

    }  // end handler
    IL_009f:  nop
    IL_00a0:  ldarg.0
    IL_00a1:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TransactionManager()
    IL_00a6:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartTransaction()
    IL_00ab:  stloc.0
    .try
    {
      IL_00ac:  nop
      IL_00ad:  ldloc.0
      IL_00ae:  ldarg.0
      IL_00af:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_BlockTableId()
      IL_00b4:  ldc.i4.0
      IL_00b5:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
      IL_00ba:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTable
      IL_00bf:  stloc.1
      IL_00c0:  ldloc.1
      IL_00c1:  ldstr      "_DOT"
      IL_00c6:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::Has(string)
      IL_00cb:  ldc.i4.0
      IL_00cc:  ceq
      IL_00ce:  stloc.s    V_7
      IL_00d0:  ldloc.s    V_7
      IL_00d2:  brtrue.s   IL_00e6

      IL_00d4:  ldloc.1
      IL_00d5:  ldstr      "_DOT"
      IL_00da:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::get_Item(string)
      IL_00df:  stloc.s    V_6
      IL_00e1:  leave      IL_01e8

      IL_00e6:  ldloc.1
      IL_00e7:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::UpgradeOpen()
      IL_00ec:  nop
      IL_00ed:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord::.ctor()
      IL_00f2:  stloc.3
      IL_00f3:  ldloc.3
      IL_00f4:  ldstr      "_DOT"
      IL_00f9:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord::set_Name(string)
      IL_00fe:  nop
      IL_00ff:  ldloc.3
      IL_0100:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Origin()
      IL_0105:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord::set_Origin(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
      IL_010a:  nop
      IL_010b:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Polyline::.ctor()
      IL_0110:  stloc.s    V_4
      IL_0112:  ldloc.s    V_4
      IL_0114:  ldc.i4.0
      IL_0115:  ldc.r8     -0.25
      IL_011e:  ldc.r8     0.0
      IL_0127:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.Geometry.Point2d::.ctor(float64,
                                                                                           float64)
      IL_012c:  ldc.r8     1.
      IL_0135:  ldc.r8     0.5
      IL_013e:  ldc.r8     0.5
      IL_0147:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Polyline::AddVertexAt(int32,
                                                                                                          valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point2d,
                                                                                                          float64,
                                                                                                          float64,
                                                                                                          float64)
      IL_014c:  nop
      IL_014d:  ldloc.s    V_4
      IL_014f:  ldc.i4.1
      IL_0150:  ldc.r8     0.25
      IL_0159:  ldc.r8     0.0
      IL_0162:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.Geometry.Point2d::.ctor(float64,
                                                                                           float64)
      IL_0167:  ldc.r8     1.
      IL_0170:  ldc.r8     0.5
      IL_0179:  ldc.r8     0.5
      IL_0182:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Polyline::AddVertexAt(int32,
                                                                                                          valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point2d,
                                                                                                          float64,
                                                                                                          float64,
                                                                                                          float64)
      IL_0187:  nop
      IL_0188:  ldloc.s    V_4
      IL_018a:  ldc.i4.1
      IL_018b:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Polyline::set_Closed(bool)
      IL_0190:  nop
      IL_0191:  ldloc.s    V_4
      IL_0193:  ldc.i4     0xc3
      IL_0198:  ldc.i4.0
      IL_0199:  call       class [Acdbmgd]Autodesk.AutoCAD.Colors.Color [Acdbmgd]Autodesk.AutoCAD.Colors.Color::FromColorIndex(valuetype [Acdbmgd]Autodesk.AutoCAD.Colors.ColorMethod,
                                                                                                                               int16)
      IL_019e:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Entity::set_Color(class [Acdbmgd]Autodesk.AutoCAD.Colors.Color)
      IL_01a3:  nop
      IL_01a4:  ldloc.3
      IL_01a5:  ldloc.s    V_4
      IL_01a7:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord::AppendEntity(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Entity)
      IL_01ac:  pop
      IL_01ad:  ldloc.1
      IL_01ae:  ldloc.3
      IL_01af:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::Add(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord)
      IL_01b4:  stloc.s    V_5
      IL_01b6:  ldloc.0
      IL_01b7:  ldloc.3
      IL_01b8:  ldc.i4.1
      IL_01b9:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::AddNewlyCreatedDBObject(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject,
                                                                                                                         bool)
      IL_01be:  nop
      IL_01bf:  ldloc.0
      IL_01c0:  ldloc.s    V_4
      IL_01c2:  ldc.i4.1
      IL_01c3:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::AddNewlyCreatedDBObject(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject,
                                                                                                                         bool)
      IL_01c8:  nop
      IL_01c9:  ldloc.0
      IL_01ca:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::Commit()
      IL_01cf:  nop
      IL_01d0:  ldloc.s    V_5
      IL_01d2:  stloc.s    V_6
      IL_01d4:  leave.s    IL_01e8

    }  // end .try
    finally
    {
      IL_01d6:  ldloc.0
      IL_01d7:  ldnull
      IL_01d8:  ceq
      IL_01da:  stloc.s    V_7
      IL_01dc:  ldloc.s    V_7
      IL_01de:  brtrue.s   IL_01e7

      IL_01e0:  ldloc.0
      IL_01e1:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_01e6:  nop
      IL_01e7:  endfinally
    }  // end handler
    IL_01e8:  nop
    IL_01e9:  ldloc.s    V_6
    IL_01eb:  ret
  } // end of method Toado100::EnsureDotArrow

  .method public hidebysig static valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId 
          CreateOrUpdateLeader(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db,
                               class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction tr,
                               class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord ms,
                               valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d targetPt,
                               valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d landingPt,
                               class Tvxd.Meps.ToadoSettings s,
                               string blockName,
                               string blockHandle,
                               valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId existingLeaderId,
                               valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId layerId,
                               valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId styleId,
                               valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId dotArrowId) cil managed
  {
    // Code size       671 (0x29f)
    .maxstack  5
    .locals init (class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader V_0,
             bool V_1,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText V_2,
             int32 V_3,
             int32 V_4,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_5,
             bool V_6)
    IL_0000:  nop
    IL_0001:  ldarg.s    existingLeaderId
    IL_0003:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_Null()
    IL_0008:  call       bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::op_Equality(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                               valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
    IL_000d:  stloc.1
    IL_000e:  ldloc.1
    IL_000f:  ldc.i4.0
    IL_0010:  ceq
    IL_0012:  stloc.s    V_6
    IL_0014:  ldloc.s    V_6
    IL_0016:  brtrue.s   IL_0032

    IL_0018:  nop
    IL_0019:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::.ctor()
    IL_001e:  stloc.0
    IL_001f:  ldloc.0
    IL_0020:  ldarg.0
    IL_0021:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Entity::SetDatabaseDefaults(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
    IL_0026:  nop
    IL_0027:  ldloc.0
    IL_0028:  ldc.i4.2
    IL_0029:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_ContentType(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ContentType)
    IL_002e:  nop
    IL_002f:  nop
    IL_0030:  br.s       IL_0043

    IL_0032:  nop
    IL_0033:  ldarg.1
    IL_0034:  ldarg.s    existingLeaderId
    IL_0036:  ldc.i4.1
    IL_0037:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                              valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
    IL_003c:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader
    IL_0041:  stloc.0
    IL_0042:  nop
    IL_0043:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::.ctor()
    IL_0048:  stloc.2
    IL_0049:  ldloc.2
    IL_004a:  ldarg.0
    IL_004b:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Entity::SetDatabaseDefaults(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
    IL_0050:  nop
    IL_0051:  ldarg.s    styleId
    IL_0053:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_Null()
    IL_0058:  call       bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::op_Inequality(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                 valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
    IL_005d:  ldc.i4.0
    IL_005e:  ceq
    IL_0060:  stloc.s    V_6
    IL_0062:  ldloc.s    V_6
    IL_0064:  brtrue.s   IL_006f

    IL_0066:  ldloc.2
    IL_0067:  ldarg.s    styleId
    IL_0069:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_TextStyleId(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
    IL_006e:  nop
    IL_006f:  ldloc.2
    IL_0070:  ldarg.s    s
    IL_0072:  ldfld      float64 Tvxd.Meps.ToadoSettings::Height
    IL_0077:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_TextHeight(float64)
    IL_007c:  nop
    IL_007d:  ldloc.2
    IL_007e:  ldarg.3
    IL_007f:  ldarg.s    s
    IL_0081:  ldarg.s    blockName
    IL_0083:  ldc.i4.0
    IL_0084:  call       string Tvxd.Meps.Toado100::Format(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,
                                                           class Tvxd.Meps.ToadoSettings,
                                                           string,
                                                           bool)
    IL_0089:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_Contents(string)
    IL_008e:  nop
    IL_008f:  ldloc.2
    IL_0090:  ldc.i4.5
    IL_0091:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_Attachment(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.AttachmentPoint)
    IL_0096:  nop
    IL_0097:  ldarg.s    s
    IL_0099:  ldfld      bool Tvxd.Meps.ToadoSettings::Mask
    IL_009e:  ldc.i4.0
    IL_009f:  ceq
    IL_00a1:  stloc.s    V_6
    IL_00a3:  ldloc.s    V_6
    IL_00a5:  brtrue.s   IL_00c9

    IL_00a7:  nop
    IL_00a8:  ldloc.2
    IL_00a9:  ldc.i4.1
    IL_00aa:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_BackgroundFill(bool)
    IL_00af:  nop
    IL_00b0:  ldloc.2
    IL_00b1:  ldc.i4.1
    IL_00b2:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_UseBackgroundColor(bool)
    IL_00b7:  nop
    IL_00b8:  ldloc.2
    IL_00b9:  ldc.r8     1.1499999999999999
    IL_00c2:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_BackgroundScaleFactor(float64)
    IL_00c7:  nop
    IL_00c8:  nop
    IL_00c9:  ldloc.0
    IL_00ca:  ldloc.2
    IL_00cb:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_MText(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText)
    IL_00d0:  nop
    IL_00d1:  ldarg.s    styleId
    IL_00d3:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_Null()
    IL_00d8:  call       bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::op_Inequality(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                 valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
    IL_00dd:  ldc.i4.0
    IL_00de:  ceq
    IL_00e0:  stloc.s    V_6
    IL_00e2:  ldloc.s    V_6
    IL_00e4:  brtrue.s   IL_00ef

    IL_00e6:  ldloc.0
    IL_00e7:  ldarg.s    styleId
    IL_00e9:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_TextStyleId(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
    IL_00ee:  nop
    IL_00ef:  ldloc.0
    IL_00f0:  ldarg.s    s
    IL_00f2:  ldfld      float64 Tvxd.Meps.ToadoSettings::Height
    IL_00f7:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_TextHeight(float64)
    IL_00fc:  nop
    IL_00fd:  ldloc.0
    IL_00fe:  ldc.i4.0
    IL_00ff:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_TextAngleType(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextAngleType)
    IL_0104:  nop
    IL_0105:  ldloc.0
    IL_0106:  ldc.i4.1
    IL_0107:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_TextAlignmentType(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextAlignmentType)
    IL_010c:  nop
    IL_010d:  ldarg.s    layerId
    IL_010f:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_Null()
    IL_0114:  call       bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::op_Inequality(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                 valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
    IL_0119:  ldc.i4.0
    IL_011a:  ceq
    IL_011c:  stloc.s    V_6
    IL_011e:  ldloc.s    V_6
    IL_0120:  brtrue.s   IL_012b

    IL_0122:  ldloc.0
    IL_0123:  ldarg.s    layerId
    IL_0125:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Entity::set_LayerId(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
    IL_012a:  nop
    IL_012b:  ldarg.s    s
    IL_012d:  ldfld      bool Tvxd.Meps.ToadoSettings::UnderlineBetween
    IL_0132:  ldc.i4.0
    IL_0133:  ceq
    IL_0135:  stloc.s    V_6
    IL_0137:  ldloc.s    V_6
    IL_0139:  brtrue.s   IL_0189

    IL_013b:  nop
    IL_013c:  ldloc.0
    IL_013d:  ldc.i4.0
    IL_013e:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_EnableDogleg(bool)
    IL_0143:  nop
    IL_0144:  ldloc.0
    IL_0145:  ldc.i4.1
    IL_0146:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_EnableLanding(bool)
    IL_014b:  nop
    IL_014c:  ldloc.0
    IL_014d:  ldarg.s    s
    IL_014f:  ldfld      float64 Tvxd.Meps.ToadoSettings::Height
    IL_0154:  ldc.r8     0.29999999999999999
    IL_015d:  mul
    IL_015e:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_LandingGap(float64)
    IL_0163:  nop
    IL_0164:  ldloc.0
    IL_0165:  ldc.i4.6
    IL_0166:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_TextAttachmentType(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextAttachmentType)
    IL_016b:  nop
    IL_016c:  ldloc.0
    IL_016d:  ldc.i4.6
    IL_016e:  ldc.i4.1
    IL_016f:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::SetTextAttachmentType(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextAttachmentType,
                                                                                                                 valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LeaderDirectionType)
    IL_0174:  nop
    IL_0175:  ldloc.0
    IL_0176:  ldc.i4.6
    IL_0177:  ldc.i4.2
    IL_0178:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::SetTextAttachmentType(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextAttachmentType,
                                                                                                                 valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LeaderDirectionType)
    IL_017d:  nop
    IL_017e:  ldloc.0
    IL_017f:  ldc.i4.1
    IL_0180:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_ExtendLeaderToText(bool)
    IL_0185:  nop
    IL_0186:  nop
    IL_0187:  br.s       IL_01d3

    IL_0189:  nop
    IL_018a:  ldloc.0
    IL_018b:  ldc.i4.1
    IL_018c:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_EnableDogleg(bool)
    IL_0191:  nop
    IL_0192:  ldloc.0
    IL_0193:  ldarg.s    s
    IL_0195:  ldfld      float64 Tvxd.Meps.ToadoSettings::Height
    IL_019a:  ldc.r8     1.2
    IL_01a3:  mul
    IL_01a4:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_DoglegLength(float64)
    IL_01a9:  nop
    IL_01aa:  ldloc.0
    IL_01ab:  ldc.i4.1
    IL_01ac:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_EnableLanding(bool)
    IL_01b1:  nop
    IL_01b2:  ldloc.0
    IL_01b3:  ldarg.s    s
    IL_01b5:  ldfld      float64 Tvxd.Meps.ToadoSettings::Height
    IL_01ba:  ldc.r8     0.29999999999999999
    IL_01c3:  mul
    IL_01c4:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_LandingGap(float64)
    IL_01c9:  nop
    IL_01ca:  ldloc.0
    IL_01cb:  ldc.i4.0
    IL_01cc:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_ExtendLeaderToText(bool)
    IL_01d1:  nop
    IL_01d2:  nop
    IL_01d3:  ldarg.s    s
    IL_01d5:  ldfld      bool Tvxd.Meps.ToadoSettings::Arrow
    IL_01da:  brfalse.s  IL_01ed

    IL_01dc:  ldarg.s    dotArrowId
    IL_01de:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_Null()
    IL_01e3:  call       bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::op_Inequality(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                 valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
    IL_01e8:  ldc.i4.0
    IL_01e9:  ceq
    IL_01eb:  br.s       IL_01ee

    IL_01ed:  ldc.i4.1
    IL_01ee:  nop
    IL_01ef:  stloc.s    V_6
    IL_01f1:  ldloc.s    V_6
    IL_01f3:  brtrue.s   IL_0200

    IL_01f5:  nop
    IL_01f6:  ldloc.0
    IL_01f7:  ldarg.s    dotArrowId
    IL_01f9:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_ArrowSymbolId(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
    IL_01fe:  nop
    IL_01ff:  nop
    IL_0200:  ldloc.0
    IL_0201:  ldarg.s    s
    IL_0203:  ldfld      float64 Tvxd.Meps.ToadoSettings::ArrowSize
    IL_0208:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_ArrowSize(float64)
    IL_020d:  nop
    IL_020e:  ldloc.1
    IL_020f:  ldc.i4.0
    IL_0210:  ceq
    IL_0212:  stloc.s    V_6
    IL_0214:  ldloc.s    V_6
    IL_0216:  brtrue.s   IL_025b

    IL_0218:  nop
    IL_0219:  ldloc.0
    IL_021a:  callvirt   instance int32 [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::AddLeader()
    IL_021f:  stloc.3
    IL_0220:  ldloc.0
    IL_0221:  ldloc.3
    IL_0222:  callvirt   instance int32 [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::AddLeaderLine(int32)
    IL_0227:  stloc.s    V_4
    IL_0229:  ldloc.0
    IL_022a:  ldloc.s    V_4
    IL_022c:  ldarg.3
    IL_022d:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::AddFirstVertex(int32,
                                                                                                          valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
    IL_0232:  nop
    IL_0233:  ldloc.0
    IL_0234:  ldloc.s    V_4
    IL_0236:  ldarg.s    landingPt
    IL_0238:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::AddLastVertex(int32,
                                                                                                         valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
    IL_023d:  nop
    IL_023e:  ldloc.0
    IL_023f:  ldarg.s    landingPt
    IL_0241:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_TextLocation(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
    IL_0246:  nop
    IL_0247:  ldarg.2
    IL_0248:  ldloc.0
    IL_0249:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord::AppendEntity(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Entity)
    IL_024e:  pop
    IL_024f:  ldarg.1
    IL_0250:  ldloc.0
    IL_0251:  ldc.i4.1
    IL_0252:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::AddNewlyCreatedDBObject(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject,
                                                                                                                       bool)
    IL_0257:  nop
    IL_0258:  nop
    IL_0259:  br.s       IL_0283

    IL_025b:  nop
    IL_025c:  ldloc.0
    IL_025d:  ldarg.s    landingPt
    IL_025f:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_TextLocation(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
    IL_0264:  nop
    .try
    {
      IL_0265:  nop
      IL_0266:  ldloc.0
      IL_0267:  ldc.i4.0
      IL_0268:  ldarg.3
      IL_0269:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::SetFirstVertex(int32,
                                                                                                            valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
      IL_026e:  nop
      IL_026f:  ldloc.0
      IL_0270:  ldc.i4.0
      IL_0271:  ldarg.s    landingPt
      IL_0273:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::SetLastVertex(int32,
                                                                                                           valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
      IL_0278:  nop
      IL_0279:  nop
      IL_027a:  leave.s    IL_0281

    }  // end .try
    catch [mscorlib]System.Object 
    {
      IL_027c:  pop
      IL_027d:  nop
      IL_027e:  nop
      IL_027f:  leave.s    IL_0281

    }  // end handler
    IL_0281:  nop
    IL_0282:  nop
    IL_0283:  ldloc.0
    IL_0284:  ldarg.s    blockHandle
    IL_0286:  ldarg.3
    IL_0287:  call       class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer Tvxd.Meps.Toado100::Tag(string,
                                                                                                               valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
    IL_028c:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::set_XData(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer)
    IL_0291:  nop
    IL_0292:  ldloc.0
    IL_0293:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::get_ObjectId()
    IL_0298:  stloc.s    V_5
    IL_029a:  br.s       IL_029c

    IL_029c:  ldloc.s    V_5
    IL_029e:  ret
  } // end of method Toado100::CreateOrUpdateLeader

  .method public hidebysig static class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> 
          ScanExistingLeaders(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction tr,
                              class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord ms) cil managed
  {
    // Code size       234 (0xea)
    .maxstack  3
    .locals init (class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> V_0,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_1,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject V_2,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer V_3,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[] V_4,
             string V_5,
             class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> V_6,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecordEnumerator V_7,
             bool V_8)
    IL_0000:  nop
    IL_0001:  call       class [mscorlib]System.StringComparer [mscorlib]System.StringComparer::get_OrdinalIgnoreCase()
    IL_0006:  newobj     instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::.ctor(class [mscorlib]System.Collections.Generic.IEqualityComparer`1<!0>)
    IL_000b:  stloc.0
    IL_000c:  nop
    IL_000d:  ldarg.1
    IL_000e:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecordEnumerator [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord::GetEnumerator()
    IL_0013:  stloc.s    V_7
    .try
    {
      IL_0015:  br         IL_00bb

      IL_001a:  ldloc.s    V_7
      IL_001c:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecordEnumerator::get_Current()
      IL_0021:  stloc.1
      IL_0022:  nop
      IL_0023:  ldloca.s   V_1
      IL_0025:  call       instance class [Acdbmgd]Autodesk.AutoCAD.Runtime.RXClass [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_ObjectClass()
      IL_002a:  callvirt   instance string [Acdbmgd]Autodesk.AutoCAD.Runtime.RXClass::get_DxfName()
      IL_002f:  ldstr      "MULTILEADER"
      IL_0034:  ldc.i4.5
      IL_0035:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_003a:  ldc.i4.0
      IL_003b:  ceq
      IL_003d:  stloc.s    V_8
      IL_003f:  ldloc.s    V_8
      IL_0041:  brtrue.s   IL_00ba

      IL_0043:  nop
      IL_0044:  ldarg.0
      IL_0045:  ldloc.1
      IL_0046:  ldc.i4.0
      IL_0047:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
      IL_004c:  stloc.2
      IL_004d:  ldloc.2
      IL_004e:  ldstr      "TVXD_TOADO"
      IL_0053:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::GetXDataForApplication(string)
      IL_0058:  stloc.3
      IL_0059:  ldloc.3
      IL_005a:  ldnull
      IL_005b:  call       bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer::op_Inequality(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer,
                                                                                                       class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer)
      IL_0060:  ldc.i4.0
      IL_0061:  ceq
      IL_0063:  stloc.s    V_8
      IL_0065:  ldloc.s    V_8
      IL_0067:  brtrue.s   IL_00b9

      IL_0069:  nop
      IL_006a:  ldloc.3
      IL_006b:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[] [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer::AsArray()
      IL_0070:  stloc.s    V_4
      IL_0072:  ldloc.s    V_4
      IL_0074:  ldlen
      IL_0075:  conv.i4
      IL_0076:  ldc.i4.1
      IL_0077:  cgt
      IL_0079:  ldc.i4.0
      IL_007a:  ceq
      IL_007c:  stloc.s    V_8
      IL_007e:  ldloc.s    V_8
      IL_0080:  brtrue.s   IL_00b1

      IL_0082:  nop
      IL_0083:  ldloc.s    V_4
      IL_0085:  ldc.i4.1
      IL_0086:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
      IL_008b:  call       instance object [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::get_Value()
      IL_0090:  call       string [mscorlib]System.Convert::ToString(object)
      IL_0095:  stloc.s    V_5
      IL_0097:  ldloc.s    V_5
      IL_0099:  call       bool [mscorlib]System.String::IsNullOrEmpty(string)
      IL_009e:  stloc.s    V_8
      IL_00a0:  ldloc.s    V_8
      IL_00a2:  brtrue.s   IL_00b0

      IL_00a4:  nop
      IL_00a5:  ldloc.0
      IL_00a6:  ldloc.s    V_5
      IL_00a8:  ldloc.1
      IL_00a9:  callvirt   instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::set_Item(!0,
                                                                                                                                                                                 !1)
      IL_00ae:  nop
      IL_00af:  nop
      IL_00b0:  nop
      IL_00b1:  ldloc.3
      IL_00b2:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper::Dispose()
      IL_00b7:  nop
      IL_00b8:  nop
      IL_00b9:  nop
      IL_00ba:  nop
      IL_00bb:  ldloc.s    V_7
      IL_00bd:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecordEnumerator::MoveNext()
      IL_00c2:  stloc.s    V_8
      IL_00c4:  ldloc.s    V_8
      IL_00c6:  brtrue     IL_001a

      IL_00cb:  leave.s    IL_00e1

    }  // end .try
    finally
    {
      IL_00cd:  ldloc.s    V_7
      IL_00cf:  ldnull
      IL_00d0:  ceq
      IL_00d2:  stloc.s    V_8
      IL_00d4:  ldloc.s    V_8
      IL_00d6:  brtrue.s   IL_00e0

      IL_00d8:  ldloc.s    V_7
      IL_00da:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_00df:  nop
      IL_00e0:  endfinally
    }  // end handler
    IL_00e1:  nop
    IL_00e2:  ldloc.0
    IL_00e3:  stloc.s    V_6
    IL_00e5:  br.s       IL_00e7

    IL_00e7:  ldloc.s    V_6
    IL_00e9:  ret
  } // end of method Toado100::ScanExistingLeaders

  .method public hidebysig static void  Run() cil managed
  {
    .custom instance void [accoremgd]Autodesk.AutoCAD.Runtime.CommandMethodAttribute::.ctor(string) = ( 01 00 06 54 54 4F 41 44 4F 00 00 )                // ...TTOADO..
    .custom instance void [accoremgd]Autodesk.AutoCAD.Runtime.CommandMethodAttribute::.ctor(string) = ( 01 00 07 54 2D 54 4F 41 44 4F 00 00 )             // ...T-TOADO..
    // Code size       1673 (0x689)
    .maxstack  12
    .locals init (class [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document V_0,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database V_1,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.Editor V_2,
             class Tvxd.Meps.ToadoSettings V_3,
             class Tvxd.Meps.Toado100Form V_4,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_5,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionOptions V_6,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.SelectionFilter V_7,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionResult V_8,
             class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> V_9,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction V_10,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_11,
             class [accoremgd]Autodesk.AutoCAD.ApplicationServices.DocumentLock V_12,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_13,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_14,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTable V_15,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord V_16,
             class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> V_17,
             int32 V_18,
             int32 V_19,
             int32 V_20,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference V_21,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_22,
             string V_23,
             string V_24,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_25,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions V_26,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointResult V_27,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_28,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions V_29,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointResult V_30,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions V_31,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointResult V_32,
             bool V_33,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[] V_34,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId[] V_35,
             int32 V_36,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Handle V_37)
    IL_0000:  nop
    IL_0001:  call       class [accoremgd]Autodesk.AutoCAD.ApplicationServices.DocumentCollection [accoremgd]Autodesk.AutoCAD.ApplicationServices.Core.Application::get_DocumentManager()
    IL_0006:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document [accoremgd]Autodesk.AutoCAD.ApplicationServices.DocumentCollection::get_MdiActiveDocument()
    IL_000b:  stloc.0
    IL_000c:  ldloc.0
    IL_000d:  ldnull
    IL_000e:  call       bool [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper::op_Equality(class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper,
                                                                                               class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper)
    IL_0013:  ldc.i4.0
    IL_0014:  ceq
    IL_0016:  stloc.s    V_33
    IL_0018:  ldloc.s    V_33
    IL_001a:  brtrue.s   IL_0021

    IL_001c:  br         IL_0687

    IL_0021:  ldloc.0
    IL_0022:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document::get_Database()
    IL_0027:  stloc.1
    IL_0028:  ldloc.0
    IL_0029:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.EditorInput.Editor [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document::get_Editor()
    IL_002e:  stloc.2
    IL_002f:  call       class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100::Load()
    IL_0034:  stloc.3
    IL_0035:  ldloc.3
    IL_0036:  newobj     instance void Tvxd.Meps.Toado100Form::.ctor(class Tvxd.Meps.ToadoSettings)
    IL_003b:  stloc.s    V_4
    .try
    {
      IL_003d:  nop
      IL_003e:  ldloc.s    V_4
      IL_0040:  call       valuetype [System.Windows.Forms]System.Windows.Forms.DialogResult [Acmgd]Autodesk.AutoCAD.ApplicationServices.Application::ShowModalDialog(class [System.Windows.Forms]System.Windows.Forms.Form)
      IL_0045:  ldc.i4.1
      IL_0046:  ceq
      IL_0048:  stloc.s    V_33
      IL_004a:  ldloc.s    V_33
      IL_004c:  brtrue.s   IL_0053

      IL_004e:  leave      IL_0687

      IL_0053:  ldloc.s    V_4
      IL_0055:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
      IL_005a:  stloc.3
      IL_005b:  ldloc.3
      IL_005c:  call       void Tvxd.Meps.Toado100::Save(class Tvxd.Meps.ToadoSettings)
      IL_0061:  nop
      IL_0062:  ldloc.s    V_4
      IL_0064:  ldfld      bool Tvxd.Meps.Toado100Form::TriggerRefreshOnly
      IL_0069:  ldc.i4.0
      IL_006a:  ceq
      IL_006c:  stloc.s    V_33
      IL_006e:  ldloc.s    V_33
      IL_0070:  brtrue.s   IL_0080

      IL_0072:  nop
      IL_0073:  ldloc.0
      IL_0074:  ldloc.3
      IL_0075:  call       void Tvxd.Meps.Toado100::RefreshAll(class [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document,
                                                               class Tvxd.Meps.ToadoSettings)
      IL_007a:  nop
      IL_007b:  leave      IL_0687

      IL_0080:  nop
      IL_0081:  leave.s    IL_0097

    }  // end .try
    finally
    {
      IL_0083:  ldloc.s    V_4
      IL_0085:  ldnull
      IL_0086:  ceq
      IL_0088:  stloc.s    V_33
      IL_008a:  ldloc.s    V_33
      IL_008c:  brtrue.s   IL_0096

      IL_008e:  ldloc.s    V_4
      IL_0090:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_0095:  nop
      IL_0096:  endfinally
    }  // end handler
    IL_0097:  nop
    IL_0098:  ldloc.1
    IL_0099:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::EnsureDotArrow(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
    IL_009e:  stloc.s    V_5
    IL_00a0:  newobj     instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionOptions::.ctor()
    IL_00a5:  stloc.s    V_6
    IL_00a7:  ldloc.s    V_6
    IL_00a9:  ldstr      bytearray (0A 00 43 00 68 00 CD 1E 6E 00 20 00 63 00 E1 00   // ..C.h...n. .c...
                                    63 00 20 00 42 00 6C 00 6F 00 63 00 6B 00 20 00   // c. .B.l.o.c.k. .
                                    63 00 A7 1E 6E 00 20 00 67 00 AF 1E 6E 00 20 00   // c...n. .g...n. .
                                    6E 00 68 00 E3 00 6E 00 20 00 28 00 68 00 6F 00   // n.h...n. .(.h.o.
                                    B7 1E 63 00 20 00 45 00 6E 00 74 00 65 00 72 00   // ..c. .E.n.t.e.r.
                                    20 00 11 01 C3 1E 20 00 70 00 69 00 63 00 6B 00   //  ..... .p.i.c.k.
                                    20 00 11 01 69 00 C3 1E 6D 00 20 00 74 00 F1 1E   //  ...i...m. .t...
                                    20 00 64 00 6F 00 20 00 74 00 72 00 EA 00 6E 00   //  .d.o. .t.r...n.
                                    20 00 62 00 A3 1E 6E 00 20 00 76 00 BD 1E 29 00   //  .b...n. .v...).
                                    3A 00 20 00 )                                     // :. .
    IL_00ae:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionOptions::set_MessageForAdding(string)
    IL_00b3:  nop
    IL_00b4:  ldc.i4.1
    IL_00b5:  newarr     [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_00ba:  stloc.s    V_34
    IL_00bc:  ldloc.s    V_34
    IL_00be:  ldc.i4.0
    IL_00bf:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_00c4:  ldc.i4.0
    IL_00c5:  ldstr      "INSERT"
    IL_00ca:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::.ctor(int32,
                                                                                                    object)
    IL_00cf:  stobj      [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_00d4:  ldloc.s    V_34
    IL_00d6:  newobj     instance void [accoremgd]Autodesk.AutoCAD.EditorInput.SelectionFilter::.ctor(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[])
    IL_00db:  stloc.s    V_7
    IL_00dd:  ldloc.2
    IL_00de:  ldloc.s    V_6
    IL_00e0:  ldloc.s    V_7
    IL_00e2:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionResult [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::GetSelection(class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionOptions,
                                                                                                                                                                   class [accoremgd]Autodesk.AutoCAD.EditorInput.SelectionFilter)
    IL_00e7:  stloc.s    V_8
    IL_00e9:  ldloc.s    V_8
    IL_00eb:  callvirt   instance valuetype [accoremgd]Autodesk.AutoCAD.EditorInput.PromptStatus [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionResult::get_Status()
    IL_00f0:  ldc.i4     0x13ec
    IL_00f5:  ceq
    IL_00f7:  ldc.i4.0
    IL_00f8:  ceq
    IL_00fa:  stloc.s    V_33
    IL_00fc:  ldloc.s    V_33
    IL_00fe:  brtrue     IL_04c3

    IL_0103:  nop
    IL_0104:  newobj     instance void class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::.ctor()
    IL_0109:  stloc.s    V_9
    IL_010b:  ldloc.1
    IL_010c:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TransactionManager()
    IL_0111:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartTransaction()
    IL_0116:  stloc.s    V_10
    .try
    {
      IL_0118:  nop
      IL_0119:  nop
      IL_011a:  ldloc.s    V_8
      IL_011c:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.EditorInput.SelectionSet [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionResult::get_Value()
      IL_0121:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId[] [accoremgd]Autodesk.AutoCAD.EditorInput.SelectionSet::GetObjectIds()
      IL_0126:  stloc.s    V_35
      IL_0128:  ldc.i4.0
      IL_0129:  stloc.s    V_36
      IL_012b:  br.s       IL_0171

      IL_012d:  ldloc.s    V_35
      IL_012f:  ldloc.s    V_36
      IL_0131:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId
      IL_0136:  ldobj      [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId
      IL_013b:  stloc.s    V_11
      IL_013d:  nop
      IL_013e:  ldloca.s   V_11
      IL_0140:  call       instance class [Acdbmgd]Autodesk.AutoCAD.Runtime.RXClass [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_ObjectClass()
      IL_0145:  callvirt   instance string [Acdbmgd]Autodesk.AutoCAD.Runtime.RXClass::get_DxfName()
      IL_014a:  ldstr      "INSERT"
      IL_014f:  ldc.i4.5
      IL_0150:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_0155:  ldc.i4.0
      IL_0156:  ceq
      IL_0158:  stloc.s    V_33
      IL_015a:  ldloc.s    V_33
      IL_015c:  brtrue.s   IL_016a

      IL_015e:  nop
      IL_015f:  ldloc.s    V_9
      IL_0161:  ldloc.s    V_11
      IL_0163:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::Add(!0)
      IL_0168:  nop
      IL_0169:  nop
      IL_016a:  nop
      IL_016b:  ldloc.s    V_36
      IL_016d:  ldc.i4.1
      IL_016e:  add
      IL_016f:  stloc.s    V_36
      IL_0171:  ldloc.s    V_36
      IL_0173:  ldloc.s    V_35
      IL_0175:  ldlen
      IL_0176:  conv.i4
      IL_0177:  clt
      IL_0179:  stloc.s    V_33
      IL_017b:  ldloc.s    V_33
      IL_017d:  brtrue.s   IL_012d

      IL_017f:  ldloc.s    V_10
      IL_0181:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::Commit()
      IL_0186:  nop
      IL_0187:  nop
      IL_0188:  leave.s    IL_019e

    }  // end .try
    finally
    {
      IL_018a:  ldloc.s    V_10
      IL_018c:  ldnull
      IL_018d:  ceq
      IL_018f:  stloc.s    V_33
      IL_0191:  ldloc.s    V_33
      IL_0193:  brtrue.s   IL_019d

      IL_0195:  ldloc.s    V_10
      IL_0197:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_019c:  nop
      IL_019d:  endfinally
    }  // end handler
    IL_019e:  nop
    IL_019f:  ldloc.s    V_9
    IL_01a1:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::get_Count()
    IL_01a6:  ldc.i4.0
    IL_01a7:  ceq
    IL_01a9:  ldc.i4.0
    IL_01aa:  ceq
    IL_01ac:  stloc.s    V_33
    IL_01ae:  ldloc.s    V_33
    IL_01b0:  brtrue.s   IL_01c4

    IL_01b2:  nop
    IL_01b3:  ldloc.2
    IL_01b4:  ldstr      bytearray (0A 00 4B 00 68 00 F4 00 6E 00 67 00 20 00 74 00   // ..K.h...n.g. .t.
                                    EC 00 6D 00 20 00 74 00 68 00 A5 1E 79 00 20 00   // ..m. .t.h...y. .
                                    42 00 6C 00 6F 00 63 00 6B 00 20 00 68 00 E3 1E   // B.l.o.c.k. .h...
                                    70 00 20 00 6C 00 C7 1E 2E 00 )                   // p. .l.....
    IL_01b9:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::WriteMessage(string)
    IL_01be:  nop
    IL_01bf:  br         IL_0687

    IL_01c4:  ldloc.2
    IL_01c5:  ldstr      bytearray (0A 00 10 01 61 00 6E 00 67 00 20 00 78 00 ED 1E   // ....a.n.g. .x...
                                    20 00 6C 00 FD 00 20 00 )                         //  .l... .
    IL_01ca:  ldloc.s    V_9
    IL_01cc:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::get_Count()
    IL_01d1:  box        [mscorlib]System.Int32
    IL_01d6:  ldstr      " block..."
    IL_01db:  call       string [mscorlib]System.String::Concat(object,
                                                                object,
                                                                object)
    IL_01e0:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::WriteMessage(string)
    IL_01e5:  nop
    IL_01e6:  ldloc.0
    IL_01e7:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.ApplicationServices.DocumentLock [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document::LockDocument()
    IL_01ec:  stloc.s    V_12
    .try
    {
      IL_01ee:  nop
      IL_01ef:  ldloc.1
      IL_01f0:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TransactionManager()
      IL_01f5:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartTransaction()
      IL_01fa:  stloc.s    V_10
      .try
      {
        IL_01fc:  nop
        IL_01fd:  ldloc.s    V_10
        IL_01ff:  ldloc.1
        IL_0200:  ldloc.3
        IL_0201:  ldfld      string Tvxd.Meps.ToadoSettings::Layer
        IL_0206:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::EnsureLayer(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                           class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                           string)
        IL_020b:  stloc.s    V_13
        IL_020d:  ldloc.s    V_10
        IL_020f:  ldloc.1
        IL_0210:  call       void Tvxd.Meps.Toado100::EnsureRegApp(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                   class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
        IL_0215:  nop
        IL_0216:  ldloc.s    V_10
        IL_0218:  ldloc.1
        IL_0219:  ldloc.3
        IL_021a:  ldfld      string Tvxd.Meps.ToadoSettings::Style
        IL_021f:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::EnsureTextStyle(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                               class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                               string)
        IL_0224:  stloc.s    V_14
        IL_0226:  ldloc.s    V_10
        IL_0228:  ldloc.1
        IL_0229:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_BlockTableId()
        IL_022e:  ldc.i4.0
        IL_022f:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
        IL_0234:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTable
        IL_0239:  stloc.s    V_15
        IL_023b:  ldloc.s    V_10
        IL_023d:  ldloc.s    V_15
        IL_023f:  ldsfld     string modopt([mscorlib]System.Runtime.CompilerServices.IsConst) modopt([mscorlib]System.Runtime.CompilerServices.IsConst) [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord::ModelSpace
        IL_0244:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::get_Item(string)
        IL_0249:  ldc.i4.1
        IL_024a:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
        IL_024f:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord
        IL_0254:  stloc.s    V_16
        IL_0256:  ldloc.s    V_10
        IL_0258:  ldloc.s    V_16
        IL_025a:  call       class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> Tvxd.Meps.Toado100::ScanExistingLeaders(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                                                                                                   class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord)
        IL_025f:  stloc.s    V_17
        IL_0261:  ldc.i4.0
        IL_0262:  stloc.s    V_18
        IL_0264:  ldc.i4.0
        IL_0265:  stloc.s    V_19
        IL_0267:  ldc.i4.0
        IL_0268:  stloc.s    V_20
        IL_026a:  br         IL_0452

        IL_026f:  nop
        IL_0270:  ldloc.s    V_9
        IL_0272:  ldloc.s    V_20
        IL_0274:  callvirt   instance !0 class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::get_Item(int32)
        IL_0279:  stloc.s    V_11
        IL_027b:  ldloc.s    V_10
        IL_027d:  ldloc.s    V_11
        IL_027f:  ldc.i4.0
        IL_0280:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
        IL_0285:  isinst     [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference
        IL_028a:  stloc.s    V_21
        IL_028c:  ldloc.s    V_21
        IL_028e:  ldnull
        IL_028f:  call       bool [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper::op_Equality(class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper,
                                                                                                   class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper)
        IL_0294:  brtrue.s   IL_02a2

        IL_0296:  ldloc.s    V_21
        IL_0298:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::get_IsErased()
        IL_029d:  ldc.i4.0
        IL_029e:  ceq
        IL_02a0:  br.s       IL_02a3

        IL_02a2:  ldc.i4.0
        IL_02a3:  nop
        IL_02a4:  stloc.s    V_33
        IL_02a6:  ldloc.s    V_33
        IL_02a8:  brtrue.s   IL_02af

        IL_02aa:  br         IL_044c

        IL_02af:  ldloc.s    V_21
        IL_02b1:  ldloc.3
        IL_02b2:  ldfld      bool Tvxd.Meps.ToadoSettings::Center
        IL_02b7:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d Tvxd.Meps.Toado100::Tip(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference,
                                                                                                          bool)
        IL_02bc:  stloc.s    V_22
        IL_02be:  ldloc.s    V_10
        IL_02c0:  ldloc.s    V_21
        IL_02c2:  call       string Tvxd.Meps.Toado100::NameOf(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                               class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference)
        IL_02c7:  stloc.s    V_23
        IL_02c9:  ldloc.s    V_21
        IL_02cb:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Handle [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::get_Handle()
        IL_02d0:  stloc.s    V_37
        IL_02d2:  ldloca.s   V_37
        IL_02d4:  constrained. [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Handle
        IL_02da:  callvirt   instance string [mscorlib]System.Object::ToString()
        IL_02df:  stloc.s    V_24
        IL_02e1:  ldloc.3
        IL_02e2:  ldfld      bool Tvxd.Meps.ToadoSettings::PickEach
        IL_02e7:  ldc.i4.0
        IL_02e8:  ceq
        IL_02ea:  stloc.s    V_33
        IL_02ec:  ldloc.s    V_33
        IL_02ee:  brtrue     IL_03c1

        IL_02f3:  nop
        IL_02f4:  ldstr      bytearray (0A 00 10 01 69 00 C3 1E 6D 00 20 00 11 01 B7 1E   // ....i...m. .....
                                        74 00 20 00 6E 00 68 00 E3 00 6E 00 20 00 63 00   // t. .n.h...n. .c.
                                        68 00 6F 00 20 00 7B 00 30 00 7D 00 20 00 5B 00   // h.o. .{.0.}. .[.
                                        7B 00 31 00 7D 00 2F 00 7B 00 32 00 7D 00 5D 00   // {.1.}./.{.2.}.].
                                        20 00 28 00 45 00 6E 00 74 00 65 00 72 00 20 00   //  .(.E.n.t.e.r. .
                                        3D 00 20 00 6D 00 B7 1E 63 00 20 00 11 01 CB 1E   // =. .m...c. .....
                                        6E 00 68 00 2C 00 20 00 45 00 53 00 43 00 20 00   // n.h.,. .E.S.C. .
                                        3D 00 20 00 64 00 EB 1E 6E 00 67 00 29 00 3A 00   // =. .d...n.g.).:.
                                        20 00 )                                           //  .
        IL_02f9:  ldloc.s    V_23
        IL_02fb:  ldloc.s    V_20
        IL_02fd:  ldc.i4.1
        IL_02fe:  add
        IL_02ff:  box        [mscorlib]System.Int32
        IL_0304:  ldloc.s    V_9
        IL_0306:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::get_Count()
        IL_030b:  box        [mscorlib]System.Int32
        IL_0310:  call       string [mscorlib]System.String::Format(string,
                                                                    object,
                                                                    object,
                                                                    object)
        IL_0315:  newobj     instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions::.ctor(string)
        IL_031a:  stloc.s    V_26
        IL_031c:  ldloc.s    V_26
        IL_031e:  ldc.i4.1
        IL_031f:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions::set_UseBasePoint(bool)
        IL_0324:  nop
        IL_0325:  ldloc.s    V_26
        IL_0327:  ldloc.s    V_22
        IL_0329:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptCornerOptions::set_BasePoint(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
        IL_032e:  nop
        IL_032f:  ldloc.s    V_26
        IL_0331:  ldc.i4.1
        IL_0332:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptCornerOptions::set_AllowNone(bool)
        IL_0337:  nop
        IL_0338:  ldloc.2
        IL_0339:  ldloc.s    V_26
        IL_033b:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointResult [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::GetPoint(class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions)
        IL_0340:  stloc.s    V_27
        IL_0342:  ldloc.s    V_27
        IL_0344:  callvirt   instance valuetype [accoremgd]Autodesk.AutoCAD.EditorInput.PromptStatus [accoremgd]Autodesk.AutoCAD.EditorInput.PromptResult::get_Status()
        IL_0349:  ldc.i4     0x13ec
        IL_034e:  ceq
        IL_0350:  ldc.i4.0
        IL_0351:  ceq
        IL_0353:  stloc.s    V_33
        IL_0355:  ldloc.s    V_33
        IL_0357:  brtrue.s   IL_0366

        IL_0359:  nop
        IL_035a:  ldloc.s    V_27
        IL_035c:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointResult::get_Value()
        IL_0361:  stloc.s    V_25
        IL_0363:  nop
        IL_0364:  br.s       IL_03be

        IL_0366:  ldloc.s    V_27
        IL_0368:  callvirt   instance valuetype [accoremgd]Autodesk.AutoCAD.EditorInput.PromptStatus [accoremgd]Autodesk.AutoCAD.EditorInput.PromptResult::get_Status()
        IL_036d:  ldc.i4     0x1388
        IL_0372:  ceq
        IL_0374:  ldc.i4.0
        IL_0375:  ceq
        IL_0377:  stloc.s    V_33
        IL_0379:  ldloc.s    V_33
        IL_037b:  brtrue.s   IL_03ac

        IL_037d:  nop
        IL_037e:  ldloca.s   V_25
        IL_0380:  ldloca.s   V_22
        IL_0382:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
        IL_0387:  ldloc.3
        IL_0388:  ldfld      float64 Tvxd.Meps.ToadoSettings::Distance
        IL_038d:  add
        IL_038e:  ldloca.s   V_22
        IL_0390:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
        IL_0395:  ldloc.3
        IL_0396:  ldfld      float64 Tvxd.Meps.ToadoSettings::Distance
        IL_039b:  add
        IL_039c:  ldloca.s   V_22
        IL_039e:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Z()
        IL_03a3:  call       instance void [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::.ctor(float64,
                                                                                             float64,
                                                                                             float64)
        IL_03a8:  nop
        IL_03a9:  nop
        IL_03aa:  br.s       IL_03be

        IL_03ac:  nop
        IL_03ad:  ldloc.2
        IL_03ae:  ldstr      bytearray (0A 00 10 01 E3 00 20 00 64 00 EB 1E 6E 00 67 00   // ...... .d...n.g.
                                        20 00 67 00 AF 1E 6E 00 20 00 6E 00 68 00 E3 00   //  .g...n. .n.h...
                                        6E 00 2E 00 )                                     // n...
        IL_03b3:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::WriteMessage(string)
        IL_03b8:  nop
        IL_03b9:  br         IL_0466

        IL_03be:  nop
        IL_03bf:  br.s       IL_03ee

        IL_03c1:  nop
        IL_03c2:  ldloca.s   V_25
        IL_03c4:  ldloca.s   V_22
        IL_03c6:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
        IL_03cb:  ldloc.3
        IL_03cc:  ldfld      float64 Tvxd.Meps.ToadoSettings::Distance
        IL_03d1:  add
        IL_03d2:  ldloca.s   V_22
        IL_03d4:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
        IL_03d9:  ldloc.3
        IL_03da:  ldfld      float64 Tvxd.Meps.ToadoSettings::Distance
        IL_03df:  add
        IL_03e0:  ldloca.s   V_22
        IL_03e2:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Z()
        IL_03e7:  call       instance void [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::.ctor(float64,
                                                                                             float64,
                                                                                             float64)
        IL_03ec:  nop
        IL_03ed:  nop
        IL_03ee:  ldloc.s    V_17
        IL_03f0:  ldloc.s    V_24
        IL_03f2:  callvirt   instance bool class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::ContainsKey(!0)
        IL_03f7:  brtrue.s   IL_0400

        IL_03f9:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_Null()
        IL_03fe:  br.s       IL_0409

        IL_0400:  ldloc.s    V_17
        IL_0402:  ldloc.s    V_24
        IL_0404:  callvirt   instance !1 class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::get_Item(!0)
        IL_0409:  nop
        IL_040a:  stloc.s    V_28
        IL_040c:  ldloc.1
        IL_040d:  ldloc.s    V_10
        IL_040f:  ldloc.s    V_16
        IL_0411:  ldloc.s    V_22
        IL_0413:  ldloc.s    V_25
        IL_0415:  ldloc.3
        IL_0416:  ldloc.s    V_23
        IL_0418:  ldloc.s    V_24
        IL_041a:  ldloc.s    V_28
        IL_041c:  ldloc.s    V_13
        IL_041e:  ldloc.s    V_14
        IL_0420:  ldloc.s    V_5
        IL_0422:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::CreateOrUpdateLeader(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                                    class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                                    class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord,
                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,
                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,
                                                                                                                                    class Tvxd.Meps.ToadoSettings,
                                                                                                                                    string,
                                                                                                                                    string,
                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
        IL_0427:  pop
        IL_0428:  ldloc.s    V_28
        IL_042a:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_Null()
        IL_042f:  call       bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::op_Equality(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                   valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
        IL_0434:  ldc.i4.0
        IL_0435:  ceq
        IL_0437:  stloc.s    V_33
        IL_0439:  ldloc.s    V_33
        IL_043b:  brtrue.s   IL_0445

        IL_043d:  ldloc.s    V_18
        IL_043f:  ldc.i4.1
        IL_0440:  add
        IL_0441:  stloc.s    V_18
        IL_0443:  br.s       IL_044b

        IL_0445:  ldloc.s    V_19
        IL_0447:  ldc.i4.1
        IL_0448:  add
        IL_0449:  stloc.s    V_19
        IL_044b:  nop
        IL_044c:  ldloc.s    V_20
        IL_044e:  ldc.i4.1
        IL_044f:  add
        IL_0450:  stloc.s    V_20
        IL_0452:  ldloc.s    V_20
        IL_0454:  ldloc.s    V_9
        IL_0456:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::get_Count()
        IL_045b:  clt
        IL_045d:  stloc.s    V_33
        IL_045f:  ldloc.s    V_33
        IL_0461:  brtrue     IL_026f

        IL_0466:  ldloc.s    V_10
        IL_0468:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::Commit()
        IL_046d:  nop
        IL_046e:  ldloc.2
        IL_046f:  ldstr      bytearray (0A 00 5B 00 54 00 2D 00 54 00 4F 00 41 00 44 00   // ..[.T.-.T.O.A.D.
                                        4F 00 5D 00 20 00 48 00 6F 00 E0 00 6E 00 20 00   // O.]. .H.o...n. .
                                        74 00 A5 1E 74 00 3A 00 20 00 54 00 A1 1E 6F 00   // t...t.:. .T...o.
                                        20 00 6D 00 DB 1E 69 00 20 00 7B 00 30 00 7D 00   //  .m...i. .{.0.}.
                                        2C 00 20 00 43 00 AD 1E 70 00 20 00 6E 00 68 00   // ,. .C...p. .n.h.
                                        AD 1E 74 00 20 00 7B 00 31 00 7D 00 20 00 6E 00   // ..t. .{.1.}. .n.
                                        68 00 E3 00 6E 00 20 00 74 00 CD 1E 61 00 20 00   // h...n. .t...a. .
                                        11 01 D9 1E 2E 00 )                               // ......
        IL_0474:  ldloc.s    V_18
        IL_0476:  box        [mscorlib]System.Int32
        IL_047b:  ldloc.s    V_19
        IL_047d:  box        [mscorlib]System.Int32
        IL_0482:  call       string [mscorlib]System.String::Format(string,
                                                                    object,
                                                                    object)
        IL_0487:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::WriteMessage(string)
        IL_048c:  nop
        IL_048d:  nop
        IL_048e:  leave.s    IL_04a4

      }  // end .try
      finally
      {
        IL_0490:  ldloc.s    V_10
        IL_0492:  ldnull
        IL_0493:  ceq
        IL_0495:  stloc.s    V_33
        IL_0497:  ldloc.s    V_33
        IL_0499:  brtrue.s   IL_04a3

        IL_049b:  ldloc.s    V_10
        IL_049d:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_04a2:  nop
        IL_04a3:  endfinally
      }  // end handler
      IL_04a4:  nop
      IL_04a5:  nop
      IL_04a6:  leave.s    IL_04bc

    }  // end .try
    finally
    {
      IL_04a8:  ldloc.s    V_12
      IL_04aa:  ldnull
      IL_04ab:  ceq
      IL_04ad:  stloc.s    V_33
      IL_04af:  ldloc.s    V_33
      IL_04b1:  brtrue.s   IL_04bb

      IL_04b3:  ldloc.s    V_12
      IL_04b5:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_04ba:  nop
      IL_04bb:  endfinally
    }  // end handler
    IL_04bc:  nop
    IL_04bd:  nop
    IL_04be:  br         IL_0687

    IL_04c3:  nop
    IL_04c4:  ldloc.0
    IL_04c5:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.ApplicationServices.DocumentLock [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document::LockDocument()
    IL_04ca:  stloc.s    V_12
    .try
    {
      IL_04cc:  nop
      IL_04cd:  br         IL_0666

      IL_04d2:  nop
      IL_04d3:  ldstr      bytearray (0A 00 50 00 69 00 63 00 6B 00 20 00 11 01 69 00   // ..P.i.c.k. ...i.
                                      C3 1E 6D 00 20 00 63 00 A7 1E 6E 00 20 00 67 00   // ..m. .c...n. .g.
                                      AF 1E 6E 00 20 00 6E 00 68 00 E3 00 6E 00 20 00   // ..n. .n.h...n. .
                                      74 00 CD 1E 61 00 20 00 11 01 D9 1E 20 00 28 00   // t...a. ..... .(.
                                      45 00 6E 00 74 00 65 00 72 00 20 00 68 00 6F 00   // E.n.t.e.r. .h.o.
                                      B7 1E 63 00 20 00 45 00 53 00 43 00 20 00 11 01   // ..c. .E.S.C. ...
                                      C3 1E 20 00 6B 00 BF 1E 74 00 20 00 74 00 68 00   // .. .k...t. .t.h.
                                      FA 00 63 00 29 00 3A 00 20 00 )                   // ..c.).:. .
      IL_04d8:  newobj     instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions::.ctor(string)
      IL_04dd:  stloc.s    V_29
      IL_04df:  ldloc.2
      IL_04e0:  ldloc.s    V_29
      IL_04e2:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointResult [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::GetPoint(class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions)
      IL_04e7:  stloc.s    V_30
      IL_04e9:  ldloc.s    V_30
      IL_04eb:  callvirt   instance valuetype [accoremgd]Autodesk.AutoCAD.EditorInput.PromptStatus [accoremgd]Autodesk.AutoCAD.EditorInput.PromptResult::get_Status()
      IL_04f0:  ldc.i4     0x13ec
      IL_04f5:  ceq
      IL_04f7:  stloc.s    V_33
      IL_04f9:  ldloc.s    V_33
      IL_04fb:  brtrue.s   IL_0502

      IL_04fd:  br         IL_066e

      IL_0502:  ldloc.s    V_30
      IL_0504:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointResult::get_Value()
      IL_0509:  stloc.s    V_22
      IL_050b:  ldstr      bytearray (0A 00 50 00 69 00 63 00 6B 00 20 00 11 01 69 00   // ..P.i.c.k. ...i.
                                      C3 1E 6D 00 20 00 11 01 B7 1E 74 00 20 00 6E 00   // ..m. .....t. .n.
                                      68 00 E3 00 6E 00 20 00 28 00 68 00 B0 01 DB 1E   // h...n. .(.h.....
                                      6E 00 67 00 20 00 74 00 F1 1E 20 00 64 00 6F 00   // n.g. .t... .d.o.
                                      20 00 33 00 36 00 30 00 20 00 11 01 D9 1E 29 00   //  .3.6.0. .....).
                                      3A 00 20 00 )                                     // :. .
      IL_0510:  newobj     instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions::.ctor(string)
      IL_0515:  stloc.s    V_31
      IL_0517:  ldloc.s    V_31
      IL_0519:  ldc.i4.1
      IL_051a:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions::set_UseBasePoint(bool)
      IL_051f:  nop
      IL_0520:  ldloc.s    V_31
      IL_0522:  ldloc.s    V_22
      IL_0524:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptCornerOptions::set_BasePoint(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
      IL_0529:  nop
      IL_052a:  ldloc.s    V_31
      IL_052c:  ldc.i4.1
      IL_052d:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptCornerOptions::set_AllowNone(bool)
      IL_0532:  nop
      IL_0533:  ldloc.2
      IL_0534:  ldloc.s    V_31
      IL_0536:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointResult [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::GetPoint(class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions)
      IL_053b:  stloc.s    V_32
      IL_053d:  ldloc.s    V_32
      IL_053f:  callvirt   instance valuetype [accoremgd]Autodesk.AutoCAD.EditorInput.PromptStatus [accoremgd]Autodesk.AutoCAD.EditorInput.PromptResult::get_Status()
      IL_0544:  ldc.i4     0x13ec
      IL_0549:  ceq
      IL_054b:  ldc.i4.0
      IL_054c:  ceq
      IL_054e:  stloc.s    V_33
      IL_0550:  ldloc.s    V_33
      IL_0552:  brtrue.s   IL_0561

      IL_0554:  nop
      IL_0555:  ldloc.s    V_32
      IL_0557:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointResult::get_Value()
      IL_055c:  stloc.s    V_25
      IL_055e:  nop
      IL_055f:  br.s       IL_05ad

      IL_0561:  ldloc.s    V_32
      IL_0563:  callvirt   instance valuetype [accoremgd]Autodesk.AutoCAD.EditorInput.PromptStatus [accoremgd]Autodesk.AutoCAD.EditorInput.PromptResult::get_Status()
      IL_0568:  ldc.i4     0x1388
      IL_056d:  ceq
      IL_056f:  ldc.i4.0
      IL_0570:  ceq
      IL_0572:  stloc.s    V_33
      IL_0574:  ldloc.s    V_33
      IL_0576:  brtrue.s   IL_05a7

      IL_0578:  nop
      IL_0579:  ldloca.s   V_25
      IL_057b:  ldloca.s   V_22
      IL_057d:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
      IL_0582:  ldloc.3
      IL_0583:  ldfld      float64 Tvxd.Meps.ToadoSettings::Distance
      IL_0588:  add
      IL_0589:  ldloca.s   V_22
      IL_058b:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
      IL_0590:  ldloc.3
      IL_0591:  ldfld      float64 Tvxd.Meps.ToadoSettings::Distance
      IL_0596:  add
      IL_0597:  ldloca.s   V_22
      IL_0599:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Z()
      IL_059e:  call       instance void [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::.ctor(float64,
                                                                                           float64,
                                                                                           float64)
      IL_05a3:  nop
      IL_05a4:  nop
      IL_05a5:  br.s       IL_05ad

      IL_05a7:  nop
      IL_05a8:  br         IL_066e

      IL_05ad:  ldloc.1
      IL_05ae:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TransactionManager()
      IL_05b3:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartTransaction()
      IL_05b8:  stloc.s    V_10
      .try
      {
        IL_05ba:  nop
        IL_05bb:  ldloc.s    V_10
        IL_05bd:  ldloc.1
        IL_05be:  ldloc.3
        IL_05bf:  ldfld      string Tvxd.Meps.ToadoSettings::Layer
        IL_05c4:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::EnsureLayer(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                           class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                           string)
        IL_05c9:  stloc.s    V_13
        IL_05cb:  ldloc.s    V_10
        IL_05cd:  ldloc.1
        IL_05ce:  call       void Tvxd.Meps.Toado100::EnsureRegApp(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                   class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
        IL_05d3:  nop
        IL_05d4:  ldloc.s    V_10
        IL_05d6:  ldloc.1
        IL_05d7:  ldloc.3
        IL_05d8:  ldfld      string Tvxd.Meps.ToadoSettings::Style
        IL_05dd:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::EnsureTextStyle(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                               class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                               string)
        IL_05e2:  stloc.s    V_14
        IL_05e4:  ldloc.s    V_10
        IL_05e6:  ldloc.1
        IL_05e7:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_BlockTableId()
        IL_05ec:  ldc.i4.0
        IL_05ed:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
        IL_05f2:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTable
        IL_05f7:  stloc.s    V_15
        IL_05f9:  ldloc.s    V_10
        IL_05fb:  ldloc.s    V_15
        IL_05fd:  ldsfld     string modopt([mscorlib]System.Runtime.CompilerServices.IsConst) modopt([mscorlib]System.Runtime.CompilerServices.IsConst) [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord::ModelSpace
        IL_0602:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::get_Item(string)
        IL_0607:  ldc.i4.1
        IL_0608:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
        IL_060d:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord
        IL_0612:  stloc.s    V_16
        IL_0614:  ldloc.1
        IL_0615:  ldloc.s    V_10
        IL_0617:  ldloc.s    V_16
        IL_0619:  ldloc.s    V_22
        IL_061b:  ldloc.s    V_25
        IL_061d:  ldloc.3
        IL_061e:  ldstr      ""
        IL_0623:  ldstr      ""
        IL_0628:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_Null()
        IL_062d:  ldloc.s    V_13
        IL_062f:  ldloc.s    V_14
        IL_0631:  ldloc.s    V_5
        IL_0633:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::CreateOrUpdateLeader(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                                    class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                                    class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord,
                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,
                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,
                                                                                                                                    class Tvxd.Meps.ToadoSettings,
                                                                                                                                    string,
                                                                                                                                    string,
                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
        IL_0638:  pop
        IL_0639:  ldloc.s    V_10
        IL_063b:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::Commit()
        IL_0640:  nop
        IL_0641:  nop
        IL_0642:  leave.s    IL_0658

      }  // end .try
      finally
      {
        IL_0644:  ldloc.s    V_10
        IL_0646:  ldnull
        IL_0647:  ceq
        IL_0649:  stloc.s    V_33
        IL_064b:  ldloc.s    V_33
        IL_064d:  brtrue.s   IL_0657

        IL_064f:  ldloc.s    V_10
        IL_0651:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_0656:  nop
        IL_0657:  endfinally
      }  // end handler
      IL_0658:  nop
      IL_0659:  ldloc.2
      IL_065a:  ldstr      bytearray (20 00 2D 00 3E 00 20 00 10 01 E3 00 20 00 74 00   //  .-.>. ..... .t.
                                      A1 1E 6F 00 20 00 6E 00 68 00 E3 00 6E 00 20 00   // ..o. .n.h...n. .
                                      74 00 CD 1E 61 00 20 00 11 01 D9 1E 20 00 74 00   // t...a. ..... .t.
                                      68 00 E0 00 6E 00 68 00 20 00 63 00 F4 00 6E 00   // h...n.h. .c...n.
                                      67 00 2E 00 )                                     // g...
      IL_065f:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::WriteMessage(string)
      IL_0664:  nop
      IL_0665:  nop
      IL_0666:  ldc.i4.1
      IL_0667:  stloc.s    V_33
      IL_0669:  br         IL_04d2

      IL_066e:  nop
      IL_066f:  leave.s    IL_0685

    }  // end .try
    finally
    {
      IL_0671:  ldloc.s    V_12
      IL_0673:  ldnull
      IL_0674:  ceq
      IL_0676:  stloc.s    V_33
      IL_0678:  ldloc.s    V_33
      IL_067a:  brtrue.s   IL_0684

      IL_067c:  ldloc.s    V_12
      IL_067e:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_0683:  nop
      IL_0684:  endfinally
    }  // end handler
    IL_0685:  nop
    IL_0686:  nop
    IL_0687:  nop
    IL_0688:  ret
  } // end of method Toado100::Run

  .method public hidebysig static void  RefreshCmd() cil managed
  {
    .custom instance void [accoremgd]Autodesk.AutoCAD.Runtime.CommandMethodAttribute::.ctor(string) = ( 01 00 0D 54 54 4F 41 44 4F 52 45 46 52 45 53 48   // ...TTOADOREFRESH
                                                                                                        00 00 ) 
    .custom instance void [accoremgd]Autodesk.AutoCAD.Runtime.CommandMethodAttribute::.ctor(string) = ( 01 00 0E 54 2D 54 4F 41 44 4F 52 45 46 52 45 53   // ...T-TOADOREFRES
                                                                                                        48 00 00 )                                        // H..
    // Code size       43 (0x2b)
    .maxstack  2
    .locals init (class [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document V_0,
             class Tvxd.Meps.ToadoSettings V_1,
             bool V_2)
    IL_0000:  nop
    IL_0001:  call       class [accoremgd]Autodesk.AutoCAD.ApplicationServices.DocumentCollection [accoremgd]Autodesk.AutoCAD.ApplicationServices.Core.Application::get_DocumentManager()
    IL_0006:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document [accoremgd]Autodesk.AutoCAD.ApplicationServices.DocumentCollection::get_MdiActiveDocument()
    IL_000b:  stloc.0
    IL_000c:  ldloc.0
    IL_000d:  ldnull
    IL_000e:  call       bool [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper::op_Equality(class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper,
                                                                                               class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper)
    IL_0013:  ldc.i4.0
    IL_0014:  ceq
    IL_0016:  stloc.2
    IL_0017:  ldloc.2
    IL_0018:  brtrue.s   IL_001c

    IL_001a:  br.s       IL_002a

    IL_001c:  call       class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100::Load()
    IL_0021:  stloc.1
    IL_0022:  ldloc.0
    IL_0023:  ldloc.1
    IL_0024:  call       void Tvxd.Meps.Toado100::RefreshAll(class [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document,
                                                             class Tvxd.Meps.ToadoSettings)
    IL_0029:  nop
    IL_002a:  ret
  } // end of method Toado100::RefreshCmd

  .method public hidebysig static void  RefreshAll(class [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document doc,
                                                   class Tvxd.Meps.ToadoSettings s) cil managed
  {
    // Code size       627 (0x273)
    .maxstack  12
    .locals init (class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database V_0,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.Editor V_1,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_2,
             class [accoremgd]Autodesk.AutoCAD.ApplicationServices.DocumentLock V_3,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction V_4,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_5,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_6,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTable V_7,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord V_8,
             int32 V_9,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_10,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader V_11,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer V_12,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[] V_13,
             string V_14,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_15,
             string V_16,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Handle V_17,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_18,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference V_19,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_20,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecordEnumerator V_21,
             bool V_22)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document::get_Database()
    IL_0007:  stloc.0
    IL_0008:  ldarg.0
    IL_0009:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.EditorInput.Editor [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document::get_Editor()
    IL_000e:  stloc.1
    IL_000f:  ldloc.0
    IL_0010:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::EnsureDotArrow(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
    IL_0015:  stloc.2
    IL_0016:  ldarg.0
    IL_0017:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.ApplicationServices.DocumentLock [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document::LockDocument()
    IL_001c:  stloc.3
    .try
    {
      IL_001d:  nop
      IL_001e:  ldloc.0
      IL_001f:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TransactionManager()
      IL_0024:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartTransaction()
      IL_0029:  stloc.s    V_4
      .try
      {
        IL_002b:  nop
        IL_002c:  ldloc.s    V_4
        IL_002e:  ldloc.0
        IL_002f:  ldarg.1
        IL_0030:  ldfld      string Tvxd.Meps.ToadoSettings::Layer
        IL_0035:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::EnsureLayer(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                           class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                           string)
        IL_003a:  stloc.s    V_5
        IL_003c:  ldloc.s    V_4
        IL_003e:  ldloc.0
        IL_003f:  call       void Tvxd.Meps.Toado100::EnsureRegApp(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                   class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
        IL_0044:  nop
        IL_0045:  ldloc.s    V_4
        IL_0047:  ldloc.0
        IL_0048:  ldarg.1
        IL_0049:  ldfld      string Tvxd.Meps.ToadoSettings::Style
        IL_004e:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::EnsureTextStyle(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                               class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                               string)
        IL_0053:  stloc.s    V_6
        IL_0055:  ldloc.s    V_4
        IL_0057:  ldloc.0
        IL_0058:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_BlockTableId()
        IL_005d:  ldc.i4.0
        IL_005e:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
        IL_0063:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTable
        IL_0068:  stloc.s    V_7
        IL_006a:  ldloc.s    V_4
        IL_006c:  ldloc.s    V_7
        IL_006e:  ldsfld     string modopt([mscorlib]System.Runtime.CompilerServices.IsConst) modopt([mscorlib]System.Runtime.CompilerServices.IsConst) [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord::ModelSpace
        IL_0073:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::get_Item(string)
        IL_0078:  ldc.i4.1
        IL_0079:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
        IL_007e:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord
        IL_0083:  stloc.s    V_8
        IL_0085:  ldc.i4.0
        IL_0086:  stloc.s    V_9
        IL_0088:  nop
        IL_0089:  ldloc.s    V_8
        IL_008b:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecordEnumerator [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord::GetEnumerator()
        IL_0090:  stloc.s    V_21
        .try
        {
          IL_0092:  br         IL_01fd

          IL_0097:  ldloc.s    V_21
          IL_0099:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecordEnumerator::get_Current()
          IL_009e:  stloc.s    V_10
          IL_00a0:  nop
          IL_00a1:  ldloca.s   V_10
          IL_00a3:  call       instance class [Acdbmgd]Autodesk.AutoCAD.Runtime.RXClass [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_ObjectClass()
          IL_00a8:  callvirt   instance string [Acdbmgd]Autodesk.AutoCAD.Runtime.RXClass::get_DxfName()
          IL_00ad:  ldstr      "MULTILEADER"
          IL_00b2:  ldc.i4.5
          IL_00b3:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                             valuetype [mscorlib]System.StringComparison)
          IL_00b8:  stloc.s    V_22
          IL_00ba:  ldloc.s    V_22
          IL_00bc:  brtrue.s   IL_00c3

          IL_00be:  br         IL_01fd

          IL_00c3:  ldloc.s    V_4
          IL_00c5:  ldloc.s    V_10
          IL_00c7:  ldc.i4.1
          IL_00c8:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                    valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
          IL_00cd:  isinst     [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader
          IL_00d2:  stloc.s    V_11
          IL_00d4:  ldloc.s    V_11
          IL_00d6:  ldnull
          IL_00d7:  call       bool [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper::op_Equality(class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper,
                                                                                                     class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper)
          IL_00dc:  ldc.i4.0
          IL_00dd:  ceq
          IL_00df:  stloc.s    V_22
          IL_00e1:  ldloc.s    V_22
          IL_00e3:  brtrue.s   IL_00ea

          IL_00e5:  br         IL_01fd

          IL_00ea:  ldloc.s    V_11
          IL_00ec:  ldstr      "TVXD_TOADO"
          IL_00f1:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::GetXDataForApplication(string)
          IL_00f6:  stloc.s    V_12
          IL_00f8:  ldloc.s    V_12
          IL_00fa:  ldnull
          IL_00fb:  call       bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer::op_Equality(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer,
                                                                                                         class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer)
          IL_0100:  ldc.i4.0
          IL_0101:  ceq
          IL_0103:  stloc.s    V_22
          IL_0105:  ldloc.s    V_22
          IL_0107:  brtrue.s   IL_010e

          IL_0109:  br         IL_01fd

          IL_010e:  ldloc.s    V_12
          IL_0110:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[] [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer::AsArray()
          IL_0115:  stloc.s    V_13
          IL_0117:  ldloc.s    V_13
          IL_0119:  ldlen
          IL_011a:  conv.i4
          IL_011b:  ldc.i4.1
          IL_011c:  bgt.s      IL_0125

          IL_011e:  ldstr      ""
          IL_0123:  br.s       IL_0137

          IL_0125:  ldloc.s    V_13
          IL_0127:  ldc.i4.1
          IL_0128:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
          IL_012d:  call       instance object [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::get_Value()
          IL_0132:  call       string [mscorlib]System.Convert::ToString(object)
          IL_0137:  nop
          IL_0138:  stloc.s    V_14
          IL_013a:  ldloc.s    V_11
          IL_013c:  ldc.i4.0
          IL_013d:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::GetFirstVertex(int32)
          IL_0142:  stloc.s    V_15
          IL_0144:  ldstr      ""
          IL_0149:  stloc.s    V_16
          IL_014b:  ldloc.s    V_14
          IL_014d:  call       bool [mscorlib]System.String::IsNullOrEmpty(string)
          IL_0152:  stloc.s    V_22
          IL_0154:  ldloc.s    V_22
          IL_0156:  brtrue.s   IL_01d2

          IL_0158:  nop
          .try
          {
            IL_0159:  nop
            IL_015a:  ldloca.s   V_17
            IL_015c:  ldloc.s    V_14
            IL_015e:  ldc.i4.s   16
            IL_0160:  call       int64 [mscorlib]System.Convert::ToInt64(string,
                                                                         int32)
            IL_0165:  call       instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Handle::.ctor(int64)
            IL_016a:  nop
            IL_016b:  ldloc.0
            IL_016c:  ldloc.s    V_17
            IL_016e:  ldloca.s   V_18
            IL_0170:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::TryGetObjectId(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Handle,
                                                                                                                   valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId&)
            IL_0175:  brfalse.s  IL_0180

            IL_0177:  ldloca.s   V_18
            IL_0179:  call       instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_IsErased()
            IL_017e:  br.s       IL_0181

            IL_0180:  ldc.i4.1
            IL_0181:  nop
            IL_0182:  stloc.s    V_22
            IL_0184:  ldloc.s    V_22
            IL_0186:  brtrue.s   IL_01c8

            IL_0188:  nop
            IL_0189:  ldloc.s    V_4
            IL_018b:  ldloc.s    V_18
            IL_018d:  ldc.i4.0
            IL_018e:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                      valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
            IL_0193:  isinst     [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference
            IL_0198:  stloc.s    V_19
            IL_019a:  ldloc.s    V_19
            IL_019c:  ldnull
            IL_019d:  call       bool [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper::op_Inequality(class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper,
                                                                                                         class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper)
            IL_01a2:  ldc.i4.0
            IL_01a3:  ceq
            IL_01a5:  stloc.s    V_22
            IL_01a7:  ldloc.s    V_22
            IL_01a9:  brtrue.s   IL_01c7

            IL_01ab:  nop
            IL_01ac:  ldloc.s    V_19
            IL_01ae:  ldarg.1
            IL_01af:  ldfld      bool Tvxd.Meps.ToadoSettings::Center
            IL_01b4:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d Tvxd.Meps.Toado100::Tip(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference,
                                                                                                              bool)
            IL_01b9:  stloc.s    V_15
            IL_01bb:  ldloc.s    V_4
            IL_01bd:  ldloc.s    V_19
            IL_01bf:  call       string Tvxd.Meps.Toado100::NameOf(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                   class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference)
            IL_01c4:  stloc.s    V_16
            IL_01c6:  nop
            IL_01c7:  nop
            IL_01c8:  nop
            IL_01c9:  leave.s    IL_01d0

          }  // end .try
          catch [mscorlib]System.Object 
          {
            IL_01cb:  pop
            IL_01cc:  nop
            IL_01cd:  nop
            IL_01ce:  leave.s    IL_01d0

          }  // end handler
          IL_01d0:  nop
          IL_01d1:  nop
          IL_01d2:  ldloc.s    V_11
          IL_01d4:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::get_TextLocation()
          IL_01d9:  stloc.s    V_20
          IL_01db:  ldloc.0
          IL_01dc:  ldloc.s    V_4
          IL_01de:  ldloc.s    V_8
          IL_01e0:  ldloc.s    V_15
          IL_01e2:  ldloc.s    V_20
          IL_01e4:  ldarg.1
          IL_01e5:  ldloc.s    V_16
          IL_01e7:  ldloc.s    V_14
          IL_01e9:  ldloc.s    V_10
          IL_01eb:  ldloc.s    V_5
          IL_01ed:  ldloc.s    V_6
          IL_01ef:  ldloc.2
          IL_01f0:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::CreateOrUpdateLeader(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                                      class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                                      class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord,
                                                                                                                                      valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,
                                                                                                                                      valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,
                                                                                                                                      class Tvxd.Meps.ToadoSettings,
                                                                                                                                      string,
                                                                                                                                      string,
                                                                                                                                      valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                      valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                      valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                      valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
          IL_01f5:  pop
          IL_01f6:  ldloc.s    V_9
          IL_01f8:  ldc.i4.1
          IL_01f9:  add
          IL_01fa:  stloc.s    V_9
          IL_01fc:  nop
          IL_01fd:  ldloc.s    V_21
          IL_01ff:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecordEnumerator::MoveNext()
          IL_0204:  stloc.s    V_22
          IL_0206:  ldloc.s    V_22
          IL_0208:  brtrue     IL_0097

          IL_020d:  leave.s    IL_0223

        }  // end .try
        finally
        {
          IL_020f:  ldloc.s    V_21
          IL_0211:  ldnull
          IL_0212:  ceq
          IL_0214:  stloc.s    V_22
          IL_0216:  ldloc.s    V_22
          IL_0218:  brtrue.s   IL_0222

          IL_021a:  ldloc.s    V_21
          IL_021c:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
          IL_0221:  nop
          IL_0222:  endfinally
        }  // end handler
        IL_0223:  nop
        IL_0224:  ldloc.s    V_4
        IL_0226:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::Commit()
        IL_022b:  nop
        IL_022c:  ldloc.1
        IL_022d:  ldstr      bytearray (0A 00 5B 00 54 00 2D 00 54 00 4F 00 41 00 44 00   // ..[.T.-.T.O.A.D.
                                        4F 00 5D 00 20 00 10 01 E3 00 20 00 6C 00 E0 00   // O.]. ..... .l...
                                        6D 00 20 00 6D 00 DB 1E 69 00 20 00 28 00 52 00   // m. .m...i. .(.R.
                                        65 00 66 00 72 00 65 00 73 00 68 00 29 00 20 00   // e.f.r.e.s.h.). .
                                        7B 00 30 00 7D 00 20 00 6E 00 68 00 E3 00 6E 00   // {.0.}. .n.h...n.
                                        20 00 74 00 CD 1E 61 00 20 00 11 01 D9 1E 20 00   //  .t...a. ..... .
                                        74 00 72 00 EA 00 6E 00 20 00 62 00 A3 1E 6E 00   // t.r...n. .b...n.
                                        20 00 76 00 BD 1E 2E 00 )                         //  .v.....
        IL_0232:  ldloc.s    V_9
        IL_0234:  box        [mscorlib]System.Int32
        IL_0239:  call       string [mscorlib]System.String::Format(string,
                                                                    object)
        IL_023e:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::WriteMessage(string)
        IL_0243:  nop
        IL_0244:  nop
        IL_0245:  leave.s    IL_025b

      }  // end .try
      finally
      {
        IL_0247:  ldloc.s    V_4
        IL_0249:  ldnull
        IL_024a:  ceq
        IL_024c:  stloc.s    V_22
        IL_024e:  ldloc.s    V_22
        IL_0250:  brtrue.s   IL_025a

        IL_0252:  ldloc.s    V_4
        IL_0254:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_0259:  nop
        IL_025a:  endfinally
      }  // end handler
      IL_025b:  nop
      IL_025c:  nop
      IL_025d:  leave.s    IL_0271

    }  // end .try
    finally
    {
      IL_025f:  ldloc.3
      IL_0260:  ldnull
      IL_0261:  ceq
      IL_0263:  stloc.s    V_22
      IL_0265:  ldloc.s    V_22
      IL_0267:  brtrue.s   IL_0270

      IL_0269:  ldloc.3
      IL_026a:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_026f:  nop
      IL_0270:  endfinally
    }  // end handler
    IL_0271:  nop
    IL_0272:  ret
  } // end of method Toado100::RefreshAll

  .method public hidebysig static class Tvxd.Meps.ToadoResult 
          Apply(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db,
                class [mscorlib]System.Collections.Generic.IEnumerable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> ids,
                class Tvxd.Meps.ToadoSettings s,
                class [mscorlib]System.Func`4<int32,int32,valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>> pick) cil managed
  {
    // Code size       574 (0x23e)
    .maxstack  12
    .locals init (class Tvxd.Meps.ToadoResult V_0,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction V_1,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_2,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_3,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_4,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTable V_5,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord V_6,
             class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> V_7,
             class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> V_8,
             int32 V_9,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_10,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference V_11,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_12,
             string V_13,
             string V_14,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_15,
             valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d> V_16,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_17,
             class Tvxd.Meps.ToadoResult V_18,
             bool V_19,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Handle V_20)
    IL_0000:  nop
    IL_0001:  newobj     instance void Tvxd.Meps.ToadoResult::.ctor()
    IL_0006:  stloc.0
    IL_0007:  ldarg.0
    IL_0008:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TransactionManager()
    IL_000d:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartTransaction()
    IL_0012:  stloc.1
    .try
    {
      IL_0013:  nop
      IL_0014:  ldloc.1
      IL_0015:  ldarg.0
      IL_0016:  ldarg.2
      IL_0017:  ldfld      string Tvxd.Meps.ToadoSettings::Layer
      IL_001c:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::EnsureLayer(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                         class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                         string)
      IL_0021:  stloc.2
      IL_0022:  ldloc.1
      IL_0023:  ldarg.0
      IL_0024:  call       void Tvxd.Meps.Toado100::EnsureRegApp(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                 class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
      IL_0029:  nop
      IL_002a:  ldloc.1
      IL_002b:  ldarg.0
      IL_002c:  ldarg.2
      IL_002d:  ldfld      string Tvxd.Meps.ToadoSettings::Style
      IL_0032:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::EnsureTextStyle(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                             string)
      IL_0037:  stloc.3
      IL_0038:  ldarg.0
      IL_0039:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::EnsureDotArrow(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
      IL_003e:  stloc.s    V_4
      IL_0040:  ldloc.1
      IL_0041:  ldarg.0
      IL_0042:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_BlockTableId()
      IL_0047:  ldc.i4.0
      IL_0048:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
      IL_004d:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTable
      IL_0052:  stloc.s    V_5
      IL_0054:  ldloc.1
      IL_0055:  ldloc.s    V_5
      IL_0057:  ldsfld     string modopt([mscorlib]System.Runtime.CompilerServices.IsConst) modopt([mscorlib]System.Runtime.CompilerServices.IsConst) [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord::ModelSpace
      IL_005c:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::get_Item(string)
      IL_0061:  ldc.i4.1
      IL_0062:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
      IL_0067:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord
      IL_006c:  stloc.s    V_6
      IL_006e:  ldloc.1
      IL_006f:  ldloc.s    V_6
      IL_0071:  call       class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> Tvxd.Meps.Toado100::ScanExistingLeaders(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                                                                                                 class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord)
      IL_0076:  stloc.s    V_7
      IL_0078:  ldarg.1
      IL_0079:  newobj     instance void class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::.ctor(class [mscorlib]System.Collections.Generic.IEnumerable`1<!0>)
      IL_007e:  stloc.s    V_8
      IL_0080:  ldc.i4.0
      IL_0081:  stloc.s    V_9
      IL_0083:  br         IL_0205

      IL_0088:  nop
      IL_0089:  ldloc.s    V_8
      IL_008b:  ldloc.s    V_9
      IL_008d:  callvirt   instance !0 class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::get_Item(int32)
      IL_0092:  stloc.s    V_10
      IL_0094:  ldloc.1
      IL_0095:  ldloc.s    V_10
      IL_0097:  ldc.i4.0
      IL_0098:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
      IL_009d:  isinst     [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference
      IL_00a2:  stloc.s    V_11
      IL_00a4:  ldloc.s    V_11
      IL_00a6:  ldnull
      IL_00a7:  call       bool [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper::op_Equality(class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper,
                                                                                                 class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper)
      IL_00ac:  brtrue.s   IL_00ba

      IL_00ae:  ldloc.s    V_11
      IL_00b0:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::get_IsErased()
      IL_00b5:  ldc.i4.0
      IL_00b6:  ceq
      IL_00b8:  br.s       IL_00bb

      IL_00ba:  ldc.i4.0
      IL_00bb:  nop
      IL_00bc:  stloc.s    V_19
      IL_00be:  ldloc.s    V_19
      IL_00c0:  brtrue.s   IL_00d6

      IL_00c2:  nop
      IL_00c3:  ldloc.0
      IL_00c4:  dup
      IL_00c5:  ldfld      int32 Tvxd.Meps.ToadoResult::skipped
      IL_00ca:  ldc.i4.1
      IL_00cb:  add
      IL_00cc:  stfld      int32 Tvxd.Meps.ToadoResult::skipped
      IL_00d1:  br         IL_01ff

      IL_00d6:  ldloc.s    V_11
      IL_00d8:  ldarg.2
      IL_00d9:  ldfld      bool Tvxd.Meps.ToadoSettings::Center
      IL_00de:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d Tvxd.Meps.Toado100::Tip(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference,
                                                                                                        bool)
      IL_00e3:  stloc.s    V_12
      IL_00e5:  ldloc.1
      IL_00e6:  ldloc.s    V_11
      IL_00e8:  call       string Tvxd.Meps.Toado100::NameOf(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference)
      IL_00ed:  stloc.s    V_13
      IL_00ef:  ldloc.s    V_11
      IL_00f1:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Handle [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::get_Handle()
      IL_00f6:  stloc.s    V_20
      IL_00f8:  ldloca.s   V_20
      IL_00fa:  constrained. [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Handle
      IL_0100:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_0105:  stloc.s    V_14
      IL_0107:  ldarg.3
      IL_0108:  ldnull
      IL_0109:  ceq
      IL_010b:  stloc.s    V_19
      IL_010d:  ldloc.s    V_19
      IL_010f:  brtrue.s   IL_0167

      IL_0111:  nop
      IL_0112:  ldarg.3
      IL_0113:  ldloc.s    V_9
      IL_0115:  ldc.i4.1
      IL_0116:  add
      IL_0117:  ldloc.s    V_8
      IL_0119:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::get_Count()
      IL_011e:  ldloc.s    V_12
      IL_0120:  callvirt   instance !3 class [mscorlib]System.Func`4<int32,int32,valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>>::Invoke(!0,
                                                                                                                                                                                                                                           !1,
                                                                                                                                                                                                                                           !2)
      IL_0125:  stloc.s    V_16
      IL_0127:  ldloca.s   V_16
      IL_0129:  call       instance bool valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>::get_HasValue()
      IL_012e:  brtrue.s   IL_015a

      IL_0130:  ldloca.s   V_12
      IL_0132:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
      IL_0137:  ldarg.2
      IL_0138:  ldfld      float64 Tvxd.Meps.ToadoSettings::Distance
      IL_013d:  add
      IL_013e:  ldloca.s   V_12
      IL_0140:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
      IL_0145:  ldarg.2
      IL_0146:  ldfld      float64 Tvxd.Meps.ToadoSettings::Distance
      IL_014b:  add
      IL_014c:  ldloca.s   V_12
      IL_014e:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Z()
      IL_0153:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::.ctor(float64,
                                                                                           float64,
                                                                                           float64)
      IL_0158:  br.s       IL_0161

      IL_015a:  ldloca.s   V_16
      IL_015c:  call       instance !0 valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>::get_Value()
      IL_0161:  nop
      IL_0162:  stloc.s    V_15
      IL_0164:  nop
      IL_0165:  br.s       IL_0194

      IL_0167:  nop
      IL_0168:  ldloca.s   V_15
      IL_016a:  ldloca.s   V_12
      IL_016c:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
      IL_0171:  ldarg.2
      IL_0172:  ldfld      float64 Tvxd.Meps.ToadoSettings::Distance
      IL_0177:  add
      IL_0178:  ldloca.s   V_12
      IL_017a:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
      IL_017f:  ldarg.2
      IL_0180:  ldfld      float64 Tvxd.Meps.ToadoSettings::Distance
      IL_0185:  add
      IL_0186:  ldloca.s   V_12
      IL_0188:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Z()
      IL_018d:  call       instance void [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::.ctor(float64,
                                                                                           float64,
                                                                                           float64)
      IL_0192:  nop
      IL_0193:  nop
      IL_0194:  ldloc.s    V_7
      IL_0196:  ldloc.s    V_14
      IL_0198:  callvirt   instance bool class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::ContainsKey(!0)
      IL_019d:  brtrue.s   IL_01a6

      IL_019f:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_Null()
      IL_01a4:  br.s       IL_01af

      IL_01a6:  ldloc.s    V_7
      IL_01a8:  ldloc.s    V_14
      IL_01aa:  callvirt   instance !1 class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::get_Item(!0)
      IL_01af:  nop
      IL_01b0:  stloc.s    V_17
      IL_01b2:  ldarg.0
      IL_01b3:  ldloc.1
      IL_01b4:  ldloc.s    V_6
      IL_01b6:  ldloc.s    V_12
      IL_01b8:  ldloc.s    V_15
      IL_01ba:  ldarg.2
      IL_01bb:  ldloc.s    V_13
      IL_01bd:  ldloc.s    V_14
      IL_01bf:  ldloc.s    V_17
      IL_01c1:  ldloc.2
      IL_01c2:  ldloc.3
      IL_01c3:  ldloc.s    V_4
      IL_01c5:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::CreateOrUpdateLeader(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                                  class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                                  class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord,
                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,
                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,
                                                                                                                                  class Tvxd.Meps.ToadoSettings,
                                                                                                                                  string,
                                                                                                                                  string,
                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
      IL_01ca:  pop
      IL_01cb:  ldloc.s    V_17
      IL_01cd:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_Null()
      IL_01d2:  call       bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::op_Equality(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                 valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
      IL_01d7:  ldc.i4.0
      IL_01d8:  ceq
      IL_01da:  stloc.s    V_19
      IL_01dc:  ldloc.s    V_19
      IL_01de:  brtrue.s   IL_01f0

      IL_01e0:  ldloc.0
      IL_01e1:  dup
      IL_01e2:  ldfld      int32 Tvxd.Meps.ToadoResult::created
      IL_01e7:  ldc.i4.1
      IL_01e8:  add
      IL_01e9:  stfld      int32 Tvxd.Meps.ToadoResult::created
      IL_01ee:  br.s       IL_01fe

      IL_01f0:  ldloc.0
      IL_01f1:  dup
      IL_01f2:  ldfld      int32 Tvxd.Meps.ToadoResult::updated
      IL_01f7:  ldc.i4.1
      IL_01f8:  add
      IL_01f9:  stfld      int32 Tvxd.Meps.ToadoResult::updated
      IL_01fe:  nop
      IL_01ff:  ldloc.s    V_9
      IL_0201:  ldc.i4.1
      IL_0202:  add
      IL_0203:  stloc.s    V_9
      IL_0205:  ldloc.s    V_9
      IL_0207:  ldloc.s    V_8
      IL_0209:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::get_Count()
      IL_020e:  clt
      IL_0210:  stloc.s    V_19
      IL_0212:  ldloc.s    V_19
      IL_0214:  brtrue     IL_0088

      IL_0219:  ldloc.1
      IL_021a:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::Commit()
      IL_021f:  nop
      IL_0220:  nop
      IL_0221:  leave.s    IL_0235

    }  // end .try
    finally
    {
      IL_0223:  ldloc.1
      IL_0224:  ldnull
      IL_0225:  ceq
      IL_0227:  stloc.s    V_19
      IL_0229:  ldloc.s    V_19
      IL_022b:  brtrue.s   IL_0234

      IL_022d:  ldloc.1
      IL_022e:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_0233:  nop
      IL_0234:  endfinally
    }  // end handler
    IL_0235:  nop
    IL_0236:  ldloc.0
    IL_0237:  stloc.s    V_18
    IL_0239:  br.s       IL_023b

    IL_023b:  ldloc.s    V_18
    IL_023d:  ret
  } // end of method Toado100::Apply

  .method private hidebysig specialname rtspecialname static 
          void  .cctor() cil managed
  {
    // Code size       17 (0x11)
    .maxstack  8
    IL_0000:  ldnull
    IL_0001:  stsfld     string Tvxd.Meps.Toado100::IniOverride
    IL_0006:  call       class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()
    IL_000b:  stsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
    IL_0010:  ret
  } // end of method Toado100::.cctor

  .property string IniPath()
  {
    .get string Tvxd.Meps.Toado100::get_IniPath()
  } // end of property Toado100::IniPath
} // end of class Tvxd.Meps.Toado100


// =============================================================

// *********** DISASSEMBLY COMPLETE ***********************
// WARNING: Created Win32 resource file C:\Users\ADMIN\.gemini\antigravity\scratch\ToadoPart.res
