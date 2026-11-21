;|
========================================================================
                      HƯỚNG DẪN SỬ DỤNG LISP
========================================================================
Tên LISP  : Đánh số thứ tự cột & Đảo pha tự động
Tên lệnh  : STTPHA
Phiên bản : V3

V2: bổ sung chế độ 1 PHA (không ghi đuôi pha)
V3: số bắt đầu nhận ĐƯỜNG SỐ PHÂN CẤP - 1 hoặc 5.1 hoặc 5.1.1

BƯỚC THỰC HIỆN:
1. Gõ lệnh APPLOAD để load file LISP này vào AutoCAD.
2. Gõ lệnh STTPHA rồi nhấn Enter.
3. Nhập Tiền tố (Ví dụ: II-Đ hoặc I-Đ).
4. Nhập Số cột bắt đầu. Nhận cả 3 kiểu:
   - 1      -> 1, 2, 3, 4...          (tuyến chính, y như cũ)
   - 5.1    -> 5.1, 5.2, 5.3...       (nhánh treo ở cột 5)
   - 5.1.1  -> 5.1.1, 5.1.2, 5.1.3... (nhánh con của nhánh 5.1)
   QUY TẮC: luôn TĂNG CẤP CUỐI CÙNG, các cấp phía trước giữ nguyên.
   Gõ 0 ở đầu thì giữ nguyên số 0 đó: 01 -> 02 -> 03... 09 -> 10.
5. Chọn số pha của đường dây:
   - Gõ 3 (hoặc Enter) : dây 3 PHA -> tên có đuôi pha, tự xoay A->B->C->A
   - Gõ 1              : dây 1 PHA -> KHÔNG ghi pha
6. Nếu chọn 3 pha thì nhập tiếp Pha bắt đầu (A, B hoặc C).
7. Click chuột lần lượt vào từng Block cột đèn trên bản vẽ.
8. Nhấn Enter để kết thúc. Máy báo số cột đã đánh + đường số tiếp theo.

VÍ DỤ TÊN SINH RA:
   Tiền tố II-Đ | bắt đầu 5.1 | 3 pha từ A -> II-Đ5.1-A, II-Đ5.2-B...
   Tiền tố II-Đ | bắt đầu 5.1 | 1 pha       -> II-Đ5.1, II-Đ5.2, II-Đ5.3
========================================================================
|;

(vl-load-com)

;;; ================= HÀM PHỤ (tên riêng stt- , không đụng lisp khác) ==========

