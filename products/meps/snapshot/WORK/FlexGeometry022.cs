using System;using System.Globalization;
namespace Tvxd.Meps {
 // TVXD-MEPS-FLEXDUCT-022 pilot, pure part (no CAD types; compiles alone for offline tests).
 // Straight-LINE port of "Ong Noi Mem.lsp" (C:NM), free LISP by ssg, June 2008, www.cadviet.com
 // (http://www.cadviet.com/forum/topic/4207-ve-ong-mem-kieu-ruot-ga/). Attribution kept; the LISP itself is never executed.
 // Source rhythm for a straight spine O→T, length L, diameter D (u = unit O→T, v = left normal, P(x,s) = O + u·x + v·s·D/2):
 //   n = floor(4L/D) (must be ≥ 2: the source's terminal "a 90 …" is only valid in arc mode, entered at i = 2), S = L/n, B = tan(90°/4)
 //   start  [p3 p4 p2 p1 p3]            = P(S,-1) P(S,+1) P(0,+1) P(0,-1) P(S,-1)          bulges 0 0 0 0 -B
 //   i=2..n [p5 p6 p4 p3 p5]            = P(iS,-1) P(iS,+1) P((i-1)S,+1) P((i-1)S,-1) P(iS,-1)  bulges 0 -B 0 -B (-B, or +B after the last cell)
 //   end    [p6 p5 p6]                  = P(L,+1) P(L,-1) P(L,+1)                           bulges +1 +1 0  (source arcs 90°, 180°, 180°)
 // Bulge = tan(included angle / 4); the source's "a -90" is a clockwise 90° arc → -B. Vertex count 5n+3. The source retraces each lower arc once
 // (p3→p5 twice); this is kept on purpose so the outline is vertex-for-vertex the source's. The spine is recoverable from the body:
 // O = mid(v2,v3), T = mid(v[last],v[last-1]). Unlike the source, the spine direction is fixed by the stored flag, not re-guessed from a pick.
 public static class FlexGeometry022 {
  public const int MinCells=2,MaxCells=2000;
  public static readonly double B90=Math.Tan(Math.PI/8);
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  public static bool Finite(double x){return !double.IsNaN(x)&&!double.IsInfinity(x);}
  // Number of corrugation cells for length L and diameter D; 0 when the inputs are not positive finite numbers.
  public static int Cells(double L,double D){if(!Finite(L)||!Finite(D)||L<=0||D<=0)return 0;double q=4*L/D;if(!Finite(q)||q>int.MaxValue/2)return int.MaxValue;return (int)Math.Floor(q+1e-9);}
  // Null when L/D can be drawn; otherwise a user message (Vietnamese) saying why. Nothing is clamped or guessed.
  public static string Check(double L,double D){if(!Finite(D)||D<=0)return "Đường kính phải là số dương hữu hạn.";
   if(!Finite(L)||L<=0)return "Đường tâm phải có chiều dài dương hữu hạn trong mặt phẳng XY.";
   int n=Cells(L,D);if(n<MinCells)return "Đường tâm quá ngắn: cần chiều dài ≥ D/2 = "+(D/2).ToString("0.###",Inv)+" (đang "+L.ToString("0.###",Inv)+").";
   if(n>MaxCells)return "Quá nhiều nếp gấp ("+(n==int.MaxValue?"rất lớn":n.ToString(Inv))+" > "+MaxCells+"): đường kính quá nhỏ so với chiều dài.";return null;}
  // Vertices of the body for spine O→T. xs/ys/bulges have 5n+3 entries. Throws ArgumentException when Check fails.
  public static int Build(double ox,double oy,double tx,double ty,double D,out double[] xs,out double[] ys,out double[] bulges){
   double dx=tx-ox,dy=ty-oy,L=Math.Sqrt(dx*dx+dy*dy);string why=Check(L,D);if(why!=null)throw new ArgumentException(why);
   int n=Cells(L,D);double S=L/n,ux=dx/L,uy=dy/L,vx=-uy,vy=ux,h=D/2,B=B90;int count=5*n+3,k=0;
   xs=new double[count];ys=new double[count];bulges=new double[count];var X=xs;var Y=ys;var G=bulges;
   Action<double,int,double> add=(x,s,b)=>{double px=ox+ux*x+vx*s*h,py=oy+uy*x+vy*s*h;
    // The last cell boundary uses x = L (not n·S) so the end section is not shifted by rounding.
    X[k]=px;Y[k]=py;G[k]=b;k++;};
   add(S,-1,0);add(S,1,0);add(0,1,0);add(0,-1,0);add(S,-1,-B);
   for(int i=2;i<=n;i++){double x0=(i-1)*S,x1=i==n?L:i*S;add(x1,-1,0);add(x1,1,-B);add(x0,1,0);add(x0,-1,-B);add(x1,-1,i<n?-B:B);}
   add(L,1,1);add(L,-1,1);add(L,1,0);
   if(k!=count)throw new InvalidOperationException("FLEX022: vertex count "+k+" ≠ "+count);return n;}
  // Spine O and T recovered from a body vertex list (null-safe callers check count ≥ 13 first).
  public static void Ends(double[] xs,double[] ys,out double ox,out double oy,out double tx,out double ty){int m=xs.Length;
   ox=(xs[2]+xs[3])/2;oy=(ys[2]+ys[3])/2;tx=(xs[m-1]+xs[m-2])/2;ty=(ys[m-1]+ys[m-2])/2;}
  // Same outline within tolerance (count, every point, every bulge). Used to decide VERIFIED; any difference → UNVERIFIED.
  public static bool Same(double[] ax,double[] ay,double[] ab,double[] bx,double[] by,double[] bb,double tol){
   if(ax.Length!=bx.Length||ay.Length!=by.Length||ab.Length!=bb.Length||ax.Length!=ay.Length||ax.Length!=ab.Length)return false;
   for(int i=0;i<ax.Length;i++){if(!Finite(ax[i])||!Finite(ay[i])||Math.Abs(ax[i]-bx[i])>tol||Math.Abs(ay[i]-by[i])>tol||Math.Abs(ab[i]-bb[i])>1e-9)return false;}return true;}
  public static double Tolerance(double L,double D){return 1e-8*Math.Max(1,Math.Max(Math.Abs(L),Math.Abs(D)));}
  // Known drawing units only (code = INSUNITS value). Unitless and anything else → null (caller refuses; no assumed mm).
  public static bool Unit(int insunits,out string name,out double metres){
   switch(insunits){case 4:name="mm";metres=0.001;return true;case 5:name="cm";metres=0.01;return true;case 14:name="dm";metres=0.1;return true;
    case 6:name="m";metres=1;return true;case 1:name="in";metres=0.0254;return true;case 2:name="ft";metres=0.3048;return true;}
   name=null;metres=0;return false;}
  public static string R(double x){return x.ToString("R",Inv);}
  public static double P(string s){double v;if(s==null||!double.TryParse(s,NumberStyles.Float,Inv,out v)||!Finite(v))throw new FormatException("FLEX022: số không hợp lệ '"+s+"'.");return v;}
 }
}
