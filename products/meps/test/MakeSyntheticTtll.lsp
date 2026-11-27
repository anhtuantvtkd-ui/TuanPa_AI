; Tạo bản vẽ synthetic kiểm thử T-THTTLL (không chứa dữ liệu khách hàng)
(defun syn:make-layer (name)
  (if (not (tblsearch "LAYER" name))
    (entmake (list '(0 . "LAYER") '(100 . "AcDbSymbolTableRecord") '(100 . "AcDbLayerTableRecord")
                   (cons 2 name) '(70 . 0) '(62 . 7)))))

(defun syn:make-block-with-attrib (blkName tag prompt isDrifted / p1 p2)
  (if (not (tblsearch "BLOCK" blkName))
    (progn
      (entmake (list '(0 . "BLOCK") (cons 2 blkName) '(70 . 2) '(10 0.0 0.0 0.0)))
      (if isDrifted
        (progn
          ; Hình học nằm tại (50,50) để test basepoint trôi xa khỏi WCS center
          (entmake (list '(0 . "LWPOLYLINE") '(100 . "AcDbEntity") '(100 . "AcDbPolyline")
                         '(90 . 4) '(70 . 1)
                         '(10 49.0 49.0) '(10 51.0 49.0) '(10 51.0 51.0) '(10 49.0 51.0))))
        (entmake (list '(0 . "LWPOLYLINE") '(100 . "AcDbEntity") '(100 . "AcDbPolyline")
                       '(90 . 4) '(70 . 1)
                       '(10 -0.5 -0.5) '(10 0.5 -0.5) '(10 0.5 0.5) '(10 -0.5 0.5))))
      (entmake (list '(0 . "ATTDEF") '(100 . "AcDbEntity") '(100 . "AcDbText")
                     '(10 0.0 0.0 0.0) '(40 . 1.0) '(1 . "")
                     '(100 . "AcDbAttributeDefinition") (cons 3 prompt) (cons 2 tag) '(70 . 0)))
      (entmake '((0 . "ENDBLK"))))))

(defun syn:make-pole-block (blkName)
  (if (not (tblsearch "BLOCK" blkName))
    (progn
      (entmake (list '(0 . "BLOCK") (cons 2 blkName) '(70 . 0) '(10 0.0 0.0 0.0)))
      (entmake (list '(0 . "CIRCLE") '(10 0.0 0.0 0.0) '(40 . 0.4)))
      (entmake '((0 . "ENDBLK"))))))

(defun syn:insert-attr (blkName pt tag val)
  (entmake (list '(0 . "INSERT") (cons 2 blkName) (cons 10 (list (car pt) (cadr pt) 0.0)) '(66 . 1)))
  (entmake (list '(0 . "ATTRIB") '(100 . "AcDbEntity") '(100 . "AcDbText")
                 (cons 10 (list (car pt) (cadr pt) 0.0)) '(40 . 1.0) (cons 1 val)
                 '(100 . "AcDbAttribute") (cons 2 tag) '(70 . 0)))
  (entmake '((0 . "SEQEND"))))

(defun syn:dim (layer p1 p2 txt)
  (entmake (list '(0 . "DIMENSION") '(100 . "AcDbEntity") (cons 8 layer)
                 '(100 . "AcDbDimension")
                 (cons 10 (list (* 0.5 (+ (car p1) (car p2))) (+ (* 0.5 (+ (cadr p1) (cadr p2))) 2.0) 0.0))
                 '(70 . 33) (cons 1 txt)
                 '(100 . "AcDbAlignedDimension")
                 (cons 13 (list (car p1) (cadr p1) 0.0))
                 (cons 14 (list (car p2) (cadr p2) 0.0)))))

(defun syn:build-all ()
  (syn:make-layer "TC vien thong")
  (syn:make-layer "KT")
  (syn:make-layer "S-DIM")
  (syn:make-block-with-attrib "BE2 DAN59" "TVXD_LABEL_033" "Label" T)
  (syn:make-block-with-attrib "ganivo" "TVXD_LABEL_033" "Label" nil)
  (syn:make-block-with-attrib "tupptt" "TVXD_LABEL_033" "Label" nil)
  (syn:make-pole-block "Cot Dien")

  ; BE với basepoint trôi (-50, -50) -> tâm hình học WCS tại (0,0), (80,0), (160,0), (80,50)
  (syn:insert-attr "BE2 DAN59" '(-50.0 -50.0) "TVXD_LABEL_033" "B1")
  (syn:insert-attr "BE2 DAN59" '(30.0 -50.0) "TVXD_LABEL_033" "B2")
  (syn:insert-attr "BE2 DAN59" '(110.0 -50.0) "TVXD_LABEL_033" "B3")
  (syn:insert-attr "BE2 DAN59" '(30.0 0.0) "TVXD_LABEL_033" "B4")
  ; Ký hiệu nhãn rỗng + dim/polyline cụm ký hiệu tại (500,500) -> phải bị bỏ qua
  (syn:insert-attr "BE2 DAN59" '(450.0 450.0) "TVXD_LABEL_033" "")
  (syn:dim "S-DIM" '(500.0 500.0) '(510.0 500.0) "2D110(10m)")
  (entmake '((0 . "LWPOLYLINE") (100 . "AcDbEntity") (8 . "TC vien thong") (100 . "AcDbPolyline")
             (90 . 2) (70 . 0) (10 500.0 500.0) (10 510.0 500.0)))

  ; Tủ & Ganivo (B1 -> G1 chuẩn hóa)
  (syn:insert-attr "tupptt" '(80.0 1.0) "TVXD_LABEL_033" "TT-01")
  (syn:insert-attr "ganivo" '(90.0 1.0) "TVXD_LABEL_033" "B1")
  (syn:insert-attr "ganivo" '(100.0 1.0) "TVXD_LABEL_033" "G2")

  ; Cột điện + MTEXT + LEADER + Polyline TC vien thong
  (entmake '((0 . "INSERT") (2 . "Cot Dien") (10 180.0 0.0 0.0)))
  (entmake '((0 . "MTEXT") (100 . "AcDbEntity") (100 . "AcDbMText")
             (10 185.0 5.0 0.0) (40 . 1.0) (1 . "TÊN CỘT: 19/1 - LOẠI CỘT: LT10")))
  (entmake '((0 . "LEADER") (100 . "AcDbEntity") (100 . "AcDbLeader")
             (10 180.0 0.0 0.0) (10 185.0 5.0 0.0)))
  (entmake '((0 . "LWPOLYLINE") (100 . "AcDbEntity") (8 . "TC vien thong") (100 . "AcDbPolyline")
             (90 . 3) (70 . 0) (10 0.0 0.0) (10 80.0 0.0) (10 180.0 0.0)))

  ; Dim tuyến 2D110 + QĐ + ghép nhiều dim
  (syn:dim "S-DIM" '(0.0 0.0) '(80.0 0.0) "2D110(80m)")
  (syn:dim "S-DIM" '(30.0 0.0) '(45.0 0.0) "Q\\U+0110(15m)")
  (syn:dim "S-DIM" '(80.0 0.0) '(115.0 0.0) "2D110(35m)")
  (syn:dim "S-DIM" '(115.0 0.0) '(160.0 0.0) "2D110(45m)")

  ; Nút giao bậc 4 tại (80, 25) cho 4D110
  (syn:dim "S-DIM" '(80.0 0.0) '(80.0 25.0) "4D110(25m)")
  (syn:dim "S-DIM" '(80.0 25.0) '(80.0 50.0) "4D110(25m)")
  (syn:dim "S-DIM" '(0.0 0.0) '(80.0 25.0) "4D110(84m)")
  (syn:dim "S-DIM" '(80.0 25.0) '(160.0 0.0) "4D110(84m)")

  ; Đoạn ngắn 1m B2 <-> TT-01 và nhánh cột 1D110 kèm QĐHT > KC (25m > 20m)
  (syn:dim "S-DIM" '(80.0 0.0) '(80.0 1.0) "1D110(1m)")
  (syn:dim "S-DIM" '(160.0 0.0) '(180.0 0.0) "D110(20m)")
  (syn:dim "S-DIM" '(162.0 0.0) '(178.0 0.0) "Q\\U+0110(25m)")

  ; Mạng D60
  (syn:dim "S-DIM" '(80.0 1.0) '(90.0 1.0) "2D60(10m)")
  (syn:dim "S-DIM" '(90.0 1.0) '(100.0 1.0) "2D60(10m)")

  ; 1 dim dead-end bất thường gần B1
  (syn:dim "S-DIM" '(10.0 10.0) '(22.0 10.0) "2D110(12m)")
  (princ "\nSynthetic TTLL drawing built.")
  (princ))
