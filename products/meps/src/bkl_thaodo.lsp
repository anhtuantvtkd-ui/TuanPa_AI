; BKL_THAODO (Lệnh: T-TDDZ / BKL_THAODO)
; Tự động bóc tách khối lượng tháo dỡ đường dây hạ thế 0,4kV & cột bê tông hiện trạng
; theo mẫu DICH CHUYEN DZ0.4KV.xlsx (bổ sung đầy đủ cột 1H7.5..2LT10, TĐKCS, Loa, Camera,
; ABC 2x16..4x95, H1..Tủ bù).
(vl-load-com)

(defun bkl-td:trim (s)
  (vl-string-trim (strcat " " (chr 9) (chr 10) (chr 13)) s))

(defun bkl-td:split (str sep / pos out)
  (while (setq pos (vl-string-search sep str))
    (setq out (cons (bkl-td:trim (substr str 1 pos)) out)
          str (substr str (+ pos (strlen sep) 1))))
  (reverse (cons (bkl-td:trim str) out)))

(defun bkl-td:mtext-lines (s / i ch code out lines cur)
  ; Tách MTEXT thành danh sách các dòng văn bản sạch (bỏ mã định dạng \P, \A, \C, \F...)
  (setq i 1 out "")
  (while (<= i (strlen s))
    (setq ch (substr s i 1) i (1+ i))
    (cond
      ((member ch '("{" "}")))
      ((= ch "\\")
       (setq code (substr s i 1) i (1+ i))
       (cond
         ((= code "P") (setq out (strcat out "\n")))
         ((= code "~") (setq out (strcat out " ")))
         ((member code '("\\" "{" "}")) (setq out (strcat out code)))
         ((and (member code '("A" "C" "c" "F" "f" "H" "Q" "T" "W" "p"))
               (setq cur (vl-string-search ";" s (1- i))))
          (setq i (+ cur 2)))))
      (T (setq out (strcat out ch)))))
  (setq lines (bkl-td:split out "\n"))
  (vl-remove-if '(lambda (x) (= x "")) lines))

(defun bkl-td:xy (a b)
  (distance (list (car a) (cadr a)) (list (car b) (cadr b))))

(defun bkl-td:parse-count-tag (tok / i ch digits tag)
  (setq tok (bkl-td:trim tok) i 1 digits "")
  (while (and (<= i (strlen tok)) (wcmatch (setq ch (substr tok i 1)) "[0-9]"))
    (setq digits (strcat digits ch) i (1+ i)))
  (setq tag (bkl-td:trim (substr tok i)))
  (list (if (/= digits "") (atoi digits) 1) (strcase tag)))

(defun bkl-td:norm-pole-type (s / u)
  (setq u (strcase (vl-list->string (vl-remove 32 (vl-string->list s)))))
  (cond
    ((= u "LT8.5") "1LT8.5")
    ((= u "LT10") "1LT10")
    ((= u "LT7.5") "1LT7.5")
    ((= u "H7.5") "1H7.5")
    ((= u "H8.5") "1H8.5")
    (T u)))

(defun bkl-td:parse-pole (entity data / raw lines line u name ptype keep
                                      xa cd tdkcs loa cam h1 h2 h4 h3f htg tb
                                      toks tok parsed cnt tag pos noteParts)
  (setq raw "")
  (foreach f data (if (member (car f) '(1 3)) (setq raw (strcat raw (cdr f)))))
  (setq lines (bkl-td:mtext-lines raw))
  (if (vl-some '(lambda (l) (or (wcmatch (strcase l) "*TÊN CỘT*")
                                (wcmatch (strcase l) "*LOẠI CỘT*")
                                (wcmatch (strcase l) "*HT:*"))) lines)
    (progn
      (setq xa 0 cd 0 tdkcs 0 loa 0 cam 0 h1 0 h2 0 h4 0 h3f 0 htg 0 tb 0)
      (foreach line lines
        (setq u (strcase (bkl-td:trim line)))
        (cond
          ((wcmatch u "*TÊN CỘT:*")
           (setq name (bkl-td:trim (substr line (+ (vl-string-search ":" line) 2)))))
          ((wcmatch u "*LOẠI CỘT:*")
           (setq ptype (bkl-td:norm-pole-type (substr line (+ (vl-string-search ":" line) 2)))))
          ((wcmatch u "*HT:*")
           (if (wcmatch u "*GIỮ*") (setq keep T))
           ; Trường hợp ghi chung trên dòng HT: "HT: THÁO DỠ, 1LOA" hoặc "HT: THÁO DỠ, CĐM"
           (if (vl-string-search "," line)
             (setq toks (append toks (cdr (bkl-td:split line ","))))))
          (T
           (setq line (vl-string-trim "- " line))
           (setq toks (append toks (bkl-td:split line ","))))))
      (foreach tok toks
        (setq parsed (bkl-td:parse-count-tag tok)
              cnt (car parsed) tag (cadr parsed))
        (cond
          ((wcmatch tag "*XÀ*,*X4F*,*X1-4F*,*X1F*") (setq xa (+ xa cnt)))
          ((wcmatch tag "*CĐ*ĐÔI*,*CDM*DOI*") (setq cd (+ cd (* 2 cnt))))
          ((wcmatch tag "*CĐ*,*CDM*") (setq cd (+ cd cnt)))
          ((wcmatch tag "*TĐKCS*") (setq tdkcs (+ tdkcs cnt)))
          ((wcmatch tag "*CAMERA*,*TĐK+CAM*,*CAM+TĐK*") (setq cam (+ cam cnt)))
          ((wcmatch tag "*LOA*") (setq loa (+ loa cnt)))
          ((wcmatch tag "H1*") (setq h1 (+ h1 cnt)))
          ((wcmatch tag "H2*") (setq h2 (+ h2 cnt)))
          ((wcmatch tag "H4*") (setq h4 (+ h4 cnt)))
          ((wcmatch tag "H3F*") (setq h3f (+ h3f cnt)))
          ((wcmatch tag "HTG*") (setq htg (+ htg cnt)))
          ((wcmatch tag "TB*,*TỦ BÙ*") (setq tb (+ tb cnt)))))
      (setq pos (cdr (assoc 10 data)))
      (if (= (cdr (assoc 0 data)) "TEXT") (setq pos (trans pos entity 0)))
      (setq pos (list (car pos) (cadr pos)))
      (list 'pole pos name ptype keep
            (list xa cd tdkcs loa cam)
            (list h1 h2 h4 h3f htg tb)))))

(defun bkl-td:parse-wire (entity data / raw s u pos mult idx ch digits body cut tdPos lenStr lenVal wtype)
  (setq raw "")
  (foreach f data (if (member (car f) '(1 3)) (setq raw (strcat raw (cdr f)))))
  (setq s (car (bkl-td:mtext-lines raw)))
  (if s
    (progn
      (setq u (strcase (vl-list->string (vl-remove 32 (vl-string->list s)))))
      (if (and (wcmatch u "*VX*(TD-*") (not (vl-string-search ":" u)))
        (progn
          (setq idx 1 digits "")
          (while (and (<= idx (strlen u)) (wcmatch (setq ch (substr u idx 1)) "[0-9]"))
            (setq digits (strcat digits ch) idx (1+ idx)))
          (setq mult (if (/= digits "") (atoi digits) 1)
                body (substr u idx)
                cut (vl-string-search "(TD-" body)
                tdPos (+ cut 5)
                lenStr (substr body tdPos))
          (if (setq ch (vl-string-search "M)" lenStr))
            (setq lenStr (substr lenStr 1 ch)))
          (setq lenVal (atof (vl-string-translate "," "." lenStr))
                body (substr body 1 cut))
          (setq wtype (cond
                        ((= body "VX2X16") "2X16")
                        ((= body "VX16") "4X16")
                        ((= body "VX25") "4X25")
                        ((= body "VX35") "4X35")
                        ((= body "VX2X50") "2X50")
                        ((= body "VX50") "4X50")
                        ((= body "VX70") "4X70")
                        ((= body "VX95") "4X95")))
          (if (and wtype (> lenVal 0))
            (progn
              (setq pos (cdr (assoc 10 data)))
              (if (= (cdr (assoc 0 data)) "TEXT") (setq pos (trans pos entity 0)))
              (list 'wire (list (car pos) (cadr pos)) wtype mult lenVal s))))))))

(defun bkl-td:collect (selection / i ent data kind rec poles wires segs pts prev cur leaders)
  (setq i 0)
  (while (< i (sslength selection))
    (setq ent (ssname selection i) i (1+ i) data (entget ent) kind (cdr (assoc 0 data)))
    (cond
      ((member kind '("TEXT" "MTEXT"))
       (cond
         ((setq rec (bkl-td:parse-pole ent data)) (setq poles (cons (cdr rec) poles)))
         ((setq rec (bkl-td:parse-wire ent data)) (setq wires (cons (cdr rec) wires)))))
      ((= kind "LWPOLYLINE")
       (setq pts (mapcar 'cdr (vl-remove-if-not '(lambda (x) (= (car x) 10)) data)))
       (setq prev (car pts) pts (cdr pts))
       (foreach cur pts
         (if (> (bkl-td:xy prev cur) 0.5)
           (setq segs (cons (list prev cur) segs)))
         (setq prev cur)))
      ((= kind "LINE")
       (setq prev (list (cadr (assoc 10 data)) (caddr (assoc 10 data)))
             cur  (list (cadr (assoc 11 data)) (caddr (assoc 11 data))))
       (if (> (bkl-td:xy prev cur) 0.5)
         (setq segs (cons (list prev cur) segs))))
      ((= kind "LEADER")
       (setq pts (mapcar 'cdr (vl-remove-if-not '(lambda (x) (= (car x) 10)) data)))
       (if (>= (length pts) 2)
         (setq leaders (cons (list (last pts) (car pts)) leaders))))))
  (list poles wires segs leaders))

(defun bkl-td:snap-poles (poles leaders / out idx p pos name ld bestD bestTip d)
  ; Nếu có LEADER trỏ từ hộp chữ vào cột, lấy tọa độ đầu mũi tên làm tọa độ cột
  (setq idx 0)
  (foreach p poles
    (setq pos (car p) name (cadr p))
    (if (not name) (setq idx (1+ idx) name (strcat "Cột Đèn " (itoa idx))))
    (setq bestD 8.0 bestTip pos)
    (foreach ld leaders
      (setq d (bkl-td:xy pos (car ld)))
      (if (< d bestD) (setq bestD d bestTip (cadr ld))))
    (setq out (cons (cons bestTip (cons name (cddr p))) out)))
  (reverse out))

(defun bkl-td:seg-dist (p a b / dx dy len2 tval qx qy)
  (setq dx (- (car b) (car a)) dy (- (cadr b) (cadr a)) len2 (+ (* dx dx) (* dy dy)))
  (if (<= len2 1e-9) (bkl-td:xy p a)
    (progn
      (setq tval (max 0.0 (min 1.0 (/ (+ (* (- (car p) (car a)) dx) (* (- (cadr p) (cadr a)) dy)) len2)))
            qx (+ (car a) (* tval dx)) qy (+ (cadr a) (* tval dy)))
      (bkl-td:xy p (list qx qy)))))

(defun bkl-td:build-rows (poles wires / usedWires sortedPoles rows stt i p1 p2 dmin bestW
                                      w d span wtype mult w2x16 w4x16 w4x25 w4x35 w2x50 w4x50 w4x70 w4x95
                                      ptype keep acc hom poleCols r note lastRow)
  (setq sortedPoles (vl-sort poles '(lambda (a b) (< (caar a) (caar b))))
        stt 0 r 4)
  ; Tạo các khoảng cột liên tiếp theo tuyến và gom nhãn dây gần nhất vào từng khoảng
  (setq i 1)
  (while (< i (length sortedPoles))
    (setq p1 (nth (1- i) sortedPoles)
          p2 (nth i sortedPoles)
          i (1+ i)
          stt (1+ stt) r (1+ r)
          span 0.0
          w2x16 0 w4x16 0 w4x25 0 w4x35 0 w2x50 0 w4x50 0 w4x70 0 w4x95 0)
    (foreach w wires
      (if (<= (bkl-td:seg-dist (car w) (car p1) (car p2)) 15.0)
        (progn
          (setq usedWires (cons w usedWires)
                wtype (cadr w) mult (caddr w)
                span (max span (cadddr w)))
          (cond
            ((= wtype "2X16") (setq w2x16 (+ w2x16 mult)))
            ((= wtype "4X16") (setq w4x16 (+ w4x16 mult)))
            ((= wtype "4X25") (setq w4x25 (+ w4x25 mult)))
            ((= wtype "4X35") (setq w4x35 (+ w4x35 mult)))
            ((= wtype "2X50") (setq w2x50 (+ w2x50 mult)))
            ((= wtype "4X50") (setq w4x50 (+ w4x50 mult)))
            ((= wtype "4X70") (setq w4x70 (+ w4x70 mult)))
            ((= wtype "4X95") (setq w4x95 (+ w4x95 mult)))))))
    (if (<= span 0.0) (setq span (float (max 1 (fix (+ 0.5 (bkl-td:xy (car p1) (car p2))))))))
    (setq ptype (caddr p2) keep (cadddr p2) acc (nth 4 p2) hom (nth 5 p2)
          note (if keep (strcat "Cột " (cadr p2) " giữ nguyên") ""))
    (setq poleCols
      (if keep '(0 0 0 0 0 0 0 0 0)
        (list (if (= ptype "1H7.5") 1 0) (if (= ptype "2H7.5") 1 0)
              (if (= ptype "1H8.5") 1 0) (if (= ptype "2H8.5") 1 0)
              (if (= ptype "1LT7.5") 1 0) (if (= ptype "1LT8.5") 1 0)
              (if (= ptype "2LT8.5") 1 0) (if (= ptype "1LT10") 1 0)
              (if (= ptype "2LT10") 1 0))))
    (setq rows (cons (append (list (itoa stt) (cadr p1) (cadr p2) (rtos span 2 1))
                             (mapcar '(lambda (x) (if (> x 0) (itoa x) "")) poleCols)
                             (mapcar '(lambda (x) (if (> x 0) (itoa x) "")) acc)
                             (mapcar '(lambda (m / rs)
                                        (setq rs (itoa r))
                                        (if (> m 0) (strcat "=D" rs "*" (itoa m)) ""))
                                     (list w2x16 w4x16 w4x25 w4x35 w2x50 w4x50 w4x70 w4x95))
                             (mapcar '(lambda (x) (if (> x 0) (itoa x) "")) hom)
                             (list note)) rows)))
  (if (> r 4)
    (progn
      (setq lastRow (itoa r))
      (setq rows (cons (list "TỔNG CỘNG" "" "" (strcat "=SUM(D5:D" lastRow ")")
                             (strcat "=SUM(E5:E" lastRow ")") (strcat "=SUM(F5:F" lastRow ")")
                             (strcat "=SUM(G5:G" lastRow ")") (strcat "=SUM(H5:H" lastRow ")")
                             (strcat "=SUM(I5:I" lastRow ")") (strcat "=SUM(J5:J" lastRow ")")
                             (strcat "=SUM(K5:K" lastRow ")") (strcat "=SUM(L5:L" lastRow ")")
                             (strcat "=SUM(M5:M" lastRow ")") (strcat "=SUM(N5:N" lastRow ")")
                             (strcat "=SUM(O5:O" lastRow ")") (strcat "=SUM(P5:P" lastRow ")")
                             (strcat "=SUM(Q5:Q" lastRow ")") (strcat "=SUM(R5:R" lastRow ")")
                             (strcat "=SUM(S5:S" lastRow ")") (strcat "=SUM(T5:T" lastRow ")")
                             (strcat "=SUM(U5:U" lastRow ")") (strcat "=SUM(V5:V" lastRow ")")
                             (strcat "=SUM(W5:W" lastRow ")") (strcat "=SUM(X5:X" lastRow ")")
                             (strcat "=SUM(Y5:Y" lastRow ")") (strcat "=SUM(Z5:Z" lastRow ")")
                             (strcat "=SUM(AA5:AA" lastRow ")") (strcat "=SUM(AB5:AB" lastRow ")")
                             (strcat "=SUM(AC5:AC" lastRow ")") (strcat "=SUM(AD5:AD" lastRow ")")
                             (strcat "=SUM(AE5:AE" lastRow ")") (strcat "=SUM(AF5:AF" lastRow ")")
                             "") rows))))
  (reverse rows))

(defun bkl-td:format-xlsx (csvPath xlsxPath / psScript psFile wsh cmd)
  (setq psFile (strcat (getvar "TEMPPREFIX") "bkl_td_excel.ps1"))
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
      (write-line "  $ws.Cells.Font.Name = 'Times New Roman'; $ws.Cells.Font.Size = 11" psScript)
      (write-line "  $ws.Range('A1:AG1').Merge(); $ws.Range('A1:AG1').Font.Size = 13; $ws.Range('A1:AG1').Font.Bold = $true" psScript)
      (write-line "  foreach ($m in @('A2:A4','B2:B4','C2:C4','D2:D4','E2:M3','N2:R3','S2:Z3','AA2:AF2','AA3:AF3','AG2:AG4')) { $ws.Range($m).Merge() }" psScript)
      (write-line "  $h = $ws.Range('A2:AG4'); $h.Font.Bold = $true; $h.WrapText = $true; $h.HorizontalAlignment = -4108; $h.VerticalAlignment = -4108" psScript)
      (write-line "  $ws.Range('A5:AG' + $lastRow).VerticalAlignment = -4108" psScript)
      (write-line "  $ws.Range('D5:AF' + $lastRow).HorizontalAlignment = -4108" psScript)
      (write-line "  $ws.Range('A' + $lastRow + ':C' + $lastRow).Merge(); $ws.Rows.Item($lastRow).Font.Bold = $true" psScript)
      (write-line "  $ws.Range('A2:AG' + $lastRow).Borders.LineStyle = 1; $ws.Range('A2:AG' + $lastRow).Borders.Weight = 2" psScript)
      (write-line "  $ws.Columns.Item(1).ColumnWidth = 6; $ws.Columns.Item(2).ColumnWidth = 20; $ws.Columns.Item(3).ColumnWidth = 22; $ws.Columns.Item(4).ColumnWidth = 11; $ws.Columns.Item(33).ColumnWidth = 30" psScript)
      (write-line "  $wb.SaveAs($xlsxPath, 51); $wb.Close($false)" psScript)
      (write-line "} finally { $excel.Quit(); [System.Runtime.InteropServices.Marshal]::ReleaseComObject($excel) | Out-Null }" psScript)
      (close psScript)
      (if (setq wsh (vlax-get-or-create-object "WScript.Shell"))
        (progn
          (setq cmd (strcat "powershell.exe -ExecutionPolicy Bypass -WindowStyle Hidden -File \"" psFile "\" \"" csvPath "\" \"" xlsxPath "\""))
          (vlax-invoke-method wsh 'Run cmd 0 1)
          (vlax-release-object wsh)
          (findfile xlsxPath))))))

(defun c:BKL_THAODO (/ filter ss col poles wires rows path csvPath xlsxPath ext fp headers row line val i)
  (setq filter (list '(0 . "TEXT,MTEXT,LWPOLYLINE,LINE,LEADER")
                     (cons 410 (if (= (getvar "CVPORT") 1) (getvar "CTAB") "Model"))))
  (princ "\nQuét chọn các cột điện, nhãn dây tháo dỡ và tuyến dây <Enter để chọn tất cả>: ")
  (setq ss (ssget))
  (if (not ss) (setq ss (ssget "_X" filter)))
  (if (not ss)
    (princ "\nBKL_THAODO: Không có đối tượng phù hợp.")
    (progn
      (setq col (bkl-td:collect ss)
            poles (bkl-td:snap-poles (car col) (cadddr col))
            wires (cadr col))
      (if (< (length poles) 2)
        (princ "\nBKL_THAODO: Cần ít nhất 2 cột có hộp thông tin TÊN CỘT / LOẠI CỘT / HT.")
        (if (setq path (getfiled "Lưu bảng khối lượng tháo dỡ 0,4kV"
                         (strcat (getvar "DWGPREFIX") "KL_THAO_DO_DZ04KV.xlsx") "xlsx;csv" 1))
          (progn
            (setq ext (strcase (substr path (- (strlen path) 3))))
            (if (= ext "XLSX")
              (setq xlsxPath path csvPath (strcat (substr path 1 (- (strlen path) 5)) ".csv"))
              (setq csvPath path xlsxPath (strcat (substr path 1 (- (strlen path) 4)) ".xlsx")))
            (setq headers
              '(("PHẦN: KHỐI LƯỢNG THÁO DỠ ĐƯỜNG DÂY HẠ THẾ 0,4KV")
                ("Số TT" "Từ cột" "Đến cột" "Khoảng cột (m)"
                 "Cột bê tông tháo dỡ" "" "" "" "" "" "" "" ""
                 "Xà, phụ kiện" "" "" "" ""
                 "Cáp nhôm vặn xoắn tháo dỡ (m)" "" "" "" "" "" "" ""
                 "Hòm công tơ tháo dỡ" "" "" "" "" "" "Ghi chú")
                ("" "" "" "" "" "" "" "" "" "" "" "" "" "" "" "" "" "" "" "" "" "" "" "" "" "" "Tháo dỡ" "" "" "" "" "" "")
                ("" "" "" "" "1H7.5" "2H7.5" "1H8.5" "2H8.5" "1LT7.5" "1LT8.5" "2LT8.5" "1LT10" "2LT10"
                 "Xà X4F/X1F" "Cần đèn CĐ1" "TĐKCS" "Loa" "Camera + TĐK"
                 "ABC 2x16" "ABC 4x16" "ABC 4x25" "ABC 4x35" "ABC 2x50" "ABC 4x50" "ABC 4x70" "ABC 4x95"
                 "H1" "H2" "H4" "H3F" "HTG" "Tủ bù" "")))
            (setq rows (append headers (bkl-td:build-rows poles wires))
                  fp (open csvPath "w" "utf8-bom"))
            (if fp
              (progn
                (foreach row rows
                  (setq i 0 line "")
                  (repeat 33
                    (if (> i 0) (setq line (strcat line ";")))
                    (if row (setq val (car row) row (cdr row)) (setq val ""))
                    (setq line (strcat line val))
                    (setq i (1+ i)))
                  (write-line line fp))
                (close fp)
                (bkl-td:format-xlsx csvPath xlsxPath)
                (princ (strcat "\nBKL_THAODO: Đã xuất " (itoa (1- (length (bkl-td:build-rows poles wires))))
                               " khoảng cột vào: " xlsxPath)))
              (princ "\nBKL_THAODO: Không ghi được file CSV.")))))))
  (princ))

(defun c:T-TDDZ () (c:BKL_THAODO))
(defun c:BKL_TD () (c:BKL_THAODO))
(princ "\n=== BKL_THAODO da nap. Go lenh T-TDDZ hoac BKL_THAODO de boc tach thao do DZ 0.4kV ===")
(princ)
