; TASK-0024: bảng bóc tách theo Book1.xlsx và docs/tasks/TASK-0024-spec.md.
; Tuyến thẳng theo X, tọa độ WCS theo mét. Đầu mút Dim dọc tuyến
; nối nhau trong 0.25 m để chấp nhận sai lệch điểm bắt trên bản vẽ mẫu.
; Nhãn hộ/cáp chiếu theo X tối đa 2 m tới nút lô. Nhãn cáp nằm giữa tuyến và nhãn hộ.
; Với lô ngay tủ, lấy Dim rẽ nhánh tại nút làm chiều dài cột D.

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
      (if (and n (> n 0)) (setq result (itoa (fix (+ n 0.5))))))
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
    (bklcap:fail (strcat "Dim thiếu chiều dài hợp lệ (handle " (cdr (assoc 5 data)) ")."))))

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

(defun bklcap:cabinet (entity data / block attrs next record name station)
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
      (if (wcmatch (strcase (if station station "")) "TBA *")
        (setq station (bklcap:trim (substr station 5))))
      (if (not (and name station (/= station "")))
        (bklcap:fail "Block Tu Cong To thiếu TEN_TU hoặc TEN_TBA."))
      (list (trans (cdr (assoc 10 data)) entity 0)
            (strcat (vl-string-translate "Ðð" "Đđ" name) "/" station)))))

