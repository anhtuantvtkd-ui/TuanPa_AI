;; =========================================================================
;; AUTO LISP: TAO NET VE DIEN TUY Y - Lenh: TDAY
;; -------------------------------------------------------------------------
;; NGUOI DUNG TU VE HINH DANG NET BANG CHUOI KY TU:
;;   dau "-"  = 1 doan net gach (moi dau "-" dai bang nhau)
;;   con lai  = chu / ky hieu (do nguoi dung tu chon)
;;
;;   Vi du:   -//-22KV-//-        ->  —//—22KV—//—22KV—//—22KV—...
;;            -//-22KV-//-22KV-//-  ->  ket qua y het vi du tren
;;            -cs-//-             ->  —cs—//—cs—//—cs—//—...
;;            -35-35-             ->  —35—35—35—...
;;            --35--              ->  moi doan gach dai gap doi
;;
;; 2 XU LY TU DONG QUAN TRONG (cho ra nhip DEU TAM TAP):
;;  1. NET LAP VONG: neu ky hieu DAU va CUOI GIONG NHAU (vd "-//-22KV-//-"),
;;     may TU BO cai o cuoi - vi khi net lap lai, ky hieu dau cua chu ky
;;     SAU da dung ngay do roi. Khong bo thi cho noi bi "// //" dinh nhau,
;;     nhip chu lech han (24.2 - 24.2 - 19.9).
;;  2. Moi dau "-" cho ra 1 doan gach DAI BANG NHAU -> khoang trang giua
;;     cac ky hieu deu nhau suot tuyen.
;;
;; Be rong moi chu duoc DO THAT theo TextStyle + chieu cao (ham textbox)
;; nen khoang trong luon vua khit -> NET KHONG DE LEN CHU.
;; =========================================================================

(vl-load-com)

(if (null *TD-Mota*) (setq *TD-Mota* "-//-22KV-//-"))
(if (null *TD-Cao*)  (setq *TD-Cao* 2.5))
(if (null *TD-Dash*) (setq *TD-Dash* 15.0))
(if (null *TD-Le*)   (setq *TD-Le* 1.0))

;; -------------------------------------------------------------------------
;; Do BE RONG THAT cua chuoi chu theo TextStyle + chieu cao chi dinh
;; -------------------------------------------------------------------------
(defun td-rong-chu (str sty h / kq box)
  (setq kq (vl-catch-all-apply
             '(lambda ()
                (setq box (textbox (list (cons 1 str)
                                         (cons 40 h)
                                         (cons 7 sty))))
                (- (car (cadr box)) (car (car box))))))
  (if (or (vl-catch-all-error-p kq) (null kq) (<= kq 0.0))
    (* (strlen str) h 0.8)        ; du phong khi khong do duoc
    kq))

