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

## Coordinate Transform

Coordinate transformation applies rotation about the origin, then scale, then translation:

```text
Xr = X * cos(angle) - Y * sin(angle)
Yr = X * sin(angle) + Y * cos(angle)

X' = Xr * scale + dx
Y' = Yr * scale + dy
```

Elevation `H` is preserved.