;; Tách chuỗi theo 1 ký tự -> danh sách chuỗi con
(defun stt-tach (s ch / kq tam i k c)
  (setq kq '() tam "" i 1 k (strlen s))
  (while (<= i k)
    (setq c (substr s i 1))
    (if (= c ch)
      (progn (setq kq (cons tam kq)) (setq tam ""))
      (setq tam (strcat tam c))
    )
    (setq i (1+ i))
  )
  (reverse (cons tam kq))
)

;; Chuỗi có phải TOÀN CHỮ SỐ và khác rỗng không
(defun stt-so-p (s / i k c ok)
  (setq k (strlen s))
  (if (= k 0)
    nil
    (progn
      (setq ok T i 1)
      (while (and ok (<= i k))
        (setq c (ascii (substr s i 1)))
        (if (or (< c 48) (> c 57)) (setq ok nil))
        (setq i (1+ i))
      )
      ok
    )
  )
)

;; Thêm số 0 vào đầu cho đủ độ rộng w (giữ kiểu 01, 02...)
(defun stt-pad (n w / s)
  (setq s (itoa n))
  (while (< (strlen s) w) (setq s (strcat "0" s)))
  s
)

;; Đọc chuỗi người dùng gõ -> danh sách các cấp, sai thì trả nil
;; "5.1.1" -> ("5" "1" "1")   |   "5,1" -> ("5" "1")   |   "abc" -> nil
(defun stt-doc-duongso (s / lst ok)
  (setq s (vl-string-trim " \t" s))
  (setq s (vl-string-translate "," "." s))   ;; gõ dấu phẩy cũng chấp nhận
  (while (and (> (strlen s) 0) (= (substr s 1 1) "."))
    (setq s (substr s 2))
  )
  (while (and (> (strlen s) 0) (= (substr s (strlen s) 1) "."))
    (setq s (substr s 1 (1- (strlen s))))
  )
  (if (= s "")
    nil
    (progn
      (setq lst (stt-tach s "."))
      (setq ok T)
      (foreach x lst (if (not (stt-so-p x)) (setq ok nil)))
      (if ok lst nil)
    )
  )
)

;; Ghép danh sách cấp thành chuỗi: ("5" "1") -> "5.1"
(defun stt-ghep (lst / s)
  (setq s "")
  (foreach x lst (setq s (if (= s "") x (strcat s "." x))))
  s
)

;; Tăng CẤP CUỐI CÙNG lên 1, các cấp trước giữ nguyên
;; ("5" "1") -> ("5" "2")   |   ("5" "1" "1") -> ("5" "1" "2")   |   ("1") -> ("2")
(defun stt-tang (lst / cuoi w n dau)
  (setq cuoi (last lst))
  (setq w (strlen cuoi))
  (setq n (1+ (atoi cuoi)))
  (setq dau (reverse (cdr (reverse lst))))
  (append dau
    (list (if (= (substr cuoi 1 1) "0") (stt-pad n w) (itoa n)))
  )
)

;;; ============================ LỆNH CHÍNH ===================================

(defun c:STTPHA (/ ent obj att prefix duongso tmp sopha phase str_val dem ok)
  (vl-load-com)

  (princ "\n========================================================")
  (princ "\n---> LISP DANH SO COT VA DAO PHA TU DONG (A-B-C) <---")
  (princ "\n---> Ho tro duong so phan cap 5.1 / 5.1.1 va day 1 pha <---")
  (princ "\n========================================================")

  ;; [1/4] Tiền tố
  (setq prefix (getstring t "\n[1/4] Nhap tien to Lo & Ten cot <II-Đ>: "))
  (if (= prefix "") (setq prefix "II-Đ"))

  ;; [2/4] Số / đường số bắt đầu - hỏi lại tới khi gõ đúng
  (setq duongso nil)
  (while (null duongso)
    (setq tmp (getstring "\n[2/4] Nhap so cot bat dau (1 | 5.1 | 5.1.1) <1>: "))
    (if (= (vl-string-trim " \t" tmp) "")
      (setq duongso (list "1"))
      (progn
        (setq duongso (stt-doc-duongso tmp))
        (if (null duongso)
          (princ "\n [!] Chi duoc gom CHU SO va dau CHAM. Vi du dung: 1 | 12 | 5.1 | 5.1.1")
        )
      )
    )
  )

  ;; [3/4] Đường dây mấy pha
  (setq tmp (getstring "\n[3/4] Duong day may pha? [3 = dao pha A-B-C | 1 = KHONG ghi pha] <3>: "))
  (setq tmp (vl-string-trim " \t" tmp))
  (setq sopha (if (and (/= tmp "") (= (substr tmp 1 1) "1")) 1 3))

  ;; [4/4] Pha bắt đầu - CHỈ HỎI khi 3 pha
  (if (= sopha 3)
    (progn
      (setq phase (strcase (getstring "\n[4/4] Nhap pha bat dau (A/B/C) <A>: ")))
      (if (not (member phase '("A" "B" "C"))) (setq phase "A"))
    )
    (setq phase nil)   ;; phase = nil chính là cờ "day 1 pha - khong ghi pha"
  )

  ;; Báo lại chế độ đang chạy + tên mẫu để soát trước khi click
  (if phase
    (princ (strcat "\n[OK] Che do 3 PHA  - ten mau: " prefix (stt-ghep duongso) "-" phase))
    (princ (strcat "\n[OK] Che do 1 PHA  - ten mau: " prefix (stt-ghep duongso) " (khong co duoi pha)"))
  )
  (princ (strcat "\n[OK] Se tang cap cuoi cung: "
                 (stt-ghep duongso) " -> " (stt-ghep (stt-tang duongso)) " -> ..."))
  (princ "\n[OK] Bay gio hay CLICK lan luot vao cac Block cot den... (Enter de ket thuc)")

  ;; Vòng lặp click chọn Block
  (setq dem 0)
  (while (setq ent (car (entsel "\nChon Cot den tiep theo: ")))
    (setq obj (vlax-ename->vla-object ent))
    (if (and (= (vla-get-ObjectName obj) "AcDbBlockReference")
             (= (vla-get-HasAttributes obj) :vlax-true))
      (progn
        ;; Tên cột = Tiền tố + Đường số ( + "-" + Pha nếu chạy 3 pha )
        (setq str_val
          (if phase
            (strcat prefix (stt-ghep duongso) "-" phase)
            (strcat prefix (stt-ghep duongso))
          )
        )

        ;; Gán giá trị vào Tag TEN_COT
        (setq ok nil)
        (foreach att (vlax-safearray->list (vlax-variant-value (vla-GetAttributes obj)))
          (if (= (strcase (vla-get-TagString att)) "TEN_COT")
            (progn
              (vla-put-TextString att str_val)
              (setq ok T)
            )
          )
        )

        (if ok
          (progn
            (vla-update obj)
            (princ (strcat "\n --> Da gan thanh cong: " str_val))
            (setq dem (1+ dem))
            (setq duongso (stt-tang duongso))          ;; tăng cấp cuối cùng
            (if phase                                  ;; đảo pha A -> B -> C -> A
              (setq phase
                (cond
                  ((= phase "A") "B")
                  ((= phase "B") "C")
                  (T             "A")
                )
              )
            )
          )
          (princ "\n [BO QUA] Block nay KHONG co thuoc tinh TEN_COT -> chua gan, so thu tu GIU NGUYEN.")
        )
      )
      (princ "\nDoi tuong chon khong phai Block thuoc tinh!")
    )
  )

  ;; Tổng kết để lần chạy sau nhập cho khớp
  (princ (strcat "\n========================================================"
                 "\n[XONG] Da danh so " (itoa dem) " cot."
                 "\n[XONG] So tiep theo: " (stt-ghep duongso)
                 (if phase
                   (strcat " | Pha tiep theo: " phase)
                   " | Dang o che do 1 pha (khong ghi pha)"
                 )
                 "\n========================================================"))
  (princ)
)

;; Tự động hiện bảng thông báo khi vừa APPLOAD file vào CAD
