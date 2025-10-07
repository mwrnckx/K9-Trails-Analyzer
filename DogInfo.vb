
' Upravte metodu CalculateTrialScore, aby používala hodnoty z DogInfo
Private Function CalculateTrialScore(stats As TrailStatsStructure, runnerFoundWeight As Double) As (RunnerFoundPoints As Integer, DogSpeedPoints As Integer, DogAccuracyPoints As Integer, HandlerCheckPoints As Integer)
    ' --- Scoring Configuration ---
    ' Nyní se hodnoty berou z DogInfo
    Dim pointsForFind = Me.DogInfo.PointsForFind
    Dim pointsForCheckpoint = Me.DogInfo.PointsForCheckpoint
    Dim pointsPerKmhGrossSpeed = Me.DogInfo.PointsPerKmhGrossSpeed

    ' --- Initial check ---
    If stats.TotalTime.TotalSeconds <= 0 Then
        Return (0, 0, 0, 0) ' No activity to score.
    End If
    ' --- Step 0: Initialize score accumulators ---
    Dim RunnerFoundPoints, DogSpeedPoints, DogAccuracyPoints As Integer
    Dim HandlerCheckPoints As Integer = 0

    ' --- Step 1: Calculate points based on the primary outcome (Find vs. No-Find) ---
    RunnerFoundPoints = pointsForFind * runnerFoundWeight

    ' --- Step 2: Calculate bonus points for performance metrics ---
    If stats.DogGrossSpeed > 0 Then
        Dim weight As Double = stats.MaxTeamDistance / stats.RunnerDistance
        DogSpeedPoints = CInt(Math.Round(stats.DogGrossSpeed * pointsPerKmhGrossSpeed * weight))
    End If

    '--- Step 3: Calculate points for trail following  ---
    If Double.IsNaN(stats.WeightedDistanceAlongTrailPerCent) OrElse
       Double.IsInfinity(stats.WeightedDistanceAlongTrailPerCent) OrElse
       stats.WeightedDistanceAlongTrailPerCent < Integer.MinValue OrElse
       stats.WeightedDistanceAlongTrailPerCent > Integer.MaxValue Then
        DogAccuracyPoints = 0
    Else
        DogAccuracyPoints = CInt(Math.Round(stats.WeightedDistanceAlongTrailPerCent))
    End If

    '--- Step 4: Calculate points for each reached checkpoint ---
    If stats.CheckpointsEval IsNot Nothing AndAlso stats.CheckpointsEval.Count > 1 Then
        Dim lastCheckPointIndex = stats.CheckpointsEval.Count - 2
        Dim preLastIndex = lastCheckPointIndex - 1

        For i As Integer = Math.Max(lastCheckPointIndex - 1, 0) To lastCheckPointIndex
            Dim checkPointEval = stats.CheckpointsEval(i)
            Dim Deviation = checkPointEval.deviationFromTrail
            Dim _weight As Double = Weight(Deviation)
            HandlerCheckPoints += (checkPointEval.distanceAlongTrail / (stats.RunnerDistance)) * _weight * pointsForCheckpoint
        Next
    End If

    Return (RunnerFoundPoints, DogSpeedPoints, DogAccuracyPoints, HandlerCheckPoints)
End Function
