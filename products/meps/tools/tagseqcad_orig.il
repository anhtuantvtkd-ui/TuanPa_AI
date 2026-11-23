.class public abstract auto ansi sealed beforefieldinit Tvxd.Meps.TagSeqCad073
       extends [mscorlib]System.Object
{
  .method public hidebysig static class [mscorlib]System.Collections.Generic.List`1<class Tvxd.Meps.TagSeqEntry073> 
          Entries(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db,
                  class Tvxd.Meps.TagSnapshot033 snap,
                  valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Matrix3d wcsToUcs) cil managed
  {
    // Code size       289 (0x121)
    .maxstack  3
    .locals init (class [mscorlib]System.Collections.Generic.List`1<class Tvxd.Meps.TagSeqEntry073> V_0,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenCloseTransaction V_1,
             class Tvxd.Meps.TagItem033 V_2,
             class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference V_3,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_4,
             class Tvxd.Meps.TagSeqEntry073 V_5,
             class [mscorlib]System.Collections.Generic.List`1<class Tvxd.Meps.TagSeqEntry073> V_6,
             bool V_7,
             valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class Tvxd.Meps.TagItem033> V_8,
             valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d V_9)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldnull
    IL_0003:  call       bool [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper::op_Equality(class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper,
                                                                                               class [Acdbmgd]Autodesk.AutoCAD.Runtime.DisposableWrapper)
    IL_0008:  brtrue.s   IL_0013

    IL_000a:  ldarg.1
    IL_000b:  ldnull
    IL_000c:  ceq
    IL_000e:  ldc.i4.0
    IL_000f:  ceq
    IL_0011:  br.s       IL_0014

    IL_0013:  ldc.i4.0
    IL_0014:  nop
    IL_0015:  stloc.s    V_7
    IL_0017:  ldloc.s    V_7
    IL_0019:  brtrue.s   IL_0026

    IL_001b:  ldstr      bytearray (4C 00 F1 1E 61 00 20 00 63 00 68 00 CD 1E 6E 00   // L...a. .c.h...n.
                                    20 00 6B 00 68 00 F4 00 6E 00 67 00 20 00 68 00   //  .k.h...n.g. .h.
                                    E3 1E 70 00 20 00 6C 00 C7 1E 2E 00 )             // ..p. .l.....
    IL_0020:  newobj     instance void [mscorlib]System.InvalidOperationException::.ctor(string)
    IL_0025:  throw

    IL_0026:  newobj     instance void class [mscorlib]System.Collections.Generic.List`1<class Tvxd.Meps.TagSeqEntry073>::.ctor()
    IL_002b:  stloc.0
    IL_002c:  ldarg.0
    IL_002d:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database::get_TransactionManager()
    IL_0032:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenCloseTransaction [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.TransactionManager::StartOpenCloseTransaction()
    IL_0037:  stloc.1
    .try
    {
      IL_0038:  nop
      IL_0039:  nop
      IL_003a:  ldarg.1
      IL_003b:  ldfld      class [mscorlib]System.Collections.Generic.List`1<class Tvxd.Meps.TagItem033> Tvxd.Meps.TagSnapshot033::items
      IL_0040:  callvirt   instance valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<!0> class [mscorlib]System.Collections.Generic.List`1<class Tvxd.Meps.TagItem033>::GetEnumerator()
      IL_0045:  stloc.s    V_8
      .try
      {
        IL_0047:  br         IL_00da

        IL_004c:  ldloca.s   V_8
        IL_004e:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class Tvxd.Meps.TagItem033>::get_Current()
        IL_0053:  stloc.2
        IL_0054:  nop
        IL_0055:  ldloc.1
        IL_0056:  ldloc.2
        IL_0057:  ldfld      valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId Tvxd.Meps.TagItem033::owner
        IL_005c:  ldc.i4.0
        IL_005d:  callvirt   instance class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.DBObject [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::GetObject(valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.ObjectId,
                                                                                                                                                                  valuetype [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.OpenMode)
        IL_0062:  castclass  [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference
        IL_0067:  stloc.3
        IL_0068:  ldloc.3
        IL_0069:  callvirt   instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.BlockReference::get_Position()
        IL_006e:  stloc.s    V_9
        IL_0070:  ldloca.s   V_9
        IL_0072:  ldarg.2
        IL_0073:  call       instance valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::TransformBy(valuetype [Acdbmgd]Autodesk.AutoCAD.Geometry.Matrix3d)
        IL_0078:  stloc.s    V_4
        IL_007a:  ldloc.0
        IL_007b:  newobj     instance void Tvxd.Meps.TagSeqEntry073::.ctor()
        IL_0080:  stloc.s    V_5
        IL_0082:  ldloc.s    V_5
        IL_0084:  ldloc.2
        IL_0085:  ldfld      string Tvxd.Meps.TagItem033::handle
        IL_008a:  stfld      string Tvxd.Meps.TagSeqEntry073::id
        IL_008f:  ldloc.s    V_5
        IL_0091:  ldloc.2
        IL_0092:  ldfld      string Tvxd.Meps.TagItem033::blockName
        IL_0097:  stfld      string Tvxd.Meps.TagSeqEntry073::name
        IL_009c:  ldloc.s    V_5
        IL_009e:  ldloc.2
        IL_009f:  ldfld      bool Tvxd.Meps.TagItem033::hasTag
        IL_00a4:  brtrue.s   IL_00a9

        IL_00a6:  ldnull
        IL_00a7:  br.s       IL_00af

        IL_00a9:  ldloc.2
        IL_00aa:  ldfld      string Tvxd.Meps.TagItem033::text
        IL_00af:  nop
        IL_00b0:  stfld      string Tvxd.Meps.TagSeqEntry073::oldText
        IL_00b5:  ldloc.s    V_5
        IL_00b7:  ldloca.s   V_4
        IL_00b9:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_X()
        IL_00be:  stfld      float64 Tvxd.Meps.TagSeqEntry073::x
        IL_00c3:  ldloc.s    V_5
        IL_00c5:  ldloca.s   V_4
        IL_00c7:  call       instance float64 [Acdbmgd]Autodesk.AutoCAD.Geometry.Point3d::get_Y()
        IL_00cc:  stfld      float64 Tvxd.Meps.TagSeqEntry073::y
        IL_00d1:  ldloc.s    V_5
        IL_00d3:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<class Tvxd.Meps.TagSeqEntry073>::Add(!0)
        IL_00d8:  nop
        IL_00d9:  nop
        IL_00da:  ldloca.s   V_8
        IL_00dc:  call       instance bool valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class Tvxd.Meps.TagItem033>::MoveNext()
        IL_00e1:  stloc.s    V_7
        IL_00e3:  ldloc.s    V_7
        IL_00e5:  brtrue     IL_004c

        IL_00ea:  leave.s    IL_00fb

      }  // end .try
      finally
      {
        IL_00ec:  ldloca.s   V_8
        IL_00ee:  constrained. valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class Tvxd.Meps.TagItem033>
        IL_00f4:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_00f9:  nop
        IL_00fa:  endfinally
      }  // end handler
      IL_00fb:  nop
      IL_00fc:  ldloc.1
      IL_00fd:  callvirt   instance void [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Transaction::Commit()
      IL_0102:  nop
      IL_0103:  nop
      IL_0104:  leave.s    IL_0118

    }  // end .try
    finally
    {
      IL_0106:  ldloc.1
      IL_0107:  ldnull
      IL_0108:  ceq
      IL_010a:  stloc.s    V_7
      IL_010c:  ldloc.s    V_7
      IL_010e:  brtrue.s   IL_0117

      IL_0110:  ldloc.1
      IL_0111:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_0116:  nop
      IL_0117:  endfinally
    }  // end handler
    IL_0118:  nop
    IL_0119:  ldloc.0
    IL_011a:  stloc.s    V_6
    IL_011c:  br.s       IL_011e

    IL_011e:  ldloc.s    V_6
    IL_0120:  ret
  } // end of method TagSeqCad073::Entries

  .method public hidebysig static class Tvxd.Meps.TagApply033 
          Apply(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database db,
                class Tvxd.Meps.TagSnapshot033 snap,
                class [mscorlib]System.Collections.Generic.IList`1<class Tvxd.Meps.TagSeqRow073> plan,
                float64 height,
                class [mscorlib]System.Action`1<string> guard) cil managed
  {
    // Code size       318 (0x13e)
    .maxstack  5
    .locals init (class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> V_0,
             class Tvxd.Meps.TagSeqRow073 V_1,
             class [mscorlib]System.Collections.Generic.List`1<string> V_2,
             class Tvxd.Meps.TagItem033 V_3,
             string V_4,
             class Tvxd.Meps.TagApply033 V_5,
             bool V_6,
             class [mscorlib]System.Collections.Generic.IEnumerator`1<class Tvxd.Meps.TagSeqRow073> V_7,
             valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class Tvxd.Meps.TagItem033> V_8)
    IL_0000:  nop
    IL_0001:  ldarg.1
    IL_0002:  brfalse.s  IL_001c

    IL_0004:  ldarg.2
    IL_0005:  brfalse.s  IL_001c

    IL_0007:  ldarg.2
    IL_0008:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.ICollection`1<class Tvxd.Meps.TagSeqRow073>::get_Count()
    IL_000d:  ldarg.1
    IL_000e:  ldfld      class [mscorlib]System.Collections.Generic.List`1<class Tvxd.Meps.TagItem033> Tvxd.Meps.TagSnapshot033::items
    IL_0013:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.List`1<class Tvxd.Meps.TagItem033>::get_Count()
    IL_0018:  ceq
    IL_001a:  br.s       IL_001d

    IL_001c:  ldc.i4.0
    IL_001d:  nop
    IL_001e:  stloc.s    V_6
    IL_0020:  ldloc.s    V_6
    IL_0022:  brtrue.s   IL_002f

    IL_0024:  ldstr      bytearray (42 00 A3 1E 6E 00 67 00 20 00 11 01 E1 00 6E 00   // B...n.g. .....n.
                                    68 00 20 00 73 00 D1 1E 20 00 6B 00 68 00 F4 00   // h. .s... .k.h...
                                    6E 00 67 00 20 00 6B 00 68 00 DB 1E 70 00 20 00   // n.g. .k.h...p. .
                                    6C 00 F1 1E 61 00 20 00 63 00 68 00 CD 1E 6E 00   // l...a. .c.h...n.
                                    3B 00 20 00 62 00 A3 1E 6E 00 20 00 76 00 BD 1E   // ;. .b...n. .v...
                                    20 00 6B 00 68 00 F4 00 6E 00 67 00 20 00 11 01   //  .k.h...n.g. ...
                                    D5 1E 69 00 2E 00 )                               // ..i...
    IL_0029:  newobj     instance void [mscorlib]System.InvalidOperationException::.ctor(string)
    IL_002e:  throw

    IL_002f:  call       class [mscorlib]System.StringComparer [mscorlib]System.StringComparer::get_OrdinalIgnoreCase()
    IL_0034:  newobj     instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::.ctor(class [mscorlib]System.Collections.Generic.IEqualityComparer`1<!0>)
    IL_0039:  stloc.0
    IL_003a:  nop
    IL_003b:  ldarg.2
    IL_003c:  callvirt   instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<class Tvxd.Meps.TagSeqRow073>::GetEnumerator()
    IL_0041:  stloc.s    V_7
    .try
    {
      IL_0043:  br.s       IL_0091

      IL_0045:  ldloc.s    V_7
      IL_0047:  callvirt   instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<class Tvxd.Meps.TagSeqRow073>::get_Current()
      IL_004c:  stloc.1
      IL_004d:  nop
      IL_004e:  ldloc.1
      IL_004f:  brfalse.s  IL_006a

      IL_0051:  ldloc.1
      IL_0052:  ldfld      string Tvxd.Meps.TagSeqRow073::id
      IL_0057:  brfalse.s  IL_006a

      IL_0059:  ldloc.0
      IL_005a:  ldloc.1
      IL_005b:  ldfld      string Tvxd.Meps.TagSeqRow073::id
      IL_0060:  callvirt   instance bool class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::ContainsKey(!0)
      IL_0065:  ldc.i4.0
      IL_0066:  ceq
      IL_0068:  br.s       IL_006b

      IL_006a:  ldc.i4.0
      IL_006b:  nop
      IL_006c:  stloc.s    V_6
      IL_006e:  ldloc.s    V_6
      IL_0070:  brtrue.s   IL_007d

      IL_0072:  ldstr      bytearray (42 00 A3 1E 6E 00 67 00 20 00 11 01 E1 00 6E 00   // B...n.g. .....n.
                                      68 00 20 00 73 00 D1 1E 20 00 63 00 F3 00 20 00   // h. .s... .c... .
                                      62 00 6C 00 6F 00 63 00 6B 00 20 00 74 00 72 00   // b.l.o.c.k. .t.r.
                                      F9 00 6E 00 67 00 2F 00 74 00 68 00 69 00 BF 1E   // ..n.g./.t.h.i...
                                      75 00 3B 00 20 00 62 00 A3 1E 6E 00 20 00 76 00   // u.;. .b...n. .v.
                                      BD 1E 20 00 6B 00 68 00 F4 00 6E 00 67 00 20 00   // .. .k.h...n.g. .
                                      11 01 D5 1E 69 00 2E 00 )                         // ....i...
      IL_0077:  newobj     instance void [mscorlib]System.InvalidOperationException::.ctor(string)
      IL_007c:  throw

      IL_007d:  ldloc.0
      IL_007e:  ldloc.1
      IL_007f:  ldfld      string Tvxd.Meps.TagSeqRow073::id
      IL_0084:  ldloc.1
      IL_0085:  ldfld      string Tvxd.Meps.TagSeqRow073::newText
      IL_008a:  callvirt   instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::set_Item(!0,
                                                                                                                          !1)
      IL_008f:  nop
      IL_0090:  nop
      IL_0091:  ldloc.s    V_7
      IL_0093:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
      IL_0098:  stloc.s    V_6
      IL_009a:  ldloc.s    V_6
      IL_009c:  brtrue.s   IL_0045

      IL_009e:  leave.s    IL_00b4

    }  // end .try
    finally
    {
      IL_00a0:  ldloc.s    V_7
      IL_00a2:  ldnull
      IL_00a3:  ceq
      IL_00a5:  stloc.s    V_6
      IL_00a7:  ldloc.s    V_6
      IL_00a9:  brtrue.s   IL_00b3

      IL_00ab:  ldloc.s    V_7
      IL_00ad:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_00b2:  nop
      IL_00b3:  endfinally
    }  // end handler
    IL_00b4:  nop
    IL_00b5:  newobj     instance void class [mscorlib]System.Collections.Generic.List`1<string>::.ctor()
    IL_00ba:  stloc.2
    IL_00bb:  nop
    IL_00bc:  ldarg.1
    IL_00bd:  ldfld      class [mscorlib]System.Collections.Generic.List`1<class Tvxd.Meps.TagItem033> Tvxd.Meps.TagSnapshot033::items
    IL_00c2:  callvirt   instance valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<!0> class [mscorlib]System.Collections.Generic.List`1<class Tvxd.Meps.TagItem033>::GetEnumerator()
    IL_00c7:  stloc.s    V_8
    .try
    {
      IL_00c9:  br.s       IL_010d

      IL_00cb:  ldloca.s   V_8
      IL_00cd:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class Tvxd.Meps.TagItem033>::get_Current()
      IL_00d2:  stloc.3
      IL_00d3:  nop
      IL_00d4:  ldloc.0
      IL_00d5:  ldloc.3
      IL_00d6:  ldfld      string Tvxd.Meps.TagItem033::handle
      IL_00db:  ldloca.s   V_4
      IL_00dd:  callvirt   instance bool class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::TryGetValue(!0,
                                                                                                                             !1&)
      IL_00e2:  stloc.s    V_6
      IL_00e4:  ldloc.s    V_6
      IL_00e6:  brtrue.s   IL_0103

      IL_00e8:  ldstr      "Block "
      IL_00ed:  ldloc.3
      IL_00ee:  ldfld      string Tvxd.Meps.TagItem033::handle
      IL_00f3:  ldstr      bytearray (20 00 6B 00 68 00 F4 00 6E 00 67 00 20 00 63 00   //  .k.h...n.g. .c.
                                      F3 00 20 00 74 00 72 00 6F 00 6E 00 67 00 20 00   // .. .t.r.o.n.g. .
                                      62 00 A3 1E 6E 00 67 00 20 00 11 01 E1 00 6E 00   // b...n.g. .....n.
                                      68 00 20 00 73 00 D1 1E 3B 00 20 00 62 00 A3 1E   // h. .s...;. .b...
                                      6E 00 20 00 76 00 BD 1E 20 00 6B 00 68 00 F4 00   // n. .v... .k.h...
                                      6E 00 67 00 20 00 11 01 D5 1E 69 00 2E 00 )       // n.g. .....i...
      IL_00f8:  call       string [mscorlib]System.String::Concat(string,
                                                                  string,
                                                                  string)
      IL_00fd:  newobj     instance void [mscorlib]System.InvalidOperationException::.ctor(string)
      IL_0102:  throw

      IL_0103:  ldloc.2
      IL_0104:  ldloc.s    V_4
      IL_0106:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_010b:  nop
      IL_010c:  nop
      IL_010d:  ldloca.s   V_8
      IL_010f:  call       instance bool valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class Tvxd.Meps.TagItem033>::MoveNext()
      IL_0114:  stloc.s    V_6
      IL_0116:  ldloc.s    V_6
      IL_0118:  brtrue.s   IL_00cb

      IL_011a:  leave.s    IL_012b

    }  // end .try
    finally
    {
      IL_011c:  ldloca.s   V_8
      IL_011e:  constrained. valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class Tvxd.Meps.TagItem033>
      IL_0124:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_0129:  nop
      IL_012a:  endfinally
    }  // end handler
    IL_012b:  nop
    IL_012c:  ldarg.0
    IL_012d:  ldarg.1
    IL_012e:  ldloc.2
    IL_012f:  ldarg.3
    IL_0130:  ldarg.s    guard
    IL_0132:  call       class Tvxd.Meps.TagApply033 Tvxd.Meps.TagCad033::ApplyTexts(class [Acdbmgd]Autodesk.AutoCAD.DatabaseServices.Database,
                                                                                     class Tvxd.Meps.TagSnapshot033,
                                                                                     class [mscorlib]System.Collections.Generic.IList`1<string>,
                                                                                     float64,
                                                                                     class [mscorlib]System.Action`1<string>)
    IL_0137:  stloc.s    V_5
    IL_0139:  br.s       IL_013b

    IL_013b:  ldloc.s    V_5
    IL_013d:  ret
  } // end of method TagSeqCad073::Apply

} // end of class Tvxd.Meps.TagSeqCad073