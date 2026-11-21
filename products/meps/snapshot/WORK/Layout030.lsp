;;; ==========================================================================
;;;  Layout030.lsp  (TVXD MEPS 035)
;;;  PHAN A = DAN_TRANG_TUANPA.lsp NGUYEN BAN cua nguoi dung. CHI doi ten sau
;;;  lenh: CK->T-CK, DTL->T-DTL, DTM->T-DTM, DTV->T-DTV, DTA->T-DTA,
;;;  TKI->T-TKI, cung cac dong nhac/ghi chu nhac den ten lenh. Moi ham dt-*,
;;;  bien *DT-*, so lieu, hinh hoc va thao tac giu nguyen ban goc.
;;;  PHAN B (cuoi file) = lenh T-STT rieng (ham tvxd30-*, bien *TVXD30-*),
;;;  khong goi ham dt-* va khong dat bien *DT-*.
;;;  037 (nguoi dung 2026-10-11): BO lenh T-DTV, T-DTA (ham doi ten noi bo
;;;  tvxd37-bo-T-DTV / tvxd37-bo-T-DTA, khong con la lenh). So to SO_TO dung
;;;  kieu chu Arial rieng (PHAN C, ham tvxd37-*): ATTDEF SO_TO cua block moi,
;;;  ATTRIB SO_TO khi T-CK chen khung, va T-STT khi ghi so. TEN_TO, TY_LE,
;;;  *DT-STYLE*, Standard va TEXTSTYLE khong doi.
;;; ==========================================================================
;;; ==========================================================================
;;;  DAN_TRANG.lsp
;;;  --------------------------------------------------------------------
;;;  CAC LENH:
;;;     T-CK   Dat khung vung in tren model. Chon ty le tu hop thoai so xuong,
;;;          bam chuot lien tuc, so to tu tang. Thieu block thi tu tao.
;;;     T-DTL  Sinh layout tu cac khung do. Sao chep layout mau co san trong
;;;          ban ve, dat tam nhin, dat ty le, dien ty le vao khung ten.
;;;     T-TKI  Tao lai bo block khung vung in. Binh thuong khong phai go,
;;;          vi T-CK da tu goi khi can.
;;;     (037: T-DTV, T-DTA da bo theo yeu cau nguoi dung; khong con la lenh.)
;;;
;;;  BAN VE HE MET:
;;;     Bo block co san hai he. Trong hop thoai T-CK, dong ghi them "(m)" la
;;;     block danh cho ban ve ve theo he met, vi du "1 : 500 (m)".
;;;     Chon dong ay thi khung tren model rong 172.5 m, T-DTL dat mview 2.
;;;     Dong khong co chu "(m)" giu nguyen nhu cu, danh cho ban ve he mm.
;;;     Doi chieu nhanh cho he met:
;;;        1:100  khung  34.50 x  28.70 m   mview 10
;;;        1:200  khung  69.00 x  57.40 m   mview 5
;;;        1:250  khung  86.25 x  71.75 m   mview 4
;;;        1:400  khung 138.00 x 114.80 m   mview 2.5
;;;        1:500  khung 172.50 x 143.50 m   mview 2
;;;        1:750  khung 258.75 x 215.25 m   mview 1.3333
;;;        1:1000 khung 345.00 x 287.00 m   mview 1
;;;        1:2000 khung 690.00 x 574.00 m   mview 0.5
;;;     So do lay theo o trong khung ten 345 x 287 mm. Khung ten khac thi
;;;     lisp tu do lai roi dung lai block, so tren doi theo.
;;;     Them bot ty le he met o *DT-TYLE-M*, go so nguyen la MAU SO ty le
;;;     that. Sua xong go T-TKI de dung lai bo block.
;;;
;;;  KHUNG DAT NGHIENG:
;;;     Trong T-CK go G de dat goc nghieng: go so do, bam hai diem, hoac go D
;;;     chon mot duong de lay huong. Khung sau do dat nghieng theo goc ay,
;;;     khung ao chay theo chuot nen nhin duoc vung phu truoc khi bam.
;;;     Khung da dat roi van xoay lai bang lenh ROTATE binh thuong, T-DTL va
;;;     T-DTM doc lai goc nen khong phai dat lai.
;;;     Trong khi khung ao chay theo chuot thi khong bat diem duoc. Can bat
;;;     diem thi bam phim O de bam mot diem co bat, hoac dat *DT-PREVIEW* nil
;;;     de bo han khung ao.
;;;     Ben layout lisp chay lai dung thao tac ALIGNSPACE, khong doc goc:
;;;        hai diem ben model = hai goc DUOI cua khung vung in, lisp tu lay
;;;           tu hinh chu nhat trong dinh nghia block nen bao nhieu goc
;;;           nghieng khac nhau cung khong sao;
;;;        hai diem ben giay  = hai goc DUOI cua o nhin trong khung ten;
;;;        viewport xoay sao cho canh duoi khung nam theo canh duoi o nhin,
;;;           tam khung ve giua o nhin, roi dat ty le 1/n theo thuoc tinh
;;;           TY_LE cua chinh khung do.
;;;     Chieu xoay lisp tu do lay o to dau tien nen khong phai sua tay.
;;;
;;;  KHONG CAN FILE TEMPLATE. Lisp dung layout co san trong chinh ban ve
;;;  dang mo lam mau. Quy trinh du an giu nguyen nhu cu:
;;;     1. Copy xref khung ten vao du an, sua noi dung can sua.
;;;     2. Xref khung ten do vao mot layout trong ban ve.
;;;     3. Go T-CK dat khung, go T-DTL sinh layout.
;;;
;;;  Cai mot lan:  APPLOAD  ->  nut Contents o khung Startup Suite  ->  Add
;;;
;;;  Ten layout dat theo: "<so to> - <ten to>", vi du "01 - MB DIEN TANG 1".
;;;  Khung chua dien ten to thi layout mang dung so to, vi du "01".
;;;
;;;  YEU CAU DUY NHAT VOI LAYOUT MAU:
;;;     - Co dung MOT viewport.
;;;     - Viewport do bang dung o trong danh cho hinh ve trong khung ten.
;;;     Viewport dang nhin vao dau, dang o ty le nao deu khong quan trong,
;;;     lisp ghi de ca tam nhin lan ty le cho tung ban sao.
;;; ==========================================================================

(vl-load-com)

;;; ---------------------------- CAU HINH -----------------------------------
(setq *DT-RONG*      345.0)           ; be rong vung nhin trong khung ten (mm)
(setq *DT-CAO*       287.0)           ; chieu cao vung nhin trong khung ten (mm)
(setq *DT-CAOCHU*      3.0)           ; chieu cao chu tren giay (mm)
(setq *DT-LE*          1.0)           ; le trong, dung cho CA hai phia trai va tren
(setq *DT-GIANDONG*    1.35)          ; gian dong, nhan voi chieu cao chu
;;   Vi tri chu tu tinh tu ba so tren, khong dat cung:
;;     cach mep trai = *DT-LE*
;;     cach mep tren = *DT-LE*   (bang nhau, nen chu nam vuong goc)
;;     hai dong cach nhau = *DT-GIANDONG* x *DT-CAOCHU*
;;   Doi *DT-CAOCHU* hay *DT-LE* thi hai khoang cach van tu khop nhau.
(setq *DT-STYLE*    "Standard")       ; text style cho thuoc tinh
(setq *DT-TIENTO*   "KHUNG-IN-A3-")   ; tien to ten block khung vung in
(setq *DT-LAYERKHUNG* "KHUNG_IN")     ; layer dat khung vung in
(setq *DT-LAYOUTMAU*  "MAU")          ; ten layout mau uu tien tim
(setq *DT-NGAN*       " - ")          ; dau ngan giua so to va ten to
(setq *DT-LAYERTL*    "KT_TYLE")      ; layer chua dong chu ty le trong khung ten
(setq *DT-TIENTOTL*   "1:")           ; tien to ghi truoc mau so ty le
(setq *DT-HOHANG*    10.0)            ; khoang ho giua cac khung ten tren layout (mm giay)
(setq *DT-KHOGIAY*  '(420.0 297.0))   ; kho giay khung ten, dung lam buoc xep tren layout.
                                      ; De nil thi lisp do khung bao cua xref, nhung xref
                                      ; co doi tuong lac ra ngoai thi buoc xep se qua xa.
(setq *DT-LAYERTEN* "KT_TENTO")       ; layer chua chu ten to trong khung ten
(setq *DT-LAYERSO*  "KT_SOTO")        ; layer chua chu so to trong khung ten
;; Nam kieu diem bat khi dat khung. Moi dong: ten hien ra, he so bu theo X,
;; he so bu theo Y. Diem chen cua block van luon o TAM khung, lisp tu cong bu,
;; nen T-DTL khong phai doi gi.
(setq *DT-DSBAT*
  '(("Goc duoi trai"   1.0  1.0)
    ("Goc tren trai"   1.0 -1.0)
    ("Goc duoi phai"  -1.0  1.0)
    ("Goc tren phai"  -1.0 -1.0)
    ("Tam khung"       0.0  0.0)))
(setq *DT-BATDIEM* 0)                 ; dong dang chon trong *DT-DSBAT*
(setq *DT-GOC*      0.0)              ; goc nghieng khung dang dung, radian theo WCS
(setq *DT-LATGOC*   T)                ; tu lat goc ve khoang -90..90 do cho chu khoi nguoc
(setq *DT-PREVIEW*  nil)              ; khung ao chay theo chuot khi dat khung.
                                      ; Dat nil vi cach xoa khung ao cua ban
                                      ; nay khong sach tren mot so may, de lai
                                      ; vet tren man hinh. Muon bat lai thi
                                      ; doi nil thanh T.
