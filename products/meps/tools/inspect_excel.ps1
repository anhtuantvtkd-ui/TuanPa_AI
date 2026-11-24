$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$wb = $excel.Workbooks.Open('D:\TuanPa_AI\OUTPUT\BANG_BOC_TACH_KHOI_LUONG_DUONG_DAY_0.4KV_THAN_KHUE.xlsx')
$ws = $wb.Sheets.Item(1)
$used = $ws.UsedRange
$lastRow = $used.Rows.Count
Write-Host "Total Rows: $lastRow, Cols: $($used.Columns.Count)"
Write-Host "Row $lastRow label: $($ws.Cells.Item($lastRow, 1).Text)"
Write-Host "Row $lastRow Khoang cach (Col 4): $($ws.Cells.Item($lastRow, 4).Text)"
Write-Host "Row $lastRow Hao hut (Col 5): $($ws.Cells.Item($lastRow, 5).Text)"
Write-Host "Row $lastRow Tong cap (Col 9): $($ws.Cells.Item($lastRow, 9).Text)"
Write-Host "Row $lastRow Cap 150 (Col 11): $($ws.Cells.Item($lastRow, 11).Text)"
Write-Host "Row $lastRow Cap 120 (Col 12): $($ws.Cells.Item($lastRow, 12).Text)"
Write-Host "Row $lastRow Cap 95 (Col 13): $($ws.Cells.Item($lastRow, 13).Text)"
Write-Host "Row $lastRow Cap 70 (Col 14): $($ws.Cells.Item($lastRow, 14).Text)"
Write-Host "Row $lastRow Cap 50 (Col 15): $($ws.Cells.Item($lastRow, 15).Text)"
$wb.Close($false)
$excel.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
