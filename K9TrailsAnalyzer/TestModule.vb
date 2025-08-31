'Imports System.Math
'Imports TrackVideoExporter
'Module TestModule


'    ' Modul obsahující logiku pro výpočet času pohybu a zastavení.

'    ' Struktura pro vrácení výsledků výpočtu.
'    Public Structure TimeStats
'        Public Property TotalTime As TimeSpan
'        Public Property MovingTime As TimeSpan
'        Public Property StoppedTime As TimeSpan
'    End Structure

'    ' Enum pro reprezentaci stavu (pohyb / stání).
'    Private Enum MovementState
'        Moving
'        Stopped
'    End Enum

'    ''' <summary>
'    ''' Vypočítá čistý čas pohybu a čas zastavení ze seznamu bodů trasy
'    ''' pomocí algoritmu s dvěma rychlostními prahy (hystereze), inspirovaného GPXSee.
'    ''' </summary>
'    ''' <param name="trackPoints">Seznam bodů trasy seřazených podle času.</param>
'    ''' <returns>Struktura TimeStats obsahující celkový čas pohybu a čas zastavení.</returns>
'    Public Function CalculateMovementTimes(ByVal trackPoints As List(Of TrackGeoPoint)) As TimeStats
'        ' Pokud je bodů méně než 2, nelze nic počítat.
'        If trackPoints Is Nothing OrElse trackPoints.Count < 2 Then
'            Return New TimeStats()
'        End If

'        ' --- Nastavení prahových hodnot podle GPXSee ---
'        Const MOVING_SPEED_THRESHOLD_MS As Double = 0.5  ' m/s (1.8 km/h)
'        Const STOPPED_SPEED_THRESHOLD_MS As Double = 0.1 ' m/s (0.36 km/h)

'        Dim totalMovingTime As TimeSpan = TimeSpan.Zero
'        Dim totalStoppedTime As TimeSpan = TimeSpan.Zero
'        Dim currentState As MovementState

'        ' --- Hlavní smyčka pro procházení bodů ---
'        For i As Integer = 0 To trackPoints.Count - 2
'            Dim point1 As TrackGeoPoint = trackPoints(i)
'            Dim point2 As TrackGeoPoint = trackPoints(i + 1)

'            Dim timeDiff As TimeSpan = point2.Time - point1.Time

'            ' Přeskočíme segmenty s nulovým nebo záporným časem
'            If timeDiff.TotalSeconds <= 0 Then
'                Continue For
'            End If

'            Dim distanceMeters As Double = HaversineDistance(point1, point2)
'            Dim speedMs As Double = distanceMeters / timeDiff.TotalSeconds

'            ' --- Logika stavového automatu (hystereze) ---
'            ' Pro první segment musíme určit počáteční stav
'            If i = 0 Then
'                currentState = If(speedMs > MOVING_SPEED_THRESHOLD_MS, MovementState.Moving, MovementState.Stopped)
'            Else
'                ' Pro další segmenty měníme stav pouze při překročení opačné hranice
'                If currentState = MovementState.Moving AndAlso speedMs < STOPPED_SPEED_THRESHOLD_MS Then
'                    currentState = MovementState.Stopped
'                ElseIf currentState = MovementState.Stopped AndAlso speedMs > MOVING_SPEED_THRESHOLD_MS Then
'                    currentState = MovementState.Moving
'                End If
'            End If

'            ' --- Sčítání časů podle aktuálního stavu ---
'            If currentState = MovementState.Moving Then
'                totalMovingTime = totalMovingTime.Add(timeDiff)
'            Else
'                totalStoppedTime = totalStoppedTime.Add(timeDiff)
'            End If
'        Next

'        Return New TimeStats With {
'                .MovingTime = totalMovingTime,
'                .StoppedTime = totalStoppedTime
'            }
'    End Function

'    ''' <summary>
'    ''' Vypočítá vzdálenost mezi dvěma GPS souřadnicemi pomocí Haversinova vzorce.
'    ''' </summary>
'    ''' <returns>Vzdálenost v metrech.</returns>
'    Private Function HaversineDistance(ByVal p1 As TrackGeoPoint, ByVal p2 As TrackGeoPoint) As Double
'        Const EarthRadiusMeters As Double = 6371000.0

'        Dim dLat = (p2.Location.Lat - p1.Location.Lat) * (PI / 180.0)
'        Dim dLon = (p2.Location.Lon - p1.Location.Lon) * (PI / 180.0)

'        Dim lat1Rad = p1.Location.Lat * (PI / 180.0)
'        Dim lat2Rad = p2.Location.Lat * (PI / 180.0)

'        Dim a = Sin(dLat / 2) * Sin(dLat / 2) + Sin(dLon / 2) * Sin(dLon / 2) * Cos(lat1Rad) * Cos(lat2Rad)
'        Dim c = 2 * Atan2(Sqrt(a), Sqrt(1 - a))

'        Return EarthRadiusMeters * c
'    End Function


'    ' =========================================================================
'    ' ========================== PŘÍKLAD POUŽITÍ ==============================
'    ' =========================================================================
'    Public Sub TestMain()
'        Debug.WriteLine("Spouštím ukázkový výpočet časů...")

'        ' Vytvoříme si ukázková data (např. krátký běh s pauzou)
'        Dim myTrack As New List(Of TrackGeoPoint) From {
'                     New TrackGeoPoint With {.Location = New Coordinates With {.Lat = 50.08804, .Lon = 14.42076}, .Time = DateTime.Now},
'                      New TrackGeoPoint With {.Location = New Coordinates With {.Lat = 50.0883, .Lon = 14.421}, .Time = DateTime.Now.AddSeconds(10)},
'                       New TrackGeoPoint With {.Location = New Coordinates With {.Lat = 50.0886, .Lon = 14.4213}, .Time = DateTime.Now.AddSeconds(20)},
'                     New TrackGeoPoint With {.Location = New Coordinates With {.Lat = 50.0886, .Lon = 14.4213}, .Time = DateTime.Now.AddSeconds(30)},
'                  New TrackGeoPoint With {.Location = New Coordinates With {.Lat = 50.08865, .Lon = 14.42135}, .Time = DateTime.Now.AddSeconds(40)},
'                     New TrackGeoPoint With {.Location = New Coordinates With {.Lat = 50.0889, .Lon = 14.4216}, .Time = DateTime.Now.AddSeconds(50)},
'                     New TrackGeoPoint With {.Location = New Coordinates With {.Lat = 50.0892, .Lon = 14.4219}, .Time = DateTime.Now.AddSeconds(60)}
'        }

'        ' Zavoláme naši výpočetní funkci
'        Dim result As TimeStats = CalculateMovementTimes(myTrack)
'        Dim totalTime As TimeSpan = myTrack.Last().Time - myTrack.First().Time

'        ' Vypíšeme výsledky
'        Debug.WriteLine("---------------------------------------------")
'        Debug.WriteLine($"Celkový čas záznamu: {totalTime.TotalSeconds:F0} sekund ({totalTime.ToString("g")})")
'        Debug.WriteLine($"Čistý čas (v pohybu): {result.MovingTime.TotalSeconds:F0} sekund ({result.MovingTime.ToString("g")})")
'        Debug.WriteLine($"Čas zastavení: {result.StoppedTime.TotalSeconds:F0} sekund ({result.StoppedTime.ToString("g")})")
'        Debug.WriteLine("---------------------------------------------")


'    End Sub


'End Module
