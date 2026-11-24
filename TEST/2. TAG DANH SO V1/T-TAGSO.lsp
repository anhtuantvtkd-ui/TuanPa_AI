;;; ==========================================================================
;;; PROTOTYPE LISP: T-TAGSO.lsp
;;; Muc dich: Kiem chung thuat toan danh so lien tiep cho Block Attrib / Text
;;; 1. Thu tu chon tay (Pick Order - chon tung doi tuong tu trai sang phai)
;;; 2. Quet chon (Window/Crossing) va sap xep tu trai sang phai (kem dung sai sai so Y)
;;; ==========================================================================

(vl-load-com)

(defun c:T-TAGSO-PICK ( / doc entList ent pt prefix start val tagStr)
  (setq doc (vla-get-activedocument (vlax-get-acad-object)))
  (vla-startundomark doc)
  (princ "\n--- DANH SO THEO THU TU CLICK CHON (PICK ORDER) ---")
  (setq prefix (getstring "\nNhap tien to (vi du TAG-, hoac Enter de bo trong): "))
  (setq start (getint "\nNhap so bat dau <1>: "))
  (if (null start) (setq start 1))
  
  (setq entList nil)
  (while (setq ent (car (entsel (strcat "\nChon doi tuong so " (itoa start) " (Enter de ket thuc): "))))
    (setq tagStr (strcat prefix (itoa start)))
    (sub-set-text ent tagStr)
    (setq start (1+ start))
  )
  (vla-endundomark doc)
  (princ "\nHoan tat danh so theo Pick Order!")
  (princ)
)

(defun c:T-TAGSO-LR ( / ss i ent elist pt x y lst prefix start tol rowMap tagStr)
  (setq doc (vla-get-activedocument (vlax-get-acad-object)))
  (vla-startundomark doc)
  (princ "\n--- DANH SO THEO TOA DO TU TRAI SANG PHAI (L-R GEOMETRIC) ---")
  (setq prefix (getstring "\nNhap tien to (vi du TAG-, hoac Enter de bo trong): "))
  (setq start (getint "\nNhap so bat dau <1>: "))
  (if (null start) (setq start 1))
  
  (princ "\nQuet chon cac doi tuong can danh so:")
  (if (setq ss (ssget))
    (progn
      (setq lst nil)
      (setq i 0)
      (while (< i (sslength ss))
        (setq ent (ssname ss i))
        (setq elist (entget ent))
        (setq pt (cdr (assoc 10 elist))) ; Toa do diem chen (WCS/UCS)
        (if pt
          (setq lst (cons (list (car pt) (cadr pt) ent) lst))
        )
        (setq i (1+ i))
      )
      
      ;; Sap xep tu trai sang phai (X tang dan)
      ;; Neu X bang nhau thi sap theo Y tu tren xuong (Y giam dan)
      (setq lst (vl-sort lst
        '(lambda (a b)
           (if (equal (car a) (car b) 1e-4)
             (> (cadr a) (cadr b))
             (< (car a) (car b))
           )
         )
      ))
      
      ;; Gan text theo thu tu da sap xep
      (foreach item lst
        (setq ent (caddr item))
        (setq tagStr (strcat prefix (itoa start)))
        (sub-set-text ent tagStr)
        (setq start (1+ start))
      )
      (princ (strcat "\nDa danh so thanh cong cho " (itoa (length lst)) " doi tuong!"))
    )
  )
  (vla-endundomark doc)
  (princ)
)

(defun sub-set-text (ent newText / elist obj atts)
  (setq obj (vlax-ename->vla-object ent))
  (cond
    ;; Neu la Block Reference co Attribute
    ((and (= (vla-get-objectname obj) "AcDbBlockReference")
          (= (vla-get-hasattributes obj) :vlax-true))
     (setq atts (vlax-invoke obj 'GetAttributes))
     (if atts
       (vla-put-textstring (car atts) newText)
     )
    )
    ;; Neu la Text hoac MText
    ((or (= (vla-get-objectname obj) "AcDbText")
         (= (vla-get-objectname obj) "AcDbMText"))
     (vla-put-textstring obj newText)
    )
  )
)

(princ "\n[TVXD] Da load xong T-TAGSO.lsp! Su dung lenh:")
(princ "\n   - T-TAGSO-PICK : Danh so theo thu tu click tung doi tuong")
(princ "\n   - T-TAGSO-LR   : Danh so quet chon tu trai sang phai (X tang dan)")
(princ)
