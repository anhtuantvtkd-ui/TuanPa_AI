using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Tvxd.Meps {
 // TVXD-MEPS-ROOMQTY-037 COMPLETION MODEL: PURE room completion / quantity model over the native-tested room graph (RoomQty037) and the BDGKL row
 // formulas (Bdgkl034.Calculate). No CAD reference, no IO, no UI; C# 5 / .NET 4 + System.Core. This file is ASCII; it is compiled together with
 // Bdgkl034.cs, which needs /codepage:65001. Public fields only (JavaScriptSerializer friendly). Statuses: MISSING_DATA | CHECK | PASS.
 // PASS = data-complete for quantity take-off, NOT engineering approval.
 //
 // Candidate selection (recomputed by every Calculate from batch.source; old rows/results are never trusted):
 //  - every RoomQty037.ResolveControlPaths logical path becomes one row (kind NETWORK_PATH, key = pathId "P:<switch>-><target>");
 //  - S6 LOGICAL PATH vs MEASURED SECTION: a logical path is the full controller->load association (it stays unchanged in RoomQty037 and keeps all
 //    its diagnostics). The measured row of a daisy chain S -> A -> B -> C (same channel, actual CAD path order) is the physical section from the
 //    LAST upstream device on that path: rows S->A, A->B, B->C (kind NETWORK_SECTION, key "<pathId>|SEC:<upstream>-><target>", segments = the exact
 //    suffix of the path after the upstream device). Projection only when the path is ASSOCIATED and the upstream device B is proven: listed LAMP/FAN,
 //    exactly one logical path S->B exists, it is ASSOCIATED on the same control key, and its ordered segments/nodes/intermediate devices are an
 //    exact prefix of S->C, and S->B's own projection is proven in turn (whole upstream chain). Unprovable -> the full path row is kept with source status CHECK (SECTION_UPSTREAM_UNPROVEN) plus a SELECTION blocker;
 //    a non-ASSOCIATED path through devices keeps its full path (SECTION_NOT_PROJECTED) and its own status. No nearest/straight-line inference:
 //    only segment order of the traced path is used. Paths without intermediate device (first load, fork siblings) keep key and full path.
 //    A changed section has a NEW key, so declarations/exclusions stored under the old full-path key are never reused (unmatched/orphan guards).
 //  - S6 labels: startLabel/endLabel = RoomQty037.DisplayLabels (compact type code + visible TAG, room-local display ordinal for duplicates);
 //    startName/endName (block name) and startId/endId (handle) are unchanged data.
 //  - an Associate chain row becomes a row (kind CHAIN, key "C:<from>-><to>#<smallest segment id>") only when NONE of its segments lies on a
 //    network path; a chain row fully on network paths is dropped (note, no duplicate quantity); a partial overlap is a blocker.
 //  - non-ASSOCIATED candidates stay visible; their core status keeps them MISSING_DATA/CHECK whatever numbers are filled in.
 //  - blockers: every non-INFO core diagnostic (plus INFO DEVICE_NO_SEGMENT = unbound device) unless ALL segments it owns are used by selected
 //    rows (e.g. Associate AMBIGUOUS_BRANCH of a tree fully resolved by the network, or NETWORK_NO_CONTROLLER of a valid socket chain). Diagnostics
 //    without segments (rejected segment, malformed node map, bad device, units, coverage ...) always block. Every accepted segment must be used by
 //    a row or owned by a blocker (else SEGMENT_UNACCOUNTED). External (native/UI) blockers are passed in batch.externalBlockers and always block.
 // Per row: derived read-only facts are reset on every Calculate: routeKey=rowKey, areaId=roomId, startKey/endKey=device IDs, labels=core labels,
 //  nativeLength=sum of the traced segment lengths (drawing units, unrounded), declaredUnit=room units, sharedHorizontal/Vertical/OwnerKey=allocation.
 //  Editable (start empty; no sample defaults): floorId, routingMode, ceilingHeight, start/end height mode/value, tailCount, tailEachLength, factor,
 //  assemblyCode, conduitCode (+ optional floorFinishedElevation). circuitId is prefilled once: structured "panel/circuit" when known, else the
 //  room-local control-channel TAG label. That TAG is a control/channel label, NOT an inferred feeder/panel circuit; the user may overwrite it.
 // Global declarations: independentAssembliesConfirmed (default false) = one independently declared wire assembly per logical row (no shared
 //  neutral/PE inference, no multiplication by a parsed core count); conduitMode REQUIRED, exactly SEPARATE or SHARED (no implicit choice).
 // Consistency (both modes): one device has one floor and one height declaration (mode + values) per floor; one floor has one ceiling height.
 //  Conflicts are CHECK on every row involved (never hidden behind separate keys).
 // SEPARATE: U = V = 0, owner null, every segment/drop counted by its own row.
 // SHARED (eligible rows only = Bdgkl OK with U=V=0 and no other issue; stable ordinal rowKey order):
 //  horizontal: a source segment is one physical item per (segmentId, floor, area, routingMode, conduitCode); its first row OWNS it, every later row
 //   deducts the segment's measured metres (U). Same segment with different declarations is NOT shared (role NOT_SHARED_DECLARATION_DIFFERS).
 //  vertical CEILING/FLOOR: the endpoint drop (CEILING: ceiling - height; FLOOR: height) is one physical item per (deviceId, floor, mode, conduitCode);
 //   first row owns, later rows deduct their own (identical, consistency-checked) drop (V). Zero drops are not shared (NO_DROP).
 //  WALL: no vertical sharing is inferred. Two eligible WALL rows with J > 0 meeting at the same (device, floor, WALL, conduit) are CHECK
 //   WALL_SHARED_VERTICAL_UNPROVEN (an overlap of their vertical intervals is not proven by the source).
 //  Every physical group with 2+ rows must carry one identical factor (else CHECK SHARED_FACTOR_MISMATCH on all members).
 //  Bdgkl034.Calculate receives U, V and the ordinal-smallest real owner as representative sharedOwnerKey; ALL real owners are kept in
 //  ownerRowKeys and validated: an owner that is not PASS makes the dependent CHECK (fixpoint). Bdgkl034.Aggregate is NOT used; totals below
 //  are summed from PASS rows only, keyed by exact floor + assembly / conduit code (custom/XLPE codes are kept). Tails are wire only, never conduit.
 // Batch PASS: >= 1 row, every row PASS, no blocker, no external blocker, no batch issue, rows <= MaxRows. No partial batch PASS.
 // Calculate never throws for bad data (null batch -> ArgumentNullException); an unexpected internal exception becomes batch CHECK INTERNAL_ERROR.
 // S5A optional adjustments (RoomAdjustments037; null or empty = previous behaviour): selection and source blockers always use the COMPLETE source;
 //  then valid exclusions (exact rowKey + batch.sourceSignature) remove rows from the ACTIVE set only. Consistency, allocation/sharing, the owner
 //  fixpoint and totals use active rows only, so ownership moves to the remaining eligible row. b.rows stays inclusive: an excluded row keeps its
 //  input, gets excluded=true, exclusionReason, status EXCLUDED and result=null (no quantity). pass/missingData/check count active rows; excluded
 //  counts excluded rows. A non-ASSOCIATED excluded row still blocks (batch issue/missing). No active row = CHECK. Invalid adjustments = batch CHECK,
 //  nothing applied. Supplements are evaluated after the measured totals (b.totals stays measured-only); see manualTotals / combinedTotals.
 public sealed class RoomCompletionSegment037 {public string id,cableCode;public double length;public double? lengthMetres;}
 // role: OWNER | DEDUCTED | UNSHARED | NOT_SHARED_DECLARATION_DIFFERS | SEPARATE | NOT_ALLOCATED.  shareKey: display text of the physical item key.
 public sealed class RoomHorizontalShare037 {public string segmentId,role,ownerRowKey,shareKey;public double? lengthMetres;}
 // side: START | END. role: OWNER | DEDUCTED | UNSHARED | NO_DROP | WALL_NOT_SHARED | SEPARATE | NOT_ALLOCATED.
 public sealed class RoomVerticalShare037 {public string side,deviceId,role,ownerRowKey,shareKey;public double? dropMetres;}
 // origin: NETWORK | ASSOCIATE | SELECTION. severity: core severity (NEEDS_INPUT | CHECK | BLOCKED | INFO).
 public sealed class RoomCompletionBlocker037 {public string origin,severity,code,subject,message;
  public List<string> segmentIds=new List<string>(),relatedIds=new List<string>();
  public override string ToString(){return origin+" "+severity+" "+code+" ["+subject+"] "+message;}}
 public sealed class RoomCompletionRow037 {
  // Derived, read-only (reset by Calculate)
  public string rowKey,sourceKind,sourceId,sourceStatus,roomId,units;
  public string startId,startType,startName,startLabel,endId,endType,endName,endLabel;
  public string channelKey,channelLabel,structuredKey;
  public List<string> sourceReasons=new List<string>();
  public List<RoomCompletionSegment037> segments=new List<RoomCompletionSegment037>();   // traversal order start -> end
  public double drawingLength;public double? planMetres;
  public bool excluded;public string exclusionReason;                                   // S5A: applied exclusion (status EXCLUDED)
  // Mutable declarations (editable fields only, see header) and computed results
  public BdgklInput034 input;
  public BdgklResult034 result;
  public string status;
  public List<string> missing=new List<string>(),issues=new List<string>();
  public List<RoomHorizontalShare037> horizontalShares=new List<RoomHorizontalShare037>();
  public List<RoomVerticalShare037> verticalShares=new List<RoomVerticalShare037>();
  public List<string> ownerRowKeys=new List<string>();
  public double sharedHorizontalM,sharedVerticalM;
 }
 // kind WIRE: rawM / afterFactorM = metres of the declared assembly (route metres, not core metres).
 // kind CONDUIT: horizontalM (W) / verticalM (X), hidden/exposed raw and after factor (template classification).
 public sealed class RoomCompletionTotal037 {public string kind,floorId,code;public int rows;
  public double rawM,afterFactorM,horizontalM,verticalM,hiddenRawM,exposedRawM,hiddenAfterFactorM,exposedAfterFactorM;}
 public sealed class RoomCompletionBatch037 {
  public RoomInput037 source;
  public bool independentAssembliesConfirmed;
  public string conduitMode;                                   // SEPARATE | SHARED, required
  public List<string> externalBlockers=new List<string>();     // native/UI blockers supplied by the caller; never hidden
  public List<RoomCompletionRow037> rows=new List<RoomCompletionRow037>();
  public RoomAdjustmentsData037 adjustments;                   // S5A optional declarations; null = none
  public string sourceSignature;                               // S5A: signature of the scan that produced source (assigned by the caller)
  // Computed by Calculate
  public List<RoomCompletionBlocker037> blockers=new List<RoomCompletionBlocker037>();
  public List<string> notes=new List<string>(),issues=new List<string>(),missing=new List<string>();
  public List<RoomCompletionTotal037> totals=new List<RoomCompletionTotal037>();   // measured (CAD) active PASS rows only
  public bool totalsOfficial;                                  // true only when status == PASS
  public string status;
  public int pass,missingData,check;
  public int excluded;                                         // S5A: rows with an applied exclusion
  public List<RoomExclusionState037> exclusionStates=new List<RoomExclusionState037>();
  public List<RoomSupplementState037> supplementStates=new List<RoomSupplementState037>();
  public List<RoomManualTotal037> manualTotals=new List<RoomManualTotal037>();         // MANUAL, PASS active supplements only
  public List<RoomCombinedTotal037> combinedTotals=new List<RoomCombinedTotal037>();   // CAD + MANUAL per exact kind/code/unit
 }

 public static class RoomCompletion037 {
  public const string Pass="PASS",MissingData="MISSING_DATA",Check="CHECK",Excluded="EXCLUDED";
  public const string Separate="SEPARATE",Shared="SHARED";
  public const string KindPath="NETWORK_PATH",KindSection="NETWORK_SECTION",KindChain="CHAIN";
  public const string RoleOwner="OWNER",RoleDeducted="DEDUCTED",RoleUnshared="UNSHARED",RoleNotShared="NOT_SHARED_DECLARATION_DIFFERS",
   RoleSeparate="SEPARATE",RoleNotAllocated="NOT_ALLOCATED",RoleNoDrop="NO_DROP",RoleWall="WALL_NOT_SHARED";
  public const int MaxRows=2000;
  const char US='\u001f';
  static readonly StringComparer O=StringComparer.Ordinal;
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;

  static bool Blank(string s){return string.IsNullOrWhiteSpace(s);}
  static string Num(double? v){return v.HasValue?v.Value.ToString("R",Inv):"null";}
  static void AddOnce(List<string> l,string v){if(!l.Contains(v))l.Add(v);}

  public static RoomCompletionBatch037 Create(RoomInput037 input){var b=new RoomCompletionBatch037{source=input};Calculate(b);return b;}

  public static void Calculate(RoomCompletionBatch037 batch){
   if(batch==null)throw new ArgumentNullException("batch");
   try{Run(batch);}
   catch(Exception ex){
    if(batch.issues==null)batch.issues=new List<string>();
    if(batch.totals==null)batch.totals=new List<RoomCompletionTotal037>();
    batch.issues.Add("INTERNAL_ERROR: "+ex.GetType().Name+": "+ex.Message);
    batch.totals.Clear();batch.totalsOfficial=false;batch.status=Check;
    batch.manualTotals=new List<RoomManualTotal037>();batch.combinedTotals=new List<RoomCombinedTotal037>();}}

  sealed class Extra {public RoomCompletionRow037 row;public string reason;}

  static void Run(RoomCompletionBatch037 b){
   if(b.rows==null)b.rows=new List<RoomCompletionRow037>();
   if(b.externalBlockers==null)b.externalBlockers=new List<string>();
   b.blockers=new List<RoomCompletionBlocker037>();b.notes=new List<string>();b.issues=new List<string>();b.missing=new List<string>();
   b.totals=new List<RoomCompletionTotal037>();b.totalsOfficial=false;b.status=Check;b.pass=b.missingData=b.check=0;
   b.excluded=0;b.exclusionStates=new List<RoomExclusionState037>();b.supplementStates=new List<RoomSupplementState037>();
   b.manualTotals=new List<RoomManualTotal037>();b.combinedTotals=new List<RoomCombinedTotal037>();
   var net=RoomQty037.ResolveControlPaths(b.source);var res=RoomQty037.Associate(b.source);
   var derived=Select(b,net,res);
   var extras=new List<Extra>();
   var rows=Merge(b,derived,extras);
   foreach(var r in b.rows)Reset(r);
   foreach(var x in extras){x.row.issues.Add(x.reason);
    if(x.row.input!=null){x.row.input.sharedHorizontal=0.0;x.row.input.sharedVertical=0.0;x.row.input.sharedOwnerKey=null;}
    x.row.result=Bdgkl034.Calculate(x.row.input);}
   // S5A: the complete source is already selected and merged; valid exclusions only remove rows from the ACTIVE set used below.
   string adjProblem=b.adjustments==null?null:RoomAdjustments037.Problem(b.adjustments);
   if(adjProblem!=null)b.issues.Add("ADJUSTMENTS_INVALID: "+adjProblem+" No exclusion or supplement is applied.");
   if(b.adjustments!=null&&adjProblem==null){
    var off=RoomAdjustments037.Exclusions(b,rows);
    if(off.Count>0){
     foreach(var r in rows){string why;if(off.TryGetValue(r,out why))ExcludeRow(b,r,why);}
     rows=rows.Where(r=>!r.excluded).ToList();}}
   bool modeOk=b.conduitMode==Separate||b.conduitMode==Shared;
   // Base pass: no sharing.
   foreach(var r in rows){var d=r.input;d.sharedHorizontal=0.0;d.sharedVertical=0.0;d.sharedOwnerKey=null;r.result=Bdgkl034.Calculate(d);}
   foreach(var r in rows){
    if(r.sourceStatus!=RoomQty037.Associated){string why=r.sourceReasons.Count==0?"no reason given":string.Join("; ",r.sourceReasons);
     if(r.sourceStatus==RoomQty037.Check)r.issues.Add("SOURCE_NOT_ASSOCIATED: core status CHECK ("+why+"); numeric input cannot make this row PASS.");
     else r.missing.Add("SOURCE_NOT_ASSOCIATED: core status "+(r.sourceStatus??"(null)")+" ("+why+"); resolve in CAD, numeric input cannot make this row PASS.");}
    if(string.IsNullOrEmpty(b.conduitMode))r.missing.Add("CONDUIT_MODE_REQUIRED: declare conduitMode SEPARATE or SHARED; no implicit choice.");
    else if(!modeOk)r.issues.Add("CONDUIT_MODE_INVALID: conduitMode \""+b.conduitMode+"\" is not SEPARATE or SHARED (exact).");
    if(!b.independentAssembliesConfirmed)r.missing.Add("INDEPENDENT_ASSEMBLIES_NOT_CONFIRMED: confirm that each logical row is one independently declared wire assembly.");}
   Consistency(rows);
   var elig=new HashSet<RoomCompletionRow037>(rows.Where(r=>r.result.status==Bdgkl034.Ok&&r.missing.Count==0&&r.issues.Count==0));
   Allocate(b,rows,elig);
   foreach(var r in rows){if(!elig.Contains(r)||(r.sharedHorizontalM==0&&r.sharedVerticalM==0))continue;
    var d=r.input;d.sharedHorizontal=r.sharedHorizontalM;d.sharedVertical=r.sharedVerticalM;d.sharedOwnerKey=r.ownerRowKeys[0];
    r.result=Bdgkl034.Calculate(d);}
   foreach(var r in b.rows){
    if(r.excluded)continue;
    foreach(var i in r.result.issues){var t=i.code+" ["+i.field+"] "+i.message;if(i.status==Bdgkl034.Check)r.issues.Add(t);else r.missing.Add(t);}
    r.status=r.issues.Count>0?Check:r.missing.Count>0?MissingData:Pass;}
   // Every real owner must be counted; otherwise its dependents cannot keep their deductions (no undercount).
   var byKey=new Dictionary<string,RoomCompletionRow037>(O);foreach(var r in rows)byKey[r.rowKey]=r;
   for(bool changed=true;changed;){changed=false;
    foreach(var r in rows){if(r.status!=Pass)continue;
     foreach(var k in r.ownerRowKeys){RoomCompletionRow037 o;
      if(byKey.TryGetValue(k,out o)&&o.status==Pass)continue;
      r.issues.Add("SHARED_OWNER_NOT_COUNTED: owner row "+k+" of a deducted shared item is "+(o==null?"missing":o.status)+"; deduction cannot be relied on.");
      r.status=Check;changed=true;break;}}}
   // Batch
   if(b.blockers.Count>0)b.issues.Add("SOURCE_BLOCKERS: "+b.blockers.Count.ToString(Inv)+" source blocker(s) must be resolved in CAD.");
   if(b.externalBlockers.Count>0)b.issues.Add("EXTERNAL_BLOCKERS: "+string.Join(" | ",b.externalBlockers.Select(a=>a??"(null)")));
   if(rows.Count==0){
    if(b.rows.Any(r=>r.excluded))b.issues.Add("ALL_ROWS_EXCLUDED: every measured row is excluded; a room without an active measured row cannot PASS.");
    else b.issues.Add("EMPTY_BATCH: no candidate row was selected; an empty batch cannot PASS.");}
   if(b.rows.Count>MaxRows)b.issues.Add("ROW_LIMIT: "+b.rows.Count.ToString(Inv)+" rows exceed the supported "+MaxRows.ToString(Inv)+".");
   if(string.IsNullOrEmpty(b.conduitMode))b.missing.Add("CONDUIT_MODE_REQUIRED");else if(!modeOk)b.issues.Add("CONDUIT_MODE_INVALID: \""+b.conduitMode+"\"");
   if(!b.independentAssembliesConfirmed)b.missing.Add("INDEPENDENT_ASSEMBLIES_NOT_CONFIRMED");
   foreach(var r in b.rows){if(r.excluded){b.excluded++;continue;}if(r.status==Pass)b.pass++;else if(r.status==Check)b.check++;else b.missingData++;}
   Totals(b,rows);
   RoomAdjustments037.Supplements(b,rows,adjProblem==null);
   b.status=b.issues.Count>0||b.check>0?Check:b.missing.Count>0||b.missingData>0?MissingData:Pass;
   b.totalsOfficial=b.status==Pass;}

  // S5A: an excluded row keeps its input (declarations) but has no quantity. Its source status still blocks when not ASSOCIATED.
  static void ExcludeRow(RoomCompletionBatch037 b,RoomCompletionRow037 r,string reason){
   r.excluded=true;r.exclusionReason=reason;r.status=Excluded;r.result=null;
   if(r.sourceStatus==RoomQty037.Associated)return;
   string why=r.sourceReasons.Count==0?"no reason given":string.Join("; ",r.sourceReasons);
   string t="EXCLUDED_SOURCE_NOT_ASSOCIATED: excluded row "+r.rowKey+" has core status "+(r.sourceStatus??"(null)")+" ("+why+
    "); excluding a row does not resolve its source, fix it in CAD.";
   if(r.sourceStatus==RoomQty037.Check){r.issues.Add(t);b.issues.Add(t);}
   else{r.missing.Add(t);b.missing.Add(t);}}
  static void Reset(RoomCompletionRow037 r){r.excluded=false;r.exclusionReason=null;r.missing=new List<string>();r.issues=new List<string>();r.horizontalShares=new List<RoomHorizontalShare037>();
   r.verticalShares=new List<RoomVerticalShare037>();r.ownerRowKeys=new List<string>();r.sharedHorizontalM=0;r.sharedVerticalM=0;r.result=null;r.status=Check;
   if(r.sourceReasons==null)r.sourceReasons=new List<string>();if(r.segments==null)r.segments=new List<RoomCompletionSegment037>();}

  // ---- Candidate selection ----
  static RoomCompletionBlocker037 Block(RoomCompletionBatch037 b,string origin,string sev,string code,string subject,string msg){
   var x=new RoomCompletionBlocker037{origin=origin,severity=sev,code=code,subject=subject,message=msg};b.blockers.Add(x);return x;}
  static RoomCompletionRow037 NewRow(string kind,string id,string status,RoomInput037 src){
   return new RoomCompletionRow037{sourceKind=kind,sourceId=id,sourceStatus=status,roomId=src==null?null:src.roomId,units=src==null?null:src.units};}
  static void Ends(RoomCompletionRow037 r,Dictionary<string,RoomDevice037> devs,string a,string z,string la,string lz){RoomDevice037 d;
   r.startId=a;r.startLabel=la;if(a!=null&&devs.TryGetValue(a,out d)){r.startType=d.type;r.startName=d.name;}
   r.endId=z;r.endLabel=lz;if(z!=null&&devs.TryGetValue(z,out d)){r.endType=d.type;r.endName=d.name;}}
  static void AddSegment(RoomCompletionRow037 r,string id,string cable,double len,double? upm){
   r.segments.Add(new RoomCompletionSegment037{id=id,cableCode=cable,length=len,lengthMetres=upm.HasValue?(double?)(len/upm.Value):null});}

  static List<RoomCompletionRow037> Select(RoomCompletionBatch037 b,RoomNetwork037 net,RoomResult037 res){
   var src=b.source;var outRows=new List<RoomCompletionRow037>();
   double? upm=Bdgkl034.UnitsPerMetre(src==null?null:src.units);
   var devs=new Dictionary<string,RoomDevice037>(O);
   if(src!=null&&src.devices!=null)foreach(var d in src.devices)if(d!=null&&!Blank(d.id)&&!devs.ContainsKey(d.id))devs[d.id]=d;
   var phys=new Dictionary<string,RoomPhysicalSegment037>(O);foreach(var s in net.physicalSegments)phys[s.id]=s;
   var labels=RoomQty037.DisplayLabels(src);
   Func<string,string,string> lab=(devId,fb)=>{string lv;return devId!=null&&labels.TryGetValue(devId,out lv)?lv:fb??devId;};
   var byEnds=new Dictionary<string,List<RoomLogicalPath037>>(O);
   foreach(var p in net.logicalPaths){List<RoomLogicalPath037> l;string k=p.fromId+US+p.toId;if(!byEnds.TryGetValue(k,out l))byEnds[k]=l=new List<RoomLogicalPath037>();l.Add(p);}
   var selected=new HashSet<string>(O);
   foreach(var p in net.logicalPaths){string kind=KindPath,key=p.pathId,status=p.status,fromId=p.fromId,fromLabel=p.fromLabel,extra=null,unproven=null;
    IEnumerable<string> segIds=p.segmentIds;
    if(p.intermediateDeviceIds.Count>0){
     if(p.status!=RoomQty037.Associated)
      extra="SECTION_NOT_PROJECTED: path passes device(s) "+string.Join(",",p.intermediateDeviceIds)+" but is not ASSOCIATED; the full logical path is shown until its association is resolved.";
     else{var u=Upstream(p,byEnds,devs,out unproven);
      if(u==null){status=RoomQty037.Check;
       extra="SECTION_UPSTREAM_UNPROVEN: "+unproven+"; the measured section cannot start at an upstream device, full path kept and blocked (no length or endpoint is fabricated).";
       var bl=Block(b,"SELECTION",RoomQty037.Check,"SECTION_UPSTREAM_UNPROVEN","row:"+p.pathId,"Logical path "+p.pathId+" passes device(s) "+
        string.Join(",",p.intermediateDeviceIds)+": "+unproven+". Its measured section is not projected; resolve the TAG/wiring in CAD.");
       bl.relatedIds.AddRange(p.intermediateDeviceIds);bl.relatedIds.Add(p.fromId);bl.relatedIds.Add(p.toId);}
      else{kind=KindSection;fromId=u.toId;fromLabel=u.toLabel;key=p.pathId+"|SEC:"+fromId+"->"+p.toId;segIds=p.segmentIds.Skip(u.segmentIds.Count);
       b.notes.Add("NETWORK_SECTION: row "+key+" measures "+fromId+" -> "+p.toId+", the part of logical path "+p.pathId+" after its last upstream device "+
        fromId+"; the upstream part is measured by the row(s) of "+u.pathId+" (no metre counted twice).");}}}
    var r=NewRow(kind,p.pathId,status,src);r.rowKey=key;
    Ends(r,devs,fromId,p.toId,lab(fromId,fromLabel),lab(p.toId,p.toLabel));r.channelKey=p.controlKey;r.channelLabel=p.controlLabel;r.structuredKey=p.structuredKey;
    r.sourceReasons.AddRange(p.diagnostics);foreach(var m in p.missing)r.sourceReasons.Add("missing:"+m);if(extra!=null)r.sourceReasons.Add(extra);
    foreach(var id in segIds){var s=phys[id];AddSegment(r,id,s.cableCode,s.length,upm);selected.Add(id);}
    outRows.Add(r);}
   var covered=new HashSet<string>(selected,O);
   foreach(var w in res.rows){var ids=w.SegmentIds();int inC=ids.Count(covered.Contains);
    if(inC==ids.Count){b.notes.Add("CHAIN_COVERED_BY_NETWORK: Associate row "+w.rowKey+" lies on resolved network paths; not added again.");continue;}
    if(inC>0){var bl=Block(b,"SELECTION",RoomQty037.Blocked,"AMBIGUOUS_CANDIDATE_OVERLAP","row:"+w.rowKey,
      "Associate row "+w.rowKey+" partly overlaps network path segments; candidate sets are ambiguous, no row created.");
     bl.segmentIds.AddRange(ids.Where(x=>!covered.Contains(x)).OrderBy(x=>x,O));bl.relatedIds.Add(w.fromId);bl.relatedIds.Add(w.toId);continue;}
    var r=NewRow(KindChain,w.rowKey,w.status,src);r.rowKey="C:"+w.fromId+"->"+w.toId+"#"+ids.OrderBy(x=>x,O).First();
    Ends(r,devs,w.fromId,w.toId,lab(w.fromId,w.fromLabel),lab(w.toId,w.toLabel));r.channelKey=w.controlKey;r.channelLabel=w.controlLabel;r.structuredKey=w.structuredKey;
    r.sourceReasons.AddRange(w.diagnostics);foreach(var m in w.missing)r.sourceReasons.Add("missing:"+m);
    foreach(var s in w.segments){AddSegment(r,s.id,s.cableCode,s.length,upm);selected.Add(s.id);}
    outRows.Add(r);}
   var seen=new HashSet<string>(O);
   foreach(var pair in net.diagnostics.Select(q=>new{o="NETWORK",d=q}).Concat(res.diagnostics.Select(q=>new{o="ASSOCIATE",d=q}))){var d=pair.d;
    if(!seen.Add(d.code+US+d.subject+US+string.Join(",",d.segmentIds)+US+string.Join(",",d.relatedIds)+US+d.message))continue;
    if(d.severity==RoomQty037.Info&&d.code!="DEVICE_NO_SEGMENT"){b.notes.Add(pair.o+" "+d);continue;}
    if(d.segmentIds.Count>0&&d.segmentIds.All(selected.Contains)){b.notes.Add("RESOLVED_BY_SELECTION: "+pair.o+" "+d);continue;}
    var x=Block(b,pair.o,d.severity,d.code,d.subject,d.message);x.segmentIds.AddRange(d.segmentIds);x.relatedIds.AddRange(d.relatedIds);}
   var blocked=new HashSet<string>(b.blockers.SelectMany(a=>a.segmentIds),O);
   foreach(var id in net.acceptedSegmentIds)if(!selected.Contains(id)&&!blocked.Contains(id))
    Block(b,"SELECTION",RoomQty037.Blocked,"SEGMENT_UNACCOUNTED","segment:"+id,"Accepted segment "+id+" is used by no row and owned by no blocker.").segmentIds.Add(id);
   var keys=new HashSet<string>(O);var list=new List<RoomCompletionRow037>();
   foreach(var r in outRows){if(keys.Add(r.rowKey)){list.Add(r);continue;}
    var x=Block(b,"SELECTION",RoomQty037.Blocked,"DUPLICATE_CANDIDATE_KEY","row:"+r.rowKey,"Two candidates produced row key "+r.rowKey+"; the second is not used.");
    x.segmentIds.AddRange(r.segments.Select(s=>s.id));}
   list.Sort((x,y)=>string.CompareOrdinal(x.rowKey,y.rowKey));
   return list;}

  // S6: logical path S->B of the LAST intermediate device B of the ASSOCIATED path p (S->T), returned only when B is proven to be p's predecessor on
  // the actual traced path: B is a listed LAMP/FAN, exactly one logical path S->B exists, it is ASSOCIATED on the same control key, and its ordered
  // segment IDs, node IDs and intermediate devices are an exact proper prefix of p's (so p's remaining segments run B -> T with no other device),
  // and, when S->B itself passes devices, its own projection is proven recursively (whole upstream chain). Otherwise null + the reason.
  // Uses only RoomQty037 traversal order; no geometry, distance or nearest-device rule.
  static RoomLogicalPath037 Upstream(RoomLogicalPath037 p,Dictionary<string,List<RoomLogicalPath037>> byEnds,Dictionary<string,RoomDevice037> devs,out string problem){
   problem=null;string bId=p.intermediateDeviceIds[p.intermediateDeviceIds.Count-1];RoomDevice037 bd;List<RoomLogicalPath037> l;
   if(!devs.TryGetValue(bId,out bd)||(bd.type!=RoomQty037.Lamp&&bd.type!=RoomQty037.Fan)){problem="upstream device "+bId+" is not a listed LAMP/FAN";return null;}
   if(!byEnds.TryGetValue(p.fromId+US+bId,out l)||l.Count!=1){problem="no unique logical path "+p.fromId+" -> "+bId+" to the upstream device";return null;}
   var u=l[0];
   if(u.status!=RoomQty037.Associated){problem="logical path "+u.pathId+" to upstream device "+bId+" is "+(u.status??"(null)")+" ("+string.Join("; ",u.diagnostics)+")";return null;}
   if(u.controlKey==null||!O.Equals(u.controlKey,p.controlKey)){problem="upstream path "+u.pathId+" control key "+(u.controlKey??"(none)")+" differs from "+(p.controlKey??"(none)");return null;}
   int n=u.segmentIds.Count;
   if(n==0||n>=p.segmentIds.Count||!u.segmentIds.SequenceEqual(p.segmentIds.Take(n),O)){problem="segments of "+u.pathId+" are not an exact proper prefix of "+p.pathId;return null;}
   if(u.nodeIds.Count!=n+1||p.nodeIds.Count!=p.segmentIds.Count+1||!u.nodeIds.SequenceEqual(p.nodeIds.Take(n+1),O)){problem="node order of "+u.pathId+" does not match "+p.pathId;return null;}
   if(!u.intermediateDeviceIds.SequenceEqual(p.intermediateDeviceIds.Take(p.intermediateDeviceIds.Count-1),O)){problem="intermediate devices of "+u.pathId+" do not match "+p.pathId;return null;}
   // S6_PATCH_01 (NF-S6-005): u's own measured projection must be proven too (u is projected by Select exactly when this check succeeds), else
   // p's section would rest on an unresolved predecessor. Terminates: u.segmentIds is a strict shorter prefix (checked above), so depth <= |segments|.
   if(u.intermediateDeviceIds.Count>0){string inner;
    if(Upstream(u,byEnds,devs,out inner)==null){problem="measured projection of upstream path "+u.pathId+" is unproven ("+inner+")";return null;}}
   return u;}

  // ---- Merge user rows (declarations) with the freshly derived candidates ----
  static List<RoomCompletionRow037> Merge(RoomCompletionBatch037 b,List<RoomCompletionRow037> derived,List<Extra> extras){
   var old=new Dictionary<string,RoomCompletionRow037>(O);var refs=new HashSet<RoomCompletionRow037>();var kept=new List<RoomCompletionRow037>();
   foreach(var r in b.rows){
    if(r==null){b.issues.Add("NULL_ROW: a null row entry was removed from the batch.");continue;}
    if(!refs.Add(r)){b.issues.Add("DUPLICATE_ROW_REFERENCE: the same row object was listed twice; the repeat was removed.");continue;}
    kept.Add(r);if(!string.IsNullOrEmpty(r.rowKey)&&!old.ContainsKey(r.rowKey))old[r.rowKey]=r;}
   var used=new HashSet<RoomCompletionRow037>();var list=new List<RoomCompletionRow037>();
   foreach(var t in derived){RoomCompletionRow037 r;
    if(old.TryGetValue(t.rowKey,out r)){used.Add(r);CopyDerived(t,r);if(r.input==null){r.input=new BdgklInput034();Prefill(r);}}
    else{r=t;r.input=new BdgklInput034();Prefill(r);}
    SetDerivedInput(r);list.Add(r);}
   foreach(var r in kept){if(used.Contains(r))continue;
    string why=string.IsNullOrEmpty(r.rowKey)?"ROW_KEY_MISSING: row without key is not part of the source selection; not counted."
     :old[r.rowKey]!=r?"DUPLICATE_ROW_KEY: row key "+r.rowKey+" is listed more than once; the repeat is not counted."
     :"ROW_NOT_IN_SOURCE: row "+r.rowKey+" is not produced by the current room graph; not counted (remove it or recalculate in CAD).";
    extras.Add(new Extra{row=r,reason=why});}
   b.rows=list.Concat(extras.Select(x=>x.row)).ToList();
   return list;}
  static void CopyDerived(RoomCompletionRow037 t,RoomCompletionRow037 r){
   r.sourceKind=t.sourceKind;r.sourceId=t.sourceId;r.sourceStatus=t.sourceStatus;r.roomId=t.roomId;r.units=t.units;
   r.startId=t.startId;r.startType=t.startType;r.startName=t.startName;r.startLabel=t.startLabel;
   r.endId=t.endId;r.endType=t.endType;r.endName=t.endName;r.endLabel=t.endLabel;
   r.channelKey=t.channelKey;r.channelLabel=t.channelLabel;r.structuredKey=t.structuredKey;
   r.sourceReasons=new List<string>(t.sourceReasons);r.segments=t.segments.Select(s=>new RoomCompletionSegment037{id=s.id,cableCode=s.cableCode,length=s.length,lengthMetres=s.lengthMetres}).ToList();}
  // Only circuitId is prefilled (structured panel/circuit wins over the room-local TAG label). Nothing else gets a value.
  static void Prefill(RoomCompletionRow037 r){r.input.circuitId=!Blank(r.structuredKey)?r.structuredKey:r.channelLabel;}
  static void SetDerivedInput(RoomCompletionRow037 r){var d=r.input;double sum=0;foreach(var s in r.segments)sum+=s.length;
   r.drawingLength=sum;double? upm=Bdgkl034.UnitsPerMetre(r.units);r.planMetres=upm.HasValue&&r.segments.Count>0?(double?)(sum/upm.Value):null;
   d.routeKey=r.rowKey;d.areaId=r.roomId;d.startKey=r.startId;d.endKey=r.endId;d.startLabel=r.startLabel;d.endLabel=r.endLabel;
   d.nativeLength=r.segments.Count>0?(double?)sum:null;d.declaredUnit=r.units;}

  // ---- Consistency of physical declarations ----
  sealed class End {public RoomCompletionRow037 row;public string dev,floor,mode,sig;}
  static string Sig(string mode,double? h,double? o){return (mode??"(null)")+" fixed="+Num(h)+" offset="+Num(o);}
  static void Consistency(List<RoomCompletionRow037> rows){var ends=new List<End>();
   foreach(var r in rows){var d=r.input;
    if(!Blank(r.startId))ends.Add(new End{row=r,dev=r.startId,floor=d.floorId,mode=d.startHeightMode,sig=Sig(d.startHeightMode,d.startFixedHeight,d.startCeilingOffset)});
    if(!Blank(r.endId))ends.Add(new End{row=r,dev=r.endId,floor=d.floorId,mode=d.endHeightMode,sig=Sig(d.endHeightMode,d.endFixedHeight,d.endCeilingOffset)});}
   foreach(var g in ends.Where(a=>!string.IsNullOrEmpty(a.floor)).GroupBy(a=>a.dev,O)){var fl=g.Select(a=>a.floor).Distinct(O).OrderBy(a=>a,O).ToList();
    if(fl.Count>1)foreach(var e in g)AddOnce(e.row.issues,"DEVICE_FLOOR_CONFLICT: device "+g.Key+" is declared on floors "+string.Join(", ",fl)+" in different rows; one device has one floor.");}
   foreach(var g in ends.Where(a=>!string.IsNullOrEmpty(a.floor)&&!string.IsNullOrEmpty(a.mode)).GroupBy(a=>a.dev+US+a.floor,O)){
    var sg=g.Select(a=>a.sig).Distinct(O).OrderBy(a=>a,O).ToList();
    if(sg.Count>1)foreach(var e in g)AddOnce(e.row.issues,"DEVICE_HEIGHT_CONFLICT: device "+e.dev+" on floor "+e.floor+" has different height declarations ("+string.Join(" | ",sg)+"); one device has one height.");}
   foreach(var g in rows.Where(a=>!string.IsNullOrEmpty(a.input.floorId)&&a.input.ceilingHeight.HasValue).GroupBy(a=>a.input.floorId,O)){
    var v=g.Select(a=>Num(a.input.ceilingHeight)).Distinct(O).OrderBy(a=>a,O).ToList();
    if(v.Count>1)foreach(var r in g)AddOnce(r.issues,"FLOOR_CEILING_CONFLICT: floor "+g.Key+" has different ceiling heights ("+string.Join(", ",v)+").");}}

  // ---- Physical allocation ----
  static string HKey(string seg,BdgklInput034 d){return seg+US+d.floorId+US+d.areaId+US+d.routingMode+US+d.conduitCode;}
  static string Show(string key){return key.Replace(US,'|');}
  sealed class Drop {public RoomCompletionRow037 row;public string side,dev,key;public double drop;}
  static void AddOwner(RoomCompletionRow037 r,string k){if(!r.ownerRowKeys.Contains(k))r.ownerRowKeys.Add(k);}
  static void Allocate(RoomCompletionBatch037 b,List<RoomCompletionRow037> rows,HashSet<RoomCompletionRow037> elig){
   if(b.conduitMode!=Shared){
    foreach(var r in rows){string role=elig.Contains(r)&&b.conduitMode==Separate?RoleSeparate:RoleNotAllocated;
     foreach(var s in r.segments)r.horizontalShares.Add(new RoomHorizontalShare037{segmentId=s.id,role=role,lengthMetres=s.lengthMetres});
     r.verticalShares.Add(new RoomVerticalShare037{side="START",deviceId=r.startId,role=role,dropMetres=StartDrop(r)});
     r.verticalShares.Add(new RoomVerticalShare037{side="END",deviceId=r.endId,role=role,dropMetres=EndDrop(r)});}
    return;}
   var groups=new List<List<RoomCompletionRow037>>();
   // Horizontal
   var hMembers=new Dictionary<string,List<RoomCompletionRow037>>(O);var segUsers=new Dictionary<string,HashSet<RoomCompletionRow037>>(O);
   foreach(var r in rows){if(!elig.Contains(r))continue;
    foreach(var s in r.segments){List<RoomCompletionRow037> l;string k=HKey(s.id,r.input);
     if(!hMembers.TryGetValue(k,out l))hMembers[k]=l=new List<RoomCompletionRow037>();if(!l.Contains(r))l.Add(r);
     HashSet<RoomCompletionRow037> u;if(!segUsers.TryGetValue(s.id,out u))segUsers[s.id]=u=new HashSet<RoomCompletionRow037>();u.Add(r);}}
   foreach(var r in rows){bool e=elig.Contains(r);
    foreach(var s in r.segments){var h=new RoomHorizontalShare037{segmentId=s.id,lengthMetres=s.lengthMetres};r.horizontalShares.Add(h);
     if(!e){h.role=RoleNotAllocated;continue;}
     string k=HKey(s.id,r.input);var l=hMembers[k];h.shareKey=Show(k);
     if(l.Count==1){h.role=segUsers[s.id].Count>1?RoleNotShared:RoleUnshared;h.ownerRowKey=r.rowKey;}
     else if(l[0]==r){h.role=RoleOwner;h.ownerRowKey=r.rowKey;}
     else{h.role=RoleDeducted;h.ownerRowKey=l[0].rowKey;r.sharedHorizontalM+=s.length/r.result.unitsPerMetre.Value;AddOwner(r,l[0].rowKey);}}}
   foreach(var kv in hMembers.OrderBy(x=>x.Key,O))if(kv.Value.Count>1)groups.Add(kv.Value);
   // Vertical
   var vMembers=new Dictionary<string,List<Drop>>(O);var wall=new Dictionary<string,List<RoomCompletionRow037>>(O);var drops=new List<Drop>();
   foreach(var r in rows){if(!elig.Contains(r))continue;var rs=r.result;var d=r.input;
    if(rs.routingMode==Bdgkl034.Wall){if(!(rs.verticalM.Value>0))continue;
     foreach(var dev in new[]{r.startId,r.endId}){string k=dev+US+d.floorId+US+Bdgkl034.Wall+US+d.conduitCode;List<RoomCompletionRow037> l;
      if(!wall.TryGetValue(k,out l))wall[k]=l=new List<RoomCompletionRow037>();l.Add(r);}
     continue;}
    foreach(var side in new[]{"START","END"}){var dev=side=="START"?r.startId:r.endId;double drop=(side=="START"?StartDrop(r):EndDrop(r)).Value;
     var it=new Drop{row=r,side=side,dev=dev,drop=drop,key=dev+US+d.floorId+US+rs.routingMode+US+d.conduitCode};drops.Add(it);
     if(drop==0)continue;List<Drop> l;if(!vMembers.TryGetValue(it.key,out l))vMembers[it.key]=l=new List<Drop>();l.Add(it);}}
   foreach(var it in drops){var r=it.row;var v=new RoomVerticalShare037{side=it.side,deviceId=it.dev,dropMetres=it.drop,shareKey=Show(it.key)};r.verticalShares.Add(v);
    if(it.drop==0){v.role=RoleNoDrop;continue;}
    var l=vMembers[it.key];
    if(l.Count==1){v.role=RoleUnshared;v.ownerRowKey=r.rowKey;}
    else if(l[0]==it){v.role=RoleOwner;v.ownerRowKey=r.rowKey;}
    else{v.role=RoleDeducted;v.ownerRowKey=l[0].row.rowKey;r.sharedVerticalM+=it.drop;AddOwner(r,l[0].row.rowKey);}}
   foreach(var kv in vMembers.OrderBy(x=>x.Key,O))if(kv.Value.Count>1)groups.Add(kv.Value.Select(x=>x.row).Distinct().ToList());
   foreach(var kv in wall.OrderBy(x=>x.Key,O)){var l=kv.Value.Distinct().ToList();bool unproven=l.Count>1;
    foreach(var r in l){var dev=kv.Key.Substring(0,kv.Key.IndexOf(US));string side=dev==r.startId?"START":"END";
     r.verticalShares.Add(new RoomVerticalShare037{side=side,deviceId=dev,role=RoleWall,shareKey=Show(kv.Key),dropMetres=null});
     if(unproven)AddOnce(r.issues,"WALL_SHARED_VERTICAL_UNPROVEN: WALL rows "+string.Join(", ",l.Select(x=>x.rowKey))+" meet at "+Show(kv.Key)+
      " with vertical runs; overlap of their vertical intervals is not proven, so no deduction is made and the rows need review.");}}
   foreach(var l in groups){var f=l.Select(a=>Num(a.result.factor)).Distinct(O).OrderBy(a=>a,O).ToList();
    if(f.Count>1)foreach(var r in l)AddOnce(r.issues,"SHARED_FACTOR_MISMATCH: rows "+string.Join(", ",l.Select(a=>a.rowKey))+" share a physical item but declare factors "+string.Join(", ",f)+".");}
   foreach(var r in rows)r.ownerRowKeys.Sort(string.CompareOrdinal);}
  // Vertical drop of one endpoint: CEILING = ceiling - height; FLOOR = height; otherwise null.
  static double? StartDrop(RoomCompletionRow037 r){return Dropped(r.result,r.result==null?null:r.result.startHeightM);}
  static double? EndDrop(RoomCompletionRow037 r){return Dropped(r.result,r.result==null?null:r.result.endHeightM);}
  static double? Dropped(BdgklResult034 x,double? h){if(x==null||!h.HasValue)return null;
   if(x.routingMode==Bdgkl034.Ceiling)return x.ceilingHeightM.HasValue?(double?)(x.ceilingHeightM.Value-h.Value):null;
   if(x.routingMode==Bdgkl034.Floor)return h;return null;}

  // ---- Totals from PASS rows only (exact codes) ----
  static void Totals(RoomCompletionBatch037 b,List<RoomCompletionRow037> rows){var map=new Dictionary<string,RoomCompletionTotal037>(O);
   Func<string,string,string,RoomCompletionTotal037> get=(kind,floor,code)=>{RoomCompletionTotal037 tt;string k=kind+US+floor+US+code;
    if(!map.TryGetValue(k,out tt))map[k]=tt=new RoomCompletionTotal037{kind=kind,floorId=floor,code=code};return tt;};
   foreach(var r in rows){if(r.status!=Pass)continue;var d=r.input;var x=r.result;
    var w=get("WIRE",d.floorId,d.assemblyCode);w.rows++;w.rawM+=x.routeM.Value;w.afterFactorM+=x.routeAfterFactorM.Value;
    var c=get("CONDUIT",d.floorId,d.conduitCode);c.rows++;c.horizontalM+=x.horizontalConduitM.Value;c.verticalM+=x.verticalConduitM.Value;
    c.hiddenRawM+=x.conduitHiddenM.Value;c.exposedRawM+=x.conduitExposedM.Value;
    c.hiddenAfterFactorM+=x.conduitHiddenAfterFactorM.Value;c.exposedAfterFactorM+=x.conduitExposedAfterFactorM.Value;}
   b.totals=map.Values.OrderBy(t=>t.kind,O).ThenBy(t=>t.floorId,O).ThenBy(t=>t.code,O).ToList();
   foreach(var t in b.totals)foreach(var v in new[]{t.rawM,t.afterFactorM,t.horizontalM,t.verticalM,t.hiddenRawM,t.exposedRawM,t.hiddenAfterFactorM,t.exposedAfterFactorM})
    if(double.IsNaN(v)||double.IsInfinity(v)){b.issues.Add("TOTAL_OVERFLOW: "+t.kind+" "+t.floorId+" "+t.code+" total is not finite.");break;}}
 }

 // TVXD-MEPS-CIRCUIT-SORT-039 PRESENTATION ORDER ONLY (CAD TUYEN grid + measured workbook). Sorted returns a NEW list holding the same row references;
 // it never sorts or mutates b.rows, a row, its input or result (Calculate, sharing ownership and totals keep the stable ordinal rowKey order).
 // Stable order: floor (input.floorId), then circuit (input.circuitId = the declared/displayed value), then the original order of the given sequence.
 // Blank/whitespace floor or circuit sorts last within its group. Keys compare outer-trimmed (the strings themselves are untouched) with Natural:
 // runs of ASCII digits compare by numeric value (any length, no overflow; leading zeros equal, e.g. 01 = 1 keeps the original order), every other
 // character case-insensitively (ToUpperInvariant, ordinal). '-' is an ordinary separator, never a sign: 1,2,10; LD-1,LD-2,LD-10; DB1/2 before DB1/10.
 public static class RoomRowOrder037 {
  public static List<RoomCompletionRow037> Sorted(IEnumerable<RoomCompletionRow037> rows){
   if(rows==null)return new List<RoomCompletionRow037>();
   return rows.Select((r,i)=>new KeyValuePair<int,RoomCompletionRow037>(i,r)).OrderBy(x=>x.Value,new RowComparer()).ThenBy(x=>x.Key).Select(x=>x.Value).ToList();}
  public static string Floor(RoomCompletionRow037 r){return r==null||r.input==null?null:r.input.floorId;}
  public static string Circuit(RoomCompletionRow037 r){return r==null||r.input==null?null:r.input.circuitId;}
  // Floor, then circuit (0 = same group; the caller keeps the original order).
  public static int Compare(RoomCompletionRow037 a,RoomCompletionRow037 b){int c=Key(Floor(a),Floor(b));return c!=0?c:Key(Circuit(a),Circuit(b));}
  // Blank/whitespace last; otherwise Natural of the outer-trimmed text.
  public static int Key(string a,string b){bool x=string.IsNullOrWhiteSpace(a),y=string.IsNullOrWhiteSpace(b);
   if(x||y)return x==y?0:x?1:-1;return Natural(a.Trim(),b.Trim());}
  public static int Natural(string a,string b){
   if(a==null||b==null)return a==null?(b==null?0:-1):1;
   int i=0,j=0;
   while(i<a.Length&&j<b.Length){char p=a[i],q=b[j];
    if(Digit(p)&&Digit(q)){
     int si=i,sj=j;while(i<a.Length&&Digit(a[i]))i++;while(j<b.Length&&Digit(b[j]))j++;
     while(si<i-1&&a[si]=='0')si++;while(sj<j-1&&b[sj]=='0')sj++;   // significant digits (a run of zeros keeps one)
     int la=i-si,lb=j-sj;if(la!=lb)return la<lb?-1:1;
     for(int k=0;k<la;k++)if(a[si+k]!=b[sj+k])return a[si+k]<b[sj+k]?-1:1;
     continue;}
    char u=char.ToUpperInvariant(p),v=char.ToUpperInvariant(q);if(u!=v)return u<v?-1:1;
    i++;j++;}
   bool ea=i>=a.Length,eb=j>=b.Length;return ea==eb?0:ea?-1:1;}
  static bool Digit(char c){return c>='0'&&c<='9';}
  sealed class RowComparer:IComparer<RoomCompletionRow037> {public int Compare(RoomCompletionRow037 a,RoomCompletionRow037 b){return RoomRowOrder037.Compare(a,b);}}
 }
}
