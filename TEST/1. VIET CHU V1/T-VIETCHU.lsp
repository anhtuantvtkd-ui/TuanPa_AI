;;; ==========================================================================
;;; LISP VIET CHU CHUYEN NGHIEP (MTEXT) - PHIEN BAN V1
;;; - Lenh goi: T-VIETCHU hoac VC
;;; - Layer mac dinh: T_TEXT (Mau 7 - Trang, Net 0.18 mm)
;;; - Text Style: Tuan_arial (Arial.ttf)
;;; - Chi hoi Chieu cao chu (luu nho gia tri gan nhat)
;;; - Xuat ra: MText
;;; ==========================================================================

(defun c:T-VIETCHU ( / *error* oldEcho oldOsm oldLayer hPt userH strText pEnd)
  (vl-load-com)

  ;; Ham xu ly loi va khoi phuc moi truong
  (defun *error* (msg)
    (if oldEcho (setvar "CMDECHO" oldEcho))
    (if oldOsm (setvar "OSMODE" oldOsm))
    (if (and oldLayer (tblsearch "LAYER" oldLayer)) (setvar "CLAYER" oldLayer))
    (if (and msg (not (wcmatch (strcase msg t) "*cancel*,*exit*")))
      (princ (strcat "\n[Loi]: " msg))
    )
    (princ)
  )

  (setq oldEcho (getvar "CMDECHO"))
  (setvar "CMDECHO" 0)

  ;; 1. Tao va kiem tra Text Style "Tuan_arial"
  (if (not (tblsearch "STYLE" "Tuan_arial"))
    (command "-STYLE" "Tuan_arial" "arial.ttf" 0.0 1.0 0.0 "N" "N")
  )
  (setvar "TEXTSTYLE" "Tuan_arial")

  ;; 2. Tao va kiem tra Layer "T_TEXT" (Mau 7, Lineweight 0.18 mm)
  (setq oldLayer (getvar "CLAYER"))
  (if (not (tblsearch "LAYER" "T_TEXT"))
    (command "-LAYER" "M" "T_TEXT" "C" 7 "T_TEXT" "LW" 0.18 "T_TEXT" "")
    (command "-LAYER" "S" "T_TEXT" "")
  )

  ;; 3. Quan ly chieu cao chu mac dinh (luu vao bien global *T_VIETCHU_HEIGHT*)
  (if (or (null *T_VIETCHU_HEIGHT*) (<= *T_VIETCHU_HEIGHT* 0))
    (setq *T_VIETCHU_HEIGHT* 250.0) ;; Mac dinh 250 (hoac 2.5 theo ti le)
  )

  (setq userH (getdist (strcat "\nNhap chieu cao chu <" (rtos *T_VIETCHU_HEIGHT* 2 2) ">: ")))
  (if (and userH (> userH 0))
    (setq *T_VIETCHU_HEIGHT* userH)
  )

  ;; 4. Vong lap viet chu MText
  (princ (strcat "\nDang viet chu voi chieu cao = " (rtos *T_VIETCHU_HEIGHT* 2 2) " (Layer: T_TEXT, Net: 0.18 mm)."))
  (princ "\nBam diem chen (Enter de ket thuc)...")

  (while (setq hPt (getpoint "\nChon diem chen chu: "))
    (setq strText (getstring T "\nNhap noi dung chu: "))
    (if (and strText (/= strText ""))
      (progn
        ;; Tao MText bang entmake (thuan khiet AutoCAD, tuong thich 100% moi ban CAD)
        (entmake
          (list
            '(0 . "MTEXT")
            '(100 . "AcDbEntity")
            '(8 . "T_TEXT")
            '(100 . "AcDbMText")
            (cons 10 hPt)
            (cons 40 *T_VIETCHU_HEIGHT*)
            (cons 1 strText)
            '(7 . "Tuan_arial")
            '(71 . 1) ;; 1 = TopLeft
            '(72 . 1)
          )
        )
      )
    )
  )

  ;; Khoi phuc cai dat
  (if (and oldLayer (tblsearch "LAYER" oldLayer)) (setvar "CLAYER" oldLayer))
  (setvar "CMDECHO" oldEcho)
  (princ "\n[Hoan tat viet chu MText].")
  (princ)
)

;; Alias lenh nhanh
(defun c:VC () (c:T-VIETCHU))