(defun bklcap:text (entity data / s key p number cut phase field close-tag cabinet-tag)
  (setq s "")
  (foreach field data
    (if (member (car field) '(1 3)) (setq s (strcat s (cdr field)))))
  (setq s (bklcap:plain s)
        key (strcase (vl-string-translate "ốà" "ỐÀ" s))
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
    ((and (wcmatch key "DSTA *") (not (vl-string-search ":" key)))
     (setq cut (vl-string-search "(" key))
     (if (and cut (setq close-tag (vl-string-search ")" key (1+ cut))))
       (setq cabinet-tag (bklcap:trim (substr s (+ cut 2) (- close-tag cut 1)))))
     (setq
           key (vl-string-translate "*×" "XX"
                 (vl-list->string
                   (vl-remove 32 (vl-string->list (if cut (substr key 1 cut) key))))))
     (setq phase (cond ((= key "DSTA2X10") 1)
                       ((member key '("DSTA16" "DSTA3X16+1X10")) 3)))
     (list 'cable p phase s cabinet-tag))))

(defun bklcap:collect (selection / i entity data kind record a b mode main branches cabinets houses cables)
  (setq i 0)
  (while (< i (sslength selection))
    (setq entity (ssname selection i) i (1+ i)
          data (entget entity) kind (cdr (assoc 0 data)))
    (cond
      ((and (= kind "INSERT") (setq record (bklcap:cabinet entity data)))
       (setq cabinets (cons record cabinets)))
      ((and (= kind "DIMENSION")
            (member (logand 7 (cdr (assoc 70 data))) '(0 1))
            (setq a (cdr (assoc 13 data)) b (cdr (assoc 14 data))))
       (if (> (car a) (car b)) (setq record a a b b record))
       (setq mode (if (> (- (car b) (car a)) (abs (- (cadr b) (cadr a)))) 'main 'branch)
             record (list a b data))
       (if (= mode 'main) (setq main (cons record main))
         (setq branches (cons record branches))))
      ((and (member kind '("TEXT" "MTEXT")) (setq record (bklcap:text entity data)))
       (if (= (car record) 'house) (setq houses (cons (cdr record) houses))
         (setq cables (cons (cdr record) cables))))))
  (list main branches cabinets houses cables))

(defun bklcap:xy (a b)
  (distance (list (car a) (cadr a)) (list (car b) (cadr b))))

(defun bklcap:chains (main / chains nodes edge dim-record)
  ; Mỗi nút giữ Dim dẫn đến nó. Giữ nguyên Dim cho đến khi tuyến cần dùng.
  (foreach dim-record (mapcar '(lambda (i) (nth i main))
                        (vl-sort-i main '(lambda (a b) (< (caar a) (caar b)))))
    (setq edge (car dim-record))
    (if (and nodes (> (bklcap:xy edge (caar nodes)) 0.25))
      (setq chains (cons (reverse nodes) chains) nodes nil))
    (if (not nodes) (setq nodes (list (list edge nil))))
    (setq nodes (cons (list (cadr dim-record) (caddr dim-record)) nodes)))
  (if nodes (setq chains (cons (reverse nodes) chains)))
  (reverse chains))

(defun bklcap:nearest (p records measure / best best-dist d tied record)
  (foreach record records
    (setq d (apply measure (list p (car record))))
    (cond
      ((or (null best-dist) (< d (- best-dist 0.000001)))
       (setq best record best-dist d tied nil))
      ((equal d best-dist 0.000001) (setq tied T))))
  (if (not tied) best))

(defun bklcap:x-gap (a b) (abs (- (car a) (car b))))

(defun bklcap:lot (p nodes / node)
  (setq node (bklcap:nearest p nodes 'bklcap:x-gap))
  (if (and node (<= (bklcap:x-gap p (car node)) 2.0))
    node))

(defun bklcap:cabinet-key (name)
  (strcase (vl-string-translate "Ððđ" "ĐđĐ" name)))

(defun bklcap:serving-cabinet (node chain cab-nodes label / eligible tag)
  (setq eligible (vl-remove-if-not '(lambda (item) (member (caddr item) chain)) cab-nodes)
        tag (nth 3 label))
  (if (and tag (/= tag ""))
    (progn
      (setq tag (bklcap:cabinet-key tag))
      (setq eligible
        (vl-remove-if-not
          '(lambda (item / name slash)
             (setq name (bklcap:cabinet-key (cadadr item))
                   slash (vl-string-search "/" name))
             (or (= tag name) (and slash (= tag (substr name 1 slash)))))
          eligible))))
  (bklcap:nearest (car node) eligible 'bklcap:x-gap))

(defun bklcap:branch (node branches / matches edge)
  (foreach edge branches
    (if (<= (min (bklcap:xy (car node) (car edge))
                 (bklcap:xy (car node) (cadr edge))) 0.25)
      (setq matches (cons edge matches))))
  (if (= (length matches) 1) (bklcap:dim-length (caddar matches))
    (bklcap:fail "Cần đúng một Dim rẽ nhánh tại lô ngay tủ.")))

(defun bklcap:route (nodes start end branches / left right i parts)
  (setq left (vl-position start nodes) right (vl-position end nodes))
  (if (= left right)
    (list (bklcap:branch start branches))
    (progn
      (setq i (1+ (min left right)))
      (while (<= i (max left right))
        (setq parts (cons (bklcap:dim-length (cadr (nth i nodes))) parts) i (1+ i)))
      (if (< left right) (reverse parts) parts))))

(defun bklcap:match-houses (input / chains nodes cab-nodes cab node groups chain house cable p
                                 candidates label prior rows)
  (if (not (and (car input) (caddr input) (cadddr input) (nth 4 input)))
    (bklcap:fail "Thiếu tủ, Dim dọc tuyến, số nhà hoặc nhãn cáp."))
  (setq chains (bklcap:chains (car input)) nodes (apply 'append chains))
  (foreach cab (caddr input)
    (setq node (bklcap:nearest (car cab) nodes 'bklcap:xy))
    (if (not (and node (<= (bklcap:xy (car cab) (car node)) 0.25)))
      (bklcap:fail (strcat "Tủ chưa khớp nút Dim: " (cadr cab))))
    (setq cab-nodes (cons (list (car node) cab node) cab-nodes)))
  (foreach house (cadddr input)
    (setq p (car house) node (bklcap:lot p nodes))
    (if (not node) (bklcap:fail (strcat "Không xác định được lô: " (caddr house))))
    (setq chain (car (vl-remove-if-not '(lambda (items) (member node items)) chains)))
    (setq candidates nil)
    (foreach cable (nth 4 input)
      (if (and (equal node (bklcap:lot (car cable) nodes))
               (<= (abs (- (caar cable) (car p))) 2.0)
               (< (* (- (cadar cable) (cadar node)) (- (cadar cable) (cadr p))) 0.0))
        (setq candidates (cons cable candidates))))
    (setq label (bklcap:nearest p candidates 'bklcap:x-gap))
    (if (or (not label) (not (cadr label)) (member label prior))
      (bklcap:fail (strcat "Thiếu, trùng hoặc sai chủng loại nhãn cáp: " (caddr house))))
    (setq cab (bklcap:serving-cabinet node chain cab-nodes label))
    (if (not cab) (bklcap:fail (strcat "Không xác định được tủ: " (caddr house))))
    (setq prior (cons label prior)
          rows (cdr (assoc (cadr cab) groups)))
    (if (assoc (cadr house) rows)
      (bklcap:fail (strcat "Trùng số nhà trong cùng tủ: " (caddr house))))
    (setq rows (cons (list (cadr house) (caddr house)
                           (bklcap:route chain (caddr cab) node (cadr input))
                           (cadr label)) rows))
    (if (assoc (cadr cab) groups)
      (setq groups (subst (cons (cadr cab) rows) (assoc (cadr cab) groups) groups))
      (setq groups (cons (cons (cadr cab) rows) groups))))
  (mapcar '(lambda (i) (nth i groups))
    (vl-sort-i groups '(lambda (a b) (< (cadar a) (cadar b))))))

(defun bklcap:sum-formula (parts / result part)
  (setq result (car parts))
  (if (cdr parts)
    (progn
      (foreach part (cdr parts) (setq result (strcat result "+" part)))
      (setq result (strcat "=" result))))
  result)

(defun bklcap:detail (house row-number / r single total)
  ; Book1.xlsx A:N, hàng chi tiết. Các phần bù E:G đều tính theo mét.
  (setq r (itoa row-number) single (= (nth 3 house) 1)
        total (strcat "D" r "+E" r "+F" r "+G" r))
  (list "" "" (cadr house) (bklcap:sum-formula (caddr house))
        "2" "=0.7+1.5" "3" (if (= (rem (car house) 2) 1) (strcat "=E" r) "0")
        (if single "1" "") (if single "" "1")
        (if single (strcat "=" total "-2") "0")
        (if single (strcat "=M" r "-J" r) (strcat "=M" r "-2"))
        (strcat "=(" total ")*J" r) (strcat "=(" total ")*I" r)))

(defun bklcap:rows (groups / rows index row-number group houses house)
  (setq rows '(("DIỄN GIẢI KHỐI LƯỢNG ĐƯỜNG DÂY 0,4KV SINH HOẠT CẤP ĐẾN HỘ DÂN")
               ("STT" "Tên tủ" "Số phụ tải (hộ)" "Từ tủ điện đến lô đất"
                "Từ rãnh cáp đến đầu lô đất" "Lên xuống rãnh cáp"
                "Lên điểm đấu vào nhà hiện trạng + dự phòng"
                "rãnh cáp từ trục chính đến hộ dân (m)"
                "Số công tơ 1F\n (2x10mm2)" "Số công tơ 3F (3x16+1x10)" "Ống HDPE gân xoắn")
               ("" "" "" "" "" "" "" "" "" "" "40/30 (DSTA 10)" "50/40 (DSTA 16)" "3x16+1x10" "2x10")
               ()
               ("A. ĐƯỜNG DÂY 0,4KV SINH HOẠT HOÀN TRẢ SAU TBA ")
               ("LỘ I"))
        rows (reverse rows) row-number 6 index 0)
  (foreach group groups
    (setq houses (vl-sort (cdr group) '(lambda (a b) (< (car a) (car b))))
          index (1+ index) row-number (1+ row-number)
          rows (cons (list (itoa index) (cadar group) (itoa (length houses))) rows))
    (foreach house houses
      (setq row-number (1+ row-number) rows (cons (bklcap:detail house row-number) rows))))
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
  (repeat 14
    (if (> i 0) (setq line (strcat line separator)))
    (if cells
      (progn
        (setq value (car cells) cells (cdr cells))
        (if (and (member i '(3 5)) (wcmatch value "[=0-9]*"))
          (setq value (vl-string-translate "." decimal value)))
        (setq line (strcat line (bklcap:csv-field value separator)))))
    (setq i (1+ i)))
  line)

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

(defun c:BKL_CAP (/ filter selection result path)
  (if (not (equal (ascii "Đ") 272))
    (princ "\nBKL_CAP: Dùng AutoCAD 2021 trở lên. Đặt LISPSYS bằng 1 rồi khởi động lại AutoCAD.")
    (progn
      (setq filter (list '(0 . "INSERT,DIMENSION,TEXT,MTEXT")
                         (cons 410 (if (= (getvar "CVPORT") 1) (getvar "CTAB") "Model"))))
      (princ "\nQuét chọn tủ điện, Dim tuyến cáp, số nhà và nhãn cáp <Enter để chọn tất cả>: ")
      (setq selection (ssget))
      (if (not selection) (setq selection (ssget "_X" filter)))
      (cond
        ((not selection) (princ "\nBKL_CAP: Không có đối tượng phù hợp."))
        ((= (type (setq result (bklcap:build selection))) 'STR)
         (princ (strcat "\nBKL_CAP: " result)))
        ((setq path (getfiled "Lưu bảng bóc tách cáp điện"
                      (strcat (getvar "DWGPREFIX") "BKL_CAP_DIEN.csv") "csv" 1))
         (if (bklcap:export path (bklcap:rows result))
           (princ (strcat "\nBKL_CAP: Đã xuất "
                          (itoa (apply '+ (mapcar '(lambda (g) (length (cdr g))) result)))
                          " hộ vào " path))
           (princ "\nBKL_CAP: Không ghi được CSV."))))))
  (princ))

(defun c:BKLCAP () (c:BKL_CAP))

(princ)
