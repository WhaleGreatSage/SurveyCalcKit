# Formulas and Conventions

This document describes the formulas used by SurveyCalcKit.Core.

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

## Elevation Closure Error

Given known start and end elevations and observed first/last point elevations:

```text
ObservedDeltaH = last.H - first.H
ComputedEndElevation = knownStartElevation + ObservedDeltaH
ClosureError = ComputedEndElevation - knownEndElevation
```

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
