using System;using System.Linq;using System.Collections.Generic;using System.Globalization;using System.Text.RegularExpressions;
#if !(STANDARDS_PURE||IDENTITY_PURE||CABLES016_PURE)
using Autodesk.AutoCAD.DatabaseServices;
#endif
namespace Tvxd.Meps {
 // TVXD-MEPS-CABLES-016: the cable choices of the wire selector (user catalog, 2026-10-09; 19 → 26 by CABLES-024, 2026-10-10). Pure part (no CAD types; compiles with Standards012
 // /define:STANDARDS_PURE, alone or with Identity015 /define:IDENTITY_PURE; /define:CABLES016_PURE also excludes the CAD part):
 //  - FROZEN list, in display order: PVC 1.5/2.5/4/6/10/16; XLPE 2×2.5/4/6/10/16; XLPE 4×2.5/4/6/10/16/25/35/50/70/95/120/150/185/240/300 (section in mm²).
 //  - explicit insulation PVC/XLPE, section, core count: XLPE 2 or 4; PVC 0 = UNSPECIFIED (not inferred as single-core, nothing derived from it).
 //  - stable code TVXD_WIRE_<INS>_[<cores>X]<section, '.'→'P'> and layer E-DAY-<INS>-[<cores>X]<section>, e.g. TVXD_WIRE_PVC_1P5 / E-DAY-XLPE-2X2P5.
 //  - layers: drafting aliases of E-DAY-CS (ACI 2 yellow, Continuous, LW 0.25 mm, plotted), provenance USER_CABLE_CATALOG_016. They are NOT a cable
 //    specification from T_CHUAN; Standards012.Layers checks they still equal E-DAY-CS.
 // No voltage, conductor material, ampacity, conduit fill, number of runs, length or quantity is stored or guessed. These are TYPE attributes of a drawn
 // route; a future takeoff must read the actual geometry (no persistent length) and must not multiply conductors by the core count.
 public sealed class CableSpec016 {
  public readonly string code,layer,label,insulation,group;public readonly double section;public readonly int cores;
  internal CableSpec016(string insulation,int cores,double section){this.insulation=insulation;this.cores=cores;this.section=section;
   string s=Cable016.SectionText(section),token=(cores>0?cores.ToString(CultureInfo.InvariantCulture)+"X":"")+s.Replace('.','P');
   code="TVXD_WIRE_"+insulation+"_"+token;layer="E-DAY-"+insulation+"-"+token;label=insulation+" "+(cores>0?cores.ToString(CultureInfo.InvariantCulture)+"×":"")+s+" mm²";
   group=cores>0?insulation+" "+cores.ToString(CultureInfo.InvariantCulture)+" lõi":insulation;}
  public bool CoresKnown{get{return cores>0;}}
  public string CoresText{get{return cores>0?cores.ToString(CultureInfo.InvariantCulture)+" lõi":"chưa xác định (không suy ra 1 lõi)";}}
 }
 public static class Cable016 {
  public const string Schema="TVXD_MEPS_CABLE",Version="016.1",Key="TVXD_MEPS_CABLE_016",Origin="USER_CABLE_016",Provenance="USER_CABLE_CATALOG_016",AliasOf="E-DAY-CS",Unit="mm2";
  public const string Pvc="PVC",Xlpe="XLPE",Linetype="Continuous";
  public const int Count=26,Aci=2,SourceAci=2,Weight=25,Fields=7;public const bool Plot=true;
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  static readonly Regex CodeRx=new Regex("^TVXD_WIRE_(PVC|XLPE)_[0-9A-Z]+$"),LayerRx=new Regex("^E-DAY-(PVC|XLPE)-[0-9A-Z]+$");
  public static string SectionText(double s){return s.ToString("0.##",Inv);}
  static CableSpec016 C(string insulation,int cores,double section){return new CableSpec016(insulation,cores,section);}
  static readonly CableSpec016[] Book=Validate(new[]{
   C(Pvc,0,1.5),C(Pvc,0,2.5),C(Pvc,0,4),C(Pvc,0,6),C(Pvc,0,10),C(Pvc,0,16),
   C(Xlpe,2,2.5),C(Xlpe,2,4),C(Xlpe,2,6),C(Xlpe,2,10),C(Xlpe,2,16),
   C(Xlpe,4,2.5),C(Xlpe,4,4),C(Xlpe,4,6),C(Xlpe,4,10),C(Xlpe,4,16),C(Xlpe,4,25),C(Xlpe,4,35),C(Xlpe,4,50),
   // TVXD-MEPS-CABLES-024 (user request 2026-10-10): appended after 4×50, ascending; the 19 rows above, their codes/layers and the record schema are unchanged.
   C(Xlpe,4,70),C(Xlpe,4,95),C(Xlpe,4,120),C(Xlpe,4,150),C(Xlpe,4,185),C(Xlpe,4,240),C(Xlpe,4,300)});
  // Checked once: Count rows, PVC cores 0 only, XLPE 2/4 only, positive finite sections, unique codes and layers, code/layer pattern.
  static CableSpec016[] Validate(CableSpec016[] b){if(b.Length!=Count)throw new InvalidOperationException("Danh mục cáp 016 phải có "+Count+" mục.");
   foreach(var c in b){bool ok=(c.insulation==Pvc&&c.cores==0)||(c.insulation==Xlpe&&(c.cores==2||c.cores==4));
    if(!ok||double.IsNaN(c.section)||double.IsInfinity(c.section)||c.section<=0||!CodeRx.IsMatch(c.code)||!LayerRx.IsMatch(c.layer)||c.code.Length>255||c.layer.Length>255)throw new InvalidOperationException("Danh mục cáp 016: mục không hợp lệ "+c.code+".");}
   if(b.Select(c=>c.code).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=b.Length||b.Select(c=>c.layer).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=b.Length)throw new InvalidOperationException("Danh mục cáp 016 có mã/layer trùng.");
   return b;}
  public static IList<CableSpec016> Entries{get{return Array.AsReadOnly(Book);}}
  // Exact layer (case-insensitive like CAD layer names) → entry; null when the layer is not one of the 19 (legacy presets, samples, other layers).
  public static CableSpec016 Find(string layer){return layer==null?null:Book.FirstOrDefault(c=>string.Equals(c.layer,layer,StringComparison.OrdinalIgnoreCase));}
  public static CableSpec016 ByCode(string code){return code==null?null:Book.FirstOrDefault(c=>string.Equals(c.code,code,StringComparison.Ordinal));}
  public static bool IsCable(string layer){return Find(layer)!=null;}
  // The 19 profile layers (drafting aliases of E-DAY-CS; Standards012.Layers adds them under CABLES016 and checks them against E-DAY-CS).
  public static List<StdLayer012> Layers(){return Book.Select(c=>new StdLayer012{name=c.layer,linetype=Linetype,
    description=(c.insulation==Pvc?"Day ":"Cap ")+c.insulation+" "+(c.cores>0?c.cores.ToString(Inv)+"x":"")+SectionText(c.section)+" mm2 (MEPS 016, net nhu "+AliasOf+")",
    sourceAci=SourceAci,aci=Aci,weight=Weight,plot=Plot,aliasOf=AliasOf,
    provenance=Provenance+": danh mục cáp người dùng chọn (TVXD-MEPS-CABLES-016); thuộc tính nét là alias của "+AliasOf+" (ACI "+Aci+", "+Linetype+", LW "+Weight+"); không phải quy cách cáp lấy từ T_CHUAN"}).ToList();}
  // The 19 selector choices, display order; kind = group heading (PVC / XLPE 2 lõi / XLPE 4 lõi), name = exact label.
  public static List<WirePreset012> Presets(){return Book.Select(c=>new WirePreset012{layer=c.layer,linetype=Linetype,kind=c.group,name=c.label,aci=Aci,weight=Weight,metric=false}).ToList();}
  // ---- stored layout: [Schema, Version, code, insulation, cores, section (invariant, mm²), unit], text only ----
  public static string[] Encode(CableSpec016 c){return new[]{Schema,Version,c.code,c.insulation,c.cores.ToString(Inv),SectionText(c.section),Unit};}
  // Accepts only an exact encoding of a catalog entry; anything else (other schema/version, unknown code, changed insulation/cores/section) is refused.
  public static CableSpec016 Decode(string[] v){if(v==null||v.Length!=Fields||v.Any(x=>x==null)||v[0]!=Schema||v[1]!=Version)throw new InvalidOperationException("Dữ liệu cáp MEPS 016 không đúng schema "+Schema+" "+Version+" (bị sửa hoặc phiên bản khác); MEPS không ghi đè.");
   var c=ByCode(v[2]);if(c==null)throw new InvalidOperationException("Dữ liệu cáp MEPS 016 có mã lạ "+v[2]+"; MEPS không ghi đè.");
   if(!Encode(c).SequenceEqual(v,StringComparer.Ordinal))throw new InvalidOperationException("Dữ liệu cáp "+c.code+" khác danh mục 016 (cách điện/số lõi/tiết diện); MEPS không ghi đè.");return c;}
 }
#if !(STANDARDS_PURE||IDENTITY_PURE||CABLES016_PURE)
 // AutoCAD side: one Xrecord under Cable016.Key in the polyline's extension dictionary. Written only through the caller's transaction (DrawPreset), so
 // an abort or UNDO removes it with the route. A malformed/different owned record is refused, never overwritten; foreign keys are never touched.
 // A copied polyline keeps its record (type attribute, not an occurrence ID).
 public static class CableCad016 {
  static string[] Values(Xrecord x){using(var d=x.Data){return d==null?new string[0]:d.AsArray().Select(v=>v.TypeCode==(short)DxfCode.Text?Convert.ToString(v.Value,CultureInfo.InvariantCulture):null).ToArray();}}
  // Cable spec of an object; null when it has none; malformed → exception.
  public static CableSpec016 Read(Transaction tr,DBObject o){if(o.ExtensionDictionary.IsNull)return null;var d=(DBDictionary)tr.GetObject(o.ExtensionDictionary,OpenMode.ForRead);if(!d.Contains(Cable016.Key))return null;
   var x=tr.GetObject(d.GetAt(Cable016.Key),OpenMode.ForRead) as Xrecord;if(x==null)throw new InvalidOperationException("Khoá "+Cable016.Key+" của đối tượng "+o.Handle+" không phải Xrecord; MEPS không ghi đè.");return Cable016.Decode(Values(x));}
  // Writes c once; the same spec already present is accepted, a different/malformed one is refused (exception, caller's transaction aborts).
  public static void Mark(Transaction tr,DBObject o,CableSpec016 c){if(c==null||Cable016.ByCode(c.code)!=c)throw new InvalidOperationException("Cáp không thuộc danh mục 016; không ghi.");
   var had=Read(tr,o);if(had!=null){if(had==c)return;throw new InvalidOperationException("Đối tượng "+o.Handle+" đã có cáp "+had.code+" khác "+c.code+"; MEPS không ghi đè.");}
   if(!o.IsWriteEnabled)o.UpgradeOpen();if(o.ExtensionDictionary.IsNull)o.CreateExtensionDictionary();var d=(DBDictionary)tr.GetObject(o.ExtensionDictionary,OpenMode.ForWrite);
   var x=new Xrecord();using(var b=new ResultBuffer(Cable016.Encode(c).Select(s=>new TypedValue((int)DxfCode.Text,s)).ToArray()))x.Data=b;d.SetAt(Cable016.Key,x);tr.AddNewlyCreatedDBObject(x,true);}
  // DrawPreset: a route on one of the Cable016.Count cable layers gets its spec (after its identity record); the six legacy presets get none.
  public static CableSpec016 MarkPreset(Transaction tr,Polyline pl,string layer){var c=Cable016.Find(layer);if(c==null)return null;
   if(!string.Equals(pl.Layer,c.layer,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Tuyến cáp "+c.code+" không nằm trên layer "+c.layer+"; huỷ vẽ.");
#if IDENTITY015
   var id=IdentityCad015.Read(tr,pl);if(id==null||!string.Equals(id.code,c.code,StringComparison.Ordinal))throw new InvalidOperationException("Tuyến cáp "+c.code+" chưa có mã nhận dạng tương ứng; huỷ vẽ.");
#endif
   Mark(tr,pl,c);return c;}
 }
#endif
}