(setq *DT-CHIEUTW*  nil)              ; chieu xoay view, lisp tu do o to dau tien
(setq *DT-TYLE*
  '(5 10 15 20 25 30 40 50 60 75 80 100 125 150 175 200 250 500 1000))
;; Ty le cho ban ve ve theo HE MET tren model. So dien vao van la MAU SO ty le
;; that, lisp tu chia 1000 khi dung block. Block sinh ra mang them chu M trong
;; ten, vi du 1:500 he m thanh KHUNG-IN-A3-M0500, nen khong de len bo mm.
(setq *DT-TYLE-M* '(100 200 250 400 500 750 1000 2000))
(setq *DT-DAUM*   "M")                ; dau danh cho block he met trong ten block
;;; -------------------------------------------------------------------------


;;; ======================= NHOM HAM DUNG CHUNG =============================

(defun dt-pad4 (n / s)
  (setq s (itoa n)) (while (< (strlen s) 4) (setq s (strcat "0" s))) s)

(defun dt-pad2 (n / s)
  (setq s (itoa n)) (if (< (strlen s) 2) (strcat "0" s) s))

;;; ==================== NHOM HAM TY LE VA HE DON VI =========================
;;; Mot ty le luon di theo cap  (mau-so . he-met)
;;;    mau-so  : so nguyen, dung 1:500 nhu nguoi doc hieu. Dung dat ten block,
;;;              dien vao thuoc tinh TY_LE va ghi vao khung ten.
;;;    he-met  : T neu ban ve ve theo he met, nil neu ve theo he mm.
;;; He so nhan dung cho moi phep tinh hinh hoc goi la HS, nghia la MOT MM TREN
;;; GIAY BANG BAO NHIEU DON VI MODEL:
;;;    he mm : hs = mau so        1:500 -> 345 mm x 500 = 172500 don vi
;;;    he m  : hs = mau so / 1000 1:500 -> 345 mm x 0.5 = 172.5  don vi
;;; Ty le viewport la 1/hs, nen 1:500 he m tu ra mview 2.

(defun dt-hs (tl) (if (cdr tl) (/ (float (car tl)) 1000.0) (float (car tl))))

(defun dt-ten-block (tl)
  (strcat *DT-TIENTO* (if (cdr tl) *DT-DAUM* "") (dt-pad4 (car tl))))

(defun dt-nhan-tl (tl)
  (strcat "1 : " (itoa (car tl)) (if (cdr tl) " (m)" "")))

;;; --- Tach ten block ra cap ty le. Khong phai ten block khung thi tra nil --
(defun dt-tach-ten (s / L p n)
  (setq L (strlen *DT-TIENTO*))
  (if (and s (> (strlen s) L)
           (= (strcase (substr s 1 L)) (strcase *DT-TIENTO*)))
    (progn
      (setq p (substr s (1+ L)))
      (if (= (strcase (substr p 1 1)) (strcase *DT-DAUM*))
        (progn (setq n (atoi (substr p 2))) (if (> n 0) (cons n T)   nil))
        (progn (setq n (atoi p))            (if (> n 0) (cons n nil) nil))))
    nil))

;;; --- So thuc ra chuoi, cat het so 0 thua o duoi --------------------------
(defun dt-so (x / s v)
  (setq s (rtos x 2 4))
  (setq v (cond ((vl-string-search "." s) ".") ((vl-string-search "," s) ",") (T nil)))
  (if v
    (progn
      (while (and (> (strlen s) 1) (= "0" (substr s (strlen s))))
        (setq s (substr s 1 (1- (strlen s)))))
      (if (= v (substr s (strlen s))) (setq s (substr s 1 (1- (strlen s)))))))
  s)

;;; ==================== NHOM HAM GOC XOAY CUA KHUNG ========================
;;; Goc luu trong *DT-GOC*, don vi radian, do theo WCS, chieu duong la nguoc
;;; kim dong ho. Diem bam va goc nguoi dung go vao deu theo UCS dang dung,
;;; nen phai quy doi truoc khi ghi vao ban ve.

(defun dt-do (a) (/ (* a 180.0) pi))

(defun dt-chuan-goc (a)
  (while (<  a 0.0)         (setq a (+ a (* 2.0 pi))))
  (while (>= a (* 2.0 pi))  (setq a (- a (* 2.0 pi))))
  a)

(defun dt-chuan-pi (a)
  (setq a (dt-chuan-goc a))
  (if (> a pi) (- a (* 2.0 pi)) a))

;;; --- Lat goc ve khoang -90 den 90 do, de chu trong khung khoi nguoc -----
(defun dt-lat-goc (a / b)
  (setq b (dt-chuan-pi a))
  (if *DT-LATGOC*
    (cond ((>  b (/ pi 2.0))     (- b pi))
          ((<= b (- (/ pi 2.0))) (+ b pi))
          (T b))
    b))

;;; --- Goc cua truc X thuoc UCS dang dung, do trong WCS -------------------
(defun dt-gocucs ( / v)
  (setq v (trans '(1.0 0.0 0.0) 1 0 T))
  (atan (cadr v) (car v)))

;;; --- Xoay mot vector quanh goc toa do -----------------------------------
(defun dt-xoay (p a / c s)
  (setq c (cos a) s (sin a))
  (list (- (* (car p) c) (* (cadr p) s))
        (+ (* (car p) s) (* (cadr p) c))
        (if (caddr p) (caddr p) 0.0)))

;;; --- Diem ra chuoi toa do -----------------------------------------------
;;; Truyen chuoi thi lenh khong dien giai lai theo osnap hay theo UCS.
(defun dt-p2s (p) (strcat (rtos (car p) 2 8) "," (rtos (cadr p) 2 8)))

;;; --- Hoi goc nghieng: go so, bam hai diem, hoac lay theo mot duong ------
(defun dt-hoi-goc ( / a s e g p d)
  (initget "Doituong Khong")
  (setq a (getangle (strcat "\nGoc nghieng khung, go so hoac bam hai diem"
                            " [Doituong/Khong] <" (rtos (dt-do *DT-GOC*) 2 2) ">: ")))
  (cond
    ((= a "Khong")
     (setq *DT-GOC* 0.0)
     (princ "\n>> Khung tro lai nam ngang."))
    ((= a "Doituong")
     (setq s (entsel "\nChon duong lay huong: "))
     (if s
       (progn
         (setq e (car s))
         (setq g (vl-catch-all-apply
                   '(lambda ()
                      (setq p (vlax-curve-getclosestpointto e (trans (cadr s) 1 0)))
                      (setq d (vlax-curve-getfirstderiv e
                                (vlax-curve-getparamatpoint e p)))
                      (atan (cadr d) (car d)))))
         (if (vl-catch-all-error-p g)
           (princ "\n[BO] Doi tuong nay khong lay duoc huong.")
           (progn
             (setq *DT-GOC* (dt-chuan-goc (dt-lat-goc g)))
             (princ (strcat "\n>> Goc khung: " (rtos (dt-do *DT-GOC*) 2 2) " do.")))))))
    ((null a) nil)
    (T
     (setq *DT-GOC* (dt-chuan-goc (dt-lat-goc (+ a (dt-gocucs)))))
     (princ (strcat "\n>> Goc khung: " (rtos (dt-do *DT-GOC*) 2 2) " do."))))
  *DT-GOC*)

;;; --- Bon dinh khung, tinh trong chinh he toa do cua ctr va gu -----------
(defun dt-4dinh (ctr n gu / w2 h2)
  (setq w2 (* 0.5 *DT-RONG* n)  h2 (* 0.5 *DT-CAO* n))
  (mapcar '(lambda (p) (mapcar '+ ctr (dt-xoay p gu)))
          (list (list (- w2) (- h2) 0.0) (list w2 (- h2) 0.0)
                (list w2 h2 0.0) (list (- w2) h2 0.0))))

;;; --- Ve va xoa khung ao. Ve chong len chinh no mot lan nua la xoa -------
(defun dt-ao-ve (ps)
  (grdraw (nth 0 ps) (nth 1 ps) 1 1)
  (grdraw (nth 1 ps) (nth 2 ps) 1 1)
  (grdraw (nth 2 ps) (nth 3 ps) 1 1)
  (grdraw (nth 3 ps) (nth 0 ps) 1 1))

(defun dt-ao-xoa ()
  (if *dt-ao* (progn (dt-ao-ve *dt-ao*) (setq *dt-ao* nil))))

;;; --- Tam khung tinh tu diem vua bam -------------------------------------
;;; pt theo he nao thi ket qua theo he do, a la goc do trong chinh he ay.
(defun dt-tam-tu-bam (pt n a / g d)
  (setq g (nth *DT-BATDIEM* *DT-DSBAT*))
  (setq d (dt-xoay (list (* (nth 1 g) 0.5 *DT-RONG* n)
                         (* (nth 2 g) 0.5 *DT-CAO*  n) 0.0) a))
  (list (+ (car pt) (car d)) (+ (cadr pt) (cadr d)) 0.0))

;;; --- Bam diem dat khung, co khung ao chay theo chuot --------------------
;;; Tra ve diem theo UCS, hoac chuoi tu khoa, hoac nil khi ket thuc.
;;; Trong che do khung ao, bam phim T D G N thay cho go tu khoa.
(defun dt-bam-diem (nhac n / gu g code val kq xong)
  (if (null *DT-PREVIEW*)
    (progn (initget "Tyle Diem Ghiten Goc") (getpoint nhac))
    (progn
      (setq gu (- *DT-GOC* (dt-gocucs)))
      (princ nhac)
      (setq xong nil  kq nil  *dt-ao* nil)
      (while (not xong)
        (setq g (vl-catch-all-apply 'grread (list T 15 0)))
        (if (vl-catch-all-error-p g)
          (setq xong T  kq nil)
          (progn
            (setq code (car g)  val (cadr g))
            (cond
              ((= code 5)
               (dt-ao-xoa)
               (setq *dt-ao* (dt-4dinh (dt-tam-tu-bam val n gu) n gu))
               (dt-ao-ve *dt-ao*))
              ((= code 3) (setq kq val  xong T))
              ((= code 2)
               (cond
                 ((member val (list  84 116)) (setq kq "Tyle"   xong T))
                 ((member val (list  68 100)) (setq kq "Diem"   xong T))
                 ((member val (list  71 103)) (setq kq "Goc"    xong T))
                 ((member val (list  78 110)) (setq kq "Ghiten" xong T))
                 ;; khung ao khong bat diem duoc, bam O de bam mot diem co bat
                 ((member val (list  79 111))
                  (dt-ao-xoa)
                  (setq kq (getpoint "\nBam diem, co bat diem: ")  xong T))
                 ((member val (list 13 32 27)) (setq kq nil xong T))
                 (T nil)))
              ((member code (list 11 12 25)) (setq kq nil xong T))
              (T nil)))))
      (dt-ao-xoa)
      kq)))

;;; --- Doc gia tri mot thuoc tinh cua block theo ten tag -------------------
(defun dt-att (e tag / a ed res)
  (setq res nil  a (entnext e))
  (while (and a (setq ed (entget a)) (= "ATTRIB" (cdr (assoc 0 ed))))
    (if (= (strcase (cdr (assoc 2 ed))) (strcase tag))
      (setq res (cdr (assoc 1 ed))))
    (setq a (entnext a)))
  (if res res ""))

;;; --- Bao dam co layer, dat khong in -------------------------------------
(defun dt-lo-layer (nm / doc lay)
  (setq doc (vla-get-ActiveDocument (vlax-get-acad-object)))
  (if (not (tblsearch "LAYER" nm))
    (progn
      (setq lay (vla-Add (vla-get-Layers doc) nm))
      (vl-catch-all-apply 'vla-put-Color (list lay 8)))
    (setq lay (vla-Item (vla-get-Layers doc) nm)))
  (if lay (vl-catch-all-apply 'vla-put-Plottable (list lay :vlax-false))))

;;; --- Hop thoai chon tu mot hoac nhieu danh sach -------------------------
;;; nhom = danh sach cac phan tu dang (nhan  danh-sach-chuoi  dong-mac-dinh)
;;; Tra ve danh sach chi so da chon, hoac nil neu huy hoac khong mo duoc.
(defun dt-thu-ket-qua ( / i r)
  (setq r nil  i 0)
  (while (< i *dt-so*)
    (setq r (cons (atoi (get_tile (strcat "k" (itoa i)))) r))
    (setq i (1+ i)))
  (setq *dt-kq* (reverse r)))

(defun dt-chon-nhieu (tieude nhom / fn f did kq i)
  (setq fn (vl-filename-mktemp "dt_dlg" nil ".dcl"))
  (setq f (open fn "w"))
  (if (null f)
    nil
    (progn
      (write-line "DT_DLG : dialog {" f)
      (write-line (strcat "  label = \"" tieude "\";") f)
      (setq i 0)
      (foreach g nhom
        (write-line "  : popup_list {" f)
        (write-line (strcat "    key = \"k" (itoa i) "\";") f)
        (write-line (strcat "    label = \"" (nth 0 g) "\";") f)
        (write-line "    width = 28;" f)
        (write-line "  }" f)
        (setq i (1+ i)))
      (write-line "  spacer;" f)
      (write-line "  ok_cancel;" f)
      (write-line "}" f)
      (close f)
      (setq did (load_dialog fn))
      (if (or (null did) (< did 0) (not (new_dialog "DT_DLG" did)))
        (progn
          (if did (unload_dialog did))
          (vl-file-delete fn)
          nil)
        (progn
          (setq i 0)
          (foreach g nhom
            (start_list (strcat "k" (itoa i)))
            (foreach x (nth 1 g) (add_list x))
            (end_list)
            (set_tile (strcat "k" (itoa i)) (itoa (if (nth 2 g) (nth 2 g) 0)))
            (setq i (1+ i)))
          (setq *dt-so* (length nhom))
          (action_tile "accept" "(dt-thu-ket-qua) (done_dialog 1)")
          (action_tile "cancel" "(done_dialog 0)")
          (setq kq (start_dialog))
          (unload_dialog did)
          (vl-file-delete fn)
          (if (= kq 1) *dt-kq* nil))))))

;;; --- Danh sach chuoi ten kieu diem bat -----------------------------------
(defun dt-ds-bat ( / ) (mapcar 'car *DT-DSBAT*))

;;; --- Doi diem bam chuot ra tam khung -------------------------------------
;;; Nhan diem theo WCS, tra ve tam khung theo WCS, da ke goc nghieng.
(defun dt-ve-tam (pt n) (dt-tam-tu-bam pt n *DT-GOC*))

;;; ================== PHAN 1: TAO BO BLOCK KHUNG VUNG IN ===================

(defun dt-rect (x1 y1 x2 y2)
  (entmake (list '(0 . "LWPOLYLINE") '(100 . "AcDbEntity") '(8 . "0")
                 '(100 . "AcDbPolyline") '(90 . 4) '(70 . 1)
                 (cons 10 (list x1 y1)) (cons 10 (list x2 y1))
                 (cons 10 (list x2 y2)) (cons 10 (list x1 y2)))))

(defun dt-attdef (pt hgt dfl prm tag flg sty)
  (entmake (list '(0 . "ATTDEF") '(100 . "AcDbEntity") '(8 . "0")
                 '(100 . "AcDbText") (cons 10 pt) (cons 40 hgt)
                 (cons 1 dfl) (cons 7 sty)
                 '(100 . "AcDbAttributeDefinition")
                 (cons 3 prm) (cons 2 tag) (cons 70 flg))))

(defun dt-xoa-block (nm / ss i doc blks)
  (if (setq ss (ssget "_X" (list (cons 0 "INSERT") (cons 2 nm))))
    (progn (setq i 0)
           (while (< i (sslength ss)) (entdel (ssname ss i)) (setq i (1+ i)))))
  (setq doc  (vla-get-ActiveDocument (vlax-get-acad-object)))
  (setq blks (vla-get-Blocks doc))
  (not (vl-catch-all-error-p
         (vl-catch-all-apply 'vla-Delete (list (vla-Item blks nm))))))

;;; --- Tao mot block khung ung voi mot cap ty le ---------------------------
(defun dt-tao-1-block (tl ghide sty blks iu / nm hs w2 h2 hc xl y1 y2)
  (setq nm (dt-ten-block tl)
        hs (dt-hs tl))
  (if (or ghide (not (tblsearch "BLOCK" nm)))
    (progn
      (if (tblsearch "BLOCK" nm) (dt-xoa-block nm))
      (setq w2 (* 0.5 *DT-RONG* hs)
            h2 (* 0.5 *DT-CAO*  hs)
            hc (*     *DT-CAOCHU* hs)
            xl (+ (- w2) (* *DT-LE* hs))
            ;; chu ve LEN tu duong chan, nen duong chan phai ha xuong
            ;; them dung mot chieu cao chu thi mep tren chu moi cach
            ;; mep khung dung bang *DT-LE*
            y1 (- h2 (* (+ *DT-LE* *DT-CAOCHU*) hs))
            y2 (- y1 (* *DT-GIANDONG* *DT-CAOCHU* hs)))
      (if (entmake (list '(0 . "BLOCK") (cons 2 nm) '(70 . 2) '(10 0.0 0.0 0.0)))
        (progn
          (dt-rect (- w2) (- h2) w2 h2)
          ;; 037: SO_TO dung kieu Arial rieng (dt-tao-bo-block dat truoc vong lap)
          (dt-attdef (list xl y1 0.0) hc ""  "So to"         "SO_TO"  0 *TVXD37-STY-SO-DANG*)
          (dt-attdef (list xl y2 0.0) hc ""  "Ten to ban ve" "TEN_TO" 0 sty)
          ;; TY_LE luon giu MAU SO that, de khung ten van ghi ra 1:500
          (dt-attdef (list 0.0 0.0 0.0) hc (itoa (car tl)) "Ty le" "TY_LE" 9 sty)
          (entmake '((0 . "ENDBLK")))
          ;; khai don vi cho dinh nghia block, he so quy doi khi chen = 1
          (vl-catch-all-apply 'vla-put-Units        (list (vla-Item blks nm) iu))
          (vl-catch-all-apply 'vla-put-BlockScaling (list (vla-Item blks nm) 0))
          1)
        0))
    0))

;;; --- Tao ca bo block. ghide = T thi tao lai ca nhung block da co ---------
(defun dt-tao-bo-block (ghide / doc blks iu sty sd n dem)
  (setq doc  (vla-get-ActiveDocument (vlax-get-acad-object)))
  (setq blks (vla-get-Blocks doc))
  (setq iu   (getvar "INSUNITS"))
  (setq sty  *DT-STYLE*)
  (if (null (tblsearch "STYLE" sty)) (setq sty "Standard"))
  (setq sd (tblsearch "STYLE" sty))
  (if (and sd (/= 0.0 (cdr (assoc 40 sd))))
    (princ (strcat "\n[LUU Y] Text style \"" sty "\" co chieu cao co dinh "
                   (rtos (cdr (assoc 40 sd)) 2 2)
                   ", chu trong block se khong theo chieu cao tinh toan.")))
  (dt-lo-layer *DT-LAYERKHUNG*)
  (setq dem 0)
  ;; 037: dam bao kieu Arial cho SO_TO TRUOC khi xoa/tao block; khong co thi khong dong vao block nao
  (if (setq *TVXD37-STY-SO-DANG* (tvxd37-sty-so-bao))
    (progn
      (foreach n *DT-TYLE*
        (setq dem (+ dem (dt-tao-1-block (cons n nil) ghide sty blks iu))))
      (foreach n *DT-TYLE-M*
        (setq dem (+ dem (dt-tao-1-block (cons n T)   ghide sty blks iu))))))
  dem)

(defun c:T-TKI ( / kw dem)
  (initget "Co Khong")
  (setq kw (getkword "\nTao lai ca nhung block da co san? [Co/Khong] <Khong>: "))
  (setq dem (dt-tao-bo-block (= kw "Co")))
  (princ (strcat "\n>> Da tao " (itoa dem) " block ("
                 (itoa (length *DT-TYLE*)) " he mm + "
                 (itoa (length *DT-TYLE-M*)) " he m). INSUNITS = "
                 (itoa (getvar "INSUNITS"))))
  (princ))


;;; ==================== PHAN 2: DAT KHUNG VUNG IN (T-CK) ===================

;;; --- Cac ty le dang co block trong ban ve --------------------------------
;;; Tra ve danh sach cap (mau-so . he-met). He m xep truoc cho de chon.
(defun dt-ds-tyle ( / bd s p lst)
  (setq lst nil  bd (tblnext "BLOCK" T))
  (while bd
    (setq s (cdr (assoc 2 bd)))
    (setq p (dt-tach-ten s))
    (if (and p (not (member p lst))) (setq lst (cons p lst)))
    (setq bd (tblnext "BLOCK")))
  (vl-sort lst
    (function (lambda (a b)
      (cond ((and (cdr a) (null (cdr b))) T)
            ((and (null (cdr a)) (cdr b)) nil)
            (T (< (car a) (car b))))))))

;;; --- So to lon nhat dang co, tra ve so tiep theo -------------------------
(defun dt-soto-tiep ( / ss i e s mx)
  (setq mx 0)
  (if (setq ss (ssget "_X" (list '(0 . "INSERT") '(410 . "Model")
                                 (cons 2 (strcat *DT-TIENTO* "*")))))
    (progn (setq i 0)
           (while (< i (sslength ss))
             (setq e (ssname ss i)  s (atoi (dt-att e "SO_TO")))
             (if (> s mx) (setq mx s))
             (setq i (1+ i)))))
  (1+ mx))

;;; --- Chen mot khung bang entmake, khong phu thuoc ATTDIA va ATTREQ -------
(defun dt-chen-khung (bname pt ang tento soto / e ed lst tag val lay off sso)
  ;; 037: kieu Arial cho SO_TO dam bao truoc khi chen; khong co thi khong chen gi
  (if (or (null (tblsearch "BLOCK" bname)) (null (setq sso (tvxd37-sty-so-bao))))
    nil
    (progn
      (setq lay (getvar "CLAYER"))
      (setq e (entnext (tblobjname "BLOCK" bname))  lst nil)
      (while e
        (setq ed (entget e))
        (if (= "ATTDEF" (cdr (assoc 0 ed))) (setq lst (cons ed lst)))
        (setq e (entnext e)))
      (setq lst (reverse lst))
      (entmake (list '(0 . "INSERT") '(100 . "AcDbEntity") (cons 8 lay)
                     '(100 . "AcDbBlockReference")
                     (if lst '(66 . 1) '(66 . 0))
                     (cons 2 bname) (cons 10 pt)
                     '(41 . 1.0) '(42 . 1.0) '(43 . 1.0) (cons 50 ang)))
      (foreach ad lst
        (setq tag (cdr (assoc 2 ad)))
        (setq val (cond ((= tag "TEN_TO") tento)
                        ((= tag "SO_TO")  soto)
                        (T (cdr (assoc 1 ad)))))
        ;; khung dat nghieng thi cho tu attdef cung phai xoay theo
        (setq off (dt-xoay (cdr (assoc 10 ad)) ang))
        (entmake (list '(0 . "ATTRIB") '(100 . "AcDbEntity") (cons 8 lay)
                       '(100 . "AcDbText")
                       (cons 10 (mapcar '+ off pt))
                       (cons 40 (cdr (assoc 40 ad)))
                       (cons 1 val)
                       ;; 037: SO_TO luon Arial, ke ca khi dinh nghia block cu dung kieu khac
                       (cons 7 (cond ((= tag "SO_TO") sso)
                                     ((assoc 7 ad) (cdr (assoc 7 ad)))
                                     (T "Standard")))
                       (cons 50 ang)
                       '(100 . "AcDbAttribute")
                       (cons 2 tag) (cons 70 (cdr (assoc 70 ad))))))
      (if lst (entmake '((0 . "SEQEND") (100 . "AcDbEntity") (8 . "0"))))
      T)))

(defun c:T-CK ( / ds vt r ins nm hs soto tento dem clayer0 *error*)
  (defun *error* (m)
    (dt-ao-xoa)
    (if clayer0 (setvar "CLAYER" clayer0))
    (if (and m (not (wcmatch (strcase m) "*CANCEL*,*QUIT*,*BREAK*")))
      (princ (strcat "\n" m)))
    (princ))
  ;; do o trong khung ten roi dong bo bo block cho khop
  (dt-dong-bo)
  ;; thieu block thi tu tao, khong bat nguoi dung go T-TKI
  (if (null (dt-ds-tyle))
    (progn
      (princ "\n>> Ban ve chua co block khung vung in, dang tao...")
      (princ (strcat "\n>> Xong " (itoa (dt-tao-bo-block nil)) " block."))))

  (setq ds (dt-ds-tyle))
  (if (null ds)
    (progn (princ "\n[DUNG] Khong tao duoc block khung vung in.") (princ))
    (progn
      ;; ban cu luu so nguyen, ban nay luu cap, gap so nguyen thi bo di
      (if (not (listp *DT-TL-DANGDUNG*)) (setq *DT-TL-DANGDUNG* nil))
      (if (null *DT-TL-DANGDUNG*) (setq *DT-TL-DANGDUNG* (car ds)))
      (setq vt (vl-position *DT-TL-DANGDUNG* ds))
      (setq vt (dt-chon-nhieu "CHEN KHUNG VUNG IN"
                 (list (list "Ty le :"    (mapcar 'dt-nhan-tl ds)
                             (if vt vt 0))
                       (list "Diem bat :" (dt-ds-bat) *DT-BATDIEM*))))
      (if (null vt)
        (progn (princ "\n>> Da huy.") (princ))
        (progn
          (setq *DT-BATDIEM*     (nth 1 vt))
          (setq *DT-TL-DANGDUNG* (nth (nth 0 vt) ds))
          (setq soto (dt-soto-tiep))
          (setq clayer0 (getvar "CLAYER"))
          (dt-lo-layer *DT-LAYERKHUNG*)
          (setvar "CLAYER" *DT-LAYERKHUNG*)
          (setq dem 0  r T)
          (while r
            (setq nm (dt-ten-block *DT-TL-DANGDUNG*))
            (setq hs (dt-hs *DT-TL-DANGDUNG*))
            (setq r (dt-bam-diem
                      (strcat "\n" (car (nth *DT-BATDIEM* *DT-DSBAT*))
                              " khung " (dt-nhan-tl *DT-TL-DANGDUNG*)
                              "  " (dt-so (* *DT-RONG* hs))
                              " x "  (dt-so (* *DT-CAO*  hs))
                              (if (cdr *DT-TL-DANGDUNG*) " m" " mm")
                              "  mview " (dt-so (/ 1.0 hs))
                              "  so to " (dt-pad2 soto)
                              "  goc " (rtos (dt-do *DT-GOC*) 2 2)
                              " [Tyle/Diem/Goc/Ghiten] (Enter de ket thuc): ")
                      hs))
            (cond
              ((= r "Tyle")
               (setq vt (dt-chon-nhieu "DOI TY LE VA DIEM BAT"
                          (list (list "Ty le :"    (mapcar 'dt-nhan-tl ds)
                                      (vl-position *DT-TL-DANGDUNG* ds))
                                (list "Diem bat :" (dt-ds-bat) *DT-BATDIEM*))))
               (if vt (progn (setq *DT-TL-DANGDUNG* (nth (nth 0 vt) ds))
                             (setq *DT-BATDIEM*     (nth 1 vt)))))
              ((= r "Diem")
               (setq *DT-BATDIEM* (rem (1+ *DT-BATDIEM*) (length *DT-DSBAT*)))
               (princ (strcat "\n>> Diem bat: " (car (nth *DT-BATDIEM* *DT-DSBAT*)))))
              ((= r "Ghiten")
               (setq *DT-HOITEN* (not *DT-HOITEN*))
               (princ (if *DT-HOITEN* "\n>> Se hoi ten to moi lan dat khung."
                                      "\n>> Khong hoi ten to nua.")))
              ((= r "Goc") (dt-hoi-goc))
              ((null r) nil)
              (T
               (setq tento (if *DT-HOITEN* (getstring T "\nTen to: ") ""))
               ;; doi diem vua bam ra tam khung theo kieu diem bat dang chon
               (setq ins (dt-ve-tam (trans r 1 0) hs))
               (if (dt-chen-khung nm ins *DT-GOC* tento (dt-pad2 soto))
                 (progn (setq dem (1+ dem)) (setq soto (1+ soto)))
                 (princ (strcat "\n[LOI] Khong co block " nm "."))))))
          (setvar "CLAYER" clayer0)
          (if (> dem 0) (redraw))
          (princ (strcat "\n>> Da dat " (itoa dem) " khung. Go T-DTL de sinh layout."))
          (princ))))))


;;; ============ NHOM HAM DUNG CHUNG CHO PHAN LAYOUT ========================

;;; --- Canh DUOI cua hinh chu nhat trong mot dinh nghia block ------------
;;; Tra ve hai diem (trai  phai) theo he toa do cua dinh nghia block.
(defun dt-day-block (nm / e ed pts xs ys)
  (if (null (tblsearch "BLOCK" nm))
    nil
    (progn
      (setq e (entnext (tblobjname "BLOCK" nm))  pts nil)
      (while (and e (null pts))
        (setq ed (entget e))
        (if (= "LWPOLYLINE" (cdr (assoc 0 ed)))
          (setq pts (mapcar 'cdr (vl-remove-if-not '(lambda (x) (= 10 (car x))) ed))))
        (setq e (entnext e)))
      (if pts
        (progn
          (setq xs (mapcar 'car pts)  ys (mapcar 'cadr pts))
          (list (list (apply 'min xs) (apply 'min ys) 0.0)
                (list (apply 'max xs) (apply 'min ys) 0.0)))
        nil))))

;;; --- Doi mot diem cua dinh nghia block ra WCS --------------------------
(defun dt-ra-wcs (p ins ang sx sy / q)
  (setq q (dt-xoay (list (* (car p) sx) (* (cadr p) sy) 0.0) ang))
  (list (+ (car ins) (car q)) (+ (cadr ins) (cadr q)) 0.0))

;;; --- Hai goc duoi cua mot khung vung in dang nam tren model ------------
;;; Lay thang tu hinh hoc cua block nen khung xoay bao nhieu, phong to thu
;;; nho ra sao deu ra dung hai diem, khong phai doc goc.
(defun dt-2diem-model (e / ed ins ang sx sy d)
  (setq ed  (entget e)
        ins (cdr (assoc 10 ed))
        ang (if (assoc 50 ed) (cdr (assoc 50 ed)) 0.0)
        sx  (if (assoc 41 ed) (cdr (assoc 41 ed)) 1.0)
        sy  (if (assoc 42 ed) (cdr (assoc 42 ed)) 1.0))
  (setq d (dt-day-block (cdr (assoc 2 ed))))
  (if d
    (list (dt-ra-wcs (car d) ins ang sx sy) (dt-ra-wcs (cadr d) ins ang sx sy))
    nil))

;;; --- Hai goc duoi cua o nhin tren giay ----------------------------------
(defun dt-2diem-giay (vp / ed c w h)
  (setq ed (entget vp)
        c  (cdr (assoc 10 ed))
        w  (cdr (assoc 40 ed))
        h  (cdr (assoc 41 ed)))
  (list (list (- (car c) (* 0.5 w)) (- (cadr c) (* 0.5 h)) 0.0)
        (list (+ (car c) (* 0.5 w)) (- (cadr c) (* 0.5 h)) 0.0)))

;;; --- Doc het khung vung in tren model, da sap theo so to ----------------
;;; Moi phan tu:  (so-to  tam  ten-to  chuoi-so-to  n  goc  goc-duoi-trai
;;;                goc-duoi-phai  hs)
;;;    n  = mau so ty le, chi de ghi ra khung ten va in thong bao
;;;    hs = he so nhan, phan layout dung cai nay de dat tam nhin va ty le
;;; Hai diem goc duoi la thu phan layout dung de xoay viewport.
(defun dt-doc-khung ( / ss i e ed pt bn tt st tl n ang d2 lst p hem hs)
  (setq lst nil)
  (if (setq ss (ssget "_X" (list '(0 . "INSERT") '(410 . "Model")
                                 (cons 2 (strcat *DT-TIENTO* "*")))))
    (progn
      (setq i 0)
      (while (< i (sslength ss))
        (setq e  (ssname ss i)  ed (entget e)
              pt (cdr (assoc 10 ed))  bn (cdr (assoc 2 ed))
              ang (if (assoc 50 ed) (cdr (assoc 50 ed)) 0.0)
              tt (dt-att e "TEN_TO")  st (dt-att e "SO_TO")
              tl (dt-att e "TY_LE"))
        ;; he don vi doc o ten block, mau so uu tien doc o thuoc tinh TY_LE
        (setq p   (dt-tach-ten bn))
        (setq hem (if p (cdr p) nil))
        (setq n   (atoi tl))
        (if (<= n 0) (setq n (if p (car p) 0)))
        (setq hs  (if (> n 0) (dt-hs (cons n hem)) 0.0))
        (setq d2 (dt-2diem-model e))
        ;; block la khung tu ve, khong do duoc hinh chu nhat thi dung so cai san
        (if (null d2)
          (setq d2 (list (mapcar '+ pt (dt-xoay (list (* -0.5 *DT-RONG* hs)
                                                      (* -0.5 *DT-CAO*  hs) 0.0) ang))
                         (mapcar '+ pt (dt-xoay (list (*  0.5 *DT-RONG* hs)
                                                      (* -0.5 *DT-CAO*  hs) 0.0) ang)))))
        (setq lst (cons (list (atoi st) pt tt st n ang (car d2) (cadr d2) hs) lst))
        (setq i (1+ i)))))
  (vl-sort lst
    (function (lambda (a b)
      (cond
        ((and (> (nth 0 a) 0) (> (nth 0 b) 0)) (< (nth 0 a) (nth 0 b)))
        ((> (nth 0 a) 0) T)
        ((> (nth 0 b) 0) nil)
        (T (if (< (abs (- (cadr (nth 1 a)) (cadr (nth 1 b)))) 1.0)
             (< (car  (nth 1 a)) (car  (nth 1 b)))
             (> (cadr (nth 1 a)) (cadr (nth 1 b))))))))))

;;; --- Huong am dang hien ra theo huong nao tren giay ---------------------
;;; Doi mot vector cua model sang he toa do man hinh cua viewport dang mo.
;;; Xoay dung thi goc nay bang dung huong ap ben giay.
(defun dt-goc-nhin (am / v)
  (setq v (vl-catch-all-apply 'trans
            (list (list (cos am) (sin am) 0.0) 0 2 T)))
  (if (vl-catch-all-error-p v) nil (atan (cadr v) (car v))))

;;; --- Xoay viewport cho huong am nam theo huong ap -----------------------
;;;   am : huong canh duoi khung, do ben model
;;;   ap : huong canh duoi o nhin, do ben giay, thuong bang 0
;;; Goi khi da o trong viewport dich. Dat mot chieu, do lai, lech thi dao
;;; chieu roi dat lai, nen khong phai sua tay chieu xoay cho tung ban ve.
(defun dt-dat-twist (vpo am ap / au ab ad phi tw g ok dau0)
  (setq au (getvar "AUNITS")  ab (getvar "ANGBASE")  ad (getvar "ANGDIR"))
  (setvar "AUNITS" 0) (setvar "ANGBASE" 0.0) (setvar "ANGDIR" 0)
  (setq phi (dt-chuan-pi (- ap am)))
  (if (< (abs phi) 1e-6)
    (command "_.DVIEW" "" "_TW" "0" "")
    (progn
      (if (null *DT-CHIEUTW*) (setq *DT-CHIEUTW* 1.0))
      (setq dau0 *DT-CHIEUTW*  ok nil)
      ;; Hai lan dau xoay bang DVIEW nhu thao tac tay, hai lan sau dat thang
      ;; thuoc tinh. Sau moi lan deu do lai huong dang hien ra tren giay.
      (foreach lan (list 1 2 3 4)
        (if (not ok)
          (progn
            (setq tw (dt-chuan-goc (* *DT-CHIEUTW* phi)))
            (if (< lan 3)
              (command "_.DVIEW" "" "_TW" (rtos (dt-do tw) 2 8) "")
              (if vpo (vl-catch-all-apply 'vla-put-TwistAngle (list vpo tw))))
            (setq g (dt-goc-nhin am))
            (if (and g (> (cos (- g ap)) 0.999))
              (setq ok T)
              (setq *DT-CHIEUTW* (- *DT-CHIEUTW*))))))
      (if (not ok)
        (progn
          ;; do khong an thi tra ve chieu quen thuoc roi bao de nguoi dung xem
          (setq *DT-CHIEUTW* dau0)
          (setq tw (dt-chuan-goc (* *DT-CHIEUTW* phi)))
          (command "_.DVIEW" "" "_TW" (rtos (dt-do tw) 2 8) "")
          (princ (strcat "\n[LUU Y] To lech " (rtos (dt-do phi) 2 2)
                         " do chua chac da xoay dung huong, xem lai to nay."))))))
  (setvar "AUNITS" au) (setvar "ANGBASE" ab) (setvar "ANGDIR" ad)
  tw)

;;; --- Can mot viewport theo HAI CAP DIEM, dung nhu ALIGNSPACE -----------
;;;   P1m P2m : hai goc duoi cua khung vung in, ben model
;;;   Q1p Q2p : hai goc duoi cua o nhin trong khung ten, ben giay
;;;   hs      : he so nhan, mot mm giay bang bao nhieu don vi model
;;; Ba viec lam giong het thao tac tay:
;;;   xoay   cho huong P1m->P2m nam theo huong Q1p->Q2p
;;;   ZOOM C dua diem model ung voi tam o nhin ve giua o nhin
;;;   scale  1/hs   (he mm 1:500 ra 1/500, he m 1:500 ra 2)
;;; Layout chua viewport phai dang mo truoc khi goi.
(defun dt-align-viewport (vp P1m P2m Q1p Q2p hs caoVP / vpo id am ap Cp dq Cm)
  (setq vpo (vlax-ename->vla-object vp))
  (vl-catch-all-apply 'vla-put-DisplayLocked (list vpo :vlax-false))
  (vl-catch-all-apply 'vla-put-ViewportOn    (list vpo :vlax-true))
  (dt-ve-giay)
  (setq am (angle P1m P2m)
        ap (angle Q1p Q2p))
  ;; tam o nhin tren giay, doi nguoc ve ben model: dai gap n lan, xoay am-ap
  (setq Cp (cdr (assoc 10 (entget vp))))
  (setq dq (list (- (car Cp) (car Q1p)) (- (cadr Cp) (cadr Q1p)) 0.0))
  (setq Cm (mapcar '+ P1m (dt-xoay (list (* (car dq) hs) (* (cadr dq) hs) 0.0)
                                   (- am ap))))
  (setq id (cdr (assoc 69 (entget vp))))
  (if (and id (> id 1))
    (vl-catch-all-apply
      '(lambda ()
         (command "_.MSPACE")
         (setvar "CVPORT" id)
         ;; ban ve khao sat hay dung UCS rieng, ep ve World roi hay dua so
         (command "_.UCS" "_W")
         (dt-dat-twist vpo am ap)
         (command "_.ZOOM" "_C" (dt-p2s Cm) (rtos (* caoVP hs) 2 8)))))
  (dt-ve-giay)
  (vl-catch-all-apply 'vla-put-CustomScale (list vpo (/ 1.0 (float hs))))
  (if id id 0))

;;; --- Can viewport vao mot to da doc duoc tu dt-doc-khung ---------------
(defun dt-can-to (vp r caoVP / qq)
  (setq qq (dt-2diem-giay vp))
  (dt-align-viewport vp (nth 6 r) (nth 7 r) (car qq) (cadr qq) (nth 8 r) caoVP))

;;; --- Tim to ung voi mot layout theo ten layout -------------------------
;;; Ten layout do T-DTL dat theo "<so to> - <ten to>", nen cat phan dau ra doi
;;; chieu voi so to. Khong ra thi doi chieu ca ten day du.
(defun dt-to-cua-layout (lname lst / v s kq r)
  (setq v (vl-string-search *DT-NGAN* lname))
  (setq s (vl-string-trim " " (if v (substr lname 1 v) lname)))
  (setq kq nil)
  (if (> (atoi s) 0)
    (foreach r lst
      (if (and (null kq) (= (atoi s) (atoi (nth 3 r)))) (setq kq r))))
  (if (null kq)
    (foreach r lst
      (if (and (null kq)
               (= (strcase lname)
                  (strcase (dt-sach (cond ((and (/= (nth 3 r) "") (/= (nth 2 r) ""))
                                           (strcat (nth 3 r) *DT-NGAN* (nth 2 r)))
                                          ((/= (nth 3 r) "") (nth 3 r))
                                          (T (nth 2 r)))))))
        (setq kq r))))
  kq)


;;; ===================== PHAN 3: SINH LAYOUT (T-DTL) =======================

(defun dt-sach (s / i c r)
  (setq r "" i 1)
  (while (<= i (strlen s))
    (setq c (substr s i 1))
    (setq r (strcat r (if (vl-string-search c "<>/\\\":;?*|=,") "-" c)))
    (setq i (1+ i)))
  (vl-string-trim " " r))

(defun dt-vp (lname / ss i e ed vp)
  (setq vp nil)
  (if (setq ss (ssget "_X" (list '(0 . "VIEWPORT") (cons 410 lname))))
    (progn (setq i 0)
           (while (and (< i (sslength ss)) (null vp))
             (setq e (ssname ss i) ed (entget e))
             (if (/= 1 (cdr (assoc 69 ed))) (setq vp e))
             (setq i (1+ i)))))
  vp)

(defun dt-co-layout (nm / doc kq)
  (setq doc (vla-get-ActiveDocument (vlax-get-acad-object))  kq nil)
  (vlax-for l (vla-get-Layouts doc)
    (if (= (strcase (vla-get-Name l)) (strcase nm)) (setq kq T)))
  kq)

(defun dt-ds-layout ( / doc lst)
  (setq doc (vla-get-ActiveDocument (vlax-get-acad-object))  lst nil)
  (vlax-for l (vla-get-Layouts doc)
    (if (/= (strcase (vla-get-Name l)) "MODEL")
      (setq lst (cons (vla-get-Name l) lst))))
  (reverse lst))

(defun dt-ten-duy-nhat (nm / g t2)
  (setq g 2  t2 nm)
  (while (dt-co-layout t2) (setq t2 (strcat nm " (" (itoa g) ")")) (setq g (1+ g)))
  t2)

(defun dt-ghi-tyle (lname n / ss i e ed noidung cnt)
  (setq cnt 0  noidung (strcat *DT-TIENTOTL* (itoa n)))
  (if (setq ss (ssget "_X" (list '(0 . "TEXT,MTEXT")
                                 (cons 8 *DT-LAYERTL*) (cons 410 lname))))
    (progn (setq i 0)
           (while (< i (sslength ss))
             (setq e (ssname ss i) ed (entget e))
             (if (assoc 1 ed)
               (progn (entmod (subst (cons 1 noidung) (assoc 1 ed) ed))
                      (entupd e) (setq cnt (1+ cnt))))
             (setq i (1+ i)))))
  cnt)

;;; --- Sap lai toan bo day tab --------------------------------------------
;;; Cac layout cu giu nguyen thu tu, cac layout moi tao xep sau, theo dung
;;; thu tu trong dsTao. Gan so thu tu chay tu 1 tro di, vi gan gia tri nam
;;; ngoai khoang hop le thi AutoCAD bo qua.
(defun dt-sap-tab (dsTao / doc lays cu tat i tenUP)
  (setq doc  (vla-get-ActiveDocument (vlax-get-acad-object)))
  (setq lays (vla-get-Layouts doc))
  (setq tenUP (mapcar 'strcase dsTao))
  (setq cu nil)
  (vlax-for l lays
    (if (and (/= (strcase (vla-get-Name l)) "MODEL")
             (not (member (strcase (vla-get-Name l)) tenUP)))
      (setq cu (cons (cons (vla-get-TabOrder l) (vla-get-Name l)) cu))))
  (setq cu (mapcar 'cdr (vl-sort cu '(lambda (a b) (< (car a) (car b))))))
  (setq tat (append cu dsTao))
  (setq i 1)
  (foreach nm tat
    (vl-catch-all-apply 'vla-put-TabOrder (list (vla-Item lays nm) i))
    (setq i (1+ i))))

;;; --- Tim layout mau IM LANG, khong hoi. Dung de do kich thuoc ------------
(defun dt-tim-layout-mau-im ( / ds kq)
  (cond
    ((and (dt-co-layout *DT-LAYOUTMAU*) (dt-vp *DT-LAYOUTMAU*)) *DT-LAYOUTMAU*)
    ((and *DT-LMAU-LANTRUOC* (dt-co-layout *DT-LMAU-LANTRUOC*)
          (dt-vp *DT-LMAU-LANTRUOC*)) *DT-LMAU-LANTRUOC*)
    (T
     (setq kq nil)
     (foreach l (dt-ds-layout)
       (if (and (null kq) (dt-vp l)) (setq kq l)))
     kq)))

;;; --- Doc kich thuoc viewport cua layout mau ------------------------------
;;; Tra ve (be-rong  chieu-cao) tinh bang mm giay, hoac nil neu khong co.
(defun dt-vung-nhin ( / lm vp o)
  (setq lm (dt-tim-layout-mau-im))
  (if (and lm (setq vp (dt-vp lm)))
    (progn
      (setq o (vlax-ename->vla-object vp))
      (list (vla-get-Width o) (vla-get-Height o)))
    nil))

;;; --- Do be rong hinh chu nhat trong mot dinh nghia block -----------------
(defun dt-rong-block (nm / e ed xs)
  (if (null (tblsearch "BLOCK" nm))
    nil
    (progn
      (setq e (entnext (tblobjname "BLOCK" nm))  xs nil)
      (while (and e (null xs))
        (setq ed (entget e))
        (if (= "LWPOLYLINE" (cdr (assoc 0 ed)))
          (setq xs (mapcar 'car
                     (mapcar 'cdr
                       (vl-remove-if-not '(lambda (x) (= 10 (car x))) ed)))))
        (setq e (entnext e)))
      (if xs (- (apply 'max xs) (apply 'min xs)) nil))))

;;; --- Dem so khung vung in dang co tren model -----------------------------
(defun dt-dem-khung ( / ss)
  (if (setq ss (ssget "_X" (list '(0 . "INSERT") '(410 . "Model")
                                 (cons 2 (strcat *DT-TIENTO* "*")))))
    (sslength ss) 0))

;;; --- Dong bo kich thuoc block voi o trong cua khung ten ------------------
;;; Doc kich thuoc viewport cua layout mau. Khac voi bo block dang co thi
;;; dung lai bo block cho khop. Nho vay doi khung ten khac khong phai sua
;;; dong nao trong file nay.
(defun dt-dong-bo (/ vn ds n0 hs0 r0 sk kw)
  (setq vn (dt-vung-nhin))
  (if (null vn)
    (princ (strcat "\n[LUU Y] Ban ve chua co layout nao co viewport."
                   "\n        Tam dung so cai san: " (rtos *DT-RONG* 2 2)
                   " x " (rtos *DT-CAO* 2 2) " mm."))
    (progn
      (if (or (> (abs (- (car  vn) *DT-RONG*)) 0.01)
              (> (abs (- (cadr vn) *DT-CAO*))  0.01))
        (progn
          (princ (strcat "\n>> O trong khung ten do duoc: "
                         (rtos (car vn) 2 2) " x " (rtos (cadr vn) 2 2) " mm."))
          (setq *DT-RONG* (car vn))
          (setq *DT-CAO*  (cadr vn))))
      ;; bo block dang co con khop khong
      (setq ds (dt-ds-tyle))
      (if ds
        (progn
          (setq n0 (car ds))
          (setq hs0 (dt-hs n0))
          (setq r0 (dt-rong-block (dt-ten-block n0)))
          (if (and r0 (> (abs (- r0 (* *DT-RONG* hs0))) 0.01))
            (progn
              (setq sk (dt-dem-khung))
              (if (= sk 0)
                (progn
                  (princ "\n>> Bo block cu khong khop, dang dung lai...")
                  (princ (strcat "\n>> Xong " (itoa (dt-tao-bo-block T)) " block.")))
                (progn
                  (princ (strcat "\n[CANH BAO] Bo block dang co ung voi o trong "
                                 (rtos (/ r0 hs0) 2 2) " mm, khac so vua do."))
                  (princ (strcat "\n            Ban ve dang co " (itoa sk)
                                 " khung da dat. Dung lai bo block se XOA het chung."))
                  (initget "Co Khong")
                  (setq kw (getkword "\nDung lai bo block? [Co/Khong] <Khong>: "))
                  (if (= kw "Co")
                    (princ (strcat "\n>> Da dung lai " (itoa (dt-tao-bo-block T)) " block."))
                    (progn
                      (princ "\n>> Giu bo block cu. Dat lai so do cu de khung van dong nhat.")
                      (setq *DT-RONG* (/ r0 hs0))
                      (setq *DT-CAO*  (* *DT-RONG* (/ (cadr vn) (car vn)))))))))))))))

;;; --- Xac dinh layout mau: uu tien MAU, roi lan truoc, roi hoi nguoi dung -
(defun dt-tim-layout-mau ( / ds vt)
  (cond
    ((dt-co-layout *DT-LAYOUTMAU*) *DT-LAYOUTMAU*)
    ((and *DT-LMAU-LANTRUOC* (dt-co-layout *DT-LMAU-LANTRUOC*)) *DT-LMAU-LANTRUOC*)
    (T
     (setq ds (dt-ds-layout))
     (if (null ds)
       nil
       (progn
         (setq vt (dt-chon-nhieu "CHON LAYOUT MAU" (list (list "Layout mau :" ds 0))))
         (if vt (nth (car vt) ds) nil))))))

(defun c:T-DTL ( / doc ss i e ed pt bn tt st tl n ang nm lst caoVP vpm lmau r
                 kw xoacu ctab0 dem bo loi idx vp vpo err gtl demTL dsTao dsLog lst2)

  (setq doc   (vla-get-ActiveDocument (vlax-get-acad-object)))
  (setq ctab0 (getvar "CTAB"))

  ;; --- layout mau ---
  (setq lmau (dt-tim-layout-mau))
  (if (null lmau)
    (progn (princ "\n[DUNG] Ban ve khong co layout nao de lam mau.") (princ))
    (progn
      (setq *DT-LMAU-LANTRUOC* lmau)
      (setq vpm (dt-vp lmau))
      (if (null vpm)
        (progn
          (princ (strcat "\n[DUNG] Layout \"" lmau "\" khong co viewport nao."))
          (princ))
        (progn
          (setq caoVP (vla-get-Height (vlax-ename->vla-object vpm)))
          (princ (strcat "\n>> Layout mau: \"" lmau "\", viewport cao "
                         (rtos caoVP 2 4) " mm giay."))

          (setq lst (dt-doc-khung))
          (if (null lst)
            (progn (princ "\n[DUNG] Khong co khung vung in nao tren Model. Go T-CK truoc.")
                   (princ))
            (progn
              (princ (strcat "\n>> Tim thay " (itoa (length lst)) " khung vung in."))

              (initget "Boqua Taolai")
              (setq kw (getkword "\nLayout trung ten thi [Boqua/Taolai] <Boqua>: "))
              (setq xoacu (= kw "Taolai"))

              (setq dem 0  bo 0  loi 0  idx 0  demTL 0  dsTao nil  dsLog nil)

              ;; Gan so thu tu roi DAO NGUOC danh sach truoc khi tao.
              ;; Lenh LAYOUT Copy dat ban sao ngay SAU layout nguon, nen tao
              ;; theo thu tu nguoc thi day tab tu no xep tu nho den lon.
              (setq lst2 nil)
              (foreach r lst
                (setq idx (1+ idx))
                (setq lst2 (cons (append r (list idx)) lst2)))

              (foreach r lst2
                (setq pt  (nth 1 r) tt (nth 2 r) st  (nth 3 r)
                      n   (nth 4 r) ang (nth 5 r) idx (nth 9 r))
                (setq nm (dt-sach
                  (cond ((and (/= st "") (/= tt "")) (strcat st *DT-NGAN* tt))
                        ((/= st "") st)
                        ((/= tt "") tt)
                        (T (dt-pad2 idx)))))
                (cond
                  ((<= n 0)
                   (setq loi (1+ loi))
                   (setq dsLog (cons (strcat "\n   [BO] " nm ", khong doc duoc ty le.") dsLog)))
                  ((and (dt-co-layout nm) (not xoacu))
                   (setq bo (1+ bo))
                   (setq dsLog (cons (strcat "\n   [BO] " nm ", layout da co san.") dsLog)))
                  (T
                   (if (and (dt-co-layout nm) xoacu)
                     (vl-catch-all-apply '(lambda () (command "_.LAYOUT" "_D" nm))))
                   (setq nm (dt-ten-duy-nhat nm))
                   (setq err (vl-catch-all-apply
                               '(lambda () (command "_.LAYOUT" "_C" lmau nm))))
                   (setq vp (dt-vp nm))
                   (if (or (vl-catch-all-error-p err) (null vp))
                     (progn (setq loi (1+ loi))
                            (setq dsLog (cons (strcat "\n   [LOI] " nm ", khong sao chep duoc layout mau.") dsLog)))
                     (progn
                       (setq vpo (vlax-ename->vla-object vp))
                       (vl-catch-all-apply '(lambda () (setvar "CTAB" nm)))
                       (dt-can-to vp r caoVP)
                       (vl-catch-all-apply 'vla-put-DisplayLocked (list vpo :vlax-true))
                       (setq gtl (dt-ghi-tyle nm n))
                       (if (> gtl 0) (setq demTL (1+ demTL)))
                       (setq dem (1+ dem))
                       (setq dsTao (cons nm dsTao))
                       (setq dsLog (cons (strcat "\n   + " nm "   ty le 1:" (itoa n)
                                      (if (< (abs (dt-chuan-pi ang)) 1e-6) ""
                                        (strcat "   goc " (rtos (dt-do ang) 2 2)))
                                      (if (> gtl 0) "" "   [chua ghi duoc ty le]")) dsLog)))))))

              ;; in lai thong bao theo dung thu tu so to
              (foreach x dsLog (princ x))

              ;; lop thu hai: gan lai so thu tu tab cho chac
              (if dsTao (dt-sap-tab dsTao))

              (vl-catch-all-apply '(lambda () (setvar "CTAB" ctab0)))
              (princ (strcat "\n>> DA TAO " (itoa dem) " layout."))
              (if (> dem 0)
                (princ (strcat "\n>> Ghi ty le vao khung ten cho " (itoa demTL) "/" (itoa dem) " layout.")))
              (if (> bo 0)  (princ (strcat "\n>> Bo qua " (itoa bo) " layout da co san.")))
              (if (> loi 0) (princ (strcat "\n>> " (itoa loi) " khung khong xu ly duoc.")))
              (princ))))))))



;;; ======================================================================
;;;  DAN NHIEU KHUNG TEN TREN MOT LAYOUT  -  LENH T-DTM
;;;  --------------------------------------------------------------------
;;;  T-DTL sinh moi to mot layout rieng. T-DTM xep tat ca cac to len CUNG MOT
;;;  layout, moi to mot khung ten va mot viewport.
;;;  Ben model khong doi gi, van dat khung bang lenh T-CK.
;;;
;;;  Layout dung de chay T-DTM can co san MOT khung ten va MOT viewport.
;;;  Lisp do vi tri tuong doi cua viewport so voi goc khung ten roi nhan
;;;  ban ca cap do ra du so to.
;;;
;;;  T-DTM KHONG goi dt-dong-bo. Luc chay T-DTM thi khung ben model da dat xong,
;;;  dung lai bo block se xoa het chung. Viec do bo kich thuoc thuoc ve T-CK.
;;; ======================================================================

(defun dt-la-xref (bn / bd)
  (setq bd (tblsearch "BLOCK" bn))
  (if bd (= 4 (logand (cdr (assoc 70 bd)) 4)) nil))

(defun dt-tim-khungten (lname / ss i e lst kq)
  (setq lst nil)
  (if (setq ss (ssget "_X" (list '(0 . "INSERT") (cons 410 lname))))
    (progn
      (setq i 0)
      (while (< i (sslength ss))
        (setq e (ssname ss i))
        (if (dt-la-xref (cdr (assoc 2 (entget e)))) (setq lst (cons e lst)))
        (setq i (1+ i)))))
  (cond
    ((= (length lst) 1) (car lst))
    (T
     (princ "\n>> Khong xac dinh duoc khung ten tren layout nay.")
     (setq kq (entsel "\nBam chon khung ten: "))
     (if kq (car kq) nil))))

(defun dt-khung-bao (e / o mn mx a b)
  (setq o (vlax-ename->vla-object e))
  (vla-GetBoundingBox o 'mn 'mx)
  (setq a (vlax-safearray->list mn)
        b (vlax-safearray->list mx))
  (list (- (car b) (car a)) (- (cadr b) (cadr a))))

(defun dt-text-layer (lname lay / ss)
  (if (setq ss (ssget "_X" (list '(0 . "TEXT,MTEXT") (cons 8 lay) (cons 410 lname))))
    (ssname ss 0)
    nil))

(defun dt-ghi-text (e s / ed)
  (if e
    (progn
      (setq ed (entget e))
      (if (assoc 1 ed)
        (progn (entmod (subst (cons 1 s) (assoc 1 ed) ed)) (entupd e))))))

;;; --- Ep ve khong gian GIAY -----------------------------------------------
;;; Dang o trong viewport ma di chep doi tuong tren giay thi AutoCAD bao
;;; "1 was not in current space", lenh bi huy va cac tham so con lai tran
;;; ra thanh lenh rac.
(defun dt-ve-giay ()
  (if (and (= 0 (getvar "TILEMODE")) (/= 1 (getvar "CVPORT")))
    (vl-catch-all-apply '(lambda () (command "_.PSPACE")))))

;;; --- Chep mot doi tuong tu p0 sang p1, tra ve ban sao -------------------
;;; Dung vla-Copy chu khong lai lenh COPY, nen khong the tran tham so.
(defun dt-chep (e p0 p1 / o o2)
  (if (null e)
    nil
    (progn
      (dt-ve-giay)
      (setq o  (vlax-ename->vla-object e))
      (setq o2 (vl-catch-all-apply 'vla-Copy (list o)))
      (if (vl-catch-all-error-p o2)
        nil
        (progn
          (vl-catch-all-apply 'vla-Move
            (list o2 (vlax-3d-point p0) (vlax-3d-point p1)))
          (vlax-vla-object->ename o2))))))

;;; --- Dat tam nhin va ty le cho mot viewport ------------------------------
;;;  Chi goi tren CHINH viewport GOC, khong goi tren cac ban sao.
;;;  So hieu viewport bi danh lai moi khi them bot viewport, nen dat tam nhin
;;;  cho ban sao vua tao ra rat de ban nham. Dat truoc roi chep thi ban sao
;;;  thua huong tam nhin, khong can dong den so hieu cua no.
(defun dt-dat-nhin (vp r caoVP)
  (dt-can-to vp r caoVP))

;;; --- Khoa het viewport tren layout, goi mot lan o cuoi -------------------
(defun dt-khoa-het (lname / e)
  (foreach e (dt-ds-ename lname '((0 . "VIEWPORT")))
    (if (/= 1 (cdr (assoc 69 (entget e))))
      (vl-catch-all-apply 'vla-put-DisplayLocked
        (list (vlax-ename->vla-object e) :vlax-true)))))

;;; --- Giu lai doi tuong trai nhat, tren nhat; xoa het cac cai con lai -----
(defun dt-giu-mot (ds / e p best bp)
  (if (null ds)
    nil
    (progn
      (setq best nil bp nil)
      (foreach e ds
        (setq p (cdr (assoc 10 (entget e))))
        (if (or (null best)
                (< (car p) (- (car bp) 0.000001))
                (and (< (abs (- (car p) (car bp))) 0.000001)
                     (> (cadr p) (cadr bp))))
          (setq best e  bp p)))
      (foreach e ds (if (not (eq e best)) (entdel e)))
      best)))

;;; --- Danh sach ename tren mot layout theo bo loc ------------------------
(defun dt-ds-ename (lname loc / ss i lst)
  (setq lst nil)
  (if (setq ss (ssget "_X" (append loc (list (cons 410 lname)))))
    (progn
      (setq i 0)
      (while (< i (sslength ss))
        (setq lst (cons (ssname ss i) lst))
        (setq i (1+ i)))))
  (reverse lst))

;;; --- Chi giu mot khung ten, mot viewport, mot chu moi layer -------------
;;; Chay T-DTM nhieu lan ma khong don thi cac lan truoc chong len nhau.
(defun dt-don-layout (lname / dsKT dsVP e)
  (setq dsKT nil)
  (foreach e (dt-ds-ename lname '((0 . "INSERT")))
    (if (dt-la-xref (cdr (assoc 2 (entget e)))) (setq dsKT (cons e dsKT))))
  (setq dsVP nil)
  (foreach e (dt-ds-ename lname '((0 . "VIEWPORT")))
    (if (/= 1 (cdr (assoc 69 (entget e)))) (setq dsVP (cons e dsVP))))
  (dt-giu-mot (reverse dsKT))
  (dt-giu-mot (reverse dsVP))
  (dt-giu-mot (dt-ds-ename lname (list '(0 . "TEXT,MTEXT") (cons 8 *DT-LAYERTL*))))
  (dt-giu-mot (dt-ds-ename lname (list '(0 . "TEXT,MTEXT") (cons 8 *DT-LAYERTEN*))))
  (dt-giu-mot (dt-ds-ename lname (list '(0 . "TEXT,MTEXT") (cons 8 *DT-LAYERSO*))))
  (length dsKT))

(defun c:T-DTM ( / lname vp vpm kt p0 bao pitchX pitchY caoVP ss i e ed
                 pt bn tt st tl n ang lst vt huong socot dem
                 tTL tTEN tSO r c p1 kt2 vp2 t2TL t2TEN t2SO idx rr idvp soKT kw ce dau)

  (setq lname (getvar "CTAB"))
  (if (= (strcase lname) "MODEL")
    (progn (princ "\n[DUNG] Hay bam sang layout muon dan trang roi go T-DTM.") (princ))
    (progn
      (dt-ve-giay)
      (vl-catch-all-apply '(lambda () (setvar "MAXACTVP" 64)))
      (setq ce (getvar "CMDECHO"))
      (setvar "CMDECHO" 0)

      ;; Don cac khung da dan tu lan chay truoc, chi giu lai mot bo
      (setq soKT (dt-don-layout lname))
      (if (> soKT 1)
        (princ (strcat "\n>> Da don " (itoa (- soKT 1))
                       " khung ten cua lan dan truoc, chi giu lai mot bo.")))

      (setq vp (dt-vp lname))
      (setq kt (dt-tim-khungten lname))
      (cond
        ((null vp)
         (setvar "CMDECHO" ce)
         (princ "\n[DUNG] Layout nay chua co viewport nao. Go MV bat hai goc o trong.")
         (princ))
        ((null kt)
         (setvar "CMDECHO" ce)
         (princ "\n[DUNG] Khong tim thay khung ten tren layout nay.")
         (princ))
        (T
         (setq vpm   (vlax-ename->vla-object vp))
         (setq caoVP (vla-get-Height vpm))
         (setq p0    (cdr (assoc 10 (entget kt))))
         (setq bao (if *DT-KHOGIAY* *DT-KHOGIAY* (dt-khung-bao kt)))
         (setq pitchX (+ (car  bao) *DT-HOHANG*))
         (setq pitchY (+ (cadr bao) *DT-HOHANG*))
         (princ (strcat "\n>> Buoc xep: " (rtos pitchX 2 1) " x " (rtos pitchY 2 1)
                        " mm  (kho giay " (rtos (car bao) 2 1) " x " (rtos (cadr bao) 2 1)
                        " cong ho " (rtos *DT-HOHANG* 2 1) ")"))

         (setq lst (dt-doc-khung))
         (if (null lst)
           (progn (setvar "CMDECHO" ce)
                  (princ "\n[DUNG] Khong co khung vung in nao tren Model. Go T-CK truoc.") (princ))
           (progn
             (setq vt (dt-chon-nhieu "DAN TRANG TREN MOT LAYOUT"
                        (list (list "Huong xep :"  (list "Ngang" "Doc") 0)
                              (list "So khung moi hang :"
                                    (list "Khong xuong dong" "2" "3" "4" "5" "6" "8" "10") 0))))
             (if (null vt)
               (progn (setvar "CMDECHO" ce) (princ "\n>> Da huy.") (princ))
               (progn
                 (setq huong (nth 0 vt))
                 (setq socot (nth 1 vt))
                 (setq socot (if (= socot 0) 9999
                               (atoi (nth socot (list "" "2" "3" "4" "5" "6" "8" "10")))))

                 (setq tTL  (dt-text-layer lname *DT-LAYERTL*))
                 (setq tTEN (dt-text-layer lname *DT-LAYERTEN*))
                 (setq tSO  (dt-text-layer lname *DT-LAYERSO*))

                 (setq dem 0 idx 0 dau nil)
                 (foreach rr lst
                   (setq pt (nth 1 rr) tt (nth 2 rr) st (nth 3 rr)
                         n  (nth 4 rr) ang (nth 5 rr))
                   (if (<= n 0)
                     (princ "\n   [BO] mot khung khong doc duoc ty le.")
                     (progn
                       (if (= idx 0)
                         ;; To dau tien dung luon bo goc, de xu ly sau cung
                         (setq dau rr)
                         (progn
                           (if (= huong 0)
                             (setq c (rem idx socot)  r (/ idx socot))
                             (setq r (rem idx socot)  c (/ idx socot)))
                           (setq p1 (list (+ (car  p0) (* c pitchX))
                                          (- (cadr p0) (* r pitchY))
                                          0.0))
                           ;; Dat tam nhin len viewport GOC truoc, roi moi chep.
                           ;; Ban sao thua huong tam nhin va ty le.
                           (setq idvp (dt-dat-nhin vp rr caoVP))
                           (setq kt2   (dt-chep kt   p0 p1))
                           (setq vp2   (dt-chep vp   p0 p1))
                           (setq t2TL  (dt-chep tTL  p0 p1))
                           (setq t2TEN (dt-chep tTEN p0 p1))
                           (setq t2SO  (dt-chep tSO  p0 p1))
                           (dt-ghi-text t2TL  (strcat *DT-TIENTOTL* (itoa n)))
                           (dt-ghi-text t2TEN tt)
                           (dt-ghi-text t2SO  st)
                           (princ (strcat "\n   + to " (if (= st "") (dt-pad2 (1+ idx)) st)
                                          "  1:" (itoa n)
                                          "  cot " (itoa c) " hang " (itoa r)
                                          "  tai " (rtos (car p1) 2 0) "," (rtos (cadr p1) 2 0)
                                          "  vp#" (itoa idvp)))))
                       (setq dem (1+ dem))
                       (setq idx (1+ idx)))))

                 ;; To dau tien: dat tam nhin cho chinh viewport goc
                 (if dau
                   (progn
                     (setq idvp (dt-dat-nhin vp dau caoVP))
                     (dt-ghi-text tTL  (strcat *DT-TIENTOTL* (itoa (nth 4 dau))))
                     (dt-ghi-text tTEN (nth 2 dau))
                     (dt-ghi-text tSO  (nth 3 dau))
                     (princ (strcat "\n   + to " (if (= (nth 3 dau) "") "01" (nth 3 dau))
                                    "  1:" (itoa (nth 4 dau))
                                    "  cot 0 hang 0"
                                    "  tai " (rtos (car p0) 2 0) "," (rtos (cadr p0) 2 0)
                                    "  vp#" (itoa idvp) "  (khung goc)"))))

                 (dt-khoa-het lname)

                 (if (> dem 64)
                   (princ (strcat "\n[LUU Y] Co " (itoa dem) " viewport tren mot layout."
                                  "\n        Go MAXACTVP dat 64 neu thay to nao bi trong.")))
                 (vl-catch-all-apply '(lambda () (command "_.REGENALL")))
                 (setvar "CMDECHO" ce)
                 (princ (strcat "\n>> Da dan " (itoa dem) " to len layout " lname "."))
                 (princ))))))))))

;;; ======================================================================
;;;  CAN LAI MOT VIEWPORT  -  LENH T-DTV
;;;  --------------------------------------------------------------------
;;;  Dung khi chi phai sua mot to: layout da co san, chon viewport roi chon
;;;  to muon nhin. Lisp xoay, dat tam nhin va ty le giong het T-DTL.
;;; ======================================================================

;;; 037: KHONG con la lenh (nguoi dung bo T-DTV). Giu than ham, ten noi bo.
(defun tvxd37-bo-T-DTV ( / lname vp lst nhan vt r pt ang n caoVP ce)
  (setq lname (getvar "CTAB"))
  (if (= (strcase lname) "MODEL")
    (progn (princ "\n[DUNG] Hay bam sang layout can sua roi go T-DTV.") (princ))
    (progn
      (dt-ve-giay)
      (setq ce (getvar "CMDECHO"))
      (setvar "CMDECHO" 0)
      (setq vp (car (entsel "\nChon viewport can can lai (Enter de lay cai dau tien): ")))
      (if (or (null vp) (/= "VIEWPORT" (cdr (assoc 0 (entget vp)))))
        (setq vp (dt-vp lname)))
      (setq lst (dt-doc-khung))
      (cond
        ((null vp)
         (setvar "CMDECHO" ce)
         (princ "\n[DUNG] Layout nay khong co viewport nao.") (princ))
        ((null lst)
         (setvar "CMDECHO" ce)
         (princ "\n[DUNG] Khong co khung vung in nao tren Model. Go T-CK truoc.") (princ))
        (T
         (setq nhan
           (mapcar '(lambda (x)
                      (strcat (if (= (nth 3 x) "") "--" (nth 3 x))
                              "  1:" (itoa (nth 4 x))
                              (if (= (nth 2 x) "") "" (strcat "  " (nth 2 x)))))
                   lst))
         ;; doan xem layout dang mo ung voi to nao, de san dong do trong bang
         (setq r (dt-to-cua-layout lname lst))
         (setq vt (dt-chon-nhieu "CHON TO CHO VIEWPORT"
                    (list (list "To :" nhan (if r (vl-position r lst) 0)))))
         (if (null vt)
           (progn (setvar "CMDECHO" ce) (princ "\n>> Da huy.") (princ))
           (progn
             (setq r   (nth (car vt) lst))
             (setq pt  (nth 1 r)  n (nth 4 r)  ang (nth 5 r))
             (setq caoVP (vla-get-Height (vlax-ename->vla-object vp)))
             (dt-can-to vp r caoVP)
             (vl-catch-all-apply 'vla-put-DisplayLocked
               (list (vlax-ename->vla-object vp) :vlax-true))
             (dt-ghi-tyle lname n)
             (setvar "CMDECHO" ce)
             (princ (strcat "\n>> Viewport da nhin to " (nth 3 r)
                            "  ty le 1:" (itoa n)
                            (if (< (abs (dt-chuan-pi ang)) 1e-6) ""
                              (strcat "  goc " (rtos (dt-do ang) 2 2)))))
             (princ))))))))

;;; ======================================================================
;;;  CAN LAI TAT CA LAYOUT  -  LENH T-DTA
;;;  --------------------------------------------------------------------
;;;  Duyet moi layout dang co, doc so to trong ten layout, tim khung mang
;;;  so to ay ben model roi ap vao viewport cua layout do. Layout nao khong
;;;  doi chieu duoc thi bo qua va bao lai.
;;; ======================================================================

;;; 037: KHONG con la lenh (nguoi dung bo T-DTA). Giu than ham, ten noi bo.
(defun tvxd37-bo-T-DTA ( / lst ds ctab0 ce kw dem bo r vp caoVP dsLog lname x)
  (setq lst (dt-doc-khung))
  (setq ds  (dt-ds-layout))
  (cond
    ((null lst)
     (princ "\n[DUNG] Khong co khung vung in nao tren Model. Go T-CK truoc.") (princ))
    ((null ds)
     (princ "\n[DUNG] Ban ve chua co layout nao.") (princ))
    (T
     (princ (strcat "\n>> Co " (itoa (length ds)) " layout va "
                    (itoa (length lst)) " khung vung in."))
     (initget "Co Khong")
     (setq kw (getkword "\nCan lai het cac layout do? [Co/Khong] <Co>: "))
     (if (= kw "Khong")
       (progn (princ "\n>> Da huy.") (princ))
       (progn
         (setq ctab0 (getvar "CTAB"))
         (setq ce (getvar "CMDECHO"))
         (setvar "CMDECHO" 0)
         (setq dem 0  bo 0  dsLog nil)
         (foreach lname ds
           (setq r  (dt-to-cua-layout lname lst))
           (setq vp (dt-vp lname))
           (cond
             ((null r)
              (setq bo (1+ bo))
              (setq dsLog (cons (strcat "\n   [BO] " lname
                                        ", khong tim ra to ung voi ten nay.") dsLog)))
             ((null vp)
              (setq bo (1+ bo))
              (setq dsLog (cons (strcat "\n   [BO] " lname
                                        ", layout khong co viewport.") dsLog)))
             (T
              (vl-catch-all-apply '(lambda () (setvar "CTAB" lname)))
              (setq caoVP (vla-get-Height (vlax-ename->vla-object vp)))
              (dt-can-to vp r caoVP)
              (vl-catch-all-apply 'vla-put-DisplayLocked
                (list (vlax-ename->vla-object vp) :vlax-true))
              (dt-ghi-tyle lname (nth 4 r))
              (setq dem (1+ dem))
              (setq dsLog (cons (strcat "\n   + " lname "   to " (nth 3 r)
                                        "   ty le 1:" (itoa (nth 4 r))
                                        (if (< (abs (dt-chuan-pi (nth 5 r))) 1e-6) ""
                                          (strcat "   goc " (rtos (dt-do (nth 5 r)) 2 2))))
                                dsLog)))))
         (foreach x (reverse dsLog) (princ x))
         (vl-catch-all-apply '(lambda () (setvar "CTAB" ctab0)))
         (setvar "CMDECHO" ce)
         (princ (strcat "\n>> Da can lai " (itoa dem) " layout."))
         (if (> bo 0) (princ (strcat "\n>> Bo qua " (itoa bo) " layout.")))
         (princ))))))

(princ "\n[DAN_TRANG] Da nap.  T-CK = dat khung (go G de nghieng khung).  T-DTL = moi to mot layout.  T-DTM = nhieu to mot layout.  T-TKI = tao lai block.")
(princ "\n            Ban ve he met: chon dong co chu (m) trong hop thoai T-CK.")
(princ)

;;; ==========================================================================
;;;  PHAN B  -  T-STT  (TVXD, rieng; KHONG thuoc ban goc DAN_TRANG)
;;;  --------------------------------------------------------------------
;;;  Danh lai so to (thuoc tinh SO_TO) cho cac khung DA CHON: 4 huong, so bat
;;;  dau, buoc 1, kiem tra truoc roi tu ap dung. Giu nguyen ngu nghia 031.
;;;  Cac ham duoi day trich NGUYEN VAN tu ban tvxd30 truoc do, CHI gom cac ham
;;;  ma c:T-STT goi toi (bac cau, de quy het). Khong goi ham dt-*, khong dat
;;;  hay doc bien *DT-*. Rieng T-STT chay qua tvxd30-chay: mot nhom Undo,
;;;  in THANH CONG / DA HUY / THAT BAI. Sau lenh goc khong dung khung nay.
;;; ==========================================================================

(setq *TVXD30-TIENTO*   "KHUNG-IN-A3-")
(setq *TVXD30-NGAN*       " - ")
(setq *TVXD30-DAUM*   "M")
(setq *TVXD30-APP*    "TVXD30")       ; ten regapp danh dau lien ket viewport (chi doc o T-STT)
(setq *TVXD30-STT-FAIL-AT* nil)       ; chi dung cho QA: gia lap loi ghi thu n
(setq *TVXD30-STT-THROW-AT* nil)      ; chi dung cho QA: nem ngoai le SAU khi ghi xong muc n

;;; ======================= NHOM HAM DUNG CHUNG =============================

(defun tvxd30-pad (n w / s)
  (setq s (itoa n))
  (while (< (strlen s) w) (setq s (strcat "0" s)))
  s)

;;; --- Chuoi chi gom chu so, khong rong -----------------------------------
(defun tvxd30-chu-so-p (s / i ok)
  (setq ok (and (= (type s) 'STR) (> (strlen s) 0))  i 1)
  (while (and ok (<= i (strlen s)))
    (if (not (wcmatch (substr s i 1) "#")) (setq ok nil))
    (setq i (1+ i)))
  ok)

;;; --- Doc so nguyen duong NGHIEM NGAT: "05" -> 5, "2D" -> nil, "" -> nil --
(defun tvxd30-so-nguyen (s / v)
  (if (= (type s) 'STR) (setq v (vl-string-trim " " s)))
  (if (and v (tvxd30-chu-so-p v) (<= (strlen v) 9) (> (atoi v) 0))
    (atoi v)
    nil))

;;; --- Tach ten block ra cap ty le, NGHIEM NGAT: sau tien to chi duoc co
;;;     mot chu M tuy chon roi toan chu so. Khong dung thi tra nil.
(defun tvxd30-tach-ten (s / L p n m)
  (setq L (strlen *TVXD30-TIENTO*))
  (if (and (= (type s) 'STR) (> (strlen s) L)
           (= (strcase (substr s 1 L)) (strcase *TVXD30-TIENTO*)))
    (progn
      (setq p (substr s (1+ L)))
      (setq m (= (strcase (substr p 1 1)) (strcase *TVXD30-DAUM*)))
      (if m (setq p (substr p 2)))
      (setq n (tvxd30-so-nguyen p))
      (if n (cons n (if m T nil)) nil))
    nil))

(defun tvxd30-noi (lst sep / r)
  (setq r "")
  (foreach x lst (setq r (if (= r "") x (strcat r sep x))))
  r)

;;; ===================== THUOC TINH CUA BLOCK ===============================

;;; --- Danh sach ename ATTRIB cua mot INSERT -------------------------------
(defun tvxd30-att-ds (e / a ed lst)
  (setq lst nil)
  (if (= 1 (cdr (assoc 66 (entget e))))
    (progn
      (setq a (entnext e))
      (while (and a (setq ed (entget a)) (= "ATTRIB" (cdr (assoc 0 ed))))
        (setq lst (cons a lst))
        (setq a (entnext a)))))
  (reverse lst))

;;; --- Cac ATTRIB mang dung tag ---------------------------------------------
(defun tvxd30-att-e (e tag / lst)
  (setq lst nil)
  (foreach a (tvxd30-att-ds e)
    (if (= (strcase (cdr (assoc 2 (entget a)))) (strcase tag))
      (setq lst (cons a lst))))
  (reverse lst))

;;; --- Gia tri thuoc tinh dau tien mang tag, khong co thi "" ----------------
(defun tvxd30-att (e tag / l)
  (setq l (tvxd30-att-e e tag))
  (if l (cdr (assoc 1 (entget (car l)))) ""))

;;; --- Layer dang khoa? -----------------------------------------------------
(defun tvxd30-layer-khoa-p (nm / ld)
  (setq ld (tblsearch "LAYER" nm))
  (and ld (= 4 (logand 4 (cdr (assoc 70 ld))))))

;;; ======================= KHUNG THUC THI AN TOAN (CHI T-STT) ===============
;;; T-STT chay qua tvxd30-chay:
;;;   - mot nhom Undo (vla-StartUndoMark / EndUndoMark);
;;;   - moi bien he thong doi qua tvxd30-setvar duoc ghi lai va tra ve;
;;;   - ham chinh tra ve (list trangthai thongbao) voi trangthai la
;;;     'ok, 'mot-phan, 'huy hoac 'loi. Loi sau trong helper goi tvxd30-dung
;;;     de dung lenh; loi bi bat luon thanh THAT BAI, khong bao thanh cong;
;;;   - khong chay long nhau, khong mo duoc Undo thi tu choi truoc moi sua;
;;;   - chi huy lenh do chinh lenh T- de dang do (so CMDNAMES), khong huy
;;;     lenh/script ben ngoai; don dep loi thi bao that, khong bao thanh cong.
;;; Ket qua lan chay cuoi luu o *TVXD30-KQ-CUOI* = (lenh trangthai thongbao van-de).

(defun tvxd30-doc () (vla-get-ActiveDocument (vlax-get-acad-object)))

(defun tvxd30-ctx-get (k) (cdr (assoc k *TVXD30-CTX*)))

(defun tvxd30-ctx-put (k v)
  (if (assoc k *TVXD30-CTX*)
    (setq *TVXD30-CTX* (subst (cons k v) (assoc k *TVXD30-CTX*) *TVXD30-CTX*))
    (setq *TVXD30-CTX* (cons (cons k v) *TVXD30-CTX*)))
  v)

(defun tvxd30-ctx-push (k v) (tvxd30-ctx-put k (cons v (tvxd30-ctx-get k))))

;;; --- Doi bien he thong, nho gia tri goc lan dau --------------------------
(defun tvxd30-setvar (nm v)
  (if (and *TVXD30-CTX* (not (assoc (strcase nm) (tvxd30-ctx-get 'vars))))
    (tvxd30-ctx-push 'vars (cons (strcase nm) (getvar nm))))
  (setvar nm v))

;;; --- Dung lenh voi ly do ro rang (lan ra tvxd30-chay) ---------------------
(defun tvxd30-dung (ly-do)
  (tvxd30-ctx-put 'ly-do ly-do)
  (exit))

;;; --- Mo khung thuc thi. Tra T, hoac chuoi ly do khi tu choi (chua sua gi).
;;; Khong ghi de trang thai cua lan chay dang mo; khong mo duoc Undo thi dung.
(defun tvxd30-bat-dau (cmd giu / r)
  (cond
    (*TVXD30-CTX*
     (strcat "Lenh " (if (tvxd30-ctx-get 'cmd) (tvxd30-ctx-get 'cmd) "T-?")
             " dang chay (trang thai chua dong); khong chay long nhau. Chua thay doi gi."))
    ((vl-catch-all-error-p
       (setq r (vl-catch-all-apply '(lambda () (vla-StartUndoMark (tvxd30-doc))) nil)))
     (strcat "Khong mo duoc nhom Undo (" (vl-catch-all-error-message r) "); chua thay doi gi."))
    (T
     (tvxd30-ctx-put 'cmd cmd)
     (tvxd30-ctx-put 'undo T)
     (tvxd30-ctx-put 'cmdnames0 (getvar "CMDNAMES"))
     (tvxd30-ctx-put 'ctab (getvar "CTAB"))
     (tvxd30-ctx-put 'cvport (getvar "CVPORT"))
     (tvxd30-ctx-put 'tilemode (getvar "TILEMODE"))
     (tvxd30-ctx-put 'giu giu)
     (tvxd30-setvar "CMDECHO" 0)
     T)))

(defun tvxd30-ctab-thay (nm / ct)
  (setq ct (tvxd30-ctx-get 'ctab))
  (if (and ct (/= (strcase ct) (strcase nm)) (tvxd30-co-layout-hoac-model ct))
    ct
    "Model"))

(defun tvxd30-co-layout-hoac-model (nm)
  (or (= (strcase nm) "MODEL") (tvxd30-co-layout nm)))

;;; --- Xoa layout do lan chay nay tao (chi khi huy/loi). Tra ten con sot ----
(defun tvxd30-xoa-layout-so-huu ( / lays con)
  (setq lays (vla-get-Layouts (tvxd30-doc))  con nil)
  (foreach nm (tvxd30-ctx-get 'layouts)
    (if (tvxd30-co-layout nm)
      (progn
        (if (= (strcase nm) (strcase (getvar "CTAB")))
          (vl-catch-all-apply 'setvar (list "CTAB" (tvxd30-ctab-thay nm))))
        (vl-catch-all-apply 'vla-Delete (list (vla-Item lays nm)))
        (if (tvxd30-co-layout nm) (setq con (cons nm con))))))
  (reverse con))

;;; --- Xoa block do lan chay nay tao, chi khi chua co ai dung. Tra ten con --
(defun tvxd30-xoa-block-so-huu ( / blks con)
  (setq blks (vla-get-Blocks (tvxd30-doc))  con nil)
  (foreach nm (tvxd30-ctx-get 'blocks)
    (if (and (tblsearch "BLOCK" nm) (not (tvxd30-block-co-ref nm)))
      (vl-catch-all-apply 'vla-Delete (list (vla-Item blks nm))))
    (if (tblsearch "BLOCK" nm) (setq con (cons nm con))))
  (reverse con))

;;; --- Lenh MOI chong len c0 (CMDNAMES doc truoc khi goi command)? ---------
(defun tvxd30-lenh-moi-p (c0 / c)
  (setq c (getvar "CMDNAMES"))
  (and (= (type c0) 'STR) (> (strlen c) (strlen c0))
       (= (strcase c0) (strcase (substr c 1 (strlen c0))))))

;;; --- Huy lenh do chinh doan code nay de dang do; khong cham lenh/script
;;;     ben ngoai (chi huy phan chong len c0). Tra T neu khong con lenh do.
(defun tvxd30-huy-lenh (c0 / n)
  (setq n 0)
  (while (and (< n 3) (tvxd30-lenh-moi-p c0))
    (vl-catch-all-apply '(lambda () (command)) nil)
    (setq n (1+ n)))
  (not (tvxd30-lenh-moi-p c0)))

;;; --- Don dep. Moi buoc tu bat loi; tra danh sach van de (nil = sach) -----
(defun tvxd30-ket-thuc (that-bai trong-error / ct cv e o r vd)
  (setq vd nil)
  ;; 1. lenh do lan chay nay de dang do thi huy; trong *error* khong goi
  ;;    command duoc nen chi bao lai
  (if trong-error
    (if (tvxd30-lenh-moi-p (tvxd30-ctx-get 'cmdnames0))
      (setq vd (cons (strcat "lenh con dang do: " (getvar "CMDNAMES")) vd)))
    (if (not (tvxd30-huy-lenh (tvxd30-ctx-get 'cmdnames0)))
      (setq vd (cons (strcat "khong huy duoc lenh dang do: " (getvar "CMDNAMES")) vd))))
  ;; 2. ve khong gian giay truoc khi doi layout
  (if (and (= 0 (getvar "TILEMODE")) (/= 1 (getvar "CVPORT")))
    (progn
      (vl-catch-all-apply
        (if trong-error '(lambda () (command-s "_.PSPACE")) '(lambda () (command "_.PSPACE")))
        nil)
      (if (/= 1 (getvar "CVPORT")) (setq vd (cons "khong ve duoc khong gian giay" vd)))))
  ;; 3. hoan lai doi tuong do lan chay nay tao
  (if that-bai
    (progn
      (setq r (vl-catch-all-apply 'tvxd30-xoa-layout-so-huu nil))
      (cond ((vl-catch-all-error-p r)
             (setq vd (cons (strcat "loi khi xoa layout tam: " (vl-catch-all-error-message r)) vd)))
            (r (setq vd (cons (strcat "khong xoa duoc layout tam: " (tvxd30-noi r ", ")) vd))))
      (setq r (vl-catch-all-apply 'tvxd30-xoa-block-so-huu nil))
      (cond ((vl-catch-all-error-p r)
             (setq vd (cons (strcat "loi khi xoa block tam: " (vl-catch-all-error-message r)) vd)))
            (r (setq vd (cons (strcat "block do lan chay nay tao con lai: " (tvxd30-noi r ", ")) vd))))))
  ;; 4. tra khoa viewport (thanh cong thi giu khoa ma lenh co y dat)
  (foreach p (tvxd30-ctx-get 'khoa)
    (if (or that-bai (not (member (car p) (tvxd30-ctx-get 'da-khoa))))
      (progn
        (setq e (handent (car p)))
        (if (and e (entget e))
          (progn
            (setq o (vlax-ename->vla-object e))
            (vl-catch-all-apply 'vla-put-DisplayLocked (list o (cdr p)))
            (setq r (vl-catch-all-apply 'vla-get-DisplayLocked (list o)))
            (if (or (vl-catch-all-error-p r) (not (eq r (cdr p))))
              (setq vd (cons (strcat "khong tra duoc khoa viewport " (car p)) vd))))))))
  ;; 5. tra layout dang mo
  (setq ct (tvxd30-ctx-get 'ctab))
  (if (and ct (/= (strcase ct) (strcase (getvar "CTAB"))))
    (progn
      (vl-catch-all-apply 'setvar (list "CTAB" ct))
      (if (/= (strcase ct) (strcase (getvar "CTAB")))
        (setq vd (cons (strcat "khong tra duoc layout \"" ct "\"") vd)))))
  ;; 6. truoc do dang o trong mot viewport cua layout nay thi vao lai
  (setq cv (tvxd30-ctx-get 'cvport))
  (if (and ct cv (> cv 1) (= 0 (tvxd30-ctx-get 'tilemode))
           (= 0 (getvar "TILEMODE")) (= 1 (getvar "CVPORT"))
           (= (strcase ct) (strcase (getvar "CTAB"))))
    (progn
      (vl-catch-all-apply
        (if trong-error '(lambda () (command-s "_.MSPACE")) '(lambda () (command "_.MSPACE")))
        nil)
      (vl-catch-all-apply 'setvar (list "CVPORT" cv))))
  ;; 7. bien he thong
  (foreach p (tvxd30-ctx-get 'vars)
    (vl-catch-all-apply 'setvar (list (car p) (cdr p)))
    (if (not (equal (getvar (car p)) (cdr p) 1e-9))
      (setq vd (cons (strcat "khong tra duoc bien " (car p)) vd))))
  ;; 8. dong nhom Undo
  (if (tvxd30-ctx-get 'undo)
    (if (vl-catch-all-error-p (vl-catch-all-apply 'vla-EndUndoMark (list (tvxd30-doc))))
      (setq vd (cons "khong dong duoc nhom Undo" vd))))
  (setq *TVXD30-CTX* nil)
  (reverse vd))

(defun tvxd30-loi-thanh-kq (m)
  (cond
    ((tvxd30-ctx-get 'ly-do) (list 'loi (tvxd30-ctx-get 'ly-do)))
    ((and (= (type m) 'STR) (wcmatch (strcase m) "*CANCEL*,*BREAK*,*QUIT*,*EXIT*"))
     (if (tvxd30-ctx-get 'giu)
       (list 'ok "Ket thuc bang Esc; cac khung da dat duoc giu.")
       (list 'huy "Nguoi dung huy (Esc).")))
    (T (list 'loi (if (= (type m) 'STR) m "Loi khong ro.")))))

;;; Don dep loi thi khong bao thanh cong: 'ok ha xuong 'mot-phan, va khong
;;; noi "da khoi phuc" khi con van de. *TVXD30-KQ-CUOI* = (lenh st msg van-de).
(defun tvxd30-bao-ket-qua (cmd kq trong-error / st msg vd)
  (if (or (not (listp kq)) (not (member (car kq) '(ok mot-phan huy loi))))
    (setq kq (list 'loi "Ham chinh khong tra ve trang thai hop le.")))
  (setq st (car kq)  msg (if (cadr kq) (cadr kq) ""))
  (setq vd (vl-catch-all-apply 'tvxd30-ket-thuc
             (list (if (member st '(huy loi)) T nil) trong-error)))
  (if (vl-catch-all-error-p vd)
    (progn
      (setq vd (list (strcat "don dep dung giua chung: " (vl-catch-all-error-message vd))))
      (if (tvxd30-ctx-get 'undo)
        (vl-catch-all-apply 'vla-EndUndoMark (list (tvxd30-doc))))
      (setq *TVXD30-CTX* nil)))
  (if (and vd (eq st 'ok)) (setq st 'mot-phan))
  (setq *TVXD30-KQ-CUOI* (list cmd st msg vd))
  (princ
    (strcat "\n[" cmd "] "
            (cond ((eq st 'ok) "THANH CONG. ")
                  ((eq st 'mot-phan) "HOAN THANH MOT PHAN, CO LOI. ")
                  ((eq st 'huy) "DA HUY. ")
                  (T "THAT BAI. "))
            (cond
              ((not (member st '(huy loi))) "")
              (vd "Don dep CHUA DAY DU (xem canh bao); kiem tra ban ve, go U de hoan tac nhom lenh neu can. ")
              (T "Da xoa layout/block do lan chay nay tao, tra bien he thong va khoa viewport; thay doi khac (neu co) go U de hoan tac nhom lenh. "))
            msg))
  (tvxd30-in-ds "[CANH BAO] Van de khi don dep:" vd)
  st)

(defun tvxd30-chay (cmd fn giu / *error* kq bd)
  (setq bd (tvxd30-bat-dau cmd giu))
  (if (= (type bd) 'STR)
    (progn
      ;; tu choi truoc moi thay doi; khong dung den trang thai lan chay khac
      (setq *TVXD30-KQ-CUOI* (list cmd 'loi bd nil))
      (princ (strcat "\n[" cmd "] THAT BAI. " bd))
      'loi)
    (progn
      (defun *error* (m)
        (tvxd30-bao-ket-qua cmd (tvxd30-loi-thanh-kq m) T)
        (princ))
      (setq kq (vl-catch-all-apply fn nil))
      (if (vl-catch-all-error-p kq)
        (setq kq (tvxd30-loi-thanh-kq (vl-catch-all-error-message kq))))
      (tvxd30-bao-ket-qua cmd kq nil))))

;;; --- Block co dang duoc chen o dau khong (moi layout va trong block khac) -
(defun tvxd30-block-co-ref (nm / bd e ed co)
  (setq co (if (ssget "_X" (list '(0 . "INSERT") (cons 2 nm))) T nil))
  (setq bd (tblnext "BLOCK" T))
  (while (and bd (not co))
    (if (= 0 (logand 4 (cdr (assoc 70 bd))))
      (progn
        (setq e (cdr (assoc -2 bd)))
        (while (and e (not co))
          (setq ed (entget e))
          (if (and ed (= "INSERT" (cdr (assoc 0 ed)))
                   (= (strcase (cdr (assoc 2 ed))) (strcase nm)))
            (setq co T))
          (setq e (entnext e)))))
    (setq bd (tblnext "BLOCK")))
  co)

;;; --- Ten block la khung vung in hop le (dung tien to, khong phai xref) ---
(defun tvxd30-la-khung-ten-p (bn / bd)
  (setq bd (tblsearch "BLOCK" bn))
  (and bd (tvxd30-tach-ten bn) (= 0 (logand 4 (cdr (assoc 70 bd))))))

;;; --- Moi INSERT khung tren Model (chi doc) -------------------------------
(defun tvxd30-ds-khung-e ( / ss i e lst)
  (setq lst nil)
  (if (setq ss (ssget "_X" (list '(0 . "INSERT") '(410 . "Model")
                                 (cons 2 (strcat *TVXD30-TIENTO* "*")))))
    (progn
      (setq i 0)
      (while (< i (sslength ss))
        (setq e (ssname ss i))
        (if (tvxd30-la-khung-ten-p (cdr (assoc 2 (entget e)))) (setq lst (cons e lst)))
        (setq i (1+ i)))))
  (reverse lst))

;;; --- In danh sach loi/canh bao, toi da 20 dong -----------------------------
(defun tvxd30-in-ds (tieude ds / i)
  (if ds
    (progn
      (princ (strcat "\n" tieude))
      (setq i 0)
      (foreach x ds
        (if (< i 20) (princ (strcat "\n   - " x)))
        (setq i (1+ i)))
      (if (> i 20) (princ (strcat "\n   ... va " (itoa (- i 20)) " dong khac."))))))

;;; --- So to doc NGHIEM NGAT tu ten layout "<so> - <ten>" hoac "<so> ----
;;; "2D PLAN" -> nil, "02 - MB" -> 2, "12" -> 12.
(defun tvxd30-so-cua-ten-layout (lname / v)
  (setq v (vl-string-search *TVXD30-NGAN* lname))
  (tvxd30-so-nguyen (if v (substr lname 1 v) lname)))

(defun tvxd30-co-layout (nm / kq)
  (setq kq nil)
  (vlax-for l (vla-get-Layouts (tvxd30-doc))
    (if (= (strcase (vla-get-Name l)) (strcase nm)) (setq kq T)))
  kq)

(defun tvxd30-ds-layout ( / lst)
  (setq lst nil)
  (vlax-for l (vla-get-Layouts (tvxd30-doc))
    (if (/= (strcase (vla-get-Name l)) "MODEL")
      (setq lst (cons (cons (vla-get-TabOrder l) (vla-get-Name l)) lst))))
  (mapcar 'cdr (vl-sort lst '(lambda (a b) (< (car a) (car b))))))

(defun tvxd30-doc-lien-ket (vp / x)
  (setq x (cdr (assoc -3 (entget vp (list *TVXD30-APP*)))))
  (setq x (cdr (assoc *TVXD30-APP* x)))
  (if (and x (= "SHEET" (cdr (car x))))
    (mapcar '(lambda (p) (if (= (cdr p) "-") nil (cdr p))) (cdr x))
    nil))

(defun tvxd30-handle (e) (if e (cdr (assoc 5 (entget e))) nil))

(defun tvxd30-xref-p (bn / bd)
  (setq bd (tblsearch "BLOCK" bn))
  (if bd (= 4 (logand (cdr (assoc 70 bd)) 4)) nil))

;;; ===================================================================
;;;  T-STT  -  DANH LAI SO TO CHO CAC KHUNG DA CHON (NUMBERING_SPEC.md)
;;;  Chi doi thuoc tinh SO_TO cua khung DA CHON. Khong doi TEN_TO, TY_LE,
;;;  vi tri, handle, khung khong chon, ten layout, viewport hay chu.
;;;  031: kiem tra truoc roi tu ap dung (khong hoi Apdung); trung so voi khung
;;;  khong chon thi dung, khong ghi.
;;;  Ghi loi giua chung thi khoi phuc toan bo so cu.
;;; ===================================================================

;;; ---------------- HAM THUAN (khong cham ban ve), QA goi truc tiep --------

;;; --- Sap thu tu. items = list (handle x y) theo WCS.
;;;     che-do 'hang: hang tren xuong duoi, trong hang trai sang phai.
;;;     che-do 'cot : cot trai sang phai, trong cot tren xuong duoi.
;;;     che-do 'bam : giu nguyen thu tu dua vao (thu tu bam).
;;;     tol: dung sai gom hang/cot (don vi ban ve), so voi phan tu DAU nhom.
;;;     031: che-do 'trai-phai 'phai-trai 'tren-duoi 'duoi-tren: so sanh CHAT
;;;     (khong dung sai, khong gom nhom), tol bo qua; xem tvxd30-stt-truoc-p.
;;; Tra danh sach handle.
(defun tvxd30-stt-xep (items che-do tol / sorted ds nhom ref v out)
  (cond
    ((eq che-do 'bam) (mapcar 'car items))
    ((member che-do '(trai-phai phai-trai tren-duoi duoi-tren))
     (mapcar 'car (tvxd30-stt-sap items che-do)))
    (T
      (setq sorted
        (vl-sort items
          (if (eq che-do 'hang)
            '(lambda (a b)
               (cond ((/= (caddr a) (caddr b)) (> (caddr a) (caddr b)))
                     ((/= (cadr a) (cadr b)) (< (cadr a) (cadr b)))
                     (T (< (car a) (car b)))))
            '(lambda (a b)
               (cond ((/= (cadr a) (cadr b)) (< (cadr a) (cadr b)))
                     ((/= (caddr a) (caddr b)) (> (caddr a) (caddr b)))
                     (T (< (car a) (car b))))))))
      (setq ds nil  nhom nil  ref nil)
      (foreach it sorted
        (setq v (if (eq che-do 'hang) (caddr it) (cadr it)))
        (if (and ref (<= (abs (- v ref)) tol))
          (setq nhom (cons it nhom))
          (progn
            (if nhom (setq ds (cons (reverse nhom) ds)))
            (setq nhom (list it)  ref v))))
      (if nhom (setq ds (cons (reverse nhom) ds)))
      (setq out nil)
      (foreach g (reverse ds)
        (setq out
          (append out
            (mapcar 'car
              (vl-sort g
                (if (eq che-do 'hang)
                  '(lambda (a b) (if (/= (cadr a) (cadr b)) (< (cadr a) (cadr b)) (< (car a) (car b))))
                  '(lambda (a b) (if (/= (caddr a) (caddr b)) (> (caddr a) (caddr b)) (< (car a) (car b))))))))))
      out)))

;;; --- 031: handle a dung truoc handle b (hex in hoa: ngan hon truoc, roi chuoi)
(defun tvxd30-stt-handle< (a b)
  (if (/= (strlen a) (strlen b)) (< (strlen a) (strlen b)) (< a b)))

;;; --- 031: item a (handle x y) dung TRUOC item b theo huong (WCS, so sanh chat).
;;;     TraiPhai X tang, PhaiTrai X giam (hoa: Y giam roi handle);
;;;     TrenDuoi Y giam, DuoiTren Y tang (hoa: X tang roi handle).
(defun tvxd30-stt-truoc-p (a b che-do / xa ya xb yb)
  (setq xa (cadr a)  ya (caddr a)  xb (cadr b)  yb (caddr b))
  (cond
    ((and (eq che-do 'trai-phai) (/= xa xb)) (< xa xb))
    ((and (eq che-do 'phai-trai) (/= xa xb)) (> xa xb))
    ((and (eq che-do 'tren-duoi) (/= ya yb)) (> ya yb))
    ((and (eq che-do 'duoi-tren) (/= ya yb)) (< ya yb))
    ((and (member che-do '(trai-phai phai-trai)) (/= ya yb)) (> ya yb))
    ((and (member che-do '(tren-duoi duoi-tren)) (/= xa xb)) (< xa xb))
    (T (tvxd30-stt-handle< (car a) (car b)))))

;;; --- 031: sap xep chen on dinh (khong dung vl-sort: khong bo phan tu trung).
(defun tvxd30-stt-sap (items che-do / out head ds it)
  (setq out nil)
  (foreach it items
    (setq head nil  ds out)
    (while (and ds (not (tvxd30-stt-truoc-p it (car ds) che-do)))
      (setq head (cons (car ds) head)  ds (cdr ds)))
    (setq out (append (reverse head) (cons it ds))))
  out)

;;; --- So moi: dem tu start, buoc step, it nhat w chu so (khong cat bot) ---
(defun tvxd30-stt-dinh-dang (n w) (tvxd30-pad n w))

;;; --- Ke hoach: list (handle so-moi). dao = T thi dao thu tu da chon ------
(defun tvxd30-stt-ke-hoach (handles start step w dao / i out)
  (setq i 0  out nil)
  (foreach h (if dao (reverse handles) handles)
    (setq out (cons (list h (tvxd30-stt-dinh-dang (+ start (* i step)) w)) out)
          i   (1+ i)))
  (reverse out))

;;; --- Va cham: so moi trung (theo gia tri so) voi khung KHONG chon -------
;;; khac = list (handle so-chuoi). Tra list (so-moi handle-khac).
(defun tvxd30-stt-va-cham (plan khac / out k)
  (setq out nil)
  (foreach p plan
    (setq k (tvxd30-so-nguyen (cadr p)))
    (foreach o khac
      (if (and k (equal k (tvxd30-so-nguyen (cadr o))))
        (setq out (cons (list (cadr p) (car o)) out)))))
  (reverse out))

;;; --- Phan tu trung lap trong danh sach chuoi ------------------------------
(defun tvxd30-trung (ds / seen dup)
  (setq seen nil  dup nil)
  (foreach x ds
    (if (member x seen)
      (if (not (member x dup)) (setq dup (cons x dup)))
      (setq seen (cons x seen))))
  (reverse dup))

;;; ---------------- KIEM TRA VA GHI (cham ban ve) ---------------------------

(defun tvxd30-410 (ed) (if (assoc 410 ed) (cdr (assoc 410 ed)) "Model"))

;;; --- Kiem mot khung cho T-STT: (ename-ATTRIB-SO_TO nil) hoac (nil ly-do) --
(defun tvxd30-stt-kiem-1 (e / ed bn at)
  (setq ed (entget e)  bn (cdr (assoc 2 ed)))
  (cond
    ((/= "INSERT" (cdr (assoc 0 ed))) (list nil "khong phai block"))
    ((/= "MODEL" (strcase (tvxd30-410 ed))) (list nil "khong nam tren Model"))
    ((tvxd30-xref-p bn) (list nil "la xref"))
    ((not (tvxd30-la-khung-ten-p bn)) (list nil (strcat "block " bn " khong phai khung vung in")))
    ((tvxd30-layer-khoa-p (cdr (assoc 8 ed))) (list nil "nam tren layer dang khoa"))
    ((/= 1 (length (setq at (tvxd30-att-e e "SO_TO"))))
     (list nil (if at "co nhieu thuoc tinh SO_TO" "thieu thuoc tinh SO_TO")))
    ((tvxd30-layer-khoa-p (cdr (assoc 8 (entget (car at)))))
     (list nil "thuoc tinh SO_TO nam tren layer khoa"))
    (T (list (car at) nil))))

;;; --- Khung dang lien ket voi layout/viewport (de canh bao) ---------------
(defun tvxd30-stt-lien-ket (hds sos / out ss i lk lname k)
  (setq out nil)
  (if (setq ss (ssget "_X" (list '(0 . "VIEWPORT") (list -3 (list *TVXD30-APP*)))))
    (progn
      (setq i 0)
      (while (< i (sslength ss))
        (setq lk (tvxd30-doc-lien-ket (ssname ss i)))
        (if (and lk (member (car lk) hds))
          (setq out (cons (strcat "khung " (car lk) " dang lien ket viewport tren layout \""
                                  (tvxd30-410 (entget (ssname ss i))) "\"") out)))
        (setq i (1+ i)))))
  (foreach lname (tvxd30-ds-layout)
    (setq k (tvxd30-so-cua-ten-layout lname))
    (if (and k (member k sos))
      (setq out (cons (strcat "layout \"" lname "\" dang doi chieu so to " (itoa k)) out))))
  (reverse out))

;;; --- Preflight CHI DOC. enames theo thu tu chon/bam.
;;; Tra (list plan ds-loi ds-canhbao); plan = list (ename-khung ename-att so-cu so-moi handle).
(defun tvxd30-stt-preflight (enames start step w dao che-do tol
                             / loi cb hds kq items bang order plan0 plan khac sel va sos x)
  (setq loi nil  cb nil  bang nil  items nil)
  (setq hds (mapcar 'tvxd30-handle enames))
  (foreach h (tvxd30-trung hds)
    (setq loi (cons (strcat "khung " h " duoc chon hai lan") loi)))
  (if (or (not (numberp start)) (< start 1) (not (numberp step)) (< step 1)
          (not (numberp w)) (< w 1))
    (setq loi (cons "so bat dau, buoc va so chu so phai la so nguyen >= 1" loi)))
  (foreach e enames
    (setq kq (tvxd30-stt-kiem-1 e))
    (if (car kq)
      (progn
        (setq bang (cons (list (tvxd30-handle e) e (car kq)
                               (cdr (assoc 1 (entget (car kq))))) bang))
        (setq x (trans (cdr (assoc 10 (entget e))) e 0))
        (setq items (cons (list (tvxd30-handle e) (car x) (cadr x)) items)))
      (setq loi (cons (strcat "khung " (tvxd30-handle e) ": " (cadr kq)) loi))))
  (setq items (reverse items))
  (if (null enames) (setq loi (cons "chua chon khung nao" loi)))
  (if loi
    (list nil (reverse loi) nil)
    (progn
      (setq order (tvxd30-stt-xep items che-do tol))
      (setq plan0 (tvxd30-stt-ke-hoach order start step w dao))
      (setq plan nil)
      (foreach p plan0
        (setq x (assoc (car p) bang))
        (setq plan (cons (list (nth 1 x) (nth 2 x) (nth 3 x) (cadr p) (car p)) plan)))
      (setq plan (reverse plan))
      ;; khung khong chon tren Model
      (setq khac nil)
      (foreach e (tvxd30-ds-khung-e)
        (if (not (member (tvxd30-handle e) hds))
          (setq khac (cons (list (tvxd30-handle e) (tvxd30-att e "SO_TO")) khac))))
      (foreach v (tvxd30-stt-va-cham plan0 khac)
        (setq loi (cons (strcat "so moi " (car v) " trung khung KHONG chon " (cadr v)) loi)))
      (foreach p plan
        (if (and (/= (vl-string-trim " " (nth 2 p)) "") (null (tvxd30-so-nguyen (nth 2 p))))
          (setq cb (cons (strcat "khung " (nth 4 p) ": so cu \"" (nth 2 p) "\" khong hop le") cb))))
      (setq sos nil)
      (foreach p plan
        (if (setq x (tvxd30-so-nguyen (nth 2 p))) (setq sos (cons x sos))))
      (setq cb (append (reverse cb) (tvxd30-stt-lien-ket hds sos)))
      (list (if loi nil plan) (reverse loi) cb))))

;;; --- Ghi mot gia tri vao ATTRIB a; tra T khi doc lai dung. Co the nem loi.
(defun tvxd30-stt-ghi-1 (a s / ed)
  (setq ed (entget a))
  (and ed (assoc 1 ed)
       (entmod (subst (cons 1 s) (assoc 1 ed) ed))
       (= s (cdr (assoc 1 (entget a))))))

;;; --- Kiem lai plan ngay truoc lan ghi dau: khung con, SO_TO van la ATTRIB
;;;     duy nhat cu, so cu chua doi. Tra nil (hop le) hoac ly do.
(defun tvxd30-stt-kiem-lai (plan / lydo kq ed)
  (setq lydo nil)
  (foreach p plan
    (if (null lydo)
      (cond
        ((null (entget (nth 0 p)))
         (setq lydo (strcat "khung " (nth 4 p) " khong con")))
        ((not (equal (car (setq kq (tvxd30-stt-kiem-1 (nth 0 p)))) (nth 1 p)))
         (setq lydo (strcat "khung " (nth 4 p) ": " (if (cadr kq) (cadr kq) "SO_TO da doi"))))
        ((or (null (setq ed (entget (nth 1 p)))) (/= (nth 2 p) (cdr (assoc 1 ed))))
         (setq lydo (strcat "khung " (nth 4 p) ": so cu da bi sua sau xem truoc"))))))
  lydo)

;;; --- Ghi plan. Loi tra ve HAY ngoai le o bat ky muc nao -> khoi phuc moi
;;;     so cu tu anh chup truoc khi ghi.
;;; QA: *TVXD30-STT-FAIL-AT* = n gia lap loi tra ve o muc n;
;;;     *TVXD30-STT-THROW-AT* = n nem ngoai le ngay SAU khi ghi xong muc n.
;;;     Ca hai duoc dat lai nil truoc moi thao tac, ke ca khi loi.
;;; Tra (T so-muc) hoac (nil ly-do khoi-phuc-du ds-handle-khoi-phuc-hong).
;;; 037: moi muc ghi CA so va kieu chu Arial (ke ca khi so khong doi); anh chup
;;;      giu so cu VA kieu chu cu, khoi phuc ca hai.
(defun tvxd30-stt-commit (plan / goc i ok lydo failAt throwAt r du hong ed sso)
  (setq goc nil  i 0  ok T  lydo nil  hong nil
        failAt  *TVXD30-STT-FAIL-AT*
        throwAt *TVXD30-STT-THROW-AT*
        *TVXD30-STT-FAIL-AT*  nil
        *TVXD30-STT-THROW-AT* nil)
  (setq r (vl-catch-all-apply 'tvxd30-stt-kiem-lai (list plan)))
  (if (not (or (vl-catch-all-error-p r) r))
    (setq sso (vl-catch-all-apply 'tvxd37-sty-so nil)))
  (cond
    ((vl-catch-all-error-p r)
     (list nil (strcat "kiem lai ke hoach loi: " (vl-catch-all-error-message r) "; chua ghi gi") T nil))
    (r (list nil (strcat "ke hoach khong con hop le (" r "); chua ghi gi") T nil))
    ((or (vl-catch-all-error-p sso) (null sso))
     (list nil (strcat "khong dam bao duoc kieu chu Arial cho so to"
                       (if (vl-catch-all-error-p sso) (strcat " (" (vl-catch-all-error-message sso) ")") "")
                       "; chua ghi so nao") T nil))
    (T
     ;; anh chup moi so cu VA kieu chu cu truoc lan ghi dau
     (foreach p plan
       (setq ed (entget (nth 1 p)))
       (setq goc (cons (list (nth 1 p) (nth 2 p) (nth 0 p) (nth 4 p)
                             (if (assoc 7 ed) (cdr (assoc 7 ed)) "Standard")) goc)))
     (setq goc (reverse goc))
     (setq r (vl-catch-all-apply
               '(lambda ()
                  (foreach p plan
                    (if ok
                      (progn
                        (setq i (1+ i))
                        (cond
                          ((and failAt (= i failAt))
                           (setq ok nil  lydo (strcat "gia lap loi ghi o muc " (itoa i))))
                          ((not (tvxd37-ghi-so-kieu (nth 1 p) (nth 3 p) sso))
                           (setq ok nil  lydo (strcat "ghi/doc lai sai o khung " (nth 4 p))))
                          (T
                           (entupd (nth 0 p))
                           (if (and throwAt (= i throwAt)) (exit))))))))
               nil))
     (if (vl-catch-all-error-p r)
       (setq ok nil
             lydo (strcat "ngoai le o muc " (itoa i) ": " (vl-catch-all-error-message r))))
     (if ok
       (list T (length plan))
       (progn
         ;; khoi phuc tung muc, moi muc tu bat loi; ghi lai chinh xac muc hong
         (foreach g goc
           (setq r (vl-catch-all-apply
                     '(lambda ()
                        (setq ed (entget (car g)))
                        (if (and ed (not (tvxd37-so-kieu-dung-p ed (cadr g) (nth 4 g))))
                          (progn
                            (tvxd37-ghi-so-kieu (car g) (cadr g) (nth 4 g))
                            (entupd (nth 2 g))))
                        (setq ed (entget (car g)))
                        (and ed (tvxd37-so-kieu-dung-p ed (cadr g) (nth 4 g))))
                     nil))
           (if (or (vl-catch-all-error-p r) (not r))
             (setq hong (cons (nth 3 g) hong))))
         (setq du (null hong))
         (list nil lydo du (reverse hong)))))))

(defun tvxd30-ss->ds (ss / i lst)
  (setq lst nil  i 0)
  (if ss
    (while (< i (sslength ss))
      (setq lst (cons (ssname ss i) lst)  i (1+ i))))
  (reverse lst))

;;; 031: chon vung ngay -> MOT cau hoi huong (4 tu khoa) -> so bat dau -> tu ap dung.
;;; Buoc 1, khong them so 0 (1,2,...), khong dung sai/dao/Apdung. Esc truoc khi
;;; nhap xong so bat dau: ban ve khong doi. Kiem tra truoc va ghi/khoi phuc dung
;;; tvxd30-stt-preflight / tvxd30-stt-commit nhu 030.
(defun tvxd30-stt-chinh ( / kw enames che-do start kq plan)
  (if (/= 1 (getvar "TILEMODE"))
    (tvxd30-dung "Hay chuyen sang tab Model roi go T-STT."))
  (princ "\nChon cac khung vung in can danh lai so: ")
  (setq enames (tvxd30-ss->ds (ssget '((0 . "INSERT")))))
  (if (null enames)
    (list 'huy "Khong chon khung nao.")
    (progn
      (initget "TraiPhai PhaiTrai TrenDuoi DuoiTren")
      (setq kw (getkword "\nHuong danh so [TraiPhai (trai->phai)/PhaiTrai (phai->trai)/TrenDuoi (tren->duoi)/DuoiTren (duoi->tren)] <TraiPhai>: "))
      (setq che-do (cond ((= kw "PhaiTrai") 'phai-trai) ((= kw "TrenDuoi") 'tren-duoi)
                         ((= kw "DuoiTren") 'duoi-tren) (T 'trai-phai)))
      (initget 6) (setq start (getint "\nSo bat dau <1>: ")) (if (null start) (setq start 1))
      (setq kq (tvxd30-stt-preflight enames start 1 1 nil che-do 0.0))
      (cond
        ((nth 1 kq)
         (tvxd30-in-ds "[LOI] Khong ghi gi:" (nth 1 kq))
         (list 'loi "Kiem tra truoc that bai; ban ve khong doi."))
        (T
         (setq plan (car kq))
         (tvxd30-in-ds "[CANH BAO] Lenh nay KHONG doi ten layout/viewport/chu; can doi chieu lai sau:" (nth 2 kq))
         (setq kq (tvxd30-stt-commit plan))
         (if (car kq)
           (list 'ok (strcat "Da danh lai so " (itoa (cadr kq)) " khung: " (itoa start) "-"
                             (itoa (+ start (cadr kq) -1)) ". Go U de hoan tac."))
           (list 'loi (strcat "Ghi loi: " (cadr kq) "; "
                              (if (nth 2 kq) "da khoi phuc toan bo so cu."
                                  (strcat "KHOI PHUC KHONG DAY DU o khung "
                                          (tvxd30-noi (nth 3 kq) ", ")
                                          "; go U de hoan tac."))))))))))

(defun c:T-STT () (tvxd30-chay "T-STT" 'tvxd30-stt-chinh nil) (princ))

;;; ==========================================================================
;;;  PHAN C (037)  -  KIEU CHU ARIAL CHO SO TO (thuoc tinh SO_TO)
;;;  --------------------------------------------------------------------
;;;  Kieu chu rieng TVXD_SOTO_ARIAL (ten da bi kieu chu KHAC dung thi thu
;;;  _2 .. _9): font Arial thuong (khong dam, khong nghieng), cao 0, he so
;;;  rong 1, goc nghieng 0, danh dau bang xdata TVXD_MEPS_SOTO037. Chi dung
;;;  lai kieu chu co dau cua ta VA con dung cac gia tri tren; khong bao gio
;;;  sua kieu chu co san (ke ca Standard), khong doi TEXTSTYLE, khong tu
;;;  chuyen doi khung cu khi nap file. Chi tao khi T-TKI / T-CK / T-STT can.
;;; ==========================================================================
(setq *TVXD37-STY-TEN* "TVXD_SOTO_ARIAL"
      *TVXD37-APP*     "TVXD_MEPS_SOTO037"
      *TVXD37-DAU*     '((1000 . "SOTO_STYLE") (1000 . "1")))

;;; --- Font cua mot TextStyle (VLA): (typeface bold italic) hoac nil ----------
(defun tvxd37-font (so / tf b it cs pf r)
  (setq r (vl-catch-all-apply 'vla-GetFont (list so 'tf 'b 'it 'cs 'pf)))
  (if (or (vl-catch-all-error-p r) (/= 'STR (type tf))) nil (list tf b it)))

;;; --- Kieu chu nm la cua ta va dung Arial thuong, cao 0, rong 1, nghieng 0? --
(defun tvxd37-kieu-cua-ta-p (nm / e ed x f)
  (and (setq e (tblobjname "STYLE" nm))
       (setq ed (entget e (list *TVXD37-APP*)))
       (setq x (cdr (assoc *TVXD37-APP* (cdr (assoc -3 ed)))))
       (equal x *TVXD37-DAU*)
       (equal 0.0 (cdr (assoc 40 ed)) 1e-9)
       (equal 1.0 (cdr (assoc 41 ed)) 1e-9)
       (equal 0.0 (cdr (assoc 50 ed)) 1e-9)
       (= 0 (logand 5 (cdr (assoc 70 ed))))      ; khong phai shape, khong chu dung
       (setq f (tvxd37-font (vlax-ename->vla-object e)))
       (= "ARIAL" (strcase (car f)))
       (eq :vlax-false (cadr f))
       (eq :vlax-false (caddr f))))

;;; --- Tao kieu chu moi ten nm (chi goi khi ten con trong). Doc lai that bai
;;;     thi xoa kieu vua tao (chua doi tuong nao dung) va tra nil.
(defun tvxd37-tao-kieu (nm / doc so r e)
  (setq doc (vla-get-ActiveDocument (vlax-get-acad-object)))
  (setq r (vl-catch-all-apply
            '(lambda ()
               (setq so (vla-Add (vla-get-TextStyles doc) nm))
               (vla-SetFont so "Arial" :vlax-false :vlax-false 0 34)
               (vla-put-Height so 0.0)
               (vla-put-Width so 1.0)
               (vla-put-ObliqueAngle so 0.0)
               (if (null (tblsearch "APPID" *TVXD37-APP*)) (regapp *TVXD37-APP*))
               (entmod (append (entget (tblobjname "STYLE" nm))
                               (list (list -3 (cons *TVXD37-APP* *TVXD37-DAU*)))))
               ;; entmod lam mat typeface (GetFont rong, DXF3 van ARIAL.TTF):
               ;; dat lai font SAU dau; sau day khong sua kieu chu nua.
               (vla-SetFont so "Arial" :vlax-false :vlax-false 0 34)
               T)
            nil))
  (if (and (not (vl-catch-all-error-p r)) (tvxd37-kieu-cua-ta-p nm))
    nm
    (progn
      (if (setq e (tblobjname "STYLE" nm))
        (vl-catch-all-apply 'vla-Delete (list (vlax-ename->vla-object e))))
      nil)))

;;; --- Ten kieu chu Arial cho SO_TO (dung lai hoac tao), hoac nil ------------
(defun tvxd37-sty-so ( / k nm kq)
  (setq k 1  kq nil)
  (while (and (null kq) (<= k 9))
    (setq nm (if (= k 1) *TVXD37-STY-TEN* (strcat *TVXD37-STY-TEN* "_" (itoa k))))
    (if (tblsearch "STYLE" nm)
      (if (tvxd37-kieu-cua-ta-p nm) (setq kq nm))
      (progn (setq kq (tvxd37-tao-kieu nm)) (setq k 9)))   ; ten trong: tao 1 lan, khong thu tiep
    (setq k (1+ k)))
  kq)

;;; --- Nhu tren, in ly do khi khong co (cho T-TKI / T-CK) ---------------------
(defun tvxd37-sty-so-bao ( / s)
  (if (null (setq s (tvxd37-sty-so)))
    (princ (strcat "\n[LOI] Khong dam bao duoc kieu chu Arial " *TVXD37-STY-TEN*
                   " cho so to (ten da bi kieu chu khac dung den _9, hoac tao/doc lai font loi)."
                   " Khong tao/chen khung; kieu chu co san khong bi sua.")))
  s)

;;; --- ATTRIB ed co dung gia tri s va kieu chu sty? ---------------------------
(defun tvxd37-so-kieu-dung-p (ed s sty)
  (and ed sty (= s (cdr (assoc 1 ed)))
       (= (strcase sty) (strcase (if (assoc 7 ed) (cdr (assoc 7 ed)) "Standard")))))

;;; --- Ghi gia tri s VA kieu chu sty vao ATTRIB a trong MOT entmod; tra T khi
;;;     doc lai dung ca hai. Khong doi vi tri, cao, rong, layer, mau. Co the nem loi.
(defun tvxd37-ghi-so-kieu (a s sty / ed)
  (setq ed (entget a))
  (and ed (assoc 1 ed) sty (tblsearch "STYLE" sty)
       (progn
         (setq ed (subst (cons 1 s) (assoc 1 ed) ed))
         (setq ed (if (assoc 7 ed)
                    (subst (cons 7 sty) (assoc 7 ed) ed)
                    (append ed (list (cons 7 sty)))))
         (entmod ed))
       (tvxd37-so-kieu-dung-p (entget a) s sty)))

;;; ---------------------------------------------------------------------------
(princ "\n            T-STT = danh lai so to cho cac khung da chon (4 huong, so bat dau, buoc 1).")
(princ)
