using System;using System.Linq;using System.Collections.Generic;using System.Globalization;using System.Text;using System.Text.RegularExpressions;
#if !IDENTITY_PURE
using Autodesk.AutoCAD.DatabaseServices;
#endif
namespace Tvxd.Meps {
 // TVXD-MEPS-IDENTITY-015: readable, stable type codes for clean new drawings, and the source-backed nine-switch legend.
 // Pure part (no CAD types; compiled alone with /define:IDENTITY_PURE together with Core, Library, Legend, Meps06 and Standards012 /define:STANDARDS_PURE):
 //  - a FROZEN codebook: entry ID → code for the 67 entries of SOURCE_CODEBOOK.json (current catalog, one source hash). A code is TVXD_ + the source block
 //    name upper-cased, '-' and any other character outside A–Z/0–9 collapsed to one '_'. The nine switches are coded explicitly: E-CT-1..6 →
 //    TVXD_E_CT_1C_01..06 (one-way), E-CT-2C-1..3 → TVXD_E_CT_2C_01..03 (two-way). Codes never depend on hide/show, insertion counts or drawings.
 //  - an entry imported later (outside the book): TVXD_<normalised name>_<12-hex entry ID upper-case>; no category, type or specification is inferred.
 //  - wire preset codes TVXD_WIRE_<preset layer> for the six Standards012.PresetMap layers; a captured sample is TVXD_SAMPLE_<sample GUID>, recorded as
 //    UNCLASSIFIED (a drawing style snapshot, not a cable/conduit specification).
 //  - Preflight: the production legend check with the nine switches always present (reference-only, count 0) without touching the actual counts.
 // The codebook names a type; it is not a quantity. MEPS 015 has no takeoff/BOQ engine.
 public sealed class CodeEntry015 {public string id,name,category,code;}
 // One identity record, used both in the drawing registry and on an entity. origin: FROZEN_015 (codebook), IMPORTED (catalog entry outside the book),
 // PRESET_012 (profile wire preset), SAMPLE_UNCLASSIFIED (captured sample). Empty strings where a value does not exist; nothing is guessed.
 public sealed class IdentityRecord015 {public string kind,code,sourceId="",alias="",sourceSha256="",category="",origin;
  public bool SameAs(IdentityRecord015 o){return o!=null&&Identity015.Encode(this).SequenceEqual(Identity015.Encode(o),StringComparer.Ordinal);}}
 public static class Identity015 {
  public const string Schema="TVXD_MEPS_IDENTITY",Version="015.1",Key="TVXD_MEPS_IDENTITY_015",Prefix="TVXD_";
  public const string KindBlock="BLOCK",KindPreset="WIRE_PRESET",KindSample="WIRE_SAMPLE";
  public const string OriginFrozen="FROZEN_015",OriginImported="IMPORTED",OriginPreset="PRESET_012",OriginSample="SAMPLE_UNCLASSIFIED";
#if CABLES016
  // TVXD-MEPS-CABLES-016: wire preset of one of the 19 user cables (Cable016). Code = Cable016 code; no T_CHUAN source hash (not an LSP classification).
  public const string OriginCable=Cable016.Origin;
#endif
  // Preflight item statuses added by the 015 wrapper.
  public const string ReferenceOnly="REFERENCE_ONLY",SwitchMissing="SWITCH_MISSING";
  // Source hash of every codebook entry (SOURCE_CODEBOOK.json; LibEntry.sourceSha256).
  public const string CodebookSha256="c4356be550c9436356489fb7fb49d575d53682f6e7e203a3fdd40d90b7c9ac6a";
  // Longest stored text value (identity Xrecords and registry keys stay far below AutoCAD's symbol/string limits).
  public const int MaxText=255;
  static readonly Regex Id12=new Regex("^[0-9a-f]{12}$"),CodeRx=new Regex("^TVXD_[A-Z0-9]+(_[A-Z0-9]+)*$"),Hex12Tail=new Regex("_[0-9A-F]{12}$");
  static bool Same(string a,string b){return string.Equals(a,b,StringComparison.OrdinalIgnoreCase);}
  static CodeEntry015 F(string id,string name,string category,string code){return new CodeEntry015{id=id,name=name,category=category,code=code};}
  // Frozen from SOURCE_CODEBOOK.json (TVXD-MEPS-IDENTITY-015), in its order. Never edited to follow a drawing or the library state.
  static readonly CodeEntry015[] Book={
   F("737f0ba88589","E-TU-AT","E-TU","TVXD_E_TU_AT"),F("7f58826bac98","E-TU-NOI","E-TU","TVXD_E_TU_NOI"),F("39e573d0b486","E-TU-MODULE","E-TU","TVXD_E_TU_MODULE"),
   F("0ae8dc444979","E-SDNL-CC","E-SDNL","TVXD_E_SDNL_CC"),F("db598c8bf87a","E-SDNL-DONGHO","E-SDNL","TVXD_E_SDNL_DONGHO"),F("f389b6a91800","E-SDNL-CTCM","E-SDNL","TVXD_E_SDNL_CTCM"),
   F("5fed70405bfc","E-SDNL-DENBAO","E-SDNL","TVXD_E_SDNL_DENBAO"),
   F("af06e101199b","E-CT-1","E-CT","TVXD_E_CT_1C_01"),F("7c3b3f10bf73","E-CT-2","E-CT","TVXD_E_CT_1C_02"),F("6eb1c9a43e1a","E-CT-3","E-CT","TVXD_E_CT_1C_03"),
   F("f86dcd0aacba","E-CT-4","E-CT","TVXD_E_CT_1C_04"),F("0bfd9b2cf670","E-CT-5","E-CT","TVXD_E_CT_1C_05"),F("acdb9d18b907","E-CT-6","E-CT","TVXD_E_CT_1C_06"),
   F("288cc0186435","E-CT-2C-1","E-CT","TVXD_E_CT_2C_01"),F("649e3503d144","E-CT-2C-2","E-CT","TVXD_E_CT_2C_02"),F("fba189fcc8c4","E-CT-2C-3","E-CT","TVXD_E_CT_2C_03"),
   F("1c43dd59608b","E-AT-2P","E-AT","TVXD_E_AT_2P"),
   F("3a5a91f081ba","E-OC-1","E-OC","TVXD_E_OC_1"),F("edc622b6329c","E-OC-2","E-OC","TVXD_E_OC_2"),F("7ca5e4427909","E-OC-3","E-OC","TVXD_E_OC_3"),
   F("195831414e45","E-OC-SAN-1","E-OC","TVXD_E_OC_SAN_1"),F("b2a27ff98406","E-OC-SAN-2","E-OC","TVXD_E_OC_SAN_2"),
   F("e665d1f5fbf3","E-QUAT-TR","E-QUAT","TVXD_E_QUAT_TR"),F("be665e40d744","E-QUAT-HS-1","E-QUAT","TVXD_E_QUAT_HS_1"),F("ed0d4a1455df","E-QUAT-HS-2","E-QUAT","TVXD_E_QUAT_HS_2"),
   F("35c6cb156a40","E-QUAT-HS-3","E-QUAT","TVXD_E_QUAT_HS_3"),F("94ea5c5a70c0","E-QUAT-DAO-D400","E-QUAT","TVXD_E_QUAT_DAO_D400"),F("f178f698a536","E-QUAT-TUONG-D400","E-QUAT","TVXD_E_QUAT_TUONG_D400"),
   F("24c5e33efbe5","E-QUAT-TG-TUONG","E-QUAT","TVXD_E_QUAT_TG_TUONG"),F("ffdaeb3ea6d8","E-QUAT-TG-TRAN","E-QUAT","TVXD_E_QUAT_TG_TRAN"),
   F("cb710280cc0a","E-DEN-OP-D172","E-DEN","TVXD_E_DEN_OP_D172"),F("08ddeae3a682","E-DEN-OP-D225","E-DEN","TVXD_E_DEN_OP_D225"),F("df10a0f37e89","E-DEN-OP-D300","E-DEN","TVXD_E_DEN_OP_D300"),
   F("52b693eb0840","E-DEN-DL-9W","E-DEN","TVXD_E_DEN_DL_9W"),F("9e26be2331ae","E-DEN-DL-12W","E-DEN","TVXD_E_DEN_DL_12W"),F("49c932e17cf3","E-DEN-DL-16W","E-DEN","TVXD_E_DEN_DL_16W"),
   F("befb5d1624a6","E-DEN-DL-25W","E-DEN","TVXD_E_DEN_DL_25W"),F("35dcb3fc7031","E-DEN-DL-7W","E-DEN","TVXD_E_DEN_DL_7W"),
   F("90987e13011a","E-DEN-TQ-1X20","E-DEN","TVXD_E_DEN_TQ_1X20"),F("833ae702cb56","E-DEN-TQ-2X20","E-DEN","TVXD_E_DEN_TQ_2X20"),
   F("5874b30dd8b2","E-DEN-L-1X20","E-DEN","TVXD_E_DEN_L_1X20"),F("66c7c493f3e0","E-DEN-L-2X20","E-DEN","TVXD_E_DEN_L_2X20"),
   F("5f581896f84f","E-DEN-CA-2X20","E-DEN","TVXD_E_DEN_CA_2X20"),F("bd9514545251","E-DEN-BANG-1X18","E-DEN","TVXD_E_DEN_BANG_1X18"),F("d570abf99f93","E-DEN-M26","E-DEN","TVXD_E_DEN_M26"),
   F("d689033378e9","E-DEN-PN-600X600","E-DEN","TVXD_E_DEN_PN_600X600"),F("79788a26bdef","E-DEN-PN-600X1200","E-DEN","TVXD_E_DEN_PN_600X1200"),
   F("3a1d6eb463a6","E-DEN-PN-300X300","E-DEN","TVXD_E_DEN_PN_300X300"),F("5ef20f6a2b8a","E-DEN-PN-300X600","E-DEN","TVXD_E_DEN_PN_300X600"),
   F("9ca9fcb4f29b","E-DEN-PN-150X1200","E-DEN","TVXD_E_DEN_PN_150X1200"),F("c631328ab134","E-DEN-PN-300X1200","E-DEN","TVXD_E_DEN_PN_300X1200"),
   F("93a812d2db47","E-DEN-DUIXOAY","E-DEN","TVXD_E_DEN_DUIXOAY"),F("c993ec335896","E-DEN-PHA","E-DEN","TVXD_E_DEN_PHA"),F("da92af583090","E-DEN-CHUM","E-DEN","TVXD_E_DEN_CHUM"),
   F("06210c8c1829","E-DH-TUONG","E-DH","TVXD_E_DH_TUONG"),F("52c0dec1fc2e","E-DH-CASSETTE","E-DH","TVXD_E_DH_CASSETTE"),F("a8591483b0f6","E-DH-ONGGIO","E-DH","TVXD_E_DH_ONGGIO"),
   F("8a2f9061bd46","E-DH-NONG","E-DH","TVXD_E_DH_NONG"),
   F("60ce22c137eb","E-DAY-LEN","E-DAY","TVXD_E_DAY_LEN"),F("71862ae81ba2","E-DAY-XUYEN","E-DAY","TVXD_E_DAY_XUYEN"),F("1ec0423b815b","E-DAY-CHO","E-DAY","TVXD_E_DAY_CHO"),
   F("458d71810086","E-NHE-RJ45","E-NHE","TVXD_E_NHE_RJ45"),F("3bb4ec1dabb2","E-NHE-TEL","E-NHE","TVXD_E_NHE_TEL"),
   F("a2af7bfe4ad4","E-X-ONG-HOI","E-X","TVXD_E_X_ONG_HOI"),F("a1d34e9981d3","E-X-ONG-CAP","E-X","TVXD_E_X_ONG_CAP"),F("db54f5eacb2e","E-X-MAY-ONGGIO","E-X","TVXD_E_X_MAY_ONGGIO"),
   F("8f5b6421b13b","E-X-ONGGIO-CT","E-X","TVXD_E_X_ONGGIO_CT")};
  public const int BookCount=67;
  // The nine switches the legend always shows, in source order: legend.json row 10 (one-way E-CT-1..6) and row 11 (two-way E-CT-2C-1..3).
  public static readonly string[] OneWay={"af06e101199b","7c3b3f10bf73","6eb1c9a43e1a","f86dcd0aacba","0bfd9b2cf670","acdb9d18b907"},TwoWay={"288cc0186435","649e3503d144","fba189fcc8c4"};
  public static IEnumerable<string> Switches{get{return OneWay.Concat(TwoWay);}}
  static readonly Dictionary<string,CodeEntry015> byId=Build();
  // The book is checked once: 67 entries, 12-hex IDs, unique IDs/names/codes, each code = TVXD_ + Normalize(name) except the nine explicit switch codes,
  // no book code ends like an import code (_ + 12 hex), so later imports can never take a book code.
  static Dictionary<string,CodeEntry015> Build(){var d=new Dictionary<string,CodeEntry015>(StringComparer.Ordinal);
   if(Book.Length!=BookCount)throw new InvalidOperationException("Sổ mã 015 phải có "+BookCount+" mục.");
   for(int i=0;i<OneWay.Length;i++)if(Book.Single(x=>x.id==OneWay[i]).code!=Prefix+"E_CT_1C_0"+(i+1))throw new InvalidOperationException("Mã công tắc một chiều sai thứ tự.");
   for(int i=0;i<TwoWay.Length;i++)if(Book.Single(x=>x.id==TwoWay[i]).code!=Prefix+"E_CT_2C_0"+(i+1))throw new InvalidOperationException("Mã công tắc hai chiều sai thứ tự.");
   foreach(var f in Book){if(!Id12.IsMatch(f.id)||d.ContainsKey(f.id))throw new InvalidOperationException("Sổ mã 015: ID không hợp lệ hoặc trùng: "+f.id);
    bool sw=OneWay.Contains(f.id)||TwoWay.Contains(f.id);if(!sw&&f.code!=Prefix+Normalize(f.name))throw new InvalidOperationException("Sổ mã 015: mã "+f.code+" không theo quy tắc tên "+f.name);
    if(!ValidCode(f.code)||Hex12Tail.IsMatch(f.code))throw new InvalidOperationException("Sổ mã 015: mã không hợp lệ "+f.code);d[f.id]=f;}
   if(Book.Select(x=>x.code).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=Book.Length||Book.Select(x=>x.name).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=Book.Length)throw new InvalidOperationException("Sổ mã 015 có mã/tên trùng.");
   return d;}
  public static IList<CodeEntry015> Codebook{get{return Book.Select(f=>new CodeEntry015{id=f.id,name=f.name,category=f.category,code=f.code}).ToList();}}
  public static bool IsFrozen(string entryId){return entryId!=null&&byId.ContainsKey(entryId);}
  // QA aid: every book ID recomputed with Library.EntryId from the codebook hash and name (empty list = consistent).
  public static List<string> SelfCheck(){var issues=new List<string>();foreach(var f in Book){string id;try{id=Library.EntryId(CodebookSha256,f.name);}catch(Exception ex){issues.Add(f.name+": "+ex.Message);continue;}if(id!=f.id)issues.Add(f.name+": ID "+f.id+" ≠ "+id);}return issues;}
  // Upper case (invariant); every run of characters outside A–Z / 0–9 becomes one '_'; no leading/trailing '_'. Diacritics are not transliterated.
  public static string Normalize(string alias){var b=new StringBuilder();foreach(char ch in (alias??"").ToUpperInvariant()){bool ok=(ch>='A'&&ch<='Z')||(ch>='0'&&ch<='9');
    if(ok){b.Append(ch);continue;}if(b.Length>0&&b[b.Length-1]!='_')b.Append('_');}
   while(b.Length>0&&b[b.Length-1]=='_')b.Length--;return b.ToString();}
  public static bool ValidCode(string code){return code!=null&&code.Length<=MaxText&&CodeRx.IsMatch(code);}
  // Code of a library entry. A book ID must still carry the book name and source hash (else refused: the catalog no longer matches the frozen book).
  public static string BlockCode(LibEntry e){return BlockRecord(e).code;}
  public static IdentityRecord015 BlockRecord(LibEntry e){if(e==null||e.id==null||!Id12.IsMatch(e.id))throw new InvalidOperationException("Mục thư viện không có ID hợp lệ; không gán mã.");
   CodeEntry015 f;if(byId.TryGetValue(e.id,out f)){if(!Same(f.name,e.name)||!Same(e.sourceSha256,CodebookSha256))throw new InvalidOperationException("Mục "+e.id+" ("+e.name+") khác sổ mã 015 ("+f.name+", nguồn "+CodebookSha256.Substring(0,8)+"); MEPS không tự gán mã.");return FrozenRecord(f);}
   Library.ValidName(e.name);string a=Normalize(e.name);string code=Prefix+(a.Length>0?a+"_":"")+e.id.ToUpperInvariant();if(!ValidCode(code))throw new InvalidOperationException("Không tạo được mã hợp lệ cho "+e.name+".");
   return new IdentityRecord015{kind=KindBlock,code=code,sourceId=e.id,alias=e.name,sourceSha256=e.sourceSha256??"",category="",origin=OriginImported};}
  static IdentityRecord015 FrozenRecord(CodeEntry015 f){return new IdentityRecord015{kind=KindBlock,code=f.code,sourceId=f.id,alias=f.name,sourceSha256=CodebookSha256,category=f.category,origin=OriginFrozen};}
  // Six wire presets only (Standards012.PresetMap); anything else is refused. Under CABLES016 also the 19 Cable016 layers (their stable cable code).
  public static string WireCode(string layer){
#if CABLES016
   var cable=Cable016.Find(layer);if(cable!=null)return cable.code;
#endif
   var m=Standards012.PresetMap.FirstOrDefault(p=>Same(p[0],layer));if(m==null)throw new InvalidOperationException("Layer "+(layer??"(trống)")+" không phải nét chuẩn của bảng nét dây; không có mã.");return Prefix+"WIRE_"+Normalize(m[0]);}
  public static IdentityRecord015 PresetRecord(string layer){
#if CABLES016
   var cable=Cable016.Find(layer);if(cable!=null)return new IdentityRecord015{kind=KindPreset,code=cable.code,alias=cable.layer,origin=OriginCable};
#endif
   string code=WireCode(layer);var s=Standards012.Layer(layer);return new IdentityRecord015{kind=KindPreset,code=code,alias=s.name,sourceSha256=Standards012.SourceSha256,origin=OriginPreset};}
  // The six legacy presets; under CABLES016 followed by the 19 cables (registry 67 + 6 + 19 = 92 with the base catalog).
  public static IEnumerable<IdentityRecord015> PresetRecords(){var old=Standards012.PresetMap.Select(p=>PresetRecord(p[0]));
#if CABLES016
   return old.Concat(Cable016.Entries.Select(c=>PresetRecord(c.layer)));
#else
   return old;
#endif
  }
  // Captured sample: GUID (any accepted format) → 32 upper-case hex digits. UNCLASSIFIED: no cable, conduit, conductor count or diameter is implied.
  public static string SampleCode(string sampleId){Guid g;if(!Guid.TryParse(sampleId??"",out g))throw new InvalidOperationException("Mẫu nét không có ID hợp lệ; không có mã.");return Prefix+"SAMPLE_"+g.ToString("N").ToUpperInvariant();}
  public static IdentityRecord015 SampleRecord(string sampleId,string layer){string code=SampleCode(sampleId);return new IdentityRecord015{kind=KindSample,code=code,sourceId=code.Substring((Prefix+"SAMPLE_").Length).ToLowerInvariant(),alias=layer??"",origin=OriginSample};}
  // ---- stored layout: [Schema, Version, kind, code, sourceId, alias, sourceSha256, category, origin], text only ----
  public const int Fields=9;
  public static void Check(IdentityRecord015 r){if(r==null)throw new InvalidOperationException("Thiếu bản ghi mã.");
   bool ok=(r.kind==KindBlock&&(r.origin==OriginFrozen||r.origin==OriginImported))||(r.kind==KindPreset&&r.origin==OriginPreset)||(r.kind==KindSample&&r.origin==OriginSample);
#if CABLES016
   // Cable preset: only a Cable016 code with its own layer as alias and no source ID/hash/category.
   if(!ok&&r.kind==KindPreset&&r.origin==OriginCable){var c=Cable016.ByCode(r.code);ok=c!=null&&Same(r.alias,c.layer)&&r.sourceId==""&&r.sourceSha256==""&&r.category=="";}
#endif
   if(!ok||!ValidCode(r.code))throw new InvalidOperationException("Bản ghi mã không hợp lệ ("+r.kind+"/"+r.origin+" "+r.code+").");
   foreach(var s in new[]{r.sourceId,r.alias,r.sourceSha256,r.category})if(s==null||s.Length>MaxText||s.Any(char.IsControl))throw new InvalidOperationException("Bản ghi mã "+r.code+" có giá trị thiếu/quá dài/ký tự điều khiển.");}
  public static string[] Encode(IdentityRecord015 r){return new[]{Schema,Version,r.kind,r.code,r.sourceId??"",r.alias??"",r.sourceSha256??"",r.category??"",r.origin};}
  public static IdentityRecord015 Decode(string[] v){if(v==null||v.Length!=Fields||v.Any(x=>x==null)||v[0]!=Schema||v[1]!=Version)throw new InvalidOperationException("Dữ liệu mã MEPS 015 không đúng schema "+Schema+" "+Version+" (bị sửa hoặc phiên bản khác); MEPS không ghi đè.");
   var r=new IdentityRecord015{kind=v[2],code=v[3],sourceId=v[4],alias=v[5],sourceSha256=v[6],category=v[7],origin=v[8]};Check(r);return r;}
  // Registry content for a project: the 67 book codes, every current catalog entry (book entries must still match the book), the six presets. Definitions
  // available, NOT quantities. Codes unique.
  public static List<IdentityRecord015> RegistryPlan(LibCatalog catalog){var list=Book.Select(f=>FrozenRecord(f)).ToList();
   foreach(var e in (catalog==null?new List<LibEntry>():catalog.entries).OrderBy(x=>x.id,StringComparer.Ordinal)){var r=BlockRecord(e);if(r.origin==OriginImported)list.Add(r);}
   list.AddRange(PresetRecords());foreach(var r in list)Check(r);
   if(list.Select(r=>r.code).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=list.Count)throw new InvalidOperationException("Sổ mã dự án có mã trùng; không ghi.");return list;}
  // Look-alike names: a book/catalog code, alone or with the _k suffix of a private copy. Used only to REPORT unmarked references; never to adopt one.
  public static bool LooksCanonical(string name,LibCatalog catalog){if(string.IsNullOrEmpty(name)||!name.StartsWith(Prefix,StringComparison.OrdinalIgnoreCase))return false;
   string bare=Regex.Replace(name,"_[0-9]+$","");var codes=new HashSet<string>(Book.Select(f=>f.code),StringComparer.OrdinalIgnoreCase);
   if(catalog!=null)foreach(var e in catalog.entries)try{codes.Add(BlockCode(e));}catch(InvalidOperationException){}
   return codes.Contains(name)||codes.Contains(bare);}
  // ---- legend (production wrapper of Meps06.PreflightForExport) ----
  // The legend always contains the nine source switches (rows 10 and 11 of the source table carry their texts and grouping). Missing ones are added to a
  // SEPARATE list as reference-only symbols (count 0, no handles); the caller's records are not modified. used/placements of the result are the actual
  // model values. An empty model stays blocked; a switch missing from the library is a blocker (no partial legend). Source/stale/foreign checks are those
  // of PreflightForExport, unchanged. TVXD-MEPS-LIBDELETE-021: the production caller (Plugin.RunLegend) passes Library.WithHistory(catalog), so a switch or
  // placed symbol deleted from the tool keeps its source-backed row; RegistryPlan/EnsureProject keep the ACTIVE catalog (a deleted entry is not offered).
  public static List<UsedSymbol> Augment(List<UsedSymbol> used,LibCatalog catalog,out List<string> missing){missing=new List<string>();var list=new List<UsedSymbol>(used??new List<UsedSymbol>());
   foreach(var id in Switches){if(list.Any(u=>u.entryId==id))continue;var e=catalog==null?null:catalog.entries.FirstOrDefault(x=>x.id==id);if(e==null){missing.Add(id);continue;}
    list.Add(new UsedSymbol{entryId=e.id,name=e.name,sourceSha256=e.sourceSha256,count=0,referenceOnly=true});}
   return list;}
  public static LegendPreflight Preflight(List<UsedSymbol> used,LegendMap map,LibCatalog catalog,List<ForeignRef> foreign){used=used??new List<UsedSymbol>();if(catalog==null)throw new InvalidOperationException("Thiếu catalog thư viện.");
   List<string> missing;var aug=Augment(used,catalog,out missing);
   var p=Meps06.PreflightForExport(aug,map,id=>catalog.entries.FirstOrDefault(e=>e.id==id),foreign);
   p.used=used.Count;p.placements=used.Sum(u=>u.count);
   var refs=new HashSet<string>(aug.Where(u=>u.referenceOnly).Select(u=>u.entryId),StringComparer.Ordinal);
   foreach(var i in p.items.Where(i=>i.entryId!=null&&refs.Contains(i.entryId)))i.reason="CHỈ THAM CHIẾU (chưa đặt trong Model, 0 lần, không tính khối lượng) — "+i.reason;
   if(refs.Count>0)p.items.Add(new PreflightItem{name="(công tắc tham chiếu)",status=ReferenceOnly,blocking=false,
    reason="Bảng ký hiệu luôn có 6 công tắc một chiều và 3 công tắc hai chiều theo hàng nguồn; "+refs.Count+" ký hiệu chưa đặt trong Model được thêm chỉ để tham chiếu: "+string.Join(", ",aug.Where(u=>u.referenceOnly).Select(u=>u.name)),
    action="Không phải số lượng thiết bị. Số loại/lần đặt ở trên là của Model thực tế."});
   foreach(var id in missing){var f=byId[id];p.items.Add(new PreflightItem{name=f.name,entryId=id,status=SwitchMissing,blocking=true,reason="công tắc bắt buộc của bảng ký hiệu ("+f.code+") không có trong thư viện "+Meps05.LibraryDir,
    action="Thêm lại nguồn chứa "+f.name+" bằng '+'. MEPS không xuất bảng thiếu công tắc."});}
   if(used.Count==0&&!p.items.Any(i=>i.status==Meps06.Empty))p.items.Add(new PreflightItem{name="(Model)",status=Meps06.Empty,blocking=true,reason="Model chưa có ký hiệu MEPS nào (đã bỏ qua xref, paper space, khối đã xoá); công tắc tham chiếu không thay cho thiết bị đã đặt",action="Đặt ký hiệu từ bảng MEPS rồi thử lại."});
   return p;}
 }
#if !IDENTITY_PURE
 // Result of one EnsureProject.
 public sealed class IdentityReport015 {public int added,existing,total;public string Summary(){return "sổ mã "+Identity015.Schema+" "+Identity015.Version+": thêm "+added+", có sẵn đúng "+existing+" / "+total+" mã (định nghĩa có thể dùng, không phải số lượng)";}}
 // AutoCAD side. Registry: named-object dictionary key Identity015.Key → DBDictionary {"SCHEMA": [Schema, Version]; <code>: record}. Entity identity: one
 // Xrecord under Identity015.Key in the entity's extension dictionary. Every write goes through the caller's transaction (insertion/draw) or one own
 // transaction (EnsureProject), so UNDO and an abort remove it with the rest. Existing owned keys that are malformed or different are refused, never
 // overwritten; foreign keys are never touched. A copied entity keeps its record: it is a TYPE code, not a unique occurrence ID.
 public static class IdentityCad015 {
  public const string SchemaKey="SCHEMA";
  static string[] Values(Xrecord x){using(var d=x.Data){return d==null?new string[0]:d.AsArray().Select(v=>v.TypeCode==(short)DxfCode.Text?Convert.ToString(v.Value,CultureInfo.InvariantCulture):null).ToArray();}}
  static ResultBuffer Buffer(IdentityRecord015 r){return new ResultBuffer(Identity015.Encode(r).Select(s=>new TypedValue((int)DxfCode.Text,s)).ToArray());}
  // Identity of an object; null when it has none; malformed → exception.
  public static IdentityRecord015 Read(Transaction tr,DBObject o){if(o.ExtensionDictionary.IsNull)return null;var d=(DBDictionary)tr.GetObject(o.ExtensionDictionary,OpenMode.ForRead);if(!d.Contains(Identity015.Key))return null;
   var x=tr.GetObject(d.GetAt(Identity015.Key),OpenMode.ForRead) as Xrecord;if(x==null)throw new InvalidOperationException("Khoá "+Identity015.Key+" của đối tượng "+o.Handle+" không phải Xrecord; MEPS không ghi đè.");return Identity015.Decode(Values(x));}
  // Writes r once; the same record already present is accepted, a different/malformed one is refused (exception, caller's transaction aborts).
  public static void Write(Transaction tr,DBObject o,IdentityRecord015 r){Identity015.Check(r);var had=Read(tr,o);if(had!=null){if(had.SameAs(r))return;throw new InvalidOperationException("Đối tượng "+o.Handle+" đã có mã "+had.code+" khác "+r.code+"; MEPS không ghi đè.");}
   if(!o.IsWriteEnabled)o.UpgradeOpen();if(o.ExtensionDictionary.IsNull)o.CreateExtensionDictionary();var d=(DBDictionary)tr.GetObject(o.ExtensionDictionary,OpenMode.ForWrite);
   var x=new Xrecord();using(var b=Buffer(r))x.Data=b;d.SetAt(Identity015.Key,x);tr.AddNewlyCreatedDBObject(x,true);}
  public static void MarkBlock(Transaction tr,BlockReference br,LibEntry e){Write(tr,br,Identity015.BlockRecord(e));}
  public static void MarkPreset(Transaction tr,Polyline poly,PresetSnapshot012 p){Write(tr,poly,Identity015.PresetRecord(p.layer));}
  public static void MarkSample(Transaction tr,Polyline poly,WireStyle010 s){Write(tr,poly,Identity015.SampleRecord(s.id,s.layer));}
  static void CheckSchema(Transaction tr,DBDictionary reg){if(!reg.Contains(SchemaKey))throw new InvalidOperationException("Sổ mã "+Identity015.Key+" trong bản vẽ thiếu SCHEMA; MEPS không ghi đè.");
   var x=tr.GetObject(reg.GetAt(SchemaKey),OpenMode.ForRead) as Xrecord;var v=x==null?null:Values(x);
   if(v==null||v.Length!=2||v[0]!=Identity015.Schema||v[1]!=Identity015.Version)throw new InvalidOperationException("Sổ mã "+Identity015.Key+" trong bản vẽ khác schema "+Identity015.Schema+" "+Identity015.Version+"; MEPS không ghi đè.");}
  // Records of the drawing registry by code; null when the drawing has none. Malformed → exception.
  public static Dictionary<string,IdentityRecord015> ReadRegistry(Transaction tr,Database db){var nod=(DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForRead);if(!nod.Contains(Identity015.Key))return null;
   var reg=tr.GetObject(nod.GetAt(Identity015.Key),OpenMode.ForRead) as DBDictionary;if(reg==null)throw new InvalidOperationException("Khoá "+Identity015.Key+" của bản vẽ không phải từ điển MEPS; MEPS không ghi đè.");CheckSchema(tr,reg);
   var have=new Dictionary<string,IdentityRecord015>(StringComparer.OrdinalIgnoreCase);
   foreach(DBDictionaryEntry de in reg){if(de.Key==SchemaKey)continue;var x=tr.GetObject(de.Value,OpenMode.ForRead) as Xrecord;if(x==null)throw new InvalidOperationException("Sổ mã: mục "+de.Key+" không phải Xrecord; MEPS không ghi đè.");
    var r=Identity015.Decode(Values(x));if(!string.Equals(r.code,de.Key,StringComparison.Ordinal))throw new InvalidOperationException("Sổ mã: khoá "+de.Key+" khác mã "+r.code+"; MEPS không ghi đè.");have[de.Key]=r;}
   return have;}
  // Clean-project registry (TVXDMEPS012PREP only): created when absent; existing equal codes kept; missing codes added; any malformed or different owned
  // record aborts the whole transaction (nothing written). Running it again with the same catalog writes nothing. No block definition is cloned here.
  public static IdentityReport015 EnsureProject(Database db,LibCatalog catalog){var want=Identity015.RegistryPlan(catalog);var rep=new IdentityReport015{total=want.Count};
   using(var tr=db.TransactionManager.StartTransaction()){var have=ReadRegistry(tr,db);DBDictionary reg;
    if(have==null){var nod=(DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForWrite);reg=new DBDictionary();nod.SetAt(Identity015.Key,reg);tr.AddNewlyCreatedDBObject(reg,true);
     var s=new Xrecord();using(var b=new ResultBuffer(new TypedValue((int)DxfCode.Text,Identity015.Schema),new TypedValue((int)DxfCode.Text,Identity015.Version)))s.Data=b;reg.SetAt(SchemaKey,s);tr.AddNewlyCreatedDBObject(s,true);
     have=new Dictionary<string,IdentityRecord015>(StringComparer.OrdinalIgnoreCase);}
    else reg=(DBDictionary)tr.GetObject(((DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForRead)).GetAt(Identity015.Key),OpenMode.ForRead);
    foreach(var r in want){IdentityRecord015 h;if(have.TryGetValue(r.code,out h)){if(!h.SameAs(r))throw new InvalidOperationException("Sổ mã trong bản vẽ có "+r.code+" khác định nghĩa hiện hành ("+h.alias+" ≠ "+r.alias+"); MEPS không ghi đè, chưa ghi gì.");rep.existing++;continue;}
     if(!reg.IsWriteEnabled)reg.UpgradeOpen();var x=new Xrecord();using(var b=Buffer(r))x.Data=b;reg.SetAt(r.code,x);tr.AddNewlyCreatedDBObject(x,true);have[r.code]=r;rep.added++;}
    tr.Commit();}
   return rep;}
 }
#endif
}
