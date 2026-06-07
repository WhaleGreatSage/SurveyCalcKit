# Formulas and Conventions

This document describes the formulas used by SurveyCalcKit.Core.

## Excel Data Conventions

Excel point import uses `.xlsx` files with headers on the first row:

```text
Name | X | Y | H
```

`Name`, `X`, and `Y` are required. `H` is optional. Imported points use the same formulas below as text point input. Excel export does not change calculation formulas; it writes calculated results into structured worksheets.

Exported workbook types include:

- `Traverse`: segment differences, distances, azimuth, elevation difference, and slope.
- `Leveling Summary` and `Adjusted Elevations`: leveling closure and adjusted elevations.
- `Polygon Area` and `Polygon Points`: shoelace area summary and source points.
- `Report`: WinForms report text, one line per row.

## Segment Differences

For two points `from` and `to`:

```text
Dx = to.X - from.X
Dy = to.Y - from.Y
```

## 2D Distance

```text
Distance2D = sqrt(Dx^2 + Dy^2)
```

## Delta H

When both points have elevation values:

```text
DeltaH = to.H - from.H
```

When either point is missing elevation, `DeltaH` is not calculated.

## 3D Distance

When `DeltaH` is available:

```text
Distance3D = sqrt(Distance2D^2 + DeltaH^2)
```

## Slope Percentage

When `Distance2D` is not zero and `DeltaH` is available:

```text
SlopePercent = DeltaH / Distance2D * 100
```

If the horizontal distance is zero, slope is not calculated to avoid division by zero.

## Azimuth

SurveyCalcKit reports azimuth in degrees from `0` to `360` using:

```text
AzimuthDegrees = atan2(Dy, Dx) * 180 / pi
```

Negative results are normalized by adding `360`.

Examples:

```text
Dx= 1, Dy= 1 -> 45 degrees
Dx=-1, Dy= 1 -> 135 degrees
Dx=-1, Dy=-1 -> 225 degrees
Dx= 1, Dy=-1 -> 315 degrees
```

SurveyCalcKit uses this same mathematical coordinate convention for coordinate forward calculation: the angle is measured counterclockwise from the positive X axis. It is not the north-clockwise bearing convention used in some field books.

## Coordinate Forward Calculation

Given a known start point, azimuth angle, and horizontal distance:

```text
Start = (X0, Y0)
A = azimuth in degrees
D = horizontal distance
```

Convert degrees to radians internally:

```text
Radians = A * pi / 180
```

Then calculate:

```text
DeltaX = D * cos(Radians)
DeltaY = D * sin(Radians)

X1 = X0 + DeltaX
Y1 = Y0 + DeltaY
```

Azimuth input is normalized to `0` through `360` before calculation. Zero distance is valid and returns the start coordinate. Negative distance is reported as a warning and is not used as a signed reverse direction.

## Chainage and Offset Projection

Given a baseline from start point `A` to end point `B`, and a target point `P`:

```text
vx = B.X - A.X
vy = B.Y - A.Y

wx = P.X - A.X
wy = P.Y - A.Y

L2 = vx * vx + vy * vy
BaselineLength = sqrt(L2)
```

The projection ratio along the infinite baseline is:

```text
t = (wx * vx + wy * vy) / L2
```

The projected coordinate is:

```text
ProjectionX = A.X + t * vx
ProjectionY = A.Y + t * vy
```

Distance along the baseline and final chainage are:

```text
Along = t * BaselineLength
Chainage = StartChainage + Along
```

Perpendicular offset is the distance from the target point to the projection point:

```text
Offset = sqrt((P.X - ProjectionX)^2 + (P.Y - ProjectionY)^2)
```

Side is determined with the 2D cross product:

```text
Cross = vx * wy - vy * wx
```

- `Cross > tolerance`: `Left`
- `Cross < -tolerance`: `Right`
- otherwise: `OnLine`

The projection is inside the baseline segment when:

```text
0 <= t <= 1
```

If the baseline length is zero, SurveyCalcKit returns a warning and avoids division by zero.

## Elevation Closure Error

Given known start and end elevations and observed first/last point elevations:

```text
ObservedDeltaH = last.H - first.H
ComputedEndElevation = knownStartElevation + ObservedDeltaH
ClosureError = ComputedEndElevation - knownEndElevation
```

## Leveling Route Closure Error

A leveling route starts from a benchmark elevation and closes on another known benchmark. Each station contains a backsight reading and a foresight reading.

```text
SumBacksight = sum(all backsight readings)
SumForesight = sum(all foresight readings)

ObservedHeightDifference = SumBacksight - SumForesight
KnownHeightDifference = EndElevation - StartElevation
ClosureError = ObservedHeightDifference - KnownHeightDifference
```

## Leveling Route Height Adjustment

SurveyCalcKit distributes leveling closure error equally by station count:

```text
CorrectionPerStation = -ClosureError / StationCount
Correction_i = CorrectionPerStation * i
AdjustedElevation_i = RawElevation_i + Correction_i
```

Raw elevations are accumulated from the start benchmark:

```text
RawElevation_i = RawElevation_(i-1) + Backsight_i - Foresight_i
```

If there are no observations, the station count is zero and no correction is calculated. Negative backsight or foresight values are reported as warnings.

## Leveling Adjustment Limitations

This simple method is intended for beginner workflows. It distributes correction by station count only. It does not weight by sight length, route distance, instrument precision, environmental conditions, or least-squares observation models.

## Closed Traverse Coordinate Closure

A closed traverse should return to the starting point. In practice, small observation and rounding errors often leave a small coordinate closure error:

```text
fx = end.X - start.X
fy = end.Y - start.Y
f = sqrt(fx^2 + fy^2)
```

SurveyCalcKit treats input as closed when the first and last point names match, or when the first and last coordinates are within a small coordinate tolerance.

## Relative Closure Ratio

The relative closure ratio compares the total traverse length with the linear closure error:

```text
RelativeClosureRatio = TotalLength / f
```

Reports display this as `1:n`. If `f` is zero, the ratio is reported as infinity because the traverse has perfect coordinate closure.

## Bowditch / Compass Rule Adjustment

Bowditch adjustment distributes the coordinate closure error across each segment in proportion to segment length:

```text
CorrectionDx_i = -fx * SegmentLength_i / TotalLength
CorrectionDy_i = -fy * SegmentLength_i / TotalLength

AdjustedDx_i = OriginalDx_i + CorrectionDx_i
AdjustedDy_i = OriginalDy_i + CorrectionDy_i
```

Adjusted point coordinates are then accumulated from the starting point using the adjusted segment differences. The final adjusted coordinate should return to the starting coordinate within rounding tolerance.

## Bowditch Limitations

This simple Compass Rule implementation is intended for beginner workflows and classroom examples. It assumes segment observations have comparable quality and distributes error only by segment length. It does not model instrument precision, angle/distance weighting, covariance, or least-squares network adjustment.

## Coordinate Transform

Coordinate transformation applies rotation about the origin, then scale, then translation:

```text
Xr = X * cos(angle) - Y * sin(angle)
Yr = X * sin(angle) + Y * cos(angle)

X' = Xr * scale + dx
Y' = Yr * scale + dy
```

Elevation `H` is preserved.
