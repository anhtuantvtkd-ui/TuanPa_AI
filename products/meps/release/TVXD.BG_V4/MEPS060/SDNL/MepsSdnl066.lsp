;;; Companion to the persistent partial CUIx. No security/profile values are changed here.
(vl-load-com)
(defun TVXD:UI:root ()
 (if (and (boundp 'tvxd-meps-sdnl-root) tvxd-meps-sdnl-root (vl-file-directory-p tvxd-meps-sdnl-root)) tvxd-meps-sdnl-root nil))
(defun TVXD:UI:open (relative / root path)
 (setq root (TVXD:UI:root))
 (if root
  (progn (setq path (strcat root "/" relative))
   (if (or (findfile path) (vl-file-directory-p path))
    (startapp "explorer.exe" (strcat "\"" path "\""))
    (alert "Khong tim thay tai nguyen SDNL. Giu nguyen ca folder da cai.")))
  (alert "Chua nap TVXD_SDNL.cuix.")) (princ))
(defun c:SDNLTEMPLATES () (TVXD:UI:open "MAU"))
(defun c:SDNLPROJECTS () (TVXD:UI:open "PROJECTS"))
(defun c:SDNLHELP () (TVXD:UI:open "HUONG_DAN_SU_DUNG.docx"))
(princ "\nSDNL ready: Ribbon SDNL / SDNLMANAGER.")
(princ)

;;; ============================================================================
;;; SDNL_PRODUCTION.LSP
;;; AutoCAD 2021 - Excel -> kiem tra / ve moi / dong bo so do nguyen ly dien
;;; Nguon tham chieu: DXF mau chuan + Excel chuan + stable gan nhat.
;;; Chi cung cap 3 lenh: SDNLCHECK / SDNLNEW / SDNLUPDATE
;;; ============================================================================


(vl-load-com)

(setq *C1* 623.5
      *P* 1280.0
      *LE* 723.9
      *NET* 56.0
      *H* 200.0
      *NEN* 0.8
      *Y_BANG* '(-8944.7 -9753.5 -13694.7 -14503.5)
      *K_TRAI* -643.8
      *K_PHAI* 982.9
      *K_DUOI* -2818.2
      *K_TREN* 3053.6
      *TDT_L* -1493.5
      *TDT_R* 982.9
      *TDT_TX* -255.3
      *NOIDAT* 1402.8
      *PTT_DX* 436.1
      *LO_DY* -1649.1
      *KICHTHUOC* "800x600x250"
      *KIEU* "Tuan_arial"
      *SDT-SHEET* "KHOI_A_DXF")

(setq *NHAN*
  (list
    (list "C\\U+00C1P C\\U+1EA4P NGU\\U+1ED2N" -4202.7 3679.9)
    (list "APTOMAT T\\U+1ED4NG" -3657.3 1611.7)
    (list "THANH C\\U+00C1I" -3657.3 16.9)
    (list "APTOMAT L\\U+1ED8 RA" -3657.3 -1637.1)
    (list "L\\U+1ED8 RA" -4202.7 -3174.6)
    (list "C\\U+00C1P \\U+0110I\\U+1EC6N /" -4202.7 -5847.0)
    (list "C\\U+00C1CH L\\U+1EAEP \\U+0110\\U+1EB6T" -4202.7 -6392.5)
    (list "PHA" -4202.7 -9345.3)
    (list "PH\\U+1EE4 T\\U+1EA2I" -4202.7 -11743.3)
    (list "P(KW)" -4202.7 -14086.2)))

(setq *NHAN_PHAI* -1458.7)
(setq *NHAN_NGANG*
  (list
    (list 4231.3 -4566.4) (list 3053.6 -4566.4)
    (list 469.0 -3839.1) (list -440.1 -3839.1)
    (list -2818.2 -4566.4) (list -3545.5 -4566.4)
    (list -8934.7 -4566.4) (list -9753.5 -4566.4)
    (list -13694.7 -4566.4) (list -14503.5 -4566.4)))

(setq *SDT-PANEL-APP* "SDNL_PANEL"
      *SDT-CURRENT-PANEL* nil
      *SDT-LAST-ERROR* nil
      *SDT-XL* nil
      *SDT-BOOKS* nil
      *SDT-BOOK* nil
      *SDT-SHEETS* nil
      *SDT-SH* nil
      *SDT-USED* nil
      *SDT-ROWS* nil
      *SDT-CELLS* nil
      *SDT-TEMPFILE* nil
      *SDT-FAST-SA* nil
      *SDT-FAST-R0* 1
      *SDT-FAST-C0* 1
      *SDT-FAST-RLB* 1
      *SDT-FAST-CLB* 1
      *SDT-FAST-RUB* 0
      *SDT-FAST-CUB* 0
      *SDT-MAX-COL* 1)


(defun SDT:trim (s)
  (if s (vl-string-trim " \t\r\n" s) ""))

(defun SDT:split (s sep / p out)
  (setq s (if s s ""))
  (while (setq p (vl-string-search sep s))
    (setq out (cons (substr s 1 p) out)
          s   (substr s (+ p 1 (strlen sep)))))
  (reverse (cons s out)))

(defun SDT:fld (r i)
  (if (< i (length r)) (nth i r) ""))

(defun SDT:lead-zero (s)
  ;; Bat buoc so thap phan nho hon 1 co so 0 phia truoc.
  ;; Khong phu thuoc DIMZIN / cach RTOS rut gon so cua AutoCAD.
  ;; .32 -> 0.32 ; -.25 -> -0.25 ; ,32 -> 0,32
  (setq s (if s (SDT:trim s) ""))
  (cond
    ((= s "") "")
    ((or (= (substr s 1 1) ".") (= (substr s 1 1) ","))
      (strcat "0" s))
    ((and (> (strlen s) 1)
          (or (= (substr s 1 2) "-.") (= (substr s 1 2) "-,")))
      (strcat "-0" (substr s 2)))
    (T s)))

(defun SDT:num2str-2 (x / neg ax scaled ip fp s)
  ;; Dinh dang toi da 2 chu so thap phan, doc lap DIMZIN/DIMDSEP.
  ;; Giu so 0 co nghia: 40 -> "40", 10 -> "10"; 22.536 -> "22.54".
  (setq neg (< x 0.0)
        ax (abs x)
        scaled (fix (+ (* ax 100.0) 0.5))
        ip (fix (/ scaled 100.0))
        fp (- scaled (* ip 100))
        s (itoa ip))
  (cond
    ((= fp 0) nil)
    ((= (rem fp 10) 0)
      (setq s (strcat s "." (itoa (fix (/ fp 10.0))))))
    (T
      (setq s (strcat s "." (if (< fp 10) "0" "") (itoa fp)))))
  (if (and neg (/= scaled 0)) (strcat "-" s) s))

(defun SDT:num-clean (s / i ch out done x)
  ;; Lay phan so o dau text va lam tron toi da 2 so le nhu DXF mau,
  ;; nhung khong dung RTOS de tranh DIMZIN bien 40 -> 4, 10 -> 1.
  (setq s (SDT:trim (if s s ""))
        i 1 out "" done nil)
  (while (and (<= i (strlen s)) (not done))
    (setq ch (substr s i 1))
    (if (vl-string-search ch "0123456789+-.,")
      (setq out (strcat out ch))
      (setq done T))
    (setq i (1+ i)))
  (if (= out "")
    s
    (progn
      (setq out (vl-string-translate "," "." out)
            x (atof out))
      (SDT:num2str-2 x))))

(defun SDT:parse-cb (s / a loai cuc inm icu)
  ;; "MCB - 3P - 16A - 4.5kA" -> ("MCB" "3P" "16" "4.5kA")
  (setq a (mapcar 'SDT:trim (SDT:split s " - ")))
  (setq loai (if (> (length a) 0) (nth 0 a) ""))
  (setq cuc  (if (> (length a) 1) (nth 1 a) ""))
  (setq inm  (if (> (length a) 2) (SDT:num-clean (nth 2 a)) ""))
  (setq icu  (if (> (length a) 3) (nth 3 a) ""))
  (list loai cuc inm icu))

(defun SDT:parse-pair (s / a)
  ;; "11.9 kW / 0.7" -> ("11.9" "0.7")
  (setq a (SDT:split (if s s "") "/"))
  (list
    (if (> (length a) 0) (SDT:num-clean (nth 0 a)) "")
    (if (> (length a) 1) (SDT:num-clean (nth 1 a)) "")))

(defun SDT:reg-panel-app ()
  (if (not (tblsearch "APPID" *SDT-PANEL-APP*))
    (regapp *SDT-PANEL-APP*))
  T)

(defun SDT:tag-ename (en / ed)
  (if (and en *SDT-CURRENT-PANEL* (/= *SDT-CURRENT-PANEL* ""))
    (progn
      (SDT:reg-panel-app)
      (setq ed (entget en (list *SDT-PANEL-APP*)))
      (entmod
        (append ed
          (list
            (list -3
              (list *SDT-PANEL-APP*
                (cons 1000 *SDT-CURRENT-PANEL*))))))))
  (if *SD153-ID* (SD153:stamp en))
  en)

(defun SDT:tag-obj (obj / en)
  (if obj
    (progn
      (setq en (vlax-vla-object->ename obj))
      (SDT:tag-ename en)))
  obj)

(defun SDT:mk (lst / en)
  (setq en (entmakex lst))
  (SDT:tag-ename en))

(defun SDT:get-panel-tag (en / ed xd rec a)
  (if en
    (progn
      ;; Neu click vao ATTRIB thi PANEL_ID nam tren INSERT cha.
      (setq ed (entget en))
      (if (= (cdr (assoc 0 ed)) "ATTRIB")
        (progn
          (setq a (cdr (assoc 330 ed)))
          (if a (setq en a))))
      (setq ed (entget en (list *SDT-PANEL-APP*))
            xd (assoc -3 ed))
      (if xd
        (progn
          (setq rec (cadr xd))
          (if rec
            (cdr (assoc 1000 (cdr rec)))
            nil))
        nil))
    nil))


(defun SDP:ensure-layer (name color lw / )
  (if (not (tblsearch "LAYER" name))
    (entmakex
      (list '(0 . "LAYER") '(100 . "AcDbSymbolTableRecord")
            '(100 . "AcDbLayerTableRecord") (cons 2 name)
            '(70 . 0) (cons 62 color) '(6 . "Continuous") (cons 370 lw))))
  T)

(defun SDP:ensure-text-style (/ )
  (if (not (tblsearch "STYLE" "Tuan_arial"))
    (entmakex
      (list '(0 . "STYLE") '(100 . "AcDbSymbolTableRecord")
            '(100 . "AcDbTextStyleTableRecord")
            '(2 . "Tuan_arial") '(70 . 0) '(40 . 0.0) '(41 . 0.9)
            '(50 . 0.0) '(71 . 0) '(42 . 200.0)
            '(3 . "arial.ttf") '(4 . ""))))
  (setq *KIEU* "Tuan_arial")
  T)

(defun SDT:chuan-bi (/ )
  (SDT:reg-panel-app)
  (SDP:ensure-layer "CDC-E-Equip" 2 40)
  (SDP:ensure-layer "CCU-E-Note" 3 30)
  (SDP:ensure-text-style)
  (princ (strcat "\n>> Kieu chu: " *KIEU*))
  T)


(defun SDT:L2 (x1 y1 x2 y2 lay)
  (SDT:mk
    (list '(0 . "LINE") (cons 8 lay)
          (list 10 x1 y1 0.0) (list 11 x2 y2 0.0))))

(defun SDT:TX2 (s x y canh lay rot)
  (SDT:mk
    (list '(0 . "TEXT") (cons 8 lay)
          (list 10 x y 0.0) (list 11 x y 0.0)
          (cons 40 *H*) (cons 41 *NEN*) (cons 1 s) (cons 7 *KIEU*)
          (cons 50 (* rot (/ pi 180.0)))
          (cons 72 canh) '(73 . 2))))

(defun SDT:chen (ten x y alist / msp obj arr at tag v)
  (setq msp (vla-get-ModelSpace (vla-get-ActiveDocument (vlax-get-acad-object))))
  (setq obj (vla-InsertBlock msp (vlax-3d-point x y 0.0) ten 1.0 1.0 1.0 0.0))
  (SDT:tag-obj obj)
  (if (= :vlax-true (vla-get-HasAttributes obj))
    (progn
      (setq arr (vlax-variant-value (vla-GetAttributes obj)))
      (foreach at (vlax-safearray->list arr)
        (setq tag (strcase (vla-get-TagString at))
              v   (cdr (assoc tag alist)))
        (if v (vla-put-TextString at (if (= v "") " " v))))))
  obj)

(defun SDT:try-unvariant (v / r)
  ;; Khong dung ham TYPE: thu go VARIANT bang catch; neu khong phai VARIANT thi giu nguyen.
  (setq r (vl-catch-all-apply 'vlax-variant-value (list v)))
  (if (vl-catch-all-error-p r) v r))

(defun SDT:xval->str (v / vv u)
  (cond
    ((null v) "")
    (T
      (setq u (SDT:try-unvariant v))
      (if (not (eq u v))
        (SDT:xval->str u)
        (if (numberp v)
          ;; Giu quy tac hien thi 2 so le cua DXF mau, khong phu thuoc DIMZIN.
          (SDT:num2str-2 v)
          (SDT:lead-zero (SDT:trim (vl-princ-to-string v))))))))

(defun SDT:unwrap (v / r next done)
  ;; Go 1 hay nhieu lop VARIANT ma khong can TYPE.
  (setq done nil)
  (while (not done)
    (setq r (vl-catch-all-apply 'vlax-variant-value (list v)))
    (if (vl-catch-all-error-p r)
      (setq done T)
      (setq v r)))
  v)

(defun SDT:safe-release (o / r)
  ;; Khong kiem tra TYPE; de VL-CATCH xu ly neu o khong phai COM object.
  (if o
    (progn
      (setq r (vl-catch-all-apply 'vlax-object-released-p (list o)))
      (if (and (not (vl-catch-all-error-p r)) (not r))
        (vl-catch-all-apply 'vlax-release-object (list o))))))

(defun SDT:excel-cleanup ( / )
  ;; Luon thu dong workbook/Excel do SDNL tu tao, ke ca khi doc bi loi.
  (if *SDT-BOOK*
    (vl-catch-all-apply 'vlax-invoke-method
                        (list *SDT-BOOK* 'Close :vlax-false)))
  (if *SDT-XL*
    (vl-catch-all-apply 'vlax-invoke-method (list *SDT-XL* 'Quit)))

  (foreach o (list *SDT-CELLS* *SDT-ROWS* *SDT-USED* *SDT-SH*
                   *SDT-SHEETS* *SDT-BOOK* *SDT-BOOKS* *SDT-XL*)
    (SDT:safe-release o))

  (setq *SDT-CELLS* nil
        *SDT-ROWS* nil
        *SDT-USED* nil
        *SDT-SH* nil
        *SDT-SHEETS* nil
        *SDT-BOOK* nil
        *SDT-BOOKS* nil
        *SDT-XL* nil)

  ;; Xoa ban sao tam sau khi Excel da dong sach.
  (if (and *SDT-TEMPFILE* (findfile *SDT-TEMPFILE*))
    (vl-catch-all-apply 'vl-file-delete (list *SDT-TEMPFILE*)))
  (setq *SDT-TEMPFILE* nil)
  (gc))

(defun SDT:make-local-copy (src / tmpdir dst ms fso r szsrc szdst ws cmd rc)
  ;; Sau copy kiem tra kich thuoc file truoc khi giao cho Excel.
  (setq *SDT-LAST-ERROR* nil)
  (setq src (vl-string-translate "/" "\\" src))
  (princ (strcat "\n>> File nguon: " src))
  (if (not (findfile src))
    (progn
      (setq *SDT-LAST-ERROR* (strcat "Khong tim thay file da chon: " src))
      nil)
    (progn
      (setq tmpdir (getvar "TEMPPREFIX"))
      (if (or (null tmpdir) (= tmpdir "")) (setq tmpdir (getenv "TEMP")))
      (if (or (null tmpdir) (= tmpdir "")) (setq tmpdir "C:\\Temp\\"))
      (if (/= (substr tmpdir (strlen tmpdir) 1) "\\")
        (setq tmpdir (strcat tmpdir "\\")))
      (setq ms (itoa (getvar "MILLISECS")))
      (setq dst (strcat tmpdir "SDNL_" ms ".xlsx"))
      (if (findfile dst) (vl-catch-all-apply 'vl-file-delete (list dst)))

      ;; Cach 1: FileSystemObject.CopyFile - copy binary, ho tro Unicode/OneDrive tot hon.
      (setq fso (vl-catch-all-apply 'vlax-create-object (list "Scripting.FileSystemObject")))
      (if (not (vl-catch-all-error-p fso))
        (progn
          (setq r (vl-catch-all-apply 'vlax-invoke-method
                                     (list fso 'CopyFile src dst :vlax-true)))
          (SDT:safe-release fso)))

      ;; Kiem tra file sao chep. Neu khong co/size lech, thu COPY /B cua Windows va cho ket thuc.
      (setq szsrc (vl-file-size src)
            szdst (if (findfile dst) (vl-file-size dst) nil))
      (if (or (not (findfile dst))
              (null szdst)
              (< szdst 1000)
              (and szsrc (/= szsrc szdst)))
        (progn
          (if (findfile dst) (vl-catch-all-apply 'vl-file-delete (list dst)))
          (setq ws (vl-catch-all-apply 'vlax-create-object (list "WScript.Shell")))
          (if (not (vl-catch-all-error-p ws))
            (progn
              (setq cmd (strcat "cmd.exe /c copy /b /y \"" src "\" \"" dst "\" >nul"))
              (setq rc (vl-catch-all-apply 'vlax-invoke-method
                                          (list ws 'Run cmd 0 :vlax-true)))
              (SDT:safe-release ws)))
          (setq szdst (if (findfile dst) (vl-file-size dst) nil))))

      (if (and (findfile dst)
               szdst
               (>= szdst 1000)
               (or (null szsrc) (= szsrc szdst)))
        (progn
          (setq *SDT-TEMPFILE* dst)
          (princ (strcat "\n>> Da tao ban sao nhi phan: " dst
                         " (" (itoa szdst) " bytes)"))
          dst)
        (progn
          (setq *SDT-LAST-ERROR*
            (strcat "Copy XLSX khong hop le. Size nguon="
                    (if szsrc (itoa szsrc) "?")
                    ", size tam=" (if szdst (itoa szdst) "?")
                    ". Thu chuot phai file OneDrive > Always keep on this device."))
          nil)))))

(defun SDT:find-row (cells r1 r2 label / r s hit)
  (setq r r1 hit nil)
  (while (and (<= r r2) (null hit))
    (setq s (strcase (SDT:xcell cells r 1)))
    (if (= s (strcase label)) (setq hit r))
    (setq r (1+ r)))
  hit)


(defun SDF:clear-fast ()
  (setq *SDT-FAST-SA* nil
        *SDT-FAST-R0* 1
        *SDT-FAST-C0* 1
        *SDT-FAST-RLB* 1
        *SDT-FAST-CLB* 1
        *SDT-FAST-RUB* 0
        *SDT-FAST-CUB* 0
        *SDT-MAX-COL* 1))

(defun SDF:fast-cell (r c / rr cc got)
  (if (or (null *SDT-FAST-SA*) (null r) (null c))
    ""
    (progn
      (setq rr (+ *SDT-FAST-RLB* (- r *SDT-FAST-R0*))
            cc (+ *SDT-FAST-CLB* (- c *SDT-FAST-C0*)))
      (if (or (< rr *SDT-FAST-RLB*) (> rr *SDT-FAST-RUB*)
              (< cc *SDT-FAST-CLB*) (> cc *SDT-FAST-CUB*))
        ""
        (progn
          (setq got (vl-catch-all-apply 'vlax-safearray-get-element
                                        (list *SDT-FAST-SA* rr cc)))
          (if (vl-catch-all-error-p got)
            (progn
              (setq *SDT-LAST-ERROR*
                (strcat "Khong doc duoc du lieu RAM tai R" (itoa r)
                        "C" (itoa c) ": " (vl-catch-all-error-message got)))
              "")
            (SDT:xval->str got)))))))

(defun SDT:xcell (cells r c / )
  (if (or (null r) (null c))
    (progn
      (if (null *SDT-LAST-ERROR*)
        (setq *SDT-LAST-ERROR* "Thieu hang/cot bat buoc trong sheet KHOI_A_DXF."))
      "")
    (if (= cells 'SDF_FAST)
      (SDF:fast-cell r c)
      (progn
        (setq *SDT-LAST-ERROR* "Production chi cho phep doc Excel tu UsedRange trong RAM.")
        ""))))

(defun SDP:required-row (cells r1 r2 label / r)
  (setq r (SDT:find-row cells r1 r2 label))
  (if (and (null r) (null *SDT-LAST-ERROR*))
    (setq *SDT-LAST-ERROR*
      (strcat "Thieu dong bat buoc '" label "' trong cum tu bat dau tai dong " (itoa r1) ".")))
  r)

(defun SDP:any-circuit-data-p (cells c rows / hit r)
  (setq hit nil)
  (foreach r rows
    (if (and r (/= (SDT:xcell cells r c) ""))
      (setq hit T)))
  hit)

(defun SDT:read-one-panel (cells r1 r2 / matu nguon capnguon cbtong pdkd ptti kt
                                      rlo rcb rcap rlap rpha rpt rpkw rid rows c lastc cb lo
                                      cbp pp qq ptype pcap dtu)
  (setq matu     (SDT:xcell cells r1 2)
        rlo      (SDP:required-row cells r1 r2 "L\\U+1ED8 RA")
        rcb      (SDP:required-row cells r1 r2 "APTOMAT L\\U+1ED8 RA")
        rcap     (SDP:required-row cells r1 r2 "C\\U+00C1P \\U+0110I\\U+1EC6N")
        rlap     (SDP:required-row cells r1 r2 "C\\U+00C1CH L\\U+1EAEP \\U+0110\\U+1EB6T")
        rpha     (SDP:required-row cells r1 r2 "PHA")
        rpt      (SDP:required-row cells r1 r2 "PH\\U+1EE4 T\\U+1EA2I")
        rpkw     (SDP:required-row cells r1 r2 "P(KW)")
        rid      (SDP:required-row cells r1 r2 "ID M\\U+1EA0CH"))
  ;; Ghi nho hang ID MACH / LO RA cua tu de canh bao trung chi ra dung o Excel.
  (setq *SDC-PANEL-ROWS* (cons (list (SDT:trim matu) rid rlo) *SDC-PANEL-ROWS*))

  (setq nguon    (SDT:xcell cells (SDP:required-row cells r1 r2 "NGU\\U+1ED2N T\\U+1EEA") 2)
        capnguon (SDT:xcell cells (SDP:required-row cells r1 r2 "C\\U+00C1P C\\U+1EA4P NGU\\U+1ED2N") 2)
        cbtong   (SDT:xcell cells (SDP:required-row cells r1 r2 "APTOMAT T\\U+1ED4NG") 2)
        pdkd     (SDT:xcell cells (SDP:required-row cells r1 r2 "P\\U+0111 / K\\U+0111") 2)
        ptti     (SDT:xcell cells (SDP:required-row cells r1 r2 "Ptt / Itt") 2)
        kt       (SDT:xcell cells (SDP:required-row cells r1 r2 "K\\U+00CDCH TH\\U+01AF\\U+1EDAC T\\U+1EE6") 2))

  (setq cbp (SDT:parse-cb cbtong)
        pp  (SDT:parse-pair pdkd)
        qq  (SDT:parse-pair ptti)
        ptype (if (vl-string-search "." matu) "TP" "TU")
        lo '())

  (if (null *SDT-LAST-ERROR*)
    (progn
      (setq rows (list rlo rcb rcap rlap rpha rpt rpkw rid)
            c 2 lastc 1)
      (while (<= c *SDT-MAX-COL*)
        (if (SDP:any-circuit-data-p cells c rows)
          (setq lastc c))
        (setq c (1+ c)))

      ;; Doc lien tuc tu cot B den cot cuoi co du lieu. Cot bi trong o giua
      ;; van duoc tao record de validator bat loi thieu MA LO/ID MACH.
      (setq c 2)
      (while (<= c lastc)
        (setq cb (SDT:parse-cb (SDT:xcell cells rcb c)))
        (setq lo
          (append lo
            (list
              (list "LO"
                    (SDT:xcell cells rlo c)
                    (SDT:xcell cells rpt c)
                    (nth 0 cb) (nth 1 cb) (nth 2 cb) (nth 3 cb)
                    (SDT:xcell cells rcap c)
                    (SDT:xcell cells rlap c)
                    (SDT:xcell cells rpha c)
                    (SDT:xcell cells rpkw c)
                    (SDT:xcell cells rid c)))))
        (setq c (1+ c)))))

  (setq pcap
    (if (and lo (member (strcase (nth 1 cbp)) '("1P" "1P+N" "2P")))
      (strcase (SDT:fld (car lo) 9))
      ""))

  (setq dtu
    (list ptype matu matu kt
          (nth 0 cbp) (nth 1 cbp) (nth 2 cbp) (nth 3 cbp)
          capnguon (nth 0 pp) (nth 1 pp) (nth 0 qq) (nth 1 qq)
          "" nguon pcap))
  (list matu dtu lo))

(defun SDT:read-excel (fp / localfp xl books book sheets sh used rows cols nrow ncol
                           firstrow firstcol maxrow maxcol cells bulk sa lb1 ub1 lb2 ub2
                           r starts data r2 got result)
  (SDT:excel-cleanup)
  (SDF:clear-fast)
  (setq *SDT-LAST-ERROR* nil
        localfp (SDT:make-local-copy fp))
  (if (null localfp)
    (list 'SDT_ERROR *SDT-LAST-ERROR*)
    (progn
      (setq xl (vlax-create-object "Excel.Application")
            *SDT-XL* xl)
      (vlax-put-property xl 'Visible :vlax-false)
      (vlax-put-property xl 'DisplayAlerts :vlax-false)
      (setq books (SDT:unwrap (vlax-get-property xl 'Workbooks)) *SDT-BOOKS* books
            book  (SDT:unwrap (vlax-invoke-method books 'Open localfp)) *SDT-BOOK* book
            sheets (SDT:unwrap (vlax-get-property book 'Worksheets)) *SDT-SHEETS* sheets
            sh (SDT:unwrap (vlax-get-property sheets 'Item *SDT-SHEET*)) *SDT-SH* sh
            used (SDT:unwrap (vlax-get-property sh 'UsedRange)) *SDT-USED* used
            rows (SDT:unwrap (vlax-get-property used 'Rows)) *SDT-ROWS* rows
            cols (SDT:unwrap (vlax-get-property used 'Columns)))

      (setq nrow (vlax-get-property rows 'Count)
            ncol (vlax-get-property cols 'Count)
            firstrow (vlax-get-property used 'Row)
            firstcol (vlax-get-property used 'Column)
            maxrow (+ firstrow nrow -1)
            maxcol (+ firstcol ncol -1)
            *SDT-MAX-COL* maxcol)

      (princ (strcat "\n>> Excel: nap UsedRange " (itoa nrow) "x" (itoa ncol) " vao RAM 1 lan..."))
      (setq bulk (vl-catch-all-apply 'vlax-get-property (list used 'Value2)))
      (if (vl-catch-all-error-p bulk)
        (setq *SDT-LAST-ERROR*
          (strcat "Khong doc duoc UsedRange.Value2: " (vl-catch-all-error-message bulk)))
        (progn
          (setq sa (SDT:try-unvariant bulk)
                lb1 (vl-catch-all-apply 'vlax-safearray-get-l-bound (list sa 1))
                ub1 (vl-catch-all-apply 'vlax-safearray-get-u-bound (list sa 1))
                lb2 (vl-catch-all-apply 'vlax-safearray-get-l-bound (list sa 2))
                ub2 (vl-catch-all-apply 'vlax-safearray-get-u-bound (list sa 2)))
          (if (or (vl-catch-all-error-p lb1) (vl-catch-all-error-p ub1)
                  (vl-catch-all-error-p lb2) (vl-catch-all-error-p ub2))
            (setq *SDT-LAST-ERROR* "UsedRange.Value2 khong tra SAFEARRAY 2 chieu.")
            (progn
              (setq *SDT-FAST-SA* sa
                    *SDT-FAST-R0* firstrow *SDT-FAST-C0* firstcol
                    *SDT-FAST-RLB* lb1 *SDT-FAST-RUB* ub1
                    *SDT-FAST-CLB* lb2 *SDT-FAST-CUB* ub2
                    cells 'SDF_FAST)
              (setq starts '() r firstrow)
              (while (and (<= r maxrow) (null *SDT-LAST-ERROR*))
                (if (= (strcase (SDT:xcell cells r 1)) (strcase "T\\U+1EE6 \\U+0110I\\U+1EC6N"))
                  (setq starts (append starts (list r))))
                (setq r (1+ r)))
              (if (= (length starts) 0)
                (setq *SDT-LAST-ERROR* "Khong tim thay dong T\\U+1EE6 \\U+0110I\\U+1EC6N trong sheet KHOI_A_DXF.")
                (progn
                  (setq data '() r 0)
                  (while (and (< r (length starts)) (null *SDT-LAST-ERROR*))
                    (setq r2 (if (< (1+ r) (length starts))
                               (1- (nth (1+ r) starts)) maxrow)
                          got (vl-catch-all-apply 'SDT:read-one-panel
                                                (list cells (nth r starts) r2)))
                    (if (vl-catch-all-error-p got)
                      (setq *SDT-LAST-ERROR*
                        (strcat "Loi tach tu thu " (itoa (1+ r)) ": "
                                (vl-catch-all-error-message got)))
                      (setq data (append data (list got))))
                    (setq r (1+ r)))
                  (if (null *SDT-LAST-ERROR*)
                    (princ (strcat "\n>> Excel: da tach " (itoa (length data)) " tu trong RAM.")))))))))
      (setq result (if *SDT-LAST-ERROR* (list 'SDT_ERROR *SDT-LAST-ERROR*) data))
      (SDF:clear-fast)
      (SDT:safe-release cols)
      result)))


(defun SDT:ve-khung (x0 y0 n / W r)
  (setq W (max (+ *C1* (* (1- n) *P*) *LE*)
               (+ *C1* (* 3 *P*) *LE*)))

  (SDT:mk
    (list '(0 . "LWPOLYLINE") '(8 . "CDC-E-Equip") '(100 . "AcDbEntity")
          '(100 . "AcDbPolyline") '(90 . 2) '(70 . 0) (cons 43 *NET*)
          (list 10 x0 y0) (list 10 (+ x0 W) y0)))

  (foreach r *Y_BANG*
    (SDT:L2 (- x0 16.5) (+ y0 r) (+ x0 W -83.9) (+ y0 r) "CCU-E-Note"))
  (SDT:L2 (+ x0 W -83.9) (+ y0 -8944.7) (+ x0 W -83.9) (+ y0 -14503.5) "CCU-E-Note")

  (SDT:L2 (+ x0 *K_TRAI*) (+ y0 *K_DUOI*) (+ x0 W *K_PHAI*) (+ y0 *K_DUOI*) "CDC-E-Equip")
  (SDT:L2 (+ x0 W *K_PHAI*) (+ y0 *K_DUOI*) (+ x0 W *K_PHAI*) (+ y0 *K_TREN*) "CDC-E-Equip")
  (SDT:L2 (+ x0 W *K_PHAI*) (+ y0 *K_TREN*) (+ x0 *K_TRAI*) (+ y0 *K_TREN*) "CDC-E-Equip")
  (SDT:L2 (+ x0 *K_TRAI*) (+ y0 *K_TREN*) (+ x0 *K_TRAI*) (+ y0 *K_DUOI*) "CDC-E-Equip")

  (SDT:L2 (+ x0 W *TDT_R*) (+ y0 2531.9) (+ x0 W *TDT_L*) (+ y0 2531.9) "CDC-E-Equip")
  (SDT:mk
    (list '(0 . "LWPOLYLINE") '(8 . "CDC-E-Equip") '(100 . "AcDbEntity")
          '(100 . "AcDbPolyline") '(90 . 3) '(70 . 0)
          (list 10 (+ x0 W *TDT_R*) (+ y0 2010.0))
          (list 10 (+ x0 W *TDT_L*) (+ y0 2010.0))
          (list 10 (+ x0 W *TDT_L*) (+ y0 *K_TREN*))))

  (if (tblsearch "BLOCK" "A$C6A807301")
    (SDT:mk
      (list '(0 . "INSERT") '(8 . "CDC-E-Equip") '(2 . "A$C6A807301")
            (list 10 (+ x0 W *NOIDAT*) (+ y0 -2814.4) 0.0)
            '(41 . 0.91) '(42 . 0.91) '(43 . 0.91))))

  (foreach r *NHAN_NGANG*
    (SDT:L2 (+ x0 *NHAN_PHAI*) (+ y0 (car r)) (+ x0 (cadr r)) (+ y0 (car r)) "0"))
  (SDT:L2 (+ x0 -4566.4) (+ y0 4231.3) (+ x0 -4566.4) (+ y0 -14503.5) "0")
  (SDT:L2 (+ x0 -3839.1) (+ y0 3053.6) (+ x0 -3839.1) (+ y0 -2818.2) "0")
  (foreach r *NHAN*
    (SDT:TX2 (car r) (+ x0 (cadr r)) (+ y0 (caddr r)) 0 "0" 0.0))
  (SDT:TX2 "T\\U+1EE6 \\U+0110I\\U+1EC6N" (+ x0 -4202.8) (+ y0 -168.9) 1 "0" 90.0)
  W)

(defun SDT:ve-tu (x0 y0 matu dtu lo / n W i r pcap motpha)
  (setq *SDT-CURRENT-PANEL* matu)
  (setq n (length lo)
        W (SDT:ve-khung x0 y0 n))

  (setq pcap   (strcase (SDT:fld dtu 15))
        motpha (and (member (strcase (SDT:fld dtu 5)) '("1P" "1P+N" "2P"))
                    (member pcap '("A" "B" "C"))))

  (if (and motpha
           (vl-some '(lambda (r) (not (= (strcase (SDT:fld r 9)) pcap))) lo))
    (princ (strcat "\n   [CANH BAO PHA] " matu " co lo khac pha cap " pcap)))

  (SDT:chen
    (if (= (strcase (SDT:fld dtu 0)) "TP") "SDNL_TU_PHONG" "SDNL_TU")
    (+ x0 (/ W 2.0)) y0
    (list
      (cons "TIEU_DE" (strcat "S\\U+01A0 \\U+0110\\U+1ED2 NGUY\\U+00CAN L\\U+00DD " (SDT:fld dtu 2)))
      (cons "ID_MACH" (strcat matu "-AT"))
      (cons "AT_LOAI" (SDT:fld dtu 4))
      (cons "AT_CUC"  (SDT:fld dtu 5))
      (cons "AT_IDM"  (if (= (SDT:fld dtu 6) "") "" (strcat (SDT:fld dtu 6) "A")))
      (cons "AT_ICU"  (SDT:fld dtu 7))
      (cons "CAP_NGUON" (SDT:fld dtu 8))
      (cons "NGUON_TU" (if (= (SDT:fld dtu 14) "")
                          "T\\U+1EEA T\\U+1EE6 \\U+0110I\\U+1EC6N H\\U+1EA0 TH\\U+1EBE \\U+0110\\U+1EBEN"
                          (strcat "T\\U+1EEA T\\U+1EE6 " (SDT:fld dtu 14) " \\U+0110\\U+1EBEN")))))

  (SDT:chen "SDNL_PTT" (+ x0 W *PTT_DX*) y0
    (list
      (cons "TEN_TU" (SDT:fld dtu 2))
      (cons "KICH_THUOC" (if (= (SDT:fld dtu 3) "") *KICHTHUOC* (SDT:fld dtu 3)))
      (cons "DONG_PD" (if (= (SDT:fld dtu 9) "") ""
                        (strcat "P\\U+0111 =" (SDT:lead-zero (SDT:trim (SDT:fld dtu 9))) "kW, K\\U+0111=" (SDT:lead-zero (SDT:trim (SDT:fld dtu 10))))))
      (cons "DONG_PTT" (if (= (SDT:fld dtu 11) "") ""
                         (strcat "Ptt =" (SDT:lead-zero (SDT:trim (SDT:fld dtu 11))) "kW, Itt=" (SDT:lead-zero (SDT:trim (SDT:fld dtu 12))) "A")))))

  (setq i 0)
  (foreach r lo
    (SDT:chen "SDNL_LO" (+ x0 *C1* (* i *P*)) (+ y0 *LO_DY*)
      (list
        (cons "MA_MACH" (SDT:fld r 1))
        (cons "PHU_TAI" (SDT:fld r 2))
        (cons "LOAI_AT" (SDT:fld r 3))
        (cons "SO_CUC" (SDT:fld r 4))
        (cons "IDM" (if (= (SDT:fld r 5) "") "" (strcat (SDT:fld r 5) "A")))
        (cons "ICU" (SDT:fld r 6))
        (cons "CAP" (SDT:fld r 7))
        (cons "ONG" (SDT:fld r 8))
        (cons "PHA" (SDT:fld r 9))
        (cons "P_KW" (SDT:lead-zero (SDT:trim (SDT:fld r 10))))
        (cons "ID_MACH" (SDT:fld r 11))))
    (setq i (1+ i)))
  (setq *SDT-CURRENT-PANEL* nil)
  W)

(defun SDT:get-attr (obj tag / arr at hit t1)
  (setq hit nil)
  (if (= :vlax-true (vla-get-HasAttributes obj))
    (progn
      (setq arr (vlax-variant-value (vla-GetAttributes obj)))
      (foreach at (vlax-safearray->list arr)
        (setq t1 (strcase (vla-get-TagString at)))
        (if (= t1 (strcase tag))
          (setq hit (vla-get-TextString at))))))
  hit)

(defun SDT:set-attrs (obj alist / arr at tag v n)
  (setq n 0)
  (if (= :vlax-true (vla-get-HasAttributes obj))
    (progn
      (setq arr (vlax-variant-value (vla-GetAttributes obj)))
      (foreach at (vlax-safearray->list arr)
        (setq tag (strcase (vla-get-TagString at))
              v   (cdr (assoc tag alist)))
        (if v
          (progn
            (vla-put-TextString at (if (= v "") " " v))
            (setq n (1+ n)))))))
  n)

(defun SDT:index-add (idx key obj / a)
  ;; AutoCAD 2021 AutoLISP khong co SETCDR.
  ;; Cap nhat danh sach theo kieu functional bang SUBST.
  (if (setq a (assoc key idx))
    (subst (cons key (cons obj (cdr a))) a idx)
    (cons (cons key (list obj)) idx)))

(defun SDT:update-indexed (idx key alist / a objs obj n)
  (setq n 0)
  (if (setq a (assoc key idx))
    (progn
      (setq objs (cdr a))
      (foreach obj objs
        (SDT:set-attrs obj alist)
        (setq n (1+ n)))))
  n)

(defun SDT:tu-attrs (matu dtu)
  (list
    (cons "TIEU_DE" (strcat "S\\U+01A0 \\U+0110\\U+1ED2 NGUY\\U+00CAN L\\U+00DD " (SDT:fld dtu 2)))
    (cons "ID_MACH" (strcat matu "-AT"))
    (cons "AT_LOAI" (SDT:fld dtu 4))
    (cons "AT_CUC"  (SDT:fld dtu 5))
    (cons "AT_IDM"  (if (= (SDT:fld dtu 6) "") "" (strcat (SDT:fld dtu 6) "A")))
    (cons "AT_ICU"  (SDT:fld dtu 7))
    (cons "CAP_NGUON" (SDT:fld dtu 8))
    (cons "NGUON_TU" (if (= (SDT:fld dtu 14) "")
                        "T\\U+1EEA T\\U+1EE6 \\U+0110I\\U+1EC6N H\\U+1EA0 TH\\U+1EBE \\U+0110\\U+1EBEN"
                        (strcat "T\\U+1EEA T\\U+1EE6 " (SDT:fld dtu 14) " \\U+0110\\U+1EBEN")))))

(defun SDT:ptt-attrs (dtu)
  (list
    (cons "TEN_TU" (SDT:fld dtu 2))
    (cons "KICH_THUOC" (if (= (SDT:fld dtu 3) "") *KICHTHUOC* (SDT:fld dtu 3)))
    (cons "DONG_PD" (if (= (SDT:fld dtu 9) "") ""
                      (strcat "P\\U+0111 =" (SDT:lead-zero (SDT:trim (SDT:fld dtu 9))) "kW, K\\U+0111=" (SDT:lead-zero (SDT:trim (SDT:fld dtu 10))))))
    (cons "DONG_PTT" (if (= (SDT:fld dtu 11) "") ""
                       (strcat "Ptt =" (SDT:lead-zero (SDT:trim (SDT:fld dtu 11))) "kW, Itt=" (SDT:lead-zero (SDT:trim (SDT:fld dtu 12))) "A")))))

(defun SDT:lo-attrs (r)
  (list
    (cons "MA_MACH" (SDT:fld r 1))
    (cons "PHU_TAI" (SDT:fld r 2))
    (cons "LOAI_AT" (SDT:fld r 3))
    (cons "SO_CUC" (SDT:fld r 4))
    (cons "IDM" (if (= (SDT:fld r 5) "") "" (strcat (SDT:fld r 5) "A")))
    (cons "ICU" (SDT:fld r 6))
    (cons "CAP" (SDT:fld r 7))
    (cons "ONG" (SDT:fld r 8))
    (cons "PHA" (SDT:fld r 9))
    (cons "P_KW" (SDT:lead-zero (SDT:trim (SDT:fld r 10))))
    (cons "ID_MACH" (SDT:fld r 11))))

(defun SDT:ends-with (s suf / ls lf)
  (setq s (if s s "")
        suf (if suf suf "")
        ls (strlen s)
        lf (strlen suf))
  (and (>= ls lf)
       (= (strcase (substr s (+ 1 (- ls lf)) lf)) (strcase suf))))

(defun SDT:panel-id-of-block (obj / nm id ma suf)
  ;; Tra ve ma tu chu quan cua 1 trong 4 block SDNL.
  (setq nm (strcase (vla-get-Name obj)))
  (cond
    ((or (= nm "SDNL_TU") (= nm "SDNL_TU_PHONG"))
      (setq id (SDT:get-attr obj "ID_MACH"))
      (if (and id (SDT:ends-with id "-AT"))
        (substr id 1 (- (strlen id) 3))
        nil))
    ((= nm "SDNL_PTT")
      (SDT:get-attr obj "TEN_TU"))
    ((= nm "SDNL_LO")
      (setq id (SDT:get-attr obj "ID_MACH")
            ma (SDT:get-attr obj "MA_MACH")
            suf (if ma (strcat "-" ma) nil))
      (if (and id suf (SDT:ends-with id suf))
        (substr id 1 (- (strlen id) (strlen suf)))
        nil))
    (T nil)))

(defun SDT:block-point (obj / v p)
  (setq v (vl-catch-all-apply 'vla-get-InsertionPoint (list obj)))
  (if (vl-catch-all-error-p v)
    nil
    (progn
      (setq p (vl-catch-all-apply 'vlax-safearray->list
                                  (list (vlax-variant-value v))))
      (if (vl-catch-all-error-p p) nil p))))

(defun SDT:index-one-object (idx obj / nm id)
  (setq nm (strcase (vla-get-Name obj)))
  (cond
    ((or (= nm "SDNL_TU") (= nm "SDNL_TU_PHONG"))
      (setq id (SDT:get-attr obj "ID_MACH"))
      (if (and id (/= id ""))
        (SDT:index-add idx (strcat "P|" (strcase id)) obj)
        idx))
    ((= nm "SDNL_PTT")
      (setq id (SDT:get-attr obj "TEN_TU"))
      (if (and id (/= id ""))
        (SDT:index-add idx (strcat "T|" (strcase id)) obj)
        idx))
    ((= nm "SDNL_LO")
      (setq id (SDT:get-attr obj "ID_MACH"))
      (if (and id (/= id ""))
        (SDT:index-add idx (strcat "L|" (strcase id)) obj)
        idx))
    (T idx)))

(defun SDT:index-from-objects (objs / idx obj)
  (setq idx '())
  (foreach obj objs
    (if (= (vla-get-ObjectName obj) "AcDbBlockReference")
      (setq idx (SDT:index-one-object idx obj))))
  idx)

(defun SDT:find-panel-data (data panel / one hit)
  (setq hit nil)
  (foreach one data
    (if (= (strcase (car one)) (strcase panel))
      (setq hit one)))
  hit)

(defun SDT:panel-width (n)
  (max (+ *C1* (* (1- n) *P*) *LE*)
       (+ *C1* (* 3 *P*) *LE*)))

(defun SDT:panel-main-info (objs / obj nm mainpt nlo W x0 y0)
  (setq mainpt nil nlo 0)
  (foreach obj objs
    (if (= (vla-get-ObjectName obj) "AcDbBlockReference")
      (progn
        (setq nm (strcase (vla-get-Name obj)))
        (cond
          ((or (= nm "SDNL_TU") (= nm "SDNL_TU_PHONG"))
            (if (null mainpt) (setq mainpt (SDT:block-point obj))))
          ((= nm "SDNL_LO")
            (setq nlo (1+ nlo)))))))
  (if mainpt
    (progn
      (setq W  (SDT:panel-width nlo)
            x0 (- (car mainpt) (/ W 2.0))
            y0 (cadr mainpt))
      (list x0 y0 nlo W))
    nil))

(defun SDT:update-one-panel-data (idx one / matu dtu los r key n np nt nl)
  (setq matu (car one)
        dtu  (cadr one)
        los  (caddr one)
        np 0 nt 0 nl 0)
  (setq key (strcat "P|" (strcase (strcat matu "-AT")))
        n   (SDT:update-indexed idx key (SDT:tu-attrs matu dtu))
        np  (+ np n))
  (setq key (strcat "T|" (strcase matu))
        n   (SDT:update-indexed idx key (SDT:ptt-attrs dtu))
        nt  (+ nt n))
  (foreach r los
    (setq key (strcat "L|" (strcase (SDT:fld r 11)))
          n   (SDT:update-indexed idx key (SDT:lo-attrs r))
          nl  (+ nl n)))
  (list np nt nl))

(defun SDT:obj-center (en / obj mn mx a b r)
  (setq obj (vl-catch-all-apply 'vlax-ename->vla-object (list en)))
  (if (vl-catch-all-error-p obj)
    nil
    (progn
      (setq r (vl-catch-all-apply 'vla-GetBoundingBox (list obj 'mn 'mx)))
      (if (vl-catch-all-error-p r)
        nil
        (progn
          (setq a (vlax-safearray->list mn)
                b (vlax-safearray->list mx))
          (list (/ (+ (car a) (car b)) 2.0)
                (/ (+ (cadr a) (cadr b)) 2.0)
                0.0))))))

(defun SDC:blank-p (s)
  (or (null s) (= (SDT:trim s) "")))

(defun SDC:starts-with-ci (s prefix / ls lp)
  (setq s (if s s "")
        prefix (if prefix prefix "")
        ls (strlen s)
        lp (strlen prefix))
  (and (>= ls lp)
       (= (strcase (substr s 1 lp)) (strcase prefix))))

(defun SDC:add-msg (lst s)
  (if (member s lst) lst (append lst (list s))))

(defun SDC:one-phase-panel-p (dtu / p)
  (setq p (strcase (SDT:fld dtu 5)))
  (or (= p "1P") (= p "1P+N") (= p "2P")))

(defun SDC:norm (s)
  (strcase (SDT:trim (if s s ""))))

(defun SDC:count-norm (key lst / n x k)
  (setq n 0
        k (SDC:norm key))
  (foreach x lst
    (if (= (SDC:norm x) k)
      (setq n (1+ n))))
  n)

;; Vi tri o Excel cho canh bao trung (chi dung de bao, khong doi du lieu ve).
(if (not (boundp '*SDC-PANEL-ROWS*)) (setq *SDC-PANEL-ROWS* nil))
(defun SDC:col-letter (c / s)
  (setq s "")
  (while (> c 0)
    (setq s (strcat (chr (+ 65 (rem (1- c) 26))) s)
          c (/ (1- c) 26)))
  s)
(defun SDC:panel-rows (panel / hit x)
  (foreach x *SDC-PANEL-ROWS*
    (if (and (null hit) (= (SDC:norm (car x)) (SDC:norm panel)))
      (setq hit x)))
  hit)
;; k = thu tu lo trong tu (0 = cot B). rowkind: 1 = hang ID MACH, 2 = hang LO RA.
(defun SDC:cell-ref (panel k rowkind / pr row col)
  (setq pr  (SDC:panel-rows panel)
        row (if pr (nth rowkind pr))
        col (SDC:col-letter (+ k 2)))
  (if (numberp row) (strcat col (itoa row)) (strcat "cot " col)))

(defun SDC:validate-data (data / errs warns dups panelIds circuitIds one panel dtu los r
                                ma id phutai cbtype cbcuc cbin cap pha pkw prefix
                                maList expected k here first idFirst maFirst)
  ;; dups: trung MA LO / ID MACH - chi canh bao, khong chan ve.
  ;; Ket qua (errs warns dups); ham goi cu chi doc car/cadr nen van tuong thich.
  (setq errs '() warns '() dups '() panelIds '() circuitIds '() idFirst '())

  ;; Pass 1: thu ma tu va ID mach tren toan pham vi (kem o Excel lan dau gap).
  (foreach one data
    (setq panel (car one)
          los   (caddr one)
          k     0)
    (if (not (SDC:blank-p panel))
      (setq panelIds (append panelIds (list panel))))
    (foreach r los
      (setq id (SDT:fld r 11))
      (if (not (SDC:blank-p id))
        (progn
          (setq circuitIds (append circuitIds (list id)))
          (if (not (assoc (SDC:norm id) idFirst))
            (setq idFirst (cons (cons (SDC:norm id) (strcat panel " o " (SDC:cell-ref panel k 1)))
                                idFirst)))))
      (setq k (1+ k))))

  ;; Pass 2: kiem tra tung tu / tung lo.
  (foreach one data
    (setq panel (car one)
          dtu   (cadr one)
          los   (caddr one)
          maList '()
          maFirst '()
          k     0)

    ;; Thu MA MACH trong rieng tu nay de bat trung L01/L02...
    (foreach r los
      (setq ma (SDT:fld r 1))
      (if (not (SDC:blank-p ma))
        (progn
          (setq maList (append maList (list ma)))
          (if (not (assoc (SDC:norm ma) maFirst))
            (setq maFirst (cons (cons (SDC:norm ma) (SDC:cell-ref panel k 2)) maFirst)))))
      (setq k (1+ k)))
    (setq k 0)

    (if (SDC:blank-p panel)
      (setq errs (SDC:add-msg errs "Co TU bi thieu MA TU."))
      (progn
        (if (> (SDC:count-norm panel panelIds) 1)
          (setq errs (SDC:add-msg errs (strcat "Trung MA TU: " panel))))

        (if (SDC:blank-p (SDT:fld dtu 8))
          (setq warns (SDC:add-msg warns (strcat panel ": thieu CAP CAP NGUON."))))
        (if (or (SDC:blank-p (SDT:fld dtu 4))
                (SDC:blank-p (SDT:fld dtu 6)))
          (setq warns (SDC:add-msg warns (strcat panel ": thieu thong so APTOMAT TONG."))))

        (setq prefix (strcat (SDT:trim panel) "-"))
        (foreach r los
          (setq ma      (SDT:fld r 1)
                phutai  (SDT:fld r 2)
                cbtype  (SDT:fld r 3)
                cbcuc   (strcase (SDT:trim (SDT:fld r 4)))
                cbin    (SDT:fld r 5)
                cap     (SDT:fld r 7)
                pha     (strcase (SDT:trim (SDT:fld r 9)))
                pkw     (SDT:fld r 10)
                id      (SDT:fld r 11))

          ;; 1) Ma lo bat buoc va khong duoc trung trong cung tu.
          (if (SDC:blank-p ma)
            (setq errs (SDC:add-msg errs (strcat panel ": co LO bi thieu MA MACH.")))
            (if (> (SDC:count-norm ma maList) 1)
              (progn
                (setq here  (SDC:cell-ref panel k 2)
                      first (cdr (assoc (SDC:norm ma) maFirst)))
                (if (/= here first)
                  (setq dups (SDC:add-msg dups
                    (strcat panel ": MA LO " (SDT:trim ma) " o " here
                            " trung voi o " first " (hang LO RA)")))))))

          ;; 2) ID bat buoc, khong trung tren toan pham vi, va phai khop PANEL-MALO.
          (if (SDC:blank-p id)
            (setq errs (SDC:add-msg errs
              (strcat panel "/" ma ": thieu ID MACH.")))
            (progn
              (if (> (SDC:count-norm id circuitIds) 1)
                (progn
                  (setq here  (strcat panel " o " (SDC:cell-ref panel k 1))
                        first (cdr (assoc (SDC:norm id) idFirst)))
                  (if (/= here first)
                    (setq dups (SDC:add-msg dups
                      (strcat panel ": ID MACH " (SDT:trim id) " o "
                              (SDC:cell-ref panel k 1) " trung voi " first
                              " (hang ID MACH)"))))))

              (if (not (SDC:starts-with-ci (SDT:trim id) prefix))
                (setq errs (SDC:add-msg errs
                  (strcat panel "/" ma ": ID MACH khong thuoc dung tu -> " (SDT:trim id)))))

              (if (not (SDC:blank-p ma))
                (progn
                  (setq expected (strcat (SDT:trim panel) "-" (SDT:trim ma)))
                  (if (/= (SDC:norm id) (SDC:norm expected))
                    (setq errs (SDC:add-msg errs
                      (strcat panel "/" ma ": ID MACH khong khop MA LO. Dang co "
                              (SDT:trim id) " | phai la " expected))))))))

          ;; 3) Cac canh bao du lieu thiet ke - khong tu sua.
          (if (or (SDC:blank-p cbtype) (SDC:blank-p cbin))
            (setq warns (SDC:add-msg warns
              (strcat panel "/" ma ": thieu thong so APTOMAT LO RA."))))
          (if (SDC:blank-p cap)
            (setq warns (SDC:add-msg warns
              (strcat panel "/" ma ": thieu CAP DIEN."))))
          (if (SDC:blank-p phutai)
            (setq warns (SDC:add-msg warns
              (strcat panel "/" ma ": thieu PHU TAI."))))
          (if (SDC:blank-p pkw)
            (setq warns (SDC:add-msg warns
              (strcat panel "/" ma ": thieu P(kW)."))))

          (if (and (SDC:one-phase-panel-p dtu)
                   (or (= cbcuc "3P") (= pha "ABC")))
            (setq warns (SDC:add-msg warns
              (strcat panel "/" ma
                      ": tu 1 pha nhung lo khai bao 3P/ABC - can xac nhan."))))
          (setq k (1+ k)))))
  )
  (list errs warns dups))

;; In va hien hop thoai canh bao trung; khong chan ve.
(defun SDC:report-dups (dups / n s msg)
  (if dups
    (progn
      (SDC:print-list "CANH BAO TRUNG (VAN VE - KIEM TRA LAI EXCEL)" dups)
      (setq n 0 msg "")
      (foreach s dups
        (if (< n 10) (setq msg (strcat msg "\n - " s)))
        (setq n (1+ n)))
      (if (> n 10) (setq msg (strcat msg "\n ... con " (itoa (- n 10)) " muc nua (xem dong lenh).")))
      (alert (strcat "CANH BAO: du lieu co " (itoa n) " muc trung." msg
                     "\n\nVan ve so do. Nen sua Excel roi dung Cap nhat bo.")))))

(defun SDC:print-list (title lst / n s)
  (princ (strcat "\n" title ": " (itoa (length lst))))
  (setq n 0)
  (foreach s lst
    (if (< n 50)
      (princ (strcat "\n   - " s)))
    (setq n (1+ n)))
  (if (> (length lst) 50)
    (princ (strcat "\n   ... con " (itoa (- (length lst) 50)) " muc nua."))))


(defun SDP:block-tags (name / doc blks blk got obj tags)
  (setq tags '()
        doc (vla-get-ActiveDocument (vlax-get-acad-object))
        blks (vla-get-Blocks doc)
        got (vl-catch-all-apply 'vla-Item (list blks name)))
  (if (not (vl-catch-all-error-p got))
    (progn
      (setq blk got)
      (vlax-for obj blk
        (if (= (vla-get-ObjectName obj) "AcDbAttributeDefinition")
          (setq tags (cons (strcase (vla-get-TagString obj)) tags))))))
  tags)

(defun SDP:required-tags (name)
  (cond
    ((member (strcase name) '("SDNL_TU" "SDNL_TU_PHONG"))
      '("AT_LOAI" "AT_CUC" "AT_IDM" "AT_ICU" "TIEU_DE" "CAP_NGUON" "NGUON_TU" "ID_MACH"))
    ((= (strcase name) "SDNL_PTT")
      '("DONG_PD" "DONG_PTT" "TEN_TU" "KICH_THUOC"))
    ((= (strcase name) "SDNL_LO")
      '("LOAI_AT" "SO_CUC" "IDM" "ICU" "MA_MACH" "CAP" "ONG" "PHA" "PHU_TAI" "P_KW" "ID_MACH"))
    (T '())))

(defun SDP:blocks-ready-p (/ missing bad b tags req t1)
  (setq missing '() bad '())
  (foreach b '("SDNL_LO" "SDNL_TU" "SDNL_TU_PHONG" "SDNL_PTT")
    (if (not (tblsearch "BLOCK" b))
      (setq missing (append missing (list b)))
      (progn
        (setq tags (SDP:block-tags b)
              req (SDP:required-tags b))
        (foreach t1 req
          (if (not (member (strcase t1) tags))
            (setq bad (SDC:add-msg bad (strcat b ": thieu Attribute " t1))))))))
  (if missing
    (progn
      (princ "\n[DUNG] Ban ve thieu block bat buoc:")
      (foreach b missing (princ (strcat "\n   - " b)))))
  (if bad (SDC:print-list "LOI CAU TRUC BLOCK" bad))
  (if (not (tblsearch "BLOCK" "A$C6A807301"))
    (princ "\n[CANH BAO] Thieu block ky hieu noi dat A$C6A807301; phan ky hieu nay se khong duoc chen."))
  (and (= (length missing) 0) (= (length bad) 0)))

(defun SDP:confirm-validation (data title / val errs warns ans)
  (setq val (SDC:validate-data data)
        errs (car val)
        warns (cadr val))
  (princ (strcat "\n\n=== " title " ==="
                 "\n   So tu doc duoc: " (itoa (length data))))
  (SDC:print-list "LOI PHAI SUA" errs)
  (SDC:print-list "CANH BAO" warns)
  (if (null errs) (SDC:report-dups (caddr val)))
  (cond
    ((> (length errs) 0)
      (princ "\n[DUNG] Co loi du lieu. CAD chua bi thay doi.") nil)
    ((> (length warns) 0)
      (initget "Co Khong")
      (setq ans (getkword "\nCo canh bao. Van tiep tuc? [Co/Khong] <Khong>: "))
      (= ans "Co"))
    (T T)))

(defun SDP:draw-data (data / org doc i one loaitu ok err xcur ycur step W tong hang failed)
  (setq tong (length data))
  (if (= tong 0)
    (progn (princ "\n[DUNG] Excel khong co cum TU DIEN nao de ve.") nil)
    (progn
      (setq org (getpoint "\nBam diem dat TU TANG dau tien (ben trai): "))
      (if (null org)
        nil
        (progn
          (setq doc (vla-get-ActiveDocument (vlax-get-acad-object)))
          (SD153:undo-begin doc)
          (setq err (vl-catch-all-apply 'SDT:chuan-bi '()))
          (if (vl-catch-all-error-p err)
            (setq failed (strcat "Khong chuan bi duoc moi truong ve: " (vl-catch-all-error-message err))))
          (setq i 0 ok 0 hang -1 xcur (car org) ycur (cadr org))
          (while (and (< i tong) (null failed))
            (setq one (nth i data)
                  loaitu (strcase (SDT:fld (cadr one) 0)))
            (if (= loaitu "TU")
              (progn
                (setq hang (1+ hang)
                      xcur (car org)
                      ycur (- (cadr org) (* hang 26000.0)))))
            (setq err (vl-catch-all-apply 'SDT:ve-tu
                         (list xcur ycur (car one) (cadr one) (caddr one))))
            (if (vl-catch-all-error-p err)
              (setq failed (strcat (car one) ": " (vl-catch-all-error-message err)))
              (progn
                (setq W err step (+ W 4566.4 1402.8 2500.0)
                      ok (1+ ok) xcur (+ xcur step))))
            (setq i (1+ i)))
          (if (and (null failed) *SD153-ID*)
            (progn (setq err (vl-catch-all-apply 'SD153:commit nil))
              (if (vl-catch-all-error-p err) (setq failed (vl-catch-all-error-message err)))))
          (SD153:undo-end doc)
          (setq *SDT-CURRENT-PANEL* nil)
          (if failed
            (progn
              (princ (strcat "\n[LOI VE] " failed))
              (command-s "_.UNDO" 1)
              (vla-Regen doc 1)
              (princ "\n[DUNG] Da UNDO toan bo lan ve moi.") nil)
            (progn
              (vla-Regen doc 1)
              (princ (strcat "\n=== SDNLNEW XONG: " (itoa ok) "/" (itoa tong)
                             " tu, " (itoa (1+ hang)) " hang tang ===")) T)))))))

(defun SDP:existing-panel-conflicts (data / wanted doc msp obj en pid nm out one)
  ;; SDNLNEW khong duoc tao them bat ky PANEL_ID nao da co tren CAD,
  ;; ke ca khi cum cu bi hong/mat block TU nhung con hinh hoc XData.
  (setq wanted '() out '())
  (foreach one data (setq wanted (cons (SDC:norm (car one)) wanted)))
  (setq doc (vla-get-ActiveDocument (vlax-get-acad-object))
        msp (vla-get-ModelSpace doc))
  (vlax-for obj msp
    (setq en (vlax-vla-object->ename obj)
          pid (SDT:get-panel-tag en))
    (if (and pid (/= (SDT:trim pid) "") (member (SDC:norm pid) wanted))
      (setq out (SDC:add-msg out (SDT:trim pid))))
    (if (= (vla-get-ObjectName obj) "AcDbBlockReference")
      (progn
        (setq nm (strcase (vla-get-Name obj)))
        (if (SDP:known-block-p nm)
          (progn
            (setq pid (SDT:panel-id-of-block obj))
            (if (and pid (/= (SDT:trim pid) "") (member (SDC:norm pid) wanted))
              (setq out (SDC:add-msg out (SDT:trim pid)))))))))
  out)

(defun SDP:known-block-p (nm)
  (member (strcase (if nm nm "")) '("SDNL_TU" "SDNL_TU_PHONG" "SDNL_PTT" "SDNL_LO")))

(defun SDP:main-block-p (nm)
  (member (strcase (if nm nm "")) '("SDNL_TU" "SDNL_TU_PHONG")))

(defun SDP:pair-add (recs panel anchor ismain / key a oldpanel oldanchor oldmain newrec)
  (setq key (SDC:norm panel) a (assoc key recs))
  (if a
    (setq oldpanel (nth 1 a) oldanchor (nth 2 a) oldmain (nth 3 a))
    (setq oldpanel panel oldanchor nil oldmain nil))
  (setq newrec
    (list key oldpanel
          (if (or ismain (null oldanchor)) anchor oldanchor)
          (or oldmain ismain)))
  (if a (subst newrec a recs) (cons newrec recs)))

(defun SDP:selection-pairs (ss / recs errs i en obj nm tag derived pid anchor h ismain a out)
  ;; Duyet selection dung 1 lan. Doi tuong co PANEL_ID XData nao cung co the
  ;; nhan dien pham vi; block SDNL duoc kiem tra them Attribute ID.
  (setq recs '() errs '() i 0)
  (if ss
    (while (< i (sslength ss))
      (setq en (ssname ss i)
            obj (vl-catch-all-apply 'vlax-ename->vla-object (list en))
            nm "" derived nil ismain nil)
      (if (not (vl-catch-all-error-p obj))
        (progn
          (setq tag (SDT:get-panel-tag en))
          (if (= (vla-get-ObjectName obj) "AcDbBlockReference")
            (progn
              (setq nm (strcase (vla-get-Name obj)))
              (if (SDP:known-block-p nm)
                (progn
                  (setq derived (SDT:panel-id-of-block obj)
                        h (vla-get-Handle obj)
                        ismain (SDP:main-block-p nm))
                  (if (or (null tag) (= (SDT:trim tag) ""))
                    (setq errs (SDC:add-msg errs
                      (strcat "CAD block " nm " handle " h ": thieu PANEL_ID XData."))))
                  (if (or (null derived) (= (SDT:trim derived) ""))
                    (setq errs (SDC:add-msg errs
                      (strcat "CAD block " nm " handle " h ": Attribute ID khong hop le."))))
                  (if (and tag derived (/= (SDT:trim tag) "") (/= (SDT:trim derived) "")
                           (/= (SDC:norm tag) (SDC:norm derived)))
                    (setq errs (SDC:add-msg errs
                      (strcat "CAD block " nm " handle " h ": PANEL_ID=" tag
                              " khong khop Attribute=" derived "."))))))))
          (setq pid (if (and tag (/= (SDT:trim tag) "")) tag derived))
          (if (and pid (/= (SDT:trim pid) ""))
            (progn
              (setq anchor (SDT:obj-center en))
              (if anchor (setq recs (SDP:pair-add recs pid anchor ismain)))))))
      (setq i (1+ i))))
  (setq out '())
  (foreach a recs
    (setq out (append out (list (list (nth 1 a) (nth 2 a))))))
  (list out errs))

(defun SDP:scope-pair-from-en (en / obj nm tag derived pid anchor h errs)
  (setq errs '() obj (vl-catch-all-apply 'vlax-ename->vla-object (list en)))
  (if (vl-catch-all-error-p obj)
    (list nil (list "Khong doc duoc doi tuong CAD da chon."))
    (progn
      (setq tag (SDT:get-panel-tag en)
            derived nil)
      (if (= (vla-get-ObjectName obj) "AcDbBlockReference")
        (progn
          (setq nm (strcase (vla-get-Name obj)))
          (if (SDP:known-block-p nm)
            (setq derived (SDT:panel-id-of-block obj)))))
      (if (and (= (vla-get-ObjectName obj) "AcDbBlockReference")
               (SDP:known-block-p nm)
               (or (null tag) (= (SDT:trim tag) "")))
        (setq errs (SDC:add-msg errs
          (strcat "CAD block " nm " handle " (vla-get-Handle obj) ": thieu PANEL_ID XData."))))
      (if (and tag derived (/= (SDT:trim tag) "") (/= (SDT:trim derived) "")
               (/= (SDC:norm tag) (SDC:norm derived)))
        (setq errs (SDC:add-msg errs
          (strcat "PANEL_ID=" tag " khong khop Attribute=" derived "."))))
      (setq pid (if (and tag (/= (SDT:trim tag) "")) tag derived)
            anchor (SDT:obj-center en))
      (if (and pid anchor)
        (list (list pid anchor) errs)
        (list nil (SDC:add-msg errs "Khong xac dinh duoc PANEL_ID/vi tri cua tu."))))))

;; Inventory record: (KEY PANEL ANCHOR OBJECTS NTU NPTT NLO)
(defun SDP:inventory-init (pairs / inv p)
  (setq inv '())
  (foreach p pairs
    (setq inv (cons (list (SDC:norm (car p)) (car p) (cadr p) '() 0 0 0) inv)))
  inv)

(defun SDP:inventory-add (inv pid obj nm / key a objs ntu nptt nlo newa)
  (setq key (SDC:norm pid) a (assoc key inv))
  (if a
    (progn
      (setq objs (cons obj (nth 3 a)) ntu (nth 4 a) nptt (nth 5 a) nlo (nth 6 a))
      (cond
        ((SDP:main-block-p nm) (setq ntu (1+ ntu)))
        ((= (strcase (if nm nm "")) "SDNL_PTT") (setq nptt (1+ nptt)))
        ((= (strcase (if nm nm "")) "SDNL_LO") (setq nlo (1+ nlo))))
      (setq newa (list key (nth 1 a) (nth 2 a) objs ntu nptt nlo))
      (subst newa a inv))
    inv))

(defun SDP:scan-scope-once (pairs initialErrs / inv errs doc msp obj en nm known tag derived pid a aTag aDer ctr h)
  (setq inv (SDP:inventory-init pairs)
        errs initialErrs
        doc (vla-get-ActiveDocument (vlax-get-acad-object))
        msp (vla-get-ModelSpace doc))
  (vlax-for obj msp
    (if (SD153:allowed (vlax-vla-object->ename obj)) (progn
    (setq en (vlax-vla-object->ename obj)
          nm "" known nil derived nil)
    (if (= (vla-get-ObjectName obj) "AcDbBlockReference")
      (progn
        (setq nm (strcase (vla-get-Name obj)) known (SDP:known-block-p nm))
        (if known (setq derived (SDT:panel-id-of-block obj)))))
    (setq tag (SDT:get-panel-tag en)
          aTag (if (and tag (/= (SDT:trim tag) "")) (assoc (SDC:norm tag) inv) nil)
          aDer (if (and derived (/= (SDT:trim derived) "")) (assoc (SDC:norm derived) inv) nil)
          a (if aTag aTag aDer)
          pid (if (and tag (/= (SDT:trim tag) "")) tag derived))
    (if a
      (progn
        (setq ctr (SDT:obj-center en))
        (if (and ctr (nth 2 a) (<= (distance ctr (nth 2 a)) 50000.0))
          (progn
            (if known
              (progn
                (setq h (vla-get-Handle obj))
                (if (or (null tag) (= (SDT:trim tag) ""))
                  (setq errs (SDC:add-msg errs
                    (strcat "CAD block " nm " handle " h ": thieu PANEL_ID XData."))))
                (if (or (null derived) (= (SDT:trim derived) ""))
                  (setq errs (SDC:add-msg errs
                    (strcat "CAD block " nm " handle " h ": Attribute ID khong hop le."))))
                (if (and tag derived (/= (SDT:trim tag) "") (/= (SDT:trim derived) "")
                         (/= (SDC:norm tag) (SDC:norm derived)))
                  (setq errs (SDC:add-msg errs
                    (strcat "CAD block " nm " handle " h ": PANEL_ID=" tag
                            " khong khop Attribute=" derived "."))))))
            ;; Chi doi tuong co XData hop le moi duoc dua vao danh sach xoa/rebuild.
            (if (and tag (/= (SDT:trim tag) "") (= (SDC:norm tag) (car a)))
              (setq inv (SDP:inventory-add inv tag obj nm))))
          ;; Cung PANEL_ID/Attribute ID nhung nam xa cum da chon: khong tu dong xoa.
          ;; Day la dau hieu ban sao/trung PANEL_ID can nguoi dung xu ly truoc.
          (if known
            (setq errs (SDC:add-msg errs
              (strcat "CAD block " nm " handle " (vla-get-Handle obj)
                      ": PANEL_ID " pid " xuat hien o vi tri khac -> nghi trung PANEL_ID."))))))))))
  (list inv errs))

(defun SDP:excel-signature (one / out r)
  (setq out '())
  (foreach r (caddr one)
    (setq out (append out (list (SDC:norm (SDT:fld r 11))))))
  out)

(defun SDP:cad-lo-signature (objs / tmp obj nm id pt item out)
  (setq tmp '())
  (foreach obj objs
    (if (= (vla-get-ObjectName obj) "AcDbBlockReference")
      (progn
        (setq nm (strcase (vla-get-Name obj)))
        (if (= nm "SDNL_LO")
          (progn
            (setq id (SDT:get-attr obj "ID_MACH")
                  pt (SDT:block-point obj))
            (if (and id pt)
              (setq tmp (cons (list (car pt) (SDC:norm id)) tmp))))))))
  (setq tmp (vl-sort tmp '(lambda (a b) (< (car a) (car b))))
        out '())
  (foreach item tmp (setq out (append out (list (cadr item)))))
  out)

(defun SDP:inventory-have-panel-p (inv panel / hit a)
  (setq hit nil)
  (foreach a inv (if (= (car a) (SDC:norm panel)) (setq hit T)))
  hit)

(defun SDP:check-integrity (inv scanErrs data fullScope / errs changes a panel ntu nptt nlo one expected cadSig xlsSig oneData)
  (setq errs scanErrs changes '())
  (foreach a inv
    (setq panel (nth 1 a) ntu (nth 4 a) nptt (nth 5 a) nlo (nth 6 a)
          one (SDT:find-panel-data data panel))
    (cond
      ((= ntu 0) (setq errs (SDC:add-msg errs (strcat panel ": thieu block TU chu quan."))))
      ((> ntu 1) (setq errs (SDC:add-msg errs
        (strcat "Trung PANEL_ID tren CAD: " panel " co " (itoa ntu) " block TU chu quan.")))))
    (if (/= nptt 1)
      (setq errs (SDC:add-msg errs
        (strcat panel ": so block PTT = " (itoa nptt) " (phai = 1)."))))
    (if (null one)
      (setq errs (SDC:add-msg errs (strcat "CAD co TU " panel " nhung Excel khong co.")))
      (progn
        (setq expected (length (caddr one))
              cadSig (SDP:cad-lo-signature (nth 3 a))
              xlsSig (SDP:excel-signature one))
        (cond
          ((/= nlo expected)
            (setq changes (SDC:add-msg changes
              (strcat panel ": CAD " (itoa nlo) " lo / Excel " (itoa expected) " lo -> REBUILD."))))
          ((not (equal cadSig xlsSig))
            (setq changes (SDC:add-msg changes
              (strcat panel ": ma/ID/thu tu lo thay doi -> REBUILD."))))))))
  (if fullScope
    (foreach oneData data
      (if (not (SDP:inventory-have-panel-p inv (car oneData)))
        (setq errs (SDC:add-msg errs
          (strcat "Excel co TU " (car oneData) " nhung CAD TOAN KHOI khong co."))))))
  (SDC:print-list "LOI TOAN VEN CAD" errs)
  (SDC:print-list "THAY DOI CAU TRUC" changes)
  (list errs changes))

(defun SDP:tasks-from-inventory (inv data / tasks a panel one objs info cadSig xlsSig state)
  (setq tasks '())
  (foreach a inv
    (setq panel (nth 1 a) one (SDT:find-panel-data data panel) objs (reverse (nth 3 a))
          info (SDT:panel-main-info objs))
    (if (and one info)
      (progn
        (setq cadSig (SDP:cad-lo-signature objs)
              xlsSig (SDP:excel-signature one)
              state (if (and (= (nth 2 info) (length (caddr one))) (equal cadSig xlsSig))
                      "UPDATE" "REBUILD"))
        (setq tasks (append tasks (list (list state panel one objs info)))))))
  tasks)

(defun SDP:safe-delete-objects (objs / obj r bad)
  (setq bad nil)
  (foreach obj objs
    (setq r (vl-catch-all-apply 'vla-Delete (list obj)))
    (if (vl-catch-all-error-p r)
      (setq bad (strcat "Khong xoa duoc entity handle " (vla-get-Handle obj) ": "
                        (vl-catch-all-error-message r)))))
  (if bad (list 'SDT_ERROR bad) T))

(defun SDP:process-tasks-atomic (tasks / doc rest tsk state panel one objs info r v upd
                                       nupdate nrebuild nptu nptt nplo failed)
  (setq doc (vla-get-ActiveDocument (vlax-get-acad-object))
        rest tasks nupdate 0 nrebuild 0 nptu 0 nptt 0 nplo 0 failed nil)
  (SD153:undo-begin doc)
  (if (SDP:tasks-have-state tasks "REBUILD")
    (setq r (vl-catch-all-apply 'SDT:chuan-bi '())))
  (if (and (SDP:tasks-have-state tasks "REBUILD") (vl-catch-all-error-p r))
    (setq failed (strcat "Khong chuan bi duoc moi truong ve: " (vl-catch-all-error-message r))))
  (while (and rest (null failed))
    (setq tsk (car rest) rest (cdr rest)
          state (nth 0 tsk) panel (nth 1 tsk) one (nth 2 tsk)
          objs (nth 3 tsk) info (nth 4 tsk))
    (cond
      ((= state "UPDATE")
        (setq r (vl-catch-all-apply 'SDT:update-one-panel-data
                                    (list (SDT:index-from-objects objs) one)))
        (if (vl-catch-all-error-p r)
          (setq failed (strcat panel ": loi update Attribute: " (vl-catch-all-error-message r)))
          (progn
            (setq upd r nupdate (1+ nupdate)
                  nptu (+ nptu (nth 0 upd)) nptt (+ nptt (nth 1 upd)) nplo (+ nplo (nth 2 upd)))
            (if (or (/= (nth 0 upd) 1) (/= (nth 1 upd) 1) (/= (nth 2 upd) (length (caddr one))))
              (setq failed (strcat panel ": so block cap nhat khong khop du kien."))))))
      ((= state "REBUILD")
        (setq r (SDP:safe-delete-objects objs))
        (if (and (listp r) (= (car r) 'SDT_ERROR))
          (setq failed (strcat panel ": " (cadr r)))
          (progn
            (setq v (vl-catch-all-apply 'SDT:ve-tu
                      (list (nth 0 info) (nth 1 info) (car one) (cadr one) (caddr one))))
            (if (vl-catch-all-error-p v)
              (setq failed (strcat panel ": loi ve lai: " (vl-catch-all-error-message v)))
              (setq nrebuild (1+ nrebuild))))))))
  (if (and (null failed) *SD153-ID*)
    (progn (setq r (vl-catch-all-apply 'SD153:commit nil))
      (if (vl-catch-all-error-p r) (setq failed (vl-catch-all-error-message r)))))
  (SD153:undo-end doc)
  (setq *SDT-CURRENT-PANEL* nil)
  (if failed
    (progn
      (princ (strcat "\n[LOI] " failed))
      (command-s "_.UNDO" 1)
      (vla-Regen doc 1)
      (princ "\n[DUNG] Da UNDO toan bo dot cap nhat; CAD tro ve trang thai truoc lenh.")
      (list nil 0 0 0 0 0))
    (progn
      (vla-Regen doc 1)
      (list T nupdate nrebuild nptu nptt nplo))))

(defun SDP:filter-data-by-pairs (data pairs / out one p hit)
  (setq out '())
  (foreach one data
    (setq hit nil)
    (foreach p pairs
      (if (= (SDC:norm (car one)) (SDC:norm (car p))) (setq hit T)))
    (if hit (setq out (append out (list one)))))
  out)


(defun SDP:confirm-update-validation (data pairs fullScope / fullVal fullErrs scopeData scopeVal scopeWarns ans)
  (setq fullVal (SDC:validate-data data)
        fullErrs (car fullVal)
        scopeData (if fullScope data (SDP:filter-data-by-pairs data pairs))
        scopeVal (SDC:validate-data scopeData)
        scopeWarns (cadr scopeVal))
  (princ (strcat "\n\n=== KIEM TRA DU LIEU TRUOC CAP NHAT ==="
                 "\n   So tu trong Excel: " (itoa (length data))
                 "\n   So tu trong pham vi: " (itoa (length scopeData))))
  (SDC:print-list "LOI PHAI SUA - TOAN FILE" fullErrs)
  (SDC:print-list "CANH BAO - PHAM VI CAP NHAT" scopeWarns)
  (SDC:print-list "CANH BAO TRUNG (van cap nhat)" (caddr fullVal))
  (cond
    ((> (length fullErrs) 0)
      (princ "\n[DUNG] Excel co loi. CAD chua bi thay doi.") nil)
    ((> (length scopeWarns) 0)
      (initget "Co Khong")
      (setq ans (getkword "\nPham vi cap nhat co canh bao. Van tiep tuc? [Co/Khong] <Khong>: "))
      (= ans "Co"))
    (T T)))

(defun SDP:tasks-have-state (tasks state / hit tsk)
  (setq hit nil)
  (foreach tsk tasks (if (= (car tsk) state) (setq hit T)))
  hit)

(defun SDP:update-scope (pairs initialErrs scopeName fullScope / fp err data scan inv scanErrs integ ierr changes tasks res)
  (if (null pairs)
    (princ "\n[DUNG] Khong nhan dien duoc tu nao trong pham vi chon.")
    (progn
      (setq fp (getfiled "Chon file Excel du lieu SDNL de CAP NHAT" "" "xlsx" 0))
      (if fp
        (progn
          (princ "\n>> Dang doc Excel UsedRange 1 lan...")
          (setq err (vl-catch-all-apply 'SDT:read-excel (list fp)))
          (SDT:excel-cleanup)
          (cond
            ((vl-catch-all-error-p err)
              (princ (strcat "\n[LOI EXCEL] " (vl-catch-all-error-message err))))
            ((and (listp err) (= (car err) 'SDT_ERROR))
              (princ (strcat "\n[LOI EXCEL] " (cadr err))))
            (T
              (setq data err)
              (if (SDP:confirm-update-validation data pairs fullScope)
                (progn
                  (princ "\n>> Dang lap inventory CAD 1 lan cho pham vi chon...")
                  (setq scan (SDP:scan-scope-once pairs initialErrs)
                        inv (car scan)
                        scanErrs (cadr scan)
                        integ (SDP:check-integrity inv scanErrs data fullScope)
                        ierr (car integ)
                        changes (cadr integ))
                  (if (> (length ierr) 0)
                    (princ "\n[DUNG] Loi toan ven CAD/Excel. CAD chua bi thay doi.")
                    (progn
                      (setq tasks (SDP:tasks-from-inventory inv data))
                      (if (/= (length tasks) (length inv))
                        (princ "\n[DUNG] Khong lap duoc task cho tat ca tu trong pham vi. CAD chua bi thay doi.")
                        (progn
                          (princ
                            (strcat "\n>> " scopeName ": " (itoa (length tasks)) " tu; "
                                    (itoa (length changes)) " tu can rebuild."))
                          (setq res (SDP:process-tasks-atomic tasks))
                          (if (car res)
                            (princ
                              (strcat "\n=== SDNLUPDATE XONG - " scopeName " ==="
                                      "\n   Tu UPDATE: " (itoa (nth 1 res))
                                      "\n   Tu REBUILD: " (itoa (nth 2 res))
                                      "\n   Block TU cap nhat: " (itoa (nth 3 res))
                                      "\n   Block PTT cap nhat: " (itoa (nth 4 res))
                                      "\n   Block LO cap nhat: " (itoa (nth 5 res)))))))))))))))))
  (princ))

(defun c:SDNLCHECK (/ fp err data val errs warns)
  (setq fp (getfiled "Chon file Excel du lieu SDNL de KIEM TRA" "" "xlsx" 0))
  (if fp
    (progn
      (setq err (vl-catch-all-apply 'SDT:read-excel (list fp)))
      (SDT:excel-cleanup)
      (cond
        ((vl-catch-all-error-p err)
          (princ (strcat "\n[LOI EXCEL] " (vl-catch-all-error-message err))))
        ((and (listp err) (= (car err) 'SDT_ERROR))
          (princ (strcat "\n[LOI EXCEL] " (cadr err))))
        (T
          (setq data err val (SDC:validate-data data) errs (car val) warns (cadr val))
          (princ (strcat "\n\n=== SDNLCHECK KET QUA ===\n   So tu: " (itoa (length data))))
          (SDC:print-list "LOI PHAI SUA" errs)
          (SDC:print-list "CANH BAO" warns)
          (SDC:print-list "CANH BAO TRUNG (van ve)" (caddr val))
          (if (= (length errs) 0)
            (princ "\n=== DAT: khong co loi chan ve/cap nhat. ===")
            (princ "\n=== CHUA DAT: sua loi truoc khi ve/cap nhat. ==="))))))
  (princ))

(defun c:SDNLNEW (/ fp err data conflicts)
  (if (SDP:blocks-ready-p)
    (progn
      (setq fp (getfiled "Chon file Excel du lieu SDNL de VE MOI" "" "xlsx" 0))
      (if fp
        (progn
          (setq err (vl-catch-all-apply 'SDT:read-excel (list fp)))
          (SDT:excel-cleanup)
          (cond
            ((vl-catch-all-error-p err)
              (princ (strcat "\n[LOI EXCEL] " (vl-catch-all-error-message err))))
            ((and (listp err) (= (car err) 'SDT_ERROR))
              (princ (strcat "\n[LOI EXCEL] " (cadr err))))
            (T
              (setq data err)
              (if (SDP:confirm-validation data "KIEM TRA DU LIEU TRUOC VE MOI")
                (progn
                  (setq conflicts (SDP:existing-panel-conflicts data))
                  (if conflicts
                    (progn
                      (SDC:print-list "PANEL_ID DA TON TAI TREN CAD" conflicts)
                      (princ "\n[DUNG] SDNLNEW khong tao ban sao trung PANEL_ID. Hay dung SDNLUPDATE."))
                    (SDP:draw-data data))))))))))
  (princ))

(defun c:SDNLUPDATE (/ mode sel pairResult pair pairErrs ss scanSel pairs selErrs p1 p2)
  (if (SDP:blocks-ready-p)
    (progn
      (initget "MotTu Vung ToanKhoi")
      (setq mode (getkword "\nChe do cap nhat [MotTu/Vung/ToanKhoi] <MotTu>: "))
      (if (null mode) (setq mode "MotTu"))
      (cond
        ((= mode "MotTu")
          (setq sel (entsel "\nChon 1 doi tuong thuoc tu: "))
          (if (null sel)
            (princ "\n[DUNG] Chua chon doi tuong.")
            (progn
              (setq pairResult (SDP:scope-pair-from-en (car sel))
                    pair (car pairResult) pairErrs (cadr pairResult))
              (if pair
                (progn
                  (princ (strcat "\n>> MOT TU nhan dien PANEL_ID: " (car pair)))
                  (SDP:update-scope (list pair) pairErrs "MOT TU" nil))
                (SDC:print-list "LOI NHAN DIEN CAD" pairErrs)))))
        ((= mode "Vung")
          (setq ss (ssget))
          (if (null ss)
            (princ "\n[DUNG] Chua chon doi tuong nao.")
            (progn
              (setq scanSel (SDP:selection-pairs ss)
                    pairs (car scanSel) selErrs (cadr scanSel))
              (SDP:update-scope pairs selErrs "VUNG" nil))))
        ((= mode "ToanKhoi")
          (setq p1 (getpoint "\nGoc thu nhat bao quanh day du bo so do: "))
          (if p1
            (progn
              (setq p2 (getcorner p1 "\nGoc doi dien: "))
              (if p2
                (progn
                  (setq ss (ssget "_C" p1 p2))
                  (if (null ss)
                    (princ "\n[DUNG] Khung chon khong co doi tuong nao.")
                    (progn
                      (setq scanSel (SDP:selection-pairs ss)
                            pairs (car scanSel) selErrs (cadr scanSel))
                      (SDP:update-scope pairs selErrs "TOAN KHOI" T)))))))))))
  (princ))

(princ "\n=== SDNL PRODUCTION DA NAP ===")
(princ "\n    SDNLCHECK / SDNLNEW / SDNLUPDATE")
(princ)


;;; Direct XLSX adapter. Original panel parser, validator, geometry and atomic updater retained.
(vl-load-com)
(defun SDT:xcell (cells r c / row pair)
 (if (or (null r) (null c))
  (progn (setq *SDT-LAST-ERROR* "Thieu hang/cot bat buoc.") "")
  (progn (setq row (assoc r tvxd-rows) pair (assoc c (cdr row)))
   (if pair (SDT:xval->str (cadr pair)) ""))))
(defun SDT:read-excel (fp / got starts rows r data end one)
 (setq *SDT-LAST-ERROR* nil got (TVXDXLSX fp))
 (if (= (type got) 'STR) (list 'SDT_ERROR got)
  (progn
   (setq *SDT-MAX-COL* (car got) tvxd-rows (cadr got) starts nil data nil)
   (foreach row tvxd-rows (if (= (strcase (SDT:xcell nil (car row) 1)) (strcase "T\\U+1EE6 \\U+0110I\\U+1EC6N")) (setq starts (append starts (list (car row))))))
   (setq rows starts)
   (while (and rows (null *SDT-LAST-ERROR*))
    (setq r (car rows) rows (cdr rows) end (if rows (1- (car rows)) (car (last tvxd-rows))))
    (setq one (SDT:read-one-panel nil r end) data (append data (list one))))
   (cond (*SDT-LAST-ERROR* (list 'SDT_ERROR *SDT-LAST-ERROR*)) ((null data) (list 'SDT_ERROR "Khong co tu dien")) (T data)))))
(defun TVXD:read-data (/ d val)
 (setq d (SDT:read-excel tvxd-file))
 (if (= (car d) 'SDT_ERROR) (progn (TVXDTOOLDONE "FAIL" (cadr d)) nil)
  (progn (setq val (SDC:validate-data d))
   (if (or (car val) (cadr val))
    (progn (SDC:print-list "LOI" (car val)) (SDC:print-list "CANH BAO" (cadr val)) (TVXDTOOLDONE "FAIL" (vl-prin1-to-string (list (car val) (cadr val)))) nil)
    (progn (SDC:report-dups (caddr val)) d)))))
(defun TVXD:save-data (d / f)
 (setq f (open (strcat tvxd-out "/PARSED_EXCEL.lspdata") "w")) (write-line (vl-prin1-to-string d) f) (close f))
(defun TVXD:finish (d msg)
 (setvar "FILEDIA" 0) (command "_.ZOOM" "_E")
 (command "_.SAVEAS" "2018" tvxd-save)
 (if (findfile tvxd-save) (progn (TVXD:save-data d) (TVXDTOOLDONE "PASS" msg)) (TVXDTOOLDONE "FAIL" "Chua luu duoc DWG")))
(defun TVXD:new (/ d ok conflicts)
 (setq d (TVXD:read-data))
 (if d
  (if (not (SDP:blocks-ready-p)) (TVXDTOOLDONE "FAIL" "Thieu block mau")
   (progn (setq conflicts (SDP:existing-panel-conflicts d))
    (if conflicts (TVXDTOOLDONE "FAIL" "Trung tu tren CAD; dung Cap nhat")
     (if (SDP:draw-data d) (TVXD:finish d (strcat "Da ve " (itoa (length d)) " tu theo mau goc.")) (TVXDTOOLDONE "FAIL" "Da huy hoac ve loi")))))))
(defun TVXD:update (/ d f old selection pairs scan integ tasks changed tsk result)
 (setq d (TVXD:read-data))
 (if d (progn
  (setq f (open (strcat tvxd-out "/PARSED_EXCEL.lspdata") "r"))
  (if (null f) (TVXDTOOLDONE "FAIL" "Thieu du lieu lan ve truoc")
   (progn (setq old (read (read-line f))) (close f)
    (setq selection (ssget "_X" (list (cons 410 "Model"))) pairs (SDP:selection-pairs selection)
          scan (SDP:scan-scope-once (car pairs) (cadr pairs)) integ (SDP:check-integrity (car scan) (cadr scan) d T))
    (cond
     ((car integ) (TVXDTOOLDONE "FAIL" (vl-prin1-to-string (car integ))))
     ((cadr integ) (TVXDTOOLDONE "FAIL" "Cau truc lo thay doi. Tao du an/ban ve moi de giu ban cu."))
     (T
      (setq tasks (SDP:tasks-from-inventory (car scan) d) changed nil)
      (foreach tsk tasks (if (not (equal (nth 2 tsk) (assoc (nth 1 tsk) old))) (setq changed (cons tsk changed))))
      (if (null changed) (TVXDTOOLDONE "NO_CHANGE" "Excel khong doi; khong ve lai, khong ghi DWG.")
       (progn (setq result (SDP:process-tasks-atomic (reverse changed)))
        (if (car result) (TVXD:finish d (strcat "Cap nhat " (itoa (length changed)) " tu; giu cac tu con lai.")) (TVXDTOOLDONE "FAIL" "Cap nhat loi; da rollback")))))))))))
(defun TVXD:run (fn / r olddia)
 (setq olddia (getvar "FILEDIA") r (vl-catch-all-apply fn nil))
 (setvar "FILEDIA" olddia)
 (if (vl-catch-all-error-p r) (TVXDTOOLDONE "FAIL" (vl-catch-all-error-message r))) (princ))
(defun c:TVXDFASTNEW () (TVXD:run 'TVXD:new))
(defun c:TVXDFASTUPDATE () (TVXD:run 'TVXD:update))
(princ)

;;; File bridge for standalone manager. No CAD managed DLL or Excel COM required.
(defun TVXDXLSX (fp / f line)
 (setq f (open tvxd-rows-file "r"))
 (if f (progn (setq line (read-line f)) (close f) (read line)) "Missing ROWS file"))
(defun TVXDTOOLDONE (status details / f ss i en ed at obj attributes tagged)
 (setq tagged 0 attributes 0 ss (if *SD153-ID* (SD153:report-ss *SD153-ID*) (ssget "_X" '((410 . "Model")))))
 (if ss (progn (setq i 0) (repeat (sslength ss)
  (setq en (ssname ss i) i (1+ i) ed (entget en '("SDNL")))
  (if (= (cdr (assoc 0 ed)) "INSERT")
   (progn (setq obj (vlax-ename->vla-object en))
    (if (= :vlax-true (vla-get-HasAttributes obj))
     (setq attributes (+ attributes (length (vlax-safearray->list (vlax-variant-value (vla-GetAttributes obj))))))))) )))
 (setq f (open tvxd-result "w"))
 (if f (progn (write-line status f) (write-line details f)
  (write-line (strcat "ATTRIBUTES=" (itoa attributes)) f)
  (write-line (strcat "ACADVER=" (getvar "ACADVER")) f) (close f)))
 (princ (strcat "\nSDNL: " status " - " details)) status)
(princ)

;;; Existing Model only. Never SAVEAS, OPEN or launch another CAD.
(vl-load-com)
(defun TVXD:finish (d msg)
 (TVXD:save-data d)
 (setq tvxd-bound-project tvxd-project)
 (TVXDTOOLDONE "PASS" (strcat msg " Chua luu; Ctrl+S de luu ban ve hien tai.")))
(defun TVXD:load-blocks (/ dbx doc blocks source names b missing arr result)
 (setq doc (vla-get-ActiveDocument (vlax-get-acad-object))
       dbx (vla-GetInterfaceObject (vlax-get-acad-object) (strcat "ObjectDBX.AxDbDocument." (substr (getvar "ACADVER") 1 2))))
 (setq result (vl-catch-all-apply
  '(lambda ()
    (vla-Open dbx tvxd-template)
    (setq source (vla-get-Blocks dbx) blocks (vla-get-Blocks doc) missing nil)
    (foreach b '("SDNL_LO" "SDNL_TU" "SDNL_TU_PHONG" "SDNL_PTT" "A$C6A807301")
     (if (not (tblsearch "BLOCK" b)) (setq missing (cons (vla-Item source b) missing))))
    (if missing
     (progn
      (setq arr (vlax-make-safearray vlax-vbObject (cons 0 (1- (length missing)))))
      (vlax-safearray-fill arr missing)
      (vla-CopyObjects dbx arr blocks))))
  nil))
 (vlax-release-object dbx)
 (if (vl-catch-all-error-p result) (progn (TVXDTOOLDONE "FAIL" (vl-catch-all-error-message result)) nil) T))
(defun TVXD:model-body (/ d obj doc)
 (cond
  ((/= (vla-get-HWND (vla-get-ActiveDocument (vlax-get-acad-object))) tvxd-expected-window) (TVXDTOOLDONE "FAIL" "Ban ve dich da thay doi."))
  ((/= (getvar "TILEMODE") 1) (TVXDTOOLDONE "FAIL" "Hay chuyen sang Model."))
  ((and tvxd-update (/= tvxd-bound-project tvxd-project)) (TVXDTOOLDONE "FAIL" "Du an chua duoc ve trong Model nay o phien hien tai. Khong cap nhat nham ban ve."))
  ((setq d (TVXD:read-data))
   (if (and (not tvxd-update) (SDP:existing-panel-conflicts d))
    (TVXDTOOLDONE "FAIL" "Trung PANEL_ID. Khong tao ban sao; hay dung Cap nhat dung du an.")
    (progn
     ;; Copy only missing named block definitions; no wrapper insertion or redefine.
     (if (not (SDP:blocks-ready-p)) (TVXD:load-blocks))
     (if (not (SDP:blocks-ready-p)) (TVXDTOOLDONE "FAIL" "Block hien co khong dung mau; khong ghi de block nguon.")
      (if tvxd-update (TVXD:update) (TVXD:new))))))))
(defun c:TVXDMODEL (/ *error* saved r)
 (setq saved (mapcar '(lambda (n) (cons n (getvar n))) '("FILEDIA" "CMDECHO" "OSMODE" "CLAYER" "CECOLOR" "CELTYPE" "TEXTSTYLE" "ATTREQ" "ATTDIA")))
 (defun *error* (msg)
  (SD153:restore-vars saved)
  (TVXDTOOLDONE "CANCELLED" (if msg msg "Da huy")) (princ))
 (setq r (vl-catch-all-apply 'TVXD:model-body nil))
 (if (vl-catch-all-error-p r) (SD153:abort-undo))
 (SD153:restore-vars saved)
 (if (vl-catch-all-error-p r) (TVXDTOOLDONE "FAIL" (vl-catch-all-error-message r)))
 (princ))
(princ)

;;; Native command: the importer is windowless; all drawing happens in this document.
(defun TVXD:native-import (source root / shell code job f setup)
 (if (/= (getvar "TILEMODE") 1) (alert "Hay chuyen sang Model truoc khi tao so do.")
  (progn
   (if (not (vl-file-directory-p (strcat root "/LOGS"))) (vl-mkdir (strcat root "/LOGS")))
   (setq job (strcat root "/LOGS/NATIVE_" (vl-filename-base (vl-filename-mktemp "JOB" (strcat root "/LOGS") ".tmp"))))
   (vl-mkdir job)
   (setq shell (vlax-create-object "WScript.Shell"))
   (setq code (vl-catch-all-apply 'vlax-invoke-method
    (list shell 'Run (strcat "\"" root "/SDNL_IMPORT.exe\" --export-rows \"" source "\" \"" job "\"") 0 :vlax-true)))
   (vlax-release-object shell)
   (cond
    ((vl-catch-all-error-p code) (alert (vl-catch-all-error-message code)))
    ((or (/= code 0) (not (findfile (strcat job "/READY.txt"))))
     (princ (strcat "\nKhong nap duoc Excel. Chi tiet: " job "/ERROR.txt"))
     (alert "Excel khong hop le hoac bi khoa. CAD chua thay doi. Xem ERROR.txt trong LOGS."))
    (T
     (setq tvxd-native-excel source tvxd-native-job job)
     job)))))
(defun TVXD:use-job (job / f setup)
 (setq f (open (strcat job "/SETUP.lspdata") "r") setup (read-line f)) (close f)
 (eval (read setup))
 (c:TVXDMODEL))
(defun TVXD:native-run (source root / job)
 (setq job (TVXD:native-import source root))
 (if job (TVXD:use-job job)) (princ))
(defun c:SDNLLOAD (/ root source job)
 (setq root (TVXD:UI:root))
 (if root
  (progn
   (setq source (getfiled "Chon Excel co sheet SDNL_DATA" (if tvxd-native-excel tvxd-native-excel "") "xlsx;xlsm" 0))
   (if source
    (progn
     (setq job (TVXD:native-import source root))
     (if job (progn (setq tvxd-native-job job) (princ "\\nDa nap Excel. Bam Tao so do va chon diem dat tren Model."))))))
  (alert "Chua nap menu TVXD_SDNL.")) (princ))
(defun c:SDNLDRAW ()
 (if (not tvxd-native-job) (c:SDNLLOAD))
 (if tvxd-native-job (TVXD:use-job tvxd-native-job)) (princ))
;;; Existing Ribbon now invokes the native command, not the manager.
(defun c:SDNLMANAGER () (c:SDNLDRAW))
(princ)

;;; SDNL153: independent persistent sets. Metadata is committed inside engine Undo marks.
(setq *SD153-ID* nil *SD153-DATA* nil *SD153-SOURCE* nil *SD153-HANDLES* nil)
(defun SD153:fail (message) (vl-exit-with-error message))
(defun SD153:owner (en / ed)
 (setq ed (entget en)) (if (= (cdr (assoc 0 ed)) "ATTRIB") (cdr (assoc 330 ed)) en))
(defun SD153:tag (en / xd)
 (if en (progn (setq en (SD153:owner en) xd (assoc -3 (entget en '("TVXD_SDNL153"))))
  (if xd (mapcar 'cdr (cdr (cadr xd)))))))
(defun SD153:stamp (en / ed h)
 (if (and en *SD153-ID*) (progn
  (regapp "TVXD_SDNL153") (setq ed (entget en) h (cdr (assoc 5 ed)))
  (if (not (entmod (append ed (list (list -3 (list "TVXD_SDNL153" '(1000 . "1") (cons 1000 *SD153-ID*) (cons 1000 h)))))))
   (SD153:fail "Khong ghi duoc ma bo SDNL."))
  (setq *SD153-HANDLES* (cons h *SD153-HANDLES*)))) en)
(defun SD153:dict (/ found en)
 (setq found (dictsearch (namedobjdict) "TVXD_SDNL_SETS153"))
 (if found (cdr (assoc -1 found)) nil))
(defun SD153:read (id / d rec text parsed item)
 (setq d (SD153:dict))
 (if (and d (setq rec (dictsearch d id)))
  (progn (setq text "") (foreach item rec (if (= (car item) 1) (setq text (strcat text (cdr item)))))
   (if (> (strlen text) 12000000) (SD153:fail "Ho so bo SDNL qua lon."))
   (setq parsed (vl-catch-all-apply 'read (list text)))
   (if (or (vl-catch-all-error-p parsed) (not (listp parsed)) (/= (length parsed) 5)
           (/= (car parsed) 1) (/= (cadr parsed) id) (/= (type (nth 2 parsed)) 'STR)
           (not (listp (nth 3 parsed))) (not (listp (nth 4 parsed))))
    (SD153:fail "Ho so bo SDNL khong hop le; khong cap nhat.")) parsed)))
(defun SD153:members (id / rec out en h tag)
 (setq rec (SD153:read id)) (if (null rec) (SD153:fail "Bo SDNL chua co ho so; ban cu/ban sao khong duoc cap nhat."))
 (foreach h (nth 4 rec)
  (if (or (/= (type h) 'STR) (null (setq en (handent h))) (null (entget en))) (SD153:fail "Bo SDNL thieu thanh vien; khong tu suy doan."))
  (setq tag (SD153:tag en))
  (if (or (/= (cdr (assoc 410 (entget en))) "Model") (not (equal tag (list "1" id h))) (member en out))
   (SD153:fail "Thanh vien bo SDNL bi doi/nhan ban; khong cap nhat."))
  (setq out (cons en out)))
 (if (null out) (SD153:fail "Bo SDNL rong.")) out)
(defun SD153:validate (id / members doc obj en tag rec check)
 (setq members (SD153:members id) rec (SD153:read id))
 (setq check (SDC:validate-data (nth 3 rec)))
 (if (or (car check) (cadr check)) (SD153:fail "Du lieu goc cua bo SDNL bi hong."))
 (setq doc (vla-get-ActiveDocument (vlax-get-acad-object)))
 (vlax-for obj (vla-get-ModelSpace doc)
  (setq en (vlax-vla-object->ename obj) tag (SD153:tag en))
  (if (and tag (= (cadr tag) id) (not (member en members)))
   (SD153:fail "Phat hien ban sao cung ma bo SDNL; khong cap nhat bo goc hoac ban sao."))) rec)
(defun SD153:allowed (en / tag)
 (setq tag (SD153:tag en))
 (if *SD153-ID* (and tag (= (cadr tag) *SD153-ID*) (member (cdr (assoc 5 (entget en))) *SD153-HANDLES*)) (null tag)))
(defun SD153:selection (id / ss en)
 (setq ss (ssadd)) (foreach en (SD153:members id) (ssadd en ss)) ss)
(defun SD153:new-id (/ id d)
 (setq id (strcat "S" (rtos (getvar "CDATE") 2 8) "_" (itoa (getvar "MILLISECS"))) d (SD153:dict))
 (while (and d (dictsearch d id)) (setq id (strcat id "X"))) id)
(defun SD153:commit (/ d rec old en text chunks handles doc obj tag h)
 (setq handles nil doc (vla-get-ActiveDocument (vlax-get-acad-object)))
 (vlax-for obj (vla-get-ModelSpace doc)
  (setq en (vlax-vla-object->ename obj) tag (SD153:tag en) h (vla-get-Handle obj))
  (if (and tag (= (cadr tag) *SD153-ID*))
   (progn (if (or (not (equal tag (list "1" *SD153-ID* h))) (not (member h *SD153-HANDLES*)))
     (SD153:fail "Thanh vien SDNL khong thuoc giao dich; huy thay doi."))
    (setq handles (cons h handles)))))
 (if (null handles) (SD153:fail "Khong co thanh vien SDNL de luu."))
 (setq d (SD153:dict))
 (if (null d) (progn (setq d (entmakex '((0 . "DICTIONARY") (100 . "AcDbDictionary") (281 . 1))))
  (if (null (dictadd (namedobjdict) "TVXD_SDNL_SETS153" d)) (SD153:fail "Khong tao duoc ho so SDNL."))))
 (setq text (vl-prin1-to-string (list 1 *SD153-ID* *SD153-SOURCE* *SD153-DATA* handles)))
 (if (> (strlen text) 12000000) (SD153:fail "Ho so SDNL qua lon."))
 (setq chunks nil) (while (> (strlen text) 0) (setq chunks (cons (cons 1 (substr text 1 240)) chunks) text (substr text (1+ (min 240 (strlen text))))))
 (setq old (dictsearch d *SD153-ID*))
 (if old (progn (setq en (cdr (assoc -1 old)))
  (if (null (entmod (append (list (cons -1 en) '(0 . "XRECORD") (cons 5 (cdr (assoc 5 old))) (cons 330 d) '(100 . "AcDbXrecord") '(280 . 1)) (reverse chunks)))) (SD153:fail "Khong cap nhat duoc ho so SDNL.")))
  (progn (setq en (entmakex (append '((0 . "XRECORD") (100 . "AcDbXrecord") (280 . 1)) (reverse chunks))))
   (if (or (null en) (null (dictadd d *SD153-ID* en))) (SD153:fail "Khong luu duoc ho so SDNL."))))
 (setq *SD153-HANDLES* handles) T)
(defun SD153:new (d)
 (setq *SD153-ID* (SD153:new-id) *SD153-DATA* d *SD153-SOURCE* (if tvxd-native-excel tvxd-native-excel "") *SD153-HANDLES* nil)
 (if (SDP:draw-data d) (TVXD:finish d "Da tao BO SDNL rieng; co the ve tiep cung Excel.") (TVXDTOOLDONE "CANCELLED" "Da huy/rollback bo SDNL moi.")))
(defun SD153:update (d rec / ss pairs scan integ tasks result)
 (setq *SD153-HANDLES* (nth 4 rec) *SD153-DATA* d)
 (setq ss (SD153:selection *SD153-ID*) pairs (SDP:selection-pairs ss) scan (SDP:scan-scope-once (car pairs) (cadr pairs))
       integ (SDP:check-integrity (car scan) (cadr scan) d T))
 (if (car integ) (SD153:fail (vl-prin1-to-string (car integ))))
 (setq tasks (SDP:tasks-from-inventory (car scan) d))
 (if (and (equal d (nth 3 rec)) (null (cadr integ))) (TVXDTOOLDONE "NO_CHANGE" "Bo SDNL da chon khong doi.")
  (progn (setq result (SDP:process-tasks-atomic tasks))
   (if (car result) (TVXD:finish d "Da cap nhat dung bo SDNL da chon.") (TVXDTOOLDONE "FAIL" "Cap nhat loi; da rollback.")))))
(defun TVXD:model-body (/ d rec)
 (cond
  ((/= (vla-get-HWND (vla-get-ActiveDocument (vlax-get-acad-object))) tvxd-expected-window) (SD153:fail "Ban ve dich da thay doi."))
  ((/= (getvar "TILEMODE") 1) (SD153:fail "Hay chuyen sang Model."))
  ((= 0 (logand (getvar "UNDOCTL") 1)) (SD153:fail "Bat UNDO truoc khi tao/cap nhat SDNL."))
  ((progn (SD153:undo-ready) (setq d (TVXD:read-data)))
   (if (and tvxd-update (null *SD153-ID*)) (SD153:fail "Chon bo SDNL bang SDNLUPDATESET."))
   (if tvxd-update (setq rec (SD153:validate *SD153-ID*)))
   (if (not (SDP:blocks-ready-p)) (TVXD:load-blocks))
   (if (not (SDP:blocks-ready-p)) (SD153:fail "Block mau khong hop le; chua ve."))
   (if tvxd-update (SD153:update d rec) (SD153:new d)))))
(defun SD153:update-command (/ selected en tag rec source root job f setup beforedoc)
 (SD153:undo-ready)
 (setq beforedoc (vla-get-HWND (vla-get-ActiveDocument (vlax-get-acad-object))))
 (setq selected (entsel "\nChon mot doi tuong trong BO SDNL can cap nhat: "))
 (if selected (progn (setq en (SD153:owner (car selected)) tag (SD153:tag en))
  (if (or (null tag) (/= (length tag) 3) (/= (car tag) "1") (/= (nth 2 tag) (cdr (assoc 5 (entget en)))))
   (SD153:fail "Doi tuong cu/ban sao khong co ho so hop le. Tao bo moi; khong cap nhat tu dong."))
  (setq *SD153-ID* (cadr tag) rec (SD153:validate *SD153-ID*))
  (setq source (getfiled "Chon Excel cap nhat BO SDNL da chon" (nth 2 rec) "xlsx;xlsm" 0))
  (if source (progn (setq root (TVXD:UI:root) job (TVXD:native-import source root))
   (if job (progn
    (setq f (open (strcat job "/SETUP.lspdata") "r") setup (read-line f)) (close f) (eval (read setup))
    (if (/= beforedoc (vla-get-HWND (vla-get-ActiveDocument (vlax-get-acad-object)))) (SD153:fail "Ban ve da doi; khong cap nhat."))
    (setq tvxd-update T *SD153-SOURCE* source) (TVXD:model-body))))))))
(defun c:SDNLUPDATESET (/ *error* *SD153-ID* *SD153-DATA* *SD153-SOURCE* *SD153-HANDLES* *SD153-UNDO* r tvxd-update saved p)
 (setq saved (mapcar '(lambda (n) (cons n (getvar n))) '("FILEDIA" "CMDECHO" "OSMODE" "CLAYER" "CECOLOR" "CELTYPE" "TEXTSTYLE" "ATTREQ" "ATTDIA")))
 (defun *error* (msg) (SD153:abort-undo) (SD153:restore-vars saved) (princ (strcat "\nSDNL: " (if msg msg "Da huy"))) (princ))
 (setq r (vl-catch-all-apply 'SD153:update-command nil))
 (if (vl-catch-all-error-p r) (SD153:abort-undo))
 (SD153:restore-vars saved)
 (if (vl-catch-all-error-p r) (princ (strcat "\nSDNL: " (vl-catch-all-error-message r)))) (princ))
(defun c:SDNLUPDATE () (princ "\nDung SDNLUPDATESET de chon dung bo. Bo cu chua co ho so: ve bo moi, khong cap nhat suy doan.") (princ))
(defun TVXD:update () (SD153:fail "Dung SDNLUPDATESET; khong cap nhat toan Model."))
;;; Dynamic local bindings clear instance state on success, failure and Esc.
(defun c:TVXDMODEL (/ *error* saved r *SD153-ID* *SD153-DATA* *SD153-SOURCE* *SD153-HANDLES* *SD153-UNDO*)
 (setq saved (mapcar '(lambda (n) (cons n (getvar n))) '("FILEDIA" "CMDECHO" "OSMODE" "CLAYER" "CECOLOR" "CELTYPE" "TEXTSTYLE" "ATTREQ" "ATTDIA")))
 (defun *error* (msg) (SD153:abort-undo) (SD153:restore-vars saved) (TVXDTOOLDONE "CANCELLED" (if msg msg "Da huy")) (princ))
 (setq r (vl-catch-all-apply 'TVXD:model-body nil))
 (if (vl-catch-all-error-p r) (SD153:abort-undo))
 (SD153:restore-vars saved)
 (if (vl-catch-all-error-p r) (TVXDTOOLDONE "FAIL" (vl-catch-all-error-message r))) (princ))
(princ)

(defun c:TVXDFASTNEW () (c:SDNLDRAW))
(defun c:TVXDFASTUPDATE () (c:SDNLUPDATESET))

(defun SD153:report-ss (id / ss doc obj en tag)
 (setq ss (ssadd) doc (vla-get-ActiveDocument (vlax-get-acad-object)))
 (vlax-for obj (vla-get-ModelSpace doc)
  (setq en (vlax-vla-object->ename obj) tag (SD153:tag en))
  (if (and tag (= (cadr tag) id)) (ssadd en ss))) ss)

;;; Explicit command-safe group ownership: never close a user's existing group.
(defun SD153:undo-ready ()
 (if (= 0 (logand (getvar "UNDOCTL") 1)) (SD153:fail "Bat UNDO truoc khi tao/cap nhat SDNL."))
 (if (/= 0 (logand (getvar "UNDOCTL") 8)) (SD153:fail "Dang co nhom UNDO mo. Ket thuc nhom cua ban truoc khi chay SDNL.")) T)
(defun SD153:restore-vars (values / pair)
 (foreach pair values (if (not (equal (getvar (car pair)) (cdr pair))) (setvar (car pair) (cdr pair)))))
(defun SD153:undo-begin (doc)
 (if *SD153-ID* (progn (SD153:undo-ready)
  (command-s "_.UNDO" "_Begin")
  (if (= 0 (logand (getvar "UNDOCTL") 8)) (SD153:fail "Khong mo duoc nhom UNDO SDNL."))
  (setq *SD153-UNDO* T)) (vla-StartUndoMark doc)))
(defun SD153:undo-end (doc)
 (if *SD153-ID* (progn
  (if (not *SD153-UNDO*) (SD153:fail "Khong so huu nhom UNDO SDNL."))
  (SD153:restore-vars saved)
  (command-s "_.UNDO" "_End")
  (if (/= 0 (logand (getvar "UNDOCTL") 8)) (SD153:fail "Nhom UNDO SDNL chua dong."))
  (setq *SD153-UNDO* nil)) (vla-EndUndoMark doc)))
(defun SD153:abort-undo ()
 (if *SD153-UNDO* (progn
  (if (/= 0 (logand (getvar "UNDOCTL") 8)) (command-s "_.UNDO" "_End"))
  (setq *SD153-UNDO* nil)
  (command-s "_.U"))) (princ))
