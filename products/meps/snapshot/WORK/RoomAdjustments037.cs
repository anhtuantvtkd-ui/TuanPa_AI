using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using IOE=System.InvalidOperationException;

namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 S5A: PURE optional per-room adjustments for RoomCompletion037 (no CAD, Forms, JSON or IO; C# 5 / .NET 4 + System.Core;
 // ASCII). Public fields only (JavaScriptSerializer friendly). batch.adjustments == null (or empty lists) = no adjustment, existing behaviour unchanged.
 //  - Exclusion (BO DONG / KHOI PHUC): one record per exact source rowKey, with the source signature captured when the user excluded it and a
 //    required reason. active=true excludes the row from the ACTIVE set only; active=false = restored, record kept as audit. Never matched by label,
 //    never remapped: a record whose signature differs from batch.sourceSignature, or whose rowKey is not produced by the current source, is
 //    batch CHECK until the user re-confirms (Exclude again with the fresh signature) or restores it.
 //  - Supplement (THEM DONG): an explicit manual material quantity, separate from graph rows and measured totals. Entered metres are the final
 //    additional amount (no factor, core count or tail inferred; no CAD endpoint). null text/metres = unfinished (MISSING_DATA), 0 = valid value.
 //    active=false = removed (reversible; record kept). An active complete supplement whose kind/material code is also declared by an active
 //    measured row of the room needs overlapConfirmed=true (explicit include decision), else CHECK. Changing kind or code clears that confirmation.
 //  - Problem = structural validation (corrupt containers, IDs, kinds, codes, non-finite/negative metres, duplicates, limits). Incomplete user
 //    declarations are NOT structural problems; Calculate reports them as missing states.
 //  - Every mutating call validates the whole data and the whole request first, then mutates; failure = InvalidOperationException and nothing
 //    changed (ArgumentNullException for a null id factory).
 public sealed class RoomExclusion037 {public string rowKey,sourceSignature,reason;public bool active;}
 // kind: WIRE | CONDUIT_HIDDEN | CONDUIT_EXPOSED (exact). unit: exactly "m". circuitId optional.
 public sealed class RoomSupplement037 {
  public string id,roomId,circuitId,kind,materialCode,unit,description,reason;
  public double? metres;
  public bool active,overlapConfirmed;
 }
 public sealed class RoomAdjustmentsData037 {
  public int version=1;
  public List<RoomExclusion037> exclusions=new List<RoomExclusion037>();
  public List<RoomSupplement037> supplements=new List<RoomSupplement037>();
 }
 // ---- Computed by RoomCompletion037.Calculate (never persisted as declarations) ----
 // status: APPLIED | INACTIVE | SIGNATURE_MISMATCH | UNMATCHED.
 public sealed class RoomExclusionState037 {public string rowKey,status,reason,message;}
 // status: PASS | MISSING_DATA | CHECK | INACTIVE. overlap: an active measured row of the room declares the same kind/code.
 public sealed class RoomSupplementState037 {public string id,kind,materialCode,status;public bool overlap;
  public List<string> missing=new List<string>(),issues=new List<string>();}
 // Manual totals of PASS active supplements only. source is always "MANUAL".
 public sealed class RoomManualTotal037 {public string source,kind,code,unit;public int items;public double metres;}
 // Exact (kind, code, unit) match only; never sums unlike materials. cadM = measured after-factor metres of active PASS rows (b.totals, all floors):
 // WIRE afterFactorM, CONDUIT_HIDDEN hiddenAfterFactorM, CONDUIT_EXPOSED exposedAfterFactorM. manualM = MANUAL. combinedM = cadM + manualM.
 // Official only when batch.totalsOfficial.
 public sealed class RoomCombinedTotal037 {public string kind,code,unit;public int cadRows,manualItems;public double cadM,manualM,combinedM;}

 public static class RoomAdjustments037 {
  public const int Version=1;
  public const int MaxExclusions=RoomCompletion037.MaxRows,MaxSupplements=RoomCompletion037.MaxRows,MaxNote=1000,MaxKey=1000;
  public const string Wire="WIRE",ConduitHidden="CONDUIT_HIDDEN",ConduitExposed="CONDUIT_EXPOSED",UnitM="m",IdPrefix="MS_";
  public const string Applied="APPLIED",Inactive="INACTIVE",SignatureMismatch="SIGNATURE_MISMATCH",Unmatched="UNMATCHED",Manual="MANUAL";
  public static readonly ReadOnlyCollection<string> Kinds=new ReadOnlyCollection<string>(new[]{Wire,ConduitHidden,ConduitExposed});
  const char US='\u001f';
  static readonly StringComparer O=StringComparer.Ordinal,OI=StringComparer.OrdinalIgnoreCase;
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;

  static string Show(string s){return s==null?"(null)":"\""+s+"\"";}
  static bool Unset(string s){return string.IsNullOrEmpty(s);}
  static bool IsHex(char c){return (c>='0'&&c<='9')||(c>='a'&&c<='f')||(c>='A'&&c<='F');}

  // ---- Field checks (null = acceptable) ----
  // Exact key/signature text: required, <= MaxKey, no control character. Never trimmed.
  static string KeyProblem(string s,string what){
   if(Unset(s))return what+" is empty.";
   if(s.Length>MaxKey)return what+" is longer than "+MaxKey.ToString(Inv)+" characters.";
   foreach(var c in s)if(char.IsControl(c))return what+" contains a control character.";
   return null;}
  // Free note (description/reason): required when checked, not whitespace only, <= MaxNote, no control character.
  public static string NoteProblem(string s,string what){
   if(Unset(s))return what+" is empty.";
   if(s.Trim().Length==0)return what+" contains only white space.";
   if(s.Length>MaxNote)return what+" is longer than "+MaxNote.ToString(Inv)+" characters.";
   foreach(var c in s)if(char.IsControl(c))return what+" contains a control character.";
   return null;}
  // Material / circuit code: the exact Bdgkl034 text rule used by measured assembly/conduit codes (so exact matching is meaningful).
  public static string CodeProblem(string s,string what){var t=Bdgkl034.TextProblem(s);return t==null?null:what+" "+Show(s)+" is not usable: "+t+".";}
  // null (blank, unfinished) is acceptable here; Calculate reports it as missing.
  public static string MetresProblem(double? v){
   if(!v.HasValue)return null;
   if(double.IsNaN(v.Value)||double.IsInfinity(v.Value))return "Metres must be a finite number.";
   if(v.Value<0)return "Metres must be >= 0 (0 is a valid explicit value).";
   return null;}
  public static string KindProblem(string k){return Unset(k)||Kinds.Contains(k)?null:"Kind "+Show(k)+" is not "+string.Join("/",Kinds)+" (exact).";}
  static string IdProblem(string id){
   if(id==null||!id.StartsWith(IdPrefix,StringComparison.Ordinal)||id.Length!=IdPrefix.Length+32||!id.Skip(IdPrefix.Length).All(IsHex))
    return "Supplement ID "+Show(id)+" is not "+IdPrefix+"<32 hex>.";
   if(id.Skip(IdPrefix.Length).All(c=>c=='0'))return "Supplement ID "+id+" uses the empty Guid.";
   return null;}
  static string OptionalCode(string s,string what){return Unset(s)?null:CodeProblem(s,what);}
  static string OptionalNote(string s,string what){return Unset(s)?null:NoteProblem(s,what);}

  // Whole-data structural validation. null = valid. Corrupt lists/records are reported, never hidden or repaired.
  public static string Problem(RoomAdjustmentsData037 d){
   if(d==null)return "Adjustments are null.";
   if(d.version!=Version)return "Adjustments version "+d.version.ToString(Inv)+" differs from supported version "+Version.ToString(Inv)+".";
   if(d.exclusions==null)return "Exclusion list is null.";
   if(d.supplements==null)return "Supplement list is null.";
   if(d.exclusions.Count>MaxExclusions)return d.exclusions.Count.ToString(Inv)+" exclusions exceed the supported "+MaxExclusions.ToString(Inv)+".";
   if(d.supplements.Count>MaxSupplements)return d.supplements.Count.ToString(Inv)+" supplements exceed the supported "+MaxSupplements.ToString(Inv)+".";
   var keys=new HashSet<string>(O);string t;
   foreach(var e in d.exclusions){
    if(e==null)return "An exclusion record is null.";
    t=KeyProblem(e.rowKey,"Exclusion row key");if(t!=null)return t;
    if(!keys.Add(e.rowKey))return "Row "+e.rowKey+" has two exclusion records.";
    t=KeyProblem(e.sourceSignature,"Exclusion source signature (row "+e.rowKey+")");if(t!=null)return t;
    t=NoteProblem(e.reason,"Exclusion reason (row "+e.rowKey+")");if(t!=null)return t;}
   var ids=new HashSet<string>(OI);
   foreach(var s in d.supplements){
    if(s==null)return "A supplement record is null.";
    t=IdProblem(s.id);if(t!=null)return t;
    if(!ids.Add(s.id))return "Supplement ID "+s.id+" is used twice.";
    t=KeyProblem(s.roomId,"Room id of supplement "+s.id);if(t!=null)return t;
    if(!O.Equals(s.unit,UnitM))return "Unit of supplement "+s.id+" is "+Show(s.unit)+", not \"m\".";
    t=KindProblem(s.kind);if(t!=null)return t+" (supplement "+s.id+")";
    t=OptionalCode(s.materialCode,"Material code of supplement "+s.id);if(t!=null)return t;
    t=OptionalCode(s.circuitId,"Circuit of supplement "+s.id);if(t!=null)return t;
    t=MetresProblem(s.metres);if(t!=null)return t+" (supplement "+s.id+")";
    t=OptionalNote(s.description,"Description of supplement "+s.id);if(t!=null)return t;
    t=OptionalNote(s.reason,"Reason of supplement "+s.id);if(t!=null)return t;}
   return null;}

  // Deep copy; null lists/records stay null so Problem still reports them. Never mutates the caller's object.
  public static RoomAdjustmentsData037 Copy(RoomAdjustmentsData037 d){
   if(d==null)return null;
   return new RoomAdjustmentsData037{version=d.version,
    exclusions=d.exclusions==null?null:d.exclusions.Select(e=>e==null?null:new RoomExclusion037{rowKey=e.rowKey,sourceSignature=e.sourceSignature,reason=e.reason,active=e.active}).ToList(),
    supplements=d.supplements==null?null:d.supplements.Select(s=>s==null?null:Copy(s)).ToList()};}
  public static RoomSupplement037 Copy(RoomSupplement037 s){
   if(s==null)return null;
   return new RoomSupplement037{id=s.id,roomId=s.roomId,circuitId=s.circuitId,kind=s.kind,materialCode=s.materialCode,unit=s.unit,
    description=s.description,reason=s.reason,metres=s.metres,active=s.active,overlapConfirmed=s.overlapConfirmed};}

  static void Fail(string m){throw new IOE(m+" Nothing changed.");}
  static void Check(RoomAdjustmentsData037 d){var e=Problem(d);if(e!=null)Fail("Adjustments are invalid: "+e);}
  static string Blank(string s){return s==""?null:s;}
  static RoomSupplement037 Supplement(RoomAdjustmentsData037 d,string roomId,string id){
   var t=KeyProblem(roomId,"Room id");if(t!=null)Fail(t);
   var s=d.supplements.FirstOrDefault(x=>OI.Equals(x.id,id));
   if(s==null)Fail("Supplement "+Show(id)+" does not exist.");
   if(!O.Equals(s.roomId,roomId))Fail("Supplement "+s.id+" belongs to room "+s.roomId+", not "+roomId+".");
   return s;}

  // ---- Exclusions ----
  // BO DONG: exclude the exact source row captured under sourceSignature. Re-excluding an existing record (also an UNMATCHED/SIGNATURE_MISMATCH
  // one after the user checked it) updates signature and reason and re-activates it. Reason required.
  public static RoomExclusion037 Exclude(RoomAdjustmentsData037 d,string rowKey,string sourceSignature,string reason){
   Check(d);
   var t=KeyProblem(rowKey,"Row key");if(t!=null)Fail(t);
   t=KeyProblem(sourceSignature,"Source signature");if(t!=null)Fail(t+" A row can only be excluded against a completed scan.");
   t=NoteProblem(reason,"Exclusion reason");if(t!=null)Fail(t);
   var e=d.exclusions.FirstOrDefault(x=>O.Equals(x.rowKey,rowKey));
   if(e==null){if(d.exclusions.Count>=MaxExclusions)Fail("Already "+MaxExclusions.ToString(Inv)+" exclusion records.");
    e=new RoomExclusion037{rowKey=rowKey};d.exclusions.Add(e);}
   e.sourceSignature=sourceSignature;e.reason=reason;e.active=true;
   return e;}
  // KHOI PHUC: the record stays (audit), active=false. true = it was active; false = it was already restored. Unknown row key = error.
  public static bool Restore(RoomAdjustmentsData037 d,string rowKey){
   Check(d);
   var e=d.exclusions.FirstOrDefault(x=>O.Equals(x.rowKey,rowKey));
   if(e==null)Fail("Row "+Show(rowKey)+" has no exclusion record.");
   bool was=e.active;e.active=false;return was;}

  // ---- Supplements ----
  // THEM DONG: new active, blank (unfinished) record of roomId with a fresh MS_ id. Unfinished records keep the batch MISSING_DATA.
  public static RoomSupplement037 AddSupplement(RoomAdjustmentsData037 d,string roomId,Func<Guid> ids){
   if(ids==null)throw new ArgumentNullException("ids");
   Check(d);
   var t=KeyProblem(roomId,"Room id");if(t!=null)Fail(t);
   if(d.supplements.Count>=MaxSupplements)Fail("Already "+MaxSupplements.ToString(Inv)+" supplement records.");
   var g=ids();if(g==Guid.Empty)Fail("The id factory returned the empty Guid.");
   string id=IdPrefix+g.ToString("N");
   if(d.supplements.Any(x=>OI.Equals(x.id,id)))Fail("Supplement ID "+id+" already exists.");
   var s=new RoomSupplement037{id=id,roomId=roomId,unit=UnitM,active=true};
   d.supplements.Add(s);return s;}
  // Sets all editable values of one supplement at once (null or "" = blank/unfinished). Invalid values refuse the whole edit.
  // A change of kind or material code clears overlapConfirmed (the include decision applied to the previous material).
  public static RoomSupplement037 UpdateSupplement(RoomAdjustmentsData037 d,string roomId,string id,string kind,string materialCode,double? metres,
   string circuitId,string description,string reason){
   Check(d);var s=Supplement(d,roomId,id);
   kind=Blank(kind);materialCode=Blank(materialCode);circuitId=Blank(circuitId);description=Blank(description);reason=Blank(reason);
   var t=KindProblem(kind);if(t!=null)Fail(t);
   t=OptionalCode(materialCode,"Material code");if(t!=null)Fail(t);
   t=MetresProblem(metres);if(t!=null)Fail(t);
   t=OptionalCode(circuitId,"Circuit");if(t!=null)Fail(t);
   t=OptionalNote(description,"Description");if(t!=null)Fail(t);
   t=OptionalNote(reason,"Reason");if(t!=null)Fail(t);
   if(!O.Equals(s.kind,kind)||!O.Equals(s.materialCode,materialCode))s.overlapConfirmed=false;
   s.kind=kind;s.materialCode=materialCode;s.metres=metres.HasValue&&metres.Value==0?0.0:metres;s.circuitId=circuitId;s.description=description;s.reason=reason;
   return s;}
  // XOA (reversible, active=false) / KHOI PHUC (active=true). Record and values are kept. true = the flag changed.
  public static bool SetSupplementActive(RoomAdjustmentsData037 d,string roomId,string id,bool active){
   Check(d);var s=Supplement(d,roomId,id);
   if(s.active==active)return false;s.active=active;return true;}
  // Explicit include decision for a manual quantity that overlaps a measured material of the room. true = the flag changed.
  public static bool ConfirmOverlap(RoomAdjustmentsData037 d,string roomId,string id,bool confirmed){
   Check(d);var s=Supplement(d,roomId,id);
   if(s.overlapConfirmed==confirmed)return false;s.overlapConfirmed=confirmed;return true;}

  // ---- Integration with RoomCompletion037.Calculate (internal) ----
  // Applies valid exclusions to the derived source rows (complete selection already done). Returns the rows to exclude; records states and
  // batch CHECK issues for active records that do not match the current source exactly. Requires Problem(b.adjustments) == null.
  internal static Dictionary<RoomCompletionRow037,string> Exclusions(RoomCompletionBatch037 b,List<RoomCompletionRow037> derived){
   var off=new Dictionary<RoomCompletionRow037,string>();
   var byKey=new Dictionary<string,RoomCompletionRow037>(O);
   foreach(var r in derived)if(r.rowKey!=null&&!byKey.ContainsKey(r.rowKey))byKey[r.rowKey]=r;
   foreach(var e in b.adjustments.exclusions){
    var st=new RoomExclusionState037{rowKey=e.rowKey,reason=e.reason};b.exclusionStates.Add(st);
    RoomCompletionRow037 r;bool found=byKey.TryGetValue(e.rowKey,out r);
    if(!e.active){st.status=Inactive;st.message="Restored; record kept for audit"+(found?".":" (row not in the current source).");continue;}
    if(!O.Equals(e.sourceSignature,b.sourceSignature)){st.status=SignatureMismatch;
     st.message="Exclusion of row "+e.rowKey+" was captured on another source signature"+(b.sourceSignature==null?" and the current signature is unknown":"")+
      (found?"":"; the row key is not in the current source either")+". Not applied; confirm it again on the current scan or restore it.";
     b.issues.Add("EXCLUSION_SIGNATURE_MISMATCH: "+st.message);continue;}
    if(!found){st.status=Unmatched;
     st.message="Excluded row "+e.rowKey+" is not produced by the current source; nothing is remapped. Restore the record or recheck the drawing.";
     b.issues.Add("EXCLUSION_UNMATCHED: "+st.message);continue;}
    st.status=Applied;st.message="Excluded: "+e.reason;off[r]=e.reason;}
   return off;}

  // Evaluates supplements, adds their missing/check states to the batch, fills manualTotals and combinedTotals. active = active derived rows.
  // adjustmentsUsable=false (null or invalid adjustments): no supplement is evaluated; combined totals are measured only.
  internal static void Supplements(RoomCompletionBatch037 b,List<RoomCompletionRow037> active,bool adjustmentsUsable){
   var manual=new Dictionary<string,RoomManualTotal037>(O);
   if(adjustmentsUsable&&b.adjustments!=null){
    string room=b.source==null?null:b.source.roomId;
    var wireCodes=new HashSet<string>(active.Where(r=>r.input!=null&&r.input.assemblyCode!=null).Select(r=>r.input.assemblyCode),O);
    var pipeCodes=new HashSet<string>(active.Where(r=>r.input!=null&&r.input.conduitCode!=null).Select(r=>r.input.conduitCode),O);
    foreach(var s in b.adjustments.supplements){
     var st=new RoomSupplementState037{id=s.id,kind=s.kind,materialCode=s.materialCode};b.supplementStates.Add(st);
     if(!O.Equals(s.roomId,room))st.issues.Add("SUPPLEMENT_ROOM_MISMATCH: supplement "+s.id+" belongs to room "+s.roomId+", not to the calculated room "+Show(room)+".");
     if(!s.active){if(st.issues.Count==0){st.status=Inactive;continue;}}
     else{
      if(Unset(s.kind))st.missing.Add("SUPPLEMENT_KIND_MISSING: supplement "+s.id+" has no kind ("+string.Join("/",Kinds)+").");
      if(Unset(s.materialCode))st.missing.Add("SUPPLEMENT_CODE_MISSING: supplement "+s.id+" has no material code.");
      if(!s.metres.HasValue)st.missing.Add("SUPPLEMENT_METRES_MISSING: supplement "+s.id+" has no metres (blank is unfinished; 0 must be entered explicitly).");
      if(Unset(s.description))st.missing.Add("SUPPLEMENT_DESCRIPTION_MISSING: supplement "+s.id+" has no description.");
      if(Unset(s.reason))st.missing.Add("SUPPLEMENT_REASON_MISSING: supplement "+s.id+" has no reason.");
      if(!Unset(s.kind)&&!Unset(s.materialCode)){
       st.overlap=(s.kind==Wire?wireCodes:pipeCodes).Contains(s.materialCode);
       if(st.overlap&&!s.overlapConfirmed)st.issues.Add("SUPPLEMENT_OVERLAP_UNCONFIRMED: supplement "+s.id+" adds "+s.kind+" "+s.materialCode+
        " which is also measured in this room; confirm explicitly that it is an additional quantity (no automatic merge or removal).");}}
     st.status=st.issues.Count>0?RoomCompletion037.Check:st.missing.Count>0?RoomCompletion037.MissingData:RoomCompletion037.Pass;
     b.issues.AddRange(st.issues);b.missing.AddRange(st.missing);
     if(st.status!=RoomCompletion037.Pass)continue;
     string k=s.kind+US+s.materialCode+US+s.unit;RoomManualTotal037 mt;
     if(!manual.TryGetValue(k,out mt))manual[k]=mt=new RoomManualTotal037{source=Manual,kind=s.kind,code=s.materialCode,unit=s.unit};
     mt.items++;mt.metres+=s.metres.Value;}}
   b.manualTotals=manual.Values.OrderBy(x=>x.kind,O).ThenBy(x=>x.code,O).ToList();
   var comb=new Dictionary<string,RoomCombinedTotal037>(O);
   Func<string,string,RoomCombinedTotal037> get=(kind,code)=>{RoomCombinedTotal037 c;string k=kind+US+code+US+UnitM;
    if(!comb.TryGetValue(k,out c))comb[k]=c=new RoomCombinedTotal037{kind=kind,code=code,unit=UnitM};return c;};
   foreach(var t in b.totals){
    if(t.kind=="WIRE"){var c=get(Wire,t.code);c.cadRows+=t.rows;c.cadM+=t.afterFactorM;}
    else if(t.kind=="CONDUIT"){var h=get(ConduitHidden,t.code);h.cadRows+=t.rows;h.cadM+=t.hiddenAfterFactorM;
     var x=get(ConduitExposed,t.code);x.cadRows+=t.rows;x.cadM+=t.exposedAfterFactorM;}}
   foreach(var m in b.manualTotals){var c=get(m.kind,m.code);c.manualItems+=m.items;c.manualM+=m.metres;}
   b.combinedTotals=comb.Values.OrderBy(x=>x.kind,O).ThenBy(x=>x.code,O).ToList();
   foreach(var c in b.combinedTotals){c.combinedM=c.cadM+c.manualM;
    if(double.IsNaN(c.combinedM)||double.IsInfinity(c.combinedM))b.issues.Add("TOTAL_OVERFLOW: combined "+c.kind+" "+c.code+" total is not finite.");}
   foreach(var m in b.manualTotals)if(double.IsNaN(m.metres)||double.IsInfinity(m.metres))b.issues.Add("TOTAL_OVERFLOW: manual "+m.kind+" "+m.code+" total is not finite.");}
 }
}
