param([string]$SourcePath, [string]$OutputPath)
$ErrorActionPreference = 'Stop'
$root = Split-Path (Split-Path $PSScriptRoot)
if (!$SourcePath) { $SourcePath = Join-Path $root 'OUTPUT/TASK-0023-tuyen.txt' }
if (!$OutputPath) { $OutputPath = Join-Path $root 'OUTPUT/BANG_BOC_TACH_KHOI_LUONG_DUONG_DAY_0.4KV_THAN_KHUE.xlsx' }
$OutputPath = [IO.Path]::GetFullPath($OutputPath)
if (Test-Path -LiteralPath $OutputPath) { Remove-Item -LiteralPath $OutputPath -Force }

$stations = [ordered]@{ A='TBA Dịch Tễ'; B='TBA V69'; C='TBA UBND Song Mai'; D='TBA Phúc Đức 2' }
$sizes = 150,120,95,70,50
$neutral = 120,95,70,50,35

$headers = @(
    'Tên lộ',
    'Từ tủ',
    'Đến tủ/cột',
    'Khoảng cách (m)',
    'Hao hụt, trùng cáp 1% (m)',
    'Lên cột, tủ (m)',
    'Xuống cột, tủ (m)',
    'Dự phòng, lèo (m)',
    'Tổng chiều dài cáp (m)',
    'Tiếp địa T4C-1.5 (bộ)'
)
$headers += @(for ($i=0; $i -lt 5; $i++) { "Cu/XLPE/PVC/DSTA/PVC 3x$($sizes[$i])+1x$($neutral[$i]) mm² (m)" })
$headers += @(
    'Rãnh cáp trên hè (m)',
    'Rãnh cáp qua đường (m)',
    'Rãnh cáp hiện trạng (m)',
    'Trùng rãnh cáp (m)',
    'Ống luồn HDPE 110/90 (m)',
    'Ống luồn HDPE 105/80 (m)',
    'Ống luồn HDPE 85/65 (m)',
    'Ống qua đường UPVC D140 (m)',
    'Ống qua đường UPVC D110 (m)',
    'Tủ công tơ (tủ)',
    'Tủ bù (tủ)'
)
$headers += @($sizes | ForEach-Object { "Đầu cáp co nhiệt DSTA $_ (bộ)" })
$headers += @(
    'Xà CSV+CN (bộ)',
    'Chống sét van hạ thế (quả)',
    'Giá ôm cáp (bộ)',
    'Ống nối đồng nhôm (cái)',
    'Phụ kiện khác'
)

# Đọc dữ liệu các tuyến từ TASK-0023-tuyen.txt
$records = @(Get-Content -LiteralPath $SourcePath -Encoding UTF8 | ForEach-Object {
    $line = $_.Trim()
    if (!$line -or $line.StartsWith('#')) { return }
    $parts = $line.Split('|')
    $key = $parts[0]
    $feeder = $parts[1]
    $from = if ($parts.Length -gt 2) { $parts[2] } else { '' }
    $to = if ($parts.Length -gt 3) { $parts[3] } else { '' }
    $cable = if ($parts.Length -gt 4) { $parts[4] } else { '' }
    $pipe = if ($parts.Length -gt 5) { $parts[5] } else { '' }
    
    $cables = @($cable -split ',' | Where-Object { $_ })
    $pipes = @($pipe -split ',' | Where-Object { $_ })
    [pscustomobject]@{ Key=$key; Feeder=$feeder; From=$from; To=$to; Cable=$cables; Pipe=$pipes }
})

function Get-Column([int]$Index) {
    if ($Index -le 26) { return [string][char](64 + $Index) }
    $first = [char][int](64 + [Math]::Floor(($Index - 1) / 26))
    $second = [char][int](65 + (($Index - 1) % 26))
    return "$first$second"
}

