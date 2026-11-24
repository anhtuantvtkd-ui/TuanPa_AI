;;; ==============================================================
;;;  TK_CHITIET_TUYEN - BO BOC KHOI LUONG "PROMAX"  (phien ban V118)
;;;  Chieu sang + Dien sinh hoat
;;; --------------------------------------------------------------
;;;  LENH CONG KHAI (V118 - namespace T-):
;;;         T-THCS         quet vung -> bang thong ke CHIEU SANG
;;;         T-THDIEN       quet vung -> bang thong ke DIEN
;;;         T-THONGSO      khai thong so len/xuong [ChieuSang/Dien]
;;;         T-VEDAYCS      ve duong day chieu sang theo ket qua T-THCS
;;;         T-DIMNET       ghi kich thuoc theo tung doan net
;;;         T-KIEMTRA      QA/QC vung chon
;;;         (file XuatBangSangExcel.lsp / lenh XBE la module ngoai, chua doi namespace)
;;;  QUY UOC: net (layer) mang ten LOAI CAP; block thiet bi 1 hinh
;;;         + uu tien TAG thuoc tinh chuan; fallback vi tri LOCAL block (khong phu thuoc xoay)
;;;         mang tien to I- / II- / III-...
;;;  Lich su 75 doi phien ban: luu ho so noi bo, da luoc khoi file.
;;; ==============================================================

(vl-load-com)

;; ================= CAU HINH - SUA THEO CHUAN CONG TY =================
(setq *TK-LenXuongDen*   4.40)   ; m cap len xuong den moi khoang
(setq *TK-TruOng*        2.00)   ; ong = cap - X (m)
(setq *TK-BanKinhTim*    15.0)   ; ban kinh tim text cap quanh khoang (m)
(setq *TK-NguongHDPE-1*  6.0)    ; tiet dien <= 6mm2  -> HDPE 40/30
(setq *TK-NguongHDPE-2*  25.0)   ; tiet dien <= 25mm2 -> HDPE 50/40, lon hon -> 65/50
(setq *TK-LenXuongTu*    4.4)    ; len xuong TU (ap cho khoang xuat tuyen) (m)
(setq *TK-NoiManh*       1.0)    ; 2 dau manh duong cach nhau <= 1m = cham nhau
                                 ; -> tu noi thanh 1 duong lien mach truoc khi do
(setq *TK-DimCach*       1.5)    ; duong kich thuoc tu sinh cach tuyen (m)
;; Len xuong ban ve DIEN theo loai thiet bi (khai bang T-THONGSO > Dien)
;; Len xuong CHIEU SANG theo thiet bi (khai bang T-THONGSO > ChieuSang)
(setq *TK-CS-DenD*  2.2)         ; 1 dau cot den
(setq *TK-CS-TuD*   2.2)         ; 1 dau tu dien chieu sang
(setq *TK-DIEN-HT*  0.0)         ; cot dien HA THE
(setq *TK-DIEN-TT*  0.0)         ; cot dien TRUNG THE
(setq *TK-DIEN-CT*  0.0)         ; tu CONG TO
(setq *TK-DIEN-TBA* 0.0)         ; TBA
(setq *TK-DIEN-TB*  0.0)         ; tu bu
(setq *TK-DauPline*      20.0)   ; diem chen block cot cach tuyen pline <= gia
                                 ; tri nay (m) thi coi la cot nam tren tuyen do
                                 ; (block den co diem chen o than den phia tren
                                 ; tuyen nen can nguong rong; chieu vuong goc
                                 ; tu triet tieu do lech, so do van chuan)
(setq *TK-MaTu*          "KCS")  ; TEN_COT chua chuoi nay = TU dieu khien
;; Co CHI DINH CHE DO do lenh T-THCS / T-THDIEN dat truoc khi goi.
;; nil = tu phan xu (chi con la luoi an toan - khong lenh nao dat nil).
(setq *TK-CheDo* nil)
                                 ; chieu sang (nut so 0 dau tuyen)
(setq *TK-GomCumQD*      10.0)   ; ghi chu ong QD cach nhau <= 10m = CUNG 1 cho
                                 ; qua duong (chung 1 ranh, lay ong dai nhat)
(setq *TK-RanhMacDinh*   "RÃNH 1-3 CÁP") ; loai ranh mac dinh; khoang khac loai
                                          ; thi ghi text "R..." gan khoang do
(setq *TK-XuatCSV*       T)      ; T = xuat kem file CSV, nil = khong
(setq *TK-TextStyle*     "MEP_TEXT_STYLE")
;; =====================================================================

