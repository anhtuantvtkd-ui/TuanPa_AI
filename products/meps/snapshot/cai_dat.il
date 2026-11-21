
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.8.3928.0
//  Copyright (c) Microsoft Corporation.  All rights reserved.



// Metadata version: v4.0.30319
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly extern System.Web.Extensions
{
  .publickeytoken = (31 BF 38 56 AD 36 4E 35 )                         // 1.8V.6N5
  .ver 4:0:0:0
}
.assembly extern System.Core
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly extern System
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly InstallR27.compile
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module InstallR27.compile.exe
// MVID: {3AB2FE80-DDC0-4ED2-B647-0A5D7F6439BB}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x051C0000


// =============== CLASS MEMBERS DECLARATION ===================

.class private auto ansi beforefieldinit InstallUpgrade096
       extends [mscorlib]System.Object
{
  .class auto ansi nested private beforefieldinit MoveRecord
         extends [mscorlib]System.Object
  {
    .field public string old_path
    .field public string new_path
    .field public class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> files
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ret
    } // end of method MoveRecord::.ctor

  } // end of class MoveRecord

  .class auto ansi sealed nested private beforefieldinit '<Files>d__0'
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEnumerable`1<string>,
                    [mscorlib]System.Collections.IEnumerable,
                    class [mscorlib]System.Collections.Generic.IEnumerator`1<string>,
                    [mscorlib]System.Collections.IEnumerator,
                    [mscorlib]System.IDisposable
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field private string '<>2__current'
    .field private int32 '<>1__state'
    .field private int32 '<>l__initialThreadId'
    .field public string root
    .field public string '<>3__root'
    .field public string '<path>5__1'
    .field public string '<f>5__2'
    .field public string[] '<>7__wrap4'
    .field public int32 '<>7__wrap5'
    .field public class [mscorlib]System.Collections.Generic.IEnumerator`1<string> '<>7__wrap6'
    .method private hidebysig newslot virtual final 
            instance class [mscorlib]System.Collections.Generic.IEnumerator`1<string> 
            'System.Collections.Generic.IEnumerable<System.String>.GetEnumerator'() cil managed
    {
      .custom instance void [mscorlib]System.Diagnostics.DebuggerHiddenAttribute::.ctor() = ( 01 00 00 00 ) 
      .override  method instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<string>::GetEnumerator()
      // Code size       70 (0x46)
      .maxstack  2
      .locals init (class InstallUpgrade096/'<Files>d__0' V_0,
               class [mscorlib]System.Collections.Generic.IEnumerator`1<string> V_1,
               bool V_2)
      IL_0000:  call       int32 [mscorlib]System.Environment::get_CurrentManagedThreadId()
      IL_0005:  ldarg.0
      IL_0006:  ldfld      int32 InstallUpgrade096/'<Files>d__0'::'<>l__initialThreadId'
      IL_000b:  bne.un.s   IL_001c

      IL_000d:  ldarg.0
      IL_000e:  ldfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
      IL_0013:  ldc.i4.s   -2
      IL_0015:  ceq
      IL_0017:  ldc.i4.0
      IL_0018:  ceq
      IL_001a:  br.s       IL_001d

      IL_001c:  ldc.i4.1
      IL_001d:  nop
      IL_001e:  stloc.2
      IL_001f:  ldloc.2
      IL_0020:  brtrue.s   IL_002d

      IL_0022:  ldarg.0
      IL_0023:  ldc.i4.0
      IL_0024:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
      IL_0029:  ldarg.0
      IL_002a:  stloc.0
      IL_002b:  br.s       IL_0034

      IL_002d:  ldc.i4.0
      IL_002e:  newobj     instance void InstallUpgrade096/'<Files>d__0'::.ctor(int32)
      IL_0033:  stloc.0
      IL_0034:  ldloc.0
      IL_0035:  ldarg.0
      IL_0036:  ldfld      string InstallUpgrade096/'<Files>d__0'::'<>3__root'
      IL_003b:  stfld      string InstallUpgrade096/'<Files>d__0'::root
      IL_0040:  ldloc.0
      IL_0041:  stloc.1
      IL_0042:  br.s       IL_0044

      IL_0044:  ldloc.1
      IL_0045:  ret
    } // end of method '<Files>d__0'::'System.Collections.Generic.IEnumerable<System.String>.GetEnumerator'

    .method private hidebysig newslot virtual final 
            instance class [mscorlib]System.Collections.IEnumerator 
            System.Collections.IEnumerable.GetEnumerator() cil managed
    {
      .custom instance void [mscorlib]System.Diagnostics.DebuggerHiddenAttribute::.ctor() = ( 01 00 00 00 ) 
      .override [mscorlib]System.Collections.IEnumerable::GetEnumerator
      // Code size       11 (0xb)
      .maxstack  1
      .locals init (class [mscorlib]System.Collections.IEnumerator V_0)
      IL_0000:  ldarg.0
      IL_0001:  call       instance class [mscorlib]System.Collections.Generic.IEnumerator`1<string> InstallUpgrade096/'<Files>d__0'::'System.Collections.Generic.IEnumerable<System.String>.GetEnumerator'()
      IL_0006:  stloc.0
      IL_0007:  br.s       IL_0009

      IL_0009:  ldloc.0
      IL_000a:  ret
    } // end of method '<Files>d__0'::System.Collections.IEnumerable.GetEnumerator

    .method private hidebysig newslot virtual final 
            instance bool  MoveNext() cil managed
    {
      .override [mscorlib]System.Collections.IEnumerator::MoveNext
      // Code size       366 (0x16e)
      .maxstack  3
      .locals init (bool V_0,
               int32 V_1,
               bool V_2)
      .try
      {
        IL_0000:  ldarg.0
        IL_0001:  ldfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
        IL_0006:  stloc.1
        IL_0007:  ldloc.1
        IL_0008:  switch     ( 
                              IL_002d,
                              IL_002f,
                              IL_002f,
                              IL_0023,
                              IL_0028)
        IL_0021:  br.s       IL_002f

        IL_0023:  br         IL_00f3

        IL_0028:  br         IL_012a

        IL_002d:  br.s       IL_0034

        IL_002f:  br         IL_015f

        IL_0034:  ldarg.0
        IL_0035:  ldc.i4.m1
        IL_0036:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
        IL_003b:  nop
        IL_003c:  nop
        IL_003d:  ldarg.0
        IL_003e:  ldc.i4.1
        IL_003f:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
        IL_0044:  ldarg.0
        IL_0045:  ldarg.0
        IL_0046:  ldfld      string InstallUpgrade096/'<Files>d__0'::root
        IL_004b:  call       string[] [mscorlib]System.IO.Directory::GetFileSystemEntries(string)
        IL_0050:  stfld      string[] InstallUpgrade096/'<Files>d__0'::'<>7__wrap4'
        IL_0055:  ldarg.0
        IL_0056:  ldc.i4.0
        IL_0057:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>7__wrap5'
        IL_005c:  br         IL_0140

        IL_0061:  ldarg.0
        IL_0062:  ldarg.0
        IL_0063:  ldfld      string[] InstallUpgrade096/'<Files>d__0'::'<>7__wrap4'
        IL_0068:  ldarg.0
        IL_0069:  ldfld      int32 InstallUpgrade096/'<Files>d__0'::'<>7__wrap5'
        IL_006e:  ldelem.ref
        IL_006f:  stfld      string InstallUpgrade096/'<Files>d__0'::'<path>5__1'
        IL_0074:  nop
        IL_0075:  ldarg.0
        IL_0076:  ldfld      string InstallUpgrade096/'<Files>d__0'::'<path>5__1'
        IL_007b:  call       valuetype [mscorlib]System.IO.FileAttributes [mscorlib]System.IO.File::GetAttributes(string)
        IL_0080:  ldc.i4     0x400
        IL_0085:  and
        IL_0086:  ldc.i4.0
        IL_0087:  ceq
        IL_0089:  stloc.2
        IL_008a:  ldloc.2
        IL_008b:  brtrue.s   IL_0098

        IL_008d:  ldstr      "Linked payload not supported"
        IL_0092:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
        IL_0097:  throw

        IL_0098:  ldarg.0
        IL_0099:  ldfld      string InstallUpgrade096/'<Files>d__0'::'<path>5__1'
        IL_009e:  call       bool [mscorlib]System.IO.Directory::Exists(string)
        IL_00a3:  ldc.i4.0
        IL_00a4:  ceq
        IL_00a6:  stloc.2
        IL_00a7:  ldloc.2
        IL_00a8:  brtrue.s   IL_0113

        IL_00aa:  nop
        IL_00ab:  nop
        IL_00ac:  ldarg.0
        IL_00ad:  ldarg.0
        IL_00ae:  ldfld      string InstallUpgrade096/'<Files>d__0'::'<path>5__1'
        IL_00b3:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<string> InstallUpgrade096::Files(string)
        IL_00b8:  callvirt   instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<string>::GetEnumerator()
        IL_00bd:  stfld      class [mscorlib]System.Collections.Generic.IEnumerator`1<string> InstallUpgrade096/'<Files>d__0'::'<>7__wrap6'
        IL_00c2:  ldarg.0
        IL_00c3:  ldc.i4.2
        IL_00c4:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
        IL_00c9:  br.s       IL_00fa

        IL_00cb:  ldarg.0
        IL_00cc:  ldarg.0
        IL_00cd:  ldfld      class [mscorlib]System.Collections.Generic.IEnumerator`1<string> InstallUpgrade096/'<Files>d__0'::'<>7__wrap6'
        IL_00d2:  callvirt   instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<string>::get_Current()
        IL_00d7:  stfld      string InstallUpgrade096/'<Files>d__0'::'<f>5__2'
        IL_00dc:  ldarg.0
        IL_00dd:  ldarg.0
        IL_00de:  ldfld      string InstallUpgrade096/'<Files>d__0'::'<f>5__2'
        IL_00e3:  stfld      string InstallUpgrade096/'<Files>d__0'::'<>2__current'
        IL_00e8:  ldarg.0
        IL_00e9:  ldc.i4.3
        IL_00ea:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
        IL_00ef:  ldc.i4.1
        IL_00f0:  stloc.0
        IL_00f1:  leave.s    IL_016b

        IL_00f3:  ldarg.0
        IL_00f4:  ldc.i4.2
        IL_00f5:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
        IL_00fa:  ldarg.0
        IL_00fb:  ldfld      class [mscorlib]System.Collections.Generic.IEnumerator`1<string> InstallUpgrade096/'<Files>d__0'::'<>7__wrap6'
        IL_0100:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
        IL_0105:  stloc.2
        IL_0106:  ldloc.2
        IL_0107:  brtrue.s   IL_00cb

        IL_0109:  ldarg.0
        IL_010a:  call       instance void InstallUpgrade096/'<Files>d__0'::'<>m__Finally7'()
        IL_010f:  nop
        IL_0110:  nop
        IL_0111:  br.s       IL_0131

        IL_0113:  ldarg.0
        IL_0114:  ldarg.0
        IL_0115:  ldfld      string InstallUpgrade096/'<Files>d__0'::'<path>5__1'
        IL_011a:  stfld      string InstallUpgrade096/'<Files>d__0'::'<>2__current'
        IL_011f:  ldarg.0
        IL_0120:  ldc.i4.4
        IL_0121:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
        IL_0126:  ldc.i4.1
        IL_0127:  stloc.0
        IL_0128:  leave.s    IL_016b

        IL_012a:  ldarg.0
        IL_012b:  ldc.i4.1
        IL_012c:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
        IL_0131:  nop
        IL_0132:  ldarg.0
        IL_0133:  ldarg.0
        IL_0134:  ldfld      int32 InstallUpgrade096/'<Files>d__0'::'<>7__wrap5'
        IL_0139:  ldc.i4.1
        IL_013a:  add
        IL_013b:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>7__wrap5'
        IL_0140:  ldarg.0
        IL_0141:  ldfld      int32 InstallUpgrade096/'<Files>d__0'::'<>7__wrap5'
        IL_0146:  ldarg.0
        IL_0147:  ldfld      string[] InstallUpgrade096/'<Files>d__0'::'<>7__wrap4'
        IL_014c:  ldlen
        IL_014d:  conv.i4
        IL_014e:  clt
        IL_0150:  stloc.2
        IL_0151:  ldloc.2
        IL_0152:  brtrue     IL_0061

        IL_0157:  ldarg.0
        IL_0158:  call       instance void InstallUpgrade096/'<Files>d__0'::'<>m__Finally3'()
        IL_015d:  nop
        IL_015e:  nop
        IL_015f:  ldc.i4.0
        IL_0160:  stloc.0
        IL_0161:  leave.s    IL_016b

      }  // end .try
      fault
      {
        IL_0163:  ldarg.0
        IL_0164:  call       instance void InstallUpgrade096/'<Files>d__0'::System.IDisposable.Dispose()
        IL_0169:  nop
        IL_016a:  endfinally
      }  // end handler
      IL_016b:  nop
      IL_016c:  ldloc.0
      IL_016d:  ret
    } // end of method '<Files>d__0'::MoveNext

    .method private hidebysig newslot specialname virtual final 
            instance string  'System.Collections.Generic.IEnumerator<System.String>.get_Current'() cil managed
    {
      .custom instance void [mscorlib]System.Diagnostics.DebuggerHiddenAttribute::.ctor() = ( 01 00 00 00 ) 
      .override  method instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<string>::get_Current()
      // Code size       11 (0xb)
      .maxstack  1
      .locals init (string V_0)
      IL_0000:  ldarg.0
      IL_0001:  ldfld      string InstallUpgrade096/'<Files>d__0'::'<>2__current'
      IL_0006:  stloc.0
      IL_0007:  br.s       IL_0009

      IL_0009:  ldloc.0
      IL_000a:  ret
    } // end of method '<Files>d__0'::'System.Collections.Generic.IEnumerator<System.String>.get_Current'

    .method private hidebysig newslot virtual final 
            instance void  System.Collections.IEnumerator.Reset() cil managed
    {
      .custom instance void [mscorlib]System.Diagnostics.DebuggerHiddenAttribute::.ctor() = ( 01 00 00 00 ) 
      .override [mscorlib]System.Collections.IEnumerator::Reset
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  newobj     instance void [mscorlib]System.NotSupportedException::.ctor()
      IL_0005:  throw
    } // end of method '<Files>d__0'::System.Collections.IEnumerator.Reset

    .method private hidebysig newslot virtual final 
            instance void  System.IDisposable.Dispose() cil managed
    {
      .override [mscorlib]System.IDisposable::Dispose
      // Code size       93 (0x5d)
      .maxstack  2
      .locals init (int32 V_0)
      IL_0000:  ldarg.0
      IL_0001:  ldfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
      IL_0006:  stloc.0
      IL_0007:  ldloc.0
      IL_0008:  ldc.i4.1
      IL_0009:  sub
      IL_000a:  switch     ( 
                            IL_0027,
                            IL_0023,
                            IL_0021,
                            IL_0025)
      IL_001f:  br.s       IL_0029

      IL_0021:  br.s       IL_002b

      IL_0023:  br.s       IL_002b

      IL_0025:  br.s       IL_002b

      IL_0027:  br.s       IL_002b

      IL_0029:  br.s       IL_005c

      IL_002b:  ldarg.0
      IL_002c:  ldfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
      IL_0031:  stloc.0
      IL_0032:  ldloc.0
      IL_0033:  ldc.i4.2
      IL_0034:  sub
      IL_0035:  switch     ( 
                            IL_0046,
                            IL_0044)
      IL_0042:  br.s       IL_0048

      IL_0044:  br.s       IL_004a

      IL_0046:  br.s       IL_004a

      IL_0048:  br.s       IL_0055

      .try
      {
        IL_004a:  leave.s    IL_0054

      }  // end .try
      finally
      {
        IL_004c:  ldarg.0
        IL_004d:  call       instance void InstallUpgrade096/'<Files>d__0'::'<>m__Finally7'()
        IL_0052:  nop
        IL_0053:  endfinally
      }  // end handler
      IL_0054:  nop
      IL_0055:  ldarg.0
      IL_0056:  call       instance void InstallUpgrade096/'<Files>d__0'::'<>m__Finally3'()
      IL_005b:  nop
      IL_005c:  ret
    } // end of method '<Files>d__0'::System.IDisposable.Dispose

    .method private hidebysig newslot specialname virtual final 
            instance object  System.Collections.IEnumerator.get_Current() cil managed
    {
      .custom instance void [mscorlib]System.Diagnostics.DebuggerHiddenAttribute::.ctor() = ( 01 00 00 00 ) 
      .override [mscorlib]System.Collections.IEnumerator::get_Current
      // Code size       11 (0xb)
      .maxstack  1
      .locals init (object V_0)
      IL_0000:  ldarg.0
      IL_0001:  ldfld      string InstallUpgrade096/'<Files>d__0'::'<>2__current'
      IL_0006:  stloc.0
      IL_0007:  br.s       IL_0009

      IL_0009:  ldloc.0
      IL_000a:  ret
    } // end of method '<Files>d__0'::System.Collections.IEnumerator.get_Current

    .method public hidebysig specialname rtspecialname 
            instance void  .ctor(int32 '<>1__state') cil managed
    {
      .custom instance void [mscorlib]System.Diagnostics.DebuggerHiddenAttribute::.ctor() = ( 01 00 00 00 ) 
      // Code size       25 (0x19)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ldarg.0
      IL_0007:  ldarg.1
      IL_0008:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
      IL_000d:  ldarg.0
      IL_000e:  call       int32 [mscorlib]System.Environment::get_CurrentManagedThreadId()
      IL_0013:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>l__initialThreadId'
      IL_0018:  ret
    } // end of method '<Files>d__0'::.ctor

    .method private hidebysig instance void 
            '<>m__Finally3'() cil managed
    {
      // Code size       8 (0x8)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldc.i4.m1
      IL_0002:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
      IL_0007:  ret
    } // end of method '<Files>d__0'::'<>m__Finally3'

    .method private hidebysig instance void 
            '<>m__Finally7'() cil managed
    {
      // Code size       33 (0x21)
      .maxstack  2
      .locals init (bool V_0)
      IL_0000:  ldarg.0
      IL_0001:  ldc.i4.1
      IL_0002:  stfld      int32 InstallUpgrade096/'<Files>d__0'::'<>1__state'
      IL_0007:  ldarg.0
      IL_0008:  ldfld      class [mscorlib]System.Collections.Generic.IEnumerator`1<string> InstallUpgrade096/'<Files>d__0'::'<>7__wrap6'
      IL_000d:  ldnull
      IL_000e:  ceq
      IL_0010:  stloc.0
      IL_0011:  ldloc.0
      IL_0012:  brtrue.s   IL_0020

      IL_0014:  ldarg.0
      IL_0015:  ldfld      class [mscorlib]System.Collections.Generic.IEnumerator`1<string> InstallUpgrade096/'<Files>d__0'::'<>7__wrap6'
      IL_001a:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_001f:  nop
      IL_0020:  ret
    } // end of method '<Files>d__0'::'<>m__Finally7'

    .property instance string 'System.Collections.Generic.IEnumerator<System.String>.Current'()
    {
      .get instance string InstallUpgrade096/'<Files>d__0'::'System.Collections.Generic.IEnumerator<System.String>.get_Current'()
    } // end of property '<Files>d__0'::'System.Collections.Generic.IEnumerator<System.String>.Current'
    .property instance object System.Collections.IEnumerator.Current()
    {
      .get instance object InstallUpgrade096/'<Files>d__0'::System.Collections.IEnumerator.get_Current()
    } // end of property '<Files>d__0'::System.Collections.IEnumerator.Current
  } // end of class '<Files>d__0'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClassb'
         extends [mscorlib]System.Object
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .field public int32 offset
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClassb'::.ctor

    .method public hidebysig instance string 
            '<Hashes>b__a'(string f) cil managed
    {
      // Code size       17 (0x11)
      .maxstack  2
      .locals init (string V_0)
      IL_0000:  ldarg.1
      IL_0001:  ldarg.0
      IL_0002:  ldfld      int32 InstallUpgrade096/'<>c__DisplayClassb'::offset
      IL_0007:  callvirt   instance string [mscorlib]System.String::Substring(int32)
      IL_000c:  stloc.0
      IL_000d:  br.s       IL_000f

      IL_000f:  ldloc.0
      IL_0010:  ret
    } // end of method '<>c__DisplayClassb'::'<Hashes>b__a'

  } // end of class '<>c__DisplayClassb'

  .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass1c'
         extends [mscorlib]System.Object
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    .class auto ansi sealed nested private beforefieldinit '<>c__DisplayClass1e'
           extends [mscorlib]System.Object
    {
      .field public class InstallUpgrade096/'<>c__DisplayClass1c' 'CS$<>8__locals1d'
      .field public valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string> pair
      .method public hidebysig specialname rtspecialname 
              instance void  .ctor() cil managed
      {
        // Code size       7 (0x7)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
        IL_0006:  ret
      } // end of method '<>c__DisplayClass1e'::.ctor

      .method public hidebysig instance bool 
              '<Run>b__14'(string p) cil managed
      {
        // Code size       46 (0x2e)
        .maxstack  3
        .locals init (bool V_0)
        IL_0000:  ldarg.0
        IL_0001:  ldflda     valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string> InstallUpgrade096/'<>c__DisplayClass1c'/'<>c__DisplayClass1e'::pair
        IL_0006:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
        IL_000b:  ldarg.1
        IL_000c:  ldc.i4.5
        IL_000d:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                           valuetype [mscorlib]System.StringComparison)
        IL_0012:  brtrue.s   IL_0027

        IL_0014:  ldarg.1
        IL_0015:  ldarg.0
        IL_0016:  ldflda     valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string> InstallUpgrade096/'<>c__DisplayClass1c'/'<>c__DisplayClass1e'::pair
        IL_001b:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
        IL_0020:  call       bool InstallUpgrade096::Inside(string,
                                                            string)
        IL_0025:  br.s       IL_0028

        IL_0027:  ldc.i4.1
        IL_0028:  nop
        IL_0029:  stloc.0
        IL_002a:  br.s       IL_002c

        IL_002c:  ldloc.0
        IL_002d:  ret
      } // end of method '<>c__DisplayClass1e'::'<Run>b__14'

    } // end of class '<>c__DisplayClass1e'

    .field public class [mscorlib]System.Collections.Generic.List`1<string> preserveRoots
    .field public string localPointer
    .field public string pointerValue
    .method public hidebysig specialname rtspecialname 
            instance void  .ctor() cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ret
    } // end of method '<>c__DisplayClass1c'::.ctor

    .method public hidebysig instance bool 
            '<Run>b__12'(string p) cil managed
    {
      // Code size       36 (0x24)
      .maxstack  3
      .locals init (bool V_0)
      IL_0000:  ldarg.0
      IL_0001:  ldfld      string InstallUpgrade096/'<>c__DisplayClass1c'::localPointer
      IL_0006:  ldarg.1
      IL_0007:  ldc.i4.5
      IL_0008:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_000d:  brtrue.s   IL_001d

      IL_000f:  ldarg.1
      IL_0010:  ldarg.0
      IL_0011:  ldfld      string InstallUpgrade096/'<>c__DisplayClass1c'::localPointer
      IL_0016:  call       bool InstallUpgrade096::Inside(string,
                                                          string)
      IL_001b:  br.s       IL_001e

      IL_001d:  ldc.i4.1
      IL_001e:  nop
      IL_001f:  stloc.0
      IL_0020:  br.s       IL_0022

      IL_0022:  ldloc.0
      IL_0023:  ret
    } // end of method '<>c__DisplayClass1c'::'<Run>b__12'

    .method public hidebysig instance bool 
            '<Run>b__13'(valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string> pair) cil managed
    {
      // Code size       114 (0x72)
      .maxstack  3
      .locals init (class InstallUpgrade096/'<>c__DisplayClass1c'/'<>c__DisplayClass1e' V_0,
               bool V_1)
      IL_0000:  newobj     instance void InstallUpgrade096/'<>c__DisplayClass1c'/'<>c__DisplayClass1e'::.ctor()
      IL_0005:  stloc.0
      IL_0006:  ldloc.0
      IL_0007:  ldarg.0
      IL_0008:  stfld      class InstallUpgrade096/'<>c__DisplayClass1c' InstallUpgrade096/'<>c__DisplayClass1c'/'<>c__DisplayClass1e'::'CS$<>8__locals1d'
      IL_000d:  ldloc.0
      IL_000e:  ldarg.1
      IL_000f:  stfld      valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string> InstallUpgrade096/'<>c__DisplayClass1c'/'<>c__DisplayClass1e'::pair
      IL_0014:  ldsfld     class [System.Core]System.Collections.Generic.HashSet`1<string> InstallUpgrade096::ManagedSdnl
      IL_0019:  ldloc.0
      IL_001a:  ldflda     valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string> InstallUpgrade096/'<>c__DisplayClass1c'/'<>c__DisplayClass1e'::pair
      IL_001f:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
      IL_0024:  callvirt   instance bool class [System.Core]System.Collections.Generic.HashSet`1<string>::Contains(!0)
      IL_0029:  brtrue.s   IL_0044

      IL_002b:  ldarg.0
      IL_002c:  ldfld      class [mscorlib]System.Collections.Generic.List`1<string> InstallUpgrade096/'<>c__DisplayClass1c'::preserveRoots
      IL_0031:  ldloc.0
      IL_0032:  ldftn      instance bool InstallUpgrade096/'<>c__DisplayClass1c'/'<>c__DisplayClass1e'::'<Run>b__14'(string)
      IL_0038:  newobj     instance void class [mscorlib]System.Func`2<string,bool>::.ctor(object,
                                                                                           native int)
      IL_003d:  call       bool [System.Core]System.Linq.Enumerable::Any<string>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                 class [mscorlib]System.Func`2<!!0,bool>)
      IL_0042:  brtrue.s   IL_006b

      IL_0044:  ldarg.0
      IL_0045:  ldfld      string InstallUpgrade096/'<>c__DisplayClass1c'::pointerValue
      IL_004a:  brfalse.s  IL_0067

      IL_004c:  ldloc.0
      IL_004d:  ldflda     valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string> InstallUpgrade096/'<>c__DisplayClass1c'/'<>c__DisplayClass1e'::pair
      IL_0052:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
      IL_0057:  ldstr      "MEPS_LIBRARY_PATH.txt"
      IL_005c:  ldc.i4.5
      IL_005d:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_0062:  ldc.i4.0
      IL_0063:  ceq
      IL_0065:  br.s       IL_0068

      IL_0067:  ldc.i4.1
      IL_0068:  nop
      IL_0069:  br.s       IL_006c

      IL_006b:  ldc.i4.0
      IL_006c:  nop
      IL_006d:  stloc.1
      IL_006e:  br.s       IL_0070

      IL_0070:  ldloc.1
      IL_0071:  ret
    } // end of method '<>c__DisplayClass1c'::'<Run>b__13'

  } // end of class '<>c__DisplayClass1c'

  .field private static literal string Pointer = "MEPS_LIBRARY_PATH.txt"
  .field public static string FailAt
  .field private static initonly class [System.Web.Extensions]System.Web.Script.Serialization.JavaScriptSerializer Json
  .field private static string Base
  .field private static initonly class [System.Core]System.Collections.Generic.HashSet`1<string> ManagedSdnl
  .field private static class [mscorlib]System.Func`2<string,bool> 'CS$<>9__CachedAnonymousMethodDelegatee'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class [mscorlib]System.Func`2<string,bool> 'CS$<>9__CachedAnonymousMethodDelegate17'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class [mscorlib]System.Func`2<string,bool> 'CS$<>9__CachedAnonymousMethodDelegate18'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class [mscorlib]System.Func`2<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>,string> 'CS$<>9__CachedAnonymousMethodDelegate19'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private static class [mscorlib]System.Func`2<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>,string> 'CS$<>9__CachedAnonymousMethodDelegate1a'
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .method private hidebysig static void  Fault(string point) cil managed
  {
    // Code size       37 (0x25)
    .maxstack  2
    .locals init (bool V_0)
    IL_0000:  nop
    IL_0001:  ldsfld     string InstallUpgrade096::FailAt
    IL_0006:  ldarg.0
    IL_0007:  call       bool [mscorlib]System.String::op_Equality(string,
                                                                   string)
    IL_000c:  ldc.i4.0
    IL_000d:  ceq
    IL_000f:  stloc.0
    IL_0010:  ldloc.0
    IL_0011:  brtrue.s   IL_0024

    IL_0013:  ldstr      "QA injected failure: "
    IL_0018:  ldarg.0
    IL_0019:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_001e:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_0023:  throw

    IL_0024:  ret
  } // end of method InstallUpgrade096::Fault

  .method public hidebysig static int32  Main(string[] args) cil managed
  {
    .entrypoint
    // Code size       48 (0x30)
    .maxstack  2
    .locals init (int32 V_0,
             int32 V_1,
             bool V_2)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  call       int32 InstallUpgrade096::Run(string[])
    IL_0007:  stloc.0
    IL_0008:  ldarg.0
    IL_0009:  ldstr      "--test"
    IL_000e:  call       bool [System.Core]System.Linq.Enumerable::Contains<string>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                    !!0)
    IL_0013:  stloc.2
    IL_0014:  ldloc.2
    IL_0015:  brtrue.s   IL_002a

    IL_0017:  nop
    IL_0018:  ldstr      "Nhan Enter de dong."
    IL_001d:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_0022:  nop
    IL_0023:  call       string [mscorlib]System.Console::ReadLine()
    IL_0028:  pop
    IL_0029:  nop
    IL_002a:  ldloc.0
    IL_002b:  stloc.1
    IL_002c:  br.s       IL_002e

    IL_002e:  ldloc.1
    IL_002f:  ret
  } // end of method InstallUpgrade096::Main

  .method private hidebysig static string 
          Safe(string path) cil managed
  {
    // Code size       149 (0x95)
    .maxstack  3
    .locals init (string V_0,
             string V_1,
             string V_2,
             bool V_3)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  call       string [mscorlib]System.IO.Path::GetFullPath(string)
    IL_0007:  stloc.0
    IL_0008:  ldloc.0
    IL_0009:  ldsfld     string InstallUpgrade096::Base
    IL_000e:  ldsfld     char [mscorlib]System.IO.Path::DirectorySeparatorChar
    IL_0013:  box        [mscorlib]System.Char
    IL_0018:  call       string [mscorlib]System.String::Concat(object,
                                                                object)
    IL_001d:  ldc.i4.5
    IL_001e:  callvirt   instance bool [mscorlib]System.String::StartsWith(string,
                                                                           valuetype [mscorlib]System.StringComparison)
    IL_0023:  stloc.3
    IL_0024:  ldloc.3
    IL_0025:  brtrue.s   IL_0032

    IL_0027:  ldstr      "Path outside install root"
    IL_002c:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_0031:  throw

    IL_0032:  ldloc.0
    IL_0033:  stloc.1
    IL_0034:  br.s       IL_006f

    IL_0036:  ldloc.1
    IL_0037:  call       bool [mscorlib]System.IO.Directory::Exists(string)
    IL_003c:  brtrue.s   IL_0046

    IL_003e:  ldloc.1
    IL_003f:  call       bool [mscorlib]System.IO.File::Exists(string)
    IL_0044:  brfalse.s  IL_0057

    IL_0046:  ldloc.1
    IL_0047:  call       valuetype [mscorlib]System.IO.FileAttributes [mscorlib]System.IO.File::GetAttributes(string)
    IL_004c:  ldc.i4     0x400
    IL_0051:  and
    IL_0052:  ldc.i4.0
    IL_0053:  ceq
    IL_0055:  br.s       IL_0058

    IL_0057:  ldc.i4.1
    IL_0058:  nop
    IL_0059:  stloc.3
    IL_005a:  ldloc.3
    IL_005b:  brtrue.s   IL_0068

    IL_005d:  ldstr      "Linked install path not supported"
    IL_0062:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_0067:  throw

    IL_0068:  ldloc.1
    IL_0069:  call       string [mscorlib]System.IO.Path::GetDirectoryName(string)
    IL_006e:  stloc.1
    IL_006f:  ldloc.1
    IL_0070:  brfalse.s  IL_0089

    IL_0072:  ldloc.1
    IL_0073:  callvirt   instance int32 [mscorlib]System.String::get_Length()
    IL_0078:  ldsfld     string InstallUpgrade096::Base
    IL_007d:  callvirt   instance int32 [mscorlib]System.String::get_Length()
    IL_0082:  clt
    IL_0084:  ldc.i4.0
    IL_0085:  ceq
    IL_0087:  br.s       IL_008a

    IL_0089:  ldc.i4.0
    IL_008a:  nop
    IL_008b:  stloc.3
    IL_008c:  ldloc.3
    IL_008d:  brtrue.s   IL_0036

    IL_008f:  ldloc.0
    IL_0090:  stloc.2
    IL_0091:  br.s       IL_0093

    IL_0093:  ldloc.2
    IL_0094:  ret
  } // end of method InstallUpgrade096::Safe

  .method private hidebysig static class [mscorlib]System.Collections.Generic.IEnumerable`1<string> 
          Files(string root) cil managed
  {
    // Code size       21 (0x15)
    .maxstack  2
    .locals init (class InstallUpgrade096/'<Files>d__0' V_0,
             class [mscorlib]System.Collections.Generic.IEnumerable`1<string> V_1)
    IL_0000:  ldc.i4.s   -2
    IL_0002:  newobj     instance void InstallUpgrade096/'<Files>d__0'::.ctor(int32)
    IL_0007:  stloc.0
    IL_0008:  ldloc.0
    IL_0009:  ldarg.0
    IL_000a:  stfld      string InstallUpgrade096/'<Files>d__0'::'<>3__root'
    IL_000f:  ldloc.0
    IL_0010:  stloc.1
    IL_0011:  br.s       IL_0013

    IL_0013:  ldloc.1
    IL_0014:  ret
  } // end of method InstallUpgrade096::Files

  .method private hidebysig static string 
          Hash(string f) cil managed
  {
    // Code size       84 (0x54)
    .maxstack  3
    .locals init (class [mscorlib]System.IO.FileStream V_0,
             class [mscorlib]System.Security.Cryptography.SHA256 V_1,
             string V_2,
             bool V_3)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  call       class [mscorlib]System.IO.FileStream [mscorlib]System.IO.File::OpenRead(string)
    IL_0007:  stloc.0
    .try
    {
      IL_0008:  call       class [mscorlib]System.Security.Cryptography.SHA256 [mscorlib]System.Security.Cryptography.SHA256::Create()
      IL_000d:  stloc.1
      .try
      {
        IL_000e:  ldloc.1
        IL_000f:  ldloc.0
        IL_0010:  callvirt   instance uint8[] [mscorlib]System.Security.Cryptography.HashAlgorithm::ComputeHash(class [mscorlib]System.IO.Stream)
        IL_0015:  call       string [mscorlib]System.BitConverter::ToString(uint8[])
        IL_001a:  ldstr      "-"
        IL_001f:  ldstr      ""
        IL_0024:  callvirt   instance string [mscorlib]System.String::Replace(string,
                                                                              string)
        IL_0029:  callvirt   instance string [mscorlib]System.String::ToLowerInvariant()
        IL_002e:  stloc.2
        IL_002f:  leave.s    IL_0051

      }  // end .try
      finally
      {
        IL_0031:  ldloc.1
        IL_0032:  ldnull
        IL_0033:  ceq
        IL_0035:  stloc.3
        IL_0036:  ldloc.3
        IL_0037:  brtrue.s   IL_0040

        IL_0039:  ldloc.1
        IL_003a:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_003f:  nop
        IL_0040:  endfinally
      }  // end handler
    }  // end .try
    finally
    {
      IL_0041:  ldloc.0
      IL_0042:  ldnull
      IL_0043:  ceq
      IL_0045:  stloc.3
      IL_0046:  ldloc.3
      IL_0047:  brtrue.s   IL_0050

      IL_0049:  ldloc.0
      IL_004a:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_004f:  nop
      IL_0050:  endfinally
    }  // end handler
    IL_0051:  nop
    IL_0052:  ldloc.2
    IL_0053:  ret
  } // end of method InstallUpgrade096::Hash

  .method private hidebysig static string 
          NormalizeDirectory(string 'value') cil managed
  {
    // Code size       74 (0x4a)
    .maxstack  4
    .locals init (string V_0,
             string V_1,
             string V_2,
             string V_3,
             char[] V_4)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  call       string [mscorlib]System.IO.Path::GetFullPath(string)
    IL_0007:  stloc.0
    IL_0008:  ldloc.0
    IL_0009:  call       string [mscorlib]System.IO.Path::GetPathRoot(string)
    IL_000e:  stloc.1
    IL_000f:  ldloc.0
    IL_0010:  ldc.i4.2
    IL_0011:  newarr     [mscorlib]System.Char
    IL_0016:  stloc.s    V_4
    IL_0018:  ldloc.s    V_4
    IL_001a:  ldc.i4.0
    IL_001b:  ldsfld     char [mscorlib]System.IO.Path::DirectorySeparatorChar
    IL_0020:  stelem.i2
    IL_0021:  ldloc.s    V_4
    IL_0023:  ldc.i4.1
    IL_0024:  ldsfld     char [mscorlib]System.IO.Path::AltDirectorySeparatorChar
    IL_0029:  stelem.i2
    IL_002a:  ldloc.s    V_4
    IL_002c:  callvirt   instance string [mscorlib]System.String::TrimEnd(char[])
    IL_0031:  stloc.2
    IL_0032:  ldloc.2
    IL_0033:  callvirt   instance int32 [mscorlib]System.String::get_Length()
    IL_0038:  ldloc.1
    IL_0039:  callvirt   instance int32 [mscorlib]System.String::get_Length()
    IL_003e:  blt.s      IL_0043

    IL_0040:  ldloc.2
    IL_0041:  br.s       IL_0044

    IL_0043:  ldloc.1
    IL_0044:  nop
    IL_0045:  stloc.3
    IL_0046:  br.s       IL_0048

    IL_0048:  ldloc.3
    IL_0049:  ret
  } // end of method InstallUpgrade096::NormalizeDirectory

  .method private hidebysig static class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> 
          Hashes(string root) cil managed
  {
    // Code size       104 (0x68)
    .maxstack  4
    .locals init (class InstallUpgrade096/'<>c__DisplayClassb' V_0,
             class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> V_1,
             char V_2)
    IL_0000:  newobj     instance void InstallUpgrade096/'<>c__DisplayClassb'::.ctor()
    IL_0005:  stloc.0
    IL_0006:  nop
    IL_0007:  ldarg.0
    IL_0008:  call       string InstallUpgrade096::NormalizeDirectory(string)
    IL_000d:  starg.s    root
    IL_000f:  ldloc.0
    IL_0010:  ldarg.0
    IL_0011:  ldsfld     char [mscorlib]System.IO.Path::DirectorySeparatorChar
    IL_0016:  stloc.2
    IL_0017:  ldloca.s   V_2
    IL_0019:  call       instance string [mscorlib]System.Char::ToString()
    IL_001e:  callvirt   instance bool [mscorlib]System.String::EndsWith(string)
    IL_0023:  brtrue.s   IL_002f

    IL_0025:  ldarg.0
    IL_0026:  callvirt   instance int32 [mscorlib]System.String::get_Length()
    IL_002b:  ldc.i4.1
    IL_002c:  add
    IL_002d:  br.s       IL_0035

    IL_002f:  ldarg.0
    IL_0030:  callvirt   instance int32 [mscorlib]System.String::get_Length()
    IL_0035:  nop
    IL_0036:  stfld      int32 InstallUpgrade096/'<>c__DisplayClassb'::offset
    IL_003b:  ldarg.0
    IL_003c:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<string> InstallUpgrade096::Files(string)
    IL_0041:  ldloc.0
    IL_0042:  ldftn      instance string InstallUpgrade096/'<>c__DisplayClassb'::'<Hashes>b__a'(string)
    IL_0048:  newobj     instance void class [mscorlib]System.Func`2<string,string>::.ctor(object,
                                                                                           native int)
    IL_004d:  ldnull
    IL_004e:  ldftn      string InstallUpgrade096::Hash(string)
    IL_0054:  newobj     instance void class [mscorlib]System.Func`2<string,string>::.ctor(object,
                                                                                           native int)
    IL_0059:  call       class [mscorlib]System.StringComparer [mscorlib]System.StringComparer::get_OrdinalIgnoreCase()
    IL_005e:  call       class [mscorlib]System.Collections.Generic.Dictionary`2<!!1,!!2> [System.Core]System.Linq.Enumerable::ToDictionary<string,string,string>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                  class [mscorlib]System.Func`2<!!0,!!1>,
                                                                                                                                                                  class [mscorlib]System.Func`2<!!0,!!2>,
                                                                                                                                                                  class [mscorlib]System.Collections.Generic.IEqualityComparer`1<!!1>)
    IL_0063:  stloc.1
    IL_0064:  br.s       IL_0066

    IL_0066:  ldloc.1
    IL_0067:  ret
  } // end of method InstallUpgrade096::Hashes

  .method private hidebysig static void  Save(string path,
                                              object 'value') cil managed
  {
    // Code size       26 (0x1a)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldsfld     class [System.Web.Extensions]System.Web.Script.Serialization.JavaScriptSerializer InstallUpgrade096::Json
    IL_0007:  ldarg.1
    IL_0008:  callvirt   instance string [System.Web.Extensions]System.Web.Script.Serialization.JavaScriptSerializer::Serialize(object)
    IL_000d:  ldc.i4.1
    IL_000e:  newobj     instance void [mscorlib]System.Text.UTF8Encoding::.ctor(bool)
    IL_0013:  call       void [mscorlib]System.IO.File::WriteAllText(string,
                                                                     string,
                                                                     class [mscorlib]System.Text.Encoding)
    IL_0018:  nop
    IL_0019:  ret
  } // end of method InstallUpgrade096::Save

  .method private hidebysig static void  Verify(string root,
                                                class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> hashes) cil managed
  {
    // Code size       109 (0x6d)
    .maxstack  2
    .locals init (valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string> V_0,
             valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string> V_1,
             bool V_2)
    IL_0000:  nop
    IL_0001:  nop
    IL_0002:  ldarg.1
    IL_0003:  callvirt   instance valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<!0,!1> class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::GetEnumerator()
    IL_0008:  stloc.1
    .try
    {
      IL_0009:  br.s       IL_004f

      IL_000b:  ldloca.s   V_1
      IL_000d:  call       instance valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<!0,!1> valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string>::get_Current()
      IL_0012:  stloc.0
      IL_0013:  ldarg.0
      IL_0014:  ldloca.s   V_0
      IL_0016:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
      IL_001b:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_0020:  call       string InstallUpgrade096::Hash(string)
      IL_0025:  ldloca.s   V_0
      IL_0027:  call       instance !1 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Value()
      IL_002c:  call       bool [mscorlib]System.String::op_Inequality(string,
                                                                       string)
      IL_0031:  ldc.i4.0
      IL_0032:  ceq
      IL_0034:  stloc.2
      IL_0035:  ldloc.2
      IL_0036:  brtrue.s   IL_004f

      IL_0038:  ldstr      "File verification failed: "
      IL_003d:  ldloca.s   V_0
      IL_003f:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
      IL_0044:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_0049:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_004e:  throw

      IL_004f:  ldloca.s   V_1
      IL_0051:  call       instance bool valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string>::MoveNext()
      IL_0056:  stloc.2
      IL_0057:  ldloc.2
      IL_0058:  brtrue.s   IL_000b

      IL_005a:  leave.s    IL_006b

    }  // end .try
    finally
    {
      IL_005c:  ldloca.s   V_1
      IL_005e:  constrained. valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string>
      IL_0064:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_0069:  nop
      IL_006a:  endfinally
    }  // end handler
    IL_006b:  nop
    IL_006c:  ret
  } // end of method InstallUpgrade096::Verify

  .method private hidebysig static void  CopyTree(string from,
                                                  string 'to',
                                                  class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> hashes) cil managed
  {
    // Code size       111 (0x6f)
    .maxstack  3
    .locals init (valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string> V_0,
             string V_1,
             valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string> V_2,
             bool V_3)
    IL_0000:  nop
    IL_0001:  nop
    IL_0002:  ldarg.2
    IL_0003:  callvirt   instance valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<!0,!1> class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::GetEnumerator()
    IL_0008:  stloc.2
    .try
    {
      IL_0009:  br.s       IL_0049

      IL_000b:  ldloca.s   V_2
      IL_000d:  call       instance valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<!0,!1> valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string>::get_Current()
      IL_0012:  stloc.0
      IL_0013:  nop
      IL_0014:  ldarg.1
      IL_0015:  ldloca.s   V_0
      IL_0017:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
      IL_001c:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_0021:  call       string InstallUpgrade096::Safe(string)
      IL_0026:  stloc.1
      IL_0027:  ldloc.1
      IL_0028:  call       string [mscorlib]System.IO.Path::GetDirectoryName(string)
      IL_002d:  call       class [mscorlib]System.IO.DirectoryInfo [mscorlib]System.IO.Directory::CreateDirectory(string)
      IL_0032:  pop
      IL_0033:  ldarg.0
      IL_0034:  ldloca.s   V_0
      IL_0036:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
      IL_003b:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_0040:  ldloc.1
      IL_0041:  ldc.i4.1
      IL_0042:  call       void [mscorlib]System.IO.File::Copy(string,
                                                               string,
                                                               bool)
      IL_0047:  nop
      IL_0048:  nop
      IL_0049:  ldloca.s   V_2
      IL_004b:  call       instance bool valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string>::MoveNext()
      IL_0050:  stloc.3
      IL_0051:  ldloc.3
      IL_0052:  brtrue.s   IL_000b

      IL_0054:  leave.s    IL_0065

    }  // end .try
    finally
    {
      IL_0056:  ldloca.s   V_2
      IL_0058:  constrained. valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string>
      IL_005e:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_0063:  nop
      IL_0064:  endfinally
    }  // end handler
    IL_0065:  nop
    IL_0066:  ldarg.1
    IL_0067:  ldarg.2
    IL_0068:  call       void InstallUpgrade096::Verify(string,
                                                        class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>)
    IL_006d:  nop
    IL_006e:  ret
  } // end of method InstallUpgrade096::CopyTree

  .method private hidebysig static bool  Inside(string root,
                                                string full) cil managed
  {
    // Code size       50 (0x32)
    .maxstack  5
    .locals init (bool V_0,
             char[] V_1)
    IL_0000:  nop
    IL_0001:  ldarg.1
    IL_0002:  ldarg.0
    IL_0003:  ldc.i4.1
    IL_0004:  newarr     [mscorlib]System.Char
    IL_0009:  stloc.1
    IL_000a:  ldloc.1
    IL_000b:  ldc.i4.0
    IL_000c:  ldsfld     char [mscorlib]System.IO.Path::DirectorySeparatorChar
    IL_0011:  stelem.i2
    IL_0012:  ldloc.1
    IL_0013:  callvirt   instance string [mscorlib]System.String::TrimEnd(char[])
    IL_0018:  ldsfld     char [mscorlib]System.IO.Path::DirectorySeparatorChar
    IL_001d:  box        [mscorlib]System.Char
    IL_0022:  call       string [mscorlib]System.String::Concat(object,
                                                                object)
    IL_0027:  ldc.i4.5
    IL_0028:  callvirt   instance bool [mscorlib]System.String::StartsWith(string,
                                                                           valuetype [mscorlib]System.StringComparison)
    IL_002d:  stloc.0
    IL_002e:  br.s       IL_0030

    IL_0030:  ldloc.0
    IL_0031:  ret
  } // end of method InstallUpgrade096::Inside

  .method private hidebysig static void  NoLinks(string full) cil managed
  {
    // Code size       74 (0x4a)
    .maxstack  2
    .locals init (string V_0,
             bool V_1)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  stloc.0
    IL_0003:  br.s       IL_003e

    IL_0005:  ldloc.0
    IL_0006:  call       bool [mscorlib]System.IO.Directory::Exists(string)
    IL_000b:  brtrue.s   IL_0015

    IL_000d:  ldloc.0
    IL_000e:  call       bool [mscorlib]System.IO.File::Exists(string)
    IL_0013:  brfalse.s  IL_0026

    IL_0015:  ldloc.0
    IL_0016:  call       valuetype [mscorlib]System.IO.FileAttributes [mscorlib]System.IO.File::GetAttributes(string)
    IL_001b:  ldc.i4     0x400
    IL_0020:  and
    IL_0021:  ldc.i4.0
    IL_0022:  ceq
    IL_0024:  br.s       IL_0027

    IL_0026:  ldc.i4.1
    IL_0027:  nop
    IL_0028:  stloc.1
    IL_0029:  ldloc.1
    IL_002a:  brtrue.s   IL_0037

    IL_002c:  ldstr      "Linked library path not supported"
    IL_0031:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_0036:  throw

    IL_0037:  ldloc.0
    IL_0038:  call       string [mscorlib]System.IO.Path::GetDirectoryName(string)
    IL_003d:  stloc.0
    IL_003e:  ldloc.0
    IL_003f:  ldnull
    IL_0040:  ceq
    IL_0042:  ldc.i4.0
    IL_0043:  ceq
    IL_0045:  stloc.1
    IL_0046:  ldloc.1
    IL_0047:  brtrue.s   IL_0005

    IL_0049:  ret
  } // end of method InstallUpgrade096::NoLinks

  .method private hidebysig static class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> 
          ManagedSdnlHashes(string source) cil managed
  {
    // Code size       132 (0x84)
    .maxstack  3
    .locals init (class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> V_0,
             string V_1,
             string V_2,
             class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> V_3,
             valuetype [System.Core]System.Collections.Generic.HashSet`1/Enumerator<string> V_4,
             bool V_5)
    IL_0000:  nop
    IL_0001:  call       class [mscorlib]System.StringComparer [mscorlib]System.StringComparer::get_OrdinalIgnoreCase()
    IL_0006:  newobj     instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::.ctor(class [mscorlib]System.Collections.Generic.IEqualityComparer`1<!0>)
    IL_000b:  stloc.0
    IL_000c:  nop
    IL_000d:  ldsfld     class [System.Core]System.Collections.Generic.HashSet`1<string> InstallUpgrade096::ManagedSdnl
    IL_0012:  callvirt   instance valuetype [System.Core]System.Collections.Generic.HashSet`1/Enumerator<!0> class [System.Core]System.Collections.Generic.HashSet`1<string>::GetEnumerator()
    IL_0017:  stloc.s    V_4
    .try
    {
      IL_0019:  br.s       IL_005f

      IL_001b:  ldloca.s   V_4
      IL_001d:  call       instance !0 valuetype [System.Core]System.Collections.Generic.HashSet`1/Enumerator<string>::get_Current()
      IL_0022:  stloc.1
      IL_0023:  nop
      IL_0024:  ldarg.0
      IL_0025:  ldloc.1
      IL_0026:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_002b:  stloc.2
      IL_002c:  ldloc.2
      IL_002d:  call       void InstallUpgrade096::NoLinks(string)
      IL_0032:  nop
      IL_0033:  ldloc.2
      IL_0034:  call       bool [mscorlib]System.IO.File::Exists(string)
      IL_0039:  stloc.s    V_5
      IL_003b:  ldloc.s    V_5
      IL_003d:  brtrue.s   IL_0050

      IL_003f:  ldstr      "Missing managed SDNL program: "
      IL_0044:  ldloc.1
      IL_0045:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_004a:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_004f:  throw

      IL_0050:  ldloc.0
      IL_0051:  ldloc.1
      IL_0052:  ldloc.2
      IL_0053:  call       string InstallUpgrade096::Hash(string)
      IL_0058:  callvirt   instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::Add(!0,
                                                                                                                     !1)
      IL_005d:  nop
      IL_005e:  nop
      IL_005f:  ldloca.s   V_4
      IL_0061:  call       instance bool valuetype [System.Core]System.Collections.Generic.HashSet`1/Enumerator<string>::MoveNext()
      IL_0066:  stloc.s    V_5
      IL_0068:  ldloc.s    V_5
      IL_006a:  brtrue.s   IL_001b

      IL_006c:  leave.s    IL_007d

    }  // end .try
    finally
    {
      IL_006e:  ldloca.s   V_4
      IL_0070:  constrained. valuetype [System.Core]System.Collections.Generic.HashSet`1/Enumerator<string>
      IL_0076:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_007b:  nop
      IL_007c:  endfinally
    }  // end handler
    IL_007d:  nop
    IL_007e:  ldloc.0
    IL_007f:  stloc.3
    IL_0080:  br.s       IL_0082

    IL_0082:  ldloc.3
    IL_0083:  ret
  } // end of method InstallUpgrade096::ManagedSdnlHashes

  .method private hidebysig static string 
          ValidatePointer(string keep,
                          string[] old,
                          [out] string& local) cil managed
  {
    // Code size       611 (0x263)
    .maxstack  4
    .locals init (string V_0,
             string V_1,
             string V_2,
             string V_3,
             bool V_4,
             string V_5,
             bool V_6,
             char[] V_7,
             string[] V_8,
             int32 V_9)
    IL_0000:  nop
    IL_0001:  ldarg.2
    IL_0002:  ldnull
    IL_0003:  stind.ref
    IL_0004:  ldarg.0
    IL_0005:  ldstr      "MEPS_LIBRARY_PATH.txt"
    IL_000a:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_000f:  stloc.0
    IL_0010:  ldloc.0
    IL_0011:  call       bool [mscorlib]System.IO.File::Exists(string)
    IL_0016:  stloc.s    V_6
    IL_0018:  ldloc.s    V_6
    IL_001a:  brtrue.s   IL_0024

    IL_001c:  ldnull
    IL_001d:  stloc.s    V_5
    IL_001f:  br         IL_0260

    IL_0024:  ldloc.0
    IL_0025:  call       void InstallUpgrade096::NoLinks(string)
    IL_002a:  nop
    IL_002b:  ldloc.0
    IL_002c:  call       string [mscorlib]System.IO.File::ReadAllText(string)
    IL_0031:  callvirt   instance string [mscorlib]System.String::Trim()
    IL_0036:  stloc.1
    IL_0037:  ldloc.1
    IL_0038:  call       bool [mscorlib]System.String::IsNullOrWhiteSpace(string)
    IL_003d:  brtrue.s   IL_0060

    IL_003f:  ldloc.1
    IL_0040:  ldc.i4.3
    IL_0041:  newarr     [mscorlib]System.Char
    IL_0046:  stloc.s    V_7
    IL_0048:  ldloc.s    V_7
    IL_004a:  ldc.i4.0
    IL_004b:  ldc.i4.s   13
    IL_004d:  stelem.i2
    IL_004e:  ldloc.s    V_7
    IL_0050:  ldc.i4.1
    IL_0051:  ldc.i4.s   10
    IL_0053:  stelem.i2
    IL_0054:  ldloc.s    V_7
    IL_0056:  callvirt   instance int32 [mscorlib]System.String::IndexOfAny(char[])
    IL_005b:  ldc.i4.0
    IL_005c:  clt
    IL_005e:  br.s       IL_0061

    IL_0060:  ldc.i4.0
    IL_0061:  nop
    IL_0062:  stloc.s    V_6
    IL_0064:  ldloc.s    V_6
    IL_0066:  brtrue.s   IL_0073

    IL_0068:  ldstr      "Invalid library pointer"
    IL_006d:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_0072:  throw

    IL_0073:  ldloc.1
    IL_0074:  call       bool [mscorlib]System.IO.Path::IsPathRooted(string)
    IL_0079:  brtrue.s   IL_0084

    IL_007b:  ldarg.0
    IL_007c:  ldloc.1
    IL_007d:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_0082:  br.s       IL_0085

    IL_0084:  ldloc.1
    IL_0085:  nop
    IL_0086:  call       string InstallUpgrade096::NormalizeDirectory(string)
    IL_008b:  stloc.2
    IL_008c:  ldarg.0
    IL_008d:  call       string InstallUpgrade096::NormalizeDirectory(string)
    IL_0092:  starg.s    keep
    IL_0094:  ldloc.2
    IL_0095:  ldloc.2
    IL_0096:  call       string [mscorlib]System.IO.Path::GetPathRoot(string)
    IL_009b:  ldc.i4.5
    IL_009c:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                       valuetype [mscorlib]System.StringComparison)
    IL_00a1:  ldc.i4.0
    IL_00a2:  ceq
    IL_00a4:  stloc.s    V_6
    IL_00a6:  ldloc.s    V_6
    IL_00a8:  brtrue.s   IL_00b5

    IL_00aa:  ldstr      "Library cannot be drive root"
    IL_00af:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_00b4:  throw

    IL_00b5:  ldloc.2
    IL_00b6:  call       void InstallUpgrade096::NoLinks(string)
    IL_00bb:  nop
    IL_00bc:  ldloc.2
    IL_00bd:  call       bool [mscorlib]System.IO.Directory::Exists(string)
    IL_00c2:  brfalse.s  IL_00d6

    IL_00c4:  ldloc.2
    IL_00c5:  ldstr      "catalog.json"
    IL_00ca:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_00cf:  call       bool [mscorlib]System.IO.File::Exists(string)
    IL_00d4:  br.s       IL_00d7

    IL_00d6:  ldc.i4.0
    IL_00d7:  nop
    IL_00d8:  stloc.s    V_6
    IL_00da:  ldloc.s    V_6
    IL_00dc:  brtrue.s   IL_00e9

    IL_00de:  ldstr      "Missing pointed library catalog"
    IL_00e3:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_00e8:  throw

    IL_00e9:  ldsfld     class [System.Web.Extensions]System.Web.Script.Serialization.JavaScriptSerializer InstallUpgrade096::Json
    IL_00ee:  ldloc.2
    IL_00ef:  ldstr      "catalog.json"
    IL_00f4:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_00f9:  call       string [mscorlib]System.IO.File::ReadAllText(string)
    IL_00fe:  callvirt   instance object [System.Web.Extensions]System.Web.Script.Serialization.JavaScriptSerializer::DeserializeObject(string)
    IL_0103:  pop
    IL_0104:  ldloc.2
    IL_0105:  call       class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> InstallUpgrade096::Hashes(string)
    IL_010a:  pop
    IL_010b:  nop
    IL_010c:  ldarg.1
    IL_010d:  stloc.s    V_8
    IL_010f:  ldc.i4.0
    IL_0110:  stloc.s    V_9
    IL_0112:  br.s       IL_0157

    IL_0114:  ldloc.s    V_8
    IL_0116:  ldloc.s    V_9
    IL_0118:  ldelem.ref
    IL_0119:  stloc.3
    IL_011a:  ldloc.3
    IL_011b:  ldarg.0
    IL_011c:  ldc.i4.5
    IL_011d:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                       valuetype [mscorlib]System.StringComparison)
    IL_0122:  brtrue.s   IL_013e

    IL_0124:  ldloc.2
    IL_0125:  ldloc.3
    IL_0126:  ldc.i4.5
    IL_0127:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                       valuetype [mscorlib]System.StringComparison)
    IL_012c:  brtrue.s   IL_013a

    IL_012e:  ldloc.3
    IL_012f:  ldloc.2
    IL_0130:  call       bool InstallUpgrade096::Inside(string,
                                                        string)
    IL_0135:  ldc.i4.0
    IL_0136:  ceq
    IL_0138:  br.s       IL_013b

    IL_013a:  ldc.i4.0
    IL_013b:  nop
    IL_013c:  br.s       IL_013f

    IL_013e:  ldc.i4.1
    IL_013f:  nop
    IL_0140:  stloc.s    V_6
    IL_0142:  ldloc.s    V_6
    IL_0144:  brtrue.s   IL_0151

    IL_0146:  ldstr      "Library points to another installation being archi"
    + "ved"
    IL_014b:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_0150:  throw

    IL_0151:  ldloc.s    V_9
    IL_0153:  ldc.i4.1
    IL_0154:  add
    IL_0155:  stloc.s    V_9
    IL_0157:  ldloc.s    V_9
    IL_0159:  ldloc.s    V_8
    IL_015b:  ldlen
    IL_015c:  conv.i4
    IL_015d:  clt
    IL_015f:  stloc.s    V_6
    IL_0161:  ldloc.s    V_6
    IL_0163:  brtrue.s   IL_0114

    IL_0165:  ldloc.2
    IL_0166:  ldarg.0
    IL_0167:  ldc.i4.5
    IL_0168:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                       valuetype [mscorlib]System.StringComparison)
    IL_016d:  ldc.i4.0
    IL_016e:  ceq
    IL_0170:  stloc.s    V_6
    IL_0172:  ldloc.s    V_6
    IL_0174:  brtrue.s   IL_0181

    IL_0176:  ldstr      "Library cannot be installation root"
    IL_017b:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_0180:  throw

    IL_0181:  ldarg.0
    IL_0182:  ldloc.2
    IL_0183:  call       bool InstallUpgrade096::Inside(string,
                                                        string)
    IL_0188:  ldc.i4.0
    IL_0189:  ceq
    IL_018b:  stloc.s    V_6
    IL_018d:  ldloc.s    V_6
    IL_018f:  brtrue.s   IL_01ba

    IL_0191:  nop
    IL_0192:  ldarg.2
    IL_0193:  ldloc.2
    IL_0194:  ldarg.0
    IL_0195:  callvirt   instance int32 [mscorlib]System.String::get_Length()
    IL_019a:  ldc.i4.1
    IL_019b:  add
    IL_019c:  callvirt   instance string [mscorlib]System.String::Substring(int32)
    IL_01a1:  stind.ref
    IL_01a2:  ldsfld     string InstallUpgrade096::Base
    IL_01a7:  ldstr      "MEPS060"
    IL_01ac:  ldarg.2
    IL_01ad:  ldind.ref
    IL_01ae:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string,
                                                                  string)
    IL_01b3:  stloc.s    V_5
    IL_01b5:  br         IL_0260

    IL_01ba:  ldsfld     string InstallUpgrade096::Base
    IL_01bf:  ldloc.2
    IL_01c0:  call       bool InstallUpgrade096::Inside(string,
                                                        string)
    IL_01c5:  brfalse.s  IL_0200

    IL_01c7:  ldloc.2
    IL_01c8:  ldsfld     string InstallUpgrade096::Base
    IL_01cd:  callvirt   instance int32 [mscorlib]System.String::get_Length()
    IL_01d2:  ldc.i4.1
    IL_01d3:  add
    IL_01d4:  callvirt   instance string [mscorlib]System.String::Substring(int32)
    IL_01d9:  ldc.i4.1
    IL_01da:  newarr     [mscorlib]System.Char
    IL_01df:  stloc.s    V_7
    IL_01e1:  ldloc.s    V_7
    IL_01e3:  ldc.i4.0
    IL_01e4:  ldsfld     char [mscorlib]System.IO.Path::DirectorySeparatorChar
    IL_01e9:  stelem.i2
    IL_01ea:  ldloc.s    V_7
    IL_01ec:  callvirt   instance string[] [mscorlib]System.String::Split(char[])
    IL_01f1:  ldc.i4.0
    IL_01f2:  ldelem.ref
    IL_01f3:  ldstr      ".MEPS060_INSTALL_"
    IL_01f8:  ldc.i4.5
    IL_01f9:  callvirt   instance bool [mscorlib]System.String::StartsWith(string,
                                                                           valuetype [mscorlib]System.StringComparison)
    IL_01fe:  br.s       IL_0201

    IL_0200:  ldc.i4.0
    IL_0201:  nop
    IL_0202:  stloc.s    V_4
    IL_0204:  ldloc.2
    IL_0205:  ldsfld     string InstallUpgrade096::Base
    IL_020a:  ldc.i4.5
    IL_020b:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                       valuetype [mscorlib]System.StringComparison)
    IL_0210:  brtrue.s   IL_0248

    IL_0212:  ldsfld     string InstallUpgrade096::Base
    IL_0217:  ldstr      "ARCHIVE"
    IL_021c:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_0221:  ldloc.2
    IL_0222:  call       bool InstallUpgrade096::Inside(string,
                                                        string)
    IL_0227:  brtrue.s   IL_0248

    IL_0229:  ldloc.2
    IL_022a:  ldsfld     string InstallUpgrade096::Base
    IL_022f:  ldstr      "ARCHIVE"
    IL_0234:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_0239:  ldc.i4.5
    IL_023a:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                       valuetype [mscorlib]System.StringComparison)
    IL_023f:  brtrue.s   IL_0248

    IL_0241:  ldloc.s    V_4
    IL_0243:  ldc.i4.0
    IL_0244:  ceq
    IL_0246:  br.s       IL_0249

    IL_0248:  ldc.i4.0
    IL_0249:  nop
    IL_024a:  stloc.s    V_6
    IL_024c:  ldloc.s    V_6
    IL_024e:  brtrue.s   IL_025b

    IL_0250:  ldstr      "External library collides with managed archive or "
    + "staging"
    IL_0255:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_025a:  throw

    IL_025b:  ldloc.2
    IL_025c:  stloc.s    V_5
    IL_025e:  br.s       IL_0260

    IL_0260:  ldloc.s    V_5
    IL_0262:  ret
  } // end of method InstallUpgrade096::ValidatePointer

  .method private hidebysig static void  ValidatePayload(string package,
                                                         string source) cil managed
  {
    // Code size       705 (0x2c1)
    .maxstack  4
    .locals init (string V_0,
             class [mscorlib]System.Collections.Generic.List`1<class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>> V_1,
             class [System.Core]System.Collections.Generic.HashSet`1<string> V_2,
             class [mscorlib]System.Collections.Generic.Dictionary`2<string,object> V_3,
             string V_4,
             string V_5,
             string V_6,
             bool V_7,
             valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>> V_8,
             char[] V_9,
             class [mscorlib]System.Collections.Generic.IEnumerator`1<string> V_10)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldstr      "SHA256SUMS.json"
    IL_0007:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_000c:  stloc.0
    IL_000d:  ldloc.0
    IL_000e:  call       bool [mscorlib]System.IO.File::Exists(string)
    IL_0013:  stloc.s    V_7
    IL_0015:  ldloc.s    V_7
    IL_0017:  brtrue.s   IL_0024

    IL_0019:  ldstr      "Missing SHA256SUMS.json"
    IL_001e:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_0023:  throw

    IL_0024:  ldloc.0
    IL_0025:  call       void InstallUpgrade096::NoLinks(string)
    IL_002a:  nop
    IL_002b:  ldsfld     class [System.Web.Extensions]System.Web.Script.Serialization.JavaScriptSerializer InstallUpgrade096::Json
    IL_0030:  ldloc.0
    IL_0031:  call       string [mscorlib]System.IO.File::ReadAllText(string)
    IL_0036:  callvirt   instance !!0 [System.Web.Extensions]System.Web.Script.Serialization.JavaScriptSerializer::Deserialize<class [mscorlib]System.Collections.Generic.List`1<class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>>>(string)
    IL_003b:  stloc.1
    IL_003c:  ldloc.1
    IL_003d:  brfalse.s  IL_004d

    IL_003f:  ldloc.1
    IL_0040:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.List`1<class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>>::get_Count()
    IL_0045:  ldc.i4.0
    IL_0046:  ceq
    IL_0048:  ldc.i4.0
    IL_0049:  ceq
    IL_004b:  br.s       IL_004e

    IL_004d:  ldc.i4.0
    IL_004e:  nop
    IL_004f:  stloc.s    V_7
    IL_0051:  ldloc.s    V_7
    IL_0053:  brtrue.s   IL_0060

    IL_0055:  ldstr      "Empty package manifest"
    IL_005a:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_005f:  throw

    IL_0060:  call       class [mscorlib]System.StringComparer [mscorlib]System.StringComparer::get_OrdinalIgnoreCase()
    IL_0065:  newobj     instance void class [System.Core]System.Collections.Generic.HashSet`1<string>::.ctor(class [mscorlib]System.Collections.Generic.IEqualityComparer`1<!0>)
    IL_006a:  stloc.2
    IL_006b:  nop
    IL_006c:  ldloc.1
    IL_006d:  callvirt   instance valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<!0> class [mscorlib]System.Collections.Generic.List`1<class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>>::GetEnumerator()
    IL_0072:  stloc.s    V_8
    .try
    {
      IL_0074:  br         IL_0222

      IL_0079:  ldloca.s   V_8
      IL_007b:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>>::get_Current()
      IL_0080:  stloc.3
      IL_0081:  nop
      IL_0082:  ldloc.3
      IL_0083:  brfalse.s  IL_00c6

      IL_0085:  ldloc.3
      IL_0086:  ldstr      "path"
      IL_008b:  callvirt   instance bool class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>::ContainsKey(!0)
      IL_0090:  brfalse.s  IL_00c6

      IL_0092:  ldloc.3
      IL_0093:  ldstr      "sha256"
      IL_0098:  callvirt   instance bool class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>::ContainsKey(!0)
      IL_009d:  brfalse.s  IL_00c6

      IL_009f:  ldloc.3
      IL_00a0:  ldstr      "path"
      IL_00a5:  callvirt   instance !1 class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>::get_Item(!0)
      IL_00aa:  isinst     [mscorlib]System.String
      IL_00af:  brfalse.s  IL_00c6

      IL_00b1:  ldloc.3
      IL_00b2:  ldstr      "sha256"
      IL_00b7:  callvirt   instance !1 class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>::get_Item(!0)
      IL_00bc:  isinst     [mscorlib]System.String
      IL_00c1:  ldnull
      IL_00c2:  cgt.un
      IL_00c4:  br.s       IL_00c7

      IL_00c6:  ldc.i4.0
      IL_00c7:  nop
      IL_00c8:  stloc.s    V_7
      IL_00ca:  ldloc.s    V_7
      IL_00cc:  brtrue.s   IL_00d9

      IL_00ce:  ldstr      "Invalid manifest entry"
      IL_00d3:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_00d8:  throw

      IL_00d9:  ldloc.3
      IL_00da:  ldstr      "path"
      IL_00df:  callvirt   instance !1 class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>::get_Item(!0)
      IL_00e4:  castclass  [mscorlib]System.String
      IL_00e9:  stloc.s    V_4
      IL_00eb:  ldloc.3
      IL_00ec:  ldstr      "sha256"
      IL_00f1:  callvirt   instance !1 class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>::get_Item(!0)
      IL_00f6:  castclass  [mscorlib]System.String
      IL_00fb:  stloc.s    V_5
      IL_00fd:  ldloc.s    V_4
      IL_00ff:  call       bool [mscorlib]System.String::IsNullOrWhiteSpace(string)
      IL_0104:  brtrue     IL_0197

      IL_0109:  ldloc.s    V_4
      IL_010b:  ldloc.s    V_4
      IL_010d:  callvirt   instance string [mscorlib]System.String::Trim()
      IL_0112:  call       bool [mscorlib]System.String::op_Inequality(string,
                                                                       string)
      IL_0117:  brtrue.s   IL_0197

      IL_0119:  ldloc.s    V_4
      IL_011b:  call       bool [mscorlib]System.IO.Path::IsPathRooted(string)
      IL_0120:  brtrue.s   IL_0197

      IL_0122:  ldloc.s    V_4
      IL_0124:  ldc.i4.3
      IL_0125:  newarr     [mscorlib]System.Char
      IL_012a:  stloc.s    V_9
      IL_012c:  ldloc.s    V_9
      IL_012e:  ldc.i4.0
      IL_012f:  ldc.i4.s   92
      IL_0131:  stelem.i2
      IL_0132:  ldloc.s    V_9
      IL_0134:  ldc.i4.1
      IL_0135:  ldc.i4.s   58
      IL_0137:  stelem.i2
      IL_0138:  ldloc.s    V_9
      IL_013a:  callvirt   instance int32 [mscorlib]System.String::IndexOfAny(char[])
      IL_013f:  ldc.i4.0
      IL_0140:  bge.s      IL_0197

      IL_0142:  ldloc.s    V_4
      IL_0144:  ldc.i4.1
      IL_0145:  newarr     [mscorlib]System.Char
      IL_014a:  stloc.s    V_9
      IL_014c:  ldloc.s    V_9
      IL_014e:  ldc.i4.0
      IL_014f:  ldc.i4.s   47
      IL_0151:  stelem.i2
      IL_0152:  ldloc.s    V_9
      IL_0154:  callvirt   instance string[] [mscorlib]System.String::Split(char[])
      IL_0159:  ldsfld     class [mscorlib]System.Func`2<string,bool> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegatee'
      IL_015e:  brtrue.s   IL_0173

      IL_0160:  ldnull
      IL_0161:  ldftn      bool InstallUpgrade096::'<ValidatePayload>b__d'(string)
      IL_0167:  newobj     instance void class [mscorlib]System.Func`2<string,bool>::.ctor(object,
                                                                                           native int)
      IL_016c:  stsfld     class [mscorlib]System.Func`2<string,bool> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegatee'
      IL_0171:  br.s       IL_0173

      IL_0173:  ldsfld     class [mscorlib]System.Func`2<string,bool> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegatee'
      IL_0178:  call       bool [System.Core]System.Linq.Enumerable::Any<string>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                 class [mscorlib]System.Func`2<!!0,bool>)
      IL_017d:  brtrue.s   IL_0197

      IL_017f:  ldloc.s    V_5
      IL_0181:  ldstr      "\\A[0-9a-fA-F]{64}\\z"
      IL_0186:  call       bool [System]System.Text.RegularExpressions.Regex::IsMatch(string,
                                                                                      string)
      IL_018b:  brfalse.s  IL_0197

      IL_018d:  ldloc.2
      IL_018e:  ldloc.s    V_4
      IL_0190:  callvirt   instance bool class [System.Core]System.Collections.Generic.HashSet`1<string>::Add(!0)
      IL_0195:  br.s       IL_0198

      IL_0197:  ldc.i4.0
      IL_0198:  nop
      IL_0199:  stloc.s    V_7
      IL_019b:  ldloc.s    V_7
      IL_019d:  brtrue.s   IL_01aa

      IL_019f:  ldstr      "Unsafe or duplicate manifest entry"
      IL_01a4:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_01a9:  throw

      IL_01aa:  ldarg.0
      IL_01ab:  ldloc.s    V_4
      IL_01ad:  ldc.i4.s   47
      IL_01af:  ldsfld     char [mscorlib]System.IO.Path::DirectorySeparatorChar
      IL_01b4:  callvirt   instance string [mscorlib]System.String::Replace(char,
                                                                            char)
      IL_01b9:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_01be:  call       string [mscorlib]System.IO.Path::GetFullPath(string)
      IL_01c3:  stloc.s    V_6
      IL_01c5:  ldarg.0
      IL_01c6:  ldloc.s    V_6
      IL_01c8:  call       bool InstallUpgrade096::Inside(string,
                                                          string)
      IL_01cd:  brfalse.s  IL_01d8

      IL_01cf:  ldloc.s    V_6
      IL_01d1:  call       bool [mscorlib]System.IO.File::Exists(string)
      IL_01d6:  br.s       IL_01d9

      IL_01d8:  ldc.i4.0
      IL_01d9:  nop
      IL_01da:  stloc.s    V_7
      IL_01dc:  ldloc.s    V_7
      IL_01de:  brtrue.s   IL_01f2

      IL_01e0:  ldstr      "Missing manifest file: "
      IL_01e5:  ldloc.s    V_4
      IL_01e7:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_01ec:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_01f1:  throw

      IL_01f2:  ldloc.s    V_6
      IL_01f4:  call       void InstallUpgrade096::NoLinks(string)
      IL_01f9:  nop
      IL_01fa:  ldloc.s    V_6
      IL_01fc:  call       string InstallUpgrade096::Hash(string)
      IL_0201:  ldloc.s    V_5
      IL_0203:  ldc.i4.5
      IL_0204:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                         valuetype [mscorlib]System.StringComparison)
      IL_0209:  stloc.s    V_7
      IL_020b:  ldloc.s    V_7
      IL_020d:  brtrue.s   IL_0221

      IL_020f:  ldstr      "Payload hash mismatch: "
      IL_0214:  ldloc.s    V_4
      IL_0216:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_021b:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_0220:  throw

      IL_0221:  nop
      IL_0222:  ldloca.s   V_8
      IL_0224:  call       instance bool valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>>::MoveNext()
      IL_0229:  stloc.s    V_7
      IL_022b:  ldloc.s    V_7
      IL_022d:  brtrue     IL_0079

      IL_0232:  leave.s    IL_0243

    }  // end .try
    finally
    {
      IL_0234:  ldloca.s   V_8
      IL_0236:  constrained. valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>>
      IL_023c:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_0241:  nop
      IL_0242:  endfinally
    }  // end handler
    IL_0243:  nop
    IL_0244:  nop
    IL_0245:  ldarg.1
    IL_0246:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<string> InstallUpgrade096::Files(string)
    IL_024b:  callvirt   instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<string>::GetEnumerator()
    IL_0250:  stloc.s    V_10
    .try
    {
      IL_0252:  br.s       IL_029c

      IL_0254:  ldloc.s    V_10
      IL_0256:  callvirt   instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<string>::get_Current()
      IL_025b:  stloc.s    V_6
      IL_025d:  nop
      IL_025e:  ldloc.s    V_6
      IL_0260:  ldarg.0
      IL_0261:  callvirt   instance int32 [mscorlib]System.String::get_Length()
      IL_0266:  ldc.i4.1
      IL_0267:  add
      IL_0268:  callvirt   instance string [mscorlib]System.String::Substring(int32)
      IL_026d:  ldsfld     char [mscorlib]System.IO.Path::DirectorySeparatorChar
      IL_0272:  ldc.i4.s   47
      IL_0274:  callvirt   instance string [mscorlib]System.String::Replace(char,
                                                                            char)
      IL_0279:  stloc.s    V_4
      IL_027b:  ldloc.2
      IL_027c:  ldloc.s    V_4
      IL_027e:  callvirt   instance bool class [System.Core]System.Collections.Generic.HashSet`1<string>::Contains(!0)
      IL_0283:  stloc.s    V_7
      IL_0285:  ldloc.s    V_7
      IL_0287:  brtrue.s   IL_029b

      IL_0289:  ldstr      "Unlisted payload file: "
      IL_028e:  ldloc.s    V_4
      IL_0290:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_0295:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_029a:  throw

      IL_029b:  nop
      IL_029c:  ldloc.s    V_10
      IL_029e:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
      IL_02a3:  stloc.s    V_7
      IL_02a5:  ldloc.s    V_7
      IL_02a7:  brtrue.s   IL_0254

      IL_02a9:  leave.s    IL_02bf

    }  // end .try
    finally
    {
      IL_02ab:  ldloc.s    V_10
      IL_02ad:  ldnull
      IL_02ae:  ceq
      IL_02b0:  stloc.s    V_7
      IL_02b2:  ldloc.s    V_7
      IL_02b4:  brtrue.s   IL_02be

      IL_02b6:  ldloc.s    V_10
      IL_02b8:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_02bd:  nop
      IL_02be:  endfinally
    }  // end handler
    IL_02bf:  nop
    IL_02c0:  ret
  } // end of method InstallUpgrade096::ValidatePayload

  .method private hidebysig static void  CopyEmptyDirectories(string from,
                                                              string 'to') cil managed
  {
    // Code size       101 (0x65)
    .maxstack  2
    .locals init (string V_0,
             string V_1,
             string[] V_2,
             int32 V_3,
             bool V_4)
    IL_0000:  nop
    IL_0001:  nop
    IL_0002:  ldarg.0
    IL_0003:  call       string[] [mscorlib]System.IO.Directory::GetDirectories(string)
    IL_0008:  stloc.2
    IL_0009:  ldc.i4.0
    IL_000a:  stloc.3
    IL_000b:  br.s       IL_0058

    IL_000d:  ldloc.2
    IL_000e:  ldloc.3
    IL_000f:  ldelem.ref
    IL_0010:  stloc.0
    IL_0011:  nop
    IL_0012:  ldloc.0
    IL_0013:  call       valuetype [mscorlib]System.IO.FileAttributes [mscorlib]System.IO.File::GetAttributes(string)
    IL_0018:  ldc.i4     0x400
    IL_001d:  and
    IL_001e:  ldc.i4.0
    IL_001f:  ceq
    IL_0021:  stloc.s    V_4
    IL_0023:  ldloc.s    V_4
    IL_0025:  brtrue.s   IL_0032

    IL_0027:  ldstr      "Linked preserved directory"
    IL_002c:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_0031:  throw

    IL_0032:  ldarg.1
    IL_0033:  ldloc.0
    IL_0034:  call       string [mscorlib]System.IO.Path::GetFileName(string)
    IL_0039:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_003e:  call       string InstallUpgrade096::Safe(string)
    IL_0043:  stloc.1
    IL_0044:  ldloc.1
    IL_0045:  call       class [mscorlib]System.IO.DirectoryInfo [mscorlib]System.IO.Directory::CreateDirectory(string)
    IL_004a:  pop
    IL_004b:  ldloc.0
    IL_004c:  ldloc.1
    IL_004d:  call       void InstallUpgrade096::CopyEmptyDirectories(string,
                                                                      string)
    IL_0052:  nop
    IL_0053:  nop
    IL_0054:  ldloc.3
    IL_0055:  ldc.i4.1
    IL_0056:  add
    IL_0057:  stloc.3
    IL_0058:  ldloc.3
    IL_0059:  ldloc.2
    IL_005a:  ldlen
    IL_005b:  conv.i4
    IL_005c:  clt
    IL_005e:  stloc.s    V_4
    IL_0060:  ldloc.s    V_4
    IL_0062:  brtrue.s   IL_000d

    IL_0064:  ret
  } // end of method InstallUpgrade096::CopyEmptyDirectories

  .method public hidebysig static int32  Run(string[] args) cil managed
  {
    // Code size       2778 (0xada)
    .maxstack  8
    .locals init (bool V_0,
             string V_1,
             string V_2,
             string V_3,
             string V_4,
             string V_5,
             class [mscorlib]System.Collections.Generic.List`1<class InstallUpgrade096/MoveRecord> V_6,
             bool V_7,
             class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> V_8,
             class [mscorlib]System.Collections.Generic.Dictionary`2<string,object> V_9,
             string V_10,
             string[] V_11,
             string V_12,
             string V_13,
             class [mscorlib]System.IO.FileStream V_14,
             string V_15,
             int32 V_16,
             int32 V_17,
             string V_18,
             class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> V_19,
             valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string> V_20,
             class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> V_21,
             string V_22,
             class InstallUpgrade096/MoveRecord V_23,
             class InstallUpgrade096/MoveRecord V_24,
             class [mscorlib]System.Func`2<string,bool> V_25,
             class InstallUpgrade096/'<>c__DisplayClass1c' V_26,
             class [mscorlib]System.Exception V_27,
             class [mscorlib]System.Exception V_28,
             string V_29,
             int32 V_30,
             valuetype [mscorlib]System.Guid V_31,
             valuetype [mscorlib]System.DateTime V_32,
             bool V_33,
             char[] V_34,
             string[] V_35,
             int32 V_36,
             class [mscorlib]System.Collections.Generic.IEnumerator`1<string> V_37,
             string[] V_38,
             valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<string> V_39,
             valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string> V_40,
             class InstallUpgrade096/MoveRecord[] V_41,
             class [mscorlib]System.Collections.Generic.IEnumerator`1<class InstallUpgrade096/MoveRecord> V_42)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldstr      "--test"
    IL_0007:  call       bool [System.Core]System.Linq.Enumerable::Contains<string>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                    !!0)
    IL_000c:  stloc.0
    IL_000d:  call       class [mscorlib]System.AppDomain [mscorlib]System.AppDomain::get_CurrentDomain()
    IL_0012:  callvirt   instance string [mscorlib]System.AppDomain::get_BaseDirectory()
    IL_0017:  ldstr      "MEPS060"
    IL_001c:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_0021:  stloc.1
    IL_0022:  ldsfld     string InstallUpgrade096::Base
    IL_0027:  ldstr      "MEPS060"
    IL_002c:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_0031:  stloc.2
    IL_0032:  call       valuetype [mscorlib]System.Guid [mscorlib]System.Guid::NewGuid()
    IL_0037:  stloc.s    V_31
    IL_0039:  ldloca.s   V_31
    IL_003b:  ldstr      "N"
    IL_0040:  call       instance string [mscorlib]System.Guid::ToString(string)
    IL_0045:  stloc.3
    IL_0046:  ldsfld     string InstallUpgrade096::Base
    IL_004b:  ldstr      ".MEPS060_INSTALL_"
    IL_0050:  ldloc.3
    IL_0051:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_0056:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_005b:  stloc.s    V_4
    IL_005d:  ldsfld     string InstallUpgrade096::Base
    IL_0062:  ldstr      "ARCHIVE"
    IL_0067:  ldstr      "MEPS060"
    IL_006c:  call       valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::get_Now()
    IL_0071:  stloc.s    V_32
    IL_0073:  ldloca.s   V_32
    IL_0075:  ldstr      "yyyyMMdd_HHmmss"
    IL_007a:  call       instance string [mscorlib]System.DateTime::ToString(string)
    IL_007f:  ldstr      "_"
    IL_0084:  ldloc.3
    IL_0085:  ldc.i4.0
    IL_0086:  ldc.i4.6
    IL_0087:  callvirt   instance string [mscorlib]System.String::Substring(int32,
                                                                            int32)
    IL_008c:  call       string [mscorlib]System.String::Concat(string,
                                                                string,
                                                                string)
    IL_0091:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string,
                                                                  string,
                                                                  string)
    IL_0096:  stloc.s    V_5
    IL_0098:  newobj     instance void class [mscorlib]System.Collections.Generic.List`1<class InstallUpgrade096/MoveRecord>::.ctor()
    IL_009d:  stloc.s    V_6
    IL_009f:  ldc.i4.0
    IL_00a0:  stloc.s    V_7
    .try
    {
      IL_00a2:  ldnull
      IL_00a3:  stloc.s    V_25
      IL_00a5:  newobj     instance void InstallUpgrade096/'<>c__DisplayClass1c'::.ctor()
      IL_00aa:  stloc.s    V_26
      IL_00ac:  nop
      IL_00ad:  ldloc.0
      IL_00ae:  brtrue.s   IL_00c4

      IL_00b0:  ldstr      "acad"
      IL_00b5:  call       class [System]System.Diagnostics.Process[] [System]System.Diagnostics.Process::GetProcessesByName(string)
      IL_00ba:  ldlen
      IL_00bb:  conv.i4
      IL_00bc:  ldc.i4.0
      IL_00bd:  cgt
      IL_00bf:  ldc.i4.0
      IL_00c0:  ceq
      IL_00c2:  br.s       IL_00c5

      IL_00c4:  ldc.i4.1
      IL_00c5:  nop
      IL_00c6:  stloc.s    V_33
      IL_00c8:  ldloc.s    V_33
      IL_00ca:  brtrue.s   IL_00d7

      IL_00cc:  ldstr      "Hay luu va dong AutoCAD truoc khi cai. Chua thay d"
      + "oi ban cu."
      IL_00d1:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_00d6:  throw

      IL_00d7:  ldloc.1
      IL_00d8:  ldstr      "TVXD.Meps.dll"
      IL_00dd:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_00e2:  call       bool [mscorlib]System.IO.File::Exists(string)
      IL_00e7:  stloc.s    V_33
      IL_00e9:  ldloc.s    V_33
      IL_00eb:  brtrue.s   IL_00f8

      IL_00ed:  ldstr      "Giai nen TOAN BO ZIP truoc khi cai."
      IL_00f2:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_00f7:  throw

      IL_00f8:  call       class [mscorlib]System.AppDomain [mscorlib]System.AppDomain::get_CurrentDomain()
      IL_00fd:  callvirt   instance string [mscorlib]System.AppDomain::get_BaseDirectory()
      IL_0102:  ldc.i4.2
      IL_0103:  newarr     [mscorlib]System.Char
      IL_0108:  stloc.s    V_34
      IL_010a:  ldloc.s    V_34
      IL_010c:  ldc.i4.0
      IL_010d:  ldsfld     char [mscorlib]System.IO.Path::DirectorySeparatorChar
      IL_0112:  stelem.i2
      IL_0113:  ldloc.s    V_34
      IL_0115:  ldc.i4.1
      IL_0116:  ldsfld     char [mscorlib]System.IO.Path::AltDirectorySeparatorChar
      IL_011b:  stelem.i2
      IL_011c:  ldloc.s    V_34
      IL_011e:  callvirt   instance string [mscorlib]System.String::TrimEnd(char[])
      IL_0123:  ldloc.1
      IL_0124:  call       void InstallUpgrade096::ValidatePayload(string,
                                                                   string)
      IL_0129:  nop
      IL_012a:  ldloc.1
      IL_012b:  call       class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> InstallUpgrade096::ManagedSdnlHashes(string)
      IL_0130:  stloc.s    V_8
      IL_0132:  ldsfld     class [System.Web.Extensions]System.Web.Script.Serialization.JavaScriptSerializer InstallUpgrade096::Json
      IL_0137:  ldloc.1
      IL_0138:  ldstr      "RELEASE.json"
      IL_013d:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_0142:  call       string [mscorlib]System.IO.File::ReadAllText(string)
      IL_0147:  callvirt   instance !!0 [System.Web.Extensions]System.Web.Script.Serialization.JavaScriptSerializer::Deserialize<class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>>(string)
      IL_014c:  stloc.s    V_9
      IL_014e:  ldloc.s    V_9
      IL_0150:  ldstr      "dll_sha256"
      IL_0155:  callvirt   instance !1 class [mscorlib]System.Collections.Generic.Dictionary`2<string,object>::get_Item(!0)
      IL_015a:  call       string [mscorlib]System.Convert::ToString(object)
      IL_015f:  stloc.s    V_10
      IL_0161:  ldloc.1
      IL_0162:  ldstr      "TVXD.Meps.dll"
      IL_0167:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_016c:  call       string InstallUpgrade096::Hash(string)
      IL_0171:  ldloc.s    V_10
      IL_0173:  call       bool [mscorlib]System.String::op_Inequality(string,
                                                                       string)
      IL_0178:  ldc.i4.0
      IL_0179:  ceq
      IL_017b:  stloc.s    V_33
      IL_017d:  ldloc.s    V_33
      IL_017f:  brtrue.s   IL_018c

      IL_0181:  ldstr      "DLL khong khop bo cai."
      IL_0186:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_018b:  throw

      IL_018c:  ldsfld     string InstallUpgrade096::Base
      IL_0191:  call       class [mscorlib]System.IO.DirectoryInfo [mscorlib]System.IO.Directory::CreateDirectory(string)
      IL_0196:  pop
      IL_0197:  ldloc.s    V_4
      IL_0199:  call       string InstallUpgrade096::Safe(string)
      IL_019e:  pop
      IL_019f:  ldloc.2
      IL_01a0:  call       string InstallUpgrade096::Safe(string)
      IL_01a5:  pop
      IL_01a6:  ldloc.s    V_5
      IL_01a8:  call       string InstallUpgrade096::Safe(string)
      IL_01ad:  pop
      IL_01ae:  ldsfld     string InstallUpgrade096::Base
      IL_01b3:  call       string[] [mscorlib]System.IO.Directory::GetDirectories(string)
      IL_01b8:  ldsfld     class [mscorlib]System.Func`2<string,bool> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegate17'
      IL_01bd:  brtrue.s   IL_01d2

      IL_01bf:  ldnull
      IL_01c0:  ldftn      bool InstallUpgrade096::'<Run>b__10'(string)
      IL_01c6:  newobj     instance void class [mscorlib]System.Func`2<string,bool>::.ctor(object,
                                                                                           native int)
      IL_01cb:  stsfld     class [mscorlib]System.Func`2<string,bool> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegate17'
      IL_01d0:  br.s       IL_01d2

      IL_01d2:  ldsfld     class [mscorlib]System.Func`2<string,bool> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegate17'
      IL_01d7:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0> [System.Core]System.Linq.Enumerable::Where<string>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                            class [mscorlib]System.Func`2<!!0,bool>)
      IL_01dc:  ldnull
      IL_01dd:  ldftn      valuetype [mscorlib]System.DateTime [mscorlib]System.IO.Directory::GetLastWriteTime(string)
      IL_01e3:  newobj     instance void class [mscorlib]System.Func`2<string,valuetype [mscorlib]System.DateTime>::.ctor(object,
                                                                                                                          native int)
      IL_01e8:  call       class [System.Core]System.Linq.IOrderedEnumerable`1<!!0> [System.Core]System.Linq.Enumerable::OrderByDescending<string,valuetype [mscorlib]System.DateTime>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                                       class [mscorlib]System.Func`2<!!0,!!1>)
      IL_01ed:  call       !!0[] [System.Core]System.Linq.Enumerable::ToArray<string>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
      IL_01f2:  stloc.s    V_11
      IL_01f4:  nop
      IL_01f5:  ldloc.s    V_11
      IL_01f7:  stloc.s    V_35
      IL_01f9:  ldc.i4.0
      IL_01fa:  stloc.s    V_36
      IL_01fc:  br         IL_02f8

      IL_0201:  ldloc.s    V_35
      IL_0203:  ldloc.s    V_36
      IL_0205:  ldelem.ref
      IL_0206:  stloc.s    V_12
      IL_0208:  nop
      IL_0209:  ldloc.s    V_12
      IL_020b:  call       string InstallUpgrade096::Safe(string)
      IL_0210:  pop
      IL_0211:  ldloc.s    V_12
      IL_0213:  ldstr      "TVXD.Meps.dll"
      IL_0218:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_021d:  call       bool [mscorlib]System.IO.File::Exists(string)
      IL_0222:  brfalse.s  IL_023c

      IL_0224:  ldloc.s    V_12
      IL_0226:  ldstr      "LIBRARY_0_6"
      IL_022b:  ldstr      "catalog.json"
      IL_0230:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string,
                                                                    string)
      IL_0235:  call       bool [mscorlib]System.IO.File::Exists(string)
      IL_023a:  br.s       IL_023d

      IL_023c:  ldc.i4.0
      IL_023d:  nop
      IL_023e:  stloc.s    V_33
      IL_0240:  ldloc.s    V_33
      IL_0242:  brtrue.s   IL_0256

      IL_0244:  ldstr      "Thu muc chua xac dinh la MEPS; giu nguyen: "
      IL_0249:  ldloc.s    V_12
      IL_024b:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_0250:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_0255:  throw

      IL_0256:  nop
      IL_0257:  ldloc.s    V_12
      IL_0259:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<string> InstallUpgrade096::Files(string)
      IL_025e:  ldsfld     class [mscorlib]System.Func`2<string,bool> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegate18'
      IL_0263:  brtrue.s   IL_0278

      IL_0265:  ldnull
      IL_0266:  ldftn      bool InstallUpgrade096::'<Run>b__11'(string)
      IL_026c:  newobj     instance void class [mscorlib]System.Func`2<string,bool>::.ctor(object,
                                                                                           native int)
      IL_0271:  stsfld     class [mscorlib]System.Func`2<string,bool> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegate18'
      IL_0276:  br.s       IL_0278

      IL_0278:  ldsfld     class [mscorlib]System.Func`2<string,bool> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegate18'
      IL_027d:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0> [System.Core]System.Linq.Enumerable::Where<string>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                            class [mscorlib]System.Func`2<!!0,bool>)
      IL_0282:  callvirt   instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<string>::GetEnumerator()
      IL_0287:  stloc.s    V_37
      .try
      {
        IL_0289:  br.s       IL_02cd

        IL_028b:  ldloc.s    V_37
        IL_028d:  callvirt   instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<string>::get_Current()
        IL_0292:  stloc.s    V_13
        IL_0294:  nop
        .try
        {
          IL_0295:  nop
          IL_0296:  ldloc.s    V_13
          IL_0298:  ldc.i4.3
          IL_0299:  ldc.i4.3
          IL_029a:  ldc.i4.0
          IL_029b:  call       class [mscorlib]System.IO.FileStream [mscorlib]System.IO.File::Open(string,
                                                                                                   valuetype [mscorlib]System.IO.FileMode,
                                                                                                   valuetype [mscorlib]System.IO.FileAccess,
                                                                                                   valuetype [mscorlib]System.IO.FileShare)
          IL_02a0:  stloc.s    V_14
          .try
          {
            IL_02a2:  nop
            IL_02a3:  nop
            IL_02a4:  leave.s    IL_02ba

          }  // end .try
          finally
          {
            IL_02a6:  ldloc.s    V_14
            IL_02a8:  ldnull
            IL_02a9:  ceq
            IL_02ab:  stloc.s    V_33
            IL_02ad:  ldloc.s    V_33
            IL_02af:  brtrue.s   IL_02b9

            IL_02b1:  ldloc.s    V_14
            IL_02b3:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
            IL_02b8:  nop
            IL_02b9:  endfinally
          }  // end handler
          IL_02ba:  nop
          IL_02bb:  nop
          IL_02bc:  leave.s    IL_02cb

        }  // end .try
        catch [mscorlib]System.Object 
        {
          IL_02be:  pop
          IL_02bf:  nop
          IL_02c0:  ldstr      "Hay luu va dong AutoCAD dang nap MEPS cu, roi chay"
          + " lai. Chua thay doi ban cu."
          IL_02c5:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
          IL_02ca:  throw

        }  // end handler
        IL_02cb:  nop
        IL_02cc:  nop
        IL_02cd:  ldloc.s    V_37
        IL_02cf:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
        IL_02d4:  stloc.s    V_33
        IL_02d6:  ldloc.s    V_33
        IL_02d8:  brtrue.s   IL_028b

        IL_02da:  leave.s    IL_02f0

      }  // end .try
      finally
      {
        IL_02dc:  ldloc.s    V_37
        IL_02de:  ldnull
        IL_02df:  ceq
        IL_02e1:  stloc.s    V_33
        IL_02e3:  ldloc.s    V_33
        IL_02e5:  brtrue.s   IL_02ef

        IL_02e7:  ldloc.s    V_37
        IL_02e9:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_02ee:  nop
        IL_02ef:  endfinally
      }  // end handler
      IL_02f0:  nop
      IL_02f1:  nop
      IL_02f2:  ldloc.s    V_36
      IL_02f4:  ldc.i4.1
      IL_02f5:  add
      IL_02f6:  stloc.s    V_36
      IL_02f8:  ldloc.s    V_36
      IL_02fa:  ldloc.s    V_35
      IL_02fc:  ldlen
      IL_02fd:  conv.i4
      IL_02fe:  clt
      IL_0300:  stloc.s    V_33
      IL_0302:  ldloc.s    V_33
      IL_0304:  brtrue     IL_0201

      IL_0309:  ldloc.s    V_11
      IL_030b:  ldlen
      IL_030c:  conv.i4
      IL_030d:  ldc.i4.1
      IL_030e:  beq.s      IL_0313

      IL_0310:  ldnull
      IL_0311:  br.s       IL_0317

      IL_0313:  ldloc.s    V_11
      IL_0315:  ldc.i4.0
      IL_0316:  ldelem.ref
      IL_0317:  nop
      IL_0318:  stloc.s    V_15
      IL_031a:  ldloc.s    V_11
      IL_031c:  ldlen
      IL_031d:  conv.i4
      IL_031e:  ldc.i4.1
      IL_031f:  cgt
      IL_0321:  ldc.i4.0
      IL_0322:  ceq
      IL_0324:  stloc.s    V_33
      IL_0326:  ldloc.s    V_33
      IL_0328:  brtrue     IL_03cd

      IL_032d:  nop
      IL_032e:  ldloc.0
      IL_032f:  ldc.i4.0
      IL_0330:  ceq
      IL_0332:  stloc.s    V_33
      IL_0334:  ldloc.s    V_33
      IL_0336:  brtrue.s   IL_0343

      IL_0338:  ldstr      "Multiple old installations: interactive library ch"
      + "oice required"
      IL_033d:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_0342:  throw

      IL_0343:  ldstr      "Co nhieu thu vien cu. Chon thu vien dang dung de g"
      + "iu:"
      IL_0348:  call       void [mscorlib]System.Console::WriteLine(string)
      IL_034d:  nop
      IL_034e:  ldc.i4.0
      IL_034f:  stloc.s    V_16
      IL_0351:  br.s       IL_0377

      IL_0353:  ldloc.s    V_16
      IL_0355:  ldc.i4.1
      IL_0356:  add
      IL_0357:  box        [mscorlib]System.Int32
      IL_035c:  ldstr      ". "
      IL_0361:  ldloc.s    V_11
      IL_0363:  ldloc.s    V_16
      IL_0365:  ldelem.ref
      IL_0366:  call       string [mscorlib]System.String::Concat(object,
                                                                  object,
                                                                  object)
      IL_036b:  call       void [mscorlib]System.Console::WriteLine(string)
      IL_0370:  nop
      IL_0371:  ldloc.s    V_16
      IL_0373:  ldc.i4.1
      IL_0374:  add
      IL_0375:  stloc.s    V_16
      IL_0377:  ldloc.s    V_16
      IL_0379:  ldloc.s    V_11
      IL_037b:  ldlen
      IL_037c:  conv.i4
      IL_037d:  clt
      IL_037f:  stloc.s    V_33
      IL_0381:  ldloc.s    V_33
      IL_0383:  brtrue.s   IL_0353

      IL_0385:  ldstr      "Nhap so: "
      IL_038a:  call       void [mscorlib]System.Console::Write(string)
      IL_038f:  nop
      IL_0390:  call       string [mscorlib]System.Console::ReadLine()
      IL_0395:  ldloca.s   V_17
      IL_0397:  call       bool [mscorlib]System.Int32::TryParse(string,
                                                                 int32&)
      IL_039c:  brfalse.s  IL_03b0

      IL_039e:  ldloc.s    V_17
      IL_03a0:  ldc.i4.1
      IL_03a1:  blt.s      IL_03b0

      IL_03a3:  ldloc.s    V_17
      IL_03a5:  ldloc.s    V_11
      IL_03a7:  ldlen
      IL_03a8:  conv.i4
      IL_03a9:  cgt
      IL_03ab:  ldc.i4.0
      IL_03ac:  ceq
      IL_03ae:  br.s       IL_03b1

      IL_03b0:  ldc.i4.0
      IL_03b1:  nop
      IL_03b2:  stloc.s    V_33
      IL_03b4:  ldloc.s    V_33
      IL_03b6:  brtrue.s   IL_03c3

      IL_03b8:  ldstr      "Lua chon khong hop le; chua thay doi ban cu."
      IL_03bd:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_03c2:  throw

      IL_03c3:  ldloc.s    V_11
      IL_03c5:  ldloc.s    V_17
      IL_03c7:  ldc.i4.1
      IL_03c8:  sub
      IL_03c9:  ldelem.ref
      IL_03ca:  stloc.s    V_15
      IL_03cc:  nop
      IL_03cd:  ldloc.s    V_26
      IL_03cf:  newobj     instance void class [mscorlib]System.Collections.Generic.List`1<string>::.ctor()
      IL_03d4:  stfld      class [mscorlib]System.Collections.Generic.List`1<string> InstallUpgrade096/'<>c__DisplayClass1c'::preserveRoots
      IL_03d9:  ldloc.s    V_26
      IL_03db:  ldnull
      IL_03dc:  stfld      string InstallUpgrade096/'<>c__DisplayClass1c'::localPointer
      IL_03e1:  ldloc.s    V_26
      IL_03e3:  ldnull
      IL_03e4:  stfld      string InstallUpgrade096/'<>c__DisplayClass1c'::pointerValue
      IL_03e9:  ldloc.s    V_15
      IL_03eb:  ldnull
      IL_03ec:  ceq
      IL_03ee:  stloc.s    V_33
      IL_03f0:  ldloc.s    V_33
      IL_03f2:  brtrue     IL_04d1

      IL_03f7:  nop
      IL_03f8:  nop
      IL_03f9:  ldc.i4.4
      IL_03fa:  newarr     [mscorlib]System.String
      IL_03ff:  stloc.s    V_38
      IL_0401:  ldloc.s    V_38
      IL_0403:  ldc.i4.0
      IL_0404:  ldstr      "LIBRARY_0_6"
      IL_0409:  stelem.ref
      IL_040a:  ldloc.s    V_38
      IL_040c:  ldc.i4.1
      IL_040d:  ldstr      "HTKT_LIBRARY"
      IL_0412:  stelem.ref
      IL_0413:  ldloc.s    V_38
      IL_0415:  ldc.i4.2
      IL_0416:  ldstr      "RECOVERY_DATA"
      IL_041b:  stelem.ref
      IL_041c:  ldloc.s    V_38
      IL_041e:  ldc.i4.3
      IL_041f:  ldstr      "SDNL"
      IL_0424:  stelem.ref
      IL_0425:  ldloc.s    V_38
      IL_0427:  stloc.s    V_35
      IL_0429:  ldc.i4.0
      IL_042a:  stloc.s    V_36
      IL_042c:  br.s       IL_0461

      IL_042e:  ldloc.s    V_35
      IL_0430:  ldloc.s    V_36
      IL_0432:  ldelem.ref
      IL_0433:  stloc.s    V_18
      IL_0435:  ldloc.s    V_15
      IL_0437:  ldloc.s    V_18
      IL_0439:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_043e:  call       bool [mscorlib]System.IO.Directory::Exists(string)
      IL_0443:  ldc.i4.0
      IL_0444:  ceq
      IL_0446:  stloc.s    V_33
      IL_0448:  ldloc.s    V_33
      IL_044a:  brtrue.s   IL_045b

      IL_044c:  ldloc.s    V_26
      IL_044e:  ldfld      class [mscorlib]System.Collections.Generic.List`1<string> InstallUpgrade096/'<>c__DisplayClass1c'::preserveRoots
      IL_0453:  ldloc.s    V_18
      IL_0455:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_045a:  nop
      IL_045b:  ldloc.s    V_36
      IL_045d:  ldc.i4.1
      IL_045e:  add
      IL_045f:  stloc.s    V_36
      IL_0461:  ldloc.s    V_36
      IL_0463:  ldloc.s    V_35
      IL_0465:  ldlen
      IL_0466:  conv.i4
      IL_0467:  clt
      IL_0469:  stloc.s    V_33
      IL_046b:  ldloc.s    V_33
      IL_046d:  brtrue.s   IL_042e

      IL_046f:  ldloc.s    V_26
      IL_0471:  ldloc.s    V_15
      IL_0473:  ldloc.s    V_11
      IL_0475:  ldloc.s    V_26
      IL_0477:  ldflda     string InstallUpgrade096/'<>c__DisplayClass1c'::localPointer
      IL_047c:  call       string InstallUpgrade096::ValidatePointer(string,
                                                                     string[],
                                                                     string&)
      IL_0481:  stfld      string InstallUpgrade096/'<>c__DisplayClass1c'::pointerValue
      IL_0486:  ldloc.s    V_26
      IL_0488:  ldfld      string InstallUpgrade096/'<>c__DisplayClass1c'::localPointer
      IL_048d:  brfalse.s  IL_04b4

      IL_048f:  ldloc.s    V_26
      IL_0491:  ldfld      class [mscorlib]System.Collections.Generic.List`1<string> InstallUpgrade096/'<>c__DisplayClass1c'::preserveRoots
      IL_0496:  ldloc.s    V_25
      IL_0498:  brtrue.s   IL_04ab

      IL_049a:  ldloc.s    V_26
      IL_049c:  ldftn      instance bool InstallUpgrade096/'<>c__DisplayClass1c'::'<Run>b__12'(string)
      IL_04a2:  newobj     instance void class [mscorlib]System.Func`2<string,bool>::.ctor(object,
                                                                                           native int)
      IL_04a7:  stloc.s    V_25
      IL_04a9:  br.s       IL_04ab

      IL_04ab:  ldloc.s    V_25
      IL_04ad:  call       bool [System.Core]System.Linq.Enumerable::Any<string>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                 class [mscorlib]System.Func`2<!!0,bool>)
      IL_04b2:  br.s       IL_04b5

      IL_04b4:  ldc.i4.1
      IL_04b5:  nop
      IL_04b6:  stloc.s    V_33
      IL_04b8:  ldloc.s    V_33
      IL_04ba:  brtrue.s   IL_04d0

      IL_04bc:  ldloc.s    V_26
      IL_04be:  ldfld      class [mscorlib]System.Collections.Generic.List`1<string> InstallUpgrade096/'<>c__DisplayClass1c'::preserveRoots
      IL_04c3:  ldloc.s    V_26
      IL_04c5:  ldfld      string InstallUpgrade096/'<>c__DisplayClass1c'::localPointer
      IL_04ca:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<string>::Add(!0)
      IL_04cf:  nop
      IL_04d0:  nop
      IL_04d1:  call       class [mscorlib]System.StringComparer [mscorlib]System.StringComparer::get_OrdinalIgnoreCase()
      IL_04d6:  newobj     instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::.ctor(class [mscorlib]System.Collections.Generic.IEqualityComparer`1<!0>)
      IL_04db:  stloc.s    V_19
      IL_04dd:  ldloc.s    V_15
      IL_04df:  ldnull
      IL_04e0:  ceq
      IL_04e2:  stloc.s    V_33
      IL_04e4:  ldloc.s    V_33
      IL_04e6:  brtrue     IL_05a5

      IL_04eb:  nop
      IL_04ec:  ldloc.s    V_26
      IL_04ee:  ldfld      class [mscorlib]System.Collections.Generic.List`1<string> InstallUpgrade096/'<>c__DisplayClass1c'::preserveRoots
      IL_04f3:  callvirt   instance valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<!0> class [mscorlib]System.Collections.Generic.List`1<string>::GetEnumerator()
      IL_04f8:  stloc.s    V_39
      .try
      {
        IL_04fa:  br         IL_0583

        IL_04ff:  ldloca.s   V_39
        IL_0501:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<string>::get_Current()
        IL_0506:  stloc.s    V_18
        IL_0508:  nop
        IL_0509:  ldloc.s    V_15
        IL_050b:  ldloc.s    V_18
        IL_050d:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                      string)
        IL_0512:  call       class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> InstallUpgrade096::Hashes(string)
        IL_0517:  callvirt   instance valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<!0,!1> class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::GetEnumerator()
        IL_051c:  stloc.s    V_40
        .try
        {
          IL_051e:  br.s       IL_0564

          IL_0520:  ldloca.s   V_40
          IL_0522:  call       instance valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<!0,!1> valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string>::get_Current()
          IL_0527:  stloc.s    V_20
          IL_0529:  ldsfld     class [System.Core]System.Collections.Generic.HashSet`1<string> InstallUpgrade096::ManagedSdnl
          IL_052e:  ldloc.s    V_18
          IL_0530:  ldloca.s   V_20
          IL_0532:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
          IL_0537:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                        string)
          IL_053c:  callvirt   instance bool class [System.Core]System.Collections.Generic.HashSet`1<string>::Contains(!0)
          IL_0541:  stloc.s    V_33
          IL_0543:  ldloc.s    V_33
          IL_0545:  brtrue.s   IL_0564

          IL_0547:  ldloc.s    V_19
          IL_0549:  ldloc.s    V_18
          IL_054b:  ldloca.s   V_20
          IL_054d:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
          IL_0552:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                        string)
          IL_0557:  ldloca.s   V_20
          IL_0559:  call       instance !1 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Value()
          IL_055e:  callvirt   instance void class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::set_Item(!0,
                                                                                                                              !1)
          IL_0563:  nop
          IL_0564:  ldloca.s   V_40
          IL_0566:  call       instance bool valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string>::MoveNext()
          IL_056b:  stloc.s    V_33
          IL_056d:  ldloc.s    V_33
          IL_056f:  brtrue.s   IL_0520

          IL_0571:  leave.s    IL_0582

        }  // end .try
        finally
        {
          IL_0573:  ldloca.s   V_40
          IL_0575:  constrained. valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string>
          IL_057b:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
          IL_0580:  nop
          IL_0581:  endfinally
        }  // end handler
        IL_0582:  nop
        IL_0583:  ldloca.s   V_39
        IL_0585:  call       instance bool valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<string>::MoveNext()
        IL_058a:  stloc.s    V_33
        IL_058c:  ldloc.s    V_33
        IL_058e:  brtrue     IL_04ff

        IL_0593:  leave.s    IL_05a4

      }  // end .try
      finally
      {
        IL_0595:  ldloca.s   V_39
        IL_0597:  constrained. valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<string>
        IL_059d:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_05a2:  nop
        IL_05a3:  endfinally
      }  // end handler
      IL_05a4:  nop
      IL_05a5:  ldloc.1
      IL_05a6:  call       class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> InstallUpgrade096::Hashes(string)
      IL_05ab:  ldloc.s    V_26
      IL_05ad:  ldftn      instance bool InstallUpgrade096/'<>c__DisplayClass1c'::'<Run>b__13'(valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>)
      IL_05b3:  newobj     instance void class [mscorlib]System.Func`2<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>,bool>::.ctor(object,
                                                                                                                                                                 native int)
      IL_05b8:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0> [System.Core]System.Linq.Enumerable::Where<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                                                                  class [mscorlib]System.Func`2<!!0,bool>)
      IL_05bd:  ldsfld     class [mscorlib]System.Func`2<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>,string> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegate19'
      IL_05c2:  brtrue.s   IL_05d7

      IL_05c4:  ldnull
      IL_05c5:  ldftn      string InstallUpgrade096::'<Run>b__15'(valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>)
      IL_05cb:  newobj     instance void class [mscorlib]System.Func`2<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>,string>::.ctor(object,
                                                                                                                                                                   native int)
      IL_05d0:  stsfld     class [mscorlib]System.Func`2<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>,string> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegate19'
      IL_05d5:  br.s       IL_05d7

      IL_05d7:  ldsfld     class [mscorlib]System.Func`2<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>,string> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegate19'
      IL_05dc:  ldsfld     class [mscorlib]System.Func`2<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>,string> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegate1a'
      IL_05e1:  brtrue.s   IL_05f6

      IL_05e3:  ldnull
      IL_05e4:  ldftn      string InstallUpgrade096::'<Run>b__16'(valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>)
      IL_05ea:  newobj     instance void class [mscorlib]System.Func`2<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>,string>::.ctor(object,
                                                                                                                                                                   native int)
      IL_05ef:  stsfld     class [mscorlib]System.Func`2<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>,string> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegate1a'
      IL_05f4:  br.s       IL_05f6

      IL_05f6:  ldsfld     class [mscorlib]System.Func`2<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>,string> InstallUpgrade096::'CS$<>9__CachedAnonymousMethodDelegate1a'
      IL_05fb:  call       class [mscorlib]System.StringComparer [mscorlib]System.StringComparer::get_OrdinalIgnoreCase()
      IL_0600:  call       class [mscorlib]System.Collections.Generic.Dictionary`2<!!1,!!2> [System.Core]System.Linq.Enumerable::ToDictionary<valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>,string,string>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                                                                                          class [mscorlib]System.Func`2<!!0,!!1>,
                                                                                                                                                                                                                                          class [mscorlib]System.Func`2<!!0,!!2>,
                                                                                                                                                                                                                                          class [mscorlib]System.Collections.Generic.IEqualityComparer`1<!!1>)
      IL_0605:  stloc.s    V_21
      IL_0607:  nop
      IL_0608:  ldloc.s    V_8
      IL_060a:  callvirt   instance valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<!0,!1> class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::GetEnumerator()
      IL_060f:  stloc.s    V_40
      .try
      {
        IL_0611:  br.s       IL_066a

        IL_0613:  ldloca.s   V_40
        IL_0615:  call       instance valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<!0,!1> valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string>::get_Current()
        IL_061a:  stloc.s    V_20
        IL_061c:  ldloc.s    V_21
        IL_061e:  ldloca.s   V_20
        IL_0620:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
        IL_0625:  callvirt   instance bool class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::ContainsKey(!0)
        IL_062a:  brfalse.s  IL_064b

        IL_062c:  ldloc.s    V_21
        IL_062e:  ldloca.s   V_20
        IL_0630:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
        IL_0635:  callvirt   instance !1 class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::get_Item(!0)
        IL_063a:  ldloca.s   V_20
        IL_063c:  call       instance !1 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Value()
        IL_0641:  call       bool [mscorlib]System.String::op_Inequality(string,
                                                                         string)
        IL_0646:  ldc.i4.0
        IL_0647:  ceq
        IL_0649:  br.s       IL_064c

        IL_064b:  ldc.i4.0
        IL_064c:  nop
        IL_064d:  stloc.s    V_33
        IL_064f:  ldloc.s    V_33
        IL_0651:  brtrue.s   IL_066a

        IL_0653:  ldstr      "Managed SDNL source changed: "
        IL_0658:  ldloca.s   V_20
        IL_065a:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
        IL_065f:  call       string [mscorlib]System.String::Concat(string,
                                                                    string)
        IL_0664:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
        IL_0669:  throw

        IL_066a:  ldloca.s   V_40
        IL_066c:  call       instance bool valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string>::MoveNext()
        IL_0671:  stloc.s    V_33
        IL_0673:  ldloc.s    V_33
        IL_0675:  brtrue.s   IL_0613

        IL_0677:  leave.s    IL_0688

      }  // end .try
      finally
      {
        IL_0679:  ldloca.s   V_40
        IL_067b:  constrained. valuetype [mscorlib]System.Collections.Generic.Dictionary`2/Enumerator<string,string>
        IL_0681:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_0686:  nop
        IL_0687:  endfinally
      }  // end handler
      IL_0688:  nop
      IL_0689:  ldloc.s    V_4
      IL_068b:  call       class [mscorlib]System.IO.DirectoryInfo [mscorlib]System.IO.Directory::CreateDirectory(string)
      IL_0690:  pop
      IL_0691:  ldloc.1
      IL_0692:  ldloc.s    V_4
      IL_0694:  ldloc.s    V_21
      IL_0696:  call       void InstallUpgrade096::CopyTree(string,
                                                            string,
                                                            class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>)
      IL_069b:  nop
      IL_069c:  ldloc.s    V_15
      IL_069e:  ldnull
      IL_069f:  ceq
      IL_06a1:  stloc.s    V_33
      IL_06a3:  ldloc.s    V_33
      IL_06a5:  brtrue.s   IL_0719

      IL_06a7:  nop
      IL_06a8:  nop
      IL_06a9:  ldloc.s    V_26
      IL_06ab:  ldfld      class [mscorlib]System.Collections.Generic.List`1<string> InstallUpgrade096/'<>c__DisplayClass1c'::preserveRoots
      IL_06b0:  callvirt   instance valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<!0> class [mscorlib]System.Collections.Generic.List`1<string>::GetEnumerator()
      IL_06b5:  stloc.s    V_39
      .try
      {
        IL_06b7:  br.s       IL_06ed

        IL_06b9:  ldloca.s   V_39
        IL_06bb:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<string>::get_Current()
        IL_06c0:  stloc.s    V_18
        IL_06c2:  nop
        IL_06c3:  ldloc.s    V_4
        IL_06c5:  ldloc.s    V_18
        IL_06c7:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                      string)
        IL_06cc:  call       string InstallUpgrade096::Safe(string)
        IL_06d1:  stloc.s    V_22
        IL_06d3:  ldloc.s    V_22
        IL_06d5:  call       class [mscorlib]System.IO.DirectoryInfo [mscorlib]System.IO.Directory::CreateDirectory(string)
        IL_06da:  pop
        IL_06db:  ldloc.s    V_15
        IL_06dd:  ldloc.s    V_18
        IL_06df:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                      string)
        IL_06e4:  ldloc.s    V_22
        IL_06e6:  call       void InstallUpgrade096::CopyEmptyDirectories(string,
                                                                          string)
        IL_06eb:  nop
        IL_06ec:  nop
        IL_06ed:  ldloca.s   V_39
        IL_06ef:  call       instance bool valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<string>::MoveNext()
        IL_06f4:  stloc.s    V_33
        IL_06f6:  ldloc.s    V_33
        IL_06f8:  brtrue.s   IL_06b9

        IL_06fa:  leave.s    IL_070b

      }  // end .try
      finally
      {
        IL_06fc:  ldloca.s   V_39
        IL_06fe:  constrained. valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<string>
        IL_0704:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_0709:  nop
        IL_070a:  endfinally
      }  // end handler
      IL_070b:  nop
      IL_070c:  ldloc.s    V_15
      IL_070e:  ldloc.s    V_4
      IL_0710:  ldloc.s    V_19
      IL_0712:  call       void InstallUpgrade096::CopyTree(string,
                                                            string,
                                                            class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>)
      IL_0717:  nop
      IL_0718:  nop
      IL_0719:  ldloc.s    V_26
      IL_071b:  ldfld      string InstallUpgrade096/'<>c__DisplayClass1c'::pointerValue
      IL_0720:  ldnull
      IL_0721:  ceq
      IL_0723:  stloc.s    V_33
      IL_0725:  ldloc.s    V_33
      IL_0727:  brtrue.s   IL_0748

      IL_0729:  ldloc.s    V_4
      IL_072b:  ldstr      "MEPS_LIBRARY_PATH.txt"
      IL_0730:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_0735:  ldloc.s    V_26
      IL_0737:  ldfld      string InstallUpgrade096/'<>c__DisplayClass1c'::pointerValue
      IL_073c:  ldc.i4.0
      IL_073d:  newobj     instance void [mscorlib]System.Text.UTF8Encoding::.ctor(bool)
      IL_0742:  call       void [mscorlib]System.IO.File::WriteAllText(string,
                                                                       string,
                                                                       class [mscorlib]System.Text.Encoding)
      IL_0747:  nop
      IL_0748:  ldloc.s    V_4
      IL_074a:  ldloc.s    V_8
      IL_074c:  call       void InstallUpgrade096::Verify(string,
                                                          class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>)
      IL_0751:  nop
      IL_0752:  ldloc.s    V_11
      IL_0754:  ldlen
      IL_0755:  conv.i4
      IL_0756:  ldc.i4.0
      IL_0757:  cgt
      IL_0759:  ldc.i4.0
      IL_075a:  ceq
      IL_075c:  stloc.s    V_33
      IL_075e:  ldloc.s    V_33
      IL_0760:  brtrue.s   IL_076a

      IL_0762:  ldloc.s    V_5
      IL_0764:  call       class [mscorlib]System.IO.DirectoryInfo [mscorlib]System.IO.Directory::CreateDirectory(string)
      IL_0769:  pop
      IL_076a:  nop
      IL_076b:  ldloc.s    V_11
      IL_076d:  stloc.s    V_35
      IL_076f:  ldc.i4.0
      IL_0770:  stloc.s    V_36
      IL_0772:  br         IL_0847

      IL_0777:  ldloc.s    V_35
      IL_0779:  ldloc.s    V_36
      IL_077b:  ldelem.ref
      IL_077c:  stloc.s    V_12
      IL_077e:  nop
      IL_077f:  newobj     instance void InstallUpgrade096/MoveRecord::.ctor()
      IL_0784:  stloc.s    V_24
      IL_0786:  ldloc.s    V_24
      IL_0788:  ldloc.s    V_12
      IL_078a:  call       string InstallUpgrade096::Safe(string)
      IL_078f:  stfld      string InstallUpgrade096/MoveRecord::old_path
      IL_0794:  ldloc.s    V_24
      IL_0796:  ldloc.s    V_5
      IL_0798:  ldloc.s    V_12
      IL_079a:  call       string [mscorlib]System.IO.Path::GetFileName(string)
      IL_079f:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_07a4:  call       string InstallUpgrade096::Safe(string)
      IL_07a9:  stfld      string InstallUpgrade096/MoveRecord::new_path
      IL_07ae:  ldloc.s    V_24
      IL_07b0:  ldloc.s    V_12
      IL_07b2:  call       class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> InstallUpgrade096::Hashes(string)
      IL_07b7:  stfld      class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> InstallUpgrade096/MoveRecord::files
      IL_07bc:  ldloc.s    V_24
      IL_07be:  stloc.s    V_23
      IL_07c0:  ldloc.s    V_23
      IL_07c2:  ldfld      string InstallUpgrade096/MoveRecord::new_path
      IL_07c7:  call       bool [mscorlib]System.IO.Directory::Exists(string)
      IL_07cc:  ldc.i4.0
      IL_07cd:  ceq
      IL_07cf:  stloc.s    V_33
      IL_07d1:  ldloc.s    V_33
      IL_07d3:  brtrue.s   IL_07e0

      IL_07d5:  ldstr      "Backup destination exists"
      IL_07da:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_07df:  throw

      IL_07e0:  ldloc.s    V_5
      IL_07e2:  ldstr      "MANIFEST.json"
      IL_07e7:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_07ec:  ldloc.s    V_6
      IL_07ee:  ldc.i4.1
      IL_07ef:  newarr     InstallUpgrade096/MoveRecord
      IL_07f4:  stloc.s    V_41
      IL_07f6:  ldloc.s    V_41
      IL_07f8:  ldc.i4.0
      IL_07f9:  ldloc.s    V_23
      IL_07fb:  stelem.ref
      IL_07fc:  ldloc.s    V_41
      IL_07fe:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0> [System.Core]System.Linq.Enumerable::Concat<class InstallUpgrade096/MoveRecord>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>,
                                                                                                                                                                         class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
      IL_0803:  call       !!0[] [System.Core]System.Linq.Enumerable::ToArray<class InstallUpgrade096/MoveRecord>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
      IL_0808:  call       void InstallUpgrade096::Save(string,
                                                        object)
      IL_080d:  nop
      IL_080e:  ldloc.s    V_23
      IL_0810:  ldfld      string InstallUpgrade096/MoveRecord::old_path
      IL_0815:  ldloc.s    V_23
      IL_0817:  ldfld      string InstallUpgrade096/MoveRecord::new_path
      IL_081c:  call       void [mscorlib]System.IO.Directory::Move(string,
                                                                    string)
      IL_0821:  nop
      IL_0822:  ldloc.s    V_6
      IL_0824:  ldloc.s    V_23
      IL_0826:  callvirt   instance void class [mscorlib]System.Collections.Generic.List`1<class InstallUpgrade096/MoveRecord>::Add(!0)
      IL_082b:  nop
      IL_082c:  ldloc.s    V_23
      IL_082e:  ldfld      string InstallUpgrade096/MoveRecord::new_path
      IL_0833:  ldloc.s    V_23
      IL_0835:  ldfld      class [mscorlib]System.Collections.Generic.Dictionary`2<string,string> InstallUpgrade096/MoveRecord::files
      IL_083a:  call       void InstallUpgrade096::Verify(string,
                                                          class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>)
      IL_083f:  nop
      IL_0840:  nop
      IL_0841:  ldloc.s    V_36
      IL_0843:  ldc.i4.1
      IL_0844:  add
      IL_0845:  stloc.s    V_36
      IL_0847:  ldloc.s    V_36
      IL_0849:  ldloc.s    V_35
      IL_084b:  ldlen
      IL_084c:  conv.i4
      IL_084d:  clt
      IL_084f:  stloc.s    V_33
      IL_0851:  ldloc.s    V_33
      IL_0853:  brtrue     IL_0777

      IL_0858:  ldstr      "after-archive"
      IL_085d:  call       void InstallUpgrade096::Fault(string)
      IL_0862:  nop
      IL_0863:  ldloc.s    V_4
      IL_0865:  call       string InstallUpgrade096::Safe(string)
      IL_086a:  ldloc.2
      IL_086b:  call       string InstallUpgrade096::Safe(string)
      IL_0870:  call       void [mscorlib]System.IO.Directory::Move(string,
                                                                    string)
      IL_0875:  nop
      IL_0876:  ldc.i4.1
      IL_0877:  stloc.s    V_7
      IL_0879:  ldstr      "after-activate"
      IL_087e:  call       void InstallUpgrade096::Fault(string)
      IL_0883:  nop
      IL_0884:  ldloc.2
      IL_0885:  ldloc.s    V_19
      IL_0887:  call       void InstallUpgrade096::Verify(string,
                                                          class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>)
      IL_088c:  nop
      IL_088d:  ldloc.2
      IL_088e:  ldloc.s    V_8
      IL_0890:  call       void InstallUpgrade096::Verify(string,
                                                          class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>)
      IL_0895:  nop
      IL_0896:  ldloc.2
      IL_0897:  ldstr      "TVXD.Meps.dll"
      IL_089c:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_08a1:  call       string InstallUpgrade096::Hash(string)
      IL_08a6:  ldloc.s    V_10
      IL_08a8:  call       bool [mscorlib]System.String::op_Inequality(string,
                                                                       string)
      IL_08ad:  ldc.i4.0
      IL_08ae:  ceq
      IL_08b0:  stloc.s    V_33
      IL_08b2:  ldloc.s    V_33
      IL_08b4:  brtrue.s   IL_08c1

      IL_08b6:  ldstr      "Installed DLL mismatch"
      IL_08bb:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
      IL_08c0:  throw

      IL_08c1:  ldloc.2
      IL_08c2:  ldstr      "INSTALL_RESULT.json"
      IL_08c7:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_08cc:  ldstr      "PASS"
      IL_08d1:  ldloc.2
      IL_08d2:  ldloc.s    V_11
      IL_08d4:  ldlen
      IL_08d5:  conv.i4
      IL_08d6:  ldloc.s    V_19
      IL_08d8:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.Dictionary`2<string,string>::get_Count()
      IL_08dd:  ldloc.s    V_11
      IL_08df:  ldlen
      IL_08e0:  conv.i4
      IL_08e1:  ldc.i4.0
      IL_08e2:  bgt.s      IL_08e7

      IL_08e4:  ldnull
      IL_08e5:  br.s       IL_08e9

      IL_08e7:  ldloc.s    V_5
      IL_08e9:  nop
      IL_08ea:  newobj     instance void class '<>f__AnonymousType0`5'<string,string,int32,int32,string>::.ctor(!0,
                                                                                                                !1,
                                                                                                                !2,
                                                                                                                !3,
                                                                                                                !4)
      IL_08ef:  call       void InstallUpgrade096::Save(string,
                                                        object)
      IL_08f4:  nop
      IL_08f5:  ldstr      "Da cai/nang cap MEPS: "
      IL_08fa:  ldloc.2
      IL_08fb:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_0900:  call       void [mscorlib]System.Console::WriteLine(string)
      IL_0905:  nop
      IL_0906:  ldstr      "Chi con mot thu muc MEPS060 dang dung; du lieu tro"
      + "ng LIBRARY_0_6, HTKT_LIBRARY, RECOVERY_DATA, SDNL cu da duoc giu (neu c"
      + "o), tru 3 tep chuong trinh SDNL duoc cap nhat."
      IL_090b:  call       void [mscorlib]System.Console::WriteLine(string)
      IL_0910:  nop
      IL_0911:  ldloc.s    V_11
      IL_0913:  ldlen
      IL_0914:  conv.i4
      IL_0915:  ldc.i4.0
      IL_0916:  cgt
      IL_0918:  ldc.i4.0
      IL_0919:  ceq
      IL_091b:  stloc.s    V_33
      IL_091d:  ldloc.s    V_33
      IL_091f:  brtrue.s   IL_0933

      IL_0921:  ldstr      "Ban cu da roi vi tri cai; ban du phong: "
      IL_0926:  ldloc.s    V_5
      IL_0928:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_092d:  call       void [mscorlib]System.Console::WriteLine(string)
      IL_0932:  nop
      IL_0933:  ldstr      "Mo lai CAD, NETLOAD: "
      IL_0938:  ldloc.2
      IL_0939:  ldstr      "TVXD.Meps.dll"
      IL_093e:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                    string)
      IL_0943:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_0948:  call       void [mscorlib]System.Console::WriteLine(string)
      IL_094d:  nop
      IL_094e:  ldstr      "Sau do go T-MEP."
      IL_0953:  call       void [mscorlib]System.Console::WriteLine(string)
      IL_0958:  nop
      IL_0959:  ldc.i4.0
      IL_095a:  stloc.s    V_30
      IL_095c:  leave      IL_0ad6

    }  // end .try
    catch [mscorlib]System.Exception 
    {
      IL_0961:  stloc.s    V_27
      IL_0963:  nop
      .try
      {
        IL_0964:  nop
        IL_0965:  ldloc.s    V_7
        IL_0967:  brfalse.s  IL_0974

        IL_0969:  ldloc.2
        IL_096a:  call       bool [mscorlib]System.IO.Directory::Exists(string)
        IL_096f:  ldc.i4.0
        IL_0970:  ceq
        IL_0972:  br.s       IL_0975

        IL_0974:  ldc.i4.1
        IL_0975:  nop
        IL_0976:  stloc.s    V_33
        IL_0978:  ldloc.s    V_33
        IL_097a:  brtrue.s   IL_0999

        IL_097c:  ldloc.2
        IL_097d:  call       string InstallUpgrade096::Safe(string)
        IL_0982:  ldloc.s    V_4
        IL_0984:  ldstr      "_FAILED"
        IL_0989:  call       string [mscorlib]System.String::Concat(string,
                                                                    string)
        IL_098e:  call       string InstallUpgrade096::Safe(string)
        IL_0993:  call       void [mscorlib]System.IO.Directory::Move(string,
                                                                      string)
        IL_0998:  nop
        IL_0999:  nop
        IL_099a:  ldloc.s    V_6
        IL_099c:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0> [System.Core]System.Linq.Enumerable::AsEnumerable<class InstallUpgrade096/MoveRecord>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
        IL_09a1:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0> [System.Core]System.Linq.Enumerable::Reverse<class InstallUpgrade096/MoveRecord>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
        IL_09a6:  callvirt   instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<class InstallUpgrade096/MoveRecord>::GetEnumerator()
        IL_09ab:  stloc.s    V_42
        .try
        {
          IL_09ad:  br.s       IL_09fd

          IL_09af:  ldloc.s    V_42
          IL_09b1:  callvirt   instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<class InstallUpgrade096/MoveRecord>::get_Current()
          IL_09b6:  stloc.s    V_23
          IL_09b8:  ldloc.s    V_23
          IL_09ba:  ldfld      string InstallUpgrade096/MoveRecord::old_path
          IL_09bf:  call       bool [mscorlib]System.IO.Directory::Exists(string)
          IL_09c4:  brtrue.s   IL_09d7

          IL_09c6:  ldloc.s    V_23
          IL_09c8:  ldfld      string InstallUpgrade096/MoveRecord::new_path
          IL_09cd:  call       bool [mscorlib]System.IO.Directory::Exists(string)
          IL_09d2:  ldc.i4.0
          IL_09d3:  ceq
          IL_09d5:  br.s       IL_09d8

          IL_09d7:  ldc.i4.1
          IL_09d8:  nop
          IL_09d9:  stloc.s    V_33
          IL_09db:  ldloc.s    V_33
          IL_09dd:  brtrue.s   IL_09fd

          IL_09df:  ldloc.s    V_23
          IL_09e1:  ldfld      string InstallUpgrade096/MoveRecord::new_path
          IL_09e6:  call       string InstallUpgrade096::Safe(string)
          IL_09eb:  ldloc.s    V_23
          IL_09ed:  ldfld      string InstallUpgrade096/MoveRecord::old_path
          IL_09f2:  call       string InstallUpgrade096::Safe(string)
          IL_09f7:  call       void [mscorlib]System.IO.Directory::Move(string,
                                                                        string)
          IL_09fc:  nop
          IL_09fd:  ldloc.s    V_42
          IL_09ff:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
          IL_0a04:  stloc.s    V_33
          IL_0a06:  ldloc.s    V_33
          IL_0a08:  brtrue.s   IL_09af

          IL_0a0a:  leave.s    IL_0a20

        }  // end .try
        finally
        {
          IL_0a0c:  ldloc.s    V_42
          IL_0a0e:  ldnull
          IL_0a0f:  ceq
          IL_0a11:  stloc.s    V_33
          IL_0a13:  ldloc.s    V_33
          IL_0a15:  brtrue.s   IL_0a1f

          IL_0a17:  ldloc.s    V_42
          IL_0a19:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
          IL_0a1e:  nop
          IL_0a1f:  endfinally
        }  // end handler
        IL_0a20:  nop
        IL_0a21:  nop
        IL_0a22:  leave.s    IL_0a48

      }  // end .try
      catch [mscorlib]System.Exception 
      {
        IL_0a24:  stloc.s    V_28
        IL_0a26:  nop
        IL_0a27:  ldstr      "Can khoi phuc tu "
        IL_0a2c:  ldloc.s    V_5
        IL_0a2e:  ldstr      ": "
        IL_0a33:  ldloc.s    V_28
        IL_0a35:  callvirt   instance string [mscorlib]System.Exception::get_Message()
        IL_0a3a:  call       string [mscorlib]System.String::Concat(string,
                                                                    string,
                                                                    string,
                                                                    string)
        IL_0a3f:  call       void [mscorlib]System.Console::WriteLine(string)
        IL_0a44:  nop
        IL_0a45:  nop
        IL_0a46:  leave.s    IL_0a48

      }  // end handler
      IL_0a48:  nop
      IL_0a49:  ldstr      "CHUA CAI XONG: "
      IL_0a4e:  ldloc.s    V_27
      IL_0a50:  callvirt   instance string [mscorlib]System.Exception::get_Message()
      IL_0a55:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_0a5a:  call       void [mscorlib]System.Console::WriteLine(string)
      IL_0a5f:  nop
      IL_0a60:  nop
      IL_0a61:  ldc.i4.2
      IL_0a62:  newarr     [mscorlib]System.String
      IL_0a67:  stloc.s    V_38
      IL_0a69:  ldloc.s    V_38
      IL_0a6b:  ldc.i4.0
      IL_0a6c:  ldloc.s    V_4
      IL_0a6e:  stelem.ref
      IL_0a6f:  ldloc.s    V_38
      IL_0a71:  ldc.i4.1
      IL_0a72:  ldloc.s    V_4
      IL_0a74:  ldstr      "_FAILED"
      IL_0a79:  call       string [mscorlib]System.String::Concat(string,
                                                                  string)
      IL_0a7e:  stelem.ref
      IL_0a7f:  ldloc.s    V_38
      IL_0a81:  stloc.s    V_35
      IL_0a83:  ldc.i4.0
      IL_0a84:  stloc.s    V_36
      IL_0a86:  br.s       IL_0ac3

      IL_0a88:  ldloc.s    V_35
      IL_0a8a:  ldloc.s    V_36
      IL_0a8c:  ldelem.ref
      IL_0a8d:  stloc.s    V_29
      IL_0a8f:  ldloc.s    V_29
      IL_0a91:  call       bool [mscorlib]System.IO.Directory::Exists(string)
      IL_0a96:  ldc.i4.0
      IL_0a97:  ceq
      IL_0a99:  stloc.s    V_33
      IL_0a9b:  ldloc.s    V_33
      IL_0a9d:  brtrue.s   IL_0abd

      IL_0a9f:  ldstr      "Thu muc tam duoc giu de kiem tra, khong tu xoa: "
      IL_0aa4:  ldloc.s    V_29
      IL_0aa6:  ldstr      "; ly do: "
      IL_0aab:  ldloc.s    V_27
      IL_0aad:  callvirt   instance string [mscorlib]System.Exception::get_Message()
      IL_0ab2:  call       string [mscorlib]System.String::Concat(string,
                                                                  string,
                                                                  string,
                                                                  string)
      IL_0ab7:  call       void [mscorlib]System.Console::WriteLine(string)
      IL_0abc:  nop
      IL_0abd:  ldloc.s    V_36
      IL_0abf:  ldc.i4.1
      IL_0ac0:  add
      IL_0ac1:  stloc.s    V_36
      IL_0ac3:  ldloc.s    V_36
      IL_0ac5:  ldloc.s    V_35
      IL_0ac7:  ldlen
      IL_0ac8:  conv.i4
      IL_0ac9:  clt
      IL_0acb:  stloc.s    V_33
      IL_0acd:  ldloc.s    V_33
      IL_0acf:  brtrue.s   IL_0a88

      IL_0ad1:  ldc.i4.1
      IL_0ad2:  stloc.s    V_30
      IL_0ad4:  leave.s    IL_0ad6

    }  // end handler
    IL_0ad6:  nop
    IL_0ad7:  ldloc.s    V_30
    IL_0ad9:  ret
  } // end of method InstallUpgrade096::Run

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  } // end of method InstallUpgrade096::.ctor

  .method private hidebysig static bool  '<ValidatePayload>b__d'(string x) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       73 (0x49)
    .maxstack  4
    .locals init (bool V_0,
             char[] V_1)
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance int32 [mscorlib]System.String::get_Length()
    IL_0006:  brfalse.s  IL_0042

    IL_0008:  ldarg.0
    IL_0009:  ldstr      "."
    IL_000e:  call       bool [mscorlib]System.String::op_Equality(string,
                                                                   string)
    IL_0013:  brtrue.s   IL_0042

    IL_0015:  ldarg.0
    IL_0016:  ldstr      ".."
    IL_001b:  call       bool [mscorlib]System.String::op_Equality(string,
                                                                   string)
    IL_0020:  brtrue.s   IL_0042

    IL_0022:  ldarg.0
    IL_0023:  ldc.i4.2
    IL_0024:  newarr     [mscorlib]System.Char
    IL_0029:  stloc.1
    IL_002a:  ldloc.1
    IL_002b:  ldc.i4.0
    IL_002c:  ldc.i4.s   32
    IL_002e:  stelem.i2
    IL_002f:  ldloc.1
    IL_0030:  ldc.i4.1
    IL_0031:  ldc.i4.s   46
    IL_0033:  stelem.i2
    IL_0034:  ldloc.1
    IL_0035:  callvirt   instance string [mscorlib]System.String::TrimEnd(char[])
    IL_003a:  ldarg.0
    IL_003b:  call       bool [mscorlib]System.String::op_Inequality(string,
                                                                     string)
    IL_0040:  br.s       IL_0043

    IL_0042:  ldc.i4.1
    IL_0043:  nop
    IL_0044:  stloc.0
    IL_0045:  br.s       IL_0047

    IL_0047:  ldloc.0
    IL_0048:  ret
  } // end of method InstallUpgrade096::'<ValidatePayload>b__d'

  .method private hidebysig static bool  '<Run>b__10'(string d) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       22 (0x16)
    .maxstack  3
    .locals init (bool V_0)
    IL_0000:  ldarg.0
    IL_0001:  call       string [mscorlib]System.IO.Path::GetFileName(string)
    IL_0006:  ldstr      "^MEPS060(\?:_\\d{8}(\?:_R\\d+)\?)\?$"
    IL_000b:  ldc.i4.1
    IL_000c:  call       bool [System]System.Text.RegularExpressions.Regex::IsMatch(string,
                                                                                    string,
                                                                                    valuetype [System]System.Text.RegularExpressions.RegexOptions)
    IL_0011:  stloc.0
    IL_0012:  br.s       IL_0014

    IL_0014:  ldloc.0
    IL_0015:  ret
  } // end of method InstallUpgrade096::'<Run>b__10'

  .method private hidebysig static bool  '<Run>b__11'(string f) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       22 (0x16)
    .maxstack  3
    .locals init (bool V_0)
    IL_0000:  ldarg.0
    IL_0001:  call       string [mscorlib]System.IO.Path::GetExtension(string)
    IL_0006:  ldstr      ".dll"
    IL_000b:  ldc.i4.5
    IL_000c:  callvirt   instance bool [mscorlib]System.String::Equals(string,
                                                                       valuetype [mscorlib]System.StringComparison)
    IL_0011:  stloc.0
    IL_0012:  br.s       IL_0014

    IL_0014:  ldloc.0
    IL_0015:  ret
  } // end of method InstallUpgrade096::'<Run>b__11'

  .method private hidebysig static string 
          '<Run>b__15'(valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string> p) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       12 (0xc)
    .maxstack  1
    .locals init (string V_0)
    IL_0000:  ldarga.s   p
    IL_0002:  call       instance !0 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Key()
    IL_0007:  stloc.0
    IL_0008:  br.s       IL_000a

    IL_000a:  ldloc.0
    IL_000b:  ret
  } // end of method InstallUpgrade096::'<Run>b__15'

  .method private hidebysig static string 
          '<Run>b__16'(valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string> p) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       12 (0xc)
    .maxstack  1
    .locals init (string V_0)
    IL_0000:  ldarga.s   p
    IL_0002:  call       instance !1 valuetype [mscorlib]System.Collections.Generic.KeyValuePair`2<string,string>::get_Value()
    IL_0007:  stloc.0
    IL_0008:  br.s       IL_000a

    IL_000a:  ldloc.0
    IL_000b:  ret
  } // end of method InstallUpgrade096::'<Run>b__16'

  .method private hidebysig specialname rtspecialname static 
          void  .cctor() cil managed
  {
    // Code size       132 (0x84)
    .maxstack  4
    .locals init (class [System.Web.Extensions]System.Web.Script.Serialization.JavaScriptSerializer V_0,
             string[] V_1)
    IL_0000:  newobj     instance void [System.Web.Extensions]System.Web.Script.Serialization.JavaScriptSerializer::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldc.i4     0x7fffffff
    IL_000c:  callvirt   instance void [System.Web.Extensions]System.Web.Script.Serialization.JavaScriptSerializer::set_MaxJsonLength(int32)
    IL_0011:  nop
    IL_0012:  ldloc.0
    IL_0013:  stsfld     class [System.Web.Extensions]System.Web.Script.Serialization.JavaScriptSerializer InstallUpgrade096::Json
    IL_0018:  ldstr      "LOCALAPPDATA"
    IL_001d:  call       string [mscorlib]System.Environment::GetEnvironmentVariable(string)
    IL_0022:  ldstr      "TVXD"
    IL_0027:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_002c:  call       string [mscorlib]System.IO.Path::GetFullPath(string)
    IL_0031:  stsfld     string InstallUpgrade096::Base
    IL_0036:  ldc.i4.3
    IL_0037:  newarr     [mscorlib]System.String
    IL_003c:  stloc.1
    IL_003d:  ldloc.1
    IL_003e:  ldc.i4.0
    IL_003f:  ldstr      "SDNL"
    IL_0044:  ldstr      "SDNL_IMPORT.exe"
    IL_0049:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_004e:  stelem.ref
    IL_004f:  ldloc.1
    IL_0050:  ldc.i4.1
    IL_0051:  ldstr      "SDNL"
    IL_0056:  ldstr      "SDNL_IMPORT.exe.config"
    IL_005b:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_0060:  stelem.ref
    IL_0061:  ldloc.1
    IL_0062:  ldc.i4.2
    IL_0063:  ldstr      "SDNL"
    IL_0068:  ldstr      "MepsSdnl066.lsp"
    IL_006d:  call       string [mscorlib]System.IO.Path::Combine(string,
                                                                  string)
    IL_0072:  stelem.ref
    IL_0073:  ldloc.1
    IL_0074:  call       class [mscorlib]System.StringComparer [mscorlib]System.StringComparer::get_OrdinalIgnoreCase()
    IL_0079:  newobj     instance void class [System.Core]System.Collections.Generic.HashSet`1<string>::.ctor(class [mscorlib]System.Collections.Generic.IEnumerable`1<!0>,
                                                                                                              class [mscorlib]System.Collections.Generic.IEqualityComparer`1<!0>)
    IL_007e:  stsfld     class [System.Core]System.Collections.Generic.HashSet`1<string> InstallUpgrade096::ManagedSdnl
    IL_0083:  ret
  } // end of method InstallUpgrade096::.cctor

} // end of class InstallUpgrade096

.class private auto ansi sealed beforefieldinit '<>f__AnonymousType0`5'<'<status>j__TPar','<destination>j__TPar','<previous_installations>j__TPar','<preserved_files>j__TPar',
                                                                        '<rollback_archive>j__TPar'>
       extends [mscorlib]System.Object
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 ) 
  .field private initonly !'<status>j__TPar' '<status>i__Field'
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 ) 
  .field private initonly !'<destination>j__TPar' '<destination>i__Field'
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 ) 
  .field private initonly !'<previous_installations>j__TPar' '<previous_installations>i__Field'
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 ) 
  .field private initonly !'<preserved_files>j__TPar' '<preserved_files>i__Field'
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 ) 
  .field private initonly !'<rollback_archive>j__TPar' '<rollback_archive>i__Field'
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 ) 
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(!'<status>j__TPar' status,
                               !'<destination>j__TPar' destination,
                               !'<previous_installations>j__TPar' previous_installations,
                               !'<preserved_files>j__TPar' preserved_files,
                               !'<rollback_archive>j__TPar' rollback_archive) cil managed
  {
    .custom instance void [mscorlib]System.Diagnostics.DebuggerHiddenAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       44 (0x2c)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      !0 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<status>i__Field'
    IL_000d:  ldarg.0
    IL_000e:  ldarg.2
    IL_000f:  stfld      !1 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<destination>i__Field'
    IL_0014:  ldarg.0
    IL_0015:  ldarg.3
    IL_0016:  stfld      !2 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<previous_installations>i__Field'
    IL_001b:  ldarg.0
    IL_001c:  ldarg.s    preserved_files
    IL_001e:  stfld      !3 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<preserved_files>i__Field'
    IL_0023:  ldarg.0
    IL_0024:  ldarg.s    rollback_archive
    IL_0026:  stfld      !4 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<rollback_archive>i__Field'
    IL_002b:  ret
  } // end of method '<>f__AnonymousType0`5'::.ctor

  .method public hidebysig specialname instance !'<status>j__TPar' 
          get_status() cil managed
  {
    // Code size       11 (0xb)
    .maxstack  1
    .locals init (!'<status>j__TPar' V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      !0 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<status>i__Field'
    IL_0006:  stloc.0
    IL_0007:  br.s       IL_0009

    IL_0009:  ldloc.0
    IL_000a:  ret
  } // end of method '<>f__AnonymousType0`5'::get_status

  .method public hidebysig specialname instance !'<destination>j__TPar' 
          get_destination() cil managed
  {
    // Code size       11 (0xb)
    .maxstack  1
    .locals init (!'<destination>j__TPar' V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      !1 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<destination>i__Field'
    IL_0006:  stloc.0
    IL_0007:  br.s       IL_0009

    IL_0009:  ldloc.0
    IL_000a:  ret
  } // end of method '<>f__AnonymousType0`5'::get_destination

  .method public hidebysig specialname instance !'<previous_installations>j__TPar' 
          get_previous_installations() cil managed
  {
    // Code size       11 (0xb)
    .maxstack  1
    .locals init (!'<previous_installations>j__TPar' V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      !2 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<previous_installations>i__Field'
    IL_0006:  stloc.0
    IL_0007:  br.s       IL_0009

    IL_0009:  ldloc.0
    IL_000a:  ret
  } // end of method '<>f__AnonymousType0`5'::get_previous_installations

  .method public hidebysig specialname instance !'<preserved_files>j__TPar' 
          get_preserved_files() cil managed
  {
    // Code size       11 (0xb)
    .maxstack  1
    .locals init (!'<preserved_files>j__TPar' V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      !3 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<preserved_files>i__Field'
    IL_0006:  stloc.0
    IL_0007:  br.s       IL_0009

    IL_0009:  ldloc.0
    IL_000a:  ret
  } // end of method '<>f__AnonymousType0`5'::get_preserved_files

  .method public hidebysig specialname instance !'<rollback_archive>j__TPar' 
          get_rollback_archive() cil managed
  {
    // Code size       11 (0xb)
    .maxstack  1
    .locals init (!'<rollback_archive>j__TPar' V_0)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      !4 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<rollback_archive>i__Field'
    IL_0006:  stloc.0
    IL_0007:  br.s       IL_0009

    IL_0009:  ldloc.0
    IL_000a:  ret
  } // end of method '<>f__AnonymousType0`5'::get_rollback_archive

  .method public hidebysig virtual instance string 
          ToString() cil managed
  {
    .custom instance void [mscorlib]System.Diagnostics.DebuggerHiddenAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       179 (0xb3)
    .maxstack  2
    .locals init (class [mscorlib]System.Text.StringBuilder V_0,
             string V_1)
    IL_0000:  newobj     instance void [mscorlib]System.Text.StringBuilder::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldloc.0
    IL_0007:  ldstr      "{ status = "
    IL_000c:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_0011:  pop
    IL_0012:  ldloc.0
    IL_0013:  ldarg.0
    IL_0014:  ldfld      !0 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<status>i__Field'
    IL_0019:  box        !'<status>j__TPar'
    IL_001e:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(object)
    IL_0023:  pop
    IL_0024:  ldloc.0
    IL_0025:  ldstr      ", destination = "
    IL_002a:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_002f:  pop
    IL_0030:  ldloc.0
    IL_0031:  ldarg.0
    IL_0032:  ldfld      !1 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<destination>i__Field'
    IL_0037:  box        !'<destination>j__TPar'
    IL_003c:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(object)
    IL_0041:  pop
    IL_0042:  ldloc.0
    IL_0043:  ldstr      ", previous_installations = "
    IL_0048:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_004d:  pop
    IL_004e:  ldloc.0
    IL_004f:  ldarg.0
    IL_0050:  ldfld      !2 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<previous_installations>i__Field'
    IL_0055:  box        !'<previous_installations>j__TPar'
    IL_005a:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(object)
    IL_005f:  pop
    IL_0060:  ldloc.0
    IL_0061:  ldstr      ", preserved_files = "
    IL_0066:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_006b:  pop
    IL_006c:  ldloc.0
    IL_006d:  ldarg.0
    IL_006e:  ldfld      !3 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<preserved_files>i__Field'
    IL_0073:  box        !'<preserved_files>j__TPar'
    IL_0078:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(object)
    IL_007d:  pop
    IL_007e:  ldloc.0
    IL_007f:  ldstr      ", rollback_archive = "
    IL_0084:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_0089:  pop
    IL_008a:  ldloc.0
    IL_008b:  ldarg.0
    IL_008c:  ldfld      !4 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<rollback_archive>i__Field'
    IL_0091:  box        !'<rollback_archive>j__TPar'
    IL_0096:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(object)
    IL_009b:  pop
    IL_009c:  ldloc.0
    IL_009d:  ldstr      " }"
    IL_00a2:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_00a7:  pop
    IL_00a8:  ldloc.0
    IL_00a9:  callvirt   instance string [mscorlib]System.Object::ToString()
    IL_00ae:  stloc.1
    IL_00af:  br.s       IL_00b1

    IL_00b1:  ldloc.1
    IL_00b2:  ret
  } // end of method '<>f__AnonymousType0`5'::ToString

  .method public hidebysig virtual instance bool 
          Equals(object 'value') cil managed
  {
    .custom instance void [mscorlib]System.Diagnostics.DebuggerHiddenAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       137 (0x89)
    .maxstack  3
    .locals init (class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'> V_0,
             bool V_1)
    IL_0000:  ldarg.1
    IL_0001:  isinst     class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>
    IL_0006:  stloc.0
    IL_0007:  ldloc.0
    IL_0008:  brfalse.s  IL_0082

    IL_000a:  call       class [mscorlib]System.Collections.Generic.EqualityComparer`1<!0> class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<status>j__TPar'>::get_Default()
    IL_000f:  ldarg.0
    IL_0010:  ldfld      !0 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<status>i__Field'
    IL_0015:  ldloc.0
    IL_0016:  ldfld      !0 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<status>i__Field'
    IL_001b:  callvirt   instance bool class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<status>j__TPar'>::Equals(!0,
                                                                                                                                 !0)
    IL_0020:  brfalse.s  IL_0082

    IL_0022:  call       class [mscorlib]System.Collections.Generic.EqualityComparer`1<!0> class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<destination>j__TPar'>::get_Default()
    IL_0027:  ldarg.0
    IL_0028:  ldfld      !1 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<destination>i__Field'
    IL_002d:  ldloc.0
    IL_002e:  ldfld      !1 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<destination>i__Field'
    IL_0033:  callvirt   instance bool class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<destination>j__TPar'>::Equals(!0,
                                                                                                                                      !0)
    IL_0038:  brfalse.s  IL_0082

    IL_003a:  call       class [mscorlib]System.Collections.Generic.EqualityComparer`1<!0> class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<previous_installations>j__TPar'>::get_Default()
    IL_003f:  ldarg.0
    IL_0040:  ldfld      !2 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<previous_installations>i__Field'
    IL_0045:  ldloc.0
    IL_0046:  ldfld      !2 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<previous_installations>i__Field'
    IL_004b:  callvirt   instance bool class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<previous_installations>j__TPar'>::Equals(!0,
                                                                                                                                                 !0)
    IL_0050:  brfalse.s  IL_0082

    IL_0052:  call       class [mscorlib]System.Collections.Generic.EqualityComparer`1<!0> class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<preserved_files>j__TPar'>::get_Default()
    IL_0057:  ldarg.0
    IL_0058:  ldfld      !3 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<preserved_files>i__Field'
    IL_005d:  ldloc.0
    IL_005e:  ldfld      !3 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<preserved_files>i__Field'
    IL_0063:  callvirt   instance bool class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<preserved_files>j__TPar'>::Equals(!0,
                                                                                                                                          !0)
    IL_0068:  brfalse.s  IL_0082

    IL_006a:  call       class [mscorlib]System.Collections.Generic.EqualityComparer`1<!0> class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<rollback_archive>j__TPar'>::get_Default()
    IL_006f:  ldarg.0
    IL_0070:  ldfld      !4 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<rollback_archive>i__Field'
    IL_0075:  ldloc.0
    IL_0076:  ldfld      !4 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<rollback_archive>i__Field'
    IL_007b:  callvirt   instance bool class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<rollback_archive>j__TPar'>::Equals(!0,
                                                                                                                                           !0)
    IL_0080:  br.s       IL_0083

    IL_0082:  ldc.i4.0
    IL_0083:  nop
    IL_0084:  stloc.1
    IL_0085:  br.s       IL_0087

    IL_0087:  ldloc.1
    IL_0088:  ret
  } // end of method '<>f__AnonymousType0`5'::Equals

  .method public hidebysig virtual instance int32 
          GetHashCode() cil managed
  {
    .custom instance void [mscorlib]System.Diagnostics.DebuggerHiddenAttribute::.ctor() = ( 01 00 00 00 ) 
    // Code size       137 (0x89)
    .maxstack  3
    .locals init (int32 V_0,
             int32 V_1)
    IL_0000:  ldc.i4     0x768342b8
    IL_0005:  stloc.0
    IL_0006:  ldc.i4     0xa5555529
    IL_000b:  ldloc.0
    IL_000c:  mul
    IL_000d:  call       class [mscorlib]System.Collections.Generic.EqualityComparer`1<!0> class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<status>j__TPar'>::get_Default()
    IL_0012:  ldarg.0
    IL_0013:  ldfld      !0 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<status>i__Field'
    IL_0018:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<status>j__TPar'>::GetHashCode(!0)
    IL_001d:  add
    IL_001e:  stloc.0
    IL_001f:  ldc.i4     0xa5555529
    IL_0024:  ldloc.0
    IL_0025:  mul
    IL_0026:  call       class [mscorlib]System.Collections.Generic.EqualityComparer`1<!0> class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<destination>j__TPar'>::get_Default()
    IL_002b:  ldarg.0
    IL_002c:  ldfld      !1 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<destination>i__Field'
    IL_0031:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<destination>j__TPar'>::GetHashCode(!0)
    IL_0036:  add
    IL_0037:  stloc.0
    IL_0038:  ldc.i4     0xa5555529
    IL_003d:  ldloc.0
    IL_003e:  mul
    IL_003f:  call       class [mscorlib]System.Collections.Generic.EqualityComparer`1<!0> class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<previous_installations>j__TPar'>::get_Default()
    IL_0044:  ldarg.0
    IL_0045:  ldfld      !2 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<previous_installations>i__Field'
    IL_004a:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<previous_installations>j__TPar'>::GetHashCode(!0)
    IL_004f:  add
    IL_0050:  stloc.0
    IL_0051:  ldc.i4     0xa5555529
    IL_0056:  ldloc.0
    IL_0057:  mul
    IL_0058:  call       class [mscorlib]System.Collections.Generic.EqualityComparer`1<!0> class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<preserved_files>j__TPar'>::get_Default()
    IL_005d:  ldarg.0
    IL_005e:  ldfld      !3 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<preserved_files>i__Field'
    IL_0063:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<preserved_files>j__TPar'>::GetHashCode(!0)
    IL_0068:  add
    IL_0069:  stloc.0
    IL_006a:  ldc.i4     0xa5555529
    IL_006f:  ldloc.0
    IL_0070:  mul
    IL_0071:  call       class [mscorlib]System.Collections.Generic.EqualityComparer`1<!0> class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<rollback_archive>j__TPar'>::get_Default()
    IL_0076:  ldarg.0
    IL_0077:  ldfld      !4 class '<>f__AnonymousType0`5'<!'<status>j__TPar',!'<destination>j__TPar',!'<previous_installations>j__TPar',!'<preserved_files>j__TPar',!'<rollback_archive>j__TPar'>::'<rollback_archive>i__Field'
    IL_007c:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.EqualityComparer`1<!'<rollback_archive>j__TPar'>::GetHashCode(!0)
    IL_0081:  add
    IL_0082:  stloc.0
    IL_0083:  ldloc.0
    IL_0084:  stloc.1
    IL_0085:  br.s       IL_0087

    IL_0087:  ldloc.1
    IL_0088:  ret
  } // end of method '<>f__AnonymousType0`5'::GetHashCode

  .property instance !'<status>j__TPar' status()
  {
    .get instance !'<status>j__TPar' '<>f__AnonymousType0`5'::get_status()
  } // end of property '<>f__AnonymousType0`5'::status
  .property instance !'<destination>j__TPar'
          destination()
  {
    .get instance !'<destination>j__TPar' '<>f__AnonymousType0`5'::get_destination()
  } // end of property '<>f__AnonymousType0`5'::destination
  .property instance !'<previous_installations>j__TPar'
          previous_installations()
  {
    .get instance !'<previous_installations>j__TPar' '<>f__AnonymousType0`5'::get_previous_installations()
  } // end of property '<>f__AnonymousType0`5'::previous_installations
  .property instance !'<preserved_files>j__TPar'
          preserved_files()
  {
    .get instance !'<preserved_files>j__TPar' '<>f__AnonymousType0`5'::get_preserved_files()
  } // end of property '<>f__AnonymousType0`5'::preserved_files
  .property instance !'<rollback_archive>j__TPar'
          rollback_archive()
  {
    .get instance !'<rollback_archive>j__TPar' '<>f__AnonymousType0`5'::get_rollback_archive()
  } // end of property '<>f__AnonymousType0`5'::rollback_archive
} // end of class '<>f__AnonymousType0`5'


// =============================================================

// *********** DISASSEMBLY COMPLETE ***********************
// WARNING: Created Win32 resource file C:\Users\ADMIN\.gemini\antigravity\scratch\cai_dat.res
