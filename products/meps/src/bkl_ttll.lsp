; TASK-0026: Lệnh T-THTTLL (alias BKL_TTLL, TTLL)
; Bóc tách khối lượng hệ thống Thông tin liên lạc (TTLL) từ bản vẽ AutoCAD (DXF-only)
; Xuất file Excel .xlsx 3 sheet (THKL, DG TTLL, DM LOAI ONG) với công thức <f> + giá trị <v>
; và file .csv đối soát + danh sách WARNINGS.
(vl-load-com)

(defun bkl-ttll:trim (s)
  (if s (vl-string-trim (strcat " " (chr 9) (chr 10) (chr 13)) s) ""))

(defun bkl-ttll:split (str sep / pos out)
  (setq out nil)
  (while (setq pos (vl-string-search sep str))
    (setq out (cons (bkl-ttll:trim (substr str 1 pos)) out)
          str (substr str (+ pos (strlen sep) 1))))
  (reverse (cons (bkl-ttll:trim str) out)))

(defun bkl-ttll:hex-val (s / i ch v code)
  (setq i 1 v 0)
  (while (<= i (strlen s))
    (setq ch (ascii (strcase (substr s i 1))) i (1+ i))
    (cond
      ((and (>= ch 48) (<= ch 57)) (setq code (- ch 48)))
      ((and (>= ch 65) (<= ch 70)) (setq code (+ 10 (- ch 65))))
      (T (setq code -1)))
    (if (>= code 0) (setq v (+ (* v 16) code)) (setq v -1 i 999)))
  v)

