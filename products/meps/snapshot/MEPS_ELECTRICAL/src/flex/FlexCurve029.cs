using System;using System.Collections.Generic;using System.Globalization;
using Autodesk.AutoCAD.DatabaseServices;using Autodesk.AutoCAD.Geometry;
using DbPolyline=Autodesk.AutoCAD.DatabaseServices.Polyline;
namespace Tvxd.Meps {
 // TVXD-MEPS-FLEXUI-029 native geometry PILOT (not integrated; no command, no palette, no XData, no database write).
 // Build(source, D, fromEnd) returns the flex body outline for an existing centerline, read-only: only Curve queries are made (native arc length,
 // WCS points, first/second derivatives, LWPOLYLINE segment queries). The source is never converted, exploded, filleted or modified.
 // Supported: LINE, open LWPOLYLINE (straight, sharp-cornered, bulged), ARC, SPLINE, horizontal (LWPOLYLINE/ARC normal +Z or -Z; LINE/SPLINE equal Z).
 // Body rhythm = FlexGeometry022 (port of "Ong Noi Mem.lsp", ssg 2008, cadviet.com) generalised from P(x,s) = O + u·x + v·s·D/2 to stations:
 //   Q(k,s) = c_k + s·w_k   (c_k = WCS point of station k on the centerline, w_k = half-section vector, s = ±1), same 5n+3 vertices and bulges.
 // Stations (forward along the curve):
 //   - path start/end, a normal section (w = left normal × D/2);
 //   - every LWPOLYLINE vertex between two non-zero segments (exact break distance). A tangent jump of θ gives a MITER section on the bisector of the
 //     two left normals, w = m·(D/2)/cos(θ/2). SCHEMATIC display only: no fillet, no bend radius is asserted, the centerline is unchanged;
 //   - interior stations evenly spaced in each free span: m = floor(4F/D) intervals (as 022), F = segment length minus the corner keep-outs
 //     e = (D/2)·tan(θ/2) at its ends (the miter's inner point lies e before/after the corner; a neighbour section closer than that would cross it).
 // LINE and a LWPOLYLINE with exactly one non-zero straight segment use FlexGeometry022.Build itself (old rhythm oracle, LinearOracle = true).
 // Length = native Curve length (GetDistanceAtParameter(EndParam) - (StartParam)), never the body. fromEnd reverses the stations and negates w.
 // Refusals (FlexCurveException029.Code; nothing is written anywhere): NULL, ERASED, DIAMETER, UNSUPPORTED, CLOSED, NONPLANAR, ZERO_LENGTH, CHECK
 // (022 too short / too many cells), TOO_MANY, REVERSAL (tangent turns back ~180°, miter undefined), CORNER_FOLD (segment shorter than the keep-outs of
 // its corners), CURVATURE (local radius <= D/2: the inner wall would fold; geometric, NOT an engineering bend radius), CUSP (zero derivative or
 // sampled tangent turns more than 90° between stations), FOLD (generic check: a wall point does not advance between two stations), NONFINITE, INVALID.
 // LIMITS (stated, not hidden): spline curvature/cusp checks are SAMPLED at stations and interval midpoints only; a spike between samples can pass.
 // Spline C0 kinks are not mitered. Arc-segment keep-outs are measured along the arc (approximation). No global self-intersection or near-pass test
 // (route crossing itself or two parts closer than D are drawn overlapping). Walls between stations are chords + the source's -B bulge arcs
 // (straight chords on intervals touching a miter station, F029-GEOM-01): display-only deviation from a true offset. Polyline vertex-side tangents are evaluated at parameter ±ParamStep inside the adjacent segment.
 public sealed class FlexShape029 {public double[] X,Y,Bulges;public double Length,Elevation;public int Cells;public string SourceType;public int Miters;public bool LinearOracle;}
 public sealed class FlexCurveException029:Exception {public readonly string Code;public FlexCurveException029(string code,string message):base(message){Code=code;}}
 public static class FlexCurve029 {
  public const double ParamStep=1e-7;
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  struct Station {public double s,cx,cy,wx,wy;public bool miter;}
  sealed class Seg {public int k;public double d0,d1,p0,p1,radius;public bool arc;}
  static FlexCurveException029 Fail(string code,string msg){return new FlexCurveException029(code,msg);}
  static string F(double x){return x.ToString("0.###",Inv);}
  public static FlexShape029 Build(Curve source,double diameter,bool fromEnd){
   try{return Run(source,diameter,fromEnd);}
   catch(FlexCurveException029){throw;}
   catch(ArgumentException ex){throw Fail("CHECK",ex.Message);}
   catch(System.Exception ex){throw Fail("INVALID","Không đọc được hình học đường tâm ("+ex.GetType().Name+": "+ex.Message+"). Chưa sửa bản vẽ.");}}
  static FlexShape029 Run(Curve c,double D,bool fromEnd){
   if(c==null)throw Fail("NULL","Chưa chọn đường tâm.");if(c.IsErased)throw Fail("ERASED","Đường tâm đã bị xoá.");
   if(!FlexGeometry022.Finite(D)||D<=0)throw Fail("DIAMETER","Đường kính phải là số dương hữu hạn.");
   string type=c is Line?"LINE":c is DbPolyline?"LWPOLYLINE":c is Arc?"ARC":c is Spline?"SPLINE":null;
   if(type==null)throw Fail("UNSUPPORTED","Chỉ nhận LINE, PLINE (LWPOLYLINE), ARC hoặc SPLINE làm đường tâm; "+c.GetType().Name+" chưa hỗ trợ. Chưa sửa bản vẽ.");
   double L=c.GetDistanceAtParameter(c.EndParam)-c.GetDistanceAtParameter(c.StartParam);
   if(!FlexGeometry022.Finite(L))throw Fail("NONFINITE","Chiều dài đường tâm không hữu hạn.");
   double tol=FlexGeometry022.Tolerance(L,D);Point3d a=c.StartPoint,b=c.EndPoint;
   if(!(L>tol))throw Fail("ZERO_LENGTH","Đường tâm có chiều dài 0.");
   if(c.Closed||a.DistanceTo(b)<=tol)throw Fail("CLOSED","Đường tâm khép kín (hai đầu trùng nhau) chưa hỗ trợ: không xác định được đầu/cuối ống. Chưa sửa bản vẽ.");
   Planar(c,type,a,b,tol);
   string why=FlexGeometry022.Check(L,D);if(why!=null)throw Fail("CHECK",why);
   var poly=c as DbPolyline;List<Seg> segs=poly==null?null:Segments(poly,tol);
   if(c is Line||(segs!=null&&segs.Count==1&&!segs[0].arc)){
    Point3d s=c is Line?a:poly.GetPointAtParameter(segs[0].p0),e=c is Line?b:poly.GetPointAtParameter(segs[0].p1);
    double ox=fromEnd?e.X:s.X,oy=fromEnd?e.Y:s.Y,tx=fromEnd?s.X:e.X,ty=fromEnd?s.Y:e.Y;double[] xs,ys,bs;
    int n0=FlexGeometry022.Build(ox,oy,tx,ty,D,out xs,out ys,out bs);
    return Finish(new FlexShape029{X=xs,Y=ys,Bulges=bs,Length=L,Elevation=a.Z,Cells=n0,SourceType=type,LinearOracle=true});}
   int miters;var st=segs!=null?PolyStations(poly,segs,D,tol,out miters):SmoothStations(c,type,L,D,out miters);
   int n=st.Count-1;if(n<FlexGeometry022.MinCells)throw Fail("CHECK","Đường tâm quá ngắn so với D (chỉ "+n+" nếp).");
   if(n>FlexGeometry022.MaxCells)throw Fail("TOO_MANY","Quá nhiều nếp gấp ("+n+" > "+FlexGeometry022.MaxCells+").");
   Fold(st,tol);
   double zt=Math.Max(tol,1e-9*Math.Abs(a.Z));
   foreach(var x in st)if(!(Math.Abs(c.GetPointAtDist(Math.Max(0,Math.Min(L,x.s))).Z-a.Z)<=zt))throw Fail("NONPLANAR","Đường tâm không nằm trong một mặt phẳng ngang (Z thay đổi tại "+F(x.s)+").");
   if(fromEnd){st.Reverse();for(int i=0;i<st.Count;i++){var x=st[i];x.wx=-x.wx;x.wy=-x.wy;st[i]=x;}}// miter flags travel with their stations
   var shape=Rhythm(st);shape.Length=L;shape.Elevation=a.Z;shape.SourceType=type;shape.Miters=miters;return Finish(shape);}
  // Horizontal only. LWPOLYLINE/ARC: normal +Z or -Z (WCS points are used, so the OCS sign does not matter). LINE: equal Z. SPLINE: every control point
  // at the same Z (then the whole spline lies in that plane).
  static void Planar(Curve c,string type,Point3d a,Point3d b,double tol){bool ok;
   var pl=c as DbPolyline;var arc=c as Arc;var sp=c as Spline;
   if(pl!=null)ok=pl.Normal.IsParallelTo(Vector3d.ZAxis);else if(arc!=null)ok=arc.Normal.IsParallelTo(Vector3d.ZAxis);
   else if(sp!=null){ok=true;double z=a.Z,zt=Math.Max(tol,1e-9*Math.Abs(z));for(int i=0;i<sp.NumControlPoints&&ok;i++)ok=Math.Abs(sp.GetControlPointAt(i).Z-z)<=zt;}
   else ok=Math.Abs(a.Z-b.Z)<=tol;
   if(!ok)throw Fail("NONPLANAR",type+" không nằm ngang (không song song mặt XY); bản thí điểm chỉ nhận đường tâm nằm ngang. Chưa sửa bản vẽ.");}
  // Non-zero LINE/ARC segments of an open LWPOLYLINE (coincident/zero-length segments are skipped, not removed from the source).
  static List<Seg> Segments(DbPolyline pl,double tol){int nv=pl.NumberOfVertices;if(nv<2)throw Fail("ZERO_LENGTH","PLINE có ít hơn 2 đỉnh.");
   if(nv-1>4*FlexGeometry022.MaxCells)throw Fail("TOO_MANY","PLINE có quá nhiều đỉnh ("+nv+").");var list=new List<Seg>();
   for(int k=0;k<nv-1;k++){var t=pl.GetSegmentType(k);double d0=pl.GetDistanceAtParameter(k),d1=pl.GetDistanceAtParameter(k+1);
    if((t!=SegmentType.Line&&t!=SegmentType.Arc)||!(d1-d0>tol))continue;
    var g=new Seg{k=k,d0=d0,d1=d1,p0=k,p1=k+1,arc=t==SegmentType.Arc};if(g.arc)g.radius=pl.GetArcSegment2dAt(k).Radius;list.Add(g);}
   if(list.Count==0)throw Fail("ZERO_LENGTH","PLINE không có đoạn nào có chiều dài.");
   if(list.Count>FlexGeometry022.MaxCells)throw Fail("TOO_MANY","PLINE có quá nhiều đoạn ("+list.Count+").");return list;}
  // Unit XY tangent of increasing parameter; false when the XY derivative vanishes (cusp/degenerate).
  static bool Dir(Curve c,double p,double scale,out double tx,out double ty){var d=c.GetFirstDerivative(p);double len=Math.Sqrt(d.X*d.X+d.Y*d.Y);
   if(!FlexGeometry022.Finite(len)||!(len>1e-12*Math.Max(1,scale))){tx=ty=0;return false;}tx=d.X/len;ty=d.Y/len;return true;}
  static Station Normal(double s,Point3d p,double tx,double ty,double h){return new Station{s=s,cx=p.X,cy=p.Y,wx=-ty*h,wy=tx*h};}
  static Station At(Curve c,double s,double L,double h,string type){double p=c.GetParameterAtDistance(s);double tx,ty;
   if(!Dir(c,p,L,out tx,out ty))throw Fail("CUSP",type+": tiếp tuyến suy biến (điểm nhọn) tại khoảng cách "+F(s)+". Chưa sửa bản vẽ.");return Normal(s,c.GetPointAtParameter(p),tx,ty,h);}
  static int Intervals(double len,double D){return len>0?(int)Math.Floor(4*len/D+1e-9):0;}
  // LWPOLYLINE: start, interior stations per segment, a miter station at every join, end. Counts are bounded before any point is sampled.
  static List<Station> PolyStations(DbPolyline pl,List<Seg> segs,double D,double tol,out int miters){double h=D/2,L=segs[segs.Count-1].d1;int ns=segs.Count;miters=0;
   foreach(var g in segs)if(g.arc&&!(g.radius>h))throw Fail("CURVATURE","Đoạn cung thứ "+(g.k+1)+" của PLINE có bán kính "+F(g.radius)+" ≤ D/2 = "+F(h)+": thành trong của ống tự gập (giới hạn hình học, không phải bán kính uốn kỹ thuật). Chưa sửa bản vẽ.");
   var join=new Station[ns];var keep=new double[ns];
   for(int j=0;j<ns-1;j++){Seg A=segs[j],B=segs[j+1];double ix,iy,ox,oy;
    if(!Dir(pl,A.p1-ParamStep,L,out ix,out iy)||!Dir(pl,B.p0+ParamStep,L,out ox,out oy))throw Fail("CUSP","PLINE: tiếp tuyến suy biến tại đỉnh "+(A.k+2)+".");
    double mx=-iy-oy,my=ix+ox,ml=Math.Sqrt(mx*mx+my*my),cs=ml/2;
    if(!(cs>1e-9))throw Fail("REVERSAL","PLINE quay ngược 180° tại đỉnh "+(A.k+2)+" (khoảng cách "+F(A.d1)+"): mặt cắt góc không xác định. Chưa sửa bản vẽ.");
    var p=pl.GetPointAtParameter(A.p1);mx/=ml;my/=ml;join[j]=new Station{s=A.d1,cx=p.X,cy=p.Y,wx=mx*h/cs,wy=my*h/cs};
    keep[j]=h*Math.Sqrt(Math.Max(0,1-cs*cs))/cs;if(keep[j]>tol){miters++;join[j].miter=true;}}
   var spans=new int[ns];var free=new double[ns];long total=1;
   for(int j=0;j<ns;j++){Seg A=segs[j];double e0=j==0?0:keep[j-1],e1=j==ns-1?0:keep[j],len=A.d1-A.d0,f=len-e0-e1;
    if(f<-tol)throw Fail("CORNER_FOLD","Đoạn PLINE thứ "+(A.k+1)+" dài "+F(len)+" ngắn hơn phần cần cho mặt cắt góc ở hai đầu ("+F(e0+e1)+" với D = "+F(D)+"): thân ống tự gập tại góc (giới hạn hình học). Chưa sửa bản vẽ.");
    free[j]=Math.Max(0,f);spans[j]=Intervals(free[j],D);total+=Math.Max(0,spans[j]-1)+1;}
   if(total-1>FlexGeometry022.MaxCells)throw Fail("TOO_MANY","Quá nhiều nếp gấp ("+(total-1)+" > "+FlexGeometry022.MaxCells+"): đường kính quá nhỏ so với chiều dài.");
   var st=new List<Station>((int)total);double sx,sy;
   if(!Dir(pl,segs[0].p0+ParamStep,L,out sx,out sy))throw Fail("CUSP","PLINE: tiếp tuyến suy biến tại đầu.");st.Add(Normal(segs[0].d0,pl.GetPointAtParameter(segs[0].p0),sx,sy,h));
   for(int j=0;j<ns;j++){Seg A=segs[j];double a=A.d0+(j==0?0:keep[j-1]);int m=spans[j];
    for(int i=1;i<m;i++)st.Add(At(pl,a+free[j]*i/m,L,h,"PLINE"));
    if(j<ns-1)st.Add(join[j]);
    else{if(!Dir(pl,A.p1-ParamStep,L,out sx,out sy))throw Fail("CUSP","PLINE: tiếp tuyến suy biến tại cuối.");st.Add(Normal(A.d1,pl.GetPointAtParameter(A.p1),sx,sy,h));}}
   return st;}
  // ARC/SPLINE: evenly spaced stations by native arc length (no interior break points). SPLINE: sampled curvature and tangent-turn checks.
  static List<Station> SmoothStations(Curve c,string type,double L,double D,out int miters){double h=D/2;miters=0;var arc=c as Arc;
   if(arc!=null&&!(arc.Radius>h))throw Fail("CURVATURE","ARC có bán kính "+F(arc.Radius)+" ≤ D/2 = "+F(h)+": thành trong của ống tự gập (giới hạn hình học, không phải bán kính uốn kỹ thuật). Chưa sửa bản vẽ.");
   int m=Intervals(L,D);var st=new List<Station>(m+1);double tx,ty;
   if(!Dir(c,c.StartParam,L,out tx,out ty))throw Fail("CUSP",type+": tiếp tuyến suy biến tại đầu.");st.Add(Normal(0,c.StartPoint,tx,ty,h));
   for(int i=1;i<m;i++)st.Add(At(c,L*i/m,L,h,type));
   if(!Dir(c,c.EndParam,L,out tx,out ty))throw Fail("CUSP",type+": tiếp tuyến suy biến tại cuối.");st.Add(Normal(L,c.EndPoint,tx,ty,h));
   if(c is Spline)for(int i=0;i<st.Count;i++){Curvature(c,c.GetParameterAtDistance(Math.Min(L,st[i].s)),h,st[i].s,L);
    if(i>0){Curvature(c,c.GetParameterAtDistance((st[i-1].s+st[i].s)/2),h,(st[i-1].s+st[i].s)/2,L);
     if(!(st[i-1].wx*st[i].wx+st[i-1].wy*st[i].wy>0))throw Fail("CUSP","SPLINE đổi hướng quá 90° giữa "+F(st[i-1].s)+" và "+F(st[i].s)+" (điểm nhọn/quay đầu, kiểm tra theo mẫu). Chưa sửa bản vẽ.");}}
   return st;}
  static void Curvature(Curve c,double p,double h,double s,double L){var d1=c.GetFirstDerivative(p);var d2=c.GetSecondDerivative(p);double q=d1.X*d1.X+d1.Y*d1.Y;
   if(!FlexGeometry22Finite(q)||!(Math.Sqrt(q)>1e-12*Math.Max(1,L)))throw Fail("CUSP","SPLINE: tiếp tuyến suy biến tại "+F(s)+".");
   double k=Math.Abs(d1.X*d2.Y-d1.Y*d2.X)/(q*Math.Sqrt(q));
   if(!FlexGeometry22Finite(k)||k*h>=1)throw Fail("CURVATURE","SPLINE có bán kính cong ≈ "+(k>0?F(1/k):"0")+" ≤ D/2 = "+F(h)+" tại "+F(s)+": thành trong của ống tự gập (giới hạn hình học, kiểm tra theo mẫu). Chưa sửa bản vẽ.");}
  static bool FlexGeometry22Finite(double x){return FlexGeometry022.Finite(x);}
  // Generic local check: on both walls the section point must advance along the chord between consecutive stations (otherwise sections cross).
  static void Fold(List<Station> st,double tol){for(int k=0;k+1<st.Count;k++){Station p=st[k],q=st[k+1];double chx=q.cx-p.cx,chy=q.cy-p.cy;
    if(!(chx*chx+chy*chy>tol*tol))throw Fail("FOLD","Hai mặt cắt trùng nhau tại "+F(p.s)+".");
    for(int s=-1;s<=1;s+=2){double dx=q.cx+s*q.wx-(p.cx+s*p.wx),dy=q.cy+s*q.wy-(p.cy+s*p.wy);
     if(!(dx*chx+dy*chy>0))throw Fail("FOLD","Thân ống tự gập giữa khoảng cách "+F(p.s)+" và "+F(q.s)+" (đường tâm cong/gấp quá gắt so với D; giới hạn hình học, không phải quy định kỹ thuật). Chưa sửa bản vẽ.");}}}
  // 022 rhythm on stations: start [Q1- Q1+ Q0+ Q0- Q1-], cells i=2..n [Qi- Qi+ Qi-1+ Qi-1- Qi-], end [Qn+ Qn- Qn+]; bulges as FlexGeometry022.Build.
  static FlexShape029 Rhythm(List<Station> st){int n=st.Count-1,count=5*n+3,i=0;double B=FlexGeometry022.B90;var X=new double[count];var Y=new double[count];var G=new double[count];
   Action<int,int,double> add=(k,s,b)=>{X[i]=st[k].cx+s*st[k].wx;Y[i]=st[k].cy+s*st[k].wy;G[i]=b;i++;};
   // F029-GEOM-01: a longitudinal wall link p→q keeps the -B corrugation unless p or q is a miter station (longer keep-out interval): then a straight
   // offset wall. Cross-sections and the end caps are unchanged.
   Func<int,int,double> wall=(p,q)=>st[p].miter||st[q].miter?0:-B;
   add(1,-1,0);add(1,1,0);add(0,1,0);add(0,-1,0);add(1,-1,wall(1,2));
   for(int j=2;j<=n;j++){add(j,-1,0);add(j,1,wall(j,j-1));add(j-1,1,0);add(j-1,-1,wall(j-1,j));add(j,-1,j<n?wall(j,j+1):B);}
   add(n,1,1);add(n,-1,1);add(n,1,0);
   if(i!=count)throw Fail("INVALID","FLEX029: số đỉnh "+i+" ≠ "+count+".");return new FlexShape029{X=X,Y=Y,Bulges=G,Cells=n};}
  static FlexShape029 Finish(FlexShape029 r){for(int i=0;i<r.X.Length;i++)if(!FlexGeometry022.Finite(r.X[i])||!FlexGeometry022.Finite(r.Y[i])||!FlexGeometry022.Finite(r.Bulges[i]))throw Fail("NONFINITE","Đỉnh thân ống không hữu hạn tại "+i+".");
   if(r.X.Length!=5*r.Cells+3)throw Fail("INVALID","FLEX029: số đỉnh không khớp số nếp.");return r;}
 }
}
