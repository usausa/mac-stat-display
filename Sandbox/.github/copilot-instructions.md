# Copilot Instructions

## Project Guidelines
- For this repository, dashboard UI should avoid overlap between timeseries charts and value text; network chart should render upload/download mirrored around a center line; process and HW monitor panels should not show utilization gauges; timeseries charts only for CPU, memory, and network; panel sizes should be weighted by information density rather than uniform.
- Widget cards should use a smaller border radius and remove the decorative top line. Metric widgets should place the title on the left and the value on the right, with the title wrapping to two lines if needed. Gauge widgets should not show a title, and label widgets should use a distinct style without borders.