(defun bkl-ttll:plain (s / i ch code end out hex uval)
  (if (not s) ""
    (progn
      (setq i 1 out "")
      (while (<= i (strlen s))
        (setq ch (substr s i 1) i (1+ i))
        (cond
          ((member ch '("{" "}")) nil)
          ((= ch "\\")
           (if (<= i (strlen s))
             (progn
               (setq code (substr s i 1) i (1+ i))
               (cond
                 ((member code '("P" "~")) (setq out (strcat out " ")))
                 ((member code '("\\" "{" "}")) (setq out (strcat out code)))
                 ((and (member code '("U" "u"))
                       (<= (+ i 4) (strlen s))
                       (= (substr s i 1) "+")
                       (>= (setq uval (bkl-ttll:hex-val (substr s (1+ i) 4))) 0))
                  (setq out (strcat out (cond ((= uval 272) "Đ") ((= uval 273) "đ") ((< uval 256) (chr uval)) (T "")))
                        i (+ i 5)))
                 ((and (member code '("A" "C" "c" "F" "f" "H" "Q" "T" "W" "p" "S"))
                       (setq end (vl-string-search ";" s (1- i))))
                  (setq i (+ end 2)))
                 ((member code '("L" "l" "O" "o" "K" "k")) nil)))))
          (T (setq out (strcat out ch)))))
      (bkl-ttll:trim out))))

(defun bkl-ttll:xy (a b)
  (distance (list (car a) (cadr a)) (list (car b) (cadr b))))

(defun bkl-ttll:pt2d (p)
  (list (float (car p)) (float (cadr p))))

(defun bkl-ttll:seg-dist (p a b / dx dy len2 tval qx qy)
  (setq dx (- (car b) (car a))
        dy (- (cadr b) (cadr a))
        len2 (+ (* dx dx) (* dy dy)))
  (if (<= len2 1e-9)
    (bkl-ttll:xy p a)
    (progn
      (setq tval (max 0.0 (min 1.0 (/ (+ (* (- (car p) (car a)) dx)
                                          (* (- (cadr p) (cadr a)) dy))
                                       len2)))
            qx (+ (car a) (* tval dx))
            qy (+ (cadr a) (* tval dy)))
      (bkl-ttll:xy p (list qx qy)))))

(defun bkl-ttll:chain-dist (p pts / i best d)
  (setq best 1e18 i 1)
  (while (< i (length pts))
    (setq d (bkl-ttll:seg-dist p (nth (1- i) pts) (nth i pts)))
    (if (< d best) (setq best d))
    (setq i (1+ i)))
  best)

(defun bkl-ttll:block-name (name / owner tag original tbl)
  (if (and name
           (wcmatch name "`**")
           (setq tbl (tblobjname "BLOCK" name))
           (setq owner (cdr (assoc 330 (entget tbl))))
           (setq tag (assoc -3 (entget owner '("AcDbBlockRepBTag"))))
           (setq original (handent (cdr (assoc 1005 (cdadr tag))))))
    (cdr (assoc 2 (entget original)))
    name))

(defun bkl-ttll:insert-wcs-center (ent data rawBlk / insOcs sx sy rot cs sn
                                                        next rec kind pts r cx cy
                                                        minx maxx miny maxy p)
  (setq insOcs (cdr (assoc 10 data))
        sx (cond ((cdr (assoc 41 data))) (T 1.0))
        sy (cond ((cdr (assoc 42 data))) (T 1.0))
        rot (cond ((cdr (assoc 50 data))) (T 0.0))
        cs (cos rot)
        sn (sin rot)
        pts nil)
  (if (and rawBlk (setq next (cdr (assoc -2 (tblsearch "BLOCK" rawBlk)))))
    (while (and next (/= (cdr (assoc 0 (setq rec (entget next)))) "ENDBLK"))
      (setq kind (cdr (assoc 0 rec)))
      (cond
        ((= kind "WIPEOUT") nil)
        ((= kind "LWPOLYLINE")
         (foreach f rec
           (if (= (car f) 10)
             (setq pts (cons (cdr f) pts)))))
        ((= kind "LINE")
         (if (assoc 10 rec) (setq pts (cons (cdr (assoc 10 rec)) pts)))
         (if (assoc 11 rec) (setq pts (cons (cdr (assoc 11 rec)) pts))))
        ((= kind "CIRCLE")
         (if (assoc 10 rec)
           (progn
             (setq p (cdr (assoc 10 rec))
                   r (cond ((cdr (assoc 40 rec))) (T 0.0))
                   cx (car p) cy (cadr p))
             (setq pts (cons (list (- cx r) (- cy r) 0.0) pts)
                   pts (cons (list (+ cx r) (+ cy r) 0.0) pts))))))
      (setq next (entnext next))))
  (if pts
    (progn
      (setq minx 1e18 maxx -1e18 miny 1e18 maxy -1e18)
      (foreach p pts
        (setq cx (* (car p) sx)
              cy (* (cadr p) sy)
              r (trans (list (+ (car insOcs) (- (* cx cs) (* cy sn)))
                             (+ (cadr insOcs) (+ (* cx sn) (* cy cs)))
                             (caddr insOcs))
                       ent 0))
        (if (< (car r) minx) (setq minx (car r)))
        (if (> (car r) maxx) (setq maxx (car r)))
        (if (< (cadr r) miny) (setq miny (cadr r)))
        (if (> (cadr r) maxy) (setq maxy (cadr r))))
      (list (* 0.5 (+ minx maxx)) (* 0.5 (+ miny maxy))))
    (bkl-ttll:pt2d (trans insOcs ent 0))))

(defun bkl-ttll:read-insert-label (ent data rawBlk / next rec attrs val)
  (setq attrs nil)
  (if (equal (cdr (assoc 66 data)) 1)
    (progn
      (setq next (entnext ent))
      (while (and next (= (cdr (assoc 0 (setq rec (entget next)))) "ATTRIB"))
        (setq attrs (cons (cons (strcase (bkl-ttll:trim (cdr (assoc 2 rec))))
                                (bkl-ttll:plain (cdr (assoc 1 rec))))
                          attrs)
              next (entnext next)))))
  (setq val (cdr (assoc "TVXD_LABEL_033" attrs)))
  (if (or (not val) (= val ""))
    (foreach a (reverse attrs)
      (if (and (or (not val) (= val "")) (/= (cdr a) ""))
        (setq val (cdr a)))))
  (bkl-ttll:trim val))

(defun bkl-ttll:norm-ga-label (lbl / rest)
  (setq lbl (bkl-ttll:trim lbl))
  (if (and (>= (strlen lbl) 2)
           (= (strcase (substr lbl 1 1)) "B")
           (not (wcmatch (setq rest (substr lbl 2)) "*[~0-9]*")))
    (strcat "G" rest)
    lbl))

(defun bkl-ttll:norm-pole-label (raw / u pos s cut)
  (setq u (strcase raw))
  (if (setq pos (vl-string-search "TÊN CỘT:" u))
    (setq s (bkl-ttll:trim (substr raw (+ pos 9))))
    (if (setq pos (vl-string-search "TEN COT:" u))
      (setq s (bkl-ttll:trim (substr raw (+ pos 9))))))
  (if s
    (progn
      (if (setq cut (vl-string-search "- LOẠI CỘT" (strcase s)))
        (setq s (bkl-ttll:trim (substr s 1 cut))))
      (if (setq cut (vl-string-search " - " s))
        (setq s (bkl-ttll:trim (substr s 1 cut))))
      (if (and (> (strlen s) 0) (= (substr s (strlen s) 1) "-"))
        (setq s (bkl-ttll:trim (substr s 1 (1- (strlen s))))))
      (if (and (/= s "") (not (wcmatch (strcase s) "CỘT*")) (not (wcmatch (strcase s) "COT*")))
        (setq s (strcat "Cột " s)))
      (if (/= s "") s nil))))

(defun bkl-ttll:parse-dim-text (txt meas / u posOpen posClose inside numStr netType lenVal)
  (setq txt (bkl-ttll:plain txt)
        u (strcase txt))
  (while (vl-string-search " " u)
    (setq u (vl-string-subst "" " " u)))
  (setq netType
    (cond
      ((wcmatch u "*4D110*") "4D110")
      ((wcmatch u "*3D110*") "3D110")
      ((wcmatch u "*2D110*") "2D110")
      ((or (wcmatch u "*1D110*") (wcmatch u "*D110*")) "1D110")
      ((or (wcmatch u "*2D60*") (wcmatch u "*D60*")) "D60")
      ((wcmatch u "*32/25*") "32/25")
      ((wcmatch u "*110/90*") "110/90")
      ((wcmatch u "*65/50*") "65/50")
      ((or (wcmatch u "*QĐ*") (wcmatch u "*Qđ*") (wcmatch u "*QÐ*") (wcmatch u "*Qð*") (wcmatch u "*QD*")) "QD")
      (T nil)))
  (if netType
    (progn
      (setq lenVal nil)
      (if (and (setq posOpen (vl-string-search "(" u))
               (setq posClose (vl-string-search ")" u posOpen)))
        (progn
          (setq inside (substr u (+ posOpen 2) (- posClose posOpen 1)))
          (if (and (> (strlen inside) 0) (= (substr inside (strlen inside) 1) "M"))
            (setq inside (substr inside 1 (1- (strlen inside)))))
          (setq numStr (vl-string-translate "," "." inside))
          (if (and (/= numStr "")
                   (not (wcmatch numStr "*[~0-9.]*"))
                   (not (wcmatch numStr "*.*.*"))
                   (> (atof numStr) 0))
            (setq lenVal (fix (+ (atof numStr) 0.5))))))
      (if (not lenVal)
        (setq lenVal (max 1 (fix (+ meas 0.5)))))
      (list netType lenVal txt))))

(defun bkl-ttll:read-dim-text (data / blk next rec raw f)
  (setq blk (cdr (assoc 2 data))
        raw "")
  (if (and blk (tblsearch "BLOCK" blk))
    (progn
      (setq next (cdr (assoc -2 (tblsearch "BLOCK" blk))))
      (while (and next (/= (cdr (assoc 0 (setq rec (entget next)))) "ENDBLK"))
        (if (and (member (cdr (assoc 0 rec)) '("MTEXT" "TEXT")) (= raw ""))
          (progn
            (foreach f rec (if (member (car f) '(1 3)) (setq raw (strcat raw (cdr f)))))
            (setq raw (bkl-ttll:plain raw))))
        (setq next (entnext next)))))
  (if (or (= raw "") (= raw "<>"))
    (setq raw (bkl-ttll:plain (cond ((cdr (assoc 1 data))) (T "")))))
  raw)

(defun bkl-ttll:near-any-node-p (pt nodes rad / found n)
  (setq found nil)
  (foreach n nodes
    (if (and (not found) (<= (bkl-ttll:xy pt (nth 2 n)) rad))
      (setq found T)))
  found)

(defun bkl-ttll:collect (selection / i ent data kind rawBlk effBlk bu lbl pos
                                    beNodes gaNodes tuNodes poleNodes legendNodes allNodes
                                    poleTexts leaders rawPolys rawDims
                                    pts prev cur closed f p13 p14 meas parsed
                                    polys dims pt d bestD bestP bestLd ld p dm candScore bestScore
                                    activeDims changed nextActive other hit1 hit2 collectWarns)
  (setq i 0
        beNodes nil gaNodes nil tuNodes nil poleNodes nil legendNodes nil
        poleTexts nil leaders nil rawPolys nil rawDims nil)
  (while (< i (sslength selection))
    (setq ent (ssname selection i)
          i (1+ i)
          data (entget ent)
          kind (cdr (assoc 0 data)))
    (cond
      ((= kind "INSERT")
       (setq rawBlk (cdr (assoc 2 data))
             effBlk (bkl-ttll:block-name rawBlk)
             bu (strcase effBlk))
       (cond
         ((or (wcmatch bu "*BE2*DAN59*") (wcmatch bu "BE2*") (wcmatch bu "*BE_CAP*"))
          (setq lbl (bkl-ttll:read-insert-label ent data rawBlk))
          (if (/= lbl "")
            (progn
              (setq pos (bkl-ttll:insert-wcs-center ent data rawBlk))
              (setq beNodes (cons (list (strcat "BE:" lbl) 'BE pos lbl) beNodes)))
            (setq legendNodes (cons (list "LEG" 'LEG (bkl-ttll:insert-wcs-center ent data rawBlk) "") legendNodes))))
         ((wcmatch bu "*GANIVO*")
          (setq lbl (bkl-ttll:read-insert-label ent data rawBlk))
          (if (/= lbl "")
            (progn
              (setq lbl (bkl-ttll:norm-ga-label lbl)
                    pos (bkl-ttll:insert-wcs-center ent data rawBlk))
              (setq gaNodes (cons (list (strcat "GA:" lbl) 'GA pos lbl) gaNodes)))
            (setq legendNodes (cons (list "LEG" 'LEG (bkl-ttll:insert-wcs-center ent data rawBlk) "") legendNodes))))
         ((or (wcmatch bu "*TUPPTT*") (wcmatch bu "*TU*TT*"))
          (setq lbl (bkl-ttll:read-insert-label ent data rawBlk))
          (if (/= lbl "")
            (progn
              (setq pos (bkl-ttll:insert-wcs-center ent data rawBlk))
              (setq tuNodes (cons (list (strcat "TU:" lbl) 'TU pos lbl) tuNodes)))
            (setq legendNodes (cons (list "LEG" 'LEG (bkl-ttll:insert-wcs-center ent data rawBlk) "") legendNodes))))
         ((or (wcmatch bu "*COT*DIEN*") (wcmatch bu "CHT*") (wcmatch bu "CCT*"))
          (setq pos (bkl-ttll:insert-wcs-center ent data rawBlk)
                lbl (if (wcmatch bu "CCT*") "CCT" "CHT"))
          (setq poleNodes (cons (list (strcat "POLE:" (itoa (length poleNodes))) 'POLE pos lbl nil) poleNodes)))))
      ((member kind '("TEXT" "MTEXT"))
       (setq rawBlk "")
       (foreach f data (if (member (car f) '(1 3)) (setq rawBlk (strcat rawBlk (cdr f)))))
       (setq lbl (bkl-ttll:norm-pole-label (bkl-ttll:plain rawBlk)))
       (if lbl
         (progn
           (setq pos (cdr (assoc 10 data)))
           (if (= kind "TEXT") (setq pos (trans pos ent 0)))
           (setq poleTexts (cons (list (bkl-ttll:pt2d pos) lbl) poleTexts)))))
      ((= kind "LEADER")
       (setq pts (mapcar '(lambda (x) (bkl-ttll:pt2d (cdr x)))
                         (vl-remove-if-not '(lambda (x) (= (car x) 10)) data)))
       (if (>= (length pts) 2)
         (setq leaders (cons (list (last pts) (car pts)) leaders))))
      ((= kind "LWPOLYLINE")
       (setq pts (mapcar '(lambda (x) (bkl-ttll:pt2d (cdr x)))
                         (vl-remove-if-not '(lambda (x) (= (car x) 10)) data))
             closed (= (logand (cond ((cdr (assoc 70 data))) (T 0)) 1) 1))
       (if (>= (length pts) 2)
         (progn
           (setq prev (car pts) cur (cdr pts))
           (foreach p cur
             (if (> (bkl-ttll:xy prev p) 0.05)
               (setq rawPolys (cons (list prev p) rawPolys)))
             (setq prev p))
           (if (and closed (> (bkl-ttll:xy (last pts) (car pts)) 0.05))
             (setq rawPolys (cons (list (last pts) (car pts)) rawPolys))))))
      ((= kind "LINE")
       (setq prev (bkl-ttll:pt2d (cdr (assoc 10 data)))
             cur (bkl-ttll:pt2d (cdr (assoc 11 data))))
       (if (> (bkl-ttll:xy prev cur) 0.05)
         (setq rawPolys (cons (list prev cur) rawPolys))))
      ((= kind "DIMENSION")
       (setq p13 (bkl-ttll:pt2d (cdr (assoc 13 data)))
             p14 (bkl-ttll:pt2d (cdr (assoc 14 data)))
             meas (bkl-ttll:xy p13 p14)
             parsed (bkl-ttll:parse-dim-text (bkl-ttll:read-dim-text data) meas))
       (if parsed
         (setq rawDims (cons (list (car parsed) (cadr parsed) (caddr parsed) p13 p14) rawDims))))))

  ; Gán nhãn TÊN CỘT vào POLE gần nhất (ưu tiên LEADER <= 8m, fallback <= 30m ưu tiên cột có dim)
  (foreach pt poleTexts
    (setq pos (car pt) lbl (cadr pt) bestLd nil bestD 8.0)
    (foreach ld leaders
      (setq d (bkl-ttll:xy pos (car ld)))
      (if (< d bestD) (setq bestD d bestLd (cadr ld))))
    (setq bestD (if bestLd 8.0 30.0)
          pos (if bestLd bestLd pos)
          bestP nil
          bestScore 1e18)
    (foreach p poleNodes
      (setq d (bkl-ttll:xy pos (nth 2 p)))
      (if (< d bestD)
        (progn
          (setq candScore
            (if bestLd d
              (+ d
                 (if (vl-some '(lambda (dm)
                                 (or (<= (bkl-ttll:xy (nth 2 p) (nth 3 dm)) 7.5)
                                     (<= (bkl-ttll:xy (nth 2 p) (nth 4 dm)) 7.5)))
                              rawDims)
                   0.0 100.0)
                 (if (nth 4 p) 200.0 0.0))))
          (if (< candScore bestScore)
            (setq bestScore candScore bestP p)))))
    (if bestP
      (setq poleNodes
        (mapcar '(lambda (x)
                   (if (equal (car x) (car bestP))
                     (list (car x) 'POLE (nth 2 x) lbl T)
                     x))
                poleNodes))))

  ; Lọc cụm KÝ HIỆU đúng bán kính 15m theo đặc tả: chỉ neo vào nút mạng có nhãn (BE, GA, TU, Cột có nhãn TÊN CỘT:)
  (setq allNodes (append beNodes gaNodes tuNodes
                         (vl-remove-if-not '(lambda (x) (nth 4 x)) poleNodes))
        polys nil
        dims nil
        collectWarns nil)
  (setq activeDims (mapcar '(lambda (dm)
                              (or (bkl-ttll:near-any-node-p (nth 3 dm) allNodes 15.0)
                                  (bkl-ttll:near-any-node-p (nth 4 dm) allNodes 15.0)))
                           rawDims))
  (setq changed T)
  (while changed
    (setq changed nil
          i 0
          nextActive nil)
    (foreach dm rawDims
      (if (nth i activeDims)
        (setq nextActive (cons T nextActive))
        (progn
          (setq p13 (nth 3 dm) p14 (nth 4 dm)
                f nil
                pt 0)
          (foreach other rawDims
            (if (and (not f) (nth pt activeDims))
              (if (or (<= (bkl-ttll:xy p13 (nth 3 other)) 2.2)
                      (<= (bkl-ttll:xy p13 (nth 4 other)) 2.2)
                      (<= (bkl-ttll:xy p14 (nth 3 other)) 2.2)
                      (<= (bkl-ttll:xy p14 (nth 4 other)) 2.2))
                (setq f T)))
            (setq pt (1+ pt)))
          (if f (setq changed T))
          (setq nextActive (cons f nextActive))))
      (setq i (1+ i)))
    (setq activeDims (reverse nextActive)))

  ; Dim QĐ nằm dọc theo đoạn dim đang hoạt động (<= 4.5m) cũng thuộc mạng hợp lệ
  (setq i 0 nextActive nil)
  (foreach dm rawDims
    (if (nth i activeDims)
      (setq nextActive (cons T nextActive))
      (if (= (car dm) "QD")
        (progn
          (setq p13 (nth 3 dm) p14 (nth 4 dm) hit1 nil hit2 nil pt 0)
          (foreach other rawDims
            (if (and (nth pt activeDims) (/= (car other) "QD"))
              (progn
                (if (<= (bkl-ttll:seg-dist p13 (nth 3 other) (nth 4 other)) 4.5) (setq hit1 T))
                (if (<= (bkl-ttll:seg-dist p14 (nth 3 other) (nth 4 other)) 4.5) (setq hit2 T))))
            (setq pt (1+ pt)))
          (setq nextActive (cons (and hit1 hit2) nextActive)))
        (setq nextActive (cons nil nextActive))))
    (setq i (1+ i)))
  (setq activeDims (reverse nextActive))

  (setq i 0)
  (foreach dm rawDims
    (if (nth i activeDims)
      (setq dims (cons dm dims))
      (if (not (or (bkl-ttll:near-any-node-p (nth 3 dm) legendNodes 15.0)
                   (bkl-ttll:near-any-node-p (nth 4 dm) legendNodes 15.0)))
        (setq collectWarns
          (cons (strcat "Dim nằm cô lập (> 15m khỏi mọi nút mạng có nhãn): "
                        (car dm) "(" (itoa (cadr dm)) "m) [" (caddr dm) "]")
                collectWarns))))
    (setq i (1+ i)))

  (foreach seg rawPolys
    (if (or (bkl-ttll:near-any-node-p (car seg) allNodes 15.0)
            (bkl-ttll:near-any-node-p (cadr seg) allNodes 15.0))
      (setq polys (cons seg polys))))

  (list beNodes gaNodes tuNodes poleNodes polys dims (reverse collectWarns)))

(defun bkl-ttll:node-num (lbl / i ch digits)
  (setq i 1 digits "")
  (while (<= i (strlen lbl))
    (setq ch (substr lbl i 1) i (1+ i))
    (if (wcmatch ch "[0-9]")
      (setq digits (strcat digits ch))
      (if (/= digits "") (setq i 9999))))
  (if (/= digits "") (atoi digits) 999999))

(defun bkl-ttll:has-poly-near (pt polys rad / ok seg)
  (setq ok nil)
  (foreach seg polys
    (if (and (not ok) (<= (bkl-ttll:seg-dist pt (car seg) (cadr seg)) rad))
      (setq ok T)))
  ok)

(defun bkl-ttll:snap-terminal (pt terms / best d n rad)
  (setq best nil)
  (foreach n terms
    (setq rad (if (= (nth 1 n) 'GA) 2.2 2.6)
          d (bkl-ttll:xy pt (nth 2 n)))
    (if (and (<= d rad) (or (not best) (< d (car best))))
      (setq best (list d n))))
  (if best (cadr best) nil))

(defun bkl-ttll:anti-collapse-pair (p13 p14 terms / cand1 cand2 n d bestPair bestSum s)
  (setq cand1 nil cand2 nil)
  (foreach n terms
    (setq d (bkl-ttll:xy p13 (nth 2 n)))
    (if (<= d 3.0) (setq cand1 (cons (list d n) cand1)))
    (setq d (bkl-ttll:xy p14 (nth 2 n)))
    (if (<= d 3.0) (setq cand2 (cons (list d n) cand2))))
  (setq bestPair nil bestSum 1e18)
  (foreach a cand1
    (foreach b cand2
      (if (/= (car (cadr a)) (car (cadr b)))
        (progn
          (setq s (+ (car a) (car b)))
          (if (< s bestSum)
            (setq bestSum s bestPair (list (cadr a) (cadr b))))))))
  bestPair)

(defun bkl-ttll:unit-vec (a b / dx dy len)
  (setq dx (- (car b) (car a))
        dy (- (cadr b) (cadr a))
        len (sqrt (+ (* dx dx) (* dy dy))))
  (if (> len 1e-9)
    (list (/ dx len) (/ dy len))
    (list 1.0 0.0)))

(defun bkl-ttll:dot (u v)
  (+ (* (car u) (car v)) (* (cadr u) (cadr v))))

(defun bkl-ttll:merge-pts (e1 e2 vid / pts1 pts2)
  (setq pts1 (if (= (cadr e1) vid) (reverse (nth 4 e1)) (nth 4 e1))
        pts2 (if (= (cadr e2) vid) (nth 4 e2) (reverse (nth 4 e2))))
  (append pts1 pts2))

(defun bkl-ttll:build-network (netType dims hardTerms poleNodes allowPole polys trunkSegs
                                / verts edges warnings nextVid nextEid dm p13 p14 t1 t2 pair
                                  v1 v2 get-or-add-vert deg1-changed adj e otherPt
                                  bestT d n bestPole onTrunk poleNear ok1d
                                  changed vid inc e1 e2 u1 u2 newPts newDims
                                  d0 d1 d2 d3 s01 s02 s03 pA pB pC pD spans)
  (setq verts nil ; list of (vid isTerm termObj pt)
        edges nil ; list of (eid v1 v2 dims pts)
        warnings nil
        nextVid 0
        nextEid 0)

  (defun get-or-add-vert (pt / found v bestD d)
    (setq found nil bestD 2.2)
    (foreach v verts
      (if (not (cadr v))
        (progn
          (setq d (bkl-ttll:xy pt (nth 3 v)))
          (if (< d bestD) (setq bestD d found (car v))))))
    (if found found
      (progn
        (setq nextVid (1+ nextVid)
              found (strcat "V:" (itoa nextVid))
              verts (cons (list found nil nil pt) verts))
        found)))

  (defun add-term-vert (termObj / vid)
    (setq vid (car termObj))
    (if (not (assoc vid verts))
      (setq verts (cons (list vid T termObj (nth 2 termObj)) verts)))
    vid)

  (foreach dm dims
    (if (= (car dm) netType)
      (progn
        (setq p13 (nth 3 dm)
              p14 (nth 4 dm)
              t1 (bkl-ttll:snap-terminal p13 hardTerms)
              t2 (bkl-ttll:snap-terminal p14 hardTerms))
        (if (and t1 t2 (= (car t1) (car t2)))
          (if (setq pair (bkl-ttll:anti-collapse-pair p13 p14 hardTerms))
            (setq t1 (car pair) t2 (cadr pair))))
        (setq v1 (if t1 (add-term-vert t1) (get-or-add-vert p13))
              v2 (if t2 (add-term-vert t2) (get-or-add-vert p14)))
        (if (= v1 v2)
          (setq warnings (cons (strcat netType "(" (itoa (cadr dm)) "m) [" (caddr dm) "] bị chập 2 đầu tại 1 nút (" v1 ").") warnings))
          (progn
            (setq nextEid (1+ nextEid))
            (setq edges (cons (list nextEid v1 v2 (list (cadr dm)) (list p13 p14) (list (caddr dm))) edges)))))))

  ; Pha 2: Khớp đầu cụt (deg == 1) cho đỉnh phi-terminal
  (setq deg1-changed T)
  (while deg1-changed
    (setq deg1-changed nil)
    (foreach v verts
      (if (and (not deg1-changed) (not (cadr v)))
        (progn
          (setq vid (car v)
                inc (vl-remove-if-not '(lambda (e) (or (= (cadr e) vid) (= (caddr e) vid))) edges))
          (if (= (length inc) 1)
            (progn
              (setq e (car inc)
                    otherPt (if (= (cadr e) vid) (caddr e) (cadr e))
                    bestT nil bestD 5.5)
              (foreach n hardTerms
                (if (/= (car n) otherPt)
                  (progn
                    (setq d (bkl-ttll:xy (nth 3 v) (nth 2 n)))
                    (if (< d bestD) (setq bestD d bestT n)))))
              (if (and (not bestT) allowPole)
                (progn
                  (setq bestPole nil bestD 1000.0)
                  (foreach n poleNodes
                    (setq d (bkl-ttll:xy (nth 3 v) (nth 2 n)))
                    (if (<= d 7.5)
                      (progn
                        (setq ok1d T)
                        (if (= netType "1D110")
                          (progn
                            (setq poleNear (or (nth 4 n) (bkl-ttll:has-poly-near (nth 2 n) polys 5.0) (bkl-ttll:has-poly-near (nth 3 v) polys 5.0))
                                  onTrunk (bkl-ttll:has-poly-near (nth 2 n) trunkSegs 2.5))
                            (if (or (not poleNear) (and onTrunk (not (nth 4 n))))
                              (setq ok1d nil))))
                        (if ok1d
                          (progn
                            ; Ưu tiên cột có nhãn TÊN CỘT:
                            (setq d (if (nth 4 n) d (+ d 100.0)))
                            (if (< d bestD) (setq bestD d bestPole n)))))))
                  (setq bestT bestPole)))
              (if bestT
                (progn
                  (setq newVid (add-term-vert bestT)
                        edges (mapcar '(lambda (ed)
                                         (list (car ed)
                                               (if (= (cadr ed) vid) newVid (cadr ed))
                                               (if (= (caddr ed) vid) newVid (caddr ed))
                                               (nth 3 ed)
                                               (nth 4 ed)
                                               (nth 5 ed)))
                                      edges)
                        verts (vl-remove-if '(lambda (x) (= (car x) vid)) verts)
                        deg1-changed T)))))))))

  ; Thu gọn đỉnh trung gian deg == 2 và xử lý nút giao deg == 4
  (setq changed T)
  (while changed
    (setq changed nil)
    (foreach v verts
      (if (and (not changed) (not (cadr v)))
        (progn
          (setq vid (car v)
                inc (vl-remove-if-not '(lambda (e) (or (= (cadr e) vid) (= (caddr e) vid))) edges))
          (cond
            ((= (length inc) 2)
             (setq e1 (car inc) e2 (cadr inc)
                   u1 (if (= (cadr e1) vid) (caddr e1) (cadr e1))
                   u2 (if (= (cadr e2) vid) (caddr e2) (cadr e2)))
             (if (/= u1 u2)
               (progn
                 (setq newDims (append (nth 3 e1) (nth 3 e2))
                       newPts (bkl-ttll:merge-pts e1 e2 vid)
                       nextEid (1+ nextEid)
                       edges (vl-remove-if '(lambda (ed) (or (= (car ed) (car e1)) (= (car ed) (car e2)))) edges)
                       edges (cons (list nextEid u1 u2 newDims newPts (append (nth 5 e1) (nth 5 e2))) edges)
                       verts (vl-remove-if '(lambda (x) (= (car x) vid)) verts)
                       changed T))))
            ((= (length inc) 4)
             (setq d0 (bkl-ttll:unit-vec (nth 3 v) (nth 3 (assoc (if (= (cadr (nth 0 inc)) vid) (caddr (nth 0 inc)) (cadr (nth 0 inc))) verts)))
                   d1 (bkl-ttll:unit-vec (nth 3 v) (nth 3 (assoc (if (= (cadr (nth 1 inc)) vid) (caddr (nth 1 inc)) (cadr (nth 1 inc))) verts)))
                   d2 (bkl-ttll:unit-vec (nth 3 v) (nth 3 (assoc (if (= (cadr (nth 2 inc)) vid) (caddr (nth 2 inc)) (cadr (nth 2 inc))) verts)))
                   d3 (bkl-ttll:unit-vec (nth 3 v) (nth 3 (assoc (if (= (cadr (nth 3 inc)) vid) (caddr (nth 3 inc)) (cadr (nth 3 inc))) verts)))
                   s01 (+ (bkl-ttll:dot d0 d1) (bkl-ttll:dot d2 d3))
                   s02 (+ (bkl-ttll:dot d0 d2) (bkl-ttll:dot d1 d3))
                   s03 (+ (bkl-ttll:dot d0 d3) (bkl-ttll:dot d1 d2)))
             (cond
               ((and (<= s01 s02) (<= s01 s03)) (setq pA 0 pB 1 pC 2 pD 3))
               ((<= s02 s03) (setq pA 0 pB 2 pC 1 pD 3))
               (T (setq pA 0 pB 3 pC 1 pD 2)))
             (setq e1 (nth pA inc) e2 (nth pB inc)
                   u1 (if (= (cadr e1) vid) (caddr e1) (cadr e1))
                   u2 (if (= (cadr e2) vid) (caddr e2) (cadr e2)))
             (setq edges (vl-remove-if '(lambda (ed) (member (car ed) (mapcar 'car inc))) edges))
             (if (/= u1 u2)
               (progn
                 (setq nextEid (1+ nextEid))
                 (setq edges (cons (list nextEid u1 u2 (append (nth 3 e1) (nth 3 e2)) (bkl-ttll:merge-pts e1 e2 vid) (append (nth 5 e1) (nth 5 e2))) edges))))
             (setq e1 (nth pC inc) e2 (nth pD inc)
                   u1 (if (= (cadr e1) vid) (caddr e1) (cadr e1))
                   u2 (if (= (cadr e2) vid) (caddr e2) (cadr e2)))
             (if (/= u1 u2)
               (progn
                 (setq nextEid (1+ nextEid))
                 (setq edges (cons (list nextEid u1 u2 (append (nth 3 e1) (nth 3 e2)) (bkl-ttll:merge-pts e1 e2 vid) (append (nth 5 e1) (nth 5 e2))) edges))))
             (setq verts (vl-remove-if '(lambda (x) (= (car x) vid)) verts)
                   changed T)))))))

  ; Lọc nhịp hợp lệ (2 đầu đều là terminal) vs cạnh cô lập/cụt -> WARNINGS
  (setq spans nil)
  (foreach e edges
    (setq v1 (assoc (cadr e) verts)
          v2 (assoc (caddr e) verts))
    (if (and v1 v2 (cadr v1) (cadr v2))
      (setq spans (cons (list netType (caddr v1) (caddr v2) (nth 3 e) (nth 4 e) (nth 5 e)) spans))
      (setq warnings
        (cons (strcat "Dim bất thường / không đóng 2 đầu terminal [" netType "]: "
                      (itoa (apply '+ (nth 3 e)))
                      "m (" (car (nth 5 e)) ")")
              warnings))))
  (list spans warnings))

(defun bkl-ttll:orient-d60 (spans / adj u v q visited head cur nexts oriented sp k1 k2)
  (setq adj nil q nil visited nil oriented nil)
  (foreach sp spans
    (setq k1 (car (nth 1 sp)) k2 (car (nth 2 sp)))
    (if (= (nth 1 (nth 1 sp)) 'TU)
      (if (not (member k1 visited)) (setq visited (cons k1 visited) q (append q (list k1)))))
    (if (= (nth 1 (nth 2 sp)) 'TU)
      (if (not (member k2 visited)) (setq visited (cons k2 visited) q (append q (list k2))))))
  (setq head 0)
  (while (< head (length q))
    (setq cur (nth head q) head (1+ head))
    (foreach sp spans
      (setq k1 (car (nth 1 sp)) k2 (car (nth 2 sp)))
      (cond
        ((and (= k1 cur) (not (member k2 visited)))
         (setq visited (cons k2 visited) q (append q (list k2))
               oriented (cons (list (car sp) (nth 1 sp) (nth 2 sp) (nth 3 sp) (nth 4 sp) (nth 5 sp)) oriented)))
        ((and (= k2 cur) (not (member k1 visited)))
         (setq visited (cons k1 visited) q (append q (list k1))
               oriented (cons (list (car sp) (nth 2 sp) (nth 1 sp) (nth 3 sp) (reverse (nth 4 sp)) (nth 5 sp)) oriented))))))
  ; Các cạnh còn lại chưa được duyệt theo cây BFS (vòng hoặc rời rạc)
  (foreach sp spans
    (setq k1 (car (nth 1 sp)) k2 (car (nth 2 sp)))
    (if (not (vl-some '(lambda (o)
                         (or (and (= (car (nth 1 o)) k1) (= (car (nth 2 o)) k2))
                             (and (= (car (nth 1 o)) k2) (= (car (nth 2 o)) k1))))
                      oriented))
      (if (<= (bkl-ttll:node-num (nth 3 (nth 1 sp))) (bkl-ttll:node-num (nth 3 (nth 2 sp))))
        (setq oriented (cons sp oriented))
        (setq oriented (cons (list (car sp) (nth 2 sp) (nth 1 sp) (nth 3 sp) (reverse (nth 4 sp)) (nth 5 sp)) oriented)))))
  (reverse oriented))

(defun bkl-ttll:orient-and-sort (spans / s2 s4 s3 s1tu s1pole sd60 sOther sp t1 t2 n1 n2)
  (setq s2 nil s4 nil s3 nil s1tu nil s1pole nil sd60 nil sOther nil)
  (foreach sp spans
    (setq t1 (nth 1 sp) t2 (nth 2 sp))
    (cond
      ((= (car sp) "2D110")
       (cond
         ((and (= (nth 1 t1) 'POLE) (= (nth 1 t2) 'BE))
          (if (<= (bkl-ttll:node-num (nth 3 t2)) 1)
            (setq s2 (cons sp s2))
            (setq s2 (cons (list "2D110" t2 t1 (nth 3 sp) (reverse (nth 4 sp)) (nth 5 sp)) s2))))
         ((and (= (nth 1 t1) 'BE) (= (nth 1 t2) 'POLE))
          (if (<= (bkl-ttll:node-num (nth 3 t1)) 1)
            (setq s2 (cons (list "2D110" t2 t1 (nth 3 sp) (reverse (nth 4 sp)) (nth 5 sp)) s2))
            (setq s2 (cons sp s2))))
         (T
          (if (<= (bkl-ttll:node-num (nth 3 t1)) (bkl-ttll:node-num (nth 3 t2)))
            (setq s2 (cons sp s2))
            (setq s2 (cons (list "2D110" t2 t1 (nth 3 sp) (reverse (nth 4 sp)) (nth 5 sp)) s2))))))
      ((= (car sp) "4D110")
       (if (<= (bkl-ttll:node-num (nth 3 t1)) (bkl-ttll:node-num (nth 3 t2)))
         (setq s4 (cons sp s4))
         (setq s4 (cons (list "4D110" t2 t1 (nth 3 sp) (reverse (nth 4 sp)) (nth 5 sp)) s4))))
      ((= (car sp) "3D110")
       (if (<= (bkl-ttll:node-num (nth 3 t1)) (bkl-ttll:node-num (nth 3 t2)))
         (setq s3 (cons sp s3))
         (setq s3 (cons (list "3D110" t2 t1 (nth 3 sp) (reverse (nth 4 sp)) (nth 5 sp)) s3))))
      ((= (car sp) "1D110")
       (cond
         ((or (= (nth 1 t1) 'TU) (= (nth 1 t2) 'TU))
          (if (= (nth 1 t2) 'TU)
            (setq s1tu (cons sp s1tu))
            (setq s1tu (cons (list "1D110" t2 t1 (nth 3 sp) (reverse (nth 4 sp)) (nth 5 sp)) s1tu))))
         (T
          (if (= (nth 1 t2) 'POLE)
            (setq s1pole (cons sp s1pole))
            (setq s1pole (cons (list "1D110" t2 t1 (nth 3 sp) (reverse (nth 4 sp)) (nth 5 sp)) s1pole))))))
      ((= (car sp) "D60")
       (setq sd60 (cons sp sd60)))
      (T (setq sOther (cons sp sOther)))))

  (setq s2 (vl-sort s2 '(lambda (a b)
                          (setq n1 (if (= (nth 1 (nth 1 a)) 'POLE) 0 (bkl-ttll:node-num (nth 3 (nth 1 a))))
                                n2 (if (= (nth 1 (nth 1 b)) 'POLE) 0 (bkl-ttll:node-num (nth 3 (nth 1 b)))))
                          (< n1 n2)))
        s4 (vl-sort s4 '(lambda (a b) (< (bkl-ttll:node-num (nth 3 (nth 1 a))) (bkl-ttll:node-num (nth 3 (nth 1 b))))))
        s3 (vl-sort s3 '(lambda (a b) (< (bkl-ttll:node-num (nth 3 (nth 1 a))) (bkl-ttll:node-num (nth 3 (nth 1 b))))))
        s1tu (vl-sort s1tu '(lambda (a b) (< (bkl-ttll:node-num (nth 3 (nth 2 a))) (bkl-ttll:node-num (nth 3 (nth 2 b))))))
        s1pole (vl-sort s1pole '(lambda (a b) (< (bkl-ttll:node-num (nth 3 (nth 1 a))) (bkl-ttll:node-num (nth 3 (nth 1 b))))))
        sd60 (bkl-ttll:orient-d60 sd60)
        sd60 (vl-sort sd60 '(lambda (a b) (< (bkl-ttll:node-num (nth 3 (nth 2 a))) (bkl-ttll:node-num (nth 3 (nth 2 b)))))))
  (append s2 s4 s3 s1tu sd60 s1pole sOther))

(defun bkl-ttll:process (selection / collected beNodes gaNodes tuNodes poleNodes polys dims
                                   trunkSegs sp res spans warnings net qdDims qdMap
                                   dm p13 p14 bestIdx bestDist bestRank minDist cands c idx d1 d2 dist rank
                                   usedBe usedGa usedTu rows rowIdx r beCnt gaCnt tuCnt
                                   fromNode toNode item qList kcVal qdVal)
  (setq collected (bkl-ttll:collect selection)
        beNodes (nth 0 collected)
        gaNodes (nth 1 collected)
        tuNodes (nth 2 collected)
        poleNodes (nth 3 collected)
        polys (nth 4 collected)
        dims (nth 5 collected))

  (setq spans nil warnings (reverse (nth 6 collected)) trunkSegs nil)
  (foreach net '("4D110" "3D110" "2D110")
    (setq res (bkl-ttll:build-network net dims beNodes poleNodes T polys nil)
          spans (append spans (car res))
          warnings (append warnings (cadr res))))
  (foreach sp spans
    (setq trunkSegs (cons (list (car (nth 4 sp)) (last (nth 4 sp))) trunkSegs)))

  (setq res (bkl-ttll:build-network "1D110" dims (append beNodes tuNodes) poleNodes T polys trunkSegs)
        spans (append spans (car res))
        warnings (append warnings (cadr res)))

  (setq res (bkl-ttll:build-network "D60" dims (append tuNodes gaNodes) poleNodes nil polys nil)
        spans (append spans (car res))
        warnings (append warnings (cadr res)))

  (foreach net '("32/25" "110/90" "65/50")
    (setq res (bkl-ttll:build-network net dims (append beNodes tuNodes gaNodes) poleNodes nil polys nil)
          spans (append spans (car res))
          warnings (append warnings (cadr res))))

  (setq spans (bkl-ttll:orient-and-sort spans))

  ; Ánh xạ QĐ / QĐHT vào nhịp gần nhất (2 pass: tìm minDist thực sự <= 4.5m rồi tie-break <= minDist + 1e-6 theo 4D110 -> 2D110 -> 3D110 -> 1D110)
  (setq qdDims (vl-remove-if-not '(lambda (d) (= (car d) "QD")) dims)
        qdMap nil)
  (foreach dm qdDims
    (setq p13 (nth 3 dm) p14 (nth 4 dm)
          cands nil minDist 1e18 idx 0)
    ; Pass 1: tìm minDist nhỏ nhất thực sự trong các nhịp hợp lệ (<= 4.5m)
    (foreach sp spans
      (if (member (car sp) '("4D110" "3D110" "2D110" "1D110"))
        (progn
          (setq d1 (bkl-ttll:chain-dist p13 (nth 4 sp))
                d2 (bkl-ttll:chain-dist p14 (nth 4 sp)))
          (if (and (<= d1 4.5) (<= d2 4.5))
            (progn
              (setq dist (max d1 d2)
                    rank (cond ((= (car sp) "4D110") 0)
                               ((= (car sp) "2D110") 1)
                               ((= (car sp) "3D110") 2)
                               ((= (car sp) "1D110") 3)
                               (T 4))
                    cands (cons (list dist rank idx) cands))
              (if (< dist minDist) (setq minDist dist))))))
      (setq idx (1+ idx)))
    ; Pass 2: trong các nhịp có dist <= (+ minDist 1e-6), chọn nhịp có rank nhỏ nhất
    (setq bestIdx nil bestRank 99 bestDist 1e18)
    (foreach c cands
      (if (<= (car c) (+ minDist 1e-6))
        (if (or (< (cadr c) bestRank)
                (and (= (cadr c) bestRank) (< (car c) bestDist)))
          (setq bestRank (cadr c) bestDist (car c) bestIdx (caddr c)))))
    (if bestIdx
      (setq qdMap (cons (cons bestIdx (cadr dm)) qdMap))
      (setq warnings (cons (strcat "Dim QĐ không gắn được vào nhịp nào: " (itoa (cadr dm)) "m (" (caddr dm) ")") warnings))))

  ; Đếm duy nhất BE, GA, TU
  (setq usedBe nil usedGa nil usedTu nil rows nil idx 0)
  ; Pass 1: ưu tiên gán BE ở dòng 2D110 (To), GA ở D60 (To), TU ở 1D110 (To)
  (foreach sp spans
    (setq fromNode (nth 1 sp) toNode (nth 2 sp)
          beCnt 0 gaCnt 0 tuCnt 0)
    (if (and (= (car sp) "2D110") (= (nth 1 toNode) 'BE) (not (member (car toNode) usedBe)))
      (setq beCnt 1 usedBe (cons (car toNode) usedBe)))
    (if (and (= (car sp) "D60") (= (nth 1 toNode) 'GA) (not (member (car toNode) usedGa)))
      (setq gaCnt 1 usedGa (cons (car toNode) usedGa)))
    (if (and (= (car sp) "1D110") (= (nth 1 toNode) 'TU) (not (member (car toNode) usedTu)))
      (setq tuCnt 1 usedTu (cons (car toNode) usedTu)))
    (setq rows (cons (list sp beCnt gaCnt tuCnt) rows)))
  (setq rows (reverse rows))

  ; Pass 2: đảm bảo bất kỳ BE/GA/TU nào xuất hiện trong nhịp đều được đếm đủ 1 lần
  (setq rows
    (mapcar
      '(lambda (r)
         (setq sp (car r) beCnt (cadr r) gaCnt (caddr r) tuCnt (cadddr r)
               fromNode (nth 1 sp) toNode (nth 2 sp))
         (foreach nd (list toNode fromNode)
           (cond
             ((and (= (nth 1 nd) 'BE) (not (member (car nd) usedBe)))
              (setq beCnt (1+ beCnt) usedBe (cons (car nd) usedBe)))
             ((and (= (nth 1 nd) 'GA) (not (member (car nd) usedGa)))
              (setq gaCnt (1+ gaCnt) usedGa (cons (car nd) usedGa)))
             ((and (= (nth 1 nd) 'TU) (not (member (car nd) usedTu)))
              (setq tuCnt (1+ tuCnt) usedTu (cons (car nd) usedTu)))))
         (list sp beCnt gaCnt tuCnt))
      rows))

  ; Cảnh báo nút có trong vùng chọn nhưng không nối vào nhịp nào
  (foreach nd beNodes
    (if (not (member (car nd) usedBe))
      (setq warnings (cons (strcat "Bể cáp không nối vào nhịp nào: " (nth 3 nd)) warnings))))
  (foreach nd gaNodes
    (if (not (member (car nd) usedGa))
      (setq warnings (cons (strcat "Ganivo không nối vào nhịp nào: " (nth 3 nd)) warnings))))
  (foreach nd tuNodes
    (if (not (member (car nd) usedTu))
      (setq warnings (cons (strcat "Tủ thông tin không nối vào nhịp nào: " (nth 3 nd)) warnings))))

  ; Kiểm tra từng tủ có đường cấp 1D110 chưa
  (foreach nd tuNodes
    (if (and (member (car nd) usedTu)
             (not (vl-some '(lambda (sp) (and (= (car sp) "1D110")
                                              (or (= (car (nth 1 sp)) (car nd)) (= (car (nth 2 sp)) (car nd)))))
                           spans)))
      (setq warnings (cons (strcat "Tủ " (nth 3 nd) " thiếu dim nguồn 1D110 nối tới bể cáp.") warnings))))

  ; Gắn danh sách QĐ vào từng row và kiểm tra QĐHT > KC
  (setq idx 0)
  (setq rows
    (mapcar
      '(lambda (r / qList item)
         (setq qList nil)
         (foreach item qdMap
           (if (= (car item) idx) (setq qList (append qList (list (cdr item))))))
         (setq idx (1+ idx))
         (append r (list qList)))
      rows))

  (foreach item rows
    (setq sp (car item)
          qList (nth 4 item)
          kcVal (apply '+ (nth 3 sp))
          qdVal (if qList (apply '+ qList) 0))
    (if (> qdVal kcVal)
      (setq warnings
        (cons (strcat "Cảnh báo QĐHT > KC tại nhịp "
                      (nth 3 (nth 1 sp)) " -> " (nth 3 (nth 2 sp))
                      " [" (car sp) "]: QĐHT=" (itoa qdVal) "m > KC=" (itoa kcVal) "m.")
              warnings))))

  (list rows (reverse warnings)))

(defun bkl-ttll:join-ints (lst sep / out x)
  (setq out "")
  (foreach x lst
    (setq out (if (= out "") (itoa x) (strcat out sep (itoa x)))))
  out)

(defun bkl-ttll:write-csv (csvPath rows warnings / stream r sp qList eSum gSum line)
  (if (setq stream (open csvPath "w" "utf8-bom"))
    (progn
      (write-line "STT,Điểm đầu,Điểm cuối,Loại tuyến,Khoảng cách (m),QĐ (m),QĐHT (m),QHH (m),Lên/xuống tủ,Lên/xuống cột,Lên tường,Tổng dài (m),Hệ số,Ống D110 (m),Ống D60 (m),Bể (bể),Ganivo (ga),Tủ (tủ),Ghi chú" stream)
      (setq r 0)
      (foreach item rows
        (setq r (1+ r)
              sp (car item)
              qList (nth 4 item)
              eSum (apply '+ (nth 3 sp))
              gSum (if qList (apply '+ qList) 0)
              line (strcat (itoa r) ","
                           "\"" (nth 3 (nth 1 sp)) "\","
                           "\"" (nth 3 (nth 2 sp)) "\","
                           (car sp) ","
                           (itoa eSum) ",0," (itoa gSum) ",0,"
                           (itoa (+ (if (= (nth 1 (nth 1 sp)) 'TU) 1 0) (if (= (nth 1 (nth 2 sp)) 'TU) 1 0))) ","
                           (itoa (* 2 (+ (if (= (nth 1 (nth 1 sp)) 'POLE) 1 0) (if (= (nth 1 (nth 2 sp)) 'POLE) 1 0)))) ","
                           (if (= (car sp) "32/25") "2" "0") ","
                           (itoa (+ eSum
                                    (if (= (nth 1 (nth 1 sp)) 'TU) 1 0) (if (= (nth 1 (nth 2 sp)) 'TU) 1 0)
                                    (* 2 (+ (if (= (nth 1 (nth 1 sp)) 'POLE) 1 0) (if (= (nth 1 (nth 2 sp)) 'POLE) 1 0)))
                                    (if (= (car sp) "32/25") 2 0))) ","
                           (cond ((= (car sp) "4D110") "4") ((= (car sp) "3D110") "3") ((member (car sp) '("2D110" "D60" "65/50")) "2") (T "1")) ","
                           (if (wcmatch (car sp) "*D110")
                             (itoa (* (+ eSum
                                         (if (= (nth 1 (nth 1 sp)) 'TU) 1 0) (if (= (nth 1 (nth 2 sp)) 'TU) 1 0)
                                         (* 2 (+ (if (= (nth 1 (nth 1 sp)) 'POLE) 1 0) (if (= (nth 1 (nth 2 sp)) 'POLE) 1 0))))
                                      (cond ((= (car sp) "4D110") 4) ((= (car sp) "3D110") 3) ((= (car sp) "2D110") 2) (T 1))))
                             "0") ","
                           (if (= (car sp) "D60")
                             (itoa (* 2 (+ eSum (if (= (nth 1 (nth 1 sp)) 'TU) 1 0) (if (= (nth 1 (nth 2 sp)) 'TU) 1 0))))
                             "0") ","
                           (itoa (cadr item)) ","
                           (itoa (caddr item)) ","
                           (itoa (cadddr item)) ","
                           (if (> (length (nth 3 sp)) 1) (bkl-ttll:join-ints (nth 3 sp) "+") "")))
        (write-line line stream))
      (if warnings
        (progn
          (write-line "" stream)
          (write-line "WARNINGS" stream)
          (foreach w warnings
            (write-line (strcat "\"" w "\"") stream))))
      (close stream)
      T)))

(defun bkl-ttll:format-xlsx (rows warnings xlsxPath / dataFile warnFile psFile f wsh cmd sp exitCode okWrite)
  (setq dataFile (strcat (getvar "TEMPPREFIX") "bkl_ttll_rows.tsv")
        warnFile (strcat (getvar "TEMPPREFIX") "bkl_ttll_warn.txt")
        psFile   (strcat (getvar "TEMPPREFIX") "bkl_ttll_xlsx.ps1")
        okWrite  T)
  (if (setq f (open dataFile "w" "utf8"))
    (progn
      (foreach item rows
        (setq sp (car item))
        (write-line
          (strcat (nth 3 (nth 1 sp)) "\t"
                  (nth 3 (nth 2 sp)) "\t"
                  (car sp) "\t"
                  (bkl-ttll:join-ints (nth 3 sp) "+") "\t"
                  (bkl-ttll:join-ints (nth 4 item) "+") "\t"
                  (itoa (cadr item)) "\t"
                  (itoa (caddr item)) "\t"
                  (itoa (cadddr item)))
          f))
      (close f))
    (setq okWrite nil))
  (if (and okWrite (setq f (open warnFile "w" "utf8")))
    (progn
      (foreach w warnings (write-line w f))
      (close f))
    (setq okWrite nil))
  (if (and okWrite (setq f (open psFile "w" "utf8-bom")))
    (progn
      (write-line "$ErrorActionPreference = 'Stop'" f)
      (write-line "$dataPath = $args[0]; $warnPath = $args[1]; $xlsxPath = [System.IO.Path]::GetFullPath($args[2])" f)
      (write-line "Add-Type -AssemblyName System.IO.Compression" f)
      (write-line "Add-Type -AssemblyName System.IO.Compression.FileSystem" f)
      (write-line "function Esc([string]$s) { if ($null -eq $s) { return '' }; return [System.Security.SecurityElement]::Escape($s) }" f)
      (write-line "function ColName([int]$n) { $s=''; while($n -gt 0){ $m=($n-1)%26; $s=[char](65+$m)+$s; $n=[math]::Floor(($n-1)/26) }; return $s }" f)
      (write-line "function CellStr([string]$ref, [string]$val, [int]$st) { return '<c r=\"' + $ref + '\" t=\"inlineStr\" s=\"' + $st + '\"><is><t>' + (Esc $val) + '</t></is></c>' }" f)
      (write-line "function CellNum([string]$ref, [double]$val, [int]$st) { return '<c r=\"' + $ref + '\" s=\"' + $st + '\"><v>' + $val + '</v></c>' }" f)
      (write-line "function CellFml([string]$ref, [string]$fml, [double]$val, [int]$st) { return '<c r=\"' + $ref + '\" s=\"' + $st + '\"><f>' + (Esc $fml) + '</f><v>' + $val + '</v></c>' }" f)
      (write-line "$rawRows = @(); if (Test-Path $dataPath) { $rawRows = Get-Content -Path $dataPath -Encoding UTF8 | Where-Object { $_ -ne '' } }" f)
      (write-line "$warns = @(); if (Test-Path $warnPath) { $warns = Get-Content -Path $warnPath -Encoding UTF8 | Where-Object { $_ -ne '' } }" f)
      (write-line "$N = $rawRows.Count; $totRow = 4 + $N" f)
      (write-line "$coefMap = @{ '1D110'=1; '2D110'=2; '3D110'=3; '4D110'=4; 'D60'=2; '32/25'=1; '110/90'=1; '65/50'=2 }" f)
      (write-line "$sums = @{}; foreach ($c in @('E','F','G','H','I','J','K','L','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','AA','AB','AC','AD','AE','AF','AG','AH','AI','AJ','AK','AL','AM','AN','AO')) { $sums[$c] = 0.0 }" f)
      (write-line "$dgRowsXml = New-Object System.Text.StringBuilder" f)
      (write-line "$r = 3" f)
      (write-line "foreach ($line in $rawRows) {" f)
      (write-line "  $r++; $p = $line -split \"`t\"" f)
      (write-line "  $b = $p[0]; $c = $p[1]; $d = $p[2]; $eList = $p[3]; $gList = $p[4]" f)
      (write-line "  $be = [int]$p[5]; $ga = [int]$p[6]; $tu = [int]$p[7]" f)
      (write-line "  $eParts = $eList -split '\\+'; $eVal = 0; foreach ($x in $eParts) { if ($x -ne '') { $eVal += [int]$x } }" f)
      (write-line "  $gParts = @(); if ($gList -ne '') { $gParts = $gList -split '\\+' }; $gVal = 0; foreach ($x in $gParts) { if ($x -ne '') { $gVal += [int]$x } }" f)
      (write-line "  $fVal = 0; $hVal = 0" f)
      (write-line "  $iVal = ($(if($b.StartsWith('TT')){1}else{0}) + $(if($c.StartsWith('TT')){1}else{0}))" f)
      (write-line "  $jVal = 2 * ($(if($b.StartsWith('Cột')){1}else{0}) + $(if($c.StartsWith('Cột')){1}else{0}))" f)
      (write-line "  $mVal = $coefMap[$d]; $kVal = $(if($d -eq '32/25'){ $mVal * 2 } else { 0 })" f)
      (write-line "  $lVal = $eVal + $iVal + $jVal + $kVal" f)
      (write-line "  $nVal = $(if($d.EndsWith('D110')){ $lVal * $mVal } else { 0 })" f)
      (write-line "  $oVal = $(if($d -eq 'D60'){ $lVal * $mVal } else { 0 })" f)
      (write-line "  $pVal = $(if($d -eq '32/25'){ $lVal * $mVal } else { 0 })" f)
      (write-line "  $qVal = $(if($d -eq '110/90'){ $lVal * $mVal } else { 0 })" f)
      (write-line "  $rVal = $(if($d -eq '65/50'){ $lVal * $mVal } else { 0 })" f)
      (write-line "  $sVal = $(if($d -eq '1D110'){ [math]::Max(0, $eVal-$fVal-$gVal-$hVal) } else { 0 }); $tVal = $(if($d -eq '1D110'){$fVal}else{0}); $uVal = $(if($d -eq '1D110'){$gVal}else{0}); $vVal = $(if($d -eq '1D110'){$hVal}else{0})" f)
      (write-line "  $wVal = $(if($d -eq '2D110'){ [math]::Max(0, $eVal-$fVal-$gVal) } else { 0 }); $xVal = $(if($d -eq '2D110'){$fVal}else{0}); $yVal = $(if($d -eq '2D110'){$gVal}else{0})" f)
      (write-line "  $zVal = $(if($d -eq '3D110'){ [math]::Max(0, $eVal-$fVal-$gVal) } else { 0 }); $aaVal = $(if($d -eq '3D110'){$fVal}else{0}); $abVal = $(if($d -eq '3D110'){$gVal}else{0})" f)
      (write-line "  $acVal = $(if($d -eq '4D110'){ [math]::Max(0, $eVal-$fVal-$gVal) } else { 0 }); $adVal = $(if($d -eq '4D110'){$fVal}else{0}); $aeVal = $(if($d -eq '4D110'){$gVal}else{0})" f)
      (write-line "  $afVal = $(if($d -eq 'D60'){ [math]::Max(0, $eVal-$fVal-$gVal) } else { 0 }); $agVal = $(if($d -eq 'D60'){$fVal}else{0}); $ahVal = $(if($d -eq 'D60'){$gVal}else{0})" f)
      (write-line "  $aiVal = $(if($d -eq '110/90'){ [math]::Max(0, $eVal-$fVal-$gVal) } else { 0 }); $ajVal = $(if($d -eq '65/50'){ [math]::Max(0, $eVal-$fVal-$gVal) } else { 0 }); $akVal = $(if($d -eq '32/25'){ [math]::Max(0, $eVal-$fVal-$gVal) } else { 0 })" f)
      (write-line "  $sums['E']+=$eVal; $sums['F']+=$fVal; $sums['G']+=$gVal; $sums['H']+=$hVal; $sums['I']+=$iVal; $sums['J']+=$jVal; $sums['K']+=$kVal; $sums['L']+=$lVal" f)
      (write-line "  $sums['N']+=$nVal; $sums['O']+=$oVal; $sums['P']+=$pVal; $sums['Q']+=$qVal; $sums['R']+=$rVal" f)
      (write-line "  $sums['S']+=$sVal; $sums['T']+=$tVal; $sums['U']+=$uVal; $sums['V']+=$vVal; $sums['W']+=$wVal; $sums['X']+=$xVal; $sums['Y']+=$yVal" f)
      (write-line "  $sums['Z']+=$zVal; $sums['AA']+=$aaVal; $sums['AB']+=$abVal; $sums['AC']+=$acVal; $sums['AD']+=$adVal; $sums['AE']+=$aeVal" f)
      (write-line "  $sums['AF']+=$afVal; $sums['AG']+=$agVal; $sums['AH']+=$ahVal; $sums['AI']+=$aiVal; $sums['AJ']+=$ajVal; $sums['AK']+=$akVal" f)
      (write-line "  $sums['AL']+=$be; $sums['AN']+=$ga; $sums['AO']+=$tu" f)
      (write-line "  $note = $(if($eParts.Count -gt 1){ $eList } else { '' })" f)
      (write-line "  [void]$dgRowsXml.Append(\"<row r=`\"$r`\" ht=`\"18`\" customHeight=`\"1`\">\")" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"A$r\" 'ROW()-ROW($A$3)' ($r - 3) 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellStr \"B$r\" $b 3))" f)
      (write-line "  [void]$dgRowsXml.Append((CellStr \"C$r\" $c 3))" f)
      (write-line "  [void]$dgRowsXml.Append((CellStr \"D$r\" $d 2))" f)
      (write-line "  if ($eParts.Count -gt 1) { [void]$dgRowsXml.Append((CellFml \"E$r\" $eList $eVal 2)) } else { [void]$dgRowsXml.Append((CellNum \"E$r\" $eVal 2)) }" f)
      (write-line "  [void]$dgRowsXml.Append((CellNum \"F$r\" $fVal 2))" f)
      (write-line "  if ($gParts.Count -gt 1) { [void]$dgRowsXml.Append((CellFml \"G$r\" $gList $gVal 2)) } else { [void]$dgRowsXml.Append((CellNum \"G$r\" $gVal 2)) }" f)
      (write-line "  [void]$dgRowsXml.Append((CellNum \"H$r\" $hVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"I$r\" \"IF(LEFT(`$B$r,2)=`\"TT`\",1,0)+IF(LEFT(`$C$r,2)=`\"TT`\",1,0)\" $iVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"J$r\" \"2*(IF(LEFT(`$B$r,3)=`\"Cột`\",1,0)+IF(LEFT(`$C$r,3)=`\"Cột`\",1,0))\" $jVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"K$r\" \"IF(`$D$r=`\"32/25`\",`$M$r*2,0)\" $kVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"L$r\" \"`$E$r+`$I$r+`$J$r+`$K$r\" $lVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"M$r\" \"INDEX('DM LOAI ONG'!`$D`$5:`$D`$12,MATCH(`$D$r,'DM LOAI ONG'!`$A`$5:`$A`$12,0))\" $mVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"N$r\" \"IF(RIGHT(`$D$r,4)=`\"D110`\",`$L$r*`$M$r,0)\" $nVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"O$r\" \"IF(`$D$r=`\"D60`\",`$L$r*`$M$r,0)\" $oVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"P$r\" \"IF(`$D$r=`\"32/25`\",`$L$r*`$M$r,0)\" $pVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"Q$r\" \"IF(`$D$r=`\"110/90`\",`$L$r*`$M$r,0)\" $qVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"R$r\" \"IF(`$D$r=`\"65/50`\",`$L$r*`$M$r,0)\" $rVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"S$r\" \"IF(`$D$r=`\"1D110`\",MAX(0,`$E$r-`$F$r-`$G$r-`$H$r),0)\" $sVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"T$r\" \"IF(`$D$r=`\"1D110`\",`$F$r,0)\" $tVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"U$r\" \"IF(`$D$r=`\"1D110`\",`$G$r,0)\" $uVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"V$r\" \"IF(`$D$r=`\"1D110`\",`$H$r,0)\" $vVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"W$r\" \"IF(`$D$r=`\"2D110`\",MAX(0,`$E$r-`$F$r-`$G$r),0)\" $wVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"X$r\" \"IF(`$D$r=`\"2D110`\",`$F$r,0)\" $xVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"Y$r\" \"IF(`$D$r=`\"2D110`\",`$G$r,0)\" $yVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"Z$r\" \"IF(`$D$r=`\"3D110`\",MAX(0,`$E$r-`$F$r-`$G$r),0)\" $zVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"AA$r\" \"IF(`$D$r=`\"3D110`\",`$F$r,0)\" $aaVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"AB$r\" \"IF(`$D$r=`\"3D110`\",`$G$r,0)\" $abVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"AC$r\" \"IF(`$D$r=`\"4D110`\",MAX(0,`$E$r-`$F$r-`$G$r),0)\" $acVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"AD$r\" \"IF(`$D$r=`\"4D110`\",`$F$r,0)\" $adVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"AE$r\" \"IF(`$D$r=`\"4D110`\",`$G$r,0)\" $aeVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"AF$r\" \"IF(`$D$r=`\"D60`\",MAX(0,`$E$r-`$F$r-`$G$r),0)\" $afVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"AG$r\" \"IF(`$D$r=`\"D60`\",`$F$r,0)\" $agVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"AH$r\" \"IF(`$D$r=`\"D60`\",`$G$r,0)\" $ahVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"AI$r\" \"IF(`$D$r=`\"110/90`\",MAX(0,`$E$r-`$F$r-`$G$r),0)\" $aiVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"AJ$r\" \"IF(`$D$r=`\"65/50`\",MAX(0,`$E$r-`$F$r-`$G$r),0)\" $ajVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellFml \"AK$r\" \"IF(`$D$r=`\"32/25`\",MAX(0,`$E$r-`$F$r-`$G$r),0)\" $akVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellNum \"AL$r\" $be 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellNum \"AM$r\" 0 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellNum \"AN$r\" $ga 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellNum \"AO$r\" $tu 2))" f)
      (write-line "  $apVal = if (($fVal + $gVal + $hVal) -gt $eVal) { 'CHECK: QĐHT>KC' } else { 'PASS' }" f)
      (write-line "  [void]$dgRowsXml.Append((CellStr \"AP$r\" $apVal 2))" f)
      (write-line "  [void]$dgRowsXml.Append((CellStr \"AQ$r\" $note 3))" f)
      (write-line "  [void]$dgRowsXml.Append('</row>')" f)
      (write-line "}" f)
      (write-line "[void]$dgRowsXml.Append(\"<row r=`\"$totRow`\" ht=`\"22`\" customHeight=`\"1`\">\")" f)
      (write-line "[void]$dgRowsXml.Append((CellStr \"A$totRow\" 'Tổng cộng' 4))" f)
      (write-line "[void]$dgRowsXml.Append((CellStr \"B$totRow\" '' 4))" f)
      (write-line "[void]$dgRowsXml.Append((CellStr \"C$totRow\" '' 4))" f)
      (write-line "[void]$dgRowsXml.Append((CellStr \"D$totRow\" '' 4))" f)
      (write-line "foreach ($col in @('E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z','AA','AB','AC','AD','AE','AF','AG','AH','AI','AJ','AK','AL','AM','AN','AO')) {" f)
      (write-line "  if ($col -eq 'M') { [void]$dgRowsXml.Append((CellStr \"M$totRow\" '' 4)) } else {" f)
      (write-line "    $endR = [math]::Max(4, $totRow - 1)" f)
      (write-line "    $fml = if ($N -gt 0) { \"SUM(${col}`$4:${col}$endR)\" } else { '0' }" f)
      (write-line "    [void]$dgRowsXml.Append((CellFml \"$col$totRow\" $fml $sums[$col] 4))" f)
      (write-line "  }" f)
      (write-line "}" f)
      (write-line "[void]$dgRowsXml.Append((CellStr \"AP$totRow\" '' 4))" f)
      (write-line "[void]$dgRowsXml.Append((CellStr \"AQ$totRow\" '' 4))" f)
      (write-line "[void]$dgRowsXml.Append('</row>')" f)

      ; Sheet 1: THKL
      (write-line "$d12 = $sums['W']; $d18 = [math]::Ceiling($d12 / 15.0)" f)
      (write-line "$thklItems = @(" f)
      (write-line "  @('GANIVO CHỜ LUỒN CÁP THÔNG TIN','GA',\"'DG TTLL'!AN$totRow\",$sums['AN'])," f)
      (write-line "  @('BỂ CÁP 2 TẤM ĐAN DƯỚI HÈ','BỂ',\"'DG TTLL'!AL$totRow\",$sums['AL'])," f)
      (write-line "  @('ỐNG NHỰA UPVC D60 DÀY 3,6MM','M',\"'DG TTLL'!O$totRow\",$sums['O'])," f)
      (write-line "  @('ỐNG NHỰA UPVC D110 DÀY 5MM','M',\"'DG TTLL'!N$totRow\",$sums['N'])," f)
      (write-line "  @('ỐNG NHỰA XOẮN HDPE D32/25MM ĐẾN TỪNG LÔ ĐẤT','M','D3*2*3',($sums['AN']*6))," f)
      (write-line "  @('CỐNG CÁP HDPE 32/25 ĐẾN TỪNG LÔ ĐẤT','M','D3*2',($sums['AN']*2))," f)
      (write-line "  @('CỐNG CÁP THÔNG TIN 1 ỐNG D110 DƯỚI HÈ','M',\"'DG TTLL'!S$totRow\",$sums['S'])," f)
      (write-line "  @('CỐNG CÁP THÔNG TIN 1 ỐNG D110 QUA ĐƯỜNG HIỆN TRẠNG','M',\"'DG TTLL'!U$totRow\",$sums['U'])," f)
      (write-line "  @('CỐNG CÁP THÔNG TIN 1 ỐNG D110 QUA HÈ HIỆN TRẠNG','M',\"'DG TTLL'!V$totRow\",$sums['V'])," f)
      (write-line "  @('CỐNG CÁP THÔNG TIN 2 ỐNG D110 DƯỚI HÈ','M',\"'DG TTLL'!W$totRow\",$sums['W'])," f)
      (write-line "  @('CỐNG CÁP THÔNG TIN 2 ỐNG D110 QUA ĐƯỜNG HIỆN TRẠNG','M',\"'DG TTLL'!Y$totRow\",$sums['Y'])," f)
      (write-line "  @('CỐNG CÁP THÔNG TIN 4 ỐNG D110 DƯỚI HÈ','M',\"'DG TTLL'!AC$totRow\",$sums['AC'])," f)
      (write-line "  @('CỐNG CÁP THÔNG TIN 4 ỐNG D110 QUA ĐƯỜNG HIỆN TRẠNG','M',\"'DG TTLL'!AE$totRow\",$sums['AE'])," f)
      (write-line "  @('TỦ PHÂN PHỐI THÔNG TIN','TỦ',\"'DG TTLL'!AO$totRow\",$sums['AO'])," f)
      (write-line "  @('BỆ TỦ PHÂN PHỐI THÔNG TIN','BỆ','D16',$sums['AO'])," f)
      (write-line "  @('MỐC BÁO HIỆU CÁP THÔNG TIN','MỐC','ROUNDUP(D12/15,0)',$d18)" f)
      (write-line ")" f)
      (write-line "$s1 = New-Object System.Text.StringBuilder" f)
      (write-line "[void]$s1.Append('<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">')" f)
      (write-line "[void]$s1.Append('<cols><col min=\"1\" max=\"1\" width=\"7\" customWidth=\"1\"/><col min=\"2\" max=\"2\" width=\"58\" customWidth=\"1\"/><col min=\"3\" max=\"3\" width=\"10\" customWidth=\"1\"/><col min=\"4\" max=\"4\" width=\"16\" customWidth=\"1\"/><col min=\"5\" max=\"5\" width=\"24\" customWidth=\"1\"/></cols><sheetData>')" f)
      (write-line "[void]$s1.Append('<row r=\"1\" ht=\"28\" customHeight=\"1\">' + (CellStr 'A1' 'BẢNG KHỐI LƯỢNG CHÍNH HỆ THỐNG THÔNG TIN LIÊN LẠC' 1) + '</row>')" f)
      (write-line "[void]$s1.Append('<row r=\"2\" ht=\"24\" customHeight=\"1\">' + (CellStr 'A2' 'STT' 4) + (CellStr 'B2' 'TÊN KHỐI LƯỢNG CÔNG VIỆC' 4) + (CellStr 'C2' 'ĐƠN VỊ' 4) + (CellStr 'D2' 'KHỐI LƯỢNG' 4) + (CellStr 'E2' 'GHI CHÚ' 4) + '</row>')" f)
      (write-line "$ri = 2; foreach ($it in $thklItems) { $ri++; $stt = $ri - 2; [void]$s1.Append(\"<row r=`\"$ri`\" ht=`\"20`\" customHeight=`\"1`\">\" + (CellFml \"A$ri\" 'ROW()-ROW($A$2)' $stt 2) + (CellStr \"B$ri\" $it[0] 3) + (CellStr \"C$ri\" $it[1] 2) + (CellFml \"D$ri\" $it[2] $it[3] 2) + (CellStr \"E$ri\" '' 3) + '</row>') }" f)
      (write-line "$warnMerges = ''" f)
      (write-line "if ($warns.Count -gt 0) {" f)
      (write-line "  [void]$s1.Append('<row r=\"21\" ht=\"22\" customHeight=\"1\">' + (CellStr 'A21' 'CẢNH BÁO ĐỐI SOÁT CAD' 4) + (CellStr 'B21' '' 4) + (CellStr 'C21' '' 4) + (CellStr 'D21' '' 4) + (CellStr 'E21' '' 4) + '</row>')" f)
      (write-line "  $warnMerges = '<mergeCell ref=\"A21:E21\"/>'; $wr = 21" f)
      (write-line "  foreach ($w in $warns) { $wr++; [void]$s1.Append(\"<row r=`\"$wr`\" ht=`\"20`\" customHeight=`\"1`\">\" + (CellStr \"A$wr\" ([string]($wr - 21)) 2) + (CellStr \"B$wr\" $w 3) + (CellStr \"C$wr\" '' 3) + (CellStr \"D$wr\" '' 3) + (CellStr \"E$wr\" '' 3) + '</row>'); $warnMerges += \"<mergeCell ref=`\"B${wr}:E${wr}`\"/>\" }" f)
      (write-line "}" f)
      (write-line "[void]$s1.Append(\"</sheetData><mergeCells><mergeCell ref=`\"A1:E1`\"/>$warnMerges</mergeCells></worksheet>\")" f)

      ; Sheet 2: DG TTLL
      (write-line "$hdr3 = @('','Điểm đầu','Điểm cuối','','Khoảng cách theo nhãn (m)','Qua đường QĐ theo nhãn (m)','Qua đường hiện trạng QĐHT theo nhãn (m)','Qua hè hiện trạng theo nhãn (m)','Lên/xuống tủ thông tin (m)','Lên/xuống cột điện (m)','Lên tường nhà hộ dân (m)','Tổng chiều dài tính ống (m)','Hệ số ống','Ống UPVC D110x5mm (m)','Ống UPVC D60x3,6mm (m)','Ống HDPE xoắn 32/25 (m)','Ống HDPE xoắn 110/90 (m)','Ống HDPE xoắn 65/50 (m)','Cống cáp 1D110 dưới hè (m)','Cống cáp 1D110 dưới đường (m)','Cống cáp 1D110 qua đường hiện trạng QĐHT (m)','Cống cáp 1D110 qua hè hiện trạng (m)','Cống cáp 2D110 dưới hè (m)','Cống cáp 2D110 dưới đường (m)','Cống cáp 2D110 qua đường hiện trạng QĐHT (m)','Cống cáp 3D110 dưới hè (m)','Cống cáp 3D110 dưới đường (m)','Cống cáp 3D110 qua đường hiện trạng QĐHT (m)','Cống cáp 4D110 dưới hè (m)','Cống cáp 4D110 dưới đường (m)','Cống cáp 4D110 qua đường hiện trạng QĐHT (m)','Cống cáp 2D60 dưới hè (m)','Cống cáp 2D60 dưới đường (m)','Cống cáp 2D60 qua đường hiện trạng QĐHT (m)','Cống cáp 1 ống HDPE 110/90 dưới hè (m)','Cống cáp 2 ống HDPE 65/50 dưới hè (m)','Cống cáp HDPE 32/25 (m)','Bể cáp 2 tấm đan dưới hè (bể)','Bể cáp 2 tấm đan dưới đường (bể)','Ganivo (ga)','Tủ thông tin (tủ)','','')" f)
      (write-line "$s2 = New-Object System.Text.StringBuilder" f)
      (write-line "[void]$s2.Append('<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><cols>')" f)
      (write-line "for ($ci=1; $ci -le 43; $ci++) { $w = if($ci -eq 1){6} elseif($ci -le 3){16} elseif($ci -eq 4){10} elseif($ci -eq 43){22} else {13}; [void]$s2.Append(\"<col min=`\"$ci`\" max=`\"$ci`\" width=`\"$w`\" customWidth=`\"1`\"/>\") }" f)
      (write-line "[void]$s2.Append('</cols><sheetData>')" f)
      (write-line "[void]$s2.Append('<row r=\"1\" ht=\"28\" customHeight=\"1\">' + (CellStr 'A1' 'BẢNG DIỄN GIẢI KHỐI LƯỢNG THÔNG TIN LIÊN LẠC - MẶT BẰNG HỆ THỐNG ỐNG CHỜ ĐƯỜNG DÂY THÔNG TIN' 1) + '</row>')" f)
      (write-line "$r2 = '<row r=\"2\" ht=\"22\" customHeight=\"1\">'; $hdr2 = @{1='STT';2='Vị trí';4='Loại tuyến';5='INPUT';9='CALC';14='RESULT';19='RESULT';38='SELECTION';42='CHECK';43='Ghi chú'}; for($ci=1;$ci -le 43;$ci++){ $cn=ColName $ci; $txt=if($hdr2.ContainsKey($ci)){$hdr2[$ci]}else{''}; $r2 += (CellStr \"${cn}2\" $txt 4) }; $r2 += '</row>'" f)
      (write-line "$r3 = '<row r=\"3\" ht=\"42\" customHeight=\"1\">'; for($ci=1;$ci -le 43;$ci++){ $cn=ColName $ci; $r3 += (CellStr \"${cn}3\" $hdr3[$ci-1] 4) }; $r3 += '</row>'" f)
      (write-line "[void]$s2.Append($r2); [void]$s2.Append($r3); [void]$s2.Append($dgRowsXml.ToString())" f)
      (write-line "[void]$s2.Append('</sheetData><mergeCells><mergeCell ref=\"A1:AK1\"/><mergeCell ref=\"A2:A3\"/><mergeCell ref=\"B2:C2\"/><mergeCell ref=\"D2:D3\"/><mergeCell ref=\"E2:H2\"/><mergeCell ref=\"I2:M2\"/><mergeCell ref=\"N2:R2\"/><mergeCell ref=\"S2:AK2\"/><mergeCell ref=\"AL2:AO2\"/><mergeCell ref=\"AP2:AP3\"/><mergeCell ref=\"AQ2:AQ3\"/></mergeCells></worksheet>')" f)

      ; Sheet 3: DM LOAI ONG
      (write-line "$dmRows = @(@('1D110','Ống UPVC D110x5mm, 1 ống/tuyến','Đã có cột tính ống và cống cáp',1),@('2D110','Ống UPVC D110x5mm, 2 ống/tuyến','Đã có cột tính ống và cống cáp',2),@('3D110','Ống UPVC D110x5mm, 3 ống/tuyến','Đã có cột tính ống và cống cáp',3),@('4D110','Ống UPVC D110x5mm, 4 ống/tuyến','Đã có cột tính ống và cống cáp',4),@('D60','Ống UPVC D60x3,6mm','Đã có cột tính ống và cống cáp 2D60',2),@('32/25','Ống HDPE xoắn D32/25','Đã có cột tính',1),@('110/90','Ống HDPE xoắn D110/90','MISSING DATA - chưa có cột tính khối lượng',1),@('65/50','Ống HDPE xoắn D65/50','MISSING DATA - chưa có cột tính khối lượng',2))" f)
      (write-line "$s3 = New-Object System.Text.StringBuilder" f)
      (write-line "[void]$s3.Append('<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><cols><col min=\"1\" max=\"1\" width=\"18\" customWidth=\"1\"/><col min=\"2\" max=\"2\" width=\"34\" customWidth=\"1\"/><col min=\"3\" max=\"3\" width=\"48\" customWidth=\"1\"/><col min=\"4\" max=\"4\" width=\"20\" customWidth=\"1\"/></cols><sheetData>')" f)
      (write-line "[void]$s3.Append('<row r=\"1\" ht=\"24\" customHeight=\"1\">' + (CellStr 'A1' 'DANH MỤC LOẠI TUYẾN ỐNG (nguồn cho danh sách chọn cột D - sheet DG TTLL)' 1) + '</row>')" f)
      (write-line "[void]$s3.Append('<row r=\"4\" ht=\"24\" customHeight=\"1\">' + (CellStr 'A4' 'Loại tuyến (mã chọn)' 4) + (CellStr 'B4' 'Mô tả' 4) + (CellStr 'C4' 'Tình trạng tính khối lượng trong DG TTLL' 4) + (CellStr 'D4' 'Hệ số ống (ống/tuyến)' 4) + '</row>')" f)
      (write-line "$ri = 4; foreach ($it in $dmRows) { $ri++; [void]$s3.Append(\"<row r=`\"$ri`\" ht=`\"20`\" customHeight=`\"1`\">\" + (CellStr \"A$ri\" $it[0] 2) + (CellStr \"B$ri\" $it[1] 3) + (CellStr \"C$ri\" $it[2] 3) + (CellNum \"D$ri\" $it[3] 2) + '</row>') }" f)
      (write-line "[void]$s3.Append('</sheetData><mergeCells><mergeCell ref=\"A1:D1\"/></mergeCells></worksheet>')" f)

      ; Package parts
      (write-line "$ct = '<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/><Override PartName=\"/xl/worksheets/sheet2.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/><Override PartName=\"/xl/worksheets/sheet3.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>'" f)
      (write-line "$rels = '<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>'" f)
      (write-line "$wb = '<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"THKL\" sheetId=\"1\" r:id=\"rId1\"/><sheet name=\"DG TTLL\" sheetId=\"2\" r:id=\"rId2\"/><sheet name=\"DM LOAI ONG\" sheetId=\"3\" r:id=\"rId3\"/></sheets></workbook>'" f)
      (write-line "$wbRels = '<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/><Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet2.xml\"/><Relationship Id=\"rId3\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet3.xml\"/><Relationship Id=\"rId4\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/></Relationships>'" f)
      (write-line "$styles = '<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><fonts count=\"2\"><font><sz val=\"11\"/><name val=\"Times New Roman\"/></font><font><b/><sz val=\"12\"/><name val=\"Times New Roman\"/></font></fonts><fills count=\"2\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill></fills><borders count=\"2\"><border><left/><right/><top/><bottom/><diagonal/></border><border><left style=\"thin\"><color auto=\"1\"/></left><right style=\"thin\"><color auto=\"1\"/></right><top style=\"thin\"><color auto=\"1\"/></top><bottom style=\"thin\"><color auto=\"1\"/></bottom><diagonal/></border></borders><cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs><cellXfs count=\"5\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/><xf numFmtId=\"0\" fontId=\"1\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\"/></xf><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"left\" vertical=\"center\"/></xf><xf numFmtId=\"0\" fontId=\"1\" fillId=\"0\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment horizontal=\"center\" vertical=\"center\" wrapText=\"1\"/></xf></cellXfs></styleSheet>'" f)

      (write-line "$dir = [System.IO.Path]::GetDirectoryName($xlsxPath); if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }" f)
      (write-line "$baseDir = if ($dir) { $dir } else { '.' }" f)
      (write-line "$tmpXlsx = [System.IO.Path]::Combine($baseDir, ([System.IO.Path]::GetFileName($xlsxPath) + '.' + [System.Guid]::NewGuid().ToString('N') + '.tmp'))" f)
      (write-line "$fs = $null; $zip = $null" f)
      (write-line "try {" f)
      (write-line "  $fs = New-Object System.IO.FileStream($tmpXlsx, [System.IO.FileMode]::Create)" f)
      (write-line "  $zip = New-Object System.IO.Compression.ZipArchive($fs, [System.IO.Compression.ZipArchiveMode]::Create)" f)
      (write-line "  $utf8 = New-Object System.Text.UTF8Encoding($false)" f)
      (write-line "  function AddEntry([string]$name, [string]$content) { $e = $zip.CreateEntry($name); $sw = New-Object System.IO.StreamWriter($e.Open(), $utf8); $sw.Write($content); $sw.Close() }" f)
      (write-line "  AddEntry '[Content_Types].xml' $ct" f)
      (write-line "  AddEntry '_rels/.rels' $rels" f)
      (write-line "  AddEntry 'xl/workbook.xml' $wb" f)
      (write-line "  AddEntry 'xl/_rels/workbook.xml.rels' $wbRels" f)
      (write-line "  AddEntry 'xl/styles.xml' $styles" f)
      (write-line "  AddEntry 'xl/worksheets/sheet1.xml' $s1.ToString()" f)
      (write-line "  AddEntry 'xl/worksheets/sheet2.xml' $s2.ToString()" f)
      (write-line "  AddEntry 'xl/worksheets/sheet3.xml' $s3.ToString()" f)
      (write-line "  $zip.Dispose(); $zip = $null" f)
      (write-line "  $fs.Dispose(); $fs = $null" f)
      (write-line "  if (-not (Test-Path -LiteralPath $tmpXlsx) -or ((Get-Item -LiteralPath $tmpXlsx).Length -le 0)) { throw 'Temp XLSX missing or empty.' }" f)
      (write-line "  Move-Item -LiteralPath $tmpXlsx -Destination $xlsxPath -Force" f)
      (write-line "} catch {" f)
      (write-line "  if ($zip) { try { $zip.Dispose() } catch {} }" f)
      (write-line "  if ($fs) { try { $fs.Dispose() } catch {} }" f)
      (write-line "  if (Test-Path -LiteralPath $tmpXlsx) { Remove-Item -LiteralPath $tmpXlsx -Force -ErrorAction SilentlyContinue }" f)
      (write-line "  Write-Error $_" f)
      (write-line "  exit 1" f)
      (write-line "}" f)
      (close f)
      (if (setq wsh (vlax-get-or-create-object "WScript.Shell"))
        (progn
          (setq cmd (strcat "powershell.exe -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"" psFile "\" \"" dataFile "\" \"" warnFile "\" \"" xlsxPath "\"")
                exitCode (vlax-invoke-method wsh 'Run cmd 0 1))
          (vlax-release-object wsh)
          (if (and (equal exitCode 0) (findfile xlsxPath))
            xlsxPath
            nil))))))

(defun bkl-ttll:run-with-path (selection path / res rows warnings ext xlsxPath csvPath csvOk xlsxOk)
  (if (not selection)
    (princ "\nT-THTTLL: Không có đối tượng phù hợp.")
    (progn
      (setq res (bkl-ttll:process selection)
            rows (car res)
            warnings (cadr res)
            ext (if (>= (strlen path) 4) (strcase (substr path (- (strlen path) 3))) ""))
      (if (= ext "XLSX")
        (setq xlsxPath path
              csvPath (strcat (substr path 1 (- (strlen path) 5)) ".csv"))
        (setq csvPath path
              xlsxPath (strcat (substr path 1 (- (strlen path) 4)) ".xlsx")))
      (setq csvOk (bkl-ttll:write-csv csvPath rows warnings)
            xlsxOk (bkl-ttll:format-xlsx rows warnings xlsxPath))
      (if (and csvOk xlsxOk)
        (progn
          (princ (strcat "\nT-THTTLL: Đã bóc tách " (itoa (length rows)) " nhịp (" (itoa (length warnings)) " cảnh báo)."))
          (princ (strcat "\n  -> " xlsxPath))
          (princ (strcat "\n  -> " csvPath))
          (foreach w warnings (princ (strcat "\n  [WARN] " w))))
        (progn
          (if (not csvOk)
            (princ (strcat "\nT-THTTLL [ERROR]: Không thể ghi tệp CSV: " csvPath)))
          (if (not xlsxOk)
            (princ (strcat "\nT-THTTLL [ERROR]: Tạo tệp Excel XLSX thất bại (PowerShell exit code != 0 hoặc lỗi ghi tệp): " xlsxPath)))))))
  (princ))

(defun c:T-THTTLL (/ filter selection path defaultName)
  (if (not (equal (ascii "Đ") 272))
    (princ "\nT-THTTLL: Dùng AutoCAD 2021 trở lên. Đặt LISPSYS bằng 1 rồi khởi động lại AutoCAD.")
    (progn
      (setq filter (list '(0 . "INSERT,DIMENSION,TEXT,MTEXT,LWPOLYLINE,LINE,LEADER")
                         (cons 410 (if (= (getvar "CVPORT") 1) (getvar "CTAB") "Model"))))
      (princ "\nQuét chọn hệ thống TTLL (Bể, Ganivo, Tủ, Cột, Dim, Polyline) <Enter để chọn tất cả>: ")
      (setq selection (ssget))
      (if (not selection) (setq selection (ssget "_X" filter)))
      (if selection
        (progn
          (setq defaultName (strcat (getvar "DWGPREFIX") "BANG_THKL_TTLL.xlsx"))
          (if (setq path (getfiled "Lưu bảng khối lượng thông tin liên lạc" defaultName "xlsx;csv" 1))
            (bkl-ttll:run-with-path selection path)))
        (princ "\nT-THTTLL: Không có đối tượng phù hợp."))))
  (princ))

(defun c:BKL_TTLL () (c:T-THTTLL))
(defun c:TTLL () (c:T-THTTLL))

(princ)
