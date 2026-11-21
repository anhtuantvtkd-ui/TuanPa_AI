
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.8.3928.0
//  Copyright (c) Microsoft Corporation.  All rights reserved.


// warning : THIS IS A PARTIAL DISASSEMBLY, NOT SUITABLE FOR RE-ASSEMBLING



.class public abstract auto ansi sealed beforefieldinit Tvxd.Meps.Toado100
       extends [mscorlib]System.Object
{
  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass6'
         extends [mscorlib]System.Object
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public class [accoremgd]Autodesk.AutoCAD.EditorInput.Editor ed
    .field public valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Matrix3d ucs
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass6'::.ctor

    .method public hidebysig instance valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d> 
            '<Run>b__5'(int32 i,
                        int32 n,
                        valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d tip) cil managed
    {
      // Code size       195 (0xc3)
      .maxstack  4
      .locals init (class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointResult V_0,
               class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions V_1,
               valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d> V_2,
               object[] V_3,
               bool V_4,
               valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d> V_5,
               valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_6)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldfld      class [accoremgd]Autodesk.AutoCAD.EditorInput.Editor Tvxd.Meps.Toado100/'<>c__DisplayClass6'::ed
      IL_0007:  ldc.i4.5
      IL_0008:  newarr     [mscorlib]System.Object
      IL_000d:  stloc.3
      IL_000e:  ldloc.3
      IL_000f:  ldc.i4.0
      IL_0010:  ldstr      bytearray (0A 00 10 01 69 00 C3 1E 6D 00 20 00 11 01 B7 1E   // ....i...m. .....
                                      74 00 20 00 6E 00 68 00 E3 00 6E 00 20 00 )       // t. .n.h...n. .
      IL_0015:  stelem.ref
      IL_0016:  ldloc.3
      IL_0017:  ldc.i4.1
      IL_0018:  ldarg.1
      IL_0019:  box        [mscorlib]System.Int32
      IL_001e:  stelem.ref
      IL_001f:  ldloc.3
      IL_0020:  ldc.i4.2
      IL_0021:  ldstr      "/"
      IL_0026:  stelem.ref
      IL_0027:  ldloc.3
      IL_0028:  ldc.i4.3
      IL_0029:  ldarg.2
      IL_002a:  box        [mscorlib]System.Int32
      IL_002f:  stelem.ref
      IL_0030:  ldloc.3
      IL_0031:  ldc.i4.4
      IL_0032:  ldstr      bytearray (20 00 28 00 45 00 6E 00 74 00 65 00 72 00 20 00   //  .(.E.n.t.e.r. .
                                      3D 00 20 00 76 00 CB 1E 20 00 74 00 72 00 ED 00   // =. .v... .t.r...
                                      20 00 6D 00 B7 1E 63 00 20 00 11 01 CB 1E 6E 00   //  .m...c. .....n.
                                      68 00 2C 00 20 00 45 00 53 00 43 00 20 00 68 00   // h.,. .E.S.C. .h.
                                      E7 1E 79 00 20 00 63 00 A3 1E 20 00 6C 00 C7 1E   // ..y. .c... .l...
                                      6E 00 68 00 29 00 3A 00 20 00 )                   // n.h.).:. .
      IL_0037:  stelem.ref
      IL_0038:  ldloc.3
      IL_0039:  call       string [mscorlib]System.String::Concat(object[])
      IL_003e:  newobj     instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions::.ctor(string)
      IL_0043:  stloc.1
      IL_0044:  ldloc.1
      IL_0045:  ldc.i4.1
      IL_0046:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions::set_UseBasePoint(bool)
      IL_004b:  nop
      IL_004c:  ldloc.1
      IL_004d:  ldarg.3
      IL_004e:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptCornerOptions::set_BasePoint(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
      IL_0053:  nop
      IL_0054:  ldloc.1
      IL_0055:  ldc.i4.1
      IL_0056:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptCornerOptions::set_AllowNone(bool)
      IL_005b:  nop
      IL_005c:  ldloc.1
      IL_005d:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointResult [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::GetPoint(class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointOptions)
      IL_0062:  stloc.0
      IL_0063:  ldloc.0
      IL_0064:  callvirt   instance valuetype [accoremgd]Autodesk.AutoCAD.EditorInput.PromptStatus [accoremgd]Autodesk.AutoCAD.EditorInput.PromptResult::get_Status()
      IL_0069:  ldc.i4     0x1388
      IL_006e:  ceq
      IL_0070:  ldc.i4.0
      IL_0071:  ceq
      IL_0073:  stloc.s    V_4
      IL_0075:  ldloc.s    V_4
      IL_0077:  brtrue.s   IL_0086

      IL_0079:  ldloca.s   V_5
      IL_007b:  initobj    valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>
      IL_0081:  ldloc.s    V_5
      IL_0083:  stloc.2
      IL_0084:  br.s       IL_00c1

      IL_0086:  ldloc.0
      IL_0087:  callvirt   instance valuetype [accoremgd]Autodesk.AutoCAD.EditorInput.PromptStatus [accoremgd]Autodesk.AutoCAD.EditorInput.PromptResult::get_Status()
      IL_008c:  ldc.i4     0x13ec
      IL_0091:  ceq
      IL_0093:  stloc.s    V_4
      IL_0095:  ldloc.s    V_4
      IL_0097:  brtrue.s   IL_00a4

      IL_0099:  ldstr      bytearray (10 01 E3 00 20 00 68 00 E7 1E 79 00 20 00 6C 00   // .... .h...y. .l.
                                      C7 1E 6E 00 68 00 20 00 54 00 2D 00 54 00 4F 00   // ..n.h. .T.-.T.O.
                                      41 00 44 00 4F 00 3B 00 20 00 63 00 68 00 B0 01   // A.D.O.;. .c.h...
                                      61 00 20 00 74 00 A1 1E 6F 00 20 00 6E 00 68 00   // a. .t...o. .n.h.
                                      E3 00 6E 00 20 00 6E 00 E0 00 6F 00 2E 00 )       // ..n. .n...o...
      IL_009e:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_00a3:  throw

      IL_00a4:  ldloc.0
      IL_00a5:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [accoremgd]Autodesk.AutoCAD.EditorInput.PromptPointResult::get_Value()
      IL_00aa:  stloc.s    V_6
      IL_00ac:  ldloca.s   V_6
      IL_00ae:  ldarg.0
      IL_00af:  ldfld      valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Matrix3d Tvxd.Meps.Toado100/'<>c__DisplayClass6'::ucs
      IL_00b4:  call       instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::TransformBy(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Matrix3d)
      IL_00b9:  newobj     instance void valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>::.ctor(!0)
      IL_00be:  stloc.2
      IL_00bf:  br.s       IL_00c1

      IL_00c1:  ldloc.2
      IL_00c2:  ret
    } // end of method '<>c__DisplayClass6'::'<Run>b__5'

  } // end of class '<>c__DisplayClass6'

  .field public static literal string Reg = "TVXD_TOADO"
  .field public static string IniOverride
  .field private static initonly class [mscorlib]System.Globalization.CultureInfo Inv
  .method private hidebysig specialname static 
          string  get_IniPath() cil managed
  {
    // Code size       48 (0x30)
    .maxstack  4
    .locals init (string V_0)
    IL_0000:  nop
    IL_0001:  ldsfld     string Tvxd.Meps.Toado100::IniOverride
    IL_0006:  brtrue.s   IL_0025

    IL_0008:  ldc.i4.s   28
    IL_000a:  call       string [mscorlib]System.Environment::GetFolderPath(valuetype [mscorlib]System.Environment/SpecialFolder)
    IL_000f:  ldstr      "TVXD"
    IL_0014:  ldstr      "MEPS060"
    IL_0019:  ldstr      "toado.ini"
    IL_001e:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string,
                                                                  string,
                                                                  string)
    IL_0023:  br.s       IL_002a

    IL_0025:  ldsfld     string Tvxd.Meps.Toado100::IniOverride
    IL_002a:  nop
    IL_002b:  stloc.0
    IL_002c:  br.s       IL_002e

    IL_002e:  ldloc.0
    IL_002f:  ret
  } // end of method Toado100::get_IniPath

  .method private hidebysig static float64 
          D(string v,
            float64 d,
            float64 lo,
            float64 hi) cil managed
  {
    // Code size       68 (0x44)
    .maxstack  4
    .locals init (float64 V_0,
             float64 V_1,
             bool V_2)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldc.i4     0xa7
    IL_0007:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
    IL_000c:  ldloca.s   V_0
    IL_000e:  call       bool [mscorlib]System.Double::TryParse(string,
                                                                valuetype [mscorlib]System.Globalization.NumberStyles,
                                                                class [mscorlib]System.IFormatProvider,
                                                                float64&)
    IL_0013:  brfalse.s  IL_0028

    IL_0015:  ldloc.0
    IL_0016:  call       bool [mscorlib]System.Double::IsNaN(float64)
    IL_001b:  brtrue.s   IL_0028

    IL_001d:  ldloc.0
    IL_001e:  call       bool [mscorlib]System.Double::IsInfinity(float64)
    IL_0023:  ldc.i4.0
    IL_0024:  ceq
    IL_0026:  br.s       IL_0029

    IL_0028:  ldc.i4.0
    IL_0029:  nop
    IL_002a:  stloc.2
    IL_002b:  ldloc.2
    IL_002c:  brtrue.s   IL_0032

    IL_002e:  ldarg.1
    IL_002f:  stloc.1
    IL_0030:  br.s       IL_0042

    IL_0032:  ldarg.2
    IL_0033:  ldarg.3
    IL_0034:  ldloc.0
    IL_0035:  call       float64 [mscorlib]System.Math::Min(float64,
                                                            float64)
    IL_003a:  call       float64 [mscorlib]System.Math::Max(float64,
                                                            float64)
    IL_003f:  stloc.1
    IL_0040:  br.s       IL_0042

    IL_0042:  ldloc.1
    IL_0043:  ret
  } // end of method Toado100::D

  .method private hidebysig static bool  B(string v,
                                           bool d) cil managed
  {
    // Code size       21 (0x15)
    .maxstack  2
    .locals init (bool V_0,
             bool V_1)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldloca.s   V_0
    IL_0004:  call       bool [mscorlib]System.Boolean::TryParse(string,
                                                                 bool&)
    IL_0009:  brtrue.s   IL_000e

    IL_000b:  ldarg.1
    IL_000c:  br.s       IL_000f

    IL_000e:  ldloc.0
    IL_000f:  nop
    IL_0010:  stloc.1
    IL_0011:  br.s       IL_0013

    IL_0013:  ldloc.1
    IL_0014:  ret
  } // end of method Toado100::B

  .method public hidebysig static class Tvxd.Meps.ToadoSettings 
          Load() cil managed
  {
    // Code size       926 (0x39e)
    .maxstack  5
    .locals init (class Tvxd.Meps.ToadoSettings V_0,
             string V_1,
             int32 V_2,
             string V_3,
             string V_4,
             class Tvxd.Meps.ToadoSettings V_5,
             bool V_6,
             string[] V_7,
             int32 V_8,
             string V_9,
             int32 V_10)
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
      IL_0019:  brtrue     IL_038d

      IL_001e:  nop
      IL_001f:  call       string Tvxd.Meps.Toado100::get_IniPath()
      IL_0024:  call       class [mscorlib]System.Text.Encoding [mscorlib]System.Text.Encoding::get_UTF8()
      IL_0029:  call       string[] [mscorlib]System.IO.File::ReadAllLines(string,
                                                                           class [mscorlib]System.Text.Encoding)
      IL_002e:  stloc.s    V_7
      IL_0030:  ldc.i4.0
      IL_0031:  stloc.s    V_8
      IL_0033:  br         IL_037c

      IL_0038:  ldloc.s    V_7
      IL_003a:  ldloc.s    V_8
      IL_003c:  ldelem.ref
      IL_003d:  stloc.1
      IL_003e:  nop
      IL_003f:  ldloc.1
      IL_0040:  ldc.i4.s   61
      IL_0042:  callvirt   instance int32 [mscorlib]System.String::IndexOf(char)
      IL_0047:  stloc.2
      IL_0048:  ldloc.2
      IL_0049:  ldc.i4.0
      IL_004a:  cgt
      IL_004c:  stloc.s    V_6
      IL_004e:  ldloc.s    V_6
      IL_0050:  brtrue.s   IL_0057

      IL_0052:  br         IL_0376

      IL_0057:  ldloc.1
      IL_0058:  ldc.i4.0
      IL_0059:  ldloc.2
      IL_005a:  callvirt   instance string [mscorlib]System.String::Substring(int32,
                                                                              int32)
      IL_005f:  callvirt   instance string [mscorlib]System.String::Trim()
      IL_0064:  stloc.3
      IL_0065:  ldloc.1
      IL_0066:  ldloc.2
      IL_0067:  ldc.i4.1
      IL_0068:  add
      IL_0069:  callvirt   instance string [mscorlib]System.String::Substring(int32)
      IL_006e:  callvirt   instance string [mscorlib]System.String::Trim()
      IL_0073:  stloc.s    V_4
      IL_0075:  ldloc.3
      IL_0076:  stloc.s    V_9
      IL_0078:  ldloc.s    V_9
      IL_007a:  brfalse    IL_0375

      IL_007f:  volatile.
      IL_0081:  ldsfld     class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32> '<PrivateImplementationDetails>{A122A232-08A8-4229-8112-BBBD06330825}'::'$$method0x6001275-1'
      IL_0086:  brtrue     IL_0160

      IL_008b:  ldc.i4.s   16
      IL_008d:  newobj     instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::.ctor(int32)
      IL_0092:  dup
      IL_0093:  ldstr      "Style"
      IL_0098:  ldc.i4.0
      IL_0099:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_009e:  dup
      IL_009f:  ldstr      "Layer"
      IL_00a4:  ldc.i4.1
      IL_00a5:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_00aa:  dup
      IL_00ab:  ldstr      "Height"
      IL_00b0:  ldc.i4.2
      IL_00b1:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_00b6:  dup
      IL_00b7:  ldstr      "Rotation"
      IL_00bc:  ldc.i4.3
      IL_00bd:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_00c2:  dup
      IL_00c3:  ldstr      "ArrowSize"
      IL_00c8:  ldc.i4.4
      IL_00c9:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_00ce:  dup
      IL_00cf:  ldstr      "Distance"
      IL_00d4:  ldc.i4.5
      IL_00d5:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_00da:  dup
      IL_00db:  ldstr      "Decimals"
      IL_00e0:  ldc.i4.6
      IL_00e1:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_00e6:  dup
      IL_00e7:  ldstr      "Corner"
      IL_00ec:  ldc.i4.7
      IL_00ed:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_00f2:  dup
      IL_00f3:  ldstr      "Vn2000"
      IL_00f8:  ldc.i4.8
      IL_00f9:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_00fe:  dup
      IL_00ff:  ldstr      "TwoLines"
      IL_0104:  ldc.i4.s   9
      IL_0106:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_010b:  dup
      IL_010c:  ldstr      "Mask"
      IL_0111:  ldc.i4.s   10
      IL_0113:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_0118:  dup
      IL_0119:  ldstr      "Frame"
      IL_011e:  ldc.i4.s   11
      IL_0120:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_0125:  dup
      IL_0126:  ldstr      "Arrow"
      IL_012b:  ldc.i4.s   12
      IL_012d:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_0132:  dup
      IL_0133:  ldstr      "Center"
      IL_0138:  ldc.i4.s   13
      IL_013a:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_013f:  dup
      IL_0140:  ldstr      "AddName"
      IL_0145:  ldc.i4.s   14
      IL_0147:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_014c:  dup
      IL_014d:  ldstr      "PickEach"
      IL_0152:  ldc.i4.s   15
      IL_0154:  call       instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::Add(!0,
                                                                                                                    !1)
      IL_0159:  volatile.
      IL_015b:  stsfld     class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32> '<PrivateImplementationDetails>{A122A232-08A8-4229-8112-BBBD06330825}'::'$$method0x6001275-1'
      IL_0160:  volatile.
      IL_0162:  ldsfld     class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32> '<PrivateImplementationDetails>{A122A232-08A8-4229-8112-BBBD06330825}'::'$$method0x6001275-1'
      IL_0167:  ldloc.s    V_9
      IL_0169:  ldloca.s   V_10
      IL_016b:  call       instance bool class [mscorlib]System.Collections.Generic.Dictionary`2<string,int32>::TryGetValue(!0,
                                                                                                                            !1&)
      IL_0170:  brfalse    IL_0375

      IL_0175:  ldloc.s    V_10
      IL_0177:  switch     ( 
                            IL_01c1,
                            IL_01e1,
                            IL_01ee,
                            IL_0218,
                            IL_0245,
                            IL_026f,
                            IL_0299,
                            IL_02c7,
                            IL_02f5,
                            IL_0305,
                            IL_0315,
                            IL_0325,
                            IL_0335,
                            IL_0345,
                            IL_0355,
                            IL_0365)
      IL_01bc:  br         IL_0375

      IL_01c1:  ldloc.s    V_4
      IL_01c3:  callvirt   instance int32 [mscorlib]System.String::get_Length()
      IL_01c8:  ldc.i4.0
      IL_01c9:  cgt
      IL_01cb:  ldc.i4.0
      IL_01cc:  ceq
      IL_01ce:  stloc.s    V_6
      IL_01d0:  ldloc.s    V_6
      IL_01d2:  brtrue.s   IL_01dc

      IL_01d4:  ldloc.0
      IL_01d5:  ldloc.s    V_4
      IL_01d7:  stfld      string Tvxd.Meps.ToadoSettings::Style
      IL_01dc:  br         IL_0375

      IL_01e1:  ldloc.0
      IL_01e2:  ldloc.s    V_4
      IL_01e4:  stfld      string Tvxd.Meps.ToadoSettings::Layer
      IL_01e9:  br         IL_0375

      IL_01ee:  ldloc.0
      IL_01ef:  ldloc.s    V_4
      IL_01f1:  ldloc.0
      IL_01f2:  ldfld      float64 Tvxd.Meps.ToadoSettings::Height
      IL_01f7:  ldc.r8     1.e-03
      IL_0200:  ldc.r8     100000.
      IL_0209:  call       float64 Tvxd.Meps.Toado100::D(string,
                                                         float64,
                                                         float64,
                                                         float64)
      IL_020e:  stfld      float64 Tvxd.Meps.ToadoSettings::Height
      IL_0213:  br         IL_0375

      IL_0218:  ldloc.0
      IL_0219:  ldloc.s    V_4
      IL_021b:  ldc.r8     0.0
      IL_0224:  ldc.r8     -360.
      IL_022d:  ldc.r8     360.
      IL_0236:  call       float64 Tvxd.Meps.Toado100::D(string,
                                                         float64,
                                                         float64,
                                                         float64)
      IL_023b:  stfld      float64 Tvxd.Meps.ToadoSettings::Rotation
      IL_0240:  br         IL_0375

      IL_0245:  ldloc.0
      IL_0246:  ldloc.s    V_4
      IL_0248:  ldloc.0
      IL_0249:  ldfld      float64 Tvxd.Meps.ToadoSettings::ArrowSize
      IL_024e:  ldc.r8     1.e-03
      IL_0257:  ldc.r8     100000.
      IL_0260:  call       float64 Tvxd.Meps.Toado100::D(string,
                                                         float64,
                                                         float64,
                                                         float64)
      IL_0265:  stfld      float64 Tvxd.Meps.ToadoSettings::ArrowSize
      IL_026a:  br         IL_0375

      IL_026f:  ldloc.0
      IL_0270:  ldloc.s    V_4
      IL_0272:  ldloc.0
      IL_0273:  ldfld      float64 Tvxd.Meps.ToadoSettings::Distance
      IL_0278:  ldc.r8     0.5
      IL_0281:  ldc.r8     200.
      IL_028a:  call       float64 Tvxd.Meps.Toado100::D(string,
                                                         float64,
                                                         float64,
                                                         float64)
      IL_028f:  stfld      float64 Tvxd.Meps.ToadoSettings::Distance
      IL_0294:  br         IL_0375

      IL_0299:  ldloc.0
      IL_029a:  ldloc.s    V_4
      IL_029c:  ldc.r8     3.
      IL_02a5:  ldc.r8     0.0
      IL_02ae:  ldc.r8     6.
      IL_02b7:  call       float64 Tvxd.Meps.Toado100::D(string,
                                                         float64,
                                                         float64,
                                                         float64)
      IL_02bc:  conv.i4
      IL_02bd:  stfld      int32 Tvxd.Meps.ToadoSettings::Decimals
      IL_02c2:  br         IL_0375

      IL_02c7:  ldloc.0
      IL_02c8:  ldloc.s    V_4
      IL_02ca:  ldc.r8     0.0
      IL_02d3:  ldc.r8     0.0
      IL_02dc:  ldc.r8     3.
      IL_02e5:  call       float64 Tvxd.Meps.Toado100::D(string,
                                                         float64,
                                                         float64,
                                                         float64)
      IL_02ea:  conv.i4
      IL_02eb:  stfld      int32 Tvxd.Meps.ToadoSettings::Corner
      IL_02f0:  br         IL_0375

      IL_02f5:  ldloc.0
      IL_02f6:  ldloc.s    V_4
      IL_02f8:  ldc.i4.1
      IL_02f9:  call       bool Tvxd.Meps.Toado100::B(string,
                                                      bool)
      IL_02fe:  stfld      bool Tvxd.Meps.ToadoSettings::Vn2000
      IL_0303:  br.s       IL_0375

      IL_0305:  ldloc.0
      IL_0306:  ldloc.s    V_4
      IL_0308:  ldc.i4.1
      IL_0309:  call       bool Tvxd.Meps.Toado100::B(string,
                                                      bool)
      IL_030e:  stfld      bool Tvxd.Meps.ToadoSettings::TwoLines
      IL_0313:  br.s       IL_0375

      IL_0315:  ldloc.0
      IL_0316:  ldloc.s    V_4
      IL_0318:  ldc.i4.1
      IL_0319:  call       bool Tvxd.Meps.Toado100::B(string,
                                                      bool)
      IL_031e:  stfld      bool Tvxd.Meps.ToadoSettings::Mask
      IL_0323:  br.s       IL_0375

      IL_0325:  ldloc.0
      IL_0326:  ldloc.s    V_4
      IL_0328:  ldc.i4.0
      IL_0329:  call       bool Tvxd.Meps.Toado100::B(string,
                                                      bool)
      IL_032e:  stfld      bool Tvxd.Meps.ToadoSettings::Frame
      IL_0333:  br.s       IL_0375

      IL_0335:  ldloc.0
      IL_0336:  ldloc.s    V_4
      IL_0338:  ldc.i4.1
      IL_0339:  call       bool Tvxd.Meps.Toado100::B(string,
                                                      bool)
      IL_033e:  stfld      bool Tvxd.Meps.ToadoSettings::Arrow
      IL_0343:  br.s       IL_0375

      IL_0345:  ldloc.0
      IL_0346:  ldloc.s    V_4
      IL_0348:  ldc.i4.0
      IL_0349:  call       bool Tvxd.Meps.Toado100::B(string,
                                                      bool)
      IL_034e:  stfld      bool Tvxd.Meps.ToadoSettings::Center
      IL_0353:  br.s       IL_0375

      IL_0355:  ldloc.0
      IL_0356:  ldloc.s    V_4
      IL_0358:  ldc.i4.0
      IL_0359:  call       bool Tvxd.Meps.Toado100::B(string,
                                                      bool)
      IL_035e:  stfld      bool Tvxd.Meps.ToadoSettings::AddName
      IL_0363:  br.s       IL_0375

      IL_0365:  ldloc.0
      IL_0366:  ldloc.s    V_4
      IL_0368:  ldc.i4.0
      IL_0369:  call       bool Tvxd.Meps.Toado100::B(string,
                                                      bool)
      IL_036e:  stfld      bool Tvxd.Meps.ToadoSettings::PickEach
      IL_0373:  br.s       IL_0375

      IL_0375:  nop
      IL_0376:  ldloc.s    V_8
      IL_0378:  ldc.i4.1
      IL_0379:  add
      IL_037a:  stloc.s    V_8
      IL_037c:  ldloc.s    V_8
      IL_037e:  ldloc.s    V_7
      IL_0380:  ldlen
      IL_0381:  conv.i4
      IL_0382:  clt
      IL_0384:  stloc.s    V_6
      IL_0386:  ldloc.s    V_6
      IL_0388:  brtrue     IL_0038

      IL_038d:  nop
      IL_038e:  leave.s    IL_0395

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_0390:  pop
      IL_0391:  nop
      IL_0392:  nop
      IL_0393:  leave.s    IL_0395

    }  // end handler
    IL_0395:  nop
    IL_0396:  ldloc.0
    IL_0397:  stloc.s    V_5
    IL_0399:  br.s       IL_039b

    IL_039b:  ldloc.s    V_5
    IL_039d:  ret
  } // end of method Toado100::Load

  .method public hidebysig static void  Save(class Tvxd.Meps.ToadoSettings s) cil managed
  {
    // Code size       537 (0x219)
    .maxstack  5
    .locals init (class [mscorlib]System.Collections.Generic.List`1<string> V_0,
             class [mscorlib]System.Collections.Generic.List`1<string> V_1)
    IL_0000:  nop
    .try
    {
      IL_0001:  nop
      IL_0002:  call       string Tvxd.Meps.Toado100::get_IniPath()
      IL_0007:  call       string [mscorlib]System.IO.Path::GetDirectoryName(string)
      IL_000c:  call       class [mscorlib]System.IO.DirectoryInfo [mscorlib]System.IO.Directory::CreateDirectory(string)
      IL_0011:  pop
      IL_0012:  newobj     instance void class [mscorlib]System.Collections.Generic.List`1<string>::.ctor()
      IL_0017:  stloc.1
      IL_0018:  ldloc.1
      IL_0019:  ldstr      "Style="
      IL_001e:  ldarg.0
      IL_001f:  ldfld      string Tvxd.Meps.ToadoSettings::Style
      IL_0024:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_0029:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_002e:  nop
      IL_002f:  ldloc.1
      IL_0030:  ldstr      "Layer="
      IL_0035:  ldarg.0
      IL_0036:  ldfld      string Tvxd.Meps.ToadoSettings::Layer
      IL_003b:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_0040:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_0045:  nop
      IL_0046:  ldloc.1
      IL_0047:  ldstr      "Height="
      IL_004c:  ldarg.0
      IL_004d:  ldflda     float64 Tvxd.Meps.ToadoSettings::Height
      IL_0052:  ldstr      "R"
      IL_0057:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
      IL_005c:  call       instance string [mscorlib]System.Double::ToString(string,
                                                                             class [mscorlib]System.IFormatProvider)
      IL_0061:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_0066:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_006b:  nop
      IL_006c:  ldloc.1
      IL_006d:  ldstr      "Rotation="
      IL_0072:  ldarg.0
      IL_0073:  ldflda     float64 Tvxd.Meps.ToadoSettings::Rotation
      IL_0078:  ldstr      "R"
      IL_007d:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
      IL_0082:  call       instance string [mscorlib]System.Double::ToString(string,
                                                                             class [mscorlib]System.IFormatProvider)
      IL_0087:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_008c:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_0091:  nop
      IL_0092:  ldloc.1
      IL_0093:  ldstr      "ArrowSize="
      IL_0098:  ldarg.0
      IL_0099:  ldflda     float64 Tvxd.Meps.ToadoSettings::ArrowSize
      IL_009e:  ldstr      "R"
      IL_00a3:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
      IL_00a8:  call       instance string [mscorlib]System.Double::ToString(string,
                                                                             class [mscorlib]System.IFormatProvider)
      IL_00ad:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_00b2:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_00b7:  nop
      IL_00b8:  ldloc.1
      IL_00b9:  ldstr      "Distance="
      IL_00be:  ldarg.0
      IL_00bf:  ldflda     float64 Tvxd.Meps.ToadoSettings::Distance
      IL_00c4:  ldstr      "R"
      IL_00c9:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
      IL_00ce:  call       instance string [mscorlib]System.Double::ToString(string,
                                                                             class [mscorlib]System.IFormatProvider)
      IL_00d3:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_00d8:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_00dd:  nop
      IL_00de:  ldloc.1
      IL_00df:  ldstr      "Decimals="
      IL_00e4:  ldarg.0
      IL_00e5:  ldfld      int32 Tvxd.Meps.ToadoSettings::Decimals
      IL_00ea:  box        [mscorlib]System.Int32
      IL_00ef:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_00f4:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_00f9:  nop
      IL_00fa:  ldloc.1
      IL_00fb:  ldstr      "Corner="
      IL_0100:  ldarg.0
      IL_0101:  ldfld      int32 Tvxd.Meps.ToadoSettings::Corner
      IL_0106:  box        [mscorlib]System.Int32
      IL_010b:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_0110:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_0115:  nop
      IL_0116:  ldloc.1
      IL_0117:  ldstr      "Vn2000="
      IL_011c:  ldarg.0
      IL_011d:  ldfld      bool Tvxd.Meps.ToadoSettings::Vn2000
      IL_0122:  box        [mscorlib]System.Boolean
      IL_0127:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_012c:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_0131:  nop
      IL_0132:  ldloc.1
      IL_0133:  ldstr      "TwoLines="
      IL_0138:  ldarg.0
      IL_0139:  ldfld      bool Tvxd.Meps.ToadoSettings::TwoLines
      IL_013e:  box        [mscorlib]System.Boolean
      IL_0143:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_0148:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_014d:  nop
      IL_014e:  ldloc.1
      IL_014f:  ldstr      "Mask="
      IL_0154:  ldarg.0
      IL_0155:  ldfld      bool Tvxd.Meps.ToadoSettings::Mask
      IL_015a:  box        [mscorlib]System.Boolean
      IL_015f:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_0164:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_0169:  nop
      IL_016a:  ldloc.1
      IL_016b:  ldstr      "Frame="
      IL_0170:  ldarg.0
      IL_0171:  ldfld      bool Tvxd.Meps.ToadoSettings::Frame
      IL_0176:  box        [mscorlib]System.Boolean
      IL_017b:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_0180:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_0185:  nop
      IL_0186:  ldloc.1
      IL_0187:  ldstr      "Arrow="
      IL_018c:  ldarg.0
      IL_018d:  ldfld      bool Tvxd.Meps.ToadoSettings::Arrow
      IL_0192:  box        [mscorlib]System.Boolean
      IL_0197:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_019c:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_01a1:  nop
      IL_01a2:  ldloc.1
      IL_01a3:  ldstr      "Center="
      IL_01a8:  ldarg.0
      IL_01a9:  ldfld      bool Tvxd.Meps.ToadoSettings::Center
      IL_01ae:  box        [mscorlib]System.Boolean
      IL_01b3:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_01b8:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_01bd:  nop
      IL_01be:  ldloc.1
      IL_01bf:  ldstr      "AddName="
      IL_01c4:  ldarg.0
      IL_01c5:  ldfld      bool Tvxd.Meps.ToadoSettings::AddName
      IL_01ca:  box        [mscorlib]System.Boolean
      IL_01cf:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_01d4:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_01d9:  nop
      IL_01da:  ldloc.1
      IL_01db:  ldstr      "PickEach="
      IL_01e0:  ldarg.0
      IL_01e1:  ldfld      bool Tvxd.Meps.ToadoSettings::PickEach
      IL_01e6:  box        [mscorlib]System.Boolean
      IL_01eb:  call       string [mscorlib]System.String::Concat(object,
                                                                  object)
      IL_01f0:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_01f5:  nop
      IL_01f6:  ldloc.1
      IL_01f7:  stloc.0
      IL_01f8:  call       string Tvxd.Meps.Toado100::get_IniPath()
      IL_01fd:  ldloc.0
      IL_01fe:  callvirt   instance !0[] class [mscorlib]System.Collections.Generic.List`1<string>::ToArray()
      IL_0203:  ldc.i4.0
      IL_0204:  newobj     instance void [mscorlib]System.Text.UTF8Encoding::.ctor(bool)
      IL_0209:  call       void [mscorlib]System.IO.File::WriteAllLines(string,
                                                                        string[],
                                                                        class [mscorlib]System.Text.Encoding)
      IL_020e:  nop
      IL_020f:  nop
      IL_0210:  leave.s    IL_0217

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_0212:  pop
      IL_0213:  nop
      IL_0214:  nop
      IL_0215:  leave.s    IL_0217

    }  // end handler
    IL_0217:  nop
    IL_0218:  ret
  } // end of method Toado100::Save

  .method private hidebysig static string 
          Esc(string t) cil managed
  {
    // Code size       91 (0x5b)
    .maxstack  3
    .locals init (string V_0)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  dup
    IL_0003:  brtrue.s   IL_000b

    IL_0005:  pop
    IL_0006:  ldstr      ""
    IL_000b:  ldstr      "\\"
    IL_0010:  ldstr      "\\\\"
    IL_0015:  callvirt   instance string [mscorlib]System.String::Replace(string,
                                                                          string)
    IL_001a:  ldstr      "{"
    IL_001f:  ldstr      "\\{"
    IL_0024:  callvirt   instance string [mscorlib]System.String::Replace(string,
                                                                          string)
    IL_0029:  ldstr      "}"
    IL_002e:  ldstr      "\\}"
    IL_0033:  callvirt   instance string [mscorlib]System.String::Replace(string,
                                                                          string)
    IL_0038:  ldstr      "\r"
    IL_003d:  ldstr      " "
    IL_0042:  callvirt   instance string [mscorlib]System.String::Replace(string,
                                                                          string)
    IL_0047:  ldstr      "\n"
    IL_004c:  ldstr      " "
    IL_0051:  callvirt   instance string [mscorlib]System.String::Replace(string,
                                                                          string)
    IL_0056:  stloc.0
    IL_0057:  br.s       IL_0059

    IL_0059:  ldloc.0
    IL_005a:  ret
  } // end of method Toado100::Esc

  .method public hidebysig static string 
          Format(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d p,
                 class Tvxd.Meps.ToadoSettings s,
                 string name,
                 bool oneLine) cil managed
  {
    // Code size       241 (0xf1)
    .maxstack  4
    .locals init (float64 V_0,
             float64 V_1,
             string V_2,
             string V_3,
             string V_4,
             string V_5,
             string V_6,
             bool V_7)
    IL_0000:  nop
    IL_0001:  ldarg.1
    IL_0002:  ldfld      bool Tvxd.Meps.ToadoSettings::Vn2000
    IL_0007:  brtrue.s   IL_0012

    IL_0009:  ldarga.s   p
    IL_000b:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
    IL_0010:  br.s       IL_0019

    IL_0012:  ldarga.s   p
    IL_0014:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
    IL_0019:  nop
    IL_001a:  stloc.0
    IL_001b:  ldarg.1
    IL_001c:  ldfld      bool Tvxd.Meps.ToadoSettings::Vn2000
    IL_0021:  brtrue.s   IL_002c

    IL_0023:  ldarga.s   p
    IL_0025:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
    IL_002a:  br.s       IL_0033

    IL_002c:  ldarga.s   p
    IL_002e:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
    IL_0033:  nop
    IL_0034:  stloc.1
    IL_0035:  ldarg.1
    IL_0036:  ldfld      int32 Tvxd.Meps.ToadoSettings::Decimals
    IL_003b:  ldc.i4.0
    IL_003c:  ble.s      IL_0057

    IL_003e:  ldstr      "0."
    IL_0043:  ldc.i4.s   48
    IL_0045:  ldarg.1
    IL_0046:  ldfld      int32 Tvxd.Meps.ToadoSettings::Decimals
    IL_004b:  newobj     instance void [mscorlib]System.String::.ctor(char,
                                                                      int32)
    IL_0050:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_0055:  br.s       IL_005c

    IL_0057:  ldstr      "0"
    IL_005c:  nop
    IL_005d:  stloc.2
    IL_005e:  ldstr      "X="
    IL_0063:  ldloca.s   V_0
    IL_0065:  ldloc.2
    IL_0066:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
    IL_006b:  call       instance string [mscorlib]System.Double::ToString(string,
                                                                           class [mscorlib]System.IFormatProvider)
    IL_0070:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_0075:  stloc.3
    IL_0076:  ldstr      "Y="
    IL_007b:  ldloca.s   V_1
    IL_007d:  ldloc.2
    IL_007e:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
    IL_0083:  call       instance string [mscorlib]System.Double::ToString(string,
                                                                           class [mscorlib]System.IFormatProvider)
    IL_0088:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_008d:  stloc.s    V_4
    IL_008f:  ldarg.1
    IL_0090:  ldfld      bool Tvxd.Meps.ToadoSettings::TwoLines
    IL_0095:  brfalse.s  IL_009a

    IL_0097:  ldarg.3
    IL_0098:  brfalse.s  IL_00a9

    IL_009a:  ldloc.3
    IL_009b:  ldstr      ", "
    IL_00a0:  ldloc.s    V_4
    IL_00a2:  call       string [mscorlib]System.String::Concat(string,
                                                                string,
                                                                string)
    IL_00a7:  br.s       IL_00b6

    IL_00a9:  ldloc.3
    IL_00aa:  ldstr      "\\P"
    IL_00af:  ldloc.s    V_4
    IL_00b1:  call       string [mscorlib]System.String::Concat(string,
                                                                string,
                                                                string)
    IL_00b6:  nop
    IL_00b7:  stloc.s    V_5
    IL_00b9:  ldarg.1
    IL_00ba:  ldfld      bool Tvxd.Meps.ToadoSettings::AddName
    IL_00bf:  brfalse.s  IL_00cc

    IL_00c1:  ldarg.3
    IL_00c2:  brtrue.s   IL_00cc

    IL_00c4:  ldarg.2
    IL_00c5:  call       bool [mscorlib]System.String::IsNullOrEmpty(string)
    IL_00ca:  br.s       IL_00cd

    IL_00cc:  ldc.i4.1
    IL_00cd:  nop
    IL_00ce:  stloc.s    V_7
    IL_00d0:  ldloc.s    V_7
    IL_00d2:  brtrue.s   IL_00e8

    IL_00d4:  ldarg.2
    IL_00d5:  call       string Tvxd.Meps.Toado100::Esc(string)
    IL_00da:  ldstr      "\\P"
    IL_00df:  ldloc.s    V_5
    IL_00e1:  call       string [mscorlib]System.String::Concat(string,
                                                                string,
                                                                string)
    IL_00e6:  stloc.s    V_5
    IL_00e8:  ldloc.s    V_5
    IL_00ea:  stloc.s    V_6
    IL_00ec:  br.s       IL_00ee

    IL_00ee:  ldloc.s    V_6
    IL_00f0:  ret
  } // end of method Toado100::Format

  .method private hidebysig static string 
          NameOf(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction tr,
                 class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference 'br') cil managed
  {
    // Code size       271 (0x10f)
    .maxstack  3
    .locals init (valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_0,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.AttributeReference V_1,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord V_2,
             string V_3,
             class [mscorlib]System.Collections.IEnumerator V_4,
             string[] V_5,
             bool V_6,
             class [mscorlib]System.IDisposable V_7)
    IL_0000:  nop
    IL_0001:  nop
    IL_0002:  ldarg.1
    IL_0003:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.AttributeCollection [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference::get_AttributeCollection()
    IL_0008:  callvirt   instance class [mscorlib]System.Collections.IEnumerator [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.AttributeCollection::GetEnumerator()
    IL_000d:  stloc.s    V_4
    .try
    {
      IL_000f:  br         IL_00af

      IL_0014:  ldloc.s    V_4
      IL_0016:  callvirt   instance object [mscorlib]System.Collections.IEnumerator::get_Current()
      IL_001b:  unbox.any  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId
      IL_0020:  stloc.0
      IL_0021:  nop
      IL_0022:  ldarg.0
      IL_0023:  ldloc.0
      IL_0024:  ldc.i4.0
      IL_0025:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
      IL_002a:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.AttributeReference
      IL_002f:  stloc.1
      IL_0030:  ldc.i4.7
      IL_0031:  newarr     [mscorlib]System.String
      IL_0036:  stloc.s    V_5
      IL_0038:  ldloc.s    V_5
      IL_003a:  ldc.i4.0
      IL_003b:  ldstr      "TEN_COT"
      IL_0040:  stelem.ref
      IL_0041:  ldloc.s    V_5
      IL_0043:  ldc.i4.1
      IL_0044:  ldstr      "TEN_TU"
      IL_0049:  stelem.ref
      IL_004a:  ldloc.s    V_5
      IL_004c:  ldc.i4.2
      IL_004d:  ldstr      "TEN_TRAM"
      IL_0052:  stelem.ref
      IL_0053:  ldloc.s    V_5
      IL_0055:  ldc.i4.3
      IL_0056:  ldstr      "COT_DIEN"
      IL_005b:  stelem.ref
      IL_005c:  ldloc.s    V_5
      IL_005e:  ldc.i4.4
      IL_005f:  ldstr      "MA_TB"
      IL_0064:  stelem.ref
      IL_0065:  ldloc.s    V_5
      IL_0067:  ldc.i4.5
      IL_0068:  ldstr      "TAG"
      IL_006d:  stelem.ref
      IL_006e:  ldloc.s    V_5
      IL_0070:  ldc.i4.6
      IL_0071:  ldstr      "TAG-1"
      IL_0076:  stelem.ref
      IL_0077:  ldloc.s    V_5
      IL_0079:  ldloc.1
      IL_007a:  callvirt   instance string [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.AttributeReference::get_Tag()
      IL_007f:  call       class [mscorlib]System.StringComparer [mscorlib]System.StringComparer::get_OrdinalIgnoreCase()
      IL_0084:  call       bool [System.Core]System.Linq.Enumerable::Contains<string>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                      !!0,
                                                                                      class [mscorlib]System.Collections.Generic.IEqualityComparer`1<!!0>)
      IL_0089:  brfalse.s  IL_0098

      IL_008b:  ldloc.1
      IL_008c:  callvirt   instance string [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBText::get_TextString()
      IL_0091:  call       bool [mscorlib]System.String::IsNullOrWhiteSpace(string)
      IL_0096:  br.s       IL_0099

      IL_0098:  ldc.i4.1
      IL_0099:  nop
      IL_009a:  stloc.s    V_6
      IL_009c:  ldloc.s    V_6
      IL_009e:  brtrue.s   IL_00ae

      IL_00a0:  ldloc.1
      IL_00a1:  callvirt   instance string [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBText::get_TextString()
      IL_00a6:  callvirt   instance string [mscorlib]System.String::Trim()
      IL_00ab:  stloc.3
      IL_00ac:  leave.s    IL_010c

      IL_00ae:  nop
      IL_00af:  ldloc.s    V_4
      IL_00b1:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
      IL_00b6:  stloc.s    V_6
      IL_00b8:  ldloc.s    V_6
      IL_00ba:  brtrue     IL_0014

      IL_00bf:  leave.s    IL_00de

    }  // end .try
    finally
    {
      IL_00c1:  ldloc.s    V_4
      IL_00c3:  isinst     [mscorlib]System.IDisposable
      IL_00c8:  stloc.s    V_7
      IL_00ca:  ldloc.s    V_7
      IL_00cc:  ldnull
      IL_00cd:  ceq
      IL_00cf:  stloc.s    V_6
      IL_00d1:  ldloc.s    V_6
      IL_00d3:  brtrue.s   IL_00dd

      IL_00d5:  ldloc.s    V_7
      IL_00d7:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_00dc:  nop
      IL_00dd:  endfinally
    }  // end handler
    IL_00de:  nop
    IL_00df:  ldarg.0
    IL_00e0:  ldarg.1
    IL_00e1:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference::get_IsDynamicBlock()
    IL_00e6:  brtrue.s   IL_00f0

    IL_00e8:  ldarg.1
    IL_00e9:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference::get_BlockTableRecord()
    IL_00ee:  br.s       IL_00f6

    IL_00f0:  ldarg.1
    IL_00f1:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference::get_DynamicBlockTableRecord()
    IL_00f6:  nop
    IL_00f7:  ldc.i4.0
    IL_00f8:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                              valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
    IL_00fd:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord
    IL_0102:  stloc.2
    IL_0103:  ldloc.2
    IL_0104:  callvirt   instance string [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord::get_Name()
    IL_0109:  stloc.3
    IL_010a:  br.s       IL_010c

    IL_010c:  nop
    IL_010d:  ldloc.3
    IL_010e:  ret
  } // end of method Toado100::NameOf

  .method private hidebysig static valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d 
          Tip(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference 'br',
              bool center) cil managed
  {
    // Code size       141 (0x8d)
    .maxstack  3
    .locals init (valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Extents3d V_0,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_1,
             bool V_2,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_3)
    IL_0000:  nop
    IL_0001:  ldarg.1
    IL_0002:  ldc.i4.0
    IL_0003:  ceq
    IL_0005:  stloc.2
    IL_0006:  ldloc.2
    IL_0007:  brtrue.s   IL_0081

    IL_0009:  nop
    .try
    {
      IL_000a:  nop
      IL_000b:  ldarg.0
      IL_000c:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Extents3d [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Entity::get_GeometricExtents()
      IL_0011:  stloc.0
      IL_0012:  ldloca.s   V_0
      IL_0014:  call       instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Extents3d::get_MinPoint()
      IL_0019:  stloc.3
      IL_001a:  ldloca.s   V_3
      IL_001c:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
      IL_0021:  ldloca.s   V_0
      IL_0023:  call       instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Extents3d::get_MaxPoint()
      IL_0028:  stloc.3
      IL_0029:  ldloca.s   V_3
      IL_002b:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
      IL_0030:  add
      IL_0031:  ldc.r8     2.
      IL_003a:  div
      IL_003b:  ldloca.s   V_0
      IL_003d:  call       instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Extents3d::get_MinPoint()
      IL_0042:  stloc.3
      IL_0043:  ldloca.s   V_3
      IL_0045:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
      IL_004a:  ldloca.s   V_0
      IL_004c:  call       instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Extents3d::get_MaxPoint()
      IL_0051:  stloc.3
      IL_0052:  ldloca.s   V_3
      IL_0054:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
      IL_0059:  add
      IL_005a:  ldc.r8     2.
      IL_0063:  div
      IL_0064:  ldarg.0
      IL_0065:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference::get_Position()
      IL_006a:  stloc.3
      IL_006b:  ldloca.s   V_3
      IL_006d:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Z()
      IL_0072:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::.ctor(float64,
                                                                                           float64,
                                                                                           float64)
      IL_0077:  stloc.1
      IL_0078:  leave.s    IL_008a

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_007a:  pop
      IL_007b:  nop
      IL_007c:  nop
      IL_007d:  leave.s    IL_007f

    }  // end handler
    IL_007f:  nop
    IL_0080:  nop
    IL_0081:  ldarg.0
    IL_0082:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference::get_Position()
    IL_0087:  stloc.1
    IL_0088:  br.s       IL_008a

    IL_008a:  nop
    IL_008b:  ldloc.1
    IL_008c:  ret
  } // end of method Toado100::Tip

  .method private hidebysig static valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId 
          EnsureLayer(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction tr,
                      class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db,
                      string name) cil managed
  {
    // Code size       239 (0xef)
    .maxstack  3
    .locals init (class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTable V_0,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTableRecord V_1,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTableRecord V_2,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_3,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTableRecord V_4,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_5,
             bool V_6)
    IL_0000:  nop
    IL_0001:  ldarg.2
    IL_0002:  call       bool [mscorlib]System.String::IsNullOrWhiteSpace(string)
    IL_0007:  ldc.i4.0
    IL_0008:  ceq
    IL_000a:  stloc.s    V_6
    IL_000c:  ldloc.s    V_6
    IL_000e:  brtrue.s   IL_001d

    IL_0010:  ldarg.1
    IL_0011:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_Clayer()
    IL_0016:  stloc.s    V_5
    IL_0018:  br         IL_00ec

    IL_001d:  ldarg.2
    IL_001e:  callvirt   instance string [mscorlib]System.String::Trim()
    IL_0023:  starg.s    name
    IL_0025:  ldarg.0
    IL_0026:  ldarg.1
    IL_0027:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_LayerTableId()
    IL_002c:  ldc.i4.0
    IL_002d:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                              valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
    IL_0032:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTable
    IL_0037:  stloc.0
    IL_0038:  ldloc.0
    IL_0039:  ldarg.2
    IL_003a:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::Has(string)
    IL_003f:  ldc.i4.0
    IL_0040:  ceq
    IL_0042:  stloc.s    V_6
    IL_0044:  ldloc.s    V_6
    IL_0046:  brtrue.s   IL_00a1

    IL_0048:  nop
    IL_0049:  ldarg.0
    IL_004a:  ldloc.0
    IL_004b:  ldarg.2
    IL_004c:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::get_Item(string)
    IL_0051:  ldc.i4.0
    IL_0052:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                              valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
    IL_0057:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTableRecord
    IL_005c:  stloc.1
    IL_005d:  ldloc.1
    IL_005e:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTableRecord::get_IsLocked()
    IL_0063:  brtrue.s   IL_0078

    IL_0065:  ldloc.1
    IL_0066:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTableRecord::get_IsFrozen()
    IL_006b:  brtrue.s   IL_0078

    IL_006d:  ldloc.1
    IL_006e:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTableRecord::get_IsOff()
    IL_0073:  ldc.i4.0
    IL_0074:  ceq
    IL_0076:  br.s       IL_0079

    IL_0078:  ldc.i4.0
    IL_0079:  nop
    IL_007a:  stloc.s    V_6
    IL_007c:  ldloc.s    V_6
    IL_007e:  brtrue.s   IL_0096

    IL_0080:  ldstr      "Layer "
    IL_0085:  ldarg.2
    IL_0086:  ldstr      bytearray (20 00 11 01 61 00 6E 00 67 00 20 00 6B 00 68 00   //  ...a.n.g. .k.h.
                                    F3 00 61 00 2F 00 74 00 AF 1E 74 00 2F 00 11 01   // ..a./.t...t./...
                                    F3 00 6E 00 67 00 20 00 62 00 03 01 6E 00 67 00   // ..n.g. .b...n.g.
                                    2E 00 20 00 4D 00 DF 1E 20 00 6C 00 61 00 79 00   // .. .M... .l.a.y.
                                    65 00 72 00 20 00 68 00 6F 00 B7 1E 63 00 20 00   // e.r. .h.o...c. .
                                    63 00 68 00 CD 1E 6E 00 20 00 6C 00 61 00 79 00   // c.h...n. .l.a.y.
                                    65 00 72 00 20 00 6B 00 68 00 E1 00 63 00 2E 00 ) // e.r. .k.h...c...
    IL_008b:  call       string [mscorlib]System.String::Concat(string,
                                                                string,
                                                                string)
    IL_0090:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_0095:  throw

    IL_0096:  ldloc.0
    IL_0097:  ldarg.2
    IL_0098:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::get_Item(string)
    IL_009d:  stloc.s    V_5
    IL_009f:  br.s       IL_00ec

    IL_00a1:  ldarg.2
    IL_00a2:  ldc.i4.0
    IL_00a3:  call       void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolUtilityServices::ValidateSymbolName(string,
                                                                                                                   bool)
    IL_00a8:  nop
    IL_00a9:  ldloc.0
    IL_00aa:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::UpgradeOpen()
    IL_00af:  nop
    IL_00b0:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTableRecord::.ctor()
    IL_00b5:  stloc.s    V_4
    IL_00b7:  ldloc.s    V_4
    IL_00b9:  ldarg.2
    IL_00ba:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord::set_Name(string)
    IL_00bf:  nop
    IL_00c0:  ldloc.s    V_4
    IL_00c2:  ldc.i4     0xc3
    IL_00c7:  ldc.i4.2
    IL_00c8:  call       class [Acdbmgd]Autodesk.AutoCAD.Colors.Color [Acdbmgd]Autodesk.AutoCAD.Colors.Color::FromColorIndex(valuetype [Acdbmgd]Autodesk.AutoCAD.Colors.ColorMethod,
                                                                                                                             int16)
    IL_00cd:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.LayerTableRecord::set_Color(class [Acdbmgd]Autodesk.AutoCAD.Colors.Color)
    IL_00d2:  nop
    IL_00d3:  ldloc.s    V_4
    IL_00d5:  stloc.2
    IL_00d6:  ldloc.0
    IL_00d7:  ldloc.2
    IL_00d8:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::Add(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord)
    IL_00dd:  stloc.3
    IL_00de:  ldarg.0
    IL_00df:  ldloc.2
    IL_00e0:  ldc.i4.1
    IL_00e1:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::AddNewlyCreatedDBObject(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject,
                                                                                                                       bool)
    IL_00e6:  nop
    IL_00e7:  ldloc.3
    IL_00e8:  stloc.s    V_5
    IL_00ea:  br.s       IL_00ec

    IL_00ec:  ldloc.s    V_5
    IL_00ee:  ret
  } // end of method Toado100::EnsureLayer

  .method private hidebysig static void  EnsureRegApp(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction tr,
                                                      class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db) cil managed
  {
    // Code size       85 (0x55)
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
    IL_001f:  ldc.i4.0
    IL_0020:  ceq
    IL_0022:  stloc.3
    IL_0023:  ldloc.3
    IL_0024:  brtrue.s   IL_0028

    IL_0026:  br.s       IL_0054

    IL_0028:  ldloc.0
    IL_0029:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::UpgradeOpen()
    IL_002e:  nop
    IL_002f:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.RegAppTableRecord::.ctor()
    IL_0034:  stloc.2
    IL_0035:  ldloc.2
    IL_0036:  ldstr      "TVXD_TOADO"
    IL_003b:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord::set_Name(string)
    IL_0040:  nop
    IL_0041:  ldloc.2
    IL_0042:  stloc.1
    IL_0043:  ldloc.0
    IL_0044:  ldloc.1
    IL_0045:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::Add(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord)
    IL_004a:  pop
    IL_004b:  ldarg.0
    IL_004c:  ldloc.1
    IL_004d:  ldc.i4.1
    IL_004e:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::AddNewlyCreatedDBObject(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject,
                                                                                                                       bool)
    IL_0053:  nop
    IL_0054:  ret
  } // end of method Toado100::EnsureRegApp

  .method private hidebysig static valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId 
          StyleId(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction tr,
                  class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db,
                  string name,
                  class [mscorlib]System.Collections.Generic.List`1<string> warnings) cil managed
  {
    // Code size       94 (0x5e)
    .maxstack  3
    .locals init (class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTable V_0,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_1,
             bool V_2)
    IL_0000:  nop
    IL_0001:  ldarg.2
    IL_0002:  call       bool [mscorlib]System.String::IsNullOrEmpty(string)
    IL_0007:  brtrue.s   IL_001a

    IL_0009:  ldarg.2
    IL_000a:  ldstr      "Tuan_arial"
    IL_000f:  ldc.i4.5
    IL_0010:  call       bool [mscorlib]System.String::Equals(string,
                                                              string,
                                                              valuetype [mscorlib]System.StringComparison)
    IL_0015:  ldc.i4.0
    IL_0016:  ceq
    IL_0018:  br.s       IL_001b

    IL_001a:  ldc.i4.0
    IL_001b:  nop
    IL_001c:  stloc.2
    IL_001d:  ldloc.2
    IL_001e:  brtrue.s   IL_002b

    IL_0020:  ldarg.1
    IL_0021:  ldarg.0
    IL_0022:  ldarg.3
    IL_0023:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.LegendStandard099::TextStyle(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                              class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                              class [mscorlib]System.Collections.Generic.List`1<string>)
    IL_0028:  stloc.1
    IL_0029:  br.s       IL_005c

    IL_002b:  ldarg.0
    IL_002c:  ldarg.1
    IL_002d:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TextStyleTableId()
    IL_0032:  ldc.i4.0
    IL_0033:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                              valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
    IL_0038:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTable
    IL_003d:  stloc.0
    IL_003e:  ldloc.0
    IL_003f:  ldarg.2
    IL_0040:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::Has(string)
    IL_0045:  brtrue.s   IL_0051

    IL_0047:  ldarg.1
    IL_0048:  ldarg.0
    IL_0049:  ldarg.3
    IL_004a:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.LegendStandard099::TextStyle(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                              class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                              class [mscorlib]System.Collections.Generic.List`1<string>)
    IL_004f:  br.s       IL_0058

    IL_0051:  ldloc.0
    IL_0052:  ldarg.2
    IL_0053:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::get_Item(string)
    IL_0058:  nop
    IL_0059:  stloc.1
    IL_005a:  br.s       IL_005c

    IL_005c:  ldloc.1
    IL_005d:  ret
  } // end of method Toado100::StyleId

  .method public hidebysig static class [mscorlib]System.Collections.Generic.List`1<string> 
          StyleNames(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db) cil managed
  {
    // Code size       192 (0xc0)
    .maxstack  3
    .locals init (class [mscorlib]System.Collections.Generic.List`1<string> V_0,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction V_1,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_2,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTableRecord V_3,
             class [mscorlib]System.Collections.Generic.List`1<string> V_4,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableEnumerator V_5,
             bool V_6)
    IL_0000:  nop
    IL_0001:  newobj     instance void class [mscorlib]System.Collections.Generic.List`1<string>::.ctor()
    IL_0006:  stloc.0
    IL_0007:  ldarg.0
    IL_0008:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TransactionManager()
    IL_000d:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartTransaction()
    IL_0012:  stloc.1
    .try
    {
      IL_0013:  nop
      IL_0014:  nop
      IL_0015:  ldloc.1
      IL_0016:  ldarg.0
      IL_0017:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TextStyleTableId()
      IL_001c:  ldc.i4.0
      IL_001d:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
      IL_0022:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTable
      IL_0027:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableEnumerator [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTable::GetEnumerator()
      IL_002c:  stloc.s    V_5
      .try
      {
        IL_002e:  br.s       IL_0072

        IL_0030:  ldloc.s    V_5
        IL_0032:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableEnumerator::get_Current()
        IL_0037:  stloc.2
        IL_0038:  nop
        IL_0039:  ldloc.1
        IL_003a:  ldloc.2
        IL_003b:  ldc.i4.0
        IL_003c:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
        IL_0041:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextStyleTableRecord
        IL_0046:  stloc.3
        IL_0047:  ldloc.3
        IL_0048:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord::get_IsDependent()
        IL_004d:  brtrue.s   IL_005c

        IL_004f:  ldloc.3
        IL_0050:  callvirt   instance string [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord::get_Name()
        IL_0055:  call       bool [mscorlib]System.String::IsNullOrEmpty(string)
        IL_005a:  br.s       IL_005d

        IL_005c:  ldc.i4.1
        IL_005d:  nop
        IL_005e:  stloc.s    V_6
        IL_0060:  ldloc.s    V_6
        IL_0062:  brtrue.s   IL_0071

        IL_0064:  ldloc.0
        IL_0065:  ldloc.3
        IL_0066:  callvirt   instance string [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableRecord::get_Name()
        IL_006b:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
        IL_0070:  nop
        IL_0071:  nop
        IL_0072:  ldloc.s    V_5
        IL_0074:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolTableEnumerator::MoveNext()
        IL_0079:  stloc.s    V_6
        IL_007b:  ldloc.s    V_6
        IL_007d:  brtrue.s   IL_0030

        IL_007f:  leave.s    IL_0095

      }  // end .try
      finally
      {
        IL_0081:  ldloc.s    V_5
        IL_0083:  ldnull
        IL_0084:  ceq
        IL_0086:  stloc.s    V_6
        IL_0088:  ldloc.s    V_6
        IL_008a:  brtrue.s   IL_0094

        IL_008c:  ldloc.s    V_5
        IL_008e:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_0093:  nop
        IL_0094:  endfinally
      }  // end handler
      IL_0095:  nop
      IL_0096:  nop
      IL_0097:  leave.s    IL_00ab

    }  // end .try
    finally
    {
      IL_0099:  ldloc.1
      IL_009a:  ldnull
      IL_009b:  ceq
      IL_009d:  stloc.s    V_6
      IL_009f:  ldloc.s    V_6
      IL_00a1:  brtrue.s   IL_00aa

      IL_00a3:  ldloc.1
      IL_00a4:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_00a9:  nop
      IL_00aa:  endfinally
    }  // end handler
    IL_00ab:  nop
    IL_00ac:  ldloc.0
    IL_00ad:  call       class [mscorlib]System.StringComparer [mscorlib]System.StringComparer::get_OrdinalIgnoreCase()
    IL_00b2:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Sort(class [mscorlib]System.Collections.Generic.IComparer`1<!0>)
    IL_00b7:  nop
    IL_00b8:  ldloc.0
    IL_00b9:  stloc.s    V_4
    IL_00bb:  br.s       IL_00bd

    IL_00bd:  ldloc.s    V_4
    IL_00bf:  ret
  } // end of method Toado100::StyleNames

  .method private hidebysig static class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer 
          Tag(string handle,
              valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d pos) cil managed
  {
    // Code size       171 (0xab)
    .maxstack  3
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
    IL_0030:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::.ctor(int32,
                                                                                                    object)
    IL_0035:  stobj      [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_003a:  ldloc.1
    IL_003b:  ldc.i4.2
    IL_003c:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_0041:  ldc.i4     0x410
    IL_0046:  ldarga.s   pos
    IL_0048:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
    IL_004d:  box        [mscorlib]System.Double
    IL_0052:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::.ctor(int32,
                                                                                                    object)
    IL_0057:  stobj      [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_005c:  ldloc.1
    IL_005d:  ldc.i4.3
    IL_005e:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_0063:  ldc.i4     0x410
    IL_0068:  ldarga.s   pos
    IL_006a:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
    IL_006f:  box        [mscorlib]System.Double
    IL_0074:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::.ctor(int32,
                                                                                                    object)
    IL_0079:  stobj      [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_007e:  ldloc.1
    IL_007f:  ldc.i4.4
    IL_0080:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_0085:  ldc.i4     0x410
    IL_008a:  ldarga.s   pos
    IL_008c:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Z()
    IL_0091:  box        [mscorlib]System.Double
    IL_0096:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::.ctor(int32,
                                                                                                    object)
    IL_009b:  stobj      [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_00a0:  ldloc.1
    IL_00a1:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer::.ctor(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[])
    IL_00a6:  stloc.0
    IL_00a7:  br.s       IL_00a9

    IL_00a9:  ldloc.0
    IL_00aa:  ret
  } // end of method Toado100::Tag

  .method private hidebysig static void  Style(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader ml,
                                               class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText mt,
                                               class Tvxd.Meps.ToadoSettings s,
                                               valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId style) cil managed
  {
    // Code size       116 (0x74)
    .maxstack  3
    .locals init (bool V_0)
    IL_0000:  nop
    IL_0001:  ldarg.1
    IL_0002:  ldarg.3
    IL_0003:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_TextStyleId(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
    IL_0008:  nop
    IL_0009:  ldarg.1
    IL_000a:  ldarg.2
    IL_000b:  ldfld      float64 Tvxd.Meps.ToadoSettings::Height
    IL_0010:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_TextHeight(float64)
    IL_0015:  nop
    IL_0016:  ldarg.1
    IL_0017:  ldarg.2
    IL_0018:  ldfld      float64 Tvxd.Meps.ToadoSettings::Rotation
    IL_001d:  ldc.r8     3.1415926535897931
    IL_0026:  mul
    IL_0027:  ldc.r8     180.
    IL_0030:  div
    IL_0031:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_Rotation(float64)
    IL_0036:  nop
    IL_0037:  ldarg.1
    IL_0038:  ldc.i4.4
    IL_0039:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_Attachment(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.AttachmentPoint)
    IL_003e:  nop
    IL_003f:  ldarg.1
    IL_0040:  ldarg.2
    IL_0041:  ldfld      bool Tvxd.Meps.ToadoSettings::Mask
    IL_0046:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_BackgroundFill(bool)
    IL_004b:  nop
    IL_004c:  ldarg.2
    IL_004d:  ldfld      bool Tvxd.Meps.ToadoSettings::Mask
    IL_0052:  ldc.i4.0
    IL_0053:  ceq
    IL_0055:  stloc.0
    IL_0056:  ldloc.0
    IL_0057:  brtrue.s   IL_0073

    IL_0059:  nop
    IL_005a:  ldarg.1
    IL_005b:  ldc.i4.1
    IL_005c:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_UseBackgroundColor(bool)
    IL_0061:  nop
    IL_0062:  ldarg.1
    IL_0063:  ldc.r8     1.1499999999999999
    IL_006c:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_BackgroundScaleFactor(float64)
    IL_0071:  nop
    IL_0072:  nop
    IL_0073:  ret
  } // end of method Toado100::Style

  .method private hidebysig static void  LeaderLook(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader ml,
                                                    class Tvxd.Meps.ToadoSettings s) cil managed
  {
    // Code size       102 (0x66)
    .maxstack  3
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  ldfld      bool Tvxd.Meps.ToadoSettings::Frame
    IL_0008:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_EnableFrameText(bool)
    IL_000d:  nop
    IL_000e:  ldarg.0
    IL_000f:  ldarg.1
    IL_0010:  ldfld      bool Tvxd.Meps.ToadoSettings::Arrow
    IL_0015:  brtrue.s   IL_0022

    IL_0017:  ldc.r8     1.e-04
    IL_0020:  br.s       IL_0028

    IL_0022:  ldarg.1
    IL_0023:  ldfld      float64 Tvxd.Meps.ToadoSettings::ArrowSize
    IL_0028:  nop
    IL_0029:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_ArrowSize(float64)
    IL_002e:  nop
    IL_002f:  ldarg.0
    IL_0030:  ldc.i4.1
    IL_0031:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_EnableDogleg(bool)
    IL_0036:  nop
    IL_0037:  ldarg.0
    IL_0038:  ldarg.1
    IL_0039:  ldfld      float64 Tvxd.Meps.ToadoSettings::Height
    IL_003e:  ldc.r8     1.2
    IL_0047:  mul
    IL_0048:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_DoglegLength(float64)
    IL_004d:  nop
    IL_004e:  ldarg.0
    IL_004f:  ldarg.1
    IL_0050:  ldfld      float64 Tvxd.Meps.ToadoSettings::Height
    IL_0055:  ldc.r8     0.29999999999999999
    IL_005e:  mul
    IL_005f:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_LandingGap(float64)
    IL_0064:  nop
    IL_0065:  ret
  } // end of method Toado100::LeaderLook

  .method public hidebysig static class Tvxd.Meps.ToadoResult 
          Apply(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db,
                class [mscorlib]System.Collections.Generic.IEnumerable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> ids,
                class Tvxd.Meps.ToadoSettings s,
                class [mscorlib]System.Func`4<int32,int32,valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>> pick) cil managed
  {
    // Code size       1459 (0x5b3)
    .maxstack  4
    .locals init (class Tvxd.Meps.ToadoResult V_0,
             class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> V_1,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction V_2,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_3,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_4,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord V_5,
             class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> V_6,
             class [Acdbmgd]Autodesk.AutoCAD.Runtime.RXClass V_7,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_8,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject V_9,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer V_10,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[] V_11,
             int32 V_12,
             int32 V_13,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference V_14,
             string V_15,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_16,
             string V_17,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId V_18,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader V_19,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_20,
             bool V_21,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Vector3d V_22,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText V_23,
             bool V_24,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_25,
             valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d> V_26,
             float64 V_27,
             float64 V_28,
             float64 V_29,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_30,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader V_31,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText V_32,
             int32 V_33,
             int32 V_34,
             class Tvxd.Meps.ToadoResult V_35,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecordEnumerator V_36,
             bool V_37,
             valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId> V_38,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Handle V_39,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Vector3d V_40,
             valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d> V_41)
    IL_0000:  nop
    IL_0001:  newobj     instance void Tvxd.Meps.ToadoResult::.ctor()
    IL_0006:  stloc.0
    IL_0007:  ldarg.1
    IL_0008:  call       class [mscorlib]System.Collections.Generic.List`1<!!0> [System.Core]System.Linq.Enumerable::ToList<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_000d:  stloc.1
    IL_000e:  ldarg.0
    IL_000f:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TransactionManager()
    IL_0014:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartTransaction()
    IL_0019:  stloc.2
    .try
    {
      IL_001a:  nop
      IL_001b:  ldloc.2
      IL_001c:  ldarg.0
      IL_001d:  ldarg.2
      IL_001e:  ldfld      string Tvxd.Meps.ToadoSettings::Layer
      IL_0023:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::EnsureLayer(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                         class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                         string)
      IL_0028:  stloc.3
      IL_0029:  ldloc.2
      IL_002a:  ldarg.0
      IL_002b:  call       void Tvxd.Meps.Toado100::EnsureRegApp(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                 class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
      IL_0030:  nop
      IL_0031:  ldloc.2
      IL_0032:  ldarg.0
      IL_0033:  ldarg.2
      IL_0034:  ldfld      string Tvxd.Meps.ToadoSettings::Style
      IL_0039:  ldloc.0
      IL_003a:  ldfld      class [mscorlib]System.Collections.Generic.List`1<string> Tvxd.Meps.ToadoResult::warnings
      IL_003f:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.Toado100::StyleId(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                                                                                     class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                                                     string,
                                                                                                                     class [mscorlib]System.Collections.Generic.List`1<string>)
      IL_0044:  stloc.s    V_4
      IL_0046:  ldloc.2
      IL_0047:  ldarg.0
      IL_0048:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.SymbolUtilityServices::GetBlockModelSpaceId(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
      IL_004d:  ldc.i4.1
      IL_004e:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
      IL_0053:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord
      IL_0058:  stloc.s    V_5
      IL_005a:  newobj     instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::.ctor()
      IL_005f:  stloc.s    V_6
      IL_0061:  ldtoken    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader
      IL_0066:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
      IL_006b:  call       class [Acdbmgd]Autodesk.AutoCAD.Runtime.RXClass [Acdbmgd]Autodesk.AutoCAD.Runtime.RXObject::GetClass(class [mscorlib]System.Type)
      IL_0070:  stloc.s    V_7
      IL_0072:  nop
      IL_0073:  ldloc.s    V_5
      IL_0075:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecordEnumerator [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord::GetEnumerator()
      IL_007a:  stloc.s    V_36
      .try
      {
        IL_007c:  br         IL_0119

        IL_0081:  ldloc.s    V_36
        IL_0083:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecordEnumerator::get_Current()
        IL_0088:  stloc.s    V_8
        IL_008a:  nop
        IL_008b:  ldloca.s   V_8
        IL_008d:  call       instance class [Acdbmgd]Autodesk.AutoCAD.Runtime.RXClass [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId::get_ObjectClass()
        IL_0092:  ldloc.s    V_7
        IL_0094:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.Runtime.RXClass::IsDerivedFrom(class [Acdbmgd]Autodesk.AutoCAD.Runtime.RXClass)
        IL_0099:  stloc.s    V_37
        IL_009b:  ldloc.s    V_37
        IL_009d:  brtrue.s   IL_00a1

        IL_009f:  br.s       IL_0119

        IL_00a1:  ldloc.2
        IL_00a2:  ldloc.s    V_8
        IL_00a4:  ldc.i4.0
        IL_00a5:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
        IL_00aa:  stloc.s    V_9
        IL_00ac:  ldloc.s    V_9
        IL_00ae:  ldstr      "TVXD_TOADO"
        IL_00b3:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::GetXDataForApplication(string)
        IL_00b8:  stloc.s    V_10
        .try
        {
          IL_00ba:  nop
          IL_00bb:  ldloc.s    V_10
          IL_00bd:  ldnull
          IL_00be:  call       bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer::op_Equality(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer,
                                                                                                         class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer)
          IL_00c3:  ldc.i4.0
          IL_00c4:  ceq
          IL_00c6:  stloc.s    V_37
          IL_00c8:  ldloc.s    V_37
          IL_00ca:  brtrue.s   IL_00ce

          IL_00cc:  leave.s    IL_0119

          IL_00ce:  ldloc.s    V_10
          IL_00d0:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[] [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer::AsArray()
          IL_00d5:  stloc.s    V_11
          IL_00d7:  ldloc.s    V_11
          IL_00d9:  ldlen
          IL_00da:  conv.i4
          IL_00db:  ldc.i4.5
          IL_00dc:  clt
          IL_00de:  stloc.s    V_37
          IL_00e0:  ldloc.s    V_37
          IL_00e2:  brtrue.s   IL_0100

          IL_00e4:  ldloc.s    V_6
          IL_00e6:  ldloc.s    V_11
          IL_00e8:  ldc.i4.1
          IL_00e9:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
          IL_00ee:  call       instance object [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::get_Value()
          IL_00f3:  call       string [mscorlib]System.Convert::ToString(object)
          IL_00f8:  ldloc.s    V_8
          IL_00fa:  callvirt   instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::set_Item(!0,
                                                                                                                                                                                     !1)
          IL_00ff:  nop
          IL_0100:  nop
          IL_0101:  leave.s    IL_0117

        }  // end .try
        finally
        {
          IL_0103:  ldloc.s    V_10
          IL_0105:  ldnull
          IL_0106:  ceq
          IL_0108:  stloc.s    V_37
          IL_010a:  ldloc.s    V_37
          IL_010c:  brtrue.s   IL_0116

          IL_010e:  ldloc.s    V_10
          IL_0110:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
          IL_0115:  nop
          IL_0116:  endfinally
        }  // end handler
        IL_0117:  nop
        IL_0118:  nop
        IL_0119:  nop
        IL_011a:  ldloc.s    V_36
        IL_011c:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecordEnumerator::MoveNext()
        IL_0121:  stloc.s    V_37
        IL_0123:  ldloc.s    V_37
        IL_0125:  brtrue     IL_0081

        IL_012a:  leave.s    IL_0140

      }  // end .try
      finally
      {
        IL_012c:  ldloc.s    V_36
        IL_012e:  ldnull
        IL_012f:  ceq
        IL_0131:  stloc.s    V_37
        IL_0133:  ldloc.s    V_37
        IL_0135:  brtrue.s   IL_013f

        IL_0137:  ldloc.s    V_36
        IL_0139:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_013e:  nop
        IL_013f:  endfinally
      }  // end handler
      IL_0140:  nop
      IL_0141:  ldloc.1
      IL_0142:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::get_Count()
      IL_0147:  stloc.s    V_12
      IL_0149:  ldc.i4.0
      IL_014a:  stloc.s    V_13
      IL_014c:  nop
      IL_014d:  ldloc.1
      IL_014e:  callvirt   instance valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<!0> class [mscorlib]System.Collections.Generic.List`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::GetEnumerator()
      IL_0153:  stloc.s    V_38
      .try
      {
        IL_0155:  br         IL_056c

        IL_015a:  ldloca.s   V_38
        IL_015c:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::get_Current()
        IL_0161:  stloc.s    V_8
        IL_0163:  nop
        IL_0164:  ldloc.s    V_13
        IL_0166:  ldc.i4.1
        IL_0167:  add
        IL_0168:  stloc.s    V_13
        IL_016a:  ldloc.2
        IL_016b:  ldloc.s    V_8
        IL_016d:  ldc.i4.0
        IL_016e:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
        IL_0173:  isinst     [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference
        IL_0178:  stloc.s    V_14
        IL_017a:  ldloc.s    V_14
        IL_017c:  ldnull
        IL_017d:  call       bool [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper::op_Equality(class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper,
                                                                                                   class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper)
        IL_0182:  brtrue.s   IL_0190

        IL_0184:  ldloc.s    V_14
        IL_0186:  callvirt   instance bool [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::get_IsErased()
        IL_018b:  ldc.i4.0
        IL_018c:  ceq
        IL_018e:  br.s       IL_0191

        IL_0190:  ldc.i4.0
        IL_0191:  nop
        IL_0192:  stloc.s    V_37
        IL_0194:  ldloc.s    V_37
        IL_0196:  brtrue.s   IL_01ac

        IL_0198:  nop
        IL_0199:  ldloc.0
        IL_019a:  dup
        IL_019b:  ldfld      int32 Tvxd.Meps.ToadoResult::skipped
        IL_01a0:  ldc.i4.1
        IL_01a1:  add
        IL_01a2:  stfld      int32 Tvxd.Meps.ToadoResult::skipped
        IL_01a7:  br         IL_056c

        IL_01ac:  ldloc.s    V_14
        IL_01ae:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Handle [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::get_Handle()
        IL_01b3:  stloc.s    V_39
        IL_01b5:  ldloca.s   V_39
        IL_01b7:  constrained. [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Handle
        IL_01bd:  callvirt   instance string [mscorlib]System.Object::ToString()
        IL_01c2:  stloc.s    V_15
        IL_01c4:  ldloc.s    V_14
        IL_01c6:  ldarg.2
        IL_01c7:  ldfld      bool Tvxd.Meps.ToadoSettings::Center
        IL_01cc:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d Tvxd.Meps.Toado100::Tip(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference,
                                                                                                          bool)
        IL_01d1:  stloc.s    V_16
        IL_01d3:  ldloc.s    V_16
        IL_01d5:  ldarg.2
        IL_01d6:  ldarg.2
        IL_01d7:  ldfld      bool Tvxd.Meps.ToadoSettings::AddName
        IL_01dc:  brtrue.s   IL_01e1

        IL_01de:  ldnull
        IL_01df:  br.s       IL_01e9

        IL_01e1:  ldloc.2
        IL_01e2:  ldloc.s    V_14
        IL_01e4:  call       string Tvxd.Meps.Toado100::NameOf(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction,
                                                               class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference)
        IL_01e9:  nop
        IL_01ea:  ldc.i4.0
        IL_01eb:  call       string Tvxd.Meps.Toado100::Format(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,
                                                               class Tvxd.Meps.ToadoSettings,
                                                               string,
                                                               bool)
        IL_01f0:  stloc.s    V_17
        IL_01f2:  ldloc.s    V_6
        IL_01f4:  ldloc.s    V_15
        IL_01f6:  ldloca.s   V_18
        IL_01f8:  callvirt   instance bool class [mscorlib]System.Collections.Generic.Dictionary`2<string,valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::TryGetValue(!0,
                                                                                                                                                                                      !1&)
        IL_01fd:  ldc.i4.0
        IL_01fe:  ceq
        IL_0200:  stloc.s    V_37
        IL_0202:  ldloc.s    V_37
        IL_0204:  brtrue     IL_03dc

        IL_0209:  nop
        IL_020a:  ldloc.2
        IL_020b:  ldloc.s    V_18
        IL_020d:  ldc.i4.1
        IL_020e:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
        IL_0213:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader
        IL_0218:  stloc.s    V_19
        IL_021a:  ldloc.s    V_16
        IL_021c:  stloc.s    V_20
        IL_021e:  ldloc.s    V_19
        IL_0220:  ldstr      "TVXD_TOADO"
        IL_0225:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::GetXDataForApplication(string)
        IL_022a:  stloc.s    V_10
        .try
        {
          IL_022c:  nop
          IL_022d:  ldloc.s    V_10
          IL_022f:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[] [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer::AsArray()
          IL_0234:  stloc.s    V_11
          IL_0236:  ldloc.s    V_11
          IL_0238:  ldc.i4.2
          IL_0239:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
          IL_023e:  call       instance object [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::get_Value()
          IL_0243:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
          IL_0248:  call       float64 [mscorlib]System.Convert::ToDouble(object,
                                                                          class [mscorlib]System.IFormatProvider)
          IL_024d:  ldloc.s    V_11
          IL_024f:  ldc.i4.3
          IL_0250:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
          IL_0255:  call       instance object [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::get_Value()
          IL_025a:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
          IL_025f:  call       float64 [mscorlib]System.Convert::ToDouble(object,
                                                                          class [mscorlib]System.IFormatProvider)
          IL_0264:  ldloc.s    V_11
          IL_0266:  ldc.i4.4
          IL_0267:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
          IL_026c:  call       instance object [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::get_Value()
          IL_0271:  ldsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
          IL_0276:  call       float64 [mscorlib]System.Convert::ToDouble(object,
                                                                          class [mscorlib]System.IFormatProvider)
          IL_027b:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::.ctor(float64,
                                                                                               float64,
                                                                                               float64)
          IL_0280:  stloc.s    V_20
          IL_0282:  nop
          IL_0283:  leave.s    IL_0299

        }  // end .try
        finally
        {
          IL_0285:  ldloc.s    V_10
          IL_0287:  ldnull
          IL_0288:  ceq
          IL_028a:  stloc.s    V_37
          IL_028c:  ldloc.s    V_37
          IL_028e:  brtrue.s   IL_0298

          IL_0290:  ldloc.s    V_10
          IL_0292:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
          IL_0297:  nop
          IL_0298:  endfinally
        }  // end handler
        IL_0299:  nop
        IL_029a:  ldc.i4.0
        IL_029b:  stloc.s    V_21
        IL_029d:  ldloc.s    V_16
        IL_029f:  ldloc.s    V_20
        IL_02a1:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Vector3d [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::op_Subtraction(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,
                                                                                                                                              valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
        IL_02a6:  stloc.s    V_22
        IL_02a8:  ldloca.s   V_22
        IL_02aa:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Vector3d::get_Length()
        IL_02af:  ldc.r8     1.0000000000000001e-09
        IL_02b8:  cgt
        IL_02ba:  ldc.i4.0
        IL_02bb:  ceq
        IL_02bd:  stloc.s    V_37
        IL_02bf:  ldloc.s    V_37
        IL_02c1:  brtrue.s   IL_02f6

        IL_02c3:  nop
        IL_02c4:  ldloc.s    V_19
        IL_02c6:  ldloc.s    V_22
        IL_02c8:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Matrix3d [Acdbmgd]Autodesk.AutoCAD.Geometry.Matrix3d::Displacement(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Vector3d)
        IL_02cd:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Entity::TransformBy(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Matrix3d)
        IL_02d2:  nop
        IL_02d3:  ldloc.s    V_19
        IL_02d5:  ldloc.s    V_15
        IL_02d7:  ldloc.s    V_16
        IL_02d9:  call       class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer Tvxd.Meps.Toado100::Tag(string,
                                                                                                                   valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
        IL_02de:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::set_XData(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer)
        IL_02e3:  nop
        IL_02e4:  ldloc.0
        IL_02e5:  dup
        IL_02e6:  ldfld      int32 Tvxd.Meps.ToadoResult::moved
        IL_02eb:  ldc.i4.1
        IL_02ec:  add
        IL_02ed:  stfld      int32 Tvxd.Meps.ToadoResult::moved
        IL_02f2:  ldc.i4.1
        IL_02f3:  stloc.s    V_21
        IL_02f5:  nop
        IL_02f6:  ldloc.s    V_19
        IL_02f8:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::get_MText()
        IL_02fd:  stloc.s    V_23
        IL_02ff:  ldc.i4.0
        IL_0300:  stloc.s    V_24
        IL_0302:  ldarg.2
        IL_0303:  ldfld      bool Tvxd.Meps.ToadoSettings::Reformat
        IL_0308:  ldc.i4.0
        IL_0309:  ceq
        IL_030b:  stloc.s    V_37
        IL_030d:  ldloc.s    V_37
        IL_030f:  brtrue.s   IL_0335

        IL_0311:  nop
        IL_0312:  ldloc.s    V_19
        IL_0314:  ldloc.s    V_23
        IL_0316:  ldarg.2
        IL_0317:  ldloc.s    V_4
        IL_0319:  call       void Tvxd.Meps.Toado100::Style(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader,
                                                            class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText,
                                                            class Tvxd.Meps.ToadoSettings,
                                                            valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
        IL_031e:  nop
        IL_031f:  ldloc.s    V_19
        IL_0321:  ldarg.2
        IL_0322:  call       void Tvxd.Meps.Toado100::LeaderLook(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader,
                                                                 class Tvxd.Meps.ToadoSettings)
        IL_0327:  nop
        IL_0328:  ldloc.s    V_19
        IL_032a:  ldloc.3
        IL_032b:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Entity::set_LayerId(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
        IL_0330:  nop
        IL_0331:  ldc.i4.1
        IL_0332:  stloc.s    V_24
        IL_0334:  nop
        IL_0335:  ldloc.s    V_23
        IL_0337:  callvirt   instance string [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::get_Contents()
        IL_033c:  ldloc.s    V_17
        IL_033e:  call       bool [mscorlib]System.String::op_Inequality(string,
                                                                         string)
        IL_0343:  brtrue.s   IL_034c

        IL_0345:  ldloc.s    V_24
        IL_0347:  ldc.i4.0
        IL_0348:  ceq
        IL_034a:  br.s       IL_034d

        IL_034c:  ldc.i4.0
        IL_034d:  nop
        IL_034e:  stloc.s    V_37
        IL_0350:  ldloc.s    V_37
        IL_0352:  brtrue.s   IL_03b9

        IL_0354:  nop
        IL_0355:  ldloc.s    V_19
        IL_0357:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::get_TextLocation()
        IL_035c:  stloc.s    V_25
        IL_035e:  ldloc.s    V_23
        IL_0360:  ldloc.s    V_17
        IL_0362:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_Contents(string)
        IL_0367:  nop
        IL_0368:  ldloc.s    V_19
        IL_036a:  ldloc.s    V_23
        IL_036c:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_MText(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText)
        IL_0371:  nop
        IL_0372:  ldloc.s    V_19
        IL_0374:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::get_TextLocation()
        IL_0379:  ldloc.s    V_25
        IL_037b:  call       valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Vector3d [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::op_Subtraction(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,
                                                                                                                                              valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
        IL_0380:  stloc.s    V_40
        IL_0382:  ldloca.s   V_40
        IL_0384:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Vector3d::get_Length()
        IL_0389:  ldc.r8     1.0000000000000001e-09
        IL_0392:  cgt
        IL_0394:  ldc.i4.0
        IL_0395:  ceq
        IL_0397:  stloc.s    V_37
        IL_0399:  ldloc.s    V_37
        IL_039b:  brtrue.s   IL_03a7

        IL_039d:  ldloc.s    V_19
        IL_039f:  ldloc.s    V_25
        IL_03a1:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_TextLocation(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
        IL_03a6:  nop
        IL_03a7:  ldloc.0
        IL_03a8:  dup
        IL_03a9:  ldfld      int32 Tvxd.Meps.ToadoResult::updated
        IL_03ae:  ldc.i4.1
        IL_03af:  add
        IL_03b0:  stfld      int32 Tvxd.Meps.ToadoResult::updated
        IL_03b5:  ldc.i4.1
        IL_03b6:  stloc.s    V_21
        IL_03b8:  nop
        IL_03b9:  ldloc.s    V_23
        IL_03bb:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper::Dispose()
        IL_03c0:  nop
        IL_03c1:  ldloc.s    V_21
        IL_03c3:  stloc.s    V_37
        IL_03c5:  ldloc.s    V_37
        IL_03c7:  brtrue.s   IL_03d7

        IL_03c9:  ldloc.0
        IL_03ca:  dup
        IL_03cb:  ldfld      int32 Tvxd.Meps.ToadoResult::same
        IL_03d0:  ldc.i4.1
        IL_03d1:  add
        IL_03d2:  stfld      int32 Tvxd.Meps.ToadoResult::same
        IL_03d7:  br         IL_056c

        IL_03dc:  ldarg.3
        IL_03dd:  brtrue.s   IL_03eb

        IL_03df:  ldloca.s   V_41
        IL_03e1:  initobj    valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>
        IL_03e7:  ldloc.s    V_41
        IL_03e9:  br.s       IL_03f7

        IL_03eb:  ldarg.3
        IL_03ec:  ldloc.s    V_13
        IL_03ee:  ldloc.s    V_12
        IL_03f0:  ldloc.s    V_16
        IL_03f2:  callvirt   instance !3 class [mscorlib]System.Func`4<int32,int32,valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>>::Invoke(!0,
                                                                                                                                                                                                                                             !1,
                                                                                                                                                                                                                                             !2)
        IL_03f7:  nop
        IL_03f8:  stloc.s    V_26
        IL_03fa:  ldarg.2
        IL_03fb:  ldfld      float64 Tvxd.Meps.ToadoSettings::Height
        IL_0400:  stloc.s    V_27
        IL_0402:  ldarg.2
        IL_0403:  ldfld      int32 Tvxd.Meps.ToadoSettings::Corner
        IL_0408:  ldc.i4.1
        IL_0409:  beq.s      IL_0417

        IL_040b:  ldarg.2
        IL_040c:  ldfld      int32 Tvxd.Meps.ToadoSettings::Corner
        IL_0411:  ldc.i4.3
        IL_0412:  beq.s      IL_0417

        IL_0414:  ldc.i4.1
        IL_0415:  br.s       IL_0418

        IL_0417:  ldc.i4.m1
        IL_0418:  nop
        IL_0419:  conv.r8
        IL_041a:  stloc.s    V_28
        IL_041c:  ldarg.2
        IL_041d:  ldfld      int32 Tvxd.Meps.ToadoSettings::Corner
        IL_0422:  ldc.i4.2
        IL_0423:  beq.s      IL_0431

        IL_0425:  ldarg.2
        IL_0426:  ldfld      int32 Tvxd.Meps.ToadoSettings::Corner
        IL_042b:  ldc.i4.3
        IL_042c:  beq.s      IL_0431

        IL_042e:  ldc.i4.1
        IL_042f:  br.s       IL_0432

        IL_0431:  ldc.i4.m1
        IL_0432:  nop
        IL_0433:  conv.r8
        IL_0434:  stloc.s    V_29
        IL_0436:  ldloca.s   V_26
        IL_0438:  call       instance bool valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>::get_HasValue()
        IL_043d:  brtrue.s   IL_047f

        IL_043f:  ldloca.s   V_16
        IL_0441:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
        IL_0446:  ldloc.s    V_28
        IL_0448:  ldarg.2
        IL_0449:  ldfld      float64 Tvxd.Meps.ToadoSettings::Distance
        IL_044e:  mul
        IL_044f:  ldloc.s    V_27
        IL_0451:  mul
        IL_0452:  add
        IL_0453:  ldloca.s   V_16
        IL_0455:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
        IL_045a:  ldloc.s    V_29
        IL_045c:  ldarg.2
        IL_045d:  ldfld      float64 Tvxd.Meps.ToadoSettings::Distance
        IL_0462:  mul
        IL_0463:  ldloc.s    V_27
        IL_0465:  mul
        IL_0466:  ldc.r8     0.59999999999999998
        IL_046f:  mul
        IL_0470:  add
        IL_0471:  ldloca.s   V_16
        IL_0473:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Z()
        IL_0478:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::.ctor(float64,
                                                                                             float64,
                                                                                             float64)
        IL_047d:  br.s       IL_0486

        IL_047f:  ldloca.s   V_26
        IL_0481:  call       instance !0 valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>::get_Value()
        IL_0486:  nop
        IL_0487:  stloc.s    V_30
        IL_0489:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::.ctor()
        IL_048e:  stloc.s    V_31
        IL_0490:  ldloc.s    V_31
        IL_0492:  ldarg.0
        IL_0493:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Entity::SetDatabaseDefaults(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
        IL_0498:  nop
        IL_0499:  ldloc.s    V_31
        IL_049b:  ldc.i4.2
        IL_049c:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_ContentType(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ContentType)
        IL_04a1:  nop
        IL_04a2:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::.ctor()
        IL_04a7:  stloc.s    V_32
        IL_04a9:  ldloc.s    V_32
        IL_04ab:  ldarg.0
        IL_04ac:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Entity::SetDatabaseDefaults(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
        IL_04b1:  nop
        IL_04b2:  ldloc.s    V_32
        IL_04b4:  ldloc.s    V_17
        IL_04b6:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText::set_Contents(string)
        IL_04bb:  nop
        IL_04bc:  ldloc.s    V_31
        IL_04be:  ldloc.s    V_32
        IL_04c0:  ldarg.2
        IL_04c1:  ldloc.s    V_4
        IL_04c3:  call       void Tvxd.Meps.Toado100::Style(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader,
                                                            class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText,
                                                            class Tvxd.Meps.ToadoSettings,
                                                            valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
        IL_04c8:  nop
        IL_04c9:  ldloc.s    V_31
        IL_04cb:  ldloc.s    V_32
        IL_04cd:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_MText(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MText)
        IL_04d2:  nop
        IL_04d3:  ldloc.s    V_31
        IL_04d5:  ldloc.s    V_4
        IL_04d7:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_TextStyleId(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
        IL_04dc:  nop
        IL_04dd:  ldloc.s    V_31
        IL_04df:  ldloc.s    V_27
        IL_04e1:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_TextHeight(float64)
        IL_04e6:  nop
        IL_04e7:  ldloc.s    V_31
        IL_04e9:  ldc.i4.0
        IL_04ea:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_TextAngleType(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TextAngleType)
        IL_04ef:  nop
        IL_04f0:  ldloc.s    V_31
        IL_04f2:  ldloc.3
        IL_04f3:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Entity::set_LayerId(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId)
        IL_04f8:  nop
        IL_04f9:  ldloc.s    V_31
        IL_04fb:  ldarg.2
        IL_04fc:  call       void Tvxd.Meps.Toado100::LeaderLook(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader,
                                                                 class Tvxd.Meps.ToadoSettings)
        IL_0501:  nop
        IL_0502:  ldloc.s    V_31
        IL_0504:  ldloc.s    V_30
        IL_0506:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::set_TextLocation(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
        IL_050b:  nop
        IL_050c:  ldloc.s    V_31
        IL_050e:  callvirt   instance int32 [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::AddLeader()
        IL_0513:  stloc.s    V_33
        IL_0515:  ldloc.s    V_31
        IL_0517:  ldloc.s    V_33
        IL_0519:  callvirt   instance int32 [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::AddLeaderLine(int32)
        IL_051e:  stloc.s    V_34
        IL_0520:  ldloc.s    V_31
        IL_0522:  ldloc.s    V_34
        IL_0524:  ldloc.s    V_16
        IL_0526:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::AddFirstVertex(int32,
                                                                                                              valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
        IL_052b:  nop
        IL_052c:  ldloc.s    V_31
        IL_052e:  ldloc.s    V_34
        IL_0530:  ldloc.s    V_30
        IL_0532:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.MLeader::AddLastVertex(int32,
                                                                                                             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
        IL_0537:  nop
        IL_0538:  ldloc.s    V_5
        IL_053a:  ldloc.s    V_31
        IL_053c:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockTableRecord::AppendEntity(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Entity)
        IL_0541:  pop
        IL_0542:  ldloc.2
        IL_0543:  ldloc.s    V_31
        IL_0545:  ldc.i4.1
        IL_0546:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::AddNewlyCreatedDBObject(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject,
                                                                                                                           bool)
        IL_054b:  nop
        IL_054c:  ldloc.s    V_31
        IL_054e:  ldloc.s    V_15
        IL_0550:  ldloc.s    V_16
        IL_0552:  call       class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer Tvxd.Meps.Toado100::Tag(string,
                                                                                                                   valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
        IL_0557:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject::set_XData(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ResultBuffer)
        IL_055c:  nop
        IL_055d:  ldloc.0
        IL_055e:  dup
        IL_055f:  ldfld      int32 Tvxd.Meps.ToadoResult::created
        IL_0564:  ldc.i4.1
        IL_0565:  add
        IL_0566:  stfld      int32 Tvxd.Meps.ToadoResult::created
        IL_056b:  nop
        IL_056c:  ldloca.s   V_38
        IL_056e:  call       instance bool valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>::MoveNext()
        IL_0573:  stloc.s    V_37
        IL_0575:  ldloc.s    V_37
        IL_0577:  brtrue     IL_015a

        IL_057c:  leave.s    IL_058d

      }  // end .try
      finally
      {
        IL_057e:  ldloca.s   V_38
        IL_0580:  constrained. valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>
        IL_0586:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_058b:  nop
        IL_058c:  endfinally
      }  // end handler
      IL_058d:  nop
      IL_058e:  ldloc.2
      IL_058f:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::Commit()
      IL_0594:  nop
      IL_0595:  nop
      IL_0596:  leave.s    IL_05aa

    }  // end .try
    finally
    {
      IL_0598:  ldloc.2
      IL_0599:  ldnull
      IL_059a:  ceq
      IL_059c:  stloc.s    V_37
      IL_059e:  ldloc.s    V_37
      IL_05a0:  brtrue.s   IL_05a9

      IL_05a2:  ldloc.2
      IL_05a3:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_05a8:  nop
      IL_05a9:  endfinally
    }  // end handler
    IL_05aa:  nop
    IL_05ab:  ldloc.0
    IL_05ac:  stloc.s    V_35
    IL_05ae:  br.s       IL_05b0

    IL_05b0:  ldloc.s    V_35
    IL_05b2:  ret
  } // end of method Toado100::Apply

  .method public hidebysig static void  Run() cil managed
  {
    // Code size       695 (0x2b7)
    .maxstack  4
    .locals init (class [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document V_0,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionOptions V_1,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.SelectionFilter V_2,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionResult V_3,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId[] V_4,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionResult V_5,
             class Tvxd.Meps.ToadoSettings V_6,
             class [mscorlib]System.Collections.Generic.List`1<string> V_7,
             class Tvxd.Meps.Toado100Form V_8,
             class [mscorlib]System.Func`4<int32,int32,valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>> V_9,
             class Tvxd.Meps.ToadoResult V_10,
             string V_11,
             class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionOptions V_12,
             class Tvxd.Meps.Toado100/'<>c__DisplayClass6' V_13,
             valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[] V_14,
             bool V_15,
             valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<string> V_16,
             object[] V_17)
    IL_0000:  newobj     instance void Tvxd.Meps.Toado100/'<>c__DisplayClass6'::.ctor()
    IL_0005:  stloc.s    V_13
    IL_0007:  nop
    IL_0008:  call       class [accoremgd]Autodesk.AutoCAD.ApplicationServices.DocumentCollection [accoremgd]Autodesk.AutoCAD.ApplicationServices.Core.Application::get_DocumentManager()
    IL_000d:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document [accoremgd]Autodesk.AutoCAD.ApplicationServices.DocumentCollection::get_MdiActiveDocument()
    IL_0012:  stloc.0
    IL_0013:  ldloc.s    V_13
    IL_0015:  ldloc.0
    IL_0016:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.EditorInput.Editor [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document::get_Editor()
    IL_001b:  stfld      class [accoremgd]Autodesk.AutoCAD.EditorInput.Editor Tvxd.Meps.Toado100/'<>c__DisplayClass6'::ed
    IL_0020:  newobj     instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionOptions::.ctor()
    IL_0025:  stloc.s    V_12
    IL_0027:  ldloc.s    V_12
    IL_0029:  ldstr      bytearray (0A 00 51 00 75 00 E9 00 74 00 20 00 63 00 68 00   // ..Q.u...t. .c.h.
                                    CD 1E 6E 00 20 00 63 00 E1 00 63 00 20 00 62 00   // ..n. .c...c. .b.
                                    6C 00 6F 00 63 00 6B 00 20 00 63 00 A7 1E 6E 00   // l.o.c.k. .c...n.
                                    20 00 67 00 68 00 69 00 20 00 74 00 CD 1E 61 00   //  .g.h.i. .t...a.
                                    20 00 11 01 D9 1E 20 00 28 00 45 00 6E 00 74 00   //  ..... .(.E.n.t.
                                    65 00 72 00 20 00 3D 00 20 00 74 00 A5 1E 74 00   // e.r. .=. .t...t.
                                    20 00 63 00 A3 1E 20 00 62 00 6C 00 6F 00 63 00   //  .c... .b.l.o.c.
                                    6B 00 20 00 74 00 72 00 6F 00 6E 00 67 00 20 00   // k. .t.r.o.n.g. .
                                    4D 00 6F 00 64 00 65 00 6C 00 29 00 3A 00 20 00 ) // M.o.d.e.l.).:. .
    IL_002e:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionOptions::set_MessageForAdding(string)
    IL_0033:  nop
    IL_0034:  ldloc.s    V_12
    IL_0036:  stloc.1
    IL_0037:  ldc.i4.1
    IL_0038:  newarr     [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_003d:  stloc.s    V_14
    IL_003f:  ldloc.s    V_14
    IL_0041:  ldc.i4.0
    IL_0042:  ldelema    [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_0047:  ldc.i4.0
    IL_0048:  ldstr      "INSERT"
    IL_004d:  newobj     instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue::.ctor(int32,
                                                                                                    object)
    IL_0052:  stobj      [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue
    IL_0057:  ldloc.s    V_14
    IL_0059:  newobj     instance void [accoremgd]Autodesk.AutoCAD.EditorInput.SelectionFilter::.ctor(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TypedValue[])
    IL_005e:  stloc.2
    IL_005f:  ldloc.s    V_13
    IL_0061:  ldfld      class [accoremgd]Autodesk.AutoCAD.EditorInput.Editor Tvxd.Meps.Toado100/'<>c__DisplayClass6'::ed
    IL_0066:  ldloc.1
    IL_0067:  ldloc.2
    IL_0068:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionResult [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::GetSelection(class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionOptions,
                                                                                                                                                                   class [accoremgd]Autodesk.AutoCAD.EditorInput.SelectionFilter)
    IL_006d:  stloc.3
    IL_006e:  ldloc.3
    IL_006f:  callvirt   instance valuetype [accoremgd]Autodesk.AutoCAD.EditorInput.PromptStatus [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionResult::get_Status()
    IL_0074:  ldc.i4     0x13ec
    IL_0079:  ceq
    IL_007b:  ldc.i4.0
    IL_007c:  ceq
    IL_007e:  stloc.s    V_15
    IL_0080:  ldloc.s    V_15
    IL_0082:  brtrue.s   IL_0093

    IL_0084:  ldloc.3
    IL_0085:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.EditorInput.SelectionSet [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionResult::get_Value()
    IL_008a:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId[] [accoremgd]Autodesk.AutoCAD.EditorInput.SelectionSet::GetObjectIds()
    IL_008f:  stloc.s    V_4
    IL_0091:  br.s       IL_00ee

    IL_0093:  ldloc.3
    IL_0094:  callvirt   instance valuetype [accoremgd]Autodesk.AutoCAD.EditorInput.PromptStatus [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionResult::get_Status()
    IL_0099:  ldc.i4     0xffffec77
    IL_009e:  ceq
    IL_00a0:  ldc.i4.0
    IL_00a1:  ceq
    IL_00a3:  stloc.s    V_15
    IL_00a5:  ldloc.s    V_15
    IL_00a7:  brtrue.s   IL_00e9

    IL_00a9:  nop
    IL_00aa:  ldloc.s    V_13
    IL_00ac:  ldfld      class [accoremgd]Autodesk.AutoCAD.EditorInput.Editor Tvxd.Meps.Toado100/'<>c__DisplayClass6'::ed
    IL_00b1:  ldloc.2
    IL_00b2:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionResult [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::SelectAll(class [accoremgd]Autodesk.AutoCAD.EditorInput.SelectionFilter)
    IL_00b7:  stloc.s    V_5
    IL_00b9:  ldloc.s    V_5
    IL_00bb:  callvirt   instance valuetype [accoremgd]Autodesk.AutoCAD.EditorInput.PromptStatus [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionResult::get_Status()
    IL_00c0:  ldc.i4     0x13ec
    IL_00c5:  ceq
    IL_00c7:  stloc.s    V_15
    IL_00c9:  ldloc.s    V_15
    IL_00cb:  brtrue.s   IL_00d8

    IL_00cd:  ldstr      bytearray (4D 00 6F 00 64 00 65 00 6C 00 20 00 6B 00 68 00   // M.o.d.e.l. .k.h.
                                    F4 00 6E 00 67 00 20 00 63 00 F3 00 20 00 62 00   // ..n.g. .c... .b.
                                    6C 00 6F 00 63 00 6B 00 2E 00 )                   // l.o.c.k...
    IL_00d2:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_00d7:  throw

    IL_00d8:  ldloc.s    V_5
    IL_00da:  callvirt   instance class [accoremgd]Autodesk.AutoCAD.EditorInput.SelectionSet [accoremgd]Autodesk.AutoCAD.EditorInput.PromptSelectionResult::get_Value()
    IL_00df:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId[] [accoremgd]Autodesk.AutoCAD.EditorInput.SelectionSet::GetObjectIds()
    IL_00e4:  stloc.s    V_4
    IL_00e6:  nop
    IL_00e7:  br.s       IL_00ee

    IL_00e9:  br         IL_02b5

    IL_00ee:  call       class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100::Load()
    IL_00f3:  stloc.s    V_6
    IL_00f5:  ldloc.0
    IL_00f6:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document::get_Database()
    IL_00fb:  call       class [mscorlib]System.Collections.Generic.List`1<string> Tvxd.Meps.Toado100::StyleNames(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database)
    IL_0100:  stloc.s    V_7
    IL_0102:  ldloc.s    V_7
    IL_0104:  ldstr      "Tuan_arial"
    IL_0109:  call       class [mscorlib]System.StringComparer [mscorlib]System.StringComparer::get_OrdinalIgnoreCase()
    IL_010e:  call       bool [System.Core]System.Linq.Enumerable::Contains<string>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                    !!0,
                                                                                    class [mscorlib]System.Collections.Generic.IEqualityComparer`1<!!0>)
    IL_0113:  stloc.s    V_15
    IL_0115:  ldloc.s    V_15
    IL_0117:  brtrue.s   IL_0127

    IL_0119:  ldloc.s    V_7
    IL_011b:  ldc.i4.0
    IL_011c:  ldstr      "Tuan_arial"
    IL_0121:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Insert(int32,
                                                                                                         !0)
    IL_0126:  nop
    IL_0127:  ldloc.s    V_6
    IL_0129:  ldloc.s    V_7
    IL_012b:  ldloc.s    V_4
    IL_012d:  ldlen
    IL_012e:  conv.i4
    IL_012f:  newobj     instance void Tvxd.Meps.Toado100Form::.ctor(class Tvxd.Meps.ToadoSettings,
                                                                     class [mscorlib]System.Collections.Generic.IList`1<string>,
                                                                     int32)
    IL_0134:  stloc.s    V_8
    .try
    {
      IL_0136:  nop
      IL_0137:  ldloc.s    V_8
      IL_0139:  call       valuetype [System.Windows.Forms]System.Windows.Forms.DialogResult [Acmgd]Autodesk.AutoCAD.ApplicationServices.Application::ShowModalDialog(class [System.Windows.Forms]System.Windows.Forms.Form)
      IL_013e:  ldc.i4.1
      IL_013f:  ceq
      IL_0141:  stloc.s    V_15
      IL_0143:  ldloc.s    V_15
      IL_0145:  brtrue.s   IL_014c

      IL_0147:  leave      IL_02b5

      IL_014c:  ldloc.s    V_8
      IL_014e:  ldfld      class Tvxd.Meps.ToadoSettings Tvxd.Meps.Toado100Form::S
      IL_0153:  stloc.s    V_6
      IL_0155:  nop
      IL_0156:  leave.s    IL_016c

    }  // end .try
    finally
    {
      IL_0158:  ldloc.s    V_8
      IL_015a:  ldnull
      IL_015b:  ceq
      IL_015d:  stloc.s    V_15
      IL_015f:  ldloc.s    V_15
      IL_0161:  brtrue.s   IL_016b

      IL_0163:  ldloc.s    V_8
      IL_0165:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_016a:  nop
      IL_016b:  endfinally
    }  // end handler
    IL_016c:  nop
    IL_016d:  ldloc.s    V_6
    IL_016f:  call       void Tvxd.Meps.Toado100::Save(class Tvxd.Meps.ToadoSettings)
    IL_0174:  nop
    IL_0175:  ldloc.s    V_13
    IL_0177:  ldloc.s    V_13
    IL_0179:  ldfld      class [accoremgd]Autodesk.AutoCAD.EditorInput.Editor Tvxd.Meps.Toado100/'<>c__DisplayClass6'::ed
    IL_017e:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Matrix3d [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::get_CurrentUserCoordinateSystem()
    IL_0183:  stfld      valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Matrix3d Tvxd.Meps.Toado100/'<>c__DisplayClass6'::ucs
    IL_0188:  ldnull
    IL_0189:  stloc.s    V_9
    IL_018b:  ldloc.s    V_6
    IL_018d:  ldfld      bool Tvxd.Meps.ToadoSettings::PickEach
    IL_0192:  ldc.i4.0
    IL_0193:  ceq
    IL_0195:  stloc.s    V_15
    IL_0197:  ldloc.s    V_15
    IL_0199:  brtrue.s   IL_01aa

    IL_019b:  ldloc.s    V_13
    IL_019d:  ldftn      instance valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d> Tvxd.Meps.Toado100/'<>c__DisplayClass6'::'<Run>b__5'(int32,
                                                                                                                                                                                   int32,
                                                                                                                                                                                   valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d)
    IL_01a3:  newobj     instance void class [mscorlib]System.Func`4<int32,int32,valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>>::.ctor(object,
                                                                                                                                                                                                                                          native int)
    IL_01a8:  stloc.s    V_9
    IL_01aa:  ldloc.0
    IL_01ab:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database [accoremgd]Autodesk.AutoCAD.ApplicationServices.Document::get_Database()
    IL_01b0:  ldloc.s    V_4
    IL_01b2:  ldloc.s    V_6
    IL_01b4:  ldloc.s    V_9
    IL_01b6:  call       class Tvxd.Meps.ToadoResult Tvxd.Meps.Toado100::Apply(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                               class [mscorlib]System.Collections.Generic.IEnumerable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId>,
                                                                               class Tvxd.Meps.ToadoSettings,
                                                                               class [mscorlib]System.Func`4<int32,int32,valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d,valuetype [mscorlib]System.Nullable`1<valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d>>)
    IL_01bb:  stloc.s    V_10
    IL_01bd:  nop
    IL_01be:  ldloc.s    V_10
    IL_01c0:  ldfld      class [mscorlib]System.Collections.Generic.List`1<string> Tvxd.Meps.ToadoResult::warnings
    IL_01c5:  callvirt   instance valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<!0> class [mscorlib]System.Collections.Generic.List`1<string>::GetEnumerator()
    IL_01ca:  stloc.s    V_16
    .try
    {
      IL_01cc:  br.s       IL_01f0

      IL_01ce:  ldloca.s   V_16
      IL_01d0:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<string>::get_Current()
      IL_01d5:  stloc.s    V_11
      IL_01d7:  ldloc.s    V_13
      IL_01d9:  ldfld      class [accoremgd]Autodesk.AutoCAD.EditorInput.Editor Tvxd.Meps.Toado100/'<>c__DisplayClass6'::ed
      IL_01de:  ldstr      "\n"
      IL_01e3:  ldloc.s    V_11
      IL_01e5:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_01ea:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::WriteMessage(string)
      IL_01ef:  nop
      IL_01f0:  ldloca.s   V_16
      IL_01f2:  call       instance bool valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<string>::MoveNext()
      IL_01f7:  stloc.s    V_15
      IL_01f9:  ldloc.s    V_15
      IL_01fb:  brtrue.s   IL_01ce

      IL_01fd:  leave.s    IL_020e

    }  // end .try
    finally
    {
      IL_01ff:  ldloca.s   V_16
      IL_0201:  constrained. valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<string>
      IL_0207:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_020c:  nop
      IL_020d:  endfinally
    }  // end handler
    IL_020e:  nop
    IL_020f:  ldloc.s    V_13
    IL_0211:  ldfld      class [accoremgd]Autodesk.AutoCAD.EditorInput.Editor Tvxd.Meps.Toado100/'<>c__DisplayClass6'::ed
    IL_0216:  ldc.i4.s   11
    IL_0218:  newarr     [mscorlib]System.Object
    IL_021d:  stloc.s    V_17
    IL_021f:  ldloc.s    V_17
    IL_0221:  ldc.i4.0
    IL_0222:  ldstr      bytearray (0A 00 54 00 2D 00 54 00 4F 00 41 00 44 00 4F 00   // ..T.-.T.O.A.D.O.
                                    3A 00 20 00 74 00 A1 1E 6F 00 20 00 6D 00 DB 1E   // :. .t...o. .m...
                                    69 00 20 00 )                                     // i. .
    IL_0227:  stelem.ref
    IL_0228:  ldloc.s    V_17
    IL_022a:  ldc.i4.1
    IL_022b:  ldloc.s    V_10
    IL_022d:  ldfld      int32 Tvxd.Meps.ToadoResult::created
    IL_0232:  box        [mscorlib]System.Int32
    IL_0237:  stelem.ref
    IL_0238:  ldloc.s    V_17
    IL_023a:  ldc.i4.2
    IL_023b:  ldstr      bytearray (2C 00 20 00 63 00 AD 1E 70 00 20 00 6E 00 68 00   // ,. .c...p. .n.h.
                                    AD 1E 74 00 20 00 63 00 68 00 EF 1E 20 00 )       // ..t. .c.h... .
    IL_0240:  stelem.ref
    IL_0241:  ldloc.s    V_17
    IL_0243:  ldc.i4.3
    IL_0244:  ldloc.s    V_10
    IL_0246:  ldfld      int32 Tvxd.Meps.ToadoResult::updated
    IL_024b:  box        [mscorlib]System.Int32
    IL_0250:  stelem.ref
    IL_0251:  ldloc.s    V_17
    IL_0253:  ldc.i4.4
    IL_0254:  ldstr      bytearray (2C 00 20 00 6E 00 68 00 E3 00 6E 00 20 00 11 01   // ,. .n.h...n. ...
                                    69 00 20 00 74 00 68 00 65 00 6F 00 20 00 62 00   // i. .t.h.e.o. .b.
                                    6C 00 6F 00 63 00 6B 00 20 00 11 01 E3 00 20 00   // l.o.c.k. ..... .
                                    64 00 DD 1E 69 00 20 00 )                         // d...i. .
    IL_0259:  stelem.ref
    IL_025a:  ldloc.s    V_17
    IL_025c:  ldc.i4.5
    IL_025d:  ldloc.s    V_10
    IL_025f:  ldfld      int32 Tvxd.Meps.ToadoResult::moved
    IL_0264:  box        [mscorlib]System.Int32
    IL_0269:  stelem.ref
    IL_026a:  ldloc.s    V_17
    IL_026c:  ldc.i4.6
    IL_026d:  ldstr      bytearray (2C 00 20 00 67 00 69 00 EF 1E 20 00 6E 00 67 00   // ,. .g.i... .n.g.
                                    75 00 79 00 EA 00 6E 00 20 00 )                   // u.y...n. .
    IL_0272:  stelem.ref
    IL_0273:  ldloc.s    V_17
    IL_0275:  ldc.i4.7
    IL_0276:  ldloc.s    V_10
    IL_0278:  ldfld      int32 Tvxd.Meps.ToadoResult::same
    IL_027d:  box        [mscorlib]System.Int32
    IL_0282:  stelem.ref
    IL_0283:  ldloc.s    V_17
    IL_0285:  ldc.i4.8
    IL_0286:  ldstr      bytearray (2C 00 20 00 62 00 CF 1E 20 00 71 00 75 00 61 00   // ,. .b... .q.u.a.
                                    20 00 )                                           //  .
    IL_028b:  stelem.ref
    IL_028c:  ldloc.s    V_17
    IL_028e:  ldc.i4.s   9
    IL_0290:  ldloc.s    V_10
    IL_0292:  ldfld      int32 Tvxd.Meps.ToadoResult::skipped
    IL_0297:  box        [mscorlib]System.Int32
    IL_029c:  stelem.ref
    IL_029d:  ldloc.s    V_17
    IL_029f:  ldc.i4.s   10
    IL_02a1:  ldstr      bytearray (2E 00 20 00 43 00 68 00 A1 1E 79 00 20 00 6C 00   // .. .C.h...y. .l.
                                    A1 1E 69 00 20 00 54 00 2D 00 54 00 4F 00 41 00   // ..i. .T.-.T.O.A.
                                    44 00 4F 00 20 00 73 00 61 00 75 00 20 00 6B 00   // D.O. .s.a.u. .k.
                                    68 00 69 00 20 00 64 00 DD 1E 69 00 20 00 62 00   // h.i. .d...i. .b.
                                    6C 00 6F 00 63 00 6B 00 20 00 11 01 C3 1E 20 00   // l.o.c.k. ..... .
                                    6E 00 68 00 E3 00 6E 00 20 00 63 00 AD 1E 70 00   // n.h...n. .c...p.
                                    20 00 6E 00 68 00 AD 1E 74 00 2E 00 )             //  .n.h...t...
    IL_02a6:  stelem.ref
    IL_02a7:  ldloc.s    V_17
    IL_02a9:  call       string [mscorlib]System.String::Concat(object[])
    IL_02ae:  callvirt   instance void [accoremgd]Autodesk.AutoCAD.EditorInput.Editor::WriteMessage(string)
    IL_02b3:  nop
    IL_02b4:  nop
    IL_02b5:  nop
    IL_02b6:  ret
  } // end of method Toado100::Run

  .method private hidebysig specialname rtspecialname static 
          void  .cctor() cil managed
  {
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  call       class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()
    IL_0005:  stsfld     class [mscorlib]System.Globalization.CultureInfo Tvxd.Meps.Toado100::Inv
    IL_000a:  ret
  } // end of method Toado100::.cctor

  .property string IniPath()
  {
    .get string Tvxd.Meps.Toado100::get_IniPath()
  } // end of property Toado100::IniPath
} // end of class Tvxd.Meps.Toado100

// *********** DISASSEMBLY COMPLETE ***********************