function Write-Total([int]$Row, [string]$Label, [int]$Level, [scriptblock]$References) {
    $sheet.Range("A" + $Row + ":C" + $Row).Merge()
    $sheet.Range("A" + $Row).Value2 = $Label
    $band = $sheet.Range("A" + $Row + ":" + $last + $Row)
    $band.Font.Bold = $true
    $band.Interior.Color = @(0, 15722204, 14671806, 6828582)[$Level]
    if ($Level -eq 3) { $band.Font.Color = 16777215 }
    for ($i=4; $i -le $headers.Count; $i++) {
        $column = Get-Column $i
        $ref = & $References $column
        $sheet.Range("$column$Row").Formula = '=IF(COUNT({0})>0,SUM({0}),"")' -f $ref
    }
}

$excel = $null
$book = $null
try {
    $excel = New-Object -ComObject Excel.Application
    $excel.DisplayAlerts = $false
    $book = $excel.Workbooks.Add(-4167)
    $sheet = $book.Worksheets.Item(1)
    $sheet.Name = 'Bóc tách ĐD 0.4kV'
    $last = Get-Column $headers.Count

    # Header title
    $rangeTitle1 = "A1:" + $last + "1"
    $sheet.Range($rangeTitle1).Merge()
    $sheet.Range("A1").Value2 = 'BẢNG KÊ KHỐI LƯỢNG ĐƯỜNG DÂY HẠ THẾ 0,4KV'
    $sheet.Range("A1").Font.Size = 16
    $sheet.Range("A1").Font.Bold = $true
    $sheet.Range("A1").Font.Color = 16777215
    $sheet.Range("A1").Interior.Color = 6828582
    $sheet.Range("A1").HorizontalAlignment = -4108
    $sheet.Range("A1").RowHeight = 35

    $rangeTitle2 = "A2:" + $last + "2"
    $sheet.Range($rangeTitle2).Merge()
    $sheet.Range("A2").Value2 = 'Công trình: Cải tạo, nâng cấp đường Thân Khuê, phường Đa Mai, TP Bắc Ninh - Hạng mục: Cấp điện sinh hoạt 0.4kV'
    $sheet.Range("A2").Font.Size = 11
    $sheet.Range("A2").Font.Italic = $true
    $sheet.Range("A2").HorizontalAlignment = -4108

    $rangeTitle3 = "A3:" + $last + "3"
    $sheet.Range($rangeTitle3).Merge()
    $sheet.Range("A3").Value2 = 'Căn cứ: Hồ sơ thiết kế bản vẽ thi công MBSH-01 đến MBSH-22 (23 trang bản vẽ)'
    $sheet.Range("A3").Font.Size = 10
    $sheet.Range("A3").Font.Italic = $true
    $sheet.Range("A3").HorizontalAlignment = -4108

    # Table column headers (Row 5)
    for ($i=0; $i -lt $headers.Count; $i++) {
        $address = (Get-Column ($i+1)) + '5'
        $sheet.Range($address).Value2 = $headers[$i]
    }

    $row = 6
    $stationTotals = @()
    $detailCount = 0

    foreach ($key in $stations.Keys) {
        $feederTotals = @()
        $stationRecords = @($records | Where-Object Key -eq $key)
        $feeders = @($stationRecords.Feeder | Select-Object -Unique)
        
        foreach ($feeder in $feeders) {
            $start = $row
            $feederRecs = @($stationRecords | Where-Object Feeder -eq $feeder)
            
            foreach ($record in $feederRecs) {
                $cCount = $record.Cable.Count
                $pCount = $record.Pipe.Count
                $itemCount = [Math]::Max(1, [Math]::Max($cCount, $pCount))

                for ($j=0; $j -lt $itemCount; $j++) {
                    $values = New-Object 'object[,]' 1, $headers.Count
                    $values[0,0] = "$($stations[$key]) / Lộ $feeder"
                    $values[0,1] = $record.From
                    $values[0,2] = $record.To

                    $hasCable = ($j -lt $cCount)
                    if ($hasCable) {
                        $cParts = $record.Cable[$j].Split(':')
                        $size = [int]$cParts[0]
                        $distance = [double]$cParts[1]
                        $idx = [array]::IndexOf($sizes, $size)
                        
                        $values[0,3] = $distance

                        # Lên cột / tủ
                        if ($record.From -match 'Cột|1LT|1H|2LT|LT') {
                            $values[0,5] = 7.0
                        } elseif ($record.From -match 'TBA|T[ĐB]-|TPP|Tủ') {
                            $values[0,5] = 2.5
                        } else {
                            $values[0,5] = 2.5
                        }

                        # Xuống cột / tủ
                        if ($record.To -match 'Cột|1LT|1H|2LT|LT') {
                            $values[0,6] = 7.0
                        } elseif ($record.To -match 'TBA|T[ĐB]-|TPP|Tủ') {
                            $values[0,6] = 2.5
                        } else {
                            $values[0,6] = 2.5
                        }

                        # Dự phòng, lèo
                        $values[0,7] = 6.0

                        # Tiếp địa T4C-1.5: 1 bộ
                        $values[0,9] = 1

                        # Đầu cáp co nhiệt: 2 bộ
                        if ($idx -ge 0) {
                            $values[0, (26 + $idx)] = 2
                        }

                        # Phụ kiện khi lên cột
                        if ($record.To -match 'Cột|1LT|1H|2LT|LT' -or $record.From -match 'Cột|1LT|1H|2LT|LT') {
                            $values[0, 31] = 1 # Xà CSV+CN
                            $values[0, 32] = 3 # CSV hạ thế (3 quả)
                            $values[0, 33] = 1 # Giá ôm cáp
                            $values[0, 34] = 4 # Ống nối đồng nhôm (4 cái)
                        }

                        # Tủ công tơ / Tủ bù
                        if ($record.To -match 'TĐ-') {
                            $values[0, 24] = 1 # Tủ công tơ
                        }
                        if ($record.To -match 'TB-') {
                            $values[0, 25] = 1 # Tủ bù
                        }
                    }

                    # Xử lý ống qua đường
                    if ($j -lt $pCount) {
                        $pParts = $record.Pipe[$j].Split(':')
                        $pSize = $pParts[0]
                        $pLen = [double]$pParts[1]
                        if ($pSize -eq '140') {
                            $values[0, 22] = $pLen # UPVC D140
                            $values[0, 16] = $pLen # Rãnh cáp qua đường
                        } elseif ($pSize -eq '110') {
                            $values[0, 23] = $pLen # UPVC D110
                            $values[0, 16] = $pLen # Rãnh cáp qua đường
                        }
                    }

                    # Rãnh cáp trên hè: nếu có khoảng cách cáp và không phải toàn bộ qua đường
                    if ($hasCable) {
                        $qdLen = if ($j -lt $pCount) { [double]($record.Pipe[$j].Split(':')[1]) } else { 0.0 }
                        $heLen = [Math]::Max(0.0, $distance - $qdLen)
                        if ($heLen -gt 0) {
                            $values[0, 15] = $heLen
                        }
                    }

                    # Ghi dữ liệu vào dòng
                    for ($i=0; $i -lt $headers.Count; $i++) {
                        if ($null -ne $values[0,$i] -and $values[0,$i] -ne '') {
                            $address = (Get-Column ($i+1)) + $row
                            $cell = $sheet.Range($address)
                            [void]$cell.GetType().InvokeMember('Value2', 'SetProperty', $null, $cell, @($values[0,$i]))
                        }
                    }

                    # Đặt công thức tính toán
                    # Hao hụt: 1%
                    $sheet.Range("E$row").Formula = '=IF(ISNUMBER(D{0}),ROUND(D{0}*0.01,2),"")' -f $row
                    # Tổng cộng chiều dài cáp = Khoảng cách + Hao hụt + Lên cột/tủ + Xuống cột/tủ + Dự phòng
                    $sheet.Range("I$row").Formula = '=IF(ISNUMBER(D{0}),SUM(D{0}:H{0}),"")' -f $row

                    if ($hasCable -and $idx -ge 0) {
                        # Cột cáp tương ứng
                        $colCap = Get-Column (11 + $idx)
                        $sheet.Range("$colCap$row").Formula = '=IF(ISNUMBER(I{0}),I{0},"")' -f $row

                        # Ống luồn HDPE tương ứng
                        # 150 -> 110/90 (Col T = 20)
                        # 120 -> 105/80 (Col U = 21)
                        # 95  -> 105/80 (Col U = 21)
                        # 70  -> 85/65  (Col V = 22)
                        # 50  -> 85/65  (Col V = 22)
                        $colHdpe = @('T','U','U','V','V')[$idx]
                        $sheet.Range("$colHdpe$row").Formula = '=IF(ISNUMBER({0}{1}),{0}{1},"")' -f $colCap, $row
                    }

                    # Kẻ dòng xen kẽ nhẹ
                    if ($row % 2 -eq 0) {
                        $sheet.Range("A" + $row + ":" + $last + $row).Interior.Color = 16448250
                    }

                    $row++
                    $detailCount++
                }
            }

            $end = $row - 1
            Write-Total $row "TỔNG LỘ $feeder" 1 { param($c) "$c$($start):$c$end" }
            $feederTotals += $row
            $row++
        }

        Write-Total $row "TỔNG $($stations[$key])" 2 { param($c) ($feederTotals | ForEach-Object { "$c$_" }) -join ',' }
        $stationTotals += $row
        $row++
    }

    # Dòng tổng cộng toàn dự án
    Write-Total $row 'TỔNG CỘNG TOÀN DỰ ÁN' 3 { param($c) ($stationTotals | ForEach-Object { "$c$_" }) -join ',' }

    # Định dạng toàn bộ bảng
    $rangeTable = "A5:" + $last + $row
    $table = $sheet.Range($rangeTable)
    $table.Font.Name = 'Segoe UI'
    $table.Font.Size = 10
    $table.VerticalAlignment = -4108
    $table.Borders.LineStyle = 1
    $table.Borders.Weight = 2
    $table.Borders.Color = 13421772
    $table.WrapText = $true

    # Căn lề
    $sheet.Range("A6:C$row").HorizontalAlignment = -4131 # Căn trái cho tên lộ, từ tủ, đến tủ
    $rangeNums = "D6:" + $last + $row
    $sheet.Range($rangeNums).HorizontalAlignment = -4152 # Căn phải cho số
    $sheet.Range("D6:I$row").NumberFormat = '#,##0.00'
    $sheet.Range("K6:O$row").NumberFormat = '#,##0.00'
    $sheet.Range("P6:X$row").NumberFormat = '#,##0.00'
    $sheet.Range("J6:J$row").NumberFormat = '#,##0'
    $rangeCounts = "Y6:" + $last + $row
    $sheet.Range($rangeCounts).NumberFormat = '#,##0'

    # Độ rộng cột
    $sheet.Range('A:A').ColumnWidth = 26
    $sheet.Range('B:B').ColumnWidth = 22
    $sheet.Range('C:C').ColumnWidth = 32
    $sheet.Range("D:I").ColumnWidth = 14
    $sheet.Range('J:J').ColumnWidth = 12
    $sheet.Range('K:O').ColumnWidth = 20
    $sheet.Range('P:S').ColumnWidth = 14
    $sheet.Range('T:X').ColumnWidth = 16
    $sheet.Range('Y:Z').ColumnWidth = 12
    $sheet.Range('AA:AE').ColumnWidth = 16
    $sheet.Range('AF:AJ').ColumnWidth = 15

    # Định dạng tiêu đề cột
    $rangeHdr = "A5:" + $last + "5"
    $heading = $sheet.Range($rangeHdr)
    $heading.RowHeight = 55
    $heading.HorizontalAlignment = -4108
    $heading.Font.Bold = $true
    $heading.Interior.Color = 14803425

    # Cố định dòng tiêu đề
    $window = $excel.ActiveWindow
    $window.DisplayGridlines = $true
    $window.SplitRow = 5
    $window.SplitColumn = 3
    $window.FreezePanes = $true

    $excel.CalculateFull()
    $book.SaveAs($OutputPath, 51)
    Write-Output "Excel created successfully: $OutputPath ($detailCount detail rows)"
} finally {
    if ($null -ne $book) { $book.Close($false) }
    if ($null -ne $excel) { $excel.Quit() }
}
