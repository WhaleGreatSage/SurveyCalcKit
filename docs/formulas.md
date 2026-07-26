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

## Coordinate Inverse Calculation

Given two known points:

```text
From = (X1, Y1, H1)
To = (X2, Y2, H2)
```

SurveyCalcKit calculates:

```text
DeltaX = X2 - X1
DeltaY = Y2 - Y1
Distance2D = sqrt(DeltaX^2 + DeltaY^2)
AzimuthDegrees = atan2(DeltaY, DeltaX) * 180 / pi
```

The azimuth uses the same convention as traverse calculations: degrees counterclockwise from the positive X axis, normalized to `0` through `360`.

If both heights exist:

```text
DeltaH = H2 - H1
Distance3D = sqrt(Distance2D^2 + DeltaH^2)
```

If the two coordinates are identical or extremely close, SurveyCalcKit reports a warning and uses `0` degrees for azimuth.

## Batch Segment Table

For a point sequence, each row is calculated from consecutive points:

```text
Row_i = Point_i -> Point_(i+1)
```

The row uses the same segment formulas for `DeltaX`, `DeltaY`, `Distance2D`, `AzimuthDegrees`, `DeltaH`, and `SlopePercent`.

Cumulative distance is:

```text
CumulativeDistance_i = sum(Distance2D_1 ... Distance2D_i)
```

Repeated consecutive coordinates are reported as warnings because distance and slope checks may be weak.

## Angle Format Conversion

Decimal degrees to degrees-minutes-seconds:

```text
Degrees = floor(abs(decimalDegrees))
MinutesFloat = (abs(decimalDegrees) - Degrees) * 60
Minutes = floor(MinutesFloat)
Seconds = (MinutesFloat - Minutes) * 60
```

The sign is preserved for negative angles:

```text
-12.5 degrees = -12°30'00.00"
```

Degrees-minutes-seconds to decimal degrees:

```text
DecimalDegrees = sign * (Degrees + Minutes / 60 + Seconds / 3600)
```

Degree-radian conversion:

```text
Radians = DecimalDegrees * pi / 180
DecimalDegrees = Radians * 180 / pi
```

DMS output is rounded for display only. Calculations use the decimal degree value.

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

## Enhanced Traverse Quality Evaluation

Enhanced closed traverse quality evaluation reuses coordinate closure and total length:

```text
fx = end.X - start.X
fy = end.Y - start.Y
f = sqrt(fx^2 + fy^2)
RelativeClosureDenominator = TotalLength / f
```

If `f` is zero within tolerance, SurveyCalcKit reports an infinite relative closure denominator and a perfect closure warning.

When an allowable relative closure denominator is provided:

```text
PassesLinearClosureLimit = RelativeClosureDenominator >= AllowableRelativeClosureDenominator
```

For optional angular closure, SurveyCalcKit uses a simple closed-polygon interior-angle check. For an `n`-sided closed traverse:

```text
TheoreticalInteriorAngleSum = (n - 2) * 180 degrees
AngularClosureErrorSeconds = (sum(ObservedAnglesDegrees) - TheoreticalInteriorAngleSum) * 3600
AllowableAngularClosureSeconds = AllowableAngularClosureSecondsPerStation * sqrt(n)
```

The angular check passes when:

```text
abs(AngularClosureErrorSeconds) <= AllowableAngularClosureSeconds
```

Quality grades are intentionally simple:

- `Failed`: linear or angular limit fails.
- `Excellent`: perfect closure or relative denominator at least 10000.
- `Good`: relative denominator at least 5000.
- `Pass`: limits are satisfied but not high enough for the stronger grades.
- `NotEvaluated`: too few points, zero total length, or no usable limits.

This module is a quality screening tool. It is not a least-squares network adjustment.

## Circular Curve Elements

For a simple circular curve:

```text
R = radius
Delta = deflection/intersection angle in degrees
PI = PI chainage
```

Convert half the angle to radians:

```text
HalfAngle = Delta * pi / 180 / 2
```

Then calculate:

```text
T = R * tan(HalfAngle)
L = pi * R * Delta / 180
E = R * (sec(HalfAngle) - 1)
M = R * (1 - cos(HalfAngle))
PC/ZY = PI - T
PT/YZ = PC/ZY + L
```

SurveyCalcKit validates `R > 0` and `0 < Delta < 180`. Units are meters and degrees.

## Vertical Curve Elements

For a simple parabolic vertical curve:

```text
PVI = point of vertical intersection chainage
E_pvi = PVI elevation
g1 = incoming grade percent / 100
g2 = outgoing grade percent / 100
L = curve length
A = g2 - g1
```

PVC and PVT chainages are calculated from the PVI:

```text
PVC = PVI - L / 2
PVT = PVI + L / 2
```

Endpoint elevations are:

```text
PVC_Elevation = E_pvi - g1 * L / 2
PVT_Elevation = E_pvi + g2 * L / 2
```

For a design chainage `x`:

```text
d = x - PVC
TangentElevation = PVC_Elevation + g1 * d
CurveElevation = PVC_Elevation + g1 * d + (A / (2 * L)) * d^2
VerticalOffset = CurveElevation - TangentElevation
```

Curve classification uses the algebraic grade difference:

- `A > 0`: sag curve.
- `A < 0`: crest curve.
- `A` near zero: no meaningful vertical curve.

SurveyCalcKit allows design chainages outside the PVC/PVT range, marks them as outside the curve, and reports a warning. If `L <= 0`, the calculator returns warnings and avoids division by zero.

## Cross-Section Earthwork Area

At each offset, elevation difference is:

```text
d = GroundElevation - DesignElevation
```

- `d > 0`: cut.
- `d < 0`: fill.
- `d = 0`: ground is on the design elevation.

