param(
    [string]$csvPath,
    [string]$xlsxPath
)
$csvPath = [System.IO.Path]::GetFullPath($csvPath)
$xlsxPath = [System.IO.Path]::GetFullPath($xlsxPath)
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
try {
    # Open CSV
    $wb = $excel.Workbooks.Open($csvPath)
    $ws = $wb.Sheets.Item(1)
    $lastRow = $ws.UsedRange.Rows.Count
    
    # Font toan bo sheet: Times New Roman, 11
    $ws.Cells.Font.Name = "Times New Roman"
    $ws.Cells.Font.Size = 11
    
    # Do rong cac cot chuan can doi (15 cot: A den O)
    $ws.Columns.Item(1).ColumnWidth = 7      # STT
    $ws.Columns.Item(2).ColumnWidth = 16     # Ten tu
    $ws.Columns.Item(3).ColumnWidth = 14     # So nha
    $ws.Columns.Item(4).ColumnWidth = 12     # Tu tu dien den lo dat
    $ws.Columns.Item(5).ColumnWidth = 12     # Tu ranh cap den dau lo dat
    $ws.Columns.Item(6).ColumnWidth = 12     # Len xuong ranh cap
    $ws.Columns.Item(7).ColumnWidth = 12     # Len diem dau vao nha
    $ws.Columns.Item(8).ColumnWidth = 13     # ranh cap tu truc chinh
    $ws.Columns.Item(9).ColumnWidth = 8      # 1F
    $ws.Columns.Item(10).ColumnWidth = 8     # 3F
    $ws.Columns.Item(11).ColumnWidth = 13    # 40/30 (DSTA 10)
    $ws.Columns.Item(12).ColumnWidth = 13    # 50/40 (DSTA 16)
    $ws.Columns.Item(13).ColumnWidth = 13    # 50/40 (Cho ong)
    $ws.Columns.Item(14).ColumnWidth = 13    # 3x16+1x10
    $ws.Columns.Item(15).ColumnWidth = 13    # 2x10

    # Dong 1: Tieu de bang
    $ws.Rows.Item(1).RowHeight = 32
    $title = $ws.Range("A1:O1")
    $title.Merge()
    $title.Font.Size = 13
    $title.Font.Bold = $true
    $title.HorizontalAlignment = -4108 # xlCenter
    $title.VerticalAlignment = -4108   # xlCenter

    # Dong 2-4: Tieu de cac cot
    $ws.Rows.Item(2).RowHeight = 45
    $ws.Rows.Item(3).RowHeight = 25
    $ws.Rows.Item(4).RowHeight = 15
    
    # Merge cac tieu de cot
    $ws.Range("A2:A4").Merge() # STT
    $ws.Range("B2:B4").Merge() # Ten tu
    $ws.Range("C2:C4").Merge() # So phu tai (ho)
    $ws.Range("D2:D4").Merge() # Tu tu dien den lo dat
    $ws.Range("E2:E4").Merge() # Tu ranh cap den dau lo dat
    $ws.Range("F2:F4").Merge() # Len xuong ranh cap
    $ws.Range("G2:G4").Merge() # Len diem dau vao nha
    $ws.Range("H2:H4").Merge() # ranh cap tu truc chinh den ho dan (m)
    $ws.Range("I2:I4").Merge() # So cong to 1F
    $ws.Range("J2:J4").Merge() # So cong to 3F
    $ws.Range("K2:M2").Merge() # Ong HDPE gan xoan
    $ws.Range("K3:K4").Merge() # 40/30 (DSTA 10)
    $ws.Range("L3:L4").Merge() # 50/40 (DSTA 16)
    $ws.Range("M3:M4").Merge() # 50/40 (Cho ong)
    $ws.Range("N2:O2").Merge() # Cap ngam ha the
    $ws.Range("N3:N4").Merge() # 3x16+1x10
    $ws.Range("O3:O4").Merge() # 2x10
    
    # Dinh dang Header
    $headerRange = $ws.Range("A2:O4")
    $headerRange.Font.Bold = $true
    $headerRange.Font.Size = 11
    $headerRange.WrapText = $true
    $headerRange.HorizontalAlignment = -4108
    $headerRange.VerticalAlignment = -4108
    
    # Dong 5: A. DUONG DAY 0,4KV...
    $ws.Rows.Item(5).RowHeight = 22
    $ws.Range("A5:O5").Merge()
    $sec5 = $ws.Range("A5:O5")
    $sec5.Font.Bold = $true
    $sec5.HorizontalAlignment = -4131 # xlLeft
    $sec5.VerticalAlignment = -4108
    
    # Dong 6: LO I
    $ws.Rows.Item(6).RowHeight = 20
    $ws.Range("A6:O6").Merge()
    $sec6 = $ws.Range("A6:O6")
    $sec6.Font.Bold = $true
    $sec6.HorizontalAlignment = -4131 # xlLeft
    $sec6.VerticalAlignment = -4108
    
    # Dinh dang cac dong du lieu (Dong 7 den het)
    if ($lastRow -ge 7) {
        $dataRange = $ws.Range("A7:O$lastRow")
        $dataRange.HorizontalAlignment = -4108 # Can giua tat ca
        $dataRange.VerticalAlignment = -4108   # Can giua chieu doc
        for ($r = 7; $r -le $lastRow; $r++) {
            $ws.Rows.Item($r).RowHeight = 18
            if ($ws.Cells.Item($r, 1).Text -ne '') {
                $ws.Rows.Item($r).Font.Bold = $true
            }
        }
    }
    
    # Bo toan bo khung vien bang (A2 den dong cuoi)
    $tableRange = $ws.Range("A2:O$lastRow")
    # Tat ca cac duong vien trong va ngoai: LineStyle = 1 (xlContinuous), Weight = 2 (xlThin)
    $tableRange.Borders.LineStyle = 1
    $tableRange.Borders.Weight = 2
    
    # Duong vien bao ngoai bang: Weight = 2
    for ($edge = 1; $edge -le 4; $edge++) {
        $tableRange.Borders.Item($edge).Weight = 2
    }
    
    # Luu thanh file .XLSX
    $wb.SaveAs($xlsxPath, 51)
    $wb.Close($false)
    Write-Host "SUCCESS: Formatted XLSX saved to $xlsxPath"
} finally {
    $excel.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($excel) | Out-Null
}
