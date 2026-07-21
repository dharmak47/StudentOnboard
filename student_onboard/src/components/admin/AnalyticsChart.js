import React from 'react';
import './AnalyticsChart.css';

export const AnalyticsChart = ({
  type = 'line',
  data = {},
  title = '',
  options = {}
}) => {
  const defaultOptions = {
    responsive: true,
    maintainAspectRatio: true,
    ...options
  };

  const renderLineChart = () => {
    if (!data.labels || !data.datasets) return null;

    const maxValue = Math.max(
      ...data.datasets.flatMap(d => d.data || [])
    );
    const scale = maxValue > 0 ? 100 / maxValue : 100;

    return (
      <div className="chart-container">
        <svg viewBox="0 0 600 300" className="chart-svg line-chart">
          {/* Grid */}
          <defs>
            <pattern id="grid" width="60" height="30" patternUnits="userSpaceOnUse">
              <path d="M 60 0 L 0 0 0 30" fill="none" stroke="#f0f0f0" strokeWidth="1" />
            </pattern>
          </defs>
          <rect width="600" height="300" fill="url(#grid)" />

          {/* Y-axis */}
          <line x1="50" y1="10" x2="50" y2="270" stroke="#333" strokeWidth="2" />
          {/* X-axis */}
          <line x1="50" y1="270" x2="580" y2="270" stroke="#333" strokeWidth="2" />

          {/* Y-axis labels */}
          {[0, 0.25, 0.5, 0.75, 1].map((val, idx) => (
            <text
              key={`y-${idx}`}
              x="45"
              y={270 - val * 260 + 5}
              textAnchor="end"
              fontSize="12"
              fill="#666"
            >
              {Math.round(val * maxValue)}
            </text>
          ))}

          {/* Line and points */}
          {data.datasets.map((dataset, datasetIdx) => {
            const points = dataset.data.map((value, idx) => ({
              x: 50 + (idx + 1) * ((580 - 50) / (dataset.data.length)),
              y: 270 - (value * scale)
            }));

            const pathData = points.map((p, idx) => `${idx === 0 ? 'M' : 'L'} ${p.x} ${p.y}`).join(' ');

            return (
              <g key={`dataset-${datasetIdx}`}>
                {/* Line */}
                <path
                  d={pathData}
                  fill="none"
                  stroke={dataset.borderColor || '#4CAF50'}
                  strokeWidth="2"
                />
                {/* Points */}
                {points.map((p, idx) => (
                  <circle
                    key={`point-${idx}`}
                    cx={p.x}
                    cy={p.y}
                    r="4"
                    fill={dataset.borderColor || '#4CAF50'}
                  />
                ))}
              </g>
            );
          })}

          {/* X-axis labels */}
          {data.labels.map((label, idx) => (
            <text
              key={`x-${idx}`}
              x={50 + (idx + 1) * ((580 - 50) / data.labels.length)}
              y="290"
              textAnchor="middle"
              fontSize="12"
              fill="#666"
            >
              {label}
            </text>
          ))}
        </svg>
      </div>
    );
  };

  const renderBarChart = () => {
    if (!data.labels || !data.datasets) return null;

    const maxValue = Math.max(
      ...data.datasets.flatMap(d => d.data || [])
    );
    const scale = maxValue > 0 ? 100 / maxValue : 100;
    const barWidth = (530 / (data.labels.length * 1.5)) || 20;

    return (
      <div className="chart-container">
        <svg viewBox="0 0 600 300" className="chart-svg bar-chart">
          {/* Grid */}
          <defs>
            <pattern id="grid-bar" width="60" height="30" patternUnits="userSpaceOnUse">
              <path d="M 60 0 L 0 0 0 30" fill="none" stroke="#f0f0f0" strokeWidth="1" />
            </pattern>
          </defs>
          <rect width="600" height="300" fill="url(#grid-bar)" />

          {/* Axes */}
          <line x1="50" y1="10" x2="50" y2="270" stroke="#333" strokeWidth="2" />
          <line x1="50" y1="270" x2="580" y2="270" stroke="#333" strokeWidth="2" />

          {/* Y-axis labels */}
          {[0, 0.25, 0.5, 0.75, 1].map((val, idx) => (
            <text
              key={`y-${idx}`}
              x="45"
              y={270 - val * 260 + 5}
              textAnchor="end"
              fontSize="12"
              fill="#666"
            >
              {Math.round(val * maxValue)}
            </text>
          ))}

          {/* Bars */}
          {data.datasets.map((dataset, datasetIdx) =>
            dataset.data.map((value, idx) => {
              const x = 50 + idx * ((530 / data.labels.length) + 10) + 20;
              const height = value * scale * 2.6;
              const y = 270 - height;
              return (
                <rect
                  key={`bar-${datasetIdx}-${idx}`}
                  x={x}
                  y={y}
                  width={barWidth}
                  height={height}
                  fill={dataset.backgroundColor || '#4CAF50'}
                  opacity="0.8"
                />
              );
            })
          )}

          {/* X-axis labels */}
          {data.labels.map((label, idx) => (
            <text
              key={`x-${idx}`}
              x={50 + idx * ((530 / data.labels.length) + 10) + 20 + barWidth / 2}
              y="290"
              textAnchor="middle"
              fontSize="12"
              fill="#666"
            >
              {label}
            </text>
          ))}
        </svg>
      </div>
    );
  };

  const renderPieChart = () => {
    if (!data.labels || !data.datasets || !data.datasets[0]) return null;

    const dataset = data.datasets[0];
    const total = dataset.data.reduce((a, b) => a + b, 0);
    const colors = dataset.backgroundColor || [
      '#4CAF50', '#2196F3', '#FF9800', '#F44336', '#9C27B0', '#00BCD4'
    ];

    let currentAngle = -Math.PI / 2;
    const slices = dataset.data.map((value, idx) => {
      const sliceAngle = (value / total) * 2 * Math.PI;
      const startAngle = currentAngle;
      const endAngle = currentAngle + sliceAngle;

      const x1 = 150 + 100 * Math.cos(startAngle);
      const y1 = 150 + 100 * Math.sin(startAngle);
      const x2 = 150 + 100 * Math.cos(endAngle);
      const y2 = 150 + 100 * Math.sin(endAngle);

      const largeArc = sliceAngle > Math.PI ? 1 : 0;
      const pathData = `M 150 150 L ${x1} ${y1} A 100 100 0 ${largeArc} 1 ${x2} ${y2} Z`;

      currentAngle = endAngle;

      return {
        path: pathData,
        color: colors[idx % colors.length],
        label: data.labels[idx],
        value: value,
        percentage: ((value / total) * 100).toFixed(1)
      };
    });

    return (
      <div className="chart-container pie-container">
        <svg viewBox="0 0 300 300" className="chart-svg pie-chart">
          {slices.map((slice, idx) => (
            <g key={`slice-${idx}`}>
              <path d={slice.path} fill={slice.color} opacity="0.8" stroke="white" strokeWidth="2" />
            </g>
          ))}
        </svg>
        <div className="pie-legend">
          {slices.map((slice, idx) => (
            <div key={`legend-${idx}`} className="legend-item">
              <span
                className="legend-color"
                style={{ backgroundColor: slice.color }}
              />
              <span className="legend-label">
                {slice.label}: {slice.percentage}%
              </span>
            </div>
          ))}
        </div>
      </div>
    );
  };

  return (
    <div className="analytics-chart">
      {title && <h3 className="chart-title">{title}</h3>}
      {type === 'line' && renderLineChart()}
      {type === 'bar' && renderBarChart()}
      {type === 'pie' && renderPieChart()}
    </div>
  );
};
