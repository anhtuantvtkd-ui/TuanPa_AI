; TASK-0024 / TASK-0025+: Bảng bóc tách khối lượng đường dây 0,4kV sinh hoạt.
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