For two adjacent offsets with width `w` and differences with the same sign, the trapezoidal area is:

```text
Area = (abs(d1) + abs(d2)) * w / 2
```

When `d1` and `d2` have opposite signs, SurveyCalcKit assumes a straight ground line and locates the zero crossing:

```text
w1 = w * abs(d1) / (abs(d1) + abs(d2))
w2 = w - w1
```

The two triangular areas are assigned independently to cut or fill:

```text
Area1 = abs(d1) * w1 / 2
Area2 = abs(d2) * w2 / 2
```

This prevents cut and fill within one offset interval from cancelling each other.

## Average End-Area Volume

For two consecutive sections at chainages `S1` and `S2`:

```text
L = S2 - S1
CutVolume = (CutArea1 + CutArea2) * L / 2
FillVolume = (FillArea1 + FillArea2) * L / 2
NetVolume = CutVolume - FillVolume
```

Total cut and fill are accumulated separately. The calculator requires a positive chainage interval and avoids volume calculations for zero-length intervals.

The method assumes linear change between sections and a horizontal design elevation across each section. It does not include formation templates, side slopes, shrink/swell factors, prismoidal corrections, surface triangulation, or mass-haul optimization.

## DXF Export Conventions

DXF export does not change calculation formulas. It writes parsed point coordinates directly to a basic DXF `ENTITIES` section:

- `POINT` entities for point locations.
- `TEXT` entities for point labels when labels are enabled.
- `LWPOLYLINE` for the connected point sequence when at least two points are available.

Coordinates are written as X/Y drawing coordinates in the same unit as the input data, usually meters. `ClosePolyline` only closes the DXF polyline entity; it does not alter the source point list or perform traverse adjustment.

## Route Alignment Azimuth Convention

Route-alignment calculations use the same convention as coordinate forward and traverse calculations:

~~~text
X = planar horizontal coordinate
Y = planar vertical coordinate
Azimuth = degrees counterclockwise from the positive X axis
LEFT direction sign = +1
RIGHT direction sign = -1
~~~

Heading is always normalized to 0 through 360 degrees for output. A left curve increases the heading; a right curve decreases it.

## Clothoid Transition Curve

For radius R, spiral length Ls, signed direction d, and local distance s:

~~~text
A = sqrt(R * Ls)
k(s) = d * s / (R * Ls)
theta(s) = d * s^2 / (2 * R * Ls)
shift = Ls^2 / (24 * R)
~~~

SurveyCalcKit evaluates coordinates with deterministic composite Simpson integration rather than a first-order coordinate approximation:

~~~text
x(s) = integral(0..s) cos(heading0 + theta(u)) du
y(s) = integral(0..s) sin(heading0 + theta(u)) du
~~~

For reverse clothoids, curvature changes linearly from d/R to zero. For a general transition with start curvature k0 and end curvature k1:

~~~text
k(s) = k0 + (k1 - k0) * s / L
heading(s) = heading0 + k0 * s + (k1 - k0) * s^2 / (2 * L)
~~~

## Composite Horizontal Alignment

Each tangent, clothoid, or circular-arc element starts from the calculated end state of its predecessor. Tangents have zero curvature. Circular arcs use constant signed curvature:

~~~text
k = d / R
heading(s) = heading0 + k * s
x(s) = x0 + (sin(heading(s)) - sin(heading0)) / k
y(s) = y0 + (-cos(heading(s)) + cos(heading0)) / k
~~~

The engine reports curvature discontinuity warnings when an element's required start curvature does not match the preceding computed end curvature.

## Chainage Query and Multi-Segment Projection

An alignment query locates the element whose chainage interval contains the requested value, then evaluates that element at local distance:

~~~text
localDistance = queryChainage - elementStartChainage
~~~

For sampled centerlines, every segment is tested. With segment vector v and point vector w:

~~~text
tRaw = dot(w, v) / dot(v, v)
t = clamp(tRaw, 0, 1)
projection = start + t * v
chainage = chainageStart + t * (chainageEnd - chainageStart)
cross = vx * wy - vy * wx
~~~

The nearest projection is selected. Cross-product sign determines signed offset: positive is Left, negative is Right, and zero is OnLine.

## GeoJSON Coordinate Order

GeoJSON import and export uses coordinate arrays in [X, Y] order, with optional [X, Y, H]. Values are treated as raw planar numbers. SurveyCalcKit does not infer, transform, or validate coordinate reference systems, longitude/latitude, or map projections.

## Batch Stakeout Coordinates

Stakeout uses the same azimuth convention as traverse and coordinate forward calculation: degrees counterclockwise from the positive X axis.

Given:

```text
Origin = (X0, Y0)
A = baseline azimuth in degrees
S0 = start chainage
S = stakeout chainage
O = offset
```

Convert azimuth to radians and compute unit vectors:

```text
ux = cos(A)
uy = sin(A)
leftX = -sin(A)
leftY = cos(A)
alongDistance = S - S0
```

Stakeout coordinate:

```text
X = X0 + alongDistance * ux + O * leftX
Y = Y0 + alongDistance * uy + O * leftY
```

Offset sign convention:

- `O > 0`: left side of the baseline direction.
- `O < 0`: right side of the baseline direction.
- `O = 0`: on the baseline.

The batch stakeout module assumes a straight baseline. Circular alignment, spiral transition curve, and direct stakeout-result DXF export are future extensions.

## Coordinate Transform

Coordinate transformation applies rotation about the origin, then scale, then translation:

```text
Xr = X * cos(angle) - Y * sin(angle)
Yr = X * sin(angle) + Y * cos(angle)

X' = Xr * scale + dx
Y' = Yr * scale + dy
```

Elevation `H` is preserved.