(defun tk-chay-thongke (/ *error* doc sys_cmdecho sys_osmode ss i ent obj name tag
                      v_ten v_ts pt lst_cot_raw lst_cap_raw main_poles branch_poles
                      pairs text_h row_h pt_tbl tbl row c stt p1 p2 pt1 pt2
                      best_cap tuyen_name l_dim l_dang l_cap l_ong unique_pole_types
                      unique_cap_types unique_hdpe_types num_pole_types num_cap_types
                      num_hdpe_types total_cols total_rows visited_poles sum_dim
                      sum_dang sum_cap sum_cap_types sum_hdpe_types sum_pole_types
                      row_pole_counts idx_cap idx_hdpe idx_pole cap_short hdpe_short
                      cnt_k m_val b_list k col_w styleObj item att missing csvrows
                      fn f s fmt_ok res skipped notcount cot
                      lst_qd_raw unique_qd_types num_qd_types qd_alloc qd_item
                      best_i best_d dqd pidx v_qd sum_qd_types pair qd_nm s2
                      lst_ranh_raw unique_ranh_types num_ranh_types ranh_decl
                      cur rtype r_he r_qd idx_r sum_rhe sum_rqd
                      qd_pts qd_clusters rqd_alloc lst2 cl found2 ptq lenq
                      lst_pluscap lst_cr_pts cap_span plus_span plus_hdpe
                      cr_spans v2 cnt lst_crcap k2 crlen_span crv row_cab sum_cab
                      cab_list lst_pline pline_kc plv obj2 q1 q2 pe
                      src_pl src_note src_auto m1 m2 wa wb wp pl0 ss2 dim_list
                      lo_groups lo_marks lo_starts lo_ends all_mp all_bp grp
                      gpairs snap li lo_nm
                      lst_dim_kc dimkc_span spanobj
                      lo_d lo_c lo_day sub2 wc wd
                      lst_crlen crlen2 r_cr sum_cr crcand crlo che_do)

  ;; doc co che do RO I XOA NGAY -> lan chay sau khong bi dinh co cu
  (setq che_do *TK-CheDo*)
  (setq *TK-CheDo* nil)
  (princ (strcat "\n[TK_CHITIET_TUYEN phien ban V118] "
                 (cond ((= che_do "CS")   "T-THCS - thong ke CHIEU SANG")
                       ((= che_do "DIEN") "T-THDIEN - thong ke DIEN")
                       (T "che do tu phan xu"))))
  (if (null (tk-doc-thongso))
    (progn
      (princ "\n>> Ban ve nay CHUA khai thong so du an - nhap ngay:")
      (tk-nhap-thongso)))
  (princ (strcat "\n>> Dau vao: xuong tu+len den " (rtos *TK-LenXuongTu* 2 1)
                 "m | xuong den+len den " (rtos *TK-LenXuongDen* 2 1)
                 "m  (doi: lenh T-THONGSO)"))
  (setq doc (vla-get-ActiveDocument (vlax-get-acad-object)))

  ;; --- 1. XU LY LOI & UNDO ---
  (defun *error* (msg)
    (if sys_cmdecho (setvar "CMDECHO" sys_cmdecho))
    (if sys_osmode  (setvar "OSMODE"  sys_osmode))
    (if (and tbl (not (vlax-erased-p tbl)))
      (vl-catch-all-apply 'vlax-put-property
        (list tbl 'RegenerateTableSuppressed :vlax-false)))
    (vla-EndUndoMark doc)
    (if (and msg (not (wcmatch (strcase msg t) "*cancel*,*exit*")))
      (princ (strcat "\n[LOI LISP]: " msg)))
    (princ))

  (setq sys_cmdecho (getvar "CMDECHO"))
  (setq sys_osmode  (getvar "OSMODE"))
  (setvar "CMDECHO" 0)
  (vla-StartUndoMark doc)

  ;; --- 2. TEXT STYLE CHUAN (HEIGHT = 0 DE TABLE TU DIEU CHINH) ---
  (if (null (tblsearch "STYLE" *TK-TextStyle*))
    (progn
      (setq styleObj (vla-add (vla-get-TextStyles doc) *TK-TextStyle*))
      (vla-SetFont styleObj "Arial" :vlax-false :vlax-false 0 0)
      (vla-put-Height styleObj 0.0))
    (progn
      (setq styleObj (vla-item (vla-get-TextStyles doc) *TK-TextStyle*))
      (vla-put-Height styleObj 0.0)))
  (setvar "TEXTSTYLE" *TK-TextStyle*)

  ;; --- 3. CHIEU CAO CHU ---
  (setq text_h (getreal "\nNhap chieu cao chu trong Bang (Model 1/100 dung 250, Layout 2.5) <250>: "))
  (if (or (null text_h) (<= text_h 0)) (setq text_h 250.0))
  (setq row_h (* text_h 2.2))

  ;; --- 4. QUET CHON DU LIEU ---
  (princ "\nQuet chon Block cot den/tu, PLINE tuyen va cac ghi chu")
  (princ "\n(keo khung tu PHAI sang TRAI - Crossing - de duong tuyen dai chac chan dinh vao):")
  (setq ss (ssget '((-4 . "<OR")
                    (0 . "INSERT")
                    (0 . "DIMENSION,TEXT,MTEXT")
                    (0 . "LWPOLYLINE,POLYLINE,LINE,SPLINE,ARC")
                    (-4 . "OR>"))))

  (if ss
    (progn
      (setq lst_cot_raw '() lst_cap_raw '() lst_qd_raw '() lst_ranh_raw '()
            lst_pluscap '() lst_cr_pts '() lst_crcap '() lst_pline '()
            lst_dim_kc '() lst_crlen '()
            unique_pole_types '() unique_cap_types '() unique_hdpe_types '()
            unique_qd_types '() unique_ranh_types '())

      (repeat (setq i (sslength ss))
        (setq ent  (ssname ss (setq i (1- i)))
              obj  (vlax-ename->vla-object ent)
              name (vla-get-ObjectName obj))

        ;; A. BLOCK COT DEN
        (if (= name "AcDbBlockReference")
          (if (= (vla-get-HasAttributes obj) :vlax-true)
            (progn
              (setq v_ten "" v_ts "")
              (foreach att (vlax-safearray->list (vlax-variant-value (vla-GetAttributes obj)))
                (setq tag (strcase (vla-get-TagString att)))
                (cond
                  ((or (= tag "TEN_COT") (= tag "TENCOT") (= tag "NAME"))
                   (setq v_ten (vla-get-TextString att)))
                  ((or (= tag "THONG_SO") (= tag "THONGSO") (= tag "LOAICOT"))
                   (setq v_ts (vla-get-TextString att)))))
              (setq pt (vlax-safearray->list (vlax-variant-value (vla-get-InsertionPoint obj))))
              (if (/= v_ten "")
                (progn
                  (if (= v_ts "") (setq v_ts "Mac dinh"))
                  ;; (a) TEN_COT hoac THONG_SO chua ma tu (tim tren chuoi GOC,
                  ;;     khong qua lam sach de mien nhiem ma \U+ tieng Viet)
                  (if (or (vl-string-search *TK-MaTu* (strcase v_ten))
                          (vl-string-search *TK-MaTu* (strcase v_ts)))
                    (setq lst_cot_raw
                      (cons (list v_ten v_ts pt 0.0 T) lst_cot_raw))
                    (progn
                      (if (not (member v_ts unique_pole_types))
                        (setq unique_pole_types (cons v_ts unique_pole_types)))
                      (setq lst_cot_raw
                        (cons (list v_ten v_ts pt (Get-Pole-Num v_ten)
                                    nil (Get-Pole-Path v_ten))
                              lst_cot_raw)))))
                ;; (b) Khong co TEN_COT: neu BAT KY thuoc tinh nao chua ma tu
                ;;     -> van nhan la TU (tag dat ten gi cung duoc)
                (progn
                  (setq v_ten "")
                  (foreach att (vlax-safearray->list (vlax-variant-value (vla-GetAttributes obj)))
                    (if (and (= v_ten "")
                             (vl-string-search *TK-MaTu*
                                               (strcase (vla-get-TextString att))))
                      (setq v_ten (vla-get-TextString att))))
                  (if (/= v_ten "")
                    (setq lst_cot_raw
                      (cons (list v_ten "TU" pt 0.0 T) lst_cot_raw))
                    (setq skipped (1+ (cond (skipped) (0))))))))))

        ;; B. TEXT / DIM: "+cap di nho" | "CR" | cap chinh | ong QD | loai ranh
        (if (member name '("AcDbRotatedDimension" "AcDbAlignedDimension" "AcDbText" "AcDbMText"))
          (cond
            ;; B1. Soi cap DI NHO: bat dau bang dau "+"
            ((setq item (Parse-PlusCap-Obj obj))
             (setq cap_short (nth 2 item))
             (if (not (member cap_short unique_cap_types))
               (setq unique_cap_types (cons cap_short unique_cap_types)))
             (setq lst_pluscap (cons item lst_pluscap)))
            ;; B1b. Cap khoang NHANH di chung ranh: "CR-DSTA 16(25m)"
            ((setq item (Parse-CRCap-Obj obj))
             (setq cap_short (nth 2 item))
             (if (not (member cap_short unique_cap_types))
               (setq unique_cap_types (cons cap_short unique_cap_types)))
             (setq lst_crcap (cons item lst_crcap)))
            ;; B2. Ghi chu "CR" - khoang di chung ranh
            ((setq item (Parse-CRLen-Obj obj))
             (setq lst_crlen (cons item lst_crlen)))
            ((setq item (Parse-CR-Obj obj))
             (setq lst_cr_pts (cons item lst_cr_pts)))
            ;; B2b. Dong chu roi chua ma tu ("TDKCS-01") = TU dieu khien
            ((and (setq item (tk-rawtext obj))
                  (vl-string-search *TK-MaTu* (strcase item)))
             (setq lst_cot_raw
               (cons (list (Get-QD-Name item "") "TU" (tk-objpt obj) 0.0 T)
                     lst_cot_raw)))
            ;; B3. Cap chinh (DSTA...)
            ((setq item (Parse-Cap-Obj obj))
             (setq cap_short (nth 2 item))
             (if (not (member cap_short unique_cap_types))
               (setq unique_cap_types (cons cap_short unique_cap_types)))
             (setq lst_cap_raw (cons item lst_cap_raw)))
            ;; B4. Ong qua duong
            ((setq item (Parse-QD-Obj obj))
             (setq qd_nm nil)
             (foreach s2 unique_qd_types
               (if (and (null qd_nm)
                        (= (tk-qd-norm s2) (tk-qd-norm (nth 2 item))))
                 (setq qd_nm s2)))
             (if (null qd_nm)
               (progn
                 (setq qd_nm (nth 2 item))
                 (setq unique_qd_types (cons qd_nm unique_qd_types))))
             (setq lst_qd_raw (cons (list (car item) (cadr item) qd_nm) lst_qd_raw)))
            ;; B5. Loai ranh ("R...")
            ((setq item (Parse-Ranh-Obj obj))
             (setq qd_nm nil)
             (foreach s2 unique_ranh_types
               (if (and (null qd_nm)
                        (= (tk-qd-norm s2) (tk-qd-norm (cadr item))))
                 (setq qd_nm s2)))
             (if (null qd_nm)
               (progn
                 (setq qd_nm (cadr item))
                 (setq unique_ranh_types (cons qd_nm unique_ranh_types))))
             (setq lst_ranh_raw (cons (list (car item) qd_nm) lst_ranh_raw))))))

        ;; B6. DIM TRAN (khong mang chu DSTA/CR/QD/R) = KHOANG CACH do tay
        (if (and (member name '("AcDbRotatedDimension" "AcDbAlignedDimension"))
                 (/= (strcase (vla-get-Layer obj)) "TK-KICH-THUOC")
                 (null (Get-DSTA-Spec (vla-get-Layer obj)))
                 (null (Parse-Cap-Obj obj))
                 (null (Parse-PlusCap-Obj obj))
                 (null (Parse-CRCap-Obj obj))
                 (null (Parse-CR-Obj obj))
                 (null (Parse-QD-Obj obj))
                 (null (Parse-Ranh-Obj obj)))
          (progn
            (setq v2 (vl-catch-all-apply 'vla-get-Measurement (list obj)))
            (if (and (not (vl-catch-all-error-p v2))
                     (numberp v2)
                     (> v2 0.001))
              (setq lst_dim_kc
                (cons (list v2 (tk-objpt obj)) lst_dim_kc)))))

        ;; C. MOI DUONG PLINE/LINE/SPLINE LIEN MACH = 1 CHIEU DAI:
        ;; lay 2 DAU + TRON chieu dai ca duong (gap khuc van do du)
        (if (member name '("AcDbPolyline" "AcDb2dPolyline" "AcDb3dPolyline"
                           "AcDbLine" "AcDbSpline"))
          (progn
            (setq q1  (vlax-curve-getStartPoint obj)
                  q2  (vlax-curve-getEndPoint obj)
                  plv (vlax-curve-getDistAtParam obj
                        (vlax-curve-getEndParam obj)))
            (if (> plv 0.001)
              (setq lst_pline (cons (list q1 q2 plv obj) lst_pline)))))

      ;; Tra lai thu tu xuat hien (vi vong quet dung cons)
      (setq lst_cot_raw       (reverse lst_cot_raw)
            lst_cap_raw       (reverse lst_cap_raw)
            lst_qd_raw        (reverse lst_qd_raw)
            unique_pole_types (reverse unique_pole_types))

      ;; --- PHAN XU CHE DO ---
      ;; T-THDIEN -> chay thang nhanh DIEN, KHONG doan gi ca
      ;; T-THCS -> chay thang nhanh CHIEU SANG, khong bao gio nhay sang dien
      ;; khong co co (khong xay ra) -> tu doan: khong cot den = dien
      (if (or (= che_do "DIEN")
              (and (null che_do) (null lst_cot_raw)))
        (progn
          (if (null che_do)
            (progn
              (princ "\n[Thong bao] Vung chon KHONG co cot den chieu sang")
              (princ "\n-> tu chuyen CHE DO DEM VAN NANG: block theo ten + duong theo lop.")))
          (tk-dien-chitiet ss text_h)
          (exit)))
      (if (and (= che_do "CS") (null lst_cot_raw))
        (progn
          (princ "\n[LOI] Lenh T-THCS nhung vung chon KHONG co cot den chieu sang nao.")
          (princ "\n      (cot den = block co thuoc tinh tag TEN_COT / TENCOT / NAME)")
          (princ "\n      Neu day la ban ve DIEN, hay dung lenh T-THDIEN.")
          (exit)))
      (if (and skipped (> skipped 0))
        (princ (strcat "\n[LUU Y] " (itoa skipped)
                       " block co thuoc tinh nhung THIEU TEN_COT -> da bo qua.")))

      (setq unique_cap_types (vl-sort unique_cap_types 'tk-numlist-less))

      (foreach cap_short unique_cap_types
        (setq hdpe_short (Get-HDPE-Type cap_short))
        (if (not (member hdpe_short unique_hdpe_types))
          (setq unique_hdpe_types (cons hdpe_short unique_hdpe_types))))
      (setq unique_hdpe_types (vl-sort unique_hdpe_types 'tk-numlist-less))

      (setq unique_qd_types (vl-sort unique_qd_types 'tk-numlist-less))

      ;; Cot den: thap -> cao (so dau tien trong ten = chieu cao cot)
      (setq unique_pole_types (vl-sort unique_pole_types 'tk-numlist-less))

      ;; Loai ranh mac dinh luon co mat trong danh sach
      (setq qd_nm nil)
      (foreach s2 unique_ranh_types
        (if (and (null qd_nm)
                 (= (tk-qd-norm s2) (tk-qd-norm *TK-RanhMacDinh*)))
          (setq qd_nm s2)))
      (if (null qd_nm)
        (setq unique_ranh_types (cons *TK-RanhMacDinh* unique_ranh_types)))
      (setq unique_ranh_types (vl-sort unique_ranh_types 'tk-numlist-less))

      ;; --- 5. PHAN LOAI COT CHINH & NHANH ---
      (setq main_poles '() branch_poles '() cab_list '())
      (foreach item lst_cot_raw
        (cond
          ((nth 4 item)                       ; tu dieu khien: xu ly rieng
           (setq cab_list (cons item cab_list)))
          ((< (length (nth 5 item)) 2)
           (setq main_poles (cons item main_poles)))
          (t (setq branch_poles (cons item branch_poles)))))
      (setq cab_list (vl-sort (reverse cab_list)
                              '(lambda (a b) (tk-numlist-less (car a) (car b)))))
      ;; Gop tu bi nhan doi (block + dong chu cung 1 cho): cach < 15m = 1 tu
      (setq lst2 '())
      (foreach item cab_list
        (setq found2 nil)
        (foreach cur lst2
          (if (< (distance (caddr item) (caddr cur)) 15.0) (setq found2 T)))
        (if (not found2) (setq lst2 (cons item lst2))))
      (setq cab_list (reverse lst2))
      ;; lst_cot_raw = cot den + danh sach tu DA gop (de khoi dem trung)
      (setq lst_cot_raw
        (append (vl-remove-if '(lambda (x) (nth 4 x)) lst_cot_raw) cab_list))
      ;; Bao cao nhan dien de nguoi dung doi chieu ngay
      (princ (strcat "\n>> Nhan dien: "
                     (itoa (- (length lst_cot_raw) (length cab_list)))
                     " cot den + " (itoa (length cab_list)) " tu dieu khien"))
      (foreach item cab_list (princ (strcat "  [" (car item) "]")))
      (if (null cab_list)
        (princ (strcat "\n[LUU Y] KHONG thay tu dieu khien nao trong vung chon!"
                       "\n        Tu duoc nhan khi: block co thuoc tinh chua \""
                       *TK-MaTu* "\" (vd TDKCS-01),"
                       "\n        HOAC co dong chu roi \"TDKCS-...\" dat canh tu.")))
      (setq branch_poles (reverse branch_poles))
      (setq main_poles (vl-sort (reverse main_poles)
                                '(lambda (a b) (< (nth 3 a) (nth 3 b)))))
      ;; --- 5b. TACH LO theo tien to ten cot (I-, II-, III-...) ---
      (setq lo_groups '())
      (foreach item main_poles
        (setq lo_nm (tk-lo-prefix (car item)))
        (if (setq pair (assoc lo_nm lo_groups))
          (setq lo_groups
            (subst (list lo_nm (cons item (cadr pair)) (caddr pair))
                   pair lo_groups))
          (setq lo_groups (cons (list lo_nm (list item) '()) lo_groups))))
      (foreach item branch_poles
        (setq lo_nm (tk-lo-prefix (car item)))
        (if (setq pair (assoc lo_nm lo_groups))
          (setq lo_groups
            (subst (list lo_nm (cadr pair) (cons item (caddr pair)))
                   pair lo_groups))
          (setq lo_groups (cons (list lo_nm '() (list item)) lo_groups))))
      (setq lo_groups
        (vl-sort (mapcar '(lambda (grp)
                            (list (car grp)
                                  (reverse (cadr grp))
                                  (reverse (caddr grp))))
                         lo_groups)
                 '(lambda (a b)
                    (if (= (tk-lo-rank (car a)) (tk-lo-rank (car b)))
                      (< (car a) (car b))
                      (< (tk-lo-rank (car a)) (tk-lo-rank (car b)))))))
      (if (> (length lo_groups) 1)
        (progn
          (princ (strcat "\n>> Phat hien " (itoa (length lo_groups)) " LO: "))
          (foreach pair lo_groups (princ (strcat "[LO " (car pair) "] ")))))
      ;; --- 6. GHEP KHOANG + XUAT TUYEN: chay RIENG TUNG LO ---
      (setq all_mp main_poles all_bp branch_poles)
      (setq ss2 (ssget "_X" '((0 . "LWPOLYLINE,POLYLINE,LINE,SPLINE"))))
      (setq pairs '() lo_marks '())
      (foreach grp lo_groups
        (setq main_poles (cadr grp) branch_poles (caddr grp))
        (setq k 1)
        (while (< k (length main_poles))
          (if (equal (nth 3 (nth k main_poles))
                     (nth 3 (nth (1- k) main_poles)) 0.001)
            (princ (strcat "\n[CANH BAO] 2 cot chinh TRUNG SO "
                           (rtos (nth 3 (nth k main_poles)) 2 1) " ("
                           (car (nth (1- k) main_poles)) " va "
                           (car (nth k main_poles))
                           ") - ghep khoang co the sai, nen danh so khong trung.")))
          (setq k (1+ k)))
        (setq lst2 (append main_poles branch_poles))
        (setq gpairs '() i 0)
        (while (< i (length main_poles))
          (setq p1 (nth i main_poles))
          (if (zerop i)
            (setq gpairs (append gpairs (tk-emit-subtree p1 lst2))))
          (if (< (1+ i) (length main_poles))
            (progn
              (setq p2 (nth (1+ i) main_poles))
              (setq gpairs (append gpairs (list (list p1 p2))))
              (setq gpairs (append gpairs (tk-emit-subtree p2 lst2)))))
          (setq i (1+ i)))
        (foreach qd_item (reverse cab_list)
          (setq found2 nil best_i 1e9)
          (if ss2
            (progn
              (setq m2 0)
              (repeat (sslength ss2)
                (setq obj2 (vlax-ename->vla-object (ssname ss2 m2)))
                (setq plv (vl-catch-all-apply
                            '(lambda (o)
                               (list (vlax-curve-getStartPoint o)
                                     (vlax-curve-getEndPoint o)))
                            (list obj2)))
                (if (not (vl-catch-all-error-p plv))
                  (foreach q1 (list (list (car plv) (cadr plv))
                                    (list (cadr plv) (car plv)))
                    (setq dqd (distance (car q1) (caddr qd_item)))
                    (if (and (< dqd *TK-DauPline*) (< dqd best_i))
                      (progn
                        (setq wa T)
                        (foreach cur (append main_poles branch_poles)
                          (if (< (distance (car q1) (caddr cur)) dqd)
                            (setq wa nil)))
                        (if wa
                          (progn
                            (setq wb nil wp 1e9)
                            (foreach cur (append main_poles branch_poles)
                              (setq v2 (distance (cadr q1) (caddr cur)))
                              (if (< v2 wp) (setq wp v2 wb cur)))
                            (if (and wb (< wp *TK-DauPline*))
                              (setq best_i dqd found2 wb))))))))
                (setq m2 (1+ m2)))))
          (if (null found2)
            (progn
              (setq best_d 1e9)
              (foreach cur (append main_poles branch_poles)
                (setq dqd (distance (caddr qd_item) (caddr cur)))
                (if (< dqd best_d) (setq best_d dqd found2 cur)))))
          (if found2
            (progn
              (setq gpairs (cons (list qd_item found2) gpairs))
              (princ (strcat "\n>> Xuat tuyen"
                             (if (> (length lo_groups) 1)
                               (strcat " [LO " (car grp) "]") "")
                             ": " (car qd_item) " -> " (car found2)
                             (if (< best_i 1e8) " (theo duong net)"
                               " (den gan nhat)"))))
            (princ (strcat "\n[CANH BAO] Tu " (car qd_item)
                           " khong tim thay den nao de noi xuat tuyen."))))
        (setq pairs (append pairs gpairs))
        (setq lo_marks (append lo_marks
                               (list (cons (car grp) (length gpairs))))))
      ;; V112: xuat danh sach khoang ra bien toan cuc cho lenh T-VEDAYCS dung lai
      ;; (chi GAN bien, khong doi bat ky hanh vi nao cua bang thong ke)
      (setq *TK-Pairs* pairs)
      (setq main_poles all_mp branch_poles all_bp)
      (setq lo_starts '() lo_ends '() i 0)
      (foreach pair lo_marks
        (setq lo_starts (cons (cons i (car pair)) lo_starts))
        (setq i (+ i (cdr pair)))
        (setq lo_ends (cons (cons (1- i) (car pair)) lo_ends)))

      ;; --- 6a-1. GAN MOI DUONG PLINE VAO KHOANG CUA NO ---
      ;; Moi duong lien mach tim cap cot co 2 vi tri gan 2 DAU duong
      ;; nhat (xet ca 2 chieu, so sanh tuong doi). Tron chieu dai ca
      ;; duong = khoang cach cua khoang do. 1 khoang co 2 duong cung
      ;; noi -> giu duong bam cot sat nhat.
      ;; --- 6a-1. MOI DUONG PLINE LIEN MACH = 1 KHOANG (THEO 2 DAU) ---
      ;; Moi DAU duong tu tim COT/TU GAN NHAT (khong nguong -> mien
      ;; nhiem do lech diem chen). 2 cot do dat ten khoang; TRON chieu
      ;; dai duong = khoang cach. 2 soi cung khoang -> cong ca 2.
      ;; Buoc 1: TU QUET duong tuyen TOAN BAN VE - nguoi dung chi can
      ;; quet trung cot, duong khong can nam trong vung chon
      (setq ss2 (ssget "_X" '((0 . "LWPOLYLINE,POLYLINE,LINE,SPLINE"))))
      (setq lst2 '())
      (foreach qd_item lst_pline
        (if (cadddr qd_item)
          (setq lst2 (cons (vla-get-Handle (cadddr qd_item)) lst2))))
      (if ss2
        (progn
          (setq m2 0)
          (repeat (sslength ss2)
            (setq ent  (ssname ss2 m2)
                  obj2 (vlax-ename->vla-object ent))
            (if (not (member (vla-get-Handle obj2) lst2))
              (progn
                (setq q1  (vlax-curve-getStartPoint obj2)
                      q2  (vlax-curve-getEndPoint obj2)
                      plv (vlax-curve-getDistAtParam obj2
                            (vlax-curve-getEndParam obj2)))
                ;; chi lay duong co CA 2 dau cham cot/tu cua lo dang chay
                (setq best_d 1e9 best_i 1e9)
                (foreach item lst_cot_raw
                  (setq dqd (distance q1 (caddr item)))
                  (if (< dqd best_d) (setq best_d dqd))
                  (setq dqd (distance q2 (caddr item)))
                  (if (< dqd best_i) (setq best_i dqd)))
                (if (and (> plv 0.001)
                         (< best_d *TK-DauPline*)
                         (< best_i *TK-DauPline*))
                  (setq lst_pline
                    (cons (list q1 q2 plv obj2) lst_pline)))))
            (setq m2 (1+ m2)))))

      (setq pline_kc '() k2 0 m1 0 pl0 '() spanobj '())
      (if lst_pline (princ "\n>> Chi tiet nhan duong theo 2 dau:"))
      (foreach qd_item lst_pline
        (setq q1 (car qd_item)
              q2 (cadr qd_item)
              best_d 1e9 found2 nil best_i 1e9 cnt nil)
        (foreach item lst_cot_raw
          (setq dqd (distance q1 (caddr item)))
          (if (< dqd best_d) (setq best_d dqd found2 item))
          (setq dqd (distance q2 (caddr item)))
          (if (< dqd best_i) (setq best_i dqd cnt item)))
        ;; ===== KHONG DE 2 DAU DAY ROI VAO 2 LO KHAC NHAU =====
        ;; (cung tinh than voi phan dien) 2 lo di chung hanh lang thi
        ;; cot lo nay nam sat cot lo kia, dau day de "bat" nham sang
        ;; cot cua lo khac -> cap (dau,cuoi) khong con la khoang lien
        ;; ke -> CA SOI DAY BI BO, khoang do phai do thang chim bay
        ;; (ngan hon thuc te). Cach xu ly: dau nao cham GAN HON thi
        ;; tin lo cua dau do, roi tim lai cot cho dau kia TRONG CUNG
        ;; LO ay (tu TDKCS khong co tien to lo -> hop le voi moi lo).
        (if (and found2 cnt)
          (progn
            (setq lo_d (tk-lo-prefix (car found2)))
            (setq lo_c (tk-lo-prefix (car cnt)))
            (if (and (/= lo_d "") (/= lo_c "") (/= lo_d lo_c))
              (progn
                (setq lo_day (if (< best_d best_i) lo_d lo_c))
                (setq wc nil wd 1e9)
                (foreach item lst_cot_raw
                  (setq sub2 (tk-lo-prefix (car item)))
                  (if (or (= sub2 "") (= sub2 lo_day))
                    (progn
                      (setq dqd (distance
                                  (if (< best_d best_i) q2 q1)
                                  (caddr item)))
                      (if (< dqd wd) (setq wd dqd wc item)))))
                (if (and wc (< wd *TK-DauPline*))
                  (progn
                    (if (< m1 12)
                      (princ (strcat "\n   [duong] 2 dau roi vao 2 LO KHAC"
                                     " NHAU (" lo_d " / " lo_c ") -> giu lo "
                                     lo_day ", doi dau kia ve cot ["
                                     (car wc) "]")))
                    (if (< best_d best_i)
                      (setq cnt wc best_i wd)
                      (setq found2 wc best_d wd))))))))
        (cond
          ((or (null found2) (null cnt)) nil)
          ((or (> best_d *TK-DauPline*) (> best_i *TK-DauPline*))
           ;; dau duong khong cham cot lo nay -> khong thuoc lo, bo qua
           (setq k2 (1+ k2)))
          ((eq found2 cnt)
           (if (< m1 12)
             (princ (strcat "\n   [duong] dai " (rtos (caddr qd_item) 2 1)
                            " -> 2 dau cung ve 1 cot [" (car found2)
                            "] -> bo qua")))
           (setq k2 (1+ k2)))
          (t
           (setq pidx 0 v2 nil)
           (foreach pair pairs
             (if (and (null v2)
                      (or (and (eq (car pair) found2) (eq (cadr pair) cnt))
                          (and (eq (car pair) cnt) (eq (cadr pair) found2))))
               (setq v2 pidx))
             (setq pidx (1+ pidx)))
           (if v2
             (progn
               (setq pline_kc (tk-inc pline_kc (itoa v2) (caddr qd_item)))
               ;; nho doi tuong net nguon: 1 duong = field song; 2+ duong = thoi
               (setq cur (assoc (itoa v2) spanobj))
               (setq spanobj
                 (if cur
                   (subst (cons (itoa v2) nil) cur spanobj)
                   (cons (cons (itoa v2) (cadddr qd_item)) spanobj)))
               (if (< m1 12)
                 (princ (strcat "\n   [duong] dai " (rtos (caddr qd_item) 2 1)
                                " -> khoang [" (car found2) " -> " (car cnt)
                                "] (2 dau cham cot cach "
                                (rtos best_d 2 1) "m / "
                                (rtos best_i 2 1) "m)")))
               ;; TEN NET (layer) cua duong = loai cap tu khai
               (if (cadddr qd_item)
                 (progn
                   (setq qd_nm (Get-DSTA-Spec
                                 (vla-get-Layer (cadddr qd_item))))
                   (if qd_nm
                     (progn
                       (if (not (member qd_nm unique_cap_types))
                         (setq unique_cap_types
                           (cons qd_nm unique_cap_types)))
                       (setq wa (Get-HDPE-Type qd_nm))
                       (if (and wa (/= wa "NONE")
                                (not (member wa unique_hdpe_types)))
                         (setq unique_hdpe_types
                           (cons wa unique_hdpe_types)))
                       (setq pl0
                         (cons (list (itoa v2) qd_nm (caddr qd_item)) pl0))
                       (if (< m1 12)
                         (princ (strcat " (net " qd_nm ")"))))))))
             (progn
               (setq k2 (1+ k2))
               (if (< m1 12)
                 (princ (strcat "\n   [duong] dai " (rtos (caddr qd_item) 2 1)
                                " -> noi [" (car found2) "] voi [" (car cnt)
                                "] KHONG PHAI khoang lien ke -> bo qua")))))))
        (setq m1 (1+ m1)))
      (if lst_pline
        (princ (strcat "\n>> Pline: " (itoa (length lst_pline))
                       " duong - nhan chieu dai cho "
                       (itoa (length pline_kc)) "/"
                       (itoa (length pairs)) " khoang.")))

      ;; --- 6a-2. DIM TRAN: cong gia tri do ve khoang gan nhat ---
      (setq dimkc_span '())
      (foreach qd_item lst_dim_kc
        (setq cur (tk-nearest-pair (cadr qd_item) pairs))
        (if (car cur)
          (setq dimkc_span
            (tk-inc dimkc_span (itoa (car cur)) (car qd_item)))))
      (if lst_dim_kc
        (princ (strcat "\n>> Dim tran: " (itoa (length lst_dim_kc))
                       " dim khoang cach - gan cho "
                       (itoa (length dimkc_span)) " khoang.")))

      ;; --- 6a. GAN GHI CHU CAP VE KHOANG GAN NHAT + CONG DON ---
      ;; Moi dim/text LUON gan ve khoang gan nhat (khong loai bo);
      ;; cung khoang -> cong don (khoang chu L/U dim 2-3 lan tu cong lai)
      (setq cap_span '() plus_span '() plus_hdpe '() cr_spans '()
            crlen_span '() k 0 cnt 0)
      ;; TEN NET chi khai LOAI cap (danh dau 0m); chieu dai cua net
      ;; di dung tang 3 (duong tuyen) de CHU CO MET de duoc duong
      (foreach item pl0
        (setq cap_span (tk-inc cap_span
                               (strcat (car item) "|" (cadr item))
                               0.0)))
      ;; Loai moi ghi danh tu ten net -> SAP XEP LAI be den lon
      (setq unique_cap_types  (vl-sort unique_cap_types  'tk-numlist-less))
      (setq unique_hdpe_types (vl-sort unique_hdpe_types 'tk-numlist-less))
      (foreach qd_item lst_cap_raw
        (setq cur (tk-nearest-pair (cadr qd_item) pairs))
        (if (car cur)
          (progn
            (setq cap_span (tk-inc cap_span
                                   (strcat (itoa (car cur)) "|" (nth 2 qd_item))
                                   (car qd_item)))
            (if (> (cadr cur) *TK-BanKinhTim*) (setq k (1+ k)))
            (if (< (- (caddr cur) (cadr cur)) 2.0)
              (princ (strcat "\n[LUU Y] 1 ghi chu cap ("
                             (nth 2 qd_item) " " (rtos (car qd_item) 2 0)
                             "m) nam lung lo giua 2 khoang (chenh < 2m)"
                             " - da gan vao khoang thu "
                             (itoa (1+ (car cur))) ", kiem tra lai."))))))
      (foreach qd_item lst_pluscap
        (setq cur (tk-nearest-pair (cadr qd_item) pairs))
        (if (car cur)
          (progn
            (setq plus_span (tk-inc plus_span
                                    (strcat (itoa (car cur)) "|" (nth 2 qd_item))
                                    (car qd_item)))
            ;; soi di nho co ong HDPE rieng, khong tru dau len cot
            (setq plus_hdpe (tk-inc plus_hdpe
                                    (strcat (itoa (car cur)) "|"
                                            (Get-HDPE-Type (nth 2 qd_item)))
                                    (car qd_item)))
            (if (> (cadr cur) *TK-BanKinhTim*) (setq cnt (1+ cnt))))))
      ;; Ho ghi chu CR: CHI nham vao khoang NHANH (khong dinh truc chinh)
      (setq k2 0)
      (foreach qd_item lst_crcap
        (setq cur (tk-nearest-branch (cadr qd_item) pairs))
        (if (car cur)
          (progn
            ;; met cap cong vao khoang nhanh nhu 1 dim cua nhanh
            (setq cap_span (tk-inc cap_span
                                   (strcat (itoa (car cur)) "|" (nth 2 qd_item))
                                   (car qd_item)))
            ;; va ghi nhan SO MET di chung ranh (chi mien ranh dung phan nay)
            (setq crlen_span (tk-inc crlen_span (itoa (car cur))
                                     (car qd_item))))
          (setq k2 (1+ k2))))
      (foreach qd_item lst_cr_pts
        (setq cur (tk-nearest-branch qd_item pairs))
        (if (car cur)
          (if (not (member (car cur) cr_spans))
            (setq cr_spans (cons (car cur) cr_spans)))
          (setq k2 (1+ k2))))
      ;; ===== CHUNG RANH: ghi chu "CR-II(34,2)" =====
      ;; BAT BUOC khai lo (giong het phan dien). 2 lo di chung hanh
      ;; lang thi ca 2 khoang deu di qua cho dat chu -> do hinh hoc
      ;; khong the phan biet, phai khai ro.
      (setq crlen2 '())
      (foreach qd_item lst_crlen
        (setq crlo (caddr qd_item))
        (if (or (null crlo) (= crlo ""))
          (princ (strcat "\n[LUU Y] Ghi chu chung ranh "
                         (rtos (car qd_item) 2 1)
                         "m CHUA KHAI LO nen KHONG duoc tinh."
                         " Hay ghi ro lo, vd: CR-II(34,2)."))
          (progn
            ;; chi xet cac khoang co THIET BI thuoc dung lo do
            (setq crcand '() pidx 0)
            (foreach pair pairs
              (if (or (= (tk-lo-prefix (car (car pair))) crlo)
                      (= (tk-lo-prefix (car (cadr pair))) crlo))
                (setq crcand (cons (list pidx pair) crcand)))
              (setq pidx (1+ pidx)))
            (if (null crcand)
              (princ (strcat "\n[LUU Y] Ghi chu CR-" crlo
                             "(" (rtos (car qd_item) 2 1)
                             ") nhung KHONG co khoang nao thuoc lo "
                             crlo " - kiem tra lai ten lo."))
              (progn
                ;; trong cung lo: chon khoang GAN NHAT voi cho dat chu
                (setq best_d 1e9 v2 nil)
                (foreach cur crcand
                  (setq dqd (Point-To-Segment-Dist
                              (cadr qd_item)
                              (caddr (car (cadr cur)))
                              (caddr (cadr (cadr cur)))))
                  (if (< dqd best_d) (setq best_d dqd v2 (car cur))))
                (if v2
                  (progn
                    (setq crlen2 (tk-inc crlen2 (itoa v2) (car qd_item)))
                    (princ (strcat "\n>> Chung ranh " (rtos (car qd_item) 2 1)
                                   "m [LO " crlo "] -> khoang ["
                                   (car (car (nth v2 pairs))) " -> "
                                   (car (cadr (nth v2 pairs))) "]")))))))))
      (if (> k2 0)
        (princ (strcat "\n[CANH BAO] " (itoa k2)
                       " ghi chu CR/CR-cap khong tim thay khoang NHANH nao"
                       " trong vung chon -> bo qua.")))
      (if (> k 0)
        (princ (strcat "\n[LUU Y] " (itoa k)
                       " ghi chu cap nam xa hon " (rtos *TK-BanKinhTim* 2 0)
                       "m van duoc gan vao khoang gan nhat - soat lai neu thay la.")))
      (if (> cnt 0)
        (princ (strcat "\n[LUU Y] " (itoa cnt)
                       " ghi chu cap di nho (+) nam xa - da gan vao khoang gan nhat.")))

      ;; --- 6b. PHAN BO ONG QUA DUONG VE KHOANG GAN NHAT ---
      (setq qd_alloc '() qd_pts '() k 0)
      (foreach qd_item lst_qd_raw
        (setq cur (tk-nearest-pair (nth 1 qd_item) pairs))
        (if (car cur)
          (progn
            (setq qd_alloc (tk-inc qd_alloc
                                   (strcat (itoa (car cur)) "|" (nth 2 qd_item))
                                   (car qd_item)))
            ;; luu (khoang, vi tri, chieu dai) de gom cum tinh RANH
            (setq qd_pts (cons (list (car cur) (nth 1 qd_item) (car qd_item))
                               qd_pts))
            (if (> (cadr cur) *TK-BanKinhTim*) (setq k (1+ k))))))
      (if (> k 0)
        (princ (strcat "\n[LUU Y] " (itoa k)
                       " ghi chu ong qua duong nam xa - da gan vao khoang gan nhat.")))

      ;; --- 6b2. RANH QUA DUONG THEO CUM DAO (1 cho dao = 1 ranh) ---
      ;; Cac ghi chu cung khoang, cach nhau <= *TK-GomCumQD* -> chung 1 cum;
      ;; ranh cua cum = ong DAI NHAT; ranh cua khoang = tong cac cum.
      (setq qd_clusters '())
      (foreach qd_item qd_pts
        (setq pidx (car qd_item)
              ptq  (cadr qd_item)
              lenq (caddr qd_item)
              cur  (assoc pidx qd_clusters)
              lst2 (if cur (cdr cur) '())
              found2 nil)
        (foreach cl lst2
          (if (and (null found2)
                   (<= (distance ptq (car cl)) *TK-GomCumQD*))
            (setq found2 cl)))
        (if found2
          (progn
            (if (> (abs (- lenq (cdr found2))) 1.0)
              (princ (strcat "\n[LUU Y] Khoang thu " (itoa (1+ pidx))
                             " co 2 ong chung 1 cho qua duong nhung chieu dai"
                             " lech nhau > 1m - ranh lay theo ong dai nhat.")))
            (setq lst2 (subst (cons (car found2) (max (cdr found2) lenq))
                              found2 lst2)))
          (setq lst2 (cons (cons ptq lenq) lst2)))
        (setq qd_clusters
          (if cur
            (subst (cons pidx lst2) cur qd_clusters)
            (cons (cons pidx lst2) qd_clusters))))
      (setq rqd_alloc '())
      (foreach cur qd_clusters
        (setq lenq 0.0)
        (foreach cl (cdr cur) (setq lenq (+ lenq (cdr cl))))
        (setq rqd_alloc (cons (cons (car cur) lenq) rqd_alloc)))

      ;; --- 6c. GAN LOAI RANH DA GHI CHU VE KHOANG GAN NHAT ---
      (setq ranh_decl '())
      (foreach qd_item lst_ranh_raw
        (setq cur (tk-nearest-pair (car qd_item) pairs))
        (if (car cur)
          (progn
            (setq best_i (car cur) best_d (cadr cur))
            (setq cur (assoc best_i ranh_decl))
            (cond
              ((null cur)
               (setq ranh_decl (cons (list best_i (cadr qd_item) best_d) ranh_decl)))
              (t
               (if (/= (tk-qd-norm (cadr cur)) (tk-qd-norm (cadr qd_item)))
                 (princ (strcat "\n[CANH BAO] Khoang thu " (itoa (1+ best_i))
                                " co 2 ghi chu ranh KHAC loai - lay ghi chu gan tuyen hon.")))
               (if (< best_d (caddr cur))
                 (setq ranh_decl
                   (subst (list best_i (cadr qd_item) best_d) cur ranh_decl))))))))

      ;; --- 7. VE BANG THONG KE ---
      (if pairs
        (progn
          (setq num_cap_types  (length unique_cap_types)
                num_hdpe_types (length unique_hdpe_types)
                num_qd_types   (length unique_qd_types)
                num_ranh_types (length unique_ranh_types)
                num_pole_types (length unique_pole_types)
                total_cols     (+ 6 num_cap_types num_hdpe_types num_qd_types
                                  (* 2 num_ranh_types) num_pole_types 1))

          (setq pt_tbl (getpoint "\nClick chon diem dat Bang Thong Ke: "))
          (if pt_tbl
            (progn
              (setq total_rows
                (+ 3 (length pairs)
                   (if (> (length lo_marks) 1)
                     (* 2 (length lo_marks)) 0)))
              (setq tbl (vla-addTable (vla-get-ModelSpace doc)
                                      (vlax-3d-point pt_tbl)
                                      total_rows total_cols
                                      row_h (* text_h 15.0)))

              ;; >>> TOI UU 1: TAT TINH TOAN LAI BANG TRONG KHI DO SO <<<
              (vl-catch-all-apply 'vlax-put-property
                (list tbl 'RegenerateTableSuppressed :vlax-true))

              ;; >>> TOI UU 2: FORMAT THEO LOAI HANG (tu phat hien may cu) <<<
              ;; rowTypes: 1=Title 2=Header 4=Data (7 = tat ca)
              (setq fmt_ok T)
              (foreach res (list
                  (vl-catch-all-apply 'vla-SetTextStyle  (list tbl 7 *TK-TextStyle*))
                  (vl-catch-all-apply 'vla-SetTextHeight (list tbl 1 (* text_h 1.3)))
                  (vl-catch-all-apply 'vla-SetTextHeight (list tbl 2 text_h))
                  (vl-catch-all-apply 'vla-SetTextHeight (list tbl 4 text_h))
                  (vl-catch-all-apply 'vla-SetAlignment  (list tbl 7 acMiddleCenter)))
                (if (vl-catch-all-error-p res) (setq fmt_ok nil)))
              (if (null fmt_ok)
                (princ "\n[Thong bao] AutoCAD ban cu: tu chuyen sang dinh dang tung o."))

              ;; Tieu de + header
              (tk-settext tbl 0 0 "BẢNG THỐNG KÊ CHI TIẾT TUYẾN CỘT ĐÈN, CÁP VÀ ỐNG BẢO VỆ")
              (tk-settext tbl 1 0 "STT")
              (tk-settext tbl 1 1 "CỘT ĐÈN / KHOẢNG TUYẾN")
              (tk-settext tbl 1 2 "KHOẢNG CÁCH MB (m)")
              (tk-settext tbl 1 3 "LÊN XUỐNG\nĐÈN (m)")
              (tk-settext tbl 1 4 "TỔNG CỘNG *1%\n(m)")
              (setq k 0)
              (while (< k num_cap_types)
                (tk-settext tbl 1 (+ 5 k) (strcat (strcase (nth k unique_cap_types)) "\n(m)"))
                (setq k (1+ k)))
              (setq k 0)
              (while (< k num_hdpe_types)
                (tk-settext tbl 1 (+ 5 num_cap_types k)
                             (strcat "L ỐNG " (strcase (nth k unique_hdpe_types))
                                     "\n(-" (rtos *TK-TruOng* 2 0) "m) (m)"))
                (setq k (1+ k)))
              (setq k 0)
              (while (< k num_qd_types)
                (tk-settext tbl 1 (+ 5 num_cap_types num_hdpe_types k)
                             (strcat (nth k unique_qd_types) "\n(m)"))
                (setq k (1+ k)))
              (tk-settext tbl 1
                (+ 5 num_cap_types num_hdpe_types num_qd_types)
                "CHUNG R\U+00C3NH\n(m)")
              (setq k 0)
              (while (< k num_ranh_types)
                (tk-settext tbl 1 (+ 6 num_cap_types num_hdpe_types num_qd_types k)
                             (strcat (nth k unique_ranh_types) " QĐ\n(m)"))
                (setq k (1+ k)))
              (setq k 0)
              (while (< k num_ranh_types)
                (tk-settext tbl 1 (+ 6 num_cap_types num_hdpe_types num_qd_types
                                     num_ranh_types k)
                             (strcat (nth k unique_ranh_types) " HÈ\n(m)"))
                (setq k (1+ k)))
              (setq k 0)
              (while (< k num_pole_types)
                (tk-settext tbl 1 (+ 6 num_cap_types num_hdpe_types num_qd_types (* 2 num_ranh_types) k)
                             (strcat "CỘT ĐÈN + BỘ ĐÈN:\n" (strcase (nth k unique_pole_types))))
                (setq k (1+ k)))
              (tk-settext tbl 1 (+ 6 num_cap_types num_hdpe_types num_qd_types (* 2 num_ranh_types) num_pole_types)
                           "TỦ ĐIỀU KHIỂN\nCHIẾU SÁNG")

              ;; Khoi tao tong
              (setq sum_cap_types '())
              (repeat num_cap_types (setq sum_cap_types (cons 0.0 sum_cap_types)))
              (setq sum_hdpe_types '())
              (repeat num_hdpe_types (setq sum_hdpe_types (cons 0.0 sum_hdpe_types)))
              (setq sum_cr 0.0)
              (setq sum_qd_types '())
              (repeat num_qd_types (setq sum_qd_types (cons 0.0 sum_qd_types)))
              (setq sum_rhe '())
              (repeat num_ranh_types (setq sum_rhe (cons 0.0 sum_rhe)))
              (setq sum_rqd '())
              (repeat num_ranh_types (setq sum_rqd (cons 0.0 sum_rqd)))
              (setq sum_pole_types '())
              (repeat num_pole_types (setq sum_pole_types (cons 0 sum_pole_types)))

              (setq visited_poles '() missing '() csvrows '() sum_cab 0
                    row 2 stt 1 pidx 0 sum_dim 0.0 sum_dang 0.0 sum_cap 0.0
                    src_pl 0 src_note 0 src_auto 0 dim_list '() li 0 snap nil)

              ;; --- VONG LAP DIEN DU LIEU (chi con setText, khong format o) ---
              (foreach item pairs
                ;; dau LO moi (nhieu lo): dong de muc + reset STT + snapshot
                (if (and (> (length lo_marks) 1)
                         (setq lo_nm (cdr (assoc li lo_starts))))
                  (progn
                    (vla-setRowHeight tbl row (* row_h 1.15))
                    (vl-catch-all-apply 'vla-MergeCells
                      (list tbl row row 0 (1- total_cols)))
                    (tk-settext tbl row 0 (strcat "L\U+1ED8 " lo_nm))
                    (vl-catch-all-apply 'vla-SetCellAlignment
                      (list tbl row 0 acMiddleLeft))
                    (setq row (1+ row) stt 1)
                    (setq snap (list sum_dim sum_dang sum_cap sum_cap_types
                                     sum_hdpe_types sum_qd_types sum_rqd
                                     sum_rhe sum_pole_types sum_cab sum_cr))))
                (setq p1 (car item) p2 (cadr item)
                      tuyen_name (strcat (car p1) " \U+2192 " (car p2)))

                ;; Cap chinh: LOAI lay tu ghi chu (khong bat buoc ghi met);
                ;; CHIEU DAI uu tien PLINE noi 2 cot, khong co pline thi
                ;; lay tong so met cac ghi chu (cach cu)
                (setq l_dim 0.0 plv -1.0 cap_short "NONE" cnt 0 k 0)
                (while (< k num_cap_types)
                  (setq v_qd (cdr (assoc (strcat (itoa pidx) "|"
                                                 (nth k unique_cap_types))
                                         cap_span)))
                  (if v_qd
                    (progn
                      (setq cnt (1+ cnt))
                      (if (> v_qd plv)
                        (setq plv v_qd
                              cap_short (nth k unique_cap_types)))))
                  (setq k (1+ k)))
                (if (> plv 0) (setq l_dim plv))
                (if (> cnt 1)
                  (princ (strcat "\n[LUU Y] Khoang thu " (itoa (1+ pidx))
                                 " co " (itoa cnt) " loai cap chinh - lay loai"
                                 " tong lon nhat (soi di nho phai ghi dau +).")))
                ;; UU TIEN: GHI CHU co so met (nguoi ve chu dong khai)
                ;; > duong tuyen > do thang 2 cot
                (setq plv (cond ((cdr (assoc (itoa pidx) pline_kc))) (0.0)))
                ;; DIM tran nguoi ve dat cho khoang nay = uu tien cao nhat
                (setq v2 (cond ((cdr (assoc (itoa pidx) dimkc_span))) (0.0)))
                (if (> v2 0) (setq l_dim v2))
                (cond
                  ((> l_dim 0)                       ; dim/ghi chu: tieng noi cuoi
                   (setq src_note (1+ src_note) crv nil))
                  ((> plv 0)                         ; chua khai -> theo duong ve
                   (setq l_dim plv)
                   (setq src_pl (1+ src_pl) crv T))
                  (t                                 ; khong gi ca -> do thang
                   (setq l_dim (distance (caddr p1) (caddr p2)))
                   (setq crv nil)
                   (if (> l_dim 0) (setq src_auto (1+ src_auto)))))
                (if (> l_dim 0)
                  (setq dim_list
                    (cons (list (caddr p1) (caddr p2) l_dim
                                (if crv
                                  (cdr (assoc (itoa pidx) spanobj))
                                  nil)
                                cap_short)
                          dim_list)))
                ;; Co chieu dai nhung chua biet LOAI cap -> nhac soat
                (if (and (> l_dim 0) (= cap_short "NONE"))
                  (setq missing (cons tuyen_name missing)))
                (if (> l_dim 0)
                  (progn
                    ;; khoang co TU o 1 dau -> dung len xuong TU
                    (setq l_dang (if (or (nth 4 p1) (nth 4 p2))
                                   *TK-LenXuongTu*
                                   *TK-LenXuongDen*)
                          ;; TONG CONG *1%: cong hao hut, trung cap
                          l_cap  (* 1.01 (+ l_dim l_dang))
                          l_ong  (- l_cap *TK-TruOng*))
                    (if (< l_ong 0) (setq l_ong 0.00))
                    (setq hdpe_short (if (= cap_short "NONE") "NONE"
                                       (Get-HDPE-Type cap_short))))
                  (progn
                    (setq l_dang 0.00 l_cap 0.00 l_ong 0.00 hdpe_short "NONE")
                    (setq missing (cons tuyen_name missing))))

                (setq sum_dim  (+ sum_dim l_dim)
                      sum_dang (+ sum_dang l_dang)
                      sum_cap  (+ sum_cap l_cap))

                (vla-setRowHeight tbl row row_h)
                (tk-settext tbl row 0 (itoa stt))
                (tk-settext tbl row 1 tuyen_name)
                (vla-SetCellAlignment tbl row 1 acMiddleLeft) ; ngoai le duy nhat
                (tk-settext tbl row 2 (rtos l_dim 2 1))
                (tk-settext tbl row 3 (rtos l_dang 2 1))
                (tk-settext tbl row 4 (rtos l_cap 2 1))

                (setq idx_cap (vl-position cap_short unique_cap_types))
                (setq k 0)
                (while (< k num_cap_types)
                  (setq cur (assoc (strcat (itoa pidx) "|"
                                           (nth k unique_cap_types))
                                   plus_span))
                  (setq v_qd (if cur (if (> (cdr cur) 0) (cdr cur) l_dim) 0.0))
                  (setq v2 (+ v_qd (if (and idx_cap (= k idx_cap) (> l_cap 0))
                                     l_cap 0.0)))
                  (if (> v2 0)
                    (progn
                      (tk-settext tbl row (+ 5 k) (rtos v2 2 1))
                      (setq sum_cap_types (Set-Nth k (+ (nth k sum_cap_types) v2) sum_cap_types)))
                    (tk-settext tbl row (+ 5 k) "-"))
                  (setq k (1+ k)))

                (setq idx_hdpe (vl-position hdpe_short unique_hdpe_types))
                (setq k 0)
                (while (< k num_hdpe_types)
                  (setq cur (assoc (strcat (itoa pidx) "|"
                                           (nth k unique_hdpe_types))
                                   plus_hdpe))
                  (setq v_qd (if cur (if (> (cdr cur) 0) (cdr cur) l_dim) 0.0))
                  (setq v2 (+ v_qd (if (and idx_hdpe (= k idx_hdpe) (> l_ong 0))
                                     l_ong 0.0)))
                  (if (> v2 0)
                    (progn
                      (tk-settext tbl row (+ 5 num_cap_types k) (rtos v2 2 1))
                      (setq sum_hdpe_types (Set-Nth k (+ (nth k sum_hdpe_types) v2) sum_hdpe_types)))
                    (tk-settext tbl row (+ 5 num_cap_types k) "-"))
                  (setq k (1+ k)))

                (setq k 0)
                (while (< k num_qd_types)
                  (setq v_qd (cdr (assoc (strcat (itoa pidx) "|"
                                                 (nth k unique_qd_types))
                                         qd_alloc)))
                  (if (and v_qd (> v_qd 0))
                    (progn
                      (tk-settext tbl row (+ 5 num_cap_types num_hdpe_types k)
                                   (rtos v_qd 2 1))
                      (setq sum_qd_types
                        (Set-Nth k (+ (nth k sum_qd_types) v_qd) sum_qd_types)))
                    (tk-settext tbl row (+ 5 num_cap_types num_hdpe_types k) "-"))
                  (setq k (1+ k)))

                ;; Chieu dai RANH qua duong = theo CUM dao (khong phai tong ong)
                (setq r_qd (cond ((cdr (assoc pidx rqd_alloc))) (0.0)))
                ;; Loai ranh cua khoang: theo ghi chu, khong co -> mac dinh
                (setq cur   (assoc pidx ranh_decl)
                      rtype (if cur (cadr cur) *TK-RanhMacDinh*)
                      idx_r nil
                      k 0)
                (foreach s2 unique_ranh_types
                  (if (and (null idx_r)
                           (= (tk-qd-norm s2) (tk-qd-norm rtype)))
                    (setq idx_r k))
                  (setq k (1+ k)))
                ;; RANH HE = KC - met CR-DSTA - ranh qua duong
                ;;           - met KHAI CHUNG RANH voi lo khac "CR-II(34,2)"
                (setq cur (assoc (itoa pidx) crlen_span))
                (setq crv (if cur (if (> (cdr cur) 0) (cdr cur) l_dim) 0.0))
                (setq r_cr (cond ((cdr (assoc (itoa pidx) crlen2))) (0.0)))
                (if (> r_cr l_dim) (setq r_cr l_dim))
                (setq r_he (if (> l_dim 0)
                             (max 0.0 (- l_dim crv r_qd r_cr)) 0.0))
                ;; Ghi chu "CR" DON: CA khoang di chung ranh -> ranh = 0
                (if (member pidx cr_spans)
                  (setq r_qd 0.0 r_he 0.0 r_cr 0.0))
                ;; lam tron 1 so ngay khi cong don -> dong TONG luon khop
                (setq r_cr (tk-lam-tron1 r_cr))
                (setq r_qd (tk-lam-tron1 r_qd))
                (setq r_he (tk-lam-tron1 r_he))
                (tk-settext tbl row
                  (+ 5 num_cap_types num_hdpe_types num_qd_types)
                  (if (> r_cr 0) (rtos r_cr 2 1) "-"))
                (setq sum_cr (+ sum_cr r_cr))
                (setq k 0)
                (while (< k num_ranh_types)
                  (if (and idx_r (= k idx_r) (> r_qd 0))
                    (progn
                      (tk-settext tbl row
                        (+ 6 num_cap_types num_hdpe_types num_qd_types k)
                        (rtos r_qd 2 1))
                      (setq sum_rqd (Set-Nth k (+ (nth k sum_rqd) r_qd) sum_rqd)))
                    (tk-settext tbl row
                      (+ 6 num_cap_types num_hdpe_types num_qd_types k) "-"))
                  (setq k (1+ k)))
                (setq k 0)
                (while (< k num_ranh_types)
                  (if (and idx_r (= k idx_r) (> r_he 0))
                    (progn
                      (tk-settext tbl row
                        (+ 6 num_cap_types num_hdpe_types num_qd_types
                           num_ranh_types k)
                        (rtos r_he 2 1))
                      (setq sum_rhe (Set-Nth k (+ (nth k sum_rhe) r_he) sum_rhe)))
                    (tk-settext tbl row
                      (+ 6 num_cap_types num_hdpe_types num_qd_types
                         num_ranh_types k) "-"))
                  (setq k (1+ k)))

                (setq row_pole_counts '() row_cab 0)
                (repeat num_pole_types (setq row_pole_counts (cons 0 row_pole_counts)))
                (if (not (member (caddr p1) visited_poles))
                  (progn
                    (setq visited_poles (cons (caddr p1) visited_poles))
                    (if (nth 4 p1)
                      (setq row_cab (1+ row_cab))
                      (progn
                        (setq idx_pole (vl-position (cadr p1) unique_pole_types))
                        (if idx_pole
                          (setq row_pole_counts
                            (Set-Nth idx_pole (1+ (nth idx_pole row_pole_counts)) row_pole_counts)))))))
                (if (not (member (caddr p2) visited_poles))
                  (progn
                    (setq visited_poles (cons (caddr p2) visited_poles))
                    (if (nth 4 p2)
                      (setq row_cab (1+ row_cab))
                      (progn
                        (setq idx_pole (vl-position (cadr p2) unique_pole_types))
                        (if idx_pole
                          (setq row_pole_counts
                            (Set-Nth idx_pole (1+ (nth idx_pole row_pole_counts)) row_pole_counts)))))))

                (setq k 0)
                (while (< k num_pole_types)
                  (setq cnt_k (nth k row_pole_counts))
                  (if (> cnt_k 0)
                    (progn
                      (tk-settext tbl row (+ 6 num_cap_types num_hdpe_types num_qd_types (* 2 num_ranh_types) k)
                                   (itoa cnt_k))
                      (setq sum_pole_types (Set-Nth k (+ (nth k sum_pole_types) cnt_k) sum_pole_types)))
                    (tk-settext tbl row (+ 6 num_cap_types num_hdpe_types num_qd_types (* 2 num_ranh_types) k) "-"))
                  (setq k (1+ k)))
                (if (> row_cab 0)
                  (progn
                    (tk-settext tbl row (+ 6 num_cap_types num_hdpe_types num_qd_types (* 2 num_ranh_types) num_pole_types) (itoa row_cab))
                    (setq sum_cab (+ sum_cab row_cab)))
                  (tk-settext tbl row (+ 6 num_cap_types num_hdpe_types num_qd_types (* 2 num_ranh_types) num_pole_types) "-"))

                ;; Luu dong cho CSV
                (if *TK-XuatCSV*
                  (progn
                    (setq s (strcat (itoa stt) ";" (car p1) " -> " (car p2) ";"
                                    (rtos l_dim 2 1) ";" (rtos l_dang 2 1) ";"
                                    (rtos l_cap 2 1)))
                    (setq k 0)
                    (while (< k num_cap_types)
                      (setq cur (assoc (strcat (itoa pidx) "|"
                                               (nth k unique_cap_types))
                                       plus_span))
                      (setq v_qd (if cur (if (> (cdr cur) 0) (cdr cur) l_dim) 0.0))
                      (setq v2 (+ v_qd (if (and idx_cap (= k idx_cap) (> l_cap 0))
                                         l_cap 0.0)))
                      (setq s (strcat s ";" (if (> v2 0) (rtos v2 2 1) "")))
                      (setq k (1+ k)))
                    (setq k 0)
                    (while (< k num_hdpe_types)
                      (setq cur (assoc (strcat (itoa pidx) "|"
                                               (nth k unique_hdpe_types))
                                       plus_hdpe))
                      (setq v_qd (if cur (if (> (cdr cur) 0) (cdr cur) l_dim) 0.0))
                      (setq v2 (+ v_qd (if (and idx_hdpe (= k idx_hdpe) (> l_ong 0))
                                         l_ong 0.0)))
                      (setq s (strcat s ";" (if (> v2 0) (rtos v2 2 1) "")))
                      (setq k (1+ k)))
                    (setq k 0)
                    (while (< k num_qd_types)
                      (setq v_qd (cdr (assoc (strcat (itoa pidx) "|"
                                                     (nth k unique_qd_types))
                                             qd_alloc)))
                      (setq s (strcat s ";" (if (and v_qd (> v_qd 0))
                                              (rtos v_qd 2 1) "")))
                      (setq k (1+ k)))
                    (setq k 0)
                    (while (< k num_ranh_types)
                      (setq s (strcat s ";" (if (and idx_r (= k idx_r) (> r_qd 0))
                                              (rtos r_qd 2 1) "")))
                      (setq k (1+ k)))
                    (setq k 0)
                    (while (< k num_ranh_types)
                      (setq s (strcat s ";" (if (and idx_r (= k idx_r) (> r_he 0))
                                              (rtos r_he 2 1) "")))
                      (setq k (1+ k)))
                    (setq k 0)
                    (while (< k num_pole_types)
                      (setq cnt_k (nth k row_pole_counts))
                      (setq s (strcat s ";" (if (> cnt_k 0) (itoa cnt_k) "")))
                      (setq k (1+ k)))
                    (setq s (strcat s ";" (if (> row_cab 0) (itoa row_cab) "")))
                    (setq csvrows (cons s csvrows))))

                (setq row (1+ row) stt (1+ stt) pidx (1+ pidx))
                ;; cuoi LO: dong tong lo = hieu so voi snapshot
                (if (and (> (length lo_marks) 1)
                         (setq lo_nm (cdr (assoc li lo_ends))))
                  (progn
                    (tk-dong-tong tbl row
                      (strcat "T\U+1ED4NG C\U+1ED8NG TO\U+00C0N L\U+1ED8 " lo_nm)
                      (- sum_dim (nth 0 snap))
                      (- sum_dang (nth 1 snap))
                      (- sum_cap (nth 2 snap))
                      (mapcar '- sum_cap_types (nth 3 snap))
                      (mapcar '- sum_hdpe_types (nth 4 snap))
                      (mapcar '- sum_qd_types (nth 5 snap))
                      (mapcar '- sum_rqd (nth 6 snap))
                      (mapcar '- sum_rhe (nth 7 snap))
                      (mapcar '- sum_pole_types (nth 8 snap))
                      (- sum_cab (nth 9 snap))
                      (- sum_cr (nth 10 snap)))
                    (setq row (1+ row))))
                (setq li (1+ li)))
              (princ (strcat "\n>> Nguon khoang cach: " (itoa src_pl)
                             " tu duong tuyen | " (itoa src_note)
                             " tu ghi chu/dim | " (itoa src_auto)
                             " do thang giua 2 cot."))

              ;; --- DONG TONG CUOI ---
              (tk-dong-tong tbl row
                (if (> (length lo_marks) 1)
                  "T\U+1ED4NG C\U+1ED8NG TO\U+00C0N B\U+1ED8" "TỔNG CỘNG TOÀN LỘ")
                sum_dim sum_dang sum_cap sum_cap_types sum_hdpe_types
                sum_qd_types sum_rqd sum_rhe sum_pole_types sum_cab sum_cr)

              ;; --- RONG COT ---
              (vla-setColumnWidth tbl 0 (* text_h 4.0))
              (vla-setColumnWidth tbl 1 (* text_h 20.0))
              (vla-setColumnWidth tbl 2 (* text_h 15.0))
              (vla-setColumnWidth tbl 3 (* text_h 15.0))
              (vla-setColumnWidth tbl 4 (* text_h 15.0))
              (setq k 0)
              (while (< k num_cap_types)
                (vla-setColumnWidth tbl (+ 5 k) (* text_h 16.0))
                (setq k (1+ k)))
              (setq k 0)
              (while (< k num_hdpe_types)
                (vla-setColumnWidth tbl (+ 5 num_cap_types k) (* text_h 18.0))
                (setq k (1+ k)))
              (setq k 0)
              (while (< k num_qd_types)
                (vla-setColumnWidth tbl (+ 5 num_cap_types num_hdpe_types k)
                                    (* text_h 15.0))
                (setq k (1+ k)))
              (vla-setColumnWidth tbl
                (+ 5 num_cap_types num_hdpe_types num_qd_types)
                (* text_h 16.0))
              (setq k 0)
              (while (< k (* 2 num_ranh_types))
                (vla-setColumnWidth tbl
                  (+ 6 num_cap_types num_hdpe_types num_qd_types k)
                  (* text_h 16.0))
                (setq k (1+ k)))
              (setq k 0)
              (while (< k num_pole_types)
                (setq col_w (* (strlen (nth k unique_pole_types)) text_h 0.95))
                (if (< col_w (* text_h 18.0)) (setq col_w (* text_h 18.0)))
                (vla-setColumnWidth tbl (+ 6 num_cap_types num_hdpe_types num_qd_types (* 2 num_ranh_types) k) col_w)
                (setq k (1+ k)))
              (vla-setColumnWidth tbl (+ 6 num_cap_types num_hdpe_types num_qd_types (* 2 num_ranh_types) num_pole_types) (* text_h 16.0))

              ;; >>> BAT LAI TINH TOAN BANG + REGEN de chu chac chan hien <<<
              (vl-catch-all-apply 'vlax-put-property
                (list tbl 'RegenerateTableSuppressed :vlax-false))
              (vl-catch-all-apply 'vla-Regen (list doc acAllViewports))

              ;; --- XUAT CSV ---
              (if (and *TK-XuatCSV* csvrows)
                (progn
                  (setq fn (strcat (getvar "DWGPREFIX")
                                   (vl-filename-base (getvar "DWGNAME"))
                                   "_TKTUYEN.csv"))
                  (setq f (open fn "w"))
                  (if f
                    (progn
                      (setq s "STT;Khoang tuyen;KC MB (m);Len xuong (m);Tong cong *1% (m)")
                      (foreach cell unique_cap_types  (setq s (strcat s ";" cell " (m)")))
                      (foreach cell unique_hdpe_types (setq s (strcat s ";Ong " cell " (m)")))
                      (foreach cell unique_qd_types   (setq s (strcat s ";" cell " (m)")))
                      (foreach cell unique_ranh_types (setq s (strcat s ";" cell " QD (m)")))
                      (foreach cell unique_ranh_types (setq s (strcat s ";" cell " HE (m)")))
                      (foreach cell unique_pole_types (setq s (strcat s ";" cell " (cai)")))
                      (setq s (strcat s ";Tu dieu khien chieu sang (cai)"))
                      (write-line s f)
                      (foreach s (reverse csvrows) (write-line s f))
                      (setq s (strcat "TONG;;" (rtos sum_dim 2 1) ";"
                                      (rtos sum_dang 2 1) ";" (rtos sum_cap 2 1)))
                      (foreach cell sum_cap_types  (setq s (strcat s ";" (rtos cell 2 1))))
                      (foreach cell sum_hdpe_types (setq s (strcat s ";" (rtos cell 2 1))))
                      (foreach cell sum_qd_types   (setq s (strcat s ";" (rtos cell 2 1))))
                      (foreach cell sum_rqd        (setq s (strcat s ";" (rtos cell 2 1))))
                      (foreach cell sum_rhe        (setq s (strcat s ";" (rtos cell 2 1))))
                      (foreach cell sum_pole_types (setq s (strcat s ";" (itoa cell))))
                      (setq s (strcat s ";" (itoa sum_cab)))
                      (write-line s f)
                      (close f)
                      (princ (strcat "\n>> Da xuat CSV: " fn))))))

              ;; --- KIEM TRA COT CHUA VAO THONG KE ---
              (setq notcount '())
              (foreach cot lst_cot_raw
                (if (not (member (caddr cot) visited_poles))
                  (setq notcount (cons (car cot) notcount))))
              (princ (strcat "\n>> Tong so cot den nhan dien: "
                             (itoa (length lst_cot_raw))
                             " | da vao thong ke: "
                             (itoa (- (length lst_cot_raw) (length notcount)))))
              (if notcount
                (progn
                  (princ (strcat "\n[CANH BAO] " (itoa (length notcount))
                                 " cot CHUA vao thong ke (nhanh thieu cot chinh"
                                 " / so cot trung giua cac lo?): "))
                  (foreach s (reverse notcount) (princ (strcat s "  ")))))

              ;; --- CANH BAO KHOANG THIEU CAP ---
              (if missing
                (progn
                  (princ (strcat "\n[CANH BAO] " (itoa (length missing))
                                 " khoang KHONG tim thay text cap trong ban kinh "
                                 (rtos *TK-BanKinhTim* 2 0) "m (dang ghi 0):"))
                  (foreach s (reverse missing) (princ (strcat "\n   - " s)))))

              (princ "\n[SUCCESS] Da xuat Bang Thong Ke thanh cong!")
)))
        (princ "\n[LOI]: Khong tao duoc khoang tuyen! Kiem tra Block Cot den (phai co Attribute TEN_COT).")))
    (princ "\n[LOI]: Khong chon duoc doi tuong nao!"))

  (*error* nil)
  (princ))

;; ========================================================================
;; HELPER FUNCTIONS (giu nguyen logic ban goc, chi thay hang so cau hinh)
;; ========================================================================

;; Ghi text vao o bang; neu may khong ho tro format theo loai hang
;; (fmt_ok = nil) thi tu dinh dang tung o nhu ban goc (van nhanh vi
;; regen bang dang tat). text_h va fmt_ok nhin thay dong (dynamic scope).
(defun tk-settext (tbl r c v)
  (vla-setText tbl r c v)
  (if (null fmt_ok)
    (progn
      (vl-catch-all-apply 'vla-SetCellTextStyle  (list tbl r c *TK-TextStyle*))
      (vl-catch-all-apply 'vla-SetCellTextHeight
        (list tbl r c (if (= r 0) (* text_h 1.3) text_h)))
      (vl-catch-all-apply 'vla-SetCellAlignment
        (list tbl r c (if (and (>= r 2) (= c 1)) acMiddleLeft acMiddleCenter))))))

;; So sanh 2 ten theo DAY SO trong ten: so dau tien truoc, bang nhau
;; thi so tiep theo... (vi du 6MD-80W < 8MD-100W < 8MD-120W < 9MD-120W)
(defun tk-numlist-less (a b / la lb ra rb done res)
  (setq la (Extract-All-Nums a)
        lb (Extract-All-Nums b)
        done nil res nil)
  (while (and (not done) (or la lb))
    (setq ra (if la (car la) -1)
          rb (if lb (car lb) -1))
    (cond
      ((< ra rb) (setq res T   done T))
      ((> ra rb) (setq res nil done T))
      (t (setq la (cdr la) lb (cdr lb)))))
  (if (not done) (setq res (< (strcase a) (strcase b))))
  res)

;; Tim khoang gan diem nhat. Khoang cach = MIN(den duong noi thang,
;; den cot dau, den cot cuoi) -> dim tren chan dung chu L/U van bam
;; dung khoang cua no. Tra ve (chi-so d-gan-nhat d-nhi) de phat hien
;; ghi chu lung lo giua 2 khoang.
(defun tk-nearest-pair (pt pairs / b1i b1d b2d pidx dqd pa pb pair)
  (setq b1i nil b1d 1e9 b2d 1e9 pidx 0)
  (foreach pair pairs
    (setq pa (caddr (car pair))
          pb (caddr (cadr pair)))
    (setq dqd (min (Point-To-Segment-Dist pt pa pb)
                   (distance pt pa)
                   (distance pt pb)))
    (cond
      ((< dqd b1d) (setq b2d b1d b1d dqd b1i pidx))
      ((< dqd b2d) (setq b2d dqd)))
    (setq pidx (1+ pidx)))
  (list b1i b1d b2d))

;; Khoang NHANH = co it nhat 1 dau la cot nhanh (so thap phan x.y)
(defun tk-branch-pair-p (pair)
  (or (> (length (nth 5 (car pair))) 1)
      (> (length (nth 5 (cadr pair))) 1)))

;; Tim khoang NHANH gan diem nhat (danh rieng cho ho ghi chu CR)
(defun tk-nearest-branch (pt pairs / b1i b1d pidx dqd pa pb pair)
  (setq b1i nil b1d 1e9 pidx 0)
  (foreach pair pairs
    (if (tk-branch-pair-p pair)
      (progn
        (setq pa (caddr (car pair))
              pb (caddr (cadr pair)))
        (setq dqd (min (Point-To-Segment-Dist pt pa pb)
                       (distance pt pa)
                       (distance pt pb)))
        (if (< dqd b1d) (setq b1d dqd b1i pidx))))
    (setq pidx (1+ pidx)))
  (cons b1i b1d))

;; Lay chuoi chu cua dim/text (uu tien TextOverride)
(defun tk-rawtext (obj / txt)
  (if (vlax-property-available-p obj 'TextOverride)
    (setq txt (vla-get-TextOverride obj))
    (setq txt ""))
  (if (or (= txt "") (= txt "<>"))
    (if (vlax-property-available-p obj 'TextString)
      (setq txt (vla-get-TextString obj))))
  (if (and txt (/= txt "")) txt nil))

;; Diem dai dien cua dim/text
(defun tk-objpt (obj)
  (if (member (vla-get-ObjectName obj)
              '("AcDbRotatedDimension" "AcDbAlignedDimension"))
    (vlax-safearray->list (vlax-variant-value (vla-get-TextPosition obj)))
    (vlax-safearray->list (vlax-variant-value (vla-get-InsertionPoint obj)))))

;; ========================================================================
;; DOC THUOC TINH BLOCK THEO NGHIA - KHONG PHU THUOC GOC XOAY / MIRROR
;; V114: sua loi block xoay 180 do lam dao TEN_TU <-> TEN_TBA.
;; Nguyen tac:
;;   1) Uu tien TAG co nghia (TEN_TU, TEN_COT, TEN_TRAM...)
;;   2) Neu ban ve cu khong co TAG chuan, moi fallback theo thu tu Y LOCAL
;;      cua block (da khử Rotation va YScaleFactor), KHONG dung toa do Y WCS.
;; ========================================================================

(defun tk-get-atts-safe (obj / a)
  (setq a nil)
  (if (and obj (= (vla-get-HasAttributes obj) :vlax-true))
    (progn
      (setq a
        (vl-catch-all-apply
          '(lambda (o)
             (vlax-safearray->list
               (vlax-variant-value (vla-GetAttributes o))))
          (list obj)))
      (if (vl-catch-all-error-p a) (setq a nil))))
  a)

;; Lay gia tri dau tien theo danh sach TAG uu tien.
(defun tk-att-value (atts tags / item tag txt res)
  (setq res nil)
  (foreach item atts
    (if (null res)
      (progn
        (setq tag (strcase (vla-get-TagString item))
              txt (vl-string-trim " " (vla-get-TextString item)))
        (if (and (member tag tags) (/= txt ""))
          (setq res txt)))))
  res)

;; Diem hien thi that cua Attribute (uu tien TextAlignmentPoint neu can).
(defun tk-att-point (item / q1)
  (setq q1 (vlax-get item 'InsertionPoint))
  (vl-catch-all-apply
    '(lambda (o)
       (if (/= (vla-get-Alignment o) acAlignmentLeft)
         (setq q1 (vlax-get o 'TextAlignmentPoint))))
    (list item))
  q1)

;; Tra ve (dong-tren dong-thu-2) theo truc Y CUC BO cua block.
;; Vi tri LOCAL nay giu nguyen khi block xoay 90/180/270 do va khi mirror Y.
(defun tk-att-top2-local (obj atts / p0 ang ys item q1 dx dy ly txt mid v2)
  (setq p0  (vlax-get obj 'InsertionPoint)
        ang (cond ((vlax-property-available-p obj 'Rotation)
                   (vla-get-Rotation obj))
                  (0.0))
        ys  (cond ((vlax-property-available-p obj 'YScaleFactor)
                   (vla-get-YScaleFactor obj))
                  (1.0)))
  (if (< (abs ys) 1e-12) (setq ys 1.0))
  (setq mid nil v2 nil)
  (foreach item atts
    (setq txt (vl-string-trim " " (vla-get-TextString item)))
    (if (/= txt "")
      (progn
        (setq q1 (tk-att-point item)
              dx (- (car q1)  (car p0))
              dy (- (cadr q1) (cadr p0))
              ;; chieu vector WCS len truc +Y cua block, sau do bo mirror Y
              ly (/ (+ (* dx (- (sin ang)))
                       (* dy    (cos ang)))
                    ys))
        (if (or (null mid) (> ly (car mid)))
          (progn
            (setq v2 mid)
            (setq mid (cons ly txt)))
          (if (or (null v2) (> ly (car v2)))
            (setq v2 (cons ly txt)))))))
  (list (if mid (cdr mid) nil)
        (if v2  (cdr v2)  nil)))

;; Doc (TEN_THIET_BI TEN_TBA_ME) cho nhanh T-THDIEN.
;; Bo sung them MA_TB / DEVICE_ID / PARENT_ID de san sang cho schema chuan moi.
(defun tk-dien-read-labels (obj atts / key parent fb)
  (setq key
    (tk-att-value atts
      '("DEVICE_ID" "MA_TB" "TEN_TU" "TEN_COT" "TEN_TRAM"
        "TEN_THIET_BI" "TENTHIETBI" "TENCOT" "NAME")))
  (setq parent
    (tk-att-value atts
      '("PARENT_ID" "TEN_TBA" "TBA" "TBA_SO" "PARENT")))
  ;; Ho so cu: giu tuong thich nhung fallback da doc theo he truc LOCAL.
  (if (or (null key) (null parent))
    (setq fb (tk-att-top2-local obj atts)))
  (if (null key) (setq key (car fb)))
  (if (and (null parent) (cadr fb) (/= (cadr fb) key))
    (setq parent (cadr fb)))
  (list key parent))

;; Lenh kiem tra nhanh TAG/nhan sau khi xoay block.
(defun tk-cmd-checknhan (/ ss i obj atts info nm rot src)
  (princ "\n[T-KIEMTRA/NHAN V118] Chon cac block can kiem tra nhan: ")
  (setq ss (ssget '((0 . "INSERT"))))
  (if ss
    (progn
      (setq i 0)
      (repeat (sslength ss)
        (setq obj (vlax-ename->vla-object (ssname ss i))
              atts (tk-get-atts-safe obj)
              nm (vl-catch-all-apply 'vla-get-EffectiveName (list obj)))
        (if (vl-catch-all-error-p nm) (setq nm (vla-get-Name obj)))
        (if atts
          (progn
            (setq info (tk-dien-read-labels obj atts)
                  rot (* 180.0 (/ (vla-get-Rotation obj) pi))
                  src (if (tk-att-value atts
                            '("DEVICE_ID" "MA_TB" "TEN_TU" "TEN_COT" "TEN_TRAM"
                              "TEN_THIET_BI" "TENTHIETBI" "TENCOT" "NAME"))
                        "TAG" "FALLBACK"))
            (princ
              (strcat "\n - " nm
                      " | rot=" (rtos rot 2 2)
                      " | TEN=" (if (car info) (car info) "?")
                      " | TBA_ME=" (if (cadr info) (cadr info) "")
                      " | " src))))
        (setq i (1+ i))))
    (princ "\n[Thong bao] Khong chon block nao."))
  (princ))

;; Soi cap DI NHO (di qua khong len cot): ghi chu bat dau bang dau "+"
;; vi du "+DSTA 10mm2 (52m)" -> (chieu-dai diem loai-cap)
(defun Parse-PlusCap-Obj (obj / txt cl cap_spec l)
  (setq txt (tk-rawtext obj))
  (if txt
    (progn
      (setq cl (vl-string-left-trim " " (Clean-CAD-String txt)))
      (if (and (> (strlen cl) 0) (= (substr cl 1 1) "+"))
        (progn
          (setq cap_spec (Get-DSTA-Spec txt)
                l        (Get-Span-Length obj txt))
          (if cap_spec
            (list (cond ((and l (> l 0)) l) (0.0)) (tk-objpt obj) cap_spec)))))))

;; Cap cua khoang nhanh DI CHUNG RANH: "CR-DSTA 16(25m)" / "CR DSTA..."
;; -> (chieu-dai diem loai-cap); dong thoi danh dau khoang nhanh chung ranh
(defun Parse-CRCap-Obj (obj / txt cl cap_spec l)
  (setq txt (tk-rawtext obj))
  (if txt
    (progn
      (setq cl (vl-string-left-trim " " (Clean-CAD-String txt)))
      (if (and (>= (strlen cl) 3)
               (= (substr cl 1 2) "CR")
               (member (substr cl 3 1) '("-" " " "D"))
               (vl-string-search "DSTA" cl))
        (progn
          (setq cap_spec (Get-DSTA-Spec txt)
                l        (Get-Span-Length obj txt))
          (if cap_spec
            (list (cond ((and l (> l 0)) l) (0.0)) (tk-objpt obj) cap_spec)))))))

;; Ghi chu "CR": khoang di CHUNG RANH voi khoang khac -> tra ve diem
;; Ghi chu "CR(34.2M)" = doan DI CHUNG RANH voi lo khac.
;; CHI TRU RANH, KHONG cong cap, KHONG doi khoang cach.
;; (Khac "CR" tran = ca khoang chung ranh; khac "CR-DSTA 16(25m)"
;;  = doan day noi tiep di chung ranh, co cong cap.)
;; Chi dung trong BANG DIEN - khong dung toi nhanh chieu sang.
(defun Parse-CRLen-Obj (obj / txt txt2 cl l p sub lo)
  (setq txt (tk-rawtext obj))
  (if txt
    (progn
      ;; doi dau PHAY thap phan -> dau CHAM truoc khi doc so
      ;; (ham tk-met-trong-ngoac dung chung chi hieu dau cham; sua
      ;;  rieng o day de khong dung toi nhanh chieu sang)
      (setq txt2 (vl-string-translate "," "." txt))
      (setq cl (vl-string-right-trim " "
                 (vl-string-left-trim " " (Clean-CAD-String txt2))))
      (if (and (>= (strlen cl) 3)
               (= (strcase (substr cl 1 2)) "CR")
               (setq p (vl-string-search "(" cl))
               (>= p 2)
               (setq l (tk-met-trong-ngoac txt2))
               (> l 0))
        (progn
          ;; phan giua "CR" va dau "(" = TEN LO, BAT BUOC phai co:
          ;;   "CR-II(34,2)"  "CR-III(34,2)"  "CR II(34,2M)"
          ;; (chu "M" co hay khong deu duoc)
          (setq sub (vl-string-trim " -" (strcase (substr cl 3 (- p 2)))))
          (setq lo (if (and (/= sub "") (tk-lama-so sub)) sub ""))
          ;; giua "CR" va "(" co chu LA (vd "CR-DSTA 150") -> KHONG
          ;; phai ghi chu chung ranh, tra nil de parser khac xu ly
          (if (or (= sub "") (/= lo ""))
            (list l (tk-objpt obj) lo)))))))

(defun Parse-CR-Obj (obj / txt cl)
  (setq txt (tk-rawtext obj))
  (if txt
    (progn
      (setq cl (vl-string-right-trim " "
                 (vl-string-left-trim " " (Clean-CAD-String txt))))
      (if (= cl "CR") (tk-objpt obj)))))

;; Doc thong so du an da luu trong ban ve (tra ve nil neu chua khai)
(defun tk-doc-thongso (/ a)
  (if (setq a (vlax-ldata-get "TK_THONGSO" "LXDEN"))  (setq *TK-LenXuongDen* a))
  (if (setq a (vlax-ldata-get "TK_THONGSO" "LXTU"))   (setq *TK-LenXuongTu* a))
  (if (setq a (vlax-ldata-get "TK_THONGSO" "TRUONG")) (setq *TK-TruOng* a))
  ;; khoa moi theo-thiet-bi; ban ve cu (chi co tong) -> tu quy doi
  (cond
    ((setq a (vlax-ldata-get "TK_THONGSO" "CSDEND"))
     (setq *TK-CS-DenD* a)
     (if (setq a (vlax-ldata-get "TK_THONGSO" "CSTUD"))
       (setq *TK-CS-TuD* a)))
    ((vlax-ldata-get "TK_THONGSO" "LXDEN")
     (setq *TK-CS-DenD* (/ *TK-LenXuongDen* 2.0)
           *TK-CS-TuD*  (- *TK-LenXuongTu* *TK-CS-DenD*))))
  (vlax-ldata-get "TK_THONGSO" "LXDEN"))

;; Nhap thong so du an va luu vao ban ve
(defun tk-nhap-thongso (/ v)
  (princ "\n=== KHAI BAO LEN XUONG CHIEU SANG (luu ngay trong ban ve) ===")
  (initget 4)
  (setq v (getreal (strcat "\n1. Chieu dai len hoac xuong TU DIEN CHIEU SANG (m) <"
                           (rtos *TK-CS-TuD* 2 2) ">: ")))
  (if v (setq *TK-CS-TuD* v))
  (initget 4)
  (setq v (getreal (strcat "\n2. Chieu dai len hoac xuong COT DEN (m) <"
                           (rtos *TK-CS-DenD* 2 2) ">: ")))
  (if v (setq *TK-CS-DenD* v))
  ;; khoang = tong len xuong cua 2 thiet bi 2 dau (giong ben dien)
  (setq *TK-LenXuongDen* (* 2.0 *TK-CS-DenD*)          ; den -> den
        *TK-LenXuongTu*  (+ *TK-CS-TuD* *TK-CS-DenD*)) ; tu  -> den
  (vlax-ldata-put "TK_THONGSO" "CSDEND" *TK-CS-DenD*)
  (vlax-ldata-put "TK_THONGSO" "CSTUD"  *TK-CS-TuD*)
  (vlax-ldata-put "TK_THONGSO" "LXDEN"  *TK-LenXuongDen*)
  (vlax-ldata-put "TK_THONGSO" "LXTU"   *TK-LenXuongTu*)
  (vlax-ldata-put "TK_THONGSO" "TRUONG" *TK-TruOng*)
  (princ (strcat "\n>> Da luu: tu " (rtos *TK-CS-TuD* 2 2)
                 "m | den " (rtos *TK-CS-DenD* 2 2)
                 "m -> khoang den-den = " (rtos *TK-LenXuongDen* 2 2)
                 "m, khoang tu-den = " (rtos *TK-LenXuongTu* 2 2) "m."))
  (princ))


;; ==== THONG SO DIEN: doc tu ban ve / nhap 4 cau ====
(defun tk-doc-thongso-dien (/ a co)
  (setq co nil)
  (if (setq a (vlax-ldata-get "TK_THONGSO" "DIENHT"))  (setq *TK-DIEN-HT* a co T))
  (if (setq a (vlax-ldata-get "TK_THONGSO" "DIENTT"))  (setq *TK-DIEN-TT* a co T))
  (if (setq a (vlax-ldata-get "TK_THONGSO" "DIENCT"))  (setq *TK-DIEN-CT* a co T))
  (if (setq a (vlax-ldata-get "TK_THONGSO" "DIENTBA")) (setq *TK-DIEN-TBA* a co T))
  (if (setq a (vlax-ldata-get "TK_THONGSO" "DIENTB"))  (setq *TK-DIEN-TB* a co T))
  co)

(defun tk-nhap-thongso-dien (/ v)
  (princ "\n=== KHAI BAO LEN XUONG BAN VE DIEN (luu ngay trong ban ve) ===")
  (initget 4)
  (setq v (getreal (strcat "\n1. Chieu dai len hoac xuong COT DIEN HA THE"
                           " (ke ca ten co chu \"CD\") (m) <"
                           (rtos *TK-DIEN-HT* 2 2) ">: ")))
  (if v (setq *TK-DIEN-HT* v))
  (initget 4)
  (setq v (getreal (strcat "\n2. Chieu dai len hoac xuong COT DIEN TRUNG THE"
                           " (ke ca ten co chu \"CTT\") (m) <"
                           (rtos *TK-DIEN-TT* 2 2) ">: ")))
  (if v (setq *TK-DIEN-TT* v))
  (initget 4)
  (setq v (getreal (strcat "\n3. Chieu dai len hoac xuong TU CONG TO (m) <"
                           (rtos *TK-DIEN-CT* 2 2) ">: ")))
  (if v (setq *TK-DIEN-CT* v))
  (initget 4)
  (setq v (getreal (strcat "\n4. Chieu dai len hoac xuong TBA (m) <"
                           (rtos *TK-DIEN-TBA* 2 2) ">: ")))
  (if v (setq *TK-DIEN-TBA* v))
  (initget 4)
  (setq v (getreal (strcat "\n5. Chieu dai len hoac xuong TU BU (m) <"
                           (rtos *TK-DIEN-TB* 2 2) ">: ")))
  (if v (setq *TK-DIEN-TB* v))
  (vlax-ldata-put "TK_THONGSO" "DIENHT"  *TK-DIEN-HT*)
  (vlax-ldata-put "TK_THONGSO" "DIENTT"  *TK-DIEN-TT*)
  (vlax-ldata-put "TK_THONGSO" "DIENCT"  *TK-DIEN-CT*)
  (vlax-ldata-put "TK_THONGSO" "DIENTBA" *TK-DIEN-TBA*)
  (vlax-ldata-put "TK_THONGSO" "DIENTB"  *TK-DIEN-TB*)
  (princ (strcat "\n>> Da luu: cot dien ha the " (rtos *TK-DIEN-HT* 2 2)
                 "m | cot dien trung the " (rtos *TK-DIEN-TT* 2 2)
                 "m | cong to " (rtos *TK-DIEN-CT* 2 2)
                 "m | TBA " (rtos *TK-DIEN-TBA* 2 2)
                 "m | tu bu " (rtos *TK-DIEN-TB* 2 2)
                 "m. Moi soi day tu cong len xuong cua 2 thiet bi 2 dau."))
  (princ))


;; Phan loai thiet bi theo ten/thuoc tinh (khong phan biet dau tieng Viet)
(defun tk-loai-thietbi (nm / k)
  (setq k (strcase nm))
  (cond
    ((wcmatch k "*TBA*")           4)
    ;; cot dien TRUNG THE: nhan them ma "CTT" (Đ-CTT dau tuyen,
    ;; G-CTT-x.x giua tuyen, C-CTT cuoi tuyen) - CUNG LOAI voi cot
    ;; trung the thuong (dung chung so len/xuong TT)
    ((wcmatch k "*TRUNG*,*-CTT-*,*-CTT,CTT-*") 2)
    ((wcmatch k "*TU BU*,*T? B?*,TB-*") 5)
    ;; cot dien: ten chua "CD" boc boi dau "-" hoac dung dau (ca 2
    ;; bang ma cua chu D co dau: CP1258 1 byte / UTF-8 2 byte), hoac
    ;; ghi day du "COT DIEN" - khong phu thuoc tien to lo dung truoc
    ;; cot dien HA THE: "CD" boc dau "-" 2 phia (giua chuoi), o
    ;; DAU chuoi, hoac o CUOI chuoi (Đ-CĐ/C-CĐ khong co dau "-" sau)
    ((wcmatch k "*-CD-*,*-C?-*,*-C??-*,*-CD,*-C?,*-C??,CD-*,C?-*,C??-*,*C\U+1ED8T \U+0110I\U+1EC6N*") 6)
    ((wcmatch k "*C*NG T*,*TU DIEN*,*T? ?I?N*,TD-*,T?-*,T??-*") 3)
    (t 1)))

;; Loc so met trong ngoac: "DSTA 25(38.4M)" -> 38.4 ; khong co -> nil
(defun tk-met-trong-ngoac (str / i j sub)
  (setq i (vl-string-search "(" str))
  (if i
    (progn
      (setq j (vl-string-search ")" str i))
      (if (and j (> j (+ i 1)))
        (progn
          (setq sub (substr str (+ i 2) (- j i 1)))
          (while (and (> (strlen sub) 0)
                      (not (member (substr sub (strlen sub) 1)
                                   '("0" "1" "2" "3" "4" "5"
                                     "6" "7" "8" "9" "."))))
            (setq sub (substr sub 1 (1- (strlen sub)))))
          (if (> (strlen sub) 0) (distof sub 2)))))))

;; Rong cot theo dong chu dai nhat trong o (don vi = chieu cao chu)
(defun tk-w-chu (str h / mx cur i ch)
  (setq mx 0 cur 0 i 1)
  (while (<= i (strlen str))
    (setq ch (substr str i 1))
    (if (= ch "\n")
      (progn (if (> cur mx) (setq mx cur)) (setq cur 0))
      (setq cur (1+ cur)))
    (setq i (1+ i)))
  (if (> cur mx) (setq mx cur))
  (* h (+ 1.8 (* 0.62 mx))))

;; Tach chuoi theo 1 ky tu phan cach (khong dung regex)
(defun tk-split-str (str sep / i out cur ch)
  (setq out '() cur "" i 1)
  (while (<= i (strlen str))
    (setq ch (substr str i 1))
    (if (= ch sep)
      (progn (setq out (append out (list cur))) (setq cur ""))
      (setq cur (strcat cur ch)))
    (setq i (1+ i)))
  (append out (list cur)))

;; TU DONG BE DONG dau muc BANG DIEN toi da N ky tu/dong (gap tu
;; theo dau cach, khong be giua tu) - de cot LUON GON co dinh bat
;; ke dau muc goc dai bao nhieu; chu tu xuong hang, hang tieu de tu
;; cao len chu KHONG lam cot phinh ra.
(defun tk-wrap-dien (str maxc / segs seg words w cur lines out)
  (setq segs (tk-split-str str "\n"))
  (setq out "")
  (foreach seg segs
    (setq words (tk-split-str seg " "))
    (setq lines '() cur "")
    (foreach w words
      (cond
        ((= cur "") (setq cur w))
        ((<= (+ (strlen cur) 1 (strlen w)) maxc)
         (setq cur (strcat cur " " w)))
        (t (setq lines (append lines (list cur))) (setq cur w))))
    (if (/= cur "") (setq lines (append lines (list cur))))
    (foreach l lines
      (setq out (if (= out "") l (strcat out "\n" l)))))
  out)

;; Rong cot RIENG cho BANG DIEN - gon hon tk-w-chu chung (chieu
;; sang khong dung ham nay nen sua o day khong dung toi chieu sang).
;; He so nho hon vi da mua cac dau muc dai bang xuong dong o noi goi.
(defun tk-w-chu-dien (str h / mx cur i ch)
  (setq mx 0 cur 0 i 1)
  (while (<= i (strlen str))
    (setq ch (substr str i 1))
    (if (= ch "\n")
      (progn (if (> cur mx) (setq mx cur)) (setq cur 0))
      (setq cur (1+ cur)))
    (setq i (1+ i)))
  (if (> cur mx) (setq mx cur))
  (* h (+ 1.2 (* 0.52 mx))))

;; Ten nhom chuan tieng Viet theo loai thiet bi
(defun tk-ten-loai (code)
  (cond ((= code 2) "CỘT TRUNG THẾ")
        ((= code 3) "TỦ CÔNG TƠ")
        ((= code 4) "TBA")
        ((= code 5) "TỦ BÙ")
        ((= code 6) "CỘT ĐIỆN")
        (t "CỘT HẠ THẾ")))

;; Hieu 2 alist so: (a - b) theo tung khoa cua a
(defun tk-alist-hieu (a b)
  (mapcar '(lambda (p)
             (cons (car p)
                   (- (cdr p)
                      (cond ((cdr (assoc (car p) b))) (0.0)))))
          a))

;; Ghi 1 dong tong BANG DIEN (dung chung: tong lo + tong toan bo)
;; dua vao dynamic scope: laylist onglist nhomlist, tk-settext
(defun tk-dien-tong (tbl r nhan kc lx tc caps2 ongs2 nhom2 qd2 rqd2 rhe2 cr2
                     / c v item)
  (tk-settext tbl r 1 nhan)
  (tk-settext tbl r 2 (rtos kc 2 1))
  (tk-settext tbl r 3 (rtos lx 2 1))
  (tk-settext tbl r 4 (rtos tc 2 1))
  (setq c 5)
  (foreach item laylist
    (setq v (cond ((cdr (assoc item caps2))) (0.0)))
    (tk-settext tbl r c (rtos v 2 1))
    (setq c (1+ c)))
  (foreach item onglist
    (setq v (cond ((cdr (assoc item ongs2))) (0.0)))
    (tk-settext tbl r c (rtos v 2 1))
    (setq c (1+ c)))
  (foreach item qdlist
    (setq v (cond ((cdr (assoc item qd2))) (0.0)))
    (tk-settext tbl r c (rtos v 2 1))
    (setq c (1+ c)))
  (tk-settext tbl r c (rtos (cond (cr2) (0.0)) 2 1))
  (setq c (1+ c))
  (foreach item ranhlist
    (setq v (cond ((cdr (assoc item rqd2))) (0.0)))
    (tk-settext tbl r c (rtos v 2 1))
    (setq c (1+ c)))
  (foreach item ranhlist
    (setq v (cond ((cdr (assoc item rhe2))) (0.0)))
    (tk-settext tbl r c (rtos v 2 1))
    (setq c (1+ c)))
  (foreach item nhomlist
    (setq v (cond ((cdr (assoc item nhom2))) (0.0)))
    (tk-settext tbl r c (rtos v 2 0))
    (setq c (1+ c))))

;; Tien to LO cua ten cot: phan truoc dau "-" dau tien, chi nhan
;; khi KHONG chua chu so ("I-D1-A" -> "I"; "TDKCS-02"/"D2-B" -> "")
;; ---------------------------------------------------------------
;; SO LA MA -> SO NGUYEN (dung RIENG cho BANG DIEN)
;; Ban cu tk-lo-rank chi co bang tra I..X nen tu XI tro di bi tra ve
;; 99 -> tk-lo-prefix coi nhu KHONG THUOC LO NAO -> bang dien mat
;; khoi luong cua cac lo tu XI. Ham nay doc DUNG moi so La Ma.
;; CHI nhan cac chu I, V, X, L (toi da LXXXIX = 89) - CO Y KHONG
;; nhan C, D, M de tranh dam vao ma thiet bi cua chinh du an:
;; "CD-1.1" (cot dien), "CTT-01" (cot trung the), neu nhan C/D thi
;; "CD" se bi doc thanh so La Ma 400 va bi hieu nham la ten lo.
;; Tra ve nil neu KHONG phai so La Ma hop le.
;; Lam tron 2 so thap phan (dung cho bang dien - de tong cac dong
;; cong lai dung bang con so hien tren bang, khong le lech do lam
;; tron rieng tung o luc in)
;; Lam tron 1 SO THAP PHAN cho BANG DIEN (user chot: bang chi hien
;; 1 so le cho gon). Lam tron NGAY KHI cong don, khong phai chi luc
;; in -> dong TONG luon = dung tong cac dong dang hien tren bang.
;; Doc DUONG SO nhieu tang o CUOI ten: "II-CD-1.1.1" -> (1 1 1)
(defun tk-duong-so (nm / i sub out cur ch n)
  (setq i (strlen nm))
  (while (and (> i 0) (/= (substr nm i 1) "-")) (setq i (1- i)))
  (setq sub (if (> i 0) (substr nm (1+ i)) nm))
  (setq out '() cur "" n 1)
  (while (<= n (strlen sub))
    (setq ch (substr sub n 1))
    (if (and (>= ch "0") (<= ch "9"))
      (setq cur (strcat cur ch))
      (if (/= cur "")
        (progn (setq out (append out (list (atoi cur)))) (setq cur ""))))
    (setq n (1+ n)))
  (if (/= cur "") (setq out (append out (list (atoi cur)))))
  out)

;; So sanh 2 duong so: (1 1 1) < (1 2) -> nhanh con di TRUOC than chinh
(defun tk-duong-nho-hon (a b / pa pb kq xong)
  (setq pa (tk-duong-so a) pb (tk-duong-so b) xong nil kq nil)
  (while (and (not xong) (or pa pb))
    (cond
      ((null pa) (setq kq T   xong T))
      ((null pb) (setq kq nil xong T))
      ((< (car pa) (car pb)) (setq kq T   xong T))
      ((> (car pa) (car pb)) (setq kq nil xong T))
      (T (setq pa (cdr pa) pb (cdr pb)))))
  kq)

(defun tk-lam-tron1 (v)
  (/ (fix (+ (* v 10.0) (if (< v 0) -0.5 0.5))) 10.0))

(defun tk-lam-tron2 (v)
  (/ (fix (+ (* v 100.0) (if (< v 0) -0.5 0.5))) 100.0))

(defun tk-lama-so (s / i ch v prev tong ok)
  (setq s (strcase s))
  (if (= s "")
    nil
    (progn
      (setq i (strlen s) tong 0 prev 0 ok T)
      (while (and ok (> i 0))
        (setq ch (substr s i 1))
        (setq v (cond ((= ch "I") 1)
                      ((= ch "V") 5)
                      ((= ch "X") 10)
                      ((= ch "L") 50)
                      (T nil)))
        (if (null v)
          (setq ok nil)
          (if (< v prev)
            (setq tong (- tong v))
            (progn (setq tong (+ tong v)) (setq prev v))))
        (setq i (1- i)))
      (if (and ok (> tong 0)) tong nil))))

;; Thu tu lo cho BANG DIEN: doc dung moi so La Ma (I, II, ... XV,
;; XX, XXX, L...); khong phai so La Ma -> 999 (xep cuoi)
(defun tk-lo-rank-dien (p / v)
  (setq v (tk-lama-so p))
  (if v v 999))

;; Tien to lo cho BANG DIEN: phan truoc dau "-" dau tien, chi nhan
;; khi la SO LA MA hop le (khong gioi han I..X nhu ban cu)
(defun tk-lo-prefix-dien (nm / i sub)
  (setq i (vl-string-search "-" nm))
  (if (and i (> i 0))
    (progn
      (setq sub (strcase (substr nm 1 i)))
      (if (tk-lama-so sub) sub ""))
    ""))

(defun tk-lo-prefix (nm / i sub)
  ;; CHIEU SANG - dung DUNG PHUONG PHAP nhu phan dien (tk-lama-so):
  ;; truoc day chi co bang tra cung I..X nen cot den tu lo XI tro di
  ;; bi coi la KHONG THUOC LO NAO -> bang mat khoi luong cua cac lo do.
  ;; Nay doc dung moi so La Ma I..LXXXIX (89). Van CO Y khong nhan
  ;; C/D/M de khong dam vao ma thiet bi (CD, CTT, TDKCS...).
  (setq i (vl-string-search "-" nm))
  (if (and i (> i 0))
    (progn
      (setq sub (strcase (substr nm 1 i)))
      (if (tk-lama-so sub) sub ""))
    ""))

;; Thu tu lo: so La Ma quen thuoc truoc, la -> xep sau theo ABC
(defun tk-lo-rank (p / v)
  ;; CHIEU SANG - xep thu tu lo theo GIA TRI SO cua so La Ma
  ;; (khong phai ABC, de I < II < IV < IX < X < XI < XV < XX < XL < L)
  (setq v (tk-lama-so p))
  (if v v 999))

;; Ghi 1 DONG TONG (dung chung: tong tung lo + tong toan bo)
;; Dua vao dynamic scope: row_h text_h num_* tu lenh chinh
(defun tk-dong-tong (tbl row nhan vd vg vc lcap lhdpe lqd lrqd lrhe lpole vcab
                     vcr
                     / k cnt_k)
  (vla-setRowHeight tbl row (* row_h 1.25))
  (tk-settext tbl row 1 nhan)
  (tk-settext tbl row 2 (rtos vd 2 1))
  (tk-settext tbl row 3 (rtos vg 2 1))
  (tk-settext tbl row 4 (rtos vc 2 1))
  (setq k 0)
  (while (< k num_cap_types)
    (tk-settext tbl row (+ 5 k) (rtos (nth k lcap) 2 1))
    (setq k (1+ k)))
  (setq k 0)
  (while (< k num_hdpe_types)
    (tk-settext tbl row (+ 5 num_cap_types k) (rtos (nth k lhdpe) 2 1))
    (setq k (1+ k)))
  (setq k 0)
  (while (< k num_qd_types)
    (tk-settext tbl row (+ 5 num_cap_types num_hdpe_types k)
                 (rtos (nth k lqd) 2 1))
    (setq k (1+ k)))
  (tk-settext tbl row
    (+ 5 num_cap_types num_hdpe_types num_qd_types)
    (rtos (cond (vcr) (0.0)) 2 1))
  (setq k 0)
  (while (< k num_ranh_types)
    (tk-settext tbl row
      (+ 6 num_cap_types num_hdpe_types num_qd_types k)
      (rtos (nth k lrqd) 2 1))
    (setq k (1+ k)))
  (setq k 0)
  (while (< k num_ranh_types)
    (tk-settext tbl row
      (+ 6 num_cap_types num_hdpe_types num_qd_types num_ranh_types k)
      (rtos (nth k lrhe) 2 1))
    (setq k (1+ k)))
  (setq k 0)
  (while (< k num_pole_types)
    (setq cnt_k (nth k lpole))
    (tk-settext tbl row (+ 6 num_cap_types num_hdpe_types num_qd_types (* 2 num_ranh_types) k)
                 (itoa (fix cnt_k)))
    (setq k (1+ k)))
  (tk-settext tbl row (+ 6 num_cap_types num_hdpe_types num_qd_types (* 2 num_ranh_types) num_pole_types) (itoa (fix vcab))))

;; member dung EQ (dinh danh thuc the), khong dung equal nhu member
(defun tk-mem-eq (x lst / co)
  (foreach it lst (if (eq it x) (setq co T)))
  co)

;; khoang NHANH (dien): dau cuoi la thiet bi duoi chu (02A) -> CR/CR-cap
;; chi duoc nham vao day, giong "khoang nhanh" ben chieu sang
(defun tk-branch-p-dien (pr)
  (/= (tk-bo-duoi-nhanh (car (caddr pr))) (car (caddr pr))))

;; Tim khoang GAN NHAT theo CA DOAN (nhu tk-nearest-pair ben chieu
;; sang) - KHONG dung trung diem nua: nhanh NGAN dung canh truc DAI
;; co trung diem gan hon trung diem truc -> cuop nham ghi chu cua
;; truc. Do toi CA DOAN moi phan biet dung "ghi chu nam TREN duong
;; nao". spans2 co the la spans day du hoac list da loc nhanh rieng.
(defun tk-nearest-span-dien (pt spans2 / b1i b1d pa pb dqd pair)
  (setq b1i nil b1d 1e9)
  (foreach pair spans2
    (setq pa (caddr (cadr pair)) pb (caddr (caddr pair)))
    (setq dqd (min (Point-To-Segment-Dist pt pa pb)
                   (distance pt pa) (distance pt pb)))
    (if (< dqd b1d) (setq b1d dqd b1i (car pair))))
  (list b1i b1d))

;; "I-TD-02A" -> "I-TD-02": cat cac CHU CAI ngay sau cum so dau.
;; Ten khac sau khi cat = thiet bi NHANH (treo vao node cung ten cat)
(defun tk-bo-duoi-nhanh (nm / i n ch st en)
  (setq i 1 n (strlen nm) st nil en nil)
  (while (and (<= i n) (null en))
    (setq ch (substr nm i 1))
    (if (and (null st) (>= ch "0") (<= ch "9"))
      (setq st i))
    (if (and st (or (< ch "0") (> ch "9")))
      (setq en i))
    (setq i (1+ i)))
  (if en
    (progn
      (setq i en)
      (while (and (<= i n)
                  (setq ch (substr nm i 1))
                  (or (and (>= ch "A") (<= ch "Z"))
                      (and (>= ch "a") (<= ch "z"))))
        (setq i (1+ i)))
      (strcat (substr nm 1 (1- en)) (substr nm i)))
    nm))

;; Bac nguon dien: so NHO = nguon hon (dung dau mui ten khoang)
(defun tk-rank-nguon (code)
  (cond ((= code 2) 0)   ; cot trung the (luoi cap ve)
        ((= code 4) 1)   ; TBA - xuat tuyen ha the
        ((= code 5) 2)   ; tu bu (ngay sau TBA - theo bang chuan)
        ((= code 3) 3)   ; tu dien / cong to
        (t 4)))          ; cot ha the

;; Ten loai ASCII (dung cho command line)
(defun tk-loai-ascii (code)
  (cond ((= code 2) "trung the")
        ((= code 3) "cong to")
        ((= code 4) "TBA")
        ((= code 5) "tu bu")
        ((= code 6) "cot dien")
        (t "ha the")))

(defun tk-lx-dien (code)
  (cond ((= code 2) *TK-DIEN-TT*)
        ((= code 3) *TK-DIEN-CT*)
        ((= code 4) *TK-DIEN-TBA*)
        ((= code 5) *TK-DIEN-TB*)
        ;; loai 6 (cot dien "CD") dung CHUNG so cau 1 - cau 1 tu dau
        ;; da la "COT DIEN HA THE", khong can hoi rieng
        (t *TK-DIEN-HT*)))

;; Chieu dai doc tuyen tai 1 diem tren pline (ben vung voi sai so):
;; uu tien getParamAtPoint -> getDistAtParam, du phong getDistAtPoint
(defun tk-dist-at (obj2 q / pr)
  (setq pr (vl-catch-all-apply 'vlax-curve-getParamAtPoint (list obj2 q)))
  (if (and pr (not (vl-catch-all-error-p pr)))
    (vlax-curve-getDistAtParam obj2 pr)
    (progn
      (setq pr (vl-catch-all-apply 'vlax-curve-getDistAtPoint (list obj2 q)))
      (if (and pr (not (vl-catch-all-error-p pr))) pr nil))))

(defun tk-inc (alst key val / cur)
  (if (setq cur (assoc key alst))
    (subst (cons key (+ (cdr cur) val)) cur alst)
    (cons (cons key val) alst)))

;; Nhan dien ghi chu ong QUA DUONG: chuoi co "Q" + "D<so>" va KHONG phai DSTA
;; Vi du "QD D60 (12m)", "QD D90", "QD D110" -> tra ve "D60"/"D90"/"D110"
(defun Get-QD-Type (raw / cl p i c num)
  (setq cl (Clean-CAD-String raw))
  (if (or (vl-string-search "DSTA" cl)
          (not (vl-string-search "Q" cl)))
    nil
    (progn
      (setq p nil i 1)
      (while (and (null p) (< i (strlen cl)))
        (if (and (= (substr cl i 1) "D")
                 (>= (substr cl (1+ i) 1) "0")
                 (<= (substr cl (1+ i) 1) "9"))
          (setq p i)
          (setq i (1+ i))))
      (if p
        (progn
          (setq i (1+ p) num "")
          (while (and (<= i (strlen cl))
                      (>= (setq c (substr cl i 1)) "0")
                      (<= c "9"))
            (setq num (strcat num c))
            (setq i (1+ i)))
          (if (/= num "") (strcat "D" num) nil))
        nil))))

;; Lay NGUYEN VAN ten user ghi (giu dau tieng Viet / ma \U+):
;; - cat bo phan "(...)" chua so met
;; - bo cum so dung RIENG o cuoi (la so met kieu "QD D110 15")
;;   nhung giu so dinh lien chu ("D60" khong bi cat)
(defun Get-QD-Name (raw fallback / s p tail i c alldigit)
  (setq s raw)
  (if (setq p (vl-string-search "(" s))
    (setq s (substr s 1 p)))
  (setq s (vl-string-right-trim " " (vl-string-left-trim " " s)))
  (if (setq p (vl-string-position (ascii " ") s nil T))
    (progn
      (setq tail (substr s (+ p 2))
            alldigit (> (strlen tail) 0)
            i 1)
      (while (<= i (strlen tail))
        (setq c (substr tail i 1))
        (if (not (or (and (>= c "0") (<= c "9")) (= c ".") (= c ",")))
          (setq alldigit nil))
        (setq i (1+ i)))
      (if alldigit
        (setq s (vl-string-right-trim " " (substr s 1 p))))))
  (if (= s "") (strcat "QD " fallback) s))

;; Chuan hoa ten de GOM NHOM (so sanh): in hoa + don khoang trang
(defun tk-qd-norm (s / out i c prev)
  (setq s (strcase s) out "" prev nil i 1)
  (while (<= i (strlen s))
    (setq c (substr s i 1))
    (if (= c " ")
      (if (not prev) (setq out (strcat out " ") prev T))
      (setq out (strcat out c) prev nil))
    (setq i (1+ i)))
  (vl-string-right-trim " " (vl-string-left-trim " " out)))

;; Ghi chu LOAI RANH: bat dau bang "R", co chu so, khong phai DSTA/QD
(defun Get-Ranh-P (raw / cl)
  (setq cl (vl-string-left-trim " " (Clean-CAD-String raw)))
  (and (> (strlen cl) 0)
       (= (substr cl 1 1) "R")
       (Extract-All-Nums cl)
       (not (vl-string-search "DSTA" cl))
       (not (Get-QD-Type raw))))

;; Doc 1 ghi chu loai ranh: tra ve (diem ten-nguyen-van) hoac nil
(defun Parse-Ranh-Obj (obj / txt pt)
  (if (vlax-property-available-p obj 'TextOverride)
    (setq txt (vla-get-TextOverride obj))
    (setq txt ""))
  (if (or (= txt "") (= txt "<>"))
    (if (vlax-property-available-p obj 'TextString)
      (setq txt (vla-get-TextString obj))))
  (if (and txt (/= txt "") (Get-Ranh-P txt))
    (progn
      (if (member (vla-get-ObjectName obj)
                  '("AcDbRotatedDimension" "AcDbAlignedDimension"))
        (setq pt (vlax-safearray->list (vlax-variant-value (vla-get-TextPosition obj))))
        (setq pt (vlax-safearray->list (vlax-variant-value (vla-get-InsertionPoint obj)))))
      (list pt (Get-QD-Name txt "")))
    nil))

;; Doc 1 ghi chu QD: tra ve (chieu-dai diem ten-ong) hoac nil
;; Chieu dai: DIM -> so do that; TEXT -> so trong ngoac hoac so thu 2
(defun Parse-QD-Obj (obj / txt pt qd_type l)
  (if (vlax-property-available-p obj 'TextOverride)
    (setq txt (vla-get-TextOverride obj))
    (setq txt ""))
  (if (or (= txt "") (= txt "<>"))
    (if (vlax-property-available-p obj 'TextString)
      (setq txt (vla-get-TextString obj))))
  (setq qd_type (Get-QD-Type txt))
  (if qd_type
    (progn
      (if (member (vla-get-ObjectName obj)
                  '("AcDbRotatedDimension" "AcDbAlignedDimension"))
        (setq pt (vlax-safearray->list (vlax-variant-value (vla-get-TextPosition obj))))
        (setq pt (vlax-safearray->list (vlax-variant-value (vla-get-InsertionPoint obj)))))
      (setq l (Get-Span-Length obj txt))
      (if (and l (> l 0))
        (list l pt (Get-QD-Name txt qd_type))
        nil))
    nil))

;; So CUOI CUNG trong chuoi = tiet dien cap ("DSTA 4x16" -> 16)
(defun tk-last-num (str / i len ch num out)
  (setq i 1 len (strlen str) num "" out nil)
  (while (<= i len)
    (setq ch (substr str i 1))
    (if (and (>= ch "0") (<= ch "9"))
      (setq num (strcat num ch))
      (progn
        (if (/= num "") (setq out num))
        (setq num "")))
    (setq i (1+ i)))
  (if (/= num "") (setq out num))
  (if out (atoi out) 0))

;; Tiet dien loi pha chinh: SO ngay sau chu "x" dau tien dang so-x-so
;; ("3x25+1x16" -> 25); khong co "x" -> so cuoi ("DSTA 16" -> 16)
(defun tk-tietdien (str / cl i len prv nxt val num)
  (setq cl (strcase str) val 0)
  ;; (1) co duoi don vi "MM2" -> tiet dien = SO NGAY TRUOC no
  (setq i (vl-string-search "MM2" cl))
  (if i
    (progn
      (setq num "")
      (while (and (> i 0)
                  (>= (substr cl i 1) "0") (<= (substr cl i 1) "9"))
        (setq num (strcat (substr cl i 1) num)
              i   (1- i)))
      (if (/= num "") (setq val (atoi num)))))
  ;; (2) dang so-x-so -> so sau chu X dau tien (loi pha chinh)
  (if (= val 0)
    (progn
      (setq i 2 len (strlen cl))
      (while (and (= val 0) (< i len))
        (if (and (= (substr cl i 1) "X")
                 (>= (setq prv (substr cl (1- i) 1)) "0") (<= prv "9")
                 (>= (setq nxt (substr cl (1+ i) 1)) "0") (<= nxt "9"))
          (setq val (atoi (substr cl (1+ i)))))
        (setq i (1+ i)))))
  ;; (3) con lai -> so cuoi trong ten
  (if (> val 0) val (tk-last-num cl)))

;; Suy ong HDPE tu TIET DIEN cap theo BANG TRA CHUAN
(defun Get-HDPE-Type (cap_short / sz)
  (setq sz (tk-tietdien cap_short))
  (cond
    ((<= sz 0)   "NONE")
    ((<= sz 6)   "HDPE 40/30")
    ((<= sz 16)  "HDPE 50/40")
    ((<= sz 35)  "HDPE 65/50")
    ((<= sz 70)  "HDPE 85/65")
    ((<= sz 120) "HDPE 105/80")
    ((<= sz 185) "HDPE 110/90")
    ((<= sz 240) "HDPE 130/100")
    (t           "HDPE 160/125")))

(defun Clean-CAD-String (str / i len ch out pos)
  (setq str (strcase str))
  (while (setq pos (vl-string-search "\\P" str))
    (setq str (strcat (substr str 1 pos) " " (substr str (+ pos 3)))))
  (setq out "" i 1 len (strlen str))
  (while (<= i len)
    (setq ch (substr str i 1))
    (cond
      ((member ch '("{" "}")) nil)
      ((= ch "\\")
       (setq i (1+ i))
       (while (and (<= i len) (not (member (substr str i 1) '(";" " "))))
         (setq i (1+ i))))
      (t (setq out (strcat out ch))))
    (setq i (1+ i)))
  out)

(defun Extract-All-Nums (str / i len ch current_num num_list inside_num dot_found)
  (setq str (Clean-CAD-String str))
  (setq i 1 len (strlen str) current_num "" num_list '() inside_num nil dot_found nil)
  (while (<= i len)
    (setq ch (substr str i 1))
    (cond
      ((and (>= ch "0") (<= ch "9"))
       (setq current_num (strcat current_num ch))
       (setq inside_num t))
      ((and (= ch ".") inside_num (not dot_found))
       (setq current_num (strcat current_num ch))
       (setq dot_found t))
      (t
       (if inside_num
         (progn
           (if (/= current_num "")
             (setq num_list (cons (atof current_num) num_list)))
           (setq current_num "" inside_num nil dot_found nil)))))
    (setq i (1+ i)))
  (if (and inside_num (/= current_num ""))
    (setq num_list (cons (atof current_num) num_list)))
  (reverse num_list))

(defun Get-DSTA-Spec (raw_str / clean_str pos_dsta pos_mm2 i len num_str)
  (setq clean_str (Clean-CAD-String raw_str))
  (setq pos_dsta (vl-string-search "DSTA" clean_str))
  (cond
    (pos_dsta
     (setq i (+ pos_dsta 5) len (strlen clean_str) num_str "")
     (while (and (<= i len)
                 (not (and (>= (substr clean_str i 1) "0") (<= (substr clean_str i 1) "9"))))
       (setq i (1+ i)))
     (while (and (<= i len) (>= (substr clean_str i 1) "0") (<= (substr clean_str i 1) "9"))
       (setq num_str (strcat num_str (substr clean_str i 1)))
       (setq i (1+ i)))
     (if (/= num_str "") (strcat "DSTA " num_str "mm2") nil))
    ((setq pos_mm2 (vl-string-search "MM2" clean_str))
     (setq i pos_mm2 num_str "")
     (while (and (> i 0)
                 (not (and (>= (substr clean_str i 1) "0") (<= (substr clean_str i 1) "9"))))
       (setq i (1- i)))
     (while (and (> i 0) (>= (substr clean_str i 1) "0") (<= (substr clean_str i 1) "9"))
       (setq num_str (strcat (substr clean_str i 1) num_str))
       (setq i (1- i)))
     (if (/= num_str "") (strcat "DSTA " num_str "mm2") nil))
    (t nil)))

(defun Get-Span-Length (obj raw_str / clean_str pos_open pos_close len_str num_list)
  (if (member (vla-get-ObjectName obj) '("AcDbRotatedDimension" "AcDbAlignedDimension"))
    (vla-get-Measurement obj)
    (progn
      (setq clean_str (Clean-CAD-String raw_str))
      (setq pos_open  (vl-string-search "(" clean_str))
      (setq pos_close (vl-string-search ")" clean_str))
      (cond
        ((and pos_open pos_close (> pos_close pos_open))
         (setq len_str (substr clean_str (+ pos_open 2) (- pos_close pos_open 1)))
         (Get-First-Num len_str))
        (t
         (setq num_list (Extract-All-Nums clean_str))
         (if (>= (length num_list) 2) (nth 1 num_list) 0.0))))))

(defun Parse-Cap-Obj (obj / txt pt cap_spec l_dim)
  (if (vlax-property-available-p obj 'TextOverride)
    (setq txt (vla-get-TextOverride obj))
    (setq txt ""))
  (if (or (= txt "") (= txt "<>"))
    (if (vlax-property-available-p obj 'TextString)
      (setq txt (vla-get-TextString obj))))
  (if (member (vla-get-ObjectName obj) '("AcDbRotatedDimension" "AcDbAlignedDimension"))
    (setq pt (vlax-safearray->list (vlax-variant-value (vla-get-TextPosition obj))))
    (setq pt (vlax-safearray->list (vlax-variant-value (vla-get-InsertionPoint obj)))))
  (setq cap_spec (Get-DSTA-Spec txt))
  (setq l_dim (Get-Span-Length obj txt))
  (if cap_spec
    (list (cond ((and l_dim (> l_dim 0)) l_dim) (0.0)) pt cap_spec)
    nil))

(defun Point-To-Segment-Dist (pt pt1 pt2 / x0 y0 x1 y1 x2 y2 dx dy len_sq u px py)
  (setq x0 (car pt)  y0 (cadr pt)
        x1 (car pt1) y1 (cadr pt1)
        x2 (car pt2) y2 (cadr pt2)
        dx (- x2 x1) dy (- y2 y1)
        len_sq (+ (* dx dx) (* dy dy)))
  (if (zerop len_sq)
    (distance (list x0 y0) (list x1 y1))
    (progn
      (setq u (/ (+ (* (- x0 x1) dx) (* (- y0 y1) dy)) len_sq))
      (if (and (>= u -0.15) (<= u 1.15))
        (progn
          (setq px (+ x1 (* u dx)) py (+ y1 (* u dy)))
          (distance (list x0 y0) (list px py)))
        1e9))))



;; Doc DUONG SO day du tu ten cot: "II-D2.2.1-C" -> (2 2 1)
(defun Get-Pole-Path (str / i len ch num path searching)
  (setq str (strcase str) i 1 len (strlen str) path '() searching T)
  (while (and (<= i len) searching)
    (if (and (>= (setq ch (substr str i 1)) "0") (<= ch "9"))
      (setq searching nil)
      (setq i (1+ i))))
  (while (and (<= i len)
              (>= (setq ch (substr str i 1)) "0") (<= ch "9"))
    (setq num "")
    (while (and (<= i len)
                (>= (setq ch (substr str i 1)) "0") (<= ch "9"))
      (setq num (strcat num ch))
      (setq i (1+ i)))
    (setq path (cons (atoi num) path))
    (if (and (< i len)
             (= (substr str i 1) ".")
             (>= (substr str (1+ i) 1) "0")
             (<= (substr str (1+ i) 1) "9"))
      (setq i (1+ i))))
  (reverse path))

;; n phan tu dau cua danh sach
(defun tk-take (lst n / out)
  (setq out '())
  (while (and lst (> n 0))
    (setq out (cons (car lst) out)
          lst (cdr lst)
          n   (1- n)))
  (reverse out))

;; par co phai duong so CHA truc tiep cua chi khong ((2 2) cha cua (2 2 1))
(defun tk-path-child-p (par chi)
  (and (= (length chi) (1+ (length par)))
       (equal par (tk-take chi (length par)))))

;; Sinh cac khoang cua CAY con duoi 1 nut theo thu tu uu tien:
;; den nut nao thi boc het nhanh con cua nut do TRUOC roi moi sang nut ke
(defun tk-emit-subtree (node allp / kids prev out kid)
  (setq kids (vl-sort
               (vl-remove-if-not
                 '(lambda (x) (tk-path-child-p (nth 5 node) (nth 5 x)))
                 allp)
               '(lambda (a b) (< (last (nth 5 a)) (last (nth 5 b))))))
  (setq out '() prev node)
  (foreach kid kids
    (setq out (append out (list (list prev kid))))
    (setq out (append out (tk-emit-subtree kid allp)))
    (setq prev kid))
  out)

(defun Get-Pole-Num (str / i len ch res dot_found searching)
  (setq str (strcase str))
  (setq res "" dot_found nil searching t i 1 len (strlen str))
  (while (and (<= i len) searching)
    (setq ch (substr str i 1))
    (if (and (>= ch "0") (<= ch "9"))
      (while (and (<= i len) searching)
        (setq ch (substr str i 1))
        (cond
          ((and (>= ch "0") (<= ch "9"))
           (setq res (strcat res ch))
           (setq i (1+ i)))
          ((and (= ch ".") (not dot_found))
           (setq res (strcat res ch))
           (setq dot_found t)
           (setq i (1+ i)))
          (t (setq searching nil))))
      (setq i (1+ i))))
  (if (/= res "") (atof res) 0.0))

(defun Set-Nth (idx val lst / i res)
  (setq i 0 res '())
  (foreach item lst
    (if (= i idx)
      (setq res (cons val res))
      (setq res (cons item res)))
    (setq i (1+ i)))
  (reverse res))

(defun Get-First-Num (str / num_list)
  (setq num_list (Extract-All-Nums str))
  (if num_list (car num_list) 0.0))

;; ============================================================
;; T-KIEMTRA - soi vung chon: may dang NHIN THAY nhung gi
;; ============================================================
(defun tk-cmd-kiemtra-vung-debug (/ ss i ent obj name cnts cur curves blocks att atts info
                      v_ten pt k pe plv q1 q2)
  (princ "\n[T-KIEMTRA/VUNG] Quet chon vung tuyen can kiem tra: ")
  (setq ss (ssget))
  (if ss
    (progn
      (setq cnts '() curves '() blocks '() i 0)
      (repeat (sslength ss)
        (setq ent  (ssname ss i)
              obj  (vlax-ename->vla-object ent)
              name (vla-get-ObjectName obj))
        (setq cur  (assoc name cnts))
        (setq cnts (if cur
                     (subst (cons name (1+ (cdr cur))) cur cnts)
                     (cons (cons name 1) cnts)))
        (if (member name '("AcDbPolyline" "AcDb2dPolyline" "AcDb3dPolyline"
                           "AcDbLine" "AcDbSpline" "AcDbArc"))
          (progn
            (setq pe  (if (member name '("AcDbLine" "AcDbSpline" "AcDbArc"))
                        1
                        (fix (+ 0.5 (vlax-curve-getEndParam obj))))
                  plv (vlax-curve-getDistAtParam obj
                        (vlax-curve-getEndParam obj))
                  q1  (vlax-curve-getStartPoint obj)
                  q2  (vlax-curve-getEndPoint obj))
            (setq curves (cons (list name pe plv q1 q2) curves))))
        (if (and (= name "AcDbBlockReference")
                 (= (vla-get-HasAttributes obj) :vlax-true))
          (progn
            ;; V115: dung cung bo doc TAG voi T-THDIEN, khong gioi han TEN_COT.
            (setq atts  (tk-get-atts-safe obj)
                  info  (if atts (tk-dien-read-labels obj atts) nil)
                  v_ten (if (and info (car info)) (car info) ""))
            (setq pt (vlax-safearray->list
                       (vlax-variant-value (vla-get-InsertionPoint obj))))
            (setq blocks (cons (list v_ten pt) blocks))))
        (setq i (1+ i)))
      (princ "\n===== KET QUA KIEM TRA VUNG CHON =====")
      (princ "\n1) SO LUONG DOI TUONG THEO LOAI:")
      (foreach cur (reverse cnts)
        (princ (strcat "\n   " (car cur) " : " (itoa (cdr cur)))))
      (princ (strcat "\n2) DUONG TUYEN TIM THAY: " (itoa (length curves))))
      (setq k 0)
      (foreach cur (reverse curves)
        (if (< k 15)
          (princ (strcat "\n   [" (nth 0 cur) "] "
                         (itoa (nth 1 cur)) " doan, dai "
                         (rtos (nth 2 cur) 2 2)
                         " | dau (" (rtos (car (nth 3 cur)) 2 1) ", "
                         (rtos (cadr (nth 3 cur)) 2 1) ") cuoi ("
                         (rtos (car (nth 4 cur)) 2 1) ", "
                         (rtos (cadr (nth 4 cur)) 2 1) ")")))
        (setq k (1+ k)))
      (if (> (length curves) 15)
        (princ (strcat "\n   ... va " (itoa (- (length curves) 15)) " duong nua")))
      (princ (strcat "\n3) BLOCK CO THUOC TINH: " (itoa (length blocks))))
      (foreach cur (reverse blocks)
        (princ (strcat "\n   ["
                       (if (= (car cur) "") "(THIEU DEVICE_ID/TEN_*)" (car cur))
                       "] chen (" (rtos (car (cadr cur)) 2 1) ", "
                       (rtos (cadr (cadr cur)) 2 1) ")")))
      (princ "\n===== HET - bam F2 mo cua so chu, chup gui de chan doan ====="))
    (princ "\nKhong chon duoc doi tuong nao."))
  (princ))

;; ============================================================
;; T-DIMNET - quet chon cac duong net, tu dim TUNG DOAN THANG
;; ============================================================
(defun tkdim-1doan (msp q1 q2 lay2 sty / L wp item tmp)
  ;; chuan hoa huong doan (trai->phai; thang dung: duoi->tren)
  ;; de duong dim LUON nam mot phia thong nhat, bat ke chieu ve
  (if (or (< (car q2) (car q1))
          (and (equal (car q1) (car q2) 1e-6)
               (< (cadr q2) (cadr q1))))
    (progn (setq tmp q1) (setq q1 q2) (setq q2 tmp)))
  (setq L (distance q1 q2))
  (if (> L 0.001)
    (progn
      (setq wp (list (- (* 0.5 (+ (car q1) (car q2)))
                        (* *TK-DimCach* (/ (- (cadr q2) (cadr q1)) L)))
                     (+ (* 0.5 (+ (cadr q1) (cadr q2)))
                        (* *TK-DimCach* (/ (- (car q2) (car q1)) L)))
                     0.0))
      ;; kieu dim da duoc DAT ACTIVE truoc khi ve -> dim thuan kieu,
      ;; khong dinh override cua kieu cu
      (setq item (vla-AddDimAligned msp
                   (vlax-3d-point q1)
                   (vlax-3d-point q2)
                   (vlax-3d-point wp)))
      (vla-put-Layer item lay2)
      T)))

(defun c:T-DIMNET (/ doc msp ss i ent obj name lay lay2 sty pe k q1 q2 d cnt)
  (princ "\n[T-DIMNET] Quet chon cac duong net can ghi kich thuoc: ")
  (setq ss (ssget '((0 . "LWPOLYLINE,POLYLINE,LINE,SPLINE"))))
  (if ss
    (progn
      (setq doc (vla-get-ActiveDocument (vlax-get-acad-object))
            msp (vla-get-ModelSpace doc))
      (if (setq d (vlax-ldata-get "TK_THONGSO" "DIMCACH"))
        (setq *TK-DimCach* d))
      (initget 4)
      (setq d (getreal (strcat "\nDuong dim cach net (m) <"
                               (rtos *TK-DimCach* 2 2) ">: ")))
      (if d (setq *TK-DimCach* d))
      (vlax-ldata-put "TK_THONGSO" "DIMCACH" *TK-DimCach*)
      (if (not (tblsearch "LAYER" "TK-KICH-THUOC"))
        (vla-Add (vla-get-Layers doc) "TK-KICH-THUOC"))
      ;; luu kieu dim hien hanh de khoi phuc cuoi lenh
      (setq d (vla-get-ActiveDimStyle doc))
      (setq i 0 cnt 0)
      (repeat (sslength ss)
        (setq ent  (ssname ss i)
              obj  (vlax-ename->vla-object ent)
              name (vla-get-ObjectName obj)
              lay  (vla-get-Layer obj)
              ;; kieu dim TRUNG TEN lop net; dim dat tren lop net cap
              sty  (if (tblsearch "DIMSTYLE" lay) lay nil)
              lay2 (if (Get-DSTA-Spec lay) lay "TK-KICH-THUOC"))
        ;; DAT kieu dim trung ten net lam ACTIVE truoc khi ve
        (if sty
          (vl-catch-all-apply
            '(lambda ()
               (vla-put-ActiveDimStyle doc
                 (vla-Item (vla-get-DimStyles doc) sty))))
          (princ (strcat "\n[LUU Y] net \"" lay
                         "\" chua co dim style trung ten"
                         " -> dung kieu hien hanh.")))
        (if (member name '("AcDbLine" "AcDbSpline"))
          ;; LINE/SPLINE: ca doi tuong 1 doan
          (if (tkdim-1doan msp
                           (vlax-curve-getStartPoint obj)
                           (vlax-curve-getEndPoint obj)
                           lay2 sty)
            (setq cnt (1+ cnt)))
          ;; POLYLINE: moi doan giua 2 dinh 1 dim
          (progn
            (setq pe (fix (+ 0.5 (vlax-curve-getEndParam obj)))
                  k  (fix (vlax-curve-getStartParam obj)))
            (while (< k pe)
              (setq q1 (vlax-curve-getPointAtParam obj k)
                    q2 (vlax-curve-getPointAtParam obj (1+ k)))
              (if (tkdim-1doan msp q1 q2 lay2 sty)
                (setq cnt (1+ cnt)))
              (setq k (1+ k)))))
        (setq i (1+ i)))
      ;; khoi phuc kieu dim hien hanh truoc khi chay lenh
      (vl-catch-all-apply '(lambda () (vla-put-ActiveDimStyle doc d)))
      (princ (strcat "\n>> Da ghi " (itoa cnt)
                     " kich thuoc theo tung doan net (kieu dim trung"
                     " ten lop net neu co)."))))
  (princ))

;; ============================================================
;; TKDEM - dem block theo TEN + do duong theo LOP (van nang)
;; ============================================================
;; Bo SO DUOI + dau cach/gach cuoi ten ("Tu cong to 1" -> "Tu cong to")
(defun tk-bo-so-duoi (str / len ch)
  (setq len (strlen str))
  (while (and (> len 0)
              (setq ch (substr str len 1))
              (or (and (>= ch "0") (<= ch "9"))
                  (member ch '(" " "-" "_" "."))))
    (setq len (1- len)))
  (substr str 1 len))

(defun tk-dem-vannang (ss h / doc msp i ent obj name key blks laylen laylx
                         laycnt eqpts plv pt tbl nb nl nr r item cnt atts q1 q2
                         lx best_d dqd code ong sd sl st sc sr)
  (if ss
    (progn
      (setq doc (vla-get-ActiveDocument (vlax-get-acad-object))
            msp (vla-get-ModelSpace doc)
            blks '() laylen '() laylx '() laycnt '() eqpts '() i 0)
      ;; font Arial dong bo bang chieu sang
      (if (null (tblsearch "STYLE" *TK-TextStyle*))
        (progn
          (setq atts (vla-add (vla-get-TextStyles doc) *TK-TextStyle*))
          (vla-SetFont atts "Arial" :vlax-false :vlax-false 0 0)
          (vla-put-Height atts 0.0)))
      ;; thong so len xuong dien: chua khai thi hoi
      (if (null (tk-doc-thongso-dien)) (tk-nhap-thongso-dien))
      (princ (strcat "\n>> Len xuong: ha the " (rtos *TK-DIEN-HT* 2 2)
                     "m | trung the " (rtos *TK-DIEN-TT* 2 2)
                     "m | cong to " (rtos *TK-DIEN-CT* 2 2)
                     "m | TBA " (rtos *TK-DIEN-TBA* 2 2)
                     "m  (doi: lenh T-THONGSO)"))
      ;; VONG 1: block (dem + lam diem thiet bi), duong (dai + 2 dau)
      (repeat (sslength ss)
        (setq ent  (ssname ss i)
              obj  (vlax-ename->vla-object ent)
              name (vla-get-ObjectName obj))
        (if (= name "AcDbBlockReference")
          (progn
            (setq key nil)
            (if (= (vla-get-HasAttributes obj) :vlax-true)
              (progn
                (setq atts (vl-catch-all-apply
                             '(lambda (o)
                                (vlax-safearray->list
                                  (vlax-variant-value (vla-GetAttributes o))))
                             (list obj)))
                (if (and (not (vl-catch-all-error-p atts)) atts)
                  (foreach item atts
                    (if (and (null key)
                             (/= (vla-get-TextString item) ""))
                      (setq key (tk-bo-so-duoi
                                  (vla-get-TextString item))))))))
            (if (or (null key) (= key ""))
              (progn
                (setq key (vl-catch-all-apply 'vla-get-EffectiveName
                                              (list obj)))
                (if (vl-catch-all-error-p key)
                  (setq key (vla-get-Name obj)))))
            (setq blks (tk-inc blks key 1.0))
            ;; diem thiet bi de soi 2 dau day
            (setq eqpts (cons (cons (tk-loai-thietbi key)
                                    (vlax-get obj 'InsertionPoint))
                              eqpts)))
          (progn
            (setq plv (vl-catch-all-apply
                        '(lambda ()
                           (vlax-curve-getDistAtParam obj
                             (vlax-curve-getEndParam obj)))))
            (if (and (not (vl-catch-all-error-p plv)) plv (> plv 0.0))
              (progn
                (setq key (vla-get-Layer obj)
                      q1  (vlax-curve-getStartPoint obj)
                      q2  (vlax-curve-getEndPoint obj))
                (setq laylen (tk-inc laylen key plv))
                (setq laycnt (tk-inc laycnt key 1.0))
                ;; moi DAU day: thiet bi gan nhat trong *TK-DauPline*
                ;; -> cong len xuong cua loai do
                (setq lx 0.0)
                (foreach item (list q1 q2)
                  (setq best_d 1e9 code nil)
                  (foreach ent eqpts
                    (setq dqd (distance item (cdr ent)))
                    (if (< dqd best_d)
                      (setq best_d dqd code (car ent))))
                  (if (and code (< best_d *TK-DauPline*))
                    (setq lx (+ lx (tk-lx-dien code)))))
                (if (> lx 0.0)
                  (setq laylx (tk-inc laylx key lx)))))))
        (setq i (1+ i)))
      ;; sap xep theo ten
      (setq blks   (vl-sort blks   '(lambda (a b) (< (car a) (car b))))
            laylen (vl-sort laylen '(lambda (a b) (< (car a) (car b))))
            nb (length blks) nl (length laylen))
      (if (= (+ nb nl) 0)
        (princ "\n[Thong bao] Khong co block hay duong nao trong vung chon.")
        (progn
          (if (null h)
            (progn
              (initget 4)
              (setq h (getreal "\nChieu cao chu trong Bang <250>: "))
              (if (null h) (setq h 250.0))))
          (setq pt (getpoint "\nClick diem dat Bang Thong Ke: "))
          (if pt
            (progn
              (setq nr (+ 3 nb nl))
              (setq tbl (vla-AddTable msp (vlax-3d-point pt)
                                      nr 9 (* 2.2 h) (* 6.0 h)))
              (vl-catch-all-apply 'vla-SetTextStyle (list tbl 7 *TK-TextStyle*))
              (vla-SetColumnWidth tbl 0 (* 3.0 h))
              (vla-SetColumnWidth tbl 1 (* 14.0 h))
              (vla-SetColumnWidth tbl 2 (* 4.0 h))
              (vla-SetColumnWidth tbl 3 (* 6.5 h))
              (vla-SetColumnWidth tbl 4 (* 6.0 h))
              (vla-SetColumnWidth tbl 5 (* 6.0 h))
              (vla-SetColumnWidth tbl 6 (* 6.0 h))
              (vla-SetColumnWidth tbl 7 (* 6.0 h))
              (vla-SetColumnWidth tbl 8 (* 6.0 h))
              (vla-SetTextHeight tbl 63 h)
              (vla-SetText tbl 0 0 "BANG THONG KE KHOI LUONG")
              (vla-SetText tbl 1 0 "STT")
              (vla-SetText tbl 1 1 "TEN BLOCK / LOP DAY")
              (vla-SetText tbl 1 2 "DON\nVI")
              (vla-SetText tbl 1 3 "SO LUONG /\nCHIEU DAI")
              (vla-SetText tbl 1 4 "LEN XUONG\n(m)")
              (vla-SetText tbl 1 5 "TONG CAP\n(m)")
              (vla-SetText tbl 1 6 "LOAI ONG")
              (vla-SetText tbl 1 7 "ONG HDPE\n(m)")
              (vla-SetText tbl 1 8 "RANH\n(m)")
              (setq r 2 cnt 1 sd 0.0 sl 0.0 st 0.0 sc 0.0 sr 0.0)
              (foreach item blks
                (vla-SetText tbl r 0 (itoa cnt))
                (vla-SetText tbl r 1 (car item))
                (vla-SetText tbl r 2 "cai")
                (vla-SetText tbl r 3 (rtos (cdr item) 2 0))
                (vla-SetText tbl r 4 "-")
                (vla-SetText tbl r 5 "-")
                (vla-SetText tbl r 6 "-")
                (vla-SetText tbl r 7 "-")
                (vla-SetText tbl r 8 "-")
                (setq r (1+ r) cnt (1+ cnt)))
              (foreach item laylen
                (setq lx  (cond ((cdr (assoc (car item) laylx))) (0.0)))
                (setq ong (Get-HDPE-Type (car item)))
                (setq plv (- (+ (cdr item) lx)
                             (* *TK-TruOng*
                                (cond ((cdr (assoc (car item) laycnt))) (0.0)))))
                (if (< plv 0.0) (setq plv 0.0))
                (vla-SetText tbl r 0 (itoa cnt))
                (vla-SetText tbl r 1 (car item))
                (vla-SetText tbl r 2 "m")
                (vla-SetText tbl r 3 (rtos (cdr item) 2 2))
                (vla-SetText tbl r 4 (rtos lx 2 2))
                (vla-SetText tbl r 5 (rtos (+ (cdr item) lx) 2 2))
                (vla-SetText tbl r 6 (if (= ong "NONE") "-" ong))
                (vla-SetText tbl r 7 (if (= ong "NONE") "-" (rtos plv 2 2)))
                (vla-SetText tbl r 8 (rtos (cdr item) 2 2))
                (setq sd (+ sd (cdr item))
                      sl (+ sl lx)
                      st (+ st (cdr item) lx)
                      sc (+ sc (if (= ong "NONE") 0.0 plv))
                      sr (+ sr (cdr item)))
                (princ (strcat "\n   [" (car item) "] dai "
                               (rtos (cdr item) 2 2) " + len xuong "
                               (rtos lx 2 2) " = "
                               (rtos (+ (cdr item) lx) 2 2)
                               "m | ong " (if (= ong "NONE") "-" ong)))
                (setq r (1+ r) cnt (1+ cnt)))
              ;; dong tong (phan day)
              (vla-SetText tbl r 1 "TONG CONG")  ; (bang dem an - giu ASCII)
              (vla-SetText tbl r 2 "-")
              (vla-SetText tbl r 3 (rtos sd 2 2))
              (vla-SetText tbl r 4 (rtos sl 2 2))
              (vla-SetText tbl r 5 (rtos st 2 2))
              (vla-SetText tbl r 6 "-")
              (vla-SetText tbl r 7 (rtos sc 2 2))
              (vla-SetText tbl r 8 (rtos sr 2 2))
              (princ (strcat "\n[SUCCESS] Da thong ke " (itoa nb)
                             " loai block + " (itoa nl)
                             " lop day (tong cap = dai + len xuong;"
                             " ong theo tiet dien; ranh = mat bang)."
                             " Dung XBE de sang Excel."))))))))
  (princ))

;; ============================================================
;; BANG CHI TIET DIEN THEO KHOANG (be nguyen kieu chieu sang)
;; ============================================================
(defun tk-dien-chitiet (ss h / doc msp i ent obj name key atts nodes spans
                          caps q1 q2 plv lx best_d dqd na nb2 pt tbl r cnt
                          lst2 ss2 m2 obj2 item pair cur lay laylist onglist
                          nhomlist nr nc c v ong counted sKC sLX sTC sRA
                          capsum ongsum nhomsum dimkc mid
                          dmarks dstarts dends li2 snap2 tbaname grp2 lo_nm
                          sKC0 sLX0 sTC0 sRA0 fmt_ok sec_rows
                          seghits specmap tds tb0 grp3 pr kdx segkey v2 cdlist
                          lo_d lo_c lo_day loc2 k2 uu uubest
                          lst_bd bdnode
                          front wseen mm1 mm2 nhlist
                          mainpool visited walkseq nx wstack walkpairs ed
                          lst_pluscap plus_span plus_hdpe
                          lst_crcap crlen_span lst_cr_pts cr_spans
                          lst_qd_raw qdlist qd_alloc qd_pts qd_clusters rqd_alloc
                          lst_ranh_raw ranhlist ranh_decl
                          ptq lenq found3 rtype idx_r r_qd r_he crv
                          lst_crlen crlen2 r_cr crsum crcand crlo
                          qdsum rqdsum rhesum branch_p pair2)
  (if ss
    (progn
      (setq doc (vla-get-ActiveDocument (vlax-get-acad-object)))
      (setq msp (vla-get-ModelSpace doc))
      (if (null (tk-doc-thongso-dien)) (tk-nhap-thongso-dien))
      (princ
        (strcat "\n>> Len xuong: ha the " (rtos *TK-DIEN-HT* 2 2)
                "m | trung the " (rtos *TK-DIEN-TT* 2 2)
                "m | cong to " (rtos *TK-DIEN-CT* 2 2)
                "m | TBA " (rtos *TK-DIEN-TBA* 2 2)
                "m  (doi: lenh T-THONGSO)"))
      (if (null (tblsearch "STYLE" *TK-TextStyle*))
        (progn
          (setq atts (vla-add (vla-get-TextStyles doc) *TK-TextStyle*))
          (vla-SetFont atts "Arial" :vlax-false :vlax-false 0 0)
          (vla-put-Height atts 0.0)))
      (setq nodes '())
      (setq i 0)
      (repeat (sslength ss)
        (setq ent (ssname ss i))
        (setq obj (vlax-ename->vla-object ent))
        (setq name (vla-get-ObjectName obj))
        (if (= name "AcDbBlockReference")
          (progn
            ;; ten block de phan loai thiet bi
            (setq lay (vl-catch-all-apply 'vla-get-EffectiveName (list obj)))
            (if (vl-catch-all-error-p lay) (setq lay (vla-get-Name obj)))
            ;; V114: doc TEN/TBA_ME theo TAG co nghia; khong lay theo Y WCS.
            ;; Neu block cu thieu TAG chuan thi fallback theo Y LOCAL cua block,
            ;; vi vay xoay 180 do / 90 do / mirror khong con dao nhan.
            (setq key nil v2 nil)
            (setq atts (tk-get-atts-safe obj))
            (if atts
              (progn
                (setq v (tk-dien-read-labels obj atts))
                (setq key (car v))
                (if (cadr v) (setq v2 (cons 0.0 (cadr v))))))
            (if (or (null key) (= key "")) (setq key lay))
            ;; loai: TEN BLOCK truoc, thuoc tinh bo khuyet
            (setq cnt (tk-loai-thietbi lay))
            (if (= cnt 1) (setq cnt (tk-loai-thietbi key)))
            ;; ===== DAU "*" O CUOI TEN = DIEM XUAT PHAT =====
            ;; vd "I-CĐ-01*" -> khai chinh thiet bi nay la diem dau
            ;; chuoi. Cat dau "*" ra khoi ten ngay tai day nen BANG
            ;; van hien ten sach "I-CĐ-01", va moi cho khac (lo, so
            ;; phan cap, nhanh treo) doc y nhu binh thuong.
            ;; Dung dau "*" (khong phai chu cai) de KHONG dam vao quy
            ;; uoc "chu cai sau so = nhanh treo" (02A, 02B).
            (setq uu nil)
            (if (and (> (strlen key) 1)
                     (= (substr key (strlen key) 1) "*"))
              (progn
                (setq key (vl-string-right-trim " " 
                            (substr key 1 (1- (strlen key)))))
                (setq uu T)))
            (setq item (list key cnt (vlax-get obj 'InsertionPoint)
                             (if v2 (cdr v2) "")))
            (if uu (setq lst_bd (cons item lst_bd)))
            (setq nodes (cons item nodes))))
        (setq i (1+ i)))
      (if (null nodes)
        (princ "\n[Thong bao] Khong co block thiet bi nao trong vung chon.")
        (progn
          (setq spans '())
          (setq caps '())
          (setq dimkc '())
          (setq laylist '())
          (setq specmap '())
          ;; ===== BUOC 1: DO DAY - moi doan giua 2 thiet bi ke nhau
          ;; DOC theo day; dinh danh THEO THUC THE (2 he TBA co ten
          ;; thiet bi trung nhau van tach bach). Loc khung bao truoc
          ;; khi goi COM de tang toc.
          (setq seghits '())
          (setq ss2 (ssget "_X" '((0 . "LWPOLYLINE,POLYLINE,LINE,SPLINE,ARC"))))
          (if ss2
            (progn
              (setq m2 0)
              (repeat (sslength ss2)
                (setq obj2 (vlax-ename->vla-object (ssname ss2 m2)))
                (setq plv (vl-catch-all-apply
                            '(lambda (o)
                               (vlax-curve-getDistAtParam o
                                 (vlax-curve-getEndParam o)))
                            (list obj2)))
                (if (and (not (vl-catch-all-error-p plv))
                         (numberp plv)
                         (> plv 0.001))
                  (progn
                    ;; LOC NHANH kieu chieu sang: chi day co DAU MUT
                    ;; cham thiet bi moi duoc do chi tiet (day khung
                    ;; ten, net phu... bi loai ngay sau 2 lenh re)
                    (setq q1 (vlax-curve-getStartPoint obj2))
                    (setq q2 (vlax-curve-getEndPoint obj2))
                    (setq v2 nil)
                    (foreach item nodes
                      (if (and (null v2)
                               (or (< (distance q1 (caddr item))
                                      *TK-DauPline*)
                                   (< (distance q2 (caddr item))
                                      *TK-DauPline*)))
                        (setq v2 T)))
                    (setq lst2 '())
                    (if v2
                      (foreach item nodes
                        (setq pt (caddr item))
                        (setq q1 (vl-catch-all-apply
                                   '(lambda (o p)
                                      (vlax-curve-getClosestPointTo o p))
                                   (list obj2 pt)))
                        (if (not (vl-catch-all-error-p q1))
                          (progn
                            (setq dqd (distance q1 pt))
                            (if (< dqd *TK-DauPline*)
                              (progn
                                (setq v (vl-catch-all-apply
                                          '(lambda (o p)
                                             (vlax-curve-getDistAtPoint o p))
                                          (list obj2 q1)))
                                (if (and (not (vl-catch-all-error-p v))
                                         (numberp v))
                                  (setq lst2
                                    (cons (cons v item) lst2)))))))))
                    (setq lst2 (vl-sort lst2
                                        '(lambda (a b) (< (car a) (car b)))))
                    ;; ===== KHONG CAT DAY TAI THIET BI KHAC LO =====
                    ;; 2 lo di chung hanh lang thi cap lo nay chay sat
                    ;; tu cua lo kia (vd chi 1.4m) -> truoc day bi cat
                    ;; nham: nua dau cong don sang lo kia, nua sau mat
                    ;; luon, con lo minh phai do thang chim bay.
                    ;; Lo cua soi day = lo cua thiet bi o 2 DAU day
                    ;; (dau nao co tien to lo thi lay); thiet bi GIUA
                    ;; day ma khac lo -> BO QUA, khong cat tai do.
                    ;; (day noi 2 lo khac nhau -> khong loc, giu nguyen)
                    (if (> (length lst2) 2)
                      (progn
                        (setq lo_d (tk-lo-prefix-dien
                                     (car (cdr (nth 0 lst2)))))
                        (setq lo_c (tk-lo-prefix-dien
                                     (car (cdr (nth (1- (length lst2))
                                                    lst2)))))
                        (setq lo_day (cond ((/= lo_c "") lo_c)
                                           ((/= lo_d "") lo_d)
                                           (T "")))
                        (if (and (/= lo_d "") (/= lo_c "") (/= lo_d lo_c))
                          (setq lo_day ""))
                        (if (/= lo_day "")
                          (progn
                            (setq loc2 '() k2 0)
                            (foreach cur lst2
                              (if (or (= k2 0)
                                      (= k2 (1- (length lst2)))
                                      (= (tk-lo-prefix-dien
                                           (car (cdr cur))) "")
                                      (= (tk-lo-prefix-dien
                                           (car (cdr cur))) lo_day))
                                (setq loc2 (append loc2 (list cur)))
                                (princ (strcat "\n[LUU Y] Day lo " lo_day
                                               " chay sat \"" (car (cdr cur))
                                               "\" (lo khac) - KHONG cat day"
                                               " tai do (co the 2 lo di"
                                               " CHUNG RANH doan nay).")))
                              (setq k2 (1+ k2)))
                            (setq lst2 loc2)))))
                    (if (>= (length lst2) 2)
                      (progn
                        (setq c 0)
                        (while (< (1+ c) (length lst2))
                          (setq na  (cdr (nth c lst2)))
                          (setq nb2 (cdr (nth (1+ c) lst2)))
                          (setq q2 (- (car (nth (1+ c) lst2))
                                      (car (nth c lst2))))
                          (if (= c 0)
                            (setq q2 (+ q2 (car (nth 0 lst2)))))
                          (if (= (+ c 2) (length lst2))
                            (setq q2 (+ q2 (- plv (car (nth (1+ c) lst2))))))
                          (if (and (not (eq na nb2)) (> q2 0.001))
                            (setq seghits
                              (cons (list na nb2 q2 (vla-get-Layer obj2))
                                    seghits)))
                          (setq c (1+ c)))))))
                (setq m2 (1+ m2)))))
          ;; ===== BUOC 2: CHUOI CO DINH THEO SO (nhu chieu sang) =====
          ;; Nhom thiet bi theo (TBA me = DONG THU 2 cua block, LO =
          ;; tien to La Ma). Thu tu khoang: TBA -> [tu bu chen theo
          ;; vi tri] -> TD theo so tang dan. Day/ghi chu/dim CHI
          ;; cung cap SO DO - ve kieu gi cung khong pha duoc thu tu.
          (setq grp3 '())
          (foreach item nodes
            (if (/= 4 (cadr item))
              (progn
                (setq segkey (strcat (cond ((nth 3 item)) (""))
                                     "~" (tk-lo-prefix-dien (car item))))
                (if (setq pair (assoc segkey grp3))
                  (setq grp3 (subst (append pair (list item)) pair grp3))
                  (setq grp3 (append grp3 (list (list segkey item))))))))
          ;; sap nhom: ten TBA me truoc, trong TBA thi lo I < II < III
          (setq grp3
            (vl-sort grp3
                     '(lambda (a b)
                        (if (= (substr (car a) 1
                                       (vl-string-search "~" (car a)))
                               (substr (car b) 1
                                       (vl-string-search "~" (car b))))
                          (< (tk-lo-rank-dien
                               (substr (car a)
                                       (+ 2 (vl-string-search "~" (car a)))))
                             (tk-lo-rank-dien
                               (substr (car b)
                                       (+ 2 (vl-string-search "~" (car b))))))
                          (< (car a) (car b))))))
          (foreach grp2 grp3
            (setq tbaname (substr (car grp2) 1
                                  (vl-string-search "~" (car grp2))))
            (setq lo_nm (substr (car grp2)
                                (+ 2 (vl-string-search "~" (car grp2)))))
            ;; tds theo SO tang dan; tu bu tach rieng
            (setq tds '())
            (setq tb0 nil)
            (setq cdlist '())
            (setq nhlist '())
            (foreach item (cdr grp2)
              (cond
                ((= 5 (cadr item))
                 (if (null tb0)
                   (setq tb0 item)
                   (princ (strcat "\n[CANH BAO] " (car grp2)
                                  " co hon 1 tu bu - chi nhan "
                                  (car tb0)))))
                ((= 6 (cadr item))
                 ;; cot dien: KHONG gioi han so luong, chen theo DAY
                 ;; y het tu bu (ten khong dang tin de suy vi tri)
                 (setq cdlist (cons item cdlist)))
                ((/= (tk-bo-duoi-nhanh (car item)) (car item))
                 ;; duoi chu sau so (02A) = NHANH treo
                 (setq nhlist (cons item nhlist)))
                (t (setq tds (cons item tds)))))
            (setq nhlist
              (vl-sort nhlist
                       '(lambda (a b) (< (car a) (car b)))))
            (setq tds
              (vl-sort (reverse tds)
                       '(lambda (a b)
                          (< (cond ((Get-First-Num (car a))) (0))
                             (cond ((Get-First-Num (car b))) (0))))))
            ;; mainpool = TAT CA thiet bi chinh (tu dien + tu bu) cua
            ;; lo nay - thu tu trong list KHONG quan trong, day quyet
            ;; dinh thu tu that qua vong DI THEO DAY ben duoi
            (setq mainpool tds)
            (if tb0 (setq mainpool (append mainpool (list tb0))))
            (setq mainpool (append mainpool cdlist))
            ;; TBA me: node loai 4 ten = dong 2; khong co -> gan nhat
            (setq pr nil)
            (foreach item nodes
              (if (and (null pr) (= 4 (cadr item))
                       (= (car item) tbaname))
                (setq pr item)))
            (if (and (null pr) tds)
              (progn
                (setq best_d 1e9)
                (foreach item nodes
                  (if (= 4 (cadr item))
                    (progn
                      (setq dqd (distance (caddr item) (caddr (car tds))))
                      (if (< dqd best_d)
                        (progn (setq best_d dqd) (setq pr item))))))))
            ;; ===== DI THEO DAY qua toan bo mainpool (tu dien + tu bu
            ;; xen ke tu nhien theo day that) - SO chi de PHA THE BI
            ;; khi day dut quang / nhieu nhanh cung luc =====
            (setq visited '())
            (setq walkseq '())
            (setq walkpairs '())
            (setq wstack '())
            (setq nx nil)
            ;; ===== DIEM XUAT PHAT: thiet bi co dau "*" cuoi ten =====
            ;; Uu tien CAO NHAT - da danh dau thi may khong doan gi nua
            ;; (de len ca truong hop co TBA).
            (setq bdnode nil)
            (foreach item lst_bd
              (if (and (null bdnode) (tk-mem-eq item mainpool))
                (setq bdnode item)))
            (if bdnode
              (progn
                (setq nx bdnode)
                (princ (strcat "\n>> [" (car grp2) "] Diem xuat phat KHAI"
                               " bang dau * o cuoi ten: " (car bdnode)))))
            (if (and pr (null nx))
              (foreach edge seghits
                (cond
                  ((and (null nx) (eq (car edge) pr)
                        (tk-mem-eq (cadr edge) mainpool))
                   (setq nx (cadr edge)))
                  ((and (null nx) (eq (cadr edge) pr)
                        (tk-mem-eq (car edge) mainpool))
                   (setq nx (car edge))))))
            (if (null nx)
              ;; ===== CHON DIEM XUAT PHAT khi KHONG co TBA =====
              ;; Cot dien nam DAU / GIUA / CUOI tuyen deu duoc, ten
              ;; bat ky - nen KHONG duoc chon theo so (de roi vao
              ;; GIUA chuoi, bang doc nguoc xuoi). Chon theo DAY:
              ;;   hang 0: DAU MUT (chi 1 day) va KHONG phai cot dien
              ;;           -> "da so tu dien roi den cot dien"
              ;;   hang 1: DAU MUT la cot dien (nhom toan cot dien)
              ;;   hang 2: nam giua chuoi (mach vong, khong co dau mut)
              ;; Cung hang thi lay duong so nho hon de on dinh.
              (progn
                (setq uubest 9)
                (foreach item mainpool
                  (setq k2 0)
                  (foreach edge seghits
                    (if (or (and (eq (car edge) item)
                                 (tk-mem-eq (cadr edge) mainpool))
                            (and (eq (cadr edge) item)
                                 (tk-mem-eq (car edge) mainpool)))
                      (setq k2 (1+ k2))))
                  (setq uu (cond ((and (= k2 1) (/= 6 (cadr item))) 0)
                                 ((= k2 1) 1)
                                 (T 2)))
                  (if (or (null nx)
                          (< uu uubest)
                          (and (= uu uubest)
                               (tk-duong-nho-hon (car item) (car nx))))
                    (setq nx item uubest uu)))))
            (if (and (null pr) nx)
              (princ (strcat "\n[LUU Y] " (car grp2)
                             ": khong thay block TBA trong vung chon"
                             " -> bang khong co hang xuat tuyen tu TBA,"
                             " bat dau tu " (car nx) ".")))
            ;; DFS CO BUOC LUI: re het nhanh con roi QUAY LAI node cha
            ;; di tiep than chinh (giong duyet cay ben chieu sang).
            ;; Truoc day KHONG co buoc lui -> gap ngo cut la nhay dai
            ;; sang cot so nho nhat con lai, sinh ra hang MA kieu
            ;; "1.1.1 -> 1.2" (2 cot nay khong he co day noi nhau).
            (if nx
              (progn
                (setq walkseq (list nx))
                (setq visited (list nx))
                (setq wstack (list nx))))
            (while wstack
              (setq cur (car wstack))
              (setq nx nil)
              (foreach edge seghits
                (cond
                  ((and (eq (car edge) cur)
                        (tk-mem-eq (cadr edge) mainpool)
                        (not (tk-mem-eq (cadr edge) visited))
                        (or (null nx)
                            (tk-duong-nho-hon (car (cadr edge)) (car nx))))
                   (setq nx (cadr edge)))
                  ((and (eq (cadr edge) cur)
                        (tk-mem-eq (car edge) mainpool)
                        (not (tk-mem-eq (car edge) visited))
                        (or (null nx)
                            (tk-duong-nho-hon (car (car edge)) (car nx))))
                   (setq nx (car edge)))))
              (if nx
                (progn
                  (setq walkpairs (append walkpairs (list (list cur nx))))
                  (setq walkseq (append walkseq (list nx)))
                  (setq visited (cons nx visited))
                  (setq wstack (cons nx wstack)))
                ;; het hang xom chua tham -> LUI VE node cha
                (setq wstack (cdr wstack)))
              ;; lui het ma van con node chua tham: day dut quang
              (if (null wstack)
                (progn
                  (setq nx nil)
                  (foreach item mainpool
                    (if (and (not (tk-mem-eq item visited))
                             (or (null nx)
                                 (tk-duong-nho-hon (car item) (car nx))))
                      (setq nx item)))
                  (if nx
                    (progn
                      (princ (strcat "\n[LUU Y] " (car grp2)
                                     ": day KHONG LIEN TUC - nhay sang "
                                     (car nx) " theo so"
                                     " (kiem tra co thieu day noi khong)."))
                      (if walkseq
                        (setq walkpairs
                          (append walkpairs
                                  (list (list (last walkseq) nx)))))
                      (setq walkseq (append walkseq (list nx)))
                      (setq visited (cons nx visited))
                      (setq wstack (list nx)))))))
            (if pr
              (princ (strcat "\n>> Xuat tuyen [" (car grp2) "]: "
                             (car pr) " -> "
                             (if walkseq (car (car walkseq)) "?"))))
            ;; cap khoang: xuat tuyen + [nhanh cua node] + chuoi DI
            ;; THEO DAY (nhu chieu sang: 02 -> 02A truoc, roi den ke)
            (setq lst2 '())
            (if (and pr walkseq)
              (setq lst2 (list (list pr (car walkseq)))))
            ;; nhanh treo (duoi chu: 02A) cua node dau chuoi
            (if walkseq
              (foreach cur nhlist
                (if (= (tk-bo-duoi-nhanh (car cur)) (car (car walkseq)))
                  (setq lst2 (append lst2
                                     (list (list (car walkseq) cur)))))))
            ;; cac CANH DFS (cha -> con) - da dung thu tu duyet cay
            (foreach ed walkpairs
              (setq lst2 (append lst2 (list ed)))
              (foreach cur nhlist
                (if (= (tk-bo-duoi-nhanh (car cur)) (car (cadr ed)))
                  (setq lst2 (append lst2
                                     (list (list (cadr ed) cur)))))))
            ;; dang ky: do = DAY dung cap thuc the / do thang
            (foreach pair lst2
              ;; chieu mui ten = chieu HANH TRINH chuoi (thang truoc
              ;; -> thang sau), khong con ep theo bac nguon tinh
              (setq na (car pair))
              (setq nb2 (cadr pair))
              (setq q2 nil)
              (setq lay nil)
              (foreach cur seghits
                (if (or (and (eq (car cur) na) (eq (cadr cur) nb2))
                        (and (eq (car cur) nb2) (eq (cadr cur) na)))
                  (progn
                    (setq q2 (+ (cond (q2) (0.0)) (caddr cur)))
                    (if (null lay) (setq lay (nth 3 cur))))))
              (if (null q2)
                (setq q2 (distance (caddr na) (caddr nb2))))
              (setq lx (+ (tk-lx-dien (cadr na))
                          (tk-lx-dien (cadr nb2))))
              (setq key (strcat tbaname "~" lo_nm "~" (car na)
                                " \\U+2192 " (car nb2)))
              (setq spans (cons (list key na nb2 q2 lx 1
                                      tbaname lo_nm)
                                spans))
              (if lay
                (progn
                  (if (not (member lay laylist))
                    (setq laylist (cons lay laylist)))
                  (setq caps
                    (tk-inc caps (strcat key "|" lay)
                            (* 1.01 (+ q2 lx))))))))
          (setq i 0)
          (repeat (sslength ss)
            (setq ent (ssname ss i))
            (setq obj (vlax-ename->vla-object ent))
            (setq name (vla-get-ObjectName obj))
            ;; nhu chieu sang: +cap phu | CR-cap | CR don | ong QD | loai
            ;; ranh deu duoc nhan dien TRUOC khi roi ve DSTA/dim/KC
            (setq plv nil)
            (cond
              ((member name '("AcDbText" "AcDbMText"))
               (cond
                 ((setq item (Parse-PlusCap-Obj obj))
                  (setq lst_pluscap (cons item lst_pluscap)))
                 ((setq item (Parse-CRCap-Obj obj))
                  (setq lst_crcap (cons item lst_crcap)))
                 ((setq item (Parse-CRLen-Obj obj))
                  (setq lst_crlen (cons item lst_crlen)))
                 ((setq item (Parse-CR-Obj obj))
                  (setq lst_cr_pts (cons item lst_cr_pts)))
                 ((setq item (Parse-QD-Obj obj))
                  (setq lst_qd_raw (cons item lst_qd_raw)))
                 ((setq item (Parse-Ranh-Obj obj))
                  (setq lst_ranh_raw (cons item lst_ranh_raw)))
                 (t
                  (setq plv (vl-catch-all-apply
                              '(lambda (o)
                                 (tk-met-trong-ngoac (vla-get-TextString o)))
                              (list obj))))))
              ((and (member name '("AcDbRotatedDimension"
                                   "AcDbAlignedDimension"))
                    (/= (strcase (vla-get-Layer obj)) "TK-KICH-THUOC")
                    (not (member (vla-get-Layer obj) laylist)))
               (setq plv (vl-catch-all-apply 'vla-get-Measurement (list obj)))))
            (if plv
              (progn
                (if (and (not (vl-catch-all-error-p plv))
                         (numberp plv)
                         (> plv 0.001))
                  (progn
                    (setq pt (tk-objpt obj))
                    (setq item (tk-nearest-span-dien pt spans))
                    (setq cur (car item) best_d (cadr item))
                    (if cur
                      (progn
                        (setq dimkc (tk-inc dimkc cur plv))
                        ;; ghi chu "DSTA n(..M)" khai LOAI cap cho khoang
                        (if (member name '("AcDbText" "AcDbMText"))
                          (progn
                            (setq v2 (Get-DSTA-Spec (tk-rawtext obj)))
                            (if v2
                              (setq specmap
                                (cons (cons (strcat "S|" cur) v2)
                                      specmap)))))))))))
            (setq i (1+ i)))
          ;; ===== NHU CHIEU SANG: +cap phu | CR-cap | CR | QD | R =====
          ;; tim khoang GAN NHAT theo doan day; CR-cap CONG THEM vao
          ;; khoang cach MB (dimkc) - no la doan day THAT di chung
          ;; ranh voi truc, KHONG phai cap phu roi rac; +/QD/R nham
          ;; MOI khoang; CR-cap/CR CHI nham khoang NHANH.
          (foreach qd_item lst_pluscap
            (setq item (tk-nearest-span-dien (cadr qd_item) spans))
            (setq cur (car item))
            (if cur
              (progn
                (if (not (member (caddr qd_item) laylist))
                  (setq laylist (cons (caddr qd_item) laylist)))
                (setq plus_span (tk-inc plus_span
                                        (strcat cur "|" (caddr qd_item))
                                        (car qd_item)))
                (setq plus_hdpe (tk-inc plus_hdpe
                                        (strcat cur "|"
                                                (Get-HDPE-Type (caddr qd_item)))
                                        (car qd_item))))))
          (foreach qd_item lst_crcap
            (setq item (tk-nearest-span-dien (cadr qd_item)
                         (vl-remove-if-not 'tk-branch-p-dien spans)))
            (setq cur (car item))
            (if cur
              (progn
                (if (not (member (caddr qd_item) laylist))
                  (setq laylist (cons (caddr qd_item) laylist)))
                (setq caps (tk-inc caps (strcat cur "|" (caddr qd_item))
                                   (car qd_item)))
                (setq crlen_span (tk-inc crlen_span cur (car qd_item)))
                ;; doan CR di chung ranh la mot phan CHIEU DAI THAT
                ;; cua khoang (nhu doan sau) - cong vao KC, khong chi
                ;; tinh rieng cap/ranh
                (setq dimkc (tk-inc dimkc cur (car qd_item))))))
          (foreach qd_item lst_cr_pts
            (setq item (tk-nearest-span-dien qd_item
                         (vl-remove-if-not 'tk-branch-p-dien spans)))
            (setq cur (car item))
            (if cur
              (if (not (member cur cr_spans))
                (setq cr_spans (cons cur cr_spans)))))
          ;; ===== CR(so met) = doan DI CHUNG RANH voi lo khac =====
          ;; 2 lo di chung 1 hanh lang thi CA HAI khoang deu di qua cho
          ;; dat chu -> do hinh hoc KHONG the phan biet ghi chu thuoc lo
          ;; nao. Nen chon lo theo 2 lop:
          ;;  1. Neu ghi chu co khai lo ("CR-II(34,2M)") -> chi xet cac
          ;;     khoang cua dung lo do.
          ;;  2. Khong khai -> BO cac khoang ma tru xong gan het ranh
          ;;     (KC - CR <= 0.5m): do chinh la lo DA DAO ranh (vd lo I
          ;;     KC 34.22 tru 34.20 con 0.02) - phai giu nguyen so da
          ;;     dao; chi lo AN THEO (con ranh rieng) moi nhan CR.
          (setq crlen2 '())
          (foreach qd_item lst_crlen
            (setq crlo (caddr qd_item))
            (if (or (null crlo) (= crlo ""))
              ;; CHUA KHAI LO -> khong doan, bao cho nguoi dung khai lai
              (princ (strcat "\n[LUU Y] Ghi chu chung ranh "
                             (rtos (car qd_item) 2 2)
                             "m CHUA KHAI LO nen KHONG duoc tinh."
                             " Hay ghi ro lo, vd: CR-II(34,2)"
                             " hoac CR-III(34,2)."))
              (progn
                (setq crcand
                  (vl-remove-if-not
                    '(lambda (p)
                       (= (strcase (cond ((nth 7 p)) (""))) crlo))
                    spans))
                (if (null crcand)
                  (princ (strcat "\n[LUU Y] Ghi chu chung ranh CR-" crlo
                                 "(" (rtos (car qd_item) 2 2)
                                 ") nhung KHONG co khoang nao thuoc lo "
                                 crlo " - kiem tra lai ten lo."))
                  (progn
                    ;; trong cung lo, uu tien khoang con du ranh de tru
                    (setq loc2
                      (vl-remove-if
                        '(lambda (p)
                           (<= (- (cond ((cdr (assoc (car p) dimkc)))
                                        ((nth 3 p)))
                                  (car qd_item))
                               0.5))
                        crcand))
                    (if loc2 (setq crcand loc2))
                    (setq item (tk-nearest-span-dien (cadr qd_item) crcand))
                    (setq cur (car item))
                    (if cur
                      (progn
                        (setq crlen2 (tk-inc crlen2 cur (car qd_item)))
                        (princ (strcat "\n>> Chung ranh " (rtos (car qd_item) 2 2)
                                       "m [LO " crlo "] -> khoang: "
                                       (substr cur
                                         (+ 2 (vl-string-search "~" cur
                                                (1+ (vl-string-search "~" cur))))))))))))))
          (setq qd_pts '())
          (foreach qd_item lst_qd_raw
            (setq item (tk-nearest-span-dien (cadr qd_item) spans))
            (setq cur (car item))
            (if cur
              (progn
                (setq qd_nm nil)
                (foreach s2 qdlist
                  (if (and (null qd_nm) (= (tk-qd-norm s2)
                                           (tk-qd-norm (caddr qd_item))))
                    (setq qd_nm s2)))
                (if (null qd_nm)
                  (progn (setq qd_nm (caddr qd_item))
                         (setq qdlist (cons qd_nm qdlist))))
                (setq qd_alloc (tk-inc qd_alloc (strcat cur "|" qd_nm)
                                       (car qd_item)))
                (setq qd_pts (cons (list cur (cadr qd_item) (car qd_item))
                                   qd_pts)))))
          ;; cum dao qua duong: 1 cho dao (cach nhau <= *TK-GomCumQD*) = 1 ranh
          (setq qd_clusters '())
          (foreach qd_item qd_pts
            (setq cur (car qd_item) ptq (cadr qd_item) lenq (caddr qd_item))
            (setq pair (assoc cur qd_clusters))
            (setq lst2 (if pair (cdr pair) '()))
            (setq found3 nil)
            (foreach cl2 lst2
              (if (and (null found3) (<= (distance ptq (car cl2)) *TK-GomCumQD*))
                (setq found3 cl2)))
            (if found3
              (setq lst2 (subst (cons (car found3) (max (cdr found3) lenq))
                                found3 lst2))
              (setq lst2 (cons (cons ptq lenq) lst2)))
            (setq qd_clusters
              (if pair (subst (cons cur lst2) pair qd_clusters)
                (cons (cons cur lst2) qd_clusters))))
          (setq rqd_alloc '())
          (foreach pair qd_clusters
            (setq lenq 0.0)
            (foreach cl2 (cdr pair) (setq lenq (+ lenq (cdr cl2))))
            (setq rqd_alloc (cons (cons (car pair) lenq) rqd_alloc)))
          (foreach qd_item lst_ranh_raw
            (setq item (tk-nearest-span-dien (car qd_item) spans))
            (setq cur (car item) best_d (cadr item))
            (if cur
              (progn
                (setq pair (assoc cur ranh_decl))
                (cond
                  ((null pair)
                   (setq ranh_decl
                     (cons (list cur (cadr qd_item) best_d) ranh_decl)))
                  (t
                   (if (< best_d (caddr pair))
                     (setq ranh_decl
                       (subst (list cur (cadr qd_item) best_d) pair ranh_decl))))))))
          (setq qdlist (vl-sort qdlist 'tk-numlist-less))
          (setq ranhlist '())
          (foreach pair ranh_decl
            (if (not (member (cadr pair) ranhlist))
              (setq ranhlist (cons (cadr pair) ranhlist))))
          (if (not (member *TK-RanhMacDinh* ranhlist))
            (setq ranhlist (cons *TK-RanhMacDinh* ranhlist)))
          (setq ranhlist (vl-sort ranhlist 'tk-numlist-less))
          ;; ghi chu spec: cap cot theo LOAI khai (de len lop day) -
          ;; chay SAU CUNG de dung dimkc DA CONG DU (ke ca CR-cap)
          (foreach pair spans
            (setq v2 (cdr (assoc (strcat "S|" (car pair)) specmap)))
            (if v2
              (progn
                (if (not (member v2 laylist))
                  (setq laylist (cons v2 laylist)))
                (foreach lay laylist
                  (if (assoc (strcat (car pair) "|" lay) caps)
                    (setq caps
                      (vl-remove (assoc (strcat (car pair) "|" lay) caps)
                                 caps))))
                (setq plv (cond ((cdr (assoc (car pair) dimkc)))
                                ((nth 3 pair))))
                (setq caps
                  (tk-inc caps (strcat (car pair) "|" v2)
                          (* 1.01 (+ plv (nth 4 pair))))))))
          (if (null spans)
            (princ "\n[Thong bao] Khong co day nao noi 2 thiet bi - khong tao duoc khoang.")
            (progn
              (setq spans (reverse spans))   ; thu tu hanh trinh mach day
              ;; nhom theo nhan (TBA me ~ lo) - thu tu xuat hien
              (setq grp3 '())
              (foreach pair spans
                (setq segkey (strcat (cond ((nth 6 pair)) (""))
                                     "~" (cond ((nth 7 pair)) (""))))
                (if (null (assoc segkey grp3))
                  (setq grp3 (append grp3
                                     (list (list segkey
                                                 (cond ((nth 7 pair)) (""))
                                                 (cond ((nth 6 pair)) (""))))))))
              (setq dmarks '())
              (setq lst2 '())
              (foreach grp2 grp3
                (setq cur '())
                (foreach pair spans
                  (if (= (strcat (cond ((nth 6 pair)) (""))
                                 "~" (cond ((nth 7 pair)) ("")))
                         (car grp2))
                    (setq cur (append cur (list pair)))))
                (setq dmarks (append dmarks
                                     (list (list (cadr grp2)
                                                 (caddr grp2)
                                                 (length cur)))))
                (setq lst2 (append lst2 cur)))
              (setq spans lst2)
              (setq dstarts '() dends '() i 0)
              (foreach pair dmarks
                (setq dstarts (cons (cons i pair) dstarts))
                (setq i (+ i (caddr pair)))
                (setq dends (cons (cons (1- i) (car pair)) dends)))
              (setq onglist '())
              (setq nhomlist '())
              (foreach lay laylist
                (setq ong (Get-HDPE-Type lay))
                (if (and (/= ong "NONE") (not (member ong onglist)))
                  (setq onglist (cons ong onglist))))
              (setq nhomlist '())
              (foreach item '(6 5 4 3 2 1)
                (if (vl-some '(lambda (nd) (= (cadr nd) item)) nodes)
                  (setq nhomlist
                    (cons (tk-ten-loai item) nhomlist))))
              (setq laylist (vl-sort laylist 'tk-numlist-less))
              (setq onglist (vl-sort onglist 'tk-numlist-less))
              (if (null h)
                (progn
                  (initget 4)
                  (setq h (getreal "\nChieu cao chu trong Bang <250>: "))
                  (if (null h) (setq h 250.0))))
              (setq pt (getpoint "\nClick diem dat Bang Thong Ke: "))
              (if pt
                (progn
                  (setq nc (+ 6 (length laylist)      ; +1 = cot CHUNG RANH
                              (length onglist)
                              (length qdlist)
                              (* 2 (length ranhlist))
                              (length nhomlist)))
                  (setq v 0)
                  (setq tbaname "")
                  (foreach pair dmarks
                    (setq v (+ v 2
                               (if (and (/= (cadr pair) "")
                                        (/= (cadr pair) tbaname))
                                 1 0)))
                    (if (/= (cadr pair) "")
                      (setq tbaname (cadr pair))))
                  (setq nr (+ 3 (length spans)
                              (if (> (length dmarks) 1) v 0)))
                  (setq tbl (vla-AddTable msp (vlax-3d-point pt)
                                          nr nc (* 2.2 h) (* 6.0 h)))
                  ;; thu dinh dang CA BANG 1 phat (nhu chieu sang):
                  ;; thanh cong -> fmt_ok=T -> moi o chi 1 lenh ghi (nhanh);
                  ;; may cu that bai -> fallback tung o nhu truoc
                  (setq mid (vl-catch-all-apply 'vla-SetTextStyle
                                                (list tbl 7 *TK-TextStyle*)))
                  (vl-catch-all-apply 'vla-SetAlignment (list tbl 7 5))
                  (if (not (vl-catch-all-error-p mid)) (setq fmt_ok T))
                  (if (null fmt_ok)
                    (princ "\n[Thong bao] AutoCAD ban cu: tu chuyen dinh dang tung o (cham hon)."))
                  ;; tat ve lai bang trong luc dien du lieu (tang toc)
                  (vl-catch-all-apply
                    '(lambda ()
                       (vla-put-RegenerateTableSuppressed tbl :vlax-true)))
                  (vla-SetTextHeight tbl 63 h)
                  (tk-settext tbl 0 0 "BẢNG THỐNG KÊ CHI TIẾT TUYẾN ĐIỆN, CÁP VÀ ỐNG BẢO VỆ")
                  ;; dau muc: don vi xuong dong; o gian theo chu
                  (setq lst2
                    (append
                      (list "STT"
                            "ĐIỂM ĐẦU \\U+2192 ĐIỂM CUỐI"
                            "KHOẢNG CÁCH MB\n(m)"
                            "LÊN XUỐNG\n(m)"
                            "TỔNG CỘNG *1%\n(m)")
                      (mapcar '(lambda (x) (strcat x "\n(m)")) laylist)
                      (mapcar '(lambda (x)
                                 (strcat "L \U+1ED0NG " (strcase x)
                                         "\n(-" (rtos *TK-TruOng* 2 0)
                                         "m)(m)"))
                              onglist)
                      (mapcar '(lambda (x)
                                 (strcat "L \U+1ED0NG QUA\n\U+0110\U+01AF\U+1EDCNG "
                                         x "\n(-" (rtos *TK-TruOng* 2 0)
                                         "m)(m)"))
                              qdlist)
                      (list "CHUNG R\U+00C3NH\n(m)")
                      (mapcar '(lambda (x) (strcat x "\nQUA \U+0110\U+01AF\U+1EDCNG\n(m)"))
                              ranhlist)
                      (mapcar '(lambda (x) (strcat x "\nH\U+00C8\n(m)"))
                              ranhlist)
                      (mapcar '(lambda (x)
                                 (strcat x
                                         (cond
                                           ((= x "TBA") "\n(b\U+1ED9)")
                                           ((= x "CỘT ĐIỆN") "\n(c\U+1ED9t)")
                                           (t "\n(cái)"))))
                              nhomlist)))
                  (setq c 0)
                  (foreach item lst2
                    (tk-settext tbl 1 c item)
                    (setq c (1+ c)))
                  ;; RONG COT: co dinh theo he so x chieu cao chu, dung
                  ;; Y HET cong thuc bang chieu sang (khong tinh theo
                  ;; do dai dau muc - chu tu xuong dong trong o)
                  (vla-SetColumnWidth tbl 0 (* h 4.0))
                  (vla-SetColumnWidth tbl 1 (* h 20.0))
                  (vla-SetColumnWidth tbl 2 (* h 15.0))
                  (vla-SetColumnWidth tbl 3 (* h 15.0))
                  (vla-SetColumnWidth tbl 4 (* h 15.0))
                  (setq c 0)
                  (while (< c (length laylist))
                    (vla-SetColumnWidth tbl (+ 5 c) (* h 16.0))
                    (setq c (1+ c)))
                  (setq c 0)
                  (while (< c (length onglist))
                    (vla-SetColumnWidth tbl (+ 5 (length laylist) c) (* h 18.0))
                    (setq c (1+ c)))
                  (setq c 0)
                  (while (< c (length qdlist))
                    (vla-SetColumnWidth tbl
                      (+ 5 (length laylist) (length onglist) c) (* h 15.0))
                    (setq c (1+ c)))
                  (vla-SetColumnWidth tbl
                    (+ 5 (length laylist) (length onglist) (length qdlist))
                    (* h 16.0))
                  (setq c 0)
                  (while (< c (* 2 (length ranhlist)))
                    (vla-SetColumnWidth tbl
                      (+ 6 (length laylist) (length onglist) (length qdlist) c)
                      (* h 16.0))
                    (setq c (1+ c)))
                  (setq c 0)
                  (while (< c (length nhomlist))
                    (setq v (* (strlen (nth c nhomlist)) h 0.95))
                    (if (< v (* h 18.0)) (setq v (* h 18.0)))
                    (vla-SetColumnWidth tbl
                      (+ 6 (length laylist) (length onglist) (length qdlist)
                         (* 2 (length ranhlist)) c)
                      v)
                    (setq c (1+ c)))
                  (setq r 2)
                  (setq cnt 1)
                  (setq counted '())
                  (setq sKC 0.0)
                  (setq sLX 0.0)
                  (setq sTC 0.0)
                  (setq capsum '())
                  (setq ongsum '())
                  (setq nhomsum '())
                  (setq qdsum '())
                  (setq rqdsum '())
                  (setq rhesum '())
                  (setq crsum 0.0)
                  (setq li2 0)
                  (setq snap2 nil)
                  (setq sec_rows '())
                  (setq tbaname "")
                  (foreach pair spans
                    ;; dau LO: 2 dong de muc (ten TBA + LO x), reset STT
                    (if (and (> (length dmarks) 1)
                             (setq cur (cdr (assoc li2 dstarts))))
                      (progn
                        ;; dong ten TBA: chi ve 1 LAN cho ca cum lo
                        (if (and (/= (cadr cur) "")
                                 (/= (cadr cur) tbaname))
                          (progn
                            (setq tbaname (cadr cur))
                            (vl-catch-all-apply 'vla-MergeCells
                              (list tbl r r 0 (1- nc)))
                            (tk-settext tbl r 0 (cadr cur))
                            (setq sec_rows (cons r sec_rows))
                            (setq r (1+ r))))
                        (vl-catch-all-apply 'vla-MergeCells
                          (list tbl r r 0 (1- nc)))
                        (tk-settext tbl r 0
                          (strcat "L\U+1ED8 "
                                  (if (= (car cur) "") "-" (car cur))))
                        (setq sec_rows (cons r sec_rows))
                        (setq r (1+ r))
                        (setq cnt 1)
                        (setq snap2 (list sKC sLX sTC
                                          capsum ongsum nhomsum
                                          qdsum rqdsum rhesum crsum))))
                    (setq plv (cond ((cdr (assoc (car pair) dimkc)))
                                    ((nth 3 pair))))
                    (setq lx (nth 4 pair))
                    ;; trace: may cong so nao voi so nao
                    (princ (strcat "\n   [" (itoa cnt) "] "
                                   (car (cadr pair)) " ("
                                   (tk-loai-ascii (cadr (cadr pair))) " "
                                   (rtos (tk-lx-dien (cadr (cadr pair))) 2 2)
                                   ") + " (car (caddr pair)) " ("
                                   (tk-loai-ascii (cadr (caddr pair))) " "
                                   (rtos (tk-lx-dien (cadr (caddr pair))) 2 2)
                                   ") -> len xuong 1 soi = "
                                   (rtos (+ (tk-lx-dien (cadr (cadr pair)))
                                            (tk-lx-dien (cadr (caddr pair))))
                                         2 2) "m"))
                    (if (and (= 0.0 (tk-lx-dien (cadr (cadr pair))))
                             (= 0.0 (tk-lx-dien (cadr (caddr pair)))))
                      (princ "  [LUU Y] ca 2 dau = 0 -> kiem tra khai bao T-THONGSO"))
                    (tk-settext tbl r 0 (itoa cnt))
                    (tk-settext tbl r 1
                      (substr (car pair)
                              (+ 2 (vl-string-search "~" (car pair)
                                     (1+ (vl-string-search "~"
                                            (car pair)))))))
                    ;; lam tron 1 so ROI moi vua in vua cong don
                    (setq v2 (tk-lam-tron1 plv))
                    (tk-settext tbl r 2 (rtos v2 2 1))
                    (setq sKC (+ sKC v2))
                    (setq v2 (tk-lam-tron1 lx))
                    (tk-settext tbl r 3 (rtos v2 2 1))
                    (setq sLX (+ sLX v2))
                    (setq v2 (tk-lam-tron1 (* 1.01 (+ plv lx))))
                    (tk-settext tbl r 4 (rtos v2 2 1))
                    (setq sTC (+ sTC v2))
                    (setq c 5)
                    (foreach lay laylist
                      (setq v (cdr (assoc (strcat (car pair) "|" lay) caps)))
                      (setq crv (cdr (assoc (strcat (car pair) "|" lay) plus_span)))
                      (if crv (setq v (+ (cond (v) (0.0)) crv)))
                      (if v (setq v (tk-lam-tron1 v)))
                      (tk-settext tbl r c (if (and v (> v 0)) (rtos v 2 1) "-"))
                      (if (and v (> v 0)) (setq capsum (tk-inc capsum lay v)))
                      (setq c (1+ c)))
                    (foreach ong onglist
                      (setq v nil)
                      (foreach lay laylist
                        (if (= (Get-HDPE-Type lay) ong)
                          (progn
                            (setq cur
                              (cdr (assoc (strcat (car pair) "|" lay) caps)))
                            (if cur
                              (setq v (+ (cond (v) (0.0)) cur))))))
                      ;; y nguyen chieu sang: ong chinh - tru ong (1 lan);
                      ;; ong cap phu (+) KHONG tru dau, cong them sau
                      (if v (setq v (- v *TK-TruOng*)))
                      (if (and v (< v 0.0)) (setq v 0.0))
                      (setq crv (cdr (assoc (strcat (car pair) "|" ong) plus_hdpe)))
                      (if crv (setq v (+ (cond (v) (0.0)) crv)))
                      (if v (setq v (tk-lam-tron1 v)))
                      (tk-settext tbl r c (if (and v (> v 0)) (rtos v 2 1) "-"))
                      (if (and v (> v 0)) (setq ongsum (tk-inc ongsum ong v)))
                      (setq c (1+ c)))
                    (foreach qd_nm qdlist
                      (setq v (cdr (assoc (strcat (car pair) "|" qd_nm) qd_alloc)))
                      (if v (setq v (tk-lam-tron1 v)))
                      (tk-settext tbl r c (if (and v (> v 0)) (rtos v 2 1) "-"))
                      (if (and v (> v 0)) (setq qdsum (tk-inc qdsum qd_nm v)))
                      (setq c (1+ c)))
                    ;; ranh: qua duong = theo CUM dao; he = KC - CR-cap - qua duong
                    (setq r_qd (cond ((cdr (assoc (car pair) rqd_alloc))) (0.0)))
                    (setq pair2 (assoc (car pair) ranh_decl))
                    (setq rtype (if pair2 (cadr pair2) *TK-RanhMacDinh*))
                    (setq crv (cdr (assoc (car pair) crlen_span)))
                    (setq crv (if crv (if (> crv 0) crv plv) 0.0))
                    ;; met DI CHUNG RANH voi lo khac (ghi chu "CR(xx M)")
                    (setq r_cr (cond ((cdr (assoc (car pair) crlen2))) (0.0)))
                    (if (> r_cr plv) (setq r_cr plv))
                    (setq r_he (if (> plv 0)
                                 (max 0.0 (- plv crv r_qd r_cr)) 0.0))
                    (if (member (car pair) cr_spans)
                      (setq r_qd 0.0 r_he 0.0 r_cr 0.0))
                    ;; lam tron 2 so thap phan ngay tu day de cac dong
                    ;; cong lai dung bang so hien tren bang
                    (setq r_cr (tk-lam-tron1 r_cr))
                    (setq r_qd (tk-lam-tron1 r_qd))
                    (setq r_he (tk-lam-tron1 r_he))
                    ;; cot CHUNG RANH
                    (tk-settext tbl r c (if (> r_cr 0) (rtos r_cr 2 1) "-"))
                    (if (> r_cr 0) (setq crsum (+ crsum r_cr)))
                    (setq c (1+ c))
                    (setq idx_r nil)
                    (setq k 0)
                    (foreach s2 ranhlist
                      (if (and (null idx_r) (= (tk-qd-norm s2) (tk-qd-norm rtype)))
                        (setq idx_r k))
                      (setq k (1+ k)))
                    (setq k 0)
                    (foreach s2 ranhlist
                      (if (and idx_r (= k idx_r) (> r_qd 0))
                        (progn
                          (tk-settext tbl r c (rtos r_qd 2 1))
                          (setq rqdsum (tk-inc rqdsum s2 r_qd)))
                        (tk-settext tbl r c "-"))
                      (setq c (1+ c))
                      (setq k (1+ k)))
                    (setq k 0)
                    (foreach s2 ranhlist
                      (if (and idx_r (= k idx_r) (> r_he 0))
                        (progn
                          (tk-settext tbl r c (rtos r_he 2 1))
                          (setq rhesum (tk-inc rhesum s2 r_he)))
                        (tk-settext tbl r c "-"))
                      (setq c (1+ c))
                      (setq k (1+ k)))
                    (setq lst2 '())
                    (foreach item (list (cadr pair) (caddr pair))
                      (if (not (member (car item) counted))
                        (progn
                          (setq counted (cons (car item) counted))
                          (setq key (tk-ten-loai (cadr item)))
                          (setq lst2 (tk-inc lst2 key 1.0))
                          (setq nhomsum (tk-inc nhomsum key 1.0)))))
                    (foreach item nhomlist
                      (setq v (cdr (assoc item lst2)))
                      (tk-settext tbl r c (if v (rtos v 2 0) "-"))
                      (setq c (1+ c)))
                    (setq r (1+ r))
                    (setq cnt (1+ cnt))
                    ;; cuoi LO: dong tong lo = hieu snapshot
                    (if (and (> (length dmarks) 1)
                             (setq lo_nm (cdr (assoc li2 dends))))
                      (progn
                        (tk-dien-tong tbl r
                          (strcat "T\U+1ED4NG C\U+1ED8NG L\U+1ED8 " lo_nm)
                          (- sKC (nth 0 snap2))
                          (- sLX (nth 1 snap2))
                          (- sTC (nth 2 snap2))
                          (tk-alist-hieu capsum (nth 3 snap2))
                          (tk-alist-hieu ongsum (nth 4 snap2))
                          (tk-alist-hieu nhomsum (nth 5 snap2))
                          (tk-alist-hieu qdsum (nth 6 snap2))
                          (tk-alist-hieu rqdsum (nth 7 snap2))
                          (tk-alist-hieu rhesum (nth 8 snap2))
                          (- crsum (nth 9 snap2)))
                        (setq r (1+ r))))
                    (setq li2 (1+ li2)))
                  ;; dem not thiet bi dung le chua noi day
                  (foreach item nodes
                    (if (not (member (car item) counted))
                      (progn
                        (setq counted (cons (car item) counted))
                        (setq nhomsum
                          (tk-inc nhomsum (tk-ten-loai (cadr item)) 1.0)))))
                  (tk-dien-tong tbl r
                    (if (> (length dmarks) 1)
                      "T\U+1ED4NG C\U+1ED8NG TO\U+00C0N B\U+1ED8"
                      "TỔNG CỘNG")
                    sKC sLX sTC capsum ongsum nhomsum qdsum rqdsum rhesum
                    crsum)
                  ;; can chu vao o: ep TUNG O can giua (5), de muc trai (4)
                  (setq r 0)
                  (while (< r nr)
                    (setq c 0)
                    (while (< c nc)
                      (vl-catch-all-apply 'vla-SetCellAlignment
                        (list tbl r c 5))
                      (setq c (1+ c)))
                    (setq r (1+ r)))
                  (foreach v sec_rows
                    (vl-catch-all-apply 'vla-SetCellAlignment
                      (list tbl v 0 4)))
                  ;; bat lai ve bang sau khi dien xong
                  (vl-catch-all-apply
                    '(lambda ()
                       (vla-put-RegenerateTableSuppressed tbl :vlax-false)))
                  (princ
                    (strcat "\n[SUCCESS] Bang chi tiet tuyen DIEN: "
                            (itoa (length spans)) " khoang | "
                            (itoa (length laylist)) " loai day | "
                            (itoa (length nhomlist))
                            " nhom thiet bi. Dung XBE de sang Excel."))))))))))
  (princ))

;; V115: TKDEM da bo khoi command namespace.
;; Ham tk-dem-vannang van duoc giu noi bo vi T-THDIEN/T-THCS can dung engine nay.

;; ================================================================
;; 2 LENH CHI DINH THANG CHE DO - namespace cong ty T-:
;;   T-THCS   = THong ke Chieu Sang
;;   T-THDIEN = THong ke DIEN
;; KHONG tu doan: goi lenh nao chay dung nhanh do, du vung chon co
;; block la (vd block mang tag NAME) hay khong.
;; Lenh TKTONGHOP cu DA BO - than lenh gio la ham noi bo
;; tk-chay-thongke, chi 2 lenh tren goi toi.
;; Khoa luu thong so trong file DWG cung da doi sang "TK_THONGSO"
;; (khong con dinh ten lenh cu). Ban ve khai bang phien ban truoc
;; se can khai lai 1 lan bang T-THONGSO.
;; ================================================================
;; ================================================================
;; COMMAND PUBLIC V118 - TOAN BO BAT DAU BANG T-
;; ================================================================
(defun c:T-THONGSO (/ ch)
  (initget "ChieuSang Dien")
  (setq ch (getkword "\n[T-THONGSO] Chon nhom thong so [ChieuSang/Dien] <ChieuSang>: "))
  (cond
    ((or (null ch) (= ch "ChieuSang")) (tk-nhap-thongso))
    ((= ch "Dien") (tk-nhap-thongso-dien)))
  (princ))

;; ========================================================================
;; T-KIEMTRA / VUNG - QA/QC V1
;; Muc tieu V118: kiem tra du lieu THIET BI truoc khi boc khoi luong.
;; CHUA kiem tra topo/tuyen cap phuc tap o phien ban nay.
;;
;; Rule V1:
;;   QA01 ERROR - Thieu ID/ten thiet bi co nghia.
;;   QA02 ERROR - Con placeholder (TEN_COT, TEN_TU, THONG_SO...).
;;   QA03 ERROR - Block CO khai bao tag cha (TBA/PARENT) nhung de trong/placeholder.
;;   QA04 ERROR - DEVICE_ID/MA_TB chuan bi trung.
;;   QA05 WARN  - Ten cu chi canh bao trung khi CUNG TAG + CUNG TEN + CUNG TBA/PARENT.
;;   QA06 WARN  - Phai dung FALLBACK vi block chua co TAG ID chuan.
;;
;; Rotation/mirror KHONG phai loi. V114 da doc theo TAG/LOCAL coordinate.
;; Chi canh bao khi block phai dung FALLBACK, vi do la du lieu chua du chuan.
;; ========================================================================

(defun tk-qa-token (s / r i ch)
  ;; Chuan hoa chuoi de nhan biet placeholder, khong dung de thay doi ID that.
  (setq s (strcase (vl-string-trim " \t\r\n" (if s s "")))
        r ""
        i 1)
  (while (<= i (strlen s))
    (setq ch (substr s i 1))
    (if (not (member ch '(" " "_" "-" "." ":" "/" "\\" "(" ")" "[" "]")))
      (setq r (strcat r ch)))
    (setq i (1+ i)))
  r)

(defun tk-qa-placeholder-p (tag txt / a b)
  (setq a (tk-qa-token tag)
        b (tk-qa-token txt))
  (and (/= b "")
       (or (= a b)
           (member b
             '("TENCOT" "TENTU" "TENTBA" "TENTRAM" "TENTHIETBI"
               "THONGSO" "DEVICEID" "MATB" "PARENTID" "TBASO"
               "NAME" "PLACEHOLDER" "XXX" "TEST" "MAU")))))

(defun tk-qa-att-first (atts tags / item tag txt res)
  ;; Tra ve (TAG VALUE) dau tien co gia tri theo danh sach uu tien.
  (setq res nil)
  (foreach item atts
    (if (null res)
      (progn
        (setq tag (strcase (vla-get-TagString item))
              txt (vl-string-trim " \t\r\n" (vla-get-TextString item)))
        (if (and (member tag tags) (/= txt ""))
          (setq res (list tag txt))))))
  res)

(defun tk-qa-has-tag-p (atts tags / ok item tag)
  (setq ok nil)
  (foreach item atts
    (if (null ok)
      (progn
        (setq tag (strcase (vla-get-TagString item)))
        (if (member tag tags) (setq ok T)))))
  ok)

(defun tk-qa-effective-name (obj / nm)
  (setq nm (vl-catch-all-apply 'vla-get-EffectiveName (list obj)))
  (if (vl-catch-all-error-p nm) (setq nm (vla-get-Name obj)))
  nm)

(defun tk-qa-point (obj)
  (vlax-safearray->list
    (vlax-variant-value (vla-get-InsertionPoint obj))))

(defun tk-qa-join (lst sep / out)
  (setq out "")
  (foreach x lst
    (setq out (if (= out "") x (strcat out sep x))))
  out)

(defun tk-qa-issue-add (sev code obj id msg issues / pt nm lay h)
  (setq pt  (tk-qa-point obj)
        nm  (tk-qa-effective-name obj)
        lay (vla-get-Layer obj)
        h   (vla-get-Handle obj))
  (cons (list sev code (if id id "") nm lay h pt msg) issues))

(defun tk-qa-print-issue (it / sev code id nm lay h pt msg)
  (setq sev  (nth 0 it)
        code (nth 1 it)
        id   (nth 2 it)
        nm   (nth 3 it)
        lay  (nth 4 it)
        h    (nth 5 it)
        pt   (nth 6 it)
        msg  (nth 7 it))
  (princ
    (strcat "\n[" sev "] " code
            " | " (if (= id "") "ID=?" (strcat "ID=" id))
            " | block=" nm
            " | layer=" lay
            " | H=" h
            " | (" (rtos (car pt) 2 2) "," (rtos (cadr pt) 2 2) ")"
            " | " msg)))

(defun tk-qa-add-seen (key rec seen / cur)
  ;; seen: ((KEY rec rec...) ...)
  (setq cur (assoc key seen))
  (if cur
    (subst (cons key (cons rec (cdr cur))) cur seen)
    (cons (list key rec) seen)))

(defun tk-cmd-kiemtra-vung
       (/ ss i obj atts identry pentry info id idtag parent parenttags idtags
          phlist item tag txt issues seen cur rec grp recs sev srcs
          total natt nok nfallback nrot nmirror nerr nwarn rot xs ys
          *error*)
  (vl-load-com)
  (setq idtags
    '("DEVICE_ID" "MA_TB" "TEN_TU" "TEN_COT" "TEN_TRAM"
      "TEN_THIET_BI" "TENTHIETBI" "TENCOT" "NAME")
        parenttags '("PARENT_ID" "TEN_TBA" "TBA" "TBA_SO" "PARENT")
        total 0 natt 0 nok 0 nfallback 0 nrot 0 nmirror 0
        nerr 0 nwarn 0 issues nil seen nil)

  (princ "\n=== T-KIEMTRA / VUNG V1 ===")
  (princ "\nQuet/khoanh VUNG can kiem tra (Window/Crossing/Polygon tuy y): ")
  ;; V118: chi kiem tra block INSERT trong vung NGUOI DUNG chon.
  ;; Khong quet _X/toan Model Space de tranh bi nhieu boi block rac/legend.
  (setq ss (ssget '((0 . "INSERT"))))
  (if (null ss)
    (princ "\nKhong chon duoc block INSERT nao trong vung.")
    (progn
      (setq i 0)
      (repeat (sslength ss)
        (setq obj (vlax-ename->vla-object (ssname ss i))
              total (1+ total))
        (if (= (vla-get-HasAttributes obj) :vlax-true)
          (progn
            (setq natt (1+ natt)
                  atts (tk-get-atts-safe obj)
                  identry (if atts (tk-qa-att-first atts idtags) nil)
                  idtag   (if identry (car identry) nil)
                  id      (if identry (cadr identry) nil)
                  pentry  (if atts (tk-qa-att-first atts parenttags) nil)
                  parent  (if pentry (cadr pentry) nil)
                  phlist nil)

            ;; Thong ke rotation/mirror de xac nhan engine doc on, KHONG coi la loi.
            (setq rot (if (vlax-property-available-p obj 'Rotation)
                        (vla-get-Rotation obj) 0.0)
                  xs  (if (vlax-property-available-p obj 'XScaleFactor)
                        (vla-get-XScaleFactor obj) 1.0)
                  ys  (if (vlax-property-available-p obj 'YScaleFactor)
                        (vla-get-YScaleFactor obj) 1.0))
            (if (> (abs rot) 1e-9) (setq nrot (1+ nrot)))
            (if (or (< xs 0.0) (< ys 0.0)) (setq nmirror (1+ nmirror)))

            ;; QA02: tim placeholder o BAT KY attribute co gia tri.
            (foreach item atts
              (setq tag (strcase (vla-get-TagString item))
                    txt (vl-string-trim " \t\r\n" (vla-get-TextString item)))
              (if (and (/= txt "") (tk-qa-placeholder-p tag txt))
                (setq phlist (cons (strcat tag "='" txt "'") phlist))))
            (if phlist
              (progn
                (setq issues
                  (tk-qa-issue-add
                    "ERROR" "QA02_PLACEHOLDER" obj id
                    (strcat "Con du lieu mau: " (tk-qa-join (reverse phlist) ", "))
                    issues))
                (setq nerr (1+ nerr))))

            ;; QA01 + QA06: ID semantic.
            ;; Placeholder da duoc QA02 bao 1 lan, KHONG bao them QA01 de tranh rac.
            (cond
              ((or (null id) (= (vl-string-trim " " id) ""))
               (setq info (if atts (tk-dien-read-labels obj atts) nil))
               (if (and info (car info)
                        (not (tk-qa-placeholder-p "DEVICE_ID" (car info))))
                 (progn
                   (setq id (car info)
                         idtag "FALLBACK"
                         nfallback (1+ nfallback)
                         issues
                           (tk-qa-issue-add
                             "WARN" "QA06_FALLBACK_ID" obj id
                             "Chua co TAG ID chuan; dang suy ten bang fallback local-coordinate."
                             issues)
                         nwarn (1+ nwarn)))
                 (progn
                   (setq issues
                     (tk-qa-issue-add
                       "ERROR" "QA01_MISSING_ID" obj id
                       "Khong tim thay ID/ten thiet bi co nghia."
                       issues))
                   (setq nerr (1+ nerr)))))
              ((tk-qa-placeholder-p idtag id)
               nil) ; QA02 da bao placeholder
              (T
               (setq nok (1+ nok))))

            ;; QA03: neu block DA KHAI BAO field parent/TBA thi field do bat buoc co gia tri co nghia.
            ;; Khong ep block khong co parent-tag phai co parent (vd cot den thong thuong).
            (if (and atts (tk-qa-has-tag-p atts parenttags)
                     (or (null parent) (= (vl-string-trim " " parent) "")
                         (and pentry (tk-qa-placeholder-p (car pentry) parent))))
              (progn
                (setq issues
                  (tk-qa-issue-add
                    "ERROR" "QA03_MISSING_PARENT" obj id
                    "Block co khai bao TBA/PARENT nhung gia tri dang trong hoac placeholder."
                    issues))
                (setq nerr (1+ nerr))))

            ;; Luu ID hop le de kiem tra trung. Bo qua placeholder/missing.
            ;; V118:
            ;; - DEVICE_ID/MA_TB: khoa chuan, duy nhat trong vung -> gom theo ID.
            ;; - TEN_TU/TEN_COT/...: chi coi la trung khi CUNG TAG + CUNG TEN + CUNG PARENT/TBA.
            (if (and id (/= (vl-string-trim " " id) "")
                     idtag (/= idtag "FALLBACK")
                     (not (tk-qa-placeholder-p idtag id)))
              (progn
                (setq rec (list idtag id parent obj))
                (if (or (= idtag "DEVICE_ID") (= idtag "MA_TB"))
                  (setq seen
                    (tk-qa-add-seen
                      (strcat "STD|" (strcase (vl-string-trim " " id)))
                      rec seen))
                  (setq seen
                    (tk-qa-add-seen
                      (strcat "LEG|" idtag "|"
                              (strcase (vl-string-trim " " id)) "|"
                              (strcase (vl-string-trim " " (if parent parent ""))))
                      rec seen)))))))
        (setq i (1+ i)))

      ;; QA04/QA05 - xu ly duplicate theo nhom sau khi quet xong.
      ;; rec = (IDTAG ID PARENT OBJ)
      (foreach grp seen
        (setq recs (cdr grp))
        (if (> (length recs) 1)
          (progn
            (setq srcs (mapcar 'car recs))
            ;; DEVICE_ID/MA_TB la khoa chuan -> trung ID la ERROR, bat ke parent.
            (if (or (member "DEVICE_ID" srcs) (member "MA_TB" srcs))
              (progn
                (setq sev "ERROR" nerr (1+ nerr))
                (setq issues
                  (tk-qa-issue-add
                    sev "QA04_DUPLICATE_DEVICE_ID" (nth 3 (car recs)) (cadr (car recs))
                    (strcat "ID chuan bi trung " (itoa (length recs)) " lan. Handles: "
                            (tk-qa-join (mapcar '(lambda (r) (vla-get-Handle (nth 3 r))) recs) ",")
                            ". Can sua truoc khi boc.")
                    issues)))
              (progn
                ;; Legacy group da duoc tao theo TAG + ID + PARENT/TBA.
                ;; Vi vay chi canh bao khi ca 3 thanh phan nay giong nhau.
                (setq sev "WARN" nwarn (1+ nwarn))
                (setq issues
                  (tk-qa-issue-add
                    sev "QA05_DUPLICATE_LEGACY_NAME" (nth 3 (car recs)) (cadr (car recs))
                    (strcat "Ten cu trung trong cung pham vi logic: TAG=" (car (car recs))
                            ", PARENT=" (if (nth 2 (car recs)) (nth 2 (car recs)) "(trong)")
                            ", so lan=" (itoa (length recs)) ". Handles: "
                            (tk-qa-join (mapcar '(lambda (r) (vla-get-Handle (nth 3 r))) recs) ",")
                            ". Can xem lai; chua ket luan sai neu day la detail/legend co chu y.")
                    issues)))))))

      ;; Luu ket qua cho cac phien ban sau co the dung lam gate, NHUNG V118 chua khoa T-THCS/T-THDIEN.
      (setq *TK-QA-Last-Errors* nerr
            *TK-QA-Last-Warnings* nwarn
            *TK-QA-Last-Pass* (= nerr 0))

      (princ "\n\n===== LOI BAT BUOC SUA =====")
      (foreach cur (reverse issues)
        (if (= (car cur) "ERROR") (tk-qa-print-issue cur)))
      (princ "\n\n===== CANH BAO CAN XEM =====")
      (foreach cur (reverse issues)
        (if (= (car cur) "WARN") (tk-qa-print-issue cur)))

      (princ "\n\n===== TONG KET T-KIEMTRA V1 =====")
      (princ (strcat "\nINSERT trong vung chon    : " (itoa total)))
      (princ (strcat "\nBlock co Attribute       : " (itoa natt)))
      (princ (strcat "\nID doc truc tiep bang TAG: " (itoa nok)))
      (princ (strcat "\nBlock dung FALLBACK       : " (itoa nfallback)))
      (princ (strcat "\nBlock co Rotation != 0    : " (itoa nrot) " (khong phai loi)"))
      (princ (strcat "\nBlock co scale am/mirror  : " (itoa nmirror) " (khong phai loi)"))
      (princ (strcat "\nERROR                     : " (itoa nerr)))
      (princ (strcat "\nWARNING                   : " (itoa nwarn)))
      (if (> nerr 0)
        (progn
          (princ "\nKET LUAN: VUNG CHON CHUA SACH DU LIEU. Nen sua cac ERROR truoc khi boc khoi luong vung nay.")
          (princ "\n          V118 CHUA tu dong khoa T-THCS/T-THDIEN de tranh anh huong quy trinh hien tai."))
        (princ "\nKET LUAN: PASS QA V1 CHO VUNG DA CHON. Co the boc khoi luong vung nay; WARNING neu co thi nen xem lai."))
      (princ "\n====================================")))
  (princ))

(defun c:T-KIEMTRA ()
  ;; V118: 1 lenh = 1 viec. Luon kiem tra dung vung nguoi dung chon.
  ;; Cac ham debug/Nhan cu chi giu noi bo, khong dua ra menu de tranh rac thao tac.
  (tk-cmd-kiemtra-vung)
  (princ))

(defun c:T-THCS ()   (setq *TK-CheDo* "CS")   (tk-chay-thongke))
(defun c:T-THDIEN () (setq *TK-CheDo* "DIEN") (tk-chay-thongke))


;;; ==========================================================================
;;;  T-VEDAYCS  (V118) - VE DUONG DAY NOI CAC COT DEN THEO DUNG THU TU BANG THCS
;;; --------------------------------------------------------------------------
;;;  CACH DUNG:  chay T-THCS nhu binh thuong  ->  go T-VEDAYCS
;;;  - Lay nguyen danh sach khoang *TK-Pairs* ma T-THCS vua dung de len bang,
;;;    nen day ve ra KHOP TUYET DOI voi bang khoi luong.
;;;  - Moi khoang ve 1 LWPOLYLINE RIENG (dung quy uoc tang 3: 1 duong lien
;;;    mach = 1 khoang). Pline cho phep chen them dinh de nan theo via he
;;;    ma VAN la 1 doi tuong -> khong bi tach thanh 2 khoang.
;;;    Nhanh treo (2.1, 2.1.1...) tu dong dung thu tu.
;;;  - CO ve doan tu dieu khien -> cot den dau tuyen (da nam san trong
;;;    *TK-Pairs* do T-THCS ghep).
;;;  - Khoang NAO DA CO DAY thi BO QUA, khong ve de. Ly do: tang 3 cong
;;;    ca 2 soi neu 1 khoang co 2 duong -> ve de la nhan doi khoi luong.
;;;  KHONG SUA mot dong logic nao cua phan chieu sang dang chay.
;;; ==========================================================================

(setq *TK-LayerDay* "TK-TUYEN-CS")   ; lop chua duong day do lenh nay ve ra
(setq *TK-MauDay*   4)               ; mau cua lop (4 = cyan)
(setq *TK-KieuDay*  "PLINE")         ; "PLINE" = LWPOLYLINE (nen dung) | "LINE" = doan thang

;; Ve 1 doan day giua 2 diem. Moi khoang van la 1 DOI TUONG RIENG.
;; Dung LWPOLYLINE de sau nay chen them dinh nan theo via he ma van la
;; 1 duong LIEN MACH -> tang 3 van tinh dung 1 khoang.
(defun tkvd-ve-doan (msp pa pb lay / arr ent)
  (if (= (strcase *TK-KieuDay*) "LINE")
    (setq ent (vla-AddLine msp (vlax-3d-point pa) (vlax-3d-point pb)))
    (progn
      (setq arr (vlax-make-safearray vlax-vbDouble '(0 . 3)))
      (vlax-safearray-fill arr (list (car pa) (cadr pa) (car pb) (cadr pb)))
      (setq ent (vla-AddLightWeightPolyline msp (vlax-make-variant arr)))
      ;; LWPOLYLINE la doi tuong 2D: dat cao do bang cao do cua diem dau
      (vla-put-Elevation ent (if (caddr pa) (caddr pa) 0.0))))
  (vla-put-Layer ent lay)
  ent)

;; Tao lop neu chua co, tra ve ten lop
(defun tkvd-tao-lop (ten mau / lay)
  (if (not (tblsearch "LAYER" ten))
    (progn
      (setq lay (vla-Add (vla-get-Layers
                           (vla-get-ActiveDocument (vlax-get-acad-object))) ten))
      (vla-put-Color lay mau)))
  ten)

;; Tim diem pt trong danh sach lst theo dung sai tol
(defun tkvd-tim (pt lst tol / found)
  (setq found nil)
  (foreach q lst
    (if (and (null found) (< (distance pt q) tol)) (setq found q)))
  found)

;; Gom danh sach diem cot (khong trung) tu danh sach khoang
(defun tkvd-danhsach-diem (prs / out p)
  (setq out '())
  (foreach pr prs
    (foreach nd pr
      (setq p (caddr nd))
      (if (and p (not (tkvd-tim p out 0.001)))
        (setq out (cons p out)))))
  (reverse out))

;; Diem cot GAN NHAT voi pt, chi nhan khi trong nguong *TK-DauPline*
;; (dung dung luat gan duong cua tang 3 nen khong bat nham sang khoang ke ben)
(defun tkvd-gan-nhat (pt lst / best bd d)
  (setq best nil bd 1e18)
  (foreach q lst
    (setq d (distance pt q))
    (if (< d bd) (setq bd d best q)))
  (if (< bd *TK-DauPline*) best nil))

;; 2 khoang co trung nhau khong (khong ke chieu ve)
(defun tkvd-cung-khoang (pa pb qa qb)
  (or (and (< (distance pa qa) 0.001) (< (distance pb qb) 0.001))
      (and (< (distance pa qb) 0.001) (< (distance pb qa) 0.001))))

(defun c:T-VEDAYCS (/ doc msp ss m obj plv pts a b occ prs pr pa pb co
                    n_ve n_bo n_loi n_z lay ent ten1 ten2)
  (vl-load-com)
  (if (null *TK-Pairs*)
    (progn
      (princ "\n[T-VEDAYCS] Chua co du lieu tuyen trong bo nho.")
      (princ "\n          Hay chay lenh T-THCS truoc (quet vung ban ve), xong roi go lai T-VEDAYCS.")
      (princ))
    (progn
      (setq doc (vla-get-ActiveDocument (vlax-get-acad-object))
            msp (vla-get-ModelSpace doc)
            prs *TK-Pairs*)
      (vla-StartUndoMark doc)
      (setq lay (tkvd-tao-lop *TK-LayerDay* *TK-MauDay*))
      (setq pts (tkvd-danhsach-diem prs))
      (princ (strcat "\n=== T-VEDAYCS: " (itoa (length prs)) " khoang, "
                     (itoa (length pts)) " diem cot/tu ==="))

      ;; --- B1. QUET DUONG DA CO -> LAP DANH SACH KHOANG DA CO DAY ---
      (setq occ '() n_loi 0
            ss  (ssget "_X" '((0 . "LWPOLYLINE,POLYLINE,LINE,SPLINE")))
            m   0)
      (if ss
        (repeat (sslength ss)
          (setq obj (vlax-ename->vla-object (ssname ss m)))
          (setq plv (vl-catch-all-apply
                      '(lambda (o) (list (vlax-curve-getStartPoint o)
                                         (vlax-curve-getEndPoint o)))
                      (list obj)))
          (if (not (vl-catch-all-error-p plv))
            (progn
              (setq a (tkvd-gan-nhat (car plv) pts)
                    b (tkvd-gan-nhat (cadr plv) pts))
              (if (and a b (> (distance a b) 0.001))
                (setq occ (cons (list a b) occ)))))
          (setq m (1+ m))))
      (princ (strcat "\n>> Da co san " (itoa (length occ)) " doan day tren ban ve."))

      ;; --- B2. VE TUNG KHOANG THEO DUNG THU TU CUA BANG ---
      (setq n_ve 0 n_bo 0 n_z 0)
      (foreach pr prs
        (setq pa   (caddr (car pr))
              pb   (caddr (cadr pr))
              ten1 (car (car pr))
              ten2 (car (cadr pr)))
        (if (and pa pb (> (distance pa pb) 0.001))
          (progn
            (setq co nil)
            (foreach o occ
              (if (and (null co) (tkvd-cung-khoang pa pb (car o) (cadr o)))
                (setq co T)))
            (if co
              (progn
                (setq n_bo (1+ n_bo))
                (princ (strcat "\n   [BO QUA] " ten1 " -> " ten2 " : da co day.")))
              (progn
                (setq ent (tkvd-ve-doan msp pa pb lay))
                (if (> (abs (- (if (caddr pa) (caddr pa) 0.0)
                               (if (caddr pb) (caddr pb) 0.0))) 0.001)
                  (setq n_z (1+ n_z)))
                ;; ghi vao occ ngay de khong ve trung trong cung 1 lan chay
                (setq occ  (cons (list pa pb) occ)
                      n_ve (1+ n_ve))
                (princ (strcat "\n   [VE] " ten1 " -> " ten2)))))
          (setq n_loi (1+ n_loi))))
      (vla-EndUndoMark doc)
      (princ (strcat "\n=== T-VEDAYCS XONG: ve moi " (itoa n_ve) " doan | bo qua "
                     (itoa n_bo) " doan da co day"
                     (if (> n_loi 0)
                       (strcat " | " (itoa n_loi) " khoang khong lay duoc toa do") "")
                     " | Kieu: " *TK-KieuDay*
                     " | Lop: " lay " ==="))
      (if (> n_z 0)
        (princ (strcat "\n[LUU Y] " (itoa n_z) " khoang co 2 dau LECH CAO DO. Pline la doi tuong 2D"
                       "\n         nen da lam phang theo cao do cot dau khoang."
                       "\n         Muon giu nguyen cao do 2 dau thi dat (setq *TK-KieuDay* \"LINE\").")))
      (princ "\n>> Buoc tiep theo: nan cac doan vua ve theo via he thuc te,")
      (princ "\n   roi chay lai T-THCS de tang 3 lay chieu dai THAT vao bang.")
      (princ))))

;;; ============================================================
;;; CDS - CONG DIM: chon cac Dimension, cong tong va thong ke theo TEN tren dim.
;;;  - Gia tri dim: so ghi de (text chi la so) -> lay so do; con lai -> gia tri do that (DXF 42).
;;;  - TEN dim: tim "DSTA nn" trong text dim (gom tien to/hau to), neu khong co thi trong ten LAYER;
;;;    neu van khong co thi lay text ghi de, cuoi cung la ten layer.
;;;  - Don vi: dim do bang don vi ve; "he so chia ra met" mac dinh 1 (ve theo m), 1000 neu ve theo mm.
;;;  - Xong in ket qua + hoi diem dat BANG (Enter = khong ve bang). Khong sua dim.
;;; ============================================================
(defun cds-numstr-p (s / i c ok dg)
  (setq s (vl-string-trim " " s) i 1 ok (/= s "") dg nil)
  (while (and ok (<= i (strlen s)))
    (setq c (substr s i 1))
    (cond ((and (>= c "0") (<= c "9")) (setq dg T))
          ((member c '("." "," "-" "+")) nil)
          (t (setq ok nil)))
    (setq i (1+ i)))
  (and ok dg))

(defun cds-str2num (s)
  (atof (vl-string-translate "," "." (vl-string-trim " " s))))

(defun cds-sortkey (s / i c out run)
  (setq i 1 out "" run "")
  (while (<= i (strlen s))
    (setq c (substr s i 1))
    (if (and (>= c "0") (<= c "9"))
      (setq run (strcat run c))
      (progn
        (if (/= run "")
          (setq out (strcat out (substr (strcat "000000" run) (- (+ (strlen run) 7) 6))) run ""))
        (setq out (strcat out c))))
    (setq i (1+ i)))
  (if (/= run "")
    (setq out (strcat out (substr (strcat "000000" run) (- (+ (strlen run) 7) 6)))))
  out)

(defun cds-dimname (txt lay / sp nm)
  (setq sp (vl-catch-all-apply 'Get-DSTA-Spec (list txt)))
  (if (or (vl-catch-all-error-p sp) (null sp))
    (progn (setq sp (vl-catch-all-apply 'Get-DSTA-Spec (list lay)))
           (if (vl-catch-all-error-p sp) (setq sp nil))))
  (cond
    (sp (vl-string-subst "" "mm2" sp))
    (t
     (setq nm (vl-catch-all-apply 'Clean-CAD-String (list txt)))
     (if (vl-catch-all-error-p nm) (setq nm txt))
     (while (vl-string-search "<>" nm)
       (setq nm (vl-string-subst "" "<>" nm)))
     (setq nm (vl-string-trim " " nm))
     (if (or (= nm "") (cds-numstr-p nm))
       (strcase lay)
       (strcase nm)))))

;; Tinh nhom: tra ve (list tong-dim danh-sach-nhom) ; nhom = (ten tong-m so-dim), da sap xep
;; Tien to/hau to (DIMPOST cua dimstyle, "<>" = vi tri so do) lay tu bang DIMSTYLE, khong can ActiveX.
(defun cds-dimpost (ent / sty rec p)
  (setq sty (cdr (assoc 3 ent)) rec (if sty (tblsearch "DIMSTYLE" sty)) p (if rec (cdr (assoc 3 rec))))
  (if p p ""))

(defun cds-collect (ss chia / i n ent txt1 full lay v nm cell groups all)
  (setq i 0 n (sslength ss) groups nil all 0.0)
  (while (< i n)
    (setq ent (entget (ssname ss i))
          txt1 (cdr (assoc 1 ent))
          lay (cdr (assoc 8 ent)))
    (if (null txt1) (setq txt1 ""))
    (setq v (cond
              ((or (= txt1 "") (vl-string-search "<>" txt1)) (cdr (assoc 42 ent)))
              ((cds-numstr-p txt1) (cds-str2num txt1))
              (t (cdr (assoc 42 ent)))))
    (if (null v) (setq v 0.0))
    (setq v (/ v chia))
    (setq full (strcat (cds-dimpost ent) " " txt1)
          nm (cds-dimname full lay)
          cell (assoc nm groups))
    (if cell
      (setq groups (subst (list nm (+ (cadr cell) v) (1+ (caddr cell))) cell groups))
      (setq groups (cons (list nm v 1) groups)))
    (setq all (+ all v) i (1+ i)))
  (list all
        (vl-sort groups '(lambda (a b) (< (cds-sortkey (car a)) (cds-sortkey (car b)))))))
(defun cds-table (ss groups all n pt h / doc blk tbl r cnt)
  (setq doc (vla-get-Document (vlax-ename->vla-object (ssname ss 0)))
        blk (vla-get-Block (vla-get-ActiveLayout doc))
        tbl (vla-AddTable blk (vlax-3d-point pt) (+ 3 (length groups)) 4 (* 2.2 h) (* 6.0 h)))
  (if (and (boundp '*TK-TextStyle*) *TK-TextStyle*)
    (vl-catch-all-apply 'vla-SetTextStyle (list tbl 7 *TK-TextStyle*)))
  (vla-SetColumnWidth tbl 0 (* 3.0 h))
  (vla-SetColumnWidth tbl 1 (* 12.0 h))
  (vla-SetColumnWidth tbl 2 (* 5.0 h))
  (vla-SetColumnWidth tbl 3 (* 8.0 h))
  (vla-SetTextHeight tbl 63 h)
  (vla-SetText tbl 0 0 "BANG THONG KE CHIEU DAI DIM")
  (vla-SetText tbl 1 0 "STT")
  (vla-SetText tbl 1 1 "TEN (THEO DIM)")
  (vla-SetText tbl 1 2 "SO DIM")
  (vla-SetText tbl 1 3 "CHIEU DAI (m)")
  (setq r 2 cnt 1)
  (foreach g groups
    (vla-SetText tbl r 0 (itoa cnt))
    (vla-SetText tbl r 1 (car g))
    (vla-SetText tbl r 2 (itoa (caddr g)))
    (vla-SetText tbl r 3 (rtos (cadr g) 2 2))
    (setq r (1+ r) cnt (1+ cnt)))
  (vla-SetText tbl r 0 "")
  (vla-SetText tbl r 1 "TONG CONG")
  (vla-SetText tbl r 2 (itoa n))
  (vla-SetText tbl r 3 (rtos all 2 2))
  tbl)

;; Phan loi + in ket qua; pt = diem dat bang hoac nil. Tra ve T neu xong.
(defun cds-run (ss chia h pt / res all groups msg tbl)
  (setq res (cds-collect ss chia)
        all (car res) groups (cadr res)
        msg (strcat "Tong " (itoa (sslength ss)) " Dim = " (rtos all 2 2) " m"))
  (princ (strcat "\nCDS: " msg))
  (foreach g groups
    (princ (strcat "\n   " (car g) " : " (rtos (cadr g) 2 2) " m (" (itoa (caddr g)) " dim)"))
    (setq msg (strcat msg "\n" (car g) " : " (rtos (cadr g) 2 2) " m")))
  (if pt
    (progn
      (setq tbl (vl-catch-all-apply 'cds-table (list ss groups all (sslength ss) pt h)))
      (if (vl-catch-all-error-p tbl)
        (princ (strcat "\nCDS: khong ve duoc bang: " (vl-catch-all-error-message tbl)))
        (princ "\nCDS: da ve bang thong ke."))))
  (alert msg)
  T)

(defun c:CDS (/ ss k h pt)
  (vl-load-com)
  (prompt "\nChon cac Dim can cong: ")
  (setq ss (ssget '((0 . "DIMENSION"))))
  (if (null ss)
    (princ "\nCDS: chua chon Dim nao.")
    (progn
      (if (null *CDS-Chia*) (setq *CDS-Chia* 1.0))
      (initget 6)
      (setq k (getreal (strcat "\nHe so chia ra met (ve theo m = 1, theo mm = 1000) <" (rtos *CDS-Chia* 2 0) ">: ")))
      (if k (setq *CDS-Chia* k))
      (setq pt (getpoint "\nClick diem dat BANG thong ke theo ten dim (Enter = khong ve bang): "))
      (if pt
        (progn
          (setq h (* (getvar "DIMTXT") (if (> (getvar "DIMSCALE") 0.0) (getvar "DIMSCALE") 1.0)))
          (if (<= h 0.0) (setq h 2.5))
          (initget 6)
          (setq k (getreal (strcat "\nChieu cao chu trong bang <" (rtos h 2 2) ">: ")))
          (if k (setq h k))))
      (vla-StartUndoMark (vla-get-ActiveDocument (vlax-get-acad-object)))
      (cds-run ss *CDS-Chia* h pt)
      (vla-EndUndoMark (vla-get-ActiveDocument (vlax-get-acad-object)))))
  (princ))
;; ==============================================================
;; T-KLHD: THONG KE KHOI LUONG HO DAN (BKL_CAP)
;; ==============================================================
﻿; TASK-0024 / TASK-0025+: Bảng bóc tách khối lượng đường dây 0,4kV sinh hoạt.
; Hỗ trợ cả bản vẽ tuyến ngang chuẩn (CAP DIEN SCT.dwg) và bản vẽ mặt bằng thực tế
; nhiều TBA, tuyến cong/nghiêng góc bất kỳ, giao chữ T, thiếu số nhà (tự giả định),
; nhãn nhân bội (N*DSTA...), cáp 3F/50, và tự nối khoảng trống thiếu Dim.
(vl-load-com)

(defun bklcap:trim (s)
  (vl-string-trim (strcat " " (chr 9) (chr 10) (chr 13)) s))

(defun bklcap:plain (s / i ch code end out)
  (setq i 1 out "")
  (while (<= i (strlen s))
    (setq ch (substr s i 1) i (1+ i))
    (cond
      ((member ch '("{" "}")))
      ((= ch "\\")
       (setq code (substr s i 1) i (1+ i))
       (cond
         ((member code '("P" "~")) (setq out (strcat out " ")))
         ((member code '("\\" "{" "}")) (setq out (strcat out code)))
         ((and (member code '("A" "C" "c" "F" "f" "H" "Q" "T" "W" "p"))
               (setq end (vl-string-search ";" s (1- i))))
          (setq i (+ end 2)))))
      (T (setq out (strcat out ch)))))
  (bklcap:trim out))

(defun bklcap:fail (message)
  (setq validation-message message)
  (exit))

(defun bklcap:dim-length (data / s n result)
  (setq s (bklcap:plain (cond ((cdr (assoc 1 data))) (T ""))))
  (if (member s '("" "<>"))
    (progn
      (setq n (cdr (assoc 42 data)))
      (if (and n (> n 0)) (setq result (itoa (max 1 (fix (+ n 0.5)))))))
    (progn
      (setq s (strcase s T))
      (if (= (substr s (strlen s)) "m")
        (setq s (bklcap:trim (substr s 1 (1- (strlen s))))))
      (setq s (vl-string-translate "," "." s))
      (if (and (/= s "") (not (wcmatch s "*[~0-9.]*"))
               (not (wcmatch s "*.*.*")) (> (atof s) 0))
        (progn
          (if (= (substr s 1 1) ".") (setq s (strcat "0" s)))
          (setq result (vl-string-right-trim "." s))))))
  (if result result
    (progn
      (setq n (cdr (assoc 42 data)))
      (if (and n (> n 0)) (itoa (max 1 (fix (+ n 0.5))))
        (bklcap:fail (strcat "Dim thiếu chiều dài hợp lệ (handle " (cdr (assoc 5 data)) ")."))))))

(defun bklcap:attr (attrs tag / value)
  (setq value (cdr (assoc tag attrs)))
  (if (and value (/= (bklcap:trim value) "")) (bklcap:trim value)))

(defun bklcap:block-name (name / owner tag original)
  ; Dynamic block references can point to an anonymous block definition.
  (if (and (wcmatch name "`**")
           (setq owner (cdr (assoc 330 (entget (tblobjname "BLOCK" name)))))
           (setq tag (assoc -3 (entget owner '("AcDbBlockRepBTag"))))
           (setq original (handent (cdr (assoc 1005 (cdadr tag))))))
    (cdr (assoc 2 (entget original)))
    name))

(defun bklcap:cabinet-key (name)
  (strcase (vl-string-translate "Ððđ" "ĐđĐ" name)))

(defun bklcap:cabinet (entity data / block attrs next record name station raw-tba)
  (setq block (cdr (assoc 2 data)))
  (if (= (strcase (bklcap:block-name block)) "TU CONG TO")
    (progn
      (if (equal (cdr (assoc 66 data)) 1)
        (progn
          (setq next (entnext entity))
          (while (and next (= (cdr (assoc 0 (setq record (entget next)))) "ATTRIB"))
            (setq attrs (cons (cons (strcase (cdr (assoc 2 record)))
                                    (bklcap:plain (cdr (assoc 1 record)))) attrs)
                  next (entnext next)))))
      (setq next (cdr (assoc -2 (tblsearch "BLOCK" block))))
      (while (and next (/= (cdr (assoc 0 (setq record (entget next)))) "ENDBLK"))
        (if (and (= (cdr (assoc 0 record)) "ATTDEF")
                 (= (logand (cdr (assoc 70 record)) 2) 2)
                 (not (assoc (strcase (cdr (assoc 2 record))) attrs)))
          (setq attrs (cons (cons (strcase (cdr (assoc 2 record)))
                                  (bklcap:plain (cdr (assoc 1 record)))) attrs)))
        (setq next (entnext next)))
      (setq name (bklcap:attr attrs "TEN_TU")
            station (bklcap:attr attrs "TEN_TBA"))
      (if (not station) (setq station "CHUNG"))
      (setq raw-tba (vl-string-translate "Ðð" "Đđ" station))
      (if (wcmatch (strcase raw-tba) "TBA *")
        (setq station (bklcap:trim (substr raw-tba 5)))
        (setq station raw-tba))
      (if (not (and name (/= name "") (/= station "")))
        (bklcap:fail "Block Tu Cong To thiếu TEN_TU hoặc TEN_TBA."))
      (list (trans (cdr (assoc 10 data)) entity 0)
            (strcat (vl-string-translate "Ðð" "Đđ" name) "/" station)
            (vl-string-translate "Ðð" "Đđ" name)
            station))))

(defun bklcap:parse-mult (body / idx ch digits rest)
  ; Tách tiền tố nhân bội như "3*DSTA2X10" -> (list 3 "DSTA2X10")
  (setq idx 1 digits "")
  (while (and (<= idx (strlen body))
              (wcmatch (setq ch (substr body idx 1)) "[0-9]"))
    (setq digits (strcat digits ch) idx (1+ idx)))
  (if (and (/= digits "")
           (<= idx (strlen body))
           (member (substr body idx 1) '("*" "X"))
           (setq rest (substr body (1+ idx)))
           (or (wcmatch rest "DSTA*") (wcmatch rest "HDPE*")))
    (list (max 1 (atoi digits)) rest)
    (list 1 body)))

(defun bklcap:text (entity data / s key p number cut phase field close-tag cabinet-tag body mult parsed note)
  (setq s "")
  (foreach field data
    (if (member (car field) '(1 3)) (setq s (strcat s (cdr field)))))
  (setq s (bklcap:plain s)
        key (strcase (vl-string-translate "ốàÐð" "ỐÀĐĐ" s))
        p (cdr (assoc 10 data)))
  (if (= (cdr (assoc 0 data)) "TEXT") (setq p (trans p entity 0)))
  (if (and (= (cdr (assoc 0 data)) "TEXT") (assoc 11 data)
           (or (> (cond ((cdr (assoc 72 data))) (T 0)) 0)
               (> (cond ((cdr (assoc 73 data))) (T 0)) 0)))
    (setq p (trans (cdr (assoc 11 data)) entity 0)))
  (cond
    ((wcmatch key "SỐ NHÀ *,SO NHA *")
     (setq number (bklcap:trim (substr key 8)))
     (if (not (and (wcmatch number "#*") (not (wcmatch number "*[~0-9]*"))
                   (> (atoi number) 0)))
       (bklcap:fail (strcat "Số nhà không hợp lệ: " s)))
     (list 'house p (atoi number) (strcat "Số nhà " number)))
    ((and (not (vl-string-search ":" key))
          (or (wcmatch key "DSTA*") (wcmatch key "HDPE*")
              (wcmatch key "#*[`*]DSTA*") (wcmatch key "#*XDSTA*")
              (wcmatch key "#*[`*]HDPE*") (wcmatch key "#*XHDPE*")))
     (setq cut (vl-string-search "(" key))
     (if (and cut (setq close-tag (vl-string-search ")" key (1+ cut))))
       (setq cabinet-tag (bklcap:trim (substr s (+ cut 2) (- close-tag cut 1)))))
     (setq body (vl-string-translate "*×" "XX"
                  (vl-list->string
                    (vl-remove 32 (vl-string->list (if cut (substr key 1 cut) key)))))
           parsed (bklcap:parse-mult body)
           mult (car parsed)
           body (cadr parsed))
     (setq phase (cond
                   ((= body "DSTA2X10") 1)
                   ((member body '("DSTA16" "DSTA3X16+1X10")) 3)
                   ((wcmatch body "HDPE*") 0)
                   ((wcmatch body "DSTA*") 3)))
     (if (and phase (wcmatch body "DSTA*") (not (member body '("DSTA2X10" "DSTA16" "DSTA3X16+1X10"))))
       (setq note (if cut (bklcap:trim (substr s 1 cut)) s)))
     (if phase
       (list 'cable p phase s cabinet-tag mult note)))))

(defun bklcap:xy (a b)
  (distance (list (car a) (cadr a)) (list (car b) (cadr b))))

(defun bklcap:poly-segs (data layer / pts segs prev cur d)
  (if (= (strcase layer) "0")
    (progn
      (setq pts (mapcar 'cdr (vl-remove-if-not '(lambda (x) (= (car x) 10)) data)))
      (if (and (= (logand 1 (cond ((cdr (assoc 70 data))) (T 0))) 1) (> (length pts) 2))
        (setq pts (append pts (list (car pts)))))
      (setq prev (car pts) pts (cdr pts))
      (foreach cur pts
        (setq d (bklcap:xy prev cur))
        (if (and (> d 0.1) (< d 500.0))
          (setq segs (cons (list prev cur nil d (itoa (max 1 (fix (+ d 0.5)))) nil) segs)))
        (setq prev cur))))
  segs)

(defun bklcap:collect (selection / i entity data kind record a b m42 gdist dims polys cabinets houses cables segs)
  (setq i 0)
  (while (< i (sslength selection))
    (setq entity (ssname selection i) i (1+ i)
          data (entget entity) kind (cdr (assoc 0 data)))
    (cond
      ((and (= kind "INSERT") (setq record (bklcap:cabinet entity data)))
       (setq cabinets (cons record cabinets)))
      ((and (= kind "DIMENSION")
            (member (logand 7 (cdr (assoc 70 data))) '(0 1))
            (setq a (cdr (assoc 13 data)) b (cdr (assoc 14 data)))
            (setq a (list (car a) (cadr a)) b (list (car b) (cadr b)))
            (setq m42 (cdr (assoc 42 data)))
            (setq gdist (bklcap:xy a b))
            (> gdist 0.05) (< gdist 500.0)
            (or (null m42) (and (> m42 0.0) (< m42 500.0))))
       (setq dims (cons (list a b data (if (and m42 (> m42 0)) m42 gdist) (bklcap:dim-length data) T) dims)))
      ((= kind "LWPOLYLINE")
       (setq segs (bklcap:poly-segs data (cdr (assoc 8 data))))
       (if segs (setq polys (append segs polys))))
      ((and (= kind "LINE") (= (strcase (cdr (assoc 8 data))) "0"))
       (setq a (list (cadr (assoc 10 data)) (caddr (assoc 10 data)))
             b (list (cadr (assoc 11 data)) (caddr (assoc 11 data)))
             gdist (bklcap:xy a b))
       (if (and (> gdist 0.1) (< gdist 500.0))
         (setq polys (cons (list a b nil gdist (itoa (max 1 (fix (+ gdist 0.5)))) nil) polys))))
      ((and (member kind '("TEXT" "MTEXT")) (setq record (bklcap:text entity data)))
       (if (= (car record) 'house)
         (setq houses (cons (cdr record) houses))
         (setq cables (cons (cdr record) cables))))))
  (list dims polys cabinets houses cables))

(defun bklcap:seg-dist (p a b / dx dy len2 tval qx qy)
  ; Trả về (list dist t qpt) từ điểm p đến đoạn thẳng a-b
  (setq dx (- (car b) (car a))
        dy (- (cadr b) (cadr a))
        len2 (+ (* dx dx) (* dy dy)))
  (if (<= len2 1e-9)
    (list (bklcap:xy p a) 0.0 a)
    (progn
      (setq tval (/ (+ (* (- (car p) (car a)) dx) (* (- (cadr p) (cadr a)) dy)) len2)
            tval (max 0.0 (min 1.0 tval))
            qx (+ (car a) (* tval dx))
            qy (+ (cadr a) (* tval dy)))
      (list (bklcap:xy p (list qx qy)) tval (list qx qy)))))

(defun bklcap:split-t-junctions (dims / endpoints out s a b m42 lenStr isDim cuts p minx maxx miny maxy
                                      res uniq prevT prevP c subLen)
  (foreach s dims
    (setq endpoints (cons (car s) (cons (cadr s) endpoints))))
  (foreach s dims
    (setq a (car s) b (cadr s) m42 (cadddr s) lenStr (nth 4 s) isDim (nth 5 s)
          minx (- (min (car a) (car b)) 1.3) maxx (+ (max (car a) (car b)) 1.3)
          miny (- (min (cadr a) (cadr b)) 1.3) maxy (+ (max (cadr a) (cadr b)) 1.3)
          cuts nil)
    (foreach p endpoints
      (if (and (>= (car p) minx) (<= (car p) maxx)
               (>= (cadr p) miny) (<= (cadr p) maxy)
               (> (bklcap:xy p a) 0.4) (> (bklcap:xy p b) 0.4))
        (progn
          (setq res (bklcap:seg-dist p a b))
          (if (and (<= (car res) 1.2) (> (cadr res) 0.02) (< (cadr res) 0.98))
            (setq cuts (cons (cons (cadr res) (caddr res)) cuts))))))
    (if (not cuts)
      (setq out (cons s out))
      (progn
        (setq cuts (vl-sort cuts '(lambda (u v) (< (car u) (car v))))
              uniq nil)
        (foreach c cuts
          (if (or (not uniq) (> (- (car c) (caar uniq)) 0.02))
            (setq uniq (cons c uniq))))
        (setq uniq (reverse uniq) prevT 0.0 prevP a)
        (foreach c uniq
          (setq subLen (* m42 (- (car c) prevT)))
          (setq out (cons (list prevP (cdr c) nil subLen (itoa (max 1 (fix (+ subLen 0.5)))) isDim) out)
                prevT (car c) prevP (cdr c)))
        (setq subLen (* m42 (- 1.0 prevT)))
        (setq out (cons (list prevP b nil subLen (itoa (max 1 (fix (+ subLen 0.5)))) isDim) out)))))
  (reverse out))

(defun bklcap:seg-dup-p (ps segs / pa pb s sa sb found)
  (setq pa (car ps) pb (cadr ps) found nil)
  (while (and segs (not found))
    (setq s (car segs) segs (cdr segs) sa (car s) sb (cadr s))
    (if (or (and (< (bklcap:xy pa sa) 0.6) (< (bklcap:xy pb sb) 0.6))
            (and (< (bklcap:xy pa sb) 0.6) (< (bklcap:xy pb sa) 0.6)))
      (setq found T)))
  found)

(defun bklcap:find-or-add-node (pt nodesList / n found id)
  (setq found nil)
  (foreach n nodesList
    (if (and (not found)
             (<= (abs (- (car pt) (caadr n))) 0.4)
             (<= (abs (- (cadr pt) (cadadr n))) 0.4)
             (<= (bklcap:xy pt (cadr n)) 0.4))
      (setq found (car n))))
  (if found
    (list found nodesList)
    (progn
      (setq id (length nodesList))
      (list id (cons (list id pt) nodesList)))))

(defun bklcap:add-adj (adj u item / entry)
  (if (setq entry (assoc u adj))
    (subst (cons u (cons item (cdr entry))) entry adj)
    (cons (cons u (list item)) adj)))

(defun bklcap:build-graph (dims polys / splitDims allSegs ps nodesList res u v segId segMap
                                      s w adj i j pi pj nNodes nodeX nodeY degArr)
  (setq splitDims (bklcap:split-t-junctions dims)
        allSegs splitDims)
  (foreach ps polys
    (if (not (bklcap:seg-dup-p ps splitDims))
      (setq allSegs (append allSegs (list ps)))))
  (setq segId 0)
  (foreach s allSegs
    (setq res (bklcap:find-or-add-node (car s) nodesList)
          u (car res) nodesList (cadr res)
          res (bklcap:find-or-add-node (cadr s) nodesList)
          v (car res) nodesList (cadr res))
    (if (/= u v)
      (progn
        (setq w (max 0.05 (- (* (cadddr s) (if (nth 5 s) 1.0 1.5)) 0.3))
              segMap (cons (cons segId s) segMap)
              adj (bklcap:add-adj adj u (list v segId w))
              adj (bklcap:add-adj adj v (list u segId w))
              segId (1+ segId)))))
  (setq nodesList (reverse nodesList)
        nNodes (length nodesList)
        nodeX (vlax-make-safearray vlax-vbDouble (cons 0 (1- nNodes)))
        nodeY (vlax-make-safearray vlax-vbDouble (cons 0 (1- nNodes)))
        degArr (vlax-make-safearray vlax-vbLong (cons 0 (1- nNodes))))
  (foreach n nodesList
    (vlax-safearray-put-element nodeX (car n) (caadr n))
    (vlax-safearray-put-element nodeY (car n) (cadadr n))
    (vlax-safearray-put-element degArr (car n) (length (cdr (assoc (car n) adj)))))
  ; Cầu nối nhỏ <= 1.2m giữa các node sát nhau (hở mép rãnh 0.94m)
  (setq i 0)
  (while (< i nNodes)
    (setq pi (list (vlax-safearray-get-element nodeX i) (vlax-safearray-get-element nodeY i))
          j (1+ i))
    (while (< j nNodes)
      (setq pj (list (vlax-safearray-get-element nodeX j) (vlax-safearray-get-element nodeY j)))
      (if (and (<= (abs (- (car pi) (car pj))) 1.2)
               (<= (abs (- (cadr pi) (cadr pj))) 1.2)
               (<= (bklcap:xy pi pj) 1.2))
        (setq adj (bklcap:add-adj adj i (list j -2 0.01))
              adj (bklcap:add-adj adj j (list i -2 0.01))))
    (setq j (1+ j)))
    (setq i (1+ i)))
  (list nodeX nodeY degArr adj segMap splitDims allSegs nNodes))

(defun bklcap:node-pt (nodeId nodeX nodeY)
  (list (vlax-safearray-get-element nodeX nodeId)
        (vlax-safearray-get-element nodeY nodeId)))

(defun bklcap:nearest-node-id (pt nodeX nodeY nNodes / i best bestD pi d)
  (setq i 0 best 0 bestD 1e12)
  (while (< i nNodes)
    (setq pi (list (vlax-safearray-get-element nodeX i) (vlax-safearray-get-element nodeY i))
          d (bklcap:xy pt pi))
    (if (< d bestD) (setq bestD d best i))
    (setq i (1+ i)))
  (list best bestD))

(defun bklcap:dijkstra (startId adj nNodes / distArr prevNodeArr prevSegArr visArr open cur curD next e to sid w nd reached i tmp)
  (setq distArr (vlax-make-safearray vlax-vbDouble (cons 0 (1- nNodes)))
        prevNodeArr (vlax-make-safearray vlax-vbLong (cons 0 (1- nNodes)))
        prevSegArr (vlax-make-safearray vlax-vbLong (cons 0 (1- nNodes)))
        visArr (vlax-make-safearray vlax-vbLong (cons 0 (1- nNodes))))
  (setq i 0)
  (while (< i nNodes)
    (vlax-safearray-put-element distArr i 1e12)
    (vlax-safearray-put-element prevNodeArr i -1)
    (vlax-safearray-put-element prevSegArr i -1)
    (vlax-safearray-put-element visArr i 0)
    (setq i (1+ i)))
  (vlax-safearray-put-element distArr startId 0.0)
  (setq open (list startId) reached (list startId))
  (while open
    (setq cur nil curD 1e12)
    (foreach next open
      (if (and (= (vlax-safearray-get-element visArr next) 0)
               (< (vlax-safearray-get-element distArr next) curD))
        (setq cur next curD (vlax-safearray-get-element distArr next))))
    (if (not cur)
      (setq open nil)
      (progn
        (vlax-safearray-put-element visArr cur 1)
        (setq open (vl-remove cur open))
        (foreach e (cdr (assoc cur adj))
          (setq to (car e) sid (cadr e) w (caddr e))
          (if (= (vlax-safearray-get-element visArr to) 0)
            (progn
              (setq nd (+ curD w))
              (if (< nd (vlax-safearray-get-element distArr to))
                (progn
                  (vlax-safearray-put-element distArr to nd)
                  (vlax-safearray-put-element prevNodeArr to cur)
                  (vlax-safearray-put-element prevSegArr to sid)
                  (if (not (member to reached)) (setq reached (cons to reached)))
                  (if (not (member to open)) (setq open (cons to open)))))))))))
  (list startId prevNodeArr prevSegArr reached distArr))

(defun bklcap:extract-path (startId endId dijk segMap / prevNodeArr prevSegArr cur pnode sid path ok)
  (setq prevNodeArr (cadr dijk) prevSegArr (caddr dijk))
  (if (= startId endId)
    (list T nil)
    (if (= (vlax-safearray-get-element prevNodeArr endId) -1)
      (list nil nil)
      (progn
        (setq cur endId ok T)
        (while (and ok (/= cur startId))
          (setq pnode (vlax-safearray-get-element prevNodeArr cur)
                sid (vlax-safearray-get-element prevSegArr cur))
          (if (= pnode -1)
            (setq ok nil)
            (progn
              (if (>= sid 0)
                (setq path (cons (cdr (assoc sid segMap)) path)))
              (setq cur pnode))))
        (list ok path)))))

(defun bklcap:resolve-cab (tag p cabs / key matches best bestD c d)
  (if (and tag (/= tag ""))
    (progn
      (setq key (bklcap:cabinet-key tag)
            matches (vl-remove-if-not
                      '(lambda (c / k)
                         (setq k (bklcap:cabinet-key (caddr c)))
                         (or (= key k) (= key (bklcap:cabinet-key (cadr c)))))
                      cabs))))
  (if (not matches) (setq matches cabs))
  (setq best nil bestD 1e12)
  (foreach c matches
    (setq d (bklcap:xy p (car c)))
    (if (< d bestD) (setq bestD d best c)))
  best)

(defun bklcap:branch-edge-p (lastSeg prevSeg lotNodeId pCable nodeX nodeY degArr / a b endPt startPt du dv dot lenSeg lenBP px qx pdu)
  (setq a (car lastSeg) b (cadr lastSeg)
        endPt (bklcap:node-pt lotNodeId nodeX nodeY))
  (if (< (bklcap:xy a endPt) (bklcap:xy b endPt))
    (setq startPt b endPt a)
    (setq startPt a endPt b))
  (setq lenSeg (bklcap:xy startPt endPt)
        lenBP (bklcap:xy endPt pCable))
  (if (<= lenSeg 1e-6) nil
    (progn
      (setq du (list (/ (- (car endPt) (car startPt)) lenSeg) (/ (- (cadr endPt) (cadr startPt)) lenSeg))
            dv (if (> lenBP 0.05)
                 (list (/ (- (car pCable) (car endPt)) lenBP) (/ (- (cadr pCable) (cadr endPt)) lenBP))
                 du)
            dot (+ (* (car du) (car dv)) (* (cadr du) (cadr dv))))
      (and (> dot 0.25)
           (or (= (vlax-safearray-get-element degArr lotNodeId) 1)
               (and prevSeg
                    (progn
                      (setq px (- (caadr prevSeg) (caar prevSeg))
                            qx (- (cadadr prevSeg) (cadar prevSeg))
                            pdu (bklcap:xy (car prevSeg) (cadr prevSeg)))
                      (if (> pdu 1e-6)
                        (< (abs (+ (* (car du) (/ px pdu)) (* (cadr du) (/ qx pdu)))) 0.7)
                        T))))))))

(defun bklcap:trace-route (cabNodeId pCable graph cabCache / nodeX nodeY degArr adj segMap splitDims allSegs nNodes
                                                          bestScore bestSeg s res score uNode vNode endNode otherNode
                                                          dijk pathRes path compDijk a b pa pb da db gap minGap bestPair gapLen
                                                          p1 p2 rawSegs dParts branchLen lotNode lastSeg prevSeg bridged)
  (setq nodeX (car graph) nodeY (cadr graph) degArr (caddr graph)
        adj (cadddr graph) segMap (nth 4 graph) splitDims (nth 5 graph) allSegs (nth 6 graph) nNodes (nth 7 graph))
  ; Tìm đoạn Dim gần nhãn cáp nhất (ưu tiên đoạn ngắn/vừa <= 25m)
  (setq bestScore 1e12 bestSeg nil)
  (foreach s splitDims
    (setq res (bklcap:seg-dist pCable (car s) (cadr s))
          score (+ (car res) (if (> (cadddr s) 25.0) 20.0 0.0)))
    (if (< score bestScore) (setq bestScore score bestSeg s)))
  (if (not bestSeg)
    (foreach s allSegs
      (setq res (bklcap:seg-dist pCable (car s) (cadr s))
            score (+ (car res) (if (> (cadddr s) 25.0) 20.0 0.0)))
      (if (< score bestScore) (setq bestScore score bestSeg s))))
  (setq uNode (car (bklcap:nearest-node-id (car bestSeg) nodeX nodeY nNodes))
        vNode (car (bklcap:nearest-node-id (cadr bestSeg) nodeX nodeY nNodes)))
  (if (<= (bklcap:xy pCable (bklcap:node-pt uNode nodeX nodeY))
          (bklcap:xy pCable (bklcap:node-pt vNode nodeX nodeY)))
    (setq endNode uNode otherNode vNode)
    (setq endNode vNode otherNode uNode))
  (setq dijk (cdr (assoc cabNodeId cabCache)))
  (if (not dijk)
    (progn
      (setq dijk (bklcap:dijkstra cabNodeId adj nNodes))
      (setq cabCache (cons (cons cabNodeId dijk) cabCache))))
  (setq pathRes (bklcap:extract-path cabNodeId endNode dijk segMap))
  (if (not (car pathRes))
    (progn
      (setq pathRes (bklcap:extract-path cabNodeId otherNode dijk segMap))
      (if (car pathRes) (setq endNode otherNode))))
  (if (car pathRes)
    (setq rawSegs (cadr pathRes) lotNode endNode)
    (progn
      ; Cầu nối tự động khi bị đứt Dim giữa đường (tối ưu tổng chiều dài thực từ tủ tới đích)
      (setq bridged T
            compDijk (bklcap:dijkstra endNode adj nNodes)
            minGap 1e12 bestScore 1e12 bestPair nil)
      (foreach a (cadddr dijk)
        (setq pa (bklcap:node-pt a nodeX nodeY)
              da (vlax-safearray-get-element (nth 4 dijk) a))
        (foreach b (cadddr compDijk)
          (setq pb (bklcap:node-pt b nodeX nodeY)
                db (vlax-safearray-get-element (nth 4 compDijk) b)
                gap (bklcap:xy pa pb)
                score (+ da db gap))
          (if (< score bestScore)
            (setq bestScore score minGap gap bestPair (cons a b)))))
      (setq p1 (cadr (bklcap:extract-path cabNodeId (car bestPair) dijk segMap))
            p2 (cadr (bklcap:extract-path endNode (cdr bestPair) compDijk segMap))
            gapLen (itoa (max 1 (fix (+ 0.5 minGap))))
            rawSegs (append p1 (list (list (bklcap:node-pt (car bestPair) nodeX nodeY)
                                           (bklcap:node-pt (cdr bestPair) nodeX nodeY)
                                           nil minGap gapLen nil))
                            (reverse p2))
            lotNode endNode)))
  (cond
    ((not rawSegs)
     (setq dParts (list "0") branchLen "2"))
    ((= (length rawSegs) 1)
     (setq dParts (list (nth 4 (car rawSegs))) branchLen "2"))
    (T
     (setq lastSeg (last rawSegs)
           prevSeg (nth (- (length rawSegs) 2) rawSegs))
     (if (and (nth 5 lastSeg) (bklcap:branch-edge-p lastSeg prevSeg lotNode pCable nodeX nodeY degArr))
       (setq dParts (mapcar '(lambda (x) (nth 4 x)) (reverse (cdr (reverse rawSegs))))
             branchLen (nth 4 lastSeg))
       (setq dParts (mapcar '(lambda (x) (nth 4 x)) rawSegs)
             branchLen "2"))))
  (list dParts branchLen lotNode cabCache bridged))

(defun bklcap:match-houses (input / dims polys cabs houses cables graph nodeX nodeY nNodes cabNodes cab res
                                   cabCache matchedCables house cable best bestD p routeRes
                                   dParts branchLen lotNode groups rows item mult idx hname note
                                   assumedCnt bridgedCnt sortedGroups cabObj tbaKey cabKey finalRows grp)
  (setq dims (car input) polys (cadr input) cabs (caddr input)
        houses (cadddr input) cables (nth 4 input))
  (if (not (and cabs cables (or dims polys)))
    (bklcap:fail "Thiếu tủ điện, Dim/tuyến cáp hoặc nhãn cáp."))
  (setq graph (bklcap:build-graph dims polys)
        nodeX (car graph) nodeY (cadr graph) nNodes (nth 7 graph)
        assumedCnt 0 bridgedCnt 0)
  (foreach cab cabs
    (setq res (bklcap:nearest-node-id (car cab) nodeX nodeY nNodes))
    (setq cabNodes (cons (cons (cadr cab) (car res)) cabNodes)))
  ; 1. Ghép các đối tượng SỐ NHÀ x có sẵn (nếu có) với nhãn cáp gần nhất
  (foreach house houses
    (setq p (car house) best nil bestD 1e12)
    (foreach cable cables
      (if (not (member cable matchedCables))
        (progn
          (setq res (bklcap:xy p (car cable)))
          (if (and (<= res 25.0) (< res bestD))
            (setq bestD res best cable)))))
    (if best
      (progn
        (setq matchedCables (cons best matchedCables)
              cab (bklcap:resolve-cab (nth 3 best) (car best) cabs))
        (if cab
          (progn
            (setq routeRes (bklcap:trace-route (cdr (assoc (cadr cab) cabNodes)) (car best) graph cabCache)
                  dParts (car routeRes) branchLen (cadr routeRes) lotNode (caddr routeRes)
                  cabCache (cadddr routeRes))
            (if (nth 4 routeRes) (setq bridgedCnt (1+ bridgedCnt)))
            (setq rows (cdr (assoc (cadr cab) groups))
                  item (list (cadr house) (caddr house) dParts (cadr best) branchLen lotNode
                             (car best) (apply '+ (mapcar 'atof dParts))))
            (if (assoc (cadr cab) groups)
              (setq groups (subst (cons (cadr cab) (cons item rows)) (assoc (cadr cab) groups) groups))
              (setq groups (cons (cons (cadr cab) (list item)) groups))))))))
  ; 2. Tự động giả định số nhà cho các nhãn cáp chưa có SỐ NHÀ (như MB SINH SAU CT.dwg)
  (foreach cable cables
    (if (not (member cable matchedCables))
      (progn
        (setq cab (bklcap:resolve-cab (nth 3 cable) (car cable) cabs))
        (if cab
          (progn
            (setq routeRes (bklcap:trace-route (cdr (assoc (cadr cab) cabNodes)) (car cable) graph cabCache)
                  dParts (car routeRes) branchLen (cadr routeRes) lotNode (caddr routeRes)
                  cabCache (cadddr routeRes))
            (if (nth 4 routeRes) (setq bridgedCnt (1+ bridgedCnt)))
            (setq mult (cond ((nth 4 cable)) (T 1))
                  note (nth 5 cable))
            (repeat mult
              (setq assumedCnt (1+ assumedCnt)
                    rows (cdr (assoc (cadr cab) groups))
                    item (list nil note dParts (cadr cable) branchLen lotNode
                               (car cable) (apply '+ (mapcar 'atof dParts))))
              (if (assoc (cadr cab) groups)
                (setq groups (subst (cons (cadr cab) (cons item rows)) (assoc (cadr cab) groups) groups))
                (setq groups (cons (cons (cadr cab) (list item)) groups)))))))))
  (setq sortedGroups nil)
  (foreach grp groups
    (setq cab (car grp)
          rows (cdr grp)
          rows (vl-sort rows
                 '(lambda (a b)
                    (cond
                      ((and (car a) (car b)) (< (car a) (car b)))
                      ((car a) T)
                      ((car b) nil)
                      ((/= (nth 7 a) (nth 7 b)) (< (nth 7 a) (nth 7 b)))
                      ((/= (nth 5 a) (nth 5 b)) (< (nth 5 a) (nth 5 b)))
                      ((/= (car (nth 6 a)) (car (nth 6 b))) (< (car (nth 6 a)) (car (nth 6 b))))
                      (T (< (cadr (nth 6 a)) (cadr (nth 6 b))))))))
    (setq idx 0 finalRows nil)
    (foreach item rows
      (if (car item)
        (setq idx (max idx (car item))
              hname (cadr item))
        (progn
          (setq idx (1+ idx)
                note (cadr item)
                hname (if note
                        (strcat "Số nhà " (itoa idx) " (" note ")")
                        (strcat "Số nhà " (itoa idx))))))
      (setq finalRows (cons (list idx hname (caddr item) (cadddr item) (nth 4 item) (nth 5 item)) finalRows)))
    (setq cabObj (car (vl-remove-if-not '(lambda (c) (= (cadr c) cab)) cabs))
          tbaKey (if cabObj (cadddr cabObj) "")
          cabKey (if cabObj (caddr cabObj) cab)
          sortedGroups (cons (list cab tbaKey cabKey (reverse finalRows)) sortedGroups)))
  (setq sortedGroups
    (vl-sort sortedGroups
      '(lambda (a b)
         (if (/= (cadr a) (cadr b))
           (< (cadr a) (cadr b))
           (< (caddr a) (caddr b))))))
  (list (mapcar '(lambda (g) (cons (car g) (cadddr g))) sortedGroups)
        assumedCnt bridgedCnt))

(defun bklcap:sum-formula (parts / result part)
  (setq result (car parts))
  (if (cdr parts)
    (progn
      (foreach part (cdr parts) (setq result (strcat result "+" part)))
      (setq result (strcat "=" result))))
  result)

(defun bklcap:detail (house row-number first-at-lot / r phase total meter1 meter3 hdpe40 hdpe50 hdpeSpare cap3 cap1)
  ; BKL_CAP_DIEN.xlsx A:O, hàng chi tiết (15 cột). Các phần bù E:G đều tính theo mét.
  (setq r (itoa row-number)
        phase (nth 3 house)
        total (strcat "D" r "+E" r "+F" r "+G" r))
  (cond
    ((= phase 1)
     (setq meter1 "1" meter3 ""
           hdpe40 (strcat "=" total "-2")
           hdpe50 (strcat "=N" r "-J" r)
           hdpeSpare ""
           cap3 (strcat "=(" total ")*J" r)
           cap1 (strcat "=(" total ")*I" r)))
    ((= phase 3)
     (setq meter1 "" meter3 "1"
           hdpe40 "0"
           hdpe50 (strcat "=N" r "-2")
           hdpeSpare ""
           cap3 (strcat "=(" total ")*J" r)
           cap1 (strcat "=(" total ")*I" r)))
    (T ; phase = 0: chỉ có ống HDPE 50/40 (chờ dây)
     (setq meter1 "0" meter3 ""
           hdpe40 "0"
           hdpe50 (strcat "=N" r "-J" r)
           hdpeSpare (strcat "=" total "-2")
           cap3 (strcat "=(" total ")*J" r)
           cap1 (strcat "=(" total ")*I" r))))
  (list "" "" (cadr house) (bklcap:sum-formula (caddr house))
        (nth 4 house) "=0.7+1.5" "3" (if first-at-lot (strcat "=E" r) "0")
        meter1 meter3 hdpe40 hdpe50 hdpeSpare cap3 cap1))

(defun bklcap:rows (groups / rows index row-number group houses house seenLots lotId firstAtLot lastRow)
  (setq rows '(("DIỄN GIẢI KHỐI LƯỢNG ĐƯỜNG DÂY 0,4KV SINH HOẠT CẤP ĐẾN HỘ DÂN")
               ("STT" "Tên tủ" "Số phụ tải (hộ)" "Từ tủ điện đến lô đất"
                "Từ rãnh cáp đến đầu lô đất" "Lên xuống rãnh cáp"
                "Lên điểm đấu vào nhà hiện trạng + dự phòng"
                "rãnh cáp từ trục chính đến hộ dân (m)"
                "Số công tơ 1F\n (2x10mm2)" "Số công tơ 3F (3x16+1x10)" "Ống HDPE gân xoắn" "" "" "" "")
               ("" "" "" "" "" "" "" "" "" "" "40/30 (DSTA 10)" "50/40 (DSTA 16)" "50/40 (Chờ ống)" "3x16+1x10" "2x10")
               ()
               ("A. ĐƯỜNG DÂY 0,4KV SINH HOẠT HOÀN TRẢ SAU TBA ")
               ("LỘ I"))
        rows (reverse rows) row-number 6 index 0)
  (foreach group groups
    (setq houses (cdr group)
          index (1+ index) row-number (1+ row-number)
          seenLots nil
          rows (cons (list (itoa index) (car group) (itoa (length houses))) rows))
    (foreach house houses
      (setq row-number (1+ row-number)
            lotId (nth 5 house)
            firstAtLot (not (member lotId seenLots)))
      (if firstAtLot (setq seenLots (cons lotId seenLots)))
      (setq rows (cons (bklcap:detail house row-number firstAtLot) rows))))
  (if (> row-number 6)
    (progn
      (setq lastRow (itoa row-number))
      (setq rows (cons (list "" "TỔNG CỘNG"
                             (strcat "=SUM(C7:C" lastRow ")")
                             "" "" "" ""
                             (strcat "=SUM(H7:H" lastRow ")")
                             (strcat "=SUM(I7:I" lastRow ")")
                             (strcat "=SUM(J7:J" lastRow ")")
                             (strcat "=SUM(K7:K" lastRow ")")
                             (strcat "=SUM(L7:L" lastRow ")")
                             (strcat "=SUM(M7:M" lastRow ")")
                             (strcat "=SUM(N7:N" lastRow ")")
                             (strcat "=SUM(O7:O" lastRow ")")) rows))))
  (reverse rows))

(defun bklcap:csv-format (/ windows office versions version options custom decimal separator)
  ; Excel trên Windows có thể dùng dấu thập phân riêng, kể cả khi Windows dùng dấu chấm.
  (setq windows "HKEY_CURRENT_USER\\Control Panel\\International"
        office "HKEY_CURRENT_USER\\Software\\Microsoft\\Office"
        decimal (vl-registry-read windows "sDecimal")
        separator (vl-registry-read windows "sList")
        versions (vl-remove-if-not '(lambda (s) (wcmatch s "#*.#*"))
                     (vl-registry-descendents office)))
  (if (setq version (car (vl-sort versions '(lambda (a b) (> (atof a) (atof b))))))
    (progn
      (setq options (strcat office "\\" version "\\Excel\\Options"))
      (if (and (member (vl-registry-read options "UseSystemSeparators") '(0 "0"))
               (setq custom (vl-registry-read options "DecimalSeparator")))
        (setq decimal custom))))
  (if (/= decimal ",") (setq decimal "."))
  (if (not (and separator (= (strlen separator) 1))) (setq separator ","))
  (if (= decimal separator) (setq separator ";"))
  (list separator decimal))

(defun bklcap:csv-field (s separator / chars ch)
  (if (or (vl-string-search separator s) (vl-string-search "\"" s)
          (vl-string-search (chr 10) s) (vl-string-search (chr 13) s))
    (progn
      (foreach ch (vl-string->list s)
        (setq chars (cons ch chars))
        (if (= ch 34) (setq chars (cons ch chars))))
      (strcat "\"" (vl-list->string (reverse chars)) "\""))
    s))

(defun bklcap:csv-line (cells separator decimal / i line value)
  (setq i 0 line "")
  (repeat 15
    (if (> i 0) (setq line (strcat line separator)))
    (if cells
      (progn
        (setq value (car cells) cells (cdr cells))
        (if (and (member i '(3 4 5)) (wcmatch value "[=0-9]*"))
          (setq value (vl-string-translate "." decimal value)))
        (setq line (strcat line (bklcap:csv-field value separator)))))
    (setq i (1+ i)))
  line)

(defun bklcap:format-xlsx (csvPath xlsxPath / psScript psFile wsh cmd)
  (setq psFile (strcat (getvar "TEMPPREFIX") "bkl_format_excel.ps1"))
  (setq psScript (open psFile "w" "utf8-bom"))
  (if (not psScript) (setq psScript (open psFile "w")))
  (if psScript
    (progn
      (write-line "$csvPath = [System.IO.Path]::GetFullPath($args[0])" psScript)
      (write-line "$xlsxPath = [System.IO.Path]::GetFullPath($args[1])" psScript)
      (write-line "$excel = New-Object -ComObject Excel.Application" psScript)
      (write-line "$excel.Visible = $false; $excel.DisplayAlerts = $false" psScript)
      (write-line "try {" psScript)
      (write-line "  $wb = $excel.Workbooks.Open($csvPath)" psScript)
      (write-line "  $ws = $wb.Sheets.Item(1)" psScript)
      (write-line "  $lastRow = $ws.UsedRange.Rows.Count" psScript)
      (write-line "  $ws.Cells.Font.Name = 'Times New Roman'" psScript)
      (write-line "  $ws.Cells.Font.Size = 11" psScript)
      (write-line "  $widths = @(7, 18, 18, 14, 12, 12, 12, 13, 8, 8, 13, 13, 13, 13, 13)" psScript)
      (write-line "  for ($i = 0; $i -lt 15; $i++) { $ws.Columns.Item($i+1).ColumnWidth = $widths[$i] }" psScript)
      (write-line "  $ws.Rows.Item(1).RowHeight = 32" psScript)
      (write-line "  $title = $ws.Range('A1:O1'); $title.Merge(); $title.Font.Size = 13; $title.Font.Bold = $true; $title.HorizontalAlignment = -4108; $title.VerticalAlignment = -4108" psScript)
      (write-line "  $ws.Rows.Item(2).RowHeight = 45; $ws.Rows.Item(3).RowHeight = 25; $ws.Rows.Item(4).RowHeight = 15" psScript)
      (write-line "  foreach ($m in @('A2:A4','B2:B4','C2:C4','D2:D4','E2:E4','F2:F4','G2:G4','H2:H4','I2:I4','J2:J4','K2:M2','K3:K4','L3:L4','M3:M4','N2:O2','N3:N4','O3:O4')) { $ws.Range($m).Merge() }" psScript)
      (write-line "  $h = $ws.Range('A2:O4'); $h.Font.Bold = $true; $h.WrapText = $true; $h.HorizontalAlignment = -4108; $h.VerticalAlignment = -4108" psScript)
      (write-line "  $ws.Rows.Item(5).RowHeight = 22; $ws.Range('A5:O5').Merge(); $s5 = $ws.Range('A5:O5'); $s5.Font.Bold = $true; $s5.HorizontalAlignment = -4131; $s5.VerticalAlignment = -4108" psScript)
      (write-line "  $ws.Rows.Item(6).RowHeight = 20; $ws.Range('A6:O6').Merge(); $s6 = $ws.Range('A6:O6'); $s6.Font.Bold = $true; $s6.HorizontalAlignment = -4131; $s6.VerticalAlignment = -4108" psScript)
      (write-line "  if ($lastRow -ge 7) {" psScript)
      (write-line "    $d = $ws.Range('A7:O' + $lastRow); $d.HorizontalAlignment = -4108; $d.VerticalAlignment = -4108" psScript)
      (write-line "    for ($r = 7; $r -le $lastRow; $r++) {" psScript)
      (write-line "      $ws.Rows.Item($r).RowHeight = 18" psScript)
      (write-line "      if ($ws.Cells.Item($r, 1).Text -ne '' -or $ws.Cells.Item($r, 2).Text -ne '') { $ws.Rows.Item($r).Font.Bold = $true }" psScript)
      (write-line "    }" psScript)
      (write-line "  }" psScript)
      (write-line "  $table = $ws.Range('A2:O' + $lastRow)" psScript)
      (write-line "  $table.Borders.LineStyle = 1; $table.Borders.Weight = 2" psScript)
      (write-line "  $wb.SaveAs($xlsxPath, 51)" psScript)
      (write-line "  $wb.Close($false)" psScript)
      (write-line "} finally { $excel.Quit(); [System.Runtime.InteropServices.Marshal]::ReleaseComObject($excel) | Out-Null }" psScript)
      (close psScript)
      (if (setq wsh (vlax-get-or-create-object "WScript.Shell"))
        (progn
          (setq cmd (strcat "powershell.exe -ExecutionPolicy Bypass -WindowStyle Hidden -File \"" psFile "\" \"" csvPath "\" \"" xlsxPath "\""))
          (vlax-invoke-method wsh 'Run cmd 0 1)
          (vlax-release-object wsh)
          (findfile xlsxPath))))))

(defun bklcap:export (path rows / *error* stream row format)
  (defun *error* (message)
    (if stream (close stream))
    (princ (strcat "\nBKL_CAP: Lỗi ghi CSV: " message))
    (princ))
  (setq format (bklcap:csv-format))
  (if (setq stream (open path "w" "utf8-bom"))
    (progn
      (foreach row rows (write-line (bklcap:csv-line row (car format) (cadr format)) stream))
      (close stream)
      (setq stream nil)
      T)))

(defun bklcap:build (selection / validation-message result)
  (setq result (vl-catch-all-apply
                 '(lambda () (bklcap:match-houses (bklcap:collect selection)))))
  (if (vl-catch-all-error-p result)
    (if validation-message validation-message (vl-catch-all-error-message result))
    result))

(defun bklcap:run-with-path (selection path / result groups assumedCnt bridgedCnt ext xlsxPath csvPath totalHouses)
  (cond
    ((not selection) (princ "\nBKL_CAP: Không có đối tượng phù hợp."))
    ((= (type (setq result (bklcap:build selection))) 'STR)
     (princ (strcat "\nBKL_CAP: " result)))
    (T
     (setq groups (car result)
           assumedCnt (cadr result)
           bridgedCnt (caddr result)
           ext (strcase (substr path (- (strlen path) 3))))
     (if (= ext "XLSX")
       (setq xlsxPath path
             csvPath (strcat (substr path 1 (- (strlen path) 5)) ".csv"))
       (setq csvPath path
             xlsxPath (strcat (substr path 1 (- (strlen path) 4)) ".xlsx")))
     (if (bklcap:export csvPath (bklcap:rows groups))
       (progn
         (bklcap:format-xlsx csvPath xlsxPath)
         (setq totalHouses (itoa (apply '+ (mapcar '(lambda (g) (length (cdr g))) groups))))
         (princ (strcat "\nBKL_CAP: Đã xuất " totalHouses " hộ (" (itoa (length groups)) " tủ) vào file Excel định dạng chuẩn:"))
         (if (> assumedCnt 0)
           (princ (strcat "\n  (Đã tự động giả định " (itoa assumedCnt) " số nhà do bản vẽ thiếu text SỐ NHÀ)")))
         (if (> bridgedCnt 0)
           (princ (strcat "\n  (Đã tự nối " (itoa bridgedCnt) " nhánh qua khoảng trống chưa vẽ Dim)")))
         (princ (strcat "\n  -> " xlsxPath))
         (princ (strcat "\n  -> " csvPath)))
       (princ "\nBKL_CAP: Không ghi được file.")))))

(defun c:BKL_CAP (/ filter selection path defaultName)
  (if (not (equal (ascii "Đ") 272))
    (princ "\nBKL_CAP: Dùng AutoCAD 2021 trở lên. Đặt LISPSYS bằng 1 rồi khởi động lại AutoCAD.")
    (progn
      (setq filter (list '(0 . "INSERT,DIMENSION,TEXT,MTEXT,LWPOLYLINE,LINE")
                         (cons 410 (if (= (getvar "CVPORT") 1) (getvar "CTAB") "Model"))))
      (princ "\nQuét chọn tủ điện, Dim tuyến cáp, số nhà và nhãn cáp <Enter để chọn tất cả>: ")
      (setq selection (ssget))
      (if (not selection) (setq selection (ssget "_X" filter)))
      (if selection
        (progn
          (setq defaultName (strcat (getvar "DWGPREFIX") "BKL_CAP_DIEN.xlsx"))
          (if (setq path (getfiled "Lưu bảng bóc tách cáp điện" defaultName "xlsx;csv" 1))
            (bklcap:run-with-path selection path)))
        (princ "\nBKL_CAP: Không có đối tượng phù hợp."))))
  (princ))

(defun c:T-KLHD () (c:BKL_CAP))
(defun c:T_KLHD () (c:BKL_CAP))
(defun c:BKLCAP () (c:BKL_CAP))

(princ)

(princ "\n=== TK_CHITIET_TUYEN V118 da nap. Lenh: T-THCS | T-THDIEN | T-THONGSO | T-VEDAYCS | T-DIMNET | T-KIEMTRA | CDS | T-KLHD ===")
(princ)