;; -------------------------------------------------------------------------
;; Tach chuoi mo ta thanh danh sach: "-" (net gach) / chuoi chu
;; -------------------------------------------------------------------------
(defun td-tach-mota (s / i n ch cur out)
  (setq out '() cur "" i 1 n (strlen s))
  (while (<= i n)
    (setq ch (substr s i 1))
    (if (= ch "-")
      (progn
        (if (/= cur "")
          (progn (setq out (cons cur out)) (setq cur "")))
        (setq out (cons "-" out)))
      (setq cur (strcat cur ch)))
    (setq i (1+ i)))
  (if (/= cur "") (setq out (cons cur out)))
  (reverse out))

;; -------------------------------------------------------------------------
;; Bo het net gach o CUOI danh sach
;; -------------------------------------------------------------------------
(defun td-bo-gach-cuoi (lst)
  (while (and (> (length lst) 1)
              (= (nth (1- (length lst)) lst) "-"))
    (setq lst (reverse (cdr (reverse lst)))))
  lst)

;; -------------------------------------------------------------------------
;; Danh sach cac ky hieu (bo net gach) theo dung thu tu
;; -------------------------------------------------------------------------
(defun td-cac-chu (lst / out el)
  (setq out '())
  (foreach el lst (if (/= el "-") (setq out (cons el out))))
  (reverse out))

;; -------------------------------------------------------------------------
;; Chuan hoa:
;;  - Bat buoc BAT DAU bang net gach (yeu cau cua kieu net "A")
;;  - Bo net gach thua o cuoi
;;  - Neu ky hieu DAU trung ky hieu CUOI -> bo ky hieu cuoi (cho noi vong)
;; -------------------------------------------------------------------------
(defun td-chuan-hoa (lst / chu n)
  (if (or (null lst) (/= (car lst) "-"))
    (setq lst (cons "-" lst)))
  (setq lst (td-bo-gach-cuoi lst))
  (setq chu (td-cac-chu lst))
  (setq n (length chu))
  (if (and (> n 1) (= (car chu) (nth (1- n) chu)))
    (progn
      ;; bo phan tu chu cuoi cung, roi don not net gach thua
      (setq lst (reverse (cdr (reverse lst))))
      (setq lst (td-bo-gach-cuoi lst))
      (princ (strcat "\n>> Ky hieu dau va cuoi giong nhau (\"" (car chu)
                     "\") -> tu bo cai o CUOI, vi khi net lap lai thi"
                     " ky hieu dau cua chu ky sau da dung ngay do."))))
  lst)

;; -------------------------------------------------------------------------
;; Ten kieu net sinh tu chuoi mo ta: giu chu/so, "/" -> "S", con lai -> "_"
;; -------------------------------------------------------------------------
(defun td-ten (s / i n ch out)
  (setq out "" i 1 n (strlen s))
  (while (<= i n)
    (setq ch (strcase (substr s i 1)))
    (setq out (strcat out
                (cond
                  ((and (>= ch "A") (<= ch "Z")) ch)
                  ((and (>= ch "0") (<= ch "9")) ch)
                  ((= ch "/") "S")
                  (T "_"))))
    (setq i (1+ i)))
  (while (and (> (strlen out) 0) (= (substr out 1 1) "_"))
    (setq out (substr out 2)))
  (while (and (> (strlen out) 0) (= (substr out (strlen out) 1) "_"))
    (setq out (substr out 1 (1- (strlen out)))))
  (if (= out "") (setq out "NET"))
  (strcat "DIEN_" out))

;; -------------------------------------------------------------------------
(defun c:TDAY ( / mota curStyle sty cao dash le yoff lst co_chu
                  nameLT body w filepath f)
  (setvar "CMDECHO" 0)

  ;; --- 1. Mo ta hinh dang net ---
  (princ "\n--- MO TA NET: dau \"-\" = doan net gach, con lai la chu/ky hieu ---")
  (princ "\n    Vi du:  -//-22KV-//-   |   -cs-//-   |   -35-35-   |   --35--")
  (setq mota (getstring T (strcat "\n1. Mo ta net <" *TD-Mota* ">: ")))
  (if (or (null mota) (= mota "")) (setq mota *TD-Mota*))
  (setq *TD-Mota* mota)

  ;; --- 2. Kieu chu ---
  (setq curStyle (getvar "TEXTSTYLE"))
  (setq sty (getstring T (strcat "\n2. Kieu chu (TextStyle) <" curStyle ">: ")))
  (if (or (null sty) (= sty "")) (setq sty curStyle))
  (if (null (tblsearch "STYLE" sty))
    (progn
      (princ (strcat "\n[Luu y] Khong co TextStyle '" sty "'. Dung '"
                     curStyle "'."))
      (setq sty curStyle)))

  ;; --- 3. Chieu cao chu ---
  (initget 4)
  (setq cao (getreal (strcat "\n3. Chieu cao chu <" (rtos *TD-Cao* 2 2) ">: ")))
  (if (null cao) (setq cao *TD-Cao*))
  (setq *TD-Cao* cao)

  ;; --- 4. Chieu dai 1 doan net gach "-" (moi dau "-" deu dai bang nhau) ---
  (initget 4)
  (setq dash (getreal (strcat "\n4. Chieu dai 1 doan net gach \"-\" <"
                              (rtos *TD-Dash* 2 1) ">: ")))
  (if (null dash) (setq dash *TD-Dash*))
  (setq *TD-Dash* dash)

  ;; --- 5. Le ho giua net va chu ---
  (initget 4)
  (setq le (getreal (strcat "\n5. Le ho giua net va chu <"
                            (rtos *TD-Le* 2 1) ">: ")))
  (if (null le) (setq le *TD-Le*))
  (setq *TD-Le* le)

  ;; --- Phan tich mo ta ---
  (setq lst (td-chuan-hoa (td-tach-mota mota)))
  (setq co_chu (td-cac-chu lst))

  (if (null co_chu)
    (princ "\n[LOI] Mo ta khong co chu/ky hieu nao - hay go vi du: -//-22KV-//-")
    (progn
      (setq yoff (- 0.0 (/ cao 2.0)))       ; chu nam giua net
      (setq nameLT (td-ten mota))

      ;; --- Dung dinh nghia kieu net ---
      ;; "-"  -> 1 doan gach dai "dash" (moi dau "-" nhu nhau)
      ;; chu  -> -le , [chu] , -(be_rong_that + le)
      ;;         (khoang trong am chua TRON chu -> net khong cat qua chu)
      (setq body "A")
      (princ "\n>> Be rong do duoc:")
      (foreach el lst
        (if (= el "-")
          (setq body (strcat body "," (rtos dash 2 3)))
          (progn
            (setq w (td-rong-chu el sty cao))
            (princ (strcat "  \"" el "\"=" (rtos w 2 2)))
            (setq body
              (strcat body
                      ",-" (rtos le 2 3)
                      ",[\"" el "\"," sty ",S=" (rtos cao 2 3)
                      ",R=0.0,X=0.0,Y=" (rtos yoff 2 3) "]"
                      ",-" (rtos (+ w le) 2 3))))))

      ;; --- Ghi file .lin ---
      (setq filepath (strcat *TVXD-TDAY-DIR*
                             "DayDien_" nameLT ".lin"))
      (setq f (open filepath "w"))
      (write-line (strcat "*" nameLT ",Duong day " mota) f)
      (write-line body f)
      (close f)

      ;; --- Load vao ban ve ---
      ;; Neu kieu net da co, AutoCAD hoi "Reload it? <Y>" -> vong while tu
      ;; bam Enter den khi lenh ket thuc han (khong doan truoc thu tu prompt)
      (vl-cmdf "-linetype" "L" nameLT filepath "")
      (while (> (getvar "CMDACTIVE") 0) (vl-cmdf ""))

      (if (tblsearch "LTYPE" nameLT)
        (progn
          (setvar "CELTYPE" nameLT)
          (princ (strcat "\n[THANH CONG] Net \"" mota "\" (ten kieu net: "
                         nameLT ")"))
          (princ "\nChuoi lap lai: ")
          (foreach el (td-cac-chu lst) (princ (strcat "-" el)))
          (princ "-  (lap di lap lai)")
          (princ "\nBay gio dung lenh LINE hoac PLINE de ve.")
          (princ "\nNet qua day/thua: chinh LTSCALE, hoac chon duong ->"
                 " Properties -> Linetype scale."))
        (princ (strcat "\n[LOI] Khong tao duoc kieu net '" nameLT "'.")))))

  (setvar "CMDECHO" 1)
  (princ))

(defun c:DAYCHUAN ( / items idx kw chosen nameLT aci linPath oldEcho)
  (setq oldEcho (getvar "CMDECHO"))
  (setvar "CMDECHO" 0)
  (setq items '(
    ("22KV" "DIEN_SS_22KV_SS" 2 "Duong day 22kV (Xay moi)")
    ("22TK" "DIEN_SS_22TK_SS" 2 "Duong day 22kV (Thiet ke)")
    ("22HT" "DIEN_SS_22HT_SS" 2 "Duong day 22kV (Hien trang)")
    ("35KV" "DIEN_SS_35KV_SS" 2 "Duong day 35kV (Xay moi)")
    ("35TK" "DIEN_SS_35TK_SS" 2 "Duong day 35kV (Thiet ke)")
    ("35HT" "DIEN_SS_35HT_SS" 2 "Duong day 35kV (Hien trang)")
    ("04KV" "DIEN_SS_0_4KV_SS" 4 "Duong day 0.4kV")
    ("VX"   "DIEN_SS_VX_SS"    4 "Cap van xoan (VX)")
    ("CS"   "DIEN_SS_CS_SS"    6 "Duong day Chieu sang (CS)")
    ("TTLL" "DIEN_SS_TTLL_SS"  3 "Duong day TTLL")
    ("ADSS" "DIEN_SS_ADSS_SS"  5 "Cap quang ADSS")
    ("HDPE" "DIEN_SS_HDPE_SS" 42 "Ong nhua HDPE")
    ("GNV"  "DIEN_SS_GNV_SS"   7 "Tuyen ong / Ganivo (GNV)")
    ("2D60" "DIEN_SS_2D60_SS"  7 "Tuyen ong 2D60")
  ))
  (initget "22KV 22TK 22HT 35KV 35TK 35HT 04KV VX CS TTLL ADSS HDPE GNV 2D60")
  (setq kw (getkword "\nChon net day 0.20mm [22KV/22TK/22HT/35KV/35TK/35HT/04KV/VX/CS/TTLL/ADSS/HDPE/GNV/2D60] <22KV>: "))
  (if (null kw) (setq kw "22KV"))
  (setq chosen (assoc kw items))
  (if chosen
    (progn
      (setq nameLT (cadr chosen) aci (caddr chosen))
      (setq linPath (strcat *TVXD-TDAY-DIR* "DayDien_" nameLT ".lin"))
      (if (and (not (tblsearch "LTYPE" nameLT)) (findfile linPath))
        (progn
          (vl-cmdf "-linetype" "L" nameLT linPath "")
          (while (> (getvar "CMDACTIVE") 0) (vl-cmdf ""))))
      (if (tblsearch "LTYPE" nameLT) (setvar "CELTYPE" nameLT))
      (setvar "CECOLOR" (itoa aci))
      (setvar "CELWEIGHT" 20)
      (princ (strcat "\n[OK] Da chon: " (cadddr chosen) " | Kieu net: " nameLT " | Day: 0.20 mm. Hay ve PLINE/LINE."))))
  (setvar "CMDECHO" oldEcho)
  (princ))

(princ "\n[OK] Da nap. Go lenh 'TDAY' hoac 'DAYCHUAN'. Mo ta net kieu: -//-22KV-//-  hoac  -cs-//-")
(princ)
