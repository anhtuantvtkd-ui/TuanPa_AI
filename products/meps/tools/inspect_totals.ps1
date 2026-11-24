$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$wb = $excel.Workbooks.Open('D:\TuanPa_AI\OUTPUT\BANG_BOC_TACH_KHOI_LUONG_DUONG_DAY_0.4KV_THAN_KHUE.xlsx')
$ws = $wb.Sheets.Item(1)
$used = $ws.UsedRange
$lastRow = $used.Rows.Count
Write-Host "=== TỔNG CỘNG TOÀN DỰ ÁN (ROW $lastRow) ==="
for ($c = 1; $c -le $used.Columns.Count; $c++) {
    $hdr = $ws.Cells.Item(5, $c).Text
    $val = $ws.Cells.Item($lastRow, $c).Text
    if ($val) {
        Write-Host "Col $($c.ToString('D2')) [$hdr] : $val"
    }
}
$wb.Close($false)
$excel.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
